using ColossalFramework;
using ColossalFramework.Math;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace EManagersLib.Patches {
    internal readonly struct EBuildingPatch {
        private static void __CheckZoning(ref Building building, ItemClass.Zone zone1, ItemClass.Zone zone2, ref uint validCells, ref bool secondary, ref ZoneBlock block) {
            BuildingInfo.ZoningMode zoningMode = building.Info.m_zoningMode;
            int width = building.Width;
            int length = building.Length;
            Vector3 a = new Vector3(Mathf.Cos(building.m_angle), 0f, Mathf.Sin(building.m_angle)) * 8f;
            Vector3 a2 = new Vector3(a.z, 0f, -a.x);
            int rowCount = block.RowCount;
            Vector3 a3 = new Vector3(Mathf.Cos(block.m_angle), 0f, Mathf.Sin(block.m_angle)) * 8f;
            Vector3 a4 = new Vector3(a3.z, 0f, -a3.x);
            Vector3 a5 = block.m_position - building.m_position + a * (width * 0.5f - 0.5f) + a2 * (length * 0.5f - 0.5f);
            for (int i = 0; i < rowCount; i++) {
                Vector3 b = (i - 3.5f) * a4;
                int num = 0;
                while (num < 4L) {
                    if ((block.m_valid & ~block.m_shared & 1uL << (i << 3 | num)) != 0uL) {
                        ItemClass.Zone zone3 = block.GetZone(num, i);
                        bool flag = zone3 == zone1;
                        if (zone3 == zone2 && zone2 != ItemClass.Zone.None) {
                            flag = true;
                            secondary = true;
                        }
                        if (flag) {
                            Vector3 b2 = (num - 3.5f) * a3;
                            Vector3 vector = a5 + b2 + b;
                            float num2 = a.x * vector.x + a.z * vector.z;
                            float num3 = a2.x * vector.x + a2.z * vector.z;
                            int num4 = Mathf.RoundToInt(num2 / 64f);
                            int num5 = Mathf.RoundToInt(num3 / 64f);
                            bool flag2 = false;
                            if (zoningMode == BuildingInfo.ZoningMode.Straight) {
                                flag2 = (num5 == 0);
                            } else if (zoningMode == BuildingInfo.ZoningMode.CornerLeft) {
                                flag2 = ((num5 == 0 && num4 >= width - 2) || (num5 <= 1 && num4 == width - 1));
                            } else if (zoningMode == BuildingInfo.ZoningMode.CornerRight) {
                                flag2 = ((num5 == 0 && num4 <= 1) || (num5 <= 1 && num4 == 0));
                            }
                            if ((!flag2 || num == 0) && num4 >= 0 && num5 >= 0 && num4 < width && num5 < length) {
                                validCells |= 1u << (num5 << 3) + num4;
                            }
                        }
                    }
                    num++;
                }
            }
        }

        // PlopGrowables
        // DisableZoneChecking
        private static bool CheckZoning(ref Building building, ItemClass.Zone zone1, ItemClass.Zone zone2, bool allowCollapsed) {
            const int GRID = EZoneManager.ZONEGRID_RESOLUTION;
            const float HALFGRID = GRID / 2f;
            int width = building.Width;
            int length = building.Length;
            Vector3 position = building.m_position;
            Vector3 vector = new Vector3((float)Math.Cos(building.m_angle), 0f, (float)Math.Sin(building.m_angle));
            Vector3 vector2 = new Vector3(vector.z, 0f, -vector.x);
            vector *= width * 4f;
            vector2 *= length * 4f;
            Quad3 quad = new Quad3 {
                a = position - vector - vector2,
                b = position + vector - vector2,
                c = position + vector + vector2,
                d = position - vector + vector2
            };
            Vector3 vector3 = quad.Min();
            Vector3 vector4 = quad.Max();
            int num = EMath.Max((int)((vector3.x - 46f) / 64f + HALFGRID), 0);
            int num2 = EMath.Max((int)((vector3.z - 46f) / 64f + HALFGRID), 0);
            int num3 = EMath.Min((int)((vector4.x + 46f) / 64f + HALFGRID), GRID - 1);
            int num4 = EMath.Min((int)((vector4.z + 46f) / 64f + HALFGRID), GRID - 1);
            bool flag = false;
            uint num5 = 0u;
            ZoneManager instance = Singleton<ZoneManager>.instance;
            for (int i = num2; i <= num4; i++) {
                for (int j = num; j <= num3; j++) {
                    ushort num6 = instance.m_zoneGrid[i * GRID + j];
                    int num7 = 0;
                    while (num6 != 0) {
                        if (allowCollapsed || (instance.m_blocks.m_buffer[num6].m_flags & 4u) == 0u) {
                            Vector3 pos = instance.m_blocks.m_buffer[num6].m_position;
                            float num8 = EMath.Max(EMath.Max(vector3.x - 46f - pos.x, vector3.z - 46f - pos.z), EMath.Max(pos.x - vector4.x - 46f, pos.z - vector4.z - 46f));
                            if (num8 < 0f) {
                                __CheckZoning(ref building, zone1, zone2, ref num5, ref flag, ref instance.m_blocks.m_buffer[num6]);
                            }
                        }
                        num6 = instance.m_blocks.m_buffer[num6].m_nextGridBlock;
                        if (++num7 >= 49152) {
                            CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
                            break;
                        }
                    }
                }
            }
            for (int k = 0; k < length; k++) {
                for (int l = 0; l < width; l++) {
                    if ((num5 & 1u << (k << 3) + l) == 0u) {
                        return false;
                    }
                }
            }
            bool flag2;
            if (flag) {
                flag2 = (zone2 == ItemClass.Zone.CommercialHigh || zone2 == ItemClass.Zone.ResidentialHigh);
            } else {
                flag2 = (zone1 == ItemClass.Zone.CommercialHigh || zone1 == ItemClass.Zone.ResidentialHigh);
            }
            if (flag2) {
                building.m_flags |= Building.Flags.HighDensity;
            } else {
                building.m_flags &= ~Building.Flags.HighDensity;
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> CheckZoningTranspiler(IEnumerable<CodeInstruction> instructions) {
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Ldarg_2);
            yield return new CodeInstruction(OpCodes.Ldarg_3);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EBuildingPatch), nameof(CheckZoning)));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        internal void Enable(Harmony harmony) {
            try {
                harmony.Patch(AccessTools.Method(typeof(Building), nameof(Building.CheckZoning), new Type[] { typeof(ItemClass.Zone), typeof(ItemClass.Zone), typeof(bool) }),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EBuildingPatch), nameof(CheckZoningTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch Building::CheckZoning");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(Building), nameof(Building.CheckZoning), new Type[] { typeof(ItemClass.Zone), typeof(ItemClass.Zone), typeof(bool) }),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }

        }

        internal void Disable(Harmony harmony) {
            harmony.Unpatch(AccessTools.Method(typeof(Building), nameof(Building.CheckZoning),
                new Type[] { typeof(ItemClass.Zone), typeof(ItemClass.Zone), typeof(bool) }), HarmonyPatchType.Transpiler, EModule.HARMONYID);

        }
    }
}
