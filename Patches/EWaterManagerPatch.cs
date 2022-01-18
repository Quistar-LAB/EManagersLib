using ColossalFramework;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using static EManagersLib.EWaterManager;

namespace EManagersLib.Patches {
    internal readonly struct EWaterManagerPatch {
        private static IEnumerable<CodeInstruction> AwakeTranspiler(IEnumerable<CodeInstruction> instructions) {
            foreach (var code in instructions) {
                if (code.LoadsConstant(DEFAULTGRID_RESOLUTION)) {
                    code.operand = WATERGRID_RESOLUTION;
                } else if (code.LoadsConstant(DEFAULTGRID_RESOLUTION - 1)) {
                    code.operand = WATERGRID_RESOLUTION - 1;
                }
                yield return code;
            }
        }

        private static IEnumerable<CodeInstruction> CheckHeatingTranspiler(IEnumerable<CodeInstruction> instructions) {
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(WaterManager), "m_waterGrid"));
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Ldarg_2);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EWaterManager), nameof(EWaterManager.CheckHeating)));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        private static IEnumerable<CodeInstruction> CheckWaterTranspiler(IEnumerable<CodeInstruction> instructions) {
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(WaterManager), "m_waterGrid"));
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Ldarg_2);
            yield return new CodeInstruction(OpCodes.Ldarg_3);
            yield return new CodeInstruction(OpCodes.Ldarg_S, 4);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EWaterManager), nameof(EWaterManager.CheckWater)));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        private static IEnumerable<CodeInstruction> ConductHeatingToCellTranspiler(IEnumerable<CodeInstruction> instructions) {
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(WaterManager), "m_heatingPulseUnitEnd"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(WaterManager), "m_canContinue"));
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Ldarg_2);
            yield return new CodeInstruction(OpCodes.Ldarg_3);
            yield return new CodeInstruction(OpCodes.Ldarg_S, 4);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EWaterManager), nameof(EWaterManager.ConductHeatingToCell)));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        private static IEnumerable<CodeInstruction> ConductHeatingToCellsTranspiler(IEnumerable<CodeInstruction> instructions) {
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(WaterManager), "m_waterGrid"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(WaterManager), "m_heatingPulseUnitEnd"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(WaterManager), "m_canContinue"));
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Ldarg_2);
            yield return new CodeInstruction(OpCodes.Ldarg_3);
            yield return new CodeInstruction(OpCodes.Ldarg_S, 4);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EWaterManager), nameof(EWaterManager.ConductHeatingToCells)));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        private static IEnumerable<CodeInstruction> ConductHeatingToNodeTranspiler(IEnumerable<CodeInstruction> instructions) {
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(WaterManager), nameof(WaterManager.m_nodeData)));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(WaterManager), "m_heatingPulseGroups"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(WaterManager), "m_heatingPulseGroupCount"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(WaterManager), "m_heatingPulseUnitEnd"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(WaterManager), "m_canContinue"));
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Ldarg_2);
            yield return new CodeInstruction(OpCodes.Ldarg_3);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EWaterManager), nameof(EWaterManager.ConductHeatingToNode)));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        private static IEnumerable<CodeInstruction> ConductSewageToCellTranspiler(IEnumerable<CodeInstruction> instructions) {
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(WaterManager), "m_sewagePulseUnitEnd"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(WaterManager), "m_canContinue"));
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Ldarg_2);
            yield return new CodeInstruction(OpCodes.Ldarg_3);
            yield return new CodeInstruction(OpCodes.Ldarg_S, 4);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EWaterManager), nameof(EWaterManager.ConductSewageToCell)));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        private static IEnumerable<CodeInstruction> ConductSewageToCellsTranspiler(IEnumerable<CodeInstruction> instructions) {
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(WaterManager), "m_waterGrid"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(WaterManager), "m_sewagePulseUnitEnd"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(WaterManager), "m_canContinue"));
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Ldarg_2);
            yield return new CodeInstruction(OpCodes.Ldarg_3);
            yield return new CodeInstruction(OpCodes.Ldarg_S, 4);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EWaterManager), nameof(EWaterManager.ConductSewageToCells)));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        private static IEnumerable<CodeInstruction> ConductSewageToNodeTranspiler(IEnumerable<CodeInstruction> instructions) {
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(WaterManager), nameof(WaterManager.m_nodeData)));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(WaterManager), "m_sewagePulseGroups"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(WaterManager), "m_sewagePulseUnitEnd"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(WaterManager), "m_canContinue"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(WaterManager), "m_sewagePulseGroupCount"));
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Ldarg_2);
            yield return new CodeInstruction(OpCodes.Ldarg_3);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EWaterManager), nameof(EWaterManager.ConductSewageToNode)));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        private static IEnumerable<CodeInstruction> ConductWaterToCellTranspiler(IEnumerable<CodeInstruction> instructions) {
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(WaterManager), "m_waterPulseUnitEnd"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(WaterManager), "m_canContinue"));
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Ldarg_2);
            yield return new CodeInstruction(OpCodes.Ldarg_3);
            yield return new CodeInstruction(OpCodes.Ldarg_S, 4);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EWaterManager), nameof(EWaterManager.ConductWaterToCell)));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        private static IEnumerable<CodeInstruction> ConductWaterToCellsTranspiler(IEnumerable<CodeInstruction> instructions) {
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(WaterManager), "m_waterGrid"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(WaterManager), "m_waterPulseUnitEnd"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(WaterManager), "m_canContinue"));
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Ldarg_2);
            yield return new CodeInstruction(OpCodes.Ldarg_3);
            yield return new CodeInstruction(OpCodes.Ldarg_S, 4);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EWaterManager), nameof(EWaterManager.ConductWaterToCells)));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        private static IEnumerable<CodeInstruction> ConductWaterToNodeTranspiler(IEnumerable<CodeInstruction> instructions) {
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(WaterManager), nameof(WaterManager.m_nodeData)));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(WaterManager), "m_waterPulseGroups"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(WaterManager), "m_waterPulseUnitEnd"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(WaterManager), "m_canContinue"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(WaterManager), "m_waterPulseGroupCount"));
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Ldarg_2);
            yield return new CodeInstruction(OpCodes.Ldarg_3);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EWaterManager), nameof(EWaterManager.ConductWaterToNode)));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        private static IEnumerable<CodeInstruction> SimulationStepImplTranspiler(IEnumerable<CodeInstruction> instructions) {
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(WaterManager), nameof(WaterManager.m_nodeData)));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(WaterManager), "m_waterGrid"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(WaterManager), "m_waterPulseGroups"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(WaterManager), "m_sewagePulseGroups"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(WaterManager), "m_heatingPulseGroups"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(WaterManager), "m_waterPulseGroupCount"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(WaterManager), "m_waterPulseUnitStart"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(WaterManager), "m_waterPulseUnitEnd"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(WaterManager), "m_sewagePulseGroupCount"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(WaterManager), "m_sewagePulseUnitStart"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(WaterManager), "m_sewagePulseUnitEnd"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(WaterManager), "m_heatingPulseGroupCount"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(WaterManager), "m_heatingPulseUnitStart"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(WaterManager), "m_heatingPulseUnitEnd"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(WaterManager), "m_processedCells"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(WaterManager), "m_conductiveCells"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(WaterManager), "m_canContinue"));
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EWaterManager), nameof(EWaterManager.SimulationStepImpl)));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        private static IEnumerable<CodeInstruction> TryDumpSewageTranspiler(IEnumerable<CodeInstruction> instructions) {
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(WaterManager), "m_waterGrid"));
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Ldarg_2);
            yield return new CodeInstruction(OpCodes.Ldarg_3);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EWaterManager), nameof(EWaterManager.TryDumpSewage)));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        private static IEnumerable<CodeInstruction> TryFetchHeatingTranspiler(IEnumerable<CodeInstruction> instructions) {
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(WaterManager), "m_waterGrid"));
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Ldarg_2);
            yield return new CodeInstruction(OpCodes.Ldarg_3);
            yield return new CodeInstruction(OpCodes.Ldarg_S, 4);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EWaterManager), nameof(EWaterManager.TryFetchHeating)));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        private static IEnumerable<CodeInstruction> TryFetchWaterTranspiler(IEnumerable<CodeInstruction> instructions) {
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(WaterManager), "m_waterGrid"));
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Ldarg_2);
            yield return new CodeInstruction(OpCodes.Ldarg_3);
            yield return new CodeInstruction(OpCodes.Ldarg_S, 4);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EWaterManager), nameof(EWaterManager.TryFetchWater)));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        private static IEnumerable<CodeInstruction> UpdateGridTranspiler(IEnumerable<CodeInstruction> instructions) {
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(WaterManager), "m_waterGrid"));
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Ldarg_2);
            yield return new CodeInstruction(OpCodes.Ldarg_3);
            yield return new CodeInstruction(OpCodes.Ldarg_S, 4);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EWaterManager), nameof(EWaterManager.UpdateGrid)));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        private static IEnumerable<CodeInstruction> UpdateTextureTranspiler(IEnumerable<CodeInstruction> instructions) {
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(WaterManager), "m_waterGrid"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(WaterManager), "m_modifiedX1"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(WaterManager), "m_modifiedZ1"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(WaterManager), "m_modifiedX2"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(WaterManager), "m_modifiedZ2"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(WaterManager), "m_waterMapVisible"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(WaterManager), "m_waterTexture"));
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EWaterManager), nameof(EWaterManager.UpdateTexture)));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        private static IEnumerable<CodeInstruction> UpdateWaterMappingTranspiler(IEnumerable<CodeInstruction> instructions) {
            const float defZ = 1f / (DEFAULTGRID_RESOLUTION * WATERGRID_CELL_SIZE);
            const float Z = 1f / (WATERGRID_RESOLUTION * WATERGRID_CELL_SIZE);
            const float defW = 1f / DEFAULTGRID_RESOLUTION;
            const float W = 1f / WATERGRID_RESOLUTION;
            foreach (var code in instructions) {
                if (code.LoadsConstant(defZ)) {
                    code.operand = Z;
                } else if (code.LoadsConstant(defW)) {
                    code.operand = W;
                }
                yield return code;
            }
        }

        private static IEnumerable<CodeInstruction> DeserializeTranspiler(IEnumerable<CodeInstruction> instructions) {
            MethodInfo getWMInstance = AccessTools.PropertyGetter(typeof(Singleton<WaterManager>), nameof(Singleton<WaterManager>.instance));
            MethodInfo getLMInstance = AccessTools.PropertyGetter(typeof(Singleton<LoadingManager>), nameof(Singleton<LoadingManager>.instance));
            FieldInfo drainPipeMissingGuide = AccessTools.Field(typeof(WaterManager), "m_drainPipeMissingGuide");
            using (var codes = instructions.GetEnumerator()) {
                while (codes.MoveNext()) {
                    var cur = codes.Current;
                    if (cur.opcode == OpCodes.Call && cur.operand == getWMInstance && codes.MoveNext()) {
                        var next = codes.Current;
                        yield return cur;
                        yield return next;
                        if (next.opcode == OpCodes.Stloc_0) {
                            yield return new CodeInstruction(OpCodes.Ldloc_0);
                            yield return new CodeInstruction(OpCodes.Ldloc_0);
                            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(WaterManager), "m_waterTexture"));
                            yield return new CodeInstruction(OpCodes.Ldloc_0);
                            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(WaterManager), "m_modifiedX2"));
                            yield return new CodeInstruction(OpCodes.Ldloc_0);
                            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(WaterManager), "m_modifiedZ2"));
                            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EWaterManager), nameof(EWaterManager.EnsureCapacity)));
                        }
                    } else if (cur.opcode == OpCodes.Stfld && cur.operand == drainPipeMissingGuide && codes.MoveNext()) {
                        var next = codes.Current;
                        yield return cur;
                        if (next.opcode == OpCodes.Call && next.operand == getLMInstance) {
                            yield return new CodeInstruction(OpCodes.Ldloc_0).WithLabels(next.ExtractLabels());
                            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(WaterManager), "m_waterGrid"));
                            yield return new CodeInstruction(OpCodes.Ldloc_0);
                            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(WaterManager), "m_waterPulseUnits"));
                            yield return new CodeInstruction(OpCodes.Ldloc_0);
                            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(WaterManager), "m_sewagePulseUnits"));
                            yield return new CodeInstruction(OpCodes.Ldloc_0);
                            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(WaterManager), "m_heatingPulseUnits"));
                            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EWaterManager), nameof(EWaterManager.IntegratedDeserialize)));
                        }
                        yield return next;
                    } else {
                        yield return cur;
                    }
                }
            }
        }

        private static IEnumerable<CodeInstruction> SerializeTranspiler(IEnumerable<CodeInstruction> instructions) {
            FieldInfo waterGrid = AccessTools.Field(typeof(WaterManager), "m_waterGrid");
            using (var codes = instructions.GetEnumerator()) {
                while (codes.MoveNext()) {
                    var cur = codes.Current;
                    if (cur.opcode == OpCodes.Ldloc_0 && codes.MoveNext()) {
                        var next = codes.Current;
                        yield return cur;
                        yield return next;
                        if (next.opcode == OpCodes.Ldfld && next.operand == waterGrid) {
                            yield return new CodeInstruction(OpCodes.Ldloc_0);
                            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(WaterManager), "m_waterPulseUnits"));
                            yield return new CodeInstruction(OpCodes.Ldloc_0);
                            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(WaterManager), "m_sewagePulseUnits"));
                            yield return new CodeInstruction(OpCodes.Ldloc_0);
                            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(WaterManager), "m_heatingPulseUnits"));
                            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EWaterManager), nameof(EWaterManager.IntegratedSerialize)));
                        }
                    } else {
                        yield return cur;
                    }
                }
            }
        }

        private static IEnumerable<CodeInstruction> AfterDeserializeTranspiler(IEnumerable<CodeInstruction> instructions) {
            foreach (var code in instructions) {
                if (code.LoadsConstant(DEFAULTGRID_RESOLUTION - 1)) {
                    code.operand = WATERGRID_RESOLUTION - 1;
                }
                yield return code;
            }
        }

        internal void Enable(Harmony harmony) {
            try {
                harmony.Patch(AccessTools.Method(typeof(WaterManager), "Awake"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EWaterManagerPatch), nameof(AwakeTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch WaterManager::Awake");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(WaterManager), "Awake"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(WaterManager), nameof(WaterManager.CheckHeating)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EWaterManagerPatch), nameof(CheckHeatingTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch WaterManager::CheckHeating");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(WaterManager), nameof(WaterManager.CheckHeating)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(WaterManager), nameof(WaterManager.CheckWater)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EWaterManagerPatch), nameof(CheckWaterTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch WaterManager::CheckWater");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(WaterManager), nameof(WaterManager.CheckWater)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(WaterManager), "ConductHeatingToCell"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EWaterManagerPatch), nameof(ConductHeatingToCellTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch WaterManager::ConductHeatingToCell");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(WaterManager), "ConductHeatingToCell"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(WaterManager), "ConductHeatingToCells"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EWaterManagerPatch), nameof(ConductHeatingToCellsTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch WaterManager::ConductHeatingToCells");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(WaterManager), "ConductHeatingToCells"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(WaterManager), "ConductHeatingToNode"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EWaterManagerPatch), nameof(ConductHeatingToNodeTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch WaterManager::ConductHeatingToNode");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(WaterManager), "ConductHeatingToNode"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(WaterManager), "ConductSewageToCell"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EWaterManagerPatch), nameof(ConductSewageToCellTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch WaterManager::ConductSewageToCell");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(WaterManager), "ConductSewageToCell"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(WaterManager), "ConductSewageToCells"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EWaterManagerPatch), nameof(ConductSewageToCellsTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch WaterManager::ConductSewageToCells");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(WaterManager), "ConductSewageToCells"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(WaterManager), "ConductSewageToNode"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EWaterManagerPatch), nameof(ConductSewageToNodeTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch WaterManager::ConductSewageToNode");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(WaterManager), "ConductSewageToNode"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(WaterManager), "ConductWaterToCell"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EWaterManagerPatch), nameof(ConductWaterToCellTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch WaterManager::ConductWaterToCell");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(WaterManager), "ConductWaterToCell"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(WaterManager), "ConductWaterToCells"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EWaterManagerPatch), nameof(ConductWaterToCellsTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch WaterManager::ConductWaterToCells");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(WaterManager), "ConductWaterToCells"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(WaterManager), "ConductWaterToNode"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EWaterManagerPatch), nameof(ConductWaterToNodeTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch WaterManager::ConductWaterToNode");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(WaterManager), "ConductWaterToNode"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(WaterManager), "SimulationStepImpl"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EWaterManagerPatch), nameof(SimulationStepImplTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch WaterManager::SimulationStepImpl");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(WaterManager), "SimulationStepImpl"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(WaterManager), nameof(WaterManager.TryDumpSewage),
                    new Type[] { typeof(Vector3), typeof(int), typeof(int) }),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EWaterManagerPatch), nameof(TryDumpSewageTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch WaterManager::TryDumpSewage(Vector3 pos, int rate, int max)");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(WaterManager), nameof(WaterManager.TryDumpSewage),
                    new Type[] { typeof(Vector3), typeof(int), typeof(int) }),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(WaterManager), nameof(WaterManager.TryFetchHeating)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EWaterManagerPatch), nameof(TryFetchHeatingTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch WaterManager::TryFetchHeating");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(WaterManager), nameof(WaterManager.TryFetchHeating)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(WaterManager), nameof(WaterManager.TryFetchWater),
                    new Type[] { typeof(Vector3), typeof(int), typeof(int), typeof(byte).MakeByRefType() }),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EWaterManagerPatch), nameof(TryFetchWaterTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch WaterManager::TryFetchWater(Vector3 pos, int rate, int max)");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(WaterManager), nameof(WaterManager.TryFetchWater),
                    new Type[] { typeof(Vector3), typeof(int), typeof(int), typeof(byte).MakeByRefType() }),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(WaterManager), nameof(WaterManager.UpdateGrid)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EWaterManagerPatch), nameof(UpdateGridTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch WaterManager::UpdateGrid");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(WaterManager), nameof(WaterManager.UpdateGrid)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(WaterManager), "UpdateTexture"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EWaterManagerPatch), nameof(UpdateTextureTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch WaterManager::UpdateTexture");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(WaterManager), "UpdateTexture"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(WaterManager), "UpdateWaterMapping"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EWaterManagerPatch), nameof(UpdateWaterMappingTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch WaterManager::UpdateWaterMapping");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(WaterManager), "UpdateWaterMapping"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                var reverse = harmony.CreateReversePatcher(AccessTools.Method(typeof(WaterManager), "UpdateWaterMapping"),
                    new HarmonyMethod(AccessTools.Method(typeof(EWaterManager), nameof(EWaterManager.UpdateWaterMapping))));
                reverse.Patch(HarmonyReversePatchType.Original);
            } catch (Exception e) {
                EUtils.ELog("Failed to reverse patch WaterManager::UpdateWaterMapping");
                EUtils.ELog(e.Message);
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(WaterManager.Data), nameof(WaterManager.Data.Deserialize)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EWaterManagerPatch), nameof(DeserializeTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch WaterManager::Data::Deserialize");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(WaterManager.Data), nameof(WaterManager.Data.Deserialize)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(WaterManager.Data), nameof(WaterManager.Data.Serialize)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EWaterManagerPatch), nameof(SerializeTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch WaterManager::Data::Serialize");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(WaterManager.Data), nameof(WaterManager.Data.Serialize)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(WaterManager.Data), nameof(WaterManager.Data.AfterDeserialize)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EWaterManagerPatch), nameof(AfterDeserializeTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch WaterManager::Data::AfterDeserialize");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(WaterManager.Data), nameof(WaterManager.Data.AfterDeserialize)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
        }

        internal void Disable(Harmony harmony) {
            harmony.Unpatch(AccessTools.Method(typeof(WaterManager), "Awake"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(WaterManager), nameof(WaterManager.CheckHeating)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(WaterManager), nameof(WaterManager.CheckWater)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(WaterManager), "ConductHeatingToCell"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(WaterManager), "ConductHeatingToCells"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(WaterManager), "ConductHeatingToNode"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(WaterManager), "ConductSewageToCell"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(WaterManager), "ConductSewageToCells"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(WaterManager), "ConductSewageToNode"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(WaterManager), "ConductWaterToCell"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(WaterManager), "ConductWaterToCells"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(WaterManager), "ConductWaterToNode"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(WaterManager), "SimulationStepImpl"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(WaterManager), nameof(WaterManager.TryDumpSewage),
                new Type[] { typeof(Vector3), typeof(int), typeof(int) }), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(WaterManager), nameof(WaterManager.TryFetchHeating)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(WaterManager), nameof(WaterManager.TryFetchWater),
                new Type[] { typeof(Vector3), typeof(int), typeof(int), typeof(byte).MakeByRefType() }), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(WaterManager), nameof(WaterManager.UpdateGrid)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(WaterManager), "UpdateTexture"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(WaterManager), "UpdateWaterMapping"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(WaterManager), "UpdateWaterMapping"), HarmonyPatchType.ReversePatch, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(WaterManager.Data), nameof(WaterManager.Data.Deserialize)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(WaterManager.Data), nameof(WaterManager.Data.Serialize)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(WaterManager.Data), nameof(WaterManager.Data.AfterDeserialize)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
        }
    }
}
