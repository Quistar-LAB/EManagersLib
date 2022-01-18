using ColossalFramework;
using ColossalFramework.IO;
using EManagersLib.LegacyDataHandlers.EightyOneTiles;
using System;
using System.IO;
using UnityEngine;

namespace EManagersLib {
    public static class EElectricityManager {
        private const string EESERIALIZETOKEN = "fakeEM";
        private const int ELECTRICITYGRID_RESOLUTION = 462;
        private const float ELECTRICITYGRID_CELL_SIZE = 38.25f;

        /// <summary>
        /// PulseGroup and PulseUnit both have their m_x and m_z modified to ushort instead of original byte to hold more data
        /// </summary>
        public struct PulseGroup {
            public uint m_origCharge;
            public uint m_curCharge;
            public ushort m_mergeIndex;
            public ushort m_mergeCount;
            public ushort m_x;
            public ushort m_z;
        }
        public struct PulseUnit {
            public ushort m_group;
            public ushort m_node;
            public ushort m_x;
            public ushort m_z;
        }

        public static PulseGroup[] m_pulseGroups;
        public static PulseUnit[] m_pulseUnits;

        internal static void EnsureCapacity(ref Texture2D electricityTexture, ref int m_modifiedX2, ref int m_modifiedZ2) {
            if (m_modifiedX2 != ELECTRICITYGRID_RESOLUTION - 1 || m_modifiedZ2 != ELECTRICITYGRID_RESOLUTION - 1) {
                EUtils.ELog("ElectricityManager initial Awake routine were somehow skipped! re-initializing essential values.");
                m_modifiedX2 = ELECTRICITYGRID_RESOLUTION - 1;
                m_modifiedZ2 = ELECTRICITYGRID_RESOLUTION - 1;
                electricityTexture = new Texture2D(ELECTRICITYGRID_RESOLUTION, ELECTRICITYGRID_RESOLUTION, TextureFormat.RGBA32, false, true) {
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp
                };
                Shader.SetGlobalTexture("_ElectricityTexture", electricityTexture);
                Vector4 vec;
                vec.z = 1f / (ELECTRICITYGRID_CELL_SIZE * ELECTRICITYGRID_RESOLUTION);
                vec.x = 0.5f;
                vec.y = 0.5f;
                vec.w = 1f / ELECTRICITYGRID_RESOLUTION;
                Shader.SetGlobalVector("_ElectricityMapping", vec);
            }
        }

        private static Type ElectricityDataLegacyHandler(string _) => typeof(EightyOneElectricityDataContainer);
        internal unsafe static void IntegratedDeserialize(ref ElectricityManager.Cell[] electricityGrid, ref ElectricityManager.PulseGroup[] pulseGroups, ref ElectricityManager.PulseUnit[] pulseUnits) {
            try {
                if (Singleton<SimulationManager>.instance.m_serializableDataStorage.TryGetValue(EESERIALIZETOKEN, out byte[] data)) {
                    EUtils.ELog("Found 81 ElectricityManager data, loading...");
                    using (MemoryStream stream = new MemoryStream(data)) {
                        DataSerializer.Deserialize<EightyOneElectricityDataContainer>(stream, DataSerializer.Mode.Memory, ElectricityDataLegacyHandler);
                    }
                    EUtils.ELog(@"Loaded " + (data.Length / 1024f) + @"kb of 81 ElectricityManager data");
                } else {
                    int i, len;
                    const int diff = (ELECTRICITYGRID_RESOLUTION - ElectricityManager.ELECTRICITYGRID_RESOLUTION) / 2;
                    // It might seem strange to initialize buffers here, but this is indeed the best spot to do it, if the map is new to EML + 81 Tiles
                    PulseGroup[] newPulseGroups = new PulseGroup[ElectricityManager.MAX_PULSE_GROUPS];
                    PulseUnit[] newPulseUnits = new PulseUnit[32768]; /* should this be enlarged to RES^2/2? */
                    ElectricityManager.Cell[] newElectricityGrid = new ElectricityManager.Cell[ELECTRICITYGRID_RESOLUTION * ELECTRICITYGRID_RESOLUTION];
                    for (i = 0; i < ElectricityManager.ELECTRICITYGRID_RESOLUTION; i++) {
                        for (int j = 0; j < ElectricityManager.ELECTRICITYGRID_RESOLUTION; j++) {
                            newElectricityGrid[(j + diff) * ELECTRICITYGRID_RESOLUTION + (i + diff)] = electricityGrid[j * ElectricityManager.ELECTRICITYGRID_RESOLUTION + i];
                        }
                    }
                    fixed (PulseGroup* pNewPulseGroups = &newPulseGroups[0])
                    fixed (ElectricityManager.PulseGroup* pOldPulseGroups = &pulseGroups[0]) {
                        len = pulseGroups.Length;
                        PulseGroup* pNew = pNewPulseGroups;
                        ElectricityManager.PulseGroup* pOld = pOldPulseGroups;
                        for (i = 0; i < len; i++, pNew++, pOld++) {
                            pNew->m_curCharge = pOld->m_curCharge;
                            pNew->m_mergeCount = pOld->m_mergeCount;
                            pNew->m_mergeIndex = pOld->m_mergeIndex;
                            pNew->m_origCharge = pOld->m_origCharge;
                            pNew->m_x = pOld->m_x;
                            pNew->m_z = pOld->m_z;
                        }
                    }
                    fixed (PulseUnit* pNewPulseUnits = &newPulseUnits[0])
                    fixed (ElectricityManager.PulseUnit* pOldPulseUnits = &pulseUnits[0]) {
                        len = pulseUnits.Length;
                        PulseUnit* pNew = pNewPulseUnits;
                        ElectricityManager.PulseUnit* pOld = pOldPulseUnits;
                        for (i = 0; i < len; i++, pNew++, pOld++) {
                            pNew->m_group = pOld->m_group;
                            pNew->m_node = pOld->m_node;
                            pNew->m_x = pOld->m_x;
                            pNew->m_z = pOld->m_z;
                        }
                    }
                    m_pulseGroups = newPulseGroups;
                    m_pulseUnits = newPulseUnits;
                    electricityGrid = newElectricityGrid;
                    EUtils.ELog("No 81 ElectricityManager data found, Converted default new 81 format tiles");
                }
            } catch (Exception e) {
                UnityEngine.Debug.LogException(e);
            }
        }

