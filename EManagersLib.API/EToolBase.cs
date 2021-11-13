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
    }
}
