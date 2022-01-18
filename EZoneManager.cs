using ColossalFramework.Math;
using System;
using System.Threading;
using UnityEngine;

namespace EManagersLib {
    internal static class EZoneManager {
        internal const int MAX_BLOCK_COUNT = 49152;
        internal const int MAX_MAP_BLOCKS = 16384;
        internal const int MAX_ASSET_BLOCKS = 4096;
        internal const float ZONEGRID_CELL_SIZE = 64f;
        internal const int DEFGRID_RESOLUTION = 150;
        internal const int ZONEGRID_RESOLUTION = 270;

        internal static bool CheckSpace(ZoneManager zmInstance, ushort[] zoneGrid, Vector3 position, float angle, int width, int length, out int offset) {
            const float halfGrid = ZONEGRID_RESOLUTION / 2f;
            float num = EMath.Min(72f, (width + length) * 4f) + 6f;
            float num2 = position.x - num;
            float num3 = position.z - num;
            float num4 = position.x + num;
            float num5 = position.z + num;
            ulong num6 = 0uL;
            ulong num7 = 0uL;
            ulong num8 = 0uL;
            ulong num9 = 0uL;
            int num10 = EMath.Max((int)((num2 - 46f) / ZONEGRID_CELL_SIZE + halfGrid), 0);
            int num11 = EMath.Max((int)((num3 - 46f) / ZONEGRID_CELL_SIZE + halfGrid), 0);
            int num12 = EMath.Min((int)((num4 + 46f) / ZONEGRID_CELL_SIZE + halfGrid), ZONEGRID_RESOLUTION - 1);
            int num13 = EMath.Min((int)((num5 + 46f) / ZONEGRID_CELL_SIZE + halfGrid), ZONEGRID_RESOLUTION - 1);
            ZoneBlock[] blocks = zmInstance.m_blocks.m_buffer;
            for (int i = num11; i <= num13; i++) {
                for (int j = num10; j <= num12; j++) {
                    ushort num14 = zoneGrid[i * ZONEGRID_RESOLUTION + j];
                    while (num14 != 0) {
                        Vector3 position2 = blocks[num14].m_position;
                        float num16 = EMath.Max(EMath.Max(num2 - 46f - position2.x, num3 - 46f - position2.z), EMath.Max(position2.x - num4 - 46f, position2.z - num5 - 46f));
                        if (num16 < 0f) {
                            zmInstance.CheckSpace(num14, position, angle, width, length, ref num6, ref num7, ref num8, ref num9);
                        }
                        num14 = blocks[num14].m_nextGridBlock;
                    }
                }
            }
            bool result = true;
            bool flag = false;
            bool flag2 = false;
            for (int k = 0; k < length; k++) {
                for (int l = 0; l < width; l++) {
                    bool flag3;
                    if (k < 4) {
                        flag3 = ((num6 & 1uL << (k << 4 | l)) != 0uL);
                    } else if (k < 8) {
                        flag3 = ((num7 & 1uL << (k - 4 << 4 | l)) != 0uL);
                    } else if (k < 12) {
                        flag3 = ((num8 & 1uL << (k - 8 << 4 | l)) != 0uL);
                    } else {
                        flag3 = ((num9 & 1uL << (k - 12 << 4 | l)) != 0uL);
                    }
                    if (!flag3) {
                        result = false;
                        if (l < width >> 1) {
                            flag = true;
                        }
                        if (l >= width + 1 >> 1) {
                            flag2 = true;
                        }
                    }
                }
            }
            if (flag == flag2) {
                offset = 0;
            } else if (flag) {
                offset = 1;
            } else if (flag2) {
                offset = -1;
            } else {
                offset = 0;
            }
            return result;
        }

        internal static ushort[] EnsureCapacity(ZoneManager zmInstance) {
            if (zmInstance.m_zoneGrid.Length != ZONEGRID_RESOLUTION * ZONEGRID_RESOLUTION) {
                zmInstance.m_zoneGrid = new ushort[ZONEGRID_RESOLUTION * ZONEGRID_RESOLUTION];
            }
            return zmInstance.m_zoneGrid;
        }

        internal static void ReleaseBlockImplementation(ZoneManager zmInstance, ushort block, ref ZoneBlock data) {
            const float halfGrid = ZONEGRID_RESOLUTION / 2f;
            if (data.m_flags != 0u) {
                ushort[] zoneGrid = zmInstance.m_zoneGrid;
                ZoneBlock[] zoneBlocks = zmInstance.m_blocks.m_buffer;
                data.m_flags |= 2u;
                zmInstance.m_cachedBlocks.Add(data);
                int rowCount = data.RowCount;
                float angle = data.m_angle;
                Vector2 a = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 8f;
                Vector2 a2 = new Vector2(a.y, -a.x);
                Vector2 a3 = VectorUtils.XZ(data.m_position);
                UpdateBlocks(zmInstance, zoneGrid, new Quad2 {
                    a = a3 - 4f * a - 4f * a2,
                    b = a3 + 0f * a - 4f * a2,
                    c = a3 + 0f * a + (rowCount - 4) * a2,
                    d = a3 - 4f * a + (rowCount - 4) * a2
                });
                data.m_flags = 0u;
                int num = EMath.Clamp((int)(data.m_position.x / ZONEGRID_CELL_SIZE + halfGrid), 0, ZONEGRID_RESOLUTION - 1);
                int num2 = EMath.Clamp((int)(data.m_position.z / ZONEGRID_CELL_SIZE + halfGrid), 0, ZONEGRID_RESOLUTION - 1);
                int num3 = num2 * ZONEGRID_RESOLUTION + num;
                while (!Monitor.TryEnter(zmInstance.m_zoneGrid, SimulationManager.SYNCHRONIZE_TIMEOUT)) ;
                try {
                    ushort num4 = 0;
                    ushort num5 = zoneGrid[num3];
                    while (num5 != 0) {
                        if (num5 == block) {
                            if (num4 == 0) {
                                zoneGrid[num3] = data.m_nextGridBlock;
                            } else {
                                zoneBlocks[num4].m_nextGridBlock = data.m_nextGridBlock;
                            }
                            break;
                        }
                        num4 = num5;
                        num5 = zoneBlocks[num5].m_nextGridBlock;
                    }
                    data.m_nextGridBlock = 0;
                } finally {
                    Monitor.Exit(zmInstance.m_zoneGrid);
                }
                zmInstance.m_blocks.ReleaseItem(block);
                zmInstance.m_blockCount = (int)(zmInstance.m_blocks.ItemCount() - 1u);
            }
        }

