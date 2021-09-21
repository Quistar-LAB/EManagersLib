﻿using ColossalFramework;
using ColossalFramework.IO;
using ICities;
using System;
using System.IO;
using System.Threading;
using static EManagersLib.EPropManager;

namespace EManagersLib {
    public class ESerializableData : ISerializableDataExtension {
        private const string EMANAGER_PROP_KEY = @"EManagers/PropAnarchy";
        private enum Format : uint {
            Version1 = 1,
            Version2 = 2
        }

        private class Data : IDataContainer {
            private void EnsureCapacity(int maxLimit, out Array32<EPropInstance> newArray, out EPropInstance[] propBuffer) {
                if (maxLimit > MAX_PROP_LIMIT) {
                    EPropInstance[] oldBuffer = m_props.m_buffer;
                    newArray = new Array32<EPropInstance>((uint)maxLimit);
                    newArray.CreateItem(out uint _);
                    newArray.ClearUnused();
                    propBuffer = newArray.m_buffer;
                    for (int i = 1; i < DEFAULT_PROP_LIMIT; i++) {
                        if (oldBuffer[i].m_flags != 0) {
                            propBuffer[i].m_flags = oldBuffer[i].m_flags;
                            propBuffer[i].m_infoIndex = oldBuffer[i].m_infoIndex;
                            propBuffer[i].m_posX = oldBuffer[i].m_posX;
                            propBuffer[i].m_posZ = oldBuffer[i].m_posZ;
                            propBuffer[i].m_angle = oldBuffer[i].m_angle;
                        }
                    }
                    return;
                }
                newArray = m_props;
                propBuffer = m_props.m_buffer;
            }

            private void RepackBuffer(int maxLimit, int propCount, Format version, Array32<EPropInstance> existingPropBuffer) {
                if (maxLimit > MAX_PROP_LIMIT) {
                    if (propCount > MAX_PROP_LIMIT) {
                        m_props = existingPropBuffer;
                        //UpdatePropLimit(maxLimit);
                        /* UpdatePropLimit first so PropScaleFactor is updated for next statement */
                        m_updatedProps = new ulong[MAX_UPDATEDPROP_LIMIT];
                        return; /* Just return with existing buffers */
                    }
                    /* Pack the result into old buffer as we are sure there are enough space to fit in buffer */
                    EPropInstance[] existingBuffer = existingPropBuffer.m_buffer;
                    EPropInstance[] oldBuffer = m_props.m_buffer;
                    /* make sure to fill in 1~262144 props first */
                    for (int i = 1; i < DEFAULT_PROP_LIMIT; i++) {
                        if (existingBuffer[i].m_flags != 0) {
                            oldBuffer[i].m_posY = existingBuffer[i].m_posY;
                            oldBuffer[i].m_scale = existingBuffer[i].m_scale;
                            oldBuffer[i].m_color = existingBuffer[i].m_color;
                        }
                    }
                    for (uint i = DEFAULT_PROP_LIMIT, offsetIndex = 1; i < existingBuffer.Length; i++) {
                        if (existingBuffer[i].m_flags != 0) {
                            while (oldBuffer[offsetIndex].m_flags != 0) { offsetIndex++; } /* Find available slot in old buffer */
                            oldBuffer[offsetIndex].m_flags = existingBuffer[i].m_flags;
                            oldBuffer[offsetIndex].m_infoIndex = existingBuffer[i].m_infoIndex;
                            oldBuffer[offsetIndex].m_posX = existingBuffer[i].m_posX;
                            oldBuffer[offsetIndex].m_posZ = existingBuffer[i].m_posZ;
                            oldBuffer[offsetIndex].m_angle = existingBuffer[i].m_angle;
                            oldBuffer[offsetIndex].m_posY = existingBuffer[i].m_posY;
                            oldBuffer[offsetIndex].m_scale = existingBuffer[i].m_scale;
                            oldBuffer[offsetIndex].m_color = existingBuffer[i].m_color;
                        }
                    }
                }
            }

