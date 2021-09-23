using ColossalFramework;
using ColossalFramework.Math;
using UnityEngine;

namespace EManagersLib.API {
    public static class EToolBase {
        public struct RaycastOutput {
            public Vector3 m_hitPos;
            public int m_transportStopIndex;
            public int m_transportSegmentIndex;
            public int m_overlayButtonIndex;
            public uint m_treeInstance;
            public uint m_propInstance;
            public ushort m_netNode;
            public ushort m_netSegment;
            public ushort m_building;
            public ushort m_vehicle;
            public ushort m_parkedVehicle;
            public ushort m_citizenInstance;
            public ushort m_transportLine;
            public ushort m_disaster;
            public byte m_district;
            public byte m_park;
            public bool m_currentEditObject;
        }

        public static bool RayCast(ToolBase.RaycastInput input, out RaycastOutput output) {
            Vector3 origin = input.m_ray.origin;
            Vector3 normalized = input.m_ray.direction.normalized;
            Vector3 vector = input.m_ray.origin + normalized * input.m_length;
            Segment3 ray = new Segment3(origin, vector);
            output.m_hitPos = vector;
            output.m_overlayButtonIndex = 0;
            output.m_netNode = 0;
            output.m_netSegment = 0;
            output.m_building = 0;
            output.m_propInstance = 0;
            output.m_treeInstance = 0u;
            output.m_vehicle = 0;
            output.m_parkedVehicle = 0;
            output.m_citizenInstance = 0;
            output.m_transportLine = 0;
            output.m_transportStopIndex = 0;
            output.m_transportSegmentIndex = 0;
            output.m_district = 0;
            output.m_park = 0;
            output.m_disaster = 0;
            output.m_currentEditObject = false;
            bool result = false;
            float num = input.m_length;
            if (!input.m_ignoreTerrain && Singleton<TerrainManager>.instance.RayCast(ray, out Vector3 vector2)) {
                float num2 = Vector3.Distance(vector2, origin) + 100f;
                if (num2 < num) {
                    output.m_hitPos = vector2;
                    result = true;
                    num = num2;
                }
            }
            if ((input.m_ignoreNodeFlags != NetNode.Flags.All || input.m_ignoreSegmentFlags != NetSegment.Flags.All) && Singleton<NetManager>.instance.RayCast(input.m_buildObject as NetInfo, ray, input.m_netSnap, input.m_segmentNameOnly, input.m_netService.m_service, input.m_netService2.m_service, input.m_netService.m_subService, input.m_netService2.m_subService, input.m_netService.m_itemLayers, input.m_netService2.m_itemLayers, input.m_ignoreNodeFlags, input.m_ignoreSegmentFlags, out vector2, out output.m_netNode, out output.m_netSegment)) {
                float num3 = Vector3.Distance(vector2, origin);
                if (num3 < num) {
                    output.m_hitPos = vector2;
                    result = true;
                    num = num3;
                } else {
                    output.m_netNode = 0;
                    output.m_netSegment = 0;
                }
            }
            if (input.m_ignoreBuildingFlags != Building.Flags.All && Singleton<BuildingManager>.instance.RayCast(ray, input.m_buildingService.m_service, input.m_buildingService.m_subService, input.m_buildingService.m_itemLayers, input.m_ignoreBuildingFlags, out vector2, out output.m_building)) {
                float num4 = Vector3.Distance(vector2, origin);
                if (num4 < num) {
                    output.m_hitPos = vector2;
                    output.m_netNode = 0;
                    output.m_netSegment = 0;
                    result = true;
                    num = num4;
                } else {
                    output.m_building = 0;
                }
            }
            if (input.m_ignoreDisasterFlags != DisasterData.Flags.All && Singleton<DisasterManager>.instance.RayCast(ray, input.m_ignoreDisasterFlags, out vector2, out output.m_disaster)) {
                float num5 = Vector3.Distance(vector2, origin);
                if (num5 < num) {
                    output.m_hitPos = vector2;
                    output.m_netNode = 0;
                    output.m_netSegment = 0;
                    output.m_building = 0;
                    result = true;
                    num = num5;
                } else {
                    output.m_disaster = 0;
                }
            }
            if (input.m_currentEditObject && Singleton<ToolManager>.instance.m_properties.RaycastEditObject(ray, out vector2)) {
                float num6 = Vector3.Distance(vector2, origin);
                if (num6 < num) {
                    output.m_hitPos = vector2;
                    output.m_netNode = 0;
                    output.m_netSegment = 0;
                    output.m_building = 0;
                    output.m_disaster = 0;
                    output.m_currentEditObject = true;
                    result = true;
                    num = num6;
                }
            }
            if (input.m_ignorePropFlags != PropInstance.Flags.All &&
                EPropManager.RayCast(ray, input.m_propService.m_service, input.m_propService.m_subService, input.m_propService.m_itemLayers, input.m_ignorePropFlags, out vector2, out output.m_propInstance)) {
                float num7 = Vector3.Distance(vector2, origin) - 0.5f;
                if (num7 < num) {
                    output.m_hitPos = vector2;
                    output.m_netNode = 0;
                    output.m_netSegment = 0;
                    output.m_building = 0;
                    output.m_disaster = 0;
                    output.m_currentEditObject = false;
                    result = true;
                    num = num7;
                } else {
                    output.m_propInstance = 0;
                }
            }
            if (input.m_ignoreTreeFlags != TreeInstance.Flags.All && Singleton<TreeManager>.instance.RayCast(ray, input.m_treeService.m_service, input.m_treeService.m_subService, input.m_treeService.m_itemLayers, input.m_ignoreTreeFlags, out vector2, out output.m_treeInstance)) {
                float num8 = Vector3.Distance(vector2, origin) - 1f;
                if (num8 < num) {
                    output.m_hitPos = vector2;
                    output.m_netNode = 0;
                    output.m_netSegment = 0;
                    output.m_building = 0;
                    output.m_disaster = 0;
                    output.m_propInstance = 0;
                    output.m_currentEditObject = false;
                    result = true;
                    num = num8;
                } else {
                    output.m_treeInstance = 0u;
                }
            }
            if ((input.m_ignoreVehicleFlags != (Vehicle.Flags.Created | Vehicle.Flags.Deleted | Vehicle.Flags.Spawned | Vehicle.Flags.Inverted | Vehicle.Flags.TransferToTarget | Vehicle.Flags.TransferToSource | Vehicle.Flags.Emergency1 | Vehicle.Flags.Emergency2 | Vehicle.Flags.WaitingPath | Vehicle.Flags.Stopped | Vehicle.Flags.Leaving | Vehicle.Flags.Arriving | Vehicle.Flags.Reversed | Vehicle.Flags.TakingOff | Vehicle.Flags.Flying | Vehicle.Flags.Landing | Vehicle.Flags.WaitingSpace | Vehicle.Flags.WaitingCargo | Vehicle.Flags.GoingBack | Vehicle.Flags.WaitingTarget | Vehicle.Flags.Importing | Vehicle.Flags.Exporting | Vehicle.Flags.Parking | Vehicle.Flags.CustomName | Vehicle.Flags.OnGravel | Vehicle.Flags.WaitingLoading | Vehicle.Flags.Congestion | Vehicle.Flags.DummyTraffic | Vehicle.Flags.Underground | Vehicle.Flags.Transition | Vehicle.Flags.InsideBuilding | Vehicle.Flags.LeftHandDrive) || input.m_ignoreParkedVehicleFlags != VehicleParked.Flags.All) && Singleton<VehicleManager>.instance.RayCast(ray, input.m_ignoreVehicleFlags, input.m_ignoreParkedVehicleFlags, out vector2, out output.m_vehicle, out output.m_parkedVehicle)) {
                float num9 = Vector3.Distance(vector2, origin) - 0.5f;
                if (num9 < num) {
                    output.m_hitPos = vector2;
                    output.m_netNode = 0;
                    output.m_netSegment = 0;
                    output.m_building = 0;
                    output.m_disaster = 0;
                    output.m_propInstance = 0;
                    output.m_treeInstance = 0u;
                    output.m_currentEditObject = false;
                    result = true;
                    num = num9;
                } else {
                    output.m_vehicle = 0;
                    output.m_parkedVehicle = 0;
                }
            }
            if (input.m_ignoreCitizenFlags != CitizenInstance.Flags.All && Singleton<CitizenManager>.instance.RayCast(ray, input.m_ignoreCitizenFlags, out vector2, out output.m_citizenInstance)) {
                float num10 = Vector3.Distance(vector2, origin) - 0.5f;
                if (num10 < num) {
                    output.m_hitPos = vector2;
                    output.m_netNode = 0;
                    output.m_netSegment = 0;
                    output.m_building = 0;
                    output.m_disaster = 0;
                    output.m_propInstance = 0;
                    output.m_treeInstance = 0u;
                    output.m_vehicle = 0;
                    output.m_parkedVehicle = 0;
                    output.m_currentEditObject = false;
                    result = true;
                    num = num10;
                } else {
                    output.m_citizenInstance = 0;
                }
            }
            if (input.m_ignoreTransportFlags != TransportLine.Flags.All && Singleton<TransportManager>.instance.RayCast(input.m_ray, input.m_length, input.m_transportTypes, out vector2, out output.m_transportLine, out output.m_transportStopIndex, out output.m_transportSegmentIndex)) {
                float num11 = Vector3.Distance(vector2, origin) - 2f;
                if (num11 < num) {
                    output.m_hitPos = vector2;
                    output.m_netNode = 0;
                    output.m_netSegment = 0;
                    output.m_building = 0;
                    output.m_disaster = 0;
                    output.m_propInstance = 0;
                    output.m_treeInstance = 0u;
                    output.m_vehicle = 0;
                    output.m_parkedVehicle = 0;
                    output.m_citizenInstance = 0;
                    output.m_currentEditObject = false;
                    result = true;
                } else {
                    output.m_transportLine = 0;
                }
            }
            if (input.m_ignoreDistrictFlags != District.Flags.All || input.m_ignoreParkFlags != DistrictPark.Flags.All) {
                if (input.m_districtNameOnly) {
                    if (Singleton<DistrictManager>.instance.RayCast(ray, input.m_rayRight, out vector2, out output.m_district, out output.m_park)) {
                        output.m_hitPos = vector2;
                    }
                } else {
                    if (input.m_ignoreDistrictFlags != District.Flags.All) {
                        output.m_district = Singleton<DistrictManager>.instance.SampleDistrict(output.m_hitPos);
                        if ((Singleton<DistrictManager>.instance.m_districts.m_buffer[(int)output.m_district].m_flags & input.m_ignoreDistrictFlags) != District.Flags.None) {
                            output.m_district = 0;
                        }
                    }
                    if (input.m_ignoreParkFlags != DistrictPark.Flags.All) {
                        output.m_park = Singleton<DistrictManager>.instance.SamplePark(output.m_hitPos);
                        if ((Singleton<DistrictManager>.instance.m_parks.m_buffer[(int)output.m_park].m_flags & input.m_ignoreParkFlags) != DistrictPark.Flags.None) {
                            output.m_park = 0;
                        }
                        if (output.m_park != 0) {
                            output.m_district = 0;
                        }
                    }
                }
                if (output.m_district != 0 || output.m_park != 0) {
                    output.m_netNode = 0;
                    output.m_netSegment = 0;
                    output.m_building = 0;
                    output.m_disaster = 0;
                    output.m_propInstance = 0;
                    output.m_treeInstance = 0u;
                    output.m_vehicle = 0;
                    output.m_parkedVehicle = 0;
                    output.m_citizenInstance = 0;
                    output.m_transportLine = 0;
                    output.m_transportStopIndex = 0;
                    output.m_transportSegmentIndex = 0;
                    output.m_currentEditObject = false;
                    result = true;
                }
            }
            if (output.m_netNode != 0) {
                NetManager instance = Singleton<NetManager>.instance;
                NetInfo info = instance.m_nodes.m_buffer[(int)output.m_netNode].Info;
                output.m_overlayButtonIndex = info.m_netAI.RayCastNodeButton(output.m_netNode, ref instance.m_nodes.m_buffer[(int)output.m_netNode], ray);
            }
            return result;
        }
    }
}
