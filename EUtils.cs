using ColossalFramework.PlatformServices;
using EManagersLib.Extra;
using HarmonyLib;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace EManagersLib {
    /// <summary>
    /// This class provides public API to ensure that mods using this library functions correctly
    /// </summary>
    public static class EUtils {
        private const string m_debugLogFile = "oEManagerDebug.log";
        public delegate U RefGetter<U>();

        /// <summary>Make sure to cache the return value of this property before using it in a loop</summary>
        /// <returns>Returns the maximum prop limit set by Prop Anarchy. If Prop Anarchy is not installed, then returns default prop limit</returns>
        public static int GetPropMaxLimit => EPropManager.MAX_PROP_LIMIT;

        /// <summary>This library will call the queued action callback late in the initialization to ensure prop buffer points to the correct location</summary>
        internal static void ProcessQueues() { }

        /// <summary>
        /// Helper API to create delegates to get private or protected field members that would usually be accessed
        /// using slow reflection codes
        /// </summary>
        /// <typeparam name="S">Type of class where the field resides</typeparam>
        /// <typeparam name="T">Name of the private or protected field</typeparam>
        /// <param name="field"></param>
        /// <returns>Returns the delegate for fast getter to private or protected fields</returns>
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

        public static RefGetter<U> CreatePrefabRefGetter<U>(string s_field) {
            var prefab = typeof(PrefabCollection<PropInfo>);
            var fi = prefab.GetField(s_field, BindingFlags.NonPublic | BindingFlags.Static);
            if (fi == null) throw new MissingFieldException(prefab.Name, s_field);
            var s_name = "__refget_" + prefab.Name + "_fi_" + fi.Name;
            var dm = new DynamicMethod(s_name, typeof(U), new[] { prefab }, prefab, true);
            var il = dm.GetILGenerator();
            il.Emit(OpCodes.Ldsfld, fi);
            il.Emit(OpCodes.Ret);
            return (RefGetter<U>)dm.CreateDelegate(typeof(RefGetter<U>));
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

        public readonly struct ModInfo {
            public readonly ulong fileID;
            public readonly string name;
            public readonly string specialMsg;
            public readonly bool inclusive;
            public ModInfo(ulong modID, string modName, bool isInclusive) {
                fileID = modID;
                name = modName;
                specialMsg = null;
                inclusive = isInclusive;
            }
            public ModInfo(ulong modID, string modName, bool isInclusive, string extraMsg) {
                fileID = modID;
                name = modName;
                specialMsg = extraMsg;
                inclusive = isInclusive;
            }
        }

        private static readonly ModInfo[] IncompatibleMods = new ModInfo[] {
            //new ModInfo(1619685021, @"Move It", false, @"Only Move It Beta is supported at the moment"),
            new ModInfo(593588108, @"Prop & Tree Anarchy", false, @"Prop Anarchy Beta and Tree Anarchy Beta together replaces Prop & Tree Anarchy"),
            new ModInfo(791221322, @"Prop Precision", false, @"Use Prop Anarchy Beta instead"),
            new ModInfo(787611845, @"Prop Snapping", false, @"Use Prop Anarchy Beta instead"),
            new ModInfo(767233815, @"Decal Prop Fix", false, @"Use Prop Anarchy Beta instead"),
            new ModInfo(694512541, @"Prop Line Tool", false, @"Use Prop Anarchy Beta instead"),
            new ModInfo(911295408, @"Prop Scaler", false, @"Use Prop Anarchy Beta instead"),
            new ModInfo(1869561285, @"Prop Painter", false, @"Use Prop Anarchy Beta instead"),
            new ModInfo(1410003347, @"Additive Shader by Ronyx69", false, @"Use Prop Anarchy Beta instead"),
            new ModInfo(2119477759, @"[TEST]Additive Shader by aubergine18", false, @"Use Prop Anarchy Beta instead"),
            new ModInfo(878991312, @"Prop It Up", false, @"Use BOB instead"),
            new ModInfo(2153618633, @"Prop Switcher", false, @"Use BOB instead"),
            new ModInfo(518456166, @"Prop Remover", false, @"Use BOB instead"),
            new ModInfo(1890830956, @"Undo It", false),
        };

        internal static bool CheckIncompatibleMods() {
            string errorMsg = "";
            foreach (var mod in PlatformService.workshop.GetSubscribedItems()) {
                for (int i = 0; i < IncompatibleMods.Length; i++) {
                    if (mod.AsUInt64 == IncompatibleMods[i].fileID) {
                        errorMsg += '[' + IncompatibleMods[i].name + ']' + @" detected. " +
                            (IncompatibleMods[i].inclusive ? "EML already includes the same functionality. " : "This mod is incompatible with EML. ") +
                            (IncompatibleMods[i].specialMsg is null ? "\n" : IncompatibleMods[i].specialMsg + "\n\n");
                        ELog(@"Incompatible mod: [" + IncompatibleMods[i].name + @"] detected");
                    }
                }
            }
            if (errorMsg.Length > 0) {
                EDialog.MessageBox("EML detected incompatible mods", errorMsg);
                ELog("EML detected incompatible mods, please remove the following mentioned mods\n" + errorMsg);
                return false;
            }
            return true;
        }

        internal static void EnablePropPatches() {
            Harmony harmony = new Harmony(EModule.HARMONYID);
            new EPropManagerPatch().Enable(harmony);
            new EDefaultToolPatch().Enable(harmony);
            new EBulldozePatch().Enable(harmony);
            new EDisasterHelpersPatch().Enable(harmony);
            new EDistrictManagerPatch().Enable(harmony);
            new EInstanceManagerPatch().Enable(harmony);
            new EPropToolPatch().Enable(harmony);
            new EBuildingAIPatch().Enable(harmony);
            new EToolBaseCompatPatch().Enable(harmony);
        }

        internal static void DisablePropPatches() {
            Harmony harmony = new Harmony(EModule.HARMONYID);
            new EPropManagerPatch().Disable(harmony);
            new EBulldozePatch().Disable(harmony);
            new EDefaultToolPatch().Disable(harmony);
            new EDisasterHelpersPatch().Disable(harmony);
            new EDistrictManagerPatch().Disable(harmony);
            new EInstanceManagerPatch().Disable(harmony);
            new EPropToolPatch().Disable(harmony);
            new EBuildingAIPatch().Disable(harmony);
            new EToolBaseCompatPatch().Disable(harmony);
            new E81TilesCompatPatch().Disable(harmony);
        }

        /// <summary>
        /// Enables Harmony patches for other mods. Do NOT call before OnCreated() - especially DO NOT USE DoOnHarmonyReady.
        /// Mod load and instantiation order is undefined at OnEnabled, and CitiesHarmony DoOnHarmonyReady may - and often does - trigger this BEFORE target mod is lodaed.
        /// </summary>
        internal static void EnableModPatches() {
            Harmony harmony = new Harmony(EModule.HARMONYID);
            new E81TilesCompatPatch().Enable(harmony);
        }
    }
}
