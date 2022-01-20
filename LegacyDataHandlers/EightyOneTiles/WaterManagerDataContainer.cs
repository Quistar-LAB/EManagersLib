using ColossalFramework;
using ColossalFramework.IO;
using System.Reflection;
using static EManagersLib.EWaterManager;

namespace EManagersLib.LegacyDataHandlers.EightyOneTiles {
    internal class WaterManagerDataContainer : IDataContainer {
        public void AfterDeserialize(DataSerializer s) { }

        public void Deserialize(DataSerializer s) {
            int index;
            if (s.version < 241U) { // 1.3.0
                return;
            }
            WaterManager wmInstance = Singleton<WaterManager>.instance;
            FieldInfo waterPGField = typeof(WaterManager).GetField("m_waterPulseGroups", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo waterPGCField = typeof(WaterManager).GetField("m_waterPulseGroupCount", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo sewagePGField = typeof(WaterManager).GetField("m_sewagePulseGroups", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo sewagePGCField = typeof(WaterManager).GetField("m_sewagePulseGroupCount", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo heatingPGField = typeof(WaterManager).GetField("m_heatingPulseGroups", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo heatingPGCField = typeof(WaterManager).GetField("m_heatingPulseGroupCount", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo waterPUSField = typeof(WaterManager).GetField("m_waterPulseUnitStart", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo waterPUEField = typeof(WaterManager).GetField("m_waterPulseUnitEnd", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo sewagePUSField = typeof(WaterManager).GetField("m_sewagePulseUnitStart", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo sewagePUEField = typeof(WaterManager).GetField("m_sewagePulseUnitEnd", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo heatingPUSField = typeof(WaterManager).GetField("m_heatingPulseUnitStart", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo heatingPUEField = typeof(WaterManager).GetField("m_heatingPulseUnitEnd", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo processedCellField = typeof(WaterManager).GetField("m_processedCells", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo conductiveCellsField = typeof(WaterManager).GetField("m_conductiveCells", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo canContinueField = typeof(WaterManager).GetField("m_canContinue", BindingFlags.Instance | BindingFlags.NonPublic);
            WaterManager.PulseGroup[] waterPulseGroups = waterPGField.GetValue(wmInstance) as WaterManager.PulseGroup[];
            WaterManager.PulseGroup[] sewagePulseGroups = sewagePGField.GetValue(wmInstance) as WaterManager.PulseGroup[];
            WaterManager.PulseGroup[] heatingPulseGroups = heatingPGField.GetValue(wmInstance) as WaterManager.PulseGroup[];
            int waterPulseGroupCount;
            int waterPulseUnitStart;
            int waterPulseUnitEnd;
            int sewagePulseGroupCount;
            int sewagePulseUnitStart;
            int sewagePulseUnitEnd;
            int heatingPulseGroupCount;
            int heatingPulseUnitStart;
            int heatingPulseUnitEnd;
            int processedCells;
            int conductiveCells;
            bool canContinue;

            WaterManager.Cell[] waterGrid = m_waterGrid;
            int length = waterGrid.Length;
            EncodedArray.Byte byte1 = EncodedArray.Byte.BeginRead(s);
            for (index = 0; index < length; ++index)
                waterGrid[index].m_conductivity = byte1.Read();
            byte1.EndRead();
            if (s.version >= 227U) {
                EncodedArray.Byte byte2 = EncodedArray.Byte.BeginRead(s);
                for (index = 0; index < length; ++index)
                    waterGrid[index].m_conductivity2 = byte2.Read();
                byte2.EndRead();
            } else {
                for (index = 0; index < length; ++index)
                    waterGrid[index].m_conductivity2 = 0;
            }
            EncodedArray.Short short1 = EncodedArray.Short.BeginRead(s);
            for (index = 0; index < length; ++index)
                waterGrid[index].m_currentWaterPressure = waterGrid[index].m_conductivity == 0 ? (short)0 : short1.Read();
            short1.EndRead();
            EncodedArray.Short short2 = EncodedArray.Short.BeginRead(s);
            for (index = 0; index < length; ++index)
                waterGrid[index].m_currentSewagePressure = waterGrid[index].m_conductivity == 0 ? (short)0 : short2.Read();
            short2.EndRead();
            if (s.version >= 227U) {
                EncodedArray.Short short3 = EncodedArray.Short.BeginRead(s);
                for (index = 0; index < length; ++index)
                    waterGrid[index].m_currentHeatingPressure = waterGrid[index].m_conductivity2 == 0 ? (short)0 : short3.Read();
                short3.EndRead();
            } else {
                for (index = 0; index < length; ++index)
                    waterGrid[index].m_currentHeatingPressure = 0;
            }
            EncodedArray.UShort ushort1 = EncodedArray.UShort.BeginRead(s);
            for (index = 0; index < length; ++index)
                waterGrid[index].m_waterPulseGroup = waterGrid[index].m_conductivity == 0 ? ushort.MaxValue : ushort1.Read();
            ushort1.EndRead();
            EncodedArray.UShort ushort2 = EncodedArray.UShort.BeginRead(s);
            for (index = 0; index < length; ++index)
                waterGrid[index].m_sewagePulseGroup = waterGrid[index].m_conductivity == 0 ? ushort.MaxValue : ushort2.Read();
            ushort2.EndRead();
            if (s.version >= 227U) {
                EncodedArray.UShort ushort3 = EncodedArray.UShort.BeginRead(s);
                for (index = 0; index < length; ++index)
                    waterGrid[index].m_heatingPulseGroup = waterGrid[index].m_conductivity2 == 0 ? ushort.MaxValue : ushort3.Read();
                ushort3.EndRead();
            } else {
                for (index = 0; index < length; ++index)
                    waterGrid[index].m_heatingPulseGroup = ushort.MaxValue;
            }
            if (s.version >= 73U) {
                EncodedArray.UShort ushort3 = EncodedArray.UShort.BeginRead(s);
                for (index = 0; index < length; ++index)
                    waterGrid[index].m_closestPipeSegment = waterGrid[index].m_conductivity == 0 ? (ushort)0 : ushort3.Read();
                ushort3.EndRead();
            }
            if (s.version >= 227U) {
                EncodedArray.UShort ushort3 = EncodedArray.UShort.BeginRead(s);
                for (index = 0; index < length; ++index)
                    waterGrid[index].m_closestPipeSegment2 = waterGrid[index].m_conductivity2 == 0 ? (ushort)0 : ushort3.Read();
                ushort3.EndRead();
            } else {
                for (index = 0; index < length; ++index)
                    waterGrid[index].m_closestPipeSegment2 = 0;
            }
            EncodedArray.Bool bool1 = EncodedArray.Bool.BeginRead(s);
            for (index = 0; index < length; ++index)
                waterGrid[index].m_hasWater = waterGrid[index].m_conductivity != 0 && bool1.Read();
            bool1.EndRead();
            EncodedArray.Bool bool2 = EncodedArray.Bool.BeginRead(s);
            for (index = 0; index < length; ++index)
                waterGrid[index].m_hasSewage = waterGrid[index].m_conductivity != 0 && bool2.Read();
            bool2.EndRead();
            if (s.version >= 227U) {
                EncodedArray.Bool bool3 = EncodedArray.Bool.BeginRead(s);
                for (index = 0; index < length; ++index)
                    waterGrid[index].m_hasHeating = waterGrid[index].m_conductivity2 != 0 && bool3.Read();
                bool3.EndRead();
            } else {
                for (index = 0; index < length; ++index)
                    waterGrid[index].m_hasHeating = false;
            }
            EncodedArray.Bool bool4 = EncodedArray.Bool.BeginRead(s);
            for (index = 0; index < length; ++index)
                waterGrid[index].m_tmpHasWater = waterGrid[index].m_conductivity != 0 && bool4.Read();
            bool4.EndRead();
            EncodedArray.Bool bool5 = EncodedArray.Bool.BeginRead(s);
            for (index = 0; index < length; ++index)
                waterGrid[index].m_tmpHasSewage = waterGrid[index].m_conductivity != 0 && bool5.Read();
            bool5.EndRead();
            if (s.version >= 227U) {
                EncodedArray.Bool bool3 = EncodedArray.Bool.BeginRead(s);
                for (index = 0; index < length; ++index)
                    waterGrid[index].m_tmpHasHeating = waterGrid[index].m_conductivity2 != 0 && bool3.Read();
                bool3.EndRead();
            } else {
                for (index = 0; index < length; ++index)
                    waterGrid[index].m_tmpHasHeating = false;
            }
            if (s.version >= 23U) {
                EncodedArray.Byte byte2 = EncodedArray.Byte.BeginRead(s);
                for (index = 0; index < length; ++index)
                    waterGrid[index].m_pollution = waterGrid[index].m_conductivity == 0 ? (byte)0 : byte2.Read();
                byte2.EndRead();
            }
            waterPulseGroupCount = (int)s.ReadUInt16();
            for (index = 0; index < waterPulseGroupCount; ++index) {
                waterPulseGroups[index].m_origPressure = s.ReadUInt32();
                waterPulseGroups[index].m_curPressure = s.ReadUInt32();
                waterPulseGroups[index].m_collectPressure = s.version < 270U ? 0U : s.ReadUInt32();
                waterPulseGroups[index].m_mergeIndex = (ushort)s.ReadUInt16();
                waterPulseGroups[index].m_mergeCount = (ushort)s.ReadUInt16();
                waterPulseGroups[index].m_node = (ushort)s.ReadUInt16();
            }
            sewagePulseGroupCount = (int)s.ReadUInt16();
            for (index = 0; index < sewagePulseGroupCount; ++index) {
                sewagePulseGroups[index].m_origPressure = s.ReadUInt32();
                sewagePulseGroups[index].m_curPressure = s.ReadUInt32();
                sewagePulseGroups[index].m_collectPressure = s.version < 306U ? 0U : s.ReadUInt32();
                sewagePulseGroups[index].m_mergeIndex = (ushort)s.ReadUInt16();
                sewagePulseGroups[index].m_mergeCount = (ushort)s.ReadUInt16();
                sewagePulseGroups[index].m_node = (ushort)s.ReadUInt16();
            }
            if (s.version >= 227U) {
                heatingPulseGroupCount = (int)s.ReadUInt16();
                for (index = 0; index < heatingPulseGroupCount; ++index) {
                    heatingPulseGroups[index].m_origPressure = s.ReadUInt32();
                    heatingPulseGroups[index].m_curPressure = s.ReadUInt32();
                    heatingPulseGroups[index].m_mergeIndex = (ushort)s.ReadUInt16();
                    heatingPulseGroups[index].m_mergeCount = (ushort)s.ReadUInt16();
                    heatingPulseGroups[index].m_node = (ushort)s.ReadUInt16();
                }
            } else
                heatingPulseGroupCount = 0;
            int num1 = (int)s.ReadUInt16();
            waterPulseUnitStart = 0;
            waterPulseUnitEnd = num1 % m_waterPulseUnits.Length;
            for (index = 0; index < num1; ++index) {
                m_waterPulseUnits[index].m_group = (ushort)s.ReadUInt16();
                m_waterPulseUnits[index].m_node = (ushort)s.ReadUInt16();
                m_waterPulseUnits[index].m_x = (ushort)s.ReadUInt16();
                m_waterPulseUnits[index].m_z = (ushort)s.ReadUInt16();
            }
            int num2 = (int)s.ReadUInt16();
            sewagePulseUnitStart = 0;
            sewagePulseUnitEnd = num2 % m_sewagePulseUnits.Length;
            for (index = 0; index < num2; ++index) {
                m_sewagePulseUnits[index].m_group = (ushort)s.ReadUInt16();
                m_sewagePulseUnits[index].m_node = (ushort)s.ReadUInt16();
                m_sewagePulseUnits[index].m_x = (ushort)s.ReadUInt16();
                m_sewagePulseUnits[index].m_z = (ushort)s.ReadUInt16();
            }
            if (s.version >= 227U) {
                int num3 = (int)s.ReadUInt16();
                heatingPulseUnitStart = 0;
                heatingPulseUnitEnd = num3 % m_heatingPulseUnits.Length;
                for (index = 0; index < num3; ++index) {
                    m_heatingPulseUnits[index].m_group = (ushort)s.ReadUInt16();
                    m_heatingPulseUnits[index].m_node = (ushort)s.ReadUInt16();
                    m_heatingPulseUnits[index].m_x = (ushort)s.ReadUInt16();
                    m_heatingPulseUnits[index].m_z = (ushort)s.ReadUInt16();
                }
            } else {
                heatingPulseUnitStart = 0;
                heatingPulseUnitEnd = 0;
            }
            processedCells = s.ReadInt32();
            conductiveCells = s.ReadInt32();
            canContinue = s.ReadBool();

            waterPGCField.SetValue(wmInstance, waterPulseGroupCount);
            waterPUSField.SetValue(wmInstance, waterPulseUnitStart);
            waterPUEField.SetValue(wmInstance, waterPulseUnitEnd);
            sewagePGCField.SetValue(wmInstance, sewagePulseGroupCount);
            sewagePUSField.SetValue(wmInstance, sewagePulseUnitStart);
            sewagePUEField.SetValue(wmInstance, sewagePulseUnitEnd);
            heatingPGCField.SetValue(wmInstance, heatingPulseGroupCount);
            heatingPUSField.SetValue(wmInstance, heatingPulseUnitStart);
            heatingPUEField.SetValue(wmInstance, heatingPulseUnitEnd);
            processedCellField.SetValue(wmInstance, processedCells);
            conductiveCellsField.SetValue(wmInstance, conductiveCells);
            canContinueField.SetValue(wmInstance, canContinue);
        }

        public void Serialize(DataSerializer s) {
            int index;
            WaterManager wmInstance = Singleton<WaterManager>.instance;
            FieldInfo waterPGField = typeof(WaterManager).GetField("m_waterPulseGroups", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo waterPGCField = typeof(WaterManager).GetField("m_waterPulseGroupCount", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo sewagePGField = typeof(WaterManager).GetField("m_sewagePulseGroups", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo sewagePGCField = typeof(WaterManager).GetField("m_sewagePulseGroupCount", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo heatingPGField = typeof(WaterManager).GetField("m_heatingPulseGroups", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo heatingPGCField = typeof(WaterManager).GetField("m_heatingPulseGroupCount", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo waterPUSField = typeof(WaterManager).GetField("m_waterPulseUnitStart", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo waterPUEField = typeof(WaterManager).GetField("m_waterPulseUnitEnd", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo sewagePUSField = typeof(WaterManager).GetField("m_sewagePulseUnitStart", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo sewagePUEField = typeof(WaterManager).GetField("m_sewagePulseUnitEnd", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo heatingPUSField = typeof(WaterManager).GetField("m_heatingPulseUnitStart", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo heatingPUEField = typeof(WaterManager).GetField("m_heatingPulseUnitEnd", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo processedCellField = typeof(WaterManager).GetField("m_processedCells", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo conductiveCellsField = typeof(WaterManager).GetField("m_conductiveCells", BindingFlags.Instance | BindingFlags.NonPublic);
            FieldInfo canContinueField = typeof(WaterManager).GetField("m_canContinue", BindingFlags.Instance | BindingFlags.NonPublic);
            WaterManager.PulseGroup[] waterPulseGroups = waterPGField.GetValue(wmInstance) as WaterManager.PulseGroup[];
            WaterManager.PulseGroup[] sewagePulseGroups = sewagePGField.GetValue(wmInstance) as WaterManager.PulseGroup[];
            WaterManager.PulseGroup[] heatingPulseGroups = heatingPGField.GetValue(wmInstance) as WaterManager.PulseGroup[];
            int waterPulseGroupCount = (int)waterPGCField.GetValue(wmInstance);
            int waterPulseUnitStart = (int)waterPUSField.GetValue(wmInstance);
            int waterPulseUnitEnd = (int)waterPUEField.GetValue(wmInstance);
            int sewagePulseGroupCount = (int)sewagePGCField.GetValue(wmInstance);
            int sewagePulseUnitStart = (int)sewagePUSField.GetValue(wmInstance);
            int sewagePulseUnitEnd = (int)sewagePUEField.GetValue(wmInstance);
            int heatingPulseGroupCount = (int)heatingPGCField.GetValue(wmInstance);
            int heatingPulseUnitStart = (int)heatingPUSField.GetValue(wmInstance);
            int heatingPulseUnitEnd = (int)heatingPUEField.GetValue(wmInstance);
            int processedCells = (int)processedCellField.GetValue(wmInstance);
            int conductiveCells = (int)conductiveCellsField.GetValue(wmInstance);
            bool canContinue = (bool)canContinueField.GetValue(wmInstance);

            WaterManager.Cell[] cellArray = m_waterGrid;
            int length = cellArray.Length;
            EncodedArray.Byte byte1 = EncodedArray.Byte.BeginWrite(s);
            for (index = 0; index < length; ++index)
                byte1.Write(cellArray[index].m_conductivity);
            byte1.EndWrite();
            EncodedArray.Byte byte2 = EncodedArray.Byte.BeginWrite(s);
            for (index = 0; index < length; ++index)
                byte2.Write(cellArray[index].m_conductivity2);
            byte2.EndWrite();
            EncodedArray.Short short1 = EncodedArray.Short.BeginWrite(s);
            for (index = 0; index < length; ++index) {
                if (cellArray[index].m_conductivity != 0)
                    short1.Write(cellArray[index].m_currentWaterPressure);
            }
            short1.EndWrite();
            EncodedArray.Short short2 = EncodedArray.Short.BeginWrite(s);
            for (index = 0; index < length; ++index) {
                if (cellArray[index].m_conductivity != 0)
                    short2.Write(cellArray[index].m_currentSewagePressure);
            }
            short2.EndWrite();
            EncodedArray.Short short3 = EncodedArray.Short.BeginWrite(s);
            for (index = 0; index < length; ++index) {
                if (cellArray[index].m_conductivity2 != 0)
                    short3.Write(cellArray[index].m_currentHeatingPressure);
            }
            short3.EndWrite();
            EncodedArray.UShort ushort1 = EncodedArray.UShort.BeginWrite(s);
            for (index = 0; index < length; ++index) {
                if (cellArray[index].m_conductivity != 0)
                    ushort1.Write(cellArray[index].m_waterPulseGroup);
            }
            ushort1.EndWrite();
            EncodedArray.UShort ushort2 = EncodedArray.UShort.BeginWrite(s);
            for (index = 0; index < length; ++index) {
                if (cellArray[index].m_conductivity != 0)
                    ushort2.Write(cellArray[index].m_sewagePulseGroup);
            }
            ushort2.EndWrite();
            EncodedArray.UShort ushort3 = EncodedArray.UShort.BeginWrite(s);
            for (index = 0; index < length; ++index) {
                if (cellArray[index].m_conductivity2 != 0)
                    ushort3.Write(cellArray[index].m_heatingPulseGroup);
            }
            ushort3.EndWrite();
            EncodedArray.UShort ushort4 = EncodedArray.UShort.BeginWrite(s);
            for (index = 0; index < length; ++index) {
                if (cellArray[index].m_conductivity != 0)
                    ushort4.Write(cellArray[index].m_closestPipeSegment);
            }
            ushort4.EndWrite();
            EncodedArray.UShort ushort5 = EncodedArray.UShort.BeginWrite(s);
            for (index = 0; index < length; ++index) {
                if (cellArray[index].m_conductivity2 != 0)
                    ushort5.Write(cellArray[index].m_closestPipeSegment2);
            }
            ushort5.EndWrite();
            EncodedArray.Bool bool1 = EncodedArray.Bool.BeginWrite(s);
            for (index = 0; index < length; ++index) {
                if (cellArray[index].m_conductivity != 0)
                    bool1.Write(cellArray[index].m_hasWater);
            }
            bool1.EndWrite();
            EncodedArray.Bool bool2 = EncodedArray.Bool.BeginWrite(s);
            for (index = 0; index < length; ++index) {
                if (cellArray[index].m_conductivity != 0)
                    bool2.Write(cellArray[index].m_hasSewage);
            }
            bool2.EndWrite();
            EncodedArray.Bool bool3 = EncodedArray.Bool.BeginWrite(s);
            for (index = 0; index < length; ++index) {
                if (cellArray[index].m_conductivity2 != 0)
                    bool3.Write(cellArray[index].m_hasHeating);
            }
            bool3.EndWrite();
            EncodedArray.Bool bool4 = EncodedArray.Bool.BeginWrite(s);
            for (index = 0; index < length; ++index) {
                if (cellArray[index].m_conductivity != 0)
                    bool4.Write(cellArray[index].m_tmpHasWater);
            }
            bool4.EndWrite();
            EncodedArray.Bool bool5 = EncodedArray.Bool.BeginWrite(s);
            for (index = 0; index < length; ++index) {
                if (cellArray[index].m_conductivity != 0)
                    bool5.Write(cellArray[index].m_tmpHasSewage);
            }
            bool5.EndWrite();
            EncodedArray.Bool bool6 = EncodedArray.Bool.BeginWrite(s);
            for (index = 0; index < length; ++index) {
                if (cellArray[index].m_conductivity2 != 0)
                    bool6.Write(cellArray[index].m_tmpHasHeating);
            }
            bool6.EndWrite();
            EncodedArray.Byte byte3 = EncodedArray.Byte.BeginWrite(s);
            for (index = 0; index < length; ++index) {
                if (cellArray[index].m_conductivity != 0)
                    byte3.Write(cellArray[index].m_pollution);
            }
            byte3.EndWrite();
            s.WriteUInt16((uint)waterPulseGroupCount);
            for (index = 0; index < waterPulseGroupCount; ++index) {
                s.WriteUInt32(waterPulseGroups[index].m_origPressure);
                s.WriteUInt32(waterPulseGroups[index].m_curPressure);
                s.WriteUInt32(waterPulseGroups[index].m_collectPressure);
                s.WriteUInt16(waterPulseGroups[index].m_mergeIndex);
                s.WriteUInt16(waterPulseGroups[index].m_mergeCount);
                s.WriteUInt16(waterPulseGroups[index].m_node);
            }
            s.WriteUInt16((uint)sewagePulseGroupCount);
            for (index = 0; index < sewagePulseGroupCount; ++index) {
                s.WriteUInt32(sewagePulseGroups[index].m_origPressure);
                s.WriteUInt32(sewagePulseGroups[index].m_curPressure);
                s.WriteUInt32(sewagePulseGroups[index].m_collectPressure);
                s.WriteUInt16(sewagePulseGroups[index].m_mergeIndex);
                s.WriteUInt16(sewagePulseGroups[index].m_mergeCount);
                s.WriteUInt16(sewagePulseGroups[index].m_node);
            }
            s.WriteUInt16((uint)heatingPulseGroupCount);
            for (index = 0; index < heatingPulseGroupCount; ++index) {
                s.WriteUInt32(heatingPulseGroups[index].m_origPressure);
                s.WriteUInt32(heatingPulseGroups[index].m_curPressure);
                s.WriteUInt16(heatingPulseGroups[index].m_mergeIndex);
                s.WriteUInt16(heatingPulseGroups[index].m_mergeCount);
                s.WriteUInt16(heatingPulseGroups[index].m_node);
            }
            length = waterPulseUnitEnd - waterPulseUnitStart;
            if (length < 0) length += m_waterPulseUnits.Length;
            s.WriteUInt16((uint)length);
            index = waterPulseUnitStart;
            while (index != waterPulseUnitEnd) {
                s.WriteUInt16(m_waterPulseUnits[index].m_group);
                s.WriteUInt16(m_waterPulseUnits[index].m_node);
                s.WriteUInt16(m_waterPulseUnits[index].m_x);
                s.WriteUInt16(m_waterPulseUnits[index].m_z);
                if (++index >= m_waterPulseUnits.Length)
                    index = 0;
            }
            length = sewagePulseUnitEnd - sewagePulseUnitStart;
            if (length < 0)
                length += m_sewagePulseUnits.Length;
            s.WriteUInt16((uint)length);
            int index2 = sewagePulseUnitStart;
            while (index2 != sewagePulseUnitEnd) {
                s.WriteUInt16(m_sewagePulseUnits[index2].m_group);
                s.WriteUInt16(m_sewagePulseUnits[index2].m_node);
                s.WriteUInt16(m_sewagePulseUnits[index2].m_x);
                s.WriteUInt16(m_sewagePulseUnits[index2].m_z);
                if (++index2 >= m_sewagePulseUnits.Length)
                    index2 = 0;
            }
            length = heatingPulseUnitEnd - heatingPulseUnitStart;
            if (length < 0)
                length += m_heatingPulseUnits.Length;
            s.WriteUInt16((uint)length);
            int index3 = heatingPulseUnitStart;
            while (index3 != heatingPulseUnitEnd) {
                s.WriteUInt16(m_heatingPulseUnits[index3].m_group);
                s.WriteUInt16(m_heatingPulseUnits[index3].m_node);
                s.WriteUInt16(m_heatingPulseUnits[index3].m_x);
                s.WriteUInt16(m_heatingPulseUnits[index3].m_z);
                if (++index3 >= m_heatingPulseUnits.Length)
                    index3 = 0;
            }
            s.WriteInt32(processedCells);
            s.WriteInt32(conductiveCells);
            s.WriteBool(canContinue);
        }
    }
}
