using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.PlatformServices;
using ColossalFramework.Threading;
using ColossalFramework.UI;
using System;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using UnityEngine;

namespace EManagersLib {
	public class DefaultToolSample : ToolBase {
		public delegate void BuildingRelocated(ushort building);
		public CursorInfo m_cursor;
		public CursorInfo m_undergroundCursor;
		protected Vector3 m_mousePosition;
		protected Vector3 m_accuratePosition;
		protected InstanceID m_hoverInstance;
		protected InstanceID m_hoverInstance2;
		protected Ray m_mouseRay;
		protected float m_mouseRayLength;
		protected Vector3 m_rayRight;
		protected bool m_mouseRayValid;
		[NonSerialized]
		protected ToolBase.ToolErrors m_selectErrors;
		private int m_subHoverIndex;
		private int m_freneticPlayer;
		private float m_angle;
		private float m_holdTimer;
		private bool m_mouseLeftDown;
		private bool m_mouseRightDown;
		private bool m_angleChanged;
		private bool m_fixedHeight;
		private bool m_accuratePositionValid;

		protected override void Awake() {
			base.Awake();
			Singleton<InstanceManager>.instance.m_instanceChanged += new InstanceManager.ChangeAction(this.ChangeTarget);
		}

		protected override void OnDestroy() {
			Singleton<InstanceManager>.instance.m_instanceChanged -= new InstanceManager.ChangeAction(this.ChangeTarget);
			base.OnDestroy();
		}

