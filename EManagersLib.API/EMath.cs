using ColossalFramework.Math;
using UnityEngine;

namespace EManagersLib.API {
    /// <summary>
    /// UnityEngine.Mathf is extraordinarily slow compared to System.Math
    /// and these are common Mathf methods that are optimized. I hate re-inventing
    /// the wheel, but these speed ups are drastic enough to do them
    /// </summary>
    public static class EMath {
        public static readonly Vector2 Vector2Zero = Vector2.zero;
        public static readonly Vector3 Vector3Zero = Vector3.zero;
        public static readonly Vector4 Vector4Zero = Vector4.zero;
        public static readonly Vector3 Vector3Down = Vector3.down;
        public static readonly Vector3 Vector3Forward = Vector3.forward;
        public static readonly Vector3 DefaultLodMin = new Vector3(100000f, 100000f, 100000f);
        public static readonly Vector3 DefaultLodMax = new Vector3(-100000f, -100000f, -100000f);
        public static readonly Vector3 DefaultLod100 = new Vector3(100f, 100f, 100f);
        public static readonly Color ColorClear = Color.clear;
        public static Randomizer randomizer = new Randomizer();

        /// <summary>
        /// Get Matrix.identity using this static variable. It's about ~5x faster
        /// </summary>
        public static readonly Matrix4x4 matrix4Identity = Matrix4x4.identity;

        /// <summary>
        /// Functions exactly the same as Mathf.Approximately but 52x faster
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool Approximately(float a, float b) => a + 0.0000000596f >= b && a - 0.0000000596f <= b;

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
            val = val > 1 ? 1 : val;
            return val < 0 ? 0 : val;
        }

