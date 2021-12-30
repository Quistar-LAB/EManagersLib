using ColossalFramework;
using ColossalFramework.Math;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace EManagersLib {
    public static class EPropTool {
        private static Vector2 veca = new Vector2(-0.3f, -0.3f);
        private static Vector2 vecb = new Vector2(-0.3f, 0.3f);
        private static Vector2 vecc = new Vector2(0.3f, 0.3f);
        private static Vector2 vecd = new Vector2(0.3f, -0.3f);

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static ToolBase.ToolErrors CheckPlacementErrors(PropInfo info, Vector3 position, bool fixedHeight, uint id, ulong[] collidingSegmentBuffer, ulong[] collidingBuildingBuffer) {
            if (EPropManager.UsePropAnarchy) return ToolBase.ToolErrors.None;
            float scale = EPropManager.m_props.m_buffer[id].m_scale;
            float height = info.m_generatedInfo.m_size.y * scale;
            Vector2 vector = VectorUtils.XZ(position);
            Quad2 quad = default;
            quad.a = vector + veca;
            quad.b = vector + vecb;
            quad.c = vector + vecc;
            quad.d = vector + vecd;
            float y = position.y;
            float maxY = y + height;
            ItemClass.CollisionType collisionType = ItemClass.CollisionType.Terrain;
            if (fixedHeight) {
                collisionType = ItemClass.CollisionType.Elevated;
            }
            ToolBase.ToolErrors toolErrors = ToolBase.ToolErrors.None;
            if (!Singleton<ToolManager>.instance.m_properties.m_disablePropCollisions) {
                if (EPropManager.OverlapQuad(quad, y, maxY, collisionType, 0, 0)) {
                    toolErrors |= ToolBase.ToolErrors.ObjectCollision;
                }
                if (Singleton<TreeManager>.instance.OverlapQuad(quad, y, maxY, collisionType, 0, 0u)) {
                    toolErrors |= ToolBase.ToolErrors.ObjectCollision;
                }
            }
            if (Singleton<NetManager>.instance.OverlapQuad(quad, y, maxY, collisionType, info.m_class.m_layer, 0, 0, 0, collidingSegmentBuffer)) {
                toolErrors |= ToolBase.ToolErrors.ObjectCollision;
            }
            if ((Singleton<ToolManager>.instance.m_properties.m_mode & ItemClass.Availability.AssetEditor) == ItemClass.Availability.None &&
                Singleton<BuildingManager>.instance.OverlapQuad(quad, y, maxY, collisionType, info.m_class.m_layer, 0, 0, 0, collidingBuildingBuffer)) {
                toolErrors |= ToolBase.ToolErrors.ObjectCollision;
            }
            if (!info.m_requireWaterMap && Singleton<TerrainManager>.instance.HasWater(vector)) {
                toolErrors |= ToolBase.ToolErrors.CannotBuildOnWater;
            }
            if (Singleton<GameAreaManager>.instance.QuadOutOfArea(quad)) {
                toolErrors |= ToolBase.ToolErrors.OutOfArea;
            }
            return toolErrors;
        }
    }
}
