using ColossalFramework;
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
        private static FastList<uint>[] m_automaticProps;
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
        private static bool m_propsRefreshed;
        internal static PropManager m_pmInstance;
        /* Custom limit declaration */
        public static float PROP_LIMIT_SCALE = 4f;
        public static int MAX_PROP_LIMIT => (int)(DEFAULT_PROP_LIMIT * PROP_LIMIT_SCALE);
        public static int MAX_UPDATEDPROP_LIMIT => (int)(DEFAULT_UPDATEDPROP_LIMIT * PROP_LIMIT_SCALE);
        public static int MAX_MAP_PROPS_LIMIT => MAX_PROP_LIMIT - 15536;
        public static int MAX_GAME_PROPS_LIMIT => MAX_PROP_LIMIT - 5;
        /* Custom functionality */
        public static bool UsePropAnarchy;
        public static bool UsePropSnapping;
        private static EUtils.RefGetter<FastList<PrefabCollection<PropInfo>.PrefabData>> m_simulationPrefabs;

        [NonSerialized]
        public static int ID_Color;
        [NonSerialized]
        public static int ID_ObjectIndex;
        [NonSerialized]
        public static int ID_PropLocation;
        [NonSerialized]
        public static int ID_PropObjectIndex;
        [NonSerialized]
        public static int ID_PropColor;
        [NonSerialized]
        public static int ID_HeightMap;
        [NonSerialized]
        public static int ID_HeightMapping;
        [NonSerialized]
        public static int ID_SurfaceMapping;
        [NonSerialized]
        public static int ID_WaterHeightMap;
        [NonSerialized]
        public static int ID_WaterHeightMapping;
        [NonSerialized]
        public static int ID_WaterSurfaceMapping;
        [NonSerialized]
        public static int ID_MarkerAlpha;
        [NonSerialized]
        public static int ID_MainTex;
        [NonSerialized]
        public static int ID_XYSMap;
        [NonSerialized]
        public static int ID_ACIMap;
        [NonSerialized]
        public static int ID_AtlasRect;
        [NonSerialized]
        public static int ID_RollLocation;
        [NonSerialized]
        public static int ID_RollParams;
        [NonSerialized]
        public static MaterialPropertyBlock m_materialBlock;

        public static void Awake(PropManager instance) {
            m_pmInstance = instance;
            m_simulationPrefabs = EUtils.CreatePrefabRefGetter<FastList<PrefabCollection<PropInfo>.PrefabData>>("m_simulationPrefabs");
            m_mode = ItemClass.Availability.Game;
            EUtils.ELog($"Setting Prop Buffer to {MAX_PROP_LIMIT}");
            m_props = new Array32<EPropInstance>((uint)MAX_PROP_LIMIT);
            m_updatedProps = new ulong[MAX_UPDATEDPROP_LIMIT];
            m_propGrid = new uint[DEFAULT_GRID_LIMIT];
            m_automaticProps = new FastList<uint>[25];
            m_materialBlock = new MaterialPropertyBlock();
            m_markerAlpha = Shader.PropertyToID("_MarkerAlpha");
            m_propLayer = LayerMask.NameToLayer("Props");
            m_markerLayer = LayerMask.NameToLayer("Markers");
            ID_Color = Shader.PropertyToID("_Color");
            ID_ObjectIndex = Shader.PropertyToID("_ObjectIndex");
            ID_HeightMap = Shader.PropertyToID("_HeightMap");
            ID_HeightMapping = Shader.PropertyToID("_HeightMapping");
            ID_SurfaceMapping = Shader.PropertyToID("_SurfaceMapping");
            ID_WaterHeightMap = Shader.PropertyToID("_WaterHeightMap");
            ID_WaterHeightMapping = Shader.PropertyToID("_WaterHeightMapping");
            ID_WaterSurfaceMapping = Shader.PropertyToID("_WaterSurfaceMapping");
            ID_MarkerAlpha = Shader.PropertyToID("_MarkerAlpha");
            ID_MainTex = Shader.PropertyToID("_MainTex");
            ID_XYSMap = Shader.PropertyToID("_XYSMap");
            ID_ACIMap = Shader.PropertyToID("_ACIMap");
            ID_AtlasRect = Shader.PropertyToID("_AtlasRect");
            ID_PropLocation = Shader.PropertyToID("_PropLocation");
            ID_PropObjectIndex = Shader.PropertyToID("_PropObjectIndex");
            ID_PropColor = Shader.PropertyToID("_PropColor");
            ID_RollLocation = Shader.PropertyToID("_RollLocation");
            ID_RollParams = Shader.PropertyToID("_RollParams");
            m_props.CreateItem(out uint _);
        }

        public static void EnsureCapacity(PropManager pmInstance) {
            if (m_props.m_buffer.Length != MAX_PROP_LIMIT) {
                m_props = new Array32<EPropInstance>((uint)MAX_PROP_LIMIT);
                m_updatedProps = new ulong[MAX_UPDATEDPROP_LIMIT];
                m_props.CreateItem(out uint _);
            }
        }

        public unsafe static void EndRenderingImpl(PropManager pmInstance, RenderManager.CameraInfo cameraInfo) {
            int i, j, k, l;
            FastList<RenderGroup> renderedGroups = Singleton<RenderManager>.instance.m_renderedGroups;
            int layer = 1 << m_propLayer | 1 << Singleton<RenderManager>.instance.lightSystem.m_lightLayer;
            if ((m_mode & ItemClass.Availability.Game) == ItemClass.Availability.None && m_markerAlpha >= 0.001f) {
                layer |= 1 << m_markerLayer;
            }
            Randomizer randomizer = EMath.randomizer;
            Vector3 v3zero = EMath.Vector3Zero;
            Vector3 v3down = EMath.Vector3Down;
            Vector4 v4zero = EMath.Vector4Zero;
            Vector4 lodLocation = cameraInfo.m_forward * -100000f;
            Vector3 defLODMin = EMath.DefaultLodMin;
            Vector3 defLODMax = EMath.DefaultLodMax;
            Matrix4x4 identity = EMath.matrix4Identity;
            Vector4 clear = EMath.ColorClear;
            int IDColor = ID_Color;
            int IDHeightMap = ID_HeightMap;
            int IDHeightMapping = ID_HeightMapping;
            int IDSurfaceMapping = ID_SurfaceMapping;
            int IDObjectIndex = ID_ObjectIndex;
            int IDWaterHeightMap = ID_WaterHeightMap;
            int IDWaterHeightMapping = ID_WaterHeightMapping;
            int IDWaterSurfaceMapping = ID_WaterSurfaceMapping;
            int IDRollLocation = ID_RollLocation;
            int IDRollParams = ID_RollParams;
            int IDPropLocation = ID_PropLocation;
            int IDPropObjectIndex = ID_PropObjectIndex;
            int IDPropColor = ID_PropColor;
            MaterialPropertyBlock materialBlock = m_materialBlock;
            LightSystem lightSystem = Singleton<RenderManager>.instance.lightSystem;
            TerrainManager tmInstance = Singleton<TerrainManager>.instance;
            Vector4 objectIndex = RenderManager.DefaultColorLocation;
            ref float simulationTimeDelta = ref Singleton<SimulationManager>.instance.m_simulationTimeDelta;
            ref float simulationTimer = ref Singleton<SimulationManager>.instance.m_simulationTimer;
            InfoManager.InfoMode infoMode = Singleton<InfoManager>.instance.CurrentMode;
            int renderSize = renderedGroups.m_size;
            InstanceID id = default;
            Bounds bounds = default;
            Mesh mesh;
            Vector3 lodMin, lodMax;
            PropInfo info;
            Matrix4x4 matrix4x;
            Vector4 blinkVector;
            float illumRange, illumStep;
            fixed (DrawCallData* pDrawCallData = &pmInstance.m_drawCallData)
            fixed (uint* pGrid = &m_propGrid[0])
            fixed (EPropInstance* pBuf = &m_props.m_buffer[0]) {
                for (i = 0; i < renderSize; i++) {
                    RenderGroup renderGroup = renderedGroups.m_buffer[i];
                    if ((renderGroup.m_instanceMask & layer) != 0) {
                        int startX = renderGroup.m_x * (PROPGRID_RESOLUTION / 45);
                        int startZ = renderGroup.m_z * (PROPGRID_RESOLUTION / 45);
                        int endX = (renderGroup.m_x + 1) * (PROPGRID_RESOLUTION / 45) - 1;
                        int endZ = (renderGroup.m_z + 1) * (PROPGRID_RESOLUTION / 45) - 1;
                        for (j = startZ; j <= endZ; j++) {
                            for (k = startX; k <= endX; k++) {
                                uint propID = *(pGrid + (j * PROPGRID_RESOLUTION + k));
                                while (propID != 0) {
                                    EPropInstance* prop = pBuf + propID;
                                    if ((prop->m_flags & (EPropInstance.HIDDENFLAG | EPropInstance.BLOCKEDFLAG)) == 0) {
                                        info = prop->Info;
                                        Vector3 position = prop->Position;
                                        if (info.m_prefabInitialized && cameraInfo.ECheckRenderDistance(position, info.m_maxRenderDistance) && cameraInfo.Intersect(position, info.m_generatedInfo.m_size.y * info.m_maxScale)) {
                                            float scale = prop->m_scale;
                                            float angle = prop->Angle;
                                            Color color = prop->m_color;
                                            Material material = info.m_material;
                                            id.SetProp32(propID);
                                            if (info.m_requireWaterMap) {
                                                position.y = tmInstance.SampleRawHeightWithWater(position, false, 0f);
                                                tmInstance.GetHeightMapping(position, out Texture heightMap, out Vector4 heightMapping, out Vector4 surfaceMapping);
                                                tmInstance.GetWaterMapping(position, out Texture waterHeightMap, out Vector4 waterHeightMapping, out Vector4 waterSurfaceMapping);
                                                if (info.m_hasEffects) {
                                                    matrix4x = Matrix4x4.TRS(position, Quaternion.AngleAxis(angle * 57.29578f, v3down), new Vector3(scale, scale, scale));
                                                    PropInfo.Effect[] effects = info.m_effects;
                                                    for (l = 0; l < effects.Length; l++) {
                                                        effects[l].m_effect.RenderEffect(id, new EffectInfo.SpawnArea(matrix4x.MultiplyPoint(effects[l].m_position),
                                                            matrix4x.MultiplyVector(effects[l].m_direction), 0f), v3zero, 0f, 1f, -1f, simulationTimeDelta, cameraInfo);
                                                    }
                                                }
                                                if (info.m_hasRenderer && (cameraInfo.m_layerMask & 1 << info.m_prefabDataLayer) != 0) {
                                                    if (infoMode == InfoManager.InfoMode.None) {
                                                        if (info.m_illuminationOffRange.x < 1000f || info.m_illuminationBlinkType != LightEffect.BlinkType.None) {
                                                            randomizer.SetSeed(id.Index);
                                                            illumRange = info.m_illuminationOffRange.x + randomizer.Int32(100000u) * 1E-05f * (info.m_illuminationOffRange.y - info.m_illuminationOffRange.x);
                                                            objectIndex.z = EMath.SmoothStep(illumRange + 0.01f, illumRange - 0.01f, lightSystem.DayLightIntensity);
                                                            if (info.m_illuminationBlinkType != LightEffect.BlinkType.None) {
                                                                blinkVector = LightEffect.GetBlinkVector(info.m_illuminationBlinkType);
                                                                illumStep = illumRange * 3.71f + simulationTimer / blinkVector.w;
                                                                illumStep = (illumStep - (int)illumStep) * blinkVector.w;
                                                                objectIndex.z *= 1f - EMath.SmoothStep(blinkVector.x, blinkVector.y, illumStep) * EMath.SmoothStep(blinkVector.w, blinkVector.z, illumStep);
                                                            }
                                                        } else {
                                                            objectIndex.z = 1f;
                                                        }
                                                    }
                                                    if (cameraInfo is null || cameraInfo.ECheckRenderDistance(position, info.m_lodRenderDistance)) {
                                                        materialBlock.Clear();
                                                        materialBlock.SetColor(IDColor, color);
                                                        materialBlock.SetTexture(IDHeightMap, heightMap);
                                                        materialBlock.SetVector(IDHeightMapping, heightMapping);
                                                        materialBlock.SetVector(IDSurfaceMapping, surfaceMapping);
                                                        materialBlock.SetVector(IDObjectIndex, objectIndex);
                                                        materialBlock.SetTexture(IDWaterHeightMap, waterHeightMap);
                                                        materialBlock.SetVector(IDWaterHeightMapping, waterHeightMapping);
                                                        materialBlock.SetVector(IDWaterSurfaceMapping, waterSurfaceMapping);
                                                        if (!(info.m_rollLocation is null)) {
                                                            material.SetVectorArray(IDRollLocation, info.m_rollLocation);
                                                            material.SetVectorArray(IDRollParams, info.m_rollParams);
                                                        }
                                                        pDrawCallData->m_defaultCalls++;
                                                        Graphics.DrawMesh(info.m_mesh,
                                                            Matrix4x4.TRS(position, Quaternion.AngleAxis(angle * 57.29578f, v3down), new Vector3(scale, scale, scale)),
                                                            material, info.m_prefabDataLayer, null, 0, materialBlock);
                                                    } else {
                                                        if (heightMap != info.m_lodHeightMap || waterHeightMap != info.m_lodWaterHeightMap) {
                                                            if (info.m_lodCount != 0) {
                                                                materialBlock.Clear();
                                                                int lodCount;
                                                                if (info.m_lodCount <= 1) {
                                                                    mesh = info.m_lodMeshCombined1;
                                                                    lodCount = 1;
                                                                } else if (info.m_lodCount <= 4) {
                                                                    mesh = info.m_lodMeshCombined4;
                                                                    lodCount = 4;
                                                                } else if (info.m_lodCount <= 8) {
                                                                    mesh = info.m_lodMeshCombined8;
                                                                    lodCount = 8;
                                                                } else {
                                                                    mesh = info.m_lodMeshCombined16;
                                                                    lodCount = 16;
                                                                }
                                                                for (l = info.m_lodCount; l < lodCount; l++) {
                                                                    info.m_lodLocations[l] = lodLocation;
                                                                    info.m_lodObjectIndices[l] = v4zero;
                                                                    info.m_lodColors[l] = clear;
                                                                }
                                                                materialBlock.SetVectorArray(IDPropLocation, info.m_lodLocations);
                                                                materialBlock.SetVectorArray(IDPropObjectIndex, info.m_lodObjectIndices);
                                                                materialBlock.SetVectorArray(IDPropColor, info.m_lodColors);
                                                                if (info.m_requireHeightMap) {
                                                                    materialBlock.SetTexture(IDHeightMap, info.m_lodHeightMap);
                                                                    materialBlock.SetVector(IDHeightMapping, info.m_lodHeightMapping);
                                                                    materialBlock.SetVector(IDSurfaceMapping, info.m_lodSurfaceMapping);
                                                                }
                                                                if (info.m_requireWaterMap) {
                                                                    materialBlock.SetTexture(IDWaterHeightMap, info.m_lodWaterHeightMap);
                                                                    materialBlock.SetVector(IDWaterHeightMapping, info.m_lodWaterHeightMapping);
                                                                    materialBlock.SetVector(IDWaterSurfaceMapping, info.m_lodWaterSurfaceMapping);
                                                                }
                                                                if (!(info.m_rollLocation is null)) {
                                                                    info.m_lodMaterialCombined.SetVectorArray(IDRollLocation, info.m_rollLocation);
                                                                    info.m_lodMaterialCombined.SetVectorArray(IDRollParams, info.m_rollParams);
                                                                }
                                                                if (!(mesh is null)) {
                                                                    lodMin = info.m_lodMin;
                                                                    lodMax = info.m_lodMax;
                                                                    bounds.SetMinMax(new Vector3(lodMin.x - 100f, lodMin.y - 100f, lodMin.z - 100f),
                                                                                     new Vector3(lodMax.x + 100f, lodMax.y + 100f, lodMax.z + 100f));
                                                                    mesh.bounds = bounds;
                                                                    info.m_lodMin = defLODMin;
                                                                    info.m_lodMax = defLODMax;
                                                                    pDrawCallData->m_lodCalls++;
                                                                    pDrawCallData->m_batchedCalls += (info.m_lodCount - 1);
                                                                    Graphics.DrawMesh(mesh, identity, info.m_lodMaterialCombined, info.m_prefabDataLayer, null, 0, materialBlock);
                                                                }
                                                                info.m_lodCount = 0;
                                                            }
                                                            info.m_lodHeightMap = heightMap;
                                                            info.m_lodHeightMapping = heightMapping;
                                                            info.m_lodSurfaceMapping = surfaceMapping;
                                                            info.m_lodWaterHeightMap = waterHeightMap;
                                                            info.m_lodWaterHeightMapping = waterHeightMapping;
                                                            info.m_lodWaterSurfaceMapping = waterSurfaceMapping;
                                                        }
                                                        objectIndex.w = scale;
                                                        info.m_lodLocations[info.m_lodCount] = new Vector4(position.x, position.y, position.z, angle);
                                                        info.m_lodObjectIndices[info.m_lodCount] = objectIndex;
                                                        info.m_lodColors[info.m_lodCount] = color.linear;
                                                        info.m_lodMin = EMath.Min(info.m_lodMin, position);
                                                        info.m_lodMax = EMath.Max(info.m_lodMax, position);
                                                        if (++info.m_lodCount == info.m_lodLocations.Length) {
                                                            materialBlock.Clear();
                                                            int lodCount;
                                                            if (info.m_lodCount <= 1) {
                                                                mesh = info.m_lodMeshCombined1;
                                                                lodCount = 1;
                                                            } else if (info.m_lodCount <= 4) {
                                                                mesh = info.m_lodMeshCombined4;
                                                                lodCount = 4;
                                                            } else if (info.m_lodCount <= 8) {
                                                                mesh = info.m_lodMeshCombined8;
                                                                lodCount = 8;
                                                            } else {
                                                                mesh = info.m_lodMeshCombined16;
                                                                lodCount = 16;
                                                            }
                                                            for (l = info.m_lodCount; l < lodCount; l++) {
                                                                info.m_lodLocations[l] = lodLocation;
                                                                info.m_lodObjectIndices[l] = v4zero;
                                                                info.m_lodColors[l] = clear;
                                                            }
                                                            materialBlock.SetVectorArray(IDPropLocation, info.m_lodLocations);
                                                            materialBlock.SetVectorArray(IDPropObjectIndex, info.m_lodObjectIndices);
                                                            materialBlock.SetVectorArray(IDPropColor, info.m_lodColors);
                                                            if (info.m_requireHeightMap) {
                                                                materialBlock.SetTexture(IDHeightMap, info.m_lodHeightMap);
                                                                materialBlock.SetVector(IDHeightMapping, info.m_lodHeightMapping);
                                                                materialBlock.SetVector(IDSurfaceMapping, info.m_lodSurfaceMapping);
                                                            }
                                                            if (info.m_requireWaterMap) {
                                                                materialBlock.SetTexture(IDWaterHeightMap, info.m_lodWaterHeightMap);
                                                                materialBlock.SetVector(IDWaterHeightMapping, info.m_lodWaterHeightMapping);
                                                                materialBlock.SetVector(IDWaterSurfaceMapping, info.m_lodWaterSurfaceMapping);
                                                            }
                                                            if (!(info.m_rollLocation is null)) {
                                                                info.m_lodMaterialCombined.SetVectorArray(IDRollLocation, info.m_rollLocation);
                                                                info.m_lodMaterialCombined.SetVectorArray(IDRollParams, info.m_rollParams);
                                                            }
                                                            if (!(mesh is null)) {
                                                                lodMin = info.m_lodMin;
                                                                lodMax = info.m_lodMax;
                                                                bounds.SetMinMax(new Vector3(lodMin.x - 100f, lodMin.y - 100f, lodMin.z - 100f),
                                                                                 new Vector3(lodMax.x + 100f, lodMax.y + 100f, lodMax.z + 100f));
                                                                mesh.bounds = bounds;
                                                                info.m_lodMin = defLODMin;
                                                                info.m_lodMax = defLODMax;
                                                                pDrawCallData->m_lodCalls++;
                                                                pDrawCallData->m_batchedCalls += (info.m_lodCount - 1);
                                                                Graphics.DrawMesh(mesh, identity, info.m_lodMaterialCombined, info.m_prefabDataLayer, null, 0, materialBlock);
                                                            }
                                                            info.m_lodCount = 0;
                                                        }
                                                    }
                                                }
                                            } else if (info.m_requireHeightMap) {
                                                tmInstance.GetHeightMapping(position, out Texture heightMap, out Vector4 heightMapping, out Vector4 surfaceMapping);
                                                if (info.m_hasEffects) {
                                                    matrix4x = Matrix4x4.TRS(position, Quaternion.AngleAxis(angle * 57.29578f, v3down), new Vector3(scale, scale, scale));
                                                    PropInfo.Effect[] effects = info.m_effects;
                                                    for (l = 0; l < effects.Length; l++) {
                                                        effects[l].m_effect.RenderEffect(id, new EffectInfo.SpawnArea(matrix4x.MultiplyPoint(effects[l].m_position),
                                                            matrix4x.MultiplyVector(effects[l].m_direction), 0f), v3zero, 0f, 1f, -1f, simulationTimeDelta, cameraInfo);
                                                    }
                                                }
                                                if (info.m_hasRenderer && (cameraInfo.m_layerMask & 1 << info.m_prefabDataLayer) != 0) {
                                                    if (infoMode == InfoManager.InfoMode.None) {
                                                        if (info.m_illuminationOffRange.x < 1000f || info.m_illuminationBlinkType != LightEffect.BlinkType.None) {
                                                            randomizer.SetSeed(id.Index);
                                                            illumRange = info.m_illuminationOffRange.x + randomizer.Int32(100000u) * 1E-05f * (info.m_illuminationOffRange.y - info.m_illuminationOffRange.x);
                                                            objectIndex.z = EMath.SmoothStep(illumRange + 0.01f, illumRange - 0.01f, lightSystem.DayLightIntensity);
                                                            if (info.m_illuminationBlinkType != LightEffect.BlinkType.None) {
                                                                blinkVector = LightEffect.GetBlinkVector(info.m_illuminationBlinkType);
                                                                illumStep = illumRange * 3.71f + simulationTimer / blinkVector.w;
                                                                illumStep = (illumStep - (int)illumStep) * blinkVector.w;
                                                                objectIndex.z *= 1f - EMath.SmoothStep(blinkVector.x, blinkVector.y, illumStep) * EMath.SmoothStep(blinkVector.w, blinkVector.z, illumStep);
                                                            }
                                                        } else {
                                                            objectIndex.z = 1f;
                                                        }
                                                    }
                                                    if (cameraInfo is null || cameraInfo.ECheckRenderDistance(position, info.m_lodRenderDistance)) {
                                                        materialBlock.Clear();
                                                        materialBlock.SetColor(IDColor, color);
                                                        materialBlock.SetTexture(IDHeightMap, heightMap);
                                                        materialBlock.SetVector(IDHeightMapping, heightMapping);
                                                        materialBlock.SetVector(IDSurfaceMapping, surfaceMapping);
                                                        materialBlock.SetVector(IDObjectIndex, objectIndex);
                                                        if (!(info.m_rollLocation is null)) {
                                                            material.SetVectorArray(IDRollLocation, info.m_rollLocation);
                                                            material.SetVectorArray(IDRollParams, info.m_rollParams);
                                                        }
                                                        pDrawCallData->m_defaultCalls++;
                                                        Graphics.DrawMesh(info.m_mesh,
                                                            Matrix4x4.TRS(position, Quaternion.AngleAxis(angle * 57.29578f, v3down), new Vector3(scale, scale, scale)),
                                                            material, info.m_prefabDataLayer, null, 0, materialBlock);
                                                    } else {
                                                        if (heightMap != info.m_lodHeightMap) {
                                                            if (info.m_lodCount != 0) {
                                                                materialBlock.Clear();
                                                                int lodCount;
                                                                if (info.m_lodCount <= 1) {
                                                                    mesh = info.m_lodMeshCombined1;
                                                                    lodCount = 1;
                                                                } else if (info.m_lodCount <= 4) {
                                                                    mesh = info.m_lodMeshCombined4;
                                                                    lodCount = 4;
                                                                } else if (info.m_lodCount <= 8) {
                                                                    mesh = info.m_lodMeshCombined8;
                                                                    lodCount = 8;
                                                                } else {
                                                                    mesh = info.m_lodMeshCombined16;
                                                                    lodCount = 16;
                                                                }
                                                                for (l = info.m_lodCount; l < lodCount; l++) {
                                                                    info.m_lodLocations[l] = lodLocation;
                                                                    info.m_lodObjectIndices[l] = v4zero;
                                                                    info.m_lodColors[l] = clear;
                                                                }
                                                                materialBlock.SetVectorArray(IDPropLocation, info.m_lodLocations);
                                                                materialBlock.SetVectorArray(IDPropObjectIndex, info.m_lodObjectIndices);
                                                                materialBlock.SetVectorArray(IDPropColor, info.m_lodColors);
                                                                if (info.m_requireHeightMap) {
                                                                    materialBlock.SetTexture(IDHeightMap, info.m_lodHeightMap);
                                                                    materialBlock.SetVector(IDHeightMapping, info.m_lodHeightMapping);
                                                                    materialBlock.SetVector(IDSurfaceMapping, info.m_lodSurfaceMapping);
                                                                }
                                                                if (info.m_requireWaterMap) {
                                                                    materialBlock.SetTexture(IDWaterHeightMap, info.m_lodWaterHeightMap);
                                                                    materialBlock.SetVector(IDWaterHeightMapping, info.m_lodWaterHeightMapping);
                                                                    materialBlock.SetVector(IDWaterSurfaceMapping, info.m_lodWaterSurfaceMapping);
                                                                }
                                                                if (!(info.m_rollLocation is null)) {
                                                                    info.m_lodMaterialCombined.SetVectorArray(IDRollLocation, info.m_rollLocation);
                                                                    info.m_lodMaterialCombined.SetVectorArray(IDRollParams, info.m_rollParams);
                                                                }
                                                                if (!(mesh is null)) {
                                                                    lodMin = info.m_lodMin;
                                                                    lodMax = info.m_lodMax;
                                                                    bounds.SetMinMax(new Vector3(lodMin.x - 100f, lodMin.y - 100f, lodMin.z - 100f),
                                                                                     new Vector3(lodMax.x + 100f, lodMax.y + 100f, lodMax.z + 100f));
                                                                    mesh.bounds = bounds;
                                                                    info.m_lodMin = defLODMin;
                                                                    info.m_lodMax = defLODMax;
                                                                    pDrawCallData->m_lodCalls++;
                                                                    pDrawCallData->m_batchedCalls += (info.m_lodCount - 1);
                                                                    Graphics.DrawMesh(mesh, identity, info.m_lodMaterialCombined, info.m_prefabDataLayer, null, 0, materialBlock);
                                                                }
                                                                info.m_lodCount = 0;
                                                            }
                                                            info.m_lodHeightMap = heightMap;
                                                            info.m_lodHeightMapping = heightMapping;
                                                            info.m_lodSurfaceMapping = surfaceMapping;
                                                        }
                                                        objectIndex.w = scale;
                                                        info.m_lodLocations[info.m_lodCount] = new Vector4(position.x, position.y, position.z, angle);
                                                        info.m_lodObjectIndices[info.m_lodCount] = objectIndex;
                                                        info.m_lodColors[info.m_lodCount] = color.linear;
                                                        info.m_lodMin = EMath.Min(info.m_lodMin, position);
                                                        info.m_lodMax = EMath.Max(info.m_lodMax, position);
                                                        if (++info.m_lodCount == info.m_lodLocations.Length) {
                                                            materialBlock.Clear();
                                                            int lodCount;
                                                            if (info.m_lodCount <= 1) {
                                                                mesh = info.m_lodMeshCombined1;
                                                                lodCount = 1;
                                                            } else if (info.m_lodCount <= 4) {
                                                                mesh = info.m_lodMeshCombined4;
                                                                lodCount = 4;
                                                            } else if (info.m_lodCount <= 8) {
                                                                mesh = info.m_lodMeshCombined8;
                                                                lodCount = 8;
                                                            } else {
                                                                mesh = info.m_lodMeshCombined16;
                                                                lodCount = 16;
                                                            }
                                                            for (l = info.m_lodCount; l < lodCount; l++) {
                                                                info.m_lodLocations[l] = lodLocation;
                                                                info.m_lodObjectIndices[l] = v4zero;
                                                                info.m_lodColors[l] = clear;
                                                            }
                                                            materialBlock.SetVectorArray(IDPropLocation, info.m_lodLocations);
                                                            materialBlock.SetVectorArray(IDPropObjectIndex, info.m_lodObjectIndices);
                                                            materialBlock.SetVectorArray(IDPropColor, info.m_lodColors);
                                                            if (info.m_requireHeightMap) {
                                                                materialBlock.SetTexture(IDHeightMap, info.m_lodHeightMap);
                                                                materialBlock.SetVector(IDHeightMapping, info.m_lodHeightMapping);
                                                                materialBlock.SetVector(IDSurfaceMapping, info.m_lodSurfaceMapping);
                                                            }
                                                            if (info.m_requireWaterMap) {
                                                                materialBlock.SetTexture(IDWaterHeightMap, info.m_lodWaterHeightMap);
                                                                materialBlock.SetVector(IDWaterHeightMapping, info.m_lodWaterHeightMapping);
                                                                materialBlock.SetVector(IDWaterSurfaceMapping, info.m_lodWaterSurfaceMapping);
                                                            }
                                                            if (!(info.m_rollLocation is null)) {
                                                                info.m_lodMaterialCombined.SetVectorArray(IDRollLocation, info.m_rollLocation);
                                                                info.m_lodMaterialCombined.SetVectorArray(IDRollParams, info.m_rollParams);
                                                            }
                                                            if (!(mesh is null)) {
                                                                lodMin = info.m_lodMin;
                                                                lodMax = info.m_lodMax;
                                                                bounds.SetMinMax(new Vector3(lodMin.x - 100f, lodMin.y - 100f, lodMin.z - 100f),
                                                                                 new Vector3(lodMax.x + 100f, lodMax.y + 100f, lodMax.z + 100f));
                                                                mesh.bounds = bounds;
                                                                info.m_lodMin = defLODMin;
                                                                info.m_lodMax = defLODMax;
                                                                pDrawCallData->m_lodCalls++;
                                                                pDrawCallData->m_batchedCalls += (info.m_lodCount - 1);
                                                                Graphics.DrawMesh(mesh, identity, info.m_lodMaterialCombined, info.m_prefabDataLayer, null, 0, materialBlock);
                                                            }
                                                            info.m_lodCount = 0;
                                                        }
                                                    }
                                                }
                                            } else {
                                                if (info.m_hasEffects) {
                                                    matrix4x = Matrix4x4.TRS(position, Quaternion.AngleAxis(angle * 57.29578f, v3down), new Vector3(scale, scale, scale));
                                                    PropInfo.Effect[] effects = info.m_effects;
                                                    for (l = 0; l < effects.Length; l++) {
                                                        EffectInfo.SpawnArea area = new EffectInfo.SpawnArea(matrix4x.MultiplyPoint(effects[l].m_position), matrix4x.MultiplyVector(effects[l].m_direction), 0f);
                                                        effects[l].m_effect.RenderEffect(id, area, v3zero, 0f, 1f, -1f, simulationTimeDelta, cameraInfo);
                                                    }
                                                }
                                                if (info.m_hasRenderer && (cameraInfo.m_layerMask & 1 << info.m_prefabDataLayer) != 0) {
                                                    if (Singleton<InfoManager>.instance.CurrentMode == InfoManager.InfoMode.None) {
                                                        if (info.m_illuminationOffRange.x < 1000f || info.m_illuminationBlinkType != LightEffect.BlinkType.None) {
                                                            randomizer.SetSeed(id.Index);
                                                            illumRange = info.m_illuminationOffRange.x + randomizer.Int32(100000u) * 1E-05f * (info.m_illuminationOffRange.y - info.m_illuminationOffRange.x);
                                                            objectIndex.z = EMath.SmoothStep(illumRange + 0.01f, illumRange - 0.01f, lightSystem.DayLightIntensity);
                                                            if (info.m_illuminationBlinkType != LightEffect.BlinkType.None) {
                                                                blinkVector = LightEffect.GetBlinkVector(info.m_illuminationBlinkType);
                                                                illumStep = illumRange * 3.71f + simulationTimer / blinkVector.w;
                                                                illumStep = (illumStep - (int)illumStep) * blinkVector.w;
                                                                objectIndex.z *= 1f - EMath.SmoothStep(blinkVector.x, blinkVector.y, illumStep) * EMath.SmoothStep(blinkVector.w, blinkVector.z, illumStep);
                                                            }
                                                        } else {
                                                            objectIndex.z = 1f;
                                                        }
                                                    }
                                                    if (cameraInfo is null || cameraInfo.ECheckRenderDistance(position, info.m_lodRenderDistance)) {
                                                        materialBlock.Clear();
                                                        materialBlock.SetColor(IDColor, color);
                                                        materialBlock.SetVector(IDObjectIndex, objectIndex);
                                                        if (!(info.m_rollLocation is null)) {
                                                            material.SetVectorArray(IDRollLocation, info.m_rollLocation);
                                                            material.SetVectorArray(IDRollParams, info.m_rollParams);
                                                        }
                                                        pDrawCallData->m_defaultCalls++;
                                                        Graphics.DrawMesh(info.m_mesh, Matrix4x4.TRS(position, Quaternion.AngleAxis(angle * 57.29578f, v3down), new Vector3(scale, scale, scale)),
                                                                          material, info.m_prefabDataLayer, null, 0, materialBlock);
                                                    } else if (info.m_lodMaterialCombined is null) {
                                                        materialBlock.Clear();
                                                        materialBlock.SetColor(IDColor, color);
                                                        materialBlock.SetVector(IDObjectIndex, objectIndex);
                                                        if (!(info.m_rollLocation is null)) {
                                                            material.SetVectorArray(IDRollLocation, info.m_rollLocation);
                                                            material.SetVectorArray(IDRollParams, info.m_rollParams);
                                                        }
                                                        pDrawCallData->m_defaultCalls++;
                                                        Graphics.DrawMesh(info.m_lodMesh, Matrix4x4.TRS(position, Quaternion.AngleAxis(angle * 57.29578f, v3down), new Vector3(scale, scale, scale)),
                                                                            info.m_lodMaterial, info.m_prefabDataLayer, null, 0, materialBlock);
                                                    } else {
                                                        objectIndex.w = scale;
                                                        info.m_lodLocations[info.m_lodCount] = new Vector4(position.x, position.y, position.z, angle);
                                                        info.m_lodObjectIndices[info.m_lodCount] = objectIndex;
                                                        info.m_lodColors[info.m_lodCount] = color.linear;
                                                        info.m_lodMin = EMath.Min(info.m_lodMin, position);
                                                        info.m_lodMax = EMath.Max(info.m_lodMax, position);
                                                        if (++info.m_lodCount == info.m_lodLocations.Length) {
                                                            materialBlock.Clear();
                                                            int lodCount;
                                                            if (info.m_lodCount <= 1) {
                                                                mesh = info.m_lodMeshCombined1;
                                                                lodCount = 1;
                                                            } else if (info.m_lodCount <= 4) {
                                                                mesh = info.m_lodMeshCombined4;
                                                                lodCount = 4;
                                                            } else if (info.m_lodCount <= 8) {
                                                                mesh = info.m_lodMeshCombined8;
                                                                lodCount = 8;
                                                            } else {
                                                                mesh = info.m_lodMeshCombined16;
                                                                lodCount = 16;
                                                            }
                                                            for (l = info.m_lodCount; l < lodCount; l++) {
                                                                info.m_lodLocations[l] = lodLocation;
                                                                info.m_lodObjectIndices[l] = v4zero;
                                                                info.m_lodColors[l] = clear;
                                                            }
                                                            materialBlock.SetVectorArray(IDPropLocation, info.m_lodLocations);
                                                            materialBlock.SetVectorArray(IDPropObjectIndex, info.m_lodObjectIndices);
                                                            materialBlock.SetVectorArray(IDPropColor, info.m_lodColors);
                                                            if (info.m_requireHeightMap) {
                                                                materialBlock.SetTexture(IDHeightMap, info.m_lodHeightMap);
                                                                materialBlock.SetVector(IDHeightMapping, info.m_lodHeightMapping);
                                                                materialBlock.SetVector(IDSurfaceMapping, info.m_lodSurfaceMapping);
                                                            }
                                                            if (info.m_requireWaterMap) {
                                                                materialBlock.SetTexture(IDWaterHeightMap, info.m_lodWaterHeightMap);
                                                                materialBlock.SetVector(IDWaterHeightMapping, info.m_lodWaterHeightMapping);
                                                                materialBlock.SetVector(IDWaterSurfaceMapping, info.m_lodWaterSurfaceMapping);
                                                            }
                                                            if (!(info.m_rollLocation is null)) {
                                                                info.m_lodMaterialCombined.SetVectorArray(IDRollLocation, info.m_rollLocation);
                                                                info.m_lodMaterialCombined.SetVectorArray(IDRollParams, info.m_rollParams);
                                                            }
                                                            if (!(mesh is null)) {
                                                                lodMin = info.m_lodMin;
                                                                lodMax = info.m_lodMax;
                                                                bounds.SetMinMax(new Vector3(lodMin.x - 100f, lodMin.y - 100f, lodMin.z - 100f),
                                                                                 new Vector3(lodMax.x + 100f, lodMax.y + 100f, lodMax.z + 100f));
                                                                mesh.bounds = bounds;
                                                                info.m_lodMin = defLODMin;
                                                                info.m_lodMax = defLODMax;
                                                                pDrawCallData->m_lodCalls++;
                                                                pDrawCallData->m_batchedCalls += (info.m_lodCount - 1);
                                                                Graphics.DrawMesh(mesh, identity, info.m_lodMaterialCombined, info.m_prefabDataLayer, null, 0, materialBlock);
                                                            }
                                                            info.m_lodCount = 0;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    propID = prop->m_nextGridProp;
                                }
                            }
                        }
                    }
                }
                uint prefabCount = (uint)PrefabCollection<PropInfo>.PrefabCount();
                PrefabCollection<PropInfo>.PrefabData[] prefabs = m_simulationPrefabs().m_buffer;
                for (i = 0; i < prefabCount; i++) {
                    info = prefabs[i].m_prefab;
                    if (!(info is null) && info.m_lodCount != 0) {
                        materialBlock.Clear();
                        int lodCount;
                        if (info.m_lodCount <= 1) {
                            mesh = info.m_lodMeshCombined1;
                            lodCount = 1;
                        } else if (info.m_lodCount <= 4) {
                            mesh = info.m_lodMeshCombined4;
                            lodCount = 4;
                        } else if (info.m_lodCount <= 8) {
                            mesh = info.m_lodMeshCombined8;
                            lodCount = 8;
                        } else {
                            mesh = info.m_lodMeshCombined16;
                            lodCount = 16;
                        }
                        for (l = info.m_lodCount; l < lodCount; l++) {
                            info.m_lodLocations[l] = cameraInfo.m_forward * -100000f;
                            info.m_lodObjectIndices[l] = v4zero;
                            info.m_lodColors[l] = clear;
                        }
                        materialBlock.SetVectorArray(IDPropLocation, info.m_lodLocations);
                        materialBlock.SetVectorArray(IDPropObjectIndex, info.m_lodObjectIndices);
                        materialBlock.SetVectorArray(IDPropColor, info.m_lodColors);
                        if (info.m_requireHeightMap) {
                            materialBlock.SetTexture(IDHeightMap, info.m_lodHeightMap);
                            materialBlock.SetVector(IDHeightMapping, info.m_lodHeightMapping);
                            materialBlock.SetVector(IDSurfaceMapping, info.m_lodSurfaceMapping);
                        }
                        if (info.m_requireWaterMap) {
                            materialBlock.SetTexture(IDWaterHeightMap, info.m_lodWaterHeightMap);
                            materialBlock.SetVector(IDWaterHeightMapping, info.m_lodWaterHeightMapping);
                            materialBlock.SetVector(IDWaterSurfaceMapping, info.m_lodWaterSurfaceMapping);
                        }
                        if (!(info.m_rollLocation is null)) {
                            info.m_lodMaterialCombined.SetVectorArray(IDRollLocation, info.m_rollLocation);
                            info.m_lodMaterialCombined.SetVectorArray(IDRollParams, info.m_rollParams);
                        }
                        if (!(mesh is null)) {
                            lodMin = info.m_lodMin;
                            lodMax = info.m_lodMax;
                            bounds.SetMinMax(new Vector3(lodMin.x - 100f, lodMin.y - 100f, lodMin.z - 100f),
                                             new Vector3(lodMax.x + 100f, lodMax.y + 100f, lodMax.z + 100f));
                            mesh.bounds = bounds;
                            info.m_lodMin = defLODMin;
                            info.m_lodMax = defLODMax;
                            pDrawCallData->m_lodCalls++;
                            pDrawCallData->m_batchedCalls += (info.m_lodCount - 1);
                            Graphics.DrawMesh(mesh, identity, info.m_lodMaterialCombined, info.m_prefabDataLayer, null, 0, materialBlock);
                        }
                        info.m_lodCount = 0;
                    }
                }
            }
        }

        public static float SampleSmoothHeight(Vector3 worldPos) {
            float finalHeight = 0f;
            int startX = EMath.Max((int)((worldPos.x - 32f) / PROPGRID_CELL_SIZE + 135f), 0);
            int startZ = EMath.Max((int)((worldPos.z - 32f) / PROPGRID_CELL_SIZE + 135f), 0);
            int endX = EMath.Min((int)((worldPos.x + 32f) / PROPGRID_CELL_SIZE + 135f), PROPGRID_RESOLUTION - 1);
            int endZ = EMath.Min((int)((worldPos.z + 32f) / PROPGRID_CELL_SIZE + 135f), PROPGRID_RESOLUTION - 1);
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
                                float height = MathUtils.SmoothClamp01(1f - EMath.Sqrt(diameter / maxDiameter));
                                height = EMath.Lerp(worldPos.y, position.y + info.m_generatedInfo.m_size.y * 1.25f, height);
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
                EMath.SetRandomizerSeed(prop);
                EPropInstance[] props = m_props.m_buffer;
                props[prop].m_flags = EPropInstance.CREATEDFLAG | EPropInstance.SINGLEFLAG;
                props[prop].Info = info;
                props[prop].Position = position;
                props[prop].Angle = angle;
                props[prop].m_scale = info.m_minScale + EMath.randomizer.Int32(10000u) * (info.m_maxScale - info.m_minScale) * 0.0001f;
                props[prop].m_color = info.GetColor(ref EMath.randomizer);
                DistrictManager instance = Singleton<DistrictManager>.instance;
                instance.m_parks.m_buffer[instance.GetPark(position)].m_propCount++;
                ItemClass.Availability mode = Singleton<ToolManager>.instance.m_properties.m_mode;
                InitializeProp(prop, ref props[prop], (mode & ItemClass.Availability.AssetEditor) != ItemClass.Availability.None);
                UpdateProp(prop);
                pmInstance.m_propCount = (int)(m_props.ItemCount() - 1u);
                return true;
            }
            prop = 0;
            return false;
        }

        public static void ReleaseProp(this PropManager _, uint prop) {
            ref EPropInstance data = ref m_props.m_buffer[prop];
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

        public static void InitializeProp(uint prop, ref EPropInstance data, bool assetEditor) {
            int posX;
            int posZ;
            if (assetEditor) {
                posX = EMath.Clamp(((data.m_posX / 16) + 32768) * PROPGRID_RESOLUTION / 65536, 0, PROPGRID_RESOLUTION - 1);
                posZ = EMath.Clamp(((data.m_posZ / 16) + 32768) * PROPGRID_RESOLUTION / 65536, 0, PROPGRID_RESOLUTION - 1);
            } else {
                posX = EMath.Clamp((data.m_posX + 32768) * PROPGRID_RESOLUTION / 65536, 0, PROPGRID_RESOLUTION - 1);
                posZ = EMath.Clamp((data.m_posZ + 32768) * PROPGRID_RESOLUTION / 65536, 0, PROPGRID_RESOLUTION - 1);
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
            int posx;
            int posz;
            EPropInstance[] props = m_props.m_buffer;
            if (Singleton<ToolManager>.instance.m_properties.m_mode == ItemClass.Availability.AssetEditor) {
                posx = EMath.Clamp(((data.m_posX / 16) + 32768) * PROPGRID_RESOLUTION / 65536, 0, PROPGRID_RESOLUTION - 1);
                posz = EMath.Clamp(((data.m_posZ / 16) + 32768) * PROPGRID_RESOLUTION / 65536, 0, PROPGRID_RESOLUTION - 1);
            } else {
                posx = EMath.Clamp((data.m_posX + 32768) * PROPGRID_RESOLUTION / 65536, 0, PROPGRID_RESOLUTION - 1);
                posz = EMath.Clamp((data.m_posZ + 32768) * PROPGRID_RESOLUTION / 65536, 0, PROPGRID_RESOLUTION - 1);
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
            ref EPropInstance data = ref m_props.m_buffer[prop];
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

        private static int GetPropIndex(ItemClass.Service service) => service - ItemClass.Service.Residential;

        public static PropInfo GetRandomPropInfo(ref Randomizer r, ItemClass.Service service) {
            if (!m_propsRefreshed) {
                CODebugBase<LogChannel>.Error(LogChannel.Core, "Random props not refreshed yet!");
                return null;
            }
            int num = GetPropIndex(service);
            FastList<uint> fastList = m_automaticProps[num];
            if (fastList == null) {
                return null;
            }
            if (fastList.m_size == 0) {
                return null;
            }
            num = r.Int32((uint)fastList.m_size);
            return PrefabCollection<PropInfo>.GetPrefab(fastList.m_buffer[num]);
        }

        public static void RefreshAutomaticProps() {
            int num = m_automaticProps.Length;
            for (int i = 0; i < num; i++) {
                m_automaticProps[i] = null;
            }
            int prefabCount = PrefabCollection<PropInfo>.PrefabCount();
            PrefabCollection<PropInfo>.PrefabData[] prefabs = m_simulationPrefabs().m_buffer;
            for (int i = 0; i < prefabCount; i++) {
                PropInfo prefab = prefabs[i].m_prefab;
                if (!(prefab is null) && !(prefab.m_class is null) && prefab.m_class.m_service != ItemClass.Service.None && prefab.m_placementStyle == ItemClass.Placement.Automatic) {
                    if (prefab.m_requireHeightMap && prefab.m_class.m_service != ItemClass.Service.Disaster) {
                        CODebugBase<LogChannel>.Error(LogChannel.Core, string.Concat(new string[] {
                            "Automatic prop category cannot require heightmap (",
                            prefab.m_class.m_service.ToString(),
                            "->",
                            PrefabCollection<PropInfo>.PrefabName((uint)i),
                            ")"
                        }));
                    } else {
                        int propIndex = GetPropIndex(prefab.m_class.m_service);
                        if (m_automaticProps[propIndex] is null) {
                            m_automaticProps[propIndex] = new FastList<uint>();
                        }
                        m_automaticProps[propIndex].Add((uint)i);
                    }
                }
            }
            m_propsRefreshed = true;
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
            EPropInstance[] props = m_props.m_buffer;
            int startX = EMath.Max((int)((minX - 8f) / PROPGRID_CELL_SIZE + 135f), 0);
            int startZ = EMath.Max((int)((minZ - 8f) / PROPGRID_CELL_SIZE + 135f), 0);
            int endX = EMath.Min((int)((maxX + 8f) / PROPGRID_CELL_SIZE + 135f), PROPGRID_RESOLUTION - 1);
            int endZ = EMath.Min((int)((maxZ + 8f) / PROPGRID_CELL_SIZE + 135f), PROPGRID_RESOLUTION - 1);
            for (int i = startZ; i <= endZ; i++) {
                for (int j = startX; j <= endX; j++) {
                    uint propID = m_propGrid[i * PROPGRID_RESOLUTION + j];
                    while (propID != 0) {
                        Vector3 position = props[propID].Position;
                        float intersect = EMath.Max(EMath.Max(minX - 8f - position.x, minZ - 8f - position.z), EMath.Max(position.x - maxX - 8f, position.z - maxZ - 8f));
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
            ref EPropInstance prop = ref m_props.m_buffer[propID];
            if (prop.m_flags == 0) return;
            if (updateGroup) {
                int posX;
                int posZ;
                if (Singleton<ToolManager>.instance.m_properties.m_mode == ItemClass.Availability.AssetEditor) {
                    posX = EMath.Clamp(((prop.m_posX >> 4) + 32768) * PROPGRID_RESOLUTION / 65536, 0, PROPGRID_RESOLUTION - 1);
                    posZ = EMath.Clamp(((prop.m_posZ >> 4) + 32768) * PROPGRID_RESOLUTION / 65536, 0, PROPGRID_RESOLUTION - 1);
                } else {
                    posX = EMath.Clamp((prop.m_posX + 32768) * PROPGRID_RESOLUTION / 65536, 0, PROPGRID_RESOLUTION - 1);
                    posZ = EMath.Clamp((prop.m_posZ + 32768) * PROPGRID_RESOLUTION / 65536, 0, PROPGRID_RESOLUTION - 1);
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
            Vector2 vector = quad.Min();
            Vector2 vector2 = quad.Max();
            EPropInstance[] props = m_props.m_buffer;
            int startX = EMath.Max((int)((vector.x - 8f) / PROPGRID_CELL_SIZE + 135f), 0);
            int startZ = EMath.Max((int)((vector.y - 8f) / PROPGRID_CELL_SIZE + 135f), 0);
            int endX = EMath.Min((int)((vector2.x + 8f) / PROPGRID_CELL_SIZE + 135f), PROPGRID_RESOLUTION - 1);
            int endZ = EMath.Min((int)((vector2.y + 8f) / PROPGRID_CELL_SIZE + 135f), PROPGRID_RESOLUTION - 1);
            for (int i = startZ; i <= endZ; i++) {
                for (int j = startX; j <= endX; j++) {
                    uint propID = m_propGrid[i * PROPGRID_RESOLUTION + j];
                    while (propID != 0) {
                        Vector3 position = props[propID].Position;
                        float intersect = EMath.Max(EMath.Max(vector.x - 8f - position.x, vector.y - 8f - position.z), EMath.Max(position.x - vector2.x - 8f, position.z - vector2.y - 8f));
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
            Bounds bounds = new Bounds(new Vector3(0f, 512f, 0f), new Vector3(17280f, 1152f, 17280f));
            if (ray.Clip(bounds)) {
                Vector3 vector = ray.b - ray.a;
                int x1 = (int)(ray.a.x / PROPGRID_CELL_SIZE + 135f);
                int z1 = (int)(ray.a.z / PROPGRID_CELL_SIZE + 135f);
                int x2 = (int)(ray.b.x / PROPGRID_CELL_SIZE + 135f);
                int z2 = (int)(ray.b.z / PROPGRID_CELL_SIZE + 135f);
                float rangeX = EMath.Abs(vector.x);
                float rangeZ = EMath.Abs(vector.z);
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
                            startX = EMath.Max((int)((vector4.x - 72f) / PROPGRID_CELL_SIZE + 135f), 0);
                        } else {
                            startX = EMath.Max(num11, 0);
                        }
                        if ((num11 == x1 && num7 < 0) || (num11 == x2 && num7 > 0)) {
                            endX = EMath.Min((int)((vector4.x + 72f) / PROPGRID_CELL_SIZE + 135f), PROPGRID_RESOLUTION - 1);
                        } else {
                            endX = EMath.Min(num11, PROPGRID_RESOLUTION - 1);
                        }
                        startZ = EMath.Max((int)((EMath.Min(vector2.z, vector4.z) - 72f) / PROPGRID_CELL_SIZE + 135f), 0);
                        endZ = EMath.Min((int)((EMath.Max(vector2.z, vector4.z) + 72f) / PROPGRID_CELL_SIZE + 135f), PROPGRID_RESOLUTION - 1);
                    } else {
                        if ((num12 == z1 && num8 > 0) || (num12 == z2 && num8 < 0)) {
                            startZ = EMath.Max((int)((vector4.z - 72f) / PROPGRID_CELL_SIZE + 135f), 0);
                        } else {
                            startZ = EMath.Max(num12, 0);
                        }
                        if ((num12 == z1 && num8 < 0) || (num12 == z2 && num8 > 0)) {
                            endZ = EMath.Min((int)((vector4.z + 72f) / PROPGRID_CELL_SIZE + 135f), PROPGRID_RESOLUTION - 1);
                        } else {
                            endZ = EMath.Min(num12, PROPGRID_RESOLUTION - 1);
                        }
                        startX = EMath.Max((int)((EMath.Min(vector2.x, vector4.x) - 72f) / PROPGRID_CELL_SIZE + 135f), 0);
                        endX = EMath.Min((int)((EMath.Max(vector2.x, vector4.x) + 72f) / PROPGRID_CELL_SIZE + 135f), PROPGRID_RESOLUTION - 1);
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
            hit = EMath.Vector3Zero;
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
            int startX = EMath.Max((int)((x - 8f) / PROPGRID_CELL_SIZE + 135f), 0);
            int startZ = EMath.Max((int)((z - 8f) / PROPGRID_CELL_SIZE + 135f), 0);
            int endX = EMath.Min((int)((x2 + 8f) / PROPGRID_CELL_SIZE + 135f), PROPGRID_RESOLUTION - 1);
            int endZ = EMath.Min((int)((z2 + 8f) / PROPGRID_CELL_SIZE + 135f), PROPGRID_RESOLUTION - 1);
            for (int i = startZ; i <= endZ; i++) {
                for (int j = startX; j <= endX; j++) {
                    uint propID = propGrid[i * PROPGRID_RESOLUTION + j];
                    while (propID != 0) {
                        Vector3 position = props[propID].Position;
                        float intersect = EMath.Max(EMath.Max(x - 8f - position.x, z - 8f - position.z), EMath.Max(position.x - x2 - 8f, position.z - z2 - 8f));
                        if (intersect < 0f) {
                            props[propID].TerrainUpdated(propID, x, z, x2, z2);
                        }
                        propID = props[propID].m_nextGridProp;
                    }
                }
            }
        }

        public static void AfterTerrainUpdate(TerrainArea heightArea) {
            uint[] propGrid = m_propGrid;
            EPropInstance[] props = m_props.m_buffer;
            float x = heightArea.m_min.x;
            float z = heightArea.m_min.z;
            float x2 = heightArea.m_max.x;
            float z2 = heightArea.m_max.z;
            int startX = EMath.Max((int)((x - 8f) / PROPGRID_CELL_SIZE + 135f), 0);
            int startZ = EMath.Max((int)((z - 8f) / PROPGRID_CELL_SIZE + 135f), 0);
            int endX = EMath.Min((int)((x2 + 8f) / PROPGRID_CELL_SIZE + 135f), PROPGRID_RESOLUTION - 1);
            int endZ = EMath.Min((int)((z2 + 8f) / PROPGRID_CELL_SIZE + 135f), PROPGRID_RESOLUTION - 1);
            for (int i = startZ; i <= endZ; i++) {
                for (int j = startX; j <= endX; j++) {
                    uint propID = propGrid[i * PROPGRID_RESOLUTION + j];
                    while (propID != 0) {
                        Vector3 position = props[propID].Position;
                        float intersect = EMath.Max(EMath.Max(x - 8f - position.x, z - 8f - position.z), EMath.Max(position.x - x2 - 8f, position.z - z2 - 8f));
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

        public static void UpdateData(PropManager pmInstance) {
            int limit = MAX_PROP_LIMIT;
            EPropInstance[] props = m_props.m_buffer;
            for (int i = 1; i < limit; i++) {
                if (props[i].m_flags != 0 && props[i].Info is null) {
                    pmInstance.ReleaseProp((uint)i);
                }
            }
            pmInstance.m_infoCount = PrefabCollection<PropInfo>.PrefabCount();
        }
    }
}
