using ColossalFramework;
using ColossalFramework.UI;
using EManagersLib.API;
using UnityEngine;

namespace EManagersLib {
    public class EStatsPanel : UIPanel {
        public const string cacheName = "StatsPanel";
        private const float WIDTH = 600f;
        private const float HEIGHT = 500f;
        private const float HEADER = 40f;
        private const float SPACING = 10f;
        private const float SPACING22 = 22f;
        private UIButton m_closeBtn;
        private UILabel m_title;
        private UIDragHandle m_dragHandle;
        private UILabel m_HeaderDataText;
        private UILabel m_NetSegmentsText;
        private UILabel m_NetSegmentsValue;
        private UILabel m_NetNodesText;
        private UILabel m_NetNodesValue;
        private UILabel m_NetLanesText;
        private UILabel m_NetLanesValue;
        private UILabel m_BuildingsText;
        private UILabel m_BuildingsValue;
        private UILabel m_ZonedBlocksText;
        private UILabel m_ZonedBlocksValue;
        private UILabel m_VehiclesText;
        private UILabel m_VehiclesValue;
        private UILabel m_ParkedCarsText;
        private UILabel m_ParkedCarsValue;
        private UILabel m_CitizensText;
        private UILabel m_CitizensValue;
        private UILabel m_CitizenUnitsText;
        private UILabel m_CitizenUnitsValue;
        private UILabel m_CitizenAgentsText;
        private UILabel m_CitizenAgentsValue;
        private UILabel m_PathUnitsText;
        private UILabel m_PathUnitsValue;
        private UILabel m_TransportLinesText;
        private UILabel m_TransportLinesValue;
        private UILabel m_AreasText;
        private UILabel m_AreasValue;
        private UILabel m_DistrictsText;
        private UILabel m_DistrictsValue;
        private UILabel m_TreesText;
        private UILabel m_TreesValue;
        private UILabel m_UserPropsText;
        private UILabel m_UserPropsValue;
        private int GetSegmentsLimits() {
            int len = Singleton<NetManager>.instance.m_segments.m_buffer.Length;
            switch (Singleton<ToolManager>.instance.m_properties.m_mode & (ItemClass.Availability.MapEditor | ItemClass.Availability.AssetEditor)) {
            case ItemClass.Availability.MapEditor: return NetManager.MAX_MAP_SEGMENTS;
            case ItemClass.Availability.AssetEditor: return NetManager.MAX_ASSET_SEGMENTS;
            }
            return len - 512;
        }
        private int GetNodesLimits() {
            int len = Singleton<NetManager>.instance.m_segments.m_buffer.Length;
            switch (Singleton<ToolManager>.instance.m_properties.m_mode & (ItemClass.Availability.MapEditor | ItemClass.Availability.AssetEditor)) {
            case ItemClass.Availability.MapEditor: return NetManager.MAX_MAP_NODES;
            case ItemClass.Availability.AssetEditor: return NetManager.MAX_ASSET_NODES;
            }
            return len - 500;
        }
        private int GetLanesLimits() {
            int len = Singleton<NetManager>.instance.m_segments.m_buffer.Length;
            switch (Singleton<ToolManager>.instance.m_properties.m_mode & (ItemClass.Availability.MapEditor | ItemClass.Availability.AssetEditor)) {
            case ItemClass.Availability.MapEditor: return NetManager.MAX_MAP_LANES;
            case ItemClass.Availability.AssetEditor: return NetManager.MAX_ASSET_LANES;
            }
            return len - 4096;
        }
        private int GetBuildingLimits() {
            int len = Singleton<BuildingManager>.instance.m_buildings.m_buffer.Length;
            switch (Singleton<ToolManager>.instance.m_properties.m_mode & (ItemClass.Availability.MapEditor | ItemClass.Availability.AssetEditor)) {
            case ItemClass.Availability.MapEditor: return BuildingManager.MAX_MAP_BUILDINGS;
            case ItemClass.Availability.AssetEditor: return BuildingManager.MAX_ASSET_BUILDINGS;
            }
            return len - 512;
        }
        private int GetZoneLimits() {
            switch (Singleton<ToolManager>.instance.m_properties.m_mode & (ItemClass.Availability.MapEditor | ItemClass.Availability.AssetEditor)) {
            case ItemClass.Availability.MapEditor: return ZoneManager.MAX_MAP_BLOCKS;
            case ItemClass.Availability.AssetEditor: return ZoneManager.MAX_ASSET_BLOCKS;
            }
            return Singleton<ZoneManager>.instance.m_blocks.m_buffer.Length - 512;
        }
        private int GetTreeLimits() {
            int len = Singleton<TreeManager>.instance.m_trees.m_buffer.Length;
            switch (Singleton<ToolManager>.instance.m_properties.m_mode & (ItemClass.Availability.MapEditor | ItemClass.Availability.AssetEditor)) {
            case ItemClass.Availability.MapEditor: return len - (TreeManager.MAX_TREE_COUNT - TreeManager.MAX_MAP_TREES);
            case ItemClass.Availability.AssetEditor: return TreeManager.MAX_ASSET_TREES;
            }
            return len - 5;
        }
        private int GetPropLimits() {
            switch (Singleton<ToolManager>.instance.m_properties.m_mode & (ItemClass.Availability.MapEditor | ItemClass.Availability.AssetEditor)) {
            case ItemClass.Availability.MapEditor: return EPropManager.MAX_MAP_PROPS_LIMIT;
            case ItemClass.Availability.AssetEditor: return PropManager.MAX_ASSET_PROPS;
            }
            return EPropManager.MAX_GAME_PROPS_LIMIT;
        }

