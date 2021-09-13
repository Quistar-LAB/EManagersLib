using ColossalFramework;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace EManagersLib {
    public class EBulldozePatch {
        private static Action<BulldozeTool, BulldozeTool.Mode> set_BulldozingMode;
        private static Action<BulldozeTool, ItemClass.Service> set_BulldozingService;
        private static Action<BulldozeTool, ItemClass.Layer> set_BulldozingLayers;
        private static Action<BulldozeTool, float> set_DeleteTimer;
        private static Func<BulldozeTool, Vector3> get_MousePosition;
        private static Action<BulldozeTool, InstanceID> set_HoverInstance;
        private static Func<BulldozeTool, InstanceID> get_HoverInstance;
        private static Action<BulldozeTool, InstanceID> set_LastInstance;
        private static Func<BulldozeTool, InstanceID> get_LastInstance;

        public static IEnumerator DeleteProp(BulldozeTool bulldoze, uint prop) {
            DeletePropImpl(bulldoze, prop);
            yield return 0;
        }

        private static void DeletePropImpl(BulldozeTool bulldoze, uint prop) {
            if (EPropManager.m_props.m_buffer[prop].m_flags != 0) {
                PropManager propManager = Singleton<PropManager>.instance;
                set_BulldozingMode(bulldoze, BulldozeTool.Mode.PropOrTree);
                set_BulldozingService(bulldoze, ItemClass.Service.None);
                set_BulldozingLayers(bulldoze, ItemClass.Layer.None);
                set_DeleteTimer(bulldoze, 0.1f);
                propManager.ReleaseProp(prop);
                PropTool.DispatchPlacementEffect(get_MousePosition(bulldoze), true);
            }
            if (prop == get_HoverInstance(bulldoze).GetProp32()) {
                set_HoverInstance(bulldoze, InstanceID.Empty);
            }
            if (prop == get_LastInstance(bulldoze).GetProp32()) {
                set_LastInstance(bulldoze, InstanceID.Empty);
            }
        }

        private static IEnumerable<CodeInstruction> BulldozeOnToolGUITranspiler(IEnumerable<CodeInstruction> instructions) {
            MethodInfo get_Prop = AccessTools.PropertyGetter(typeof(InstanceID), nameof(InstanceID.Prop));
            var codes = instructions.GetEnumerator();
            while (codes.MoveNext()) {
                var cur = codes.Current;
                if (cur.opcode == OpCodes.Ldloca_S && codes.MoveNext()) {
                    var next = codes.Current;
                    if (next.opcode == OpCodes.Call && next.operand == get_Prop && codes.MoveNext()) {
                        var next1 = codes.Current;
                        if (next1.opcode == OpCodes.Call) {
                            yield return new CodeInstruction(OpCodes.Ldloc_1);
                            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(InstanceIDExtension), nameof(InstanceIDExtension.GetProp32)));
                            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EBulldozePatch), nameof(DeleteProp)));
                        } else {
                            yield return cur;
                            yield return next;
                            yield return next1;
                        }
                    } else {
                        yield return cur;
                        yield return next;
                    }
                } else {
                    yield return cur;
                }
            }
        }

        private static bool InitializedBulldoze = false;
        internal void Enable(Harmony harmony) {
            if (!InitializedBulldoze) {
                set_BulldozingMode = EUtils.CreateSetter<BulldozeTool, BulldozeTool.Mode>(AccessTools.Field(typeof(BulldozeTool), "m_bulldozingMode"));
                set_BulldozingService = EUtils.CreateSetter<BulldozeTool, ItemClass.Service>(AccessTools.Field(typeof(BulldozeTool), "m_bulldozingService"));
                set_BulldozingLayers = EUtils.CreateSetter<BulldozeTool, ItemClass.Layer>(AccessTools.Field(typeof(BulldozeTool), "m_bulldozingLayers"));
                set_DeleteTimer = EUtils.CreateSetter<BulldozeTool, float>(AccessTools.Field(typeof(BulldozeTool), "m_deleteTimer"));
                get_MousePosition = EUtils.CreateGetter<BulldozeTool, Vector3>(AccessTools.Field(typeof(BulldozeTool), "m_mousePosition"));
                set_HoverInstance = EUtils.CreateSetter<BulldozeTool, InstanceID>(AccessTools.Field(typeof(BulldozeTool), "m_hoverInstance"));
                get_HoverInstance = EUtils.CreateGetter<BulldozeTool, InstanceID>(AccessTools.Field(typeof(BulldozeTool), "m_hoverInstance"));
                set_LastInstance = EUtils.CreateSetter<BulldozeTool, InstanceID>(AccessTools.Field(typeof(BulldozeTool), "m_lastInstance"));
                get_LastInstance = EUtils.CreateGetter<BulldozeTool, InstanceID>(AccessTools.Field(typeof(BulldozeTool), "m_lastInstance"));
                InitializedBulldoze = true;
            }
            harmony.Patch(AccessTools.Method(typeof(BulldozeTool), "OnToolGUI"), transpiler: new HarmonyMethod(AccessTools.Method(typeof(EBulldozePatch), nameof(BulldozeOnToolGUITranspiler))));
        }

        internal void Disable(Harmony harmony) {
            harmony.Unpatch(AccessTools.Method(typeof(BulldozeTool), "OnToolGUI"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
        }
    }
}
