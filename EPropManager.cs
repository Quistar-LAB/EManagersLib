using ColossalFramework;
using ColossalFramework.IO;
using ColossalFramework.Math;
using System;
using System.Threading;
using UnityEngine;

namespace EManagersLib {
    public static class EPropManager {
        public const float PROPGRID_CELL_SIZE = 64f;
        public const int PROPGRID_RESOLUTION = 270;
        public const int DEFAULT_PROP_LIMIT = 65536;
        public const int DEFAULT_UPDATEDPROP_LIMIT = 1024;
        public const int DEFAULT_GRID_LIMIT = 72900;
        public const int DEFAULT_MAP_PROPS = 50000;
        public const int DEFAULT_GAME_PROPS_LIMIT = (DEFAULT_PROP_LIMIT - 5);
        public const int DEFAULT_ASSET_PROPS = 64;
        [NonSerialized]
        public static Array32<EPropInstance> m_props;
        [NonSerialized]
        public static ulong[] m_updatedProps;
        [NonSerialized]
        public static uint[] m_propGrid;
        public static ItemClass.Availability m_mode;
        private static int m_propLayer;
        private static int m_markerLayer;
        private static int m_markerAlpha;
        internal static PropManager m_pmInstance;
        /* Custom limit declaration */
        public static float PROP_LIMIT_SCALE = 4f;
        public static int MAX_PROP_LIMIT => (int)(DEFAULT_PROP_LIMIT * PROP_LIMIT_SCALE);
        public static int MAX_UPDATEDPROP_LIMIT => (int)(DEFAULT_UPDATEDPROP_LIMIT * PROP_LIMIT_SCALE);
        public static int MAX_MAP_PROPS_LIMIT => MAX_PROP_LIMIT - 15536;
        public static int MAX_GAME_PROPS_LIMIT => MAX_PROP_LIMIT - 5;

        internal static void Awake(PropManager instance) {
            m_pmInstance = instance;
            m_mode = ItemClass.Availability.Game;
            m_props = new Array32<EPropInstance>((uint)MAX_PROP_LIMIT);
            m_updatedProps = new ulong[MAX_UPDATEDPROP_LIMIT];
            m_propGrid = new uint[DEFAULT_GRID_LIMIT];
            m_markerAlpha = Shader.PropertyToID("_MarkerAlpha");
            m_propLayer = LayerMask.NameToLayer("Props");
            m_markerLayer = LayerMask.NameToLayer("Markers");
            m_props.CreateItem(out uint _);
        }

        internal static void EnsureCapacity(PropManager pmInstance) {
            if (m_props.m_buffer.Length != MAX_PROP_LIMIT) {
                m_props = new Array32<EPropInstance>((uint)MAX_PROP_LIMIT);
                m_updatedProps = new ulong[MAX_UPDATEDPROP_LIMIT];
                m_props.CreateItem(out uint _);
            }
        }

        public static void EndRenderingImpl(RenderManager.CameraInfo cameraInfo) {
            FastList<RenderGroup> renderedGroups = Singleton<RenderManager>.instance.m_renderedGroups;
            int layer = 1 << m_propLayer | 1 << Singleton<RenderManager>.instance.lightSystem.m_lightLayer;
            if ((m_mode & ItemClass.Availability.Game) == ItemClass.Availability.None && m_markerAlpha >= 0.001f) {
                layer |= 1 << m_markerLayer;
            }
            uint[] propGrid = m_propGrid;
            EPropInstance[] props = m_props.m_buffer;
            for (int i = 0; i < renderedGroups.m_size; i++) {
                RenderGroup renderGroup = renderedGroups.m_buffer[i];
                if ((renderGroup.m_instanceMask & layer) != 0) {
                    int startX = renderGroup.m_x * PROPGRID_RESOLUTION / 45;
                    int startZ = renderGroup.m_z * PROPGRID_RESOLUTION / 45;
                    int endX = (renderGroup.m_x + 1) * PROPGRID_RESOLUTION / 45 - 1;
                    int endZ = (renderGroup.m_z + 1) * PROPGRID_RESOLUTION / 45 - 1;
                    for (int j = startZ; j <= endZ; j++) {
                        for (int k = startX; k <= endX; k++) {
                            uint propID = propGrid[j * PROPGRID_RESOLUTION + k];
                            while (propID != 0) {
                                props[propID].RenderInstance(cameraInfo, propID, renderGroup.m_instanceMask);
                                propID = props[propID].m_nextGridProp;
                            }
                        }
                    }
                }
            }
            uint prefabCount = (uint)PrefabCollection<PropInfo>.PrefabCount();
            for (uint i = 0; i < prefabCount; i++) {
                PropInfo prefab = PrefabCollection<PropInfo>.GetPrefab(i);
                if (!(prefab is null) && prefab.m_lodCount != 0) {
                    EPropInstance.RenderLod(cameraInfo, prefab);
                }
            }
        }

