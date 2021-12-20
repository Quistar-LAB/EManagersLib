using ColossalFramework;
using ColossalFramework.Plugins;
using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;

namespace EManagersLib {

    public class E81TilesCompatPatch {
        private MethodInfo fakeParkProps;

        /// <summary>
        /// This is just the EDistrictManager.MoveParkProps code copypasted and using the 81-tiles halfgrid as a preemptive prefix patch.
        /// Yes, this is overridding a detour by way of a pre-emptive Harmony Prefix on the detour target.
        /// Normally wouldn't do things this way but since this is only interim (and don't want to touch EDistrictManager code), this is a quick and reliable way to get what we want.
        /// </summary>
        public unsafe static bool MoveParkPropsPrefix(int cellX, int cellZ, byte src, byte dest) {
            int HALFGRID = 450; // vanilla 256
            int startX = EMath.Max((int)((cellX - HALFGRID) * (19.2f / 64f) + 135f), 0);
            int startZ = EMath.Max((int)((cellZ - HALFGRID) * (19.2f / 64f) + 135f), 0);
            int endX = EMath.Min((int)((cellX - HALFGRID + 1f) * (19.2f / 64f) + 135f), 269);
            int endZ = EMath.Min((int)((cellZ - HALFGRID + 1f) * (19.2f / 64f) + 135f), 269);
            ref DistrictPark srcPark = ref Singleton<DistrictManager>.instance.m_parks.m_buffer[src];
            ref DistrictPark destPark = ref Singleton<DistrictManager>.instance.m_parks.m_buffer[dest];
            fixed (EPropInstance* pProp = &EPropManager.m_props.m_buffer[0])
            fixed (uint* pGrid = &EPropManager.m_propGrid[0]) {
                for (int i = startZ; i <= endZ; i++) {
                    for (int j = startX; j <= endX; j++) {
                        uint propID = *(pGrid + (i * 270 + j));
                        while (propID != 0) {
                            EPropInstance* prop = pProp + propID;
                            if ((prop->m_flags & EPropInstance.BLOCKEDFLAG) == 0) {
                                Vector3 position = prop->Position;
                                int x = EMath.Clamp((int)(position.x / 19.2f + 256f), 0, 511);
                                int y = EMath.Clamp((int)(position.z / 19.2f + 256f), 0, 511);
                                if (x == cellX && y == cellZ) {
                                    srcPark.m_propCount--;
                                    destPark.m_propCount++;
                                }
                            }
                            propID = prop->m_nextGridProp;
                        }
                    }
                }
            }
            // Pre-empt target method.
            return false;
        }

        internal void Enable(Harmony harmony) {
            fakeParkProps = EightyOneReflection();
            if (fakeParkProps is null) {
                Debug.Log("81 Tiles MoveParkProps not reflected");
                return;
            }
            try {
                harmony.Patch(fakeParkProps, prefix: new HarmonyMethod(AccessTools.Method(typeof(E81TilesCompatPatch), nameof(E81TilesCompatPatch.MoveParkPropsPrefix))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch 81 Tiles MoveParkProps");
                EUtils.ELog(e.Message);
                throw;
            }
        }

        internal void Disable(Harmony harmony) {
            if (!(fakeParkProps is null))
                harmony.Unpatch(fakeParkProps, HarmonyPatchType.Prefix, EModule.HARMONYID);
        }

        /// <summary>
        /// Checks to see if the 81 tiles mod is intalled and active, and if it is, returns the MethodInfo of 81 Tiles' FakeDistrictManager.MoveParkProps.
        /// </summary>
        private MethodInfo EightyOneReflection() {
            foreach (PluginManager.PluginInfo plugin in Singleton<PluginManager>.instance.GetPluginsInfo()) {
                foreach (Assembly assembly in plugin.GetAssemblies()) {
                    if (assembly.GetName().Name.Equals("EightyOne") && plugin.isEnabled) {
                        Debug.Log("81Tiles is installed");

                        // Found EightyOne.dll that's part of an enabled plugin; try to get its FakeDistrictManager class.
                        return assembly.GetType("EightyOne.ResourceManagers.FakeDistrictManager")?.GetMethod("MoveParkProps", BindingFlags.NonPublic | BindingFlags.Instance);
                    }
                }
            }

            // Fallback.
            return null;
        }
    }
}