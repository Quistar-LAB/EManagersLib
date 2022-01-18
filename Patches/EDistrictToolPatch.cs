using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using static EManagersLib.EDistrictManager;

namespace EManagersLib.Patches {
    internal readonly struct EDistrictToolPatch {
        private static IEnumerable<CodeInstruction> ApplyBrushTranspiler(IEnumerable<CodeInstruction> instructions) {
            bool sigFound = false;
            foreach (var code in instructions) {
                if (!sigFound && code.LoadsConstant(DEFAULTGRID_RESOLUTION)) {
                    sigFound = true;
                    code.operand = DISTRICTGRID_RESOLUTION;
                    yield return code;
                } else {
                    yield return code;
                }
            }
        }

        private static IEnumerable<CodeInstruction> ForceDistrictAlphaTranspiler(IEnumerable<CodeInstruction> instructions) {
            bool sigFound = false;
            foreach (var code in instructions) {
                if (!sigFound && code.LoadsConstant(DEFAULTGRID_RESOLUTION)) {
                    sigFound = true;
                    code.operand = DISTRICTGRID_RESOLUTION;
                    yield return code;
                } else {
                    yield return code;
                }
            }
        }

        private static IEnumerable<CodeInstruction> SetDistrictAlphaTranspiler(IEnumerable<CodeInstruction> instructions) {
            bool sigFound = false;
            foreach (var code in instructions) {
                if (!sigFound && code.LoadsConstant(DEFAULTGRID_RESOLUTION)) {
                    sigFound = true;
                    code.operand = DISTRICTGRID_RESOLUTION;
                    yield return code;
                } else {
                    yield return code;
                }
            }
        }

        private static IEnumerable<CodeInstruction> CheckNeighbourCellsTranspiler(IEnumerable<CodeInstruction> instructions) {
            bool sigFound = false;
            foreach (var code in instructions) {
                if (!sigFound && code.LoadsConstant(DEFAULTGRID_RESOLUTION)) {
                    sigFound = true;
                    code.operand = DISTRICTGRID_RESOLUTION;
                    yield return code;
                } else {
                    yield return code;
                }
            }
        }

        internal void Enable(Harmony harmony) {
            try {
                harmony.Patch(AccessTools.Method(typeof(DistrictTool), nameof(DistrictTool.ApplyBrush),
                    new Type[] { typeof(DistrictTool.Layer), typeof(byte), typeof(float), typeof(Vector3), typeof(Vector3) }),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EDistrictToolPatch), nameof(ApplyBrushTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch DistrictTool::ApplyBrush");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(DistrictTool), nameof(DistrictTool.ApplyBrush),
                    new Type[] { typeof(DistrictTool.Layer), typeof(byte), typeof(float), typeof(Vector3), typeof(Vector3) }),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(DistrictTool), @"ForceDistrictAlpha"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EDistrictToolPatch), nameof(ForceDistrictAlphaTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch DistrictTool::ForceDistrictAlpha");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(DistrictTool), @"ForceDistrictAlpha"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(DistrictTool), @"SetDistrictAlpha"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EDistrictToolPatch), nameof(SetDistrictAlphaTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch DistrictTool::SetDistrictAlpha");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(DistrictTool), @"SetDistrictAlpha"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(DistrictTool), @"CheckNeighbourCells"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EDistrictToolPatch), nameof(CheckNeighbourCellsTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch DistrictTool::CheckNeighbourCells");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(DistrictTool), @"CheckNeighbourCells"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
        }

        internal void Disable(Harmony harmony) {
            harmony.Unpatch(AccessTools.Method(typeof(DistrictTool), nameof(DistrictTool.ApplyBrush),
                new Type[] { typeof(DistrictTool.Layer), typeof(byte), typeof(float), typeof(Vector3), typeof(Vector3) }),
                HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(DistrictTool), @"ForceDistrictAlpha"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(DistrictTool), @"SetDistrictAlpha"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(DistrictTool), @"CheckNeighbourCells"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
        }
    }
}
