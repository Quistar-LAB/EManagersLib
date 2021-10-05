using EManagersLib.API;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace EManagersLib {
    public class EToolBaseCompatPatch {
        private static IEnumerable<CodeInstruction> TestSimulationTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
            var codes = GenericToolBaseCompatTranspiler(instructions, il);
            foreach (var code in codes) {
                EUtils.ELog(code.ToString());
            }
            return codes;
        }

        private static IEnumerable<CodeInstruction> GenericToolBaseCompatTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
            LocalBuilder raycastOutput = il.DeclareLocal(typeof(EToolBase.RaycastOutput));
            MethodInfo raycast = AccessTools.Method(typeof(ToolBase), "RayCast");
            FieldInfo m_hitPos = AccessTools.Field(typeof(ToolBase.RaycastOutput), nameof(ToolBase.RaycastOutput.m_hitPos));
            FieldInfo m_currentEditObject = AccessTools.Field(typeof(ToolBase.RaycastOutput), nameof(ToolBase.RaycastOutput.m_currentEditObject));
            FieldInfo m_netSegment = AccessTools.Field(typeof(ToolBase.RaycastOutput), nameof(ToolBase.RaycastOutput.m_netSegment));
            FieldInfo m_netNode = AccessTools.Field(typeof(ToolBase.RaycastOutput), nameof(ToolBase.RaycastOutput.m_netNode));
            FieldInfo m_building = AccessTools.Field(typeof(ToolBase.RaycastOutput), nameof(ToolBase.RaycastOutput.m_building));
            FieldInfo m_vehicle = AccessTools.Field(typeof(ToolBase.RaycastOutput), nameof(ToolBase.RaycastOutput.m_vehicle));
            FieldInfo m_parkedVehicle = AccessTools.Field(typeof(ToolBase.RaycastOutput), nameof(ToolBase.RaycastOutput.m_parkedVehicle));
            FieldInfo m_citizenInstance = AccessTools.Field(typeof(ToolBase.RaycastOutput), nameof(ToolBase.RaycastOutput.m_citizenInstance));
            FieldInfo m_overlayButtonIndex = AccessTools.Field(typeof(ToolBase.RaycastOutput), nameof(ToolBase.RaycastOutput.m_overlayButtonIndex));
            FieldInfo m_propInstance = AccessTools.Field(typeof(ToolBase.RaycastOutput), nameof(ToolBase.RaycastOutput.m_propInstance));
            FieldInfo m_treeInstance = AccessTools.Field(typeof(ToolBase.RaycastOutput), nameof(ToolBase.RaycastOutput.m_treeInstance));
            FieldInfo m_disaster = AccessTools.Field(typeof(ToolBase.RaycastOutput), nameof(ToolBase.RaycastOutput.m_disaster));
            FieldInfo m_district = AccessTools.Field(typeof(ToolBase.RaycastOutput), nameof(ToolBase.RaycastOutput.m_district));
            FieldInfo m_park = AccessTools.Field(typeof(ToolBase.RaycastOutput), nameof(ToolBase.RaycastOutput.m_park));
            FieldInfo m_transportLine = AccessTools.Field(typeof(ToolBase.RaycastOutput), nameof(ToolBase.RaycastOutput.m_transportLine));
            using (IEnumerator<CodeInstruction> codes = instructions.GetEnumerator()) {
                while (codes.MoveNext()) {
                    var cur = codes.Current;
                    if (cur.opcode == OpCodes.Ldloca_S && (cur.operand as LocalBuilder).LocalType == typeof(ToolBase.RaycastOutput)) {
                        yield return new CodeInstruction(OpCodes.Ldloca_S, raycastOutput).WithLabels(cur.labels);
                    } else if (cur.opcode == OpCodes.Call && cur.operand == raycast) {
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EToolBase), nameof(EToolBase.RayCast))).WithLabels(cur.labels);
                    } else if (cur.opcode == OpCodes.Ldfld || cur.opcode == OpCodes.Ldflda || cur.opcode == OpCodes.Stfld) {
                        if (cur.operand == m_hitPos) yield return new CodeInstruction(cur.opcode, AccessTools.Field(typeof(EToolBase.RaycastOutput), nameof(EToolBase.RaycastOutput.m_hitPos)));
                        else if (cur.operand == m_currentEditObject) yield return new CodeInstruction(cur.opcode, AccessTools.Field(typeof(EToolBase.RaycastOutput), nameof(EToolBase.RaycastOutput.m_currentEditObject)));
                        else if (cur.operand == m_netSegment) yield return new CodeInstruction(cur.opcode, AccessTools.Field(typeof(EToolBase.RaycastOutput), nameof(EToolBase.RaycastOutput.m_netSegment)));
                        else if (cur.operand == m_netNode) yield return new CodeInstruction(cur.opcode, AccessTools.Field(typeof(EToolBase.RaycastOutput), nameof(EToolBase.RaycastOutput.m_netNode)));
                        else if (cur.operand == m_building) yield return new CodeInstruction(cur.opcode, AccessTools.Field(typeof(EToolBase.RaycastOutput), nameof(EToolBase.RaycastOutput.m_building)));
                        else if (cur.operand == m_vehicle) yield return new CodeInstruction(cur.opcode, AccessTools.Field(typeof(EToolBase.RaycastOutput), nameof(EToolBase.RaycastOutput.m_vehicle)));
                        else if (cur.operand == m_citizenInstance) yield return new CodeInstruction(cur.opcode, AccessTools.Field(typeof(EToolBase.RaycastOutput), nameof(EToolBase.RaycastOutput.m_citizenInstance)));
                        else if (cur.operand == m_overlayButtonIndex) yield return new CodeInstruction(cur.opcode, AccessTools.Field(typeof(EToolBase.RaycastOutput), nameof(EToolBase.RaycastOutput.m_overlayButtonIndex)));
                        else if (cur.operand == m_propInstance) yield return new CodeInstruction(cur.opcode, AccessTools.Field(typeof(EToolBase.RaycastOutput), nameof(EToolBase.RaycastOutput.m_propInstance)));
                        else if (cur.operand == m_treeInstance) yield return new CodeInstruction(cur.opcode, AccessTools.Field(typeof(EToolBase.RaycastOutput), nameof(EToolBase.RaycastOutput.m_treeInstance)));
                        else if (cur.operand == m_disaster) yield return new CodeInstruction(cur.opcode, AccessTools.Field(typeof(EToolBase.RaycastOutput), nameof(EToolBase.RaycastOutput.m_disaster)));
                        else if (cur.operand == m_parkedVehicle) yield return new CodeInstruction(cur.opcode, AccessTools.Field(typeof(EToolBase.RaycastOutput), nameof(EToolBase.RaycastOutput.m_parkedVehicle)));
                        else if (cur.operand == m_district) yield return new CodeInstruction(cur.opcode, AccessTools.Field(typeof(EToolBase.RaycastOutput), nameof(EToolBase.RaycastOutput.m_district)));
                        else if (cur.operand == m_park) yield return new CodeInstruction(cur.opcode, AccessTools.Field(typeof(EToolBase.RaycastOutput), nameof(EToolBase.RaycastOutput.m_park)));
                        else if (cur.operand == m_transportLine) yield return new CodeInstruction(cur.opcode, AccessTools.Field(typeof(EToolBase.RaycastOutput), nameof(EToolBase.RaycastOutput.m_transportLine)));
                        else yield return cur;
                    } else if (cur.opcode == OpCodes.Initobj && cur.operand == typeof(ToolBase.RaycastOutput)) {
                        yield return new CodeInstruction(OpCodes.Initobj, typeof(EToolBase.RaycastOutput));
                    } else {
                        yield return cur;
                    }
                }
            }
        }

        internal void Enable(Harmony harmony) {
            HarmonyMethod genericToolBaseCompatPatch = new HarmonyMethod(AccessTools.Method(typeof(EToolBaseCompatPatch), nameof(GenericToolBaseCompatTranspiler)));
            harmony.Patch(AccessTools.Method(typeof(BuildingTool), nameof(BuildingTool.SimulationStep)), transpiler: genericToolBaseCompatPatch);
            harmony.Patch(AccessTools.Method(typeof(TreeTool), nameof(TreeTool.SimulationStep)), transpiler: genericToolBaseCompatPatch);
        }

        internal void Disable(Harmony harmony) {
            harmony.Unpatch(AccessTools.Method(typeof(BuildingTool), nameof(BuildingTool.SimulationStep)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(TreeTool), nameof(TreeTool.SimulationStep)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
        }
    }
}
