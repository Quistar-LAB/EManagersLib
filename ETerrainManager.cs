using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ColossalFramework;
using UnityEngine;

namespace EManagersLib {
    public static class ETerrainManager {
		// TODO: Clamping these number should not be thet solution
		// AnimalAI seems to be the culprit here, need more investigation
		internal static TerrainManager.SurfaceCell GetSurfaceCell(TerrainManager tmInstance, int x, int z) {
			TerrainPatch[] patches = tmInstance.m_patches;
			int x1 = EMath.Clamp(x / 480, 0, 8); //EMath.Min(x / 480, 8);
			int z1 = EMath.Clamp(z / 480, 0, 8); //EMath.Min(z / 480, 8);
			int index = z1 * 9 + x1;
			int simDetailIndex = patches[index].m_simDetailIndex;
			if (simDetailIndex == 0) {
				return tmInstance.SampleRawSurface(x * 0.25f, z * 0.25f);
			}
			int num4 = (simDetailIndex - 1) * 480 * 480;
			int num5 = x - x1 * 480;
			int num6 = z - z1 * 480;
			int max = tmInstance.m_detailSurface.Length - 1;
			if ((num5 == 0 && x1 != 0 && patches[index - 1].m_simDetailIndex == 0) || (num6 == 0 && z1 != 0 && patches[index - 9].m_simDetailIndex == 0)) {
				TerrainManager.SurfaceCell result = tmInstance.SampleRawSurface(x * 0.25f, z * 0.25f);
				result.m_clipped = tmInstance.m_detailSurface[EMath.Clamp(num4 + num6 * 480 + num5, 0, max)].m_clipped;
				return result;
			}
			if ((num5 == 479 && x1 != 8 && patches[index + 1].m_simDetailIndex == 0) || (num6 == 479 && z1 != 8 && patches[index + 9].m_simDetailIndex == 0)) {
				TerrainManager.SurfaceCell result2 = tmInstance.SampleRawSurface(x * 0.25f, z * 0.25f);
				result2.m_clipped = tmInstance.m_detailSurface[EMath.Clamp(num4 + num6 * 480 + num5, 0, max)].m_clipped;
				return result2;
			}
			return tmInstance.m_detailSurface[EMath.Clamp(num4 + num6 * 480 + num5, 0, max)];
		}
	}
}
