using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace EManagersLib {
    internal class EDistrictManagerPatch {
        private static IEnumerable<CodeInstruction> MoveParkPropsTranspiler(IEnumerable<CodeInstruction> instructions) {
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(DistrictManager), nameof(DistrictManager.m_parks)));
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Array8<DistrictPark>), nameof(Array8<DistrictPark>.m_buffer)));
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Ldarg_2);
            yield return new CodeInstruction(OpCodes.Ldarg_3);
            yield return new CodeInstruction(OpCodes.Ldarg_S, 4);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EDistrictManager), nameof(EDistrictManager.MoveParkProps)));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        internal void Enable(Harmony harmony) {
            harmony.Patch(AccessTools.Method(typeof(DistrictManager), "MoveParkProps"), transpiler: new HarmonyMethod(AccessTools.Method(typeof(EDistrictManagerPatch), nameof(EDistrictManagerPatch.MoveParkPropsTranspiler))));
        }

        internal void Disable(Harmony harmony) {
            harmony.Unpatch(AccessTools.Method(typeof(DistrictManager), "MoveParkProps"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
        }
    }
}
