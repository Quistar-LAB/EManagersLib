using ColossalFramework;
using ColossalFramework.Plugins;
using System.Reflection;
using UnityEngine;

namespace EManagersLib.API {
    public static class PropAPI {
        private static bool m_isEMLInstalled = false;
        public static IPropWrapper Manager;
        public struct PropWrapper : IProp {
            private readonly ushort m_index;
            private readonly PropInstance[] m_buffer;
            public uint Index => m_index;
            public Vector3 Position {
                get => m_buffer[m_index].Position;
                set => m_buffer[m_index].Position = value;
            }
            public float Angle {
                get => m_buffer[m_index].Angle;
                set => m_buffer[m_index].Angle = value;
            }
            public bool FixedHeight {
                get => m_buffer[m_index].FixedHeight;
                set => m_buffer[m_index].FixedHeight = value;
            }
            public bool Single {
                get => m_buffer[m_index].Single;
                set => m_buffer[m_index].Single = value;
            }
            public ushort m_flags {
                get => m_buffer[m_index].m_flags;
                set => m_buffer[m_index].m_flags = value;
            }
            public PropInfo Info => m_buffer[m_index].Info;
            public void MoveProp(Vector3 position) => Singleton<PropManager>.instance.MoveProp(m_index, position);
            public void UpdatePropRenderer(bool updateGroup) => Singleton<PropManager>.instance.UpdatePropRenderer(m_index, updateGroup);
            public void ReleaseProp() => Singleton<PropManager>.instance.ReleaseProp(m_index);
            public PropWrapper(ushort i, PropManager propManager) {
                m_buffer = propManager.m_props.m_buffer;
                m_index = i;
            }
        }
        public class OrigPropManager : IPropWrapper {
            public PropInfo GetInfo(InstanceID id) => Singleton<PropManager>.instance.m_props.m_buffer[id.Prop].Info;
            public IProp Buffer(uint id) => new PropWrapper((ushort)id, Singleton<PropManager>.instance);
            public IProp Buffer(InstanceID id) => new PropWrapper(id.Prop, Singleton<PropManager>.instance);
            public InstanceID SetProp(InstanceID id, uint i) => new InstanceID { Prop = (ushort)i };
            public void UpdateProps(float minX, float minZ, float maxX, float maxZ) => Singleton<PropManager>.instance.UpdateProps(minX, minZ, maxX, maxZ);
            public void UpdateProp(uint id) => Singleton<PropManager>.instance.UpdateProp((ushort)id);
            public bool CreateProp(out uint clone, PropInfo info, Vector3 position, float angle, bool single) {
                bool result = Singleton<PropManager>.instance.CreateProp(out ushort propID, ref Singleton<SimulationManager>.instance.m_randomizer, info, position, angle, single);
                clone = propID;
                return result;
            }
            public InstanceID StepOver(uint id) => new InstanceID { Prop = (ushort)id };
        }
        public struct ExtendedPropWrapper : IProp {
            private readonly uint m_index;
            private readonly EPropInstance[] m_buffer;
            public uint Index => m_index;
            public Vector3 Position {
                get => m_buffer[m_index].Position;
                set => m_buffer[m_index].Position = value;
            }
            public float Angle {
                get => m_buffer[m_index].Angle;
                set => m_buffer[m_index].Angle = value;
            }
            public bool FixedHeight {
                get => m_buffer[m_index].FixedHeight;
                set => m_buffer[m_index].FixedHeight = value;
            }
            public bool Single {
                get => m_buffer[m_index].Single;
                set => m_buffer[m_index].Single = value;
            }
            public ushort m_flags {
                get => m_buffer[m_index].m_flags;
                set => m_buffer[m_index].m_flags = value;
            }
            public PropInfo Info => m_buffer[m_index].Info;
            public void MoveProp(Vector3 position) => Singleton<PropManager>.instance.MoveProp(m_index, position);
            public void UpdatePropRenderer(bool updateGroup) => Singleton<PropManager>.instance.UpdatePropRenderer(m_index, updateGroup);
            public void ReleaseProp() => Singleton<PropManager>.instance.ReleaseProp(m_index);
            public ExtendedPropWrapper(uint i) {
                m_buffer = EPropManager.m_props.m_buffer;
                m_index = i;
            }
        }

        public class ExtendedPropManager : IPropWrapper {
            private readonly EPropInstance[] m_propBuffer;
            public ExtendedPropManager() {
                m_propBuffer = EPropManager.m_props.m_buffer;
            }
            public PropInfo GetInfo(InstanceID id) => m_propBuffer[id.GetProp32()].Info;
            public IProp Buffer(uint id) => new ExtendedPropWrapper(id);
            public IProp Buffer(InstanceID id) => new ExtendedPropWrapper(id.GetProp32());
            public InstanceID SetProp(InstanceID id, uint i) {
                InstanceIDExtension.SetProp32ByRef(ref id, i);
                return id;
            }
            public void UpdateProps(float minX, float minZ, float maxX, float maxZ) => EPropManager.UpdateProps(Singleton<PropManager>.instance, minX, minZ, maxX, maxZ);
            public void UpdateProp(uint id) => EPropManager.UpdateProp(id);
            public bool CreateProp(out uint clone, PropInfo info, Vector3 position, float angle, bool single) =>
                Singleton<PropManager>.instance.CreateProp(out clone, ref Singleton<SimulationManager>.instance.m_randomizer, info, position, angle, single);
            public InstanceID StepOver(uint id) {
                InstanceID result = default;
                InstanceIDExtension.SetProp32ByRef(ref result, id);
                return result;
            }
        }

        /// <summary>
        /// This function initializes the API to support runtime detection of the existence of Extended Managers Library
        /// Make sure to call this function in ILoadingExtension::OnCreated or ILoadingExtension::OnLevelLoaded, but not
        /// before these callbacks.
        /// </summary>
        public static void Initialize() {
            foreach (PluginManager.PluginInfo current in Singleton<PluginManager>.instance.GetPluginsInfo()) {
                foreach (Assembly current2 in current.GetAssemblies()) {
                    if (current2.GetName().Name.ToLower().Equals("emanagerslib")) {
                        m_isEMLInstalled = true;
                        Debug.Log("EML: Is installed!");
                    }
                }
            }
            if (m_isEMLInstalled) {
                Manager = new ExtendedPropManager();
            } else {
                Manager = new OrigPropManager();
            }
        }

        /// <summary>
        /// This is a helper function to get the prop Array32
        /// </summary>
        /// <returns>Returns Array32 prop array</returns>
        public static Array32<EPropInstance> GetPropArray() => EPropManager.m_props;

        /// <summary>
        /// This is a helper function to get the prop buffer array
        /// </summary>
        /// <returns>Returns EPropInstance[] within Array32 of prop array</returns>
        public static EPropInstance[] GetPropBuffer() => EPropManager.m_props.m_buffer;

        /// <summary>
        /// This is a helper function to get the current max limit set for props
        /// </summary>
        /// <returns>Returns an int that indicates the current max prop limit</returns>
        public static int GetPropLimit() => EPropManager.MAX_PROP_LIMIT;
    }
}