        public static float SampleSmoothHeight(Vector3 worldPos) {
            int Max(int a, int b) => (a <= b) ? b : a;
            int Min(int a, int b) => (a >= b) ? b : a;
            float finalHeight = 0f;
            int startX = Max((int)((worldPos.x - 32f) / PROPGRID_CELL_SIZE + 135f), 0);
            int startZ = Max((int)((worldPos.z - 32f) / PROPGRID_CELL_SIZE + 135f), 0);
            int endX = Min((int)((worldPos.x + 32f) / PROPGRID_CELL_SIZE + 135f), PROPGRID_RESOLUTION - 1);
            int endZ = Min((int)((worldPos.z + 32f) / PROPGRID_CELL_SIZE + 135f), PROPGRID_RESOLUTION - 1);
            uint[] propGrid = m_propGrid;
            EPropInstance[] props = m_props.m_buffer;
            for (int i = startZ; i <= endZ; i++) {
                for (int j = startX; j <= endX; j++) {
                    uint propID = propGrid[i * PROPGRID_RESOLUTION + j];
                    while (propID != 0) {
                        if (!props[propID].Blocked) {
                            Vector3 position = props[propID].Position;
                            Vector3 vector = worldPos - position;
                            float maxDiameter = 1024f;
                            float diameter = vector.x * vector.x + vector.z * vector.z;
                            if (diameter < maxDiameter) {
                                PropInfo info = props[propID].Info;
                                float height = MathUtils.SmoothClamp01(1f - Mathf.Sqrt(diameter / maxDiameter));
                                height = Mathf.Lerp(worldPos.y, position.y + info.m_generatedInfo.m_size.y * 1.25f, height);
                                if (height > finalHeight) {
                                    finalHeight = height;
                                }
                            }
                        }
                        propID = props[propID].m_nextGridProp;
                    }
                }
            }
            return finalHeight;
        }

        public static bool CreateProp(this PropManager pmInstance, out uint prop, ref Randomizer randomizer, PropInfo info, Vector3 position, float angle, bool single) {
            if (!pmInstance.CheckLimits()) {
                prop = 0;
                return false;
            }
            if (m_props.CreateItem(out uint propID, ref randomizer)) {
                prop = propID;
                Randomizer rand = new Randomizer((int)prop);
                m_props.m_buffer[prop].m_flags = EPropInstance.CREATEDFLAG;
                m_props.m_buffer[prop].Info = info;
                m_props.m_buffer[prop].Single = single;
                m_props.m_buffer[prop].Blocked = false;
                m_props.m_buffer[prop].Position = position;
                m_props.m_buffer[prop].Angle = angle;
                m_props.m_buffer[prop].m_scale = info.m_minScale + rand.Int32(10000u) * (info.m_maxScale - info.m_minScale) * 0.0001f;
                m_props.m_buffer[prop].m_color = info.GetColor(ref rand);
                DistrictManager instance = Singleton<DistrictManager>.instance;
                instance.m_parks.m_buffer[instance.GetPark(position)].m_propCount++;
                ItemClass.Availability mode = Singleton<ToolManager>.instance.m_properties.m_mode;
                InitializeProp(prop, ref m_props.m_buffer[prop], (mode & ItemClass.Availability.AssetEditor) != ItemClass.Availability.None);
                UpdateProp(prop);
                pmInstance.m_propCount = (int)(m_props.ItemCount() - 1u);
                return true;
            }
            prop = 0;
            return false;
        }

        public static void ReleaseProp(this PropManager _, uint prop) {
            ReleasePropImplementation(prop, ref m_props.m_buffer[prop]);
        }

        public static void InitializeProp(uint prop, ref EPropInstance data, bool assetEditor) {
            int Clamp(int value, int min, int max) {
                value = (value < min) ? min : value;
                return (value > max) ? max : value;
            }
            int posX;
            int posZ;
            if (assetEditor) {
                posX = Clamp(((data.m_posX / 16) + 32768) * PROPGRID_RESOLUTION / 65536, 0, PROPGRID_RESOLUTION - 1);
                posZ = Clamp(((data.m_posZ / 16) + 32768) * PROPGRID_RESOLUTION / 65536, 0, PROPGRID_RESOLUTION - 1);
            } else {
                posX = Clamp((data.m_posX + 32768) * PROPGRID_RESOLUTION / 65536, 0, PROPGRID_RESOLUTION - 1);
                posZ = Clamp((data.m_posZ + 32768) * PROPGRID_RESOLUTION / 65536, 0, PROPGRID_RESOLUTION - 1);
            }
            int grid = posZ * PROPGRID_RESOLUTION + posX;
            while (!Monitor.TryEnter(m_propGrid, SimulationManager.SYNCHRONIZE_TIMEOUT)) { }
            try {
                m_props.m_buffer[prop].m_nextGridProp = m_propGrid[grid];
                m_propGrid[grid] = prop;
            } finally {
                Monitor.Exit(m_propGrid);
            }
        }

