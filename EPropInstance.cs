using ColossalFramework;
using ColossalFramework.Math;
using System;
using System.Runtime.InteropServices;
using UnityEngine;
using static EManagersLib.EPropManager;

namespace EManagersLib {
    [StructLayout(LayoutKind.Explicit)]
    public struct EPropInstance {
        public const ushort CREATEDFLAG = 0x0001;
        public const ushort DELETEDFLAG = 0x0002;
        public const ushort HIDDENFLAG = 0x0004;
        public const ushort HIDDENMASK = 0xfffb;
        public const ushort CONFORMFLAG = 0x0008;
        public const ushort SINGLEFLAG = 0x0010;
        public const ushort SINGLEMASK = 0xffef;
        public const ushort FIXEDHEIGHTFLAG = 0x0020;
        public const ushort FIXEDHEIGHTMASK = 0xffdf;
        public const ushort BLOCKEDFLAG = 0x0040;
        public const ushort BLOCKEDMASK = 0xffbf;

        [Flags]
        public enum Flags : ushort {
            None = 0x0000,
            Created = 0x0001,
            Deleted = 0x0002,
            Hidden = 0x0004,
            Conform = 0x0008,
            Single = 0x0010,
            FixedHeight = 0x0020,
            Blocked = 0x0040,
            All = 0xffff
        }
        [FieldOffset(0)] public PropInstance propInstance;
        [FieldOffset(0)] public ushort __old_m_nextGridProp_Placeholder__; /* m_nextGridProp */
        [FieldOffset(2)] public short m_posX;
        [FieldOffset(4)] public short m_posZ;
        [FieldOffset(6)] public ushort m_posY;
        [FieldOffset(8)] public ushort m_angle;
        [FieldOffset(10)] public ushort m_flags;
        [FieldOffset(12)] public ushort m_infoIndex;
        [FieldOffset(14)] public uint m_nextGridProp;
        [FieldOffset(18)] public float m_scale;
        [FieldOffset(22)] public float m_preciseX;
        [FieldOffset(26)] public float m_preciseZ;
        [FieldOffset(30)] public Color m_color;

        public PropInfo Info {
            get => PrefabCollection<PropInfo>.GetPrefab(m_infoIndex);
            set => m_infoIndex = (ushort)EMath.Clamp(value.m_prefabDataIndex, 0, 65535);
        }
        public bool Single {
            get => (m_flags & SINGLEFLAG) != 0u;
            set => m_flags = value ? (ushort)(m_flags | SINGLEFLAG) : (ushort)(m_flags & SINGLEMASK);
        }
        public bool FixedHeight {
            get => (m_flags & FIXEDHEIGHTFLAG) != 0u;
            set => m_flags = value ? (ushort)(m_flags | FIXEDHEIGHTFLAG) : (ushort)(m_flags & FIXEDHEIGHTMASK);
        }
        public bool Blocked {
            get => (m_flags & BLOCKEDFLAG) != 0u;
            set => m_flags = value ? (UsePropAnarchy ? m_flags : (ushort)(m_flags | BLOCKEDFLAG)) : (ushort)(m_flags & BLOCKEDMASK);
        }
        public bool Hidden {
            get => (m_flags & HIDDENFLAG) != 0u;
            set => m_flags = value ? (ushort)(m_flags | HIDDENFLAG) : (ushort)(m_flags & HIDDENMASK);
        }
        public Vector3 Position {
            get {
                Vector3 result;
                if (m_mode == ItemClass.Availability.AssetEditor) {
                    result.x = m_posX * 0.0164794922f;
                    result.y = m_posY / PROPGRID_CELL_SIZE;
                    result.z = m_posZ * 0.0164794922f;
                    return result;
                }
                result.x = (m_posX + m_preciseX) * 0.263671875f;
                result.y = m_posY / PROPGRID_CELL_SIZE;
                result.z = (m_posZ + m_preciseZ) * 0.263671875f;
                return result;
            }
            set {
                if (m_mode == ItemClass.Availability.AssetEditor) {
                    m_posX = (short)EMath.Clamp(EMath.RoundToInt(value.x * 60.68148f), -32767, 32767);
                    m_posZ = (short)EMath.Clamp(EMath.RoundToInt(value.z * 60.68148f), -32767, 32767);
                    m_posY = (ushort)EMath.Clamp(EMath.RoundToInt(value.y * 64f), 0, 65535);
                } else {
                    m_posX = (short)EMath.Clamp(EMath.RoundToInt(value.x * 3.79259253f), -32767, 32767);
                    m_posZ = (short)EMath.Clamp(EMath.RoundToInt(value.z * 3.79259253f), -32767, 32767);
                    m_posY = (ushort)EMath.Clamp(EMath.RoundToInt(value.y * 64f), 0, 65535);
                    m_preciseX = value.x * 3.79259253f - m_posX;
                    m_preciseZ = value.z * 3.79259253f - m_posZ;
                }
            }
        }

        public float Angle {
            get => m_angle * 9.58738E-05f;
            set => m_angle = (ushort)(value * 10430.3779f + 0.5f);
        }

