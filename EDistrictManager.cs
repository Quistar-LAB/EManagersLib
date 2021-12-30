using System.Runtime.CompilerServices;
using UnityEngine;

namespace EManagersLib {
    public static class EDistrictManager {
        public const int DEFAULTGRID_RESULTION = 512;
        public const float DISTRICTGRID_CELL_SIZE = 19.2f;
        public const int DISTRICTGRID_RESOLUTION = 900;
        public const int MAX_DISTRICT_COUNT = 128;
        public const float CELL_AREA_TO_SQUARE = 5.76f;

        private static int ID_DistrictsA1;
        private static int ID_DistrictsA2;
        private static int ID_DistrictsB1;
        private static int ID_DistrictsB2;
        private static int ID_EdgeColorA;
        private static int ID_AreaColorA;
        private static int ID_EdgeColorB;
        private static int ID_AreaColorB;
        private static int ID_ColorR;
        private static int ID_ColorG;
        private static int ID_ColorB;
        private static int ID_ColorA;
        private static int ID_ColorRG;
        private static int ID_DistrictMapping;
        private static int ID_Highlight1;
        private static int ID_Highlight2;

        public static void Awake(DistrictManager districtManager) {
            districtManager.m_districtGrid = new DistrictManager.Cell[DISTRICTGRID_RESOLUTION * DISTRICTGRID_RESOLUTION];
            districtManager.m_parkGrid = new DistrictManager.Cell[DISTRICTGRID_RESOLUTION * DISTRICTGRID_RESOLUTION];
            //districtManager.m_color
            ID_DistrictsA1 = Shader.PropertyToID("_DistrictsA1");
            ID_DistrictsA2 = Shader.PropertyToID("_DistrictsA2");
            ID_DistrictsB1 = Shader.PropertyToID("_DistrictsB1");
            ID_DistrictsB2 = Shader.PropertyToID("_DistrictsB2");
            ID_DistrictMapping = Shader.PropertyToID("_DistrictMapping");
            ID_Highlight1 = Shader.PropertyToID("_Highlight1");
            ID_Highlight2 = Shader.PropertyToID("_Highlight2");
            ID_EdgeColorA = Shader.PropertyToID("_EdgeColorA");
            ID_AreaColorA = Shader.PropertyToID("_AreaColorA");
            ID_EdgeColorB = Shader.PropertyToID("_EdgeColorB");
            ID_AreaColorB = Shader.PropertyToID("_AreaColorB");
            ID_ColorR = Shader.PropertyToID("_ColorR");
            ID_ColorG = Shader.PropertyToID("_ColorG");
            ID_ColorB = Shader.PropertyToID("_ColorB");
            ID_ColorA = Shader.PropertyToID("_ColorA");
            ID_ColorRG = Shader.PropertyToID("_ColorRG");
        }


        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void MoveParkProps(DistrictPark[] parks, int cellX, int cellZ, byte src, byte dest) {
            int startX = EMath.Max((int)((cellX - 256f) * (19.2f / 64f) + 135f), 0);
            int startZ = EMath.Max((int)((cellZ - 256f) * (19.2f / 64f) + 135f), 0);
            int endX = EMath.Min((int)((cellX - 256f + 1f) * (19.2f / 64f) + 135f), 269);
            int endZ = EMath.Min((int)((cellZ - 256f + 1f) * (19.2f / 64f) + 135f), 269);
            EPropInstance[] props = EPropManager.m_props.m_buffer;
            uint[] propGrid = EPropManager.m_propGrid;
            for (int i = startZ; i <= endZ; i++) {
                for (int j = startX; j <= endX; j++) {
                    uint propID = propGrid[i * 270 + j];
                    while (propID != 0) {
                        if ((props[propID].m_flags & EPropInstance.BLOCKEDFLAG) == 0) {
                            Vector3 position = props[propID].Position;
                            int x = EMath.Clamp((int)(position.x / 19.2f + 256f), 0, 511);
                            int y = EMath.Clamp((int)(position.z / 19.2f + 256f), 0, 511);
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
    }
}