        public static void FinalizeProp(uint prop, ref EPropInstance data) {
            int Clamp(int value, int min, int max) {
                value = (value < min) ? min : value;
                return (value > max) ? max : value;
            }
            int posx;
            int posz;
            EPropInstance[] props = m_props.m_buffer;
            if (Singleton<ToolManager>.instance.m_properties.m_mode == ItemClass.Availability.AssetEditor) {
                posx = Clamp(((data.m_posX / 16) + 32768) * PROPGRID_RESOLUTION / 65536, 0, PROPGRID_RESOLUTION - 1);
                posz = Clamp(((data.m_posZ / 16) + 32768) * PROPGRID_RESOLUTION / 65536, 0, PROPGRID_RESOLUTION - 1);
            } else {
                posx = Clamp((data.m_posX + 32768) * PROPGRID_RESOLUTION / 65536, 0, PROPGRID_RESOLUTION - 1);
                posz = Clamp((data.m_posZ + 32768) * PROPGRID_RESOLUTION / 65536, 0, PROPGRID_RESOLUTION - 1);
            }
            int grid = posz * PROPGRID_RESOLUTION + posx;
            while (!Monitor.TryEnter(m_propGrid, SimulationManager.SYNCHRONIZE_TIMEOUT)) { }
            try {
                uint curID = 0;
                uint propID = m_propGrid[grid];
                while (propID != 0) {
                    if (propID == prop) {
                        if (curID == 0) {
                            m_propGrid[grid] = data.m_nextGridProp;
                        } else {
                            props[curID].m_nextGridProp = data.m_nextGridProp;
                        }
                        break;
                    }
                    curID = propID;
                    propID = props[propID].m_nextGridProp;
                }
                data.m_nextGridProp = 0;
            } finally {
                Monitor.Exit(m_propGrid);
            }
            int x = posx * 45 / PROPGRID_RESOLUTION;
            int z = posz * 45 / PROPGRID_RESOLUTION;
            Singleton<RenderManager>.instance.UpdateGroup(x, z, m_propLayer);
            PropInfo info = data.Info;
            if (!(info is null) && info.m_effectLayer != -1) {
                Singleton<RenderManager>.instance.UpdateGroup(x, z, info.m_effectLayer);
            }
        }

        public static void MoveProp(this PropManager _, uint prop, Vector3 position) {
            MovePropImplementation(prop, ref m_props.m_buffer[prop], position);
        }

        private static void MovePropImplementation(uint prop, ref EPropInstance data, Vector3 position) {
            if (data.m_flags != 0) {
                if (!data.Blocked) {
                    DistrictManager instance = Singleton<DistrictManager>.instance;
                    instance.m_parks.m_buffer[instance.GetPark(data.Position)].m_propCount--;
                    instance.m_parks.m_buffer[instance.GetPark(position)].m_propCount++;
                }
                ItemClass.Availability mode = Singleton<ToolManager>.instance.m_properties.m_mode;
                FinalizeProp(prop, ref data);
                data.Position = position;
                InitializeProp(prop, ref data, (mode & ItemClass.Availability.AssetEditor) != ItemClass.Availability.None);
                UpdateProp(prop);
            }
        }

        private static void ReleasePropImplementation(uint prop, ref EPropInstance data) {
            if (data.m_flags != 0) {
                EInstanceID id = default;
                id.Prop = prop;
                Singleton<InstanceManager>.instance.ReleaseInstance(id.OrigID);
                data.m_flags |= EPropInstance.DELETEDFLAG;
                data.UpdateProp(prop);
                m_pmInstance.UpdatePropRenderer(prop, true);
                if (!data.Blocked) {
                    DistrictManager instance = Singleton<DistrictManager>.instance;
                    instance.m_parks.m_buffer[instance.GetPark(data.Position)].m_propCount--;
                }
                data.m_flags = 0;
                FinalizeProp(prop, ref data);
                m_props.ReleaseItem(prop);
                m_pmInstance.m_propCount = (int)(m_props.ItemCount() - 1u);
            }
        }

        public static void UpdateProp(uint prop) {
            m_updatedProps[prop >> 6] |= 1uL << (int)prop;
            m_pmInstance.m_propsUpdated = true;
        }

