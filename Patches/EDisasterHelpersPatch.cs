using HarmonyLib;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace EManagersLib {
    internal class EDisasterHelpersPatch {
        private static IEnumerable<CodeInstruction> DestroyPropsTranspiler(IEnumerable<CodeInstruction> instructions) {
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Ldarg_2);
            yield return new CodeInstruction(OpCodes.Ldarg_3);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EDisasterHelpers), nameof(EDisasterHelpers.DestroyProps)));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        internal void Enable(Harmony harmony) {
            harmony.Patch(AccessTools.Method(typeof(DisasterHelpers), nameof(DisasterHelpers.DestroyProps)), transpiler: new HarmonyMethod(AccessTools.Method(typeof(EDisasterHelpersPatch), nameof(DestroyPropsTranspiler))));
        }

        internal void Disable(Harmony harmony) {
            harmony.Unpatch(AccessTools.Method(typeof(DisasterHelpers), nameof(DisasterHelpers.DestroyProps)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
        }
    }
}
