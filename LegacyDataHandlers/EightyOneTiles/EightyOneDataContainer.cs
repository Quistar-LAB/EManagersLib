using ColossalFramework;
using ColossalFramework.IO;

namespace EManagersLib.LegacyDataHandlers.EightyOneTiles {
    public sealed class EightyOneDataContainer : IDataContainer {
        public void Serialize(DataSerializer s) {
            int[] areaGrid = Singleton<GameAreaManager>.instance.m_areaGrid;
            var @byte = EncodedArray.Byte.BeginWrite(s);
            for (var i = 0; i < areaGrid.Length; i++) {
                @byte.Write((byte)areaGrid[i]);
            }
            @byte.EndWrite();
        }

        public void Deserialize(DataSerializer s) {
            int[] areaGrid = Singleton<GameAreaManager>.instance.m_areaGrid;
            var @byte = EncodedArray.Byte.BeginRead(s);
            for (var i = EGameAreaManager.DEFAULTAREACOUNT; i < areaGrid.Length; i++) {
                areaGrid[i] = @byte.Read();
            }
            @byte.EndRead();
        }

        public void AfterDeserialize(DataSerializer s) { }
    }
}
