using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace EManagersLib.Patches {
    internal readonly struct ETerrainManagerPatch {
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> GetUnlockableTerrainFlatnessTranspiler(IEnumerable<CodeInstruction> instructions) {
            foreach (var code in EGameAreaManagerPatch.ReplaceGetTileXZ(instructions)) {
                if (code.LoadsConstant(25f)) {
                    code.operand = (float)EGameAreaManager.CUSTOMAREACOUNT;
                    yield return code;
                } else if (code.opcode == OpCodes.Ldc_I4_2) {
                    code.opcode = OpCodes.Ldc_I4_0;
                    yield return code;
                } else if (code.opcode == OpCodes.Ldc_I4_5) {
                    yield return new CodeInstruction(OpCodes.Ldc_I4, EGameAreaManager.CUSTOMGRIDSIZE);
                } else {
                    yield return code;
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> GetTileFlatnessTranspiler(IEnumerable<CodeInstruction> instructions) {
            foreach (var code in EGameAreaManagerPatch.ReplaceGetTileXZ(instructions)) {
                if (code.opcode == OpCodes.Ldc_I4_2) {
                    code.opcode = OpCodes.Ldc_I4_0;
                    yield return code;
                } else {
                    yield return code;
                }
            }
        }

        // This routine causes array out of index issue when 81 Tiles is enabled.
        // This runs inside many AI routines, thus need to find a solution
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> GetSurfaceCellTranspiler(IEnumerable<CodeInstruction> instructions) {
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Ldarg_2);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ETerrainManager), nameof(ETerrainManager.GetSurfaceCell)));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        private static IEnumerable<CodeInstruction> SampleDetailSurfaceTranspiler(IEnumerable<CodeInstruction> instructions) {
            MethodInfo getSurfaceCell = AccessTools.Method(typeof(TerrainManager), nameof(TerrainManager.GetSurfaceCell));
            foreach (var code in instructions) {
                if (code.opcode == OpCodes.Call && code.operand == getSurfaceCell) {
                    code.operand = AccessTools.Method(typeof(ETerrainManager), nameof(ETerrainManager.GetSurfaceCell));
                    yield return code;
                } else {
                    yield return code;
                }
            }
        }

        internal void Enable(Harmony harmony) {
            try {
                harmony.Patch(AccessTools.Method(typeof(TerrainManager), nameof(TerrainManager.GetUnlockableTerrainFlatness)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(ETerrainManagerPatch), nameof(GetUnlockableTerrainFlatnessTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch TerrainManager::GetUnlockableTerrainFlatness");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(TerrainManager), nameof(TerrainManager.GetUnlockableTerrainFlatness)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(TerrainManager), nameof(TerrainManager.GetTileFlatness)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(ETerrainManagerPatch), nameof(GetTileFlatnessTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch TerrainManager::GetTileFlatness");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(TerrainManager), nameof(TerrainManager.GetTileFlatness)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(TerrainManager), nameof(TerrainManager.GetSurfaceCell)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(ETerrainManagerPatch), nameof(GetSurfaceCellTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch TerrainManager::GetSurfaceCell");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(TerrainManager), nameof(TerrainManager.GetSurfaceCell)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(TerrainManager), nameof(TerrainManager.SampleDetailSurface), new Type[] { typeof(float), typeof(float) }),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(ETerrainManagerPatch), nameof(SampleDetailSurfaceTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch TerrainManager::SampleDetailSurface");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(TerrainManager), nameof(TerrainManager.SampleDetailSurface), new Type[] { typeof(float), typeof(float) }),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
        }

        internal void Disable(Harmony harmony) {
            harmony.Unpatch(AccessTools.Method(typeof(TerrainManager), nameof(TerrainManager.GetUnlockableTerrainFlatness)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(TerrainManager), nameof(TerrainManager.GetTileFlatness)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(TerrainManager), nameof(TerrainManager.GetSurfaceCell)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
        }
    }
}
