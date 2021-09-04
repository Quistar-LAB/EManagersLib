using ColossalFramework;
using ColossalFramework.Math;
using System;
using UnityEngine;

namespace EManagersLib {
    public static class EPropInstance {
        public const ushort PROP_CREATED_FLAG = 0x0001;
        public const ushort PROP_CREATED_MASK = 0xfffe;
        public const ushort PROP_DELETED_FLAG = 0x0002;
        public const ushort PROP_DELETED_MASK = 0xfffd;
        public const ushort PROP_HIDDEN_FLAG = 0x0004;
        public const ushort PROP_HIDDEN_MASK = 0xfffb;
        public const ushort PROP_SINGLE_FLAG = 0x0010;
        public const ushort PROP_SINGLE_MASK = 0xffef;
        public const ushort PROP_FIXEDHEIGHT_FLAG = 0x0020;
        public const ushort PROP_FIXEDHEIGHT_MASK = 0xffdf;
        public const ushort PROP_BLOCKED_FLAG = 0x0040;
        public const ushort PROP_BLOCKED_MASK = 0xffbf;

        [Flags]
        public enum Flags {
            None = 0x0000,
            Created = PROP_CREATED_FLAG,
            Deleted = PROP_DELETED_FLAG,
            Hidden = PROP_HIDDEN_FLAG,
            Single = PROP_SINGLE_FLAG,
            FixedHeight = PROP_FIXEDHEIGHT_FLAG,
            Blocked = PROP_BLOCKED_FLAG,
            All = 0xffff
        }

        public static void RenderInstance(this PropInstance instance, RenderManager.CameraInfo cameraInfo, uint propID, int layerMask) {
            if ((instance.m_flags & (PROP_HIDDEN_FLAG | PROP_BLOCKED_FLAG)) != 0) return;
            PropInfo info = instance.Info;
            Vector3 position = instance.Position;
            TerrainManager terrain = Singleton<TerrainManager>.instance;
            if (info.m_requireWaterMap) position.y = terrain.SampleRawHeightWithWater(position, false, 0f);
            if (!cameraInfo.CheckRenderDistance(position, info.m_maxRenderDistance)) return;
            if (!cameraInfo.Intersect(position, info.m_generatedInfo.m_size.y * info.m_maxScale)) return;

            float angle = instance.Angle;
            Randomizer randomizer = new Randomizer(propID);
            float scale = info.m_minScale + randomizer.Int32(10000u) * (info.m_maxScale - info.m_minScale) * 0.0001f;
            Color color = info.GetColor(ref randomizer);
            Vector4 defaultColorLocation = RenderManager.DefaultColorLocation;
            EInstanceID id = default;
            id.Prop = propID;
            if (info.m_requireWaterMap) {
                terrain.GetHeightMapping(position, out Texture heightMap, out Vector4 heightMapping, out Vector4 surfaceMapping);
                terrain.GetWaterMapping(position, out Texture waterHeightMap, out Vector4 waterHeightMapping, out Vector4 waterSurfaceMapping);
                RenderInstance(cameraInfo, info, id, position, scale, angle, color, defaultColorLocation, true, heightMap, heightMapping, surfaceMapping, waterHeightMap, waterHeightMapping, waterSurfaceMapping);
            } else if (info.m_requireHeightMap) {
                terrain.GetHeightMapping(position, out Texture heightMap, out Vector4 heightMapping, out Vector4 surfaceMapping);
                RenderInstance(cameraInfo, info, id, position, scale, angle, color, defaultColorLocation, true, heightMap, heightMapping, surfaceMapping);
            } else {
                RenderInstance(cameraInfo, info, id, position, scale, angle, color, defaultColorLocation, true);
            }
        }

