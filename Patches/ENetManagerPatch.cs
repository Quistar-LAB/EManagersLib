using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace EManagersLib.Patches {
    internal readonly struct ENetManagerPatch {
        private static IEnumerable<CodeInstruction> AwakeTranspiler(IEnumerable<CodeInstruction> instructions) {
            const int defTileNodeCount = 925;
            const int customTileNodeCount = 37 * EGameAreaManager.CUSTOMGRIDSIZE * EGameAreaManager.CUSTOMGRIDSIZE;
            foreach (var code in instructions) {
                if (code.LoadsConstant(defTileNodeCount)) {
                    code.operand = customTileNodeCount;
                    yield return code;
                } else {
                    yield return code;
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> GetTileNodeCountTranpiler(IEnumerable<CodeInstruction> instructions) => EGameAreaManagerPatch.ReplaceGetTileXZ(instructions);

        internal void Enable(Harmony harmony) {
            try {
                harmony.Patch(AccessTools.Method(typeof(NetManager), "Awake"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(ENetManagerPatch), nameof(AwakeTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch NetManager::Awake");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(NetManager), "Awake"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(NetManager), nameof(NetManager.GetTileNodeCount)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(ENetManagerPatch), nameof(GetTileNodeCountTranpiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch NetManager::GetTileNodeCount");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(NetManager), nameof(NetManager.GetTileNodeCount)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
        }

        internal void Disable(Harmony harmony) {
            harmony.Unpatch(AccessTools.Method(typeof(NetManager), "Awake"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(NetManager), nameof(NetManager.GetTileNodeCount)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
        }
    }
}
