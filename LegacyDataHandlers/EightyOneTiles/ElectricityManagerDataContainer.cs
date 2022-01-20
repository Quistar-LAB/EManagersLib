using ColossalFramework;
using ColossalFramework.IO;
using System.Reflection;
using static EManagersLib.EElectricityManager;

namespace EManagersLib.LegacyDataHandlers.EightyOneTiles {
    internal sealed class ElectricityManagerDataContainer : IDataContainer {
        public void AfterDeserialize(DataSerializer s) { }

        public void Deserialize(DataSerializer s) {
            int i, length;
            int pulseGroupCount;
            int processedCells;
            int conductiveCells;
            bool canContinue;
            FieldInfo PGCField = typeof(ElectricityManager).GetField("m_pulseGroupCount", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo PUSField = typeof(ElectricityManager).GetField("m_pulseUnitStart", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo PUEField = typeof(ElectricityManager).GetField("m_pulseUnitEnd", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo processedCellsField = typeof(ElectricityManager).GetField("m_processedCells", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo conductiveCellsField = typeof(ElectricityManager).GetField("m_conductiveCells", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo canContinueField = typeof(ElectricityManager).GetField("m_canContinue", BindingFlags.Instance | BindingFlags.NonPublic);
            ElectricityManager emInstance = Singleton<ElectricityManager>.instance;
            ElectricityManager.Cell[] electricityGrid = m_electricityGrid;
            ushort[] nodeGroups = emInstance.m_nodeGroups;
            PulseGroup[] pulseGroups = m_pulseGroups;
            PulseUnit[] pulseUnits = m_pulseUnits;

            length = electricityGrid.Length;
            EncodedArray.Byte @byte = EncodedArray.Byte.BeginRead(s);
            for (i = 0; i < length; i++) {
                electricityGrid[i].m_conductivity = @byte.Read();
            }
            @byte.EndRead();

            EncodedArray.Short @short = EncodedArray.Short.BeginRead(s);
            for (i = 0; i < length; i++) {
                if (electricityGrid[i].m_conductivity != 0) {
                    electricityGrid[i].m_currentCharge = @short.Read();
                } else {
                    electricityGrid[i].m_currentCharge = 0;
                }
            }
            @short.EndRead();
            EncodedArray.UShort uShort = EncodedArray.UShort.BeginRead(s);
            for (i = 0; i < length; i++) {
                if (electricityGrid[i].m_conductivity != 0) {
                    electricityGrid[i].m_extraCharge = uShort.Read();
                } else {
                    electricityGrid[i].m_extraCharge = 0;
                }
            }
            uShort.EndRead();
            EncodedArray.UShort uShort2 = EncodedArray.UShort.BeginRead(s);
            for (i = 0; i < length; i++) {
                if (electricityGrid[i].m_conductivity != 0) {
                    electricityGrid[i].m_pulseGroup = uShort2.Read();
                } else {
                    electricityGrid[i].m_pulseGroup = 65535;
                }
            }
            uShort2.EndRead();

            EncodedArray.Bool @bool = EncodedArray.Bool.BeginRead(s);
            for (i = 0; i < length; i++) {
                if (electricityGrid[i].m_conductivity != 0) {
                    electricityGrid[i].m_electrified = @bool.Read();
                } else {
                    electricityGrid[i].m_electrified = false;
                }
            }
            @bool.EndRead();
            EncodedArray.Bool bool2 = EncodedArray.Bool.BeginRead(s);
            for (i = 0; i < length; i++) {
                if (electricityGrid[i].m_conductivity != 0) {
                    electricityGrid[i].m_tmpElectrified = bool2.Read();
                } else {
                    electricityGrid[i].m_tmpElectrified = false;
                }
            }
            bool2.EndRead();

            pulseGroupCount = (int)s.ReadUInt16();
            PGCField.SetValue(emInstance, pulseGroupCount);
            for (i = 0; i < pulseGroupCount; i++) {
                pulseGroups[i].m_origCharge = s.ReadUInt32();
                pulseGroups[i].m_curCharge = s.ReadUInt32();
                pulseGroups[i].m_mergeIndex = (ushort)s.ReadUInt16();
                pulseGroups[i].m_mergeCount = (ushort)s.ReadUInt16();
                pulseGroups[i].m_x = (ushort)s.ReadUInt16();
                pulseGroups[i].m_z = (ushort)s.ReadUInt16();
            }

            length = (int)s.ReadUInt16();
            PUSField.SetValue(emInstance, 0);
            PUEField.SetValue(emInstance, length % pulseUnits.Length);
            for (i = 0; i < length; i++) {
                pulseUnits[i].m_group = (ushort)s.ReadUInt16();
                pulseUnits[i].m_node = (ushort)s.ReadUInt16();
                pulseUnits[i].m_x = (ushort)s.ReadUInt16();
                pulseUnits[i].m_z = (ushort)s.ReadUInt16();
            }

            EncodedArray.UShort uShort4 = EncodedArray.UShort.BeginRead(s);
            for (i = 0; i < 32768; i++) {
                nodeGroups[i] = uShort4.Read();
            }
            uShort4.EndRead();

            processedCells = s.ReadInt32();
            conductiveCells = s.ReadInt32();
            canContinue = s.ReadBool();
            processedCellsField.SetValue(emInstance, processedCells);
            conductiveCellsField.SetValue(emInstance, conductiveCells);
            canContinueField.SetValue(emInstance, canContinue);
        }

        public void Serialize(DataSerializer s) {
            int i, length;
            FieldInfo PGCField = typeof(ElectricityManager).GetField("m_pulseGroupCount", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo PUSField = typeof(ElectricityManager).GetField("m_pulseUnitStart", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo PUEField = typeof(ElectricityManager).GetField("m_pulseUnitEnd", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo processedCellsField = typeof(ElectricityManager).GetField("m_processedCells", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo conductiveCellsField = typeof(ElectricityManager).GetField("m_conductiveCells", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo canContinueField = typeof(ElectricityManager).GetField("m_canContinue", BindingFlags.Instance | BindingFlags.NonPublic);
            ElectricityManager emInstance = Singleton<ElectricityManager>.instance;
            ushort[] nodeGroups = emInstance.m_nodeGroups;
            PulseGroup[] pulseGroups = m_pulseGroups;
            PulseUnit[] pulseUnits = m_pulseUnits;
            int pulseGroupCount = (int)PGCField.GetValue(emInstance);
            int pulseUnitStart = (int)PUSField.GetValue(emInstance);
            int pulseUnitEnd = (int)PUEField.GetValue(emInstance);
            int processedCells = (int)processedCellsField.GetValue(emInstance);
            int conductiveCells = (int)conductiveCellsField.GetValue(emInstance);
            bool canContinue = (bool)canContinueField.GetValue(emInstance);

            ElectricityManager.Cell[] electricityGrid = m_electricityGrid;
            length = electricityGrid.Length;
            EncodedArray.Byte @byte = EncodedArray.Byte.BeginWrite(s);
            for (i = 0; i < length; i++) {
                @byte.Write(electricityGrid[i].m_conductivity);
            }
            @byte.EndWrite();
            EncodedArray.Short @short = EncodedArray.Short.BeginWrite(s);
            for (i = 0; i < length; i++) {
                if (electricityGrid[i].m_conductivity != 0) {
                    @short.Write(electricityGrid[i].m_currentCharge);
                }
            }
            @short.EndWrite();
            EncodedArray.UShort uShort = EncodedArray.UShort.BeginWrite(s);
            for (i = 0; i < length; i++) {
                if (electricityGrid[i].m_conductivity != 0) {
                    uShort.Write(electricityGrid[i].m_extraCharge);
                }
            }
            uShort.EndWrite();
            EncodedArray.UShort uShort2 = EncodedArray.UShort.BeginWrite(s);
            for (i = 0; i < length; i++) {
                if (electricityGrid[i].m_conductivity != 0) {
                    uShort2.Write(electricityGrid[i].m_pulseGroup);
                }
            }
            uShort2.EndWrite();
            EncodedArray.Bool @bool = EncodedArray.Bool.BeginWrite(s);
            for (i = 0; i < length; i++) {
                if (electricityGrid[i].m_conductivity != 0) {
                    @bool.Write(electricityGrid[i].m_electrified);
                }
            }
            @bool.EndWrite();
            EncodedArray.Bool bool2 = EncodedArray.Bool.BeginWrite(s);
            for (i = 0; i < length; i++) {
                if (electricityGrid[i].m_conductivity != 0) {
                    bool2.Write(electricityGrid[i].m_tmpElectrified);
                }
            }
            bool2.EndWrite();

            s.WriteUInt16((uint)pulseGroupCount);
            for (i = 0; i < pulseGroupCount; i++) {
                s.WriteUInt32(pulseGroups[i].m_origCharge);
                s.WriteUInt32(pulseGroups[i].m_curCharge);
                s.WriteUInt16(pulseGroups[i].m_mergeIndex);
                s.WriteUInt16(pulseGroups[i].m_mergeCount);
                s.WriteUInt16(pulseGroups[i].m_x);
                s.WriteUInt16(pulseGroups[i].m_z);
            }

            length = pulseUnitEnd - pulseUnitStart;
            if (length < 0) {
                length += m_pulseUnits.Length;
            }
            s.WriteUInt16((uint)length);
            i = pulseUnitStart;
            while (i != pulseUnitEnd) {
                s.WriteUInt16(pulseUnits[i].m_group);
                s.WriteUInt16(pulseUnits[i].m_node);
                s.WriteUInt16(pulseUnits[i].m_x);
                s.WriteUInt16(pulseUnits[i].m_z);
                if (++i >= pulseUnits.Length) {
                    i = 0;
                }
            }
            EncodedArray.UShort uShort3 = EncodedArray.UShort.BeginWrite(s);
            for (i = 0; i < 32768; i++) {
                uShort3.Write(nodeGroups[i]);
            }
            uShort3.EndWrite();
            s.WriteInt32(processedCells);
            s.WriteInt32(conductiveCells);
            s.WriteBool(canContinue);
        }
    }
}