        internal unsafe static void IntegratedSerialize(ref ElectricityManager.Cell[] electricityGrid, ref ElectricityManager.PulseGroup[] pulseGroups, ref ElectricityManager.PulseUnit[] pulseUnits) {
            int i, j, len;
            ElectricityManager.Cell[] defEGrid = new ElectricityManager.Cell[ElectricityManager.ELECTRICITYGRID_RESOLUTION * ElectricityManager.ELECTRICITYGRID_RESOLUTION];
            const int diff = (ELECTRICITYGRID_RESOLUTION - ElectricityManager.ELECTRICITYGRID_RESOLUTION) / 2;
            for (i = 0; i < ElectricityManager.ELECTRICITYGRID_RESOLUTION; i += 1) {
                for (j = 0; j < ElectricityManager.ELECTRICITYGRID_RESOLUTION; j += 1) {
                    defEGrid[j * ElectricityManager.ELECTRICITYGRID_RESOLUTION + i] = electricityGrid[(j + diff) * ELECTRICITYGRID_RESOLUTION + (i + diff)];
                }
            }
            fixed (PulseGroup* pPulseGroups = &m_pulseGroups[0])
            fixed (ElectricityManager.PulseGroup* pOldPulseGroups = &pulseGroups[0]) {
                len = pulseGroups.Length;
                PulseGroup* pNew = pPulseGroups;
                ElectricityManager.PulseGroup* pOld = pOldPulseGroups;
                for (i = 0; i < len; i++, pNew++, pOld++) {
                    pOld->m_curCharge = pNew->m_curCharge;
                    pOld->m_mergeCount = pNew->m_mergeCount;
                    pOld->m_mergeIndex = pNew->m_mergeIndex;
                    pOld->m_origCharge = pNew->m_origCharge;
                    pOld->m_x = (byte)EMath.Clamp(pNew->m_x, 0, 255);
                    pOld->m_z = (byte)EMath.Clamp(pNew->m_z, 0, 255);
                }
            }
            fixed (PulseUnit* pNewPulseUnits = &m_pulseUnits[0])
            fixed (ElectricityManager.PulseUnit* pOldPulseUnits = &pulseUnits[0]) {
                len = pulseUnits.Length;
                PulseUnit* pNew = pNewPulseUnits;
                ElectricityManager.PulseUnit* pOld = pOldPulseUnits;
                for (i = 0; i < len; i++, pNew++, pOld++) {
                    pOld->m_group = pNew->m_group;
                    pOld->m_node = pNew->m_node;
                    pOld->m_x = (byte)EMath.Clamp(pNew->m_x, 0, 255);
                    pOld->m_z = (byte)EMath.Clamp(pNew->m_z, 0, 255);
                }
            }
            byte[] data;
            using (var stream = new MemoryStream()) {
                DataSerializer.Serialize(stream, DataSerializer.Mode.Memory, BuildConfig.DATA_FORMAT_VERSION, new EightyOneElectricityDataContainer());
                data = stream.ToArray();
            }
            ESerializableData.SaveData(EESERIALIZETOKEN, data);
            EUtils.ELog($"Saved {data.Length / 1024f}kb of 81 ElectricityManager data");

            electricityGrid = defEGrid;
        }

