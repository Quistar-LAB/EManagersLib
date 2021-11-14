using ColossalFramework;
using ColossalFramework.Math;
using System.Runtime.InteropServices;
using UnityEngine;

namespace EManagersLib.API {
    public static class EToolBase {
        [StructLayout(LayoutKind.Explicit)]
        public struct RaycastOutput {
            [FieldOffset(0)] public ToolBase.RaycastOutput m_raycastOutput;
            [FieldOffset(0)] public Vector3 m_hitPos;
            [FieldOffset(12)] public int m_transportStopIndex;
            [FieldOffset(16)] public int m_transportSegmentIndex;
            [FieldOffset(20)] public int m_overlayButtonIndex;
            [FieldOffset(24)] public uint m_treeInstance;
            [FieldOffset(28)] public ushort __oldnetNode; /* original m_netNode placeholder */
            [FieldOffset(30)] public ushort __oldnetSegment; /* original m_netSegment placeholder */
            [FieldOffset(32)] public ushort __oldbuilding; /* original m_building placeholder */
            [FieldOffset(34)] public ushort __oldpropInstance; /* original m_propInstance placeholder */
            [FieldOffset(36)] public ushort m_vehicle;
            [FieldOffset(38)] public ushort m_parkedVehicle;
            [FieldOffset(40)] public ushort m_citizenInstance;
            [FieldOffset(42)] public ushort m_transportLine;
            [FieldOffset(44)] public ushort m_disaster;
            [FieldOffset(46)] public byte m_district;
            [FieldOffset(47)] public byte m_park;
            [FieldOffset(48)] public bool m_currentEditObject;
            [FieldOffset(52)] public uint m_propInstance;
            [FieldOffset(56)] public uint m_netNode;
            [FieldOffset(60)] public uint m_netSegment;
            [FieldOffset(64)] public uint m_building;
        }

