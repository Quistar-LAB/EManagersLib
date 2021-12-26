using System.Collections.Generic;
using UnityEngine.Collections;

namespace EManagersLib {
    public static class EArray32 {
        public static Array32<EPropInstance> CreateNativePropArray(int size) {
            NativeArray<EPropInstance> nativeBuf = new NativeArray<EPropInstance>(size, Allocator.Persistent);
            Array32<EPropInstance> propBuf = new Array32<EPropInstance>((uint)size);
            return propBuf;
        }
    }

    public static class Array32Extension {
        public static uint[] m_unusedItems;
        public static int m_unusedCount;
        public static IEnumerable<uint> NextFreeItems<T>(this Array32<T> array, int numItems) {
            if (m_unusedCount >= numItems) {
                for (int i = 0; i < numItems; i++) {
                    yield return m_unusedItems[m_unusedCount - i];
                }
            }
        }
    }
}