        internal static void ConductToCell(PulseGroup[] pulseGroups, ElectricityManager.Cell[] electricityGrid,
            PulseUnit[] pulseUnits, int pulseGroupCount, ref int pulseUnitEnd, ref bool canContinue, ref ElectricityManager.Cell cell, ushort group, int x, int z, int limit) {
            if (cell.m_conductivity >= limit) {
                if (cell.m_conductivity < 64) {
                    bool flag = true;
                    bool flag2 = true;
                    int num = z * ELECTRICITYGRID_RESOLUTION + x;
                    if (x > 0 && electricityGrid[num - 1].m_conductivity >= 64) {
                        flag = false;
                    }
                    if (x < (ELECTRICITYGRID_RESOLUTION - 1) && electricityGrid[num + 1].m_conductivity >= 64) {
                        flag = false;
                    }
                    if (z > 0 && electricityGrid[num - ELECTRICITYGRID_RESOLUTION].m_conductivity >= 64) {
                        flag2 = false;
                    }
                    if (z < (ELECTRICITYGRID_RESOLUTION - 1) && electricityGrid[num + ELECTRICITYGRID_RESOLUTION].m_conductivity >= 64) {
                        flag2 = false;
                    }
                    if (flag || flag2) {
                        return;
                    }
                }
                if (cell.m_pulseGroup == 65535) {
                    PulseUnit pulseUnit;
                    pulseUnit.m_group = group;
                    pulseUnit.m_node = 0;
                    pulseUnit.m_x = (byte)x;
                    pulseUnit.m_z = (byte)z;
                    pulseUnits[pulseUnitEnd] = pulseUnit;
                    if (++pulseUnitEnd == pulseUnits.Length) {
                        pulseUnitEnd = 0;
                    }
                    cell.m_pulseGroup = group;
                    canContinue = true;
                } else {
                    ushort rootGroup = GetRootGroup(pulseGroups, cell.m_pulseGroup);
                    if (rootGroup != group) {
                        MergeGroups(pulseGroups, pulseGroupCount, group, rootGroup);
                        cell.m_pulseGroup = group;
                        canContinue = true;
                    }
                }
            }
        }

        private static void ConductToCells(PulseGroup[] pulseGroups, ElectricityManager.Cell[] electricityGrid, PulseUnit[] pulseUnits,
            int pulseGroupCount, ref int pulseUnitEnd, ref bool canContinue, ushort group, float worldX, float worldZ) {
            const float halfGrid = ELECTRICITYGRID_RESOLUTION / 2f;
            int num = (int)(worldX / ELECTRICITYGRID_CELL_SIZE + halfGrid);
            int num2 = (int)(worldZ / ELECTRICITYGRID_CELL_SIZE + halfGrid);
            if (num >= 0 && num < ELECTRICITYGRID_RESOLUTION && num2 >= 0 && num2 < ELECTRICITYGRID_RESOLUTION) {
                int num3 = num2 * ELECTRICITYGRID_RESOLUTION + num;
                ConductToCell(pulseGroups, electricityGrid, pulseUnits, pulseGroupCount, ref pulseUnitEnd, ref canContinue, ref electricityGrid[num3], group, num, num2, 1);
            }
        }

        internal static void ConductToNode(ushort[] nodeGroups, PulseGroup[] pulseGroups, PulseUnit[] pulseUnits,
            int pulseGroupCount, ref int pulseUnitEnd, ref bool canContinue, ushort nodeIndex, ref NetNode node, ushort group, float minX, float minZ, float maxX, float maxZ) {
            if (node.m_position.x >= minX && node.m_position.z >= minZ && node.m_position.x <= maxX && node.m_position.z <= maxZ) {
                NetInfo info = node.Info;
                if (info.m_class.m_service == ItemClass.Service.Electricity) {
                    if (nodeGroups[nodeIndex] == 65535) {
                        PulseUnit pulseUnit;
                        pulseUnit.m_group = group;
                        pulseUnit.m_node = nodeIndex;
                        pulseUnit.m_x = 0;
                        pulseUnit.m_z = 0;
                        pulseUnits[pulseUnitEnd] = pulseUnit;
                        if (++pulseUnitEnd == pulseUnits.Length) {
                            pulseUnitEnd = 0;
                        }
                        nodeGroups[nodeIndex] = group;
                        canContinue = true;
                    } else {
                        ushort rootGroup = GetRootGroup(pulseGroups, nodeGroups[nodeIndex]);
                        if (rootGroup != group) {
                            MergeGroups(pulseGroups, pulseGroupCount, group, rootGroup);
                            nodeGroups[nodeIndex] = group;
                            canContinue = true;
                        }
                    }
                }
            }
        }

