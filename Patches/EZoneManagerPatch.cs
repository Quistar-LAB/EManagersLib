using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using static EManagersLib.EZoneManager;

namespace EManagersLib.Patches {
    internal readonly struct EZoneManagerPatch {
        private static IEnumerable<CodeInstruction> AwakeTranspiler(IEnumerable<CodeInstruction> instructions) {
            foreach (var code in instructions) {
                if (code.LoadsConstant(DEFGRID_RESOLUTION * DEFGRID_RESOLUTION)) {
                    code.operand = ZONEGRID_RESOLUTION * ZONEGRID_RESOLUTION;
                }
                yield return code;
            }
        }

        private static IEnumerable<CodeInstruction> CheckSpaceTranspiler(IEnumerable<CodeInstruction> instructions) {
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ZoneManager), nameof(ZoneManager.m_zoneGrid)));
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Ldarg_2);
            yield return new CodeInstruction(OpCodes.Ldarg_3);
            yield return new CodeInstruction(OpCodes.Ldarg_S, 4);
            yield return new CodeInstruction(OpCodes.Ldarg_S, 5);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EZoneManager), nameof(EZoneManager.CheckSpace)));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        private static IEnumerable<CodeInstruction> InitializeBlockTranspiler(IEnumerable<CodeInstruction> instructions) {
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

        private static IEnumerable<CodeInstruction> ReleaseBlockImplementationTranspiler(IEnumerable<CodeInstruction> instructions) {
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Ldarg_2);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EZoneManager), nameof(EZoneManager.ReleaseBlockImplementation)));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        private static IEnumerable<CodeInstruction> TerrainUpdatedTranspiler(IEnumerable<CodeInstruction> instructions) {
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ZoneManager), nameof(ZoneManager.m_zoneGrid)));
            yield return new CodeInstruction(OpCodes.Ldarg_3);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EZoneManager), nameof(EZoneManager.TerrainUpdated)));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        private static IEnumerable<CodeInstruction> UpdateBlocksTranspiler(IEnumerable<CodeInstruction> instructions) {
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(ZoneManager), nameof(ZoneManager.m_zoneGrid)));
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EZoneManager), nameof(EZoneManager.UpdateBlocks)));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        private static IEnumerable<CodeInstruction> DeserializeTranspiler(IEnumerable<CodeInstruction> instructions) {
            FieldInfo zoneGrid = AccessTools.Field(typeof(ZoneManager), nameof(ZoneManager.m_zoneGrid));
            using (var codes = instructions.GetEnumerator()) {
                while (codes.MoveNext()) {
                    var cur = codes.Current;
                    if (cur.opcode == OpCodes.Ldloc_0 && codes.MoveNext()) {
                        var next = codes.Current;
                        if (next.opcode == OpCodes.Ldfld && next.operand == zoneGrid) {
                            yield return cur;
                            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EZoneManager), nameof(EZoneManager.EnsureCapacity)));
                        } else {
                            yield return cur;
                            yield return next;
                        }
                    } else {
                        yield return cur;
                    }
                }
            }
        }

        internal void Enable(Harmony harmony) {
            try {
                harmony.Patch(AccessTools.Method(typeof(ZoneManager), "Awake"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EZoneManagerPatch), nameof(AwakeTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch ZoneManager::Awake");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(ZoneManager), "Awake"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(ZoneManager), nameof(ZoneManager.CheckSpace),
                    new Type[] { typeof(Vector3), typeof(float), typeof(int), typeof(int), typeof(int).MakeByRefType() }),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EZoneManagerPatch), nameof(CheckSpaceTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch ZoneManager::CheckSpace");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(ZoneManager), nameof(ZoneManager.CheckSpace),
                    new Type[] { typeof(Vector3), typeof(float), typeof(int), typeof(int), typeof(int).MakeByRefType() }),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(ZoneManager), "InitializeBlock"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EZoneManagerPatch), nameof(InitializeBlockTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch ZoneManager::InitializeBlock");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(ZoneManager), "InitializeBlock"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(ZoneManager), "ReleaseBlockImplementation"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EZoneManagerPatch), nameof(ReleaseBlockImplementationTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch ZoneManager::ReleaseBlockImplementation");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(ZoneManager), "ReleaseBlockImplementation"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(ZoneManager), nameof(ZoneManager.TerrainUpdated)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EZoneManagerPatch), nameof(TerrainUpdatedTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch ZoneManager::TerrainUpdated");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(ZoneManager), nameof(ZoneManager.TerrainUpdated)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(ZoneManager), nameof(ZoneManager.UpdateBlocks)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EZoneManagerPatch), nameof(UpdateBlocksTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch ZoneManager::UpdateBlocks");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(ZoneManager), nameof(ZoneManager.UpdateBlocks)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(ZoneManager.Data), nameof(ZoneManager.Data.Deserialize)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EZoneManagerPatch), nameof(DeserializeTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch ZoneManager::Data::Deserialize");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(ZoneManager), nameof(ZoneManager.Data.Deserialize)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
        }

        internal void Disable(Harmony harmony) {
            harmony.Unpatch(AccessTools.Method(typeof(ZoneManager), "Awake"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(ZoneManager), nameof(ZoneManager.CheckSpace)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(ZoneManager), "InitializeBlock"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(ZoneManager), "ReleaseBlockImplementation"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(ZoneManager), nameof(ZoneManager.TerrainUpdated)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(ZoneManager), nameof(ZoneManager.UpdateBlocks)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(ZoneManager.Data), nameof(ZoneManager.Data.Deserialize)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
        }
    }
}
