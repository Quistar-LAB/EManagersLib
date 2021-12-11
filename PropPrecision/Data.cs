using ColossalFramework.IO;
using EManagersLib;

/* Special class to load old Prop Precision data
 */
namespace PropPrecision {
    public class Data : IDataContainer {
        public void Serialize(DataSerializer s) { }

        public void Deserialize(DataSerializer s) {
            EPropInstance[] props = EPropManager.m_props.m_buffer;
            var arraySize = s.ReadInt32();
            for (int i = 0; i < arraySize; i++) {
                uint propID = s.ReadUInt16();
                props[propID].m_preciseX = s.ReadUInt16();
                props[propID].m_preciseZ = s.ReadUInt16();
            }
        }

        public void AfterDeserialize(DataSerializer s) { }
    }
}