        private void CreateText() {
            NetManager netInstance = Singleton<NetManager>.instance;
            VehicleManager vehicleInstance = Singleton<VehicleManager>.instance;
            CitizenManager citizenInstance = Singleton<CitizenManager>.instance;
            GameAreaManager areaInstance = Singleton<GameAreaManager>.instance;
            TreeManager treeInstance = Singleton<TreeManager>.instance;
            m_HeaderDataText = AddUIComponent<UILabel>();
            m_HeaderDataText.textScale = 0.825f;
            m_HeaderDataText.text = "Object Type     [#maxsize]  |  [#defaultLimit]:         [#itemCount]";
            m_HeaderDataText.tooltip = "Maxsize is the max size of the holding array, defaultlimit is the number the game or map by default is restricting you too.\n" +
                                       "#InUse: is the number you are currently using.\n" +
                                       "Optionally #itemCount is shown as checksum, it should never be more than one higher than #InUse if shown.";
            m_HeaderDataText.relativePosition = new Vector3(SPACING, 50f);
            m_HeaderDataText.autoSize = true;
            m_NetSegmentsText = AddUIComponent<UILabel>();
            m_NetSegmentsText.text = "Net Segments     [" + netInstance.m_segments.m_size + "]  |  [" + GetSegmentsLimits() + "]:";
            m_NetSegmentsText.tooltip = "You can think of segments as roads, but they are used for far more then just roads.\n Each segment is typically connected in a chain of other segments to a 'node'.";
            m_NetSegmentsText.relativePosition = new Vector3(SPACING, m_HeaderDataText.relativePosition.y + SPACING22);
            m_NetSegmentsText.autoSize = true;
            m_NetSegmentsText.name = "NetSegments";
            m_NetNodesText = AddUIComponent<UILabel>();
            m_NetNodesText.relativePosition = new Vector3(SPACING, m_NetSegmentsText.relativePosition.y + SPACING22);
            m_NetNodesText.text = "Net Nodes     [" + netInstance.m_nodes.m_size + "]  |  [" + GetNodesLimits() + "]:";
            m_NetNodesText.tooltip = "The number of Nodes. Think of nodes sort of like intersections, or the point that the first segment of a path connects too,\n each node typically contains zero or more segments.";
            m_NetNodesText.autoSize = true;
            m_NetNodesText.name = "NetNodes";
            m_NetLanesText = AddUIComponent<UILabel>();
            m_NetLanesText.relativePosition = new Vector3(SPACING, m_NetNodesText.relativePosition.y + SPACING22);
            m_NetLanesText.text = "Net Lanes     [" + netInstance.m_lanes.m_size + "]  |  [" + GetLanesLimits() + "]:";
            m_NetLanesText.tooltip = "The number of lanes. Lanes are used by more than just roads and rail.\n Things like ped.paths, bike paths, transportlines and similar things create and use them too.\n" +
                                     "It's not alway logical most roads with only two lanes by default get assigned six lanes,\n so they can be upgraded later I presume.";
            m_NetLanesText.autoSize = true;
            m_NetLanesText.name = "NetLanes";
            m_BuildingsText = AddUIComponent<UILabel>();
            m_BuildingsText.relativePosition = new Vector3(SPACING, m_NetLanesText.relativePosition.y + SPACING22);
            m_BuildingsText.text = "Buildings    [" + Singleton<BuildingManager>.instance.m_buildings.m_size + "]  |  [" + GetBuildingLimits() + "]:";
            m_BuildingsText.tooltip = "The number of building objects created.\nSome things you might not expect count as buildings.";
            m_BuildingsText.autoSize = true;
            m_BuildingsText.name = "Buildings";
            m_ZonedBlocksText = AddUIComponent<UILabel>();
            m_ZonedBlocksText.relativePosition = new Vector3(SPACING, m_BuildingsText.relativePosition.y + SPACING22);
            m_ZonedBlocksText.text = "Zoned Blocks     [" + Singleton<ZoneManager>.instance.m_blocks.m_size + "]  |  [" + GetZoneLimits() + "]:";
            m_ZonedBlocksText.tooltip = "The number of Zoned blocks (squares) you have in your map.";
            m_ZonedBlocksText.autoSize = true;
            m_ZonedBlocksText.name = "ZonedBlocks";
            m_VehiclesText = AddUIComponent<UILabel>();
            m_VehiclesText.relativePosition = new Vector3(SPACING, m_ZonedBlocksText.relativePosition.y + SPACING22);
            m_VehiclesText.text = "Vehicles Active     [" + vehicleInstance.m_vehicles.m_size + "]  |  [" + VehicleManager.MAX_VEHICLE_COUNT + "]:";
            m_VehiclesText.tooltip = "The number of vehicles actively in use during the last update.\n Being at the max is technically ok, if if you permenantly though I suggest CSL Service Reserve mod.\n" +
                                     "Also look for glitched(or internally backed up) cargo stations.";
            m_VehiclesText.autoSize = true;
            m_VehiclesText.name = "Vehicles";
            m_ParkedCarsText = AddUIComponent<UILabel>();
            m_ParkedCarsText.relativePosition = new Vector3(SPACING, m_VehiclesText.relativePosition.y + SPACING22);
            m_ParkedCarsText.text = "Parked Cars     [" + vehicleInstance.m_parkedVehicles.m_size + "]  |  [" + VehicleManager.MAX_PARKED_COUNT + "]:";
            m_ParkedCarsText.tooltip = "The number of cars that are currently parked.";
            m_ParkedCarsText.autoSize = true;
            m_ParkedCarsText.name = "ParkedVehicles";
            m_CitizensText = AddUIComponent<UILabel>();
            m_CitizensText.relativePosition = new Vector3(SPACING, m_ParkedCarsText.relativePosition.y + SPACING22);
            m_CitizensText.text = "Citizens     [" + citizenInstance.m_citizens.m_size + "]  |  [" + CitizenManager.MAX_CITIZEN_COUNT + "]:";
            m_CitizensText.tooltip = "The number of citizens the game is currently simulating.";
            m_CitizensText.autoSize = true;
            m_CitizensText.name = "Citizens";
            m_CitizenUnitsText = AddUIComponent<UILabel>();
            m_CitizenUnitsText.relativePosition = new Vector3(SPACING, m_CitizensText.relativePosition.y + SPACING22);
            m_CitizenUnitsText.text = "Citizen Units     [" + citizenInstance.m_units.m_size + "]  |  [" + CitizenManager.MAX_UNIT_COUNT + "]:";
            m_CitizenUnitsText.tooltip = "The number of citizen units in use, these are used by ai's to hold a group of citizens\nThey can represent a home, passengers, a work site, students,etc.\n" +
                                         "For example when a cop car gets created it will create one of these to hold the cop and any criminals caught.";
            m_CitizenUnitsText.autoSize = true;
            m_CitizenUnitsText.name = "CitizenUnits";
            m_CitizenAgentsText = AddUIComponent<UILabel>();
            m_CitizenAgentsText.relativePosition = new Vector3(SPACING, m_CitizenUnitsText.relativePosition.y + SPACING22);
            m_CitizenAgentsText.text = "Citizen Instances     [" + citizenInstance.m_instances.m_size + "]  |  [" + CitizenManager.MAX_INSTANCE_COUNT + "]:";
            m_CitizenAgentsText.tooltip = "The number of cims during the last pass that were 'actively' being simulated,\nie walking, biking, chilling in the park, not at home or at work, etc";
            m_CitizenAgentsText.autoSize = true;
            m_CitizenAgentsText.name = "CitizenAgents";
            m_TransportLinesText = AddUIComponent<UILabel>();
            m_TransportLinesText.relativePosition = new Vector3(SPACING, m_CitizenAgentsText.relativePosition.y + SPACING22);
            m_TransportLinesText.text = "Transport Lines     [" + Singleton<TransportManager>.instance.m_lines.m_size + "]  |  [" + TransportManager.MAX_LINE_COUNT + "]:";
            m_TransportLinesText.tooltip = "The number of transport lines.";
            m_TransportLinesText.autoSize = true;
            m_TransportLinesText.name = "CSLShowMoreLimits_Text_10";
            m_PathUnitsText = AddUIComponent<UILabel>();
            m_PathUnitsText.relativePosition = new Vector3(SPACING, m_TransportLinesText.relativePosition.y + SPACING22);
            m_PathUnitsText.text = "Path Units     [" + Singleton<PathManager>.instance.m_pathUnits.m_size + "]  |  [" + PathManager.MAX_PATHUNIT_COUNT + "]:";
            m_PathUnitsText.tooltip = "Number of paths in use by the pathfinder.";
            m_PathUnitsText.autoSize = true;
            m_PathUnitsText.name = "PathUnits";
            m_AreasText = AddUIComponent<UILabel>();
            m_AreasText.relativePosition = new Vector3(SPACING, m_PathUnitsText.relativePosition.y + SPACING22);
            m_AreasText.text = "Areas     [" + areaInstance.m_areaGrid.Length + "]  |  [" + areaInstance.MaxAreaCount + "]:";
            m_AreasText.tooltip = @"The number of area grids you are allowed to buy\purchase.";
            m_AreasText.autoSize = true;
            m_AreasText.name = "Areas";
            m_DistrictsText = AddUIComponent<UILabel>();
            m_DistrictsText.relativePosition = new Vector3(SPACING, m_AreasText.relativePosition.y + SPACING22);
            m_DistrictsText.text = "Districts     [" + Singleton<DistrictManager>.instance.m_districts.m_size + "]  |  [" + (DistrictManager.MAX_DISTRICT_COUNT - 2) + "]:";
            m_DistrictsText.tooltip = "The number of districts.";
            m_DistrictsText.autoSize = true;
            m_DistrictsText.name = "Districts";
            m_TreesText = AddUIComponent<UILabel>();
            m_TreesText.relativePosition = new Vector3(SPACING, m_DistrictsText.relativePosition.y + SPACING22);
            m_TreesText.text = "Trees     [" + treeInstance.m_trees.m_size + "]  |  [" + GetTreeLimits() + "]:";
            m_TreesText.tooltip = ((Singleton<TreeManager>.instance.m_trees.m_size > 262144u) ? "The number of placed trees.\nUnlimited Trees mod detected!" : "The number of placed trees.\n Remember just cause you plow over a tree with a road does not mean it's gone.\n They must actually be bulldozed.");
            m_TreesText.autoSize = true;
            m_TreesText.name = "Trees";
            m_UserPropsText = AddUIComponent<UILabel>();
            m_UserPropsText.relativePosition = new Vector3(SPACING, m_TreesText.relativePosition.y + SPACING22);
            m_UserPropsText.text = "User Props     [" + EPropManager.m_props.m_size + "]  |  [" + GetPropLimits() + "]:";
            m_UserPropsText.tooltip = "The number of props placed on the map. \n This, far as I can tell is user placed prop limit, not counting those embedded with prefabs.";
            m_UserPropsText.autoSize = true;
            m_UserPropsText.name = "Props";
        }