        public static void EnhancedRenderInstance(RenderManager.CameraInfo cameraInfo, uint propID) {
            Vector3 v3zero = EMath.Vector3Zero;
            Vector3 v3down = EMath.Vector3Down;
            EPropInstance[] props = m_props.m_buffer;
            TerrainManager tmInstance = Singleton<TerrainManager>.instance;
            ref float simulationTimeDelta = ref Singleton<SimulationManager>.instance.m_simulationTimeDelta;
            if ((props[propID].m_flags & (HIDDENFLAG | BLOCKEDFLAG)) == 0) {
                PropInfo info = props[propID].Info;
                Vector3 position = props[propID].Position;
                float scale = props[propID].m_scale;
                float angle = props[propID].m_angle;
                Color color = props[propID].m_color;
                Vector4 objectIndex = RenderManager.DefaultColorLocation;
                if (info.m_prefabInitialized && cameraInfo.CheckRenderDistance(position, info.m_maxRenderDistance) && cameraInfo.Intersect(position, info.m_generatedInfo.m_size.y * info.m_maxScale)) {
                    InstanceID id = default;
                    id.SetProp32(propID);
                    if (info.m_requireWaterMap) {
                        position.y = tmInstance.SampleRawHeightWithWater(position, false, 0f);
                        tmInstance.GetHeightMapping(position, out Texture heightMap, out Vector4 heightMapping, out Vector4 surfaceMapping);
                        tmInstance.GetWaterMapping(position, out Texture waterHeightMap, out Vector4 waterHeightMapping, out Vector4 waterSurfaceMapping);
                        if (info.m_hasEffects) {
                            Matrix4x4 matrix4x = Matrix4x4.TRS(position, Quaternion.AngleAxis(angle * 57.29578f, v3down), new Vector3(scale, scale, scale));
                            PropInfo.Effect[] effects = info.m_effects;
                            int len = effects.Length;
                            for (int l = 0; l < len; l++) {
                                effects[l].m_effect.RenderEffect(id, new EffectInfo.SpawnArea(matrix4x.MultiplyPoint(effects[l].m_position),
                                    matrix4x.MultiplyVector(effects[l].m_direction), 0f), v3zero, 0f, 1f, -1f, simulationTimeDelta, cameraInfo);
                            }
                        }
                        if (info.m_hasRenderer && (cameraInfo.m_layerMask & 1 << info.m_prefabDataLayer) != 0) {
                            if (Singleton<InfoManager>.instance.CurrentMode == InfoManager.InfoMode.None) {
                                if (info.m_illuminationOffRange.x < 1000f || info.m_illuminationBlinkType != LightEffect.BlinkType.None) {
                                    LightSystem lightSystem = Singleton<RenderManager>.instance.lightSystem;
                                    EMath.SetRandomizerSeed(id.Index);
                                    float num = info.m_illuminationOffRange.x + EMath.randomizer.Int32(100000u) * 1E-05f * (info.m_illuminationOffRange.y - info.m_illuminationOffRange.x);
                                    objectIndex.z = EMath.SmoothStep(num + 0.01f, num - 0.01f, lightSystem.DayLightIntensity);
                                    if (info.m_illuminationBlinkType != LightEffect.BlinkType.None) {
                                        Vector4 blinkVector = LightEffect.GetBlinkVector(info.m_illuminationBlinkType);
                                        float num2 = num * 3.71f + Singleton<SimulationManager>.instance.m_simulationTimer / blinkVector.w;
                                        num2 = (num2 - (int)num2) * blinkVector.w;
                                        float num3 = EMath.SmoothStep(blinkVector.x, blinkVector.y, num2);
                                        float num4 = EMath.SmoothStep(blinkVector.w, blinkVector.z, num2);
                                        objectIndex.z *= 1f - num3 * num4;
                                    }
                                } else {
                                    objectIndex.z = 1f;
                                }
                            }
                            if (cameraInfo is null || cameraInfo.CheckRenderDistance(position, info.m_lodRenderDistance)) {
                                MaterialPropertyBlock materialBlock = m_materialBlock;
                                materialBlock.Clear();
                                materialBlock.SetColor(ID_Color, color);
                                materialBlock.SetTexture(ID_HeightMap, heightMap);
                                materialBlock.SetVector(ID_HeightMapping, heightMapping);
                                materialBlock.SetVector(ID_SurfaceMapping, surfaceMapping);
                                materialBlock.SetVector(ID_ObjectIndex, objectIndex);
                                materialBlock.SetTexture(ID_WaterHeightMap, waterHeightMap);
                                materialBlock.SetVector(ID_WaterHeightMapping, waterHeightMapping);
                                materialBlock.SetVector(ID_WaterSurfaceMapping, waterSurfaceMapping);
                                if (info.m_rollLocation != null) {
                                    info.m_material.SetVectorArray(ID_RollLocation, info.m_rollLocation);
                                    info.m_material.SetVectorArray(ID_RollParams, info.m_rollParams);
                                }
                                m_pmInstance.m_drawCallData.m_defaultCalls++;
                                Graphics.DrawMesh(info.m_mesh,
                                    Matrix4x4.TRS(position, Quaternion.AngleAxis(angle * 57.29578f, EMath.Vector3Down), new Vector3(scale, scale, scale)),
                                    info.m_material, info.m_prefabDataLayer, null, 0, materialBlock);
                            } else {
                                if (heightMap != info.m_lodHeightMap || waterHeightMap != info.m_lodWaterHeightMap) {
                                    if (info.m_lodCount != 0) {
                                        RenderLod(cameraInfo, info);
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
                                    RenderLod(cameraInfo, info);
                                }
                            }
                        }
                    } else if (info.m_requireHeightMap) {
                        Singleton<TerrainManager>.instance.GetHeightMapping(position, out Texture heightMap, out Vector4 heightMapping, out Vector4 surfaceMapping);
                        if (info.m_hasEffects && (true || info.m_alwaysActive)) {
                            Matrix4x4 matrix4x = default;
                            matrix4x.SetTRS(position, Quaternion.AngleAxis(angle * 57.29578f, EMath.Vector3Down), new Vector3(scale, scale, scale));
                            for (int i = 0; i < info.m_effects.Length; i++) {
                                info.m_effects[i].m_effect.RenderEffect(id, new EffectInfo.SpawnArea(matrix4x.MultiplyPoint(info.m_effects[i].m_position),
                                    matrix4x.MultiplyVector(info.m_effects[i].m_direction), 0f), EMath.Vector3Zero, 0f, 1f, -1f, simulationTimeDelta, cameraInfo);
                            }
                        }
                        if (info.m_hasRenderer && (cameraInfo.m_layerMask & 1 << info.m_prefabDataLayer) != 0) {
                            if (Singleton<InfoManager>.instance.CurrentMode == InfoManager.InfoMode.None) {
                                if (!true && !info.m_alwaysActive) {
                                    objectIndex.z = 0f;
                                } else if (info.m_illuminationOffRange.x < 1000f || info.m_illuminationBlinkType != LightEffect.BlinkType.None) {
                                    LightSystem lightSystem = Singleton<RenderManager>.instance.lightSystem;
                                    EMath.SetRandomizerSeed(id.Index);
                                    float num = info.m_illuminationOffRange.x + EMath.randomizer.Int32(100000u) * 1E-05f * (info.m_illuminationOffRange.y - info.m_illuminationOffRange.x);
                                    objectIndex.z = EMath.SmoothStep(num + 0.01f, num - 0.01f, lightSystem.DayLightIntensity);
                                    if (info.m_illuminationBlinkType != LightEffect.BlinkType.None) {
                                        Vector4 blinkVector = LightEffect.GetBlinkVector(info.m_illuminationBlinkType);
                                        float num2 = num * 3.71f + Singleton<SimulationManager>.instance.m_simulationTimer / blinkVector.w;
                                        num2 = (num2 - (int)num2) * blinkVector.w;
                                        float num3 = EMath.SmoothStep(blinkVector.x, blinkVector.y, num2);
                                        float num4 = EMath.SmoothStep(blinkVector.w, blinkVector.z, num2);
                                        objectIndex.z *= 1f - num3 * num4;
                                    }
                                } else {
                                    objectIndex.z = 1f;
                                }
                            }
                            if (cameraInfo is null || cameraInfo.CheckRenderDistance(position, info.m_lodRenderDistance)) {
                                MaterialPropertyBlock materialBlock = m_materialBlock;
                                materialBlock.Clear();
                                materialBlock.SetColor(ID_Color, color);
                                materialBlock.SetTexture(ID_HeightMap, heightMap);
                                materialBlock.SetVector(ID_HeightMapping, heightMapping);
                                materialBlock.SetVector(ID_SurfaceMapping, surfaceMapping);
                                materialBlock.SetVector(ID_ObjectIndex, objectIndex);
                                if (!(info.m_rollLocation is null)) {
                                    info.m_material.SetVectorArray(ID_RollLocation, info.m_rollLocation);
                                    info.m_material.SetVectorArray(ID_RollParams, info.m_rollParams);
                                }
                                m_pmInstance.m_drawCallData.m_defaultCalls++;
                                Graphics.DrawMesh(info.m_mesh,
                                    Matrix4x4.TRS(position, Quaternion.AngleAxis(angle * 57.29578f, EMath.Vector3Down), new Vector3(scale, scale, scale)),
                                    info.m_material, info.m_prefabDataLayer, null, 0, materialBlock);
                            } else {
                                if (heightMap != info.m_lodHeightMap) {
                                    if (info.m_lodCount != 0) {
                                        RenderLod(cameraInfo, info);
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
                                    RenderLod(cameraInfo, info);
                                }
                            }
                        }
                    } else {
                        if (info.m_hasEffects && (true || info.m_alwaysActive)) {
                            Matrix4x4 matrix4x = Matrix4x4.TRS(position, Quaternion.AngleAxis(angle * 57.29578f, EMath.Vector3Down), new Vector3(scale, scale, scale));
                            PropInfo.Effect[] effects = info.m_effects;
                            int len = effects.Length;
                            Vector3 zero = EMath.Vector3Zero;
                            for (int i = 0; i < len; i++) {
                                EffectInfo.SpawnArea area = new EffectInfo.SpawnArea(matrix4x.MultiplyPoint(effects[i].m_position), matrix4x.MultiplyVector(effects[i].m_direction), 0f);
                                effects[i].m_effect.RenderEffect(id, area, zero, 0f, 1f, -1f, simulationTimeDelta, cameraInfo);
                            }
                        }
                        if (info.m_hasRenderer && (cameraInfo.m_layerMask & 1 << info.m_prefabDataLayer) != 0) {
                            if (Singleton<InfoManager>.instance.CurrentMode == InfoManager.InfoMode.None) {
                                if (!true && !info.m_alwaysActive) {
                                    objectIndex.z = 0f;
                                } else if (info.m_illuminationOffRange.x < 1000f || info.m_illuminationBlinkType != LightEffect.BlinkType.None) {
                                    LightSystem lightSystem = Singleton<RenderManager>.instance.lightSystem;
                                    EMath.SetRandomizerSeed(id.Index);
                                    float num = info.m_illuminationOffRange.x + EMath.randomizer.Int32(100000u) * 1E-05f * (info.m_illuminationOffRange.y - info.m_illuminationOffRange.x);
                                    objectIndex.z = EMath.SmoothStep(num + 0.01f, num - 0.01f, lightSystem.DayLightIntensity);
                                    if (info.m_illuminationBlinkType != LightEffect.BlinkType.None) {
                                        Vector4 blinkVector = LightEffect.GetBlinkVector(info.m_illuminationBlinkType);
                                        float num2 = num * 3.71f + Singleton<SimulationManager>.instance.m_simulationTimer / blinkVector.w;
                                        num2 = (num2 - (int)num2) * blinkVector.w;
                                        float num3 = EMath.SmoothStep(blinkVector.x, blinkVector.y, num2);
                                        float num4 = EMath.SmoothStep(blinkVector.w, blinkVector.z, num2);
                                        objectIndex.z *= 1f - num3 * num4;
                                    }
                                } else {
                                    objectIndex.z = 1f;
                                }
                            }
                            if (cameraInfo is null || cameraInfo.CheckRenderDistance(position, info.m_lodRenderDistance)) {
                                MaterialPropertyBlock materialBlock = m_materialBlock;
                                materialBlock.Clear();
                                materialBlock.SetColor(ID_Color, color);
                                materialBlock.SetVector(ID_ObjectIndex, objectIndex);
                                if (!(info.m_rollLocation is null)) {
                                    info.m_material.SetVectorArray(ID_RollLocation, info.m_rollLocation);
                                    info.m_material.SetVectorArray(ID_RollParams, info.m_rollParams);
                                }
                                m_pmInstance.m_drawCallData.m_defaultCalls++;
                                Graphics.DrawMesh(info.m_mesh, Matrix4x4.TRS(position, Quaternion.AngleAxis(angle * 57.29578f, EMath.Vector3Down), new Vector3(scale, scale, scale)),
                                                    info.m_material, info.m_prefabDataLayer, null, 0, materialBlock);
                            } else if (info.m_lodMaterialCombined is null) {
                                MaterialPropertyBlock materialBlock2 = m_materialBlock;
                                materialBlock2.Clear();
                                materialBlock2.SetColor(ID_Color, color);
                                materialBlock2.SetVector(ID_ObjectIndex, objectIndex);
                                if (!(info.m_rollLocation is null)) {
                                    info.m_material.SetVectorArray(ID_RollLocation, info.m_rollLocation);
                                    info.m_material.SetVectorArray(ID_RollParams, info.m_rollParams);
                                }
                                m_pmInstance.m_drawCallData.m_defaultCalls++;
                                Graphics.DrawMesh(info.m_lodMesh, Matrix4x4.TRS(position, Quaternion.AngleAxis(angle * 57.29578f, Vector3.down), new Vector3(scale, scale, scale)),
                                                    info.m_lodMaterial, info.m_prefabDataLayer, null, 0, materialBlock2);
                            } else {
                                objectIndex.w = scale;
                                info.m_lodLocations[info.m_lodCount] = new Vector4(position.x, position.y, position.z, angle);
                                info.m_lodObjectIndices[info.m_lodCount] = objectIndex;
                                info.m_lodColors[info.m_lodCount] = color.linear;
                                info.m_lodMin = EMath.Min(info.m_lodMin, position);
                                info.m_lodMax = EMath.Max(info.m_lodMax, position);
                                if (++info.m_lodCount == info.m_lodLocations.Length) {
                                    RenderLod(cameraInfo, info);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void RenderInstance(RenderManager.CameraInfo cameraInfo, uint propID, int layerMask) {
            if ((m_flags & (HIDDENFLAG | BLOCKEDFLAG)) == 0) {
                PropInfo info = Info;
                Vector3 position = Position;
                if (cameraInfo.ECheckRenderDistance(position, info.m_maxRenderDistance) && cameraInfo.Intersect(position, info.m_generatedInfo.m_size.y * info.m_maxScale)) {
                    InstanceID id = default;
                    id.SetProp32(propID);
                    if (info.m_requireWaterMap) {
                        TerrainManager tmInstance = Singleton<TerrainManager>.instance;
                        position.y = tmInstance.SampleRawHeightWithWater(position, false, 0f);
                        tmInstance.GetHeightMapping(position, out Texture heightMap, out Vector4 heightMapping, out Vector4 surfaceMapping);
                        tmInstance.GetWaterMapping(position, out Texture waterHeightMap, out Vector4 waterHeightMapping, out Vector4 waterSurfaceMapping);
                        RenderInstance(cameraInfo, info, id, position, m_scale, Angle, m_color, RenderManager.DefaultColorLocation,
                                       true, heightMap, heightMapping, surfaceMapping, waterHeightMap, waterHeightMapping, waterSurfaceMapping);
                    } else if (info.m_requireHeightMap) {
                        Singleton<TerrainManager>.instance.GetHeightMapping(position, out Texture heightMap, out Vector4 heightMapping, out Vector4 surfaceMapping);
                        RenderInstance(cameraInfo, info, id, position, m_scale, Angle, m_color, RenderManager.DefaultColorLocation, true, heightMap, heightMapping, surfaceMapping);
                    } else {
                        RenderInstance(cameraInfo, info, id, position, m_scale, Angle, m_color, RenderManager.DefaultColorLocation, true);
                    }
                }
            }
        }

        public static void RenderInstance(RenderManager.CameraInfo cameraInfo, PropInfo info, InstanceID id, Vector3 position, float scale, float angle, Color color, Vector4 objectIndex, bool active) {
            if (info.m_prefabInitialized) {
                if (info.m_hasEffects && (active || info.m_alwaysActive)) {
                    Matrix4x4 matrix4x = Matrix4x4.TRS(position, Quaternion.AngleAxis(angle * 57.29578f, EMath.Vector3Down), new Vector3(scale, scale, scale));
                    float simulationTimeDelta = Singleton<SimulationManager>.instance.m_simulationTimeDelta;
                    PropInfo.Effect[] effects = info.m_effects;
                    int len = effects.Length;
                    Vector3 zero = EMath.Vector3Zero;
                    for (int i = 0; i < len; i++) {
                        EffectInfo.SpawnArea area = new EffectInfo.SpawnArea(matrix4x.MultiplyPoint(effects[i].m_position), matrix4x.MultiplyVector(effects[i].m_direction), 0f);
                        effects[i].m_effect.RenderEffect(id, area, zero, 0f, 1f, -1f, simulationTimeDelta, cameraInfo);
                    }
                }
                if (info.m_hasRenderer && (cameraInfo.m_layerMask & 1 << info.m_prefabDataLayer) != 0) {
                    if (Singleton<InfoManager>.instance.CurrentMode == InfoManager.InfoMode.None) {
                        if (!active && !info.m_alwaysActive) {
                            objectIndex.z = 0f;
                        } else if (info.m_illuminationOffRange.x < 1000f || info.m_illuminationBlinkType != LightEffect.BlinkType.None) {
                            LightSystem lightSystem = Singleton<RenderManager>.instance.lightSystem;
                            EMath.SetRandomizerSeed(id.Index);
                            float num = info.m_illuminationOffRange.x + EMath.randomizer.Int32(100000u) * 1E-05f * (info.m_illuminationOffRange.y - info.m_illuminationOffRange.x);
                            objectIndex.z = EMath.SmoothStep(num + 0.01f, num - 0.01f, lightSystem.DayLightIntensity);
                            if (info.m_illuminationBlinkType != LightEffect.BlinkType.None) {
                                Vector4 blinkVector = LightEffect.GetBlinkVector(info.m_illuminationBlinkType);
                                float num2 = num * 3.71f + Singleton<SimulationManager>.instance.m_simulationTimer / blinkVector.w;
                                num2 = (num2 - (int)num2) * blinkVector.w;
                                float num3 = EMath.SmoothStep(blinkVector.x, blinkVector.y, num2);
                                float num4 = EMath.SmoothStep(blinkVector.w, blinkVector.z, num2);
                                objectIndex.z *= 1f - num3 * num4;
                            }
                        } else {
                            objectIndex.z = 1f;
                        }
                    }
                    if (cameraInfo is null || cameraInfo.ECheckRenderDistance(position, info.m_lodRenderDistance)) {
                        MaterialPropertyBlock materialBlock = m_materialBlock;
                        materialBlock.Clear();
                        materialBlock.SetColor(ID_Color, color);
                        materialBlock.SetVector(ID_ObjectIndex, objectIndex);
                        if (!(info.m_rollLocation is null)) {
                            info.m_material.SetVectorArray(ID_RollLocation, info.m_rollLocation);
                            info.m_material.SetVectorArray(ID_RollParams, info.m_rollParams);
                        }
                        m_pmInstance.m_drawCallData.m_defaultCalls++;
                        Graphics.DrawMesh(info.m_mesh, Matrix4x4.TRS(position, Quaternion.AngleAxis(angle * 57.29578f, EMath.Vector3Down), new Vector3(scale, scale, scale)),
                                          info.m_material, info.m_prefabDataLayer, null, 0, materialBlock);
                    } else if (info.m_lodMaterialCombined is null) {
                        MaterialPropertyBlock materialBlock2 = m_materialBlock;
                        materialBlock2.Clear();
                        materialBlock2.SetColor(ID_Color, color);
                        materialBlock2.SetVector(ID_ObjectIndex, objectIndex);
                        if (!(info.m_rollLocation is null)) {
                            info.m_material.SetVectorArray(ID_RollLocation, info.m_rollLocation);
                            info.m_material.SetVectorArray(ID_RollParams, info.m_rollParams);
                        }
                        m_pmInstance.m_drawCallData.m_defaultCalls++;
                        Graphics.DrawMesh(info.m_lodMesh, Matrix4x4.TRS(position, Quaternion.AngleAxis(angle * 57.29578f, EMath.Vector3Down), new Vector3(scale, scale, scale)),
                                          info.m_lodMaterial, info.m_prefabDataLayer, null, 0, materialBlock2);
                    } else {
                        objectIndex.w = scale;
                        info.m_lodLocations[info.m_lodCount] = new Vector4(position.x, position.y, position.z, angle);
                        info.m_lodObjectIndices[info.m_lodCount] = objectIndex;
                        info.m_lodColors[info.m_lodCount] = color.linear;
                        info.m_lodMin = EMath.Min(info.m_lodMin, position);
                        info.m_lodMax = EMath.Max(info.m_lodMax, position);
                        if (++info.m_lodCount == info.m_lodLocations.Length) {
                            RenderLod(cameraInfo, info);
                        }
                    }
                }
            }
        }

        public static void RenderInstance(RenderManager.CameraInfo cameraInfo, PropInfo info, InstanceID id, Matrix4x4 matrix, Vector3 position, float scale, float angle, Color color, Vector4 objectIndex, bool active) {
            if (info.m_prefabInitialized) {
                if (info.m_hasEffects && (active || info.m_alwaysActive)) {
                    float simulationTimeDelta = Singleton<SimulationManager>.instance.m_simulationTimeDelta;
                    PropInfo.Effect[] effects = info.m_effects;
                    int len = effects.Length;
                    for (int i = 0; i < len; i++) {
                        Vector3 position2 = matrix.MultiplyPoint(effects[i].m_position);
                        Vector3 direction = matrix.MultiplyVector(effects[i].m_direction);
                        EffectInfo.SpawnArea area = new EffectInfo.SpawnArea(position2, direction, 0f);
                        effects[i].m_effect.RenderEffect(id, area, EMath.Vector3Zero, 0f, 1f, -1f, simulationTimeDelta, cameraInfo);
                    }
                }
                if (info.m_hasRenderer && (cameraInfo.m_layerMask & 1 << info.m_prefabDataLayer) != 0) {
                    if (Singleton<InfoManager>.instance.CurrentMode == InfoManager.InfoMode.None) {
                        if (!active && !info.m_alwaysActive) {
                            objectIndex.z = 0f;
                        } else if (info.m_illuminationOffRange.x < 1000f || info.m_illuminationBlinkType != LightEffect.BlinkType.None) {
                            LightSystem lightSystem = Singleton<RenderManager>.instance.lightSystem;
                            EMath.SetRandomizerSeed(id.Index);
                            float num = info.m_illuminationOffRange.x + EMath.randomizer.Int32(100000u) * 1E-05f * (info.m_illuminationOffRange.y - info.m_illuminationOffRange.x);
                            objectIndex.z = EMath.SmoothStep(num + 0.01f, num - 0.01f, lightSystem.DayLightIntensity);
                            if (info.m_illuminationBlinkType != LightEffect.BlinkType.None) {
                                Vector4 blinkVector = LightEffect.GetBlinkVector(info.m_illuminationBlinkType);
                                float num2 = num * 3.71f + Singleton<SimulationManager>.instance.m_simulationTimer / blinkVector.w;
                                num2 = (num2 - (int)num2) * blinkVector.w;
                                float num3 = EMath.SmoothStep(blinkVector.x, blinkVector.y, num2);
                                float num4 = EMath.SmoothStep(blinkVector.w, blinkVector.z, num2);
                                objectIndex.z *= 1f - num3 * num4;
                            }
                        } else {
                            objectIndex.z = 1f;
                        }
                    }
                    if (cameraInfo is null || cameraInfo.ECheckRenderDistance(position, info.m_lodRenderDistance)) {
                        PropManager instance = m_pmInstance;
                        MaterialPropertyBlock materialBlock = m_materialBlock;
                        materialBlock.Clear();
                        materialBlock.SetColor(ID_Color, color);
                        materialBlock.SetVector(ID_ObjectIndex, objectIndex);
                        if (!(info.m_rollLocation is null)) {
                            info.m_material.SetVectorArray(ID_RollLocation, info.m_rollLocation);
                            info.m_material.SetVectorArray(ID_RollParams, info.m_rollParams);
                        }
                        instance.m_drawCallData.m_defaultCalls++;
                        Graphics.DrawMesh(info.m_mesh, matrix, info.m_material, info.m_prefabDataLayer, null, 0, materialBlock);
                    } else if (info.m_lodMaterialCombined is null) {
                        MaterialPropertyBlock materialBlock = m_materialBlock;
                        materialBlock.Clear();
                        materialBlock.SetColor(ID_Color, color);
                        materialBlock.SetVector(ID_ObjectIndex, objectIndex);
                        if (!(info.m_rollLocation is null)) {
                            info.m_material.SetVectorArray(ID_RollLocation, info.m_rollLocation);
                            info.m_material.SetVectorArray(ID_RollParams, info.m_rollParams);
                        }
                        m_pmInstance.m_drawCallData.m_defaultCalls++;
                        Graphics.DrawMesh(info.m_lodMesh, matrix, info.m_lodMaterial, info.m_prefabDataLayer, null, 0, materialBlock);
                    } else {
                        objectIndex.w = scale;
                        info.m_lodLocations[info.m_lodCount] = new Vector4(position.x, position.y, position.z, angle);
                        info.m_lodObjectIndices[info.m_lodCount] = objectIndex;
                        info.m_lodColors[info.m_lodCount] = color.linear;
                        info.m_lodMin = EMath.Min(info.m_lodMin, position);
                        info.m_lodMax = EMath.Max(info.m_lodMax, position);
                        if (++info.m_lodCount == info.m_lodLocations.Length) {
                            RenderLod(cameraInfo, info);
                        }
                    }
                }
            }
        }

        public static void RenderInstance(RenderManager.CameraInfo cameraInfo, PropInfo info, InstanceID id, Vector3 position, float scale, float angle, Color color, Vector4 objectIndex, bool active, Texture heightMap, Vector4 heightMapping, Vector4 surfaceMapping) {
            if (info.m_prefabInitialized) {
                if (info.m_hasEffects && (active || info.m_alwaysActive)) {
                    Matrix4x4 matrix4x = default;
                    matrix4x.SetTRS(position, Quaternion.AngleAxis(angle * 57.29578f, EMath.Vector3Down), new Vector3(scale, scale, scale));
                    float simulationTimeDelta = Singleton<SimulationManager>.instance.m_simulationTimeDelta;
                    for (int i = 0; i < info.m_effects.Length; i++) {
                        info.m_effects[i].m_effect.RenderEffect(id, new EffectInfo.SpawnArea(matrix4x.MultiplyPoint(info.m_effects[i].m_position),
                            matrix4x.MultiplyVector(info.m_effects[i].m_direction), 0f), EMath.Vector3Zero, 0f, 1f, -1f, simulationTimeDelta, cameraInfo);
                    }
                }
                if (info.m_hasRenderer && (cameraInfo.m_layerMask & 1 << info.m_prefabDataLayer) != 0) {
                    if (Singleton<InfoManager>.instance.CurrentMode == InfoManager.InfoMode.None) {
                        if (!active && !info.m_alwaysActive) {
                            objectIndex.z = 0f;
                        } else if (info.m_illuminationOffRange.x < 1000f || info.m_illuminationBlinkType != LightEffect.BlinkType.None) {
                            LightSystem lightSystem = Singleton<RenderManager>.instance.lightSystem;
                            EMath.SetRandomizerSeed(id.Index);
                            float num = info.m_illuminationOffRange.x + EMath.randomizer.Int32(100000u) * 1E-05f * (info.m_illuminationOffRange.y - info.m_illuminationOffRange.x);
                            objectIndex.z = EMath.SmoothStep(num + 0.01f, num - 0.01f, lightSystem.DayLightIntensity);
                            if (info.m_illuminationBlinkType != LightEffect.BlinkType.None) {
                                Vector4 blinkVector = LightEffect.GetBlinkVector(info.m_illuminationBlinkType);
                                float num2 = num * 3.71f + Singleton<SimulationManager>.instance.m_simulationTimer / blinkVector.w;
                                num2 = (num2 - (int)num2) * blinkVector.w;
                                float num3 = EMath.SmoothStep(blinkVector.x, blinkVector.y, num2);
                                float num4 = EMath.SmoothStep(blinkVector.w, blinkVector.z, num2);
                                objectIndex.z *= 1f - num3 * num4;
                            }
                        } else {
                            objectIndex.z = 1f;
                        }
                    }
                    if (cameraInfo is null || cameraInfo.ECheckRenderDistance(position, info.m_lodRenderDistance)) {
                        MaterialPropertyBlock materialBlock = m_materialBlock;
                        materialBlock.Clear();
                        materialBlock.SetColor(ID_Color, color);
                        materialBlock.SetTexture(ID_HeightMap, heightMap);
                        materialBlock.SetVector(ID_HeightMapping, heightMapping);
                        materialBlock.SetVector(ID_SurfaceMapping, surfaceMapping);
                        materialBlock.SetVector(ID_ObjectIndex, objectIndex);
                        if (!(info.m_rollLocation is null)) {
                            info.m_material.SetVectorArray(ID_RollLocation, info.m_rollLocation);
                            info.m_material.SetVectorArray(ID_RollParams, info.m_rollParams);
                        }
                        m_pmInstance.m_drawCallData.m_defaultCalls++;
                        Graphics.DrawMesh(info.m_mesh,
                            Matrix4x4.TRS(position, Quaternion.AngleAxis(angle * 57.29578f, EMath.Vector3Down), new Vector3(scale, scale, scale)),
                            info.m_material, info.m_prefabDataLayer, null, 0, materialBlock);
                    } else {
                        if (heightMap != info.m_lodHeightMap) {
                            if (info.m_lodCount != 0) {
                                RenderLod(cameraInfo, info);
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
                            RenderLod(cameraInfo, info);
                        }
                    }
                }
            }
        }

        public static void RenderInstance(RenderManager.CameraInfo cameraInfo, PropInfo info, InstanceID id, Vector3 position, float scale, float angle, Color color, Vector4 objectIndex, bool active, Texture heightMap, Vector4 heightMapping, Vector4 surfaceMapping, Texture waterHeightMap, Vector4 waterHeightMapping, Vector4 waterSurfaceMapping) {
            if (info.m_prefabInitialized) {
                if (info.m_hasEffects && (active || info.m_alwaysActive)) {
                    Matrix4x4 matrix4x = default;
                    matrix4x.SetTRS(position, Quaternion.AngleAxis(angle * 57.29578f, EMath.Vector3Down), new Vector3(scale, scale, scale));
                    float simulationTimeDelta = Singleton<SimulationManager>.instance.m_simulationTimeDelta;
                    for (int i = 0; i < info.m_effects.Length; i++) {
                        info.m_effects[i].m_effect.RenderEffect(id, new EffectInfo.SpawnArea(matrix4x.MultiplyPoint(info.m_effects[i].m_position),
                            matrix4x.MultiplyVector(info.m_effects[i].m_direction), 0f), EMath.Vector3Zero, 0f, 1f, -1f, simulationTimeDelta, cameraInfo);
                    }
                }
                if (info.m_hasRenderer && (cameraInfo.m_layerMask & 1 << info.m_prefabDataLayer) != 0) {
                    if (Singleton<InfoManager>.instance.CurrentMode == InfoManager.InfoMode.None) {
                        if (!active && !info.m_alwaysActive) {
                            objectIndex.z = 0f;
                        } else if (info.m_illuminationOffRange.x < 1000f || info.m_illuminationBlinkType != LightEffect.BlinkType.None) {
                            LightSystem lightSystem = Singleton<RenderManager>.instance.lightSystem;
                            EMath.SetRandomizerSeed(id.Index);
                            float num = info.m_illuminationOffRange.x + EMath.randomizer.Int32(100000u) * 1E-05f * (info.m_illuminationOffRange.y - info.m_illuminationOffRange.x);
                            objectIndex.z = EMath.SmoothStep(num + 0.01f, num - 0.01f, lightSystem.DayLightIntensity);
                            if (info.m_illuminationBlinkType != LightEffect.BlinkType.None) {
                                Vector4 blinkVector = LightEffect.GetBlinkVector(info.m_illuminationBlinkType);
                                float num2 = num * 3.71f + Singleton<SimulationManager>.instance.m_simulationTimer / blinkVector.w;
                                num2 = (num2 - (int)num2) * blinkVector.w;
                                float num3 = EMath.SmoothStep(blinkVector.x, blinkVector.y, num2);
                                float num4 = EMath.SmoothStep(blinkVector.w, blinkVector.z, num2);
                                objectIndex.z *= 1f - num3 * num4;
                            }
                        } else {
                            objectIndex.z = 1f;
                        }
                    }
                    if (cameraInfo is null || cameraInfo.ECheckRenderDistance(position, info.m_lodRenderDistance)) {
                        MaterialPropertyBlock materialBlock = m_materialBlock;
                        materialBlock.Clear();
                        materialBlock.SetColor(ID_Color, color);
                        materialBlock.SetTexture(ID_HeightMap, heightMap);
                        materialBlock.SetVector(ID_HeightMapping, heightMapping);
                        materialBlock.SetVector(ID_SurfaceMapping, surfaceMapping);
                        materialBlock.SetVector(ID_ObjectIndex, objectIndex);
                        materialBlock.SetTexture(ID_WaterHeightMap, waterHeightMap);
                        materialBlock.SetVector(ID_WaterHeightMapping, waterHeightMapping);
                        materialBlock.SetVector(ID_WaterSurfaceMapping, waterSurfaceMapping);
                        if (info.m_rollLocation != null) {
                            info.m_material.SetVectorArray(ID_RollLocation, info.m_rollLocation);
                            info.m_material.SetVectorArray(ID_RollParams, info.m_rollParams);
                        }
                        m_pmInstance.m_drawCallData.m_defaultCalls++;
                        Graphics.DrawMesh(info.m_mesh,
                            Matrix4x4.TRS(position, Quaternion.AngleAxis(angle * 57.29578f, EMath.Vector3Down), new Vector3(scale, scale, scale)),
                            info.m_material, info.m_prefabDataLayer, null, 0, materialBlock);
                    } else {
                        if (heightMap != info.m_lodHeightMap || waterHeightMap != info.m_lodWaterHeightMap) {
                            if (info.m_lodCount != 0) {
                                RenderLod(cameraInfo, info);
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
                            RenderLod(cameraInfo, info);
                        }
                    }
                }
            }
        }

        public static void RenderLod(RenderManager.CameraInfo cameraInfo, PropInfo info) {
            MaterialPropertyBlock materialBlock = m_materialBlock;
            materialBlock.Clear();
            Mesh mesh;
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
            for (int i = info.m_lodCount; i < lodCount; i++) {
                info.m_lodLocations[i] = cameraInfo.m_forward * -100000f;
                info.m_lodObjectIndices[i] = EMath.Vector4Zero;
                info.m_lodColors[i] = EMath.ColorClear;
            }
            materialBlock.SetVectorArray(ID_PropLocation, info.m_lodLocations);
            materialBlock.SetVectorArray(ID_PropObjectIndex, info.m_lodObjectIndices);
            materialBlock.SetVectorArray(ID_PropColor, info.m_lodColors);
            if (info.m_requireHeightMap) {
                materialBlock.SetTexture(ID_HeightMap, info.m_lodHeightMap);
                materialBlock.SetVector(ID_HeightMapping, info.m_lodHeightMapping);
                materialBlock.SetVector(ID_SurfaceMapping, info.m_lodSurfaceMapping);
            }
            if (info.m_requireWaterMap) {
                materialBlock.SetTexture(ID_WaterHeightMap, info.m_lodWaterHeightMap);
                materialBlock.SetVector(ID_WaterHeightMapping, info.m_lodWaterHeightMapping);
                materialBlock.SetVector(ID_WaterSurfaceMapping, info.m_lodWaterSurfaceMapping);
            }
            if (!(info.m_rollLocation is null)) {
                info.m_lodMaterialCombined.SetVectorArray(ID_RollLocation, info.m_rollLocation);
                info.m_lodMaterialCombined.SetVectorArray(ID_RollParams, info.m_rollParams);
            }
            if (!(mesh is null)) {
                Bounds bounds = default;
                Vector3 lodMin = info.m_lodMin;
                Vector3 lodMax = info.m_lodMax;
                bounds.SetMinMax(new Vector3(lodMin.x - 100f, lodMin.y - 100f, lodMin.z - 100f),
                                 new Vector3(lodMax.x + 100f, lodMax.y + 100f, lodMax.z + 100f));
                mesh.bounds = bounds;
                info.m_lodMin = EMath.DefaultLodMin;
                info.m_lodMax = EMath.DefaultLodMax;
                m_pmInstance.m_drawCallData.m_lodCalls++;
                m_pmInstance.m_drawCallData.m_batchedCalls += (info.m_lodCount - 1);
                Graphics.DrawMesh(mesh, EMath.matrix4Identity, info.m_lodMaterialCombined, info.m_prefabDataLayer, null, 0, materialBlock);
            }
            info.m_lodCount = 0;
        }

        private bool GetSnappingState() => !Singleton<LoadingManager>.instance.m_currentlyLoading && UsePropSnapping;

        public void CalculateProp(uint propID) {
            if (!GetSnappingState() && (m_flags & (CREATEDFLAG | DELETEDFLAG)) == CREATEDFLAG) {
                if ((m_flags & FIXEDHEIGHTFLAG) == 0) {
                    Vector3 position = Position;
                    position.y = Singleton<TerrainManager>.instance.SampleDetailHeight(position);
                    m_posY = (ushort)EMath.Clamp(EMath.RoundToInt(position.y * 64f), 0, 65535);
                }
                CheckOverlap(propID);
            }
        }

        public bool CheckOverlapWithAnarchy() {
            if (Singleton<LoadingManager>.instance.m_currentlyLoading) {
                return true;
            } else if (UsePropAnarchy) {
                if (Blocked) {
                    Blocked = false;
                    DistrictManager instance = Singleton<DistrictManager>.instance;
                    byte park = instance.GetPark(Position);
                    instance.m_parks.m_buffer[park].m_propCount--;
                }
                return true;
            }
            return false;
        }

        private void CheckOverlap(uint propID) {
            if (Info is PropInfo info && !CheckOverlapWithAnarchy()) {
                ItemClass.CollisionType collisionType = (m_flags & FIXEDHEIGHTFLAG) == 0 ? ItemClass.CollisionType.Terrain : ItemClass.CollisionType.Elevated;
                Vector3 position = Position;
                float y = position.y;
                float maxY = position.y + info.m_generatedInfo.m_size.y * m_scale;
                float range = (!Single) ? 4.5f : 0.3f;
                Quad2 quad = default;
                Vector2 a = VectorUtils.XZ(position);
                quad.a = a + new Vector2(-range, -range);
                quad.b = a + new Vector2(-range, range);
                quad.c = a + new Vector2(range, range);
                quad.d = a + new Vector2(range, -range);
                bool flag = false;
                if (!(info.m_class is null)) {
                    if (Singleton<NetManager>.instance.OverlapQuad(quad, y, maxY, collisionType, info.m_class.m_layer, 0, 0, 0)) flag = true;
                    if (Singleton<BuildingManager>.instance.OverlapQuad(quad, y, maxY, collisionType, info.m_class.m_layer, 0, 0, 0)) flag = true;
                }
                if (flag != Blocked) {
                    Blocked = flag;
                    DistrictManager instance = Singleton<DistrictManager>.instance;
                    byte park = instance.GetPark(position);
                    if (flag) instance.m_parks.m_buffer[park].m_propCount--;
                    else instance.m_parks.m_buffer[park].m_propCount++;
                }
            }
        }

        public void UpdateProp(uint propID) {
            if ((m_flags & CREATEDFLAG) != 0 && Info is PropInfo info && info.m_createRuining) {
                Vector3 position = Position;
                float range = info.m_isDecal ? EMath.Max(info.m_generatedInfo.m_size.x, info.m_generatedInfo.m_size.z) * m_scale * 0.5f + 2.5f : 4.5f;
                TerrainModify.UpdateArea(position.x - range, position.z - range, position.x + range, position.z + range, false, true, false);
            }
        }

        public void TerrainUpdated(uint propID, float minX, float minZ, float maxX, float maxZ) {
            if ((m_flags & (CREATEDFLAG | DELETEDFLAG | BLOCKEDFLAG)) == 1) {
                TerrainUpdated(Info, propID, Position, Angle);
            }
        }

        public static void TerrainUpdated(PropInfo info, uint propID, Vector3 position, float angle) {
            if (!(info is null) && info.m_createRuining) {
                Vector3 a;
                Vector3 b3;
                Vector3 c;
                Vector3 d;
                if (info.m_isDecal) {
                    float scale = m_props.m_buffer[propID].m_scale;
                    Vector3 b = new Vector3(EMath.Cos(angle), 0f, EMath.Sin(angle)) * (info.m_generatedInfo.m_size.x * scale * 0.5f + 2.5f);
                    Vector3 b2 = new Vector3(-EMath.Sin(angle), 0f, EMath.Cos(angle)) * (info.m_generatedInfo.m_size.z * scale * 0.5f + 2.5f);
                    a = position - b - b2;
                    b3 = position - b + b2;
                    c = position + b + b2;
                    d = position + b - b2;
                } else {
                    a = position + new Vector3(-4.5f, 0f, -4.5f);
                    b3 = position + new Vector3(-4.5f, 0f, 4.5f);
                    c = position + new Vector3(4.5f, 0f, 4.5f);
                    d = position + new Vector3(4.5f, 0f, -4.5f);
                }
                TerrainModify.Edges edges = TerrainModify.Edges.All;
                TerrainModify.Heights heights = TerrainModify.Heights.None;
                TerrainModify.Surface surface = TerrainModify.Surface.Ruined;
                TerrainModify.ApplyQuad(a, b3, c, d, edges, heights, surface);
            }
        }

        public void AfterTerrainUpdated(uint propID, float minX, float minZ, float maxX, float maxZ) {
            if ((m_flags & (CREATEDFLAG | DELETEDFLAG | FIXEDHEIGHTFLAG)) == CREATEDFLAG) {
                Vector3 position = Position;
                position.y = Singleton<TerrainManager>.instance.SampleDetailHeight(position);
                ushort posY = (ushort)EMath.Clamp(EMath.RoundToInt(position.y * 64f), 0, 65535);
                if (posY != m_posY) {
                    bool blocked = Blocked;
                    if (!UsePropSnapping || Singleton<LoadingManager>.instance.m_currentlyLoading) m_posY = posY;
                    CheckOverlap(propID);
                    bool blocked2 = Blocked;
                    if (blocked2 != blocked) EPropManager.UpdateProp(propID);
                    else if (!blocked2) Singleton<PropManager>.instance.UpdatePropRenderer(propID, true);
                }
            }
        }

        public bool OverlapQuad(uint propID, Quad2 quad, float minY, float maxY, ItemClass.CollisionType collisionType) {
            const float range = 0.3f;
            if (Hidden) return false;
            Vector2 vector = quad.Min();
            Vector2 vector2 = quad.Max();
            Vector3 position = Position;
            if ((position.x - range > vector2.x || position.x + range < vector.x) || (position.z - range > vector2.y || position.z + range < vector.y)) {
                return false;
            }
            PropInfo info = Info;
            float height = info.m_generatedInfo.m_size.y * m_scale;
            ItemClass.CollisionType collisionType2 = ItemClass.CollisionType.Terrain;
            if ((m_flags & FIXEDHEIGHTFLAG) != 0) collisionType2 = ItemClass.CollisionType.Elevated;
            float y = position.y;
            float maxY2 = y + height;
            if (!ItemClass.CheckCollisionType(minY, maxY, y, maxY2, collisionType, collisionType2)) {
                return false;
            }
            Vector2 a = VectorUtils.XZ(position);
            return quad.Intersect(new Quad2 {
                a = a + new Vector2(-height, height),
                b = a + new Vector2(height, height),
                c = a + new Vector2(height, -height),
                d = a + new Vector2(-height, -height)
            });
        }

        public bool RayCast(uint propID, Segment3 ray, out float t, out float targetSqr) {
            t = 2f;
            targetSqr = 0f;
            if (Blocked) return false;
            PropInfo info = Info;
            float scale = m_scale;
            float height = info.m_generatedInfo.m_size.y * scale;
            float maxRadius = EMath.Max(info.m_generatedInfo.m_size.x, info.m_generatedInfo.m_size.z) * scale * 0.5f;
            Vector3 position = Position;
            Bounds bounds = new Bounds(new Vector3(position.x, position.y + height * 0.5f, position.z), new Vector3(maxRadius, height, maxRadius));
            if (!bounds.IntersectRay(new Ray(ray.a, ray.b - ray.a))) return false;
            float radius = (info.m_generatedInfo.m_size.x + info.m_generatedInfo.m_size.z) * scale * 0.125f;
            float minHeight = EMath.Min(radius, height * 0.45f);
            Segment3 segment = new Segment3(position, position);
            segment.a.y += minHeight;
            segment.b.y += (height - minHeight);
            bool result = false;
            float area = ray.DistanceSqr(segment, out float u, out float _);
            if (area < radius * radius) {
                t = u;
                targetSqr = area;
                result = true;
            }
            if (Segment1.Intersect(ray.a.y, ray.b.y, position.y, out u)) {
                radius = maxRadius;
                area = Vector3.SqrMagnitude(ray.Position(u) - position);
                if (area < radius * radius && u < t) {
                    t = u;
                    targetSqr = area;
                    result = true;
                }
            }
            return result;
        }

        public static bool PropRayCast(uint propID, Segment3 ray, out float t, out float targetSqr) {
            t = 2f;
            targetSqr = 0f;
            EPropInstance[] props = m_props.m_buffer;
            if (props[propID].Blocked) return false;
            PropInfo info = props[propID].Info;
            float scale = props[propID].m_scale;
            float height = info.m_generatedInfo.m_size.y * scale;
            float maxRadius = EMath.Max(info.m_generatedInfo.m_size.x, info.m_generatedInfo.m_size.z) * scale * 0.5f;
            Vector3 position = props[propID].Position;
            Bounds bounds = new Bounds(new Vector3(position.x, position.y + height * 0.5f, position.z), new Vector3(maxRadius, height, maxRadius));
            if (!bounds.IntersectRay(new Ray(ray.a, ray.b - ray.a))) return false;
            float radius = (info.m_generatedInfo.m_size.x + info.m_generatedInfo.m_size.z) * scale * 0.125f;
            float minHeight = EMath.Min(radius, height * 0.45f);
            Segment3 segment = new Segment3(position, position);
            segment.a.y += minHeight;
            segment.b.y += (height - minHeight);
            bool result = false;
            float area = ray.DistanceSqr(segment, out float u, out float _);
            if (area < radius * radius) {
                t = u;
                targetSqr = area;
                result = true;
            }
            if (Segment1.Intersect(ray.a.y, ray.b.y, position.y, out u)) {
                radius = maxRadius;
                area = Vector3.SqrMagnitude(ray.Position(u) - position);
                if (area < radius * radius && u < t) {
                    t = u;
                    targetSqr = area;
                    result = true;
                }
            }
            return result;
        }


        public bool CalculateGroupData(uint propID, int layer, ref int vertexCount, ref int triangleCount, ref int objectCount, ref RenderGroup.VertexArrays vertexArrays) {
            if (Blocked) return false;
            PropInfo info = Info;
            return (info.m_prefabDataLayer == layer || info.m_effectLayer == layer) && CalculateGroupData(info, layer, ref vertexCount, ref triangleCount, ref objectCount, ref vertexArrays);
        }

        public static bool CalculateGroupData(PropInfo info, int layer, ref int vertexCount, ref int triangleCount, ref int objectCount, ref RenderGroup.VertexArrays vertexArrays) {
            LightSystem lightSystem = Singleton<RenderManager>.instance.lightSystem;
            if (info.m_prefabDataLayer == layer) return true;
            if (info.m_effectLayer == layer || (info.m_effectLayer == lightSystem.m_lightLayer && layer == lightSystem.m_lightLayerFloating)) {
                bool result = false;
                PropInfo.Effect[] effects = info.m_effects;
                int len = effects.Length;
                for (int i = 0; i < len; i++) {
                    if (effects[i].m_effect.CalculateGroupData(layer, ref vertexCount, ref triangleCount, ref objectCount, ref vertexArrays)) {
                        result = true;
                    }
                }
                return result;
            }
            return false;
        }

        public void PopulateGroupData(uint propID, int layer, ref int vertexIndex, ref int triangleIndex, Vector3 groupPosition, RenderGroup.MeshData data, ref Vector3 min, ref Vector3 max, ref float maxRenderDistance, ref float maxInstanceDistance) {
            if (Blocked) return;
            PropInfo info = Info;
            if (info.m_prefabDataLayer == layer || info.m_effectLayer == layer) {
                InstanceID id = default;
                id.SetProp32(propID);
                PopulateGroupData(info, layer, id, Position, m_scale, Angle, m_color, ref vertexIndex, ref triangleIndex, groupPosition, data, ref min, ref max, ref maxRenderDistance, ref maxInstanceDistance);
            }
        }

        public static void PopulateGroupData(PropInfo info, int layer, InstanceID id, Vector3 position, float scale, float angle, Color color, ref int vertexIndex, ref int triangleIndex, Vector3 groupPosition, RenderGroup.MeshData data, ref Vector3 min, ref Vector3 max, ref float maxRenderDistance, ref float maxInstanceDistance) {
            LightSystem lightSystem = Singleton<RenderManager>.instance.lightSystem;
            if (info.m_prefabDataLayer == layer) {
                float y = info.m_generatedInfo.m_size.y * scale;
                float maxRadius = EMath.Max(info.m_generatedInfo.m_size.x, info.m_generatedInfo.m_size.z) * scale * 0.5f;
                min = EMath.Min(min, position - new Vector3(maxRadius, 0f, maxRadius));
                max = EMath.Max(max, position + new Vector3(maxRadius, y, maxRadius));
                maxRenderDistance = EMath.Max(maxRenderDistance, info.m_maxRenderDistance);
                maxInstanceDistance = EMath.Max(maxInstanceDistance, info.m_maxRenderDistance);
            } else if (info.m_effectLayer == layer || (info.m_effectLayer == lightSystem.m_lightLayer && layer == lightSystem.m_lightLayerFloating)) {
                PropInfo.Effect[] effects = info.m_effects;
                int len = effects.Length;
                Matrix4x4 matrix4x = Matrix4x4.TRS(position, Quaternion.AngleAxis(angle * 57.29578f, EMath.Vector3Down), new Vector3(scale, scale, scale));
                for (int i = 0; i < len; i++) {
                    effects[i].m_effect.PopulateGroupData(layer, id, matrix4x.MultiplyPoint(effects[i].m_position),
                        matrix4x.MultiplyVector(effects[i].m_direction), ref vertexIndex, ref triangleIndex, groupPosition, data, ref min, ref max, ref maxRenderDistance, ref maxInstanceDistance);
                }
            }
        }
    }
}