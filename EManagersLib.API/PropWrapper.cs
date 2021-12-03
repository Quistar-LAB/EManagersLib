using ColossalFramework;
using ColossalFramework.Math;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

#pragma warning disable IDE1006 // Naming Styles
namespace EManagersLib.API {
    /// <summary>
    /// Abstract definition for PropWrapper
    /// </summary>
    public abstract class PropWrapper {
        /// <summary>
        /// Cached instance of PropManager
        /// </summary>
        public static PropManager pmInstance;
        public static Randomizer m_randomizer = new Randomizer();
        /// <summary>
        /// Check if Prop Anarchy is enabled or disabled
        /// </summary>
        public abstract bool IsAnarchyEnabled { get; }
        /// <summary>
        /// Check if Prop Snapping is enabled or disabled
        /// </summary>
        public abstract bool IsSnappingEnabled { get; }
        /// <summary>
        /// Check whether prop is valid
        /// </summary>
        /// <param name="id">Prop ID</param>
        /// <returns>Returns true if prop is valid, otherwise false</returns>
        public abstract bool IsValid(uint id);
        /// <summary>
        /// Check whether prop is valid
        /// </summary>
        /// <param name="id">InstanceID</param>
        /// <returns>Returns true if prop is valid, otherwise false</returns>
        public abstract bool IsValid(InstanceID id);
        /// <summary>
        /// Get PropInfo from buffer
        /// </summary>
        /// <param name="id">id index in prop buffer</param>
        /// <returns>Returns PropInfo</returns>
        public abstract PropInfo GetInfo(uint id);
        /// <summary>
        /// Get PropInfo from buffer
        /// </summary>
        /// <param name="id">InstanceID</param>
        /// <returns>Returns PropInfo</returns>
        public abstract PropInfo GetInfo(InstanceID id);
        /// <summary>
        /// Get Prop Position
        /// </summary>
        /// <param name="id">id of prop</param>
        /// <returns>Returns Vector3 of Prop position</returns>
        public abstract Vector3 GetPosition(uint id);
        /// <summary>
        /// Get Prop Position
        /// </summary>
        /// <param name="id">InstanceID</param>
        /// <returns>Returns Vector3 of Prop position</returns>
        public abstract Vector3 GetPosition(InstanceID id);
        /// <summary>
        /// Set Prop Position
        /// </summary>
        /// <param name="id">id of prop</param>
        /// <param name="pos">Vector3 position</param>
        public abstract void SetPosition(uint id, Vector3 pos);
        /// <summary>
        /// Set Prop Position
        /// </summary>
        /// <param name="id">id of prop</param>
        /// <param name="pos">Vector3 position</param>
        public abstract void SetPosition(InstanceID id, Vector3 pos);
        /// <summary>
        /// Get Prop Angle
        /// </summary>
        /// <param name="id">prop id</param>
        /// <returns>Returns prop angle</returns>
        public abstract float GetAngle(uint id);
        /// <summary>
        /// Get Prop Angle
        /// </summary>
        /// <param name="id">InstanceID with valid prop</param>
        /// <returns>Returns prop angle</returns>
        public abstract float GetAngle(InstanceID id);
        /// <summary>
        /// Set Prop Angle
        /// </summary>
        /// <param name="id">id of prop</param>
        /// <param name="angle">New angle</param>
        public abstract void SetAngle(uint id, float angle);
        /// <summary>
        /// Set Prop Angle
        /// </summary>
        /// <param name="id">InstanceID</param>
        /// <param name="angle">New angle</param>
        public abstract void SetAngle(InstanceID id, float angle);
        /// <summary>
        /// Get Prop Single property
        /// </summary>
        /// <param name="id">Prop ID</param>
        public abstract bool GetSingle(uint id);
        /// <summary>
        /// Get Prop Single property
        /// </summary>
        /// <param name="id">InstanceID</param>
        public abstract bool GetSingle(InstanceID id);
        /// <summary>
        /// Set Prop Single property
        /// </summary>
        /// <param name="id">Prop ID</param>
        /// <param name="state">True or False</param>
        public abstract void SetSingle(uint id, bool state);
        /// <summary>
        /// Set Prop Single property
        /// </summary>
        /// <param name="id">InstanceID</param>
        /// <param name="state">True or False</param>
        public abstract void SetSingle(InstanceID id, bool state);
        /// <summary>
        /// Get Prop FixedHeight property
        /// </summary>
        /// <param name="id">prop ID</param>
        /// <returns>Returns boolean state of FixedHeight</returns>
        public abstract bool GetFixedHeight(uint id);
        /// <summary>
        /// Get Prop FixedHeight property
        /// </summary>
        /// <param name="id">InstanceID</param>
        /// <returns>Returns boolean state of FixedHeight</returns>
        public abstract bool GetFixedHeight(InstanceID id);
        /// <summary>
        /// Set Prop FixedHeight property
        /// </summary>
        /// <param name="id">prop ID</param>
        /// <param name="state">Boolean value</param>
        public abstract void SetFixedHeight(uint id, bool state);
        /// <summary>
        /// Set Prop FixedHeight property
        /// </summary>
        /// <param name="id">InstanceID</param>
        /// <param name="state">Boolean value</param>
        public abstract void SetFixedHeight(InstanceID id, bool state);
        /// <summary>
        /// Get Prop Scale
        /// </summary>
        /// <param name="id">prop ID</param>
        /// <returns>Returns the scale of prop</returns>
        public abstract float GetScale(uint id);
        /// <summary>
        /// Get Prop Scale
        /// </summary>
        /// <param name="id">InstanceID</param>
        /// <returns>Returns the scale of prop</returns>
        public abstract float GetScale(InstanceID id);
        /// <summary>
        /// Get Prop Color
        /// </summary>
        /// <param name="id">Prop ID</param>
        /// <returns>Returns the color of prop</returns>
        public abstract Color GetColor(uint id);
        /// <summary>
        /// Get Prop Color
        /// </summary>
        /// <param name="id">InstanceID</param>
        /// <returns>Returns the color of prop</returns>
        public abstract Color GetColor(InstanceID id);
        /// <summary>
        /// Get PropInstance::m_nextGridProp
        /// </summary>
        /// <param name="id">Prop ID</param>
        /// <returns>Returns PropInstance::m_nextGridProp</returns>
        public abstract uint GetNextGridProp(uint id);
        /// <summary>
        /// Get PropInstance::m_flags
        /// </summary>
        /// <param name="id">Prop ID</param>
        /// <returns></returns>
        public abstract ushort GetFlags(uint id);
        /// <summary>
        /// Get PropInstance::m_flags
        /// </summary>
        /// <param name="id">InstanceID</param>
        /// <returns></returns>
        public abstract ushort GetFlags(InstanceID id);
        /// <summary>
        /// Set PropInstance::m_flags
        /// </summary>
        /// <param name="id">Prop ID</param>
        /// <param name="value">ushort value to set</param>
        public abstract void SetFlags(uint id, ushort value);
        /// <summary>
        /// Set PropInstance::m_flags
        /// </summary>
        /// <param name="id">InstanceID</param>
        /// <param name="value">ushort value to set</param>
        public abstract void SetFlags(InstanceID id, ushort value);
        /// <summary>
        /// Get PropInstance buffer
        /// </summary>
        /// <param name="id">prop ID</param>
        /// <returns>Returns the boxed buffer</returns>
        public abstract object GetBuffer(uint id);
        /// <summary>
        /// Get PropInstance buffer
        /// </summary>
        /// <param name="id">InstanceID</param>
        /// <returns>Returns the boxed buffer</returns>
        public abstract object GetBuffer(InstanceID id);
        /// <summary>
        /// Get the whole buffer
        /// </summary>
        /// <returns></returns>
        public abstract object GetRawBuffer();
        /// <summary>
        /// Wrapper for PropManager::CreateProp
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="randomizer"></param>
        /// <param name="info"></param>
        /// <param name="position"></param>
        /// <param name="angle"></param>
        /// <param name="single"></param>
        /// <returns>Returns true if successful, otherwise false</returns>
        public abstract bool CreateProp(out uint prop, ref Randomizer randomizer, PropInfo info, Vector3 position, float angle, bool single);
        /// <summary>
        /// Simplified version of PropManager::CreateProp
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="info"></param>
        /// <param name="position"></param>
        /// <param name="angle"></param>
        /// <param name="single"></param>
        /// <returns>Returns true if successful, otherwise false</returns>
        public abstract bool CreateProp(out uint prop, PropInfo info, Vector3 position, float angle, bool single);
        /// <summary>
        /// Wrapper for PropManager::ReleaseProp
        /// </summary>
        /// <param name="prop">prop ID</param>
        public abstract void ReleaseProp(uint prop);
        /// <summary>
        /// Wrapper for PropManager::ReleaseProp
        /// </summary>
        /// <param name="id">InstanceID</param>
        public abstract void ReleaseProp(InstanceID id);
        /// <summary>
        /// Move Prop
        /// </summary>
        /// <param name="id">prop ID</param>
        /// <param name="position">New position</param>
        public abstract void MoveProp(uint id, Vector3 position);
        /// <summary>
        /// Update prop renderer
        /// </summary>
        /// <param name="id">Prop ID</param>
        /// <param name="updateGroup">update prop group</param>
        public abstract void UpdatePropRenderer(uint id, bool updateGroup);
        /// <summary>
        /// Render Prop
        /// </summary>
        /// <param name="cameraInfo"></param>
        /// <param name="info"></param>
        /// <param name="id"></param>
        /// <param name="position"></param>
        /// <param name="scale"></param>
        /// <param name="angle"></param>
        /// <param name="color"></param>
        /// <param name="objectIndex"></param>
        /// <param name="active"></param>
        public abstract void RenderInstance(RenderManager.CameraInfo cameraInfo, PropInfo info, InstanceID id, Vector3 position, float scale, float angle, Color color, Vector4 objectIndex, bool active);
        /// <summary>
        /// Render Prop
        /// </summary>
        /// <param name="cameraInfo"></param>
        /// <param name="info"></param>
        /// <param name="id"></param>
        /// <param name="position"></param>
        /// <param name="scale"></param>
        /// <param name="angle"></param>
        /// <param name="color"></param>
        /// <param name="objectIndex"></param>
        /// <param name="active"></param>
        /// <param name="heightMap"></param>
        /// <param name="heightMapping"></param>
        /// <param name="surfaceMapping"></param>
        public abstract void RenderInstance(RenderManager.CameraInfo cameraInfo, PropInfo info, InstanceID id, Vector3 position, float scale, float angle, Color color, Vector4 objectIndex, bool active, Texture heightMap, Vector4 heightMapping, Vector4 surfaceMapping);
        /// <summary>
        /// Wrapper for PropInstance::Raycast
        /// </summary>
        /// <param name="propID"></param>
        /// <param name="ray"></param>
        /// <param name="t"></param>
        /// <param name="targetSqr"></param>
        /// <returns>Returns true on a hit</returns>
        public abstract bool RayCast(uint propID, Segment3 ray, out float t, out float targetSqr);
        /// <summary>
        /// Wrapper for PropManager::UpdateProp
        /// </summary>
        /// <param name="propID"></param>
        public abstract void UpdateProp(uint propID);
        /// <summary>
        /// Wrapper for PropManager::UpdateProps
        /// </summary>
        /// <param name="minX"></param>
        /// <param name="minZ"></param>
        /// <param name="maxX"></param>
        /// <param name="maxZ"></param>
        public abstract void UpdateProps(float minX, float minZ, float maxX, float maxZ);
        /// <summary>
        /// Special IEnumerable to use for iterating through props in grid
        /// </summary>
        /// <param name="x">x position in grid</param>
        /// <param name="y">y position in grid</param>
        /// <returns>Returns IEnumerable for use in foreach</returns>
        public abstract IEnumerable<uint> GetPropGridEnumerable(int x, int y);
    }

