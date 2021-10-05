using UnityEngine;

namespace EManagersLib.API {
    public interface IProp {
        Vector3 Position { get; set; }
        float Angle { get; set; }
        bool FixedHeight { get; set; }
        bool Single { get; set; }
        ushort m_flags { get; set; }
        PropInfo Info { get; }
        uint Index { get; }
        void MoveProp(Vector3 position);
        void UpdatePropRenderer(bool updateGroup);
        void ReleaseProp();
    }

    public interface IPropWrapper {
        PropInfo GetInfo(InstanceID id);
        IProp Buffer(uint id);
        IProp Buffer(InstanceID id);
        InstanceID SetProp(InstanceID id, uint i);
        void UpdateProps(float minX, float minZ, float maxX, float maxZ);
        void UpdateProp(uint id);
        bool CreateProp(out uint clone, PropInfo info, Vector3 position, float angle, bool single);
        InstanceID StepOver(uint id);
    }
}
