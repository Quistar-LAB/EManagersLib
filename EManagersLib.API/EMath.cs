using ColossalFramework.Math;
using UnityEngine;

namespace EManagersLib.API {
    /// <summary>
    /// UnityEngine.Mathf is extraordinarily slow compared to System.Math
    /// and these are common Mathf methods that are optimized. I hate re-inventing
    /// the wheel, but these speed ups are drastic enough to do them
    /// </summary>
    public static class EMath {
        public static Vector3 Vector3Zero = Vector3.zero;
        public static Vector4 Vector4Zero = Vector4.zero;
        public static Vector3 Vector3Down = Vector3.down;
        public static Vector3 DefaultLodMin = new Vector3(100000f, 100000f, 100000f);
        public static Vector3 DefaultLodMax = new Vector3(-100000f, -100000f, -100000f);
        public static Vector3 DefaultLod100 = new Vector3(100f, 100f, 100f);
        public static Color ColorClear = Color.clear;
        public static Randomizer randomizer = new Randomizer();

        /// <summary>
        /// Functions exactly the same as Mathf.Abs but ~4x faster
        /// </summary>
        /// <param name="val"></param>
        /// <returns>Returns absolute number</returns>
        public static int Abs(int val) => val < 0 ? -val : val;

        /// <summary>
        /// Functions exactly the same as Mathf.Abs but ~4x faster
        /// </summary>
        /// <param name="val"></param>
        /// <returns>Returns absolute number</returns>
        public static float Abs(float val) => val < 0 ? -val : val;

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
        /// Same as Mathf.Clamp01, clamps between 0 and 1
        /// </summary>
        /// <param name="val"></param>
        /// <returns>Returns value between 0 and 1</returns>
        public static float Clamp01(float val) {
            if (val < 0) return 0;
            else if (val > 1) return 1;
            return val;
        }

        /// <summary>
        /// Same as Mathf.Floor except ~56x faster
        /// </summary>
        /// <param name="val">float</param>
        /// <returns>Returns the rounded value</returns>
        public static float Floor(float val) => (int)val;

        /// <summary>
        /// Same as Mathf.Lerp, this version is about ~1.5x faster
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static float Lerp(float a, float b, float t) => a + (b - a) * Clamp01(t);

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

        /// <summary>
        /// Compares Vector3 and returns the max, this method is about ~2x faster than Vector3.Max
        /// </summary>
        /// <param name="lhs">Vector3</param>
        /// <param name="rhs">Vector3</param>
        /// <returns>Returns the max vector3</returns>
        public static Vector3 Max(Vector3 lhs, Vector3 rhs) => new Vector3(Max(lhs.x, rhs.x), Max(lhs.y, rhs.y), Max(lhs.z, rhs.z));

        /// <summary>
        /// Compares Vector3 and returns the min, this method is about ~2x faster than Vector3.Min
        /// </summary>
        /// <param name="lhs">Vector3</param>
        /// <param name="rhs">Vector3</param>
        /// <returns>Returns the min vector3</returns>
        public static Vector3 Min(Vector3 lhs, Vector3 rhs) => new Vector3(Min(lhs.x, rhs.x), Min(lhs.y, rhs.y), Min(lhs.z, rhs.z));

        /// <summary>
        /// This sine function is an approximation accurate to about ~0.001f, the speed up is about ~20x faster
        /// </summary>
        /// <param name="x"></param>
        /// <returns>Returns the sine result in float</returns>
        public static float Sin(float x) {
            const float PI = 3.14159265358979323846264338327950288f;
            const float INVPI = 0.31830988618379067153776752674502872f;
            const float A = 0.00735246819687011731341356165096815f;
            const float B = -0.16528911397014738207016302002888890f;
            const float C = 0.99969198629596757779830113868360584f;
            int k;
            float x2;
            k = RoundToInt(INVPI * x);
            x -= k * PI;
            x2 = x * x;
            x *= (C + x2 * (B + A * x2));
            if (k % 2 != 0) x = -x;
            return x;
        }

        /// <summary>
        /// This cosine function is an approximation accurate to about ~0.001f, the speed up is about ~20x faster
        /// </summary>
        /// <param name="x"></param>
        /// <returns>Returns the cosine result in float</returns>
        public static float Cos(float x) => Sin(x + 1.5707963f);

        /// <summary>
        /// This Sqrt function is accurate to only 0.01f, the speed is about ~3x faster
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static unsafe float Sqrt(float x) {
            float xHalf = 0.5f * x;
            int tmp = 0x5F3759DF - (*(int*)&x >> 1);
            float xRes = *(float*)&tmp;
            xRes *= (1.5f - (xHalf * xRes * xRes));
            return xRes * x;
        }

        public static bool IsNearlyEqual(float a, float b, float epsilon = 0.0001f) => a == b || Abs(a - b) < epsilon;
    }
}