            public void Deserialize(DataSerializer s) {
                int maxLen = s.ReadInt32(); // Read in Max limit
                int propCount = 0;
                EUtils.ELog($"max length={maxLen}");
                EnsureCapacity(maxLen, out Array32<EPropInstance> newBuffer, out EPropInstance[] props);
                EncodedArray.UShort uShort = EncodedArray.UShort.BeginRead(s);
                for (int i = DEFAULT_PROP_LIMIT; i < maxLen; i++) {
                    props[i].m_flags = uShort.Read();
                }
                uShort.EndRead();
                PrefabCollection<PropInfo>.BeginDeserialize(s);
                for (int i = 1; i < maxLen; i++) {
                    if (props[i].m_flags != 0) {
                        props[i].m_infoIndex = (ushort)PrefabCollection<PropInfo>.Deserialize(true);
                        propCount++;
                    }
                }
                PrefabCollection<PropInfo>.EndDeserialize(s);
                EncodedArray.Short @short = EncodedArray.Short.BeginRead(s);
                for (int i = DEFAULT_PROP_LIMIT; i < maxLen; i++) {
                    if (props[i].m_flags != 0) {
                        props[i].m_posX = @short.Read();
                    } else {
                        props[i].m_posX = 0;
                    }
                }
                @short.EndRead();
                EncodedArray.Short @short1 = EncodedArray.Short.BeginRead(s);
                for (int i = DEFAULT_PROP_LIMIT; i < maxLen; i++) {
                    if (props[i].m_flags != 0) {
                        props[i].m_posZ = @short1.Read();
                    } else {
                        props[i].m_posZ = 0;
                    }
                }
                @short1.EndRead();
                EncodedArray.UShort uShort2 = EncodedArray.UShort.BeginRead(s);
                for (int i = DEFAULT_PROP_LIMIT; i < maxLen; i++) {
                    if (props[i].m_flags != 0) {
                        props[i].m_angle = uShort2.Read();
                    } else {
                        props[i].m_angle = 0;
                    }
                }
                uShort2.EndRead();
                //EncodedArray.UShort uShort1 = EncodedArray.UShort.BeginRead(s);
                //for (int i = 1; i < maxLen; i++) {
                //    if ((props[i].m_flags & EPropInstance.FIXEDHEIGHTFLAG) != 0) {
                //        props[i].m_posY = uShort1.Read();
                //    } else {
                //        props[i].m_posY = 0;
                //    }
                //}
                //uShort1.EndRead();
                EncodedArray.Float @float = EncodedArray.Float.BeginRead(s);
                for (int i = 1; i < maxLen; i++) {
                    if (props[i].m_flags != 0) {
                        props[i].m_scale = @float.Read();
                    }
                }
                @float.EndRead();
                EncodedArray.Float @float1 = EncodedArray.Float.BeginRead(s);
                for (int i = 1; i < maxLen; i++) {
                    if (props[i].m_flags != 0) {
                        props[i].m_color.r = @float1.Read();
                        props[i].m_color.g = @float1.Read();
                        props[i].m_color.b = @float1.Read();
                        props[i].m_color.a = @float1.Read();
                    }
                }
                @float1.EndRead();
                /* Now Resize / Repack buffer if necessary */
                RepackBuffer(maxLen, propCount, (Format)s.version, newBuffer);
            }

            public void AfterDeserialize(DataSerializer s) { }

