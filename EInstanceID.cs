using ColossalFramework.IO;
using System;
using UnityEngine;

namespace EManagersLib {
    public struct EInstanceID : IEquatable<EInstanceID> {
        private const uint OBJECT_TYPE = 4278190080u;
        private const uint OBJECT_INDEX = 16777215u;
        private const uint OBJECT_BUILDING = 16777216u;
        private const uint OBJECT_VEHICLE = 33554432u;
        private const uint OBJECT_DISTRICT = 50331648u;
        private const uint OBJECT_CITIZEN = 67108864u;
        private const uint OBJECT_NETNODE = 83886080u;
        private const uint OBJECT_NETSEGMENT = 100663296u;
        private const uint OBJECT_PARKEDVEHICLE = 117440512u;
        private const uint OBJECT_TRANSPORT = 134217728u;
        private const uint OBJECT_CITIZENINSTANCE = 150994944u;
        private const uint OBJECT_PROP = 167772160u;
        private const uint OBJECT_TREE = 184549376u;
        private const uint OBJECT_EVENT = 201326592u;
        private const uint OBJECT_NETLANE = 218103808u;
        private const uint OBJECT_BUILDINGPROP = 234881024u;
        private const uint OBJECT_NETLANEPROP = 251658240u;
        private const uint OBJECT_DISASTER = 268435456u;
        private const uint OBJECT_LIGHTNING = 285212672u;
        private const uint OBJECT_RADIOCHANNEL = 301989888u;
        private const uint OBJECT_RADIOCONTENT = 318767104u;
        private const uint OBJECT_PARK = 335544320u;
        public static EInstanceID Empty = default;
        public uint RawData { get; set; }

        public InstanceID OriginalID => new InstanceID() { RawData = RawData };

        public bool IsEmpty {
            get => (RawData & OBJECT_INDEX) == 0u;
        }

        public InstanceType Type {
            get => (InstanceType)((RawData & OBJECT_TYPE) >> 24);
            set => RawData = ((RawData & OBJECT_INDEX) | ((uint)Mathf.Clamp((int)value, 0, 255) << 24));
        }

        public uint Index {
            get => RawData & OBJECT_INDEX;
            set => RawData = (RawData & OBJECT_TYPE) | (value & OBJECT_INDEX);
        }

        public ushort Building {
            get => (ushort)(((RawData & OBJECT_TYPE) != OBJECT_BUILDING) ? 0u : (RawData & OBJECT_INDEX));
            set => RawData = (OBJECT_BUILDING | value);
        }

        public ushort Vehicle {
            get => (ushort)(((RawData & OBJECT_TYPE) != OBJECT_VEHICLE) ? 0u : (RawData & OBJECT_INDEX));
            set => RawData = (OBJECT_VEHICLE | value);
        }

        public byte District {
            get => (byte)(((RawData & OBJECT_TYPE) != OBJECT_DISTRICT) ? 0u : (RawData & OBJECT_INDEX));
            set => RawData = (OBJECT_DISTRICT | value);
        }

        public uint Citizen {
            get => ((RawData & OBJECT_TYPE) != OBJECT_CITIZEN) ? 0u : (RawData & OBJECT_INDEX);
            set => RawData = (OBJECT_CITIZEN | value);
        }

        public ushort NetNode {
            get => (ushort)(((RawData & OBJECT_TYPE) != OBJECT_NETNODE) ? 0u : (RawData & OBJECT_INDEX));
            set => RawData = (OBJECT_NETNODE | value);
        }

        public ushort NetSegment {
            get => (ushort)(((RawData & OBJECT_TYPE) != OBJECT_NETSEGMENT) ? 0u : (RawData & OBJECT_INDEX));
            set => RawData = (OBJECT_NETSEGMENT | value);
        }

        public ushort ParkedVehicle {
            get => (ushort)(((RawData & OBJECT_TYPE) != OBJECT_PARKEDVEHICLE) ? 0u : (RawData & OBJECT_INDEX));
            set => RawData = (OBJECT_PARKEDVEHICLE | value);
        }

        public ushort TransportLine {
            get => (ushort)(((RawData & OBJECT_TYPE) != OBJECT_TRANSPORT) ? 0u : (RawData & OBJECT_INDEX));
            set => RawData = (OBJECT_TRANSPORT | value);
        }

        public ushort CitizenInstance {
            get => (ushort)(((RawData & OBJECT_TYPE) != OBJECT_CITIZENINSTANCE) ? 0u : (RawData & OBJECT_INDEX));
            set => RawData = (OBJECT_CITIZENINSTANCE | value);
        }

        public uint Prop {
            get => ((RawData & OBJECT_TYPE) != OBJECT_PROP) ? 0u : (RawData & OBJECT_INDEX);
            set => RawData = (OBJECT_PROP | value);
        }

        public uint Tree {
            get => ((RawData & OBJECT_TYPE) != OBJECT_TREE) ? 0u : (RawData & OBJECT_INDEX);
            set => RawData = (OBJECT_TREE | value);
        }

        public ushort Event {
            get => (ushort)(((RawData & OBJECT_TYPE) != OBJECT_EVENT) ? 0u : (RawData & OBJECT_INDEX));
            set => RawData = (OBJECT_EVENT | value);
        }