        public static void RenderInstance(RenderManager.CameraInfo cameraInfo, PropInfo info, EInstanceID id, Vector3 position, float scale, float angle, Color color, Vector4 objectIndex, bool active) {
            if (info.m_prefabInitialized) {
                if (info.m_hasEffects && (active || info.m_alwaysActive)) {
                    Matrix4x4 matrix4x = default;
                    matrix4x.SetTRS(position, Quaternion.AngleAxis(angle * 57.29578f, Vector3.down), new Vector3(scale, scale, scale));
                    float simulationTimeDelta = Singleton<SimulationManager>.instance.m_simulationTimeDelta;
                    for (int i = 0; i < info.m_effects.Length; i++) {
                        Vector3 effPosition = matrix4x.MultiplyPoint(info.m_effects[i].m_position);
                        Vector3 direction = matrix4x.MultiplyVector(info.m_effects[i].m_direction);
                        EffectInfo.SpawnArea area = new EffectInfo.SpawnArea(effPosition, direction, 0f);
                        info.m_effects[i].m_effect.RenderEffect(id.OriginalID, area, Vector3.zero, 0f, 1f, -1f, simulationTimeDelta, cameraInfo);
                    }
                }
                if (info.m_hasRenderer && (cameraInfo.m_layerMask & 1 << info.m_prefabDataLayer) != 0) {
                    if (Singleton<InfoManager>.instance.CurrentMode == InfoManager.InfoMode.None) {
                        if (!active && !info.m_alwaysActive) {
                            objectIndex.z = 0f;
                        } else if (info.m_illuminationOffRange.x < 1000f || info.m_illuminationBlinkType != LightEffect.BlinkType.None) {
                            LightSystem lightSystem = Singleton<RenderManager>.instance.lightSystem;
                            Randomizer randomizer = new Randomizer(id.Index);
                            float num = info.m_illuminationOffRange.x + randomizer.Int32(100000u) * 1E-05f * (info.m_illuminationOffRange.y - info.m_illuminationOffRange.x);
                            objectIndex.z = MathUtils.SmoothStep(num + 0.01f, num - 0.01f, lightSystem.DayLightIntensity);
                            if (info.m_illuminationBlinkType != LightEffect.BlinkType.None) {
                                Vector4 blinkVector = LightEffect.GetBlinkVector(info.m_illuminationBlinkType);
                                float num2 = num * 3.71f + Singleton<SimulationManager>.instance.m_simulationTimer / blinkVector.w;
                                num2 = (num2 - Mathf.Floor(num2)) * blinkVector.w;
                                float num3 = MathUtils.SmoothStep(blinkVector.x, blinkVector.y, num2);
                                float num4 = MathUtils.SmoothStep(blinkVector.w, blinkVector.z, num2);
                                objectIndex.z *= 1f - num3 * num4;
                            }
                        } else {
                            objectIndex.z = 1f;
                        }
                    }
                    if (cameraInfo == null || cameraInfo.CheckRenderDistance(position, info.m_lodRenderDistance)) {
                        Matrix4x4 matrix = default;
                        matrix.SetTRS(position, Quaternion.AngleAxis(angle * 57.29578f, Vector3.down), new Vector3(scale, scale, scale));
                        PropManager instance = Singleton<PropManager>.instance;
                        MaterialPropertyBlock materialBlock = instance.m_materialBlock;
                        materialBlock.Clear();
                        materialBlock.SetColor(instance.ID_Color, color);
                        materialBlock.SetVector(instance.ID_ObjectIndex, objectIndex);
                        if (info.m_rollLocation != null) {
                            info.m_material.SetVectorArray(instance.ID_RollLocation, info.m_rollLocation);
                            info.m_material.SetVectorArray(instance.ID_RollParams, info.m_rollParams);
                        }
                        instance.m_drawCallData.m_defaultCalls++;
                        Graphics.DrawMesh(info.m_mesh, matrix, info.m_material, info.m_prefabDataLayer, null, 0, materialBlock);
                    } else if (info.m_lodMaterialCombined == null) {
                        Matrix4x4 matrix = default;
                        matrix.SetTRS(position, Quaternion.AngleAxis(angle * 57.29578f, Vector3.down), new Vector3(scale, scale, scale));
                        PropManager instance = Singleton<PropManager>.instance;
                        MaterialPropertyBlock materialBlock2 = instance.m_materialBlock;
                        materialBlock2.Clear();
                        materialBlock2.SetColor(instance.ID_Color, color);
                        materialBlock2.SetVector(instance.ID_ObjectIndex, objectIndex);
                        if (info.m_rollLocation != null) {
                            info.m_material.SetVectorArray(instance.ID_RollLocation, info.m_rollLocation);
                            info.m_material.SetVectorArray(instance.ID_RollParams, info.m_rollParams);
                        }
                        instance.m_drawCallData.m_defaultCalls++;
                        Graphics.DrawMesh(info.m_lodMesh, matrix, info.m_lodMaterial, info.m_prefabDataLayer, null, 0, materialBlock2);
                    } else {
                        objectIndex.w = scale;
                        info.m_lodLocations[info.m_lodCount] = new Vector4(position.x, position.y, position.z, angle);
                        info.m_lodObjectIndices[info.m_lodCount] = objectIndex;
                        info.m_lodColors[info.m_lodCount] = color.linear;
                        info.m_lodMin = Vector3.Min(info.m_lodMin, position);
                        info.m_lodMax = Vector3.Max(info.m_lodMax, position);
                        if (++info.m_lodCount == info.m_lodLocations.Length) {
                            PropInstance.RenderLod(cameraInfo, info);
                        }
                    }
                }
            }
        }

