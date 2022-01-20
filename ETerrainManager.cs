using System.Runtime.CompilerServices;

namespace EManagersLib {
    public static class ETerrainManager {
        internal static float GetUnlockableTerrainFlatness(TerrainPatch[] patches) {
            const float areaCount = EGameAreaManager.CUSTOMAREACOUNT;
            const int gridSize = EGameAreaManager.CUSTOMGRIDSIZE;
            float num = 0f;
            for (int z = 0; z < gridSize; z++) {
                for (int x = 0; x < gridSize; x++) {
                    num += patches[z * gridSize + x].m_flatness;
                }
            }
            return num / areaCount;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static float GetTileFlatness(TerrainPatch[] patches, int x, int z) => patches[z * EGameAreaManager.CUSTOMGRIDSIZE + x].m_flatness;
    }
}
