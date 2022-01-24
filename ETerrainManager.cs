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

        internal static TerrainManager.SurfaceCell GetSurfaceCell(TerrainManager tmInstance, TerrainPatch[] patches, int x, int z) {
            int patchX = EMath.Min(x / 480, 8);
            int patchZ = EMath.Min(z / 480, 8);
            int patchIndex = patchZ * 9 + patchX;
            int simDetailIndex = patches[patchIndex].m_simDetailIndex;
            if (simDetailIndex == 0) {
                return tmInstance.SampleRawSurface(x * 0.25f, z * 0.25f);
            }
            int detailOffset = (simDetailIndex - 1) * 480 * 480;
            int detailX = x - patchX * 480;
            int detailZ = z - patchZ * 480;
            if ((detailX == 0 && patchZ != 0 && patches[patchIndex - 1].m_simDetailIndex == 0) || (detailZ == 0 && patchZ != 0 && patches[patchIndex - 9].m_simDetailIndex == 0)) {
                TerrainManager.SurfaceCell result = tmInstance.SampleRawSurface(x * 0.25f, z * 0.25f);
                result.m_clipped = tmInstance.m_detailSurface[detailOffset + detailZ * 480 + detailX].m_clipped;
                return result;
            }
            if ((detailX == 479 && patchX != 8 && patches[patchIndex + 1].m_simDetailIndex == 0) || (detailZ == 479 && patchZ != 8 && patches[patchIndex + 9].m_simDetailIndex == 0)) {
                TerrainManager.SurfaceCell result2 = tmInstance.SampleRawSurface(x * 0.25f, z * 0.25f);
                result2.m_clipped = tmInstance.m_detailSurface[detailOffset + detailZ * 480 + detailX].m_clipped;
                return result2;
            }
            return tmInstance.m_detailSurface[detailOffset + detailZ * 480 + detailX];
        }
    }
}