    /// <summary>
    /// Prop Wrapper for default CO prop framework
    /// </summary>
    public unsafe class DefPropWrapper : PropWrapper {
        private readonly struct iterator_propGrid : IEnumerable<uint>, IEnumerable {
            private readonly int x, y;
            public iterator_propGrid(int x, int y) {
                this.x = x;
                this.y = y;
            }
            public IEnumerator<uint> GetEnumerator() {
                uint propID = pmInstance.m_propGrid[x * 270 + y];
                PropInstance[] props = m_defBuffer;
                while (propID != 0) {
                    yield return propID;
                    propID = props[propID].m_nextGridProp;
                    if (propID > 65536u) break;
                }
            }
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
        /// <summary>
        /// This buffer will be set during class initialization
        /// </summary>
        public static PropInstance[] m_defBuffer;

        /// <summary>
        /// Default prop framework wrapper initialization
        /// </summary>
        public DefPropWrapper() {
            pmInstance = Singleton<PropManager>.instance;
            m_defBuffer = pmInstance.m_props.m_buffer;
        }

        /// <summary>
        /// Check if Prop Anarchy is enabled or disabled
        /// </summary>
        public override bool IsAnarchyEnabled => false;

        /// <summary>
        /// Check if Prop Anarchy is enabled or disabled
        /// </summary>
        public override bool IsSnappingEnabled => false;

        /// <summary>
        /// Check whether prop is valid
        /// </summary>
        /// <param name="id">Prop ID</param>
        /// <returns>Returns true if prop is valid, otherwise false</returns>
        public override bool IsValid(uint id) => m_defBuffer[id].m_flags != 0;

        /// <summary>
        /// Check whether prop is valid
        /// </summary>
        /// <param name="id">InstanceID</param>
        /// <returns>Returns true if prop is valid, otherwise false</returns>
        public override bool IsValid(InstanceID id) => m_defBuffer[id.Prop].m_flags != 0;

        /// <summary>
        /// Get prop angle. This method is as performant as getting PropInstance::Angle property
        /// </summary>
        /// <param name="id">Prop id</param>
        /// <returns>Returns prop angle</returns>
        public override float GetAngle(uint id) => m_defBuffer[id].m_angle * 9.58738E-05f;

        /// <summary>
        /// Get prop angle. This method is as performant as getting PropInstance::Angle property
        /// </summary>
        /// <param name="id">InstanceID</param>
        /// <returns>Returns prop angle</returns>
        public override float GetAngle(InstanceID id) => m_defBuffer[id.Prop].m_angle * 9.58738E-05f;

        /// <summary>
        /// Set prop angle. This method is as performant as setting PropInstance::Angle property
        /// </summary>
        /// <param name="id">prop id</param>
        /// <param name="angle">New angle</param>
        public override void SetAngle(uint id, float angle) => m_defBuffer[id].m_angle = (ushort)EMath.RoundToInt(angle * 10430.3779f);

        /// <summary>
        /// Set prop angle. This method is as performant as setting PropInstance::Angle property
        /// </summary>
        /// <param name="id">InstanceID</param>
        /// <param name="angle">New angle</param>
        public override void SetAngle(InstanceID id, float angle) => m_defBuffer[id.Prop].m_angle = (ushort)EMath.RoundToInt(angle * 10430.3779f);

        /// <summary>
        /// Get PropInfo from prop ID
        /// </summary>
        /// <param name="id">prop ID</param>
        /// <returns>Returns PropInfo</returns>
        public override PropInfo GetInfo(uint id) => PrefabCollection<PropInfo>.GetPrefab(m_defBuffer[id].m_infoIndex);

        /// <summary>
        /// Get PropInfo from prop ID
        /// </summary>
        /// <param name="id">InstanceID</param>
        /// <returns>Returns PropInfo</returns>
        public override PropInfo GetInfo(InstanceID id) => PrefabCollection<PropInfo>.GetPrefab(m_defBuffer[id.Prop].m_infoIndex);

        /// <summary>
        /// Get Prop position. This method is actually slightly faster than calling PropInstance::Position property getter
        /// </summary>
        /// <param name="id">prop id</param>
        /// <returns>Returns prop position</returns>
        public override Vector3 GetPosition(uint id) {
            fixed (PropInstance* prop = &m_defBuffer[id]) {
                if (Singleton<ToolManager>.instance.m_properties.m_mode == ItemClass.Availability.AssetEditor) {
                    Vector3 result;
                    result.x = prop->m_posX * 0.0164794922f;
                    result.y = prop->m_posY * 0.015625f;
                    result.z = prop->m_posZ * 0.0164794922f;
                    return result;
                }
                Vector3 result2;
                result2.x = prop->m_posX * 0.263671875f;
                result2.y = prop->m_posY * 0.015625f;
                result2.z = prop->m_posZ * 0.263671875f;
                return result2;
            }
        }

        /// <summary>
        /// Get Prop position. This method is actually slightly faster than calling PropInstance::Position property getter
        /// </summary>
        /// <param name="id">InstanceID</param>
        /// <returns>Returns prop position</returns>
        public override Vector3 GetPosition(InstanceID id) {
            fixed (PropInstance* prop = &m_defBuffer[id.Prop]) {
                if (Singleton<ToolManager>.instance.m_properties.m_mode == ItemClass.Availability.AssetEditor) {
                    Vector3 result;
                    result.x = prop->m_posX * 0.0164794922f;
                    result.y = prop->m_posY * 0.015625f;
                    result.z = prop->m_posZ * 0.0164794922f;
                    return result;
                }
                Vector3 result2;
                result2.x = prop->m_posX * 0.263671875f;
                result2.y = prop->m_posY * 0.015625f;
                result2.z = prop->m_posZ * 0.263671875f;
                return result2;
            }
        }

        /// <summary>
        /// Set prop position. This method is actually many times faster than setting PropInstance::Position property setter due to using more efficient math routines
        /// </summary>
        /// <param name="id">Prop ID</param>
        /// <param name="pos">New position</param>
        public override void SetPosition(uint id, Vector3 pos) {
            fixed (PropInstance* prop = &m_defBuffer[id]) {
                if (Singleton<ToolManager>.instance.m_properties.m_mode == ItemClass.Availability.AssetEditor) {
                    prop->m_posX = (short)EMath.Clamp(EMath.RoundToInt(pos.x * 60.68148f), -32767, 32767);
                    prop->m_posZ = (short)EMath.Clamp(EMath.RoundToInt(pos.z * 60.68148f), -32767, 32767);
                    prop->m_posY = (ushort)EMath.Clamp(EMath.RoundToInt(pos.y * 64f), 0, 65535);
                } else {
                    prop->m_posX = (short)EMath.Clamp(EMath.RoundToInt(pos.x * 3.79259253f), -32767, 32767);
                    prop->m_posZ = (short)EMath.Clamp(EMath.RoundToInt(pos.z * 3.79259253f), -32767, 32767);
                    prop->m_posY = (ushort)EMath.Clamp(EMath.RoundToInt(pos.y * 64f), 0, 65535);
                }
            }
        }

        /// <summary>
        /// Set prop position. This method is actually many times faster than setting PropInstance::Position property setter due to using more efficient math routines
        /// </summary>
        /// <param name="id">InstanceID</param>
        /// <param name="pos">New position</param>
        public override void SetPosition(InstanceID id, Vector3 pos) {
            fixed (PropInstance* prop = &m_defBuffer[id.Prop]) {
                if (Singleton<ToolManager>.instance.m_properties.m_mode == ItemClass.Availability.AssetEditor) {
                    prop->m_posX = (short)EMath.Clamp(EMath.RoundToInt(pos.x * 60.68148f), -32767, 32767);
                    prop->m_posZ = (short)EMath.Clamp(EMath.RoundToInt(pos.z * 60.68148f), -32767, 32767);
                    prop->m_posY = (ushort)EMath.Clamp(EMath.RoundToInt(pos.y * 64f), 0, 65535);
                } else {
                    prop->m_posX = (short)EMath.Clamp(EMath.RoundToInt(pos.x * 3.79259253f), -32767, 32767);
                    prop->m_posZ = (short)EMath.Clamp(EMath.RoundToInt(pos.z * 3.79259253f), -32767, 32767);
                    prop->m_posY = (ushort)EMath.Clamp(EMath.RoundToInt(pos.y * 64f), 0, 65535);
                }
            }
        }

        /// <summary>
        /// Get prop Single property
        /// </summary>
        /// <param name="id">prop ID</param>
        /// <returns>Returns boolean state of Single</returns>
        public override bool GetSingle(uint id) => (m_defBuffer[id].m_flags & EPropInstance.SINGLEFLAG) != 0;

        /// <summary>
        /// Get prop Single property
        /// </summary>
        /// <param name="id">InstanceID</param>
        /// <returns>Returns boolean state of Single</returns>
        public override bool GetSingle(InstanceID id) => (m_defBuffer[id.Prop].m_flags & EPropInstance.SINGLEFLAG) != 0;

        /// <summary>
        /// Set prop Single property
        /// </summary>
        /// <param name="id">prop ID</param>
        /// <param name="state">True or False</param>
        public override void SetSingle(uint id, bool state) =>
            m_defBuffer[id].m_flags = state ? (ushort)(m_defBuffer[id].m_flags | EPropInstance.SINGLEFLAG) : (ushort)(m_defBuffer[id].m_flags & EPropInstance.SINGLEMASK);

        /// <summary>
        /// Set prop Single property
        /// </summary>
        /// <param name="id">InstanceID</param>
        /// <param name="state">True or False</param>
        public override void SetSingle(InstanceID id, bool state) =>
            m_defBuffer[id.GetProp32()].m_flags = state ? (ushort)(m_defBuffer[id.GetProp32()].m_flags | EPropInstance.SINGLEFLAG) : (ushort)(m_defBuffer[id.GetProp32()].m_flags & EPropInstance.SINGLEMASK);

        /// <summary>
        /// Get prop FixedHeight property
        /// </summary>
        /// <param name="id">prop ID</param>
        /// <returns>Returns FixedHeight boolean state</returns>
        public override bool GetFixedHeight(uint id) => (m_defBuffer[id].m_flags & EPropInstance.FIXEDHEIGHTFLAG) != 0;

        /// <summary>
        /// Get prop FixedHeight property
        /// </summary>
        /// <param name="id">InstanceID</param>
        /// <returns>Returns FixedHeight boolean state</returns>
        public override bool GetFixedHeight(InstanceID id) => (m_defBuffer[id.Prop].m_flags & EPropInstance.FIXEDHEIGHTFLAG) != 0;

        /// <summary>
        /// Set prop FixedHeight property
        /// </summary>
        /// <param name="id">prop ID</param>
        /// <param name="state">Boolean state to set to</param>
        public override void SetFixedHeight(uint id, bool state) => m_defBuffer[id].m_flags = state ?
            (ushort)(m_defBuffer[id].m_flags | EPropInstance.FIXEDHEIGHTFLAG) : (ushort)(m_defBuffer[id].m_flags & EPropInstance.FIXEDHEIGHTMASK);

        /// <summary>
        /// Set prop FixedHeight property
        /// </summary>
        /// <param name="id">Instance</param>
        /// <param name="state">Boolean state to set to</param>
        public override void SetFixedHeight(InstanceID id, bool state) {
            uint propID = id.Prop;
            m_defBuffer[propID].m_flags = state ?
            (ushort)(m_defBuffer[propID].m_flags | EPropInstance.FIXEDHEIGHTFLAG) : (ushort)(m_defBuffer[propID].m_flags & EPropInstance.FIXEDHEIGHTMASK);
        }

        /// <summary>
        /// Get prop scale
        /// </summary>
        /// <param name="id">prop ID</param>
        /// <returns>Returns the prop scale</returns>
        public override float GetScale(uint id) {
            PropInfo prefab = m_defBuffer[id].Info;
            Randomizer randomizer = m_randomizer;
            randomizer.seed = (ulong)(6364136223846793005L * id + 1442695040888963407L);
            return prefab.m_minScale + randomizer.Int32(10000u) * (prefab.m_maxScale - prefab.m_minScale) * 0.0001f;
        }

        /// <summary>
        /// Get prop scale
        /// </summary>
        /// <param name="id">InstanceID</param>
        /// <returns>Returns the prop scale</returns>
        public override float GetScale(InstanceID id) {
            ushort propID = id.Prop;
            Randomizer randomizer = m_randomizer;
            PropInfo prefab = m_defBuffer[propID].Info;
            randomizer.seed = (ulong)(6364136223846793005L * propID + 1442695040888963407L);
            return prefab.m_minScale + randomizer.Int32(10000u) * (prefab.m_maxScale - prefab.m_minScale) * 0.0001f;
        }

        /// <summary>
        /// Get Prop Color
        /// </summary>
        /// <param name="id">Prop ID</param>
        /// <returns>Returns prop scale</returns>
        public override Color GetColor(uint id) {
            Randomizer randomizer = m_randomizer;
            PropInfo prefab = m_defBuffer[id].Info;
            randomizer.seed = (ulong)(6364136223846793005L * id + 1442695040888963407L);
            return prefab.GetColor(ref randomizer);
        }

        /// <summary>
        /// Get Prop Color
        /// </summary>
        /// <param name="id">InstanceID</param>
        /// <returns>Returns prop scale</returns>
        public override Color GetColor(InstanceID id) {
            ushort propID = id.Prop;
            Randomizer randomizer = m_randomizer;
            PropInfo prefab = m_defBuffer[propID].Info;
            randomizer.seed = (ulong)(6364136223846793005L * propID + 1442695040888963407L);
            return prefab.GetColor(ref randomizer);
        }

        /// <summary>
        /// Get PropInstance::m_nextGridProp
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Returns the ushort value of PropInstance::m_nextGridProp casted to uint</returns>
        public override uint GetNextGridProp(uint id) => m_defBuffer[id].m_nextGridProp;

        /// <summary>
        /// Get PropInstance::m_flags
        /// </summary>
        /// <param name="id">PropID</param>
        /// <returns>Returns ushort m_flags</returns>
        public override ushort GetFlags(uint id) => m_defBuffer[id].m_flags;

        /// <summary>
        /// Get PropInstance::m_flags
        /// </summary>
        /// <param name="id">InstanceID</param>
        /// <returns>Returns ushort m_flags</returns>
        public override ushort GetFlags(InstanceID id) => m_defBuffer[id.Prop].m_flags;

        /// <summary>
        /// Set PropInstance::m_flags
        /// </summary>
        /// <param name="id">Prop ID</param>
        /// <param name="value">Set m_flags to value</param>
        public override void SetFlags(uint id, ushort value) => m_defBuffer[id].m_flags = value;

        /// <summary>
        /// Set PropInstance::m_flags
        /// </summary>
        /// <param name="id">Prop ID</param>
        /// <param name="value">Set m_flags to value</param>
        public override void SetFlags(InstanceID id, ushort value) => m_defBuffer[id.Prop].m_flags = value;

        /// <summary>
        /// Get PropInstance buffer
        /// </summary>
        /// <param name="id">prop ID</param>
        /// <returns>Returns the boxed PropInstance buffer</returns>
        public override object GetBuffer(uint id) => m_defBuffer[id];

        /// <summary>
        /// Get PropInstance buffer
        /// </summary>
        /// <param name="id">InstanceID</param>
        /// <returns>Returns the boxed PropInstance buffer</returns>
        public override object GetBuffer(InstanceID id) => m_defBuffer[id.Prop];

        /// <summary>
        /// Get the entire PropInstance buffer
        /// </summary>
        /// <returns></returns>
        public override object GetRawBuffer() => m_defBuffer;

        /// <summary>
        /// Wrapper for PropManager::CreateProp
        /// </summary>
        /// <param name="prop">Prop ID</param>
        /// <param name="randomizer">Randomizer</param>
        /// <param name="info">Prefab Info</param>
        /// <param name="position">Position</param>
        /// <param name="angle">Angle</param>
        /// <param name="single">Single</param>
        /// <returns>Returns true if successful otherwise false</returns>
        public override bool CreateProp(out uint prop, ref Randomizer randomizer, PropInfo info, Vector3 position, float angle, bool single) {
            bool result = pmInstance.CreateProp(out ushort propID, ref randomizer, info, position, angle, single);
            prop = propID;
            return result;
        }

        /// <summary>
        /// Simplified version of PropManager::CreateProp
        /// </summary>
        /// <param name="prop">Prop ID</param>
        /// <param name="info">Prefab Info</param>
        /// <param name="position">Position</param>
        /// <param name="angle">Angle</param>
        /// <param name="single">Single</param>
        /// <returns>Returns true if successful otherwise false</returns>
        public override bool CreateProp(out uint prop, PropInfo info, Vector3 position, float angle, bool single) {
            bool result = pmInstance.CreateProp(out ushort propID, ref m_randomizer, info, position, angle, single);
            prop = propID;
            return result;
        }

        /// <summary>
        /// Wrapper for PropManager::ReleaseProp
        /// </summary>
        /// <param name="prop">Prop ID</param>
        public override void ReleaseProp(uint prop) => pmInstance.ReleaseProp((ushort)prop);

        /// <summary>
        /// Wrapper for PropManager::ReleaseProp
        /// </summary>
        /// <param name="id">InstanceID</param>
        public override void ReleaseProp(InstanceID id) => pmInstance.ReleaseProp(id.Prop);

        /// <summary>
        /// Move Prop
        /// </summary>
        /// <param name="id">Prop ID</param>
        /// <param name="position">New position</param>
        public override void MoveProp(uint id, Vector3 position) => pmInstance.MoveProp((ushort)id, position);

        /// <summary>
        /// Update prop renderer
        /// </summary>
        /// <param name="id">Prop ID</param>
        /// <param name="updateGroup">Update Group</param>
        public override void UpdatePropRenderer(uint id, bool updateGroup) => pmInstance.UpdatePropRenderer((ushort)id, updateGroup);

        /// <summary>
        /// Render prop
        /// </summary>
        /// <param name="cameraInfo"></param>
        /// <param name="info"></param>
        /// <param name="id"></param>
        /// <param name="position"></param>
        /// <param name="scale"></param>
        /// <param name="angle"></param>
        /// <param name="color"></param>
        /// <param name="objectIndex"></param>
        /// <param name="active"></param>
        public override void RenderInstance(RenderManager.CameraInfo cameraInfo, PropInfo info, InstanceID id, Vector3 position, float scale, float angle, Color color, Vector4 objectIndex, bool active) =>
            PropInstance.RenderInstance(cameraInfo, info, id, position, scale, angle, color, objectIndex, active);

        /// <summary>
        /// Render prop
        /// </summary>
        /// <param name="cameraInfo"></param>
        /// <param name="info"></param>
        /// <param name="id"></param>
        /// <param name="position"></param>
        /// <param name="scale"></param>
        /// <param name="angle"></param>
        /// <param name="color"></param>
        /// <param name="objectIndex"></param>
        /// <param name="active"></param>
        /// <param name="heightMap"></param>
        /// <param name="heightMapping"></param>
        /// <param name="surfaceMapping"></param>
        public override void RenderInstance(RenderManager.CameraInfo cameraInfo, PropInfo info, InstanceID id, Vector3 position, float scale, float angle, Color color, Vector4 objectIndex, bool active, Texture heightMap, Vector4 heightMapping, Vector4 surfaceMapping) =>
            PropInstance.RenderInstance(cameraInfo, info, id, position, scale, angle, color, objectIndex, active, heightMap, heightMapping, surfaceMapping);

        /// <summary>
        /// Wrapper for PropInstance::RayCast()
        /// </summary>
        /// <param name="propID"></param>
        /// <param name="ray"></param>
        /// <param name="t"></param>
        /// <param name="targetSqr"></param>
        /// <returns>Returns true on hit, else false</returns>
        public override bool RayCast(uint propID, Segment3 ray, out float t, out float targetSqr) => m_defBuffer[propID].RayCast((ushort)propID, ray, out t, out targetSqr);

        /// <summary>
        /// Wrapper for PropManager::UpdateProp
        /// </summary>
        /// <param name="propID">Prop ID</param>
        public override void UpdateProp(uint propID) => pmInstance.UpdateProp((ushort)propID);

        /// <summary>
        /// Wrapper for PropManager::UpdateProps()
        /// </summary>
        /// <param name="minX">float</param>
        /// <param name="minZ">float</param>
        /// <param name="maxX">float</param>
        /// <param name="maxZ">float</param>
        public override void UpdateProps(float minX, float minZ, float maxX, float maxZ) => pmInstance.UpdateProps(minX, minZ, maxX, maxZ);

        /// <summary>
        /// Special IEnumerable to use for iterating through props in grid
        /// </summary>
        /// <param name="x">x position in grid</param>
        /// <param name="y">y position in grid</param>
        /// <returns>Returns IEnumerable for use in foreach</returns>
        public override IEnumerable<uint> GetPropGridEnumerable(int x, int y) => new iterator_propGrid(x, y);
    }

