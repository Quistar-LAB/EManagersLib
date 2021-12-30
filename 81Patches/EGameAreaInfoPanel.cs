using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace EManagersLib.Patches {
    internal class EGameAreaInfoPanel {
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> ShowInternalTranspiler(IEnumerable<CodeInstruction> instructions) => EGameAreaManagerPatch.ReplaceGetTileXZ(instructions);

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> UpdatePanelTranspiler(IEnumerable<CodeInstruction> instructions) => EGameAreaManagerPatch.ReplaceGetTileXZ(instructions);


        internal void Enable(Harmony harmony) {
            try {
                harmony.Patch(AccessTools.Method(typeof(GameAreaInfoPanel), "ShowInternal"),
                    transpiler: new HarmonyMethod(typeof(EGameAreaInfoPanel), nameof(ShowInternalTranspiler)));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch GameAreaInfoPanel::ShowInternal");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(GameAreaInfoPanel), "ShowInternal"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(GameAreaInfoPanel), "UpdatePanel"),
                    transpiler: new HarmonyMethod(typeof(EGameAreaInfoPanel), nameof(ShowInternalTranspiler)));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch GameAreaInfoPanel::UpdatePanel");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(GameAreaInfoPanel), "UpdatePanel"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
        }

        internal void Disable(Harmony harmony) {
            harmony.Unpatch(AccessTools.Method(typeof(GameAreaInfoPanel), "ShowInternal"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(GameAreaInfoPanel), "UpdatePanel"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
        }
    }
}