        public uint NetLane {
            get => ((RawData & OBJECT_TYPE) != OBJECT_NETLANE) ? 0u : (RawData & OBJECT_INDEX);
            set => RawData = (OBJECT_NETLANE | value);
        }

        public ushort Disaster {
            get => (ushort)(((RawData & OBJECT_TYPE) != OBJECT_DISASTER) ? 0u : (RawData & OBJECT_INDEX));
            set => RawData = (OBJECT_DISASTER | value);
        }

        public byte Lightning {
            get => (byte)(((RawData & OBJECT_TYPE) != OBJECT_LIGHTNING) ? 0u : (RawData & OBJECT_INDEX));
            set => RawData = (OBJECT_LIGHTNING | value);
        }

        public ushort RadioChannel {
            get => (ushort)(((RawData & OBJECT_TYPE) != OBJECT_RADIOCHANNEL) ? 0u : (RawData & OBJECT_INDEX));
            set => RawData = (OBJECT_RADIOCHANNEL | value);
        }

        public ushort RadioContent {
            get => (ushort)(((RawData & OBJECT_TYPE) != OBJECT_RADIOCONTENT) ? 0u : (RawData & OBJECT_INDEX));
            set => RawData = (OBJECT_RADIOCONTENT | value);
        }

        public byte Park {
            get => (byte)(((RawData & OBJECT_TYPE) != OBJECT_PARK) ? 0u : (RawData & OBJECT_INDEX));
            set => RawData = (OBJECT_PARK | value);
        }

        public void SetBuildingProp(ushort building, int propIndex) => RawData = OBJECT_BUILDINGPROP | building | ((uint)propIndex << 16);

        public void GetBuildingProp(out ushort building, out int propIndex) {
            if ((RawData & OBJECT_TYPE) == OBJECT_BUILDINGPROP) {
                building = (ushort)(RawData & 0xffffu);
                propIndex = (int)(RawData >> 16 & 0xffu);
            } else {
                building = 0;
                propIndex = 0;
            }
        }

        public void SetNetLaneProp(uint lane, int propIndex) => RawData = OBJECT_NETLANEPROP | lane | ((uint)propIndex << 18);

        public void GetNetLaneProp(out uint lane, out int propIndex) {
            if ((RawData & OBJECT_TYPE) == OBJECT_NETLANEPROP) {
                lane = (RawData & 0x0003ffffu);
                propIndex = (int)(RawData >> 18 & 0x3fu);
            } else {
                lane = 0u;
                propIndex = 0;
            }
        }

        public static explicit operator InstanceID(EInstanceID eInstance) => new InstanceID() { RawData = eInstance.RawData };
        public static explicit operator EInstanceID(InstanceID instance) => new EInstanceID() { RawData = instance.RawData };
        public static bool operator ==(EInstanceID x, EInstanceID y) => x.RawData == y.RawData;
        public static bool operator ==(EInstanceID x, InstanceID y) => x.RawData == y.RawData;
        public static bool operator ==(InstanceID x, EInstanceID y) => x.RawData == y.RawData;
        public static bool operator !=(EInstanceID x, EInstanceID y) => x.RawData != y.RawData;
        public static bool operator !=(EInstanceID x, InstanceID y) => x.RawData != y.RawData;
        public static bool operator !=(InstanceID x, EInstanceID y) => x.RawData != y.RawData;
        public static bool operator <=(EInstanceID x, EInstanceID y) => x.RawData <= y.RawData;
        public static bool operator <=(EInstanceID x, InstanceID y) => x.RawData <= y.RawData;
        public static bool operator <=(InstanceID x, EInstanceID y) => x.RawData <= y.RawData;
        public static bool operator >=(EInstanceID x, EInstanceID y) => x.RawData >= y.RawData;
        public static bool operator >=(EInstanceID x, InstanceID y) => x.RawData >= y.RawData;
        public static bool operator >=(InstanceID x, EInstanceID y) => x.RawData >= y.RawData;
        public static bool operator <(EInstanceID x, EInstanceID y) => x.RawData < y.RawData;
        public static bool operator <(EInstanceID x, InstanceID y) => x.RawData < y.RawData;
        public static bool operator <(InstanceID x, EInstanceID y) => x.RawData < y.RawData;
        public static bool operator >(EInstanceID x, EInstanceID y) => x.RawData > y.RawData;
        public static bool operator >(EInstanceID x, InstanceID y) => x.RawData > y.RawData;
        public static bool operator >(InstanceID x, EInstanceID y) => x.RawData > y.RawData;
        public override bool Equals(object obj) => ((EInstanceID)obj).RawData == RawData;
        public bool Equals(InstanceID obj) => obj.RawData == RawData;
        public bool Equals(EInstanceID obj) => obj.RawData == RawData;
        public override int GetHashCode() => RawData.GetHashCode();
        public override string ToString() => RawData.ToString();
        public void Serialize(DataSerializer s) => s.WriteUInt32(RawData);
        public void Deserialize(DataSerializer s) => RawData = s.ReadUInt32();
    }
}
