using ColossalFramework;
using ColossalFramework.Math;
using System;
using System.Reflection;
using UnityEngine;

namespace EManagersLib {
    public static class EDefaultTool {
		private static readonly Vector2 m_range_minXZ = new Vector2(-0.5f, -0.5f);
		private static readonly Vector2 m_range_minXmaxZ = new Vector2(-0.5f, 0.5f);
		private static readonly Vector2 m_range_maxXZ = new Vector2(0.5f, 0.5f);
		private static readonly Vector2 m_range_maxXminZ = new Vector2(0.5f, -0.5f);
		public static bool CheckProp(ToolController toolController, uint prop) {
			if ((toolController.m_mode & ItemClass.Availability.Editors) != ItemClass.Availability.None) return true;
			Vector2 a = VectorUtils.XZ(Singleton<PropManager>.instance.m_props.m_buffer[(int)prop].Position);
			Quad2 quad = default;
			quad.a = a + m_range_minXZ;
			quad.b = a + m_range_minXmaxZ;
			quad.c = a + m_range_maxXZ;
			quad.d = a + m_range_maxXminZ;
			return !Singleton<GameAreaManager>.instance.QuadOutOfArea(quad);
		}

		public static void SetHoverInstance(EInstanceID id, ref InstanceID hoverInstance) {
			EInstanceID hover = (EInstanceID)hoverInstance;
			if (id != hover) {
				uint propID = hover.Prop;
				uint treeID = hover.Tree;
				ushort buildingID = hover.Building;
				ushort disasterID = hover.Disaster;
				ushort transportID = hover.TransportLine;
				if (hover.TransportLine != 0u) {
					Singleton<TransportManager>.instance.m_lines.m_buffer[hoverInstance.TransportLine].m_flags &= ~TransportLine.Flags.Selected;
				} else if (propID != 0u) {
					EPropInstance[] props = EPropManager.m_props.m_buffer;
					if (props[propID].Hidden) {
						props[propID].Hidden = false;
					}
				} else if (treeID != 0u) {
					TreeInstance[] trees = Singleton<TreeManager>.instance.m_trees.m_buffer;
					if(trees[treeID].Hidden) {
						trees[treeID].Hidden = false;
						Singleton<TreeManager>.instance.UpdateTreeRenderer(treeID, true);
                    }
				} else if (buildingID != 0u) {
					Building[] buildings = Singleton<BuildingManager>.instance.m_buildings.m_buffer;
					if((buildings[buildingID].m_flags & Building.Flags.Hidden) != Building.Flags.None) {
						buildings[buildingID].m_flags &= ~Building.Flags.Hidden;
						Singleton<BuildingManager>.instance.UpdateBuildingRenderer(buildingID, true);
                    }
				} else if (disasterID != 0) {
					DisasterData[] disasters = Singleton<DisasterManager>.instance.m_disasters.m_buffer;
					if((disasters[disasterID].m_flags & DisasterData.Flags.Hidden) != DisasterData.Flags.None) {
						disasters[disasterID].m_flags &= ~DisasterData.Flags.Hidden;
                    }
				}
				hoverInstance = id.OrigID;
				if(transportID != 0) {
					Singleton<TransportManager>.instance.m_lines.m_buffer[transportID].m_flags |= TransportLine.Flags.Selected;
				}
			}
		}
	}
}