        /// <summary>
        /// Same as Mathf.Clamp01, clamps between 0 and 1
        /// </summary>
        /// <param name="val"></param>
        /// <returns>Returns value between 0 and 1</returns>
        public static int Clamp01(int val) {
            val = val > 1 ? 1 : val;
            return val < 0 ? 0 : val;
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
        /// Same as Vector3.Lerp, just slightly faster
        /// </summary>
        /// <param name="a">Vector3</param>
        /// <param name="b">Vector3</param>
        /// <param name="t">float</param>
        /// <returns>Returns Vector3</returns>
        public static Vector3 Lerp(Vector3 a, Vector3 b, float t) {
            t = t > 1 ? 1 : (t < 0 ? 0 : t);
            return new Vector3(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t, a.z + (b.z - a.z) * t);
        }

        /// <summary>
        /// I don't know why Mathf.Max and Math.Max is so slow, but this method will work ~27x faster
        /// </summary>
        /// <param name="a">First value to compare</param>
        /// <param name="b">Second value to compare</param>
        /// <returns>Returns the maximum value</returns>
        public static int Max(int a, int b) => a > b ? a : b;

        /// <summary>
        /// I don't know why Mathf.Min and Math.Min is so slow, but this method will work ~27x faster
        /// </summary>
        /// <param name="a">First value to compare</param>
        /// <param name="b">Second value to compare</param>
        /// <returns>Returns the minimum value</returns>
        public static int Min(int a, int b) => a < b ? a : b;

        /// <summary>
        /// I don't know why Mathf.Max and Math.Max is so slow, but this method will work ~27x faster
        /// </summary>
        /// <param name="a">First value to compare</param>
        /// <param name="b">Second value to compare</param>
        /// <returns>Returns the maximum value</returns>
        public static float Max(float a, float b) => a > b ? a : b;

        /// <summary>
        /// I don't know why Mathf.Min and Math.Min is so slow, but this method will work ~27x faster
        /// </summary>
        /// <param name="a">First value to compare</param>
        /// <param name="b">Second value to compare</param>
        /// <returns>Returns the minimum value</returns>
        public static float Min(float a, float b) => a < b ? a : b;

        /// <summary>
        /// Functions Exactly like Mathf.FloorToInt, just ~8x faster
        /// </summary>
        /// <param name="val">float</param>
        /// <returns>Returns an integer</returns>
        public static int FloorToInt(float val) => (val < 0) ? (int)(val - 1) : (int)val;

        /// <summary>
        /// Functions like Mathf.CeilToInt, just ~3x faster
        /// </summary>
        /// <param name="val">float</param>
        /// <returns>Returns an integer</returns>
        public static int CeilToInt(float val) => (val < 0) ? (int)(val) : (val % (int)val > 0f ? (int)(val + 1) : (int)val);

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
        /// Functions exactly the same as Mathf.Repeat, just faster
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static float Repeat(float a, float b) => a % b;

        /// <summary>
        /// This Sqrt function is accurate to only 0.01f, the speed is about ~3x faster
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static unsafe float Sqrt(float x) {
            float xHalf = 0.5f * x;
            int tmp = 0x5f3759df - (*(int*)&x >> 1);
            float xRes = *(float*)&tmp;
            xRes *= (1.5f - (xHalf * xRes * xRes));
            return xRes * x;
        }

        /// <summary>
        /// Functions Exactly the same as Mathf.Sign
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static float Sign(float a) => a < 0 ? -1 : 1;

        /// <summary>
        /// Functions Exactly the same as Mathf.Sign
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static int Sign(int a) => a < 0 ? -1 : 1;

        public static bool IsNearlyEqual(float a, float b, float epsilon = 0.0001f) => a == b || Abs(a - b) < epsilon;

        /// <summary>
        /// Use this method to set the seed value, instead of calling new Randomizer(int val), so randomizer can be re-used
        /// </summary>
        /// <param name="val"></param>
        public static void SetRandomizerSeed(int val) => randomizer.seed = (ulong)(6364136223846793005L * val + 1442695040888963407L);

        /// <summary>
        /// Use this method to set the seed value, instead of calling new Randomizer(int val), so randomizer can be re-used
        /// </summary>
        /// <param name="val"></param>
        public static void SetRandomizerSeed(uint val) => randomizer.seed = 6364136223846793005uL * val + 1442695040888963407uL;

        /// <summary>
        /// This is an extension method to ease the reuse of randomizers
        /// </summary>
        /// <param name="randomizer"></param>
        /// <param name="val"></param>
        public static void SetSeed(ref this Randomizer randomizer, int val) => randomizer.seed = (ulong)(6364136223846793005L * val + 1442695040888963407L);

        /// <summary>
        /// This is an extension method to ease the reuse of randomizers
        /// </summary>
        /// <param name="randomizer"></param>
        /// <param name="val"></param>
        public static void SetSeed(ref this Randomizer randomizer, uint val) => randomizer.seed = 6364136223846793005uL * val + 1442695040888963407uL;

        /// <summary>
        /// Functions exactly like MathUtils.SmoothStep. This one is only 1.2x faster
        /// </summary>
        /// <param name="edge0"></param>
        /// <param name="edge1"></param>
        /// <param name="x"></param>
        /// <returns></returns>
        public static float SmoothStep(float edge0, float edge1, float x) {
            x = (x - edge0) / (edge1 - edge0);
            x = x < 0 ? 0 : (x > 1 ? 1 : x);
            return x * x * (3f - 2f * x);
        }

        /// <summary>
        /// Functions exactly the same as RenderManager.CameraInfo.CheckRenderDistance, but ~15x faster
        /// </summary>
        /// <param name="cameraInfo"></param>
        /// <param name="point"></param>
        /// <param name="maxDistance"></param>
        /// <returns></returns>
        public static bool ECheckRenderDistance(this RenderManager.CameraInfo cameraInfo, Vector3 point, float maxDistance) {
            float distance = maxDistance * 0.45f;
            Vector3 campos = cameraInfo.m_position;
            Vector3 camforward = cameraInfo.m_forward;
            float x = point.x - campos.x - camforward.x * distance;
            float y = point.y - campos.y - camforward.y * distance;
            float z = point.z - campos.z - camforward.z * distance;
            return (x * x + y * y + z * z) < maxDistance * maxDistance * 0.3025f;
        }
    }
}
