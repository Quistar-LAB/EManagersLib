using ColossalFramework;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using UnityEngine;
using static EManagersLib.EDistrictManager;

namespace EManagersLib.Patches {
    internal class EDistrictManagerPatch {
        internal static IEnumerable<CodeInstruction> ReplaceDistrictConstants(IEnumerable<CodeInstruction> instructions) {
            const int DefTotalResolution = DEFAULTGRID_RESOLUTION * DEFAULTGRID_RESOLUTION;
            const int TotalResolution = DISTRICTGRID_RESOLUTION * DISTRICTGRID_RESOLUTION;
            const int DefHalfResolution = DefTotalResolution / 4;
            const int HalfResolution = TotalResolution / 4;
            const int defHalfGridSize = DEFAULTGRID_RESOLUTION / 2;
            const int halfGridSize = DISTRICTGRID_RESOLUTION / 2;
            using (var codes = instructions.GetEnumerator()) {
                while (codes.MoveNext()) {
                    var cur = codes.Current;
                    if (cur.LoadsConstant(DefTotalResolution)) {
                        cur.operand = TotalResolution;
                        yield return cur;
                    } else if (cur.LoadsConstant(DefHalfResolution)) {
                        cur.operand = HalfResolution;
                        yield return cur;
                    } else if (cur.LoadsConstant(DEFAULTGRID_RESOLUTION)) {
                        cur.operand = DISTRICTGRID_RESOLUTION;
                        yield return cur;
                    } else if (cur.LoadsConstant(DEFAULTGRID_RESOLUTION / 2f)) {
                        cur.operand = DISTRICTGRID_RESOLUTION / 2f;
                        yield return cur;
                    } else if (cur.LoadsConstant(DEFAULTGRID_RESOLUTION - 1)) {
                        cur.operand = DISTRICTGRID_RESOLUTION - 1;
                        yield return cur;
                    } else if (cur.LoadsConstant(defHalfGridSize - 1) && codes.MoveNext()) {
                        var next = codes.Current;
                        if (next.opcode == OpCodes.And) {
                            cur.operand = halfGridSize;
                            next.opcode = OpCodes.Rem;
                        }
                        yield return cur;
                        yield return next;
                    } else if (cur.opcode == OpCodes.Ldc_I4_8 && codes.MoveNext()) {
                        var next = codes.Current;
                        if (next.opcode == OpCodes.Shr) {
                            cur.opcode = OpCodes.Ldc_I4;
                            cur.operand = halfGridSize;
                            next.opcode = OpCodes.Div;
                        }
                        yield return cur;
                        yield return next;
                    } else {
                        yield return cur;
                    }
                }
            }
        }

