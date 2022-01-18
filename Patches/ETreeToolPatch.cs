using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace EManagersLib.Patches {
    internal readonly struct ETreeToolPatch {
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
                    if (cur.opcode == OpCodes.Ldloca_S && cur.operand is LocalBuilder l1 && l1.LocalType == typeof(ToolBase.RaycastOutput)) {
                        yield return new CodeInstruction(OpCodes.Ldloca_S, raycastOutput).WithLabels(cur.labels);
                    } else if (cur.opcode == OpCodes.Call && cur.operand is MethodInfo method && method == raycast) {
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EToolBase), nameof(EToolBase.RayCast))).WithLabels(cur.labels);
                    } else if (cur.opcode == OpCodes.Ldfld || cur.opcode == OpCodes.Ldflda || cur.opcode == OpCodes.Stfld) {
                        if (cur.operand is FieldInfo field) {
                            if (field == m_hitPos) yield return new CodeInstruction(cur.opcode, AccessTools.Field(typeof(EToolBase.RaycastOutput), nameof(EToolBase.RaycastOutput.m_hitPos))).WithLabels(cur.labels);
                            else if (field == m_currentEditObject) yield return new CodeInstruction(cur.opcode, AccessTools.Field(typeof(EToolBase.RaycastOutput), nameof(EToolBase.RaycastOutput.m_currentEditObject))).WithLabels(cur.labels);
                            else if (field == m_netSegment) yield return new CodeInstruction(cur.opcode, AccessTools.Field(typeof(EToolBase.RaycastOutput), nameof(EToolBase.RaycastOutput.m_netSegment))).WithLabels(cur.labels);
                            else if (field == m_netNode) yield return new CodeInstruction(cur.opcode, AccessTools.Field(typeof(EToolBase.RaycastOutput), nameof(EToolBase.RaycastOutput.m_netNode))).WithLabels(cur.labels);
                            else if (field == m_building) yield return new CodeInstruction(cur.opcode, AccessTools.Field(typeof(EToolBase.RaycastOutput), nameof(EToolBase.RaycastOutput.m_building))).WithLabels(cur.labels);
                            else if (field == m_vehicle) yield return new CodeInstruction(cur.opcode, AccessTools.Field(typeof(EToolBase.RaycastOutput), nameof(EToolBase.RaycastOutput.m_vehicle))).WithLabels(cur.labels);
                            else if (field == m_citizenInstance) yield return new CodeInstruction(cur.opcode, AccessTools.Field(typeof(EToolBase.RaycastOutput), nameof(EToolBase.RaycastOutput.m_citizenInstance))).WithLabels(cur.labels);
                            else if (field == m_overlayButtonIndex) yield return new CodeInstruction(cur.opcode, AccessTools.Field(typeof(EToolBase.RaycastOutput), nameof(EToolBase.RaycastOutput.m_overlayButtonIndex))).WithLabels(cur.labels);
                            else if (field == m_propInstance) yield return new CodeInstruction(cur.opcode, AccessTools.Field(typeof(EToolBase.RaycastOutput), nameof(EToolBase.RaycastOutput.m_propInstance))).WithLabels(cur.labels);
                            else if (field == m_treeInstance) yield return new CodeInstruction(cur.opcode, AccessTools.Field(typeof(EToolBase.RaycastOutput), nameof(EToolBase.RaycastOutput.m_treeInstance))).WithLabels(cur.labels);
                            else if (field == m_disaster) yield return new CodeInstruction(cur.opcode, AccessTools.Field(typeof(EToolBase.RaycastOutput), nameof(EToolBase.RaycastOutput.m_disaster))).WithLabels(cur.labels);
                            else if (field == m_parkedVehicle) yield return new CodeInstruction(cur.opcode, AccessTools.Field(typeof(EToolBase.RaycastOutput), nameof(EToolBase.RaycastOutput.m_parkedVehicle))).WithLabels(cur.labels);
                            else if (field == m_district) yield return new CodeInstruction(cur.opcode, AccessTools.Field(typeof(EToolBase.RaycastOutput), nameof(EToolBase.RaycastOutput.m_district))).WithLabels(cur.labels);
                            else if (field == m_park) yield return new CodeInstruction(cur.opcode, AccessTools.Field(typeof(EToolBase.RaycastOutput), nameof(EToolBase.RaycastOutput.m_park))).WithLabels(cur.labels);
                            else if (field == m_transportLine) yield return new CodeInstruction(cur.opcode, AccessTools.Field(typeof(EToolBase.RaycastOutput), nameof(EToolBase.RaycastOutput.m_transportLine))).WithLabels(cur.labels);
                            else yield return cur;
                        } else {
                            yield return cur;
                        }
                    } else if (cur.opcode == OpCodes.Initobj && cur.operand is LocalBuilder l2 && l2.LocalType == typeof(ToolBase.RaycastOutput)) {
                        cur.operand = typeof(EToolBase.RaycastOutput);
                        yield return cur;
                    } else {
                        yield return cur;
                    }
                }
            }
        }

        internal void Enable(Harmony harmony) {
            try {
                harmony.Patch(AccessTools.Method(typeof(TreeTool), nameof(TreeTool.SimulationStep)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(ETreeToolPatch), nameof(GenericToolBaseCompatTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch TreeTool::SimulationStep");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(TreeTool), nameof(TreeTool.SimulationStep)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
        }

        internal void Disable(Harmony harmony) {
            harmony.Unpatch(AccessTools.Method(typeof(TreeTool), nameof(TreeTool.SimulationStep)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
        }
    }
}