        public static void RenderInstance(RenderManager.CameraInfo cameraInfo, PropInfo info, EInstanceID id, Matrix4x4 matrix, Vector3 position, float scale, float angle, Color color, Vector4 objectIndex, bool active) {
            if (info.m_prefabInitialized) {
                if (info.m_hasEffects && (active || info.m_alwaysActive)) {
                    float simulationTimeDelta = Singleton<SimulationManager>.instance.m_simulationTimeDelta;
                    for (int i = 0; i < info.m_effects.Length; i++) {
                        Vector3 position2 = matrix.MultiplyPoint(info.m_effects[i].m_position);
                        Vector3 direction = matrix.MultiplyVector(info.m_effects[i].m_direction);
                        EffectInfo.SpawnArea area = new EffectInfo.SpawnArea(position2, direction, 0f);
                        info.m_effects[i].m_effect.RenderEffect(id.OriginalID, area, Vector3.zero, 0f, 1f, -1f, simulationTimeDelta, cameraInfo);
                    }
                }
                if (info.m_hasRenderer && (cameraInfo.m_layerMask & 1 << info.m_prefabDataLayer) != 0) {
                    if (Singleton<InfoManager>.instance.CurrentMode == InfoManager.InfoMode.None) {
                        if (!active && !info.m_alwaysActive) {
                            objectIndex.z = 0f;
                        } else if (info.m_illuminationOffRange.x < 1000f || info.m_illuminationBlinkType != LightEffect.BlinkType.None) {
                            LightSystem lightSystem = Singleton<RenderManager>.instance.lightSystem;
                            Randomizer randomizer = new Randomizer(id.Index);
                            float num = info.m_illuminationOffRange.x + (float)randomizer.Int32(100000u) * 1E-05f * (info.m_illuminationOffRange.y - info.m_illuminationOffRange.x);
                            objectIndex.z = MathUtils.SmoothStep(num + 0.01f, num - 0.01f, lightSystem.DayLightIntensity);
                            if (info.m_illuminationBlinkType != LightEffect.BlinkType.None) {
                                Vector4 blinkVector = LightEffect.GetBlinkVector(info.m_illuminationBlinkType);
                                float num2 = num * 3.71f + Singleton<SimulationManager>.instance.m_simulationTimer / blinkVector.w;
                                num2 = (num2 - Mathf.Floor(num2)) * blinkVector.w;
                                float num3 = MathUtils.SmoothStep(blinkVector.x, blinkVector.y, num2);
                                float num4 = MathUtils.SmoothStep(blinkVector.w, blinkVector.z, num2);
                                objectIndex.z *= 1f - num3 * num4;
                            }
                        } else {
                            objectIndex.z = 1f;
                        }
                    }
                    if (cameraInfo == null || cameraInfo.CheckRenderDistance(position, info.m_lodRenderDistance)) {
                        PropManager instance = Singleton<PropManager>.instance;
                        MaterialPropertyBlock materialBlock = instance.m_materialBlock;
                        materialBlock.Clear();
                        materialBlock.SetColor(instance.ID_Color, color);
                        materialBlock.SetVector(instance.ID_ObjectIndex, objectIndex);
                        if (info.m_rollLocation != null) {
                            info.m_material.SetVectorArray(instance.ID_RollLocation, info.m_rollLocation);
                            info.m_material.SetVectorArray(instance.ID_RollParams, info.m_rollParams);
                        }
                        instance.m_drawCallData.m_defaultCalls++;
                        Graphics.DrawMesh(info.m_mesh, matrix, info.m_material, info.m_prefabDataLayer, null, 0, materialBlock);
                    } else if (info.m_lodMaterialCombined == null) {
                        PropManager instance = Singleton<PropManager>.instance;
                        MaterialPropertyBlock materialBlock2 = instance.m_materialBlock;
                        materialBlock2.Clear();
                        materialBlock2.SetColor(instance.ID_Color, color);
                        materialBlock2.SetVector(instance.ID_ObjectIndex, objectIndex);
                        if (info.m_rollLocation != null) {
                            info.m_material.SetVectorArray(instance.ID_RollLocation, info.m_rollLocation);
                            info.m_material.SetVectorArray(instance.ID_RollParams, info.m_rollParams);
                        }
                        instance.m_drawCallData.m_defaultCalls++;
                        Graphics.DrawMesh(info.m_lodMesh, matrix, info.m_lodMaterial, info.m_prefabDataLayer, null, 0, materialBlock2);
                    } else {
                        objectIndex.w = scale;
                        info.m_lodLocations[info.m_lodCount] = new Vector4(position.x, position.y, position.z, angle);
                        info.m_lodObjectIndices[info.m_lodCount] = objectIndex;
                        info.m_lodColors[info.m_lodCount] = color.linear;
                        info.m_lodMin = Vector3.Min(info.m_lodMin, position);
                        info.m_lodMax = Vector3.Max(info.m_lodMax, position);
                        if (++info.m_lodCount == info.m_lodLocations.Length) {
                            PropInstance.RenderLod(cameraInfo, info);
                        }
                    }
                }
            }
        }

