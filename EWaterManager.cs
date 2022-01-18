using ColossalFramework;
using ColossalFramework.Math;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace EManagersLib {
    internal static class EWaterManager {
        internal const float WATERGRID_CELL_SIZE = 38.25f;
        internal const int DEFAULTGRID_RESOLUTION = 256;
        internal const int WATERGRID_RESOLUTION = 462;
        internal const int MAX_PULSE_GROUPS = 1024;
        internal struct PulseUnit {
            public ushort m_group;
            public ushort m_node;
            public ushort m_x;
            public ushort m_z;
        }
        private static PulseUnit[] m_waterPulseUnits;
        private static PulseUnit[] m_sewagePulseUnits;
        private static PulseUnit[] m_heatingPulseUnits;

        internal static void CheckHeating(WaterManager.Cell[] waterGrid, Vector3 pos, out bool heating) {
            const float halfGrid = WATERGRID_RESOLUTION / 2f;
            int num = EMath.Clamp((int)(pos.x / WATERGRID_CELL_SIZE + halfGrid), 0, WATERGRID_RESOLUTION - 1);
            int num2 = EMath.Clamp((int)(pos.z / WATERGRID_CELL_SIZE + halfGrid), 0, WATERGRID_RESOLUTION - 1);
            if (CheckHeatingImpl(waterGrid, pos, num, num2, out heating)) {
                return;
            }
            if (waterGrid[num2 * WATERGRID_RESOLUTION + num].m_conductivity2 == 0) {
                return;
            }
            float num3 = (num + 0.5f - halfGrid) * WATERGRID_CELL_SIZE;
            float num4 = (num2 + 0.5f - halfGrid) * WATERGRID_CELL_SIZE;
            if (pos.z > num4 && num2 < WATERGRID_RESOLUTION - 1) {
                if (CheckHeatingImpl(waterGrid, pos, num, num2 + 1, out heating)) {
                    return;
                }
            } else if (pos.z < num4 && num2 > 0 && CheckHeatingImpl(waterGrid, pos, num, num2 - 1, out heating)) {
                return;
            }
            if (pos.x > num3 && num < WATERGRID_RESOLUTION - 1) {
                if (CheckHeatingImpl(waterGrid, pos, num + 1, num2, out heating)) {
                    return;
                }
                if (pos.z > num4 && num2 < WATERGRID_RESOLUTION - 1) {
                    if (CheckHeatingImpl(waterGrid, pos, num + 1, num2 + 1, out heating)) {
                        return;
                    }
                } else if (pos.z < num4 && num2 > 0 && CheckHeatingImpl(waterGrid, pos, num + 1, num2 - 1, out heating)) {
                    return;
                }
            } else if (pos.x < num3 && num > 0) {
                if (CheckHeatingImpl(waterGrid, pos, num - 1, num2, out heating)) {
                    return;
                }
                if (pos.z > num4 && num2 < WATERGRID_RESOLUTION - 1) {
                    if (CheckHeatingImpl(waterGrid, pos, num - 1, num2 + 1, out heating)) {
                        return;
                    }
                } else if (pos.z < num4 && num2 > 0 && CheckHeatingImpl(waterGrid, pos, num - 1, num2 - 1, out heating)) {
                    return;
                }
            }
        }

        private static bool CheckHeatingImpl(WaterManager.Cell[] waterGrid, Vector3 pos, int x, int z, out bool heating) {
            int num = z * WATERGRID_RESOLUTION + x;
            ref WaterManager.Cell cell = ref waterGrid[num];
            if (cell.m_hasHeating) {
                NetManager instance = Singleton<NetManager>.instance;
                ushort closestPipeSegment = cell.m_closestPipeSegment2;
                ushort startNode = instance.m_segments.m_buffer[closestPipeSegment].m_startNode;
                ushort endNode = instance.m_segments.m_buffer[closestPipeSegment].m_endNode;
                NetNode.Flags flags = instance.m_nodes.m_buffer[startNode].m_flags;
                NetNode.Flags flags2 = instance.m_nodes.m_buffer[endNode].m_flags;
                if ((flags & flags2 & NetNode.Flags.Heating) != NetNode.Flags.None) {
                    Segment2 segment;
                    segment.a = VectorUtils.XZ(instance.m_nodes.m_buffer[startNode].m_position);
                    segment.b = VectorUtils.XZ(instance.m_nodes.m_buffer[endNode].m_position);
                    if (segment.DistanceSqr(VectorUtils.XZ(pos), out float _) < 9025.0) {
                        heating = true;
                        return true;
                    }
                }
            }
            heating = false;
            return false;
        }

        internal static void CheckWater(WaterManager.Cell[] waterGrid, Vector3 pos, out bool water, out bool sewage, out byte waterPollution) {
            const float halfGrid = WATERGRID_RESOLUTION / 2f;
            int num = EMath.Clamp((int)(pos.x / WATERGRID_CELL_SIZE + halfGrid), 0, WATERGRID_RESOLUTION - 1);
            int num2 = EMath.Clamp((int)(pos.z / WATERGRID_CELL_SIZE + halfGrid), 0, WATERGRID_RESOLUTION - 1);
            if (CheckWaterImpl(waterGrid, pos, num, num2, out water, out sewage, out waterPollution)) {
                return;
            }
            if (waterGrid[num2 * WATERGRID_RESOLUTION + num].m_conductivity == 0) {
                return;
            }
            float num3 = (num + 0.5f - halfGrid) * WATERGRID_CELL_SIZE;
            float num4 = (num2 + 0.5f - halfGrid) * WATERGRID_CELL_SIZE;
            if (pos.z > num4 && num2 < WATERGRID_RESOLUTION - 1) {
                if (CheckWaterImpl(waterGrid, pos, num, num2 + 1, out water, out sewage, out waterPollution)) {
                    return;
                }
            } else if (pos.z < num4 && num2 > 0 && CheckWaterImpl(waterGrid, pos, num, num2 - 1, out water, out sewage, out waterPollution)) {
                return;
            }
            if (pos.x > num3 && num < WATERGRID_RESOLUTION - 1) {
                if (CheckWaterImpl(waterGrid, pos, num + 1, num2, out water, out sewage, out waterPollution)) {
                    return;
                }
                if (pos.z > num4 && num2 < WATERGRID_RESOLUTION - 1) {
                    if (CheckWaterImpl(waterGrid, pos, num + 1, num2 + 1, out water, out sewage, out waterPollution)) {
                        return;
                    }
                } else if (pos.z < num4 && num2 > 0 && CheckWaterImpl(waterGrid, pos, num + 1, num2 - 1, out water, out sewage, out waterPollution)) {
                    return;
                }
            } else if (pos.x < num3 && num > 0) {
                if (CheckWaterImpl(waterGrid, pos, num - 1, num2, out water, out sewage, out waterPollution)) {
                    return;
                }
                if (pos.z > num4 && num2 < WATERGRID_RESOLUTION - 1) {
                    if (CheckWaterImpl(waterGrid, pos, num - 1, num2 + 1, out water, out sewage, out waterPollution)) {
                        return;
                    }
                } else if (pos.z < num4 && num2 > 0 && CheckWaterImpl(waterGrid, pos, num - 1, num2 - 1, out water, out sewage, out waterPollution)) {
                    return;
                }
            }
        }

        private static bool CheckWaterImpl(WaterManager.Cell[] waterGrid, Vector3 pos, int x, int z, out bool water, out bool sewage, out byte waterPollution) {
            int num = z * WATERGRID_RESOLUTION + x;
            ref WaterManager.Cell cell = ref waterGrid[num];
            if (cell.m_hasWater || cell.m_hasSewage) {
                NetManager instance = Singleton<NetManager>.instance;
                ushort closestPipeSegment = cell.m_closestPipeSegment;
                ushort startNode = instance.m_segments.m_buffer[closestPipeSegment].m_startNode;
                ushort endNode = instance.m_segments.m_buffer[closestPipeSegment].m_endNode;
                NetNode.Flags flags = instance.m_nodes.m_buffer[startNode].m_flags;
                NetNode.Flags flags2 = instance.m_nodes.m_buffer[endNode].m_flags;
                if ((flags & flags2 & (NetNode.Flags.Water | NetNode.Flags.Sewage)) != NetNode.Flags.None) {
                    Segment2 segment;
                    segment.a = VectorUtils.XZ(instance.m_nodes.m_buffer[startNode].m_position);
                    segment.b = VectorUtils.XZ(instance.m_nodes.m_buffer[endNode].m_position);
                    if (segment.DistanceSqr(VectorUtils.XZ(pos), out float _) < 9025.0) {
                        water = (cell.m_hasWater && (flags & flags2 & NetNode.Flags.Water) != NetNode.Flags.None);
                        sewage = (cell.m_hasSewage && (flags & flags2 & NetNode.Flags.Sewage) != NetNode.Flags.None);
                        if (water) {
                            waterPollution = waterGrid[num].m_pollution;
                        } else {
                            waterPollution = 0;
                        }
                        return true;
                    }
                }
            }
            water = false;
            sewage = false;
            waterPollution = 0;
            return false;
        }

        internal static void ConductHeatingToCell(ref int heatingPulseUnitEnd, ref bool canContinue, ref WaterManager.Cell cell, ushort group, int x, int z) {
            if (cell.m_conductivity2 >= 96 && cell.m_heatingPulseGroup == 65535) {
                PulseUnit pulseUnit;
                pulseUnit.m_group = group;
                pulseUnit.m_node = 0;
                pulseUnit.m_x = (ushort)x;
                pulseUnit.m_z = (ushort)z;
                m_heatingPulseUnits[heatingPulseUnitEnd] = pulseUnit;
                if (++heatingPulseUnitEnd == m_heatingPulseUnits.Length) {
                    heatingPulseUnitEnd = 0;
                }
                cell.m_heatingPulseGroup = group;
                canContinue = true;
            }
        }

        internal static void ConductHeatingToCells(WaterManager.Cell[] waterGrid, ref int heatingPulseUnitEnd, ref bool canContinue, ushort group, float worldX, float worldZ, float radius) {
            const float halfGrid = WATERGRID_RESOLUTION / 2f;
            int startX = EMath.Max((int)((worldX - radius) / WATERGRID_CELL_SIZE + halfGrid), 0);
            int startZ = EMath.Max((int)((worldZ - radius) / WATERGRID_CELL_SIZE + halfGrid), 0);
            int endX = EMath.Min((int)((worldX + radius) / WATERGRID_CELL_SIZE + halfGrid), WATERGRID_RESOLUTION - 1);
            int endZ = EMath.Min((int)((worldZ + radius) / WATERGRID_CELL_SIZE + halfGrid), WATERGRID_RESOLUTION - 1);
            float range = radius + 19.125f;
            range *= range;
            for (int i = startZ; i <= endZ; i++) {
                float finalZ = (i + 0.5f - halfGrid) * WATERGRID_CELL_SIZE - worldZ;
                for (int j = startX; j <= endX; j++) {
                    float finalX = (j + 0.5f - halfGrid) * WATERGRID_CELL_SIZE - worldX;
                    if (finalX * finalX + finalZ * finalZ < range) {
                        ConductHeatingToCell(ref heatingPulseUnitEnd, ref canContinue, ref waterGrid[i * WATERGRID_RESOLUTION + j], group, j, i);
                    }
                }
            }
        }

        internal static void ConductHeatingToNode(WaterManager.Node[] nodeData, WaterManager.PulseGroup[] heatingPulseGroups, int heatingPulseGroupCount, ref int heatingPulseUnitEnd, ref bool canContinue, ushort nodeIndex, ref NetNode node, ushort group) {
            NetInfo info = node.Info;
            if (info.m_class.m_service == ItemClass.Service.Water && info.m_class.m_level == ItemClass.Level.Level2) {
                if (nodeData[nodeIndex].m_heatingPulseGroup == 65535) {
                    PulseUnit pulseUnit;
                    pulseUnit.m_group = group;
                    pulseUnit.m_node = nodeIndex;
                    pulseUnit.m_x = 0;
                    pulseUnit.m_z = 0;
                    m_heatingPulseUnits[heatingPulseUnitEnd] = pulseUnit;
                    if (++heatingPulseUnitEnd == m_heatingPulseUnits.Length) {
                        heatingPulseUnitEnd = 0;
                    }
                    nodeData[nodeIndex].m_heatingPulseGroup = group;
                    canContinue = true;
                } else {
                    ushort rootHeatingGroup = GetRootHeatingGroup(heatingPulseGroups, nodeData[nodeIndex].m_heatingPulseGroup);
                    if (rootHeatingGroup != group) {
                        MergeHeatingGroups(heatingPulseGroups, heatingPulseGroupCount, group, rootHeatingGroup);
                        nodeData[nodeIndex].m_heatingPulseGroup = group;
                        canContinue = true;
                    }
                }
            }
        }

        internal static void ConductSewageToCell(ref int sewagePulseUnitEnd, ref bool canContinue, ref WaterManager.Cell cell, ushort group, int x, int z) {
            if (cell.m_conductivity >= 96 && cell.m_sewagePulseGroup == 65535) {
                PulseUnit pulseUnit;
                pulseUnit.m_group = group;
                pulseUnit.m_node = 0;
                pulseUnit.m_x = (ushort)x;
                pulseUnit.m_z = (ushort)z;
                m_sewagePulseUnits[sewagePulseUnitEnd] = pulseUnit;
                if (++sewagePulseUnitEnd == m_sewagePulseUnits.Length) {
                    sewagePulseUnitEnd = 0;
                }
                cell.m_sewagePulseGroup = group;
                canContinue = true;
            }
        }

        internal static void ConductSewageToCells(WaterManager.Cell[] waterGrid, ref int sewagePulseUnitEnd, ref bool canContinue, ushort group, float worldX, float worldZ, float radius) {
            const float halfGrid = WATERGRID_RESOLUTION / 2f;
            int startX = EMath.Max((int)((worldX - radius) / WATERGRID_CELL_SIZE + halfGrid), 0);
            int startZ = EMath.Max((int)((worldZ - radius) / WATERGRID_CELL_SIZE + halfGrid), 0);
            int endX = EMath.Min((int)((worldX + radius) / WATERGRID_CELL_SIZE + halfGrid), WATERGRID_RESOLUTION - 1);
            int endZ = EMath.Min((int)((worldZ + radius) / WATERGRID_CELL_SIZE + halfGrid), WATERGRID_RESOLUTION - 1);
            float range = radius + 19.125f;
            range *= range;
            for (int i = startZ; i <= endZ; i++) {
                float finalZ = (i + 0.5f - halfGrid) * WATERGRID_CELL_SIZE - worldZ;
                for (int j = startX; j <= endX; j++) {
                    float finalX = (j + 0.5f - halfGrid) * WATERGRID_CELL_SIZE - worldX;
                    if (finalX * finalX + finalZ * finalZ < range) {
                        ConductSewageToCell(ref sewagePulseUnitEnd, ref canContinue, ref waterGrid[i * WATERGRID_RESOLUTION + j], group, j, i);
                    }
                }
            }
        }

        internal static void ConductSewageToNode(WaterManager.Node[] nodeData, WaterManager.PulseGroup[] sewagePulseGroups, ref int sewagePulseUnitEnd, ref bool canContinue, int sewagePulseGroupCount, ushort nodeIndex, ref NetNode node, ushort group) {
            NetInfo info = node.Info;
            if (info.m_class.m_service == ItemClass.Service.Water && info.m_class.m_level <= ItemClass.Level.Level2) {
                if (nodeData[nodeIndex].m_sewagePulseGroup == 65535) {
                    PulseUnit pulseUnit;
                    pulseUnit.m_group = group;
                    pulseUnit.m_node = nodeIndex;
                    pulseUnit.m_x = 0;
                    pulseUnit.m_z = 0;
                    m_sewagePulseUnits[sewagePulseUnitEnd] = pulseUnit;
                    if (++sewagePulseUnitEnd == m_sewagePulseUnits.Length) {
                        sewagePulseUnitEnd = 0;
                    }
                    nodeData[nodeIndex].m_sewagePulseGroup = group;
                    canContinue = true;
                } else {
                    ushort rootSewageGroup = GetRootSewageGroup(sewagePulseGroups, nodeData[nodeIndex].m_sewagePulseGroup);
                    if (rootSewageGroup != group) {
                        MergeSewageGroups(sewagePulseGroups, sewagePulseGroupCount, group, rootSewageGroup);
                        if (sewagePulseGroups[rootSewageGroup].m_origPressure == 0u) {
                            PulseUnit pulseUnit2;
                            pulseUnit2.m_group = group;
                            pulseUnit2.m_node = nodeIndex;
                            pulseUnit2.m_x = 0;
                            pulseUnit2.m_z = 0;
                            m_sewagePulseUnits[sewagePulseUnitEnd] = pulseUnit2;
                            if (++sewagePulseUnitEnd == m_sewagePulseUnits.Length) {
                                sewagePulseUnitEnd = 0;
                            }
                        }
                        nodeData[nodeIndex].m_sewagePulseGroup = group;
                        canContinue = true;
                    }
                }
            }
        }

        internal unsafe static void ConductWaterToCell(ref int waterPulseUnitEnd, ref bool canContinue, ref WaterManager.Cell cell, ushort group, int x, int z) {
            if (cell.m_conductivity >= 96 && cell.m_waterPulseGroup == 65535) {
                fixed (PulseUnit* pulseUnit = &m_waterPulseUnits[waterPulseUnitEnd]) {
                    pulseUnit->m_group = group;
                    pulseUnit->m_node = 0;
                    pulseUnit->m_x = (ushort)x;
                    pulseUnit->m_z = (ushort)z;
                }
                if (++waterPulseUnitEnd == m_waterPulseUnits.Length) {
                    waterPulseUnitEnd = 0;
                }
                cell.m_waterPulseGroup = group;
                canContinue = true;
            }
        }

        internal static void ConductWaterToCells(WaterManager.Cell[] waterGrid, ref int waterPulseUnitEnd, ref bool canContinue, ushort group, float worldX, float worldZ, float radius) {
            const float halfGrid = WATERGRID_RESOLUTION / 2f;
            int startX = EMath.Max((int)((worldX - radius) / WATERGRID_CELL_SIZE + halfGrid), 0);
            int startZ = EMath.Max((int)((worldZ - radius) / WATERGRID_CELL_SIZE + halfGrid), 0);
            int endX = EMath.Min((int)((worldX + radius) / WATERGRID_CELL_SIZE + halfGrid), WATERGRID_RESOLUTION - 1);
            int endZ = EMath.Min((int)((worldZ + radius) / WATERGRID_CELL_SIZE + halfGrid), WATERGRID_RESOLUTION - 1);
            float range = radius + 19.125f;
            range *= range;
            for (int i = startZ; i <= endZ; i++) {
                float finalZ = (i + 0.5f - halfGrid) * WATERGRID_CELL_SIZE - worldZ;
                for (int j = startX; j <= endX; j++) {
                    float finalX = (j + 0.5f - halfGrid) * WATERGRID_CELL_SIZE - worldX;
                    if (finalX * finalX + finalZ * finalZ < range) {
                        ConductWaterToCell(ref waterPulseUnitEnd, ref canContinue, ref waterGrid[i * WATERGRID_RESOLUTION + j], group, j, i);
                    }
                }
            }
        }

        internal static void ConductWaterToNode(WaterManager.Node[] nodeData, WaterManager.PulseGroup[] waterPulseGroups,
                    ref int waterPulseUnitEnd, ref bool canContinue, int waterPulseGroupCount, ushort nodeIndex, ref NetNode node, ushort group) {
            NetInfo info = node.Info;
            if (info.m_class.m_service == ItemClass.Service.Water && info.m_class.m_level <= ItemClass.Level.Level2) {
                if (nodeData[nodeIndex].m_waterPulseGroup == 65535) {
                    PulseUnit pulseUnit;
                    pulseUnit.m_group = group;
                    pulseUnit.m_node = nodeIndex;
                    pulseUnit.m_x = 0;
                    pulseUnit.m_z = 0;
                    m_waterPulseUnits[waterPulseUnitEnd] = pulseUnit;
                    if (++waterPulseUnitEnd == m_waterPulseUnits.Length) {
                        waterPulseUnitEnd = 0;
                    }
                    nodeData[nodeIndex].m_waterPulseGroup = group;
                    canContinue = true;
                } else {
                    ushort rootWaterGroup = GetRootWaterGroup(waterPulseGroups, nodeData[nodeIndex].m_waterPulseGroup);
                    if (rootWaterGroup != group) {
                        MergeWaterGroups(nodeData, waterPulseGroups, waterPulseGroupCount, group, rootWaterGroup);
                        if (waterPulseGroups[rootWaterGroup].m_origPressure == 0u) {
                            PulseUnit pulseUnit2;
                            pulseUnit2.m_group = group;
                            pulseUnit2.m_node = nodeIndex;
                            pulseUnit2.m_x = 0;
                            pulseUnit2.m_z = 0;
                            m_waterPulseUnits[waterPulseUnitEnd] = pulseUnit2;
                            if (++waterPulseUnitEnd == m_waterPulseUnits.Length) {
                                waterPulseUnitEnd = 0;
                            }
                        }
                        nodeData[nodeIndex].m_waterPulseGroup = group;
                        canContinue = true;
                    }
                }
            }
        }

        private static ushort GetRootHeatingGroup(WaterManager.PulseGroup[] heatingPulseGroups, ushort group) {
            for (ushort mergeIndex = heatingPulseGroups[group].m_mergeIndex; mergeIndex != 65535; mergeIndex = heatingPulseGroups[group].m_mergeIndex) {
                group = mergeIndex;
            }
            return group;
        }

        private static ushort GetRootSewageGroup(WaterManager.PulseGroup[] sewagePulseGroups, ushort group) {
            for (ushort mergeIndex = sewagePulseGroups[group].m_mergeIndex; mergeIndex != 65535; mergeIndex = sewagePulseGroups[group].m_mergeIndex) {
                group = mergeIndex;
            }
            return group;
        }

        private static ushort GetRootWaterGroup(WaterManager.PulseGroup[] waterPulseGroups, ushort group) {
            for (ushort mergeIndex = waterPulseGroups[group].m_mergeIndex; mergeIndex != 65535; mergeIndex = waterPulseGroups[group].m_mergeIndex) {
                group = mergeIndex;
            }
            return group;
        }

        private static void MergeHeatingGroups(WaterManager.PulseGroup[] heatingPulseGroups, int heatingPulseGroupCount, ushort root, ushort merged) {
            WaterManager.PulseGroup pulseGroup = heatingPulseGroups[root];
            WaterManager.PulseGroup pulseGroup2 = heatingPulseGroups[merged];
            pulseGroup.m_origPressure += pulseGroup2.m_origPressure;
            if (pulseGroup2.m_mergeCount != 0) {
                for (int i = 0; i < heatingPulseGroupCount; i++) {
                    if (heatingPulseGroups[i].m_mergeIndex == merged) {
                        heatingPulseGroups[i].m_mergeIndex = root;
                        pulseGroup2.m_origPressure -= heatingPulseGroups[i].m_origPressure;
                    }
                }
                pulseGroup.m_mergeCount += pulseGroup2.m_mergeCount;
                pulseGroup2.m_mergeCount = 0;
            }
            pulseGroup.m_curPressure += pulseGroup2.m_curPressure;
            pulseGroup2.m_curPressure = 0u;
            pulseGroup.m_mergeCount += 1;
            pulseGroup2.m_mergeIndex = root;
            heatingPulseGroups[root] = pulseGroup;
            heatingPulseGroups[merged] = pulseGroup2;
        }

        private static void MergeSewageGroups(WaterManager.PulseGroup[] sewagePulseGroups, int sewagePulseGroupCount, ushort root, ushort merged) {
            WaterManager.PulseGroup pulseGroup = sewagePulseGroups[root];
            WaterManager.PulseGroup pulseGroup2 = sewagePulseGroups[merged];
            pulseGroup.m_origPressure += pulseGroup2.m_origPressure;
            pulseGroup.m_collectPressure += pulseGroup2.m_collectPressure;
            if (pulseGroup2.m_mergeCount != 0) {
                for (int i = 0; i < sewagePulseGroupCount; i++) {
                    if (sewagePulseGroups[i].m_mergeIndex == merged) {
                        sewagePulseGroups[i].m_mergeIndex = root;
                        pulseGroup2.m_origPressure -= sewagePulseGroups[i].m_origPressure;
                        pulseGroup2.m_collectPressure -= sewagePulseGroups[i].m_collectPressure;
                    }
                }
                pulseGroup.m_mergeCount += pulseGroup2.m_mergeCount;
                pulseGroup2.m_mergeCount = 0;
            }
            pulseGroup.m_curPressure += pulseGroup2.m_curPressure;
            pulseGroup2.m_curPressure = 0u;
            pulseGroup.m_mergeCount += 1;
            pulseGroup2.m_mergeIndex = root;
            sewagePulseGroups[root] = pulseGroup;
            sewagePulseGroups[merged] = pulseGroup2;
        }

        private static void MergeWaterGroups(WaterManager.Node[] nodeData, WaterManager.PulseGroup[] waterPulseGroups, int waterPulseGroupCount, ushort root, ushort merged) {
            WaterManager.PulseGroup pulseGroup = waterPulseGroups[root];
            WaterManager.PulseGroup pulseGroup2 = waterPulseGroups[merged];
            pulseGroup.m_origPressure += pulseGroup2.m_origPressure;
            pulseGroup.m_collectPressure += pulseGroup2.m_collectPressure;
            if (pulseGroup2.m_origPressure != 0u) {
                nodeData[pulseGroup.m_node].m_pollution = (byte)(nodeData[pulseGroup.m_node].m_pollution + nodeData[pulseGroup2.m_node].m_pollution + 1 >> 1);
            }
            if (pulseGroup2.m_mergeCount != 0) {
                for (int i = 0; i < waterPulseGroupCount; i++) {
                    if (waterPulseGroups[i].m_mergeIndex == merged) {
                        waterPulseGroups[i].m_mergeIndex = root;
                        pulseGroup2.m_origPressure -= waterPulseGroups[i].m_origPressure;
                        pulseGroup2.m_collectPressure -= waterPulseGroups[i].m_collectPressure;
                    }
                }
                pulseGroup.m_mergeCount += pulseGroup2.m_mergeCount;
                pulseGroup2.m_mergeCount = 0;
            }
            pulseGroup.m_curPressure += pulseGroup2.m_curPressure;
            pulseGroup2.m_curPressure = 0u;
            pulseGroup.m_mergeCount += 1;
            pulseGroup2.m_mergeIndex = root;
            waterPulseGroups[root] = pulseGroup;
            waterPulseGroups[merged] = pulseGroup2;
        }

        internal static void SimulationStepImpl(WaterManager.Node[] nodeData, WaterManager.Cell[] waterGrid,
                                                WaterManager.PulseGroup[] waterPulseGroups, WaterManager.PulseGroup[] sewagePulseGroups, WaterManager.PulseGroup[] heatingPulseGroups,
                                                ref int waterPulseGroupCount, ref int waterPulseUnitStart, ref int waterPulseUnitEnd,
                                                ref int sewagePulseGroupCount, ref int sewagePulseUnitStart, ref int sewagePulseUnitEnd,
                                                ref int heatingPulseGroupCount, ref int heatingPulseUnitStart, ref int heatingPulseUnitEnd,
                                                ref int processedCells, ref int conductiveCells, ref bool canContinue, int subStep) {
            if (subStep != 0 && subStep != 1000) {
                NetManager instance = Singleton<NetManager>.instance;
                uint currentFrameIndex = Singleton<SimulationManager>.instance.m_currentFrameIndex;
                int num = (int)(currentFrameIndex & 255u);
                if (num < 128) {
                    if (num == 0) {
                        waterPulseGroupCount = 0;
                        waterPulseUnitStart = 0;
                        waterPulseUnitEnd = 0;
                        sewagePulseGroupCount = 0;
                        sewagePulseUnitStart = 0;
                        sewagePulseUnitEnd = 0;
                        heatingPulseGroupCount = 0;
                        heatingPulseUnitStart = 0;
                        heatingPulseUnitEnd = 0;
                        processedCells = 0;
                        conductiveCells = 0;
                        canContinue = true;
                    }
                    int num2 = num * 32768 >> 7;
                    int num3 = ((num + 1) * 32768 >> 7) - 1;
                    for (int i = num2; i <= num3; i++) {
                        WaterManager.Node node = nodeData[i];
                        NetNode.Flags flags = instance.m_nodes.m_buffer[i].m_flags;
                        if (flags != NetNode.Flags.None) {
                            NetInfo info = instance.m_nodes.m_buffer[i].Info;
                            if (info.m_class.m_service == ItemClass.Service.Water && info.m_class.m_level <= ItemClass.Level.Level2) {
                                int water = (node.m_waterPulseGroup == 65535) ? 0 : 1;
                                int sewage = (node.m_sewagePulseGroup == 65535) ? 0 : 1;
                                int heating = (node.m_heatingPulseGroup == 65535) ? 0 : 1;
                                UpdateNodeWater(i, water, sewage, heating);
                                conductiveCells += 2;
                                node.m_waterPulseGroup = 65535;
                                node.m_sewagePulseGroup = 65535;
                                node.m_heatingPulseGroup = 65535;
                                if ((node.m_curWaterPressure != 0 || node.m_collectWaterPressure != 0) && waterPulseGroupCount < 1024) {
                                    WaterManager.PulseGroup pulseGroup;
                                    pulseGroup.m_origPressure = node.m_curWaterPressure;
                                    pulseGroup.m_curPressure = node.m_curWaterPressure;
                                    pulseGroup.m_collectPressure = node.m_collectWaterPressure;
                                    pulseGroup.m_mergeCount = 0;
                                    pulseGroup.m_mergeIndex = 65535;
                                    pulseGroup.m_node = (ushort)i;
                                    node.m_waterPulseGroup = (ushort)waterPulseGroupCount;
                                    waterPulseGroups[waterPulseGroupCount++] = pulseGroup;
                                    if (pulseGroup.m_origPressure != 0u) {
                                        PulseUnit pulseUnit;
                                        pulseUnit.m_group = (ushort)(waterPulseGroupCount - 1);
                                        pulseUnit.m_node = (ushort)i;
                                        pulseUnit.m_x = 0;
                                        pulseUnit.m_z = 0;
                                        m_waterPulseUnits[waterPulseUnitEnd] = pulseUnit;
                                        if (++waterPulseUnitEnd == m_waterPulseUnits.Length) {
                                            waterPulseUnitEnd = 0;
                                        }
                                    }
                                }
                                if ((node.m_curSewagePressure != 0 || node.m_collectSewagePressure != 0) && sewagePulseGroupCount < 1024) {
                                    WaterManager.PulseGroup pulseGroup2;
                                    pulseGroup2.m_origPressure = node.m_curSewagePressure;
                                    pulseGroup2.m_curPressure = node.m_curSewagePressure;
                                    pulseGroup2.m_collectPressure = node.m_collectSewagePressure;
                                    pulseGroup2.m_mergeCount = 0;
                                    pulseGroup2.m_mergeIndex = 65535;
                                    pulseGroup2.m_node = (ushort)i;
                                    node.m_sewagePulseGroup = (ushort)sewagePulseGroupCount;
                                    sewagePulseGroups[sewagePulseGroupCount++] = pulseGroup2;
                                    if (pulseGroup2.m_origPressure != 0u) {
                                        PulseUnit pulseUnit2;
                                        pulseUnit2.m_group = (ushort)(sewagePulseGroupCount - 1);
                                        pulseUnit2.m_node = (ushort)i;
                                        pulseUnit2.m_x = 0;
                                        pulseUnit2.m_z = 0;
                                        m_sewagePulseUnits[sewagePulseUnitEnd] = pulseUnit2;
                                        if (++sewagePulseUnitEnd == m_sewagePulseUnits.Length) {
                                            sewagePulseUnitEnd = 0;
                                        }
                                    }
                                }
                                if (node.m_curHeatingPressure != 0 && heatingPulseGroupCount < 1024) {
                                    WaterManager.PulseGroup pulseGroup3;
                                    pulseGroup3.m_origPressure = node.m_curHeatingPressure;
                                    pulseGroup3.m_curPressure = node.m_curHeatingPressure;
                                    pulseGroup3.m_collectPressure = 0u;
                                    pulseGroup3.m_mergeCount = 0;
                                    pulseGroup3.m_mergeIndex = 65535;
                                    pulseGroup3.m_node = (ushort)i;
                                    PulseUnit pulseUnit3;
                                    pulseUnit3.m_group = (ushort)heatingPulseGroupCount;
                                    pulseUnit3.m_node = (ushort)i;
                                    pulseUnit3.m_x = 0;
                                    pulseUnit3.m_z = 0;
                                    node.m_heatingPulseGroup = (ushort)heatingPulseGroupCount;
                                    heatingPulseGroups[heatingPulseGroupCount++] = pulseGroup3;
                                    m_heatingPulseUnits[heatingPulseUnitEnd] = pulseUnit3;
                                    if (++heatingPulseUnitEnd == m_heatingPulseUnits.Length) {
                                        heatingPulseUnitEnd = 0;
                                    }
                                }
                            } else {
                                node.m_waterPulseGroup = 65535;
                                node.m_sewagePulseGroup = 65535;
                                node.m_heatingPulseGroup = 65535;
                                node.m_extraWaterPressure = 0;
                                node.m_extraSewagePressure = 0;
                                node.m_extraHeatingPressure = 0;
                            }
                        } else {
                            node.m_waterPulseGroup = 65535;
                            node.m_sewagePulseGroup = 65535;
                            node.m_heatingPulseGroup = 65535;
                            node.m_extraWaterPressure = 0;
                            node.m_extraSewagePressure = 0;
                            node.m_extraHeatingPressure = 0;
                        }
                        node.m_curWaterPressure = 0;
                        node.m_curSewagePressure = 0;
                        node.m_curHeatingPressure = 0;
                        node.m_collectWaterPressure = 0;
                        node.m_collectSewagePressure = 0;
                        nodeData[i] = node;
                    }
                    int num4 = num * WATERGRID_RESOLUTION >> 7;
                    int num5 = ((num + 1) * WATERGRID_RESOLUTION >> 7) - 1;
                    for (int j = num4; j <= num5; j++) {
                        int num6 = j * WATERGRID_RESOLUTION;
                        for (int k = 0; k < WATERGRID_RESOLUTION; k++) {
                            WaterManager.Cell cell = waterGrid[num6];
                            cell.m_waterPulseGroup = 65535;
                            cell.m_sewagePulseGroup = 65535;
                            cell.m_heatingPulseGroup = 65535;
                            if (cell.m_conductivity >= 96) {
                                conductiveCells += 2;
                            }
                            if (cell.m_tmpHasWater != cell.m_hasWater) {
                                cell.m_hasWater = cell.m_tmpHasWater;
                            }
                            if (cell.m_tmpHasSewage != cell.m_hasSewage) {
                                cell.m_hasSewage = cell.m_tmpHasSewage;
                            }
                            if (cell.m_tmpHasHeating != cell.m_hasHeating) {
                                cell.m_hasHeating = cell.m_tmpHasHeating;
                            }
                            cell.m_tmpHasWater = false;
                            cell.m_tmpHasSewage = false;
                            cell.m_tmpHasHeating = false;
                            waterGrid[num6] = cell;
                            num6++;
                        }
                    }
                } else {
                    int num7 = (num - 127) * conductiveCells >> 7;
                    if (num == 255) {
                        num7 = 1000000000;
                    }
                    while (canContinue && processedCells < num7) {
                        canContinue = false;
                        int __waterPulseUnitEnd = waterPulseUnitEnd;
                        int __sewagePulseUnitEnd = sewagePulseUnitEnd;
                        int __heatingPulseUnitEnd = heatingPulseUnitEnd;
                        while (waterPulseUnitStart != __waterPulseUnitEnd) {
                            PulseUnit pulseUnit4 = m_waterPulseUnits[waterPulseUnitStart];
                            if (++waterPulseUnitStart == m_waterPulseUnits.Length) {
                                waterPulseUnitStart = 0;
                            }
                            pulseUnit4.m_group = GetRootWaterGroup(waterPulseGroups, pulseUnit4.m_group);
                            uint num8 = waterPulseGroups[pulseUnit4.m_group].m_curPressure;
                            if (pulseUnit4.m_node == 0) {
                                int num9 = pulseUnit4.m_z * WATERGRID_RESOLUTION + pulseUnit4.m_x;
                                WaterManager.Cell cell2 = waterGrid[num9];
                                if (cell2.m_conductivity != 0 && !cell2.m_tmpHasWater && num8 != 0u) {
                                    int num10 = EMath.Clamp((-cell2.m_currentWaterPressure), 0, (int)num8);
                                    num8 -= (uint)num10;
                                    cell2.m_currentWaterPressure += (short)num10;
                                    if (cell2.m_currentWaterPressure >= 0) {
                                        cell2.m_tmpHasWater = true;
                                        cell2.m_pollution = nodeData[waterPulseGroups[pulseUnit4.m_group].m_node].m_pollution;
                                    }
                                    waterGrid[num9] = cell2;
                                    waterPulseGroups[pulseUnit4.m_group].m_curPressure = num8;
                                }
                                if (num8 != 0u) {
                                    processedCells++;
                                } else {
                                    m_waterPulseUnits[waterPulseUnitEnd] = pulseUnit4;
                                    if (++waterPulseUnitEnd == m_waterPulseUnits.Length) {
                                        waterPulseUnitEnd = 0;
                                    }
                                }
                            } else if (num8 != 0u) {
                                processedCells++;
                                NetNode netNode = instance.m_nodes.m_buffer[pulseUnit4.m_node];
                                if (netNode.m_flags != NetNode.Flags.None && netNode.m_buildIndex < (currentFrameIndex & 4294967168u)) {
                                    byte pollution = nodeData[waterPulseGroups[pulseUnit4.m_group].m_node].m_pollution;
                                    nodeData[pulseUnit4.m_node].m_pollution = pollution;
                                    if (netNode.m_building != 0) {
                                        Singleton<BuildingManager>.instance.m_buildings.m_buffer[netNode.m_building].m_waterPollution = pollution;
                                    }
                                    ConductWaterToCells(waterGrid, ref waterPulseUnitEnd, ref canContinue, pulseUnit4.m_group, netNode.m_position.x, netNode.m_position.z, 100f);
                                    for (int l = 0; l < 8; l++) {
                                        ushort segment = netNode.GetSegment(l);
                                        if (segment != 0) {
                                            ushort startNode = instance.m_segments.m_buffer[segment].m_startNode;
                                            ushort endNode = instance.m_segments.m_buffer[segment].m_endNode;
                                            ushort num11 = (startNode != pulseUnit4.m_node) ? startNode : endNode;
                                            ConductWaterToNode(nodeData, waterPulseGroups, ref waterPulseUnitEnd, ref canContinue, waterPulseGroupCount, num11, ref instance.m_nodes.m_buffer[num11], pulseUnit4.m_group);
                                        }
                                    }
                                }
                            } else {
                                m_waterPulseUnits[waterPulseUnitEnd] = pulseUnit4;
                                if (++waterPulseUnitEnd == m_waterPulseUnits.Length) {
                                    waterPulseUnitEnd = 0;
                                }
                            }
                        }
                        while (sewagePulseUnitStart != __sewagePulseUnitEnd) {
                            PulseUnit pulseUnit5 = m_sewagePulseUnits[sewagePulseUnitStart];
                            if (++sewagePulseUnitStart == m_sewagePulseUnits.Length) {
                                sewagePulseUnitStart = 0;
                            }
                            pulseUnit5.m_group = GetRootSewageGroup(sewagePulseGroups, pulseUnit5.m_group);
                            uint num12 = sewagePulseGroups[pulseUnit5.m_group].m_curPressure;
                            if (pulseUnit5.m_node == 0) {
                                int num13 = pulseUnit5.m_z * WATERGRID_RESOLUTION + pulseUnit5.m_x;
                                WaterManager.Cell cell3 = waterGrid[num13];
                                if (cell3.m_conductivity != 0 && !cell3.m_tmpHasSewage && num12 != 0u) {
                                    int num14 = EMath.Clamp((-cell3.m_currentSewagePressure), 0, (int)num12);
                                    num12 -= (uint)num14;
                                    cell3.m_currentSewagePressure += (short)num14;
                                    if (cell3.m_currentSewagePressure >= 0) {
                                        cell3.m_tmpHasSewage = true;
                                    }
                                    waterGrid[num13] = cell3;
                                    sewagePulseGroups[pulseUnit5.m_group].m_curPressure = num12;
                                }
                                if (num12 != 0u) {
                                    processedCells++;
                                } else {
                                    m_sewagePulseUnits[sewagePulseUnitEnd] = pulseUnit5;
                                    if (++sewagePulseUnitEnd == m_sewagePulseUnits.Length) {
                                        sewagePulseUnitEnd = 0;
                                    }
                                }
                            } else if (num12 != 0u) {
                                processedCells++;
                                NetNode netNode2 = instance.m_nodes.m_buffer[pulseUnit5.m_node];
                                if (netNode2.m_flags != NetNode.Flags.None && netNode2.m_buildIndex < (currentFrameIndex & 4294967168u)) {
                                    ConductSewageToCells(waterGrid, ref sewagePulseUnitEnd, ref canContinue, pulseUnit5.m_group, netNode2.m_position.x, netNode2.m_position.z, 100f);
                                    for (int m = 0; m < 8; m++) {
                                        ushort segment2 = netNode2.GetSegment(m);
                                        if (segment2 != 0) {
                                            ushort startNode2 = instance.m_segments.m_buffer[segment2].m_startNode;
                                            ushort endNode2 = instance.m_segments.m_buffer[segment2].m_endNode;
                                            ushort num15 = (startNode2 != pulseUnit5.m_node) ? startNode2 : endNode2;
                                            ConductSewageToNode(nodeData, sewagePulseGroups, ref sewagePulseUnitEnd, ref canContinue, sewagePulseGroupCount, num15, ref instance.m_nodes.m_buffer[num15], pulseUnit5.m_group);
                                        }
                                    }
                                }
                            } else {
                                m_sewagePulseUnits[sewagePulseUnitEnd] = pulseUnit5;
                                if (++sewagePulseUnitEnd == m_sewagePulseUnits.Length) {
                                    sewagePulseUnitEnd = 0;
                                }
                            }
                        }
                        while (heatingPulseUnitStart != __heatingPulseUnitEnd) {
                            PulseUnit pulseUnit6 = m_heatingPulseUnits[heatingPulseUnitStart];
                            if (++heatingPulseUnitStart == m_heatingPulseUnits.Length) {
                                heatingPulseUnitStart = 0;
                            }
                            pulseUnit6.m_group = GetRootHeatingGroup(heatingPulseGroups, pulseUnit6.m_group);
                            uint num16 = heatingPulseGroups[pulseUnit6.m_group].m_curPressure;
                            if (pulseUnit6.m_node == 0) {
                                int num17 = pulseUnit6.m_z * WATERGRID_RESOLUTION + pulseUnit6.m_x;
                                WaterManager.Cell cell4 = waterGrid[num17];
                                if (cell4.m_conductivity2 != 0 && !cell4.m_tmpHasHeating && num16 != 0u) {
                                    int num18 = EMath.Clamp((-cell4.m_currentHeatingPressure), 0, (int)num16);
                                    num16 -= (uint)num18;
                                    cell4.m_currentHeatingPressure += (short)num18;
                                    if (cell4.m_currentHeatingPressure >= 0) {
                                        cell4.m_tmpHasHeating = true;
                                    }
                                    waterGrid[num17] = cell4;
                                    heatingPulseGroups[pulseUnit6.m_group].m_curPressure = num16;
                                }
                                if (num16 != 0u) {
                                    processedCells++;
                                } else {
                                    m_heatingPulseUnits[heatingPulseUnitEnd] = pulseUnit6;
                                    if (++heatingPulseUnitEnd == m_heatingPulseUnits.Length) {
                                        heatingPulseUnitEnd = 0;
                                    }
                                }
                            } else if (num16 != 0u) {
                                processedCells++;
                                NetNode netNode3 = instance.m_nodes.m_buffer[pulseUnit6.m_node];
                                if (netNode3.m_flags != NetNode.Flags.None && netNode3.m_buildIndex < (currentFrameIndex & 4294967168u)) {
                                    ConductHeatingToCells(waterGrid, ref heatingPulseUnitEnd, ref canContinue, pulseUnit6.m_group, netNode3.m_position.x, netNode3.m_position.z, 100f);
                                    for (int n = 0; n < 8; n++) {
                                        ushort segment3 = netNode3.GetSegment(n);
                                        if (segment3 != 0) {
                                            NetInfo info2 = instance.m_segments.m_buffer[segment3].Info;
                                            if (info2.m_class.m_service == ItemClass.Service.Water && info2.m_class.m_level == ItemClass.Level.Level2) {
                                                ushort startNode3 = instance.m_segments.m_buffer[segment3].m_startNode;
                                                ushort endNode3 = instance.m_segments.m_buffer[segment3].m_endNode;
                                                ushort num19 = (startNode3 != pulseUnit6.m_node) ? startNode3 : endNode3;
                                                ConductHeatingToNode(nodeData, heatingPulseGroups, heatingPulseGroupCount, ref heatingPulseUnitEnd, ref canContinue, num19, ref instance.m_nodes.m_buffer[num19], pulseUnit6.m_group);
                                            }
                                        }
                                    }
                                }
                            } else {
                                m_heatingPulseUnits[heatingPulseUnitEnd] = pulseUnit6;
                                if (++heatingPulseUnitEnd == m_heatingPulseUnits.Length) {
                                    heatingPulseUnitEnd = 0;
                                }
                            }
                        }
                    }
                    if (num == 255) {
                        for (int num20 = 0; num20 < waterPulseGroupCount; num20++) {
                            WaterManager.PulseGroup pulseGroup4 = waterPulseGroups[num20];
                            if (pulseGroup4.m_mergeIndex != 65535 && pulseGroup4.m_collectPressure != 0u) {
                                WaterManager.PulseGroup pulseGroup5 = waterPulseGroups[pulseGroup4.m_mergeIndex];
                                pulseGroup4.m_curPressure = pulseGroup5.m_curPressure * pulseGroup4.m_collectPressure / pulseGroup5.m_collectPressure;
                                if (pulseGroup4.m_collectPressure < pulseGroup4.m_curPressure) {
                                    pulseGroup4.m_curPressure = pulseGroup4.m_collectPressure;
                                }
                                pulseGroup5.m_curPressure -= pulseGroup4.m_curPressure;
                                pulseGroup5.m_collectPressure -= pulseGroup4.m_collectPressure;
                                waterPulseGroups[pulseGroup4.m_mergeIndex] = pulseGroup5;
                                waterPulseGroups[num20] = pulseGroup4;
                            }
                        }
                        for (int num21 = 0; num21 < waterPulseGroupCount; num21++) {
                            WaterManager.PulseGroup pulseGroup6 = waterPulseGroups[num21];
                            if (pulseGroup6.m_mergeIndex != 65535 && pulseGroup6.m_collectPressure == 0u) {
                                WaterManager.PulseGroup pulseGroup7 = waterPulseGroups[pulseGroup6.m_mergeIndex];
                                uint num22 = pulseGroup7.m_curPressure;
                                if (pulseGroup7.m_collectPressure >= num22) {
                                    num22 = 0u;
                                } else {
                                    num22 -= pulseGroup7.m_collectPressure;
                                }
                                pulseGroup6.m_curPressure = num22 * pulseGroup6.m_origPressure / pulseGroup7.m_origPressure;
                                pulseGroup7.m_curPressure -= pulseGroup6.m_curPressure;
                                pulseGroup7.m_origPressure -= pulseGroup6.m_origPressure;
                                waterPulseGroups[pulseGroup6.m_mergeIndex] = pulseGroup7;
                                waterPulseGroups[num21] = pulseGroup6;
                            }
                        }
                        for (int num23 = 0; num23 < waterPulseGroupCount; num23++) {
                            WaterManager.PulseGroup pulseGroup8 = waterPulseGroups[num23];
                            if (pulseGroup8.m_curPressure != 0u) {
                                WaterManager.Node node2 = nodeData[pulseGroup8.m_node];
                                node2.m_extraWaterPressure += (ushort)EMath.Min((int)pulseGroup8.m_curPressure, (32767 - node2.m_extraWaterPressure));
                                nodeData[pulseGroup8.m_node] = node2;
                            }
                        }
                        for (int num24 = 0; num24 < sewagePulseGroupCount; num24++) {
                            WaterManager.PulseGroup pulseGroup9 = sewagePulseGroups[num24];
                            if (pulseGroup9.m_mergeIndex != 65535 && pulseGroup9.m_collectPressure != 0u) {
                                WaterManager.PulseGroup pulseGroup10 = sewagePulseGroups[pulseGroup9.m_mergeIndex];
                                pulseGroup9.m_curPressure = pulseGroup10.m_curPressure * pulseGroup9.m_collectPressure / pulseGroup10.m_collectPressure;
                                if (pulseGroup9.m_collectPressure < pulseGroup9.m_curPressure) {
                                    pulseGroup9.m_curPressure = pulseGroup9.m_collectPressure;
                                }
                                pulseGroup10.m_curPressure -= pulseGroup9.m_curPressure;
                                pulseGroup10.m_collectPressure -= pulseGroup9.m_collectPressure;
                                sewagePulseGroups[pulseGroup9.m_mergeIndex] = pulseGroup10;
                                sewagePulseGroups[num24] = pulseGroup9;
                            }
                        }
                        for (int num25 = 0; num25 < sewagePulseGroupCount; num25++) {
                            WaterManager.PulseGroup pulseGroup11 = sewagePulseGroups[num25];
                            if (pulseGroup11.m_mergeIndex != 65535 && pulseGroup11.m_collectPressure == 0u) {
                                WaterManager.PulseGroup pulseGroup12 = sewagePulseGroups[pulseGroup11.m_mergeIndex];
                                uint num26 = pulseGroup12.m_curPressure;
                                if (pulseGroup12.m_collectPressure < num26) {
                                    num26 -= pulseGroup12.m_collectPressure;
                                }
                                pulseGroup11.m_curPressure = pulseGroup12.m_curPressure * pulseGroup11.m_origPressure / pulseGroup12.m_origPressure;
                                pulseGroup12.m_curPressure -= pulseGroup11.m_curPressure;
                                pulseGroup12.m_origPressure -= pulseGroup11.m_origPressure;
                                sewagePulseGroups[pulseGroup11.m_mergeIndex] = pulseGroup12;
                                sewagePulseGroups[num25] = pulseGroup11;
                            }
                        }
                        for (int num27 = 0; num27 < sewagePulseGroupCount; num27++) {
                            WaterManager.PulseGroup pulseGroup13 = sewagePulseGroups[num27];
                            if (pulseGroup13.m_curPressure != 0u) {
                                WaterManager.Node node3 = nodeData[pulseGroup13.m_node];
                                node3.m_extraSewagePressure += (ushort)EMath.Min((int)pulseGroup13.m_curPressure, (32767 - node3.m_extraSewagePressure));
                                nodeData[pulseGroup13.m_node] = node3;
                            }
                        }
                        for (int num28 = 0; num28 < heatingPulseGroupCount; num28++) {
                            WaterManager.PulseGroup pulseGroup14 = heatingPulseGroups[num28];
                            if (pulseGroup14.m_mergeIndex != 65535) {
                                WaterManager.PulseGroup pulseGroup15 = heatingPulseGroups[pulseGroup14.m_mergeIndex];
                                pulseGroup14.m_curPressure = pulseGroup15.m_curPressure * pulseGroup14.m_origPressure / pulseGroup15.m_origPressure;
                                pulseGroup15.m_curPressure -= pulseGroup14.m_curPressure;
                                pulseGroup15.m_origPressure -= pulseGroup14.m_origPressure;
                                heatingPulseGroups[pulseGroup14.m_mergeIndex] = pulseGroup15;
                                heatingPulseGroups[num28] = pulseGroup14;
                            }
                        }
                        for (int num29 = 0; num29 < heatingPulseGroupCount; num29++) {
                            WaterManager.PulseGroup pulseGroup16 = heatingPulseGroups[num29];
                            if (pulseGroup16.m_curPressure != 0u) {
                                WaterManager.Node node4 = nodeData[pulseGroup16.m_node];
                                node4.m_extraHeatingPressure += (ushort)EMath.Min((int)pulseGroup16.m_curPressure, (32767 - node4.m_extraHeatingPressure));
                                nodeData[pulseGroup16.m_node] = node4;
                            }
                        }
                    }
                }
            }
        }

        internal static int TryDumpSewage(WaterManager.Cell[] waterGrid, Vector3 pos, int rate, int max) {
            const float halfGrid = WATERGRID_RESOLUTION / 2f;
            if (max == 0) {
                return 0;
            }
            int x = EMath.Clamp((int)(pos.x / WATERGRID_CELL_SIZE + halfGrid), 0, WATERGRID_RESOLUTION - 1);
            int z = EMath.Clamp((int)(pos.z / WATERGRID_CELL_SIZE + halfGrid), 0, WATERGRID_RESOLUTION - 1);
            int result = 0;
            if (TryDumpSewageImpl(waterGrid, pos, x, z, rate, max, ref result)) {
                return result;
            }
            if (waterGrid[z * WATERGRID_RESOLUTION + x].m_conductivity == 0) {
                return 0;
            }
            float x1 = (x + 0.5f - halfGrid) * WATERGRID_CELL_SIZE;
            float z1 = (z + 0.5f - halfGrid) * WATERGRID_CELL_SIZE;
            if (pos.z > z1 && z < WATERGRID_RESOLUTION - 1) {
                if (TryDumpSewageImpl(waterGrid, pos, x, z + 1, rate, max, ref result)) {
                    return result;
                }
            } else if (pos.z < z1 && z > 0 && TryDumpSewageImpl(waterGrid, pos, x, z - 1, rate, max, ref result)) {
                return result;
            }
            if (pos.x > x1 && x < WATERGRID_RESOLUTION - 1) {
                if (TryDumpSewageImpl(waterGrid, pos, x + 1, z, rate, max, ref result)) {
                    return result;
                }
                if (pos.z > z1 && z < WATERGRID_RESOLUTION - 1) {
                    if (TryDumpSewageImpl(waterGrid, pos, x + 1, z + 1, rate, max, ref result)) {
                        return result;
                    }
                } else if (pos.z < z1 && z > 0 && TryDumpSewageImpl(waterGrid, pos, x + 1, z - 1, rate, max, ref result)) {
                    return result;
                }
            } else if (pos.x < x1 && x > 0) {
                if (TryDumpSewageImpl(waterGrid, pos, x - 1, z, rate, max, ref result)) {
                    return result;
                }
                if (pos.z > z1 && z < WATERGRID_RESOLUTION - 1) {
                    if (TryDumpSewageImpl(waterGrid, pos, x - 1, z + 1, rate, max, ref result)) {
                        return result;
                    }
                } else if (pos.z < z1 && z > 0 && TryDumpSewageImpl(waterGrid, pos, x - 1, z - 1, rate, max, ref result)) {
                    return result;
                }
            }
            return 0;
        }

        private static bool TryDumpSewageImpl(WaterManager.Cell[] waterGrid, Vector3 pos, int x, int z, int rate, int max, ref int result) {
            int num = z * WATERGRID_RESOLUTION + x;
            ref WaterManager.Cell cell = ref waterGrid[num];
            if (cell.m_hasSewage) {
                NetManager instance = Singleton<NetManager>.instance;
                ushort closestPipeSegment = cell.m_closestPipeSegment;
                ushort startNode = instance.m_segments.m_buffer[closestPipeSegment].m_startNode;
                ushort endNode = instance.m_segments.m_buffer[closestPipeSegment].m_endNode;
                NetNode.Flags flags = instance.m_nodes.m_buffer[startNode].m_flags;
                NetNode.Flags flags2 = instance.m_nodes.m_buffer[endNode].m_flags;
                if ((flags & flags2 & NetNode.Flags.Sewage) != NetNode.Flags.None) {
                    Segment2 segment;
                    segment.a = VectorUtils.XZ(instance.m_nodes.m_buffer[startNode].m_position);
                    segment.b = VectorUtils.XZ(instance.m_nodes.m_buffer[endNode].m_position);
                    if (segment.DistanceSqr(VectorUtils.XZ(pos), out float _) < 9025.0) {
                        rate = EMath.Min(EMath.Min(rate, max), 32768 + cell.m_currentSewagePressure);
                        cell.m_currentSewagePressure -= (short)rate;
                        waterGrid[num] = cell;
                        result = rate;
                        return true;
                    }
                }
            }
            return false;
        }

        internal static int TryFetchHeating(WaterManager.Cell[] waterGrid, Vector3 pos, int rate, int max, out bool connected) {
            const float halfGrid = WATERGRID_RESOLUTION / 2f;
            connected = false;
            if (max == 0) {
                return 0;
            }
            int num = EMath.Clamp((int)(pos.x / WATERGRID_CELL_SIZE + halfGrid), 0, WATERGRID_RESOLUTION - 1);
            int num2 = EMath.Clamp((int)(pos.z / WATERGRID_CELL_SIZE + halfGrid), 0, WATERGRID_RESOLUTION - 1);
            int result = 0;
            if (TryFetchHeatingImpl(waterGrid, pos, num, num2, rate, max, ref result, ref connected)) {
                return result;
            }
            if (waterGrid[num2 * WATERGRID_RESOLUTION + num].m_conductivity2 == 0) {
                return 0;
            }
            float num3 = (num + 0.5f - halfGrid) * WATERGRID_CELL_SIZE;
            float num4 = (num2 + 0.5f - halfGrid) * WATERGRID_CELL_SIZE;
            if (pos.z > num4 && num2 < WATERGRID_RESOLUTION - 1) {
                if (TryFetchHeatingImpl(waterGrid, pos, num, num2 + 1, rate, max, ref result, ref connected)) {
                    return result;
                }
            } else if (pos.z < num4 && num2 > 0 && TryFetchHeatingImpl(waterGrid, pos, num, num2 - 1, rate, max, ref result, ref connected)) {
                return result;
            }
            if (pos.x > num3 && num < WATERGRID_RESOLUTION - 1) {
                if (TryFetchHeatingImpl(waterGrid, pos, num + 1, num2, rate, max, ref result, ref connected)) {
                    return result;
                }
                if (pos.z > num4 && num2 < WATERGRID_RESOLUTION - 1) {
                    if (TryFetchHeatingImpl(waterGrid, pos, num + 1, num2 + 1, rate, max, ref result, ref connected)) {
                        return result;
                    }
                } else if (pos.z < num4 && num2 > 0 && TryFetchHeatingImpl(waterGrid, pos, num + 1, num2 - 1, rate, max, ref result, ref connected)) {
                    return result;
                }
            } else if (pos.x < num3 && num > 0) {
                if (TryFetchHeatingImpl(waterGrid, pos, num - 1, num2, rate, max, ref result, ref connected)) {
                    return result;
                }
                if (pos.z > num4 && num2 < WATERGRID_RESOLUTION - 1) {
                    if (TryFetchHeatingImpl(waterGrid, pos, num - 1, num2 + 1, rate, max, ref result, ref connected)) {
                        return result;
                    }
                } else if (pos.z < num4 && num2 > 0 && TryFetchHeatingImpl(waterGrid, pos, num - 1, num2 - 1, rate, max, ref result, ref connected)) {
                    return result;
                }
            }
            return 0;
        }

        private static bool TryFetchHeatingImpl(WaterManager.Cell[] waterGrid, Vector3 pos, int x, int z, int rate, int max, ref int result, ref bool connected) {
            int num = z * WATERGRID_RESOLUTION + x;
            WaterManager.Cell cell = waterGrid[num];
            if (cell.m_hasHeating) {
                NetManager instance = Singleton<NetManager>.instance;
                ushort closestPipeSegment = cell.m_closestPipeSegment2;
                ushort startNode = instance.m_segments.m_buffer[closestPipeSegment].m_startNode;
                ushort endNode = instance.m_segments.m_buffer[closestPipeSegment].m_endNode;
                NetNode.Flags flags = instance.m_nodes.m_buffer[startNode].m_flags;
                NetNode.Flags flags2 = instance.m_nodes.m_buffer[endNode].m_flags;
                if ((flags & flags2 & NetNode.Flags.Heating) != NetNode.Flags.None) {
                    Segment2 segment;
                    segment.a = VectorUtils.XZ(instance.m_nodes.m_buffer[startNode].m_position);
                    segment.b = VectorUtils.XZ(instance.m_nodes.m_buffer[endNode].m_position);
                    if (segment.DistanceSqr(VectorUtils.XZ(pos), out float _) < 9025.0) {
                        rate = EMath.Min(EMath.Min(rate, max), 32768 + cell.m_currentHeatingPressure);
                        cell.m_currentHeatingPressure -= (short)rate;
                        waterGrid[num] = cell;
                        result = rate;
                        connected = true;
                        return true;
                    }
                    return false;
                }
            }
            if (cell.m_closestPipeSegment2 != 0 && cell.m_conductivity2 >= 96) {
                NetManager instance2 = Singleton<NetManager>.instance;
                ushort closestPipeSegment2 = cell.m_closestPipeSegment2;
                ushort startNode2 = instance2.m_segments.m_buffer[closestPipeSegment2].m_startNode;
                ushort endNode2 = instance2.m_segments.m_buffer[closestPipeSegment2].m_endNode;
                Segment2 segment2;
                segment2.a = VectorUtils.XZ(instance2.m_nodes.m_buffer[startNode2].m_position);
                segment2.b = VectorUtils.XZ(instance2.m_nodes.m_buffer[endNode2].m_position);
                if (segment2.DistanceSqr(VectorUtils.XZ(pos), out float _) < 9025.0) {
                    connected = true;
                }
            }
            return false;
        }

        internal static int TryFetchWater(WaterManager.Cell[] waterGrid, Vector3 pos, int rate, int max, ref byte waterPollution) {
            const float halfGrid = WATERGRID_RESOLUTION / 2f;
            if (max == 0) {
                return 0;
            }
            int num = EMath.Clamp((int)(pos.x / WATERGRID_CELL_SIZE + halfGrid), 0, WATERGRID_RESOLUTION - 1);
            int num2 = EMath.Clamp((int)(pos.z / WATERGRID_CELL_SIZE + halfGrid), 0, WATERGRID_RESOLUTION - 1);
            int result = 0;
            if (TryFetchWaterImpl(waterGrid, pos, num, num2, rate, max, ref result, ref waterPollution)) {
                return result;
            }
            if (waterGrid[num2 * WATERGRID_RESOLUTION + num].m_conductivity == 0) {
                return 0;
            }
            float num3 = (num + 0.5f - halfGrid) * WATERGRID_CELL_SIZE;
            float num4 = (num2 + 0.5f - halfGrid) * WATERGRID_CELL_SIZE;
            if (pos.z > num4 && num2 < WATERGRID_RESOLUTION - 1) {
                if (TryFetchWaterImpl(waterGrid, pos, num, num2 + 1, rate, max, ref result, ref waterPollution)) {
                    return result;
                }
            } else if (pos.z < num4 && num2 > 0 && TryFetchWaterImpl(waterGrid, pos, num, num2 - 1, rate, max, ref result, ref waterPollution)) {
                return result;
            }
            if (pos.x > num3 && num < WATERGRID_RESOLUTION - 1) {
                if (TryFetchWaterImpl(waterGrid, pos, num + 1, num2, rate, max, ref result, ref waterPollution)) {
                    return result;
                }
                if (pos.z > num4 && num2 < WATERGRID_RESOLUTION - 1) {
                    if (TryFetchWaterImpl(waterGrid, pos, num + 1, num2 + 1, rate, max, ref result, ref waterPollution)) {
                        return result;
                    }
                } else if (pos.z < num4 && num2 > 0 && TryFetchWaterImpl(waterGrid, pos, num + 1, num2 - 1, rate, max, ref result, ref waterPollution)) {
                    return result;
                }
            } else if (pos.x < num3 && num > 0) {
                if (TryFetchWaterImpl(waterGrid, pos, num - 1, num2, rate, max, ref result, ref waterPollution)) {
                    return result;
                }
                if (pos.z > num4 && num2 < WATERGRID_RESOLUTION - 1) {
                    if (TryFetchWaterImpl(waterGrid, pos, num - 1, num2 + 1, rate, max, ref result, ref waterPollution)) {
                        return result;
                    }
                } else if (pos.z < num4 && num2 > 0 && TryFetchWaterImpl(waterGrid, pos, num - 1, num2 - 1, rate, max, ref result, ref waterPollution)) {
                    return result;
                }
            }
            return 0;
        }

        private static bool TryFetchWaterImpl(WaterManager.Cell[] waterGrid, Vector3 pos, int x, int z, int rate, int max, ref int result, ref byte waterPollution) {
            int num = z * WATERGRID_RESOLUTION + x;
            WaterManager.Cell cell = waterGrid[num];
            if (cell.m_hasWater) {
                NetManager instance = Singleton<NetManager>.instance;
                ushort closestPipeSegment = cell.m_closestPipeSegment;
                ushort startNode = instance.m_segments.m_buffer[closestPipeSegment].m_startNode;
                ushort endNode = instance.m_segments.m_buffer[closestPipeSegment].m_endNode;
                NetNode.Flags flags = instance.m_nodes.m_buffer[startNode].m_flags;
                NetNode.Flags flags2 = instance.m_nodes.m_buffer[endNode].m_flags;
                if ((flags & flags2 & NetNode.Flags.Water) != NetNode.Flags.None) {
                    Segment2 segment;
                    segment.a = VectorUtils.XZ(instance.m_nodes.m_buffer[startNode].m_position);
                    segment.b = VectorUtils.XZ(instance.m_nodes.m_buffer[endNode].m_position);
                    if (segment.DistanceSqr(VectorUtils.XZ(pos), out float _) < 9025.0) {
                        rate = EMath.Min(EMath.Min(rate, max), 32768 + cell.m_currentWaterPressure);
                        cell.m_currentWaterPressure -= (short)rate;
                        waterPollution = cell.m_pollution;
                        waterGrid[num] = cell;
                        result = rate;
                        return true;
                    }
                }
            }
            return false;
        }

        internal static void UpdateGrid(WaterManager wmInstance, WaterManager.Cell[] waterGrid, float minX, float minZ, float maxX, float maxZ) {
            const float halfGrid = WATERGRID_RESOLUTION / 2f;
            int startX = EMath.Max((int)(minX / WATERGRID_CELL_SIZE + halfGrid), 0);
            int startZ = EMath.Max((int)(minZ / WATERGRID_CELL_SIZE + halfGrid), 0);
            int endX = EMath.Min((int)(maxX / WATERGRID_CELL_SIZE + halfGrid), WATERGRID_RESOLUTION - 1);
            int endZ = EMath.Min((int)(maxZ / WATERGRID_CELL_SIZE + halfGrid), WATERGRID_RESOLUTION - 1);
            for (int i = startZ; i <= endZ; i++) {
                int gridID = i * WATERGRID_RESOLUTION + startX;
                for (int j = startX; j <= endX; j++) {
                    waterGrid[gridID].m_conductivity = 0;
                    waterGrid[gridID].m_conductivity2 = 0;
                    waterGrid[gridID].m_closestPipeSegment = 0;
                    waterGrid[gridID].m_closestPipeSegment2 = 0;
                    gridID++;
                }
            }
            float num6 = (startX - halfGrid) * WATERGRID_CELL_SIZE - 100f;
            float num7 = (startZ - halfGrid) * WATERGRID_CELL_SIZE - 100f;
            float num8 = (endX - halfGrid + 1f) * WATERGRID_CELL_SIZE + 100f;
            float num9 = (endZ - halfGrid + 1f) * WATERGRID_CELL_SIZE + 100f;
            int num10 = EMath.Max((int)(num6 / 64f + 135f), 0);
            int num11 = EMath.Max((int)(num7 / 64f + 135f), 0);
            int num12 = EMath.Min((int)(num8 / 64f + 135f), 269);
            int num13 = EMath.Min((int)(num9 / 64f + 135f), 269);
            float num14 = 100f;
            NetManager nmInstance = Singleton<NetManager>.instance;
            NetNode[] nodes = nmInstance.m_nodes.m_buffer;
            NetSegment[] segments = nmInstance.m_segments.m_buffer;
            ushort[] segmentGrid = nmInstance.m_segmentGrid;
            for (int k = num11; k <= num13; k++) {
                for (int l = num10; l <= num12; l++) {
                    ushort segmentID = segmentGrid[k * 270 + l];
                    while (segmentID != 0) {
                        NetSegment.Flags flags = segments[segmentID].m_flags;
                        if ((flags & (NetSegment.Flags.Created | NetSegment.Flags.Deleted)) == NetSegment.Flags.Created) {
                            NetInfo info = segments[segmentID].Info;
                            if (info.m_class.m_service == ItemClass.Service.Water && info.m_class.m_level <= ItemClass.Level.Level2) {
                                ushort startNode = segments[segmentID].m_startNode;
                                ushort endNode = segments[segmentID].m_endNode;
                                Vector2 a = VectorUtils.XZ(nodes[startNode].m_position);
                                Vector2 b = VectorUtils.XZ(nodes[endNode].m_position);
                                float num17 = EMath.Max(EMath.Max(num6 - a.x, num7 - a.y), EMath.Max(a.x - num8, a.y - num9));
                                float num18 = EMath.Max(EMath.Max(num6 - b.x, num7 - b.y), EMath.Max(b.x - num8, b.y - num9));
                                if (num17 < 0f || num18 < 0f) {
                                    int num19 = EMath.Max((int)((EMath.Min(a.x, b.x) - num14) / WATERGRID_CELL_SIZE + halfGrid), startX);
                                    int num20 = EMath.Max((int)((EMath.Min(a.y, b.y) - num14) / WATERGRID_CELL_SIZE + halfGrid), startZ);
                                    int num21 = EMath.Min((int)((EMath.Max(a.x, b.x) + num14) / WATERGRID_CELL_SIZE + halfGrid), endX);
                                    int num22 = EMath.Min((int)((EMath.Max(a.y, b.y) + num14) / WATERGRID_CELL_SIZE + halfGrid), endZ);
                                    for (int m = num20; m <= num22; m++) {
                                        int waterIndex = m * WATERGRID_RESOLUTION + num19;
                                        float y = (m + 0.5f - halfGrid) * WATERGRID_CELL_SIZE;
                                        for (int n = num19; n <= num21; n++) {
                                            float x = (n + 0.5f - halfGrid) * WATERGRID_CELL_SIZE;
                                            float num24 = Segment2.DistanceSqr(a, b, new Vector2(x, y), out float _);
                                            num24 = (float)Math.Sqrt(num24);
                                            if (num24 < num14 + 19.125f) {
                                                float num26 = (num14 - num24) * 0.0130718956f + 0.25f;
                                                int conductivity = EMath.Min(255, EMath.RoundToInt(num26 * 255f));
                                                if (conductivity > waterGrid[waterIndex].m_conductivity) {
                                                    waterGrid[waterIndex].m_conductivity = (byte)conductivity;
                                                    waterGrid[waterIndex].m_closestPipeSegment = segmentID;
                                                }
                                                if (info.m_class.m_level == ItemClass.Level.Level2 && conductivity > waterGrid[waterIndex].m_conductivity2) {
                                                    waterGrid[waterIndex].m_conductivity2 = (byte)conductivity;
                                                    waterGrid[waterIndex].m_closestPipeSegment2 = segmentID;
                                                }
                                            }
                                            waterIndex++;
                                        }
                                    }
                                }
                            }
                        }
                        segmentID = segments[segmentID].m_nextGridSegment;
                    }
                }
            }
            for (int num28 = startZ; num28 <= endZ; num28++) {
                int num29 = num28 * WATERGRID_RESOLUTION + startX;
                for (int num30 = startX; num30 <= endX; num30++) {
                    WaterManager.Cell cell = waterGrid[num29];
                    if (cell.m_conductivity == 0) {
                        cell.m_currentWaterPressure = 0;
                        cell.m_currentSewagePressure = 0;
                        cell.m_currentHeatingPressure = 0;
                        cell.m_waterPulseGroup = 65535;
                        cell.m_sewagePulseGroup = 65535;
                        cell.m_heatingPulseGroup = 65535;
                        cell.m_tmpHasWater = false;
                        cell.m_tmpHasSewage = false;
                        cell.m_tmpHasHeating = false;
                        cell.m_hasWater = false;
                        cell.m_hasSewage = false;
                        cell.m_hasHeating = false;
                        cell.m_pollution = 0;
                        waterGrid[num29] = cell;
                    } else if (cell.m_conductivity2 == 0) {
                        cell.m_currentHeatingPressure = 0;
                        cell.m_heatingPulseGroup = 65535;
                        cell.m_tmpHasHeating = false;
                        cell.m_hasHeating = false;
                        waterGrid[num29] = cell;
                    }
                    num29++;
                }
            }
            wmInstance.AreaModified(startX, startZ, endX, endZ);
        }

        private static void UpdateNodeWater(int nodeID, int water, int sewage, int heating) {
            InfoManager.InfoMode currentMode = Singleton<InfoManager>.instance.CurrentMode;
            NetManager instance = Singleton<NetManager>.instance;
            bool flag = false;
            NetNode.Flags flags = instance.m_nodes.m_buffer[nodeID].m_flags;
            if ((flags & NetNode.Flags.Transition) != NetNode.Flags.None) {
                NetNode[] expr_47_cp_0 = instance.m_nodes.m_buffer;
                expr_47_cp_0[nodeID].m_flags = (expr_47_cp_0[nodeID].m_flags & ~NetNode.Flags.Transition);
                return;
            }
            ushort building = instance.m_nodes.m_buffer[nodeID].m_building;
            if (building != 0) {
                BuildingManager instance2 = Singleton<BuildingManager>.instance;
                if (instance2.m_buildings.m_buffer[building].m_waterBuffer != water) {
                    instance2.m_buildings.m_buffer[building].m_waterBuffer = (ushort)water;
                    flag = (currentMode == InfoManager.InfoMode.Water || currentMode == InfoManager.InfoMode.Heating);
                }
                if (instance2.m_buildings.m_buffer[building].m_sewageBuffer != sewage) {
                    instance2.m_buildings.m_buffer[building].m_sewageBuffer = (ushort)sewage;
                    flag = (currentMode == InfoManager.InfoMode.Water || currentMode == InfoManager.InfoMode.Heating);
                }
                if (instance2.m_buildings.m_buffer[building].m_heatingBuffer != heating) {
                    instance2.m_buildings.m_buffer[building].m_heatingBuffer = (ushort)heating;
                    flag = (currentMode == InfoManager.InfoMode.Water || currentMode == InfoManager.InfoMode.Heating);
                }
                if (flag) {
                    instance2.UpdateBuildingColors(building);
                }
            }
            NetNode.Flags flags2 = flags & ~(NetNode.Flags.Water | NetNode.Flags.Sewage | NetNode.Flags.Heating);
            if (water != 0) {
                flags2 |= NetNode.Flags.Water;
            }
            if (sewage != 0) {
                flags2 |= NetNode.Flags.Sewage;
            }
            if (heating != 0) {
                flags2 |= NetNode.Flags.Heating;
            }
            if (flags2 != flags) {
                instance.m_nodes.m_buffer[nodeID].m_flags = flags2;
                flag = (currentMode == InfoManager.InfoMode.Water || currentMode == InfoManager.InfoMode.Heating);
            }
            if (flag) {
                instance.UpdateNodeColors((ushort)nodeID);
                for (int i = 0; i < 8; i++) {
                    ushort segment = instance.m_nodes.m_buffer[nodeID].GetSegment(i);
                    if (segment != 0) {
                        instance.UpdateSegmentColors(segment);
                    }
                }
            }
        }

        internal static void UpdateWaterMapping(WaterManager wmInstance) {
            const float defZ = 1f / (DEFAULTGRID_RESOLUTION * WATERGRID_CELL_SIZE);
            const float Z = 1f / (WATERGRID_RESOLUTION * WATERGRID_CELL_SIZE);
            const float defW = 1f / DEFAULTGRID_RESOLUTION;
            const float W = 1f / WATERGRID_RESOLUTION;
            IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
                foreach (var code in instructions) {
                    if (code.LoadsConstant(defZ)) {
                        code.operand = Z;
                    } else if (code.LoadsConstant(defW)) {
                        code.operand = W;
                    }
                    yield return code;
                }
            }
            _ = Transpiler(null);
        }

        internal static void UpdateTexture(WaterManager.Cell[] waterGrid, ref int modifiedX1, ref int modifiedZ1, ref int modifiedX2, ref int modifiedZ2,
                                           bool waterMapVisible, Texture2D waterTexture) {
            while (!Monitor.TryEnter(waterGrid, SimulationManager.SYNCHRONIZE_TIMEOUT)) ;
            int tempX;
            int tempZ;
            int tempX2;
            int tempZ2;
            try {
                tempX = modifiedX1;
                tempZ = modifiedZ1;
                tempX2 = modifiedX2;
                tempZ2 = modifiedZ2;
                modifiedX1 = 10000;
                modifiedZ1 = 10000;
                modifiedX2 = -10000;
                modifiedZ2 = -10000;
            } finally {
                Monitor.Exit(waterGrid);
            }
            NetManager instance = Singleton<NetManager>.instance;
            NetSegment[] segments = instance.m_segments.m_buffer;
            NetNode[] nodes = instance.m_nodes.m_buffer;
            for (int i = tempZ; i <= tempZ2; i++) {
                for (int j = tempX; j <= tempX2; j++) {
                    WaterManager.Cell cell = waterGrid[i * WATERGRID_RESOLUTION + j];
                    Color color;
                    ushort segment = !waterMapVisible ? cell.m_closestPipeSegment2 : cell.m_closestPipeSegment;
                    if (segment != 0 && j != 0 && i != 0 && j != WATERGRID_RESOLUTION - 1 && i != WATERGRID_RESOLUTION - 1) {
                        ushort startNode = segments[segment].m_startNode;
                        ushort endNode = segments[segment].m_endNode;
                        Vector3 startPos = nodes[startNode].m_position;
                        Vector3 endPos = nodes[endNode].m_position;
                        const float offset = 16f;
                        const float halfOffset = 8f;
                        const float mult = offset / 38.25f;
                        const float makePositive = WATERGRID_RESOLUTION * offset / 2f;
                        color.r = EMath.Clamp((j * offset + halfOffset - (startPos.x * mult + makePositive) + 128f) * 0.0039215686f, 0f, 1f);
                        color.g = EMath.Clamp((i * offset + halfOffset - (startPos.z * mult + makePositive) + 128f) * 0.0039215686f, 0f, 1f);
                        color.b = EMath.Clamp((j * offset + halfOffset - (endPos.x * mult + makePositive) + 128f) * 0.0039215686f, 0f, 1f);
                        color.a = EMath.Clamp((i * offset + halfOffset - (endPos.z * mult + makePositive) + 128f) * 0.0039215686f, 0f, 1f);
                    } else {
                        color.r = 0f;
                        color.g = 0f;
                        color.b = 0f;
                        color.a = 0f;
                    }
                    waterTexture.SetPixel(j, i, color);
                }
            }
            waterTexture.Apply();
        }



        internal static void EnsureCapacity(WaterManager wmInstance, ref Texture2D waterTexture, ref int modifiedX2, ref int modifiedZ2) {
            if (modifiedX2 != WATERGRID_RESOLUTION - 1 && modifiedZ2 != WATERGRID_RESOLUTION - 1) {
                modifiedX2 = WATERGRID_RESOLUTION - 1;
                modifiedZ2 = WATERGRID_RESOLUTION - 1;
                waterTexture = new Texture2D(WATERGRID_RESOLUTION, WATERGRID_RESOLUTION, TextureFormat.RGBA32, false, true) {
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp
                };
                Shader.SetGlobalTexture("_WaterTexture", waterTexture);
                UpdateWaterMapping(wmInstance);
            }
            m_waterPulseUnits = new PulseUnit[DEFAULTGRID_RESOLUTION * DEFAULTGRID_RESOLUTION / 2];
            m_sewagePulseUnits = new PulseUnit[DEFAULTGRID_RESOLUTION * DEFAULTGRID_RESOLUTION / 2];
            m_heatingPulseUnits = new PulseUnit[DEFAULTGRID_RESOLUTION * DEFAULTGRID_RESOLUTION / 2];
        }

        internal static void IntegratedDeserialize(ref WaterManager.Cell[] waterGrid, ref WaterManager.PulseUnit[] waterPulseUnits,
                                                   ref WaterManager.PulseUnit[] sewagePulseUnits, ref WaterManager.PulseUnit[] heatingPulseUnits) {
            int i, j;
            const int diff = (WATERGRID_RESOLUTION - DEFAULTGRID_RESOLUTION) / 2;
            WaterManager.Cell[] newWaterGrid = new WaterManager.Cell[WATERGRID_RESOLUTION * WATERGRID_RESOLUTION];
            for (i = 0; i < DEFAULTGRID_RESOLUTION; i++) {
                for (j = 0; j < DEFAULTGRID_RESOLUTION; j++) {
                    newWaterGrid[(j + diff) * WATERGRID_RESOLUTION + (i + diff)] = waterGrid[j * DEFAULTGRID_RESOLUTION + i];
                }
            }
            PulseUnit[] newWaterPulseUnits = m_waterPulseUnits;
            PulseUnit[] newSewagePulseUnits = m_sewagePulseUnits;
            PulseUnit[] newHeatingPulseUnits = m_heatingPulseUnits;
            for (i = 0; i < (DEFAULTGRID_RESOLUTION * DEFAULTGRID_RESOLUTION / 2); i++) {
                newWaterPulseUnits[i].m_group = waterPulseUnits[i].m_group;
                newWaterPulseUnits[i].m_node = waterPulseUnits[i].m_node;
                newWaterPulseUnits[i].m_x = waterPulseUnits[i].m_x;
                newWaterPulseUnits[i].m_z = waterPulseUnits[i].m_z;
                newSewagePulseUnits[i].m_group = sewagePulseUnits[i].m_group;
                newSewagePulseUnits[i].m_node = sewagePulseUnits[i].m_node;
                newSewagePulseUnits[i].m_x = sewagePulseUnits[i].m_x;
                newSewagePulseUnits[i].m_z = sewagePulseUnits[i].m_z;
                newHeatingPulseUnits[i].m_group = heatingPulseUnits[i].m_group;
                newHeatingPulseUnits[i].m_node = heatingPulseUnits[i].m_node;
                newHeatingPulseUnits[i].m_x = heatingPulseUnits[i].m_x;
                newHeatingPulseUnits[i].m_z = heatingPulseUnits[i].m_z;
            }
            waterGrid = newWaterGrid;
        }

        internal static WaterManager.Cell[] IntegratedSerialize(WaterManager.Cell[] waterGrid, ref WaterManager.PulseUnit[] waterPulseUnits,
                                                ref WaterManager.PulseUnit[] sewagePulseUnits, ref WaterManager.PulseUnit[] heatingPulseUnits) {
            int i, j;
            const int diff = (WATERGRID_RESOLUTION - DEFAULTGRID_RESOLUTION) / 2;
            WaterManager.Cell[] defWaterGrid = new WaterManager.Cell[DEFAULTGRID_RESOLUTION * DEFAULTGRID_RESOLUTION];
            for (i = 0; i < DEFAULTGRID_RESOLUTION; i++) {
                for (j = 0; j < DEFAULTGRID_RESOLUTION; j++) {
                    defWaterGrid[j * DEFAULTGRID_RESOLUTION + i] = waterGrid[(j + diff) * WATERGRID_RESOLUTION + (i + diff)];
                }
            }
            PulseUnit[] newWaterPulseUnits = m_waterPulseUnits;
            PulseUnit[] newSewagePulseUnits = m_sewagePulseUnits;
            PulseUnit[] newHeatingPulseUnits = m_heatingPulseUnits;
            for (i = 0; i < (DEFAULTGRID_RESOLUTION * DEFAULTGRID_RESOLUTION / 2); i++) {
                waterPulseUnits[i].m_group = newWaterPulseUnits[i].m_group;
                waterPulseUnits[i].m_node = newWaterPulseUnits[i].m_node;
                waterPulseUnits[i].m_x = (byte)EMath.Clamp(newWaterPulseUnits[i].m_x, 0, 255);
                waterPulseUnits[i].m_z = (byte)EMath.Clamp(newWaterPulseUnits[i].m_z, 0, 255);
                sewagePulseUnits[i].m_group = newSewagePulseUnits[i].m_group;
                sewagePulseUnits[i].m_node = newSewagePulseUnits[i].m_node;
                sewagePulseUnits[i].m_x = (byte)EMath.Clamp(newSewagePulseUnits[i].m_x, 0, 255);
                sewagePulseUnits[i].m_z = (byte)EMath.Clamp(newSewagePulseUnits[i].m_z, 0, 255);
                heatingPulseUnits[i].m_group = newHeatingPulseUnits[i].m_group;
                heatingPulseUnits[i].m_node = newHeatingPulseUnits[i].m_node;
                heatingPulseUnits[i].m_x = (byte)EMath.Clamp(newHeatingPulseUnits[i].m_x, 0, 255);
                heatingPulseUnits[i].m_z = (byte)EMath.Clamp(newHeatingPulseUnits[i].m_z, 0, 255);
            }
            return defWaterGrid;
        }
    }
}
