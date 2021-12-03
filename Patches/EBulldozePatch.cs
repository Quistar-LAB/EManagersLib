using ColossalFramework;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using System.Runtime.CompilerServices;

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

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static IEnumerator DeleteProp(BulldozeTool bulldoze, uint prop) {
            DeletePropImpl(bulldoze, prop);
            yield return 0;
        }

        private static void DeletePropImpl(BulldozeTool bulldoze, uint prop) {
            EUtils.ELog($"bulldoze: {bulldoze}, propID: {prop}");
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
            using (IEnumerator<CodeInstruction> codes = instructions.GetEnumerator()) {
                while (codes.MoveNext()) {
                    var cur = codes.Current;
                    if (cur.opcode == OpCodes.Ldloca_S && codes.MoveNext()) {
                        var next = codes.Current;
                        if (next.opcode == OpCodes.Call && next.operand == get_Prop && codes.MoveNext()) {
                            var next1 = codes.Current;
                            if (next1.opcode == OpCodes.Call) {
                                yield return cur;
                                yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(InstanceIDExtension), nameof(InstanceIDExtension.GetProp32ByRef)));
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
        }

        private static IEnumerable<CodeInstruction> SimulationStepTranspiler(IEnumerable<CodeInstruction> instructions) {
            var codes = __SimulationStepTranspiler(instructions);
            foreach(var code in codes) {
                EUtils.ELog(code.ToString());
            }
            return codes;
        }
        private static IEnumerable<CodeInstruction> __SimulationStepTranspiler(IEnumerable<CodeInstruction> instructions) {
            MethodInfo propGetter = AccessTools.PropertyGetter(typeof(InstanceID), nameof(InstanceID.Prop));
            MethodInfo deleteProp = AccessTools.Method(typeof(BulldozeTool), "DeletePropImpl");
            MethodInfo simulationStep = AccessTools.Method(typeof(DefaultTool), nameof(DefaultTool.SimulationStep));
            FieldInfo lastInstance = AccessTools.Field(typeof(BulldozeTool), "m_lastInstance");

            foreach (var code in instructions) {
                if(code.opcode == OpCodes.Call && code.operand == simulationStep) {
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(BulldozeTool), "m_mouseRayValid"));
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(BulldozeTool), "m_mouseRay"));
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(BulldozeTool), "m_rayRight"));
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(BulldozeTool), "m_mouseRayLength"));
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(BulldozeTool), "m_mouseLeftDown"));
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(BulldozeTool), "m_mouseRightDown"));
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(BulldozeTool), "m_mousePosition"));
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(BulldozeTool), "m_hoverInstance"));
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(BulldozeTool), "m_hoverInstance2"));
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(BulldozeTool), "m_subHoverIndex"));
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(BulldozeTool), "m_selectErrors"));
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(BulldozeTool), "m_toolController"));
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(BulldozeTool), "m_angle"));
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(BulldozeTool), "m_accuratePosition"));
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(BulldozeTool), "m_accuratePositionValid"));
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(BulldozeTool), "m_fixedHeight"));
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EDefaultToolExtension), nameof(EDefaultToolExtension.DefaultSimulationStep)));
                } else if (code.opcode == OpCodes.Call && code.operand == propGetter) {
                    code.operand = AccessTools.Method(typeof(InstanceIDExtension), nameof(InstanceIDExtension.GetProp32ByRef));
                    yield return code;
                } else if (code.opcode == OpCodes.Call && code.operand == deleteProp) {
                    code.operand = AccessTools.Method(typeof(EBulldozePatch), nameof(EBulldozePatch.DeletePropImpl));
                    yield return code;
                } else {
                    yield return code;
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
            harmony.Patch(AccessTools.Method(typeof(BulldozeTool), nameof(BulldozeTool.SimulationStep)), transpiler: new HarmonyMethod(AccessTools.Method(typeof(EBulldozePatch), nameof(SimulationStepTranspiler))));
        }

        internal void Disable(Harmony harmony) {
            harmony.Unpatch(AccessTools.Method(typeof(BulldozeTool), "OnToolGUI"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(BulldozeTool), nameof(BulldozeTool.SimulationStep)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
        }
    }
}
