using ColossalFramework;
using ColossalFramework.IO;
using EManagersLib.LegacyDataHandlers.EightyOneTiles;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace EManagersLib {
    public static class EDistrictManager {
        private const string CUSTOMDISTRICTKEY = @"fakeDM"; /* Legacy name, will re-use it */
        private const uint FORMATVERSION = 3u;
        public const int DEFAULTGRID_RESOLUTION = 512;
        public const float DISTRICTGRID_CELL_SIZE = 19.2f;
        public const int DISTRICTGRID_RESOLUTION = 900;
        public const int MAX_DISTRICT_COUNT = 128;
        public const float CELL_AREA_TO_SQUARE = 5.76f;
        internal struct TempDistrictData {
            public int m_averageX;
            public int m_averageZ;
            public int m_bestScore;
            public int m_divider;
            public int m_bestLocation;
        }
        private static readonly int[] m_distanceBuffer = new int[DISTRICTGRID_RESOLUTION * DISTRICTGRID_RESOLUTION / 4];
        private static readonly int[] m_indexBuffer = new int[DISTRICTGRID_RESOLUTION * DISTRICTGRID_RESOLUTION / 4];
        internal static readonly TempDistrictData[] m_tempData = new TempDistrictData[128];

        internal static void OnLevelLoaded() {
            DistrictManager dmInstance = Singleton<DistrictManager>.instance;
            dmInstance.AreaModified(0, 0, DISTRICTGRID_RESOLUTION - 1, DISTRICTGRID_RESOLUTION - 1, true);
            dmInstance.ParksAreaModified(0, 0, DISTRICTGRID_RESOLUTION - 1, DISTRICTGRID_RESOLUTION - 1, true);
            dmInstance.NamesModified();
            dmInstance.ParkNamesModified();
        }

        internal static void EnsureCapacity(DistrictManager dmInstance, ref Color32[] colorBuffer,
            ref Texture2D districtTexture1, ref Texture2D districtTexture2, ref Texture2D parkTexture1, ref Texture2D parkTexture2,
            ref int districtsModifiedX2, ref int districtsModifiedZ2, ref int parksModifiedX2, ref int parksModifiedZ2) {
            if (colorBuffer.Length != DISTRICTGRID_RESOLUTION * DISTRICTGRID_RESOLUTION) {
                EUtils.ELog($"Buffer did not initialize to correct size, probably a mod overrode this mod's patch to Awake, or called DistrictManager's Singleton before the patch");
                colorBuffer = new Color32[DISTRICTGRID_RESOLUTION * DISTRICTGRID_RESOLUTION];
                districtTexture1 = new Texture2D(DISTRICTGRID_RESOLUTION, DISTRICTGRID_RESOLUTION, TextureFormat.ARGB32, false, true);
                districtTexture2 = new Texture2D(DISTRICTGRID_RESOLUTION, DISTRICTGRID_RESOLUTION, TextureFormat.ARGB32, false, true);
                districtTexture1.wrapMode = TextureWrapMode.Clamp;
                districtTexture2.wrapMode = TextureWrapMode.Clamp;
                parkTexture1 = new Texture2D(DISTRICTGRID_RESOLUTION, DISTRICTGRID_RESOLUTION, TextureFormat.ARGB32, false, true);
                parkTexture2 = new Texture2D(DISTRICTGRID_RESOLUTION, DISTRICTGRID_RESOLUTION, TextureFormat.ARGB32, false, true);
                parkTexture1.wrapMode = TextureWrapMode.Clamp;
                parkTexture2.wrapMode = TextureWrapMode.Clamp;
                districtsModifiedX2 = DISTRICTGRID_RESOLUTION - 1;
                districtsModifiedZ2 = DISTRICTGRID_RESOLUTION - 1;
                parksModifiedX2 = DISTRICTGRID_RESOLUTION - 1;
                parksModifiedZ2 = DISTRICTGRID_RESOLUTION - 1;
            }
        }

        private static Type EightyOneDistrictLegacyHandler(string _) => typeof(EightyOneDistrictDataContainer);
        internal unsafe static void IntegratedDeserialize() {
            try {
                const int RESOLUTION = DISTRICTGRID_RESOLUTION * DISTRICTGRID_RESOLUTION;
                const int diff = (DISTRICTGRID_RESOLUTION - DEFAULTGRID_RESOLUTION) / 2;
                int i, j;
                DistrictManager dmInstance = Singleton<DistrictManager>.instance;
                DistrictManager.Cell[] districtGrid = dmInstance.m_districtGrid;
                DistrictManager.Cell[] parkGrid = dmInstance.m_parkGrid;
                DistrictManager.Cell[] newDistrict = new DistrictManager.Cell[RESOLUTION];
                DistrictManager.Cell[] newPark = new DistrictManager.Cell[RESOLUTION];
                fixed (DistrictManager.Cell* pDistrict = &newDistrict[0])
                fixed (DistrictManager.Cell* pPark = &newPark[0]) {
                    DistrictManager.Cell* buf = pDistrict;
                    for (i = 0; i < RESOLUTION; i++, buf++) {
                        buf->m_district1 = 0;
                        buf->m_district2 = 1;
                        buf->m_district3 = 2;
                        buf->m_district4 = 3;
                        buf->m_alpha1 = 255;
                        buf->m_alpha2 = 0;
                        buf->m_alpha3 = 0;
                        buf->m_alpha4 = 0;
                    }
                    buf = pPark;
                    for (i = 0; i < RESOLUTION; i++, buf++) {
                        buf->m_district1 = 0;
                        buf->m_district2 = 1;
                        buf->m_district3 = 2;
                        buf->m_district4 = 3;
                        buf->m_alpha1 = 255;
                        buf->m_alpha2 = 0;
                        buf->m_alpha3 = 0;
                        buf->m_alpha4 = 0;
                    }
                    if (Singleton<SimulationManager>.instance.m_serializableDataStorage.TryGetValue(CUSTOMDISTRICTKEY, out byte[] data)) {
                        EUtils.ELog("Found 81 Tiles District data, loading...");
                        dmInstance.m_districtGrid = newDistrict;
                        dmInstance.m_parkGrid = newPark;
                        using (MemoryStream stream = new MemoryStream(data)) {
                            DataSerializer.Deserialize<EightyOneDistrictDataContainer>(stream, DataSerializer.Mode.Memory, EightyOneDistrictLegacyHandler);
                        }
                        EUtils.ELog(@"Loaded " + (data.Length / 1024f) + @"kb of 81 Tiles District data");
                    } else {
                        for (i = 0, buf = pDistrict; i < DEFAULTGRID_RESOLUTION; i++) {
                            for (j = 0; j < DEFAULTGRID_RESOLUTION; j++) {
                                *(buf + ((j + diff) * DISTRICTGRID_RESOLUTION + (i + diff))) = districtGrid[j * DEFAULTGRID_RESOLUTION + i];
                            }
                        }
                        for (i = 0, buf = pPark; i < DEFAULTGRID_RESOLUTION; i++) {
                            for (j = 0; j < DEFAULTGRID_RESOLUTION; j++) {
                                *(buf + ((j + diff) * DISTRICTGRID_RESOLUTION + (i + diff))) = parkGrid[j * DEFAULTGRID_RESOLUTION + i];
                            }
                        }
                        dmInstance.m_districtGrid = newDistrict;
                        dmInstance.m_parkGrid = newPark;
                    }
                    EUtils.ELog(@"No 81 Tiles District data found");
                }
            } catch (Exception e) {
                UnityEngine.Debug.LogException(e);
            }
        }

        internal static void Serialize() {
            byte[] data;
            using (var stream = new MemoryStream()) {
                DataSerializer.Serialize(stream, DataSerializer.Mode.Memory, FORMATVERSION, new EightyOneDistrictDataContainer());
                data = stream.ToArray();
            }
            ESerializableData.SaveData(CUSTOMDISTRICTKEY, data);
            EUtils.ELog($"Saved {data.Length / 1024f}kb of 81 Tiles District data");
        }

        internal static DistrictManager.Cell[] RepackBuffer(DistrictManager.Cell[] buffer) {
            const int diff = (DISTRICTGRID_RESOLUTION - DEFAULTGRID_RESOLUTION) / 2;
            DistrictManager.Cell[] newBuf = new DistrictManager.Cell[DEFAULTGRID_RESOLUTION * DEFAULTGRID_RESOLUTION];
            for (int x = 0; x < DEFAULTGRID_RESOLUTION; x++) {
                for (int z = 0; z < DEFAULTGRID_RESOLUTION; z++) {
                    newBuf[z * DEFAULTGRID_RESOLUTION + x] = buffer[(z + diff) * DISTRICTGRID_RESOLUTION + x + diff];
                }
            }
            return newBuf;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void MoveParkProps(DistrictPark[] parks, int cellX, int cellZ, byte src, byte dest) {
#if ENABLEEIGHTYONE
            const float halfGrid = DISTRICTGRID_RESOLUTION / 2f;
            const int gridSize = DISTRICTGRID_RESOLUTION - 1;
#else
            const float halfGrid = DEFAULTGRID_RESOLUTION / 2f;
            const int gridSize = DEFAULTGRID_RESOLUTION - 1;
#endif
            int startX = EMath.Max((int)((cellX - halfGrid) * (19.2f / 64f) + 135f), 0);
            int startZ = EMath.Max((int)((cellZ - halfGrid) * (19.2f / 64f) + 135f), 0);
            int endX = EMath.Min((int)((cellX - halfGrid + 1f) * (19.2f / 64f) + 135f), 269);
            int endZ = EMath.Min((int)((cellZ - halfGrid + 1f) * (19.2f / 64f) + 135f), 269);
            EPropInstance[] props = EPropManager.m_props.m_buffer;
            uint[] propGrid = EPropManager.m_propGrid;
            for (int i = startZ; i <= endZ; i++) {
                for (int j = startX; j <= endX; j++) {
                    uint propID = propGrid[i * 270 + j];
                    while (propID != 0) {
                        if ((props[propID].m_flags & EPropInstance.BLOCKEDFLAG) == 0) {
                            Vector3 position = props[propID].Position;
                            int x = EMath.Clamp((int)(position.x / 19.2f + halfGrid), 0, gridSize);
                            int y = EMath.Clamp((int)(position.z / 19.2f + halfGrid), 0, gridSize);
                            if (x == cellX && y == cellZ) {
                                parks[src].m_propCount--;
                                parks[dest].m_propCount++;
                            }
                        }
                        propID = props[propID].m_nextGridProp;
                    }
                }
            }
        }

        internal static void NamesModified(DistrictManager dmInstance, ref bool namesModified) {
            const int halfGrid = DISTRICTGRID_RESOLUTION / 2;
            const float resolution = 19.2f * halfGrid;
            TempDistrictData[] tempData = m_tempData;
            NamesModified(dmInstance.m_districtGrid, tempData);
            for (int i = 0; i < 128; i++) {
                int bestLocation = tempData[i].m_bestLocation;
                Vector3 vector;
                vector.x = 19.2f * (bestLocation % halfGrid) * 2f - resolution;
                vector.y = 0f;
                vector.z = 19.2f * (bestLocation / halfGrid) * 2f - resolution;
                vector.y = Singleton<TerrainManager>.instance.SampleRawHeightSmoothWithWater(vector, false, 0f);
                dmInstance.m_districts.m_buffer[i].m_nameLocation = vector;
            }
            namesModified = true;
        }

        internal static void ParkNamesModified(DistrictManager dmInstance, ref bool namesModified) {
            const int halfGrid = DISTRICTGRID_RESOLUTION / 2;
            const float resolution = 19.2f * halfGrid;
            TempDistrictData[] tempData = m_tempData;
            NamesModified(dmInstance.m_parkGrid, tempData);
            for (int i = 0; i < 128; i++) {
                int bestLocation = tempData[i].m_bestLocation;
                Vector3 vector;
                vector.x = 19.2f * (bestLocation % halfGrid) * 2f - resolution;
                vector.y = 0f;
                vector.z = 19.2f * (bestLocation / halfGrid) * 2f - resolution;
                vector.y = Singleton<TerrainManager>.instance.SampleRawHeightSmoothWithWater(vector, false, 0f);
                dmInstance.m_parks.m_buffer[i].m_nameLocation = vector;
            }
            namesModified = true;
        }

        internal static void NamesModified(DistrictManager.Cell[] grid, TempDistrictData[] tempData) {
            const int startGrid = 2;
            const int halfGrid = DISTRICTGRID_RESOLUTION / 2;
            const int doubleGrid = DISTRICTGRID_RESOLUTION * 2;
            const int totalRes = DISTRICTGRID_RESOLUTION * DISTRICTGRID_RESOLUTION;
            const int halfRes = totalRes / 2;
            int x, z, curIndex;
            byte district;
            int[] distanceBuffer = m_distanceBuffer;
            int[] indexBuffer = m_indexBuffer;
            for (x = 0; x < distanceBuffer.Length; x++) {
                distanceBuffer[x] = 0;
            }
            for (x = 0; x < 128; x++) {
                tempData[x] = default;
            }
            int startIndex = 0;
            int lastIndex = 0;
            for (z = 0; z < halfGrid; z++) {
                for (x = 0; x < halfGrid; x++) {
                    int gridIndex = z * doubleGrid + x * startGrid;
                    district = grid[gridIndex].m_district1;
                    if (district != 0 && (x == 0 || z == 0 || x == halfGrid - 1 || z == halfGrid - 1 ||
                        grid[gridIndex - doubleGrid].m_district1 != district || grid[gridIndex - startGrid].m_district1 != district ||
                        grid[gridIndex + startGrid].m_district1 != district || grid[gridIndex + doubleGrid].m_district1 != district)) {
                        curIndex = z * halfGrid + x;
                        distanceBuffer[curIndex] = 1;
                        indexBuffer[lastIndex] = curIndex;
                        lastIndex = (lastIndex + 1) % (halfGrid * halfGrid);
                        tempData[district].m_averageX += x;
                        tempData[district].m_averageZ += z;
                        tempData[district].m_divider++;
                    }
                }
            }
            for (x = 0; x < 128; x++) {
                int divider = tempData[x].m_divider;
                if (divider != 0) {
                    tempData[x].m_averageX = (tempData[x].m_averageX + (divider >> 1)) / divider;
                    tempData[x].m_averageZ = (tempData[x].m_averageZ + (divider >> 1)) / divider;
                }
            }
            while (startIndex != lastIndex) {
                int index = indexBuffer[startIndex];
                startIndex = (startIndex + 1) % (halfGrid * halfGrid);
                x = index % halfGrid;
                z = index / halfGrid;
                int gridIndex = z * doubleGrid + x * startGrid;
                district = grid[gridIndex].m_district1;
                int deltaX = x - tempData[district].m_averageX;
                int deltaZ = z - tempData[district].m_averageZ;
                int best = totalRes - halfRes / distanceBuffer[index] - deltaX * deltaX - deltaZ * deltaZ;
                if (best > tempData[district].m_bestScore) {
                    tempData[district].m_bestScore = best;
                    tempData[district].m_bestLocation = index;
                }
                curIndex = index - 1;
                if (x > 0 && distanceBuffer[curIndex] == 0 && grid[gridIndex - startGrid].m_district1 == district) {
                    distanceBuffer[curIndex] = distanceBuffer[index] + 1;
                    indexBuffer[lastIndex] = curIndex;
                    lastIndex = (lastIndex + 1) % (halfGrid * halfGrid);
                }
                curIndex = index + 1;
                if (x < 255 && distanceBuffer[curIndex] == 0 && grid[gridIndex + startGrid].m_district1 == district) {
                    distanceBuffer[curIndex] = distanceBuffer[index] + 1;
                    indexBuffer[lastIndex] = curIndex;
                    lastIndex = (lastIndex + 1) % (halfGrid * halfGrid);
                }
                curIndex = index - halfGrid;
                if (z > 0 && distanceBuffer[curIndex] == 0 && grid[gridIndex - doubleGrid].m_district1 == district) {
                    distanceBuffer[curIndex] = distanceBuffer[index] + 1;
                    indexBuffer[lastIndex] = curIndex;
                    lastIndex = (lastIndex + 1) % (halfGrid * halfGrid);
                }
                curIndex = index + halfGrid;
                if (z < 255 && distanceBuffer[curIndex] == 0 && grid[gridIndex + doubleGrid].m_district1 == district) {
                    distanceBuffer[curIndex] = distanceBuffer[index] + 1;
                    indexBuffer[lastIndex] = curIndex;
                    lastIndex = (lastIndex + 1) % (halfGrid * halfGrid);
                }
            }
        }

        internal static byte SampleDistrict(Vector3 worldPos, DistrictManager.Cell[] grid) {
            int b1 = 0, __b1;
            int b2 = 0, __b2;
            int b3 = 0, __b3;
            int b4 = 0, __b4;
            int b5 = 0, __b5;
            int b6 = 0, __b6;
            int b7 = 0, __b7;
            void __SetBitAlphas(int district, int alpha) {
                __b1 = (district & 1) != 0 ? EMath.Max(__b1, alpha - 128) : EMath.Min(__b1, 128 - alpha);
                __b2 = (district & 2) != 0 ? EMath.Max(__b2, alpha - 128) : EMath.Min(__b2, 128 - alpha);
                __b3 = (district & 4) != 0 ? EMath.Max(__b3, alpha - 128) : EMath.Min(__b3, 128 - alpha);
                __b4 = (district & 8) != 0 ? EMath.Max(__b4, alpha - 128) : EMath.Min(__b4, 128 - alpha);
                __b5 = (district & 16) != 0 ? EMath.Max(__b5, alpha - 128) : EMath.Min(__b5, 128 - alpha);
                __b6 = (district & 32) != 0 ? EMath.Max(__b6, alpha - 128) : EMath.Min(__b6, 128 - alpha);
                __b7 = (district & 64) != 0 ? EMath.Max(__b7, alpha - 128) : EMath.Min(__b7, 128 - alpha);
            }
            void SetBitAlphas(DistrictManager.Cell cell, int alpha) {
                __b1 = 0;
                __b2 = 0;
                __b3 = 0;
                __b4 = 0;
                __b5 = 0;
                __b6 = 0;
                __b7 = 0;
                __SetBitAlphas(cell.m_district1, cell.m_alpha1);
                __SetBitAlphas(cell.m_district2, cell.m_alpha2);
                __SetBitAlphas(cell.m_district3, cell.m_alpha3);
                __SetBitAlphas(cell.m_district4, cell.m_alpha4);
                b1 += __b1 * alpha;
                b2 += __b2 * alpha;
                b3 += __b3 * alpha;
                b4 += __b4 * alpha;
                b5 += __b5 * alpha;
                b6 += __b6 * alpha;
                b7 += __b7 * alpha;
            }
            const int halfGrid = DISTRICTGRID_RESOLUTION / 2;
            int x = EMath.RoundToInt(worldPos.x * 13.333333f + (halfGrid * halfGrid) - halfGrid);
            int z = EMath.RoundToInt(worldPos.z * 13.333333f + (halfGrid * halfGrid) - halfGrid);
            int x1 = EMath.Clamp((int)(worldPos.x / 19.2f + halfGrid), 0, DISTRICTGRID_RESOLUTION - 1);
            int z1 = EMath.Clamp((int)(worldPos.z / 19.2f + halfGrid), 0, DISTRICTGRID_RESOLUTION - 1);
            int x2 = EMath.Min(x1 + 1, DISTRICTGRID_RESOLUTION - 1);
            int z2 = EMath.Min(z1 + 1, DISTRICTGRID_RESOLUTION - 1);
            SetBitAlphas(grid[z1 * DISTRICTGRID_RESOLUTION + x1], (255 - (x & 255)) * (255 - (z & 255)));
            SetBitAlphas(grid[z1 * DISTRICTGRID_RESOLUTION + x2], (x & 255) * (255 - (z & 255)));
            SetBitAlphas(grid[z2 * DISTRICTGRID_RESOLUTION + x1], (255 - (x & 255)) * (z & 255));
            SetBitAlphas(grid[z2 * DISTRICTGRID_RESOLUTION + x2], (x & 255) * (z & 255));
            byte b = 0;
            if (b1 > 0) {
                b |= 1;
            }
            if (b2 > 0) {
                b |= 2;
            }
            if (b3 > 0) {
                b |= 4;
            }
            if (b4 > 0) {
                b |= 8;
            }
            if (b5 > 0) {
                b |= 16;
            }
            if (b6 > 0) {
                b |= 32;
            }
            if (b7 > 0) {
                b |= 64;
            }
            return b;
        }
    }
}