        private static void ConductToNodes(ushort[] nodeGroups, PulseGroup[] pulseGroups, PulseUnit[] pulseUnits,
            int pulseGroupCount, ref int pulseUnitEnd, ref bool canContinue, ushort group, int cellX, int cellZ) {
            const float halfGrid = ELECTRICITYGRID_RESOLUTION / 2f;
            float x = (cellX - halfGrid) * ELECTRICITYGRID_CELL_SIZE;
            float z = (cellZ - halfGrid) * ELECTRICITYGRID_CELL_SIZE;
            float x1 = x + ELECTRICITYGRID_CELL_SIZE;
            float z1 = z + ELECTRICITYGRID_CELL_SIZE;
            int startX = EMath.Max((int)(x / 64f + 135f), 0);
            int startZ = EMath.Max((int)(z / 64f + 135f), 0);
            int endX = EMath.Min((int)(x1 / 64f + 135f), 269);
            int endZ = EMath.Min((int)(z1 / 64f + 135f), 269);
            NetManager instance = Singleton<NetManager>.instance;
            for (int i = startZ; i <= endZ; i++) {
                for (int j = startX; j <= endX; j++) {
                    ushort nodeID = instance.m_nodeGrid[i * 270 + j];
                    while (nodeID != 0) {
                        ConductToNode(nodeGroups, pulseGroups, pulseUnits, pulseGroupCount, ref pulseUnitEnd, ref canContinue, nodeID, ref instance.m_nodes.m_buffer[nodeID], group, x, z, x1, z1);
                        nodeID = instance.m_nodes.m_buffer[nodeID].m_nextGridNode;
                    }
                }
            }
        }

        internal static ushort GetRootGroup(PulseGroup[] pulseGroups, ushort group) {
            for (ushort mergeIndex = pulseGroups[group].m_mergeIndex; mergeIndex != 65535; mergeIndex = pulseGroups[group].m_mergeIndex) {
                group = mergeIndex;
            }
            return group;
        }

        private static void MergeGroups(PulseGroup[] pulseGroups, int pulseGroupCount, ushort root, ushort merged) {
            PulseGroup pulseGroup = pulseGroups[root];
            PulseGroup pulseGroup2 = pulseGroups[merged];
            pulseGroup.m_origCharge += pulseGroup2.m_origCharge;
            if (pulseGroup2.m_mergeCount != 0) {
                for (int i = 0; i < pulseGroupCount; i++) {
                    if (pulseGroups[i].m_mergeIndex == merged) {
                        pulseGroups[i].m_mergeIndex = root;
                        pulseGroup2.m_origCharge -= pulseGroups[i].m_origCharge;
                    }
                }
                pulseGroup.m_mergeCount += pulseGroup2.m_mergeCount;
                pulseGroup2.m_mergeCount = 0;
            }
            pulseGroup.m_curCharge += pulseGroup2.m_curCharge;
            pulseGroup2.m_curCharge = 0u;
            pulseGroup.m_mergeCount += 1;
            pulseGroup2.m_mergeIndex = root;
            pulseGroups[root] = pulseGroup;
            pulseGroups[merged] = pulseGroup2;
        }

