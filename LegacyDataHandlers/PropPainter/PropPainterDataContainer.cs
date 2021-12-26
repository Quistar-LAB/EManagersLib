using ColossalFramework.IO;
using EManagersLib;
using UnityEngine;

namespace PropPainter {
    public sealed class PropPainterDataContainer : IDataContainer {
        public unsafe void Deserialize(DataSerializer s) {
            const int DEFAULT_PROP_COUNT = 65536;
            EPropInstance[] props = EPropManager.m_props.m_buffer;
            uint len = s.ReadUInt24();
            if (len == DEFAULT_PROP_COUNT) {
                fixed (EPropInstance* pProp = EPropManager.m_props.m_buffer) {
                    EPropInstance* prop = pProp;
                    for (int i = 0; i < DEFAULT_PROP_COUNT; i++, prop++) {
                        byte a = (byte)s.ReadUInt8();
                        byte r = (byte)s.ReadUInt8();
                        byte g = (byte)s.ReadUInt8();
                        byte b = (byte)s.ReadUInt8();
                        if (a != 0x01 && r != 0x00 && g != 0x00 && b != 0x00) {
                            prop->m_color = new Color32(r, g, b, 255);
                        }
                    }
                }
            }
        }

        public void AfterDeserialize(DataSerializer s) { }

        public void Serialize(DataSerializer s) { }
    }
}
