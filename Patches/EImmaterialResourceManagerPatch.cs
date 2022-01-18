using ColossalFramework;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using static EManagersLib.EImmaterialResourceManager;

namespace EManagersLib.Patches {
    internal readonly struct EImmaterialResourceManagerPatch {
        private static IEnumerable<CodeInstruction> ReplaceConstants(IEnumerable<CodeInstruction> instructions) {
            const float defHalfGrid = DEFAULTGRID_RESOLUTION / 2f;
            const float halfGrid = RESOURCEGRID_RESOLUTION / 2f;
            foreach (var code in instructions) {
                if (code.LoadsConstant(DEFAULTGRID_RESOLUTION)) {
                    code.operand = RESOURCEGRID_RESOLUTION;
                } else if (code.LoadsConstant(defHalfGrid)) {
                    code.operand = halfGrid;
                } else if (code.LoadsConstant(DEFAULTGRID_RESOLUTION - 1)) {
                    code.operand = RESOURCEGRID_RESOLUTION - 1;
                }
                yield return code;
            }
        }

        private static IEnumerable<CodeInstruction> AddLocalResourceTranspiler(IEnumerable<CodeInstruction> instructions) {
            foreach (var code in instructions) {
                if (code.LoadsConstant(DEFAULTGRID_RESOLUTION)) {
                    code.operand = RESOURCEGRID_RESOLUTION;
                } else if (code.LoadsConstant(DEFAULTGRID_RESOLUTION - 1)) {
                    code.operand = RESOURCEGRID_RESOLUTION - 1;
                }
                yield return code;
            }
        }

        private static IEnumerable<CodeInstruction> AreaModifiedTranspiler(IEnumerable<CodeInstruction> instructions) {
            foreach (var code in instructions) {
                if (code.LoadsConstant(DEFAULTGRID_RESOLUTION)) {
                    code.operand = RESOURCEGRID_RESOLUTION;
                } else if (code.LoadsConstant(DEFAULTGRID_RESOLUTION - 1)) {
                    code.operand = RESOURCEGRID_RESOLUTION - 1;
                }
                yield return code;
            }
        }

        private static IEnumerable<CodeInstruction> AddResourceTranspiler(IEnumerable<CodeInstruction> instructions) {
            const float defHalfGrid = DEFAULTGRID_RESOLUTION / 2f;
            const float halfGrid = RESOURCEGRID_RESOLUTION / 2f;
            foreach (var code in instructions) {
                if (code.LoadsConstant(DEFAULTGRID_RESOLUTION)) {
                    code.operand = RESOURCEGRID_RESOLUTION;
                } else if (code.LoadsConstant(DEFAULTGRID_RESOLUTION - 3)) {
                    code.operand = RESOURCEGRID_RESOLUTION - 3;
                } else if (code.LoadsConstant(defHalfGrid)) {
                    code.operand = halfGrid;
                }
                yield return code;
            }
        }

        private static IEnumerable<CodeInstruction> AddObstructedResourceTranspiler(IEnumerable<CodeInstruction> instructions) {
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ImmaterialResourceManager), "m_localTempResources"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ImmaterialResourceManager), "m_tempSectorSlopes"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ImmaterialResourceManager), "m_tempSectorDistances"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ImmaterialResourceManager), "m_tempCircleMinX"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ImmaterialResourceManager), "m_tempCircleMaxX"));
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Ldarg_2);
            yield return new CodeInstruction(OpCodes.Ldarg_3);
            yield return new CodeInstruction(OpCodes.Ldarg_S, 4);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EImmaterialResourceManager), nameof(EImmaterialResourceManager.AddObstructedResource)));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        private static IEnumerable<CodeInstruction> AddParkResourceTranspiler(IEnumerable<CodeInstruction> instructions) {
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ImmaterialResourceManager), "m_localTempResources"));
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Ldarg_2);
            yield return new CodeInstruction(OpCodes.Ldarg_3);
            yield return new CodeInstruction(OpCodes.Ldarg_S, 4);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EImmaterialResourceManager), nameof(EImmaterialResourceManager.AddParkResource)));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        private static IEnumerable<CodeInstruction> AwakeTranspiler(IEnumerable<CodeInstruction> instructions) {
            FieldInfo tempAreaQueue = AccessTools.Field(typeof(ImmaterialResourceManager), "m_tempAreaQueue");
            FieldInfo tempAreaCost = AccessTools.Field(typeof(ImmaterialResourceManager), "m_tempAreaCost");
            FieldInfo tempCircleMinX = AccessTools.Field(typeof(ImmaterialResourceManager), "m_tempCircleMinX");
            FieldInfo tempCircleMaxX = AccessTools.Field(typeof(ImmaterialResourceManager), "m_tempCircleMaxX");
            FieldInfo tempSectorSlopes = AccessTools.Field(typeof(ImmaterialResourceManager), "m_tempSectorSlopes");
            FieldInfo tempSectorDistances = AccessTools.Field(typeof(ImmaterialResourceManager), "m_tempSectorDistances");
            FieldInfo modifiedX1 = AccessTools.Field(typeof(ImmaterialResourceManager), "m_modifiedX1");
            FieldInfo modifiedX2 = AccessTools.Field(typeof(ImmaterialResourceManager), "m_modifiedX2");
            using (var codes = instructions.GetEnumerator()) {
                while (codes.MoveNext()) {
                    var cur = codes.Current;
                    if (cur.opcode == OpCodes.Ldarg_0 && codes.MoveNext()) {
                        var next = codes.Current;
                        if (next.LoadsConstant(DEFAULTGRID_RESOLUTION) && codes.MoveNext()) {
                            var next1 = codes.Current;
                            if (next1.opcode == OpCodes.Newarr && codes.MoveNext()) {
                                var next2 = codes.Current;
                                if (next2.opcode == OpCodes.Stfld) {
                                    if (next2.operand == tempCircleMinX) {
                                        next.operand = RESOURCEGRID_RESOLUTION;
                                    } else if (next2.operand == tempCircleMaxX) {
                                        next.operand = RESOURCEGRID_RESOLUTION;
                                    } else if (next2.operand == tempSectorSlopes) {
                                        next.operand = RESOURCEGRID_RESOLUTION;
                                    } else if (next2.operand == tempSectorDistances) {
                                        next.operand = RESOURCEGRID_RESOLUTION;
                                    } else if (next2.operand == modifiedX1 || next2.operand == modifiedX2) {
                                        next.operand = RESOURCEGRID_RESOLUTION;
                                    }
                                }
                                yield return cur;
                                yield return next;
                                yield return next1;
                                yield return next2;
                            } else if (next1.LoadsConstant(DEFAULTGRID_RESOLUTION)) {
                                next.operand = RESOURCEGRID_RESOLUTION;
                                next1.operand = RESOURCEGRID_RESOLUTION;
                                yield return new CodeInstruction(OpCodes.Ldarg_0);
                                yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ImmaterialResourceManager), "m_resourceMapVisible"));
                                yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EImmaterialResourceManager), nameof(EImmaterialResourceManager.Awake)));
                                yield return cur;
                                yield return next;
                                yield return next1;
                            } else {
                                yield return cur;
                                yield return next;
                                yield return next1;
                            }
                        } else {
                            yield return cur;
                            yield return next;
                        }
                    } else if (cur.LoadsConstant(DEFAULTGRID_RESOLUTION)) {
                        cur.operand = RESOURCEGRID_RESOLUTION;
                        yield return cur;
                    } else if (cur.LoadsConstant(DEFAULTGRID_RESOLUTION - 1)) {
                        cur.operand = RESOURCEGRID_RESOLUTION - 1;
                        yield return cur;
                    } else {
                        yield return cur;
                    }
                }
            }
        }

        private static IEnumerable<CodeInstruction> CalculateLocalResourcesTranspiler(IEnumerable<CodeInstruction> instructions) {
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Ldarg_2);
            yield return new CodeInstruction(OpCodes.Ldarg_3);
            yield return new CodeInstruction(OpCodes.Ldarg_S, 4);
            yield return new CodeInstruction(OpCodes.Ldarg_S, 5);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EImmaterialResourceManager), nameof(EImmaterialResourceManager.CalculateLocalResources)));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        private static IEnumerable<CodeInstruction> CheckResourceTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceConstants(instructions);

        private static IEnumerable<CodeInstruction> CheckLocalResourceTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceConstants(instructions);

        private static IEnumerable<CodeInstruction> CheckLocalResourceRadialTranspiler(IEnumerable<CodeInstruction> instructions) {
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ImmaterialResourceManager), "m_localFinalResources"));
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Ldarg_2);
            yield return new CodeInstruction(OpCodes.Ldarg_3);
            yield return new CodeInstruction(OpCodes.Ldarg_S, 4);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EImmaterialResourceManager), nameof(EImmaterialResourceManager.CheckLocalResource)));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        private static IEnumerable<CodeInstruction> CheckLocalResourcesTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceConstants(instructions);

        private static IEnumerable<CodeInstruction> UpdateResourceMappingTranspiler(IEnumerable<CodeInstruction> instructions) {
            using (var codes = instructions.GetEnumerator()) {
                while (codes.MoveNext()) {
                    var cur = codes.Current;
                    if (cur.LoadsConstant(1f / (38.4f * DEFAULTGRID_RESOLUTION))) {
                        cur.operand = 1f / (38.4f * RESOURCEGRID_RESOLUTION);
                        yield return cur;
                    } else if (cur.LoadsConstant(DEFAULTGRID_RESOLUTION - 1) && codes.MoveNext()) {
                        var next = codes.Current;
                        if (next.LoadsConstant(DEFAULTGRID_RESOLUTION - 1)) {
                            cur.operand = RESOURCEGRID_RESOLUTION - 1;
                            next.operand = RESOURCEGRID_RESOLUTION - 1;
                        }
                        yield return cur;
                        yield return next;
                    } else {
                        yield return cur;
                    }
                }
            }
        }

        private static IEnumerable<CodeInstruction> UpdateTextureTranspiler(IEnumerable<CodeInstruction> instructions) {
            foreach (var code in instructions) {
                if (code.LoadsConstant(DEFAULTGRID_RESOLUTION)) {
                    code.operand = RESOURCEGRID_RESOLUTION;
                }
                yield return code;
            }
        }

        private static IEnumerable<CodeInstruction> SimulationStepImplTranspiler(IEnumerable<CodeInstruction> instructions) {
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ImmaterialResourceManager), "m_localTempResources"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ImmaterialResourceManager), "m_localFinalResources"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ImmaterialResourceManager), "m_globalTempResources"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ImmaterialResourceManager), "m_globalFinalResources"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ImmaterialResourceManager), "m_totalTempResources"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ImmaterialResourceManager), "m_totalFinalResources"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ImmaterialResourceManager), "m_totalTempResourcesMul"));
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EImmaterialResourceManager), nameof(EImmaterialResourceManager.SimulationStepImpl)));
            yield return new CodeInstruction(OpCodes.Ret);

        }

        private static IEnumerable<CodeInstruction> DeserializeTranspiler(IEnumerable<CodeInstruction> instructions) {
            MethodInfo getIRMInstance = AccessTools.PropertyGetter(typeof(Singleton<ImmaterialResourceManager>), nameof(Singleton<ImmaterialResourceManager>.instance));
            MethodInfo getLMInstance = AccessTools.PropertyGetter(typeof(Singleton<LoadingManager>), nameof(Singleton<LoadingManager>.instance));
            using (var codes = instructions.GetEnumerator()) {
                while (codes.MoveNext()) {
                    var cur = codes.Current;
                    if (cur.opcode == OpCodes.Call && cur.operand == getIRMInstance && codes.MoveNext()) {
                        var next = codes.Current;
                        yield return cur;
                        yield return next;
                        if (next.opcode == OpCodes.Stloc_0) {
                            yield return new CodeInstruction(OpCodes.Ldloc_0);
                            yield return new CodeInstruction(OpCodes.Ldloc_0);
                            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(ImmaterialResourceManager), "m_resourceTexture"));
                            yield return new CodeInstruction(OpCodes.Ldloc_0);
                            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(ImmaterialResourceManager), "m_modifiedX1"));
                            yield return new CodeInstruction(OpCodes.Ldloc_0);
                            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(ImmaterialResourceManager), "m_modifiedX2"));
                            yield return new CodeInstruction(OpCodes.Ldloc_0);
                            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(ImmaterialResourceManager), "m_tempCircleMinX"));
                            yield return new CodeInstruction(OpCodes.Ldloc_0);
                            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(ImmaterialResourceManager), "m_tempCircleMaxX"));
                            yield return new CodeInstruction(OpCodes.Ldloc_0);
                            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(ImmaterialResourceManager), "m_tempSectorSlopes"));
                            yield return new CodeInstruction(OpCodes.Ldloc_0);
                            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(ImmaterialResourceManager), "m_tempSectorDistances"));
                            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EImmaterialResourceManager), nameof(EImmaterialResourceManager.EnsureCapacity)));
                        }
                    } else if (cur.opcode == OpCodes.Blt && codes.MoveNext()) {
                        var next = codes.Current;
                        yield return cur;
                        if (next.opcode == OpCodes.Call && next.operand == getLMInstance) {
                            yield return new CodeInstruction(OpCodes.Ldloc_0).WithLabels(next.ExtractLabels());
                            yield return new CodeInstruction(OpCodes.Ldloc_0);
                            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(ImmaterialResourceManager), "m_localFinalResources"));
                            yield return new CodeInstruction(OpCodes.Ldloc_0);
                            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(ImmaterialResourceManager), "m_localTempResources"));
                            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EImmaterialResourceManager), nameof(EImmaterialResourceManager.IntegratedDeserialize)));
                        }
                        yield return next;
                    } else {
                        yield return cur;
                    }
                }
            }
        }

        internal void Enable(Harmony harmony) {
            try {
                harmony.Patch(AccessTools.Method(typeof(ImmaterialResourceManager), "AddLocalResource"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EImmaterialResourceManagerPatch), nameof(AddLocalResourceTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch ImmaterialResourceManager::AddLocalResource");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(ImmaterialResourceManager), "AddLocalResource"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(ImmaterialResourceManager), nameof(ImmaterialResourceManager.AddResource),
                    new Type[] { typeof(ImmaterialResourceManager.Resource), typeof(int), typeof(Vector3), typeof(float) }),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EImmaterialResourceManagerPatch), nameof(AddResourceTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch ImmaterialResourceManager::AddResource(Resource, int, Vector3, float)");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(ImmaterialResourceManager), nameof(ImmaterialResourceManager.AddResource),
                    new Type[] { typeof(ImmaterialResourceManager.Resource), typeof(int), typeof(Vector3), typeof(float) }),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(ImmaterialResourceManager), nameof(ImmaterialResourceManager.AddObstructedResource)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EImmaterialResourceManagerPatch), nameof(AddObstructedResourceTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch ImmaterialResourceManager::AddObstructedResource");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(ImmaterialResourceManager), nameof(ImmaterialResourceManager.AddObstructedResource)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(ImmaterialResourceManager), nameof(ImmaterialResourceManager.AddParkResource)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EImmaterialResourceManagerPatch), nameof(AddParkResourceTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch ImmaterialResourceManager::AddParkResource");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(ImmaterialResourceManager), nameof(ImmaterialResourceManager.AddParkResource)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(ImmaterialResourceManager), "AreaModified"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EImmaterialResourceManagerPatch), nameof(AreaModifiedTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch ImmaterialResourceManager::AreaModified");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(ImmaterialResourceManager), "AreaModified"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(ImmaterialResourceManager), "Awake"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EImmaterialResourceManagerPatch), nameof(AwakeTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch ImmaterialResourceManager::Awake");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(ImmaterialResourceManager), "Awake"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(ImmaterialResourceManager), "CalculateLocalResources"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EImmaterialResourceManagerPatch), nameof(CalculateLocalResourcesTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch ImmaterialResourceManager::CalculateLocalResources");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(ImmaterialResourceManager), "CalculateLocalResources"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(ImmaterialResourceManager), nameof(ImmaterialResourceManager.CheckResource)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EImmaterialResourceManagerPatch), nameof(CheckResourceTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch ImmaterialResourceManager::CheckResource");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(ImmaterialResourceManager), nameof(ImmaterialResourceManager.CheckResource)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(ImmaterialResourceManager), nameof(ImmaterialResourceManager.CheckLocalResource),
                    new Type[] { typeof(ImmaterialResourceManager.Resource), typeof(Vector3), typeof(int).MakeByRefType() }),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EImmaterialResourceManagerPatch), nameof(CheckLocalResourceTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch ImmaterialResourceManager::CheckLocalResource(ImmaterialResourceManager.Resource resource, Vector3 position, out int local)");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(ImmaterialResourceManager), nameof(ImmaterialResourceManager.CheckLocalResource),
                    new Type[] { typeof(ImmaterialResourceManager.Resource), typeof(Vector3), typeof(int).MakeByRefType() }),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(ImmaterialResourceManager), nameof(ImmaterialResourceManager.CheckLocalResource),
                    new Type[] { typeof(ImmaterialResourceManager.Resource), typeof(Vector3), typeof(float), typeof(int).MakeByRefType() }),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EImmaterialResourceManagerPatch), nameof(CheckLocalResourceRadialTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch ImmaterialResourceManager::CheckLocalResource(ImmaterialResourceManager.Resource resource, Vector3 position, float radius, out int local)");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(ImmaterialResourceManager), nameof(ImmaterialResourceManager.CheckLocalResource),
                    new Type[] { typeof(ImmaterialResourceManager.Resource), typeof(Vector3), typeof(float), typeof(int).MakeByRefType() }),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(ImmaterialResourceManager), nameof(ImmaterialResourceManager.CheckLocalResources)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EImmaterialResourceManagerPatch), nameof(CheckLocalResourcesTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch ImmaterialResourceManager::CheckLocalResources");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(ImmaterialResourceManager), nameof(ImmaterialResourceManager.CheckLocalResources)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(ImmaterialResourceManager), "SimulationStepImpl"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EImmaterialResourceManagerPatch), nameof(SimulationStepImplTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch ImmaterialResourceManager::SimulationStepImpl");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(ImmaterialResourceManager), "SimulationStepImpl"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(ImmaterialResourceManager), "UpdateResourceMapping"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EImmaterialResourceManagerPatch), nameof(UpdateResourceMappingTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch ImmaterialResourceManager::UpdateResourceMapping");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(ImmaterialResourceManager), "UpdateResourceMapping"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                ReversePatcher reverse = harmony.CreateReversePatcher(AccessTools.Method(typeof(ImmaterialResourceManager), "UpdateResourceMapping"),
                    new HarmonyMethod(AccessTools.Method(typeof(EImmaterialResourceManager), nameof(EImmaterialResourceManager.UpdateResourceMapping))));
                reverse.Patch(HarmonyReversePatchType.Snapshot);
            } catch (Exception e) {
                EUtils.ELog("Failed to create reverse patched method for ImmaterialResourceManager::UpdateResourceMapping");
                EUtils.ELog(e.Message);
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(ImmaterialResourceManager), "UpdateTexture"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EImmaterialResourceManagerPatch), nameof(UpdateTextureTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch ImmaterialResourceManager::UpdateTexture");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(ImmaterialResourceManager), "UpdateTexture"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(ImmaterialResourceManager.Data), nameof(ImmaterialResourceManager.Data.Deserialize)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EImmaterialResourceManagerPatch), nameof(DeserializeTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch ImmaterialResourceManager::Data::Deserialize");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(ImmaterialResourceManager.Data), nameof(ImmaterialResourceManager.Data.Deserialize)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }

        }

        internal void Disable(Harmony harmony) {
            harmony.Unpatch(AccessTools.Method(typeof(ImmaterialResourceManager), "AddLocalResource"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(ImmaterialResourceManager), nameof(ImmaterialResourceManager.AddResource),
                new Type[] { typeof(ImmaterialResourceManager.Resource), typeof(int), typeof(Vector3), typeof(float) }), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(ImmaterialResourceManager), nameof(ImmaterialResourceManager.AddObstructedResource)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(ImmaterialResourceManager), nameof(ImmaterialResourceManager.AddParkResource)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(ImmaterialResourceManager), "AreaModified"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(ImmaterialResourceManager), "Awake"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(ImmaterialResourceManager), "CalculateLocalResources"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(ImmaterialResourceManager), nameof(ImmaterialResourceManager.CheckResource)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(ImmaterialResourceManager), nameof(ImmaterialResourceManager.CheckLocalResource),
                new Type[] { typeof(ImmaterialResourceManager.Resource), typeof(Vector3), typeof(int).MakeByRefType() }), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(ImmaterialResourceManager), nameof(ImmaterialResourceManager.CheckLocalResource),
                new Type[] { typeof(ImmaterialResourceManager.Resource), typeof(Vector3), typeof(float), typeof(int).MakeByRefType() }), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(ImmaterialResourceManager), nameof(ImmaterialResourceManager.CheckLocalResources)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(ImmaterialResourceManager), "SimulationStepImpl"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(ImmaterialResourceManager), "UpdateResourceMapping"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(ImmaterialResourceManager), "UpdateResourceMapping"), HarmonyPatchType.ReversePatch, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(ImmaterialResourceManager), "UpdateTexture"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(ImmaterialResourceManager.Data), nameof(ImmaterialResourceManager.Data.Deserialize)),
                HarmonyPatchType.Transpiler, EModule.HARMONYID);
        }
    }
}
