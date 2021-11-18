using ColossalFramework;
using ColossalFramework.Math;
using HarmonyLib;
using UnityEngine;

namespace EManagersLib {
    public class ELightEffectPatch {
        public static bool PopulateGroupDataPrefix(LightEffect __instance, int ___m_groupLayer, int ___m_groupLayerFloating,
            float ___m_lightRange, LightType ___m_lightType, ref Quaternion ___m_lightRotation, float ___m_lightSpotAngle,
            float ___m_lightIntensity, ref Color ___m_lightColor,
            int layer, InstanceID id, Vector3 pos, Vector3 dir,
            ref int vertexIndex, ref int triangleIndex, Vector3 groupPosition, RenderGroup.MeshData data,
            ref Vector3 min, ref Vector3 max, ref float maxRenderDistance, ref float maxInstanceDistance) {
            if ((layer == ___m_groupLayer || layer == ___m_groupLayerFloating) && !(data is null)) {
                bool flag = layer == ___m_groupLayerFloating;
                Vector3 b = new Vector3(___m_lightRange, ___m_lightRange, ___m_lightRange);
                Vector3 rhs = pos - b;
                Vector3 rhs2 = pos + b;
                min = EMath.Min(min, rhs);
                max = EMath.Max(max, rhs2);
                Vector3 vector = pos - groupPosition;
                maxRenderDistance = EMath.Max(maxRenderDistance, 30000f);
                Vector3 vector2 = ___m_lightType == LightType.Spot ? Quaternion.LookRotation(dir) * (___m_lightRotation * EMath.Vector3Forward) : EMath.Vector3Forward;
                Vector4 vector3 = new Vector4(vector2.x, vector2.y, vector2.z, 100000f);
                if (___m_lightType == LightType.Spot) {
                    vector3.w = -EMath.Cos(0.0174532924f * ___m_lightSpotAngle * 0.5f);
                }
                Vector2 vector4 = new Vector2(1f / (___m_lightRange * ___m_lightRange), ___m_lightIntensity);
                EMath.SetRandomizerSeed(id.Index);
                Vector2 vector5;
                vector5.x = __instance.m_offRange.x + EMath.randomizer.Int32(100000u) * 1E-05f * (__instance.m_offRange.y - __instance.m_offRange.x);
                vector5.y = (float)__instance.m_blinkType;
                ushort building = EffectInfo.GetBuilding(id);
                Vector2 vector6;
                Vector2 vector7;
                if (building != 0 && building < 49152) { // make sure buildingID is less than 49152
                    vector6 = RenderManager.GetColorLocation(building);
                    if (flag) {
                        Vector3 v = Singleton<BuildingManager>.instance.m_buildings.m_buffer[building].CalculateMeshPosition();
                        vector7 = VectorUtils.XZ(v);
                        pos.y -= v.y;
                        vector.y -= v.y;
                    } else {
                        vector7 = EMath.Vector2Zero;
                    }
                } else {
                    vector6 = RenderManager.DefaultColorLocation;
                    vector7 = EMath.Vector2Zero;
                }
                Color color = ___m_lightColor;
                if (__instance.m_variationColors != null && __instance.m_variationColors.Length != 0) {
                    Color b2 = __instance.m_variationColors[EMath.randomizer.Int32((uint)__instance.m_variationColors.Length)];
                    color = Color.Lerp(color, b2, EMath.randomizer.Int32(1000u) * 0.001f);
                }
                color = color.linear;
                color.a = ((___m_lightType != LightType.Spot) ? 0f : __instance.m_spotLeaking);
                float num = ___m_lightRange / EMath.Cos(0.5235988f);
                float num2 = num * 0.5f;
                data.m_vertices[vertexIndex] = new Vector3(vector.x - num2, vector.y + ___m_lightRange, vector.z + ___m_lightRange);
                data.m_normals[vertexIndex] = pos;
                data.m_tangents[vertexIndex] = vector3;
                data.m_uvs[vertexIndex] = vector4;
                data.m_uvs2[vertexIndex] = vector5;
                data.m_uvs3[vertexIndex] = vector6;
                if (flag) {
                    data.m_uvs4[vertexIndex] = vector7;
                }
                data.m_colors[vertexIndex] = color;
                vertexIndex++;
                data.m_vertices[vertexIndex] = new Vector3(vector.x + num2, vector.y + ___m_lightRange, vector.z + ___m_lightRange);
                data.m_normals[vertexIndex] = pos;
                data.m_tangents[vertexIndex] = vector3;
                data.m_uvs[vertexIndex] = vector4;
                data.m_uvs2[vertexIndex] = vector5;
                data.m_uvs3[vertexIndex] = vector6;
                if (flag) {
                    data.m_uvs4[vertexIndex] = vector7;
                }
                data.m_colors[vertexIndex] = color;
                vertexIndex++;
                data.m_vertices[vertexIndex] = new Vector3(vector.x + num, vector.y, vector.z + ___m_lightRange);
                data.m_normals[vertexIndex] = pos;
                data.m_tangents[vertexIndex] = vector3;
                data.m_uvs[vertexIndex] = vector4;
                data.m_uvs2[vertexIndex] = vector5;
                data.m_uvs3[vertexIndex] = vector6;
                if (flag) {
                    data.m_uvs4[vertexIndex] = vector7;
                }
                data.m_colors[vertexIndex] = color;
                vertexIndex++;
                data.m_vertices[vertexIndex] = new Vector3(vector.x + num2, vector.y - ___m_lightRange, vector.z + ___m_lightRange);
                data.m_normals[vertexIndex] = pos;
                data.m_tangents[vertexIndex] = vector3;
                data.m_uvs[vertexIndex] = vector4;
                data.m_uvs2[vertexIndex] = vector5;
                data.m_uvs3[vertexIndex] = vector6;
                if (flag) {
                    data.m_uvs4[vertexIndex] = vector7;
                }
                data.m_colors[vertexIndex] = color;
                vertexIndex++;
                data.m_vertices[vertexIndex] = new Vector3(vector.x - num2, vector.y - ___m_lightRange, vector.z + ___m_lightRange);
                data.m_normals[vertexIndex] = pos;
                data.m_tangents[vertexIndex] = vector3;
                data.m_uvs[vertexIndex] = vector4;
                data.m_uvs2[vertexIndex] = vector5;
                data.m_uvs3[vertexIndex] = vector6;
                if (flag) {
                    data.m_uvs4[vertexIndex] = vector7;
                }
                data.m_colors[vertexIndex] = color;
                vertexIndex++;
                data.m_vertices[vertexIndex] = new Vector3(vector.x - num, vector.y, vector.z + ___m_lightRange);
                data.m_normals[vertexIndex] = pos;
                data.m_tangents[vertexIndex] = vector3;
                data.m_uvs[vertexIndex] = vector4;
                data.m_uvs2[vertexIndex] = vector5;
                data.m_uvs3[vertexIndex] = vector6;
                if (flag) {
                    data.m_uvs4[vertexIndex] = vector7;
                }
                data.m_colors[vertexIndex] = color;
                vertexIndex++;
                data.m_triangles[triangleIndex++] = vertexIndex - 6;
                data.m_triangles[triangleIndex++] = vertexIndex - 5;
                data.m_triangles[triangleIndex++] = vertexIndex - 1;
                data.m_triangles[triangleIndex++] = vertexIndex - 1;
                data.m_triangles[triangleIndex++] = vertexIndex - 5;
                data.m_triangles[triangleIndex++] = vertexIndex - 4;
                data.m_triangles[triangleIndex++] = vertexIndex - 1;
                data.m_triangles[triangleIndex++] = vertexIndex - 4;
                data.m_triangles[triangleIndex++] = vertexIndex - 2;
                data.m_triangles[triangleIndex++] = vertexIndex - 2;
                data.m_triangles[triangleIndex++] = vertexIndex - 4;
                data.m_triangles[triangleIndex++] = vertexIndex - 3;
            }
            return false;
        }

        internal void Enable(Harmony harmony) {
            harmony.Patch(AccessTools.Method(typeof(LightEffect), nameof(LightEffect.PopulateGroupData)), prefix: new HarmonyMethod(AccessTools.Method(typeof(ELightEffectPatch), nameof(PopulateGroupDataPrefix))));
        }

        internal void Disable(Harmony harmony) {
            harmony.Unpatch(AccessTools.Method(typeof(LightEffect), nameof(LightEffect.PopulateGroupData)), HarmonyPatchType.Prefix, EModule.HARMONYID);
        }
    }
}
