using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace EManagersLib.Patches {
    internal readonly struct ETerrainManagerPatch {
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> GetUnlockableTerrainFlatnessTranspiler(IEnumerable<CodeInstruction> instructions) {
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(TerrainManager), nameof(TerrainManager.m_patches)));
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ETerrainManager), nameof(ETerrainManager.GetUnlockableTerrainFlatness)));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        private static IEnumerable<CodeInstruction> GetTileFlatnessTranspiler(IEnumerable<CodeInstruction> instructions) {
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(TerrainManager), nameof(TerrainManager.m_patches)));
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Ldarg_2);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ETerrainManager), nameof(ETerrainManager.GetTileFlatness)));
            yield return new CodeInstruction(OpCodes.Ret);
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
        }

        internal void Disable(Harmony harmony) {
            harmony.Unpatch(AccessTools.Method(typeof(TerrainManager), nameof(TerrainManager.GetUnlockableTerrainFlatness)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(TerrainManager), nameof(TerrainManager.GetTileFlatness)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
        }
    }
}