        public static void UpdateProps(PropManager pmInstance, float minX, float minZ, float maxX, float maxZ) {
            int Max(int a, int b) => (a <= b) ? b : a;
            int Min(int a, int b) => (a >= b) ? b : a;
            EPropInstance[] props = m_props.m_buffer;
            int startX = Max((int)((minX - 8f) / PROPGRID_CELL_SIZE + 135f), 0);
            int startZ = Max((int)((minZ - 8f) / PROPGRID_CELL_SIZE + 135f), 0);
            int endX = Min((int)((maxX + 8f) / PROPGRID_CELL_SIZE + 135f), PROPGRID_RESOLUTION - 1);
            int endZ = Min((int)((maxZ + 8f) / PROPGRID_CELL_SIZE + 135f), PROPGRID_RESOLUTION - 1);
            for (int i = startZ; i <= endZ; i++) {
                for (int j = startX; j <= endX; j++) {
                    uint propID = m_propGrid[i * PROPGRID_RESOLUTION + j];
                    while (propID != 0) {
                        Vector3 position = props[propID].Position;
                        float intersect = Mathf.Max(Mathf.Max(minX - 8f - position.x, minZ - 8f - position.z), Mathf.Max(position.x - maxX - 8f, position.z - maxZ - 8f));
                        if (intersect < 0f) {
                            m_updatedProps[propID >> 6] |= 1uL << (int)propID;
                            pmInstance.m_propsUpdated = true;
                        }
                        propID = props[propID].m_nextGridProp;
                    }
                }
            }
        }

        public static void UpdatePropRenderer(this PropManager _, uint propID, bool updateGroup) {
            int Clamp(int value, int min, int max) {
                value = (value < min) ? min : value;
                return (value > max) ? max : value;
            }
            ref EPropInstance prop = ref m_props.m_buffer[propID];
            if (prop.m_flags == 0) return;
            if (updateGroup) {
                int posX;
                int posZ;
                if (Singleton<ToolManager>.instance.m_properties.m_mode == ItemClass.Availability.AssetEditor) {
                    posX = Clamp(((prop.m_posX >> 4) + 32768) * PROPGRID_RESOLUTION / 65536, 0, PROPGRID_RESOLUTION - 1);
                    posZ = Clamp(((prop.m_posZ >> 4) + 32768) * PROPGRID_RESOLUTION / 65536, 0, PROPGRID_RESOLUTION - 1);
                } else {
                    posX = Clamp((prop.m_posX + 32768) * PROPGRID_RESOLUTION / 65536, 0, PROPGRID_RESOLUTION - 1);
                    posZ = Clamp((prop.m_posZ + 32768) * PROPGRID_RESOLUTION / 65536, 0, PROPGRID_RESOLUTION - 1);
                }
                int x = posX * 45 / PROPGRID_RESOLUTION;
                int z = posZ * 45 / PROPGRID_RESOLUTION;
                PropInfo info = prop.Info;
                if (!(info is null) && info.m_prefabDataLayer != -1) {
                    Singleton<RenderManager>.instance.UpdateGroup(x, z, info.m_prefabDataLayer);
                }
                if (!(info is null) && info.m_effectLayer != -1) {
                    Singleton<RenderManager>.instance.UpdateGroup(x, z, info.m_effectLayer);
                }
            }
        }

        public static bool OverlapQuad(Quad2 quad, float minY, float maxY, ItemClass.CollisionType collisionType, int layer, ushort ignoreProp) {
            int Max(int a, int b) => (a <= b) ? b : a;
            int Min(int a, int b) => (a >= b) ? b : a;
            float Maxf(float a, float b) => (a <= b) ? b : a;
            Vector2 vector = quad.Min();
            Vector2 vector2 = quad.Max();
            EPropInstance[] props = m_props.m_buffer;
            int startX = Max((int)((vector.x - 8f) / PROPGRID_CELL_SIZE + 135f), 0);
            int startZ = Max((int)((vector.y - 8f) / PROPGRID_CELL_SIZE + 135f), 0);
            int endX = Min((int)((vector2.x + 8f) / PROPGRID_CELL_SIZE + 135f), PROPGRID_RESOLUTION - 1);
            int endZ = Min((int)((vector2.y + 8f) / PROPGRID_CELL_SIZE + 135f), PROPGRID_RESOLUTION - 1);
            for (int i = startZ; i <= endZ; i++) {
                for (int j = startX; j <= endX; j++) {
                    uint propID = m_propGrid[i * PROPGRID_RESOLUTION + j];
                    while (propID != 0) {
                        Vector3 position = props[propID].Position;
                        float intersect = Maxf(Maxf(vector.x - 8f - position.x, vector.y - 8f - position.z), Maxf(position.x - vector2.x - 8f, position.z - vector2.y - 8f));
                        if (intersect < 0f && props[propID].OverlapQuad(propID, quad, minY, maxY, collisionType)) {
                            return true;
                        }
                        propID = props[propID].m_nextGridProp;
                    }
                }
            }
            return false;
        }

