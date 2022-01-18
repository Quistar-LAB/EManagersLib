using ColossalFramework;
using ColossalFramework.Math;
using System;
using UnityEngine;
using static EManagersLib.EZoneManager;

namespace EManagersLib {
    internal static class EZoneBlock {
        internal static void CalculateImplementation2(ref ZoneBlock block, ushort blockID, ref ZoneBlock other, ref ulong valid, ref ulong shared, float minX, float minZ, float maxX, float maxZ) {
            if ((other.m_flags & 1u) == 0u) {
                return;
            }
            if (Mathf.Abs(other.m_position.x - block.m_position.x) >= 92f || Mathf.Abs(other.m_position.z - block.m_position.z) >= 92f) {
                return;
            }
            bool flag = (other.m_flags & 2u) != 0u;
            int rowCount = block.RowCount;
            int rowCount2 = other.RowCount;
            Vector2 a = new Vector2((float)Math.Cos(block.m_angle), (float)Math.Sin(block.m_angle)) * 8f;
            Vector2 a2 = new Vector2(a.y, -a.x);
            Vector2 vector = new Vector2((float)Math.Cos(other.m_angle), (float)Math.Sin(other.m_angle)) * 8f;
            Vector2 vector2 = new Vector2(vector.y, -vector.x);
            Vector2 a3 = VectorUtils.XZ(other.m_position);
            Quad2 quad = default;
            quad.a = a3 - 4f * vector - 4f * vector2;
            quad.b = a3 + 0f * vector - 4f * vector2;
            quad.c = a3 + 0f * vector + (rowCount2 - 4) * vector2;
            quad.d = a3 - 4f * vector + (rowCount2 - 4) * vector2;
            Vector2 vector3 = quad.Min();
            Vector2 vector4 = quad.Max();
            if (vector3.x <= maxX && vector3.y <= maxZ && minX <= vector4.x && minZ <= vector4.y) {
                Vector2 a4 = VectorUtils.XZ(block.m_position);
                Quad2 quad2 = default;
                quad2.a = a4 - 4f * a - 4f * a2;
                quad2.b = a4 + 0f * a - 4f * a2;
                quad2.c = a4 + 0f * a + (rowCount - 4) * a2;
                quad2.d = a4 - 4f * a + (rowCount - 4) * a2;
                if (!quad2.Intersect(quad)) {
                    return;
                }
                for (int i = 0; i < rowCount; i++) {
                    Vector2 b = (i - 3.99f) * a2;
                    Vector2 b2 = (i - 3.01f) * a2;
                    quad2.a = a4 - 4f * a + b;
                    quad2.b = a4 + 0f * a + b;
                    quad2.c = a4 + 0f * a + b2;
                    quad2.d = a4 - 4f * a + b2;
                    if (quad2.Intersect(quad)) {
                        int num = 0;
                        while (num < 4L) {
                            if ((valid & 1uL << (i << 3 | num)) == 0uL) {
                                break;
                            }
                            Vector2 b3 = (num - 3.99f) * a;
                            Vector2 vector5 = (num - 3.01f) * a;
                            Vector2 vector6 = a4 + (vector5 + b3 + b2 + b) * 0.5f;
                            if (Quad2.Intersect(quad.a - vector - vector2, quad.b + vector - vector2, quad.c + vector + vector2, quad.d - vector + vector2, vector6)) {
                                Quad2 quad3 = default;
                                quad3.a = a4 + b3 + b;
                                quad3.b = a4 + vector5 + b;
                                quad3.c = a4 + vector5 + b2;
                                quad3.d = a4 + b3 + b2;
                                bool flag2 = true;
                                bool flag3 = false;
                                int num2 = 0;
                                while (num2 < rowCount2 && flag2) {
                                    Vector2 b4 = (num2 - 3.99f) * vector2;
                                    Vector2 b5 = (num2 - 3.01f) * vector2;
                                    int num3 = 0;
                                    while (num3 < 4L && flag2) {
                                        if ((other.m_valid & ~other.m_shared & 1uL << (num2 << 3 | num3)) != 0uL) {
                                            Vector2 b6 = (num3 - 3.99f) * vector;
                                            Vector2 vector7 = (num3 - 3.01f) * vector;
                                            Vector2 a5 = a3 + (vector7 + b6 + b5 + b4) * 0.5f;
                                            float num4 = Vector2.SqrMagnitude(a5 - vector6);
                                            if (num4 < 144f) {
                                                if (!flag) {
                                                    float num5 = EMath.Abs(other.m_angle - block.m_angle) * 0.636619747f;
                                                    num5 -= Mathf.Floor(num5);
                                                    if (num4 < 0.01f && (num5 < 0.01f || num5 > 0.99f)) {
                                                        if (num < num3 || (num == num3 && block.m_buildIndex < other.m_buildIndex)) {
                                                            other.m_shared |= 1uL << (num2 << 3 | num3);
                                                        } else {
                                                            flag3 = true;
                                                        }
                                                    } else if (quad3.Intersect(new Quad2 {
                                                        a = a3 + b6 + b4,
                                                        b = a3 + vector7 + b4,
                                                        c = a3 + vector7 + b5,
                                                        d = a3 + b6 + b5
                                                    })) {
                                                        if ((num3 >= 4 && num >= 4) || (num3 < 4 && num < 4)) {
                                                            if ((num3 >= 2 && num >= 2) || (num3 < 2 && num < 2)) {
                                                                if (block.m_buildIndex < other.m_buildIndex) {
                                                                    other.m_valid &= ~(1uL << (num2 << 3 | num3));
                                                                } else {
                                                                    flag2 = false;
                                                                }
                                                            } else if (num3 < 2) {
                                                                flag2 = false;
                                                            } else {
                                                                other.m_valid &= ~(1uL << (num2 << 3 | num3));
                                                            }
                                                        } else if (num3 < 4) {
                                                            flag2 = false;
                                                        } else {
                                                            other.m_valid &= ~(1uL << (num2 << 3 | num3));
                                                        }
                                                    }
                                                }
                                                if (num4 < 36f && num < 4 && num3 < 4) {
                                                    ItemClass.Zone zone = block.GetZone(num, i);
                                                    ItemClass.Zone zone2 = other.GetZone(num3, num2);
                                                    if (zone == ItemClass.Zone.Unzoned) {
                                                        block.SetZone(num, i, zone2);
                                                    } else if (zone2 == ItemClass.Zone.Unzoned && !flag) {
                                                        other.SetZone(num3, num2, zone);
                                                    }
                                                }
                                            }
                                        }
                                        num3++;
                                    }
                                    num2++;
                                }
                                if (!flag2) {
                                    valid &= ~(1uL << (i << 3 | num));
                                    break;
                                }
                                if (flag3) {
                                    shared |= 1uL << (i << 3 | num);
                                }
                            }
                            num++;
                        }
                    }
                }
            }
        }

