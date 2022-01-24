using ColossalFramework;
using ColossalFramework.IO;
using ColossalFramework.Math;
using EManagersLib.LegacyDataHandlers.EightyOneTiles;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

namespace EManagersLib {
    internal static class EWaterManager {
        private const string WATERMANAGERDATAKEY = "fakeWM";
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

        internal static WaterManager.Cell[] m_waterGrid;
        internal static PulseUnit[] m_waterPulseUnits;
        internal static PulseUnit[] m_sewagePulseUnits;
        internal static PulseUnit[] m_heatingPulseUnits;

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
                NetSegment[] segments = instance.m_segments.m_buffer;
                NetNode[] nodes = instance.m_nodes.m_buffer;
                ushort closestPipeSegment = cell.m_closestPipeSegment2;
                ushort startNode = segments[closestPipeSegment].m_startNode;
                ushort endNode = segments[closestPipeSegment].m_endNode;
                NetNode.Flags flags = nodes[startNode].m_flags;
                NetNode.Flags flags2 = nodes[endNode].m_flags;
                if ((flags & flags2 & NetNode.Flags.Heating) != NetNode.Flags.None) {
                    Segment2 segment;
                    segment.a = VectorUtils.XZ(nodes[startNode].m_position);
                    segment.b = VectorUtils.XZ(nodes[endNode].m_position);
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

        internal static void ConductHeatingToCell(PulseUnit[] pulseUnits, ref int heatingPulseUnitEnd, ref bool canContinue, ref WaterManager.Cell cell, ushort group, int x, int z) {
            if (cell.m_conductivity2 >= 96 && cell.m_heatingPulseGroup == 65535) {
                ref PulseUnit pulseUnit = ref pulseUnits[heatingPulseUnitEnd];
                pulseUnit.m_group = group;
                pulseUnit.m_node = 0;
                pulseUnit.m_x = (ushort)x;
                pulseUnit.m_z = (ushort)z;
                if (++heatingPulseUnitEnd == m_heatingPulseUnits.Length) {
                    heatingPulseUnitEnd = 0;
                }
                cell.m_heatingPulseGroup = group;
                canContinue = true;
            }
        }

        internal static void ConductHeatingToCells(WaterManager.Cell[] waterGrid, ref int heatingPulseUnitEnd, ref bool canContinue, ushort group, float worldX, float worldZ, float radius) {
            const float halfGrid = WATERGRID_RESOLUTION / 2f;
            PulseUnit[] pulseUnits = m_heatingPulseUnits;
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
                        ConductHeatingToCell(pulseUnits, ref heatingPulseUnitEnd, ref canContinue, ref waterGrid[i * WATERGRID_RESOLUTION + j], group, j, i);
                    }
                }
            }
        }

        internal static void ConductHeatingToNode(WaterManager.Node[] nodeData, WaterManager.PulseGroup[] heatingPulseGroups, int heatingPulseGroupCount, ref int heatingPulseUnitEnd, ref bool canContinue, ushort nodeIndex, ref NetNode node, ushort group) {
            NetInfo info = node.Info;
            if (info.m_class.m_service == ItemClass.Service.Water && info.m_class.m_level == ItemClass.Level.Level2) {
                if (nodeData[nodeIndex].m_heatingPulseGroup == 65535) {
                    ref PulseUnit pulseUnit = ref m_heatingPulseUnits[heatingPulseUnitEnd];
                    pulseUnit.m_group = group;
                    pulseUnit.m_node = nodeIndex;
                    pulseUnit.m_x = 0;
                    pulseUnit.m_z = 0;
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

        internal static void ConductSewageToCell(PulseUnit[] pulseUnits, ref int sewagePulseUnitEnd, ref bool canContinue, ref WaterManager.Cell cell, ushort group, int x, int z) {
            if (cell.m_conductivity >= 96 && cell.m_sewagePulseGroup == 65535) {
                ref PulseUnit pulseUnit = ref pulseUnits[sewagePulseUnitEnd];
                pulseUnit.m_group = group;
                pulseUnit.m_node = 0;
                pulseUnit.m_x = (ushort)x;
                pulseUnit.m_z = (ushort)z;
                if (++sewagePulseUnitEnd == m_sewagePulseUnits.Length) {
                    sewagePulseUnitEnd = 0;
                }
                cell.m_sewagePulseGroup = group;
                canContinue = true;
            }
        }

        internal static void ConductSewageToCells(WaterManager.Cell[] waterGrid, ref int sewagePulseUnitEnd, ref bool canContinue, ushort group, float worldX, float worldZ, float radius) {
            const float halfGrid = WATERGRID_RESOLUTION / 2f;
            PulseUnit[] pulseUnits = m_sewagePulseUnits;
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
                        ConductSewageToCell(pulseUnits, ref sewagePulseUnitEnd, ref canContinue, ref waterGrid[i * WATERGRID_RESOLUTION + j], group, j, i);
                    }
                }
            }
        }

        internal static void ConductSewageToNode(WaterManager.Node[] nodeData, WaterManager.PulseGroup[] sewagePulseGroups, ref int sewagePulseUnitEnd, ref bool canContinue, int sewagePulseGroupCount, ushort nodeIndex, ref NetNode node, ushort group) {
            NetInfo info = node.Info;
            if (info.m_class.m_service == ItemClass.Service.Water && info.m_class.m_level <= ItemClass.Level.Level2) {
                if (nodeData[nodeIndex].m_sewagePulseGroup == 65535) {
                    ref PulseUnit pulseUnit = ref m_sewagePulseUnits[sewagePulseUnitEnd];
                    pulseUnit.m_group = group;
                    pulseUnit.m_node = nodeIndex;
                    pulseUnit.m_x = 0;
                    pulseUnit.m_z = 0;
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
                            ref PulseUnit pulseUnit = ref m_sewagePulseUnits[sewagePulseUnitEnd];
                            pulseUnit.m_group = group;
                            pulseUnit.m_node = nodeIndex;
                            pulseUnit.m_x = 0;
                            pulseUnit.m_z = 0;
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

        internal unsafe static void ConductWaterToCell(PulseUnit[] pulseUnits, ref int waterPulseUnitEnd, ref bool canContinue, ref WaterManager.Cell cell, ushort group, int x, int z) {
            if (cell.m_conductivity >= 96 && cell.m_waterPulseGroup == 65535) {
                ref PulseUnit pulseUnit = ref pulseUnits[waterPulseUnitEnd];
                pulseUnit.m_group = group;
                pulseUnit.m_node = 0;
                pulseUnit.m_x = (ushort)x;
                pulseUnit.m_z = (ushort)z;
                if (++waterPulseUnitEnd == m_waterPulseUnits.Length) {
                    waterPulseUnitEnd = 0;
                }
                cell.m_waterPulseGroup = group;
                canContinue = true;
            }
        }

        internal static void ConductWaterToCells(WaterManager.Cell[] waterGrid, ref int waterPulseUnitEnd, ref bool canContinue, ushort group, float worldX, float worldZ, float radius) {
            const float halfGrid = WATERGRID_RESOLUTION / 2f;
            PulseUnit[] pulseUnits = m_waterPulseUnits;
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
                        ConductWaterToCell(pulseUnits, ref waterPulseUnitEnd, ref canContinue, ref waterGrid[i * WATERGRID_RESOLUTION + j], group, j, i);
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
            int i, j;
            PulseUnit[] waterPulseUnits = m_waterPulseUnits;
            PulseUnit[] sewagePulseUnits = m_sewagePulseUnits;
            PulseUnit[] heatingPulseUnits = m_heatingPulseUnits;
            NetManager nmInstance = Singleton<NetManager>.instance;
            BuildingManager bmInstance = Singleton<BuildingManager>.instance;
            NetNode[] nodes = nmInstance.m_nodes.m_buffer;
            NetSegment[] segments = nmInstance.m_segments.m_buffer;
            Building[] buildings = bmInstance.m_buildings.m_buffer;
            if (subStep != 0 && subStep != 1000) {
                uint currentFrameIndex = Singleton<SimulationManager>.instance.m_currentFrameIndex;
                int essentialFrame = (int)(currentFrameIndex & 0xffu);
                if (essentialFrame < 128) {
                    if (essentialFrame == 0) {
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
                    int startFrame = essentialFrame * 0x8000 >> 7;
                    int endFrame = ((essentialFrame + 1) * 0x8000 >> 7) - 1;
                    for (i = startFrame; i <= endFrame; i++) {
                        ref WaterManager.Node node = ref nodeData[i];
                        NetNode.Flags flags = nodes[i].m_flags;
                        if (flags != NetNode.Flags.None) {
                            NetInfo info = nodes[i].Info;
                            if (info.m_class.m_service == ItemClass.Service.Water && info.m_class.m_level <= ItemClass.Level.Level2) {
                                int water = (node.m_waterPulseGroup == 0xffff) ? 0 : 1;
                                int sewage = (node.m_sewagePulseGroup == 0xffff) ? 0 : 1;
                                int heating = (node.m_heatingPulseGroup == 0xffff) ? 0 : 1;
                                UpdateNodeWater(bmInstance, buildings, nmInstance, nodes, i, water, sewage, heating);
                                conductiveCells += 2;
                                node.m_waterPulseGroup = 0xffff;
                                node.m_sewagePulseGroup = 0xffff;
                                node.m_heatingPulseGroup = 0xffff;
                                if ((node.m_curWaterPressure != 0 || node.m_collectWaterPressure != 0) && waterPulseGroupCount < 1024) {
                                    ref WaterManager.PulseGroup pulseGroup = ref waterPulseGroups[waterPulseGroupCount];
                                    pulseGroup.m_origPressure = node.m_curWaterPressure;
                                    pulseGroup.m_curPressure = node.m_curWaterPressure;
                                    pulseGroup.m_collectPressure = node.m_collectWaterPressure;
                                    pulseGroup.m_mergeCount = 0;
                                    pulseGroup.m_mergeIndex = 0xffff;
                                    pulseGroup.m_node = (ushort)i;
                                    node.m_waterPulseGroup = (ushort)waterPulseGroupCount;
                                    waterPulseGroupCount++;
                                    if (pulseGroup.m_origPressure != 0u) {
                                        ref PulseUnit pulseUnit = ref waterPulseUnits[waterPulseUnitEnd];
                                        pulseUnit.m_group = (ushort)(waterPulseGroupCount - 1);
                                        pulseUnit.m_node = (ushort)i;
                                        pulseUnit.m_x = 0;
                                        pulseUnit.m_z = 0;
                                        if (++waterPulseUnitEnd == waterPulseUnits.Length) {
                                            waterPulseUnitEnd = 0;
                                        }
                                    }
                                }
                                if ((node.m_curSewagePressure != 0 || node.m_collectSewagePressure != 0) && sewagePulseGroupCount < 1024) {
                                    ref WaterManager.PulseGroup pulseGroup = ref sewagePulseGroups[sewagePulseGroupCount];
                                    pulseGroup.m_origPressure = node.m_curSewagePressure;
                                    pulseGroup.m_curPressure = node.m_curSewagePressure;
                                    pulseGroup.m_collectPressure = node.m_collectSewagePressure;
                                    pulseGroup.m_mergeCount = 0;
                                    pulseGroup.m_mergeIndex = 0xffff;
                                    pulseGroup.m_node = (ushort)i;
                                    node.m_sewagePulseGroup = (ushort)sewagePulseGroupCount;
                                    sewagePulseGroupCount++;
                                    if (pulseGroup.m_origPressure != 0u) {
                                        ref PulseUnit pulseUnit = ref m_sewagePulseUnits[sewagePulseUnitEnd];
                                        pulseUnit.m_group = (ushort)(sewagePulseGroupCount - 1);
                                        pulseUnit.m_node = (ushort)i;
                                        pulseUnit.m_x = 0;
                                        pulseUnit.m_z = 0;
                                        if (++sewagePulseUnitEnd == m_sewagePulseUnits.Length) {
                                            sewagePulseUnitEnd = 0;
                                        }
                                    }
                                }
                                if (node.m_curHeatingPressure != 0 && heatingPulseGroupCount < 1024) {
                                    ref WaterManager.PulseGroup pulseGroup = ref heatingPulseGroups[heatingPulseGroupCount];
                                    pulseGroup.m_origPressure = node.m_curHeatingPressure;
                                    pulseGroup.m_curPressure = node.m_curHeatingPressure;
                                    pulseGroup.m_collectPressure = 0u;
                                    pulseGroup.m_mergeCount = 0;
                                    pulseGroup.m_mergeIndex = 0xffff;
                                    pulseGroup.m_node = (ushort)i;
                                    ref PulseUnit pulseUnit = ref m_heatingPulseUnits[heatingPulseUnitEnd];
                                    pulseUnit.m_group = (ushort)heatingPulseGroupCount;
                                    pulseUnit.m_node = (ushort)i;
                                    pulseUnit.m_x = 0;
                                    pulseUnit.m_z = 0;
                                    node.m_heatingPulseGroup = (ushort)heatingPulseGroupCount;
                                    heatingPulseGroupCount++;
                                    if (++heatingPulseUnitEnd == m_heatingPulseUnits.Length) {
                                        heatingPulseUnitEnd = 0;
                                    }
                                }
                            } else {
                                node.m_waterPulseGroup = 0xffff;
                                node.m_sewagePulseGroup = 0xffff;
                                node.m_heatingPulseGroup = 0xffff;
                                node.m_extraWaterPressure = 0;
                                node.m_extraSewagePressure = 0;
                                node.m_extraHeatingPressure = 0;
                            }
                        } else {
                            node.m_waterPulseGroup = 0xffff;
                            node.m_sewagePulseGroup = 0xffff;
                            node.m_heatingPulseGroup = 0xffff;
                            node.m_extraWaterPressure = 0;
                            node.m_extraSewagePressure = 0;
                            node.m_extraHeatingPressure = 0;
                        }
                        node.m_curWaterPressure = 0;
                        node.m_curSewagePressure = 0;
                        node.m_curHeatingPressure = 0;
                        node.m_collectWaterPressure = 0;
                        node.m_collectSewagePressure = 0;
                    }
                    startFrame = essentialFrame * WATERGRID_RESOLUTION >> 7;
                    endFrame = ((essentialFrame + 1) * WATERGRID_RESOLUTION >> 7) - 1;
                    for (i = startFrame; i <= endFrame; i++) {
                        int gridIndex = i * WATERGRID_RESOLUTION;
                        for (j = 0; j < WATERGRID_RESOLUTION; j++) {
                            ref WaterManager.Cell cell = ref waterGrid[gridIndex];
                            cell.m_waterPulseGroup = 0xffff;
                            cell.m_sewagePulseGroup = 0xffff;
                            cell.m_heatingPulseGroup = 0xffff;
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
                            gridIndex++;
                        }
                    }
                } else {
                    PulseUnit pulseUnit;
                    uint waterPressure;
                    int finalWaterPressure;
                    int maxCells = (essentialFrame - 127) * conductiveCells >> 7;
                    if (essentialFrame == 255) {
                        maxCells = 1000000000;
                    }
                    while (canContinue && processedCells < maxCells) {
                        canContinue = false;
                        int __waterPulseUnitEnd = waterPulseUnitEnd;
                        int __sewagePulseUnitEnd = sewagePulseUnitEnd;
                        int __heatingPulseUnitEnd = heatingPulseUnitEnd;
                        while (waterPulseUnitStart != __waterPulseUnitEnd) {
                            pulseUnit = waterPulseUnits[waterPulseUnitStart];
                            if (++waterPulseUnitStart == waterPulseUnits.Length) {
                                waterPulseUnitStart = 0;
                            }
                            pulseUnit.m_group = GetRootWaterGroup(waterPulseGroups, pulseUnit.m_group);
                            waterPressure = waterPulseGroups[pulseUnit.m_group].m_curPressure;
                            if (pulseUnit.m_node == 0) {
                                ref WaterManager.Cell cell = ref waterGrid[pulseUnit.m_z * WATERGRID_RESOLUTION + pulseUnit.m_x];
                                if (cell.m_conductivity != 0 && !cell.m_tmpHasWater && waterPressure != 0u) {
                                    finalWaterPressure = EMath.Clamp((-cell.m_currentWaterPressure), 0, (int)waterPressure);
                                    waterPressure -= (uint)finalWaterPressure;
                                    cell.m_currentWaterPressure += (short)finalWaterPressure;
                                    if (cell.m_currentWaterPressure >= 0) {
                                        cell.m_tmpHasWater = true;
                                        cell.m_pollution = nodeData[waterPulseGroups[pulseUnit.m_group].m_node].m_pollution;
                                    }
                                    waterPulseGroups[pulseUnit.m_group].m_curPressure = waterPressure;
                                }
                                if (waterPressure != 0u) {
                                    processedCells++;
                                } else {
                                    waterPulseUnits[waterPulseUnitEnd] = pulseUnit;
                                    if (++waterPulseUnitEnd == waterPulseUnits.Length) {
                                        waterPulseUnitEnd = 0;
                                    }
                                }
                            } else if (waterPressure != 0u) {
                                processedCells++;
                                ref NetNode netNode = ref nodes[pulseUnit.m_node];
                                if (netNode.m_flags != NetNode.Flags.None && netNode.m_buildIndex < (currentFrameIndex & 4294967168u)) {
                                    byte pollution = nodeData[waterPulseGroups[pulseUnit.m_group].m_node].m_pollution;
                                    nodeData[pulseUnit.m_node].m_pollution = pollution;
                                    if (netNode.m_building != 0) {
                                        buildings[netNode.m_building].m_waterPollution = pollution;
                                    }
                                    ConductWaterToCells(waterGrid, ref waterPulseUnitEnd, ref canContinue, pulseUnit.m_group, netNode.m_position.x, netNode.m_position.z, 100f);
                                    for (int l = 0; l < 8; l++) {
                                        ushort segment = netNode.GetSegment(l);
                                        if (segment != 0) {
                                            ushort startNode = segments[segment].m_startNode;
                                            ushort endNode = segments[segment].m_endNode;
                                            ushort nodeIndex = (startNode != pulseUnit.m_node) ? startNode : endNode;
                                            ConductWaterToNode(nodeData, waterPulseGroups, ref waterPulseUnitEnd, ref canContinue, waterPulseGroupCount, nodeIndex, ref nodes[nodeIndex], pulseUnit.m_group);
                                        }
                                    }
                                }
                            } else {
                                waterPulseUnits[waterPulseUnitEnd] = pulseUnit;
                                if (++waterPulseUnitEnd == m_waterPulseUnits.Length) {
                                    waterPulseUnitEnd = 0;
                                }
                            }
                        }
                        while (sewagePulseUnitStart != __sewagePulseUnitEnd) {
                            pulseUnit = sewagePulseUnits[sewagePulseUnitStart];
                            if (++sewagePulseUnitStart == sewagePulseUnits.Length) {
                                sewagePulseUnitStart = 0;
                            }
                            pulseUnit.m_group = GetRootSewageGroup(sewagePulseGroups, pulseUnit.m_group);
                            waterPressure = sewagePulseGroups[pulseUnit.m_group].m_curPressure;
                            if (pulseUnit.m_node == 0) {
                                ref WaterManager.Cell cell = ref waterGrid[pulseUnit.m_z * WATERGRID_RESOLUTION + pulseUnit.m_x];
                                if (cell.m_conductivity != 0 && !cell.m_tmpHasSewage && waterPressure != 0u) {
                                    finalWaterPressure = EMath.Clamp((-cell.m_currentSewagePressure), 0, (int)waterPressure);
                                    waterPressure -= (uint)finalWaterPressure;
                                    cell.m_currentSewagePressure += (short)finalWaterPressure;
                                    if (cell.m_currentSewagePressure >= 0) {
                                        cell.m_tmpHasSewage = true;
                                    }
                                    sewagePulseGroups[pulseUnit.m_group].m_curPressure = waterPressure;
                                }
                                if (waterPressure != 0u) {
                                    processedCells++;
                                } else {
                                    sewagePulseUnits[sewagePulseUnitEnd] = pulseUnit;
                                    if (++sewagePulseUnitEnd == sewagePulseUnits.Length) {
                                        sewagePulseUnitEnd = 0;
                                    }
                                }
                            } else if (waterPressure != 0u) {
                                processedCells++;
                                ref NetNode netNode = ref nodes[pulseUnit.m_node];
                                if (netNode.m_flags != NetNode.Flags.None && netNode.m_buildIndex < (currentFrameIndex & 4294967168u)) {
                                    ConductSewageToCells(waterGrid, ref sewagePulseUnitEnd, ref canContinue, pulseUnit.m_group, netNode.m_position.x, netNode.m_position.z, 100f);
                                    for (int m = 0; m < 8; m++) {
                                        ushort segment = netNode.GetSegment(m);
                                        if (segment != 0) {
                                            ushort startNode2 = segments[segment].m_startNode;
                                            ushort endNode2 = segments[segment].m_endNode;
                                            ushort nodeIndex = (startNode2 != pulseUnit.m_node) ? startNode2 : endNode2;
                                            ConductSewageToNode(nodeData, sewagePulseGroups, ref sewagePulseUnitEnd, ref canContinue, sewagePulseGroupCount, nodeIndex, ref nodes[nodeIndex], pulseUnit.m_group);
                                        }
                                    }
                                }
                            } else {
                                sewagePulseUnits[sewagePulseUnitEnd] = pulseUnit;
                                if (++sewagePulseUnitEnd == sewagePulseUnits.Length) {
                                    sewagePulseUnitEnd = 0;
                                }
                            }
                        }
                        while (heatingPulseUnitStart != __heatingPulseUnitEnd) {
                            pulseUnit = heatingPulseUnits[heatingPulseUnitStart];
                            if (++heatingPulseUnitStart == heatingPulseUnits.Length) {
                                heatingPulseUnitStart = 0;
                            }
                            pulseUnit.m_group = GetRootHeatingGroup(heatingPulseGroups, pulseUnit.m_group);
                            waterPressure = heatingPulseGroups[pulseUnit.m_group].m_curPressure;
                            if (pulseUnit.m_node == 0) {
                                ref WaterManager.Cell cell = ref waterGrid[pulseUnit.m_z * WATERGRID_RESOLUTION + pulseUnit.m_x];
                                if (cell.m_conductivity2 != 0 && !cell.m_tmpHasHeating && waterPressure != 0u) {
                                    int finalPressure = EMath.Clamp((-cell.m_currentHeatingPressure), 0, (int)waterPressure);
                                    waterPressure -= (uint)finalPressure;
                                    cell.m_currentHeatingPressure += (short)finalPressure;
                                    if (cell.m_currentHeatingPressure >= 0) {
                                        cell.m_tmpHasHeating = true;
                                    }
                                    heatingPulseGroups[pulseUnit.m_group].m_curPressure = waterPressure;
                                }
                                if (waterPressure != 0u) {
                                    processedCells++;
                                } else {
                                    heatingPulseUnits[heatingPulseUnitEnd] = pulseUnit;
                                    if (++heatingPulseUnitEnd == heatingPulseUnits.Length) {
                                        heatingPulseUnitEnd = 0;
                                    }
                                }
                            } else if (waterPressure != 0u) {
                                processedCells++;
                                ref NetNode netNode = ref nodes[pulseUnit.m_node];
                                if (netNode.m_flags != NetNode.Flags.None && netNode.m_buildIndex < (currentFrameIndex & 4294967168u)) {
                                    ConductHeatingToCells(waterGrid, ref heatingPulseUnitEnd, ref canContinue, pulseUnit.m_group, netNode.m_position.x, netNode.m_position.z, 100f);
                                    for (i = 0; i < 8; i++) {
                                        ushort segment = netNode.GetSegment(i);
                                        if (segment != 0) {
                                            NetInfo info = segments[segment].Info;
                                            if (info.m_class.m_service == ItemClass.Service.Water && info.m_class.m_level == ItemClass.Level.Level2) {
                                                ushort startNode3 = segments[segment].m_startNode;
                                                ushort endNode3 = segments[segment].m_endNode;
                                                ushort nodeIndex = (startNode3 != pulseUnit.m_node) ? startNode3 : endNode3;
                                                ConductHeatingToNode(nodeData, heatingPulseGroups, heatingPulseGroupCount, ref heatingPulseUnitEnd, ref canContinue, nodeIndex, ref nodes[nodeIndex], pulseUnit.m_group);
                                            }
                                        }
                                    }
                                }
                            } else {
                                heatingPulseUnits[heatingPulseUnitEnd] = pulseUnit;
                                if (++heatingPulseUnitEnd == heatingPulseUnits.Length) {
                                    heatingPulseUnitEnd = 0;
                                }
                            }
                        }
                    }
                    if (essentialFrame == 255) {
                        for (i = 0; i < waterPulseGroupCount; i++) {
                            ref WaterManager.PulseGroup pulseGroup = ref waterPulseGroups[i];
                            if (pulseGroup.m_mergeIndex != 0xffff && pulseGroup.m_collectPressure != 0u) {
                                ref WaterManager.PulseGroup pulseGroup5 = ref waterPulseGroups[pulseGroup.m_mergeIndex];
                                pulseGroup.m_curPressure = pulseGroup5.m_curPressure * pulseGroup.m_collectPressure / pulseGroup5.m_collectPressure;
                                if (pulseGroup.m_collectPressure < pulseGroup.m_curPressure) {
                                    pulseGroup.m_curPressure = pulseGroup.m_collectPressure;
                                }
                                pulseGroup5.m_curPressure -= pulseGroup.m_curPressure;
                                pulseGroup5.m_collectPressure -= pulseGroup.m_collectPressure;
                            }
                        }
                        for (i = 0; i < waterPulseGroupCount; i++) {
                            ref WaterManager.PulseGroup pulseGroup = ref waterPulseGroups[i];
                            if (pulseGroup.m_mergeIndex != 0xffff && pulseGroup.m_collectPressure == 0u) {
                                ref WaterManager.PulseGroup pulseGroup7 = ref waterPulseGroups[pulseGroup.m_mergeIndex];
                                uint num22 = pulseGroup7.m_curPressure;
                                if (pulseGroup7.m_collectPressure >= num22) {
                                    num22 = 0u;
                                } else {
                                    num22 -= pulseGroup7.m_collectPressure;
                                }
                                pulseGroup.m_curPressure = num22 * pulseGroup.m_origPressure / pulseGroup7.m_origPressure;
                                pulseGroup7.m_curPressure -= pulseGroup.m_curPressure;
                                pulseGroup7.m_origPressure -= pulseGroup.m_origPressure;
                            }
                        }
                        for (i = 0; i < waterPulseGroupCount; i++) {
                            ref WaterManager.PulseGroup pulseGroup = ref waterPulseGroups[i];
                            if (pulseGroup.m_curPressure != 0u) {
                                WaterManager.Node node2 = nodeData[pulseGroup.m_node];
                                node2.m_extraWaterPressure += (ushort)EMath.Min((int)pulseGroup.m_curPressure, (32767 - node2.m_extraWaterPressure));
                                nodeData[pulseGroup.m_node] = node2;
                            }
                        }
                        for (i = 0; i < sewagePulseGroupCount; i++) {
                            ref WaterManager.PulseGroup pulseGroup = ref sewagePulseGroups[i];
                            if (pulseGroup.m_mergeIndex != 0xffff && pulseGroup.m_collectPressure != 0u) {
                                ref WaterManager.PulseGroup pulseGroup10 = ref sewagePulseGroups[pulseGroup.m_mergeIndex];
                                pulseGroup.m_curPressure = pulseGroup10.m_curPressure * pulseGroup.m_collectPressure / pulseGroup10.m_collectPressure;
                                if (pulseGroup.m_collectPressure < pulseGroup.m_curPressure) {
                                    pulseGroup.m_curPressure = pulseGroup.m_collectPressure;
                                }
                                pulseGroup10.m_curPressure -= pulseGroup.m_curPressure;
                                pulseGroup10.m_collectPressure -= pulseGroup.m_collectPressure;
                            }
                        }
                        for (i = 0; i < sewagePulseGroupCount; i++) {
                            ref WaterManager.PulseGroup pulseGroup = ref sewagePulseGroups[i];
                            if (pulseGroup.m_mergeIndex != 0xffff && pulseGroup.m_collectPressure == 0u) {
                                ref WaterManager.PulseGroup pulseGroup12 = ref sewagePulseGroups[pulseGroup.m_mergeIndex];
                                uint num26 = pulseGroup12.m_curPressure;
                                if (pulseGroup12.m_collectPressure < num26) {
                                    num26 -= pulseGroup12.m_collectPressure;
                                }
                                pulseGroup.m_curPressure = pulseGroup12.m_curPressure * pulseGroup.m_origPressure / pulseGroup12.m_origPressure;
                                pulseGroup12.m_curPressure -= pulseGroup.m_curPressure;
                                pulseGroup12.m_origPressure -= pulseGroup.m_origPressure;
                            }
                        }
                        for (i = 0; i < sewagePulseGroupCount; i++) {
                            ref WaterManager.PulseGroup pulseGroup = ref sewagePulseGroups[i];
                            if (pulseGroup.m_curPressure != 0u) {
                                WaterManager.Node node3 = nodeData[pulseGroup.m_node];
                                node3.m_extraSewagePressure += (ushort)EMath.Min((int)pulseGroup.m_curPressure, (32767 - node3.m_extraSewagePressure));
                                nodeData[pulseGroup.m_node] = node3;
                            }
                        }
                        for (i = 0; i < heatingPulseGroupCount; i++) {
                            ref WaterManager.PulseGroup pulseGroup = ref heatingPulseGroups[i];
                            if (pulseGroup.m_mergeIndex != 0xffff) {
                                ref WaterManager.PulseGroup pulseGroup15 = ref heatingPulseGroups[pulseGroup.m_mergeIndex];
                                pulseGroup.m_curPressure = pulseGroup15.m_curPressure * pulseGroup.m_origPressure / pulseGroup15.m_origPressure;
                                pulseGroup15.m_curPressure -= pulseGroup.m_curPressure;
                                pulseGroup15.m_origPressure -= pulseGroup.m_origPressure;
                            }
                        }
                        for (i = 0; i < heatingPulseGroupCount; i++) {
                            ref WaterManager.PulseGroup pulseGroup = ref heatingPulseGroups[i];
                            if (pulseGroup.m_curPressure != 0u) {
                                WaterManager.Node node4 = nodeData[pulseGroup.m_node];
                                node4.m_extraHeatingPressure += (ushort)EMath.Min((int)pulseGroup.m_curPressure, (32767 - node4.m_extraHeatingPressure));
                                nodeData[pulseGroup.m_node] = node4;
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
            int i, j;
            const float halfGrid = WATERGRID_RESOLUTION / 2f;
            int startX = EMath.Max((int)(minX / WATERGRID_CELL_SIZE + halfGrid), 0);
            int startZ = EMath.Max((int)(minZ / WATERGRID_CELL_SIZE + halfGrid), 0);
            int endX = EMath.Min((int)(maxX / WATERGRID_CELL_SIZE + halfGrid), WATERGRID_RESOLUTION - 1);
            int endZ = EMath.Min((int)(maxZ / WATERGRID_CELL_SIZE + halfGrid), WATERGRID_RESOLUTION - 1);
            for (i = startZ; i <= endZ; i++) {
                int gridID = i * WATERGRID_RESOLUTION + startX;
                for (j = startX; j <= endX; j++) {
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
            for (i = num11; i <= num13; i++) {
                for (j = num10; j <= num12; j++) {
                    ushort segmentID = segmentGrid[i * 270 + j];
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
            for (i = startZ; i <= endZ; i++) {
                int gridIndex = i * WATERGRID_RESOLUTION + startX;
                for (j = startX; j <= endX; j++) {
                    ref WaterManager.Cell cell = ref waterGrid[gridIndex];
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
                    } else if (cell.m_conductivity2 == 0) {
                        cell.m_currentHeatingPressure = 0;
                        cell.m_heatingPulseGroup = 65535;
                        cell.m_tmpHasHeating = false;
                        cell.m_hasHeating = false;
                    }
                    gridIndex++;
                }
            }
            wmInstance.AreaModified(startX, startZ, endX, endZ);
        }

        private static void UpdateNodeWater(BuildingManager bmInstance, Building[] buildings, NetManager nmInstance, NetNode[] nodes, int nodeID, int water, int sewage, int heating) {
            InfoManager.InfoMode currentMode = Singleton<InfoManager>.instance.CurrentMode;
            bool flag = false;
            NetNode.Flags flags = nodes[nodeID].m_flags;
            if ((flags & NetNode.Flags.Transition) != NetNode.Flags.None) {
                nodes[nodeID].m_flags &= ~NetNode.Flags.Transition;
                return;
            }
            ushort building = nodes[nodeID].m_building;
            if (building != 0) {
                if (buildings[building].m_waterBuffer != water) {
                    buildings[building].m_waterBuffer = (ushort)water;
                    flag = (currentMode == InfoManager.InfoMode.Water || currentMode == InfoManager.InfoMode.Heating);
                }
                if (buildings[building].m_sewageBuffer != sewage) {
                    buildings[building].m_sewageBuffer = (ushort)sewage;
                    flag = (currentMode == InfoManager.InfoMode.Water || currentMode == InfoManager.InfoMode.Heating);
                }
                if (buildings[building].m_heatingBuffer != heating) {
                    buildings[building].m_heatingBuffer = (ushort)heating;
                    flag = (currentMode == InfoManager.InfoMode.Water || currentMode == InfoManager.InfoMode.Heating);
                }
                if (flag) {
                    bmInstance.UpdateBuildingColors(building);
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
                nodes[nodeID].m_flags = flags2;
                flag = (currentMode == InfoManager.InfoMode.Water || currentMode == InfoManager.InfoMode.Heating);
            }
            if (flag) {
                nmInstance.UpdateNodeColors((ushort)nodeID);
                for (int i = 0; i < 8; i++) {
                    ushort segment = nodes[nodeID].GetSegment(i);
                    if (segment != 0) {
                        nmInstance.UpdateSegmentColors(segment);
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

        private static Type WaterManagerDataLegacyHandler(string _) => typeof(WaterManagerDataContainer);
        internal static void IntegratedDeserialize(ref WaterManager.Cell[] waterGrid, ref WaterManager.PulseUnit[] waterPulseUnits,
                                                   ref WaterManager.PulseUnit[] sewagePulseUnits, ref WaterManager.PulseUnit[] heatingPulseUnits) {
            int i, j;
            const int diff = (WATERGRID_RESOLUTION - DEFAULTGRID_RESOLUTION) / 2;
            try {
                WaterManager.Cell[] newWaterGrid = new WaterManager.Cell[WATERGRID_RESOLUTION * WATERGRID_RESOLUTION];
                m_waterGrid = newWaterGrid;
                if (Singleton<SimulationManager>.instance.m_serializableDataStorage.TryGetValue(WATERMANAGERDATAKEY, out byte[] data)) {
                    EUtils.ELog("Found 81 WaterManager data, loading...");
                    using (MemoryStream stream = new MemoryStream(data)) {
                        DataSerializer.Deserialize<WaterManagerDataContainer>(stream, DataSerializer.Mode.Memory, WaterManagerDataLegacyHandler);
                    }
                    EUtils.ELog(@"Loaded " + (data.Length / 1024f) + @"kb of 81 WaterManager data");
                } else {
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
                    EUtils.ELog("No 81 WaterManager data found, initialized default buffer to new buffer framework");
                }
                waterGrid = newWaterGrid;
            } catch (Exception e) {
                EUtils.ELog($"Exception Occurred in Integrated Deserialize");
                UnityEngine.Debug.LogException(e);
            }
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

        internal static void Serialize() {
            byte[] data;
            using (var stream = new MemoryStream()) {
                DataSerializer.Serialize(stream, DataSerializer.Mode.Memory, BuildConfig.DATA_FORMAT_VERSION, new WaterManagerDataContainer());
                data = stream.ToArray();
            }
            ESerializableData.SaveData(WATERMANAGERDATAKEY, data);
            EUtils.ELog($"Saved {data.Length / 1024f}kb of 81 WaterManager data");
        }
    }
}