        public static bool RayCast(Segment3 ray, ItemClass.Service service, ItemClass.SubService subService, ItemClass.Layer itemLayers, PropInstance.Flags ignoreFlags, out Vector3 hit, out uint propIndex) {
            int Max(int a, int b) => (a <= b) ? b : a;
            int Min(int a, int b) => (a >= b) ? b : a;
            float Maxf(float a, float b) => (a <= b) ? b : a;
            float Minf(float a, float b) => (a >= b) ? b : a;
            Bounds bounds = new Bounds(new Vector3(0f, 512f, 0f), new Vector3(17280f, 1152f, 17280f));
            if (ray.Clip(bounds)) {
                Vector3 vector = ray.b - ray.a;
                int x1 = (int)(ray.a.x / PROPGRID_CELL_SIZE + 135f);
                int z1 = (int)(ray.a.z / PROPGRID_CELL_SIZE + 135f);
                int x2 = (int)(ray.b.x / PROPGRID_CELL_SIZE + 135f);
                int z2 = (int)(ray.b.z / PROPGRID_CELL_SIZE + 135f);
                float rangeX = Mathf.Abs(vector.x);
                float rangeZ = Mathf.Abs(vector.z);
                int num7;
                int num8;
                if (rangeX >= rangeZ) {
                    num7 = ((vector.x <= 0f) ? -1 : 1);
                    num8 = 0;
                    if (rangeX != 0f) {
                        vector *= PROPGRID_CELL_SIZE / rangeX;
                    }
                } else {
                    num7 = 0;
                    num8 = ((vector.z <= 0f) ? -1 : 1);
                    if (rangeZ != 0f) {
                        vector *= PROPGRID_CELL_SIZE / rangeZ;
                    }
                }
                float num9 = 2f;
                float num10 = 10000f;
                propIndex = 0;
                Vector3 vector2 = ray.a;
                Vector3 vector3 = ray.a;
                int num11 = x1;
                int num12 = z1;
                do {
                    Vector3 vector4 = vector3 + vector;
                    int startX;
                    int endX;
                    int startZ;
                    int endZ;
                    if (num7 != 0) {
                        if ((num11 == x1 && num7 > 0) || (num11 == x2 && num7 < 0)) {
                            startX = Max((int)((vector4.x - 72f) / PROPGRID_CELL_SIZE + 135f), 0);
                        } else {
                            startX = Max(num11, 0);
                        }
                        if ((num11 == x1 && num7 < 0) || (num11 == x2 && num7 > 0)) {
                            endX = Min((int)((vector4.x + 72f) / PROPGRID_CELL_SIZE + 135f), PROPGRID_RESOLUTION - 1);
                        } else {
                            endX = Min(num11, PROPGRID_RESOLUTION - 1);
                        }
                        startZ = Max((int)((Minf(vector2.z, vector4.z) - 72f) / PROPGRID_CELL_SIZE + 135f), 0);
                        endZ = Min((int)((Maxf(vector2.z, vector4.z) + 72f) / PROPGRID_CELL_SIZE + 135f), PROPGRID_RESOLUTION - 1);
                    } else {
                        if ((num12 == z1 && num8 > 0) || (num12 == z2 && num8 < 0)) {
                            startZ = Max((int)((vector4.z - 72f) / PROPGRID_CELL_SIZE + 135f), 0);
                        } else {
                            startZ = Max(num12, 0);
                        }
                        if ((num12 == z1 && num8 < 0) || (num12 == z2 && num8 > 0)) {
                            endZ = Min((int)((vector4.z + 72f) / PROPGRID_CELL_SIZE + 135f), PROPGRID_RESOLUTION - 1);
                        } else {
                            endZ = Min(num12, PROPGRID_RESOLUTION - 1);
                        }
                        startX = Max((int)((Minf(vector2.x, vector4.x) - 72f) / PROPGRID_CELL_SIZE + 135f), 0);
                        endX = Min((int)((Maxf(vector2.x, vector4.x) + 72f) / PROPGRID_CELL_SIZE + 135f), PROPGRID_RESOLUTION - 1);
                    }
                    EPropInstance[] props = m_props.m_buffer;
                    for (int i = startZ; i <= endZ; i++) {
                        for (int j = startX; j <= endX; j++) {
                            uint propID = m_propGrid[i * PROPGRID_RESOLUTION + j];
                            while (propID != 0) {
                                if ((props[propID].m_flags & (ushort)ignoreFlags) == 0 && ray.DistanceSqr(props[propID].Position) < 900f) {
                                    PropInfo info = props[propID].Info;
                                    if ((service == ItemClass.Service.None || info.m_class.m_service == service) &&
                                        (subService == ItemClass.SubService.None || info.m_class.m_subService == subService) &&
                                        (itemLayers == ItemClass.Layer.None || (info.m_class.m_layer & itemLayers) != ItemClass.Layer.None) &&
                                        props[propID].RayCast(propID, ray, out float target, out float targetSqr) && (target < num9 - 0.0001f || (target < num9 + 0.0001f && targetSqr < num10))) {
                                        num9 = target;
                                        num10 = targetSqr;
                                        propIndex = propID;
                                    }
                                }
                                propID = props[propID].m_nextGridProp;
                            }
                        }
                    }
                    vector2 = vector3;
                    vector3 = vector4;
                    num11 += num7;
                    num12 += num8;
                } while ((num11 <= x2 || num7 <= 0) && (num11 >= x2 || num7 >= 0) && (num12 <= z2 || num8 <= 0) && (num12 >= z2 || num8 >= 0));
                if (num9 != 2f) {
                    hit = ray.Position(num9);
                    return true;
                }
            }
            hit = Vector3.zero;
            propIndex = 0;
            return false;
        }

