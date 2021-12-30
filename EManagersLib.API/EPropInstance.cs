using ColossalFramework.Math;
using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace EManagersLib.API {
    [StructLayout(LayoutKind.Explicit)]
    public struct EPropInstance {
        public const float PROPGRID_CELL_SIZE = 64f;

        public const ushort CREATEDFLAG = 0x0001;
        public const ushort DELETEDFLAG = 0x0002;
        public const ushort HIDDENFLAG = 0x0004;
        public const ushort HIDDENMASK = 0xfffb;
        public const ushort CONFORMFLAG = 0x0008;
        public const ushort SINGLEFLAG = 0x0010;
        public const ushort SINGLEMASK = 0xffef;
        public const ushort FIXEDHEIGHTFLAG = 0x0020;
        public const ushort FIXEDHEIGHTMASK = 0xffdf;
        public const ushort BLOCKEDFLAG = 0x0040;
        public const ushort BLOCKEDMASK = 0xffbf;

        [Flags]
        public enum Flags : ushort {
            None = 0x0000,
            Created = 0x0001,
            Deleted = 0x0002,
            Hidden = 0x0004,
            Conform = 0x0008,
            Single = 0x0010,
            FixedHeight = 0x0020,
            Blocked = 0x0040,
            All = 0xffff
        }
        [FieldOffset(0)] public PropInstance propInstance;
        [FieldOffset(0)] public ushort __old_m_nextGridProp_Placeholder__; /* m_nextGridProp */
        [FieldOffset(2)] public short m_posX;
        [FieldOffset(4)] public short m_posZ;
        [FieldOffset(6)] public ushort m_posY;
        [FieldOffset(8)] public ushort m_angle;
        [FieldOffset(10)] public ushort m_flags;
        [FieldOffset(12)] public ushort m_infoIndex;
        [FieldOffset(14)] public uint m_nextGridProp;
        [FieldOffset(18)] public float m_scale;
        [FieldOffset(22)] public float m_preciseX;
        [FieldOffset(26)] public float m_preciseZ;
        [FieldOffset(30)] public Color m_color;

        public PropInfo Info {
            get => PrefabCollection<PropInfo>.GetPrefab(m_infoIndex);
            set => m_infoIndex = (ushort)EMath.Clamp(value.m_prefabDataIndex, 0, 65535);
        }
        public bool Single {
            get => (m_flags & SINGLEFLAG) != 0u;
            set => m_flags = value ? (ushort)(m_flags | SINGLEFLAG) : (ushort)(m_flags & SINGLEMASK);
        }
        public bool FixedHeight {
            get => (m_flags & FIXEDHEIGHTFLAG) != 0u;
            set => m_flags = value ? (ushort)(m_flags | FIXEDHEIGHTFLAG) : (ushort)(m_flags & FIXEDHEIGHTMASK);
        }
        public bool Blocked {
            get => (m_flags & BLOCKEDFLAG) != 0u;
            set => m_flags = value ? (EMLPropWrapper.PropAnarchyGetter() ? m_flags : (ushort)(m_flags | BLOCKEDFLAG)) : (ushort)(m_flags & BLOCKEDMASK);
        }
        public bool Hidden {
            get => (m_flags & HIDDENFLAG) != 0u;
            set => m_flags = value ? (ushort)(m_flags | HIDDENFLAG) : (ushort)(m_flags & HIDDENMASK);
        }
        public Vector3 Position {
            get {
                Vector3 result;
                if (EMLPropWrapper.ModeGetter() == ItemClass.Availability.AssetEditor) {
                    result.x = m_posX * 0.0164794922f;
                    result.y = m_posY * 0.015625f;
                    result.z = m_posZ * 0.0164794922f;
                    return result;
                }
                result.x = (m_posX + m_preciseX) * 0.263671875f;
                result.y = m_posY * 0.015625f;
                result.z = (m_posZ + m_preciseZ) * 0.263671875f;
                return result;
            }
            set {
                if (EMLPropWrapper.ModeGetter() == ItemClass.Availability.AssetEditor) {
                    m_posX = (short)EMath.Clamp(EMath.RoundToInt(value.x * 60.68148f), -32767, 32767);
                    m_posZ = (short)EMath.Clamp(EMath.RoundToInt(value.z * 60.68148f), -32767, 32767);
                    m_posY = (ushort)EMath.Clamp(EMath.RoundToInt(value.y * 64f), 0, 65535);
                } else {
                    m_posX = (short)EMath.Clamp(EMath.RoundToInt(value.x * 3.79259253f), -32767, 32767);
                    m_posZ = (short)EMath.Clamp(EMath.RoundToInt(value.z * 3.79259253f), -32767, 32767);
                    m_posY = (ushort)EMath.Clamp(EMath.RoundToInt(value.y * 64f), 0, 65535);
                    m_preciseX = value.x * 3.79259253f - m_posX;
                    m_preciseZ = value.z * 3.79259253f - m_posZ;
                }
            }
        }

        public float Angle {
            get => m_angle * 9.58738E-05f;
            set => m_angle = (ushort)(value * 10430.3779f + 0.5f);
        }

        public static void RenderInstance(RenderManager.CameraInfo cameraInfo, PropInfo info, InstanceID id, Vector3 position,
            float scale, float angle, Color color, Vector4 objectIndex, bool active) =>
            PropAPI.delegatedRenderInstance(cameraInfo, info, id, position, scale, angle, color, objectIndex, active);

        public static void RenderInstance(RenderManager.CameraInfo cameraInfo, PropInfo info, InstanceID id, Vector3 position,
            float scale, float angle, Color color, Vector4 objectIndex, bool active, Texture heightMap, Vector4 heightMapping, Vector4 surfaceMapping) =>
            PropAPI.delegatedRenderInstanceHeightmap(cameraInfo, info, id, position, scale, angle, color, objectIndex, active, heightMap, heightMapping, surfaceMapping);

        public bool RayCast(uint propID, Segment3 ray, out float t, out float targetSqr) =>
            PropAPI.delegatedEPropInstanceRayCast(propID, ray, out t, out targetSqr);
    }
}