        private void CreateValue() {
            m_NetSegmentsValue = AddUIComponent<UILabel>();
            m_NetSegmentsValue.relativePosition = new Vector3(m_NetSegmentsText.relativePosition.x + m_NetSegmentsText.width + SPACING * 7f, m_NetSegmentsText.relativePosition.y);
            m_NetSegmentsValue.autoSize = true;
            m_NetSegmentsValue.tooltip = "";
            m_NetSegmentsValue.name = "NetSegment_Value";
            m_NetNodesValue = AddUIComponent<UILabel>();
            m_NetNodesValue.relativePosition = new Vector3(m_NetSegmentsValue.relativePosition.x, m_NetNodesText.relativePosition.y);
            m_NetNodesValue.autoSize = true;
            m_NetNodesValue.name = "NetNodes_Value";
            m_NetLanesValue = AddUIComponent<UILabel>();
            m_NetLanesValue.relativePosition = new Vector3(m_NetNodesValue.relativePosition.x, m_NetLanesText.relativePosition.y);
            m_NetLanesValue.autoSize = true;
            m_NetLanesValue.name = "NetLanes_Value";
            m_BuildingsValue = AddUIComponent<UILabel>();
            m_BuildingsValue.relativePosition = new Vector3(m_NetLanesValue.relativePosition.x, m_BuildingsText.relativePosition.y);
            m_BuildingsValue.autoSize = true;
            m_BuildingsValue.name = "Buildings_Value";
            m_ZonedBlocksValue = AddUIComponent<UILabel>();
            m_ZonedBlocksValue.relativePosition = new Vector3(m_BuildingsValue.relativePosition.x, m_ZonedBlocksText.relativePosition.y);
            m_ZonedBlocksValue.autoSize = true;
            m_ZonedBlocksValue.name = "ZonedBlocks_Value";
            m_VehiclesValue = AddUIComponent<UILabel>();
            m_VehiclesValue.relativePosition = new Vector3(m_ZonedBlocksValue.relativePosition.x, m_VehiclesText.relativePosition.y);
            m_VehiclesValue.autoSize = true;
            m_VehiclesValue.name = "Vehicles_Value";
            m_ParkedCarsValue = AddUIComponent<UILabel>();
            m_ParkedCarsValue.relativePosition = new Vector3(m_VehiclesValue.relativePosition.x, m_ParkedCarsText.relativePosition.y);
            m_ParkedCarsValue.autoSize = true;
            m_ParkedCarsValue.name = "ParkedCars_Value";
            m_CitizensValue = AddUIComponent<UILabel>();
            m_CitizensValue.relativePosition = new Vector3(m_ParkedCarsValue.relativePosition.x, m_CitizensText.relativePosition.y);
            m_CitizensValue.autoSize = true;
            m_CitizensValue.name = "Citizens_Value";
            m_CitizenUnitsValue = AddUIComponent<UILabel>();
            m_CitizenUnitsValue.relativePosition = new Vector3(m_CitizensValue.relativePosition.x, m_CitizenUnitsText.relativePosition.y);
            m_CitizenUnitsValue.autoSize = true;
            m_CitizenUnitsValue.name = "CitizenUnits_Value";
            m_CitizenAgentsValue = AddUIComponent<UILabel>();
            m_CitizenAgentsValue.relativePosition = new Vector3(m_CitizenUnitsValue.relativePosition.x, m_CitizenAgentsText.relativePosition.y);
            m_CitizenAgentsValue.autoSize = true;
            m_CitizenAgentsValue.name = "CitizenAgents_Value";
            m_TransportLinesValue = AddUIComponent<UILabel>();
            m_TransportLinesValue.relativePosition = new Vector3(m_CitizenAgentsValue.relativePosition.x, m_TransportLinesText.relativePosition.y);
            m_TransportLinesValue.autoSize = true;
            m_TransportLinesValue.name = "TransportLines_Value";
            m_PathUnitsValue = AddUIComponent<UILabel>();
            m_PathUnitsValue.relativePosition = new Vector3(m_TransportLinesValue.relativePosition.x, m_PathUnitsText.relativePosition.y);
            m_PathUnitsValue.autoSize = true;
            m_PathUnitsValue.name = "PathUnits_Value";
            m_AreasValue = AddUIComponent<UILabel>();
            m_AreasValue.relativePosition = new Vector3(m_PathUnitsValue.relativePosition.x, m_AreasText.relativePosition.y);
            m_AreasValue.autoSize = true;
            m_AreasValue.name = "Areas_Value";
            m_DistrictsValue = AddUIComponent<UILabel>();
            m_DistrictsValue.relativePosition = new Vector3(m_AreasValue.relativePosition.x, m_DistrictsText.relativePosition.y);
            m_DistrictsValue.autoSize = true;
            m_DistrictsValue.name = "Districts_Value";
            m_TreesValue = AddUIComponent<UILabel>();
            m_TreesValue.relativePosition = new Vector3(m_DistrictsValue.relativePosition.x, m_TreesText.relativePosition.y);
            m_TreesValue.autoSize = true;
            m_TreesValue.name = "Trees_Value";
            m_UserPropsValue = AddUIComponent<UILabel>();
            m_UserPropsValue.relativePosition = new Vector3(m_TreesValue.relativePosition.x, m_UserPropsText.relativePosition.y);
            m_UserPropsValue.autoSize = true;
            m_UserPropsValue.name = "UserProps_Value";
        }