        public static void SimulationStepImpl(PropManager pmInstance, int subStep) {
            if (pmInstance.m_propsUpdated) {
                EPropInstance[] props = m_props.m_buffer;
                ulong[] updatedProps = m_updatedProps;
                int len = updatedProps.Length;
                for (int i = 0; i < len; i++) {
                    ulong index = updatedProps[i];
                    if (index != 0uL) {
                        for (int j = 0; j < 64; j++) {
                            if ((index & 1uL << j) != 0uL) {
                                uint propID = (uint)(i << 6 | j);
                                props[propID].CalculateProp(propID);
                            }
                        }
                    }
                }
                pmInstance.m_propsUpdated = false;
                for (int i = 0; i < len; i++) {
                    ulong index = updatedProps[i];
                    if (index != 0uL) {
                        updatedProps[i] = 0uL;
                        for (int l = 0; l < 64; l++) {
                            if ((index & 1uL << l) != 0uL) {
                                uint propID = (uint)(i << 6 | l);
                                props[propID].UpdateProp(propID);
                                pmInstance.UpdatePropRenderer(propID, true);
                            }
                        }
                    }
                }
            }
        }

        public static void TerrainUpdated(TerrainArea surfaceArea) {
            uint[] propGrid = m_propGrid;
            EPropInstance[] props = m_props.m_buffer;
            float x = surfaceArea.m_min.x;
            float z = surfaceArea.m_min.z;
            float x2 = surfaceArea.m_max.x;
            float z2 = surfaceArea.m_max.z;
            int startX = Mathf.Max((int)((x - 8f) / PROPGRID_CELL_SIZE + 135f), 0);
            int startZ = Mathf.Max((int)((z - 8f) / PROPGRID_CELL_SIZE + 135f), 0);
            int endX = Mathf.Min((int)((x2 + 8f) / PROPGRID_CELL_SIZE + 135f), PROPGRID_RESOLUTION - 1);
            int endZ = Mathf.Min((int)((z2 + 8f) / PROPGRID_CELL_SIZE + 135f), PROPGRID_RESOLUTION - 1);
            for (int i = startZ; i <= endZ; i++) {
                for (int j = startX; j <= endX; j++) {
                    uint propID = propGrid[i * PROPGRID_RESOLUTION + j];
                    while (propID != 0) {
                        Vector3 position = props[propID].Position;
                        float intersect = Mathf.Max(Mathf.Max(x - 8f - position.x, z - 8f - position.z), Mathf.Max(position.x - x2 - 8f, position.z - z2 - 8f));
                        if (intersect < 0f) {
                            props[propID].TerrainUpdated(propID, x, z, x2, z2);
                        }
                        propID = props[propID].m_nextGridProp;
                    }
                }
            }
        }

