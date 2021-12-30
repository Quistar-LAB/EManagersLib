using ColossalFramework;
using ColossalFramework.IO;
using EManagersLib.LegacyDataHandlers.EightyOneTiles;
using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace EManagersLib {
    public static class EGameAreaManager {
        private const string EIGHTYONE_KEY = "fakeGAM";
        private const uint SaveFormatVersion = 1;
        internal const int DEFAULTRESOLUTION = 1920;
        internal const int DEFAULTAREACOUNT = 25;
        internal const int DEFAULTGRIDSIZE = 5;
        internal const int MINAREACOUNT = 9;
        internal const int CUSTOMAREACOUNT = 81;
        internal const int CUSTOMGRIDSIZE = 9;
        internal const int CUSTOMAREATEXSIZE = 10;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void GetTileXZ(GameAreaManager _, int tile, out int x, out int z) {
            x = tile % CUSTOMGRIDSIZE;
            z = tile / CUSTOMGRIDSIZE;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int GetTileIndex(GameAreaManager _, int x, int z) => z * CUSTOMGRIDSIZE + x;

        private static Type EightyOneDataLegacyHandler(string _) => typeof(EightyOneDataContainer);

        internal static void IntegratedDeserialize() {
            try {
                if (Singleton<SimulationManager>.instance.m_serializableDataStorage.TryGetValue(EIGHTYONE_KEY, out byte[] data)) {
                    EUtils.ELog("Found 81 Tiles data, loading...");
                    using (MemoryStream stream = new MemoryStream(data)) {
                        DataSerializer.Deserialize<EightyOneDataContainer>(stream, DataSerializer.Mode.Memory, EightyOneDataLegacyHandler);
                    }
                    EUtils.ELog("Loaded " + (data.Length / 1024) + "kb of 81 Tiles data");
                } else {
                    EUtils.ELog("No 81 Tiles data found");
                }
            } catch (Exception e) {
                UnityEngine.Debug.LogException(e);
            }
        }

        internal static void Serialize() {
            byte[] data;
            using (var stream = new MemoryStream()) {
                DataSerializer.Serialize(stream, DataSerializer.Mode.Memory, SaveFormatVersion, new EightyOneDataContainer());
                data = stream.ToArray();
            }
            ESerializableData.SaveData(EIGHTYONE_KEY, data);
            EUtils.ELog($"Saved {data.Length} bytes of 81 Tiles data");
        }

    }
}
