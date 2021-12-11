using ColossalFramework;
using ColossalFramework.IO;
using EManagersLib;
using UnityEngine;

/* This is a special Data container to load old Prop Snapping data.
 * This was the simplest way, by using a custom legacy translator
 * during deserialization to cope with Type translation.
 */
namespace PropSnapping {
    public class Data : IDataContainer {
        public void Serialize(DataSerializer s) {
            EPropInstance[] props = EPropManager.m_props.m_buffer;
            s.WriteInt32(props.Length);
            var @ushort = EncodedArray.UShort.BeginWrite(s);
            for (var index = 0; index < props.Length; index++) {
                @ushort.Write(props[index].m_posY);
            }
            @ushort.EndWrite();
        }

        public void Deserialize(DataSerializer s) {
            TerrainManager tmInstance = Singleton<TerrainManager>.instance;
            EPropInstance[] props = EPropManager.m_props.m_buffer;
            var arraySize = s.ReadInt32();
            var @ushort = EncodedArray.UShort.BeginRead(s);
            for (var index = 0; index < arraySize; ++index) {
                props[index].m_posY = @ushort.Read();
                Vector3 position = props[index].Position;
                float terrainHeight = tmInstance.SampleDetailHeight(position);
                if (position.y != terrainHeight) {
                    props[index].m_flags |= EPropInstance.FIXEDHEIGHTFLAG;
                }
            }
            @ushort.EndRead();
        }

        public void AfterDeserialize(DataSerializer s) { }
    }
}