        public static void AfterTerrainUpdate(TerrainArea heightArea) {
            int Max(int a, int b) => (a <= b) ? b : a;
            int Min(int a, int b) => (a >= b) ? b : a;
            float Maxf(float a, float b) => (a <= b) ? b : a;
            uint[] propGrid = m_propGrid;
            EPropInstance[] props = m_props.m_buffer;
            float x = heightArea.m_min.x;
            float z = heightArea.m_min.z;
            float x2 = heightArea.m_max.x;
            float z2 = heightArea.m_max.z;
            int startX = Max((int)((x - 8f) / PROPGRID_CELL_SIZE + 135f), 0);
            int startZ = Max((int)((z - 8f) / PROPGRID_CELL_SIZE + 135f), 0);
            int endX = Min((int)((x2 + 8f) / PROPGRID_CELL_SIZE + 135f), PROPGRID_RESOLUTION - 1);
            int endZ = Min((int)((z2 + 8f) / PROPGRID_CELL_SIZE + 135f), PROPGRID_RESOLUTION - 1);
            for (int i = startZ; i <= endZ; i++) {
                for (int j = startX; j <= endX; j++) {
                    uint propID = propGrid[i * PROPGRID_RESOLUTION + j];
                    while (propID != 0) {
                        Vector3 position = props[propID].Position;
                        float intersect = Maxf(Maxf(x - 8f - position.x, z - 8f - position.z), Maxf(position.x - x2 - 8f, position.z - z2 - 8f));
                        if (intersect < 0f) {
                            props[propID].AfterTerrainUpdated(propID, x, z, x2, z2);
                        }
                        propID = props[propID].m_nextGridProp;
                    }
                }
            }
        }

        public static bool CalculateGroupData(int groupX, int groupZ, int layer, ref int vertexCount, ref int triangleCount, ref int objectCount, ref RenderGroup.VertexArrays vertexArrays) {
            bool result = false;
            if (layer != m_propLayer && layer != m_markerLayer && layer != Singleton<RenderManager>.instance.lightSystem.m_lightLayer) {
                return result;
            }
            uint[] propGrid = m_propGrid;
            EPropInstance[] props = m_props.m_buffer;
            int startX = groupX * PROPGRID_RESOLUTION / 45;
            int startZ = groupZ * PROPGRID_RESOLUTION / 45;
            int endX = (groupX + 1) * PROPGRID_RESOLUTION / 45 - 1;
            int endZ = (groupZ + 1) * PROPGRID_RESOLUTION / 45 - 1;
            for (int i = startZ; i <= endZ; i++) {
                for (int j = startX; j <= endX; j++) {
                    uint propID = propGrid[i * PROPGRID_RESOLUTION + j];
                    while (propID != 0) {
                        if (props[propID].CalculateGroupData(propID, layer, ref vertexCount, ref triangleCount, ref objectCount, ref vertexArrays)) {
                            result = true;
                        }
                        propID = props[propID].m_nextGridProp;
                    }
                }
            }
            return result;
        }

        public static void PopulateGroupData(int groupX, int groupZ, int layer, ref int vertexIndex, ref int triangleIndex, Vector3 groupPosition, RenderGroup.MeshData data, ref Vector3 min, ref Vector3 max, ref float maxRenderDistance, ref float maxInstanceDistance) {
            if (layer != m_propLayer && layer != m_markerLayer && layer != Singleton<RenderManager>.instance.lightSystem.m_lightLayer) return;
            uint[] propGrid = m_propGrid;
            EPropInstance[] props = m_props.m_buffer;
            int startX = groupX * PROPGRID_RESOLUTION / 45;
            int startZ = groupZ * PROPGRID_RESOLUTION / 45;
            int endX = (groupX + 1) * PROPGRID_RESOLUTION / 45 - 1;
            int endZ = (groupZ + 1) * PROPGRID_RESOLUTION / 45 - 1;
            for (int i = startZ; i <= endZ; i++) {
                for (int j = startX; j <= endX; j++) {
                    uint propID = propGrid[i * PROPGRID_RESOLUTION + j];
                    while (propID != 0) {
                        props[propID].PopulateGroupData(propID, layer, ref vertexIndex, ref triangleIndex, groupPosition, data, ref min, ref max, ref maxRenderDistance, ref maxInstanceDistance);
                        propID = props[propID].m_nextGridProp;
                    }
                }
            }
        }

        public static void UpdateData(SimulationManager.UpdateMode mode) {
            Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.BeginLoading("PropManager.UpdateData");
            UpdateData(mode);
            int limit = MAX_PROP_LIMIT;
            EPropInstance[] props = m_props.m_buffer;
            for (int i = 1; i < limit; i++) {
                if (props[i].m_flags != 0 && props[i].Info is null) {
                    m_pmInstance.ReleaseProp((uint)i);
                }
            }
            m_pmInstance.m_infoCount = PrefabCollection<PropInfo>.PrefabCount();
            Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.EndLoading();
        }

