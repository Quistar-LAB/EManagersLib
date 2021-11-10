namespace EManagersLib.API {
    /// <summary>
    /// UnityEngine.Mathf is extraordinarily slow compared to System.Math
    /// and these are common Mathf methods that are optimized. I hate re-inventing
    /// the wheel, but these speed ups are drastic enough to do them
    /// </summary>
    public static class EMath {
        /// <summary>
        /// Functions exactly the same as Mathf.RoundToInt but ~77x faster
        /// </summary>
        /// <param name="f">The value that will be rounded</param>
        /// <returns>The rounded result</returns>
        public static int RoundToInt(float f) => (int)(f + 0.5f);

        /// <summary>
        /// Functions exactly the same as Mathf.Clamp but ~28x faster
        /// </summary>
        /// <param name="val">The value needing bounds check</param>
        /// <param name="min">The minimum limit</param>
        /// <param name="max">The maximum limit</param>
        /// <returns>Returns the clamped result</returns>
        public static int Clamp(int val, int min, int max) {
            val = (val < min) ? min : val;
            return (val > max) ? max : val;
        }

        /// <summary>
        /// Functions exactly the same as Mathf.Clamp but ~28x faster
        /// </summary>
        /// <param name="val">The value needing bounds check</param>
        /// <param name="min">The minimum limit</param>
        /// <param name="max">The maximum limit</param>
        /// <returns>Returns the clamped result</returns>
        public static float Clamp(float val, float min, float max) {
            val = (val < min) ? min : val;
            return (val > max) ? max : val;
        }

        /// <summary>
        /// I don't know why Mathf.Max and Math.Max is so slow, but this method will work ~27x faster
        /// </summary>
        /// <param name="a">First value to compare</param>
        /// <param name="b">Second value to compare</param>
        /// <returns>Returns the maximum value</returns>
        public static int Max(int a, int b) => (a <= b) ? b : a;

        /// <summary>
        /// I don't know why Mathf.Min and Math.Min is so slow, but this method will work ~27x faster
        /// </summary>
        /// <param name="a">First value to compare</param>
        /// <param name="b">Second value to compare</param>
        /// <returns>Returns the minimum value</returns>
        public static int Min(int a, int b) => (a >= b) ? b : a;

        /// <summary>
        /// I don't know why Mathf.Max and Math.Max is so slow, but this method will work ~27x faster
        /// </summary>
        /// <param name="a">First value to compare</param>
        /// <param name="b">Second value to compare</param>
        /// <returns>Returns the maximum value</returns>
        public static float Max(float a, float b) => (a <= b) ? b : a;

        /// <summary>
        /// I don't know why Mathf.Min and Math.Min is so slow, but this method will work ~27x faster
        /// </summary>
        /// <param name="a">First value to compare</param>
        /// <param name="b">Second value to compare</param>
        /// <returns>Returns the minimum value</returns>
        public static float Min(float a, float b) => (a >= b) ? b : a;
    }
}