        public static void SimulationStepImpl(ElectricityManager emInstance, int subStep, ref int pulseGroupCount, ref int pulseUnitStart, ref int pulseUnitEnd,
            ref int processedCells, ref int conductiveCells, ref bool canContinue, ElectricityManager.Cell[] electricityGrid) {
            if (subStep != 0 && subStep != 1000) {
                PulseGroup[] pulseGroups = m_pulseGroups;
                PulseUnit[] pulseUnits = m_pulseUnits;
                ushort[] nodeGroups = emInstance.m_nodeGroups;
                NetManager nmInstance = Singleton<NetManager>.instance;
                NetNode[] netNodes = nmInstance.m_nodes.m_buffer;
                NetSegment[] netSegments = nmInstance.m_segments.m_buffer;
                uint currentFrameIndex = Singleton<SimulationManager>.instance.m_currentFrameIndex;
                int num = (int)(currentFrameIndex & 255u);
                if (num < 128) {
                    if (num == 0) {
                        pulseGroupCount = 0;
                        pulseUnitStart = 0;
                        pulseUnitEnd = 0;
                        processedCells = 0;
                        conductiveCells = 0;
                        canContinue = true;
                    }
                    int num2 = num * 32768 >> 7;
                    int num3 = ((num + 1) * 32768 >> 7) - 1;
                    for (int i = num2; i <= num3; i++) {
                        if (netNodes[i].m_flags != NetNode.Flags.None) {
                            NetInfo info = netNodes[i].Info;
                            if (info.m_class.m_service == ItemClass.Service.Electricity) {
                                UpdateNodeElectricity(nmInstance, netNodes, i, (nodeGroups[i] == 65535) ? 0 : 1);
                                conductiveCells++;
                            }
                        }
                        emInstance.m_nodeGroups[i] = 65535;
                    }
#if true
                    int num4 = num * 4;
                    if (num4 < ELECTRICITYGRID_RESOLUTION) {
                        int num5 = EMath.Min(ELECTRICITYGRID_RESOLUTION, num4 + 4);
                        for (int j = num4; j <= num5; j++) {
                            int eGrid = j * ELECTRICITYGRID_RESOLUTION;
                            for (int k = 0; k < ELECTRICITYGRID_RESOLUTION; k++) {
                                ElectricityManager.Cell cell = electricityGrid[eGrid];
                                if (cell.m_currentCharge > 0) {
                                    if (pulseGroupCount < 1024) {
                                        PulseGroup pulseGroup;
                                        pulseGroup.m_origCharge = (uint)cell.m_currentCharge;
                                        pulseGroup.m_curCharge = (uint)cell.m_currentCharge;
                                        pulseGroup.m_mergeCount = 0;
                                        pulseGroup.m_mergeIndex = 65535;
                                        pulseGroup.m_x = (byte)k;
                                        pulseGroup.m_z = (byte)j;
                                        PulseUnit pulseUnit;
                                        pulseUnit.m_group = (ushort)pulseGroupCount;
                                        pulseUnit.m_node = 0;
                                        pulseUnit.m_x = (byte)k;
                                        pulseUnit.m_z = (byte)j;
                                        cell.m_pulseGroup = (ushort)pulseGroupCount;
                                        pulseGroups[pulseGroupCount++] = pulseGroup;
                                        pulseUnits[pulseUnitEnd] = pulseUnit;
                                        if (++pulseUnitEnd == pulseUnits.Length) {
                                            pulseUnitEnd = 0;
                                        }
                                    } else {
                                        cell.m_pulseGroup = 65535;
                                    }
                                    cell.m_currentCharge = 0;
                                    conductiveCells++;
                                } else {
                                    cell.m_pulseGroup = 65535;
                                    if (cell.m_conductivity >= 64) {
                                        conductiveCells++;
                                    }
                                }
                                if (cell.m_tmpElectrified != cell.m_electrified) {
                                    cell.m_electrified = cell.m_tmpElectrified;
                                }
                                cell.m_tmpElectrified = (cell.m_pulseGroup != 65535);
                                electricityGrid[eGrid] = cell;
                                eGrid++;
                            }
                        }
                    }
#else
					int num4 = num * 256 >> 7;
					int num5 = ((num + 1) * 256 >> 7) - 1;
					for (int j = num4; j <= num5; j++) {
						int eGrid = j * ELECTRICITYGRID_RESOLUTION;
						for (int k = 0; k < ELECTRICITYGRID_RESOLUTION; k++) {
							ElectricityManager.Cell cell = electricityGrid[eGrid];
							if (cell.m_currentCharge > 0) {
								if (pulseGroupCount < 1024) {
									ElectricityManager.PulseGroup pulseGroup;
									pulseGroup.m_origCharge = (uint)cell.m_currentCharge;
									pulseGroup.m_curCharge = (uint)cell.m_currentCharge;
									pulseGroup.m_mergeCount = 0;
									pulseGroup.m_mergeIndex = 65535;
									pulseGroup.m_x = (byte)k;
									pulseGroup.m_z = (byte)j;
									ElectricityManager.PulseUnit pulseUnit;
									pulseUnit.m_group = (ushort)pulseGroupCount;
									pulseUnit.m_node = 0;
									pulseUnit.m_x = (byte)k;
									pulseUnit.m_z = (byte)j;
									cell.m_pulseGroup = (ushort)pulseGroupCount;
									pulseGroups[pulseGroupCount++] = pulseGroup;
									pulseUnits[pulseUnitEnd] = pulseUnit;
									if (++pulseUnitEnd == pulseUnits.Length) {
										pulseUnitEnd = 0;
									}
								} else {
									cell.m_pulseGroup = 65535;
								}
								cell.m_currentCharge = 0;
								conductiveCells++;
							} else {
								cell.m_pulseGroup = 65535;
								if (cell.m_conductivity >= 64) {
									conductiveCells++;
								}
							}
							if (cell.m_tmpElectrified != cell.m_electrified) {
								cell.m_electrified = cell.m_tmpElectrified;
							}
							cell.m_tmpElectrified = (cell.m_pulseGroup != 65535);
							electricityGrid[eGrid] = cell;
							eGrid++;
						}
					}
#endif
                } else {
                    int num7 = (num - 127) * conductiveCells >> 7;
                    if (num == 255) {
                        num7 = 1000000000;
                    }
                    while (canContinue && processedCells < num7) {
                        canContinue = false;
                        int __pulseUnitEnd = pulseUnitEnd;
                        while (pulseUnitStart != __pulseUnitEnd) {
                            ref PulseUnit pulseUnit2 = ref pulseUnits[pulseUnitStart];
                            if (++pulseUnitStart == pulseUnits.Length) {
                                pulseUnitStart = 0;
                            }
                            pulseUnit2.m_group = GetRootGroup(pulseGroups, pulseUnit2.m_group);
                            uint num8 = pulseGroups[pulseUnit2.m_group].m_curCharge;
                            if (pulseUnit2.m_node == 0) {
                                int num9 = pulseUnit2.m_z * 256 + pulseUnit2.m_x;
                                ElectricityManager.Cell cell = electricityGrid[num9];
                                if (cell.m_conductivity != 0 && !cell.m_tmpElectrified && num8 != 0u) {
                                    int num10 = EMath.Clamp((-cell.m_currentCharge), 0, (int)num8);
                                    num8 -= (uint)num10;
                                    cell.m_currentCharge += (short)num10;
                                    if (cell.m_currentCharge == 0) {
                                        cell.m_tmpElectrified = true;
                                    }
                                    electricityGrid[num9] = cell;
                                    pulseGroups[pulseUnit2.m_group].m_curCharge = num8;
                                }
                                if (num8 != 0u) {
                                    int limit = (cell.m_conductivity < 64) ? 64 : 1;
                                    processedCells++;
                                    if (pulseUnit2.m_z > 0) {
                                        ConductToCell(pulseGroups, electricityGrid, pulseUnits, pulseGroupCount, ref pulseUnitEnd, ref canContinue,
                                            ref electricityGrid[num9 - 256], pulseUnit2.m_group, pulseUnit2.m_x, (pulseUnit2.m_z - 1), limit);
                                    }
                                    if (pulseUnit2.m_x > 0) {
                                        ConductToCell(pulseGroups, electricityGrid, pulseUnits, pulseGroupCount, ref pulseUnitEnd, ref canContinue,
                                            ref electricityGrid[num9 - 1], pulseUnit2.m_group, (pulseUnit2.m_x - 1), pulseUnit2.m_z, limit);
                                    }
                                    if (pulseUnit2.m_z < 255) {
                                        ConductToCell(pulseGroups, electricityGrid, pulseUnits, pulseGroupCount, ref pulseUnitEnd, ref canContinue,
                                            ref electricityGrid[num9 + 256], pulseUnit2.m_group, pulseUnit2.m_x, (pulseUnit2.m_z + 1), limit);
                                    }
                                    if (pulseUnit2.m_x < 255) {
                                        ConductToCell(pulseGroups, electricityGrid, pulseUnits, pulseGroupCount, ref pulseUnitEnd, ref canContinue,
                                            ref electricityGrid[num9 + 1], pulseUnit2.m_group, (pulseUnit2.m_x + 1), pulseUnit2.m_z, limit);
                                    }
                                    ConductToNodes(nodeGroups, pulseGroups, pulseUnits, pulseGroupCount, ref pulseUnitEnd, ref canContinue,
                                        pulseUnit2.m_group, pulseUnit2.m_x, pulseUnit2.m_z);
                                } else {
                                    pulseUnits[pulseUnitEnd] = pulseUnit2;
                                    if (++pulseUnitEnd == pulseUnits.Length) {
                                        pulseUnitEnd = 0;
                                    }
                                }
                            } else if (num8 != 0u) {
                                processedCells++;
                                ref NetNode netNode = ref netNodes[pulseUnit2.m_node];
                                if (netNode.m_flags != NetNode.Flags.None && netNode.m_buildIndex < (currentFrameIndex & 4294967168u)) {
                                    ConductToCells(pulseGroups, electricityGrid, pulseUnits, pulseGroupCount, ref pulseUnitEnd, ref canContinue,
                                        pulseUnit2.m_group, netNode.m_position.x, netNode.m_position.z);
                                    for (int l = 0; l < 8; l++) {
                                        ushort segment = netNode.GetSegment(l);
                                        if (segment != 0) {
                                            ushort startNode = netSegments[segment].m_startNode;
                                            ushort endNode = netSegments[segment].m_endNode;
                                            ushort nodeIndex = (startNode != pulseUnit2.m_node) ? startNode : endNode;
                                            ConductToNode(nodeGroups, pulseGroups, pulseUnits, pulseGroupCount, ref pulseUnitEnd, ref canContinue,
                                                nodeIndex, ref netNodes[nodeIndex], pulseUnit2.m_group, -100000f, -100000f, 100000f, 100000f);
                                        }
                                    }
                                }
                            } else {
                                pulseUnits[pulseUnitEnd] = pulseUnit2;
                                if (++pulseUnitEnd == pulseUnits.Length) {
                                    pulseUnitEnd = 0;
                                }
                            }
                        }
                    }
                    if (num == 255) {
                        for (int m = 0; m < pulseGroupCount; m++) {
                            PulseGroup pulseGroup2 = pulseGroups[m];
                            if (pulseGroup2.m_mergeIndex != 65535) {
                                PulseGroup pulseGroup3 = pulseGroups[pulseGroup2.m_mergeIndex];
                                pulseGroup2.m_curCharge = (uint)(pulseGroup3.m_curCharge * (ulong)pulseGroup2.m_origCharge / pulseGroup3.m_origCharge);
                                pulseGroup3.m_curCharge -= pulseGroup2.m_curCharge;
                                pulseGroup3.m_origCharge -= pulseGroup2.m_origCharge;
                                pulseGroups[pulseGroup2.m_mergeIndex] = pulseGroup3;
                                pulseGroups[m] = pulseGroup2;
                            }
                        }
                        for (int n = 0; n < pulseGroupCount; n++) {
                            PulseGroup pulseGroup4 = pulseGroups[n];
                            if (pulseGroup4.m_curCharge != 0u) {
                                int num12 = pulseGroup4.m_z * 256 + pulseGroup4.m_x;
                                ElectricityManager.Cell cell3 = electricityGrid[num12];
                                if (cell3.m_conductivity != 0) {
                                    cell3.m_extraCharge += (ushort)EMath.Min((int)pulseGroup4.m_curCharge, (32767 - cell3.m_extraCharge));
                                }
                                electricityGrid[num12] = cell3;
                            }
                        }
                    }
                }
            }
        }

