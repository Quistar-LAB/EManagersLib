using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace EManagersLib.Patches {
    internal readonly struct EZoneBlockPatch {
        private static IEnumerable<CodeInstruction> CalculateImplementation2Transpiler(IEnumerable<CodeInstruction> instructions) {
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Ldarg_2);
            yield return new CodeInstruction(OpCodes.Ldarg_3);
            yield return new CodeInstruction(OpCodes.Ldarg_S, 4);
            yield return new CodeInstruction(OpCodes.Ldarg_S, 5);
            yield return new CodeInstruction(OpCodes.Ldarg_S, 6);
            yield return new CodeInstruction(OpCodes.Ldarg_S, 7);
            yield return new CodeInstruction(OpCodes.Ldarg_S, 8);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EZoneBlock), nameof(EZoneBlock.CalculateImplementation2)));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        private static IEnumerable<CodeInstruction> CalculateBlock2Transpiler(IEnumerable<CodeInstruction> instructions) {
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EZoneBlock), nameof(EZoneBlock.CalculateBlock2)));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        private static IEnumerable<CodeInstruction> SimulationStepTranspiler(IEnumerable<CodeInstruction> instructions) {
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EZoneBlock), nameof(EZoneBlock.SimulationStep)));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        internal void Enable(Harmony harmony) {
            try {
                harmony.Patch(AccessTools.Method(typeof(ZoneBlock), nameof(ZoneBlock.CalculateBlock2)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EZoneBlockPatch), nameof(CalculateBlock2Transpiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch ZoneBlock::CalculateBlock2");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(ZoneBlock), nameof(ZoneBlock.CalculateBlock2)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(ZoneBlock), "CalculateImplementation2"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EZoneBlockPatch), nameof(CalculateImplementation2Transpiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch ZoneBlock::CalculateImplementation2");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(ZoneBlock), "CalculateImplementation2"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(ZoneBlock), "SimulationStep"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EZoneBlockPatch), nameof(SimulationStepTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch ZoneBlock::SimulationStep");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(ZoneBlock), "SimulationStep"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
        }

        internal void Disable(Harmony harmony) {
            harmony.Unpatch(AccessTools.Method(typeof(ZoneBlock), nameof(ZoneBlock.CalculateBlock2)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(ZoneBlock), "CalculateImplementation2"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(ZoneBlock), "SimulationStep"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
        }
    }
}