        internal static void CalculateBlock2(ref ZoneBlock block, ushort blockID) {
            const float halfGrid = ZONEGRID_RESOLUTION / 2f;
            if ((block.m_flags & 3u) != 1u) {
                return;
            }
            int rowCount = block.RowCount;
            Vector2 a = new Vector2((float)Math.Cos(block.m_angle) * 8f, (float)Math.Sin(block.m_angle) * 8f);
            Vector2 a2 = new Vector2(a.y, -a.x);
            Vector2 a3 = VectorUtils.XZ(block.m_position);
            Vector2 vector = a3 - 4f * a - 4f * a2;
            Vector2 vector2 = a3 + 0f * a - 4f * a2;
            Vector2 vector3 = a3 + 0f * a + (rowCount - 4) * a2;
            Vector2 vector4 = a3 - 4f * a + (rowCount - 4) * a2;
            float num = EMath.Min(EMath.Min(vector.x, vector2.x), EMath.Min(vector3.x, vector4.x));
            float num2 = EMath.Min(EMath.Min(vector.y, vector2.y), EMath.Min(vector3.y, vector4.y));
            float num3 = EMath.Max(EMath.Max(vector.x, vector2.x), EMath.Max(vector3.x, vector4.x));
            float num4 = EMath.Max(EMath.Max(vector.y, vector2.y), EMath.Max(vector3.y, vector4.y));
            ulong num5 = block.m_valid;
            ulong shared = 0uL;
            ZoneManager instance = Singleton<ZoneManager>.instance;
            ZoneBlock[] cachedBlocks = instance.m_cachedBlocks.m_buffer;
            ZoneBlock[] blocks = instance.m_blocks.m_buffer;
            ushort[] zoneGrid = instance.m_zoneGrid;
            for (int i = 0; i < instance.m_cachedBlocks.m_size; i++) {
                CalculateImplementation2(ref block, blockID, ref cachedBlocks[i], ref num5, ref shared, num, num2, num3, num4);
            }
            int num6 = EMath.Max((int)((num - 46f) / ZONEGRID_CELL_SIZE + halfGrid), 0);
            int num7 = EMath.Max((int)((num2 - 46f) / ZONEGRID_CELL_SIZE + halfGrid), 0);
            int num8 = EMath.Min((int)((num3 + 46f) / ZONEGRID_CELL_SIZE + halfGrid), ZONEGRID_RESOLUTION - 1);
            int num9 = EMath.Min((int)((num4 + 46f) / ZONEGRID_CELL_SIZE + halfGrid), ZONEGRID_RESOLUTION - 1);
            for (int j = num7; j <= num9; j++) {
                for (int k = num6; k <= num8; k++) {
                    ushort num10 = zoneGrid[j * ZONEGRID_RESOLUTION + k];
                    while (num10 != 0) {
                        Vector3 position = blocks[num10].m_position;
                        float num12 = EMath.Max(EMath.Max(num - 46f - position.x, num2 - 46f - position.z), EMath.Max(position.x - num3 - 46f, position.z - num4 - 46f));
                        if (num12 < 0f && num10 != blockID) {
                            CalculateImplementation2(ref block, blockID, ref blocks[num10], ref num5, ref shared, num, num2, num3, num4);
                        }
                        num10 = blocks[num10].m_nextGridBlock;
                    }
                }
            }
            ulong num13 = 144680345676153346uL;
            for (int l = 0; l < 7; l++) {
                num5 = ((num5 & ~num13) | (num5 & num5 << 1 & num13));
                num13 <<= 1;
            }
            block.m_valid = num5;
            block.m_shared = shared;
        }

