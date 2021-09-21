using ColossalFramework;
using ColossalFramework.Math;
using UnityEngine;

namespace EManagersLib {
    public static class EDisasterHelpers {
        public static void DestroyProps(Vector3 position, float totalRadius, float removeRadius) {
            const ushort Created = (ushort)(PropInstance.Flags.Created);
            const ushort CreatedOrDeleted = (ushort)(PropInstance.Flags.Created | PropInstance.Flags.Deleted);
            int Max(int a, int b) => (a > b) ? a : b;
            int Min(int a, int b) => (a < b) ? a : b;
            float Minf(float a, float b) => (a < b) ? a : b;
            float radius = Minf(totalRadius, removeRadius);
            int startX = Max((int)((position.x - radius) / EPropManager.PROPGRID_CELL_SIZE + 135f), 0);
            int startZ = Max((int)((position.z - radius) / EPropManager.PROPGRID_CELL_SIZE + 135f), 0);
            int endX = Min((int)((position.x + radius) / EPropManager.PROPGRID_CELL_SIZE + 135f), 269);
            int endZ = Min((int)((position.z + radius) / EPropManager.PROPGRID_CELL_SIZE + 135f), 269);
            EPropInstance[] props = EPropManager.m_props.m_buffer;
            uint[] propGrid = EPropManager.m_propGrid;
            for (int i = startZ; i <= endZ; i++) {
                for (int j = startX; j <= endX; j++) {
                    uint propID = propGrid[i * EPropManager.PROPGRID_RESOLUTION + j];
                    while (propID != 0) {
                        if ((props[propID].m_flags & CreatedOrDeleted) == Created) {
                            if (VectorUtils.LengthXZ(props[propID].Position - position) < radius) {
                                Singleton<PropManager>.instance.ReleaseProp(propID);
                            }
                        }
                        propID = props[propID].m_nextGridProp;
                    }
                }
            }
        }

    }
}
