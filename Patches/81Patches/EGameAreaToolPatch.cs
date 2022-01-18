using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace EManagersLib.Patches {
    internal readonly struct EGameAreaToolPatch {
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> OnToolGUITranspiler(IEnumerable<CodeInstruction> instructions) => EGameAreaManagerPatch.ReplaceGetTileXZ(instructions);

        internal void Enable(Harmony harmony) {
            try {
                harmony.Patch(AccessTools.Method(typeof(GameAreaTool), "OnToolGUI"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EGameAreaToolPatch), nameof(OnToolGUITranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch GameAreaTool::OnToolGUI");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(GameAreaManager), "OnToolGUI"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
        }

        internal void Disable(Harmony harmony) {
            harmony.Unpatch(AccessTools.Method(typeof(GameAreaTool), "OnToolGUI"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
        }
    }
}