        private static void UpdateNodeElectricity(NetManager nmInstance, NetNode[] netNodes, int nodeID, int value) {
            InfoManager.InfoMode currentMode = Singleton<InfoManager>.instance.CurrentMode;
            bool flag = false;
            NetNode.Flags flags = netNodes[nodeID].m_flags;
            if ((flags & NetNode.Flags.Transition) != NetNode.Flags.None) {
                netNodes[nodeID].m_flags &= ~NetNode.Flags.Transition;
                return;
            }
            ushort building = netNodes[nodeID].m_building;
            if (building != 0) {
                BuildingManager bmInstance = Singleton<BuildingManager>.instance;
                Building[] buildings = bmInstance.m_buildings.m_buffer;
                if (buildings[building].m_electricityBuffer != value) {
                    buildings[building].m_electricityBuffer = (ushort)value;
                    flag = (currentMode == InfoManager.InfoMode.Electricity);
                }
                if (flag) {
                    bmInstance.UpdateBuildingColors(building);
                }
            }
            NetNode.Flags flags2 = flags & ~NetNode.Flags.Electricity;
            if (value != 0) {
                flags2 |= NetNode.Flags.Electricity;
            }
            if (flags2 != flags) {
                netNodes[nodeID].m_flags = flags2;
                flag = (currentMode == InfoManager.InfoMode.Electricity);
            }
            if (flag) {
                nmInstance.UpdateNodeColors((ushort)nodeID);
                for (int i = 0; i < 8; i++) {
                    ushort segment = netNodes[nodeID].GetSegment(i);
                    if (segment != 0) {
                        nmInstance.UpdateSegmentColors(segment);
                    }
                }
            }
        }