		protected override void OnToolGUI(Event e) {
			if (!m_toolController.IsInsideUI && e.type == EventType.MouseDown) {
				if (m_hoverInstance.NetNode != 0 && m_subHoverIndex != 0) {
					if (e.button == 0) {
						Singleton<SimulationManager>.instance.AddAction(delegate {
							InstanceID hoverInstance3 = m_hoverInstance;
							int subHoverIndex = m_subHoverIndex;
							if (hoverInstance3.NetNode != 0 && subHoverIndex != 0) {
								NetManager instance6 = Singleton<NetManager>.instance;
								NetInfo info7 = instance6.m_nodes.m_buffer[hoverInstance3.NetNode].Info;
								info7.m_netAI.ClickNodeButton(hoverInstance3.NetNode, ref instance6.m_nodes.m_buffer[hoverInstance3.NetNode], subHoverIndex);
							}
						});
					}
				} else if (m_hoverInstance.NetSegment != 0 && m_subHoverIndex != 0) {
					if (e.button == 0) {
						if (m_subHoverIndex == -1) {
							Singleton<InstanceManager>.instance.SelectInstance(m_hoverInstance);
						} else {
							m_selectErrors = ToolBase.ToolErrors.Pending;
							Singleton<SimulationManager>.instance.AddAction(StartMoving());
						}
					}
				} else if ((m_toolController.m_mode & ItemClass.Availability.MapAndAsset) != ItemClass.Availability.None) {
					if (!m_hoverInstance.IsEmpty) {
						if (e.button == 0) {
							m_selectErrors = ToolBase.ToolErrors.Pending;
							Singleton<SimulationManager>.instance.AddAction(StartMoving());
						} else if (e.button == 1) {
							m_selectErrors = ToolBase.ToolErrors.Pending;
							Singleton<SimulationManager>.instance.AddAction(StartRotating());
						}
					}
				} else if (e.button == 0) {
					if (m_selectErrors == ToolBase.ToolErrors.None || m_selectErrors == ToolBase.ToolErrors.RaycastFailed) {
						InstanceID hoverInstance = m_hoverInstance;
						Vector3 mousePosition = m_mousePosition;
						ushort building = hoverInstance.Building;
						if (building != 0) {
							BuildingInfo info = Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)building].Info;
							if (info != null && info.m_class.m_service == ItemClass.Service.PoliceDepartment) {
								if (++m_freneticPlayer == 100 && Singleton<SimulationManager>.instance.m_metaData.m_disableAchievements != SimulationMetaData.MetaBool.True && !PlatformService.achievements["FreneticPlayer"].achieved) {
									PlatformService.achievements["FreneticPlayer"].Unlock();
								}
							} else {
								m_freneticPlayer = 0;
							}
						} else {
							m_freneticPlayer = 0;
						}
						UIInput.MouseUsed();
						DefaultTool.OpenWorldInfoPanel(hoverInstance, mousePosition);
						if (hoverInstance.NetSegment != 0) {
							Singleton<SimulationManager>.instance.AddAction(delegate {
								Singleton<NetManager>.instance.m_roadNames.Disable();
							});
						} else if (!hoverInstance.IsEmpty) {
							Singleton<SimulationManager>.instance.AddAction(delegate {
								Singleton<GuideManager>.instance.m_worldInfoNotUsed.Disable();
							});
						}
						if (Application.isEditor) {
							PrefabInfo prefabInfo = InstanceManager.GetPrefabInfo(hoverInstance);
							if (prefabInfo != null) {
								CODebugBase<LogChannel>.Log(LogChannel.Core, prefabInfo.gameObject.name, prefabInfo.gameObject);
							}
						}
					}
					if ((m_toolController.m_mode & ItemClass.Availability.ScenarioEditor) != ItemClass.Availability.None && this.m_hoverInstance.Disaster != 0) {
						if (SelectingDisasterPanel.instance.m_currentEffectItem != null) {
							SelectingDisasterPanel.instance.SelectDisaster(m_hoverInstance);
						}
						m_holdTimer = 0.2f;
					}
				} else if (e.button == 1 && (m_toolController.m_mode & ItemClass.Availability.ScenarioEditor) != ItemClass.Availability.None && this.m_hoverInstance.Disaster != 0) {
					m_selectErrors = ToolBase.ToolErrors.Pending;
					Singleton<SimulationManager>.instance.AddAction(StartRotating());
				}
			} else if (e.type == EventType.MouseUp) {
				if (m_hoverInstance.NetSegment != 0 && m_subHoverIndex > 0) {
					if (e.button == 0) {
						Singleton<SimulationManager>.instance.AddAction(EndMoving());
					}
				} else if ((m_toolController.m_mode & ItemClass.Availability.MapAndAsset) != ItemClass.Availability.None) {
					if (e.button == 0) {
						Singleton<SimulationManager>.instance.AddAction(EndMoving());
					} else if (e.button == 1) {
						Singleton<SimulationManager>.instance.AddAction(EndRotating());
					}
				} else if ((m_toolController.m_mode & ItemClass.Availability.ScenarioEditor) != ItemClass.Availability.None) {
					if (e.button == 0) {
						Singleton<SimulationManager>.instance.AddAction(EndMoving());
						m_holdTimer = 0f;
					} else if (e.button == 1) {
						Singleton<SimulationManager>.instance.AddAction(EndRotating());
					}
				}
			}
			if (m_toolController.m_developerUI != null && m_toolController.m_developerUI.enabled && Cursor.visible) {
				InstanceID hoverInstance2 = m_hoverInstance;
				ushort building2 = hoverInstance2.Building;
				ushort vehicle = hoverInstance2.Vehicle;
				ushort parkedVehicle = hoverInstance2.ParkedVehicle;
				ushort citizenInstance = hoverInstance2.CitizenInstance;
				ushort transportLine = hoverInstance2.TransportLine;
				string text = null;
				if (building2 != 0) {
					BuildingManager instance = Singleton<BuildingManager>.instance;
					if ((instance.m_buildings.m_buffer[building2].m_flags & Building.Flags.Created) != Building.Flags.None) {
						BuildingInfo info2 = instance.m_buildings.m_buffer[(int)building2].Info;
						if (info2 != null) {
							text = StringUtils.SafeFormat("{0} ({1})", new object[]
							{
							info2.gameObject.name,
							building2
							});
							string debugString = info2.m_buildingAI.GetDebugString(building2, ref instance.m_buildings.m_buffer[(int)building2]);
							if (debugString != null) {
								text = text + "\n" + debugString;
							}
						}
					}
				} else if (vehicle != 0) {
					VehicleManager instance2 = Singleton<VehicleManager>.instance;
					if ((instance2.m_vehicles.m_buffer[(int)vehicle].m_flags & Vehicle.Flags.Created) != (Vehicle.Flags)0) {
						VehicleInfo info3 = instance2.m_vehicles.m_buffer[(int)vehicle].Info;
						if (info3 != null) {
							text = StringUtils.SafeFormat("{0} ({1})", new object[]
							{
							info3.gameObject.name,
							vehicle
							});
							string debugString2 = info3.m_vehicleAI.GetDebugString(vehicle, ref instance2.m_vehicles.m_buffer[(int)vehicle]);
							if (debugString2 != null) {
								text = text + "\n" + debugString2;
							}
						}
					}
				} else if (parkedVehicle != 0) {
					VehicleManager instance3 = Singleton<VehicleManager>.instance;
					if ((instance3.m_parkedVehicles.m_buffer[(int)parkedVehicle].m_flags & 1) != 0) {
						VehicleInfo info4 = instance3.m_parkedVehicles.m_buffer[(int)parkedVehicle].Info;
						if (info4 != null) {
							text = StringUtils.SafeFormat("{0} ({1})", new object[]
							{
							info4.gameObject.name,
							parkedVehicle
							});
							string debugString3 = info4.m_vehicleAI.GetDebugString(parkedVehicle, ref instance3.m_parkedVehicles.m_buffer[(int)parkedVehicle]);
							if (debugString3 != null) {
								text = text + "\n" + debugString3;
							}
						}
					}
				} else if (citizenInstance != 0) {
					CitizenManager instance4 = Singleton<CitizenManager>.instance;
					if ((instance4.m_instances.m_buffer[(int)citizenInstance].m_flags & CitizenInstance.Flags.Created) != CitizenInstance.Flags.None) {
						CitizenInfo info5 = instance4.m_instances.m_buffer[(int)citizenInstance].Info;
						if (info5 != null) {
							text = StringUtils.SafeFormat("{0} ({1})", new object[]
							{
							info5.gameObject.name,
							citizenInstance
							});
							string debugString4 = info5.m_citizenAI.GetDebugString(citizenInstance, ref instance4.m_instances.m_buffer[(int)citizenInstance]);
							if (debugString4 != null) {
								text = text + "\n" + debugString4;
							}
						}
					}
				} else if (transportLine != 0) {
					TransportManager instance5 = Singleton<TransportManager>.instance;
					if ((instance5.m_lines.m_buffer[(int)transportLine].m_flags & TransportLine.Flags.Created) != TransportLine.Flags.None) {
						TransportInfo info6 = instance5.m_lines.m_buffer[(int)transportLine].Info;
						if (info6 != null) {
							text = StringUtils.SafeFormat("{0} ({1})", new object[]
							{
							info6.gameObject.name,
							transportLine
							});
							string debugString5 = instance5.m_lines.m_buffer[(int)transportLine].GetDebugString(transportLine);
							if (debugString5 != null) {
								text = text + "\n" + debugString5;
							}
						}
					}
				}
				if (text != null) {
					Vector3 mousePosition2;
					Quaternion quaternion;
					Vector3 vector;
					if (!InstanceManager.GetPosition(hoverInstance2, out mousePosition2, out quaternion, out vector)) {
						mousePosition2 = this.m_mousePosition;
					}
					Vector3 vector2 = Camera.main.WorldToScreenPoint(mousePosition2);
					vector2.y = (float)Screen.height - vector2.y;
					Color color = GUI.color;
					GUI.color = Color.cyan;
					DeveloperUI.LabelOutline(new Rect(vector2.x, vector2.y, 500f, 500f), text, Color.black, Color.cyan, GUI.skin.label, 2f);
					GUI.color = color;
				}
			}
		}

		protected override void OnEnable() {
			base.OnEnable();
			this.m_toolController.ClearColliding();
			this.m_selectErrors = ToolBase.ToolErrors.Pending;
			this.m_accuratePositionValid = false;
			this.m_freneticPlayer = 0;
			this.m_holdTimer = 0f;
		}

		protected override void OnDisable() {
			base.OnDisable();
			base.ToolCursor = null;
			this.m_mouseRayValid = false;
			this.m_selectErrors = ToolBase.ToolErrors.Pending;
			this.m_accuratePositionValid = false;
			this.m_freneticPlayer = 0;
			this.m_holdTimer = 0f;
			Singleton<SimulationManager>.instance.AddAction(this.DisableTool());
		}

		public override void RenderGeometry(RenderManager.CameraInfo cameraInfo) {
			if ((m_toolController.m_mode & ItemClass.Availability.MapAndAsset) != ItemClass.Availability.None) {
				InstanceID hoverInstance = this.m_hoverInstance;
				if (!hoverInstance.IsEmpty && (this.m_mouseLeftDown || this.m_mouseRightDown) && this.m_selectErrors == ToolBase.ToolErrors.None && !this.m_toolController.IsInsideUI && Cursor.visible) {
					if (hoverInstance.Prop != 0) {
						PropInfo info = Singleton<PropManager>.instance.m_props.m_buffer[(int)hoverInstance.Prop].Info;
						if (info != null) {
							Randomizer randomizer = new Randomizer((int)hoverInstance.Prop);
							float scale = info.m_minScale + (float)randomizer.Int32(10000u) * (info.m_maxScale - info.m_minScale) * 0.0001f;
							Color color = info.GetColor(ref randomizer);
							if (info.m_requireHeightMap) {
								Texture heightMap;
								Vector4 heightMapping;
								Vector4 surfaceMapping;
								Singleton<TerrainManager>.instance.GetHeightMapping(this.m_mousePosition, out heightMap, out heightMapping, out surfaceMapping);
								PropInstance.RenderInstance(cameraInfo, info, hoverInstance, this.m_mousePosition, scale, this.m_angle * 0.0174532924f, color, RenderManager.DefaultColorLocation, true, heightMap, heightMapping, surfaceMapping);
							} else {
								PropInstance.RenderInstance(cameraInfo, info, hoverInstance, this.m_mousePosition, scale, this.m_angle * 0.0174532924f, color, RenderManager.DefaultColorLocation, true);
							}
						}
					} else if (hoverInstance.Tree != 0u) {
						TreeInfo info2 = Singleton<TreeManager>.instance.m_trees.m_buffer[(int)((UIntPtr)hoverInstance.Tree)].Info;
						if (info2 != null) {
							Randomizer randomizer2 = new Randomizer(hoverInstance.Tree);
							float scale2 = info2.m_minScale + (float)randomizer2.Int32(10000u) * (info2.m_maxScale - info2.m_minScale) * 0.0001f;
							float brightness = info2.m_minBrightness + (float)randomizer2.Int32(10000u) * (info2.m_maxBrightness - info2.m_minBrightness) * 0.0001f;
							global::TreeInstance.RenderInstance(null, info2, this.m_mousePosition, scale2, brightness, RenderManager.DefaultColorLocation);
						}
					} else if (hoverInstance.Building != 0) {
						BuildingInfo info3 = Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)hoverInstance.Building].Info;
						Building building = default(Building);
						building.m_position = this.m_mousePosition;
						building.m_angle = this.m_angle * 0.0174532924f;
						this.m_toolController.RenderCollidingNotifications(cameraInfo, 0, 0);
						float elevation = 0f;
						Color color2 = info3.m_buildingAI.GetColor(0, ref building, Singleton<InfoManager>.instance.CurrentMode);
						info3.m_buildingAI.RenderBuildGeometry(cameraInfo, this.m_mousePosition, this.m_angle * 0.0174532924f, elevation);
						BuildingTool.RenderGeometry(cameraInfo, info3, 0, this.m_mousePosition, this.m_angle * 0.0174532924f, true, color2);
						if (info3.m_subBuildings != null && info3.m_subBuildings.Length != 0) {
							Matrix4x4 matrix4x = default(Matrix4x4);
							matrix4x.SetTRS(this.m_mousePosition, Quaternion.AngleAxis(this.m_angle, Vector3.down), Vector3.one);
							for (int i = 0; i < info3.m_subBuildings.Length; i++) {
								BuildingInfo buildingInfo = info3.m_subBuildings[i].m_buildingInfo;
								Vector3 position = matrix4x.MultiplyPoint(info3.m_subBuildings[i].m_position);
								float angle = (info3.m_subBuildings[i].m_angle + this.m_angle) * 0.0174532924f;
								buildingInfo.m_buildingAI.RenderBuildGeometry(cameraInfo, position, angle, elevation);
								BuildingTool.RenderGeometry(cameraInfo, buildingInfo, 0, position, angle, true, color2);
							}
						}
					}
				}
			} else if ((this.m_toolController.m_mode & ItemClass.Availability.ScenarioEditor) != ItemClass.Availability.None) {
				InstanceID hoverInstance2 = this.m_hoverInstance;
				if (hoverInstance2.Disaster != 0 && (this.m_mouseLeftDown || this.m_mouseRightDown) && this.m_selectErrors == ToolBase.ToolErrors.None) {
					DisasterInfo info4 = Singleton<DisasterManager>.instance.m_disasters.m_buffer[(int)hoverInstance2.Disaster].Info;
					if (info4 != null) {
						DisasterTool.DrawMarker(info4, this.m_mousePosition, this.m_angle * 0.0174532924f);
					}
				}
			}
			if (this.EnableMouseLight() && this.m_accuratePositionValid && !this.m_toolController.IsInsideUI && Cursor.visible) {
				LightSystem lightSystem = Singleton<RenderManager>.instance.lightSystem;
				Vector3 a = this.m_accuratePosition - cameraInfo.m_position;
				float magnitude = a.magnitude;
				float num = Mathf.Sqrt(magnitude);
				float num2 = this.m_toolController.m_MouseLightIntensity.value;
				num *= 1f + num2 * 4f;
				num2 += num2 * num2 * num2 * 2f;
				num2 *= MathUtils.SmoothStep(0.9f, 0.1f, lightSystem.DayLightIntensity);
				Vector3 vector = a * (1f / Mathf.Max(1f, magnitude));
				Vector3 pos = this.m_accuratePosition - vector * (num * 0.2f);
				if (num2 > 0.001f) {
					lightSystem.DrawLight(LightType.Spot, pos, vector, Vector3.zero, Color.white, num2, num, 90f, 1f, false);
				}
			}
			base.RenderGeometry(cameraInfo);
		}

		protected virtual bool EnableMouseLight() {
			return Singleton<InfoManager>.instance.CurrentMode == InfoManager.InfoMode.None;
		}

		public override void RenderOverlay(RenderManager.CameraInfo cameraInfo) {
			InstanceID hoverInstance = this.m_hoverInstance;
			InstanceID hoverInstance2 = this.m_hoverInstance2;
			if ((this.m_toolController.m_mode & ItemClass.Availability.MapAndAsset) != ItemClass.Availability.None) {
				if (this.m_toolController.IsInsideUI || !Cursor.visible) {
					base.RenderOverlay(cameraInfo);
					return;
				}
				if (!hoverInstance.IsEmpty && (this.m_mouseLeftDown || this.m_mouseRightDown)) {
					if (hoverInstance.Prop != 0) {
						PropInfo info = Singleton<PropManager>.instance.m_props.m_buffer[(int)hoverInstance.Prop].Info;
						if (info != null) {
							Color toolColor = base.GetToolColor(false, this.m_selectErrors != ToolBase.ToolErrors.None);
							this.m_toolController.RenderColliding(cameraInfo, toolColor, toolColor, toolColor, toolColor, 0, 0);
							Randomizer randomizer = new Randomizer((int)hoverInstance.Prop);
							float scale = info.m_minScale + (float)randomizer.Int32(10000u) * (info.m_maxScale - info.m_minScale) * 0.0001f;
							PropTool.RenderOverlay(cameraInfo, info, this.m_mousePosition, scale, this.m_angle, toolColor);
						}
					} else if (hoverInstance.Tree != 0u) {
						TreeInfo info2 = Singleton<TreeManager>.instance.m_trees.m_buffer[(int)((UIntPtr)hoverInstance.Tree)].Info;
						if (info2 != null) {
							Color toolColor2 = base.GetToolColor(false, this.m_selectErrors != ToolBase.ToolErrors.None);
							this.m_toolController.RenderColliding(cameraInfo, toolColor2, toolColor2, toolColor2, toolColor2, 0, 0);
							Randomizer randomizer2 = new Randomizer(hoverInstance.Tree);
							float scale2 = info2.m_minScale + (float)randomizer2.Int32(10000u) * (info2.m_maxScale - info2.m_minScale) * 0.0001f;
							TreeTool.RenderOverlay(cameraInfo, info2, this.m_mousePosition, scale2, toolColor2);
						}
					} else if (hoverInstance.Building != 0) {
						BuildingInfo info3 = Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)hoverInstance.Building].Info;
						Color toolColor3 = base.GetToolColor(false, this.m_selectErrors != ToolBase.ToolErrors.None);
						Color toolColor4 = base.GetToolColor(true, false);
						this.m_toolController.RenderColliding(cameraInfo, toolColor3, toolColor4, toolColor3, toolColor4, 0, 0);
						info3.m_buildingAI.RenderBuildOverlay(cameraInfo, toolColor3, this.m_mousePosition, this.m_angle * 0.0174532924f, default(Segment3));
						BuildingTool.RenderOverlay(cameraInfo, info3, 0, this.m_mousePosition, this.m_angle * 0.0174532924f, toolColor3, true);
						if (info3.m_subBuildings != null && info3.m_subBuildings.Length != 0) {
							Matrix4x4 matrix4x = default(Matrix4x4);
							matrix4x.SetTRS(this.m_mousePosition, Quaternion.AngleAxis(this.m_angle, Vector3.down), Vector3.one);
							for (int i = 0; i < info3.m_subBuildings.Length; i++) {
								BuildingInfo buildingInfo = info3.m_subBuildings[i].m_buildingInfo;
								Vector3 position = matrix4x.MultiplyPoint(info3.m_subBuildings[i].m_position);
								float angle = (info3.m_subBuildings[i].m_angle + this.m_angle) * 0.0174532924f;
								buildingInfo.m_buildingAI.RenderBuildOverlay(cameraInfo, toolColor3, position, angle, default(Segment3));
								BuildingTool.RenderOverlay(cameraInfo, buildingInfo, 0, position, angle, toolColor3, true);
							}
						}
					}
					base.RenderOverlay(cameraInfo);
					return;
				}
			} else if ((this.m_toolController.m_mode & ItemClass.Availability.ScenarioEditor) != ItemClass.Availability.None && hoverInstance.Disaster != 0 && (this.m_mouseLeftDown || this.m_mouseRightDown)) {
				DisasterInfo info4 = Singleton<DisasterManager>.instance.m_disasters.m_buffer[(int)hoverInstance.Disaster].Info;
				if (info4 != null) {
					Color toolColor5 = base.GetToolColor(false, this.m_selectErrors != ToolBase.ToolErrors.None);
					DisasterTool.RenderOverlay(cameraInfo, info4, this.m_mousePosition, this.m_angle * 0.0174532924f, toolColor5);
				}
				base.RenderOverlay(cameraInfo);
				return;
			}
			if (this.m_toolController.IsInsideUI || !Cursor.visible) {
				base.RenderOverlay(cameraInfo);
				return;
			}
			switch (hoverInstance.Type) {
			case InstanceType.Building: {
				ushort building = hoverInstance.Building;
				NetManager instance = Singleton<NetManager>.instance;
				BuildingManager instance2 = Singleton<BuildingManager>.instance;
				BuildingInfo info5 = instance2.m_buildings.m_buffer[(int)building].Info;
				Color toolColor6 = base.GetToolColor(false, this.m_selectErrors != ToolBase.ToolErrors.None);
				float num = 1f;
				BuildingTool.CheckOverlayAlpha(info5, ref num);
				ushort num2 = instance2.m_buildings.m_buffer[(int)building].m_netNode;
				int num3 = 0;
				while (num2 != 0) {
					for (int j = 0; j < 8; j++) {
						ushort segment = instance.m_nodes.m_buffer[(int)num2].GetSegment(j);
						if (segment != 0 && instance.m_segments.m_buffer[(int)segment].m_startNode == num2 && (instance.m_segments.m_buffer[(int)segment].m_flags & NetSegment.Flags.Untouchable) != NetSegment.Flags.None) {
							NetTool.CheckOverlayAlpha(ref instance.m_segments.m_buffer[(int)segment], ref num);
						}
					}
					num2 = instance.m_nodes.m_buffer[(int)num2].m_nextBuildingNode;
					if (++num3 > 32768) {
						CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
						break;
					}
				}
				ushort subBuilding = instance2.m_buildings.m_buffer[(int)building].m_subBuilding;
				num3 = 0;
				while (subBuilding != 0) {
					BuildingTool.CheckOverlayAlpha(instance2.m_buildings.m_buffer[(int)subBuilding].Info, ref num);
					subBuilding = instance2.m_buildings.m_buffer[(int)subBuilding].m_subBuilding;
					if (++num3 > 49152) {
						CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
						break;
					}
				}
				toolColor6.a *= num;
				int length = instance2.m_buildings.m_buffer[(int)building].Length;
				Vector3 position2 = instance2.m_buildings.m_buffer[(int)building].m_position;
				float angle2 = instance2.m_buildings.m_buffer[(int)building].m_angle;
				BuildingTool.RenderOverlay(cameraInfo, info5, length, position2, angle2, toolColor6, false);
				num2 = instance2.m_buildings.m_buffer[(int)building].m_netNode;
				num3 = 0;
				while (num2 != 0) {
					for (int k = 0; k < 8; k++) {
						ushort segment2 = instance.m_nodes.m_buffer[(int)num2].GetSegment(k);
						if (segment2 != 0 && instance.m_segments.m_buffer[(int)segment2].m_startNode == num2 && (instance.m_segments.m_buffer[(int)segment2].m_flags & NetSegment.Flags.Untouchable) != NetSegment.Flags.None) {
							NetTool.RenderOverlay(cameraInfo, ref instance.m_segments.m_buffer[(int)segment2], toolColor6, toolColor6);
						}
					}
					num2 = instance.m_nodes.m_buffer[(int)num2].m_nextBuildingNode;
					if (++num3 > 32768) {
						CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
						break;
					}
				}
				subBuilding = instance2.m_buildings.m_buffer[(int)building].m_subBuilding;
				num3 = 0;
				while (subBuilding != 0) {
					BuildingInfo info6 = instance2.m_buildings.m_buffer[(int)subBuilding].Info;
					int length2 = instance2.m_buildings.m_buffer[(int)subBuilding].Length;
					Vector3 position3 = instance2.m_buildings.m_buffer[(int)subBuilding].m_position;
					float angle3 = instance2.m_buildings.m_buffer[(int)subBuilding].m_angle;
					BuildingTool.RenderOverlay(cameraInfo, info6, length2, position3, angle3, toolColor6, false);
					subBuilding = instance2.m_buildings.m_buffer[(int)subBuilding].m_subBuilding;
					if (++num3 > 49152) {
						CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
						break;
					}
				}
				break;
			}
			case InstanceType.Vehicle: {
				ushort vehicle = hoverInstance.Vehicle;
				VehicleManager instance3 = Singleton<VehicleManager>.instance;
				Color toolColor7 = base.GetToolColor(false, this.m_selectErrors != ToolBase.ToolErrors.None);
				float num4 = 1f;
				instance3.m_vehicles.m_buffer[(int)vehicle].CheckOverlayAlpha(ref num4);
				toolColor7.a *= num4;
				instance3.m_vehicles.m_buffer[(int)vehicle].RenderOverlay(cameraInfo, vehicle, toolColor7);
				break;
			}
			case InstanceType.District: {
				byte district = hoverInstance.District;
				if (Singleton<InfoManager>.instance.CurrentMode != InfoManager.InfoMode.Districts) {
					DistrictManager instance4 = Singleton<DistrictManager>.instance;
					Color toolColor8 = base.GetToolColor(false, this.m_selectErrors != ToolBase.ToolErrors.None);
					float num5 = 1f;
					instance4.CheckOverlayAlpha(cameraInfo, district, ref num5);
					toolColor8.a *= num5;
					instance4.RenderHighlight(cameraInfo, district, toolColor8);
				}
				break;
			}
			case InstanceType.NetSegment: {
				ushort netSegment = hoverInstance.NetSegment;
				ushort netSegment2 = hoverInstance2.NetSegment;
				NetManager instance5 = Singleton<NetManager>.instance;
				Color toolColor9 = base.GetToolColor(false, this.m_selectErrors != ToolBase.ToolErrors.None);
				float num6 = 1f;
				bool flag;
				this.GetSegmentIgnoreFlags(out flag);
				if (flag) {
					RenderManager instance6 = Singleton<RenderManager>.instance;
					uint num7;
					if (instance6.GetInstanceIndex((uint)(49152 + netSegment), out num7)) {
						InstanceManager.NameData nameData = instance6.m_instances[(int)((UIntPtr)num7)].m_nameData;
						Vector3 position4 = instance6.m_instances[(int)((UIntPtr)num7)].m_position;
						Matrix4x4 dataMatrix = instance6.m_instances[(int)((UIntPtr)num7)].m_dataMatrix2;
						float num8 = Vector3.Distance(position4, cameraInfo.m_position);
						if (nameData != null && num8 < 1000f) {
							NetInfo info7 = instance5.m_segments.m_buffer[(int)netSegment].Info;
							Bezier3 bezier = default(Bezier3);
							bezier.a = instance5.m_nodes.m_buffer[(int)instance5.m_segments.m_buffer[(int)netSegment].m_startNode].m_position;
							bezier.d = instance5.m_nodes.m_buffer[(int)instance5.m_segments.m_buffer[(int)netSegment].m_endNode].m_position;
							float snapElevation = info7.m_netAI.GetSnapElevation();
							bezier.a.y = bezier.a.y + snapElevation;
							bezier.d.y = bezier.d.y + snapElevation;
							NetSegment.CalculateMiddlePoints(bezier.a, instance5.m_segments.m_buffer[(int)netSegment].m_startDirection, bezier.d, instance5.m_segments.m_buffer[(int)netSegment].m_endDirection, true, true, out bezier.b, out bezier.c);
							float num9 = Mathf.Max(1f, Mathf.Abs(dataMatrix.m33 - dataMatrix.m30));
							float d = num8 * 0.0002f + 0.05f / (1f + num8 * 0.001f);
							Vector2 a = nameData.m_size;
							a.x += 20f;
							a.y += 10f;
							a *= d;
							float t = Mathf.Max(0f, 0.5f - a.x / num9 * 0.5f);
							float t2 = Mathf.Min(1f, 0.5f + a.x / num9 * 0.5f);
							bezier = bezier.Cut(t, t2);
							float minY = Mathf.Min(bezier.a.y, bezier.d.y);
							float maxY = Mathf.Max(bezier.a.y, bezier.d.y);
							float f = Mathf.Min(a.x + 10f, a.y + 10f) * 0.5f;
							num6 = Mathf.Min(num6, 2f / Mathf.Max(1f, Mathf.Sqrt(f)));
							toolColor9.a *= num6;
							ToolManager expr_C11_cp_0 = Singleton<ToolManager>.instance;
							expr_C11_cp_0.m_drawCallData.m_overlayCalls = expr_C11_cp_0.m_drawCallData.m_overlayCalls + 1;
							Singleton<RenderManager>.instance.OverlayEffect.DrawBezier(cameraInfo, toolColor9, bezier, a.y, a.y * 0.5f, a.y * 0.5f, minY, maxY, true, true);
						}
					}
				} else {
					NetTool.CheckOverlayAlpha(ref instance5.m_segments.m_buffer[(int)netSegment], ref num6);
					if (netSegment2 != 0) {
						NetTool.CheckOverlayAlpha(ref instance5.m_segments.m_buffer[(int)netSegment2], ref num6);
					}
					toolColor9.a *= num6;
					if (!(instance5.NetAdjust != null) || !instance5.NetAdjust.RenderOverlay(cameraInfo, netSegment, toolColor9, this.m_subHoverIndex)) {
						NetTool.RenderOverlay(cameraInfo, ref instance5.m_segments.m_buffer[(int)netSegment], toolColor9, toolColor9);
						if (netSegment2 != 0) {
							NetTool.RenderOverlay(cameraInfo, ref instance5.m_segments.m_buffer[(int)netSegment2], toolColor9, toolColor9);
						}
					}
				}
				break;
			}
			case InstanceType.ParkedVehicle: {
				ushort parkedVehicle = hoverInstance.ParkedVehicle;
				VehicleManager instance7 = Singleton<VehicleManager>.instance;
				Color toolColor10 = base.GetToolColor(false, this.m_selectErrors != ToolBase.ToolErrors.None);
				float num10 = 1f;
				instance7.m_parkedVehicles.m_buffer[(int)parkedVehicle].CheckOverlayAlpha(ref num10);
				toolColor10.a *= num10;
				instance7.m_parkedVehicles.m_buffer[(int)parkedVehicle].RenderOverlay(cameraInfo, parkedVehicle, toolColor10);
				break;
			}
			case InstanceType.CitizenInstance: {
				ushort citizenInstance = hoverInstance.CitizenInstance;
				CitizenManager instance8 = Singleton<CitizenManager>.instance;
				Color toolColor11 = base.GetToolColor(false, this.m_selectErrors != ToolBase.ToolErrors.None);
				float num11 = 1f;
				instance8.m_instances.m_buffer[(int)citizenInstance].CheckOverlayAlpha(ref num11);
				toolColor11.a *= num11;
				instance8.m_instances.m_buffer[(int)citizenInstance].RenderOverlay(cameraInfo, citizenInstance, toolColor11);
				break;
			}
			case InstanceType.Prop: {
				ushort prop = hoverInstance.Prop;
				PropManager instance9 = Singleton<PropManager>.instance;
				PropInfo info8 = instance9.m_props.m_buffer[(int)prop].Info;
				Vector3 position5 = instance9.m_props.m_buffer[(int)prop].Position;
				float angle4 = instance9.m_props.m_buffer[(int)prop].Angle;
				Randomizer randomizer3 = new Randomizer((int)prop);
				float scale3 = info8.m_minScale + (float)randomizer3.Int32(10000u) * (info8.m_maxScale - info8.m_minScale) * 0.0001f;
				Color toolColor12 = base.GetToolColor(false, this.m_selectErrors != ToolBase.ToolErrors.None);
				float num12 = 1f;
				PropTool.CheckOverlayAlpha(info8, scale3, ref num12);
				toolColor12.a *= num12;
				PropTool.RenderOverlay(cameraInfo, info8, position5, scale3, angle4, toolColor12);
				break;
			}
			case InstanceType.Tree: {
				uint tree = hoverInstance.Tree;
				TreeManager instance10 = Singleton<TreeManager>.instance;
				TreeInfo info9 = instance10.m_trees.m_buffer[(int)((UIntPtr)tree)].Info;
				Vector3 position6 = instance10.m_trees.m_buffer[(int)((UIntPtr)tree)].Position;
				Randomizer randomizer4 = new Randomizer(tree);
				float scale4 = info9.m_minScale + (float)randomizer4.Int32(10000u) * (info9.m_maxScale - info9.m_minScale) * 0.0001f;
				Color toolColor13 = base.GetToolColor(false, this.m_selectErrors != ToolBase.ToolErrors.None);
				float num13 = 1f;
				TreeTool.CheckOverlayAlpha(info9, scale4, ref num13);
				toolColor13.a *= num13;
				TreeTool.RenderOverlay(cameraInfo, info9, position6, scale4, toolColor13);
				break;
			}
			case InstanceType.Disaster: {
				ushort disaster = hoverInstance.Disaster;
				DisasterManager instance11 = Singleton<DisasterManager>.instance;
				DisasterInfo info10 = instance11.m_disasters.m_buffer[(int)disaster].Info;
				Vector3 targetPosition = instance11.m_disasters.m_buffer[(int)disaster].m_targetPosition;
				float angle5 = instance11.m_disasters.m_buffer[(int)disaster].m_angle;
				Color toolColor14 = base.GetToolColor(false, this.m_selectErrors != ToolBase.ToolErrors.None);
				float num14 = 1f;
				DisasterTool.CheckOverlayAlpha(info10, ref num14);
				toolColor14.a *= num14;
				DisasterTool.RenderOverlay(cameraInfo, info10, targetPosition, angle5, toolColor14);
				break;
			}
			case InstanceType.Park: {
				byte park = hoverInstance.Park;
				if (Singleton<InfoManager>.instance.CurrentMode != InfoManager.InfoMode.Districts) {
					DistrictManager instance12 = Singleton<DistrictManager>.instance;
					Color toolColor15 = base.GetToolColor(false, this.m_selectErrors != ToolBase.ToolErrors.None);
					float num15 = 1f;
					instance12.CheckParkOverlayAlpha(cameraInfo, park, ref num15);
					toolColor15.a *= num15;
					instance12.RenderParkHighlight(cameraInfo, park, toolColor15);
				}
				break;
			}
			}
			base.RenderOverlay(cameraInfo);
		}

		protected override void OnToolUpdate() {
			if (!m_toolController.IsInsideUI && Cursor.visible) {
				if ((Singleton<ToolManager>.instance.m_properties.m_mode & ItemClass.Availability.MapAndAsset) != ItemClass.Availability.None && (this.m_mouseLeftDown || this.m_mouseRightDown) && !this.m_hoverInstance.IsEmpty) {
					base.ShowToolInfo(true, null, this.m_mousePosition);
				} else {
					ushort building = this.m_hoverInstance.Building;
					if (building != 0) {
						BuildingManager instance = Singleton<BuildingManager>.instance;
						Vector3 position = instance.m_buildings.m_buffer[(int)building].m_position;
						base.ShowToolInfo(true, null, position);
					} else {
						base.ShowToolInfo(false, null, Vector3.zero);
					}
				}
			} else {
				base.ShowToolInfo(false, null, Vector3.zero);
			}
			if (this.m_mouseRightDown && !this.m_hoverInstance.IsEmpty) {
				float axis = Input.GetAxis("Mouse X");
				if (axis != 0f) {
					this.m_angleChanged = true;
					Singleton<SimulationManager>.instance.AddAction(this.DeltaAngle(axis * 10f));
				}
			}
			if (this.m_holdTimer > 0f) {
				this.m_holdTimer = Mathf.Max(0f, this.m_holdTimer - Time.deltaTime);
				if (this.m_holdTimer == 0f && this.m_hoverInstance.Disaster != 0) {
					this.m_selectErrors = ToolBase.ToolErrors.Pending;
					Singleton<SimulationManager>.instance.AddAction(this.StartMoving());
				}
			}
			CursorInfo cursorInfo = null;
			InfoManager.InfoMode currentMode = Singleton<InfoManager>.instance.CurrentMode;
			if (currentMode == InfoManager.InfoMode.Water || currentMode == InfoManager.InfoMode.Heating) {
				cursorInfo = this.m_undergroundCursor;
			}
			if (cursorInfo == null) {
				cursorInfo = this.m_cursor;
			}
			base.ToolCursor = cursorInfo;
		}

		protected override void OnToolLateUpdate() {
			if (!Singleton<LoadingManager>.instance.m_loadingComplete) {
				return;
			}
			byte district = this.m_hoverInstance.District;
			byte park = this.m_hoverInstance.Park;
			if (this.m_toolController.IsInsideUI || !Cursor.visible || district == 0) {
				Singleton<DistrictManager>.instance.HighlightDistrict = -1;
			} else {
				Singleton<DistrictManager>.instance.HighlightDistrict = (int)district;
			}
			if (this.m_toolController.IsInsideUI || !Cursor.visible || park == 0) {
				Singleton<DistrictManager>.instance.HighlightPark = -1;
			} else {
				Singleton<DistrictManager>.instance.HighlightPark = (int)park;
			}
			Vector3 mousePosition = Input.mousePosition;
			this.m_mouseRay = Camera.main.ScreenPointToRay(mousePosition);
			this.m_mouseRayLength = Camera.main.farClipPlane;
			this.m_rayRight = Camera.main.transform.TransformDirection(Vector3.right);
			if ((this.m_toolController.m_mode & ItemClass.Availability.ScenarioEditor) != ItemClass.Availability.None && this.m_hoverInstance.Disaster != 0 && (this.m_mouseLeftDown || this.m_mouseRightDown)) {
				this.m_mouseRayValid = true;
			} else {
				this.m_mouseRayValid = (!this.m_toolController.IsInsideUI && Cursor.visible);
			}
			ToolBase.OverrideInfoMode = false;
		}

		public void GetHoverInstance(out InstanceID id, out int subIndex) {
			id = this.m_hoverInstance;
			subIndex = this.m_subHoverIndex;
		}

		public void ChangeTarget(InstanceID oldID, InstanceID newID) {
			WorldInfoPanel.ChangeInstanceID(oldID, newID);
		}

		[DebuggerHidden]
		private IEnumerator StartMoving() {
			if (!this.m_mouseRightDown) {
				this.m_mouseLeftDown = true;
				ushort prop = this.m_hoverInstance.Prop;
				uint tree = this.m_hoverInstance.Tree;
				ushort building = this.m_hoverInstance.Building;
				ushort disaster = this.m_hoverInstance.Disaster;
				if (prop != 0) {
					this.m_angle = Singleton<PropManager>.instance.m_props.m_buffer[(int)prop].Angle * 57.29578f;
					Singleton<PropManager>.instance.m_props.m_buffer[(int)prop].Hidden = true;
				} else if (tree != 0u) {
					Singleton<TreeManager>.instance.m_trees.m_buffer[(int)((UIntPtr)tree)].Hidden = true;
					Singleton<TreeManager>.instance.UpdateTreeRenderer(tree, true);
				} else if (building != 0) {
					this.m_angle = Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)building].m_angle * 57.29578f;
					Building[] expr_14C_cp_0 = Singleton<BuildingManager>.instance.m_buildings.m_buffer;
					ushort expr_14C_cp_1 = building;
					expr_14C_cp_0[(int)expr_14C_cp_1].m_flags = (expr_14C_cp_0[(int)expr_14C_cp_1].m_flags | Building.Flags.Hidden);
					Singleton<BuildingManager>.instance.UpdateBuildingRenderer(building, true);
				} else if (disaster != 0) {
					this.m_angle = Singleton<DisasterManager>.instance.m_disasters.m_buffer[(int)disaster].m_angle * 57.29578f;
					DisasterData[] expr_1B7_cp_0 = Singleton<DisasterManager>.instance.m_disasters.m_buffer;
					ushort expr_1B7_cp_1 = disaster;
					expr_1B7_cp_0[(int)expr_1B7_cp_1].m_flags = (expr_1B7_cp_0[(int)expr_1B7_cp_1].m_flags | DisasterData.Flags.Hidden);
					ThreadHelper.dispatcher.Dispatch(delegate {
						DisasterWorldInfoPanel disasterWorldInfoPanel = UIView.library.Get<DisasterWorldInfoPanel>("DisasterWorldInfoPanel");
						if (disasterWorldInfoPanel != null) {
							disasterWorldInfoPanel.TempHide();
						}
					});
				}
			}
			yield return 0;
		}

		[DebuggerHidden]
		private IEnumerator StartRotating() {
			if (!this.m_mouseLeftDown) {
				this.m_mouseRightDown = true;
				this.m_angleChanged = false;
				ushort prop = this.m_hoverInstance.Prop;
				uint tree = this.m_hoverInstance.Tree;
				ushort building = this.m_hoverInstance.Building;
				ushort disaster = this.m_hoverInstance.Disaster;
				if (prop != 0) {
					this.m_angle = Singleton<PropManager>.instance.m_props.m_buffer[(int)prop].Angle * 57.29578f;
					Singleton<PropManager>.instance.m_props.m_buffer[(int)prop].Hidden = true;
				} else if (tree != 0u) {
					Singleton<TreeManager>.instance.m_trees.m_buffer[(int)((UIntPtr)tree)].Hidden = true;
					Singleton<TreeManager>.instance.UpdateTreeRenderer(tree, true);
				} else if (building != 0) {
					this.m_angle = Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)building].m_angle * 57.29578f;
					Building[] expr_158_cp_0 = Singleton<BuildingManager>.instance.m_buildings.m_buffer;
					ushort expr_158_cp_1 = building;
					expr_158_cp_0[(int)expr_158_cp_1].m_flags = (expr_158_cp_0[(int)expr_158_cp_1].m_flags | Building.Flags.Hidden);
					Singleton<BuildingManager>.instance.UpdateBuildingRenderer(building, true);
				} else if (disaster != 0) {
					this.m_angle = Singleton<DisasterManager>.instance.m_disasters.m_buffer[(int)disaster].m_angle * 57.29578f;
					DisasterData[] expr_1C3_cp_0 = Singleton<DisasterManager>.instance.m_disasters.m_buffer;
					ushort expr_1C3_cp_1 = disaster;
					expr_1C3_cp_0[(int)expr_1C3_cp_1].m_flags = (expr_1C3_cp_0[(int)expr_1C3_cp_1].m_flags | DisasterData.Flags.Hidden);
					ThreadHelper.dispatcher.Dispatch(delegate {
						DisasterWorldInfoPanel disasterWorldInfoPanel = UIView.library.Get<DisasterWorldInfoPanel>("DisasterWorldInfoPanel");
						if (disasterWorldInfoPanel != null) {
							disasterWorldInfoPanel.TempHide();
						}
					});
				}
			}
			yield return 0;
		}

		[DebuggerHidden]
		private IEnumerator EndMoving() {
			if (m_mouseLeftDown) {
				if (m_selectErrors == ToolBase.ToolErrors.None) {
					InstanceID id = this.m_hoverInstance;
					ushort prop = id.Prop;
					uint tree = id.Tree;
					ushort building = id.Building;
					ushort disaster = id.Disaster;
					ushort netSegment = id.NetSegment;
					int subHoverIndex = this.m_subHoverIndex;
					if (prop != 0) {
						Singleton<PropManager>.instance.MoveProp(prop, this.m_mousePosition);
						Singleton<PropManager>.instance.m_props.m_buffer[(int)prop].FixedHeight = this.m_fixedHeight;
					} else if (tree != 0u) {
						Singleton<TreeManager>.instance.MoveTree(tree, this.m_mousePosition);
						Singleton<TreeManager>.instance.m_trees.m_buffer[(int)((UIntPtr)tree)].FixedHeight = this.m_fixedHeight;
					} else if (building != 0) {
						Singleton<BuildingManager>.instance.RelocateBuilding(building, this.m_mousePosition, this.m_angle * 0.0174532924f);
					} else if (disaster != 0) {
						Singleton<DisasterManager>.instance.m_disasters.m_buffer[(int)disaster].m_targetPosition = this.m_mousePosition;
						ThreadHelper.dispatcher.Dispatch(delegate {
							DisasterWorldInfoPanel disasterWorldInfoPanel = UIView.library.Get<DisasterWorldInfoPanel>("DisasterWorldInfoPanel");
							if (disasterWorldInfoPanel != null) {
								disasterWorldInfoPanel.TempShow(id);
							}
						});
					} else if (netSegment != 0 && subHoverIndex > 0 && Singleton<NetManager>.instance.NetAdjust != null) {
						Singleton<NetManager>.instance.NetAdjust.ApplyModification(subHoverIndex);
					}
				}
				this.SetHoverInstance(InstanceID.Empty);
				this.m_subHoverIndex = 0;
				this.m_mouseLeftDown = false;
			}
			yield return 0;
		}

		[DebuggerHidden]
		private IEnumerator EndRotating() {
			if (this.m_mouseRightDown) {
				if (this.m_selectErrors == ToolBase.ToolErrors.None) {
					InstanceID id = this.m_hoverInstance;
					ushort prop = id.Prop;
					ushort building = id.Building;
					ushort disaster = id.Disaster;
					if (prop != 0) {
						if (!this.m_angleChanged) {
							this.m_angle = Mathf.Round(this.m_angle / 45f - 1f) * 45f;
							if (this.m_angle < 0f) {
								this.m_angle += 360f;
							}
							if (this.m_angle >= 360f) {
								this.m_angle -= 360f;
							}
						}
						Singleton<PropManager>.instance.m_props.m_buffer[(int)prop].Angle = this.m_angle * 0.0174532924f;
					} else if (building != 0) {
						if (!this.m_angleChanged) {
							this.m_angle = Mathf.Round(this.m_angle / 45f - 1f) * 45f;
							if (this.m_angle < 0f) {
								this.m_angle += 360f;
							}
							if (this.m_angle >= 360f) {
								this.m_angle -= 360f;
							}
						}
						Vector3 position = Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)building].m_position;
						Singleton<BuildingManager>.instance.RelocateBuilding(building, position, this.m_angle * 0.0174532924f);
					} else if (disaster != 0) {
						if (!this.m_angleChanged) {
							this.m_angle = Mathf.Round(this.m_angle / 45f - 1f) * 45f;
							if (this.m_angle < 0f) {
								this.m_angle += 360f;
							}
							if (this.m_angle >= 360f) {
								this.m_angle -= 360f;
							}
						}
						Singleton<DisasterManager>.instance.m_disasters.m_buffer[(int)disaster].m_angle = this.m_angle * 0.0174532924f;
						ThreadHelper.dispatcher.Dispatch(delegate {
							DisasterWorldInfoPanel disasterWorldInfoPanel = UIView.library.Get<DisasterWorldInfoPanel>("DisasterWorldInfoPanel");
							if (disasterWorldInfoPanel != null) {
								disasterWorldInfoPanel.TempShow(id);
							}
						});
					}
				}
				this.SetHoverInstance(InstanceID.Empty);
				this.m_subHoverIndex = 0;
				this.m_mouseRightDown = false;
			}
			yield return 0;
		}

		[DebuggerHidden]
		private IEnumerator DisableTool() {
			this.SetHoverInstance(InstanceID.Empty);
			this.SetHoverInstance2(InstanceID.Empty);
			this.m_subHoverIndex = 0;
			this.m_mouseLeftDown = false;
			this.m_mouseRightDown = false;
			yield return 0;
		}

		[DebuggerHidden]
		private IEnumerator DeltaAngle(float delta) {
			this.m_angle += delta;
			yield return 0;
		}

		public virtual bool GetTerrainIgnore() {
			return Singleton<InfoManager>.instance.CurrentMode != InfoManager.InfoMode.Districts && ((Singleton<ToolManager>.instance.m_properties.m_mode & ItemClass.Availability.MapAndAsset) == ItemClass.Availability.None || (!this.m_mouseLeftDown && !this.m_mouseRightDown) || this.m_hoverInstance.IsEmpty) && ((Singleton<ToolManager>.instance.m_properties.m_mode & ItemClass.Availability.ScenarioEditor) == ItemClass.Availability.None || (!this.m_mouseLeftDown && !this.m_mouseRightDown) || this.m_hoverInstance.Disaster == 0);
		}

		public virtual NetNode.Flags GetNodeIgnoreFlags() {
			if (Singleton<InfoManager>.instance.CurrentMode == InfoManager.InfoMode.TrafficRoutes && Singleton<InfoManager>.instance.CurrentSubMode == InfoManager.SubInfoMode.WaterPower) {
				return NetNode.Flags.None;
			}
			return NetNode.Flags.All;
		}

		public virtual NetSegment.Flags GetSegmentIgnoreFlags(out bool nameOnly) {
			if ((Singleton<ToolManager>.instance.m_properties.m_mode & ItemClass.Availability.GameAndMap) == ItemClass.Availability.None) {
				nameOnly = false;
				return NetSegment.Flags.All;
			}
			InfoManager.InfoMode currentMode = Singleton<InfoManager>.instance.CurrentMode;
			if (currentMode == InfoManager.InfoMode.Transport || currentMode == InfoManager.InfoMode.Traffic || currentMode == InfoManager.InfoMode.EscapeRoutes) {
				nameOnly = false;
				return NetSegment.Flags.All;
			}
			if (currentMode != InfoManager.InfoMode.TrafficRoutes) {
				if (Singleton<NetManager>.instance.m_roadNamesVisibleSetting) {
					nameOnly = true;
					return NetSegment.Flags.None;
				}
				nameOnly = false;
				return NetSegment.Flags.All;
			} else {
				nameOnly = false;
				if (Singleton<InfoManager>.instance.CurrentSubMode == InfoManager.SubInfoMode.WaterPower) {
					return NetSegment.Flags.All;
				}
				return NetSegment.Flags.None;
			}
		}

		public virtual Building.Flags GetBuildingIgnoreFlags() {
			if (Singleton<InfoManager>.instance.CurrentMode == InfoManager.InfoMode.Districts) {
				return Building.Flags.All;
			}
			if (Singleton<InfoManager>.instance.CurrentMode == InfoManager.InfoMode.TrafficRoutes && Singleton<InfoManager>.instance.CurrentSubMode != InfoManager.SubInfoMode.Default) {
				return Building.Flags.All;
			}
			if ((Singleton<ToolManager>.instance.m_properties.m_mode & ItemClass.Availability.Game) != ItemClass.Availability.None) {
				return Building.Flags.Original;
			}
			if ((Singleton<ToolManager>.instance.m_properties.m_mode & ItemClass.Availability.MapAndAsset) != ItemClass.Availability.None && ((!this.m_mouseLeftDown && !this.m_mouseRightDown) || this.m_hoverInstance.IsEmpty)) {
				return Building.Flags.None;
			}
			return Building.Flags.All;
		}

		public virtual global::TreeInstance.Flags GetTreeIgnoreFlags() {
			if ((Singleton<ToolManager>.instance.m_properties.m_mode & ItemClass.Availability.MapAndAsset) != ItemClass.Availability.None && ((!this.m_mouseLeftDown && !this.m_mouseRightDown) || this.m_hoverInstance.IsEmpty)) {
				return global::TreeInstance.Flags.None;
			}
			return global::TreeInstance.Flags.All;
		}

		public virtual PropInstance.Flags GetPropIgnoreFlags() {
			if ((Singleton<ToolManager>.instance.m_properties.m_mode & ItemClass.Availability.MapAndAsset) != ItemClass.Availability.None && ((!this.m_mouseLeftDown && !this.m_mouseRightDown) || this.m_hoverInstance.IsEmpty)) {
				return PropInstance.Flags.None;
			}
			return PropInstance.Flags.All;
		}

		public virtual Vehicle.Flags GetVehicleIgnoreFlags() {
			if ((Singleton<ToolManager>.instance.m_properties.m_mode & ItemClass.Availability.Game) == ItemClass.Availability.None) {
				return Vehicle.Flags.Created | Vehicle.Flags.Deleted | Vehicle.Flags.Spawned | Vehicle.Flags.Inverted | Vehicle.Flags.TransferToTarget | Vehicle.Flags.TransferToSource | Vehicle.Flags.Emergency1 | Vehicle.Flags.Emergency2 | Vehicle.Flags.WaitingPath | Vehicle.Flags.Stopped | Vehicle.Flags.Leaving | Vehicle.Flags.Arriving | Vehicle.Flags.Reversed | Vehicle.Flags.TakingOff | Vehicle.Flags.Flying | Vehicle.Flags.Landing | Vehicle.Flags.WaitingSpace | Vehicle.Flags.WaitingCargo | Vehicle.Flags.GoingBack | Vehicle.Flags.WaitingTarget | Vehicle.Flags.Importing | Vehicle.Flags.Exporting | Vehicle.Flags.Parking | Vehicle.Flags.CustomName | Vehicle.Flags.OnGravel | Vehicle.Flags.WaitingLoading | Vehicle.Flags.Congestion | Vehicle.Flags.DummyTraffic | Vehicle.Flags.Underground | Vehicle.Flags.Transition | Vehicle.Flags.InsideBuilding | Vehicle.Flags.LeftHandDrive;
			}
			if (Singleton<InfoManager>.instance.CurrentMode == InfoManager.InfoMode.Transport || Singleton<InfoManager>.instance.CurrentMode == InfoManager.InfoMode.Traffic || Singleton<InfoManager>.instance.CurrentMode == InfoManager.InfoMode.EscapeRoutes || Singleton<InfoManager>.instance.CurrentMode == InfoManager.InfoMode.Tours) {
				return (Vehicle.Flags)0;
			}
			if (Singleton<InfoManager>.instance.CurrentMode == InfoManager.InfoMode.TrafficRoutes && Singleton<InfoManager>.instance.CurrentSubMode != InfoManager.SubInfoMode.Default) {
				return Vehicle.Flags.Created | Vehicle.Flags.Deleted | Vehicle.Flags.Spawned | Vehicle.Flags.Inverted | Vehicle.Flags.TransferToTarget | Vehicle.Flags.TransferToSource | Vehicle.Flags.Emergency1 | Vehicle.Flags.Emergency2 | Vehicle.Flags.WaitingPath | Vehicle.Flags.Stopped | Vehicle.Flags.Leaving | Vehicle.Flags.Arriving | Vehicle.Flags.Reversed | Vehicle.Flags.TakingOff | Vehicle.Flags.Flying | Vehicle.Flags.Landing | Vehicle.Flags.WaitingSpace | Vehicle.Flags.WaitingCargo | Vehicle.Flags.GoingBack | Vehicle.Flags.WaitingTarget | Vehicle.Flags.Importing | Vehicle.Flags.Exporting | Vehicle.Flags.Parking | Vehicle.Flags.CustomName | Vehicle.Flags.OnGravel | Vehicle.Flags.WaitingLoading | Vehicle.Flags.Congestion | Vehicle.Flags.DummyTraffic | Vehicle.Flags.Underground | Vehicle.Flags.Transition | Vehicle.Flags.InsideBuilding | Vehicle.Flags.LeftHandDrive;
			}
			return Vehicle.Flags.Underground;
		}

		public virtual VehicleParked.Flags GetParkedVehicleIgnoreFlags() {
			if ((Singleton<ToolManager>.instance.m_properties.m_mode & ItemClass.Availability.Game) == ItemClass.Availability.None) {
				return VehicleParked.Flags.All;
			}
			if (Singleton<InfoManager>.instance.CurrentMode == InfoManager.InfoMode.TrafficRoutes && Singleton<InfoManager>.instance.CurrentSubMode != InfoManager.SubInfoMode.Default) {
				return VehicleParked.Flags.All;
			}
			return VehicleParked.Flags.Parking;
		}

		public virtual CitizenInstance.Flags GetCitizenIgnoreFlags() {
			if ((Singleton<ToolManager>.instance.m_properties.m_mode & ItemClass.Availability.Game) == ItemClass.Availability.None) {
				return CitizenInstance.Flags.All;
			}
			if (Singleton<InfoManager>.instance.CurrentMode == InfoManager.InfoMode.Transport || Singleton<InfoManager>.instance.CurrentMode == InfoManager.InfoMode.Traffic || Singleton<InfoManager>.instance.CurrentMode == InfoManager.InfoMode.EscapeRoutes || Singleton<InfoManager>.instance.CurrentMode == InfoManager.InfoMode.Tours) {
				return CitizenInstance.Flags.None;
			}
			if (Singleton<InfoManager>.instance.CurrentMode == InfoManager.InfoMode.TrafficRoutes && Singleton<InfoManager>.instance.CurrentSubMode != InfoManager.SubInfoMode.Default) {
				return CitizenInstance.Flags.All;
			}
			return CitizenInstance.Flags.Underground;
		}

		public virtual TransportLine.Flags GetTransportIgnoreFlags() {
			if (Singleton<InfoManager>.instance.CurrentMode == InfoManager.InfoMode.Transport || Singleton<InfoManager>.instance.CurrentMode == InfoManager.InfoMode.Tours) {
				return TransportLine.Flags.None;
			}
			return TransportLine.Flags.All;
		}

		public virtual int GetTransportTypes() {
			if (Singleton<InfoManager>.instance.CurrentMode == InfoManager.InfoMode.Transport) {
				return -3201;
			}
			if (Singleton<InfoManager>.instance.CurrentMode == InfoManager.InfoMode.EscapeRoutes) {
				return 128;
			}
			if (Singleton<InfoManager>.instance.CurrentMode == InfoManager.InfoMode.Tours) {
				return 3072;
			}
			return 0;
		}

		public virtual District.Flags GetDistrictIgnoreFlags() {
			if ((Singleton<ToolManager>.instance.m_properties.m_mode & ItemClass.Availability.Game) == ItemClass.Availability.None) {
				return District.Flags.All;
			}
			return District.Flags.None;
		}

		public virtual DistrictPark.Flags GetParkIgnoreFlags() {
			if ((Singleton<ToolManager>.instance.m_properties.m_mode & ItemClass.Availability.Game) == ItemClass.Availability.None) {
				return DistrictPark.Flags.All;
			}
			return DistrictPark.Flags.None;
		}

		public virtual DisasterData.Flags GetDisasterIgnoreFlags() {
			if ((Singleton<ToolManager>.instance.m_properties.m_mode & ItemClass.Availability.ScenarioEditor) != ItemClass.Availability.None && ((!this.m_mouseLeftDown && !this.m_mouseRightDown) || this.m_hoverInstance.Disaster == 0)) {
				return DisasterData.Flags.None;
			}
			return DisasterData.Flags.All;
		}

		public virtual ToolBase.RaycastService GetService() {
			ItemClass.Availability mode = Singleton<ToolManager>.instance.m_properties.m_mode;
			if ((mode & ItemClass.Availability.MapAndAsset) == ItemClass.Availability.None) {
				InfoManager.InfoMode currentMode = Singleton<InfoManager>.instance.CurrentMode;
				switch (currentMode) {
				case InfoManager.InfoMode.TrafficRoutes:
				case InfoManager.InfoMode.Tours:
					goto IL_DD;
				case InfoManager.InfoMode.Underground:
					if (Singleton<InfoManager>.instance.CurrentSubMode == InfoManager.SubInfoMode.Default) {
						return new ToolBase.RaycastService(ItemClass.Service.None, ItemClass.SubService.None, ItemClass.Layer.MetroTunnels);
					}
					return new ToolBase.RaycastService(ItemClass.Service.None, ItemClass.SubService.None, ItemClass.Layer.WaterPipes);
				case InfoManager.InfoMode.ParkMaintenance:
				case InfoManager.InfoMode.Tourism:
				case InfoManager.InfoMode.Post:
				case InfoManager.InfoMode.Industry:
					if (currentMode != InfoManager.InfoMode.Water) {
						if (currentMode == InfoManager.InfoMode.Transport) {
							return new ToolBase.RaycastService(ItemClass.Service.PublicTransport, ItemClass.SubService.None, ItemClass.Layer.Default | ItemClass.Layer.MetroTunnels | ItemClass.Layer.BlimpPaths | ItemClass.Layer.HelicopterPaths | ItemClass.Layer.FerryPaths);
						}
						if (currentMode == InfoManager.InfoMode.Traffic) {
							goto IL_DD;
						}
						if (currentMode != InfoManager.InfoMode.Heating) {
							return new ToolBase.RaycastService(ItemClass.Service.None, ItemClass.SubService.None, ItemClass.Layer.Default);
						}
					}
					return new ToolBase.RaycastService(ItemClass.Service.Water, ItemClass.SubService.None, ItemClass.Layer.Default | ItemClass.Layer.WaterPipes);
				case InfoManager.InfoMode.Fishing:
					return new ToolBase.RaycastService(ItemClass.Service.Fishing, ItemClass.SubService.None, ItemClass.Layer.Default | ItemClass.Layer.FishingPaths);
				}
IL_DD:
				return new ToolBase.RaycastService(ItemClass.Service.None, ItemClass.SubService.None, ItemClass.Layer.Default | ItemClass.Layer.MetroTunnels);
			}
			InfoManager.InfoMode currentMode2 = Singleton<InfoManager>.instance.CurrentMode;
			if (currentMode2 != InfoManager.InfoMode.Underground) {
				if (currentMode2 != InfoManager.InfoMode.Tours) {
					if (currentMode2 == InfoManager.InfoMode.Transport) {
						return new ToolBase.RaycastService(ItemClass.Service.None, ItemClass.SubService.None, ItemClass.Layer.Default | ItemClass.Layer.MetroTunnels | ItemClass.Layer.AirplanePaths | ItemClass.Layer.ShipPaths | ItemClass.Layer.Markers);
					}
					if (currentMode2 != InfoManager.InfoMode.Traffic) {
						return new ToolBase.RaycastService(ItemClass.Service.None, ItemClass.SubService.None, ItemClass.Layer.Default | ItemClass.Layer.Markers);
					}
				}
				return new ToolBase.RaycastService(ItemClass.Service.None, ItemClass.SubService.None, ItemClass.Layer.Default | ItemClass.Layer.MetroTunnels | ItemClass.Layer.Markers);
			}
			return new ToolBase.RaycastService(ItemClass.Service.None, ItemClass.SubService.None, ItemClass.Layer.MetroTunnels);
		}

		private static ToolBase.ToolErrors CheckPlacementErrors(BuildingInfo info, ref Vector3 position, ref float angle, bool fixedHeight, ushort id, ulong[] collidingSegmentBuffer, ulong[] collidingBuildingBuffer) {
			Segment3 segment = default(Segment3);
			int num = 0;
			int num2 = 0;
			float num3 = 0f;
			if ((Singleton<ToolManager>.instance.m_properties.m_mode & ItemClass.Availability.AssetEditor) != ItemClass.Availability.None) {
				return ToolBase.ToolErrors.None;
			}
			ToolBase.ToolErrors toolErrors;
			if (info.m_placementMode == BuildingInfo.PlacementMode.Shoreline || info.m_placementMode == BuildingInfo.PlacementMode.ShorelineOrGround) {
				Vector3 vector;
				Vector3 vector2;
				bool flag2;
				bool flag = BuildingTool.SnapToCanal(position, out vector, out vector2, out flag2, 40f, false);
				Vector3 vector3;
				Vector3 vector4;
				bool shorePos = Singleton<TerrainManager>.instance.GetShorePos(vector, 50f, out vector3, out vector4, out num3);
				if (flag) {
					position = vector;
					angle = Mathf.Atan2(vector2.x, -vector2.z);
					float num4 = Mathf.Max(0f, vector.y);
					float num5;
					float num6;
					float num7;
					Building.SampleBuildingHeight(position, angle, info.m_cellWidth, info.m_cellLength, info, out num5, out num6, out num7, ref num4);
					num5 -= 20f;
					num7 = Mathf.Max(position.y, num7);
					float y = position.y;
					position.y = num7;
					toolErrors = info.m_buildingAI.CheckBuildPosition(id, ref position, ref angle, num3, 0f, ref segment, out num, out num2);
					toolErrors |= BuildingTool.CheckSpace(info, BuildingInfo.PlacementMode.Shoreline, (int)id, position, num5, num7 + info.m_collisionHeight, angle, info.m_cellWidth, info.m_cellLength, true, collidingSegmentBuffer, collidingBuildingBuffer);
					if (y - num5 > 128f) {
						toolErrors |= ToolBase.ToolErrors.HeightTooHigh;
					}
				} else if (shorePos) {
					position = vector3;
					if (Singleton<TerrainManager>.instance.GetShorePos(position, 50f, out vector3, out vector4, out num3)) {
						vector3 += vector4.normalized * info.m_placementOffset;
						position = vector3;
						angle = Mathf.Atan2(vector4.x, -vector4.z);
						float num8;
						float num9;
						float num10;
						Building.SampleBuildingHeight(position, angle, info.m_cellWidth, info.m_cellLength, info, out num8, out num9, out num10);
						num8 = Mathf.Min(num3, num8);
						num10 = Mathf.Max(position.y, num10);
						float y2 = position.y;
						position.y = num10;
						toolErrors = info.m_buildingAI.CheckBuildPosition(id, ref position, ref angle, num3, 0f, ref segment, out num, out num2);
						toolErrors |= BuildingTool.CheckSpace(info, BuildingInfo.PlacementMode.Shoreline, (int)id, position, num8, num10 + info.m_collisionHeight, angle, info.m_cellWidth, info.m_cellLength, true, collidingSegmentBuffer, collidingBuildingBuffer);
						if (y2 - num3 > 128f) {
							toolErrors |= ToolBase.ToolErrors.HeightTooHigh;
						}
						if (num10 <= num3) {
							toolErrors = ((toolErrors & ~(ToolBase.ToolErrors.HeightTooHigh | ToolBase.ToolErrors.CannotConnect | ToolBase.ToolErrors.CannotBuildOnWater)) | ToolBase.ToolErrors.ShoreNotFound);
						}
					} else {
						toolErrors = info.m_buildingAI.CheckBuildPosition(id, ref position, ref angle, num3, 0f, ref segment, out num, out num2);
						toolErrors = ((toolErrors & ~(ToolBase.ToolErrors.HeightTooHigh | ToolBase.ToolErrors.CannotConnect | ToolBase.ToolErrors.CannotBuildOnWater)) | ToolBase.ToolErrors.ShoreNotFound);
					}
				} else if (info.m_placementMode == BuildingInfo.PlacementMode.ShorelineOrGround) {
					Quaternion rotation = Quaternion.AngleAxis(angle * 57.29578f, Vector3.down);
					position -= rotation * info.m_centerOffset;
					float num11;
					float num12;
					float num13;
					Building.SampleBuildingHeight(position, angle, info.m_cellWidth, info.m_cellLength, info, out num11, out num12, out num13);
					position.y = num13;
					toolErrors = info.m_buildingAI.CheckBuildPosition(id, ref position, ref angle, num3, 0f, ref segment, out num, out num2);
					toolErrors |= BuildingTool.CheckSpace(info, BuildingInfo.PlacementMode.OnGround, (int)id, position, num11, num13 + info.m_collisionHeight, angle, info.m_cellWidth, info.m_cellLength, true, collidingSegmentBuffer, collidingBuildingBuffer);
					if ((toolErrors & ToolBase.ToolErrors.CannotBuildOnWater) == ToolBase.ToolErrors.None && num12 - num11 > info.m_maxHeightOffset) {
						toolErrors |= ToolBase.ToolErrors.SlopeTooSteep;
					}
				} else {
					toolErrors = info.m_buildingAI.CheckBuildPosition(id, ref position, ref angle, num3, 0f, ref segment, out num, out num2);
					toolErrors = ((toolErrors & ~(ToolBase.ToolErrors.HeightTooHigh | ToolBase.ToolErrors.CannotConnect | ToolBase.ToolErrors.CannotBuildOnWater)) | ToolBase.ToolErrors.ShoreNotFound);
				}
			} else if (info.m_placementMode == BuildingInfo.PlacementMode.PathsideOrGround) {
				Vector3 a;
				Vector3 a2;
				if (BuildingTool.SnapToPath(position, out a, out a2, (float)Mathf.Min(info.m_cellWidth, info.m_cellLength) * 3.9f, info.m_hasPedestrianPaths)) {
					position = a - a2 * (float)info.m_cellLength * 4f;
					angle = Mathf.Atan2(-a2.x, a2.z);
					float num14;
					float num15;
					float num16;
					Building.SampleBuildingHeight(position, angle, info.m_cellWidth, info.m_cellLength, info, out num14, out num15, out num16);
					toolErrors = info.m_buildingAI.CheckBuildPosition(id, ref position, ref angle, num3, 0f, ref segment, out num, out num2);
					toolErrors |= BuildingTool.CheckSpace(info, BuildingInfo.PlacementMode.PathsideOrGround, (int)id, position, num14, position.y + info.m_collisionHeight, angle, info.m_cellWidth, info.m_cellLength, true, collidingSegmentBuffer, collidingBuildingBuffer);
					if (num15 - num14 > info.m_maxHeightOffset) {
						toolErrors |= ToolBase.ToolErrors.SlopeTooSteep;
					}
				} else {
					Quaternion rotation2 = Quaternion.AngleAxis(angle, Vector3.down);
					position -= rotation2 * info.m_centerOffset;
					float num17;
					float num18;
					float num19;
					Building.SampleBuildingHeight(position, angle, info.m_cellWidth, info.m_cellLength, info, out num17, out num18, out num19);
					position.y = num19;
					toolErrors = info.m_buildingAI.CheckBuildPosition(id, ref position, ref angle, num3, 0f, ref segment, out num, out num2);
					toolErrors |= BuildingTool.CheckSpace(info, BuildingInfo.PlacementMode.OnGround, (int)id, position, num17, num19 + info.m_collisionHeight, angle, info.m_cellWidth, info.m_cellLength, true, collidingSegmentBuffer, collidingBuildingBuffer);
					if (num18 - num17 > info.m_maxHeightOffset) {
						toolErrors |= ToolBase.ToolErrors.SlopeTooSteep;
					}
				}
			} else if (info.m_placementMode == BuildingInfo.PlacementMode.OnSurface || info.m_placementMode == BuildingInfo.PlacementMode.OnTerrain) {
				Quaternion rotation3 = Quaternion.AngleAxis(angle * 57.29578f, Vector3.down);
				position -= rotation3 * info.m_centerOffset;
				float minY;
				float num20;
				float num21;
				Building.SampleBuildingHeight(position, angle, info.m_cellWidth, info.m_cellLength, info, out minY, out num20, out num21);
				position.y = num21;
				toolErrors = info.m_buildingAI.CheckBuildPosition(id, ref position, ref angle, num3, 0f, ref segment, out num, out num2);
				toolErrors |= BuildingTool.CheckSpace(info, info.m_placementMode, (int)id, position, minY, num21 + info.m_collisionHeight, angle, info.m_cellWidth, info.m_cellLength, true, collidingSegmentBuffer, collidingBuildingBuffer);
			} else if (info.m_placementMode == BuildingInfo.PlacementMode.OnGround) {
				Quaternion rotation4 = Quaternion.AngleAxis(angle * 57.29578f, Vector3.down);
				position -= rotation4 * info.m_centerOffset;
				float num22;
				float num23;
				float num24;
				Building.SampleBuildingHeight(position, angle, info.m_cellWidth, info.m_cellLength, info, out num22, out num23, out num24);
				position.y = num24;
				toolErrors = info.m_buildingAI.CheckBuildPosition(id, ref position, ref angle, num3, 0f, ref segment, out num, out num2);
				toolErrors |= BuildingTool.CheckSpace(info, info.m_placementMode, (int)id, position, num22, num24 + info.m_collisionHeight, angle, info.m_cellWidth, info.m_cellLength, true, collidingSegmentBuffer, collidingBuildingBuffer);
				if (num23 - num22 > info.m_maxHeightOffset) {
					toolErrors |= ToolBase.ToolErrors.SlopeTooSteep;
				}
			} else if (info.m_placementMode == BuildingInfo.PlacementMode.OnWater) {
				Quaternion rotation5 = Quaternion.AngleAxis(angle * 57.29578f, Vector3.down);
				position -= rotation5 * info.m_centerOffset;
				float minY2;
				float num25;
				float num26;
				Building.SampleBuildingHeight(position, angle, info.m_cellWidth, info.m_cellLength, info, out minY2, out num25, out num26);
				position.y = num26;
				toolErrors = info.m_buildingAI.CheckBuildPosition(id, ref position, ref angle, num3, 0f, ref segment, out num, out num2);
				toolErrors |= BuildingTool.CheckSpace(info, info.m_placementMode, (int)id, position, minY2, num26 + info.m_collisionHeight, angle, info.m_cellWidth, info.m_cellLength, true, collidingSegmentBuffer, collidingBuildingBuffer);
			} else {
				toolErrors = ToolBase.ToolErrors.Pending;
				toolErrors |= info.m_buildingAI.CheckBuildPosition(id, ref position, ref angle, num3, 0f, ref segment, out num, out num2);
			}
			if (info.m_subBuildings != null && info.m_subBuildings.Length != 0) {
				Matrix4x4 matrix4x = default(Matrix4x4);
				matrix4x.SetTRS(position, Quaternion.AngleAxis(angle * 57.29578f, Vector3.down), Vector3.one);
				for (int i = 0; i < info.m_subBuildings.Length; i++) {
					BuildingInfo buildingInfo = info.m_subBuildings[i].m_buildingInfo;
					position = matrix4x.MultiplyPoint(info.m_subBuildings[i].m_position);
					float num27 = info.m_subBuildings[i].m_angle * 0.0174532924f + angle;
					Segment3 segment2 = default(Segment3);
					int num28;
					int num29;
					toolErrors |= buildingInfo.m_buildingAI.CheckBuildPosition(id, ref position, ref num27, num3, 0f, ref segment2, out num28, out num29);
					num2 += num29;
				}
			}
			return toolErrors;
		}

		public override void SimulationStep() {
			Ray mouseRay = m_mouseRay;
            ToolBase.RaycastInput input = new ToolBase.RaycastInput(mouseRay, this.m_mouseRayLength) {
                m_rayRight = m_rayRight,
                m_netService = GetService(),
                m_districtNameOnly = (Singleton<InfoManager>.instance.CurrentMode != InfoManager.InfoMode.Districts),
                m_ignoreTerrain = GetTerrainIgnore(),
                m_ignoreNodeFlags = GetNodeIgnoreFlags(),
				m_ignoreSegmentFlags = GetSegmentIgnoreFlags(out input.m_segmentNameOnly),
				m_ignoreBuildingFlags = GetBuildingIgnoreFlags(),
				m_ignoreTreeFlags = GetTreeIgnoreFlags(),
				m_ignorePropFlags = GetPropIgnoreFlags(),
				m_ignoreVehicleFlags = GetVehicleIgnoreFlags(),
				m_ignoreParkedVehicleFlags = GetParkedVehicleIgnoreFlags(),
				m_ignoreCitizenFlags = GetCitizenIgnoreFlags(),
				m_ignoreTransportFlags = GetTransportIgnoreFlags(),
				m_ignoreDistrictFlags = GetDistrictIgnoreFlags(),
				m_ignoreParkFlags = GetParkIgnoreFlags(),
				m_ignoreDisasterFlags = GetDisasterIgnoreFlags(),
				m_transportTypes = GetTransportTypes()
			};
			input.m_buildingService = input.m_netService;
			input.m_propService = input.m_netService;
			input.m_treeService = input.m_netService;
			if ((Singleton<ToolManager>.instance.m_properties.m_mode & ItemClass.Availability.AssetEditor) != ItemClass.Availability.None && (m_mouseLeftDown || m_mouseRightDown) && !m_hoverInstance.IsEmpty) {
				input.m_currentEditObject = true;
			}
			ushort num = 0;
			ToolBase.ToolErrors selectErrors = ToolBase.ToolErrors.None;
			EToolBase.RaycastOutput raycastOutput;
			if (m_mouseLeftDown && m_hoverInstance.NetSegment != 0 && m_subHoverIndex > 0) {
				if (Singleton<NetManager>.instance.NetAdjust != null && m_mouseRayValid) {
					input.m_ignoreNodeFlags = NetNode.Flags.None;
					if (EToolBase.RayCast(input, out raycastOutput) && raycastOutput.m_netSegment != 0 && raycastOutput.m_netNode != 0 &&
						Vector3.Distance(Singleton<NetManager>.instance.m_nodes.m_buffer[raycastOutput.m_netNode].m_position, raycastOutput.m_hitPos) >= 20f) {
						raycastOutput.m_netNode = 0;
					}
					Singleton<NetManager>.instance.NetAdjust.SetHoverAdjustPoint(m_subHoverIndex, raycastOutput.m_netSegment, raycastOutput.m_netNode);
				}
				m_selectErrors = selectErrors;
				return;
			}
			if ((Singleton<ToolManager>.instance.m_properties.m_mode & ItemClass.Availability.MapAndAsset) != ItemClass.Availability.None && (m_mouseLeftDown || m_mouseRightDown) && !m_hoverInstance.IsEmpty) {
                m_toolController.BeginColliding(out ulong[] collidingSegmentBuffer, out ulong[] collidingBuildingBuffer);
                try {
					if (m_mouseLeftDown) {
						if (m_mouseRayValid) {
							if (EToolBase.RayCast(input, out raycastOutput)) {
								if (m_hoverInstance.GetProp32() != 0) {
									PropInfo info = EPropManager.m_props.m_buffer[m_hoverInstance.GetProp32()].Info;
									//PropInfo info = Singleton<PropManager>.instance.m_props.m_buffer[m_hoverInstance.Prop].Info;
									selectErrors = EPropTool.CheckPlacementErrors(info, raycastOutput.m_hitPos, raycastOutput.m_currentEditObject, m_hoverInstance.GetProp32(), collidingSegmentBuffer, collidingBuildingBuffer);
								} else if (m_hoverInstance.Tree != 0u) {
									TreeInfo info2 = Singleton<TreeManager>.instance.m_trees.m_buffer[m_hoverInstance.Tree].Info;
									selectErrors = TreeTool.CheckPlacementErrors(info2, raycastOutput.m_hitPos, raycastOutput.m_currentEditObject, m_hoverInstance.Tree, collidingSegmentBuffer, collidingBuildingBuffer);
								} else if (m_hoverInstance.Building != 0) {
									BuildingInfo info3 = Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_hoverInstance.Building].Info;
									float num2 = m_angle * 0.0174532924f;
									selectErrors = CheckPlacementErrors(info3, ref raycastOutput.m_hitPos, ref num2, (Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_hoverInstance.Building].m_flags & Building.Flags.FixedHeight) != Building.Flags.None, this.m_hoverInstance.Building, collidingSegmentBuffer, collidingBuildingBuffer);
									if (num2 != m_angle * 0.0174532924f) {
										m_angle = num2 * 57.29578f;
									}
								}
							} else {
								selectErrors = ToolBase.ToolErrors.RaycastFailed;
							}
							m_mousePosition = raycastOutput.m_hitPos;
							m_accuratePosition = raycastOutput.m_hitPos;
							m_selectErrors = selectErrors;
							m_fixedHeight = raycastOutput.m_currentEditObject;
						} else {
							m_selectErrors = ToolBase.ToolErrors.RaycastFailed;
						}
					} else if (m_mouseRightDown) {
						if (m_hoverInstance.Building != 0) {
							BuildingInfo info4 = Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_hoverInstance.Building].Info;
							Vector3 position = Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_hoverInstance.Building].m_position;
							float num3 = m_angle * 0.0174532924f;
							selectErrors = CheckPlacementErrors(info4, ref position, ref num3, (Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_hoverInstance.Building].m_flags & Building.Flags.FixedHeight) != Building.Flags.None, this.m_hoverInstance.Building, collidingSegmentBuffer, collidingBuildingBuffer);
							if (num3 != m_angle * 0.0174532924f) {
								m_angle = num3 * 57.29578f;
							}
						}
						m_selectErrors = selectErrors;
					} else {
						m_selectErrors = selectErrors;
					}
				} finally {
					m_toolController.EndColliding();
				}
				return;
			}
			if ((Singleton<ToolManager>.instance.m_properties.m_mode & ItemClass.Availability.ScenarioEditor) != ItemClass.Availability.None && (m_mouseLeftDown || m_mouseRightDown) && m_hoverInstance.Disaster != 0) {
				if (m_mouseLeftDown) {
					if (m_mouseRayValid) {
						if (!EToolBase.RayCast(input, out raycastOutput)) {
							selectErrors = ToolBase.ToolErrors.RaycastFailed;
						}
						m_mousePosition = raycastOutput.m_hitPos;
						m_accuratePosition = raycastOutput.m_hitPos;
						m_selectErrors = selectErrors;
					} else {
						m_selectErrors = ToolBase.ToolErrors.RaycastFailed;
					}
				} else {
					m_selectErrors = selectErrors;
				}
				return;
			}
			if (m_mouseRayValid) {
				if (EToolBase.RayCast(input, out raycastOutput)) {
					m_accuratePosition = raycastOutput.m_hitPos;
					m_accuratePositionValid = true;
				} else {
					if (input.m_ignoreTerrain) {
						input = new ToolBase.RaycastInput(mouseRay, m_mouseRayLength);
						if (EToolBase.RayCast(input, out raycastOutput)) {
							m_accuratePosition = raycastOutput.m_hitPos;
							m_accuratePositionValid = true;
						} else {
							m_accuratePositionValid = false;
						}
					} else {
						m_accuratePositionValid = false;
					}
					selectErrors = ToolBase.ToolErrors.RaycastFailed;
				}
				if (input.m_ignoreNodeFlags == NetNode.Flags.All || raycastOutput.m_overlayButtonIndex == 0) {
					raycastOutput.m_netNode = 0;
				}
				if (raycastOutput.m_netSegment != 0 && (Singleton<NetManager>.instance.m_segments.m_buffer[raycastOutput.m_netSegment].m_flags & NetSegment.Flags.Untouchable) != NetSegment.Flags.None &&
					(this is BulldozeTool || (!input.m_segmentNameOnly && Singleton<InfoManager>.instance.CurrentMode != InfoManager.InfoMode.TrafficRoutes))) {
					raycastOutput.m_building = NetSegment.FindOwnerBuilding(raycastOutput.m_netSegment, 363f);
					raycastOutput.m_netSegment = 0;
				}
				if (raycastOutput.m_building != 0 && (Singleton<BuildingManager>.instance.m_buildings.m_buffer[raycastOutput.m_building].m_flags & Building.Flags.Untouchable) != Building.Flags.None) {
					raycastOutput.m_building = Building.FindParentBuilding(raycastOutput.m_building);
				}
				if (raycastOutput.m_citizenInstance != 0 && (Singleton<CitizenManager>.instance.m_instances.m_buffer[raycastOutput.m_citizenInstance].m_flags & CitizenInstance.Flags.RidingBicycle) != CitizenInstance.Flags.None) {
					uint citizen = Singleton<CitizenManager>.instance.m_instances.m_buffer[raycastOutput.m_citizenInstance].m_citizen;
					if (citizen != 0u) {
						ushort vehicle = Singleton<CitizenManager>.instance.m_citizens.m_buffer[citizen].m_vehicle;
						if (vehicle != 0) {
							raycastOutput.m_vehicle = vehicle;
							raycastOutput.m_citizenInstance = 0;
						}
					}
				}
				if (raycastOutput.m_netSegment != 0 && !(Singleton<NetManager>.instance.NetAdjust is null)) {
					int num4 = Singleton<NetManager>.instance.NetAdjust.CheckHoverSegment(ref raycastOutput.m_netSegment, raycastOutput.m_hitPos);
					if (num4 != 0) {
						raycastOutput.m_overlayButtonIndex = num4;
					}
				}
				num = FindSecondarySegment(raycastOutput.m_netSegment);
				if (raycastOutput.m_netNode != 0) {
					if (CheckNode(raycastOutput.m_netNode, ref selectErrors)) {
						raycastOutput.m_hitPos = Singleton<NetManager>.instance.m_nodes.m_buffer[raycastOutput.m_netNode].m_position;
					} else {
						raycastOutput.m_netNode = 0;
					}
				} else if (raycastOutput.m_netSegment != 0) {
					if (CheckSegment(raycastOutput.m_netSegment, ref selectErrors) && CheckSegment(num, ref selectErrors)) {
						raycastOutput.m_hitPos = Singleton<NetManager>.instance.m_segments.m_buffer[raycastOutput.m_netSegment].GetClosestPosition(raycastOutput.m_hitPos);
					} else {
						raycastOutput.m_netSegment = 0;
						num = 0;
					}
				} else if (raycastOutput.m_building != 0) {
					if (CheckBuilding(raycastOutput.m_building, ref selectErrors)) {
						raycastOutput.m_hitPos = Singleton<BuildingManager>.instance.m_buildings.m_buffer[raycastOutput.m_building].m_position;
					} else {
						raycastOutput.m_building = 0;
					}
				} else if (raycastOutput.m_propInstance != 0) {
					if (EDefaultTool.CheckProp(m_toolController, raycastOutput.m_propInstance)) {
						raycastOutput.m_hitPos = Singleton<PropManager>.instance.m_props.m_buffer[raycastOutput.m_propInstance].Position;
					} else {
						raycastOutput.m_propInstance = 0;
					}
				} else if (raycastOutput.m_treeInstance != 0u) {
					if (CheckTree(raycastOutput.m_treeInstance, ref selectErrors)) {
						raycastOutput.m_hitPos = Singleton<TreeManager>.instance.m_trees.m_buffer[(int)((UIntPtr)raycastOutput.m_treeInstance)].Position;
					} else {
						raycastOutput.m_treeInstance = 0u;
					}
				} else if (raycastOutput.m_vehicle != 0) {
					if (CheckVehicle(raycastOutput.m_vehicle, ref selectErrors)) {
						raycastOutput.m_hitPos = Singleton<VehicleManager>.instance.m_vehicles.m_buffer[raycastOutput.m_vehicle].GetLastFramePosition();
					} else {
						raycastOutput.m_vehicle = 0;
					}
				} else if (raycastOutput.m_parkedVehicle != 0) {
					if (CheckParkedVehicle(raycastOutput.m_parkedVehicle, ref selectErrors)) {
						raycastOutput.m_hitPos = Singleton<VehicleManager>.instance.m_parkedVehicles.m_buffer[raycastOutput.m_parkedVehicle].m_position;
					} else {
						raycastOutput.m_parkedVehicle = 0;
					}
				} else if (raycastOutput.m_citizenInstance != 0) {
					if (CheckCitizen(raycastOutput.m_citizenInstance, ref selectErrors)) {
						raycastOutput.m_hitPos = Singleton<CitizenManager>.instance.m_instances.m_buffer[raycastOutput.m_citizenInstance].GetLastFrameData().m_position;
					} else {
						raycastOutput.m_citizenInstance = 0;
					}
				} else if (raycastOutput.m_disaster != 0) {
					if (CheckDisaster(raycastOutput.m_disaster, ref selectErrors)) {
						raycastOutput.m_hitPos = Singleton<DisasterManager>.instance.m_disasters.m_buffer[raycastOutput.m_disaster].m_targetPosition;
					} else {
						raycastOutput.m_disaster = 0;
					}
				}
			} else {
				raycastOutput = default;
				selectErrors = ToolBase.ToolErrors.RaycastFailed;
				m_accuratePositionValid = false;
			}
			InstanceID empty = InstanceID.Empty;
			InstanceID empty2 = InstanceID.Empty;
			int overlayButtonIndex = raycastOutput.m_overlayButtonIndex;
			if (raycastOutput.m_netNode != 0) {
				empty.NetNode = raycastOutput.m_netNode;
			} else if (raycastOutput.m_netSegment != 0) {
				empty.NetSegment = raycastOutput.m_netSegment;
			} else if (raycastOutput.m_building != 0) {
				empty.Building = raycastOutput.m_building;
			} else if (raycastOutput.m_propInstance != 0) {
				empty.SetProp32(raycastOutput.m_propInstance);
			} else if (raycastOutput.m_treeInstance != 0u) {
				empty.Tree = raycastOutput.m_treeInstance;
			} else if (raycastOutput.m_vehicle != 0) {
				empty.Vehicle = raycastOutput.m_vehicle;
			} else if (raycastOutput.m_parkedVehicle != 0) {
				empty.ParkedVehicle = raycastOutput.m_parkedVehicle;
			} else if (raycastOutput.m_citizenInstance != 0) {
				empty.CitizenInstance = raycastOutput.m_citizenInstance;
			} else if (raycastOutput.m_district != 0) {
				empty.District = raycastOutput.m_district;
			} else if (raycastOutput.m_park != 0) {
				empty.Park = raycastOutput.m_park;
			} else if (raycastOutput.m_transportLine != 0) {
				empty.TransportLine = raycastOutput.m_transportLine;
			} else if (raycastOutput.m_disaster != 0) {
				empty.Disaster = raycastOutput.m_disaster;
			}
			if (num != 0) {
				empty2.NetSegment = num;
			}
			SetHoverInstance(empty);
			SetHoverInstance2(empty2);
			m_subHoverIndex = overlayButtonIndex;
			m_mousePosition = raycastOutput.m_hitPos;
			m_selectErrors = selectErrors;
		}

		public static ushort FindSecondarySegment(ushort segment) {
			if (segment == 0) {
				return 0;
			}
			NetManager instance = Singleton<NetManager>.instance;
			ushort num = instance.m_segments.m_buffer[(int)segment].m_startNode;
			if ((instance.m_nodes.m_buffer[(int)num].m_flags & NetNode.Flags.Double) == NetNode.Flags.None) {
				num = instance.m_segments.m_buffer[(int)segment].m_endNode;
				if ((instance.m_nodes.m_buffer[(int)num].m_flags & NetNode.Flags.Double) == NetNode.Flags.None) {
					return 0;
				}
			}
			for (int i = 0; i < 8; i++) {
				ushort segment2 = instance.m_nodes.m_buffer[(int)num].GetSegment(i);
				if (segment2 != 0 && segment2 != segment) {
					return segment2;
				}
			}
			return 0;
		}

		protected virtual bool CheckNode(ushort node, ref ToolBase.ToolErrors errors) {
			if ((this.m_toolController.m_mode & ItemClass.Availability.Editors) != ItemClass.Availability.None) {
				return true;
			}
			Vector3 position = Singleton<NetManager>.instance.m_nodes.m_buffer[(int)node].m_position;
			return !Singleton<GameAreaManager>.instance.PointOutOfArea(position);
		}

		protected virtual bool CheckSegment(ushort segment, ref ToolBase.ToolErrors errors) {
			return (this.m_toolController.m_mode & ItemClass.Availability.Editors) != ItemClass.Availability.None || !DefaultTool.IsOutOfCityArea(segment);
		}

		public static bool IsOutOfCityArea(ushort segment) {
			if (segment == 0) {
				return false;
			}
			NetManager instance = Singleton<NetManager>.instance;
			if ((instance.m_segments.m_buffer[(int)segment].m_flags & NetSegment.Flags.Original) == NetSegment.Flags.None) {
				return false;
			}
			Bezier3 bezier = default(Bezier3);
			Bezier3 bezier2 = default(Bezier3);
			Vector3 startDir;
			bool smoothStart;
			instance.m_segments.m_buffer[(int)segment].CalculateCorner(segment, false, true, true, out bezier.a, out startDir, out smoothStart);
			Vector3 endDir;
			bool smoothEnd;
			instance.m_segments.m_buffer[(int)segment].CalculateCorner(segment, false, false, true, out bezier2.d, out endDir, out smoothEnd);
			Vector3 startDir2;
			instance.m_segments.m_buffer[(int)segment].CalculateCorner(segment, false, true, false, out bezier2.a, out startDir2, out smoothStart);
			Vector3 endDir2;
			instance.m_segments.m_buffer[(int)segment].CalculateCorner(segment, false, false, false, out bezier.d, out endDir2, out smoothEnd);
			NetSegment.CalculateMiddlePoints(bezier.a, startDir, bezier.d, endDir2, smoothStart, smoothEnd, out bezier.b, out bezier.c);
			NetSegment.CalculateMiddlePoints(bezier2.a, startDir2, bezier2.d, endDir, smoothStart, smoothEnd, out bezier2.b, out bezier2.c);
			int num = 16;
			Quad2 quad;
			quad.a = VectorUtils.XZ(bezier.a);
			quad.d = VectorUtils.XZ(bezier2.a);
			for (int i = 1; i <= num; i++) {
				quad.b = VectorUtils.XZ(bezier.Position((float)i / (float)num));
				quad.c = VectorUtils.XZ(bezier2.Position((float)i / (float)num));
				if (Singleton<GameAreaManager>.instance.QuadOutOfArea(quad)) {
					return true;
				}
				quad.a = quad.b;
				quad.d = quad.c;
			}
			return false;
		}

		protected virtual bool CheckBuilding(ushort building, ref ToolBase.ToolErrors errors) {
			if ((this.m_toolController.m_mode & ItemClass.Availability.Editors) != ItemClass.Availability.None) {
				return true;
			}
			BuildingManager instance = Singleton<BuildingManager>.instance;
			if ((instance.m_buildings.m_buffer[(int)building].m_flags & Building.Flags.Original) == Building.Flags.None) {
				return true;
			}
			BuildingInfo info = instance.m_buildings.m_buffer[(int)building].Info;
			float angle = instance.m_buildings.m_buffer[(int)building].m_angle;
			int width = instance.m_buildings.m_buffer[(int)building].Width;
			int length = instance.m_buildings.m_buffer[(int)building].Length;
			Vector3 position = instance.m_buildings.m_buffer[(int)building].m_position;
			Vector2 vector = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
			Vector2 vector2 = new Vector2(vector.y, -vector.x);
			if (info.m_placementMode == BuildingInfo.PlacementMode.Roadside || info.m_placementMode == BuildingInfo.PlacementMode.PathsideOrGround) {
				vector *= (float)width * 4f - 0.8f;
				vector2 *= (float)length * 4f - 0.8f;
			} else {
				vector *= (float)width * 4f;
				vector2 *= (float)length * 4f;
			}
			if (info.m_circular) {
				vector *= 0.7f;
				vector2 *= 0.7f;
			}
			Vector2 a = VectorUtils.XZ(position);
			Quad2 quad = default(Quad2);
			quad.a = a - vector - vector2;
			quad.b = a + vector - vector2;
			quad.c = a + vector + vector2;
			quad.d = a - vector + vector2;
			return !Singleton<GameAreaManager>.instance.QuadOutOfArea(quad);
		}

		protected virtual bool CheckProp(ushort prop, ref ToolBase.ToolErrors errors) {
			if ((this.m_toolController.m_mode & ItemClass.Availability.Editors) != ItemClass.Availability.None) {
				return true;
			}
			Vector2 a = VectorUtils.XZ(Singleton<PropManager>.instance.m_props.m_buffer[(int)prop].Position);
			float num = 0.5f;
			Quad2 quad = default(Quad2);
			quad.a = a + new Vector2(-num, -num);
			quad.b = a + new Vector2(-num, num);
			quad.c = a + new Vector2(num, num);
			quad.d = a + new Vector2(num, -num);
			return !Singleton<GameAreaManager>.instance.QuadOutOfArea(quad);
		}

		protected virtual bool CheckTree(uint tree, ref ToolBase.ToolErrors errors) {
			if ((this.m_toolController.m_mode & ItemClass.Availability.Editors) != ItemClass.Availability.None) {
				return true;
			}
			Vector2 a = VectorUtils.XZ(Singleton<TreeManager>.instance.m_trees.m_buffer[(int)((UIntPtr)tree)].Position);
			float num = 0.5f;
			Quad2 quad = default(Quad2);
			quad.a = a + new Vector2(-num, -num);
			quad.b = a + new Vector2(-num, num);
			quad.c = a + new Vector2(num, num);
			quad.d = a + new Vector2(num, -num);
			return !Singleton<GameAreaManager>.instance.QuadOutOfArea(quad);
		}

		protected virtual bool CheckVehicle(ushort vehicle, ref ToolBase.ToolErrors errors) {
			return true;
		}

		protected virtual bool CheckParkedVehicle(ushort parkedVehicle, ref ToolBase.ToolErrors errors) {
			return true;
		}

		protected virtual bool CheckCitizen(ushort citizenInstance, ref ToolBase.ToolErrors errors) {
			return true;
		}

		protected virtual bool CheckDisaster(ushort disaster, ref ToolBase.ToolErrors errors) {
			return true;
		}

		private void SetHoverInstance(InstanceID id) {
			if (id != this.m_hoverInstance) {
				if (this.m_hoverInstance.TransportLine != 0) {
					TransportLine[] expr_40_cp_0 = Singleton<TransportManager>.instance.m_lines.m_buffer;
					ushort expr_40_cp_1 = this.m_hoverInstance.TransportLine;
					expr_40_cp_0[(int)expr_40_cp_1].m_flags = (expr_40_cp_0[(int)expr_40_cp_1].m_flags & ~TransportLine.Flags.Selected);
				} else if (this.m_hoverInstance.Prop != 0) {
					if (Singleton<PropManager>.instance.m_props.m_buffer[(int)this.m_hoverInstance.Prop].Hidden) {
						Singleton<PropManager>.instance.m_props.m_buffer[(int)this.m_hoverInstance.Prop].Hidden = false;
					}
				} else if (this.m_hoverInstance.Tree != 0u) {
					if (Singleton<TreeManager>.instance.m_trees.m_buffer[(int)((UIntPtr)this.m_hoverInstance.Tree)].Hidden) {
						Singleton<TreeManager>.instance.m_trees.m_buffer[(int)((UIntPtr)this.m_hoverInstance.Tree)].Hidden = false;
						Singleton<TreeManager>.instance.UpdateTreeRenderer(this.m_hoverInstance.Tree, true);
					}
				} else if (this.m_hoverInstance.Building != 0) {
					if ((Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)this.m_hoverInstance.Building].m_flags & Building.Flags.Hidden) != Building.Flags.None) {
						Building[] expr_18F_cp_0 = Singleton<BuildingManager>.instance.m_buildings.m_buffer;
						ushort expr_18F_cp_1 = this.m_hoverInstance.Building;
						expr_18F_cp_0[(int)expr_18F_cp_1].m_flags = (expr_18F_cp_0[(int)expr_18F_cp_1].m_flags & ~Building.Flags.Hidden);
						Singleton<BuildingManager>.instance.UpdateBuildingRenderer(this.m_hoverInstance.Building, true);
					}
				} else if (this.m_hoverInstance.Disaster != 0 && (Singleton<DisasterManager>.instance.m_disasters.m_buffer[(int)this.m_hoverInstance.Disaster].m_flags & DisasterData.Flags.Hidden) != DisasterData.Flags.None) {
					DisasterData[] expr_219_cp_0 = Singleton<DisasterManager>.instance.m_disasters.m_buffer;
					ushort expr_219_cp_1 = this.m_hoverInstance.Disaster;
					expr_219_cp_0[(int)expr_219_cp_1].m_flags = (expr_219_cp_0[(int)expr_219_cp_1].m_flags & ~DisasterData.Flags.Hidden);
				}
				this.m_hoverInstance = id;
				if (this.m_hoverInstance.TransportLine != 0) {
					TransportLine[] expr_260_cp_0 = Singleton<TransportManager>.instance.m_lines.m_buffer;
					ushort expr_260_cp_1 = this.m_hoverInstance.TransportLine;
					expr_260_cp_0[(int)expr_260_cp_1].m_flags = (expr_260_cp_0[(int)expr_260_cp_1].m_flags | TransportLine.Flags.Selected);
				}
			}
		}

		private void SetHoverInstance2(InstanceID id) {
			this.m_hoverInstance2 = id;
		}

		public override ToolBase.ToolErrors GetErrors() {
			return this.m_selectErrors;
		}

		public static void OpenWorldInfoPanel(InstanceID id, Vector3 position) {
			if (id.NetNode != 0) {
				WorldInfoPanel.HideAllWorldInfoPanels();
			} else if (id.NetSegment != 0) {
				if (Singleton<InstanceManager>.instance.SelectInstance(id)) {
					WorldInfoPanel.Show<RoadWorldInfoPanel>(position, id);
				}
			} else if (id.Building != 0) {
				BuildingInfo info = Singleton<BuildingManager>.instance.m_buildings.m_buffer[(int)id.Building].Info;
				ItemClass.Service service = info.m_class.m_service;
				ShelterAI shelterAI = info.m_buildingAI as ShelterAI;
				WarehouseAI warehouseAI = info.m_buildingAI as WarehouseAI;
				UniqueFactoryAI uniqueFactoryAI = info.m_buildingAI as UniqueFactoryAI;
				if (Singleton<InstanceManager>.instance.SelectInstance(id)) {
					if (info.m_buildingAI.SupportEvents(EventManager.EventType.VarsitySportsMatch, (EventManager.EventGroup)(-1))) {
						DistrictManager instance = Singleton<DistrictManager>.instance;
						byte b = instance.GetPark(position);
						if (b != 0 && (!instance.m_parks.m_buffer[(int)b].IsCampus || instance.m_parks.m_buffer[(int)b].m_parkType == DistrictPark.ParkType.GenericCampus)) {
							b = 0;
						}
						if (b != 0 && info.m_class.m_service == ItemClass.Service.VarsitySports) {
							WorldInfoPanel.Show<VarsitySportsArenaPanel>(position, id);
						} else {
							WorldInfoPanel.Show<FootballPanel>(position, id);
						}
					} else if (info.m_buildingAI.SupportEvents(EventManager.EventType.Football, (EventManager.EventGroup)(-1))) {
						WorldInfoPanel.Show<FootballPanel>(position, id);
					} else if (info.m_buildingAI.SupportEvents(EventManager.EventType.Concert, (EventManager.EventGroup)(-1))) {
						WorldInfoPanel.Show<FestivalPanel>(position, id);
					} else if (info.m_buildingAI.SupportEvents(EventManager.EventType.RocketLaunch, (EventManager.EventGroup)(-1))) {
						WorldInfoPanel.Show<ChirpXPanel>(position, id);
					} else if (shelterAI != null) {
						WorldInfoPanel.Show<ShelterWorldInfoPanel>(position, id);
					} else if (warehouseAI != null) {
						WorldInfoPanel.Show<WarehouseWorldInfoPanel>(position, id);
					} else if (uniqueFactoryAI != null) {
						WorldInfoPanel.Show<UniqueFactoryWorldInfoPanel>(position, id);
					} else if (IsCityService(service)) {
						WorldInfoPanel.Show<CityServiceWorldInfoPanel>(position, id);
					} else if (IsTransportService(service)) {
						WorldInfoPanel.Show<PublicTransportWorldInfoPanel>(position, id);
					} else {
						WorldInfoPanel.Show<ZonedBuildingWorldInfoPanel>(position, id);
					}
				}
			} else if (id.Vehicle != 0) {
				ushort firstVehicle = Singleton<VehicleManager>.instance.m_vehicles.m_buffer[(int)id.Vehicle].GetFirstVehicle(id.Vehicle);
				if (firstVehicle != 0 && Singleton<InstanceManager>.instance.SelectInstance(id)) {
					VehicleInfo info2 = Singleton<VehicleManager>.instance.m_vehicles.m_buffer[(int)firstVehicle].Info;
					ItemClass.Service service2 = info2.m_class.m_service;
					switch (service2) {
					case ItemClass.Service.Residential:
						WorldInfoPanel.Show<CitizenVehicleWorldInfoPanel>(position, id);
						goto IL_344;
					case ItemClass.Service.Commercial:
					case ItemClass.Service.Industrial:
						if (service2 == ItemClass.Service.Tourism) {
							WorldInfoPanel.Show<TouristVehicleWorldInfoPanel>(position, id);
							goto IL_344;
						}
						if (service2 != ItemClass.Service.PublicTransport) {
							WorldInfoPanel.Show<CityServiceVehicleWorldInfoPanel>(position, id);
							goto IL_344;
						}
						if (info2.m_class.m_subService == ItemClass.SubService.PublicTransportTaxi || info2.m_class.m_subService == ItemClass.SubService.PublicTransportPost) {
							WorldInfoPanel.Show<CityServiceVehicleWorldInfoPanel>(position, id);
						} else if (info2.m_class.m_level >= ItemClass.Level.Level4) {
							WorldInfoPanel.Show<CityServiceVehicleWorldInfoPanel>(position, id);
						} else {
							WorldInfoPanel.Show<PublicTransportVehicleWorldInfoPanel>(position, id);
						}
						goto IL_344;
					case ItemClass.Service.Natural:
						WorldInfoPanel.Show<MeteorWorldInfoPanel>(position, id);
						goto IL_344;
					}
				}
IL_344:;
			} else if (id.ParkedVehicle != 0) {
				if (Singleton<InstanceManager>.instance.SelectInstance(id)) {
					WorldInfoPanel.Show<CitizenVehicleWorldInfoPanel>(position, id);
				}
			} else if (id.CitizenInstance != 0) {
				CitizenInfo info3 = Singleton<CitizenManager>.instance.m_instances.m_buffer[(int)id.CitizenInstance].Info;
				if (info3.m_citizenAI.IsAnimal()) {
					if (Singleton<InstanceManager>.instance.SelectInstance(id)) {
						WorldInfoPanel.Show<AnimalWorldInfoPanel>(position, id);
					}
				} else {
					ItemClass.Service service3 = info3.m_class.m_service;
					if (service3 != ItemClass.Service.Residential) {
						if (service3 != ItemClass.Service.Tourism) {
							if (service3 != ItemClass.Service.PoliceDepartment) {
								if (Singleton<InstanceManager>.instance.SelectInstance(id)) {
									WorldInfoPanel.Show<ServicePersonWorldInfoPanel>(position, id);
								}
							} else if (info3.m_agePhase == Citizen.AgePhase.Young0) {
								uint citizen = Singleton<CitizenManager>.instance.m_instances.m_buffer[(int)id.CitizenInstance].m_citizen;
								if (citizen != 0u) {
									id.Citizen = citizen;
								}
								if (Singleton<InstanceManager>.instance.SelectInstance(id)) {
									WorldInfoPanel.Show<CitizenWorldInfoPanel>(position, id);
								}
							} else if (Singleton<InstanceManager>.instance.SelectInstance(id)) {
								WorldInfoPanel.Show<ServicePersonWorldInfoPanel>(position, id);
							}
						} else {
							uint citizen2 = Singleton<CitizenManager>.instance.m_instances.m_buffer[(int)id.CitizenInstance].m_citizen;
							if (citizen2 != 0u) {
								id.Citizen = citizen2;
							}
							if (Singleton<InstanceManager>.instance.SelectInstance(id)) {
								WorldInfoPanel.Show<TouristWorldInfoPanel>(position, id);
							}
						}
					} else {
						uint citizen3 = Singleton<CitizenManager>.instance.m_instances.m_buffer[(int)id.CitizenInstance].m_citizen;
						if (citizen3 != 0u) {
							id.Citizen = citizen3;
						}
						if (Singleton<InstanceManager>.instance.SelectInstance(id)) {
							WorldInfoPanel.Show<CitizenWorldInfoPanel>(position, id);
						}
					}
				}
			} else if (id.TransportLine != 0) {
				if (Singleton<InstanceManager>.instance.SelectInstance(id)) {
					WorldInfoPanel.Show<PublicTransportWorldInfoPanel>(position, id);
				}
			} else if (id.District > 0) {
				if (Singleton<InstanceManager>.instance.SelectInstance(id)) {
					WorldInfoPanel.Show<DistrictWorldInfoPanel>(position, id);
				}
			} else if (id.Park > 0) {
				if (Singleton<InstanceManager>.instance.SelectInstance(id)) {
					if (Singleton<DistrictManager>.instance.m_parks.m_buffer[(int)id.Park].m_parkType == DistrictPark.ParkType.Industry || Singleton<DistrictManager>.instance.m_parks.m_buffer[(int)id.Park].m_parkType == DistrictPark.ParkType.Forestry || Singleton<DistrictManager>.instance.m_parks.m_buffer[(int)id.Park].m_parkType == DistrictPark.ParkType.Farming || Singleton<DistrictManager>.instance.m_parks.m_buffer[(int)id.Park].m_parkType == DistrictPark.ParkType.Oil || Singleton<DistrictManager>.instance.m_parks.m_buffer[(int)id.Park].m_parkType == DistrictPark.ParkType.Ore) {
						WorldInfoPanel.Show<IndustryWorldInfoPanel>(position, id);
					} else if (Singleton<DistrictManager>.instance.m_parks.m_buffer[(int)id.Park].m_parkType == DistrictPark.ParkType.TradeSchool || Singleton<DistrictManager>.instance.m_parks.m_buffer[(int)id.Park].m_parkType == DistrictPark.ParkType.LiberalArts || Singleton<DistrictManager>.instance.m_parks.m_buffer[(int)id.Park].m_parkType == DistrictPark.ParkType.University || Singleton<DistrictManager>.instance.m_parks.m_buffer[(int)id.Park].m_parkType == DistrictPark.ParkType.GenericCampus) {
						WorldInfoPanel.Show<CampusWorldInfoPanel>(position, id);
					} else {
						WorldInfoPanel.Show<ParkWorldInfoPanel>(position, id);
					}
				}
			} else if (id.Disaster > 0) {
				if (Singleton<InstanceManager>.instance.SelectInstance(id)) {
					WorldInfoPanel.Show<DisasterWorldInfoPanel>(position, id);
				}
			} else {
				WorldInfoPanel.HideAllWorldInfoPanels();
			}
		}

		private static bool IsCityService(ItemClass.Service service) {
			return ItemClass.GetPublicServiceIndex(service) != -1;
		}

		private static bool IsTransportService(ItemClass.Service service) {
			return service == ItemClass.Service.PublicTransport;
		}
	}
}