    /// <summary>
    /// Prop Wrapper for EML prop framework
    /// </summary>
    public unsafe class EMLPropWrapper : PropWrapper {
        private const string EPROPMANAGER = "EManagersLib.EPropManager";

        internal delegate T Getter<T>();
        internal delegate bool CREATEPROPAPI(PropManager instance, out uint prop, ref Randomizer randomizer, PropInfo info, Vector3 position, float angle, bool single);
        internal delegate void RELEASEPROPAPI(PropManager instance, uint propID);
        internal delegate void MOVEPROPAPI(PropManager instance, uint propID, Vector3 position);
        internal delegate void UPDATEPROPAPI(uint prop);
        internal delegate void UPDATEPROPRENDERERAPI(PropManager instance, uint propID, bool updateGroup);
        internal delegate bool RAYCASTAPI(Segment3 ray, ItemClass.Service service, ItemClass.SubService subService, ItemClass.Layer itemLayers, PropInstance.Flags ignoreFlags, out Vector3 hit, out uint propIndex);

        internal static Getter<Array32<EPropInstance>> PropBufferGetter;
        internal static Getter<uint[]> PropGridGetter;
        internal static Getter<ItemClass.Availability> ModeGetter;
        internal static Getter<float> GetPropLimitScale;
        internal static Getter<bool> PropAnarchyGetter;
        internal static Getter<bool> PropSnappingGetter;
        internal static CREATEPROPAPI delegatedCreateProp;
        internal static RELEASEPROPAPI delegatedReleaseProp;
        internal static MOVEPROPAPI delegatedMoveProp;
        internal static UPDATEPROPAPI delegatedUpdateProp;
        internal static UPDATEPROPRENDERERAPI delegatedUpdatePropRenderer;
        internal static RAYCASTAPI delegatedRayCast;

