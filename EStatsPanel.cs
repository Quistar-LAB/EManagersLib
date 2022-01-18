using ColossalFramework;
using System.Collections;
using System.Text;
using UnityEngine;

namespace EManagersLib {
    internal static class EStatsPanel {
        private const int statsPanelId = 0x0009fb0a;
        private const int defFontSize = 12;
        private const float panelwidth = 330f;
        private static float panelheight = 250f;
        private const int border = 5;
        private const int spacing = 10;
        private const string m_title = "Cities Internal Limits";
        private static GUISkin skin;
        private static GUIStyle m_nameStyle;
        private static readonly StringBuilder m_stringBuffer = new StringBuilder(500);
        private delegate void GetStatAPI(StringBuilder sb);
        private static GetStatAPI GetStats;
        private static Rect m_windowRect;
        private static Rect m_nameRect;
        private static Rect m_maxSizeRect;
        private static Rect m_limitsRect;
        private static Rect m_statsRect;
        private static string m_names;
        private static string m_maxSizes;
        private static string m_limits;
        private static string m_stats;
        internal static bool m_isVisible = false;

        private static int GetSegmentsLimits(ToolManager toolInstance, NetManager netInstance) {
            int len = netInstance.m_segments.m_buffer.Length;
            switch (toolInstance.m_properties.m_mode & (ItemClass.Availability.MapEditor | ItemClass.Availability.AssetEditor)) {
            case ItemClass.Availability.MapEditor: return NetManager.MAX_MAP_SEGMENTS;
            case ItemClass.Availability.AssetEditor: return NetManager.MAX_ASSET_SEGMENTS;
            }
            return len - 512;
        }
        private static int GetNodesLimits(ToolManager toolInstance, NetManager netInstance) {
            int len = netInstance.m_segments.m_buffer.Length;
            switch (toolInstance.m_properties.m_mode & (ItemClass.Availability.MapEditor | ItemClass.Availability.AssetEditor)) {
            case ItemClass.Availability.MapEditor: return NetManager.MAX_MAP_NODES;
            case ItemClass.Availability.AssetEditor: return NetManager.MAX_ASSET_NODES;
            }
            return len - 500;
        }
        private static int GetLanesLimits(ToolManager toolInstance, NetManager netInstance) {
            int len = netInstance.m_lanes.m_buffer.Length;
            switch (toolInstance.m_properties.m_mode & (ItemClass.Availability.MapEditor | ItemClass.Availability.AssetEditor)) {
            case ItemClass.Availability.MapEditor: return NetManager.MAX_MAP_LANES;
            case ItemClass.Availability.AssetEditor: return NetManager.MAX_ASSET_LANES;
            }
            return len - 4096;
        }
        private static int GetBuildingLimits(ToolManager toolInstance, BuildingManager bmInstance) {
            int len = bmInstance.m_buildings.m_buffer.Length;
            switch (toolInstance.m_properties.m_mode & (ItemClass.Availability.MapEditor | ItemClass.Availability.AssetEditor)) {
            case ItemClass.Availability.MapEditor: return BuildingManager.MAX_MAP_BUILDINGS;
            case ItemClass.Availability.AssetEditor: return BuildingManager.MAX_ASSET_BUILDINGS;
            }
            return len - 512;
        }
        private static int GetZoneLimits(ToolManager toolInstance, ZoneManager zmInstance) {
            switch (toolInstance.m_properties.m_mode & (ItemClass.Availability.MapEditor | ItemClass.Availability.AssetEditor)) {
            case ItemClass.Availability.MapEditor: return ZoneManager.MAX_MAP_BLOCKS;
            case ItemClass.Availability.AssetEditor: return ZoneManager.MAX_ASSET_BLOCKS;
            }
            return zmInstance.m_blocks.m_buffer.Length - 512;
        }
        private static int GetTreeLimits(ToolManager toolInstance, TreeManager tmInstance) {
            int len = tmInstance.m_trees.m_buffer.Length;
            switch (toolInstance.m_properties.m_mode & (ItemClass.Availability.MapEditor | ItemClass.Availability.AssetEditor)) {
            case ItemClass.Availability.MapEditor: return len - (TreeManager.MAX_TREE_COUNT - TreeManager.MAX_MAP_TREES);
            case ItemClass.Availability.AssetEditor: return TreeManager.MAX_ASSET_TREES;
            }
            return len - 5;
        }
        private static int GetPropLimits(ToolManager toolInstance) {
            switch (toolInstance.m_properties.m_mode & (ItemClass.Availability.MapEditor | ItemClass.Availability.AssetEditor)) {
            case ItemClass.Availability.MapEditor: return EPropManager.MAX_MAP_PROPS_LIMIT;
            case ItemClass.Availability.AssetEditor: return PropManager.MAX_ASSET_PROPS;
            }
            return EPropManager.MAX_GAME_PROPS_LIMIT;
        }

