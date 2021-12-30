using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace EManagersLib.Patches {
    internal class ENaturalResourceManagerPatch {
        // Need to work this out, and its other associated methods
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> GetTileResourcesImplTranspiler(IEnumerable<CodeInstruction> instructions) {
            return instructions;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> CalculateUnlockableResourcesTranspiller(IEnumerable<CodeInstruction> instructions) => EGameAreaManagerPatch.ReplaceDefaultConstants(instructions);

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> CalculateUnlockedResourcesTranspiler(IEnumerable<CodeInstruction> instructions) => EGameAreaManagerPatch.ReplaceDefaultConstants(instructions);


        internal void Enable(Harmony harmony) {
            try {
                harmony.Patch(AccessTools.Method(typeof(NaturalResourceManager), "GetTileResourcesImpl",
                    new Type[] { typeof(NaturalResourceManager.AreaCell).MakeByRefType(), typeof(uint).MakeByRefType(),
                             typeof(uint).MakeByRefType(), typeof(uint).MakeByRefType(), typeof(uint).MakeByRefType(), typeof(uint).MakeByRefType() }),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(ENaturalResourceManagerPatch), nameof(GetTileResourcesImplTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch NaturalResourceManager::GetTileResourcesImpl");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(NaturalResourceManager), "GetTileResourcesImpl"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(NaturalResourceManager), nameof(NaturalResourceManager.CalculateUnlockableResources)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(ENaturalResourceManagerPatch), nameof(CalculateUnlockableResourcesTranspiller))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch NaturalResourceManager::CalculateUnlockableResources");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(NaturalResourceManager), nameof(NaturalResourceManager.CalculateUnlockableResources)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(NaturalResourceManager), nameof(NaturalResourceManager.CalculateUnlockedResources)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(ENaturalResourceManagerPatch), nameof(CalculateUnlockedResourcesTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch NaturalResourceManager::CalculateUnlockedResources");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(NaturalResourceManager), nameof(NaturalResourceManager.CalculateUnlockedResources)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
        }

        internal void Disable(Harmony harmony) {
            harmony.Unpatch(AccessTools.Method(typeof(NaturalResourceManager), "GetTileResourcesImpl",
                new Type[] { typeof(NaturalResourceManager.AreaCell).MakeByRefType(), typeof(uint).MakeByRefType(),
                             typeof(uint).MakeByRefType(), typeof(uint).MakeByRefType(), typeof(uint).MakeByRefType(), typeof(uint).MakeByRefType() }),
                HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(NaturalResourceManager), nameof(NaturalResourceManager.CalculateUnlockableResources)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(NaturalResourceManager), nameof(NaturalResourceManager.CalculateUnlockedResources)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
        }
    }
}
