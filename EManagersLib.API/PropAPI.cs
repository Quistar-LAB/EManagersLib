using ColossalFramework;
using ColossalFramework.Plugins;
using System;
using System.Reflection;
using UnityEngine;

namespace EManagersLib.API {
    /// <summary>
    /// Standardized Prop API for use with or without EML. All methods in this API are optimized and should be as performant or faster than original CO framework
    /// </summary>
    public static class PropAPI {
        #region INTERNALDECLARATION
        private delegate bool DefaultRayCastHandler(ToolBase.RaycastInput input, out ToolBase.RaycastOutput output);
        private delegate ushort DefaultInstanceIDPropGetter();
        private static readonly DefaultRayCastHandler DefaultRayCast = (DefaultRayCastHandler)Delegate.CreateDelegate(typeof(DefaultRayCastHandler), typeof(ToolBase).GetMethod("RayCast", BindingFlags.NonPublic | BindingFlags.Static));
        #endregion INTERNALDECLARATION

        /// <summary>
        /// This is the delegated version of custom ToolBase::RayCast.
        /// This delegate will be assigned in Initialize() to the appropriate EML version of EToolBase::RayCast or
        /// original default CO ToolBase::RayCast.
        /// </summary>
        /// <param name="input">RaycastInput parameter</param>
        /// <param name="output">EToolBase.RaycastOutput parameter</param>
        public delegate bool RAYCASTAPI(ToolBase.RaycastInput input, out EToolBase.RaycastOutput output);

        /// <summary>
        /// This is the delegated version of InstanceID.Prop or InstanceID.GetProp32() when EML is installed.
        /// This delegate will be assigned in Initialize() to the appropriate EML version of InstanceID.Prop or InstanceID.GetProp32()
        /// </summary>
        /// <param name="id">InstanceID parameter</param>
        public delegate uint INSTANCEPROPIDAPI(InstanceID id);

        /// <summary>
        /// Public boolean to check if EML is installed or not
        /// </summary>
        public static bool m_isEMLInstalled = false;

        /// <summary>
        /// Wrapper that is set during Initialize. Auto property is just as performant if not better than a regular field
        /// </summary>
        public static PropWrapper Wrapper { get; set; }

        private static bool OrigRayCast(ToolBase.RaycastInput input, out EToolBase.RaycastOutput output) {
            bool result = DefaultRayCast(input, out ToolBase.RaycastOutput raycastOutput);
            output = default;
            output.m_raycastOutput = raycastOutput;
            output.m_netNode = raycastOutput.m_netNode;
            output.m_netSegment = raycastOutput.m_netSegment;
            output.m_building = raycastOutput.m_building;
            output.m_propInstance = raycastOutput.m_propInstance;
            return result;
        }

        private static uint OrigPropInstanceID(InstanceID id) => id.Prop;

        private static uint EMLPropInstanceID(InstanceID id) => id.GetProp32();

        #region API_DECLARATION
        /// <summary>
        /// This is the delegated version of custom ToolBase::RayCast.
        /// This delegate will be assigned in Initialize() to the appropriate EML version of EToolBase::RayCast or
        /// original default CO ToolBase::RayCast.
        /// </summary>
        public static RAYCASTAPI RayCast;

        /// <summary>
        /// This is the delegated version of InstanceID.Prop or InstanceID.GetProp32() when EML is installed.
        /// This delegate will be assigned in Initialize() to the appropriate EML version of InstanceID.Prop or InstanceID.GetProp32()
        /// </summary>
        public static INSTANCEPROPIDAPI GetPropID;
        #endregion API_DECLARATION

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
                Wrapper = new EMLPropWrapper();
                RayCast = EToolBase.RayCast;
                GetPropID = EMLPropInstanceID;
            } else {
                Wrapper = new DefPropWrapper();
                RayCast = OrigRayCast;
                GetPropID = OrigPropInstanceID;
            }
        }

    }
}
