using ColossalFramework;
using ColossalFramework.Math;
using ColossalFramework.Plugins;
using System;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace EManagersLib.API {
    /// <summary>
    /// Standardized Prop API for use with or without EML. All methods in this API are optimized and should be as performant or faster than original CO framework
    /// </summary>
    public static class PropAPI {
        private const int DEFAULT_PROP_LIMIT = 65536;

        #region INTERNALDECLARATION
        private delegate bool DefaultRayCastHandler(ToolBase.RaycastInput input, out ToolBase.RaycastOutput output);
        private delegate ushort DefaultInstanceIDPropGetter();
        private static readonly DefaultRayCastHandler DefaultRayCast = (DefaultRayCastHandler)Delegate.CreateDelegate(typeof(DefaultRayCastHandler), typeof(ToolBase).GetMethod("RayCast", BindingFlags.NonPublic | BindingFlags.Static));
        #endregion INTERNALDECLARATION

        /// <summary>
        /// Helper API to create delegates to get private or protected field members that would usually be accessed
        /// using slow reflection codes
        /// </summary>
        /// <typeparam name="S">Type of class where the field resides</typeparam>
        /// <typeparam name="T">Name of the private or protected field</typeparam>
        /// <param name="field"></param>
        /// <returns>Returns the delegate for fast getter to private or protected fields</returns>
        internal static Func<S, T> CreateGetter<S, T>(FieldInfo field) {
            string methodName = field.ReflectedType.FullName + ".get_" + field.Name;
            DynamicMethod setterMethod = new DynamicMethod(methodName, typeof(T), new Type[1] { typeof(S) }, true);
            ILGenerator gen = setterMethod.GetILGenerator();
            if (field.IsStatic) {
                gen.Emit(OpCodes.Ldsfld, field);
            } else {
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldfld, field);
            }
            gen.Emit(OpCodes.Ret);
            return (Func<S, T>)setterMethod.CreateDelegate(typeof(Func<S, T>));
        }

        /// <summary>
        /// Helper API to create delegates to set private or protected field members that would usually be accessed
        /// using slow reflection codes
        /// </summary>
        /// <typeparam name="S"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="field"></param>
        /// <returns>Returns the delegate for fast setter of private or protected fields</returns>
        public static Action<S, T> CreateSetter<S, T>(FieldInfo field) {
            string methodName = field.ReflectedType.FullName + ".set_" + field.Name;
            DynamicMethod setterMethod = new DynamicMethod(methodName, null, new Type[2] { typeof(S), typeof(T) }, true);
            ILGenerator gen = setterMethod.GetILGenerator();
            if (field.IsStatic) {
                gen.Emit(OpCodes.Ldarg_1);
                gen.Emit(OpCodes.Stsfld, field);
            } else {
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldarg_1);
                gen.Emit(OpCodes.Stfld, field);
            }
            gen.Emit(OpCodes.Ret);
            return (Action<S, T>)setterMethod.CreateDelegate(typeof(Action<S, T>));
        }

        #region EPropInstanceDelegates
        internal delegate void RENDERINSTANCEAPIREGULAR(RenderManager.CameraInfo cameraInfo, PropInfo info, InstanceID id,
            Vector3 position, float scale, float angle, Color color, Vector4 objectIndex, bool active);
        internal delegate void RENDERINSTANCEAPIHEIGHTMAP(RenderManager.CameraInfo cameraInfo, PropInfo info, InstanceID id, Vector3 position,
            float scale, float angle, Color color, Vector4 objectIndex, bool active, Texture heightMap, Vector4 heightMapping, Vector4 surfaceMapping);
        internal delegate bool EPROPINSTANCERAYCAST(uint propID, Segment3 ray, out float t, out float targetSqr);

        internal static RENDERINSTANCEAPIREGULAR delegatedRenderInstance;
        internal static RENDERINSTANCEAPIHEIGHTMAP delegatedRenderInstanceHeightmap;
        internal static EPROPINSTANCERAYCAST delegatedEPropInstanceRayCast;
        #endregion EPropInstanceDelegates

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
        /// Public int to get current buffer length
        /// </summary>
        public static int PropBufferLen {
            get {
                if (m_isEMLInstalled) return (int)(DEFAULT_PROP_LIMIT * EMLPropWrapper.GetPropLimitScale());
                return 65536;
            }
        }

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
                Type eToolBase = Type.GetType("EManagersLib.EToolBase");
                Type ePropInstance = Type.GetType("EManagersLib.EPropInstance");
                MethodInfo renderInstance = ePropInstance.GetMethod("RenderInstance", BindingFlags.Public | BindingFlags.Static, Type.DefaultBinder,
                    new Type[] { typeof(RenderManager.CameraInfo), typeof(PropInfo), typeof(InstanceID), typeof(Vector3), typeof(float),
                             typeof(float), typeof(Color), typeof(Vector4), typeof(bool) }, null);
                delegatedRenderInstance = (RENDERINSTANCEAPIREGULAR)Delegate.CreateDelegate(typeof(RENDERINSTANCEAPIREGULAR),
                    ePropInstance.GetMethod("RenderInstance", BindingFlags.Public | BindingFlags.Static, Type.DefaultBinder,
                    new Type[] { typeof(RenderManager.CameraInfo), typeof(PropInfo), typeof(InstanceID), typeof(Vector3), typeof(float),
                                 typeof(float), typeof(Color), typeof(Vector4), typeof(bool) }, null));
                delegatedRenderInstanceHeightmap = (RENDERINSTANCEAPIHEIGHTMAP)Delegate.CreateDelegate(typeof(RENDERINSTANCEAPIHEIGHTMAP),
                    ePropInstance.GetMethod("RenderInstance", BindingFlags.Public | BindingFlags.Static, Type.DefaultBinder,
                    new Type[] { typeof(RenderManager.CameraInfo), typeof(PropInfo), typeof(InstanceID), typeof(Vector3), typeof(float),
                                 typeof(float), typeof(Color), typeof(Vector4), typeof(bool), typeof(Texture), typeof(Vector4), typeof(Vector4) }, null));
                delegatedEPropInstanceRayCast = (EPROPINSTANCERAYCAST)Delegate.CreateDelegate(typeof(EPROPINSTANCERAYCAST),
                    ePropInstance.GetMethod("PropRayCast", BindingFlags.Public | BindingFlags.Static));
                Wrapper = new EMLPropWrapper();
                RayCast = (RAYCASTAPI)Delegate.CreateDelegate(typeof(RAYCASTAPI), eToolBase, "RayCast");
                GetPropID = EMLPropInstanceID;
            } else {
                Wrapper = new DefPropWrapper();
                RayCast = OrigRayCast;
                GetPropID = OrigPropInstanceID;
            }
        }

    }
}