        private static Getter<T> CreateGetter<T>(string field) {
            var type = Type.GetType(EPROPMANAGER);
            var fi = type.GetField(field, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (fi == null) throw new MissingFieldException(type.Name, field);
            var s_name = "__get_" + type.Name + "_fi_" + fi.Name;
            var dm = new DynamicMethod(s_name, typeof(T), new[] { type }, type, true);
            var il = dm.GetILGenerator();
            il.Emit(OpCodes.Ldsfld, fi);
            il.Emit(OpCodes.Ret);
            return (Getter<T>)dm.CreateDelegate(typeof(Getter<T>));
        }

        private readonly struct iterator_propGrid : IEnumerable<uint>, IEnumerable {
            private readonly int x, y;
            public iterator_propGrid(int x, int y) { this.x = x; this.y = y; }
            public IEnumerator<uint> GetEnumerator() {
                uint propID = PropGridGetter()[x * 270 + y];
                while (propID != 0) {
                    yield return propID;
                    propID = m_defBuffer[propID].m_nextGridProp;
                }
            }
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        /// <summary>
        /// This buffer will be set during class initialization
        /// </summary>
        public static EPropInstance[] m_defBuffer;

        /// <summary>
        /// Default prop framework wrapper initialization
        /// </summary>
        public EMLPropWrapper() {
            Type epropManager = Type.GetType(EPROPMANAGER);
            PropBufferGetter = CreateGetter<Array32<EPropInstance>>("m_props");
            PropGridGetter = CreateGetter<uint[]>("m_propGrid");
            ModeGetter = CreateGetter<ItemClass.Availability>("m_mode");
            GetPropLimitScale = CreateGetter<float>("PROP_LIMIT_SCALE");
            PropAnarchyGetter = CreateGetter<bool>("UsePropAnarchy");
            PropSnappingGetter = CreateGetter<bool>("UsePropSnapping");
            delegatedCreateProp = (CREATEPROPAPI)Delegate.CreateDelegate(typeof(CREATEPROPAPI), epropManager, "CreateProp");
            delegatedReleaseProp = (RELEASEPROPAPI)Delegate.CreateDelegate(typeof(RELEASEPROPAPI), epropManager.GetMethod("ReleaseProp", BindingFlags.Public | BindingFlags.Static));
            delegatedMoveProp = (MOVEPROPAPI)Delegate.CreateDelegate(typeof(MOVEPROPAPI), epropManager.GetMethod("MoveProp", BindingFlags.Public | BindingFlags.Static));
            delegatedUpdateProp = (UPDATEPROPAPI)Delegate.CreateDelegate(typeof(UPDATEPROPAPI), epropManager.GetMethod("UpdateProp", BindingFlags.Public | BindingFlags.Static));
            delegatedUpdatePropRenderer = (UPDATEPROPRENDERERAPI)Delegate.CreateDelegate(typeof(UPDATEPROPRENDERERAPI), epropManager.GetMethod("UpdatePropRenderer", BindingFlags.Public | BindingFlags.Static));
            delegatedRayCast = (RAYCASTAPI)Delegate.CreateDelegate(typeof(RAYCASTAPI), epropManager.GetMethod("RayCast", BindingFlags.Public | BindingFlags.Static));
            pmInstance = Singleton<PropManager>.instance;
            m_defBuffer = PropBufferGetter().m_buffer;
        }

        /// <summary>
        /// Check if Prop Anarchy is enabled or disabled
        /// </summary>
        public override bool IsAnarchyEnabled => PropAnarchyGetter();

        /// <summary>
        /// Check if Prop Anarchy is enabled or disabled
        /// </summary>
        public override bool IsSnappingEnabled => PropSnappingGetter();

        /// <summary>
        /// Check whether prop is valid
        /// </summary>
        /// <param name="id">Prop ID</param>
        /// <returns>Returns true if prop is valid, otherwise false</returns>
        public override bool IsValid(uint id) => m_defBuffer[id].m_flags != 0;

        /// <summary>
        /// Check whether prop is valid
        /// </summary>
        /// <param name="id">InstanceID</param>
        /// <returns>Returns true if prop is valid, otherwise false</returns>
        public override bool IsValid(InstanceID id) => m_defBuffer[id.GetProp32()].m_flags != 0;

        /// <summary>
        /// Get prop angle. This method is as performant as getting PropInstance::Angle property
        /// </summary>
        /// <param name="id">Prop id</param>
        /// <returns>Returns prop angle</returns>
        public override float GetAngle(uint id) => m_defBuffer[id].m_angle * 9.58738E-05f;

        /// <summary>
        /// Get prop angle. This method is as performant as getting PropInstance::Angle property
        /// </summary>
        /// <param name="id">InstanceID</param>
        /// <returns>Returns prop angle</returns>
        public override float GetAngle(InstanceID id) => m_defBuffer[id.GetProp32()].m_angle * 9.58738E-05f;

        /// <summary>
        /// Set prop angle. This method is as performant as setting PropInstance::Angle property
        /// </summary>
        /// <param name="id">prop id</param>
        /// <param name="angle">New angle</param>
        public override void SetAngle(uint id, float angle) => m_defBuffer[id].m_angle = (ushort)EMath.RoundToInt(angle * 10430.3779f);

        /// <summary>
        /// Set prop angle. This method is as performant as setting PropInstance::Angle property
        /// </summary>
        /// <param name="id">InstanceID</param>
        /// <param name="angle">New angle</param>
        public override void SetAngle(InstanceID id, float angle) => m_defBuffer[id.GetProp32()].m_angle = (ushort)EMath.RoundToInt(angle * 10430.3779f);

        /// <summary>
        /// Get PropInfo from prop ID
        /// </summary>
        /// <param name="id">prop ID</param>
        /// <returns>Returns PropInfo</returns>
        public override PropInfo GetInfo(uint id) => PrefabCollection<PropInfo>.GetPrefab(m_defBuffer[id].m_infoIndex);

        /// <summary>
        /// Get PropInfo from prop ID
        /// </summary>
        /// <param name="id">InstanceID</param>
        /// <returns>Returns PropInfo</returns>
        public override PropInfo GetInfo(InstanceID id) => PrefabCollection<PropInfo>.GetPrefab(m_defBuffer[id.GetProp32()].m_infoIndex);

        /// <summary>
        /// Get Prop position. This method is actually slightly faster than calling PropInstance::Position property getter
        /// </summary>
        /// <param name="id">prop id</param>
        /// <returns>Returns prop position</returns>
        public override Vector3 GetPosition(uint id) => m_defBuffer[id].Position;

        /// <summary>
        /// Get Prop position. This method is actually slightly faster than calling PropInstance::Position property getter
        /// </summary>
        /// <param name="id">InstanceID</param>
        /// <returns>Returns prop position</returns>
        public override Vector3 GetPosition(InstanceID id) => m_defBuffer[id.GetProp32()].Position;

        /// <summary>
        /// Set prop position. This method is actually many times faster than setting PropInstance::Position property setter due to using more efficient math routines
        /// </summary>
        /// <param name="id">Prop ID</param>
        /// <param name="pos">New position</param>
        public override void SetPosition(uint id, Vector3 pos) => m_defBuffer[id].Position = pos;

        /// <summary>
        /// Set prop position. This method is actually many times faster than setting PropInstance::Position property setter due to using more efficient math routines
        /// </summary>
        /// <param name="id">InstanceID</param>
        /// <param name="pos">New position</param>
        public override void SetPosition(InstanceID id, Vector3 pos) => m_defBuffer[id.GetProp32()].Position = pos;

        /// <summary>
        /// Get prop Single property
        /// </summary>
        /// <param name="id">prop ID</param>
        /// <returns>Returns boolean state of Single</returns>
        public override bool GetSingle(uint id) => (m_defBuffer[id].m_flags & EPropInstance.SINGLEFLAG) != 0;

        /// <summary>
        /// Get prop Single property
        /// </summary>
        /// <param name="id">InstanceID</param>
        /// <returns>Returns boolean state of Single</returns>
        public override bool GetSingle(InstanceID id) => (m_defBuffer[id.GetProp32()].m_flags & EPropInstance.SINGLEFLAG) != 0;

        /// <summary>
        /// Set prop Single property
        /// </summary>
        /// <param name="id">prop ID</param>
        /// <param name="state">True or False</param>
        public override void SetSingle(uint id, bool state) =>
            m_defBuffer[id].m_flags = state ? (ushort)(m_defBuffer[id].m_flags | EPropInstance.SINGLEFLAG) : (ushort)(m_defBuffer[id].m_flags & EPropInstance.SINGLEMASK);

        /// <summary>
        /// Set prop Single property
        /// </summary>
        /// <param name="id">InstanceID</param>
        /// <param name="state">True or False</param>
        public override void SetSingle(InstanceID id, bool state) =>
            m_defBuffer[id.GetProp32()].m_flags = state ? (ushort)(m_defBuffer[id.GetProp32()].m_flags | EPropInstance.SINGLEFLAG) : (ushort)(m_defBuffer[id.GetProp32()].m_flags & EPropInstance.SINGLEMASK);

        /// <summary>
        /// Get prop FixedHeight property
        /// </summary>
        /// <param name="id">prop ID</param>
        /// <returns>Returns FixedHeight boolean state</returns>
        public override bool GetFixedHeight(uint id) => (m_defBuffer[id].m_flags & EPropInstance.FIXEDHEIGHTFLAG) != 0;

        /// <summary>
        /// Get prop FixedHeight property
        /// </summary>
        /// <param name="id">InstanceID</param>
        /// <returns>Returns FixedHeight boolean state</returns>
        public override bool GetFixedHeight(InstanceID id) => (m_defBuffer[id.GetProp32()].m_flags & EPropInstance.FIXEDHEIGHTFLAG) != 0;

        /// <summary>
        /// Set prop FixedHeight property
        /// </summary>
        /// <param name="id">prop ID</param>
        /// <param name="state">Boolean state to set to</param>
        public override void SetFixedHeight(uint id, bool state) => m_defBuffer[id].m_flags = state ?
            (ushort)(m_defBuffer[id].m_flags | EPropInstance.FIXEDHEIGHTFLAG) : (ushort)(m_defBuffer[id].m_flags & EPropInstance.FIXEDHEIGHTMASK);

        /// <summary>
        /// Set prop FixedHeight property
        /// </summary>
        /// <param name="id">Instance</param>
        /// <param name="state">Boolean state to set to</param>
        public override void SetFixedHeight(InstanceID id, bool state) {
            uint propID = id.GetProp32();
            m_defBuffer[propID].m_flags = state ?
            (ushort)(m_defBuffer[propID].m_flags | EPropInstance.FIXEDHEIGHTFLAG) : (ushort)(m_defBuffer[propID].m_flags & EPropInstance.FIXEDHEIGHTMASK);
        }

        /// <summary>
        /// Get prop scale
        /// </summary>
        /// <param name="id">prop ID</param>
        /// <returns>Returns the prop scale</returns>
        public override float GetScale(uint id) => m_defBuffer[id].m_scale;

        /// <summary>
        /// Get prop scale
        /// </summary>
        /// <param name="id">InstanceID</param>
        /// <returns>Returns the prop scale</returns>
        public override float GetScale(InstanceID id) => m_defBuffer[id.GetProp32()].m_scale;

        /// <summary>
        /// Get Prop Color
        /// </summary>
        /// <param name="id">Prop ID</param>
        /// <returns>Returns prop scale</returns>
        public override Color GetColor(uint id) => m_defBuffer[id].m_color;

        /// <summary>
        /// Get Prop Color
        /// </summary>
        /// <param name="id">InstanceID</param>
        /// <returns>Returns prop scale</returns>
        public override Color GetColor(InstanceID id) => m_defBuffer[id.GetProp32()].m_color;

        /// <summary>
        /// Get EPropInstance::m_nextGridProp
        /// </summary>
        /// <param name="id">Prop ID</param>
        /// <returns>Returns the uint value of PropInstance::m_nextGridProp</returns>
        public override uint GetNextGridProp(uint id) => m_defBuffer[id].m_nextGridProp;

        /// <summary>
        /// Get EPropInstance::m_flags
        /// </summary>
        /// <param name="id">Prop ID</param>
        /// <returns>Returns ushort m_flags</returns>
        public override ushort GetFlags(uint id) => m_defBuffer[id].m_flags;

        /// <summary>
        /// Get EPropInstance::m_flags
        /// </summary>
        /// <param name="id">Prop ID</param>
        /// <returns>Returns ushort m_flags</returns>
        public override ushort GetFlags(InstanceID id) => m_defBuffer[id.GetProp32()].m_flags;

        /// <summary>
        /// Set EPropInstance::m_flags
        /// </summary>
        /// <param name="id">Prop ID</param>
        /// <param name="value">Set m_flags to value</param>
        public override void SetFlags(uint id, ushort value) => m_defBuffer[id].m_flags = value;

        /// <summary>
        /// Set EPropInstance::m_flags
        /// </summary>
        /// <param name="id">Prop ID</param>
        /// <param name="value">Set m_flags to value</param>
        public override void SetFlags(InstanceID id, ushort value) => m_defBuffer[id.GetProp32()].m_flags = value;

        /// <summary>
        /// Get EPropInstance buffer
        /// </summary>
        /// <param name="id">Prop ID</param>
        /// <returns>Returns the boxed EPropInstance buffer</returns>
        public override object GetBuffer(uint id) => m_defBuffer[id];

        /// <summary>
        /// Get EPropInstance buffer
        /// </summary>
        /// <param name="id">InstanceID</param>
        /// <returns>Returns the boxed EPropInstance buffer</returns>
        public override object GetBuffer(InstanceID id) => m_defBuffer[id.GetProp32()];

        /// <summary>
        /// Get entire EPropInstance buffer
        /// </summary>
        /// <returns></returns>
        public override object GetRawBuffer() => m_defBuffer;

        /// <summary>
        /// Wrapper for EPropManager::CreateProp
        /// </summary>
        /// <param name="prop">Prop ID</param>
        /// <param name="randomizer">Randomizer</param>
        /// <param name="info">Prefab Info</param>
        /// <param name="position">Position</param>
        /// <param name="angle">Angle</param>
        /// <param name="single">Single</param>
        /// <returns>Returns true if successful otherwise false</returns>
        public override bool CreateProp(out uint prop, ref Randomizer randomizer, PropInfo info, Vector3 position, float angle, bool single) =>
            delegatedCreateProp(pmInstance, out prop, ref randomizer, info, position, angle, single);

        /// <summary>
        /// Simplified version of EPropManager::CreateProp
        /// </summary>
        /// <param name="prop">Prop ID</param>
        /// <param name="info">Prefab Info</param>
        /// <param name="position">Position</param>
        /// <param name="angle">Angle</param>
        /// <param name="single">Single</param>
        /// <returns>Returns true if successful otherwise false</returns>
        public override bool CreateProp(out uint prop, PropInfo info, Vector3 position, float angle, bool single) =>
            delegatedCreateProp(pmInstance, out prop, ref m_randomizer, info, position, angle, single);

        /// <summary>
        /// Wrapper for PropManager::ReleaseProp
        /// </summary>
        /// <param name="prop">Prop ID</param>
        public override void ReleaseProp(uint prop) => delegatedReleaseProp(pmInstance, prop);

        /// <summary>
        /// Wrapper for PropManager::ReleaseProp
        /// </summary>
        /// <param name="id">InstanceID</param>
        public override void ReleaseProp(InstanceID id) => delegatedReleaseProp(pmInstance, id.GetProp32());

        /// <summary>
        /// Move Prop
        /// </summary>
        /// <param name="id">Prop ID</param>
        /// <param name="position">New position</param>
        public override void MoveProp(uint id, Vector3 position) => delegatedMoveProp(pmInstance, id, position);

        /// <summary>
        /// Update prop renderer
        /// </summary>
        /// <param name="id">Prop ID</param>
        /// <param name="updateGroup">Update Group</param>
        public override void UpdatePropRenderer(uint id, bool updateGroup) => delegatedUpdatePropRenderer(pmInstance, id, updateGroup);

        /// <summary>
        /// Render prop
        /// </summary>
        /// <param name="cameraInfo"></param>
        /// <param name="info"></param>
        /// <param name="id"></param>
        /// <param name="position"></param>
        /// <param name="scale"></param>
        /// <param name="angle"></param>
        /// <param name="color"></param>
        /// <param name="objectIndex"></param>
        /// <param name="active"></param>
        public override void RenderInstance(RenderManager.CameraInfo cameraInfo, PropInfo info, InstanceID id, Vector3 position, float scale, float angle, Color color, Vector4 objectIndex, bool active) =>
            EPropInstance.RenderInstance(cameraInfo, info, id, position, scale, angle, color, objectIndex, active);

        /// <summary>
        /// Render prop
        /// </summary>
        /// <param name="cameraInfo"></param>
        /// <param name="info"></param>
        /// <param name="id"></param>
        /// <param name="position"></param>
        /// <param name="scale"></param>
        /// <param name="angle"></param>
        /// <param name="color"></param>
        /// <param name="objectIndex"></param>
        /// <param name="active"></param>
        /// <param name="heightMap"></param>
        /// <param name="heightMapping"></param>
        /// <param name="surfaceMapping"></param>
        public override void RenderInstance(RenderManager.CameraInfo cameraInfo, PropInfo info, InstanceID id, Vector3 position, float scale, float angle, Color color, Vector4 objectIndex, bool active, Texture heightMap, Vector4 heightMapping, Vector4 surfaceMapping) =>
            EPropInstance.RenderInstance(cameraInfo, info, id, position, scale, angle, color, objectIndex, active, heightMap, heightMapping, surfaceMapping);

        /// <summary>
        /// Wrapper for PropInstance::RayCast()
        /// </summary>
        /// <param name="propID"></param>
        /// <param name="ray"></param>
        /// <param name="t"></param>
        /// <param name="targetSqr"></param>
        /// <returns>Returns true on hit, else false</returns>
        public override bool RayCast(uint propID, Segment3 ray, out float t, out float targetSqr) => m_defBuffer[propID].RayCast(propID, ray, out t, out targetSqr);

        /// <summary>
        /// Wrapper for PropManager::UpdateProp
        /// </summary>
        /// <param name="propID">Prop ID</param>
        public override void UpdateProp(uint propID) => delegatedUpdateProp(propID);

        /// <summary>
        /// Wrapper for PropManager::UpdateProps
        /// </summary>
        /// <param name="minX"></param>
        /// <param name="minZ"></param>
        /// <param name="maxX"></param>
        /// <param name="maxZ"></param>
        public override void UpdateProps(float minX, float minZ, float maxX, float maxZ) => pmInstance.UpdateProps(minX, minZ, maxX, maxZ);

        /// <summary>
        /// Special IEnumerable to use for iterating through props in grid
        /// </summary>
        /// <param name="x">x position in grid</param>
        /// <param name="y">y position in grid</param>
        /// <returns>Returns IEnumerable for use in foreach</returns>
        public override IEnumerable<uint> GetPropGridEnumerable(int x, int y) => new iterator_propGrid(x, y);
    }
#pragma warning restore IDE1006 // Naming Styles
}
