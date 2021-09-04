using ColossalFramework;
using ColossalFramework.Math;
using System;
using System.Threading;
using UnityEngine;

namespace EManagersLib {
    public static class EPropManager {
        public const float PROPGRID_CELL_SIZE = 64f;
        public const int PROPGRID_RESOLUTION = 270;
        public const int MAX_PROP_COUNT = 65536;
        public const int MAX_MAP_PROPS = 50000;
        public const int MAX_ASSET_PROPS = 64;
        private static int m_propLayer; // Need Initialization
        [NonSerialized]
        public static Array32<PropInstance> m_props;
        [NonSerialized]
        public static uint[] m_propGrid;

        public static void Init(int layer) {
            m_propLayer = layer;
            m_props = new Array32<PropInstance>(1000);
            m_propGrid = new uint[1000];
        }

        public static bool CreateProp(this PropManager instance, out uint prop, ref Randomizer randomizer, PropInfo info, Vector3 position, float angle, bool single) {
            if (!instance.CheckLimits()) {
                prop = 0;
                return false;
            }
            if (m_props.CreateItem(out uint propID, ref randomizer)) {
                prop = propID;
                m_props.m_buffer[prop].m_flags = 1;
                m_props.m_buffer[prop].Info = info;
                m_props.m_buffer[prop].Single = single;
                m_props.m_buffer[prop].Blocked = false;
                m_props.m_buffer[prop].Position = position;
                m_props.m_buffer[prop].Angle = angle;
                DistrictManager district = Singleton<DistrictManager>.instance;
                byte park = district.GetPark(position);
                district.m_parks.m_buffer[park].m_propCount++;
                ItemClass.Availability mode = Singleton<ToolManager>.instance.m_properties.m_mode;
                instance.InitializeProp(prop, ref m_props.m_buffer[prop], (mode & ItemClass.Availability.AssetEditor) != ItemClass.Availability.None);
                instance.UpdateProp(prop);
                instance.m_propCount = (int)(m_props.ItemCount() - 1u);
                return true;
            }
            prop = 0;
            return false;
        }

        public static void ReleaseProp(this PropManager instance, uint prop) => ReleasePropImplementation(instance, prop, ref m_props.m_buffer[prop]);

        private static void InitializeProp(this PropManager instance, uint prop, ref PropInstance data, bool assetEditor) {
            int posX;
            int posZ;
            if (assetEditor) {
                posX = Mathf.Clamp(((data.m_posX / 16) + 32768) * PROPGRID_RESOLUTION / 65536, 0, PROPGRID_RESOLUTION - 1);
                posZ = Mathf.Clamp(((data.m_posZ / 16) + 32768) * PROPGRID_RESOLUTION / 65536, 0, PROPGRID_RESOLUTION - 1);
            } else {
                posX = Mathf.Clamp((data.m_posX + 32768) * PROPGRID_RESOLUTION / 65536, 0, PROPGRID_RESOLUTION - 1);
                posZ = Mathf.Clamp((data.m_posZ + 32768) * PROPGRID_RESOLUTION / 65536, 0, PROPGRID_RESOLUTION - 1);
            }
            int grid = posZ * PROPGRID_RESOLUTION + posX;
            while (!Monitor.TryEnter(instance.m_propGrid, SimulationManager.SYNCHRONIZE_TIMEOUT)) { }
            try {
                m_props.m_buffer[prop].m_nextGridProp = instance.m_propGrid[grid];
                m_propGrid[grid] = prop;
            } finally {
                Monitor.Exit(m_propGrid);
            }
        }

