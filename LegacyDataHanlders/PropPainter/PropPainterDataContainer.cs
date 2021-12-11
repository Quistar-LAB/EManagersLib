using ColossalFramework.IO;
using EManagersLib;
using UnityEngine;

namespace PropPainter {
    public class PropPainterDataContainer : IDataContainer {
        // This reads the object (from bytes)
        public void Deserialize(DataSerializer s) {
            EPropInstance[] props = EPropManager.m_props.m_buffer;
            int[] ids = s.ReadInt32Array();
            for (int i = 0; i < ids.Length; i++) {
                int h = ids[i];
                if (h != 16777216) {
                    byte r = (byte)((h >> 16) & 0xff);
                    byte g = (byte)((h >> 8) & 0xff);
                    byte b = (byte)((h) & 0xff);
                    props[i].m_color = new Color32(r, g, b, 255);
                }
            }
        }

        public void AfterDeserialize(DataSerializer s) { }

        public void Serialize(DataSerializer s) { }
    }
}