        public static void UpdateGrid(ElectricityManager emInstance, ElectricityManager.Cell[] electricityGrid, float minX, float minZ, float maxX, float maxZ) {
            const int Grid = ELECTRICITYGRID_RESOLUTION;
            const float halfGrid = ELECTRICITYGRID_RESOLUTION / 2f;
            int ex1 = EMath.Max((int)(minX / 38.25f + halfGrid), 0);
            int ez1 = EMath.Max((int)(minZ / 38.25f + halfGrid), 0);
            int ex2 = EMath.Min((int)(maxX / 38.25f + halfGrid), Grid - 1);
            int ez2 = EMath.Min((int)(maxZ / 38.25f + halfGrid), Grid - 1);
            for (int i = ez1; i <= ez2; i++) {
                int eGrid = i * Grid + ex1;
                for (int j = ex1; j <= ex2; j++) {
                    electricityGrid[eGrid++].m_conductivity = 0;
                }
            }
            int num6 = EMath.Max((int)(((ex1 - halfGrid) * 38.25f - 96f) / 64f + 135f), 0);
            int num7 = EMath.Max((int)(((ez1 - halfGrid) * 38.25f - 96f) / 64f + 135f), 0);
            int num8 = EMath.Min((int)(((ex2 - halfGrid + 1f) * 38.25f + 96f) / 64f + 135f), 269);
            int num9 = EMath.Min((int)(((ez2 - halfGrid + 1f) * 38.25f + 96f) / 64f + 135f), 269);
            Building[] buildings = Singleton<BuildingManager>.instance.m_buildings.m_buffer;
            ushort[] buildingGrid = Singleton<BuildingManager>.instance.m_buildingGrid;
            for (int k = num7; k <= num9; k++) {
                for (int l = num6; l <= num8; l++) {
                    ushort building = buildingGrid[k * 270 + l];
                    while (building != 0) {
                        Building.Flags flags = buildings[building].m_flags;
                        if ((flags & (Building.Flags.Created | Building.Flags.Deleted)) == Building.Flags.Created) {
                            buildings[building].GetInfoWidthLength(out BuildingInfo buildingInfo, out int num12, out int num13);
                            if (!(buildingInfo is null)) {
                                float radius = buildingInfo.m_buildingAI.ElectricityGridRadius();
                                if (radius > 0.1f) {
                                    Vector3 position = buildings[building].m_position;
                                    float angle = buildings[building].m_angle;
                                    Vector3 vector = new Vector3((float)Math.Cos(angle), 0f, (float)Math.Sin(angle));
                                    Vector3 vector2 = new Vector3(vector.z, 0f, -vector.x);
                                    Vector3 vector3 = position - (num12 * 4) * vector - (num13 * 4) * vector2;
                                    Vector3 vector4 = position + (num12 * 4) * vector - (num13 * 4) * vector2;
                                    Vector3 vector5 = position + (num12 * 4) * vector + (num13 * 4) * vector2;
                                    Vector3 vector6 = position - (num12 * 4) * vector + (num13 * 4) * vector2;
                                    minX = EMath.Min(EMath.Min(vector3.x, vector4.x), EMath.Min(vector5.x, vector6.x)) - radius;
                                    maxX = EMath.Max(EMath.Max(vector3.x, vector4.x), EMath.Max(vector5.x, vector6.x)) + radius;
                                    minZ = EMath.Min(EMath.Min(vector3.z, vector4.z), EMath.Min(vector5.z, vector6.z)) - radius;
                                    maxZ = EMath.Max(EMath.Max(vector3.z, vector4.z), EMath.Max(vector5.z, vector6.z)) + radius;
                                    int num15 = EMath.Max(ex1, (int)(minX / 38.25f + halfGrid));
                                    int num16 = EMath.Min(ex2, (int)(maxX / 38.25f + halfGrid));
                                    int num17 = EMath.Max(ez1, (int)(minZ / 38.25f + halfGrid));
                                    int num18 = EMath.Min(ez2, (int)(maxZ / 38.25f + halfGrid));
                                    for (int m = num17; m <= num18; m++) {
                                        for (int n = num15; n <= num16; n++) {
                                            Vector3 a;
                                            a.x = (n + 0.5f - halfGrid) * 38.25f;
                                            a.y = position.y;
                                            a.z = (m + 0.5f - halfGrid) * 38.25f;
                                            float num19 = EMath.Max(0f, EMath.Abs(Vector3.Dot(vector, a - position)) - (num12 * 4));
                                            float num20 = EMath.Max(0f, EMath.Abs(Vector3.Dot(vector2, a - position)) - (num13 * 4));
                                            float num21 = (float)Math.Sqrt(num19 * num19 + num20 * num20);
                                            if (num21 < radius + 19.125f) {
                                                float num22 = (radius - num21) * 0.0130718956f + 0.25f;
                                                int num23 = EMath.Min(255, EMath.RoundToInt(num22 * 255f));
                                                int num24 = m * Grid + n;
                                                if (num23 > electricityGrid[num24].m_conductivity) {
                                                    electricityGrid[num24].m_conductivity = (byte)num23;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        building = buildings[building].m_nextGridBuilding;
                    }
                }
            }
            for (int num25 = ez1; num25 <= ez2; num25++) {
                int num26 = num25 * 256 + ex1;
                for (int num27 = ex1; num27 <= ex2; num27++) {
                    ElectricityManager.Cell cell = electricityGrid[num26];
                    if (cell.m_conductivity == 0) {
                        cell.m_currentCharge = 0;
                        cell.m_extraCharge = 0;
                        cell.m_pulseGroup = 65535;
                        cell.m_tmpElectrified = false;
                        cell.m_electrified = false;
                        electricityGrid[num26] = cell;
                    }
                    num26++;
                }
            }
            emInstance.AreaModified(ex1, ez1, ex2, ez2);
        }
    }
}