        internal static void Initialize() {
            m_windowRect = new Rect(Screen.width - panelwidth - 5f, 120f, panelwidth, panelheight);
            BuildStatsData(out m_names, out m_maxSizes, out m_limits);
        }

        internal static void OnGUI() {
            if (m_isVisible) {
                if (skin is null) {
                    Texture2D bgTexture = new Texture2D(1, 1);
                    bgTexture.SetPixel(0, 0, new Color(0.1f, 0.1f, 0.1f, 0.6f));
                    bgTexture.Apply();

                    skin = ScriptableObject.CreateInstance<GUISkin>();
                    skin.box = new GUIStyle(GUI.skin.box);
                    skin.button = new GUIStyle(GUI.skin.button);
                    skin.horizontalScrollbar = new GUIStyle(GUI.skin.horizontalScrollbar);
                    skin.horizontalScrollbarLeftButton = new GUIStyle(GUI.skin.horizontalScrollbarLeftButton);
                    skin.horizontalScrollbarRightButton = new GUIStyle(GUI.skin.horizontalScrollbarRightButton);
                    skin.horizontalScrollbarThumb = new GUIStyle(GUI.skin.horizontalScrollbarThumb);
                    skin.horizontalSlider = new GUIStyle(GUI.skin.horizontalSlider);
                    skin.horizontalSliderThumb = new GUIStyle(GUI.skin.horizontalSliderThumb);
                    skin.label = new GUIStyle(GUI.skin.label) {
                        fontSize = defFontSize,
                        richText = true
                    };
                    skin.scrollView = new GUIStyle(GUI.skin.scrollView);
                    skin.textArea = new GUIStyle(GUI.skin.textArea);
                    skin.textField = new GUIStyle(GUI.skin.textField);
                    skin.toggle = new GUIStyle(GUI.skin.toggle);
                    skin.verticalScrollbar = new GUIStyle(GUI.skin.verticalScrollbar);
                    skin.verticalScrollbarDownButton = new GUIStyle(GUI.skin.verticalScrollbarDownButton);
                    skin.verticalScrollbarThumb = new GUIStyle(GUI.skin.verticalScrollbarThumb);
                    skin.verticalScrollbarUpButton = new GUIStyle(GUI.skin.verticalScrollbarUpButton);
                    skin.verticalSlider = new GUIStyle(GUI.skin.verticalSlider);
                    skin.verticalSliderThumb = new GUIStyle(GUI.skin.verticalSliderThumb);
                    skin.window = new GUIStyle(GUI.skin.window);
                    skin.window.normal.background = bgTexture;
                    skin.window.onNormal.background = bgTexture;

                    skin.settings.cursorColor = GUI.skin.settings.cursorColor;
                    skin.settings.cursorFlashSpeed = GUI.skin.settings.cursorFlashSpeed;
                    skin.settings.doubleClickSelectsWord = GUI.skin.settings.doubleClickSelectsWord;
                    skin.settings.selectionColor = GUI.skin.settings.selectionColor;
                    skin.settings.tripleClickSelectsLine = GUI.skin.settings.tripleClickSelectsLine;

                    panelheight = skin.label.lineHeight * 16 + (2f * 16 + spacing + border * 2);
                    float height = panelheight - border * 2 - spacing - 10f;
                    m_nameRect = new Rect(border, border + spacing, 100f, height);
                    m_maxSizeRect = new Rect(border + m_nameRect.width + spacing, border + spacing, 60f, height);
                    m_limitsRect = new Rect(border + m_maxSizeRect.x + m_maxSizeRect.width, border + spacing, 55f, height);
                    m_statsRect = new Rect(border + m_limitsRect.x + m_limitsRect.width, border + spacing, 90f, height);
                    m_nameStyle = new GUIStyle(GUI.skin.label) {
                        fontSize = defFontSize,
                        richText = true,
                        alignment = TextAnchor.UpperRight
                    };
                }
                GUI.skin = skin;
                m_windowRect = new Rect(Screen.width - panelwidth - 5f, 120f, panelwidth, panelheight);
                m_windowRect = GUI.Window(statsPanelId, m_windowRect, OutputStats, m_title);
            }
        }

