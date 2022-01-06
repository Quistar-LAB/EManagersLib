using ColossalFramework;
using ColossalFramework.IO;

namespace EManagersLib.LegacyDataHandlers.EightyOneTiles {
    public sealed class EightyOneDataContainer : IDataContainer {
        public void Serialize(DataSerializer s) {
            int[] areaGrid = Singleton<GameAreaManager>.instance.m_areaGrid;
            EncodedArray.Byte @byte = EncodedArray.Byte.BeginWrite(s);
            for (int i = 0; i < areaGrid.Length; i++) {
                @byte.Write((byte)areaGrid[i]);
            }
            @byte.EndWrite();
        }

        public void Deserialize(DataSerializer s) {
            int areaCount = 0;
            int[] areaGrid = new int[EGameAreaManager.CUSTOMAREACOUNT];
            EncodedArray.Byte @byte = EncodedArray.Byte.BeginRead(s);
            for (int i = 0; i < areaGrid.Length; i++) {
                areaGrid[i] = @byte.Read();
                if (areaGrid[i] > 0) areaCount++;
            }
            @byte.EndRead();
            GameAreaManager gamInstance = Singleton<GameAreaManager>.instance;
            gamInstance.m_areaGrid = areaGrid;
            gamInstance.m_areaCount = areaCount;
            gamInstance.m_maxAreaCount = EGameAreaManager.CUSTOMAREACOUNT;
        }

        public void AfterDeserialize(DataSerializer s) { }
    }
}
