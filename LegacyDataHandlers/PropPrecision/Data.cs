using ColossalFramework.IO;
using EManagersLib;

/* Special class to load old Prop Precision data
 */
namespace PropPrecision {
    public sealed class Data : IDataContainer {
        public void Deserialize(DataSerializer s) {
            EPropInstance[] props = EPropManager.m_props.m_buffer;
            var arraySize = s.ReadInt32();
            for (int i = 0; i < arraySize; i++) {
                uint propID = s.ReadUInt16();
                float preciseX = s.ReadUInt16();
                float preciseZ = s.ReadUInt16();
                short posX = props[propID].m_posX;
                short posZ = props[propID].m_posZ;
                preciseX = (posX > 0 ? posX + preciseX / ushort.MaxValue : posX - preciseX / ushort.MaxValue) * 0.263671875f;
                preciseZ = (posZ > 0 ? posZ + preciseZ / ushort.MaxValue : posZ - preciseZ / ushort.MaxValue) * 0.263671875f;
                props[propID].m_preciseX = preciseX * 3.79259253f - posX;
                props[propID].m_preciseZ = preciseZ * 3.79259253f - posZ;
            }
        }

        public void Serialize(DataSerializer s) { }

        public void AfterDeserialize(DataSerializer s) { }
    }
}