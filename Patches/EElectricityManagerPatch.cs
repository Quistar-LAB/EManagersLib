using ColossalFramework;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace EManagersLib.Patches {
    internal readonly struct EElectricityManagerPatch {
        private const int DEFAULTGRID_RESOLUTION = 256;
        private const int ELECTRICITYGRID_RESOLUTION = 462;
        private const float ELECTRICITYGRID_CELL_SIZE = 38.25f;

        private static IEnumerable<CodeInstruction> ReplaceConstants(IEnumerable<CodeInstruction> instructions) {
            const float defHalfGrid = DEFAULTGRID_RESOLUTION / 2f;
            const float halfGrid = ELECTRICITYGRID_RESOLUTION / 2f;
            foreach (var code in instructions) {
                if (code.LoadsConstant(DEFAULTGRID_RESOLUTION * DEFAULTGRID_RESOLUTION)) {
                    code.operand = ELECTRICITYGRID_RESOLUTION * ELECTRICITYGRID_RESOLUTION;
                    yield return code;
                } else if (code.LoadsConstant(DEFAULTGRID_RESOLUTION)) {
                    code.operand = ELECTRICITYGRID_RESOLUTION;
                    yield return code;
                } else if (code.LoadsConstant(DEFAULTGRID_RESOLUTION - 1)) {
                    code.operand = ELECTRICITYGRID_RESOLUTION - 1;
                    yield return code;
                } else if (code.LoadsConstant(defHalfGrid)) {
                    code.operand = halfGrid;
                    yield return code;
                } else {
                    yield return code;
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> AwakeTranspiler(IEnumerable<CodeInstruction> instructions) {
            foreach (var code in instructions) {
                if (code.LoadsConstant(DEFAULTGRID_RESOLUTION)) {
                    code.operand = ELECTRICITYGRID_RESOLUTION;
                    yield return code;
                } else if (code.LoadsConstant(DEFAULTGRID_RESOLUTION - 1)) {
                    code.operand = ELECTRICITYGRID_RESOLUTION - 1;
                    yield return code;
                } else {
                    yield return code;
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> CheckConductivityTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceConstants(instructions);

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> CheckElectricityTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceConstants(instructions);

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> ConductToCellTranspiler(IEnumerable<CodeInstruction> instructions) {
            yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(EElectricityManager), nameof(EElectricityManager.m_pulseGroups)));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ElectricityManager), "m_electricityGrid"));
            yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(EElectricityManager), nameof(EElectricityManager.m_pulseUnits)));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ElectricityManager), "m_pulseGroupCount"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(ElectricityManager), "m_pulseUnitEnd"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(ElectricityManager), "m_canContinue"));
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Ldarg_2);
            yield return new CodeInstruction(OpCodes.Ldarg_3);
            yield return new CodeInstruction(OpCodes.Ldarg_S, 4);
            yield return new CodeInstruction(OpCodes.Ldarg_S, 5);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EElectricityManager), nameof(EElectricityManager.ConductToCell)));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> ConductToCellsTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceConstants(instructions);

        private static IEnumerable<CodeInstruction> ConductToNodeTranspiler(IEnumerable<CodeInstruction> instructions) {
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ElectricityManager), nameof(ElectricityManager.m_nodeGroups)));
            yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(EElectricityManager), nameof(EElectricityManager.m_pulseGroups)));
            yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(EElectricityManager), nameof(EElectricityManager.m_pulseUnits)));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ElectricityManager), "m_pulseGroupCount"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(ElectricityManager), "m_pulseUnitEnd"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(ElectricityManager), "m_canContinue"));
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Ldarg_2);
            yield return new CodeInstruction(OpCodes.Ldarg_3);
            yield return new CodeInstruction(OpCodes.Ldarg_S, 4);
            yield return new CodeInstruction(OpCodes.Ldarg_S, 5);
            yield return new CodeInstruction(OpCodes.Ldarg_S, 6);
            yield return new CodeInstruction(OpCodes.Ldarg_S, 7);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EElectricityManager), nameof(EElectricityManager.ConductToNode)));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> ConductToNodesTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceConstants(instructions);

        private static IEnumerable<CodeInstruction> GetRootGroupTranspiler(IEnumerable<CodeInstruction> instructions) {
            yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(EElectricityManager), nameof(EElectricityManager.m_pulseGroups)));
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EElectricityManager), nameof(EElectricityManager.GetRootGroup)));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> SimulationStepImplTranspiler(IEnumerable<CodeInstruction> instructions) {
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(ElectricityManager), "m_pulseGroupCount"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(ElectricityManager), "m_pulseUnitStart"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(ElectricityManager), "m_pulseUnitEnd"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(ElectricityManager), "m_processedCells"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(ElectricityManager), "m_conductiveCells"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(ElectricityManager), "m_canContinue"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ElectricityManager), "m_electricityGrid"));
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EElectricityManager), nameof(EElectricityManager.SimulationStepImpl)));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> TryDumpElectricityTranpiler(IEnumerable<CodeInstruction> instructions) => ReplaceConstants(instructions);

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> TryFetchElectricityTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceConstants(instructions);

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> UpdateElectricityMappingTranspiler(IEnumerable<CodeInstruction> instructions) {
            const float defZ = 1f / (ELECTRICITYGRID_CELL_SIZE * DEFAULTGRID_RESOLUTION);
            const float Z = 1f / (ELECTRICITYGRID_CELL_SIZE * ELECTRICITYGRID_RESOLUTION);
            const float defW = 1f / DEFAULTGRID_RESOLUTION;
            const float W = 1f / ELECTRICITYGRID_RESOLUTION;
            foreach (var code in instructions) {
                if (code.LoadsConstant(defZ)) {
                    code.operand = Z;
                    yield return code;
                } else if (code.LoadsConstant(defW)) {
                    code.operand = W;
                    yield return code;
                } else {
                    yield return code;
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> UpdateGridTranspiler(IEnumerable<CodeInstruction> instructions) {
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ElectricityManager), "m_electricityGrid"));
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Ldarg_2);
            yield return new CodeInstruction(OpCodes.Ldarg_3);
            yield return new CodeInstruction(OpCodes.Ldarg_S, 4);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EElectricityManager), nameof(EElectricityManager.UpdateGrid)));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> UpdateTextureTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceConstants(instructions);

        private static IEnumerable<CodeInstruction> DeserializeTranspiler(IEnumerable<CodeInstruction> instructions) {
            MethodInfo getEMInstance = AccessTools.PropertyGetter(typeof(Singleton<ElectricityManager>), nameof(Singleton<ElectricityManager>.instance));
            MethodInfo getLMInstance = AccessTools.PropertyGetter(typeof(Singleton<LoadingManager>), nameof(Singleton<LoadingManager>.instance));
            FieldInfo canContinue = AccessTools.Field(typeof(ElectricityManager), "m_canContinue");
            using (var codes = instructions.GetEnumerator()) {
                while (codes.MoveNext()) {
                    var cur = codes.Current;
                    if (cur.opcode == OpCodes.Call && cur.operand == getEMInstance && codes.MoveNext()) {
                        var next = codes.Current;
                        if (next.opcode == OpCodes.Stloc_0) {
                            yield return cur;
                            yield return next;
                            yield return new CodeInstruction(OpCodes.Ldloc_0);
                            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(ElectricityManager), "m_electricityTexture"));
                            yield return new CodeInstruction(OpCodes.Ldloc_0);
                            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(ElectricityManager), "m_modifiedX2"));
                            yield return new CodeInstruction(OpCodes.Ldloc_0);
                            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(ElectricityManager), "m_modifiedZ2"));
                            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EElectricityManager), nameof(EElectricityManager.EnsureCapacity)));
                        } else {
                            yield return cur;
                            yield return next;
                        }
                    } else if (cur.opcode == OpCodes.Stfld && cur.operand == canContinue && codes.MoveNext()) {
                        var next = codes.Current;
                        if (next.opcode == OpCodes.Call && next.operand == getLMInstance) {
                            yield return cur;
                            yield return new CodeInstruction(OpCodes.Ldloc_0);
                            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(ElectricityManager), "m_electricityGrid"));
                            yield return new CodeInstruction(OpCodes.Ldloc_0);
                            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(ElectricityManager), "m_pulseGroups"));
                            yield return new CodeInstruction(OpCodes.Ldloc_0);
                            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(ElectricityManager), "m_pulseUnits"));
                            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EElectricityManager), nameof(EElectricityManager.IntegratedDeserialize)));
                            yield return next;
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

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> AfterDeserializeTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceConstants(instructions);

        private static IEnumerable<CodeInstruction> SerializeTranspiler(IEnumerable<CodeInstruction> instructions) {
            MethodInfo getEMInstance = AccessTools.PropertyGetter(typeof(Singleton<ElectricityManager>), nameof(Singleton<ElectricityManager>.instance));
            using (var codes = instructions.GetEnumerator()) {
                while (codes.MoveNext()) {
                    var cur = codes.Current;
                    if (cur.opcode == OpCodes.Call && cur.operand == getEMInstance && codes.MoveNext()) {
                        var next = codes.Current;
                        yield return cur;
                        yield return next;
                        if (next.opcode == OpCodes.Stloc_0) {
                            yield return new CodeInstruction(OpCodes.Ldloc_0);
                            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(ElectricityManager), "m_electricityGrid"));
                            yield return new CodeInstruction(OpCodes.Ldloc_0);
                            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(ElectricityManager), "m_pulseGroups"));
                            yield return new CodeInstruction(OpCodes.Ldloc_0);
                            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(ElectricityManager), "m_pulseUnits"));
                            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EElectricityManager), nameof(EElectricityManager.IntegratedSerialize)));
                        }
                    } else {
                        yield return cur;
                    }
                }
            }
        }

        internal void Enable(Harmony harmony) {
            try {
                harmony.Patch(AccessTools.Method(typeof(ElectricityManager), @"Awake"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EElectricityManagerPatch), nameof(AwakeTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch ElectricityManager::Awake");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(ElectricityManager), @"Awake"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(ElectricityManager), @"CheckConductivity"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EElectricityManagerPatch), nameof(CheckConductivityTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch ElectricityManager::CheckConductivity");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(ElectricityManager), @"CheckConductivity"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(ElectricityManager), @"CheckElectricity"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EElectricityManagerPatch), nameof(CheckElectricityTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch ElectricityManager::CheckElectricity");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(ElectricityManager), @"CheckElectricity"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(ElectricityManager), @"ConductToCell"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EElectricityManagerPatch), nameof(ConductToCellTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch ElectricityManager::ConductToCell");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(ElectricityManager), @"ConductToCell"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(ElectricityManager), @"ConductToCells"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EElectricityManagerPatch), nameof(ConductToCellsTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch ElectricityManager::ConductToCells");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(ElectricityManager), @"ConductToCells"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(ElectricityManager), @"ConductToNode"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EElectricityManagerPatch), nameof(ConductToNodeTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch ElectricityManager::ConductToNode");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(ElectricityManager), @"ConductToNode"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(ElectricityManager), @"ConductToNodes"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EElectricityManagerPatch), nameof(ConductToNodesTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch ElectricityManager::ConductToNodes");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(ElectricityManager), @"ConductToNodes"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(ElectricityManager), @"GetRootGroup"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EElectricityManagerPatch), nameof(GetRootGroupTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch ElectricityManager::GetRootGroup");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(ElectricityManager), @"GetRootGroup"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(ElectricityManager), @"SimulationStepImpl"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EElectricityManagerPatch), nameof(SimulationStepImplTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch ElectricityManager::SimulationStepImpl");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(ElectricityManager), @"SimulationStepImpl"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(ElectricityManager), @"TryDumpElectricity", new Type[] { typeof(int), typeof(int), typeof(int), typeof(int) }),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EElectricityManagerPatch), nameof(TryDumpElectricityTranpiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch ElectricityManager::TryDumpElectricity(Vector3 pos, int rate, int max)");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(ElectricityManager), @"TryDumpElectricity", new Type[] { typeof(int), typeof(int), typeof(int), typeof(int) }),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(ElectricityManager), @"TryDumpElectricity", new Type[] { typeof(Vector3), typeof(int), typeof(int) }),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EElectricityManagerPatch), nameof(TryDumpElectricityTranpiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch ElectricityManager::TryDumpElectricity(Vector3 pos, int rate, int max)");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(ElectricityManager), @"TryDumpElectricity", new Type[] { typeof(Vector3), typeof(int), typeof(int) }),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(ElectricityManager), @"TryFetchElectricity"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EElectricityManagerPatch), nameof(TryFetchElectricityTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch ElectricityManager::TryFetchElectricity");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(ElectricityManager), @"TryFetchElectricity"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(ElectricityManager), @"UpdateElectricityMapping"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EElectricityManagerPatch), nameof(UpdateElectricityMappingTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch ElectricityManager::UpdateElectricityMapping");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(ElectricityManager), @"UpdateElectricityMapping"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(ElectricityManager), @"UpdateGrid"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EElectricityManagerPatch), nameof(UpdateGridTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch ElectricityManager::UpdateGrid");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(ElectricityManager), @"UpdateGrid"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(ElectricityManager), @"UpdateTexture"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EElectricityManagerPatch), nameof(UpdateTextureTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch ElectricityManager::UpdateTexture");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(ElectricityManager), @"UpdateTexture"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(ElectricityManager.Data), nameof(ElectricityManager.Data.Deserialize)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EElectricityManagerPatch), nameof(DeserializeTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch ElectricityManager::Data::Deserialize");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(ElectricityManager.Data), nameof(ElectricityManager.Data.Deserialize)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(ElectricityManager.Data), nameof(ElectricityManager.Data.AfterDeserialize)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EElectricityManagerPatch), nameof(AfterDeserializeTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch ElectricityManager::Data::AfterDeserialize");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(ElectricityManager.Data), nameof(ElectricityManager.Data.AfterDeserialize)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(ElectricityManager.Data), nameof(ElectricityManager.Data.Serialize)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EElectricityManagerPatch), nameof(SerializeTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch ElectricityManager::Data::Serialize");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(ElectricityManager.Data), nameof(ElectricityManager.Data.Serialize)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
        }

        internal void Disable(Harmony harmony) {
            harmony.Unpatch(AccessTools.Method(typeof(ElectricityManager), @"Awake"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(ElectricityManager), @"CheckConductivity"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(ElectricityManager), @"CheckElectricity"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(ElectricityManager), @"ConductToCell"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(ElectricityManager), @"ConductToCells"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(ElectricityManager), @"ConductToNode"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(ElectricityManager), @"ConductToNodes"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(ElectricityManager), @"GetRootGroup"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(ElectricityManager), @"SimulationStepImpl"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(ElectricityManager), @"TryDumpElectricity",
                new Type[] { typeof(int), typeof(int), typeof(int), typeof(int) }), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(ElectricityManager), @"TryDumpElectricity",
                new Type[] { typeof(Vector3), typeof(int), typeof(int) }), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(ElectricityManager), @"TryFetchElectricity"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(ElectricityManager), @"UpdateElectricityMapping"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(ElectricityManager), @"UpdateGrid"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(ElectricityManager), @"UpdateTexture"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(ElectricityManager.Data), nameof(ElectricityManager.Data.Deserialize)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(ElectricityManager.Data), nameof(ElectricityManager.Data.AfterDeserialize)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(ElectricityManager.Data), nameof(ElectricityManager.Data.Serialize)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
        }
    }
}