        private Color GetLimitColor(int count, int limit) {
            if (count > (limit * 0.9f)) return Color.yellow;
            return Color.green;
        }

        public void RefreshLimit() {
            NetManager netInstance = Singleton<NetManager>.instance;
            VehicleManager vehicleInstance = Singleton<VehicleManager>.instance;
            CitizenManager citizenInstance = Singleton<CitizenManager>.instance;
            BuildingManager buildingInstance = Singleton<BuildingManager>.instance;
            ZoneManager zoneInstance = Singleton<ZoneManager>.instance;
            TransportManager transportInstance = Singleton<TransportManager>.instance;
            PathManager pathInstance = Singleton<PathManager>.instance;
            GameAreaManager areaInstance = Singleton<GameAreaManager>.instance;
            DistrictManager districtInstance = Singleton<DistrictManager>.instance;
            TreeManager treeInstance = Singleton<TreeManager>.instance;
            m_NetSegmentsValue.textColor = GetLimitColor((int)netInstance.m_segments.ItemCount() - 1, netInstance.m_segments.m_buffer.Length - 500);
            m_NetSegmentsValue.text = "[" + (netInstance.m_segments.ItemCount() - 1) + "]";
            m_NetNodesValue.textColor = GetLimitColor((int)netInstance.m_nodes.ItemCount() - 1, netInstance.m_nodes.m_buffer.Length - 512);
            m_NetNodesValue.text = "[" + (netInstance.m_nodes.ItemCount() - 1) + "]";
            m_NetLanesValue.textColor = GetLimitColor((int)netInstance.m_lanes.ItemCount() - 1, netInstance.m_nodes.m_buffer.Length - 4096);
            m_NetLanesValue.text = "[" + (netInstance.m_lanes.ItemCount() - 1) + "]";
            m_BuildingsValue.textColor = GetLimitColor((int)buildingInstance.m_buildings.ItemCount() - 1, buildingInstance.m_buildings.m_buffer.Length - 512);
            m_BuildingsValue.text = "[" + (buildingInstance.m_buildings.ItemCount() - 1) + "]";
            m_ZonedBlocksValue.textColor = GetLimitColor(zoneInstance.m_blockCount, zoneInstance.m_blocks.m_buffer.Length - 512);
            m_ZonedBlocksValue.text = "[" + (zoneInstance.m_blockCount) + "]";
            m_VehiclesValue.textColor = GetLimitColor((int)vehicleInstance.m_vehicles.ItemCount() - 1, vehicleInstance.m_vehicles.m_buffer.Length);
            m_VehiclesValue.text = "[" + (vehicleInstance.m_vehicles.ItemCount() - 1) + "]";
            m_ParkedCarsValue.textColor = GetLimitColor(vehicleInstance.m_parkedCount, vehicleInstance.m_parkedVehicles.m_buffer.Length);
            m_ParkedCarsValue.text = "[" + (vehicleInstance.m_parkedCount) + "]";
            m_CitizensValue.textColor = GetLimitColor(citizenInstance.m_citizenCount, citizenInstance.m_citizens.m_buffer.Length);
            m_CitizensValue.text = "[" + (citizenInstance.m_citizenCount) + "]";
            m_CitizenUnitsValue.textColor = GetLimitColor(citizenInstance.m_unitCount, citizenInstance.m_units.m_buffer.Length);
            m_CitizenUnitsValue.text = "[" + (citizenInstance.m_unitCount) + "]";
            m_CitizenAgentsValue.textColor = GetLimitColor(citizenInstance.m_instanceCount, citizenInstance.m_instances.m_buffer.Length);
            m_CitizenAgentsValue.text = "[" + (citizenInstance.m_instanceCount) + "]";
            m_TransportLinesValue.textColor = GetLimitColor(transportInstance.m_lineCount, transportInstance.m_lines.m_buffer.Length);
            m_TransportLinesValue.text = "[" + (transportInstance.m_lineCount) + "]";
            m_PathUnitsValue.textColor = GetLimitColor(pathInstance.m_pathUnitCount, pathInstance.m_pathUnits.m_buffer.Length);
            m_PathUnitsValue.text = "[" + (pathInstance.m_pathUnitCount) + "]";
            m_AreasValue.textColor = GetLimitColor(areaInstance.m_areaCount, areaInstance.m_areaGrid.Length);
            m_AreasValue.text = "[" + (areaInstance.m_areaCount) + "]";
            m_DistrictsValue.textColor = GetLimitColor(districtInstance.m_districtCount, districtInstance.m_districts.m_buffer.Length);
            m_DistrictsValue.text = "[" + (districtInstance.m_districtCount) + "]";
            m_TreesValue.textColor = GetLimitColor(treeInstance.m_treeCount, treeInstance.m_trees.m_buffer.Length - 5);
            m_TreesValue.text = "[" + (treeInstance.m_treeCount) + "]";
            m_UserPropsValue.textColor = GetLimitColor((int)EPropManager.m_props.ItemCount() - 1, EPropManager.MAX_GAME_PROPS_LIMIT);
            m_UserPropsValue.text = "[" + (EPropManager.m_props.ItemCount() - 1) + "]";
        }

