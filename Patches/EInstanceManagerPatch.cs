using ColossalFramework;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace EManagersLib {
    internal class EInstanceManagerPatch {
        private static IEnumerable<CodeInstruction> GetPositionTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
            CodeInstruction newStLoc(LocalBuilder local) {
                switch (local.LocalIndex) {
                case 0: return new CodeInstruction(OpCodes.Stloc_0);
                case 1: return new CodeInstruction(OpCodes.Stloc_1);
                case 2: return new CodeInstruction(OpCodes.Stloc_2);
                case 3: return new CodeInstruction(OpCodes.Stloc_3);
                default: return new CodeInstruction(OpCodes.Stloc_S, local);
                }
            }
            CodeInstruction newLdLoc(LocalBuilder local) {
                switch (local.LocalIndex) {
                case 0: return new CodeInstruction(OpCodes.Ldloc_0);
                case 1: return new CodeInstruction(OpCodes.Ldloc_1);
                case 2: return new CodeInstruction(OpCodes.Ldloc_2);
                case 3: return new CodeInstruction(OpCodes.Ldloc_3);
                default: return new CodeInstruction(OpCodes.Ldloc_S, local);
                }
            }
            bool skipCodes = false;
            List<Label> labels = default;
            LocalBuilder refPropInstance = il.DeclareLocal(typeof(PropInstance).MakeByRefType());
            MethodInfo getPMInstance = AccessTools.PropertyGetter(typeof(Singleton<PropManager>), nameof(Singleton<PropManager>.instance));
            foreach (var code in instructions) {
                if (!skipCodes && code.opcode == OpCodes.Call && code.operand == getPMInstance) {
                    labels = code.labels;
                    skipCodes = true;
                } else if (skipCodes && code.opcode == OpCodes.Ret) {
                    skipCodes = false;
                    yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(EPropManager), nameof(EPropManager.m_props))).WithLabels(labels);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Array32<PropInstance>), nameof(Array32<PropInstance>.m_buffer)));
                    yield return new CodeInstruction(OpCodes.Ldarga_S, 0);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(InstanceIDExtension), nameof(InstanceIDExtension.GetProp32ByRef)));
                    yield return new CodeInstruction(OpCodes.Ldelema, typeof(PropInstance));
                    yield return newStLoc(refPropInstance);
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return newLdLoc(refPropInstance);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(PropInstance), nameof(PropInstance.Position)));
                    yield return new CodeInstruction(OpCodes.Stobj, typeof(Vector3));
                    yield return new CodeInstruction(OpCodes.Ldarg_2);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(Quaternion), nameof(Quaternion.identity)));
                    yield return new CodeInstruction(OpCodes.Stobj, typeof(Quaternion));
                    yield return new CodeInstruction(OpCodes.Ldarg_3);
                    yield return newLdLoc(refPropInstance);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(PropInstance), nameof(PropInstance.Info)));
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PropInfo), nameof(PropInfo.m_generatedInfo)));
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PropInfoGen), nameof(PropInfoGen.m_size)));
                    yield return new CodeInstruction(OpCodes.Stobj, typeof(Vector3));
                    yield return new CodeInstruction(OpCodes.Ldc_I4_1);
                }
                if (!skipCodes) yield return code;
            }
            using (IEnumerator<CodeInstruction> codes = instructions.GetEnumerator()) {
                while (codes.MoveNext()) {
                    var cur = codes.Current;
                    if (cur.opcode == OpCodes.Ldarg_0) {

                    } else {
                        yield return cur;
                    }
                }
            }
        }

        internal void Enable(Harmony harmony) {
            try {
                harmony.Patch(AccessTools.Method(typeof(InstanceManager), nameof(InstanceManager.GetPosition)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EInstanceManagerPatch), nameof(GetPositionTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch InstanceManager::GetPosition");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(InstanceManager), nameof(InstanceManager.GetPosition)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
        }

        internal void Disable(Harmony harmony) {
            harmony.Unpatch(AccessTools.Method(typeof(InstanceManager), nameof(InstanceManager.GetPosition)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
        }
    }
}