        private static IEnumerable<CodeInstruction> ReplaceDistrictBounds(IEnumerable<CodeInstruction> instructions) {
            const float defVectorZ = 1f / (19.2f * DEFAULTGRID_RESOLUTION);
            const float vectorZ = 1f / (19.2f * DISTRICTGRID_RESOLUTION);
            const float defMaxXZ = 19.2f * DEFAULTGRID_RESOLUTION;
            const float maxXZ = 19.2f * DISTRICTGRID_RESOLUTION;
            const float defHalfSize = 19.2f * (DEFAULTGRID_RESOLUTION / 2f);
            const float halfSize = 19.2f * (DISTRICTGRID_RESOLUTION / 2f);
            foreach (var code in instructions) {
                if (code.LoadsConstant(defVectorZ)) {
                    code.operand = vectorZ;
                    yield return code;
                } else if (code.LoadsConstant(defMaxXZ)) {
                    code.operand = maxXZ;
                    yield return code;
                } else if (code.LoadsConstant(defHalfSize)) {
                    code.operand = halfSize;
                    yield return code;
                } else {
                    yield return code;
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> SetHighlightPolicyTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceDistrictConstants(instructions);

        // Customized array for
        // m_districtGrid = new DistrictManager.Cell[262144];
        // m_parkGrid = new DistrictManager.Cell[262144];
        // m_colorBuffer = new Color32[262144];
        // m_distanceBuffer = new ushort[65536];
        // m_indexBuffer = new ushort[65536];
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> AwakeTranspiler(IEnumerable<CodeInstruction> instructions) {
            const int TotalResolution = DISTRICTGRID_RESOLUTION * DISTRICTGRID_RESOLUTION;
            FieldInfo colorBuffer = AccessTools.Field(typeof(DistrictManager), "m_colorBuffer");
            FieldInfo distanceBuffer = AccessTools.Field(typeof(DistrictManager), "m_distanceBuffer");
            FieldInfo indexBuffer = AccessTools.Field(typeof(DistrictManager), "m_indexBuffer");
            FieldInfo tempData = AccessTools.Field(typeof(DistrictManager), "m_tempData");
            FieldInfo districtsModifiedX2 = AccessTools.Field(typeof(DistrictManager), "m_districtsModifiedX2");
            FieldInfo districtsModifiedZ2 = AccessTools.Field(typeof(DistrictManager), "m_districtsModifiedZ2");
            FieldInfo parksModifiedX2 = AccessTools.Field(typeof(DistrictManager), "m_parksModifiedX2");
            FieldInfo parksModifiedZ2 = AccessTools.Field(typeof(DistrictManager), "m_parksModifiedZ2");
            using (var codes = instructions.GetEnumerator()) {
                while (codes.MoveNext()) {
                    var cur = codes.Current;
                    if (cur.opcode == OpCodes.Ldarg_0 && codes.MoveNext()) {
                        var next = codes.Current;
                        if ((next.opcode == OpCodes.Ldc_I4 || next.opcode == OpCodes.Ldc_I4_S) && codes.MoveNext()) {
                            var next1 = codes.Current;
                            if (next1.opcode == OpCodes.Newarr && codes.MoveNext()) {
                                var next2 = codes.Current;
                                if (next2.opcode == OpCodes.Stfld) {
                                    if (next2.operand != distanceBuffer && next2.operand != indexBuffer && next2.operand != tempData) {
                                        if (next2.operand == colorBuffer) {
                                            next.operand = TotalResolution;
                                        }
                                        yield return cur;
                                        yield return next;
                                        yield return next1;
                                        yield return next2;
                                    }
                                } else {
                                    yield return cur;
                                    yield return next;
                                    yield return next1;
                                    yield return next2;
                                }
                            } else if (next1.opcode == OpCodes.Ldc_I4 || next1.opcode == OpCodes.Ldc_I4_S) {
                                if ((int)next.operand == DEFAULTGRID_RESOLUTION) {
                                    next.operand = DISTRICTGRID_RESOLUTION;
                                    next1.operand = DISTRICTGRID_RESOLUTION;
                                }
                                yield return cur;
                                yield return next;
                                yield return next1;
                            } else if (next1.opcode == OpCodes.Stfld) {
                                if (next1.operand == districtsModifiedX2 || next1.operand == districtsModifiedZ2 || next1.operand == parksModifiedX2 || next1.operand == parksModifiedZ2) {
                                    next.operand = DISTRICTGRID_RESOLUTION - 1;
                                }
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
                    } else {
                        yield return cur;
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> BeginOverlayImplTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceDistrictBounds(instructions);

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> GetDistrictXZTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceDistrictConstants(instructions);

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> GetDistrictVector3Transpiler(IEnumerable<CodeInstruction> instructions) => ReplaceDistrictConstants(instructions);

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> GetDistrictAreaTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceDistrictConstants(instructions);

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> GetParkTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceDistrictConstants(instructions);

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> GetParkAreaTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceDistrictConstants(instructions);

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> ModifyCellTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceDistrictConstants(instructions);

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> ModifyParkCellTranspiler(IEnumerable<CodeInstruction> instructions) {
            int matchCount = 0;
            foreach (var code in instructions) {
                if (code.LoadsConstant(DEFAULTGRID_RESOLUTION)) {
                    code.operand = DISTRICTGRID_RESOLUTION;
                    yield return code;
                } else if (matchCount < 2 && code.LoadsConstant(DEFAULTGRID_RESOLUTION / 2f)) {
                    matchCount++;
                    code.operand = DISTRICTGRID_RESOLUTION / 2f;
                    yield return code;
                } else {
                    yield return code;
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> MoveParkBuildingsTranspiler(IEnumerable<CodeInstruction> instructions) {
            foreach (var code in instructions) {
                if (code.LoadsConstant(DEFAULTGRID_RESOLUTION / 2f)) {
                    code.operand = DISTRICTGRID_RESOLUTION / 2f;
                    yield return code;
                } else if (code.LoadsConstant(DEFAULTGRID_RESOLUTION - 1)) {
                    code.operand = DISTRICTGRID_RESOLUTION - 1;
                    yield return code;
                } else {
                    yield return code;
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> MoveParkSegmentsTranspiler(IEnumerable<CodeInstruction> instructions) {
            foreach (var code in instructions) {
                if (code.LoadsConstant(DEFAULTGRID_RESOLUTION / 2f)) {
                    code.operand = DISTRICTGRID_RESOLUTION / 2f;
                    yield return code;
                } else if (code.LoadsConstant(DEFAULTGRID_RESOLUTION - 1)) {
                    code.operand = DISTRICTGRID_RESOLUTION - 1;
                    yield return code;
                } else {
                    yield return code;
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> MoveParkPropsTranspiler(IEnumerable<CodeInstruction> instructions) {
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(DistrictManager), nameof(DistrictManager.m_parks)));
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Array8<DistrictPark>), nameof(Array8<DistrictPark>.m_buffer)));
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Ldarg_2);
            yield return new CodeInstruction(OpCodes.Ldarg_3);
            yield return new CodeInstruction(OpCodes.Ldarg_S, 4);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EDistrictManager), nameof(EDistrictManager.MoveParkProps)));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> MoveParkTreesTranspiler(IEnumerable<CodeInstruction> instructions) {
            foreach (var code in instructions) {
                if (code.LoadsConstant(DEFAULTGRID_RESOLUTION / 2f)) {
                    code.operand = DISTRICTGRID_RESOLUTION / 2f;
                    yield return code;
                } else if (code.LoadsConstant(DEFAULTGRID_RESOLUTION - 1)) {
                    code.operand = DISTRICTGRID_RESOLUTION - 1;
                    yield return code;
                } else {
                    yield return code;
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> NamesModifiedTranspiler(IEnumerable<CodeInstruction> instructions) {
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(DistrictManager), "m_namesModified"));
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EDistrictManager), nameof(EDistrictManager.NamesModified),
                new Type[] { typeof(DistrictManager), typeof(bool).MakeByRefType() }));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> NamesModifiedCellTranspiler(IEnumerable<CodeInstruction> instructions) {
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(EDistrictManager), nameof(m_tempData)));
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EDistrictManager), nameof(NamesModified),
                new Type[] { typeof(DistrictManager.Cell[]), typeof(TempDistrictData[]) }));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> ParkNamesModifiedTranspiler(IEnumerable<CodeInstruction> instructions) {
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(DistrictManager), "m_namesModified"));
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EDistrictManager), nameof(EDistrictManager.ParkNamesModified)));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> SampleDistrictTranspiler(IEnumerable<CodeInstruction> instructions) {
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EDistrictManager), nameof(EDistrictManager.SampleDistrict)));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> UpdateNamesTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceDistrictBounds(instructions);

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> UpdateTextureTranspiler(IEnumerable<CodeInstruction> instructions) {
            const int defHalfGrid = DEFAULTGRID_RESOLUTION / 2;
            const int halfGrid = DISTRICTGRID_RESOLUTION / 2;
            const int defResolution = DEFAULTGRID_RESOLUTION * DEFAULTGRID_RESOLUTION / 4;
            const int resolution = DISTRICTGRID_RESOLUTION * DISTRICTGRID_RESOLUTION / 4;
            using (var codes = instructions.GetEnumerator()) {
                while (codes.MoveNext()) {
                    var cur = codes.Current;
                    if (cur.opcode == OpCodes.Ldc_I4_5) {
                        cur.opcode = OpCodes.Ldc_I4;
                        cur.operand = EGameAreaManager.CUSTOMGRIDSIZE;
                        yield return cur;
                    } else if (cur.LoadsConstant(defHalfGrid)) {
                        cur.operand = halfGrid;
                        yield return cur;
                    } else if (cur.LoadsConstant(defResolution)) {
                        cur.operand = resolution;
                        yield return cur;
                    } else if (cur.LoadsConstant(DEFAULTGRID_RESOLUTION - 2)) {
                        cur.operand = DISTRICTGRID_RESOLUTION - 2;
                        yield return cur;
                    } else if (cur.LoadsConstant(DEFAULTGRID_RESOLUTION)) {
                        cur.operand = DISTRICTGRID_RESOLUTION;
                        yield return cur;
                    } else {
                        yield return cur;
                    }
                }
            }
        }

