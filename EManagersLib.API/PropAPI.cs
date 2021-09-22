using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EManagersLib.API {
    public static class PropAPI {
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