        public static bool Deserialize(DataSerializer s) {
            Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.BeginDeserialize(s, "PropManager");
            PropManager instance = Singleton<PropManager>.instance;
            EnsureCapacity(instance);
            EPropInstance[] buffer = m_props.m_buffer;
            uint[] propGrid = m_propGrid;
            m_props.ClearUnused();
            SimulationManager.UpdateMode updateMode = Singleton<SimulationManager>.instance.m_metaData.m_updateMode;
            bool assetEditor = updateMode == SimulationManager.UpdateMode.NewAsset || updateMode == SimulationManager.UpdateMode.LoadAsset;
            for (int i = 0; i < propGrid.Length; i++) {
                propGrid[i] = 0;
            }
            EncodedArray.UShort uShort = EncodedArray.UShort.BeginRead(s);
            for (int i = 1; i < DEFAULT_PROP_LIMIT; i++) {
                buffer[i].m_flags = uShort.Read();
            }
            uShort.EndRead();
            PrefabCollection<PropInfo>.BeginDeserialize(s);
            for (int i = 1; i < DEFAULT_PROP_LIMIT; i++) {
                if (buffer[i].m_flags != 0) {
                    buffer[i].m_infoIndex = (ushort)PrefabCollection<PropInfo>.Deserialize(true);
                }
            }
            PrefabCollection<PropInfo>.EndDeserialize(s);
            EncodedArray.Short @short = EncodedArray.Short.BeginRead(s);
            for (int i = 1; i < DEFAULT_PROP_LIMIT; i++) {
                if (buffer[i].m_flags != 0) {
                    buffer[i].m_posX = @short.Read();
                } else {
                    buffer[i].m_posX = 0;
                }
            }
            @short.EndRead();
            EncodedArray.Short short2 = EncodedArray.Short.BeginRead(s);
            for (int i = 1; i < DEFAULT_PROP_LIMIT; i++) {
                if (buffer[i].m_flags != 0) {
                    buffer[i].m_posZ = short2.Read();
                } else {
                    buffer[i].m_posZ = 0;
                }
            }
            short2.EndRead();
            EncodedArray.UShort uShort2 = EncodedArray.UShort.BeginRead(s);
            for (int i = 1; i < DEFAULT_PROP_LIMIT; i++) {
                if (buffer[i].m_flags != 0) {
                    buffer[i].m_angle = uShort2.Read();
                } else {
                    buffer[i].m_angle = 0;
                }
            }
            uShort2.EndRead();
            ESerializableData.IntegratedPropDeserialize(buffer);
            buffer = m_props.m_buffer;
            int len = buffer.Length;
            for (int i = 1; i < len; i++) {
                buffer[i].m_nextGridProp = 0;
                //if((buffer[i].m_flags & EPropInstance.FIXEDHEIGHTFLAG) == 0) buffer[i].m_posY = 0;
                buffer[i].m_posY = 0;
                if (buffer[i].m_flags != 0) {
                    InitializeProp((uint)i, ref buffer[i], assetEditor);
                } else {
                    m_props.ReleaseItem((uint)i);
                }
            }
            Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.EndDeserialize(s, "PropManager");
            return false;
        }

        public static bool Serialize(DataSerializer s) {
            Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.BeginSerialize(s, "PropManager");
            EPropInstance[] buffer = m_props.m_buffer;
            int num = DEFAULT_PROP_LIMIT;
            EncodedArray.UShort uShort = EncodedArray.UShort.BeginWrite(s);
            for (int i = 1; i < num; i++) {
                uShort.Write(buffer[i].m_flags);
            }
            uShort.EndWrite();
            try {
                PrefabCollection<PropInfo>.BeginSerialize(s);
                for (int i = 1; i < num; i++) {
                    if (buffer[i].m_flags != 0) {
                        PrefabCollection<PropInfo>.Serialize(buffer[i].m_infoIndex);
                    }
                }
            } finally {
                PrefabCollection<PropInfo>.EndSerialize(s);
            }
            EncodedArray.Short @short = EncodedArray.Short.BeginWrite(s);
            for (int i = 1; i < num; i++) {
                if (buffer[i].m_flags != 0) {
                    @short.Write(buffer[i].m_posX);
                }
            }
            @short.EndWrite();
            EncodedArray.Short short2 = EncodedArray.Short.BeginWrite(s);
            for (int i = 1; i < num; i++) {
                if (buffer[i].m_flags != 0) {
                    short2.Write(buffer[i].m_posZ);
                }
            }
            short2.EndWrite();
            EncodedArray.UShort uShort2 = EncodedArray.UShort.BeginWrite(s);
            for (int i = 1; i < num; i++) {
                if (buffer[i].m_flags != 0) {
                    uShort2.Write(buffer[i].m_angle);
                }
            }
            uShort2.EndWrite();
            Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.EndSerialize(s, "PropManager");
            return false;
        }
    }
}