        // Assign constant values of 262144 to m_districtGrid and m_parkGrid
        private static IEnumerable<CodeInstruction> SerializeTranspiler(IEnumerable<CodeInstruction> instructions) {
            FieldInfo districtGrid = AccessTools.Field(typeof(DistrictManager), nameof(DistrictManager.m_districtGrid));
            FieldInfo parkGrid = AccessTools.Field(typeof(DistrictManager), nameof(DistrictManager.m_parkGrid));
            foreach (var code in instructions) {
                if (code.opcode == OpCodes.Ldfld && code.operand == districtGrid) {
                    yield return code;
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EDistrictManager), nameof(RepackBuffer)));
                } else if (code.opcode == OpCodes.Ldfld && code.operand == parkGrid) {
                    yield return code;
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EDistrictManager), nameof(RepackBuffer)));
                } else {
                    yield return code;
                }
            }
        }

        // Assign constant values of 262144 to m_districtGrid and m_parkGrid
        private static IEnumerable<CodeInstruction> DeserializeTranspiler(IEnumerable<CodeInstruction> instructions) {
            int sigCount = 0;
            MethodInfo getDMInstance = AccessTools.PropertyGetter(typeof(Singleton<DistrictManager>), nameof(Singleton<DistrictManager>.instance));
            MethodInfo get_lmInstance = AccessTools.PropertyGetter(typeof(Singleton<LoadingManager>), nameof(Singleton<LoadingManager>.instance));
            using (var codes = instructions.GetEnumerator()) {
                while (codes.MoveNext()) {
                    var cur = codes.Current;
                    if ((cur.opcode == OpCodes.Ldloc_3 || (cur.opcode == OpCodes.Ldloc_S && cur.operand is LocalBuilder local && local.LocalIndex == 4)) && codes.MoveNext()) {
                        var next = codes.Current;
                        if (next.opcode == OpCodes.Ldlen && codes.MoveNext()) {
                            var next1 = codes.Current;
                            if (next1.opcode == OpCodes.Conv_I4) {
                                yield return new CodeInstruction(OpCodes.Ldc_I4, DEFAULTGRID_RESOLUTION * DEFAULTGRID_RESOLUTION);
                            } else {
                                yield return cur;
                                yield return next;
                                yield return next1;
                            }
                        } else {
                            yield return cur;
                            yield return next;
                        }
                    } else if (cur.opcode == OpCodes.Call && cur.operand == getDMInstance && codes.MoveNext()) {
                        var next = codes.Current;
                        yield return cur;
                        yield return next;
                        if (next.opcode == OpCodes.Stloc_0) {
                            yield return new CodeInstruction(OpCodes.Ldloc_0);
                            yield return new CodeInstruction(OpCodes.Ldloc_0);
                            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(DistrictManager), "m_colorBuffer"));
                            yield return new CodeInstruction(OpCodes.Ldloc_0);
                            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(DistrictManager), "m_districtTexture1"));
                            yield return new CodeInstruction(OpCodes.Ldloc_0);
                            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(DistrictManager), "m_districtTexture2"));
                            yield return new CodeInstruction(OpCodes.Ldloc_0);
                            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(DistrictManager), "m_parkTexture1"));
                            yield return new CodeInstruction(OpCodes.Ldloc_0);
                            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(DistrictManager), "m_parkTexture2"));
                            yield return new CodeInstruction(OpCodes.Ldloc_0);
                            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(DistrictManager), "m_districtsModifiedX2"));
                            yield return new CodeInstruction(OpCodes.Ldloc_0);
                            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(DistrictManager), "m_districtsModifiedZ1"));
                            yield return new CodeInstruction(OpCodes.Ldloc_0);
                            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(DistrictManager), "m_parksModifiedX2"));
                            yield return new CodeInstruction(OpCodes.Ldloc_0);
                            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(DistrictManager), "m_parksModifiedZ2"));
                            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EDistrictManager), nameof(EDistrictManager.EnsureCapacity)));
                        }
                    } else if (cur.opcode == OpCodes.Call && cur.operand == get_lmInstance && sigCount++ > 0) {
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EDistrictManager), nameof(EDistrictManager.IntegratedDeserialize))).WithLabels(cur.labels);
                        yield return new CodeInstruction(cur.opcode, cur.operand);
                    } else {
                        yield return cur;
                    }
                }
            }
        }

        internal void Enable(Harmony harmony) {
#if ENABLEEIGHTYONE
            try {
                harmony.Patch(AccessTools.PropertySetter(typeof(DistrictManager), nameof(DistrictManager.HighlightPolicy)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EDistrictManagerPatch), nameof(EDistrictManagerPatch.SetHighlightPolicyTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch DistrictManager::set_HighlightPolicy");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.PropertySetter(typeof(DistrictManager), nameof(DistrictManager.HighlightPolicy)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(DistrictManager), "Awake"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EDistrictManagerPatch), nameof(EDistrictManagerPatch.AwakeTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch DistrictManager::Awake");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(DistrictManager), "Awake"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(DistrictManager), "BeginOverlayImpl"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EDistrictManagerPatch), nameof(EDistrictManagerPatch.BeginOverlayImplTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch DistrictManager::BeginOverlayImpl");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(DistrictManager), "BeginOverlayImpl"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(DistrictManager), nameof(DistrictManager.GetDistrict), new Type[] { typeof(int), typeof(int) }),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EDistrictManagerPatch), nameof(EDistrictManagerPatch.GetDistrictXZTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch DistrictManager::GetDistrict(int x, int z)");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(DistrictManager), nameof(DistrictManager.GetDistrict), new Type[] { typeof(int), typeof(int) }),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(DistrictManager), nameof(DistrictManager.GetDistrict), new Type[] { typeof(Vector3) }),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EDistrictManagerPatch), nameof(EDistrictManagerPatch.GetDistrictVector3Transpiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch DistrictManager::GetDistrict(Vector3)");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(DistrictManager), nameof(DistrictManager.GetDistrict), new Type[] { typeof(Vector3) }),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(DistrictManager), nameof(DistrictManager.GetDistrictArea)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EDistrictManagerPatch), nameof(EDistrictManagerPatch.GetDistrictAreaTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch DistrictManager::GetDistrictArea");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(DistrictManager), nameof(DistrictManager.GetDistrictArea)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(DistrictManager), nameof(DistrictManager.GetPark)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EDistrictManagerPatch), nameof(EDistrictManagerPatch.GetParkTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch DistrictManager::GetPark");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(DistrictManager), nameof(DistrictManager.GetPark)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(DistrictManager), nameof(DistrictManager.GetParkArea)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EDistrictManagerPatch), nameof(EDistrictManagerPatch.GetParkAreaTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch DistrictManager::GetParkArea");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(DistrictManager), nameof(DistrictManager.GetParkArea)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(DistrictManager), nameof(DistrictManager.ModifyCell)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EDistrictManagerPatch), nameof(EDistrictManagerPatch.ModifyCellTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch DistrictManager::ModifyCell");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(DistrictManager), nameof(DistrictManager.ModifyCell)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(DistrictManager), nameof(DistrictManager.ModifyParkCell)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EDistrictManagerPatch), nameof(EDistrictManagerPatch.ModifyParkCellTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch DistrictManager::ModifyParkCell");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(DistrictManager), nameof(DistrictManager.ModifyParkCell)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(DistrictManager), "MoveParkBuildings"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EDistrictManagerPatch), nameof(EDistrictManagerPatch.MoveParkBuildingsTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch DistrictManager::MoveParkBuildings");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(DistrictManager), "MoveParkBuildings"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(DistrictManager), "MoveParkSegments"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EDistrictManagerPatch), nameof(EDistrictManagerPatch.MoveParkSegmentsTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch DistrictManager::MoveParkSegments");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(DistrictManager), "MoveParkSegments"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(DistrictManager), nameof(DistrictManager.NamesModified)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EDistrictManagerPatch), nameof(EDistrictManagerPatch.NamesModifiedTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch DistrictManager::NamesModified");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(DistrictManager), nameof(DistrictManager.NamesModified)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(DistrictManager), nameof(DistrictManager.NamesModified), new Type[] { typeof(DistrictManager.Cell[]) }),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EDistrictManagerPatch), nameof(EDistrictManagerPatch.NamesModifiedCellTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch DistrictManager::NamesModified(DistrictManager.Cell[] cells)");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(DistrictManager), nameof(DistrictManager.NamesModified), new Type[] { typeof(DistrictManager.Cell[]) }),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
