using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace EManagersLib.Patches {
    internal readonly struct ENaturalResourceManagerPatch {
        private static IEnumerable<CodeInstruction> GetTileResourcesTranspiler(IEnumerable<CodeInstruction> instructions) {
            foreach (var code in instructions) {
                if (code.opcode == OpCodes.Ldc_I4_2) {
                    yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                } else {
                    yield return code;
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> GetTileResourcesImplTranspiler(IEnumerable<CodeInstruction> instructions) {
            return instructions;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> CalculateUnlockableResourcesTranspiller(IEnumerable<CodeInstruction> instructions) => EGameAreaManagerPatch.ReplaceDefaultConstants(instructions);

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> CalculateUnlockedResourcesTranspiler(IEnumerable<CodeInstruction> instructions) {
            foreach (var code in EGameAreaManagerPatch.ReplaceDefaultConstants(instructions)) {
                if (code.opcode == OpCodes.Ldc_I4_2) {
                    yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                } else {
                    yield return code;
                }
            }
        }


        internal void Enable(Harmony harmony) {
            try {
                harmony.Patch(AccessTools.Method(typeof(NaturalResourceManager), nameof(NaturalResourceManager.GetTileResources)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(ENaturalResourceManagerPatch), nameof(GetTileResourcesTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch NaturalResourceManager::GetTileResources");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(NaturalResourceManager), nameof(NaturalResourceManager.GetTileResources)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(NaturalResourceManager), "GetTileResourcesImpl",
                    new Type[] { typeof(NaturalResourceManager.AreaCell).MakeByRefType(), typeof(uint).MakeByRefType(),
                             typeof(uint).MakeByRefType(), typeof(uint).MakeByRefType(), typeof(uint).MakeByRefType(), typeof(uint).MakeByRefType() }),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(ENaturalResourceManagerPatch), nameof(GetTileResourcesImplTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch NaturalResourceManager::GetTileResourcesImpl");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(NaturalResourceManager), "GetTileResourcesImpl",
                    new Type[] { typeof(NaturalResourceManager.AreaCell).MakeByRefType(), typeof(uint).MakeByRefType(),
                             typeof(uint).MakeByRefType(), typeof(uint).MakeByRefType(), typeof(uint).MakeByRefType(), typeof(uint).MakeByRefType() }),
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
