using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace EManagersLib.Patches {
    internal class ETerrainManagerPatch {
        #region 81 Tiles Specific
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> GetUnlockableTerrainFlatnessTranspiler(IEnumerable<CodeInstruction> instructions) {
            foreach (var code in EGameAreaManagerPatch.ReplaceGetTileXZ(instructions)) {
                if (code.LoadsConstant(25f)) {
                    code.operand = (float)EGameAreaManager.CUSTOMAREACOUNT;
                    yield return code;
                } else if (code.opcode == OpCodes.Ldc_I4_2) {
                    code.opcode = OpCodes.Ldc_I4_0;
                    yield return code;
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

        // Test without modification
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> GetSurfaceCellTranspiler(IEnumerable<CodeInstruction> instructions) {
            return instructions;
        }
        #endregion

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
        }

        internal void Disable(Harmony harmony) {
            harmony.Unpatch(AccessTools.Method(typeof(TerrainManager), nameof(TerrainManager.GetUnlockableTerrainFlatness)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(TerrainManager), nameof(TerrainManager.GetTileFlatness)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(TerrainManager), nameof(TerrainManager.GetSurfaceCell)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
        }
    }
}
