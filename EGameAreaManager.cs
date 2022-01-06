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

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void GetStartTile(int startTile, out int x, out int z) {
            x = startTile % CUSTOMGRIDSIZE;
            z = startTile / CUSTOMGRIDSIZE;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static int RecalcStartTile(ref int startTile) {
            int x = startTile % DEFAULTGRIDSIZE;
            int z = startTile / DEFAULTGRIDSIZE;
            startTile = (z + 2) * CUSTOMGRIDSIZE + (x + 2);
            return startTile;
        }

        internal static void OnLevelLoaded() {
            TerrainManager tmInstance = Singleton<TerrainManager>.instance;
            //for (int x = 0; x < 9; x++) {
            //    for (int z = 0; z < 9; z++) {
            //        tmInstance.SetDetailedPatch(x, z);
            //    }
            //}
        }

        private static Type EightyOneDataLegacyHandler(string _) => typeof(EightyOneDataContainer);

        internal static void IntegratedDeserialize() {
            GameAreaManager gamInstance = Singleton<GameAreaManager>.instance;
            gamInstance.m_maxAreaCount = CUSTOMAREACOUNT;
            try {
                if (Singleton<SimulationManager>.instance.m_serializableDataStorage.TryGetValue(EIGHTYONE_KEY, out byte[] data)) {
                    EUtils.ELog("Found 81 Tiles data, loading...");
                    using (MemoryStream stream = new MemoryStream(data)) {
                        DataSerializer.Deserialize<EightyOneDataContainer>(stream, DataSerializer.Mode.Memory, EightyOneDataLegacyHandler);
                    }
                    EUtils.ELog(@"Loaded " + (data.Length / 1024f) + @"kb of 81 Tiles data");
                } else {
                    int[] newAreaGrid = new int[CUSTOMAREACOUNT];
                    int[] areaGrid = gamInstance.m_areaGrid;
                    for (int i = 0; i < GameAreaManager.AREAGRID_RESOLUTION; i++) {
                        for (int j = 0; j < GameAreaManager.AREAGRID_RESOLUTION; j++) {
                            int grid = areaGrid[i * GameAreaManager.AREAGRID_RESOLUTION + j];
                            newAreaGrid[(i + 2) * CUSTOMGRIDSIZE + (j + 2)] = grid;
                            if (grid > 0) {
                                EUtils.ELog($"Found start tile at i={i} j={j}");
                            }
                        }
                    }
                    gamInstance.m_areaGrid = newAreaGrid;
                    EUtils.ELog("No 81 Tiles data found, Converted default 25 tiles to 81 tiles");
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
            EUtils.ELog($"Saved {data.Length / 1024f}kb of 81 Tiles data");
        }

    }
}