        public static bool RayCast(ToolBase.RaycastInput input, out EToolBase.RaycastOutput output) {
            Vector3 origin = input.m_ray.origin;
            Vector3 normalized = input.m_ray.direction.normalized;
            Vector3 vector = input.m_ray.origin + normalized * input.m_length;
            Segment3 ray = new Segment3(origin, vector);
            output.m_raycastOutput = default;
            output.m_hitPos = vector;
            output.m_overlayButtonIndex = 0;
            output.__oldnetNode = 0;
            output.__oldnetSegment = 0;
            output.__oldpropInstance = 0;
            output.__oldbuilding = 0;
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
            float rayLength = input.m_length;
            if (!input.m_ignoreTerrain && Singleton<TerrainManager>.instance.RayCast(ray, out Vector3 vector2)) {
                float num2 = EMath.Sqrt((vector2 - origin).sqrMagnitude) + 100f;
                if (num2 < rayLength) {
                    output.m_hitPos = vector2;
                    result = true;
                    rayLength = num2;
                }
            }
            if ((input.m_ignoreNodeFlags != NetNode.Flags.All || input.m_ignoreSegmentFlags != NetSegment.Flags.All) && Singleton<NetManager>.instance.RayCast(input.m_buildObject as NetInfo, ray, input.m_netSnap, input.m_segmentNameOnly, input.m_netService.m_service, input.m_netService2.m_service, input.m_netService.m_subService, input.m_netService2.m_subService, input.m_netService.m_itemLayers, input.m_netService2.m_itemLayers, input.m_ignoreNodeFlags, input.m_ignoreSegmentFlags, out vector2, out output.__oldnetNode, out output.__oldnetSegment)) {
                float num3 = EMath.Sqrt((vector2 - origin).sqrMagnitude);
                if (num3 < rayLength) {
                    output.m_netNode = output.__oldnetNode;
                    output.m_netSegment = output.__oldnetSegment;
                    output.m_hitPos = vector2;
                    result = true;
                    rayLength = num3;
                } else {
                    output.__oldnetNode = 0;
                    output.__oldnetSegment = 0;
                    output.m_netNode = 0;
                    output.m_netSegment = 0;
                }
            }
            if (input.m_ignoreBuildingFlags != Building.Flags.All && Singleton<BuildingManager>.instance.RayCast(ray, input.m_buildingService.m_service, input.m_buildingService.m_subService, input.m_buildingService.m_itemLayers, input.m_ignoreBuildingFlags, out vector2, out output.__oldbuilding)) {
                float num4 = EMath.Sqrt((vector2 - origin).sqrMagnitude);
                if (num4 < rayLength) {
                    output.m_hitPos = vector2;
                    output.__oldnetNode = 0;
                    output.__oldnetSegment = 0;
                    output.m_netNode = 0;
                    output.m_netSegment = 0;
                    output.m_building = output.__oldbuilding;
                    result = true;
                    rayLength = num4;
                } else {
                    output.__oldbuilding = 0;
                    output.m_building = 0;
                }
            }
            if (input.m_ignoreDisasterFlags != DisasterData.Flags.All && Singleton<DisasterManager>.instance.RayCast(ray, input.m_ignoreDisasterFlags, out vector2, out output.m_disaster)) {
                float num5 = EMath.Sqrt((vector2 - origin).sqrMagnitude);
                if (num5 < rayLength) {
                    output.m_hitPos = vector2;
                    output.__oldnetNode = 0;
                    output.__oldnetSegment = 0;
                    output.__oldbuilding = 0;
                    output.m_netNode = 0;
                    output.m_netSegment = 0;
                    output.m_building = 0;
                    result = true;
                    rayLength = num5;
                } else {
                    output.m_disaster = 0;
                }
            }
            if (input.m_currentEditObject && Singleton<ToolManager>.instance.m_properties.RaycastEditObject(ray, out vector2)) {
                float num6 = EMath.Sqrt((vector2 - origin).sqrMagnitude);
                if (num6 < rayLength) {
                    output.m_hitPos = vector2;
                    output.__oldnetNode = 0;
                    output.__oldnetSegment = 0;
                    output.__oldbuilding = 0;
                    output.m_netNode = 0;
                    output.m_netSegment = 0;
                    output.m_building = 0;
                    output.m_disaster = 0;
                    output.m_currentEditObject = true;
                    result = true;
                    rayLength = num6;
                }
            }
            if (input.m_ignorePropFlags != PropInstance.Flags.All &&
                EMLPropWrapper.delegatedRayCast(ray, input.m_propService.m_service, input.m_propService.m_subService, input.m_propService.m_itemLayers, input.m_ignorePropFlags, out vector2, out output.m_propInstance)) {
                float num7 = EMath.Sqrt((vector2 - origin).sqrMagnitude) - 0.5f;
                if (num7 < rayLength) {
                    output.m_hitPos = vector2;
                    output.__oldnetNode = 0;
                    output.__oldnetSegment = 0;
                    output.__oldbuilding = 0;
                    output.m_netNode = 0;
                    output.m_netSegment = 0;
                    output.m_building = 0;
                    output.m_disaster = 0;
                    output.m_currentEditObject = false;
                    result = true;
                    rayLength = num7;
                } else {
                    output.m_propInstance = 0;
                }
            }
            if (input.m_ignoreTreeFlags != TreeInstance.Flags.All && Singleton<TreeManager>.instance.RayCast(ray, input.m_treeService.m_service, input.m_treeService.m_subService, input.m_treeService.m_itemLayers, input.m_ignoreTreeFlags, out vector2, out output.m_treeInstance)) {
                float num8 = EMath.Sqrt((vector2 - origin).sqrMagnitude) - 1f;
                if (num8 < rayLength) {
                    output.m_hitPos = vector2;
                    output.__oldnetNode = 0;
                    output.__oldnetSegment = 0;
                    output.__oldbuilding = 0;
                    output.m_netNode = 0;
                    output.m_netSegment = 0;
                    output.m_building = 0;
                    output.m_disaster = 0;
                    output.m_propInstance = 0;
                    output.m_currentEditObject = false;
                    result = true;
                    rayLength = num8;
                } else {
                    output.m_treeInstance = 0u;
                }
            }
            if ((input.m_ignoreVehicleFlags != (Vehicle.Flags.Created | Vehicle.Flags.Deleted | Vehicle.Flags.Spawned | Vehicle.Flags.Inverted | Vehicle.Flags.TransferToTarget | Vehicle.Flags.TransferToSource | Vehicle.Flags.Emergency1 | Vehicle.Flags.Emergency2 | Vehicle.Flags.WaitingPath | Vehicle.Flags.Stopped | Vehicle.Flags.Leaving | Vehicle.Flags.Arriving | Vehicle.Flags.Reversed | Vehicle.Flags.TakingOff | Vehicle.Flags.Flying | Vehicle.Flags.Landing | Vehicle.Flags.WaitingSpace | Vehicle.Flags.WaitingCargo | Vehicle.Flags.GoingBack | Vehicle.Flags.WaitingTarget | Vehicle.Flags.Importing | Vehicle.Flags.Exporting | Vehicle.Flags.Parking | Vehicle.Flags.CustomName | Vehicle.Flags.OnGravel | Vehicle.Flags.WaitingLoading | Vehicle.Flags.Congestion | Vehicle.Flags.DummyTraffic | Vehicle.Flags.Underground | Vehicle.Flags.Transition | Vehicle.Flags.InsideBuilding | Vehicle.Flags.LeftHandDrive) || input.m_ignoreParkedVehicleFlags != VehicleParked.Flags.All) && Singleton<VehicleManager>.instance.RayCast(ray, input.m_ignoreVehicleFlags, input.m_ignoreParkedVehicleFlags, out vector2, out output.m_vehicle, out output.m_parkedVehicle)) {
                float num9 = EMath.Sqrt((vector2 - origin).sqrMagnitude) - 0.5f;
                if (num9 < rayLength) {
                    output.m_hitPos = vector2;
                    output.__oldnetNode = 0;
                    output.__oldnetSegment = 0;
                    output.__oldbuilding = 0;
                    output.__oldpropInstance = 0;
                    output.m_netNode = 0;
                    output.m_netSegment = 0;
                    output.m_building = 0;
                    output.m_disaster = 0;
                    output.m_propInstance = 0;
                    output.m_treeInstance = 0u;
                    output.m_currentEditObject = false;
                    result = true;
                    rayLength = num9;
                } else {
                    output.m_vehicle = 0;
                    output.m_parkedVehicle = 0;
                }
            }
            if (input.m_ignoreCitizenFlags != CitizenInstance.Flags.All && Singleton<CitizenManager>.instance.RayCast(ray, input.m_ignoreCitizenFlags, out vector2, out output.m_citizenInstance)) {
                float num10 = EMath.Sqrt((vector2 - origin).sqrMagnitude) - 0.5f;
                if (num10 < rayLength) {
                    output.m_hitPos = vector2;
                    output.__oldnetNode = 0;
                    output.__oldnetSegment = 0;
                    output.__oldbuilding = 0;
                    output.__oldpropInstance = 0;
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
                    rayLength = num10;
                } else {
                    output.m_citizenInstance = 0;
                }
            }
            if (input.m_ignoreTransportFlags != TransportLine.Flags.All && Singleton<TransportManager>.instance.RayCast(input.m_ray, input.m_length, input.m_transportTypes, out vector2, out output.m_transportLine, out output.m_transportStopIndex, out output.m_transportSegmentIndex)) {
                float num11 = EMath.Sqrt((vector2 - origin).sqrMagnitude) - 2f;
                if (num11 < rayLength) {
                    output.m_hitPos = vector2;
                    output.__oldnetNode = 0;
                    output.__oldnetSegment = 0;
                    output.__oldbuilding = 0;
                    output.__oldpropInstance = 0;
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
                        if ((Singleton<DistrictManager>.instance.m_districts.m_buffer[output.m_district].m_flags & input.m_ignoreDistrictFlags) != District.Flags.None) {
                            output.m_district = 0;
                        }
                    }
                    if (input.m_ignoreParkFlags != DistrictPark.Flags.All) {
                        output.m_park = Singleton<DistrictManager>.instance.SamplePark(output.m_hitPos);
                        if ((Singleton<DistrictManager>.instance.m_parks.m_buffer[output.m_park].m_flags & input.m_ignoreParkFlags) != DistrictPark.Flags.None) {
                            output.m_park = 0;
                        }
                        if (output.m_park != 0) {
                            output.m_district = 0;
                        }
                    }
                }
                if (output.m_district != 0 || output.m_park != 0) {
                    output.__oldnetNode = 0;
                    output.__oldnetSegment = 0;
                    output.__oldbuilding = 0;
                    output.__oldpropInstance = 0;
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
            if (output.__oldnetNode != 0) {
                NetManager instance = Singleton<NetManager>.instance;
                NetInfo info = instance.m_nodes.m_buffer[(int)output.m_netNode].Info;
                output.m_overlayButtonIndex = info.m_netAI.RayCastNodeButton(output.__oldnetNode, ref instance.m_nodes.m_buffer[output.__oldnetNode], ray);
            }
            return result;
        }
    }
}