        internal static void TerrainUpdated(ZoneManager zmInstance, ushort[] zoneGrid, TerrainArea zoneArea) {
            const float halfGrid = ZONEGRID_RESOLUTION / 2f;
            float x = zoneArea.m_min.x;
            float z = zoneArea.m_min.z;
            float x2 = zoneArea.m_max.x;
            float z2 = zoneArea.m_max.z;
            int num = EMath.Max((int)((x - 46f) / ZONEGRID_CELL_SIZE + halfGrid), 0);
            int num2 = EMath.Max((int)((z - 46f) / ZONEGRID_CELL_SIZE + halfGrid), 0);
            int num3 = EMath.Min((int)((x2 + 46f) / ZONEGRID_CELL_SIZE + halfGrid), ZONEGRID_RESOLUTION - 1);
            int num4 = EMath.Min((int)((z2 + 46f) / ZONEGRID_CELL_SIZE + halfGrid), ZONEGRID_RESOLUTION - 1);
            ZoneBlock[] blocks = zmInstance.m_blocks.m_buffer;
            for (int i = num2; i <= num4; i++) {
                for (int j = num; j <= num3; j++) {
                    ushort num5 = zoneGrid[i * ZONEGRID_RESOLUTION + j];
                    while (num5 != 0) {
                        Vector3 position = blocks[num5].m_position;
                        float num7 = EMath.Max(EMath.Max(x - 46f - position.x, z - 46f - position.z), EMath.Max(position.x - x2 - 46f, position.z - z2 - 46f));
                        if (num7 < 0f) {
                            blocks[num5].ZonesUpdated(num5, x, z, x2, z2);
                        }
                        num5 = blocks[num5].m_nextGridBlock;
                    }
                }
            }
        }

        internal static void UpdateBlocks(ZoneManager zmInstance, ushort[] zoneGrid, Quad2 quad) {
            const float halfGrid = ZONEGRID_RESOLUTION / 2f;
            Vector2 vector = quad.Min();
            Vector2 vector2 = quad.Max();
            int num = EMath.Max((int)((vector.x - 46f) / ZONEGRID_CELL_SIZE + halfGrid), 0);
            int num2 = EMath.Max((int)((vector.y - 46f) / ZONEGRID_CELL_SIZE + halfGrid), 0);
            int num3 = EMath.Min((int)((vector2.x + 46f) / ZONEGRID_CELL_SIZE + halfGrid), ZONEGRID_RESOLUTION - 1);
            int num4 = EMath.Min((int)((vector2.y + 46f) / ZONEGRID_CELL_SIZE + halfGrid), ZONEGRID_RESOLUTION - 1);
            ZoneBlock[] blocks = zmInstance.m_blocks.m_buffer;
            for (int i = num2; i <= num4; i++) {
                for (int j = num; j <= num3; j++) {
                    ushort num5 = zoneGrid[i * ZONEGRID_RESOLUTION + j];
                    while (num5 != 0) {
                        Vector3 position = blocks[num5].m_position;
                        float num7 = EMath.Max(EMath.Max(vector.x - 46f - position.x, vector.y - 46f - position.z), EMath.Max(position.x - vector2.x - 46f, position.z - vector2.y - 46f));
                        if (num7 < 0f && (blocks[num5].m_flags & 3u) == 1u) {
                            int rowCount = blocks[num5].RowCount;
                            float angle = blocks[num5].m_angle;
                            Vector2 a = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 8f;
                            Vector2 a2 = new Vector2(a.y, -a.x);
                            Vector2 a3 = VectorUtils.XZ(position);
                            if (quad.Intersect(new Quad2 {
                                a = a3 - 4f * a - 4f * a2,
                                b = a3 + 0f * a - 4f * a2,
                                c = a3 + 0f * a + (rowCount - 4) * a2,
                                d = a3 - 4f * a + (rowCount - 4) * a2
                            })) {
                                zmInstance.m_updatedBlocks[num5 >> 6] |= 1uL << num5;
                                zmInstance.m_blocksUpdated = true;
                            }
                        }
                        num5 = blocks[num5].m_nextGridBlock;
                    }
                }
            }
        }
    }
}