        public override void Start() {
            UIView root = UIView.GetAView();
            base.Start();
            size = new Vector2(WIDTH, HEIGHT);
            backgroundSprite = "MenuPanel";
            canFocus = true;
            isInteractive = true;
            BringToFront();
            relativePosition = new Vector3((root.fixedWidth / 2 - 200), (root.fixedHeight / 2 - 350));
            opacity = 0.8f;
            cachedName = cacheName;
            m_dragHandle = AddUIComponent<UIDragHandle>();
            m_dragHandle.target = this;
            m_title = AddUIComponent<UILabel>();
            m_title.text = "Counter and Object Limit Data";
            m_title.relativePosition = new Vector3(WIDTH / 2f - m_title.width / 2f - 25f, HEADER / 2f - m_title.height / 2f);
            m_title.textAlignment = UIHorizontalAlignment.Center;
            m_title.verticalAlignment = UIVerticalAlignment.Middle;
            m_closeBtn = AddUIComponent<UIButton>();
            m_closeBtn.normalBgSprite = "buttonclose";
            m_closeBtn.hoveredBgSprite = "buttonclosehover";
            m_closeBtn.pressedBgSprite = "buttonclosepressed";
            m_closeBtn.relativePosition = new Vector3(WIDTH - 35f, 5f, 10f);
            m_closeBtn.eventClick += (c, e) => {
                CancelInvoke("RefreshLimit");
                Hide();
            };
            CreateText();
            CreateValue();
            Hide();
        }

        public override void Update() {
            if (EKeyBinding.m_toggleStatsPanel.IsPressed(Event.current)) {
                if (isVisible) {
                    CancelInvoke("RefreshLimit");
                    Hide();
                } else {
                    InvokeRepeating("RefreshLimit", 0.2f, 4f);
                    Show();
                }
            }
            base.Update();
        }
    }
}