        private static void FinalizeProp(this PropManager instance, uint prop, ref PropInstance data) {
            int posX;
            int posZ;
            if (Singleton<ToolManager>.instance.m_properties.m_mode == ItemClass.Availability.AssetEditor) {
                posX = Mathf.Clamp(((data.m_posX / 16) + 32768) * PROPGRID_RESOLUTION / 65536, 0, PROPGRID_RESOLUTION - 1);
                posZ = Mathf.Clamp(((data.m_posZ / 16) + 32768) * PROPGRID_RESOLUTION / 65536, 0, PROPGRID_RESOLUTION - 1);
            } else {
                posX = Mathf.Clamp((data.m_posX + 32768) * PROPGRID_RESOLUTION / 65536, 0, PROPGRID_RESOLUTION - 1);
                posZ = Mathf.Clamp((data.m_posZ + 32768) * PROPGRID_RESOLUTION / 65536, 0, PROPGRID_RESOLUTION - 1);
            }
            int gridIndex = posZ * PROPGRID_RESOLUTION + posX;
            while (!Monitor.TryEnter(m_propGrid, SimulationManager.SYNCHRONIZE_TIMEOUT)) { }
            try {
                uint propID = 0;
                uint nextPropID = m_propGrid[gridIndex];
                while (nextPropID != 0) {
                    if (nextPropID == prop) {
                        if (propID == 0) {
                            m_propGrid[gridIndex] = data.m_nextGridProp;
                        } else {
                            m_props.m_buffer[propID].m_nextGridProp = data.m_nextGridProp;
                        }
                        break;
                    }
                    propID = nextPropID;
                    nextPropID = m_props.m_buffer[nextPropID].m_nextGridProp;
                }
                data.m_nextGridProp = 0;
            } finally {
                Monitor.Exit(m_propGrid);
            }
            int x = posX * 45 / PROPGRID_RESOLUTION;
            int z = posZ * 45 / PROPGRID_RESOLUTION;
            Singleton<RenderManager>.instance.UpdateGroup(x, z, m_propLayer);
            PropInfo info = data.Info;
            if (!(info is null) && info.m_effectLayer != -1) {
                Singleton<RenderManager>.instance.UpdateGroup(x, z, info.m_effectLayer);
            }
        }

        public static void MoveProp(this PropManager instance, uint prop, Vector3 position) => MovePropImplementation(instance, prop, ref m_props.m_buffer[prop], position);

        private static void MovePropImplementation(this PropManager instance, uint prop, ref PropInstance data, Vector3 position) {
            if (data.m_flags != 0) {
                if (!data.Blocked) {
                    DistrictManager district = Singleton<DistrictManager>.instance;
                    byte park = district.GetPark(data.Position);
                    byte park2 = district.GetPark(position);
                    district.m_parks.m_buffer[park].m_propCount--;
                    district.m_parks.m_buffer[park2].m_propCount++;
                }
                ItemClass.Availability mode = Singleton<ToolManager>.instance.m_properties.m_mode;
                instance.FinalizeProp(prop, ref data);
                data.Position = position;
                instance.InitializeProp(prop, ref data, (mode & ItemClass.Availability.AssetEditor) != ItemClass.Availability.None);
                instance.UpdateProp(prop);
            }
        }

        private static void ReleasePropImplementation(this PropManager instance, uint prop, ref PropInstance data) {
            if (data.m_flags != 0) {
                EInstanceID id = default;
                id.Prop = prop;
                Singleton<InstanceManager>.instance.ReleaseInstance(id.OriginalID);
                data.m_flags |= 2;
                data.UpdateProp(prop);
                UpdatePropRenderer(instance, prop, true);
                if (!data.Blocked) {
                    DistrictManager district = Singleton<DistrictManager>.instance;
                    byte park = district.GetPark(data.Position);
                    district.m_parks.m_buffer[park].m_propCount--;
                }
                data.m_flags = 0;
                FinalizeProp(instance, prop, ref data);
                m_props.ReleaseItem(prop);
                instance.m_propCount = (int)(m_props.ItemCount() - 1u);
            }
        }

        public static void UpdateProp(this PropManager instance, uint prop) {
            instance.m_updatedProps[prop >> 6] |= 1uL << (int)prop;
            instance.m_propsUpdated = true;
        }

        public static void UpdateProps(this PropManager instance, float minX, float minZ, float maxX, float maxZ) {
            int startX = Mathf.Max((int)((minX - 8f) / PROPGRID_CELL_SIZE + 135f), 0);
            int startZ = Mathf.Max((int)((minZ - 8f) / PROPGRID_CELL_SIZE + 135f), 0);
            int endX = Mathf.Min((int)((maxX + 8f) / PROPGRID_CELL_SIZE + 135f), PROPGRID_RESOLUTION - 1);
            int endZ = Mathf.Min((int)((maxZ + 8f) / PROPGRID_CELL_SIZE + 135f), PROPGRID_RESOLUTION - 1);
            for (int i = startZ; i <= endZ; i++) {
                for (int j = startX; j <= endX; j++) {
                    uint propID = m_propGrid[i * PROPGRID_RESOLUTION + j];
                    while (propID != 0) {
                        Vector3 position = m_props.m_buffer[propID].Position;
                        float num7 = Mathf.Max(Mathf.Max(minX - 8f - position.x, minZ - 8f - position.z), Mathf.Max(position.x - maxX - 8f, position.z - maxZ - 8f));
                        if (num7 < 0f) {
                            instance.m_updatedProps[propID >> 6] |= 1uL << (int)propID;
                            instance.m_propsUpdated = true;
                        }
                        propID = m_props.m_buffer[propID].m_nextGridProp;
                    }
                }
            }
        }

