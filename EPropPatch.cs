using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using ColossalFramework;

namespace EManagersLib {
    public class EPropPatch : SingletonLite<EPropPatch> {
        private const string HARMONYID = "quistar.EManagersLib.mod";
        private readonly Harmony m_Harmony;

        public EPropPatch() {
            m_Harmony = new Harmony(HARMONYID);
        }

        private static IEnumerable<CodeInstruction> AwakeTranspiler(IEnumerable<CodeInstruction> instructions) {

        }

        internal void EnablePropCorePatch() {
            Harmony harmony = m_Harmony;
            harmony.Patch(AccessTools.Method(typeof(TreeManager), "Awake"), transpiler: new HarmonyMethod(AccessTools.Method(typeof(EPropPatch), nameof(AwakeTranspiler))));
        }

        internal void DisablePropCorePatch() {
            Harmony harmony = m_Harmony;
            harmony.Unpatch(AccessTools.Method(typeof(TreeManager), "Awake"), HarmonyPatchType.Transpiler, HARMONYID);
        }
    }
}
