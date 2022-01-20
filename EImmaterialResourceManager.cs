using ColossalFramework;
using ColossalFramework.Math;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace EManagersLib {
    internal static class EImmaterialResourceManager {
        internal const int RESOURCE_COUNT = 29;
        internal const int DEFAULTGRID_RESOLUTION = 256;
        internal const int RESOURCEGRID_RESOLUTION = 450;
        private struct CellLocation {
            public short m_x;
            public short m_z;
        }
        private struct AreaQueueItem {
            public int m_cost;
            public CellLocation m_location;
            public CellLocation m_source;
        }

        private static MinHeap<AreaQueueItem> m_tempAreaQueue;
        private static Dictionary<CellLocation, int> m_tempAreaCost;

        private sealed class AreaQueueItemComparer : Comparer<AreaQueueItem> {
            public override int Compare(AreaQueueItem x, AreaQueueItem y) => x.m_cost < y.m_cost ? -1 : (x.m_cost > y.m_cost ? 1 : 0);
        }

        internal static void UpdateResourceMapping(ImmaterialResourceManager irmInstance) {
            IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                using (var codes = instructions.GetEnumerator()) {
                    while (codes.MoveNext()) {
                        var cur = codes.Current;
                        if (cur.LoadsConstant(1f / (38.4f * DEFAULTGRID_RESOLUTION))) {
                            cur.operand = 1f / (38.4f * RESOURCEGRID_RESOLUTION);
                            yield return cur;
                        } else if (cur.LoadsConstant(DEFAULTGRID_RESOLUTION - 1) && codes.MoveNext()) {
                            var next = codes.Current;
                            if (next.LoadsConstant(DEFAULTGRID_RESOLUTION - 1)) {
                                cur.operand = RESOURCEGRID_RESOLUTION - 1;
                                next.operand = RESOURCEGRID_RESOLUTION - 1;
                            }
                            yield return cur;
                            yield return next;
                        } else {
                            yield return cur;
                        }
                    }
                }
            }
            _ = Transpiler(null);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void Awake(ImmaterialResourceManager.Resource resourceMapVisible) {
            m_tempAreaQueue = new MinHeap<AreaQueueItem>(new AreaQueueItemComparer());
            m_tempAreaCost = new Dictionary<CellLocation, int>();
        }

        private static void AddResource(ref ushort buffer, int rate) => buffer = (ushort)EMath.Min(buffer + rate, 65535);

        internal static int AddParkResource(ImmaterialResourceManager irmInstance, ushort[] localTempResources, ImmaterialResourceManager.Resource resource, int rate, byte park, float radius) {
            const float halfGrid = RESOURCEGRID_RESOLUTION / 2f;
            if (rate == 0) {
                return 0;
            }
            Dictionary<CellLocation, int> tempAreaCost = m_tempAreaCost;
            MinHeap<AreaQueueItem> tempAreaQueue = m_tempAreaQueue;
            tempAreaCost.Clear();
            tempAreaQueue.Clear();
            float minRadius = EMath.Max(0f, EMath.Min(radius, 19.2f));
            float maxRadius = EMath.Max(38.4f, radius + 19.2f);
            int num3 = EMath.FloorToInt(maxRadius * maxRadius / 1474.56f);
            DistrictManager instance = Singleton<DistrictManager>.instance;
            Vector3 nameLocation = instance.m_parks.m_buffer[park].m_nameLocation;
            CellLocation cellLocation;
            cellLocation.m_x = (short)EMath.Clamp((int)(nameLocation.x / 38.4000015258789d + halfGrid), 0, RESOURCEGRID_RESOLUTION - 1);
            cellLocation.m_z = (short)EMath.Clamp((int)(nameLocation.z / 38.4000015258789d + halfGrid), 0, RESOURCEGRID_RESOLUTION - 1);
            AreaQueueItem item;
            item.m_cost = 0;
            item.m_location = cellLocation;
            item.m_source = cellLocation;
            tempAreaCost[cellLocation] = 0;
            tempAreaQueue.Insert(item);
            int num4 = 0;
            while (tempAreaQueue.Size != 0) {
                item = tempAreaQueue.Extract();
                if (!tempAreaCost.TryGetValue(item.m_location, out int areaCost) || areaCost >= item.m_cost) {
                    float num6 = item.m_cost * 1474.56f;
                    int finalRate = rate;
                    if (num6 > minRadius * minRadius) {
                        finalRate = EMath.RoundToInt(finalRate * EMath.Clamp01((maxRadius - (float)Math.Sqrt(num6)) / (maxRadius - minRadius)));
                    }
                    AddResource(ref localTempResources[(int)((item.m_location.m_z * RESOURCEGRID_RESOLUTION + item.m_location.m_x) * 29 + resource)], finalRate);
                    num4 += finalRate;
                    if (item.m_location.m_x > 0 && item.m_location.m_x <= item.m_source.m_x) {
                        AreaQueueItem item2;
                        item2.m_location.m_x = (short)(item.m_location.m_x - 1);
                        item2.m_location.m_z = item.m_location.m_z;
                        if (!tempAreaCost.TryGetValue(item2.m_location, out areaCost)) {
                            areaCost = num3 + 1;
                        }
                        if (areaCost != 0) {
                            nameLocation.x = (float)(((double)item2.m_location.m_x - halfGrid + 0.5f) * 38.4000015258789d);
                            nameLocation.z = (float)(((double)item2.m_location.m_z - halfGrid + 0.5f) * 38.4000015258789d);
                            if (instance.GetPark(nameLocation) == park) {
                                item2.m_cost = 0;
                                item2.m_source = item2.m_location;
                            } else {
                                item2.m_cost = (item2.m_location.m_x - item.m_source.m_x) *
                                               (item2.m_location.m_x - item.m_source.m_x) +
                                               (item2.m_location.m_z - item.m_source.m_z) *
                                               (item2.m_location.m_z - item.m_source.m_z);
                                item2.m_source = item.m_source;
                            }
                            if (item2.m_cost < areaCost) {
                                tempAreaCost[item2.m_location] = item2.m_cost;
                                tempAreaQueue.Insert(item2);
                            }
                        }
                    }
                    if (item.m_location.m_z > 0 && item.m_location.m_z <= item.m_source.m_z) {
                        AreaQueueItem item3;
                        item3.m_location.m_x = item.m_location.m_x;
                        item3.m_location.m_z = (short)(item.m_location.m_z - 1);
                        if (!tempAreaCost.TryGetValue(item3.m_location, out areaCost)) {
                            areaCost = num3 + 1;
                        }
                        if (areaCost != 0) {
                            nameLocation.x = (float)(((double)item3.m_location.m_x - halfGrid + 0.5f) * 38.4000015258789d);
                            nameLocation.z = (float)(((double)item3.m_location.m_z - halfGrid + 0.5f) * 38.4000015258789d);
                            if (instance.GetPark(nameLocation) == park) {
                                item3.m_cost = 0;
                                item3.m_source = item3.m_location;
                            } else {
                                item3.m_cost = (item3.m_location.m_x - item.m_source.m_x) *
                                               (item3.m_location.m_x - item.m_source.m_x) +
                                               (item3.m_location.m_z - item.m_source.m_z) *
                                               (item3.m_location.m_z - item.m_source.m_z);
                                item3.m_source = item.m_source;
                            }
                            if (item3.m_cost < areaCost) {
                                tempAreaCost[item3.m_location] = item3.m_cost;
                                tempAreaQueue.Insert(item3);
                            }
                        }
                    }
                    if (item.m_location.m_x < (RESOURCEGRID_RESOLUTION - 1) && item.m_location.m_x >= item.m_source.m_x) {
                        AreaQueueItem item4;
                        item4.m_location.m_x = (short)(item.m_location.m_x + 1);
                        item4.m_location.m_z = item.m_location.m_z;
                        if (!tempAreaCost.TryGetValue(item4.m_location, out areaCost)) {
                            areaCost = num3 + 1;
                        }
                        if (areaCost != 0) {
                            nameLocation.x = (float)(((double)item4.m_location.m_x - halfGrid + 0.5f) * 38.4000015258789d);
                            nameLocation.z = (float)(((double)item4.m_location.m_z - halfGrid + 0.5f) * 38.4000015258789d);
                            if (instance.GetPark(nameLocation) == park) {
                                item4.m_cost = 0;
                                item4.m_source = item4.m_location;
                            } else {
                                item4.m_cost = (item4.m_location.m_x - item.m_source.m_x) *
                                               (item4.m_location.m_x - item.m_source.m_x) +
                                               (item4.m_location.m_z - item.m_source.m_z) *
                                               (item4.m_location.m_z - item.m_source.m_z);
                                item4.m_source = item.m_source;
                            }
                            if (item4.m_cost < areaCost) {
                                tempAreaCost[item4.m_location] = item4.m_cost;
                                tempAreaQueue.Insert(item4);
                            }
                        }
                    }
                    if (item.m_location.m_z < (RESOURCEGRID_RESOLUTION - 1) && item.m_location.m_z >= item.m_source.m_z) {
                        AreaQueueItem item5;
                        item5.m_location.m_x = item.m_location.m_x;
                        item5.m_location.m_z = (short)(item.m_location.m_z + 1);
                        if (!tempAreaCost.TryGetValue(item5.m_location, out areaCost)) {
                            areaCost = num3 + 1;
                        }
                        if (areaCost != 0) {
                            nameLocation.x = (float)(((double)item5.m_location.m_x - halfGrid + 0.5f) * 38.4000015258789d);
                            nameLocation.z = (float)(((double)item5.m_location.m_z - halfGrid + 0.5f) * 38.4000015258789d);
                            if (instance.GetPark(nameLocation) == park) {
                                item5.m_cost = 0;
                                item5.m_source = item5.m_location;
                            } else {
                                item5.m_cost = (item5.m_location.m_x - item.m_source.m_x) *
                                               (item5.m_location.m_x - item.m_source.m_x) +
                                               (item5.m_location.m_z - item.m_source.m_z) *
                                               (item5.m_location.m_z - item.m_source.m_z);
                                item5.m_source = item.m_source;
                            }
                            if (item5.m_cost < areaCost) {
                                tempAreaCost[item5.m_location] = item5.m_cost;
                                tempAreaQueue.Insert(item5);
                            }
                        }
                    }
                }
            }
            return num4;
        }

        internal static int AddObstructedResource(ushort[] localTempResources, float[] tempSectorSlopes, float[] tempSectorDistances, int[] tempCircleMinX, int[] tempCircleMaxX, ImmaterialResourceManager.Resource resource, int rate, Vector3 position, float radius) {
            const float halfGrid = RESOURCEGRID_RESOLUTION / 2f;
            const int maxSize = RESOURCEGRID_RESOLUTION - 3;
            const double ratio = 38.4000015258789d;
            if (rate == 0) {
                return 0;
            }
            ushort[] finalHeights = Singleton<TerrainManager>.instance.FinalHeights;
            for (int i = 0; i < RESOURCEGRID_RESOLUTION; i++) {
                tempSectorSlopes[i] = -100f;
                tempSectorDistances[i] = 0f;
            }
            float num = radius * 0.5f;
            float num2 = EMath.Max(38.4f, radius + 19.2f);
            int num3 = EMath.Clamp((int)(position.x / ratio + halfGrid), 2, maxSize);
            int num4 = num3;
            int num5 = EMath.Clamp((int)(position.z / ratio + halfGrid), 2, maxSize);
            int num6 = num5;
            double num7 = position.x - ((double)num3 - halfGrid + 0.5f) * ratio;
            double num8 = position.z - ((double)num5 - halfGrid + 0.5f) * ratio;
            if (num7 > 9.60000038146973d) {
                num4 = EMath.Min(num4 + 1, maxSize);
            } else if (num7 < -9.60000038146973d) {
                num3 = EMath.Max(num3 - 1, 2);
            }
            if (num8 > 9.60000038146973d) {
                num6 = EMath.Min(num6 + 1, maxSize);
            } else if (num8 < -9.60000038146973d) {
                num5 = EMath.Max(num5 - 1, 2);
            }
            int num9 = num5;
            int num10 = num6;
            int num11 = num9 + 1;
            int num12 = num10 - 1;
            int num13 = 0;
            bool flag;
            do {
                flag = false;
                float num14 = (float)(ratio * (0.75f + num13++));
                num14 *= num14;
                for (int j = num9; j <= num10; j++) {
                    int num15 = (j > num5) ? j : (num5 - j + num9);
                    float num16 = (float)(((double)num15 - halfGrid + 0.5f) * ratio);
                    float num17 = num16 - position.z;
                    int num18 = EMath.Clamp(EMath.RoundToInt(num16 / 16f + 540f), 0, 1080);
                    int num19 = num3;
                    int num20 = num4;
                    if (num15 >= num11 && num15 <= num12) {
                        num19 = EMath.Min(num19, tempCircleMinX[num15] - 1);
                        num20 = EMath.Max(num20, tempCircleMaxX[num15] + 1);
                    }
                    int k = num19;
                    while (k >= 2) {
                        float num21 = (float)(((double)k - halfGrid + 0.5f) * ratio);
                        float num22 = num21 - position.x;
                        int num23 = EMath.Clamp(EMath.RoundToInt(num21 / 16f + 540f), 0, 1080);
                        float num24 = num17 * num17 + num22 * num22;
                        if ((num19 == k || num24 < num14) && num24 < num2 * num2) {
                            float num25 = (float)Math.Sqrt(num24);
                            float num26 = rate;
                            ushort num27 = finalHeights[num18 * 1081 + num23];
                            float num28 = 0.015625f * num27;
                            float num29 = (num28 - position.y) / EMath.Max(1f, num25);
                            float num30 = EMath.Atan2(num17, num22) * 40.7436638f;
                            float num31 = EMath.Min((float)(ratio / EMath.Max(1f, num25) * 20.3718318939209d), 64f); //TODO: 64: halfgrid/2 ??
                            int num32 = EMath.RoundToInt(num30 - num31) & 0xff;
                            int num33 = EMath.RoundToInt(num30 + num31) & 0xff;
                            float num34 = 0f;
                            float num35 = 0f;
                            int num36 = num32;
                            while (true) {
                                float num37 = tempSectorSlopes[num36];
                                float num38 = tempSectorDistances[num36];
                                float num39 = EMath.Clamp(num25 - num38, 1f, 38.4f);
                                num34 += (num37 - num29) * num39;
                                num35 += num39;
                                if (num29 > num37) {
                                    tempSectorSlopes[num36] = num29;
                                    tempSectorDistances[num36] = num25;
                                }
                                if (num36 == num33) {
                                    break;
                                }
                                num36 = (num36 + 1 & 0xff);
                            }
                            num34 /= EMath.Max(1f, num35);
                            num26 *= 1.5f / EMath.Max(1f, num34 * 20f + 2.625f) - 0.5f;
                            if (num26 > 0f) {
                                if (num25 > num) {
                                    num26 *= EMath.Clamp01((num2 - num25) / (num2 - num));
                                }
                                int num40 = (int)((num15 * RESOURCEGRID_RESOLUTION + k) * 29 + resource);
                                AddResource(ref localTempResources[num40], EMath.RoundToInt(num26));
                            }
                            flag = true;
                        }
                        if (num24 >= num14) {
                            break;
                        }
                        num19 = k--;
                    }
                    k = num20;
                    while (k <= RESOURCEGRID_RESOLUTION - 3) {
                        float num41 = (float)((k - halfGrid + 0.5f) * ratio);
                        float num42 = num41 - position.x;
                        int num43 = EMath.Clamp(EMath.RoundToInt(num41 / 16f + 540f), 0, 1080);
                        float num44 = num17 * num17 + num42 * num42;
                        if ((num20 == k || num44 < num14) && k != num3 && num44 < num2 * num2) {
                            float num45 = (float)Math.Sqrt(num44);
                            float num46 = rate;
                            ushort num47 = finalHeights[num18 * 1081 + num43];
                            float num48 = 0.015625f * num47;
                            float num49 = (num48 - position.y) / EMath.Max(1f, num45);
                            float num50 = (float)Math.Atan2(num17, num42) * 40.7436638f;
                            float num51 = EMath.Min((float)(ratio / EMath.Max(1f, num45) * 20.3718318939209d), 64f);
                            int num52 = EMath.RoundToInt(num50 - num51) & 0xff;
                            int num53 = EMath.RoundToInt(num50 + num51) & 0xff;
                            float num54 = 0f;
                            float num55 = 0f;
                            int num56 = num52;
                            while (true) {
                                float num57 = tempSectorSlopes[num56];
                                float num58 = tempSectorDistances[num56];
                                float num59 = EMath.Clamp(num45 - num58, 1f, 38.4f);
                                num54 += (num57 - num49) * num59;
                                num55 += num59;
                                if (num49 > num57) {
                                    tempSectorSlopes[num56] = num49;
                                    tempSectorDistances[num56] = num45;
                                }
                                if (num56 == num53) {
                                    break;
                                }
                                num56 = (num56 + 1 & 0xff);
                            }
                            num54 /= EMath.Max(1f, num55);
                            num46 *= 1.5f / EMath.Max(1f, num54 * 20f + 2.625f) - 0.5f;
                            if (num46 > 0f) {
                                if (num45 > num) {
                                    num46 *= EMath.Clamp01((num2 - num45) / (num2 - num));
                                }
                                int num60 = (int)((num15 * RESOURCEGRID_RESOLUTION + k) * 29 + resource);
                                AddResource(ref localTempResources[num60], EMath.RoundToInt(num46));
                            }
                            flag = true;
                        }
                        if (num44 >= num14) {
                            break;
                        }
                        num20 = k++;
                    }
                    tempCircleMinX[num15] = num19;
                    tempCircleMaxX[num15] = num20;
                }
                num11 = num9;
                num12 = num10;
                if (num9 > 2) {
                    num9--;
                }
                if (num10 < RESOURCEGRID_RESOLUTION - 3) {
                    num10++;
                }
            }
            while (flag);
            return rate;
        }

        internal static bool CalculateLocalResources(int x, int z, ushort[] buffer, int[] global, ushort[] target, int index) {
            const float halfGrid = RESOURCEGRID_RESOLUTION / 2f;
            int num = buffer[index] + global[0];
            int num2 = buffer[index + 1] + global[1];
            int num3 = buffer[index + 2] + global[2];
            int num4 = buffer[index + 3] + global[3];
            int num5 = buffer[index + 4] + global[4];
            int num6 = buffer[index + 5] + global[5];
            int num7 = buffer[index + 6] + global[6];
            int num8 = buffer[index + 7] + global[7];
            int num9 = buffer[index + 8] + global[8];
            int num10 = buffer[index + 9] + global[9];
            int num11 = buffer[index + 10] + global[10];
            int num12 = buffer[index + 11] + global[11];
            int num13 = buffer[index + 12] + global[12];
            int num14 = buffer[index + 13] + global[13];
            int num15 = buffer[index + 14] + global[14];
            int num16 = buffer[index + 15];
            int num17 = buffer[index + 16] + global[16];
            int num18 = buffer[index + 17] + global[17];
            int num19 = buffer[index + 18] + global[18];
            int num20 = buffer[index + 19] + global[19];
            int num21 = buffer[index + 20] + global[20];
            int num22 = buffer[index + 21];
            int num23 = buffer[index + 22] + global[22];
            int num24 = buffer[index + 23] + global[23];
            int num25 = buffer[index + 24] + global[24];
            int num26 = buffer[index + 25] + global[25];
            int num27 = buffer[index + 26] + global[26];
            int num28 = buffer[index + 27] + global[27];
            int num29 = buffer[index + 28] + global[28];
            Rect area = new Rect((float)((x - halfGrid - 1.5f) * 38.4000015258789d), (float)((z - halfGrid - 1.5f) * 38.4000015258789d), 153.6f, 153.6f);
            Singleton<NaturalResourceManager>.instance.AveragePollutionAndWaterAndTrees(area, out float groundPollution, out float waterProximity, out float treeProximity);
            int num33 = (int)(groundPollution * 100f);
            int num34 = (int)(waterProximity * 100f);
            int num35 = (int)(treeProximity * 100f);
            if (num34 > 33 && num34 < 99) {
                area = new Rect((float)((x - halfGrid + 0.25f) * 38.4000015258789d), (float)((z - halfGrid + 0.25f) * 38.4000015258789d), 19.2f, 19.2f);
                Singleton<NaturalResourceManager>.instance.AverageWater(area, out waterProximity);
                num34 = EMath.Max(EMath.Min(num34, (int)(waterProximity * 100f)), 33);
            }
            num18 = num18 * 2 / (num2 + 50);
            num9 = (num9 * (100 - num35) + 50) / 100;
            if (num13 == 0) {
                num10 = 0;
                num11 = 50;
                num12 = 50;
            } else {
                num10 /= num13;
                num11 /= num13;
                num12 /= num13;
                num17 += EMath.Min(num13, 10) * 10;
            }
            bool flag = Singleton<GameAreaManager>.instance.PointOutOfArea(VectorUtils.X_Y(area.center));
            flag |= (x <= 1 || x >= RESOURCEGRID_RESOLUTION - 2 || z <= 1 || z >= RESOURCEGRID_RESOLUTION - 2);
            if (flag) {
                num15 = 0;
                num16 = 0;
            } else {
                int num36 = ImmaterialResourceManager.CalculateResourceEffect(num34, 33, 67, 300, 0) * Mathf.Max(0, 32 - num33) >> 5;
                int num37 = ImmaterialResourceManager.CalculateResourceEffect(num35, 10, 100, 0, 30);
                num15 += ImmaterialResourceManager.CalculateResourceEffect(num, 100, 500, 50, 100);
                num15 += ImmaterialResourceManager.CalculateResourceEffect(num3, 100, 500, 50, 100);
                num15 += ImmaterialResourceManager.CalculateResourceEffect(num2, 100, 500, 50, 100);
                num15 += ImmaterialResourceManager.CalculateResourceEffect(num4, 100, 500, 50, 100);
                num15 += ImmaterialResourceManager.CalculateResourceEffect(num5, 100, 500, 50, 100);
                num15 += ImmaterialResourceManager.CalculateResourceEffect(num6, 100, 500, 50, 100);
                num15 += ImmaterialResourceManager.CalculateResourceEffect(num7, 100, 500, 50, 100);
                num15 += ImmaterialResourceManager.CalculateResourceEffect(num8, 100, 500, 50, 100);
                num15 += ImmaterialResourceManager.CalculateResourceEffect(num20, 100, 500, 50, 100);
                num15 += ImmaterialResourceManager.CalculateResourceEffect(num14, 100, 500, 100, 200);
                num15 += ImmaterialResourceManager.CalculateResourceEffect(num12, 60, 100, 0, 50);
                num15 += ImmaterialResourceManager.CalculateResourceEffect(num11, 60, 100, 0, 50);
                num15 += ImmaterialResourceManager.CalculateResourceEffect(num21, 50, 100, 20, 25);
                num15 += ImmaterialResourceManager.CalculateResourceEffect(num24, 50, 100, 20, 25);
                num15 += ImmaterialResourceManager.CalculateResourceEffect(num22, 100, 1000, 0, 25);
                num15 += ImmaterialResourceManager.CalculateResourceEffect(num26, 100, 200, 20, 30);
                num15 += ImmaterialResourceManager.CalculateResourceEffect(num27, 100, 500, 50, 200);
                num15 += ImmaterialResourceManager.CalculateResourceEffect(num28, 100, 500, 50, 100);
                num15 += ImmaterialResourceManager.CalculateResourceEffect(num29, 100, 500, 50, 100);
                num15 -= ImmaterialResourceManager.CalculateResourceEffect(100 - num12, 60, 100, 0, 50);
                num15 -= ImmaterialResourceManager.CalculateResourceEffect(100 - num11, 60, 100, 0, 50);
                num15 -= ImmaterialResourceManager.CalculateResourceEffect(num33, 50, 255, 50, 100);
                num15 -= ImmaterialResourceManager.CalculateResourceEffect(num9, 10, 100, 0, 100);
                num15 -= ImmaterialResourceManager.CalculateResourceEffect(num10, 10, 100, 0, 100);
                num15 -= ImmaterialResourceManager.CalculateResourceEffect(num18, 50, 100, 10, 50);
                num15 -= ImmaterialResourceManager.CalculateResourceEffect(num19, 15, 50, 100, 200);
                num15 += num36;
                num15 /= 10;
                num16 += num36 * 25 / 300;
                num16 += num37;
            }
            num = EMath.Clamp(num, 0, 65535);
            num2 = EMath.Clamp(num2, 0, 65535);
            num3 = EMath.Clamp(num3, 0, 65535);
            num4 = EMath.Clamp(num4, 0, 65535);
            num5 = EMath.Clamp(num5, 0, 65535);
            num6 = EMath.Clamp(num6, 0, 65535);
            num7 = EMath.Clamp(num7, 0, 65535);
            num8 = EMath.Clamp(num8, 0, 65535);
            num9 = EMath.Clamp(num9, 0, 65535);
            num10 = EMath.Clamp(num10, 0, 65535);
            num11 = EMath.Clamp(num11, 0, 65535);
            num12 = EMath.Clamp(num12, 0, 65535);
            num13 = EMath.Clamp(num13, 0, 65535);
            num14 = EMath.Clamp(num14, 0, 65535);
            num15 = EMath.Clamp(num15, 0, 65535);
            num16 = EMath.Clamp(num16, 0, 65535);
            num17 = EMath.Clamp(num17, 0, 65535);
            num18 = EMath.Clamp(num18, 0, 65535);
            num19 = EMath.Clamp(num19, 0, 65535);
            num20 = EMath.Clamp(num20, 0, 65535);
            num21 = EMath.Clamp(num21, 0, 65535);
            num22 = EMath.Clamp(num22, 0, 65535);
            num23 = EMath.Clamp(num23, 0, 65535);
            num24 = EMath.Clamp(num24, 0, 65535);
            num25 = EMath.Clamp(num25, 0, 65535);
            num26 = EMath.Clamp(num26, 0, 65535);
            num27 = EMath.Clamp(num27, 0, 65535);
            num28 = EMath.Clamp(num28, 0, 65535);
            num29 = EMath.Clamp(num29, 0, 65535);
            DistrictManager instance = Singleton<DistrictManager>.instance;
            byte district = instance.GetDistrict(x * (EDistrictManager.DISTRICTGRID_RESOLUTION / RESOURCEGRID_RESOLUTION),
                                                 z * (EDistrictManager.DISTRICTGRID_RESOLUTION / RESOURCEGRID_RESOLUTION));
            instance.m_districts.m_buffer[district].AddGroundData(num15, num33, num17);
            bool result = false;
            if (num != target[index]) {
                target[index] = (ushort)num;
                result = true;
            }
            if (num2 != target[index + 1]) {
                target[index + 1] = (ushort)num2;
                result = true;
            }
            if (num3 != target[index + 2]) {
                target[index + 2] = (ushort)num3;
                result = true;
            }
            if (num4 != target[index + 3]) {
                target[index + 3] = (ushort)num4;
                result = true;
            }
            if (num5 != target[index + 4]) {
                target[index + 4] = (ushort)num5;
                result = true;
            }
            if (num6 != target[index + 5]) {
                target[index + 5] = (ushort)num6;
                result = true;
            }
            if (num7 != target[index + 6]) {
                target[index + 6] = (ushort)num7;
                result = true;
            }
            if (num26 != target[index + 25]) {
                target[index + 25] = (ushort)num26;
                result = true;
            }
            if (num8 != target[index + 7]) {
                target[index + 7] = (ushort)num8;
                result = true;
            }
            if (num9 != target[index + 8]) {
                target[index + 8] = (ushort)num9;
                result = true;
            }
            if (num10 != target[index + 9]) {
                target[index + 9] = (ushort)num10;
                result = true;
            }
            if (num11 != target[index + 10]) {
                target[index + 10] = (ushort)num11;
                result = true;
            }
            if (num12 != target[index + 11]) {
                target[index + 11] = (ushort)num12;
                result = true;
            }
            if (num13 != target[index + 12]) {
                target[index + 12] = (ushort)num13;
                result = true;
            }
            if (num14 != target[index + 13]) {
                target[index + 13] = (ushort)num14;
                result = true;
            }
            if (num15 != target[index + 14]) {
                target[index + 14] = (ushort)num15;
                result = true;
            }
            if (num16 != target[index + 15]) {
                target[index + 15] = (ushort)num16;
                result = true;
            }
            if (num17 != target[index + 16]) {
                target[index + 16] = (ushort)num17;
                result = true;
            }
            if (num18 != target[index + 17]) {
                target[index + 17] = (ushort)num18;
                result = true;
            }
            if (num19 != target[index + 18]) {
                target[index + 18] = (ushort)num19;
                result = true;
            }
            if (num20 != target[index + 19]) {
                target[index + 19] = (ushort)num20;
                result = true;
            }
            if (num21 != target[index + 20]) {
                target[index + 20] = (ushort)num21;
                result = true;
            }
            if (num22 != target[index + 21]) {
                target[index + 21] = (ushort)num22;
                result = true;
            }
            if (num23 != target[index + 22]) {
                target[index + 22] = (ushort)num23;
                result = true;
            }
            if (num24 != target[index + 23]) {
                target[index + 23] = (ushort)num24;
                result = true;
            }
            if (num25 != target[index + 24]) {
                target[index + 24] = (ushort)num25;
                result = true;
            }
            if (num27 != target[index + 26]) {
                target[index + 26] = (ushort)num27;
                result = true;
            }
            if (num28 != target[index + 27]) {
                target[index + 27] = (ushort)num28;
                result = true;
            }
            if (num29 != target[index + 28]) {
                target[index + 28] = (ushort)num29;
                result = true;
            }
            return result;
        }

        private static void CalculateTotalResources(int[] buffer, long[] bufferMul, int[] target) {
            int num, num2, num3, num4, num5, num6, num7, num8, num9, num10, num11, num12, num13, num14, num15;
            int num16 = buffer[15];
            int num17 = buffer[16];
            int num18, num19, num20, num21;
            int num22 = buffer[21];
            int num23 = buffer[22];
            int num24, num25, num26, num27, num28, num29;
            long num30 = bufferMul[0];
            long num31 = bufferMul[1];
            long num32 = bufferMul[2];
            long num33 = bufferMul[3];
            long num34 = bufferMul[4];
            long num35 = bufferMul[5];
            long num36 = bufferMul[6];
            long num37 = bufferMul[7];
            long num38 = bufferMul[8];
            long num39 = bufferMul[9];
            long num40 = bufferMul[10];
            long num41 = bufferMul[11];
            long num42 = bufferMul[12];
            long num43 = bufferMul[13];
            long num44 = bufferMul[14];
            long num45 = bufferMul[15];
            long num46 = bufferMul[17];
            long num47 = bufferMul[18];
            long num48 = bufferMul[19];
            long num49 = bufferMul[20];
            long num50 = bufferMul[23];
            long num51 = bufferMul[24];
            long num52 = bufferMul[25];
            long num53 = bufferMul[26];
            long num54 = bufferMul[27];
            long num55 = bufferMul[28];
            if (num17 != 0) {
                num = (int)(num30 / num17);
                num2 = (int)(num31 / num17);
                num3 = (int)(num32 / num17);
                num4 = (int)(num33 / num17);
                num5 = (int)(num34 / num17);
                num6 = (int)(num35 / num17);
                num7 = (int)(num36 / num17);
                num8 = (int)(num37 / num17);
                num10 = (int)(num39 / num17);
                num11 = (int)(num40 / num17);
                num12 = (int)(num41 / num17);
                num13 = (int)(num42 / num17);
                num14 = (int)(num43 / num17);
                num15 = (int)(num44 / num17);
                num16 = (int)(num45 / num17);
                num9 = (int)(num38 / num17);
                num18 = (int)(num46 / num17);
                num19 = (int)(num47 / num17);
                num20 = (int)(num48 / num17);
                num21 = (int)(num49 / num17);
                num24 = (int)(num50 / num17);
                num25 = (int)(num51 / num17);
                num26 = (int)(num52 / num17);
                num27 = (int)(num53 / num17);
                num28 = (int)(num54 / num17);
                num29 = (int)(num55 / num17);
            } else {
                num = 0;
                num2 = 0;
                num3 = 0;
                num4 = 0;
                num5 = 0;
                num6 = 0;
                num7 = 0;
                num8 = 0;
                num10 = 0;
                num11 = 50;
                num12 = 50;
                num13 = 0;
                num14 = 0;
                num15 = 0;
                num9 = 0;
                num18 = 0;
                num19 = 0;
                num20 = 0;
                num21 = 0;
                num24 = 0;
                num25 = 0;
                num26 = 0;
                num27 = 0;
                num28 = 0;
                num29 = 0;
            }
            num = EMath.Clamp(num, 0, 2147483647);
            num2 = EMath.Clamp(num2, 0, 2147483647);
            num3 = EMath.Clamp(num3, 0, 2147483647);
            num4 = EMath.Clamp(num4, 0, 2147483647);
            num5 = EMath.Clamp(num5, 0, 2147483647);
            num6 = EMath.Clamp(num6, 0, 2147483647);
            num7 = EMath.Clamp(num7, 0, 2147483647);
            num8 = EMath.Clamp(num8, 0, 2147483647);
            num9 = EMath.Clamp(num9, 0, 2147483647);
            num10 = EMath.Clamp(num10, 0, 2147483647);
            num11 = EMath.Clamp(num11, 0, 2147483647);
            num12 = EMath.Clamp(num12, 0, 2147483647);
            num13 = EMath.Clamp(num13, 0, 2147483647);
            num14 = EMath.Clamp(num14, 0, 2147483647);
            num15 = EMath.Clamp(num15, 0, 2147483647);
            num16 = EMath.Clamp(num16, 0, 2147483647);
            num17 = EMath.Clamp(num17, 0, 2147483647);
            num18 = EMath.Clamp(num18, 0, 2147483647);
            num19 = EMath.Clamp(num19, 0, 2147483647);
            num20 = EMath.Clamp(num20, 0, 2147483647);
            num21 = EMath.Clamp(num21, 0, 2147483647);
            num22 = EMath.Clamp(num22, 0, 2147483647);
            num23 = EMath.Clamp(num23, 0, 2147483647);
            num24 = EMath.Clamp(num24, 0, 2147483647);
            num25 = EMath.Clamp(num25, 0, 2147483647);
            num26 = EMath.Clamp(num26, 0, 2147483647);
            num27 = EMath.Clamp(num27, 0, 2147483647);
            num28 = EMath.Clamp(num28, 0, 2147483647);
            num29 = EMath.Clamp(num29, 0, 2147483647);
            target[0] = num;
            target[2] = num3;
            target[1] = num2;
            target[3] = num4;
            target[4] = num5;
            target[5] = num6;
            target[6] = num7;
            target[7] = num8;
            target[8] = num9;
            target[9] = num10;
            target[10] = num11;
            target[11] = num12;
            target[12] = num13;
            target[13] = num14;
            target[14] = num15;
            target[15] = num16;
            target[16] = num17;
            target[17] = num18;
            target[18] = num19;
            target[19] = num20;
            target[20] = num21;
            target[21] = num22;
            target[22] = num23;
            target[23] = num24;
            target[24] = num25;
            target[25] = num26;
            target[26] = num27;
            target[27] = num28;
            target[28] = num29;
        }

        internal static void CheckLocalResource(ushort[] localFinalResources, ImmaterialResourceManager.Resource resource, Vector3 position, float radius, out int local) {
            const float halfGrid = RESOURCEGRID_RESOLUTION / 2f;
            const double ratio = 38.4000015258789d;
            float num = EMath.Max(0f, EMath.Min(radius, 19.2f));
            float num2 = EMath.Max(38.4f, radius + 19.2f);
            int num3 = EMath.Max((int)((position.x - radius) / ratio + halfGrid), 2);
            int num4 = EMath.Max((int)((position.z - radius) / ratio + halfGrid), 2);
            int num5 = EMath.Min((int)((position.x + radius) / ratio + halfGrid), RESOURCEGRID_RESOLUTION - 3);
            int num6 = EMath.Min((int)((position.z + radius) / ratio + halfGrid), RESOURCEGRID_RESOLUTION - 3);
            float num7 = 0f;
            float num8 = 0f;
            for (int i = num4; i <= num6; i++) {
                float num9 = (float)((i - halfGrid + 0.5f) * ratio - position.z);
                for (int j = num3; j <= num5; j++) {
                    float num10 = (float)((j - halfGrid + 0.5f) * ratio - position.x);
                    float num11 = num9 * num9 + num10 * num10;
                    if (num11 < num2 * num2) {
                        int num12 = (int)((i * RESOURCEGRID_RESOLUTION + j) * 29 + resource);
                        int num13 = localFinalResources[num12];
                        if (num11 > num * num) {
                            float num14 = EMath.Clamp01((num2 - (float)Math.Sqrt(num11)) / (num2 - num));
                            num7 += num13 * num14;
                            num8 += num14 * num14;
                        } else {
                            num7 += num13;
                            num8 += 1f;
                        }
                    }
                }
            }
            if (num8 != 0f) {
                num7 /= num8;
            }
            local = EMath.RoundToInt(num7);
        }

        internal static void SimulationStepImpl(ImmaterialResourceManager irmInstance, ushort[] localTempResources, ushort[] localFinalResources,
                                                int[] globalTempResources, int[] globalFinalResources, int[] totalTempResources,
                                                int[] totalFinalResources, long[] totalTempResourcesMul, int subStep) {
            if (subStep != 0 && subStep != 1000) {
                uint currentFrameIndex = Singleton<SimulationManager>.instance.m_currentFrameIndex;
                int frameIndex = (int)(currentFrameIndex & 255u);
                int frameStart = frameIndex * 2; // frameIndex * 256 >> 8;
                int minX = -1; // frameIndex * 256 * 256 >> 8 & 255;
                int maxX = -1; // ((frameIndex + 1) * 256 * 256 >> 8) - 1 & 255;
                int minZ = -1;
                int maxZ = -1;
                for (int z = frameStart; z <= frameStart + 1; z++) {
                    if (z < RESOURCEGRID_RESOLUTION) {
                        for (int x = 0; x <= RESOURCEGRID_RESOLUTION - 1; x++) {
                            int index = (z * RESOURCEGRID_RESOLUTION + x) * 29;
                            if (CalculateLocalResources(x, z, localTempResources, globalFinalResources, localFinalResources, index)) {
                                minX = minX == -1 ? x : EMath.Min(minX, x);
                                maxX = EMath.Max(maxX, x);
                                minZ = minZ == -1 ? z : EMath.Min(minZ, z);
                                maxZ = EMath.Max(maxZ, z);
                            }
                            int num5 = localFinalResources[index + 16];
                            for (int i = 0; i < 29; i++) {
                                int num6 = localFinalResources[index + i];
                                totalTempResources[i] += num6;
                                totalTempResourcesMul[i] += (num6 * num5);
                                localTempResources[index + i] = 0;
                            }
                        }
                    }
                }
                if (frameIndex == 255) {
                    CalculateTotalResources(totalTempResources, totalTempResourcesMul, totalFinalResources);
                    StatisticsManager instance = Singleton<StatisticsManager>.instance;
                    StatisticBase statisticBase = instance.Acquire<StatisticArray>(StatisticType.ImmaterialResource);
                    for (int k = 0; k < 29; k++) {
                        globalFinalResources[k] = globalTempResources[k];
                        globalTempResources[k] = 0;
                        totalTempResources[k] = 0;
                        totalTempResourcesMul[k] = 0L;
                        statisticBase.Acquire<StatisticInt32>(k, 29).Set(totalFinalResources[k]);
                    }
                }
                if (minX != -1) {
                    irmInstance.AreaModified(minX, minZ, maxX, maxZ);
                }
            }
        }

        internal static void EnsureCapacity(ImmaterialResourceManager irmInstance, ref Texture2D resourceTexture, ref int[] modifiedX1, ref int[] modifiedX2, ref int[] tempCircleMinX,
                                            ref int[] tempCircleMaxX, ref int[] tempSectorSlopes, ref int[] tempSectorDistances) {
            //EUtils.ELog($"modifiedX1 Len: {modifiedX1.Length} modifiedX2 Len: {modifiedX2.Length}");

            if (modifiedX1.Length != RESOURCEGRID_RESOLUTION && modifiedX2.Length != RESOURCEGRID_RESOLUTION) {
                modifiedX1 = new int[RESOURCEGRID_RESOLUTION];
                modifiedX2 = new int[RESOURCEGRID_RESOLUTION];
                tempCircleMinX = new int[RESOURCEGRID_RESOLUTION];
                tempCircleMaxX = new int[RESOURCEGRID_RESOLUTION];
                tempSectorSlopes = new int[RESOURCEGRID_RESOLUTION];
                tempSectorDistances = new int[RESOURCEGRID_RESOLUTION];
                resourceTexture = new Texture2D(RESOURCEGRID_RESOLUTION, RESOURCEGRID_RESOLUTION, TextureFormat.Alpha8, false, true) {
                    wrapMode = TextureWrapMode.Clamp
                };
                Shader.SetGlobalTexture("_ImmaterialResources", resourceTexture);
                UpdateResourceMapping(irmInstance);
                for (int i = 0; i < RESOURCEGRID_RESOLUTION; i++) {
                    modifiedX1[i] = 0;
                    modifiedX2[i] = RESOURCEGRID_RESOLUTION - 1;
                }
            }
            if (m_tempAreaQueue is null) {
                m_tempAreaQueue = new MinHeap<AreaQueueItem>(new AreaQueueItemComparer());
            }
            if (m_tempAreaCost is null) {
                m_tempAreaCost = new Dictionary<CellLocation, int>();
            }
        }

        internal static void IntegratedDeserialize(ImmaterialResourceManager _, ref ushort[] localFinalResources, ref ushort[] localTempResources) {
            int i;
            const int len = DEFAULTGRID_RESOLUTION * DEFAULTGRID_RESOLUTION * RESOURCE_COUNT;
            ushort[] newFinalResources = new ushort[RESOURCEGRID_RESOLUTION * RESOURCEGRID_RESOLUTION * RESOURCE_COUNT];
            ushort[] newTempResources = new ushort[RESOURCEGRID_RESOLUTION * RESOURCEGRID_RESOLUTION * RESOURCE_COUNT];
            for (i = 0; i < len; i++) {
                newFinalResources[i] = localFinalResources[i];
            }
            for (i = 0; i < len; i++) {
                newTempResources[i] = localTempResources[i];
            }
            localFinalResources = newFinalResources;
            localTempResources = newTempResources;
        }

        internal static ushort[] PrepareResourcesForSerialize(ushort[] localResources) {
            const int len = DEFAULTGRID_RESOLUTION * DEFAULTGRID_RESOLUTION * RESOURCE_COUNT;
            ushort[] oldResources = new ushort[len];
            for (int i = 0; i < len; i++) {
                oldResources[i] = localResources[i];
            }
            return oldResources;
        }
    }
}
