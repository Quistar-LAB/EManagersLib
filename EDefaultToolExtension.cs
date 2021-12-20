using ColossalFramework;
using ColossalFramework.Math;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace EManagersLib {
    public static class EDefaultToolExtension {
        public static bool CheckProp(ToolController toolController, uint prop) {
            if ((toolController.m_mode & ItemClass.Availability.Editors) != ItemClass.Availability.None) return true;
            Vector2 a = VectorUtils.XZ(EPropManager.m_props.m_buffer[prop].Position);
            Quad2 quad = default;
            quad.a = a + new Vector2(-0.5f, -0.5f);
            quad.b = a + new Vector2(-0.5f, 0.5f);
            quad.c = a + new Vector2(0.5f, 0.5f);
            quad.d = a + new Vector2(0.5f, -0.5f);
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

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void StartMovingRotating(ref float angle, uint propID) {
            EPropInstance[] props = EPropManager.m_props.m_buffer;
            angle = props[propID].Angle * 57.29578f;
            props[propID].Hidden = true;
        }

        private static ToolBase.ToolErrors CheckPlacementErrors(BuildingInfo info, ref Vector3 position, ref float angle, bool fixedHeight, ushort id, ulong[] collidingSegmentBuffer, ulong[] collidingBuildingBuffer) {
            Segment3 segment = default;
            float num3 = 0f;
            if ((Singleton<ToolManager>.instance.m_properties.m_mode & ItemClass.Availability.AssetEditor) != ItemClass.Availability.None) {
                return ToolBase.ToolErrors.None;
            }
            ToolBase.ToolErrors toolErrors;
            int num;
            int num2;
            if (info.m_placementMode == BuildingInfo.PlacementMode.Shoreline || info.m_placementMode == BuildingInfo.PlacementMode.ShorelineOrGround) {
                bool flag = BuildingTool.SnapToCanal(position, out Vector3 vector, out Vector3 vector2, out bool flag2, 40f, false);
                bool shorePos = Singleton<TerrainManager>.instance.GetShorePos(vector, 50f, out Vector3 vector3, out Vector3 vector4, out num3);
                if (flag) {
                    position = vector;
                    angle = Mathf.Atan2(vector2.x, -vector2.z);
                    float num4 = EMath.Max(0f, vector.y);
                    Building.SampleBuildingHeight(position, angle, info.m_cellWidth, info.m_cellLength, info, out float num5, out float num6, out float num7, ref num4);
                    num5 -= 20f;
                    num7 = EMath.Max(position.y, num7);
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
                        Building.SampleBuildingHeight(position, angle, info.m_cellWidth, info.m_cellLength, info, out float num8, out float num9, out float num10);
                        num8 = EMath.Min(num3, num8);
                        num10 = EMath.Max(position.y, num10);
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
                    Building.SampleBuildingHeight(position, angle, info.m_cellWidth, info.m_cellLength, info, out float num11, out float num12, out float num13);
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
                if (BuildingTool.SnapToPath(position, out Vector3 a, out Vector3 a2, EMath.Min(info.m_cellWidth, info.m_cellLength) * 3.9f, info.m_hasPedestrianPaths)) {
                    position = a - a2 * info.m_cellLength * 4f;
                    angle = Mathf.Atan2(-a2.x, a2.z);
                    Building.SampleBuildingHeight(position, angle, info.m_cellWidth, info.m_cellLength, info, out float num14, out float num15, out float num16);
                    toolErrors = info.m_buildingAI.CheckBuildPosition(id, ref position, ref angle, num3, 0f, ref segment, out num, out num2);
                    toolErrors |= BuildingTool.CheckSpace(info, BuildingInfo.PlacementMode.PathsideOrGround, (int)id, position, num14, position.y + info.m_collisionHeight, angle, info.m_cellWidth, info.m_cellLength, true, collidingSegmentBuffer, collidingBuildingBuffer);
                    if (num15 - num14 > info.m_maxHeightOffset) {
                        toolErrors |= ToolBase.ToolErrors.SlopeTooSteep;
                    }
                } else {
                    Quaternion rotation2 = Quaternion.AngleAxis(angle, Vector3.down);
                    position -= rotation2 * info.m_centerOffset;
                    Building.SampleBuildingHeight(position, angle, info.m_cellWidth, info.m_cellLength, info, out float num17, out float num18, out float num19);
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
                Building.SampleBuildingHeight(position, angle, info.m_cellWidth, info.m_cellLength, info, out float minY, out float num20, out float num21);
                position.y = num21;
                toolErrors = info.m_buildingAI.CheckBuildPosition(id, ref position, ref angle, num3, 0f, ref segment, out num, out num2);
                toolErrors |= BuildingTool.CheckSpace(info, info.m_placementMode, (int)id, position, minY, num21 + info.m_collisionHeight, angle, info.m_cellWidth, info.m_cellLength, true, collidingSegmentBuffer, collidingBuildingBuffer);
            } else if (info.m_placementMode == BuildingInfo.PlacementMode.OnGround) {
                Quaternion rotation4 = Quaternion.AngleAxis(angle * 57.29578f, Vector3.down);
                position -= rotation4 * info.m_centerOffset;
                Building.SampleBuildingHeight(position, angle, info.m_cellWidth, info.m_cellLength, info, out float num22, out float num23, out float num24);
                position.y = num24;
                toolErrors = info.m_buildingAI.CheckBuildPosition(id, ref position, ref angle, num3, 0f, ref segment, out num, out num2);
                toolErrors |= BuildingTool.CheckSpace(info, info.m_placementMode, (int)id, position, num22, num24 + info.m_collisionHeight, angle, info.m_cellWidth, info.m_cellLength, true, collidingSegmentBuffer, collidingBuildingBuffer);
                if (num23 - num22 > info.m_maxHeightOffset) {
                    toolErrors |= ToolBase.ToolErrors.SlopeTooSteep;
                }
            } else if (info.m_placementMode == BuildingInfo.PlacementMode.OnWater) {
                Quaternion rotation5 = Quaternion.AngleAxis(angle * 57.29578f, Vector3.down);
                position -= rotation5 * info.m_centerOffset;
                Building.SampleBuildingHeight(position, angle, info.m_cellWidth, info.m_cellLength, info, out float minY2, out float num25, out float num26);
                position.y = num26;
                toolErrors = info.m_buildingAI.CheckBuildPosition(id, ref position, ref angle, num3, 0f, ref segment, out num, out num2);
                toolErrors |= BuildingTool.CheckSpace(info, info.m_placementMode, (int)id, position, minY2, num26 + info.m_collisionHeight, angle, info.m_cellWidth, info.m_cellLength, true, collidingSegmentBuffer, collidingBuildingBuffer);
            } else {
                toolErrors = ToolBase.ToolErrors.Pending;
                toolErrors |= info.m_buildingAI.CheckBuildPosition(id, ref position, ref angle, num3, 0f, ref segment, out num, out num2);
            }
            if (!(info.m_subBuildings is null) && info.m_subBuildings.Length != 0) {
                Matrix4x4 matrix4x = default;
                matrix4x.SetTRS(position, Quaternion.AngleAxis(angle * 57.29578f, Vector3.down), Vector3.one);
                for (int i = 0; i < info.m_subBuildings.Length; i++) {
                    BuildingInfo buildingInfo = info.m_subBuildings[i].m_buildingInfo;
                    position = matrix4x.MultiplyPoint(info.m_subBuildings[i].m_position);
                    float num27 = info.m_subBuildings[i].m_angle * 0.0174532924f + angle;
                    Segment3 segment2 = default;
                    toolErrors |= buildingInfo.m_buildingAI.CheckBuildPosition(id, ref position, ref num27, num3, 0f, ref segment2, out int num28, out int num29);
                    num2 += num29;
                }
            }
            return toolErrors;
        }

        private static bool CheckBuilding(ToolController toolController, ushort building, ref ToolBase.ToolErrors errors) {
            if ((toolController.m_mode & ItemClass.Availability.Editors) != ItemClass.Availability.None) {
                return true;
            }
            BuildingManager instance = Singleton<BuildingManager>.instance;
            if ((instance.m_buildings.m_buffer[building].m_flags & Building.Flags.Original) == Building.Flags.None) {
                return true;
            }
            BuildingInfo info = instance.m_buildings.m_buffer[building].Info;
            float angle = instance.m_buildings.m_buffer[building].m_angle;
            int width = instance.m_buildings.m_buffer[building].Width;
            int length = instance.m_buildings.m_buffer[building].Length;
            Vector3 position = instance.m_buildings.m_buffer[building].m_position;
            Vector2 vector = new Vector2(EMath.Cos(angle), EMath.Sin(angle));
            Vector2 vector2 = new Vector2(vector.y, -vector.x);
            if (info.m_placementMode == BuildingInfo.PlacementMode.Roadside || info.m_placementMode == BuildingInfo.PlacementMode.PathsideOrGround) {
                vector *= width * 4f - 0.8f;
                vector2 *= length * 4f - 0.8f;
            } else {
                vector *= width * 4f;
                vector2 *= length * 4f;
            }
            if (info.m_circular) {
                vector *= 0.7f;
                vector2 *= 0.7f;
            }
            Vector2 a = VectorUtils.XZ(position);
            Quad2 quad = default;
            quad.a = a - vector - vector2;
            quad.b = a + vector - vector2;
            quad.c = a + vector + vector2;
            quad.d = a - vector + vector2;
            return !Singleton<GameAreaManager>.instance.QuadOutOfArea(quad);
        }

        private static bool CheckNode(ToolController toolController, ushort node) {
            if ((toolController.m_mode & ItemClass.Availability.Editors) != ItemClass.Availability.None) {
                return true;
            }
            return !Singleton<GameAreaManager>.instance.PointOutOfArea(Singleton<NetManager>.instance.m_nodes.m_buffer[node].m_position);
        }

        private static bool CheckSegment(ToolController toolController, ushort segment) =>
            (toolController.m_mode & ItemClass.Availability.Editors) != ItemClass.Availability.None || !DefaultTool.IsOutOfCityArea(segment);

        private static bool CheckTree(ToolController toolController, uint tree) {
            if ((toolController.m_mode & ItemClass.Availability.Editors) != ItemClass.Availability.None) {
                return true;
            }
            Vector2 a = VectorUtils.XZ(Singleton<TreeManager>.instance.m_trees.m_buffer[tree].Position);
            float num = 0.5f;
            Quad2 quad = default;
            quad.a = a + new Vector2(-num, -num);
            quad.b = a + new Vector2(-num, num);
            quad.c = a + new Vector2(num, num);
            quad.d = a + new Vector2(num, -num);
            return !Singleton<GameAreaManager>.instance.QuadOutOfArea(quad);
        }

        private static void SetHoverInstance(InstanceID id, ref InstanceID hoverInstance) {
            if (id != hoverInstance) {
                if (hoverInstance.TransportLine != 0) {
                    Singleton<TransportManager>.instance.m_lines.m_buffer[hoverInstance.TransportLine].m_flags &= ~TransportLine.Flags.Selected;
                } else if (hoverInstance.GetProp32() != 0) {
                    if (EPropManager.m_props.m_buffer[hoverInstance.GetProp32()].Hidden) {
                        EPropManager.m_props.m_buffer[hoverInstance.GetProp32()].Hidden = false;
                    }
                } else if (hoverInstance.Tree != 0u) {
                    if (Singleton<TreeManager>.instance.m_trees.m_buffer[hoverInstance.Tree].Hidden) {
                        Singleton<TreeManager>.instance.m_trees.m_buffer[hoverInstance.Tree].Hidden = false;
                        Singleton<TreeManager>.instance.UpdateTreeRenderer(hoverInstance.Tree, true);
                    }
                } else if (hoverInstance.Building != 0) {
                    if ((Singleton<BuildingManager>.instance.m_buildings.m_buffer[hoverInstance.Building].m_flags & Building.Flags.Hidden) != Building.Flags.None) {
                        Singleton<BuildingManager>.instance.m_buildings.m_buffer[hoverInstance.Building].m_flags &= ~Building.Flags.Hidden;
                        Singleton<BuildingManager>.instance.UpdateBuildingRenderer(hoverInstance.Building, true);
                    }
                } else if (hoverInstance.Disaster != 0 && (Singleton<DisasterManager>.instance.m_disasters.m_buffer[hoverInstance.Disaster].m_flags & DisasterData.Flags.Hidden) != DisasterData.Flags.None) {
                    Singleton<DisasterManager>.instance.m_disasters.m_buffer[hoverInstance.Disaster].m_flags &= ~DisasterData.Flags.Hidden;
                }
                hoverInstance = id;
                if (hoverInstance.TransportLine != 0) {
                    Singleton<TransportManager>.instance.m_lines.m_buffer[hoverInstance.TransportLine].m_flags |= TransportLine.Flags.Selected;
                }
            }
        }

        public static void DefaultSimulationStep(BulldozeTool tool, bool mouseRayValid, Ray mouseRay, Vector3 rayRight,
            float mouseRayLength, bool mouseLeftDown, bool mouseRightDown, ref Vector3 mousePosition, ref InstanceID hoverInstance, ref InstanceID hoverInstance2, ref int subHoverIndex,
            ref ToolBase.ToolErrors selectErrors, ToolController toolController, ref float angle, ref Vector3 accuratePosition, ref bool accuratePositionValid,
            ref bool fixedHeight) {
            ToolBase.RaycastInput input = new ToolBase.RaycastInput(mouseRay, mouseRayLength);
            input.m_rayRight = rayRight;
            input.m_netService = tool.GetService();
            input.m_buildingService = input.m_netService;
            input.m_propService = input.m_netService;
            input.m_treeService = input.m_netService;
            input.m_districtNameOnly = (Singleton<InfoManager>.instance.CurrentMode != InfoManager.InfoMode.Districts);
            input.m_ignoreTerrain = tool.GetTerrainIgnore();
            input.m_ignoreNodeFlags = tool.GetNodeIgnoreFlags();
            input.m_ignoreSegmentFlags = tool.GetSegmentIgnoreFlags(out input.m_segmentNameOnly);
            input.m_ignoreBuildingFlags = tool.GetBuildingIgnoreFlags();
            input.m_ignoreTreeFlags = tool.GetTreeIgnoreFlags();
            input.m_ignorePropFlags = tool.GetPropIgnoreFlags();
            input.m_ignoreVehicleFlags = tool.GetVehicleIgnoreFlags();
            input.m_ignoreParkedVehicleFlags = tool.GetParkedVehicleIgnoreFlags();
            input.m_ignoreCitizenFlags = tool.GetCitizenIgnoreFlags();
            input.m_ignoreTransportFlags = tool.GetTransportIgnoreFlags();
            input.m_ignoreDistrictFlags = tool.GetDistrictIgnoreFlags();
            input.m_ignoreParkFlags = tool.GetParkIgnoreFlags();
            input.m_ignoreDisasterFlags = tool.GetDisasterIgnoreFlags();
            input.m_transportTypes = tool.GetTransportTypes();
            if ((Singleton<ToolManager>.instance.m_properties.m_mode & ItemClass.Availability.AssetEditor) != ItemClass.Availability.None && (mouseLeftDown || mouseRightDown) && !hoverInstance.IsEmpty) {
                input.m_currentEditObject = true;
            }
            ushort num = 0;
            selectErrors = ToolBase.ToolErrors.None;
            EToolBase.RaycastOutput raycastOutput;
            if (mouseLeftDown && hoverInstance.NetSegment != 0 && subHoverIndex > 0) {
                if (Singleton<NetManager>.instance.NetAdjust != null && mouseRayValid) {
                    input.m_ignoreNodeFlags = NetNode.Flags.None;
                    if (EToolBase.RayCast(input, out raycastOutput) && raycastOutput.m_netSegment != 0 && raycastOutput.m_netNode != 0 && Vector3.Distance(Singleton<NetManager>.instance.m_nodes.m_buffer[raycastOutput.m_netNode].m_position, raycastOutput.m_hitPos) >= 20f) {
                        raycastOutput.m_netNode = 0;
                    }
                    Singleton<NetManager>.instance.NetAdjust.SetHoverAdjustPoint(subHoverIndex, raycastOutput.__oldnetSegment, raycastOutput.__oldnetNode);
                }
                return;
            }
            if ((Singleton<ToolManager>.instance.m_properties.m_mode & ItemClass.Availability.MapAndAsset) != ItemClass.Availability.None && (mouseLeftDown || mouseRightDown) && !hoverInstance.IsEmpty) {
                toolController.BeginColliding(out ulong[] collidingSegmentBuffer, out ulong[] collidingBuildingBuffer);
                try {
                    if (mouseLeftDown) {
                        if (mouseRayValid) {
                            if (EToolBase.RayCast(input, out raycastOutput)) {
                                if (hoverInstance.GetProp32() != 0) {
                                    PropInfo info = EPropManager.m_props.m_buffer[hoverInstance.GetProp32()].Info;
                                    selectErrors = EPropTool.CheckPlacementErrors(info, raycastOutput.m_hitPos, raycastOutput.m_currentEditObject, hoverInstance.GetProp32(), collidingSegmentBuffer, collidingBuildingBuffer);
                                } else if (hoverInstance.Tree != 0u) {
                                    TreeInfo info = Singleton<TreeManager>.instance.m_trees.m_buffer[hoverInstance.Tree].Info;
                                    selectErrors = TreeTool.CheckPlacementErrors(info, raycastOutput.m_hitPos, raycastOutput.m_currentEditObject, hoverInstance.Tree, collidingSegmentBuffer, collidingBuildingBuffer);
                                } else if (hoverInstance.Building != 0) {
                                    BuildingInfo info = Singleton<BuildingManager>.instance.m_buildings.m_buffer[hoverInstance.Building].Info;
                                    float num2 = angle * 0.0174532924f;
                                    selectErrors = CheckPlacementErrors(info, ref raycastOutput.m_hitPos, ref num2, (Singleton<BuildingManager>.instance.m_buildings.m_buffer[hoverInstance.Building].m_flags & Building.Flags.FixedHeight) != Building.Flags.None, hoverInstance.Building, collidingSegmentBuffer, collidingBuildingBuffer);
                                    if (num2 != angle * 0.0174532924f) {
                                        angle = num2 * 57.29578f;
                                    }
                                }
                            } else {
                                selectErrors = ToolBase.ToolErrors.RaycastFailed;
                            }
                            mousePosition = raycastOutput.m_hitPos;
                            accuratePosition = raycastOutput.m_hitPos;
                            fixedHeight = raycastOutput.m_currentEditObject;
                        } else {
                            selectErrors = ToolBase.ToolErrors.RaycastFailed;
                        }
                    } else if (mouseRightDown) {
                        if (hoverInstance.Building != 0) {
                            BuildingInfo info4 = Singleton<BuildingManager>.instance.m_buildings.m_buffer[hoverInstance.Building].Info;
                            Vector3 position = Singleton<BuildingManager>.instance.m_buildings.m_buffer[hoverInstance.Building].m_position;
                            float num3 = angle * 0.0174532924f;
                            selectErrors = CheckPlacementErrors(info4, ref position, ref num3, (Singleton<BuildingManager>.instance.m_buildings.m_buffer[hoverInstance.Building].m_flags & Building.Flags.FixedHeight) != Building.Flags.None, hoverInstance.Building, collidingSegmentBuffer, collidingBuildingBuffer);
                            if (num3 != angle * 0.0174532924f) {
                                angle = num3 * 57.29578f;
                            }
                        }
                    }
                } finally {
                    toolController.EndColliding();
                }
                return;
            }
            if ((Singleton<ToolManager>.instance.m_properties.m_mode & ItemClass.Availability.ScenarioEditor) != ItemClass.Availability.None && (mouseLeftDown || mouseRightDown) && hoverInstance.Disaster != 0) {
                if (mouseLeftDown) {
                    if (mouseRayValid) {
                        if (!EToolBase.RayCast(input, out raycastOutput)) {
                            selectErrors = ToolBase.ToolErrors.RaycastFailed;
                        }
                        mousePosition = raycastOutput.m_hitPos;
                        accuratePosition = raycastOutput.m_hitPos;
                    } else {
                        selectErrors = ToolBase.ToolErrors.RaycastFailed;
                    }
                }
                return;
            }
            if (mouseRayValid) {
                if (EToolBase.RayCast(input, out raycastOutput)) {
                    accuratePosition = raycastOutput.m_hitPos;
                    accuratePositionValid = true;
                } else {
                    if (input.m_ignoreTerrain) {
                        input = new ToolBase.RaycastInput(mouseRay, mouseRayLength);
                        if (EToolBase.RayCast(input, out raycastOutput)) {
                            accuratePosition = raycastOutput.m_hitPos;
                            accuratePositionValid = true;
                        } else {
                            accuratePositionValid = false;
                        }
                    } else {
                        accuratePositionValid = false;
                    }
                    selectErrors = ToolBase.ToolErrors.RaycastFailed;
                }
                if (input.m_ignoreNodeFlags == NetNode.Flags.All || raycastOutput.m_overlayButtonIndex == 0) {
                    raycastOutput.m_netNode = 0;
                }
                if (raycastOutput.m_netSegment != 0 && (Singleton<NetManager>.instance.m_segments.m_buffer[raycastOutput.__oldnetSegment].m_flags & NetSegment.Flags.Untouchable) != NetSegment.Flags.None && (tool is BulldozeTool || (!input.m_segmentNameOnly && Singleton<InfoManager>.instance.CurrentMode != InfoManager.InfoMode.TrafficRoutes))) {
                    raycastOutput.m_building = NetSegment.FindOwnerBuilding(raycastOutput.__oldnetSegment, 363f);
                    raycastOutput.m_netSegment = 0;
                }
                if (raycastOutput.m_building != 0 && (Singleton<BuildingManager>.instance.m_buildings.m_buffer[raycastOutput.m_building].m_flags & Building.Flags.Untouchable) != Building.Flags.None) {
                    raycastOutput.m_building = Building.FindParentBuilding(raycastOutput.__oldbuilding);
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
                    int num4 = Singleton<NetManager>.instance.NetAdjust.CheckHoverSegment(ref raycastOutput.__oldnetSegment, raycastOutput.m_hitPos);
                    if (num4 != 0) {
                        raycastOutput.m_overlayButtonIndex = num4;
                    }
                }
                num = DefaultTool.FindSecondarySegment(raycastOutput.__oldnetSegment);
                if (raycastOutput.m_netNode != 0) {
                    if (CheckNode(toolController, raycastOutput.__oldnetNode)) {
                        raycastOutput.m_hitPos = Singleton<NetManager>.instance.m_nodes.m_buffer[raycastOutput.__oldnetNode].m_position;
                    } else {
                        raycastOutput.m_netNode = 0;
                    }
                } else if (raycastOutput.m_netSegment != 0) {
                    if (CheckSegment(toolController, raycastOutput.__oldnetSegment) && CheckSegment(toolController, num)) {
                        raycastOutput.m_hitPos = Singleton<NetManager>.instance.m_segments.m_buffer[raycastOutput.__oldnetSegment].GetClosestPosition(raycastOutput.m_hitPos);
                    } else {
                        raycastOutput.__oldnetSegment = 0;
                        num = 0;
                    }
                } else if (raycastOutput.__oldbuilding != 0) {
                    if (CheckBuilding(toolController, raycastOutput.__oldbuilding, ref selectErrors)) {
                        raycastOutput.m_hitPos = Singleton<BuildingManager>.instance.m_buildings.m_buffer[raycastOutput.__oldbuilding].m_position;
                    } else {
                        raycastOutput.m_building = 0;
                    }
                } else if (raycastOutput.m_propInstance != 0) {
                    if (CheckProp(toolController, raycastOutput.m_propInstance)) {
                        raycastOutput.m_hitPos = EPropManager.m_props.m_buffer[raycastOutput.m_propInstance].Position;
                    } else {
                        raycastOutput.m_propInstance = 0;
                    }
                } else if (raycastOutput.m_treeInstance != 0u) {
                    if (CheckTree(toolController, raycastOutput.m_treeInstance)) {
                        raycastOutput.m_hitPos = Singleton<TreeManager>.instance.m_trees.m_buffer[raycastOutput.m_treeInstance].Position;
                    } else {
                        raycastOutput.m_treeInstance = 0u;
                    }
                } else if (raycastOutput.m_vehicle != 0) {
                    raycastOutput.m_hitPos = Singleton<VehicleManager>.instance.m_vehicles.m_buffer[raycastOutput.m_vehicle].GetLastFramePosition();
                } else if (raycastOutput.m_parkedVehicle != 0) {
                    raycastOutput.m_hitPos = Singleton<VehicleManager>.instance.m_parkedVehicles.m_buffer[raycastOutput.m_parkedVehicle].m_position;
                } else if (raycastOutput.m_citizenInstance != 0) {
                    raycastOutput.m_hitPos = Singleton<CitizenManager>.instance.m_instances.m_buffer[raycastOutput.m_citizenInstance].GetLastFrameData().m_position;
                } else if (raycastOutput.m_disaster != 0) {
                    raycastOutput.m_hitPos = Singleton<DisasterManager>.instance.m_disasters.m_buffer[raycastOutput.m_disaster].m_targetPosition;
                }
            } else {
                raycastOutput = default;
                selectErrors = ToolBase.ToolErrors.RaycastFailed;
                accuratePositionValid = false;
            }
            InstanceID empty = InstanceID.Empty;
            InstanceID empty2 = InstanceID.Empty;
            int overlayButtonIndex = raycastOutput.m_overlayButtonIndex;
            if (raycastOutput.m_netNode != 0) {
                empty.NetNode = raycastOutput.__oldnetNode;
            } else if (raycastOutput.m_netSegment != 0) {
                empty.NetSegment = raycastOutput.__oldnetSegment;
            } else if (raycastOutput.m_building != 0) {
                empty.Building = raycastOutput.__oldbuilding;
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
            SetHoverInstance(empty, ref hoverInstance);
            hoverInstance2 = empty2;
            subHoverIndex = overlayButtonIndex;
            mousePosition = raycastOutput.m_hitPos;
        }
    }
}