        public static void UpdatePropRenderer(this PropManager _, uint prop, bool updateGroup) {
            if (m_props.m_buffer[prop].m_flags == 0) return;
            if (updateGroup) {
                int num;
                int num2;
                if (Singleton<ToolManager>.instance.m_properties.m_mode == ItemClass.Availability.AssetEditor) {
                    num = Mathf.Clamp(((m_props.m_buffer[prop].m_posX / 16) + 32768) * PROPGRID_RESOLUTION / 65536, 0, PROPGRID_RESOLUTION - 1);
                    num2 = Mathf.Clamp(((m_props.m_buffer[prop].m_posZ / 16) + 32768) * PROPGRID_RESOLUTION / 65536, 0, PROPGRID_RESOLUTION - 1);
                } else {
                    num = Mathf.Clamp((m_props.m_buffer[prop].m_posX + 32768) * PROPGRID_RESOLUTION / 65536, 0, PROPGRID_RESOLUTION - 1);
                    num2 = Mathf.Clamp((m_props.m_buffer[prop].m_posZ + 32768) * PROPGRID_RESOLUTION / 65536, 0, PROPGRID_RESOLUTION - 1);
                }
                int x = num * 45 / PROPGRID_RESOLUTION;
                int z = num2 * 45 / PROPGRID_RESOLUTION;
                PropInfo info = m_props.m_buffer[(int)prop].Info;
                if (info != null && info.m_prefabDataLayer != -1) {
                    Singleton<RenderManager>.instance.UpdateGroup(x, z, info.m_prefabDataLayer);
                }
                if (info != null && info.m_effectLayer != -1) {
                    Singleton<RenderManager>.instance.UpdateGroup(x, z, info.m_effectLayer);
                }
            }
        }

        public static bool OverlapQuad(this PropManager _, Quad2 quad, float minY, float maxY, ItemClass.CollisionType collisionType, int layer, ushort ignoreProp) {
            Vector2 vector = quad.Min();
            Vector2 vector2 = quad.Max();
            int num = Mathf.Max((int)((vector.x - 8f) / PROPGRID_RESOLUTION + 135f), 0);
            int num2 = Mathf.Max((int)((vector.y - 8f) / PROPGRID_RESOLUTION + 135f), 0);
            int num3 = Mathf.Min((int)((vector2.x + 8f) / PROPGRID_RESOLUTION + 135f), PROPGRID_RESOLUTION - 1);
            int num4 = Mathf.Min((int)((vector2.y + 8f) / PROPGRID_RESOLUTION + 135f), PROPGRID_RESOLUTION - 1);
            for (int i = num2; i <= num4; i++) {
                for (int j = num; j <= num3; j++) {
                    uint num5 = m_propGrid[i * PROPGRID_RESOLUTION + j];
                    while (num5 != 0) {
                        Vector3 position = m_props.m_buffer[(int)num5].Position;
                        float num7 = Mathf.Max(Mathf.Max(vector.x - 8f - position.x, vector.y - 8f - position.z), Mathf.Max(position.x - vector2.x - 8f, position.z - vector2.y - 8f));
                        if (num7 < 0f && m_props.m_buffer[(int)num5].OverlapQuad(num5, quad, minY, maxY, collisionType)) {
                            return true;
                        }
                        num5 = m_props.m_buffer[(int)num5].m_nextGridProp;
                    }
                }
            }
            return false;
        }