        internal static void CheckBlock(ref ZoneBlock block, ref ZoneBlock other, int[] xBuffer, ItemClass.Zone zone, Vector2 startPos, Vector2 xDir, Vector2 zDir, Quad2 quad) {
            float num = EMath.Abs(other.m_angle - block.m_angle) * 0.636619747f;
            num -= EMath.Floor(num);
            if (num >= 0.01f && num <= 0.99f) {
                return;
            }
            int rowCount = other.RowCount;
            Vector2 a = new Vector2((float)Math.Cos(other.m_angle), (float)Math.Sin(other.m_angle)) * 8f;
            Vector2 a2 = new Vector2(a.y, -a.x);
            ulong num2 = other.m_valid & ~(other.m_occupied1 | other.m_occupied2);
            Vector2 a3 = VectorUtils.XZ(other.m_position);
            if (!quad.Intersect(new Quad2 {
                a = a3 - 4f * a - 4f * a2,
                b = a3 - 4f * a2,
                c = a3 + (rowCount - 4) * a2,
                d = a3 - 4f * a + (rowCount - 4) * a2
            })) {
                return;
            }
            for (int i = 0; i < rowCount; i++) {
                Vector2 b = (i - 3.5f) * a2;
                for (int j = 0; j < 4; j++) {
                    if ((num2 & 1uL << (i << 3 | j)) != 0uL) {
                        if (other.GetZone(j, i) == zone) {
                            Vector2 b2 = (j - 3.5f) * a;
                            Vector2 vector = a3 + b2 + b - startPos;
                            float num3 = (vector.x * xDir.x + vector.y * xDir.y) * 0.015625f;
                            float num4 = (vector.x * zDir.x + vector.y * zDir.y) * 0.015625f;
                            int num5 = EMath.RoundToInt(num3);
                            int num6 = EMath.RoundToInt(num4);
                            if (num5 >= 0 && num5 <= 6 && num6 >= -6 && num6 <= 6) {
                                if (EMath.Abs(num3 - num5) < 0.0125f && EMath.Abs(num4 - num6) < 0.0125f) {
                                    if (j == 0 || num5 != 0) {
                                        xBuffer[num6 + 6] |= 1 << num5;
                                        if (j == 0) {
                                            xBuffer[num6 + 6] |= 1 << num5 + 16;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        internal static bool IsGoodPlace(Building[] buildings, ushort[] buildingGrid, Vector2 position) {
            int num = EMath.Max((int)((position.x - 104f) / 64f + 135f), 0);
            int num2 = EMath.Max((int)((position.y - 104f) / 64f + 135f), 0);
            int num3 = EMath.Min((int)((position.x + 104f) / 64f + 135f), 269);
            int num4 = EMath.Min((int)((position.y + 104f) / 64f + 135f), 269);
            for (int i = num2; i <= num4; i++) {
                for (int j = num; j <= num3; j++) {
                    ushort num5 = buildingGrid[i * 270 + j];
                    while (num5 != 0) {
                        Building.Flags flags = buildings[num5].m_flags;
                        if ((flags & (Building.Flags.Created | Building.Flags.Deleted)) == Building.Flags.Created) {
                            buildings[num5].GetInfoWidthLength(out BuildingInfo buildingInfo, out int num7, out int num8);
                            if (buildingInfo != null) {
                                float num9 = buildingInfo.m_buildingAI.ElectricityGridRadius();
                                if (num9 > 0.1f || buildingInfo.m_class.m_service == ItemClass.Service.Electricity) {
                                    Vector2 b = VectorUtils.XZ(buildings[num5].m_position);
                                    num9 = EMath.Max(8f, num9) + 32f;
                                    if (Vector2.SqrMagnitude(position - b) < num9 * num9) {
                                        return true;
                                    }
                                }
                            }
                        }
                        num5 = buildings[num5].m_nextGridBuilding;
                    }
                }
            }
            return false;
        }

        internal static void SimulationStep(ref ZoneBlock block, ushort blockID) {
            const float halfGrid = ZONEGRID_RESOLUTION / 2f;
            ZoneManager zmInstance = Singleton<ZoneManager>.instance;
            DistrictManager dmInstance = Singleton<DistrictManager>.instance;
            SimulationManager smInstance = Singleton<SimulationManager>.instance;
            BuildingManager bmInstance = Singleton<BuildingManager>.instance;
            District[] districts = dmInstance.m_districts.m_buffer;
            ZoneBlock[] zoneBlocks = zmInstance.m_blocks.m_buffer;
            Building[] buildings = bmInstance.m_buildings.m_buffer;
            ushort[] buildingGrid = bmInstance.m_buildingGrid;
            int rowCount = block.RowCount;
            Vector2 vector = new Vector2((float)Math.Cos(block.m_angle) * 8f, (float)Math.Sin(block.m_angle) * 8f);
            Vector2 vector2 = new Vector2(vector.y, -vector.x);
            ulong num = block.m_valid & ~(block.m_occupied1 | block.m_occupied2);
            int num2 = 0;
            ItemClass.Zone zone = ItemClass.Zone.Unzoned;
            int num3 = 0;
            while (num3 < 4 && zone == ItemClass.Zone.Unzoned) {
                num2 = smInstance.m_randomizer.Int32((uint)rowCount);
                if ((num & 1uL << (num2 << 3)) != 0uL) {
                    zone = block.GetZone(0, num2);
                }
                num3++;
            }
            byte district = dmInstance.GetDistrict(block.m_position);
            int num4;
            switch (zone) {
            case ItemClass.Zone.ResidentialLow:
                num4 = zmInstance.m_actualResidentialDemand;
                num4 += districts[district].CalculateResidentialLowDemandOffset();
                break;
            case ItemClass.Zone.ResidentialHigh:
                num4 = zmInstance.m_actualResidentialDemand;
                num4 += districts[district].CalculateResidentialHighDemandOffset();
                break;
            case ItemClass.Zone.CommercialLow:
                num4 = zmInstance.m_actualCommercialDemand;
                num4 += districts[district].CalculateCommercialLowDemandOffset();
                break;
            case ItemClass.Zone.CommercialHigh:
                num4 = zmInstance.m_actualCommercialDemand;
                num4 += districts[district].CalculateCommercialHighDemandOffset();
                break;
            case ItemClass.Zone.Industrial:
                num4 = zmInstance.m_actualWorkplaceDemand;
                num4 += districts[district].CalculateIndustrialDemandOffset();
                break;
            case ItemClass.Zone.Office:
                num4 = zmInstance.m_actualWorkplaceDemand;
                num4 += districts[district].CalculateOfficeDemandOffset();
                break;
            default:
                return;
            }
            Vector2 a = VectorUtils.XZ(block.m_position);
            Vector2 vector3 = a - 3.5f * vector + (num2 - 3.5f) * vector2;
            int[] tmpXBuffer = zmInstance.m_tmpXBuffer;
            for (int i = 0; i < 13; i++) {
                tmpXBuffer[i] = 0;
            }
            Quad2 quad = default;
            quad.a = a - 4f * vector + (num2 - 10f) * vector2;
            quad.b = a + 3f * vector + (num2 - 10f) * vector2;
            quad.c = a + 3f * vector + (num2 + 2f) * vector2;
            quad.d = a - 4f * vector + (num2 + 2f) * vector2;
            Vector2 vector4 = quad.Min();
            Vector2 vector5 = quad.Max();
            int num5 = EMath.Max((int)((vector4.x - 46f) / ZONEGRID_CELL_SIZE + halfGrid), 0);
            int num6 = EMath.Max((int)((vector4.y - 46f) / ZONEGRID_CELL_SIZE + halfGrid), 0);
            int num7 = EMath.Min((int)((vector5.x + 46f) / ZONEGRID_CELL_SIZE + halfGrid), ZONEGRID_RESOLUTION - 1);
            int num8 = EMath.Min((int)((vector5.y + 46f) / ZONEGRID_CELL_SIZE + halfGrid), ZONEGRID_RESOLUTION - 1);
            for (int j = num6; j <= num8; j++) {
                for (int k = num5; k <= num7; k++) {
                    ushort num9 = zmInstance.m_zoneGrid[j * ZONEGRID_RESOLUTION + k];
                    while (num9 != 0) {
                        Vector3 position = zoneBlocks[num9].m_position;
                        float num11 = EMath.Max(EMath.Max(vector4.x - 46f - position.x, vector4.y - 46f - position.z), EMath.Max(position.x - vector5.x - 46f, position.z - vector5.y - 46f));
                        if (num11 < 0f) {
                            CheckBlock(ref block, ref zoneBlocks[num9], tmpXBuffer, zone, vector3, vector, vector2, quad);
                        }
                        num9 = zoneBlocks[num9].m_nextGridBlock;
                    }
                }
            }
            for (int l = 0; l < 13; l++) {
                uint num12 = (uint)tmpXBuffer[l];
                int num13 = 0;
                bool flag = (num12 & 0x30000u) == 0x30000u;
                bool flag2 = false;
                while ((num12 & 1u) != 0u) {
                    num13++;
                    flag2 = ((num12 & 0x10000u) != 0u);
                    num12 >>= 1;
                }
                if (num13 == 5 || num13 == 6) {
                    if (flag2) {
                        num13 -= smInstance.m_randomizer.Int32(2u) + 2;
                    } else {
                        num13 = 4;
                    }
                    num13 |= 0x20000;
                } else if (num13 == 7) {
                    num13 = 4;
                    num13 |= 0x20000;
                }
                if (flag) {
                    num13 |= 0x10000;
                }
                tmpXBuffer[l] = num13;
            }
            int num14 = tmpXBuffer[6] & 0xffff;
            if (num14 == 0) {
                return;
            }
            bool flag3 = IsGoodPlace(buildings, buildingGrid, vector3);
            if (smInstance.m_randomizer.Int32(100u) >= num4) {
                if (flag3) {
                    zmInstance.m_goodAreaFound[(int)zone] = 1024;
                }
                return;
            }
            if (!flag3 && zmInstance.m_goodAreaFound[(int)zone] > -1024) {
                if (zmInstance.m_goodAreaFound[(int)zone] == 0) {
                    zmInstance.m_goodAreaFound[(int)zone] = -1;
                }
                return;
            }
            int num15 = 6;
            int num16 = 6;
            bool flag4 = true;
            while (true) {
                if (flag4) {
                    while (num15 != 0) {
                        if ((tmpXBuffer[num15 - 1] & 0xffff) != num14) {
                            break;
                        }
                        num15--;
                    }
                    while (num16 != 12) {
                        if ((tmpXBuffer[num16 + 1] & 0xffff) != num14) {
                            break;
                        }
                        num16++;
                    }
                } else {
                    while (num15 != 0) {
                        if ((tmpXBuffer[num15 - 1] & 0xffff) < num14) {
                            break;
                        }
                        num15--;
                    }
                    while (num16 != 12) {
                        if ((tmpXBuffer[num16 + 1] & 0xffff) < num14) {
                            break;
                        }
                        num16++;
                    }
                }
                int num17 = num15;
                int num18 = num16;
                while (num17 != 0) {
                    if ((tmpXBuffer[num17 - 1] & 0xffff) < 2) {
                        break;
                    }
                    num17--;
                }
                while (num18 != 12) {
                    if ((tmpXBuffer[num18 + 1] & 0xffff) < 2) {
                        break;
                    }
                    num18++;
                }
                bool flag5 = num17 != 0 && num17 == num15 - 1;
                bool flag6 = num18 != 12 && num18 == num16 + 1;
                if (flag5 && flag6) {
                    if (num16 - num15 > 2) {
                        break;
                    }
                    if (num14 <= 2) {
                        if (!flag4) {
                            goto Block_34;
                        }
                    } else {
                        num14--;
                    }
                } else if (flag5) {
                    if (num16 - num15 > 1) {
                        goto Block_36;
                    }
                    if (num14 <= 2) {
                        if (!flag4) {
                            goto Block_38;
                        }
                    } else {
                        num14--;
                    }
                } else if (flag6) {
                    if (num16 - num15 > 1) {
                        goto Block_40;
                    }
                    if (num14 <= 2) {
                        if (!flag4) {
                            goto Block_42;
                        }
                    } else {
                        num14--;
                    }
                } else {
                    if (num15 != num16) {
                        goto IL_882;
                    }
                    if (num14 <= 2) {
                        if (!flag4) {
                            goto Block_45;
                        }
                    } else {
                        num14--;
                    }
                }
                flag4 = false;
            }
            num15++;
            num16--;
Block_34:
            goto IL_88F;
Block_36:
            num15++;
Block_38:
            goto IL_88F;
Block_40:
            num16--;
Block_42:
Block_45:
IL_882:
IL_88F:
            int num19;
            int num20;
            if (num14 == 1 && num16 - num15 >= 1) {
                num15 += smInstance.m_randomizer.Int32((uint)(num16 - num15));
                num16 = num15 + 1;
                num19 = num15 + smInstance.m_randomizer.Int32(2u);
                num20 = num19;
            } else {
                do {
                    num19 = num15;
                    num20 = num16;
                    if (num16 - num15 == 2) {
                        if (smInstance.m_randomizer.Int32(2u) == 0) {
                            num20--;
                        } else {
                            num19++;
                        }
                    } else if (num16 - num15 == 3) {
                        if (smInstance.m_randomizer.Int32(2u) == 0) {
                            num20 -= 2;
                        } else {
                            num19 += 2;
                        }
                    } else if (num16 - num15 == 4) {
                        if (smInstance.m_randomizer.Int32(2u) == 0) {
                            num16 -= 2;
                            num20 -= 3;
                        } else {
                            num15 += 2;
                            num19 += 3;
                        }
                    } else if (num16 - num15 == 5) {
                        if (smInstance.m_randomizer.Int32(2u) == 0) {
                            num16 -= 3;
                            num20 -= 2;
                        } else {
                            num15 += 3;
                            num19 += 2;
                        }
                    } else if (num16 - num15 >= 6) {
                        if (num15 == 0 || num16 == 12) {
                            if (num15 == 0) {
                                num15 = 3;
                                num19 = 2;
                            }
                            if (num16 == 12) {
                                num16 = 9;
                                num20 = 10;
                            }
                        } else if (smInstance.m_randomizer.Int32(2u) == 0) {
                            num16 = num15 + 3;
                            num20 = num19 + 2;
                        } else {
                            num15 = num16 - 3;
                            num19 = num20 - 2;
                        }
                    }
                }
                while (num16 - num15 > 3 || num20 - num19 > 3);
            }
            int num21 = 4;
            int num22 = num16 - num15 + 1;
            BuildingInfo.ZoningMode zoningMode = BuildingInfo.ZoningMode.Straight;
            bool flag7 = true;
            for (int m = num15; m <= num16; m++) {
                num21 = EMath.Min(num21, tmpXBuffer[m] & 0xffff);
                if ((tmpXBuffer[m] & 0x20000) == 0) {
                    flag7 = false;
                }
            }
            if (num16 > num15) {
                if ((tmpXBuffer[num15] & 0x10000) != 0) {
                    zoningMode = BuildingInfo.ZoningMode.CornerLeft;
                    num20 = num15 + num20 - num19;
                    num19 = num15;
                }
                if ((tmpXBuffer[num16] & 0x10000) != 0 && (zoningMode != BuildingInfo.ZoningMode.CornerLeft || smInstance.m_randomizer.Int32(2u) == 0)) {
                    zoningMode = BuildingInfo.ZoningMode.CornerRight;
                    num19 = num16 + num19 - num20;
                    num20 = num16;
                }
            }
            int num23 = 4;
            int num24 = num20 - num19 + 1;
            BuildingInfo.ZoningMode zoningMode2 = BuildingInfo.ZoningMode.Straight;
            bool flag8 = true;
            for (int n = num19; n <= num20; n++) {
                num23 = EMath.Min(num23, tmpXBuffer[n] & 0xffff);
                if ((tmpXBuffer[n] & 0x20000) == 0) {
                    flag8 = false;
                }
            }
            if (num20 > num19) {
                if ((tmpXBuffer[num19] & 0x10000) != 0) {
                    zoningMode2 = BuildingInfo.ZoningMode.CornerLeft;
                }
                if ((tmpXBuffer[num20] & 0x10000) != 0 && (zoningMode2 != BuildingInfo.ZoningMode.CornerLeft || smInstance.m_randomizer.Int32(2u) == 0)) {
                    zoningMode2 = BuildingInfo.ZoningMode.CornerRight;
                }
            }
            ItemClass.Service service;
            ItemClass.SubService subService = ItemClass.SubService.None;
            ItemClass.Level level = ItemClass.Level.Level1;
            switch (zone) {
            case ItemClass.Zone.ResidentialLow:
                service = ItemClass.Service.Residential;
                subService = ItemClass.SubService.ResidentialLow;
                break;
            case ItemClass.Zone.ResidentialHigh:
                service = ItemClass.Service.Residential;
                subService = ItemClass.SubService.ResidentialHigh;
                break;
            case ItemClass.Zone.CommercialLow:
                service = ItemClass.Service.Commercial;
                subService = ItemClass.SubService.CommercialLow;
                break;
            case ItemClass.Zone.CommercialHigh:
                service = ItemClass.Service.Commercial;
                subService = ItemClass.SubService.CommercialHigh;
                break;
            case ItemClass.Zone.Industrial:
                service = ItemClass.Service.Industrial;
                break;
            case ItemClass.Zone.Office:
                service = ItemClass.Service.Office;
                break;
            default:
                return;
            }
            BuildingInfo buildingInfo = null;
            Vector3 vector6 = Vector3.zero;
            int num25 = 0;
            int num26 = 0;
            int num27 = 0;
            BuildingInfo.ZoningMode zoningMode3 = BuildingInfo.ZoningMode.Straight;
            int num28 = 0;
            while (num28 < 6) {
                switch (num28) {
                case 0:
                    if (zoningMode != BuildingInfo.ZoningMode.Straight) {
                        num25 = num15 + num16 + 1;
                        num26 = num21;
                        num27 = num22;
                        zoningMode3 = zoningMode;
                        goto IL_D5D;
                    }
                    break;
                case 1:
                    if (zoningMode2 != BuildingInfo.ZoningMode.Straight) {
                        num25 = num19 + num20 + 1;
                        num26 = num23;
                        num27 = num24;
                        zoningMode3 = zoningMode2;
                        goto IL_D5D;
                    }
                    break;
                case 2:
                    if (zoningMode != BuildingInfo.ZoningMode.Straight) {
                        if (num21 >= 4) {
                            num25 = num15 + num16 + 1;
                            num26 = ((!flag7) ? 2 : 3);
                            num27 = num22;
                            zoningMode3 = zoningMode;
                            goto IL_D5D;
                        }
                    }
                    break;
                case 3:
                    if (zoningMode2 != BuildingInfo.ZoningMode.Straight) {
                        if (num23 >= 4) {
                            num25 = num19 + num20 + 1;
                            num26 = ((!flag8) ? 2 : 3);
                            num27 = num24;
                            zoningMode3 = zoningMode2;
                            goto IL_D5D;
                        }
                    }
                    break;
                case 4:
                    num25 = num15 + num16 + 1;
                    num26 = num21;
                    num27 = num22;
                    zoningMode3 = BuildingInfo.ZoningMode.Straight;
                    goto IL_D5D;
                case 5:
                    num25 = num19 + num20 + 1;
                    num26 = num23;
                    num27 = num24;
                    zoningMode3 = BuildingInfo.ZoningMode.Straight;
                    goto IL_D5D;
                default:
                    goto IL_D5D;
                }
IL_E9C:
                num28++;
                continue;
IL_D5D:
                vector6 = block.m_position + VectorUtils.X_Y((num26 * 0.5f - 4f) * vector + (num25 * 0.5f + num2 - 10f) * vector2);
                if (zone == ItemClass.Zone.Industrial) {
                    ZoneBlock.GetIndustryType(vector6, out subService, out level);
                } else if (zone == ItemClass.Zone.CommercialLow || zone == ItemClass.Zone.CommercialHigh) {
                    ZoneBlock.GetCommercialType(vector6, zone, num27, num26, out subService, out level);
                } else if (zone == ItemClass.Zone.ResidentialLow || zone == ItemClass.Zone.ResidentialHigh) {
                    ZoneBlock.GetResidentialType(vector6, zone, num27, num26, out subService, out level);
                } else if (zone == ItemClass.Zone.Office) {
                    ZoneBlock.GetOfficeType(vector6, zone, num27, num26, out subService, out level);
                }
                byte district2 = dmInstance.GetDistrict(vector6);
                ushort style = districts[district2].m_Style;
                if (!(bmInstance.m_BuildingWrapper is null)) {
                    bmInstance.m_BuildingWrapper.OnCalculateSpawn(vector6, ref service, ref subService, ref level, ref style);
                }
                buildingInfo = bmInstance.GetRandomBuildingInfo(ref smInstance.m_randomizer, service, subService, level, num27, num26, zoningMode3, style);
                if (!(buildingInfo is null)) {
                    break;
                }
                goto IL_E9C;
            }
            if (buildingInfo is null) {
                return;
            }
            float num29 = Singleton<TerrainManager>.instance.WaterLevel(VectorUtils.XZ(vector6));
            if (num29 > vector6.y) {
                return;
            }
            if (Singleton<DisasterManager>.instance.IsEvacuating(vector6)) {
                return;
            }
            float num30 = block.m_angle + 1.57079637f;
            if (zoningMode3 == BuildingInfo.ZoningMode.CornerLeft && buildingInfo.m_zoningMode == BuildingInfo.ZoningMode.CornerRight) {
                num30 -= 1.57079637f;
                num26 = num27;
            } else if (zoningMode3 == BuildingInfo.ZoningMode.CornerRight && buildingInfo.m_zoningMode == BuildingInfo.ZoningMode.CornerLeft) {
                num30 += 1.57079637f;
                num26 = num27;
            }
            if (bmInstance.CreateBuilding(out ushort num31, ref smInstance.m_randomizer, buildingInfo, vector6, num30, num26, smInstance.m_currentBuildIndex)) {
                smInstance.m_currentBuildIndex += 1u;
                switch (service) {
                case ItemClass.Service.Residential:
                    zmInstance.m_actualResidentialDemand = EMath.Max(0, zmInstance.m_actualResidentialDemand - 5);
                    break;
                case ItemClass.Service.Commercial:
                    zmInstance.m_actualCommercialDemand = EMath.Max(0, zmInstance.m_actualCommercialDemand - 5);
                    break;
                case ItemClass.Service.Industrial:
                    zmInstance.m_actualWorkplaceDemand = EMath.Max(0, zmInstance.m_actualWorkplaceDemand - 5);
                    break;
                case ItemClass.Service.Office:
                    zmInstance.m_actualWorkplaceDemand = EMath.Max(0, zmInstance.m_actualWorkplaceDemand - 5);
                    break;
                }
                if (zone == ItemClass.Zone.ResidentialHigh || zone == ItemClass.Zone.CommercialHigh) {
                    buildings[num31].m_flags |= Building.Flags.HighDensity;
                }
            }
            zmInstance.m_goodAreaFound[(int)zone] = 1024;
        }
    }
}
