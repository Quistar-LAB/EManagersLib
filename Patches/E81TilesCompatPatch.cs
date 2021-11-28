using ColossalFramework;
using ColossalFramework.Plugins;
using HarmonyLib;
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
        public static bool MoveParkPropsPrefix(int cellX, int cellZ, byte src, byte dest) {
            DistrictPark[] parks = Singleton<DistrictManager>.instance.m_parks.m_buffer;
            int HALFGRID = 450; // vanilla 256

            int startX = Mathf.Max((int)((cellX - HALFGRID) * (19.2f / 64f) + 135f), 0);
            int startZ = Mathf.Max((int)((cellZ - HALFGRID) * (19.2f / 64f) + 135f), 0);
            int endX = Mathf.Min((int)((cellX - HALFGRID + 1f) * (19.2f / 64f) + 135f), 269);
            int endZ = Mathf.Min((int)((cellZ - HALFGRID + 1f) * (19.2f / 64f) + 135f), 269);
            EPropInstance[] props = EPropManager.m_props.m_buffer;
            uint[] propGrid = EPropManager.m_propGrid;
            for (int i = startZ; i <= endZ; i++) {
                for (int j = startX; j <= endX; j++) {
                    uint propID = propGrid[i * 270 + j];
                    while (propID != 0) {
                        if ((props[propID].m_flags & EPropInstance.BLOCKEDFLAG) == 0) {
                            Vector3 position = props[propID].Position;
                            int x = Mathf.Clamp((int)(position.x / 19.2f + 256f), 0, 511);
                            int y = Mathf.Clamp((int)(position.z / 19.2f + 256f), 0, 511);
                            if (x == cellX && y == cellZ) {
                                parks[src].m_propCount--;
                                parks[dest].m_propCount++;
                            }
                        }
                        propID = props[propID].m_nextGridProp;
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

            harmony.Patch(fakeParkProps, prefix: new HarmonyMethod(AccessTools.Method(typeof(E81TilesCompatPatch), nameof(E81TilesCompatPatch.MoveParkPropsPrefix))));
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