            public void Serialize(DataSerializer s) {
                int propLimit = MAX_PROP_LIMIT;
                EPropInstance[] buffer = m_props.m_buffer;
                // Important to save proplimit as it is an adjustable variable on every load
                s.WriteInt32(propLimit);
                EUtils.ELog($"Saving limit: {propLimit}");
                EncodedArray.UShort uShort = EncodedArray.UShort.BeginWrite(s);
                for (int i = DEFAULT_PROP_LIMIT; i < propLimit; i++) {
                    uShort.Write(buffer[i].m_flags);
                }
                uShort.EndWrite();
                try {
                    PrefabCollection<PropInfo>.BeginSerialize(s);
                    for (int i = 1; i < propLimit; i++) {
                        if (buffer[i].m_flags != 0) {
                            PrefabCollection<PropInfo>.Serialize(buffer[i].m_infoIndex);
                        }
                    }
                } finally {
                    PrefabCollection<PropInfo>.EndSerialize(s);
                }
                EncodedArray.Short @short = EncodedArray.Short.BeginWrite(s);
                for (int i = DEFAULT_PROP_LIMIT; i < propLimit; i++) {
                    if (buffer[i].m_flags != 0) {
                        @short.Write(buffer[i].m_posX);
                    }
                }
                @short.EndWrite();
                EncodedArray.Short @short1 = EncodedArray.Short.BeginWrite(s);
                for (int i = DEFAULT_PROP_LIMIT; i < propLimit; i++) {
                    if (buffer[i].m_flags != 0) {
                        @short1.Write(buffer[i].m_posZ);
                    }
                }
                @short1.EndWrite();
                EncodedArray.UShort uShort1 = EncodedArray.UShort.BeginWrite(s);
                for (int i = DEFAULT_PROP_LIMIT; i < propLimit; i++) {
                    if (buffer[i].m_flags != 0) {
                        uShort1.Write(buffer[i].m_angle);
                    }
                }
                uShort1.EndWrite();
                //EncodedArray.UShort uShort2 = EncodedArray.UShort.BeginWrite(s);
                //for (int i = 1; i < propLimit; i++) {
                //    if ((buffer[i].m_flags & EPropInstance.FIXEDHEIGHTFLAG) != 0) {
                //        uShort1.Write(buffer[i].m_posY);
                //    }
                //}
                //uShort2.EndWrite();
                EncodedArray.Float @float = EncodedArray.Float.BeginWrite(s);
                for (int i = 1; i < propLimit; i++) {
                    if (buffer[i].m_flags != 0) {
                        @float.Write(buffer[i].m_scale);
                    }
                }
                @float.EndWrite();
                EncodedArray.Float @float1 = EncodedArray.Float.BeginWrite(s);
                for (int i = 1; i < propLimit; i++) {
                    if (buffer[i].m_flags != 0) {
                        @float1.Write(buffer[i].m_color.r);
                        @float1.Write(buffer[i].m_color.g);
                        @float1.Write(buffer[i].m_color.b);
                        @float1.Write(buffer[i].m_color.a);
                    }
                }
                @float1.EndWrite();
            }
        }

        public static void IntegratedPropDeserialize(EPropInstance[] props) {
            try {
                if (Singleton<SimulationManager>.instance.m_serializableDataStorage.TryGetValue(EMANAGER_PROP_KEY, out byte[] data)) {
                    if (data is null) {
                        EUtils.ELog("No extra props to load");
                        return;
                    }
                    using (MemoryStream stream = new MemoryStream(data)) {
                        DataSerializer.Deserialize<Data>(stream, DataSerializer.Mode.Memory);
                    }
                }
                EUtils.ProcessQueues();
            } catch (Exception e) {
                UnityEngine.Debug.LogException(e);
            }
        }

        public void OnSaveData() {
            try {
                byte[] data;
                using (var stream = new MemoryStream()) {
                    DataSerializer.Serialize(stream, DataSerializer.Mode.Memory, (uint)Format.Version1, new Data());
                    data = stream.ToArray();
                }
                SaveData(EMANAGER_PROP_KEY, data);
                EUtils.ELog($"Saved {data.Length} bytes of data");
            } catch (Exception e) {
                UnityEngine.Debug.LogException(e);
            }
        }

        private void SaveData(string id, byte[] data) {
            SimulationManager smInstance = Singleton<SimulationManager>.instance;
            while (!Monitor.TryEnter(smInstance.m_serializableDataStorage, SimulationManager.SYNCHRONIZE_TIMEOUT)) { }
            try {
                smInstance.m_serializableDataStorage[id] = data;
            } finally {
                Monitor.Exit(smInstance.m_serializableDataStorage);
            }
        }

        public void OnCreated(ISerializableData serializedData) { }
        public void OnLoadData() { }
        public void OnReleased() { }
    }
}