        public static void RenderInstance(RenderManager.CameraInfo cameraInfo, PropInfo info, EInstanceID id, Vector3 position, float scale, float angle, Color color, Vector4 objectIndex, bool active, Texture heightMap, Vector4 heightMapping, Vector4 surfaceMapping) {
            if (info.m_prefabInitialized) {
                if (info.m_hasEffects && (active || info.m_alwaysActive)) {
                    Matrix4x4 matrix4x = default;
                    matrix4x.SetTRS(position, Quaternion.AngleAxis(angle * 57.29578f, Vector3.down), new Vector3(scale, scale, scale));
                    float simulationTimeDelta = Singleton<SimulationManager>.instance.m_simulationTimeDelta;
                    for (int i = 0; i < info.m_effects.Length; i++) {
                        Vector3 position2 = matrix4x.MultiplyPoint(info.m_effects[i].m_position);
                        Vector3 direction = matrix4x.MultiplyVector(info.m_effects[i].m_direction);
                        EffectInfo.SpawnArea area = new EffectInfo.SpawnArea(position2, direction, 0f);
                        info.m_effects[i].m_effect.RenderEffect(id.OriginalID, area, Vector3.zero, 0f, 1f, -1f, simulationTimeDelta, cameraInfo);
                    }
                }
                if (info.m_hasRenderer && (cameraInfo.m_layerMask & 1 << info.m_prefabDataLayer) != 0) {
                    if (Singleton<InfoManager>.instance.CurrentMode == InfoManager.InfoMode.None) {
                        if (!active && !info.m_alwaysActive) {
                            objectIndex.z = 0f;
                        } else if (info.m_illuminationOffRange.x < 1000f || info.m_illuminationBlinkType != LightEffect.BlinkType.None) {
                            LightSystem lightSystem = Singleton<RenderManager>.instance.lightSystem;
                            Randomizer randomizer = new Randomizer(id.Index);
                            float num = info.m_illuminationOffRange.x + randomizer.Int32(100000u) * 1E-05f * (info.m_illuminationOffRange.y - info.m_illuminationOffRange.x);
                            objectIndex.z = MathUtils.SmoothStep(num + 0.01f, num - 0.01f, lightSystem.DayLightIntensity);
                            if (info.m_illuminationBlinkType != LightEffect.BlinkType.None) {
                                Vector4 blinkVector = LightEffect.GetBlinkVector(info.m_illuminationBlinkType);
                                float num2 = num * 3.71f + Singleton<SimulationManager>.instance.m_simulationTimer / blinkVector.w;
                                num2 = (num2 - Mathf.Floor(num2)) * blinkVector.w;
                                float num3 = MathUtils.SmoothStep(blinkVector.x, blinkVector.y, num2);
                                float num4 = MathUtils.SmoothStep(blinkVector.w, blinkVector.z, num2);
                                objectIndex.z *= 1f - num3 * num4;
                            }
                        } else {
                            objectIndex.z = 1f;
                        }
                    }
                    if (cameraInfo == null || cameraInfo.CheckRenderDistance(position, info.m_lodRenderDistance)) {
                        Matrix4x4 matrix = default;
                        matrix.SetTRS(position, Quaternion.AngleAxis(angle * 57.29578f, Vector3.down), new Vector3(scale, scale, scale));
                        PropManager instance = Singleton<PropManager>.instance;
                        MaterialPropertyBlock materialBlock = instance.m_materialBlock;
                        materialBlock.Clear();
                        materialBlock.SetColor(instance.ID_Color, color);
                        materialBlock.SetTexture(instance.ID_HeightMap, heightMap);
                        materialBlock.SetVector(instance.ID_HeightMapping, heightMapping);
                        materialBlock.SetVector(instance.ID_SurfaceMapping, surfaceMapping);
                        materialBlock.SetVector(instance.ID_ObjectIndex, objectIndex);
                        if (info.m_rollLocation != null) {
                            info.m_material.SetVectorArray(instance.ID_RollLocation, info.m_rollLocation);
                            info.m_material.SetVectorArray(instance.ID_RollParams, info.m_rollParams);
                        }
                        instance.m_drawCallData.m_defaultCalls++;
                        Graphics.DrawMesh(info.m_mesh, matrix, info.m_material, info.m_prefabDataLayer, null, 0, materialBlock);
                    } else {
                        if (heightMap != info.m_lodHeightMap) {
                            if (info.m_lodCount != 0) {
                                PropInstance.RenderLod(cameraInfo, info);
                            }
                            info.m_lodHeightMap = heightMap;
                            info.m_lodHeightMapping = heightMapping;
                            info.m_lodSurfaceMapping = surfaceMapping;
                        }
                        objectIndex.w = scale;
                        info.m_lodLocations[info.m_lodCount] = new Vector4(position.x, position.y, position.z, angle);
                        info.m_lodObjectIndices[info.m_lodCount] = objectIndex;
                        info.m_lodColors[info.m_lodCount] = color.linear;
                        info.m_lodMin = Vector3.Min(info.m_lodMin, position);
                        info.m_lodMax = Vector3.Max(info.m_lodMax, position);
                        if (++info.m_lodCount == info.m_lodLocations.Length) {
                            PropInstance.RenderLod(cameraInfo, info);
                        }
                    }
                }
            }
        }

