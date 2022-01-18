using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using static EManagersLib.EZoneManager;

namespace EManagersLib.Patches {
    internal readonly struct EZoneToolPatch {
        private static IEnumerable<CodeInstruction> ReplaceConstants(IEnumerable<CodeInstruction> instructions) {
            const float defHalfGrid = DEFGRID_RESOLUTION / 2f;
            const float halfGrid = ZONEGRID_RESOLUTION / 2f;
            foreach (var code in instructions) {
                if (code.LoadsConstant(defHalfGrid)) {
                    code.operand = halfGrid;
                } else if (code.LoadsConstant(DEFGRID_RESOLUTION - 1)) {
                    code.operand = ZONEGRID_RESOLUTION - 1;
                } else if (code.LoadsConstant(DEFGRID_RESOLUTION)) {
                    code.operand = ZONEGRID_RESOLUTION;
                }
                yield return code;
            }
        }

        private static IEnumerable<CodeInstruction> ApplyBrushTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceConstants(instructions);

        private static IEnumerable<CodeInstruction> ApplyFillTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceConstants(instructions);

        private static IEnumerable<CodeInstruction> ApplyZoningTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceConstants(instructions);

        private static IEnumerable<CodeInstruction> CalculateFillBufferTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceConstants(instructions);

        internal void Enable(Harmony harmony) {
            try {
                harmony.Patch(AccessTools.Method(typeof(ZoneTool), "ApplyBrush"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EZoneToolPatch), nameof(ApplyBrushTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch ZoneTool::ApplyBrush");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(ZoneTool), "ApplyBrush"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(ZoneTool), "ApplyFill"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EZoneToolPatch), nameof(ApplyFillTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch ZoneTool::ApplyFill");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(ZoneTool), "ApplyFill"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(ZoneTool), "ApplyZoning"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EZoneToolPatch), nameof(ApplyZoningTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch ZoneTool::ApplyZoning");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(ZoneTool), "ApplyZoning"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(ZoneTool), "CalculateFillBuffer",
                    new Type[] { typeof(Vector3), typeof(Vector3), typeof(ItemClass.Zone), typeof(bool), typeof(bool) }),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EZoneToolPatch), nameof(ApplyZoningTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch ZoneTool::CalculateFillBuffer(Vector3, Vector3, ItemClass.Zone, bool, bool)");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(ZoneTool), "CalculateFillBuffer",
                    new Type[] { typeof(Vector3), typeof(Vector3), typeof(ItemClass.Zone), typeof(bool), typeof(bool) }),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
        }

        internal void Disable(Harmony harmony) {
            harmony.Unpatch(AccessTools.Method(typeof(ZoneTool), "ApplyBrush"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(ZoneTool), "ApplyFill"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(ZoneTool), "ApplyZoning"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(ZoneTool), "CalculateFillBuffer",
                new Type[] { typeof(Vector3), typeof(Vector3), typeof(ItemClass.Zone), typeof(bool), typeof(bool) }), HarmonyPatchType.Transpiler, EModule.HARMONYID);
        }
    }
}
