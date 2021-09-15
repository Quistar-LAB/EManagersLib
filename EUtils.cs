using HarmonyLib;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace EManagersLib {
    public static class EUtils {
        private const string m_debugLogFile = "oEManagerDebug.log";
        public static Func<S, T> CreateGetter<S, T>(FieldInfo field) {
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

        private static readonly Stopwatch profiler = new Stopwatch();
        internal static void CreateDebugFile() {
            profiler.Start();
            /* Create Debug Log File */
            string path = Path.Combine(Application.dataPath, m_debugLogFile);
            using (FileStream debugFile = new FileStream(path, FileMode.Create, FileAccess.ReadWrite, FileShare.None))
            using (StreamWriter sw = new StreamWriter(debugFile)) {
                sw.WriteLine($"--- {EModule.m_modName} {EModule.m_modVersion} Debug File ---");
                sw.WriteLine(Environment.OSVersion);
                sw.WriteLine($"C# CLR Version {Environment.Version}");
                sw.WriteLine($"Unity Version {Application.unityVersion}");
                sw.WriteLine("-------------------------------------");
            }
        }

        internal static void ELog(string msg) {
            var ticks = profiler.ElapsedTicks;
            using (FileStream debugFile = new FileStream(Path.Combine(Application.dataPath, m_debugLogFile), FileMode.Append))
            using (StreamWriter sw = new StreamWriter(debugFile)) {
                sw.WriteLine($"{(ticks / Stopwatch.Frequency):n0}:{(ticks % Stopwatch.Frequency):D7}-{new StackFrame(1, true).GetMethod().Name} ==> {msg}");
            }
        }

        public static void EnablePropPatches() {
            Harmony harmony = new Harmony(EModule.HARMONYID);
            new EPropManagerPatch().Enable(harmony);
            new EDefaultToolPatch().Enable(harmony);
            new EBulldozePatch().Enable(harmony);
            new EDisasterHelpersPatch().Enable(harmony);
            new EDistrictManagerPatch().Enable(harmony);
            new EInstanceManagerPatch().Enable(harmony);
            new EPropToolPatch().Enable(harmony);
            new EBuildingAIPatch().Enable(harmony);
        }

        public static void DisablePropPatches() {
            Harmony harmony = new Harmony(EModule.HARMONYID);
            new EPropManagerPatch().Disable(harmony);
            new EDefaultToolPatch().Disable(harmony);
            new EBulldozePatch().Disable(harmony);
            new EDisasterHelpersPatch().Disable(harmony);
            new EDistrictManagerPatch().Disable(harmony);
            new EInstanceManagerPatch().Disable(harmony);
            new EPropToolPatch().Disable(harmony);
            new EBuildingAIPatch().Disable(harmony);
        }
    }
}
