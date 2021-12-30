using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace EManagersLib.Patches {
    internal class ENetManagerPatch {
        // TODO: Should I patch NetManager::AddTileNode too?
        // TODO: Should I implement NetManager::UpdateNodeFlag() => Implement DontUpdateNodeFlag?

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> GetTileNodeCountTranpiler(IEnumerable<CodeInstruction> instructions) => EGameAreaManagerPatch.ReplaceGetTileXZ(instructions);

        internal void Enable(Harmony harmony) {
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
            harmony.Unpatch(AccessTools.Method(typeof(NetManager), nameof(NetManager.GetTileNodeCount)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
        }
    }
}
