using UnityEngine.Collections;

namespace EManagersLib {
    public static class EArray32 {
        public static Array32<EPropInstance> CreateNativePropArray(int size) {
            NativeArray<EPropInstance> nativeBuf = new NativeArray<EPropInstance>(size, Allocator.Persistent);
            Array32<EPropInstance> propBuf = new Array32<EPropInstance>((uint)size);
            return propBuf;
        }
    }
}
