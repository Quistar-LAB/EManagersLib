using EManagersLib.API;
using UnityEngine;

namespace EManagersLib {
    public static class EDistrictManager {
        public static void MoveParkProps(DistrictPark[] parks, int cellX, int cellZ, byte src, byte dest) {
            int startX = Mathf.Max((int)((cellX - 256f) * (19.2f / 64f) + 135f), 0);
            int startZ = Mathf.Max((int)((cellZ - 256f) * (19.2f / 64f) + 135f), 0);
            int endX = Mathf.Min((int)((cellX - 256f + 1f) * (19.2f / 64f) + 135f), 269);
            int endZ = Mathf.Min((int)((cellZ - 256f + 1f) * (19.2f / 64f) + 135f), 269);
            EPropInstance[] props = EPropManager.m_props.m_buffer;
            uint[] propGrid = EPropManager.m_propGrid;
            for (int i = startZ; i <= endZ; i++) {
                for (int j = startX; j <= endX; j++) {
                    uint propID = propGrid[i * 270 + j];
                    while (propID != 0) {
                        if ((props[propID].m_flags & EPropInstance.BLOCKEDFLAG) == 0) {
                            Vector3 position = props[propID].Position;
                            int x = Mathf.Clamp((int)(position.x / 19.2f + 256f), 0, 511);
                            int y = Mathf.Clamp((int)(position.z / 19.2f + 256f), 0, 511);
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
