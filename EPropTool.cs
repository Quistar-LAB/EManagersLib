using ColossalFramework;
using ColossalFramework.Math;
using UnityEngine;

namespace EManagersLib {
    public static class EPropTool {
        public static ToolBase.ToolErrors CheckPlacementErrors(PropInfo info, Vector3 position, bool fixedHeight, uint id, ulong[] collidingSegmentBuffer, ulong[] collidingBuildingBuffer) {
            Randomizer randomizer = new Randomizer(id);
            float scale = info.m_minScale + randomizer.Int32(10000u) * (info.m_maxScale - info.m_minScale) * 0.0001f;
            float height = info.m_generatedInfo.m_size.y * scale;
            Vector2 vector = VectorUtils.XZ(position);
            Quad2 quad = default;
            quad.a = vector + new Vector2(-0.3f, -0.3f);
            quad.b = vector + new Vector2(-0.3f, 0.3f);
            quad.c = vector + new Vector2(0.3f, 0.3f);
            quad.d = vector + new Vector2(0.3f, -0.3f);
            float y = position.y;
            float maxY = position.y + height;
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