        public static bool RayCast(this PropManager _, Segment3 ray, ItemClass.Service service, ItemClass.SubService subService, ItemClass.Layer itemLayers, PropInstance.Flags ignoreFlags, out Vector3 hit, out uint propIndex) {
            Bounds bounds = new Bounds(new Vector3(0f, 512f, 0f), new Vector3(17280f, 1152f, 17280f));
            if (ray.Clip(bounds)) {
                Vector3 vector = ray.b - ray.a;
                int num = (int)(ray.a.x / PROPGRID_RESOLUTION + 135f);
                int num2 = (int)(ray.a.z / PROPGRID_RESOLUTION + 135f);
                int num3 = (int)(ray.b.x / PROPGRID_RESOLUTION + 135f);
                int num4 = (int)(ray.b.z / PROPGRID_RESOLUTION + 135f);
                float num5 = Mathf.Abs(vector.x);
                float num6 = Mathf.Abs(vector.z);
                int num7;
                int num8;
                if (num5 >= num6) {
                    num7 = ((vector.x <= 0f) ? -1 : 1);
                    num8 = 0;
                    if (num5 != 0f) {
                        vector *= 64f / num5;
                    }
                } else {
                    num7 = 0;
                    num8 = ((vector.z <= 0f) ? -1 : 1);
                    if (num6 != 0f) {
                        vector *= 64f / num6;
                    }
                }
                float num9 = 2f;
                float num10 = 10000f;
                propIndex = 0;
                Vector3 vector2 = ray.a;
                Vector3 vector3 = ray.a;
                int num11 = num;
                int num12 = num2;
                do {
                    Vector3 vector4 = vector3 + vector;
                    int num13;
                    int num14;
                    int num15;
                    int num16;
                    if (num7 != 0) {
                        if ((num11 == num && num7 > 0) || (num11 == num3 && num7 < 0)) {
                            num13 = Mathf.Max((int)((vector4.x - 72f) / PROPGRID_RESOLUTION + 135f), 0);
                        } else {
                            num13 = Mathf.Max(num11, 0);
                        }
                        if ((num11 == num && num7 < 0) || (num11 == num3 && num7 > 0)) {
                            num14 = Mathf.Min((int)((vector4.x + 72f) / PROPGRID_RESOLUTION + 135f), 269);
                        } else {
                            num14 = Mathf.Min(num11, 269);
                        }
                        num15 = Mathf.Max((int)((Mathf.Min(vector2.z, vector4.z) - 72f) / PROPGRID_RESOLUTION + 135f), 0);
                        num16 = Mathf.Min((int)((Mathf.Max(vector2.z, vector4.z) + 72f) / PROPGRID_RESOLUTION + 135f), 269);
                    } else {
                        if ((num12 == num2 && num8 > 0) || (num12 == num4 && num8 < 0)) {
                            num15 = Mathf.Max((int)((vector4.z - 72f) / PROPGRID_RESOLUTION + 135f), 0);
                        } else {
                            num15 = Mathf.Max(num12, 0);
                        }
                        if ((num12 == num2 && num8 < 0) || (num12 == num4 && num8 > 0)) {
                            num16 = Mathf.Min((int)((vector4.z + 72f) / PROPGRID_RESOLUTION + 135f), 269);
                        } else {
                            num16 = Mathf.Min(num12, 269);
                        }
                        num13 = Mathf.Max((int)((Mathf.Min(vector2.x, vector4.x) - 72f) / PROPGRID_RESOLUTION + 135f), 0);
                        num14 = Mathf.Min((int)((Mathf.Max(vector2.x, vector4.x) + 72f) / PROPGRID_RESOLUTION + 135f), 269);
                    }
                    for (int i = num15; i <= num16; i++) {
                        for (int j = num13; j <= num14; j++) {
                            uint num17 = m_propGrid[i * PROPGRID_RESOLUTION + j];
                            while (num17 != 0) {
                                PropInstance.Flags flags = (PropInstance.Flags)m_props.m_buffer[(int)num17].m_flags;
                                if ((flags & ignoreFlags) == PropInstance.Flags.None && ray.DistanceSqr(m_props.m_buffer[(int)num17].Position) < 900f) {
                                    PropInfo info = m_props.m_buffer[(int)num17].Info;
                                    if ((service == ItemClass.Service.None || info.m_class.m_service == service) &&
                                        (subService == ItemClass.SubService.None || info.m_class.m_subService == subService) &&
                                        (itemLayers == ItemClass.Layer.None || (info.m_class.m_layer & itemLayers) != ItemClass.Layer.None) &&
                                        m_props.m_buffer[num17].RayCast(num17, ray, out float num19, out float num20) && (num19 < num9 - 0.0001f || (num19 < num9 + 0.0001f && num20 < num10))) {
                                        num9 = num19;
                                        num10 = num20;
                                        propIndex = num17;
                                    }
                                }
                                num17 = m_props.m_buffer[num17].m_nextGridProp;
                            }
                        }
                    }
                    vector2 = vector3;
                    vector3 = vector4;
                    num11 += num7;
                    num12 += num8;
                }
                while ((num11 <= num3 || num7 <= 0) && (num11 >= num3 || num7 >= 0) && (num12 <= num4 || num8 <= 0) && (num12 >= num4 || num8 >= 0));
                if (num9 != 2f) {
                    hit = ray.Position(num9);
                    return true;
                }
            }
            hit = Vector3.zero;
            propIndex = 0;
            return false;
        }
    }
}