        private static void BuildStatsData(out string names, out string maxSize, out string limits) {
            string GetLimitColor(int count, int limit) {
                if (count >= limit) return @"[<color=red>";
                else if (count > (limit * 0.9f)) return @"[<color=orange>";
                return @"[<color=lime>";
            }
            ToolManager toolInstance = Singleton<ToolManager>.instance;
            NetManager netInstance = Singleton<NetManager>.instance;
            BuildingManager bmInstance = Singleton<BuildingManager>.instance;
            ZoneManager zmInstance = Singleton<ZoneManager>.instance;
            VehicleManager vmInstance = Singleton<VehicleManager>.instance;
            CitizenManager cmInstance = Singleton<CitizenManager>.instance;
            TransportManager transInstance = Singleton<TransportManager>.instance;
            PathManager pathInstance = Singleton<PathManager>.instance;
            GameAreaManager areaInstance = Singleton<GameAreaManager>.instance;
            DistrictManager dmInstance = Singleton<DistrictManager>.instance;
            TreeManager tmInstance = Singleton<TreeManager>.instance;

            StringBuilder sb = new StringBuilder(250);
            sb.AppendLine("\n<b>Net Segments").AppendLine("Net Nodes").AppendLine("Net Lanes").
                AppendLine("Buildings").AppendLine("Zoned Blocks").AppendLine("Vehicles Active").AppendLine("Parked Vehicles").
                AppendLine("Citizens").AppendLine("Citizen Units").AppendLine("Citizen Instances").AppendLine("Transport Lines").AppendLine("Path Units").
                AppendLine("Areas").AppendLine("Districts").AppendLine("Trees").AppendLine("Props</b>");
            names = sb.ToString();
            sb.Length = 0;
            sb.AppendLine("<b>[Max Size]</b>").
                Append('[').Append(netInstance.m_segments.m_size).AppendLine("]").
                Append('[').Append(netInstance.m_nodes.m_size).AppendLine("]").
                Append('[').Append(netInstance.m_lanes.m_size).AppendLine("]").
                Append('[').Append(bmInstance.m_buildings.m_size).AppendLine("]").
                Append('[').Append(zmInstance.m_blocks.m_size).AppendLine("]").
                Append('[').Append(vmInstance.m_vehicles.m_size).AppendLine("]").
                Append('[').Append(vmInstance.m_parkedVehicles.m_size).AppendLine("]").
                Append('[').Append(cmInstance.m_citizens.m_size).AppendLine("]").
                Append('[').Append(cmInstance.m_units.m_size).AppendLine("]").
                Append('[').Append(cmInstance.m_instances.m_size).AppendLine("]").
                Append('[').Append(transInstance.m_lines.m_size).AppendLine("]").
                Append('[').Append(pathInstance.m_pathUnits.m_size).AppendLine("]").
                Append('[').Append(areaInstance.m_areaGrid.Length).AppendLine("]").
                Append('[').Append(dmInstance.m_districts.m_size).AppendLine("]").
                Append('[').Append(tmInstance.m_trees.m_size).AppendLine("]").
                Append('[').Append(EPropManager.m_props.m_size).AppendLine("]");
            maxSize = sb.ToString();
            sb.Length = 0;
            sb.AppendLine("<b>[Limit]</b>").
                Append('[').Append(GetSegmentsLimits(toolInstance, netInstance)).AppendLine("]").
                Append('[').Append(GetNodesLimits(toolInstance, netInstance)).AppendLine("]").
                Append('[').Append(GetLanesLimits(toolInstance, netInstance)).AppendLine("]").
                Append('[').Append(GetBuildingLimits(toolInstance, bmInstance)).AppendLine("]").
                Append('[').Append(GetZoneLimits(toolInstance, zmInstance)).AppendLine("]").
                Append('[').Append(VehicleManager.MAX_VEHICLE_COUNT).AppendLine("]").
                Append('[').Append(VehicleManager.MAX_PARKED_COUNT).AppendLine("]").
                Append('[').Append(CitizenManager.MAX_CITIZEN_COUNT).AppendLine("]").
                Append('[').Append(CitizenManager.MAX_UNIT_COUNT).AppendLine("]").
                Append('[').Append(CitizenManager.MAX_INSTANCE_COUNT).AppendLine("]").
                Append('[').Append(TransportManager.MAX_LINE_COUNT).AppendLine("]").
                Append('[').Append(PathManager.MAX_PATHUNIT_COUNT).AppendLine("]").
                Append('[').Append(areaInstance.MaxAreaCount).AppendLine("]").
                Append('[').Append(DistrictManager.MAX_DISTRICT_COUNT - 2).AppendLine("]").
                Append('[').Append(GetTreeLimits(toolInstance, tmInstance)).AppendLine("]").
                Append('[').Append(GetPropLimits(toolInstance)).AppendLine("]");
            limits = sb.ToString();
            GetStats = (s) => {
                int val = (int)netInstance.m_segments.ItemCount() - 1;
                s.Append(GetLimitColor(val, netInstance.m_segments.m_buffer.Length - 500)).Append(val).AppendLine("</color>]");
                val = (int)netInstance.m_nodes.ItemCount() - 1;
                s.Append(GetLimitColor(val, netInstance.m_nodes.m_buffer.Length - 512)).Append(val).AppendLine("</color>]");
                val = (int)netInstance.m_lanes.ItemCount() - 1;
                s.Append(GetLimitColor(val, netInstance.m_lanes.m_buffer.Length - 4096)).Append(val).AppendLine("</color>]");
                val = (int)bmInstance.m_buildings.ItemCount() - 1;
                s.Append(GetLimitColor(val, bmInstance.m_buildings.m_buffer.Length - 512)).Append(val).AppendLine("</color>]");
                val = (int)zmInstance.m_blocks.ItemCount() - 1;
                s.Append(GetLimitColor(val, zmInstance.m_blocks.m_buffer.Length - 512)).Append(val).AppendLine("</color>]");
                val = (int)vmInstance.m_vehicles.ItemCount() - 1;
                s.Append(GetLimitColor(val, vmInstance.m_vehicles.m_buffer.Length)).Append(val).AppendLine("</color>]");
                val = vmInstance.m_parkedCount;
                s.Append(GetLimitColor(val, vmInstance.m_parkedVehicles.m_buffer.Length)).Append(val).AppendLine("</color>]");
                val = (int)cmInstance.m_citizens.ItemCount() - 1;
                s.Append(GetLimitColor(val, cmInstance.m_citizens.m_buffer.Length)).Append(val).AppendLine("</color>]");
                val = (int)cmInstance.m_units.ItemCount() - 1;
                s.Append(GetLimitColor(val, cmInstance.m_units.m_buffer.Length)).Append(val).AppendLine("</color>]");
                val = (int)cmInstance.m_instances.ItemCount() - 1;
                s.Append(GetLimitColor(val, cmInstance.m_instances.m_buffer.Length)).Append(val).AppendLine("</color>]");
                val = (int)transInstance.m_lines.ItemCount() - 1;
                s.Append(GetLimitColor(val, transInstance.m_lines.m_buffer.Length)).Append(val).AppendLine("</color>]");
                val = (int)pathInstance.m_pathUnits.ItemCount() - 1;
                s.Append(GetLimitColor(val, pathInstance.m_pathUnits.m_buffer.Length)).Append(val).AppendLine("</color>]");
                val = areaInstance.m_areaCount;
                s.Append(GetLimitColor(val, areaInstance.m_areaGrid.Length)).Append(val).AppendLine("</color>]");
                val = dmInstance.m_districtCount;
                s.Append(GetLimitColor(val, dmInstance.m_districts.m_buffer.Length)).Append(val).AppendLine("</color>]");
                val = tmInstance.m_treeCount;
                s.Append(GetLimitColor(val, tmInstance.m_trees.m_buffer.Length - 5)).Append(val).AppendLine("</color>]");
                val = (int)EPropManager.m_props.ItemCount() - 1;
                s.Append(GetLimitColor(val, EPropManager.MAX_GAME_PROPS_LIMIT)).Append(val).AppendLine("</color>]");
            };
        }

        internal static IEnumerator UpdateStats() {
            StringBuilder sb = m_stringBuffer;
            GetStatAPI getStats = GetStats;
            while (m_isVisible) {
                sb.Length = 0;
                sb.AppendLine("<b>[Item Count]</b>");
                getStats(sb);
                m_stats = sb.ToString();
                yield return new WaitForSeconds(1.5f);
            }
        }

        private static void OutputStats(int windowID) {
            GUI.Label(m_nameRect, m_names, m_nameStyle);
            GUI.Label(m_maxSizeRect, m_maxSizes);
            GUI.Label(m_limitsRect, m_limits);
            GUI.Label(m_statsRect, m_stats);
        }
    }
}