        public static void RenderInstance(RenderManager.CameraInfo cameraInfo, PropInfo info, EInstanceID id, Vector3 position, float scale, float angle, Color color, Vector4 objectIndex, bool active, Texture heightMap, Vector4 heightMapping, Vector4 surfaceMapping, Texture waterHeightMap, Vector4 waterHeightMapping, Vector4 waterSurfaceMapping) {
            if (info.m_prefabInitialized) {
                if (info.m_hasEffects && (active || info.m_alwaysActive)) {
                    Matrix4x4 matrix4x = default;
                    matrix4x.SetTRS(position, Quaternion.AngleAxis(angle * 57.29578f, Vector3.down), new Vector3(scale, scale, scale));
                    float simulationTimeDelta = Singleton<SimulationManager>.instance.m_simulationTimeDelta;
                    for (int i = 0; i < info.m_effects.Length; i++) {
                        Vector3 position2 = matrix4x.MultiplyPoint(info.m_effects[i].m_position);
                        Vector3 direction = matrix4x.MultiplyVector(info.m_effects[i].m_direction);
                        EffectInfo.SpawnArea area = new EffectInfo.SpawnArea(position2, direction, 0f);
                        info.m_effects[i].m_effect.RenderEffect(id.OriginalID, area, Vector3.zero, 0f, 1f, -1f, simulationTimeDelta, cameraInfo);
                    }
                }
                if (info.m_hasRenderer && (cameraInfo.m_layerMask & 1 << info.m_prefabDataLayer) != 0) {
                    if (Singleton<InfoManager>.instance.CurrentMode == InfoManager.InfoMode.None) {
                        if (!active && !info.m_alwaysActive) {
                            objectIndex.z = 0f;
                        } else if (info.m_illuminationOffRange.x < 1000f || info.m_illuminationBlinkType != LightEffect.BlinkType.None) {
                            LightSystem lightSystem = Singleton<RenderManager>.instance.lightSystem;
                            Randomizer randomizer = new Randomizer(id.Index);
                            float num = info.m_illuminationOffRange.x + (float)randomizer.Int32(100000u) * 1E-05f * (info.m_illuminationOffRange.y - info.m_illuminationOffRange.x);
                            objectIndex.z = MathUtils.SmoothStep(num + 0.01f, num - 0.01f, lightSystem.DayLightIntensity);
                            if (info.m_illuminationBlinkType != LightEffect.BlinkType.None) {
                                Vector4 blinkVector = LightEffect.GetBlinkVector(info.m_illuminationBlinkType);
                                float num2 = num * 3.71f + Singleton<SimulationManager>.instance.m_simulationTimer / blinkVector.w;
                                num2 = (num2 - Mathf.Floor(num2)) * blinkVector.w;
                                float num3 = MathUtils.SmoothStep(blinkVector.x, blinkVector.y, num2);
                                float num4 = MathUtils.SmoothStep(blinkVector.w, blinkVector.z, num2);
                                objectIndex.z *= 1f - num3 * num4;
                            }
                        } else {
                            objectIndex.z = 1f;
                        }
                    }
                    if (cameraInfo == null || cameraInfo.CheckRenderDistance(position, info.m_lodRenderDistance)) {
                        Matrix4x4 matrix = default;
                        matrix.SetTRS(position, Quaternion.AngleAxis(angle * 57.29578f, Vector3.down), new Vector3(scale, scale, scale));
                        PropManager instance = Singleton<PropManager>.instance;
                        MaterialPropertyBlock materialBlock = instance.m_materialBlock;
                        materialBlock.Clear();
                        materialBlock.SetColor(instance.ID_Color, color);
                        materialBlock.SetTexture(instance.ID_HeightMap, heightMap);
                        materialBlock.SetVector(instance.ID_HeightMapping, heightMapping);
                        materialBlock.SetVector(instance.ID_SurfaceMapping, surfaceMapping);
                        materialBlock.SetVector(instance.ID_ObjectIndex, objectIndex);
                        materialBlock.SetTexture(instance.ID_WaterHeightMap, waterHeightMap);
                        materialBlock.SetVector(instance.ID_WaterHeightMapping, waterHeightMapping);
                        materialBlock.SetVector(instance.ID_WaterSurfaceMapping, waterSurfaceMapping);
                        if (info.m_rollLocation != null) {
                            info.m_material.SetVectorArray(instance.ID_RollLocation, info.m_rollLocation);
                            info.m_material.SetVectorArray(instance.ID_RollParams, info.m_rollParams);
                        }
                        instance.m_drawCallData.m_defaultCalls++;
                        Graphics.DrawMesh(info.m_mesh, matrix, info.m_material, info.m_prefabDataLayer, null, 0, materialBlock);
                    } else {
                        if (heightMap != info.m_lodHeightMap || waterHeightMap != info.m_lodWaterHeightMap) {
                            if (info.m_lodCount != 0) {
                                PropInstance.RenderLod(cameraInfo, info);
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
                        info.m_lodMin = Vector3.Min(info.m_lodMin, position);
                        info.m_lodMax = Vector3.Max(info.m_lodMax, position);
                        if (++info.m_lodCount == info.m_lodLocations.Length) {
                            RenderLod(cameraInfo, info);
                        }
                    }
                }
            }
        }

        public static void RenderLod(RenderManager.CameraInfo cameraInfo, PropInfo info) {
            PropManager instance = Singleton<PropManager>.instance;
            MaterialPropertyBlock materialBlock = instance.m_materialBlock;
            materialBlock.Clear();
            Mesh mesh;
            int num;
            if (info.m_lodCount <= 1) {
                mesh = info.m_lodMeshCombined1;
                num = 1;
            } else if (info.m_lodCount <= 4) {
                mesh = info.m_lodMeshCombined4;
                num = 4;
            } else if (info.m_lodCount <= 8) {
                mesh = info.m_lodMeshCombined8;
                num = 8;
            } else {
                mesh = info.m_lodMeshCombined16;
                num = 16;
            }
            for (int i = info.m_lodCount; i < num; i++) {
                info.m_lodLocations[i] = cameraInfo.m_forward * -100000f;
                info.m_lodObjectIndices[i] = Vector4.zero;
                info.m_lodColors[i] = new Color(0f, 0f, 0f, 0f);
            }
            materialBlock.SetVectorArray(instance.ID_PropLocation, info.m_lodLocations);
            materialBlock.SetVectorArray(instance.ID_PropObjectIndex, info.m_lodObjectIndices);
            materialBlock.SetVectorArray(instance.ID_PropColor, info.m_lodColors);
            if (info.m_requireHeightMap) {
                materialBlock.SetTexture(instance.ID_HeightMap, info.m_lodHeightMap);
                materialBlock.SetVector(instance.ID_HeightMapping, info.m_lodHeightMapping);
                materialBlock.SetVector(instance.ID_SurfaceMapping, info.m_lodSurfaceMapping);
            }
            if (info.m_requireWaterMap) {
                materialBlock.SetTexture(instance.ID_WaterHeightMap, info.m_lodWaterHeightMap);
                materialBlock.SetVector(instance.ID_WaterHeightMapping, info.m_lodWaterHeightMapping);
                materialBlock.SetVector(instance.ID_WaterSurfaceMapping, info.m_lodWaterSurfaceMapping);
            }
            if (info.m_rollLocation != null) {
                info.m_lodMaterialCombined.SetVectorArray(instance.ID_RollLocation, info.m_rollLocation);
                info.m_lodMaterialCombined.SetVectorArray(instance.ID_RollParams, info.m_rollParams);
            }
            if (mesh != null) {
                Bounds bounds = default;
                bounds.SetMinMax(info.m_lodMin - new Vector3(100f, 100f, 100f), info.m_lodMax + new Vector3(100f, 100f, 100f));
                mesh.bounds = bounds;
                info.m_lodMin = new Vector3(100000f, 100000f, 100000f);
                info.m_lodMax = new Vector3(-100000f, -100000f, -100000f);
                instance.m_drawCallData.m_lodCalls++;
                instance.m_drawCallData.m_batchedCalls += (info.m_lodCount - 1);
                Graphics.DrawMesh(mesh, Matrix4x4.identity, info.m_lodMaterialCombined, info.m_prefabDataLayer, null, 0, materialBlock);
            }
            info.m_lodCount = 0;
        }

        public static void CalculateProp(this PropInstance instance, uint propID) {
            if ((instance.m_flags & (PROP_CREATED_FLAG | PROP_DELETED_FLAG)) != PROP_CREATED_FLAG) return;
            if ((instance.m_flags & PROP_FIXEDHEIGHT_FLAG) == 0) {
                Vector3 position = instance.Position;
                position.y = Singleton<TerrainManager>.instance.SampleDetailHeight(position);
                instance.m_posY = (ushort)Mathf.Clamp(Mathf.RoundToInt(position.y * 64f), 0, 65535);
            }
            CheckOverlap(instance, propID);
        }

        private static void CheckOverlap(this PropInstance instance, uint propID) {
            PropInfo info = instance.Info;
            if (info is null) return;
            ItemClass.CollisionType collisionType;
            if ((instance.m_flags & PROP_FIXEDHEIGHT_FLAG) == 0) {
                collisionType = ItemClass.CollisionType.Terrain;
            } else {
                collisionType = ItemClass.CollisionType.Elevated;
            }
            Randomizer randomizer = new Randomizer(propID);
            float scale = info.m_minScale + randomizer.Int32(10000u) * (info.m_maxScale - info.m_minScale) * 0.0001f;
            float height = info.m_generatedInfo.m_size.y * scale;
            Vector3 position = instance.Position;
            float y = position.y;
            float maxY = position.y + height;
            float num3 = (!instance.Single) ? 4.5f : 0.3f;
            Quad2 quad = default;
            Vector2 a = VectorUtils.XZ(position);
            quad.a = a + new Vector2(-num3, -num3);
            quad.b = a + new Vector2(-num3, num3);
            quad.c = a + new Vector2(num3, num3);
            quad.d = a + new Vector2(num3, -num3);
            bool flag = false;
            if (!(info.m_class is null)) {
                if (Singleton<NetManager>.instance.OverlapQuad(quad, y, maxY, collisionType, info.m_class.m_layer, 0, 0, 0)) {
                    flag = true;
                }
                if (Singleton<BuildingManager>.instance.OverlapQuad(quad, y, maxY, collisionType, info.m_class.m_layer, 0, 0, 0)) {
                    flag = true;
                }
            }
            if (flag != instance.Blocked) {
                instance.Blocked = flag;
                DistrictManager district = Singleton<DistrictManager>.instance;
                byte park = district.GetPark(position);
                if (flag) {
                    district.m_parks.m_buffer[park].m_propCount--;
                } else {
                    district.m_parks.m_buffer[park].m_propCount++;
                }
            }
        }

        public static void UpdateProp(this PropInstance instance, uint propID) {
            if ((instance.m_flags & PROP_CREATED_FLAG) == 0) return;
            PropInfo info = instance.Info;
            if (info is null) return;
            if (info.m_createRuining) {
                Vector3 position = instance.Position;
                float size = 4.5f;
                if (info.m_isDecal) {
                    Randomizer randomizer = new Randomizer(propID);
                    float scale = info.m_minScale + randomizer.Int32(10000u) * (info.m_maxScale - info.m_minScale) * 0.0001f;
                    size = Mathf.Max(info.m_generatedInfo.m_size.x, info.m_generatedInfo.m_size.z) * scale * 0.5f + 2.5f;
                }
                float minX = position.x - size;
                float minZ = position.z - size;
                float maxX = position.x + size;
                float maxZ = position.z + size;
                TerrainModify.UpdateArea(minX, minZ, maxX, maxZ, false, true, false);
            }
        }

        public static void TerrainUpdated(this PropInstance instance, uint propID, float minX, float minZ, float maxX, float maxZ) {
            if ((instance.m_flags & (PROP_CREATED_FLAG | PROP_DELETED_FLAG)) != 1) return;
            if (!instance.Blocked) TerrainUpdated(instance.Info, propID, instance.Position, instance.Angle);
        }

        public static void TerrainUpdated(PropInfo info, uint propID, Vector3 position, float angle) {
            if (info is null) return;
            if (info.m_createRuining) {
                Vector3 a;
                Vector3 b3;
                Vector3 c;
                Vector3 d;
                if (info.m_isDecal) {
                    Randomizer randomizer = new Randomizer(propID);
                    float scale = info.m_minScale + randomizer.Int32(10000u) * (info.m_maxScale - info.m_minScale) * 0.0001f;
                    Vector3 b = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * (info.m_generatedInfo.m_size.x * scale * 0.5f + 2.5f);
                    Vector3 b2 = new Vector3(-Mathf.Sin(angle), 0f, Mathf.Cos(angle)) * (info.m_generatedInfo.m_size.z * scale * 0.5f + 2.5f);
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

        public static void AfterTerrainUpdated(this PropInstance instance, uint propID, float minX, float minZ, float maxX, float maxZ) {
            if ((instance.m_flags & (PROP_CREATED_FLAG | PROP_DELETED_FLAG)) != 1) return;
            if ((instance.m_flags & PROP_FIXEDHEIGHT_FLAG) == 0) {
                Vector3 position = instance.Position;
                position.y = Singleton<TerrainManager>.instance.SampleDetailHeight(position);
                ushort posY = (ushort)Mathf.Clamp(Mathf.RoundToInt(position.y * 64f), 0, 65535);
                if (posY != instance.m_posY) {
                    bool blocked = instance.Blocked;
                    instance.m_posY = posY;
                    CheckOverlap(instance, propID);
                    bool blocked2 = instance.Blocked;
                    if (blocked2 != blocked) {
                        Singleton<PropManager>.instance.UpdateProp(propID);
                    } else if (!blocked2) {
                        Singleton<PropManager>.instance.UpdatePropRenderer(propID, true);
                    }
                }
            }
        }

        public static bool OverlapQuad(this PropInstance instance, uint propID, Quad2 quad, float minY, float maxY, ItemClass.CollisionType collisionType) {
            if (instance.Hidden) return false;
            Vector2 vector = quad.Min();
            Vector2 vector2 = quad.Max();

            float num = 0.3f;
            Vector3 position = instance.Position;
            if (position.x - num > vector2.x || position.x + num < vector.x) {
                return false;
            }
            if (position.z - num > vector2.y || position.z + num < vector.y) {
                return false;
            }
            PropInfo info = instance.Info;
            Randomizer randomizer = new Randomizer(propID);
            float scale = info.m_minScale + randomizer.Int32(10000u) * (info.m_maxScale - info.m_minScale) * 0.0001f;
            float height = info.m_generatedInfo.m_size.y * scale;
            ItemClass.CollisionType collisionType2 = ItemClass.CollisionType.Terrain;
            if ((instance.m_flags & PROP_FIXEDHEIGHT_FLAG) != 0) {
                collisionType2 = ItemClass.CollisionType.Elevated;
            }
            float y = position.y;
            float maxY2 = position.y + height;
            if (!ItemClass.CheckCollisionType(minY, maxY, y, maxY2, collisionType, collisionType2)) {
                return false;
            }
            Vector2 a = VectorUtils.XZ(position);
            return quad.Intersect(new Quad2 {
                a = a + new Vector2(-num, num),
                b = a + new Vector2(num, num),
                c = a + new Vector2(num, -num),
                d = a + new Vector2(-num, -num)
            });
        }

        public static bool RayCast(this PropInstance instance, uint propID, Segment3 ray, out float t, out float targetSqr) {
            t = 2f;
            targetSqr = 0f;
            if (instance.Blocked) return false;
            PropInfo info = instance.Info;
            Randomizer randomizer = new Randomizer(propID);
            float num = info.m_minScale + randomizer.Int32(10000u) * (info.m_maxScale - info.m_minScale) * 0.0001f;
            float num2 = info.m_generatedInfo.m_size.y * num;
            float num3 = Mathf.Max(info.m_generatedInfo.m_size.x, info.m_generatedInfo.m_size.z) * num * 0.5f;
            Vector3 position = instance.Position;
            Bounds bounds = new Bounds(new Vector3(position.x, position.y + num2 * 0.5f, position.z), new Vector3(num3, num2, num3));
            if (!bounds.IntersectRay(new Ray(ray.a, ray.b - ray.a))) {
                return false;
            }
            float num4 = (info.m_generatedInfo.m_size.x + info.m_generatedInfo.m_size.z) * num * 0.125f;
            float num5 = Mathf.Min(num4, num2 * 0.45f);
            Segment3 segment = new Segment3(position, position);
            segment.a.y += num5;
            segment.b.y += (num2 - num5);
            bool result = false;
            float num6 = ray.DistanceSqr(segment, out float num7, out float num8);
            if (num6 < num4 * num4) {
                t = num7;
                targetSqr = num6;
                result = true;
            }
            if (Segment1.Intersect(ray.a.y, ray.b.y, position.y, out num7)) {
                num4 = num3;
                num6 = Vector3.SqrMagnitude(ray.Position(num7) - position);
                if (num6 < num4 * num4 && num7 < t) {
                    t = num7;
                    targetSqr = num6;
                    result = true;
                }
            }
            return result;
        }

        public static bool CalculateGroupData(this PropInstance instance, uint propID, int layer, ref int vertexCount, ref int triangleCount, ref int objectCount, ref RenderGroup.VertexArrays vertexArrays) {
            if (instance.Blocked) return false;
            PropInfo info = instance.Info;
            return (info.m_prefabDataLayer == layer || info.m_effectLayer == layer) && CalculateGroupData(info, layer, ref vertexCount, ref triangleCount, ref objectCount, ref vertexArrays);
        }

        public static bool CalculateGroupData(PropInfo info, int layer, ref int vertexCount, ref int triangleCount, ref int objectCount, ref RenderGroup.VertexArrays vertexArrays) {
            LightSystem lightSystem = Singleton<RenderManager>.instance.lightSystem;
            if (info.m_prefabDataLayer == layer) {
                return true;
            }
            if (info.m_effectLayer == layer || (info.m_effectLayer == lightSystem.m_lightLayer && layer == lightSystem.m_lightLayerFloating)) {
                bool result = false;
                for (int i = 0; i < info.m_effects.Length; i++) {
                    if (info.m_effects[i].m_effect.CalculateGroupData(layer, ref vertexCount, ref triangleCount, ref objectCount, ref vertexArrays)) {
                        result = true;
                    }
                }
                return result;
            }
            return false;
        }

        public static void PopulateGroupData(this PropInstance instance, uint propID, int layer, ref int vertexIndex, ref int triangleIndex, Vector3 groupPosition, RenderGroup.MeshData data, ref Vector3 min, ref Vector3 max, ref float maxRenderDistance, ref float maxInstanceDistance) {
            if (instance.Blocked) return;
            PropInfo info = instance.Info;
            if (info.m_prefabDataLayer == layer || info.m_effectLayer == layer) {
                Vector3 position = instance.Position;
                Randomizer randomizer = new Randomizer(propID);
                float scale = info.m_minScale + randomizer.Int32(10000u) * (info.m_maxScale - info.m_minScale) * 0.0001f;
                float angle = instance.Angle;
                Color color = info.GetColor(ref randomizer);
                PopulateGroupData(info, layer, new EInstanceID {
                    Prop = propID
                }, position, scale, angle, color, ref vertexIndex, ref triangleIndex, groupPosition, data, ref min, ref max, ref maxRenderDistance, ref maxInstanceDistance);
            }
        }

        public static void PopulateGroupData(PropInfo info, int layer, EInstanceID id, Vector3 position, float scale, float angle, Color color, ref int vertexIndex, ref int triangleIndex, Vector3 groupPosition, RenderGroup.MeshData data, ref Vector3 min, ref Vector3 max, ref float maxRenderDistance, ref float maxInstanceDistance) {
            LightSystem lightSystem = Singleton<RenderManager>.instance.lightSystem;
            if (info.m_prefabDataLayer == layer) {
                float y = info.m_generatedInfo.m_size.y * scale;
                float num = Mathf.Max(info.m_generatedInfo.m_size.x, info.m_generatedInfo.m_size.z) * scale * 0.5f;
                min = Vector3.Min(min, position - new Vector3(num, 0f, num));
                max = Vector3.Max(max, position + new Vector3(num, y, num));
                maxRenderDistance = Mathf.Max(maxRenderDistance, info.m_maxRenderDistance);
                maxInstanceDistance = Mathf.Max(maxInstanceDistance, info.m_maxRenderDistance);
            } else if (info.m_effectLayer == layer || (info.m_effectLayer == lightSystem.m_lightLayer && layer == lightSystem.m_lightLayerFloating)) {
                Matrix4x4 matrix4x = default;
                matrix4x.SetTRS(position, Quaternion.AngleAxis(angle * 57.29578f, Vector3.down), new Vector3(scale, scale, scale));
                for (int i = 0; i < info.m_effects.Length; i++) {
                    Vector3 pos = matrix4x.MultiplyPoint(info.m_effects[i].m_position);
                    Vector3 dir = matrix4x.MultiplyVector(info.m_effects[i].m_direction);
                    info.m_effects[i].m_effect.PopulateGroupData(layer, id.OriginalID, pos, dir, ref vertexIndex, ref triangleIndex, groupPosition, data, ref min, ref max, ref maxRenderDistance, ref maxInstanceDistance);
                }
            }
        }
    }
}