#endif
            try {
                harmony.Patch(AccessTools.Method(typeof(DistrictManager), "MoveParkProps"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EDistrictManagerPatch), nameof(EDistrictManagerPatch.MoveParkPropsTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch DistrictManager::MoveParkProps");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(DistrictManager), "MoveParkProps"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
#if ENABLEEIGHTYONE
            try {
                harmony.Patch(AccessTools.Method(typeof(DistrictManager), "MoveParkTrees"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EDistrictManagerPatch), nameof(EDistrictManagerPatch.MoveParkTreesTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch DistrictManager::MoveParkTrees");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(DistrictManager), "MoveParkTrees"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(DistrictManager), nameof(DistrictManager.ParkNamesModified)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EDistrictManagerPatch), nameof(EDistrictManagerPatch.ParkNamesModifiedTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch DistrictManager::ParkNamesModified");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(DistrictManager), nameof(DistrictManager.ParkNamesModified)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(DistrictManager), nameof(DistrictManager.SampleDistrict), new Type[] { typeof(Vector3), typeof(DistrictManager.Cell[]) }),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EDistrictManagerPatch), nameof(EDistrictManagerPatch.SampleDistrictTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch DistrictManager::SampleDistrict(Vector3 worldPos, DistrictManager.Cell[] districts)");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(DistrictManager), nameof(DistrictManager.SampleDistrict), new Type[] { typeof(Vector3), typeof(DistrictManager.Cell[]) }),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(DistrictManager), "UpdateNames"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EDistrictManagerPatch), nameof(EDistrictManagerPatch.UpdateNamesTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch DistrictManager::UpdateNames");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(DistrictManager), "UpdateNames"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(DistrictManager), "UpdateTexture"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EDistrictManagerPatch), nameof(EDistrictManagerPatch.UpdateTextureTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch DistrictManager::UpdateTexture");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(DistrictManager), "UpdateTexture"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(DistrictManager.Data), nameof(DistrictManager.Data.Serialize)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EDistrictManagerPatch), nameof(EDistrictManagerPatch.SerializeTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch DistrictManager::Data::Serialize");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(DistrictManager.Data), nameof(DistrictManager.Data.Serialize)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(DistrictManager.Data), nameof(DistrictManager.Data.Deserialize)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EDistrictManagerPatch), nameof(EDistrictManagerPatch.DeserializeTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch DistrictManager::Data::Deserialize");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(DistrictManager.Data), nameof(DistrictManager.Data.Deserialize)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
#endif
        }

        internal void Disable(Harmony harmony) {
#if ENABLEEIGHTYONE
            harmony.Unpatch(AccessTools.PropertySetter(typeof(DistrictManager), nameof(DistrictManager.HighlightPolicy)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(DistrictManager), "Awake"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(DistrictManager), "BeginOverlayImpl"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(DistrictManager), nameof(DistrictManager.GetDistrictArea)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(DistrictManager), nameof(DistrictManager.GetPark)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(DistrictManager), nameof(DistrictManager.GetParkArea)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(DistrictManager), nameof(DistrictManager.ModifyCell)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(DistrictManager), nameof(DistrictManager.ModifyParkCell)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(DistrictManager), "MoveParkBuilidngs"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(DistrictManager), "MoveParkSegments"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(DistrictManager), "MoveParkTrees"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
#endif
            harmony.Unpatch(AccessTools.Method(typeof(DistrictManager), "MoveParkProps"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
#if ENABLEEIGHTYONE
            harmony.Unpatch(AccessTools.Method(typeof(DistrictManager), nameof(DistrictManager.NamesModified)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(DistrictManager), nameof(DistrictManager.NamesModified),
                new Type[] { typeof(DistrictManager.Cell[]) }), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(DistrictManager), nameof(DistrictManager.ParkNamesModified)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(DistrictManager), nameof(DistrictManager.SampleDistrict),
                new Type[] { typeof(Vector3), typeof(DistrictManager.Cell[]) }), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(DistrictManager), "UpdateNames"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(DistrictManager), "UpdateTexture"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(DistrictManager.Data), nameof(DistrictManager.Data.Serialize)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(DistrictManager.Data), nameof(DistrictManager.Data.Deserialize)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
#endif
        }
    }
}
