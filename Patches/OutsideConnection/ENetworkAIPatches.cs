using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace EManagersLib.Patches.OutsideConnection {
    internal readonly struct ENetworkAIPatches {
        private static IEnumerable<CodeInstruction> ReplaceOutsideLimit(IEnumerable<CodeInstruction> instructions) {
            foreach (var code in instructions) {
                if (code.opcode == OpCodes.Ldc_I4_4) {
                    yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(ESettings), nameof(ESettings.m_maxOutsideConnection)));
                } else {
                    yield return code;
                }
            }
        }

        internal void Enable(Harmony harmony) {
            HarmonyMethod replaceOutsideConnection = new HarmonyMethod(AccessTools.Method(typeof(ENetworkAIPatches), nameof(ReplaceOutsideLimit)));
            try {
                harmony.Patch(AccessTools.Method(typeof(TrainTrackAI), nameof(TrainTrackAI.GetInfo)), transpiler: replaceOutsideConnection);
            } catch (Exception e) {
                EUtils.ELog("Failed to patch TrainTrackAI::GetInfo");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(TrainTrackAI), nameof(TrainTrackAI.GetInfo)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(ShipPathAI), nameof(ShipPathAI.GetInfo)), transpiler: replaceOutsideConnection);
            } catch (Exception e) {
                EUtils.ELog("Failed to patch ShipPathAI::GetInfo");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(ShipPathAI), nameof(ShipPathAI.GetInfo)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(RoadAI), nameof(RoadAI.GetInfo)), transpiler: replaceOutsideConnection);
            } catch (Exception e) {
                EUtils.ELog("Failed to patch ShipPathAI::GetInfo");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(RoadAI), nameof(RoadAI.GetInfo)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(FlightPathAI), nameof(FlightPathAI.GetInfo)), transpiler: replaceOutsideConnection);
            } catch (Exception e) {
                EUtils.ELog("Failed to patch ShipPathAI::GetInfo");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(FlightPathAI), nameof(FlightPathAI.GetInfo)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
        }

        internal void Disable(Harmony harmony) {
            harmony.Unpatch(AccessTools.Method(typeof(TrainTrackAI), nameof(TrainTrackAI.GetInfo)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(ShipPathAI), nameof(ShipPathAI.GetInfo)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(RoadAI), nameof(RoadAI.GetInfo)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(FlightPathAI), nameof(FlightPathAI.GetInfo)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
        }
    }
}
