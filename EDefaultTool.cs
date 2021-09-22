using ColossalFramework;
using ColossalFramework.Math;
using UnityEngine;
using EManagersLib.API;

namespace EManagersLib {
    public static class EDefaultTool {
        private static readonly Vector2 m_range_minXZ = new Vector2(-0.5f, -0.5f);
        private static readonly Vector2 m_range_minXmaxZ = new Vector2(-0.5f, 0.5f);
        private static readonly Vector2 m_range_maxXZ = new Vector2(0.5f, 0.5f);
        private static readonly Vector2 m_range_maxXminZ = new Vector2(0.5f, -0.5f);
        public static bool CheckProp(ToolController toolController, uint prop) {
            if ((toolController.m_mode & ItemClass.Availability.Editors) != ItemClass.Availability.None) return true;
            Vector2 a = VectorUtils.XZ(EPropManager.m_props.m_buffer[prop].Position);
            Quad2 quad = default;
            quad.a = a + m_range_minXZ;
            quad.b = a + m_range_minXmaxZ;
            quad.c = a + m_range_maxXZ;
            quad.d = a + m_range_maxXminZ;
            return !Singleton<GameAreaManager>.instance.QuadOutOfArea(quad);
        }

        public static void RenderPropGeometry(RenderManager.CameraInfo cameraInfo, ref InstanceID hoverInstance, ref Vector3 mousePosition, float angle) {
            uint propID = InstanceIDExtension.GetProp32ByRef(ref hoverInstance);
            EPropInstance[] props = EPropManager.m_props.m_buffer;
            PropInfo info = props[propID].Info;
            if (!(info is null)) {
                float scale = props[propID].m_scale;
                Color color = props[propID].m_color;
                if (info.m_requireHeightMap) {
                    Singleton<TerrainManager>.instance.GetHeightMapping(mousePosition, out Texture heightMap, out Vector4 heightMapping, out Vector4 surfaceMapping);
                    EPropInstance.RenderInstance(cameraInfo, info, hoverInstance, mousePosition, scale, angle * 0.0174532924f, color, RenderManager.DefaultColorLocation, true, heightMap, heightMapping, surfaceMapping);
                } else {
                    EPropInstance.RenderInstance(cameraInfo, info, hoverInstance, mousePosition, scale, angle * 0.0174532924f, color, RenderManager.DefaultColorLocation, true);
                }
            }
        }

        private static Color GetToolColor(ToolController toolController, bool warning, bool error) {
            if (Singleton<InfoManager>.instance.CurrentMode != InfoManager.InfoMode.None) {
                if (error) return toolController.m_errorColorInfo;
                if (warning) return toolController.m_warningColorInfo;
                return toolController.m_validColorInfo;
            } else {
                if (error) return toolController.m_errorColor;
                if (warning) return toolController.m_warningColor;
                return toolController.m_validColor;
            }
        }

        public static void RenderPropOverlay(RenderManager.CameraInfo cameraInfo, ToolController toolController, ref InstanceID hoverInstance, ref Vector3 mousePosition, float angle, ToolBase.ToolErrors selectErrors) {
            uint propID = InstanceIDExtension.GetProp32ByRef(ref hoverInstance);
            EPropInstance[] props = EPropManager.m_props.m_buffer;
            PropInfo info = props[propID].Info;
            if (!(info is null)) {
                Color toolColor = GetToolColor(toolController, false, selectErrors != ToolBase.ToolErrors.None);
                toolController.RenderColliding(cameraInfo, toolColor, toolColor, toolColor, toolColor, 0, 0);
                PropTool.RenderOverlay(cameraInfo, info, mousePosition, props[propID].m_scale, angle, toolColor);
            }
        }

        public static void RenderPropTypeOverlay(RenderManager.CameraInfo cameraInfo, ToolController toolController, ref InstanceID hoverInstance, ToolBase.ToolErrors selectErrors) {
            float range = 1f;
            uint propID = InstanceIDExtension.GetProp32ByRef(ref hoverInstance);
            EPropInstance[] props = EPropManager.m_props.m_buffer;
            PropInfo info = props[propID].Info;
            Color toolColor = GetToolColor(toolController, false, selectErrors != ToolBase.ToolErrors.None);
            PropTool.CheckOverlayAlpha(info, props[propID].m_scale, ref range);
            toolColor *= range;
            PropTool.RenderOverlay(cameraInfo, info, props[propID].Position, props[propID].m_scale, props[propID].Angle, toolColor);
        }

        public static void StartMovingRotating(ref float angle, uint propID) {
            EPropInstance[] props = EPropManager.m_props.m_buffer;
            angle = props[propID].Angle * 57.29578f;
            props[propID].Hidden = true;
        }
    }
}
