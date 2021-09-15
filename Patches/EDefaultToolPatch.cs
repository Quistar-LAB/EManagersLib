using ColossalFramework;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace EManagersLib {
    internal class EDefaultToolPatch {
        private static IEnumerable<CodeInstruction> EndMovingTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
            CodeInstruction newStLoc(LocalBuilder local) {
                switch (local.LocalIndex) {
                case 0: return new CodeInstruction(OpCodes.Stloc_0);
                case 1: return new CodeInstruction(OpCodes.Stloc_1);
                case 2: return new CodeInstruction(OpCodes.Stloc_2);
                case 3: return new CodeInstruction(OpCodes.Stloc_3);
                default: return new CodeInstruction(OpCodes.Stloc_S, local);
                }
            }
            CodeInstruction newLdLoc(LocalBuilder local) {
                switch (local.LocalIndex) {
                case 0: return new CodeInstruction(OpCodes.Ldloc_0);
                case 1: return new CodeInstruction(OpCodes.Ldloc_1);
                case 2: return new CodeInstruction(OpCodes.Ldloc_2);
                case 3: return new CodeInstruction(OpCodes.Ldloc_3);
                default: return new CodeInstruction(OpCodes.Ldloc_S, local);
                }
            }
            LocalBuilder prop32 = il.DeclareLocal(typeof(uint));
            FieldInfo id = AccessTools.Field(typeof(DefaultTool).GetNestedType("<EndMoving>c__Iterator2", BindingFlags.Instance | BindingFlags.NonPublic), "id");
            FieldInfo m_props = AccessTools.Field(typeof(PropManager), nameof(PropManager.m_props));
            MethodInfo getProp = AccessTools.PropertyGetter(typeof(InstanceID), nameof(InstanceID.Prop));
            MethodInfo getPMInstance = AccessTools.PropertyGetter(typeof(Singleton<PropManager>), nameof(Singleton<PropManager>.instance));
            MethodInfo moveProp = AccessTools.Method(typeof(PropManager), nameof(PropManager.MoveProp));
            var codes = instructions.GetEnumerator();
            while (codes.MoveNext()) {
                var cur = codes.Current;
                if (cur.opcode == OpCodes.Ldflda && cur.operand == id && codes.MoveNext()) {
                    var next = codes.Current;
                    if (next.opcode == OpCodes.Call && next.operand == getProp && codes.MoveNext()) {
                        cur.opcode = OpCodes.Ldfld;
                        yield return cur;
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(InstanceIDExtension), nameof(InstanceIDExtension.GetProp32)));
                        yield return newStLoc(prop32);
                    } else {
                        yield return cur;
                        yield return next;
                    }
                } else if (cur.opcode == OpCodes.Call && cur.operand == getPMInstance && codes.MoveNext()) {
                    var next = codes.Current;
                    if (next.opcode == OpCodes.Ldloc_2) {
                        yield return cur;
                        yield return newLdLoc(prop32);
                    } else if (next.opcode == OpCodes.Ldfld && next.operand == m_props && codes.MoveNext()) {
                        yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(EPropManager), nameof(EPropManager.m_props)));
                        yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Array32<EPropInstance>), nameof(Array32<EPropInstance>.m_buffer)));
                        codes.MoveNext();
                        yield return newLdLoc(prop32);
                    } else {
                        yield return cur;
                        yield return next;
                    }
                } else if (cur.opcode == OpCodes.Callvirt && cur.operand == moveProp) {
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EPropManager), nameof(EPropManager.MoveProp)));
                } else {
                    yield return cur;
                }
            }
        }

        private static IEnumerable<CodeInstruction> EndRotatingTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
            CodeInstruction newStLoc(LocalBuilder local) {
                switch (local.LocalIndex) {
                case 0: return new CodeInstruction(OpCodes.Stloc_0);
                case 1: return new CodeInstruction(OpCodes.Stloc_1);
                case 2: return new CodeInstruction(OpCodes.Stloc_2);
                case 3: return new CodeInstruction(OpCodes.Stloc_3);
                default: return new CodeInstruction(OpCodes.Stloc_S, local);
                }
            }
            CodeInstruction newLdLoc(LocalBuilder local) {
                switch (local.LocalIndex) {
                case 0: return new CodeInstruction(OpCodes.Ldloc_0);
                case 1: return new CodeInstruction(OpCodes.Ldloc_1);
                case 2: return new CodeInstruction(OpCodes.Ldloc_2);
                case 3: return new CodeInstruction(OpCodes.Ldloc_3);
                default: return new CodeInstruction(OpCodes.Ldloc_S, local);
                }
            }
            LocalBuilder prop32 = il.DeclareLocal(typeof(uint));
            FieldInfo id = AccessTools.Field(typeof(DefaultTool).GetNestedType("<EndRotating>c__Iterator3", BindingFlags.Instance | BindingFlags.NonPublic), "id");
            FieldInfo m_props = AccessTools.Field(typeof(PropManager), nameof(PropManager.m_props));
            MethodInfo getProp = AccessTools.PropertyGetter(typeof(InstanceID), nameof(InstanceID.Prop));
            MethodInfo getPMInstance = AccessTools.PropertyGetter(typeof(Singleton<PropManager>), nameof(Singleton<PropManager>.instance));
            var codes = instructions.GetEnumerator();
            while (codes.MoveNext()) {
                var cur = codes.Current;
                if (cur.opcode == OpCodes.Ldflda && cur.operand == id && codes.MoveNext()) {
                    var next = codes.Current;
                    if (next.opcode == OpCodes.Call && next.operand == getProp && codes.MoveNext()) {
                        cur.opcode = OpCodes.Ldfld;
                        yield return cur;
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(InstanceIDExtension), nameof(InstanceIDExtension.GetProp32)));
                        yield return newStLoc(prop32);
                    } else {
                        yield return cur;
                        yield return next;
                    }
                } else if (cur.opcode == OpCodes.Call && cur.operand == getPMInstance && codes.MoveNext()) {
                    var next = codes.Current;
                    if (next.opcode == OpCodes.Ldfld && next.operand == m_props && codes.MoveNext()) {
                        var next1 = codes.Current;
                        if (next1.opcode == OpCodes.Ldfld && codes.MoveNext()) {
                            yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(EPropManager), nameof(EPropManager.m_props))).WithLabels(cur.labels);
                            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Array32<EPropInstance>), nameof(Array32<EPropInstance>.m_buffer)));
                            yield return newLdLoc(prop32);
                        } else {
                            yield return cur;
                            yield return next;
                            yield return next1;
                        }
                    } else {
                        yield return cur;
                        yield return next;
                    }
                } else {
                    yield return cur;
                }
            }
        }

        private static IEnumerable<CodeInstruction> ReplaceUINT16PropAndGetPropTranspiler(IEnumerable<CodeInstruction> instructions) {
            FieldInfo m_props = AccessTools.Field(typeof(PropManager), nameof(PropManager.m_props));
            MethodInfo getProp = AccessTools.PropertyGetter(typeof(InstanceID), nameof(InstanceID.Prop));
            MethodInfo getPMInstance = AccessTools.PropertyGetter(typeof(Singleton<PropManager>), nameof(Singleton<PropManager>.instance));
            var codes = instructions.GetEnumerator();
            while (codes.MoveNext()) {
                var cur = codes.Current;
                if (cur.opcode == OpCodes.Ldloca_S && codes.MoveNext()) {
                    var next = codes.Current;
                    if (next.opcode == OpCodes.Call && next.operand == getProp) {
                        yield return new CodeInstruction(OpCodes.Ldloc_0).WithLabels(cur.labels);
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(InstanceIDExtension), nameof(InstanceIDExtension.GetProp32)));
                    } else {
                        yield return cur;
                        yield return next;
                    }
                } else if (cur.opcode == OpCodes.Ldflda) {
                    var next = codes.Current;
                    if (next.opcode == OpCodes.Call && next.operand == getProp) {
                        cur.opcode = OpCodes.Ldfld;
                        yield return cur;
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(InstanceIDExtension), nameof(InstanceIDExtension.GetProp32)));
                    } else {
                        yield return cur;
                        yield return next;
                    }
                } else if (cur.opcode == OpCodes.Call && cur.operand == getPMInstance && codes.MoveNext()) {
                    var next = codes.Current;
                    if (next.opcode == OpCodes.Ldfld && next.operand == m_props && codes.MoveNext()) {
                        var next1 = codes.Current;
                        if (next1.opcode == OpCodes.Ldfld) {
                            yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(EPropManager), nameof(EPropManager.m_props))).WithLabels(cur.labels);
                            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Array32<EPropInstance>), nameof(Array32<EPropInstance>.m_buffer)));
                        } else {
                            yield return cur;
                            yield return next;
                            yield return next1;
                        }
                    } else {
                        yield return cur;
                        yield return next;
                    }
                } else {
                    yield return cur;
                }
            }
        }

        private static IEnumerable<CodeInstruction> SimulationStepTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
            LocalBuilder raycastOutput = il.DeclareLocal(typeof(EToolBase.RaycastOutput));
            LocalBuilder origRaycastOutput = default;
            FieldInfo hoverInstance = AccessTools.Field(typeof(DefaultTool), "m_hoverInstance");
            FieldInfo m_props = AccessTools.Field(typeof(PropManager), nameof(PropManager.m_props));
            FieldInfo m_buffer = AccessTools.Field(typeof(Array16<PropInstance>), nameof(Array16<PropInstance>.m_buffer));
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
            MethodInfo raycast = AccessTools.Method(typeof(ToolBase), "RayCast");
            MethodInfo getProp = AccessTools.PropertyGetter(typeof(InstanceID), nameof(InstanceID.Prop));
            MethodInfo setProp = AccessTools.PropertySetter(typeof(InstanceID), nameof(InstanceID.Prop));
            MethodInfo checkPlacement = AccessTools.Method(typeof(PropTool), nameof(PropTool.CheckPlacementErrors));
            MethodInfo getPMInstance = AccessTools.PropertyGetter(typeof(Singleton<PropManager>), nameof(Singleton<PropManager>.instance));
            MethodInfo getPosition = AccessTools.PropertyGetter(typeof(PropInstance), nameof(PropInstance.Position));
            CodeInstruction modifyCode(CodeInstruction code) {
                if (code.operand == m_hitPos) return new CodeInstruction(code.opcode, AccessTools.Field(typeof(EToolBase.RaycastOutput), nameof(EToolBase.RaycastOutput.m_hitPos)));
                else if (code.operand == m_currentEditObject) return new CodeInstruction(code.opcode, AccessTools.Field(typeof(EToolBase.RaycastOutput), nameof(EToolBase.RaycastOutput.m_currentEditObject)));
                else if (code.operand == m_netSegment) return new CodeInstruction(code.opcode, AccessTools.Field(typeof(EToolBase.RaycastOutput), nameof(EToolBase.RaycastOutput.m_netSegment)));
                else if (code.operand == m_netNode) return new CodeInstruction(code.opcode, AccessTools.Field(typeof(EToolBase.RaycastOutput), nameof(EToolBase.RaycastOutput.m_netNode)));
                else if (code.operand == m_building) return new CodeInstruction(code.opcode, AccessTools.Field(typeof(EToolBase.RaycastOutput), nameof(EToolBase.RaycastOutput.m_building)));
                else if (code.operand == m_vehicle) return new CodeInstruction(code.opcode, AccessTools.Field(typeof(EToolBase.RaycastOutput), nameof(EToolBase.RaycastOutput.m_vehicle)));
                else if (code.operand == m_citizenInstance) return new CodeInstruction(code.opcode, AccessTools.Field(typeof(EToolBase.RaycastOutput), nameof(EToolBase.RaycastOutput.m_citizenInstance)));
                else if (code.operand == m_overlayButtonIndex) return new CodeInstruction(code.opcode, AccessTools.Field(typeof(EToolBase.RaycastOutput), nameof(EToolBase.RaycastOutput.m_overlayButtonIndex)));
                else if (code.operand == m_propInstance) return new CodeInstruction(code.opcode, AccessTools.Field(typeof(EToolBase.RaycastOutput), nameof(EToolBase.RaycastOutput.m_propInstance)));
                else if (code.operand == m_treeInstance) return new CodeInstruction(code.opcode, AccessTools.Field(typeof(EToolBase.RaycastOutput), nameof(EToolBase.RaycastOutput.m_treeInstance)));
                else if (code.operand == m_disaster) return new CodeInstruction(code.opcode, AccessTools.Field(typeof(EToolBase.RaycastOutput), nameof(EToolBase.RaycastOutput.m_disaster)));
                else if (code.operand == m_parkedVehicle) return new CodeInstruction(code.opcode, AccessTools.Field(typeof(EToolBase.RaycastOutput), nameof(EToolBase.RaycastOutput.m_parkedVehicle)));
                else if (code.operand == m_district) return new CodeInstruction(code.opcode, AccessTools.Field(typeof(EToolBase.RaycastOutput), nameof(EToolBase.RaycastOutput.m_district)));
                else if (code.operand == m_park) return new CodeInstruction(code.opcode, AccessTools.Field(typeof(EToolBase.RaycastOutput), nameof(EToolBase.RaycastOutput.m_park)));
                else if (code.operand == m_transportLine) return new CodeInstruction(code.opcode, AccessTools.Field(typeof(EToolBase.RaycastOutput), nameof(EToolBase.RaycastOutput.m_transportLine)));
                return code;
            }
            var codes = instructions.GetEnumerator();
            while (codes.MoveNext()) {
                var cur = codes.Current;
                if (cur.opcode == OpCodes.Ldloca_S && (cur.operand as LocalBuilder).LocalIndex == 2 && codes.MoveNext()) {
                    var next = codes.Current;
                    if (next.opcode == OpCodes.Call && next.operand == raycast) {
                        origRaycastOutput = cur.operand as LocalBuilder;
                        yield return new CodeInstruction(OpCodes.Ldloca_S, raycastOutput).WithLabels(cur.labels);
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EToolBase), nameof(EToolBase.RayCast)));
                    } else if (next.opcode == OpCodes.Call && next.operand == getPMInstance && codes.MoveNext()) {
                        var next1 = codes.Current;
                        if (next1.opcode == OpCodes.Ldfld && next1.operand == m_props && codes.MoveNext()) {
                            var next2 = codes.Current;
                            if (next2.opcode == OpCodes.Ldfld && next2.operand == m_buffer) {
                                yield return new CodeInstruction(OpCodes.Ldloca_S, raycastOutput).WithLabels(cur.labels);
                                yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(EPropManager), nameof(EPropManager.m_props)));
                                yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Array32<EPropInstance>), nameof(Array32<EPropInstance>.m_buffer)));
                            }
                        }
                    } else if (next.opcode == OpCodes.Ldfld || next.opcode == OpCodes.Ldflda) {
                        yield return new CodeInstruction(OpCodes.Ldloca_S, raycastOutput).WithLabels(cur.labels);
                        yield return modifyCode(next);
                    } else if ((next.opcode == OpCodes.Ldc_I4_0 || next.opcode == OpCodes.Ldc_I4) && codes.MoveNext()) {
                        yield return new CodeInstruction(OpCodes.Ldloca_S, raycastOutput).WithLabels(cur.labels);
                        yield return next;
                        yield return modifyCode(codes.Current);
                    } else if (next.opcode == OpCodes.Ldloca_S && (next.operand as LocalBuilder).LocalIndex == 2 && codes.MoveNext()) {
                        yield return new CodeInstruction(OpCodes.Ldloca_S, raycastOutput).WithLabels(cur.labels);
                        yield return new CodeInstruction(OpCodes.Ldloca_S, raycastOutput);
                        yield return modifyCode(codes.Current);
                    } else if (next.opcode == OpCodes.Initobj) {
                        yield return new CodeInstruction(OpCodes.Ldloca_S, raycastOutput).WithLabels(cur.labels);
                        yield return new CodeInstruction(OpCodes.Initobj, typeof(EToolBase.RaycastOutput));
                    } else {
                        if ((cur.operand as LocalBuilder).LocalIndex == 2) yield return new CodeInstruction(OpCodes.Ldloca_S, raycastOutput).WithLabels(cur.labels);
                        else yield return cur;
                        yield return next;
                    }
                } else if (cur.opcode == OpCodes.Ldflda && cur.operand == hoverInstance && codes.MoveNext()) {
                    var next = codes.Current;
                    if (next.opcode == OpCodes.Call && next.operand == getProp && codes.MoveNext()) {
                        yield return new CodeInstruction(OpCodes.Ldfld, hoverInstance).WithLabels(cur.labels);
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(InstanceIDExtension), nameof(InstanceIDExtension.GetProp32)));
                        var next1 = codes.Current;
                        if (next1.opcode == OpCodes.Ldelema && next1.operand == typeof(PropInstance)) {
                            yield return new CodeInstruction(OpCodes.Ldelema, typeof(EPropInstance));
                        } else {
                            yield return next1;
                        }
                    } else {
                        yield return cur;
                        yield return next;
                    }
                } else if (cur.opcode == OpCodes.Call && cur.operand == getPMInstance && codes.MoveNext()) {
                    var next = codes.Current;
                    if (next.opcode == OpCodes.Ldfld && next.operand == m_props && codes.MoveNext()) {
                        var next1 = codes.Current;
                        if (next1.opcode == OpCodes.Ldfld && next1.operand == m_buffer) {
                            yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(EPropManager), nameof(EPropManager.m_props))).WithLabels(cur.labels);
                            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Array32<EPropInstance>), nameof(Array32<EPropInstance>.m_buffer)));
                        } else {
                            yield return cur;
                            yield return next;
                            yield return next1;
                        }
                    } else {
                        yield return cur;
                        yield return next;
                    }
                } else if (cur.opcode == OpCodes.Ldarg_0 && codes.MoveNext()) {
                    var next = codes.Current;
                    if (next.opcode == OpCodes.Ldloca_S && (next.operand as LocalBuilder).LocalIndex == 2 && codes.MoveNext()) {
                        var next1 = codes.Current;
                        if (next1.opcode == OpCodes.Ldfld && next1.operand == m_propInstance && codes.MoveNext()) {
                            var next2 = codes.Current;
                            if (next2.opcode == OpCodes.Ldloca_S && codes.MoveNext()) {
                                var next3 = codes.Current;
                                if (next3.opcode == OpCodes.Callvirt) {
                                    yield return cur;
                                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(DefaultTool), "m_toolController"));
                                    yield return new CodeInstruction(OpCodes.Ldloca_S, raycastOutput);
                                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(EToolBase.RaycastOutput), nameof(EToolBase.RaycastOutput.m_propInstance)));
                                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EDefaultTool), nameof(EDefaultTool.CheckProp)));
                                }
                            } else {
                                yield return cur;
                                yield return next;
                                yield return next1;
                                yield return next2;
                            }
                        } else if (next1.opcode == OpCodes.Ldfld && next1.operand == m_treeInstance) {
                            yield return cur;
                            yield return new CodeInstruction(OpCodes.Ldloca_S, raycastOutput);
                            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(EToolBase.RaycastOutput), nameof(EToolBase.RaycastOutput.m_treeInstance)));
                        } else if (next1.opcode == OpCodes.Ldfld && next1.operand == m_vehicle) {
                            yield return cur;
                            yield return new CodeInstruction(OpCodes.Ldloca_S, raycastOutput);
                            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(EToolBase.RaycastOutput), nameof(EToolBase.RaycastOutput.m_vehicle)));
                        } else if (next1.opcode == OpCodes.Ldfld && next1.operand == m_parkedVehicle) {
                            yield return cur;
                            yield return new CodeInstruction(OpCodes.Ldloca_S, raycastOutput);
                            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(EToolBase.RaycastOutput), nameof(EToolBase.RaycastOutput.m_parkedVehicle)));
                        } else if (next1.opcode == OpCodes.Ldfld && next1.operand == m_building) {
                            yield return cur;
                            yield return new CodeInstruction(OpCodes.Ldloca_S, raycastOutput);
                            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(EToolBase.RaycastOutput), nameof(EToolBase.RaycastOutput.m_building)));
                        } else if (next1.opcode == OpCodes.Ldfld && next1.operand == m_hitPos) {
                            yield return cur;
                            yield return new CodeInstruction(OpCodes.Ldloca_S, raycastOutput);
                            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(EToolBase.RaycastOutput), nameof(EToolBase.RaycastOutput.m_hitPos)));
                        } else if (next1.opcode == OpCodes.Ldfld && next1.operand == m_currentEditObject) {
                            yield return cur;
                            yield return new CodeInstruction(OpCodes.Ldloca_S, raycastOutput);
                            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(EToolBase.RaycastOutput), nameof(EToolBase.RaycastOutput.m_currentEditObject)));
                        } else if (next1.opcode == OpCodes.Ldfld && next1.operand == m_netNode) {
                            yield return cur;
                            yield return new CodeInstruction(OpCodes.Ldloca_S, raycastOutput);
                            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(EToolBase.RaycastOutput), nameof(EToolBase.RaycastOutput.m_netNode)));
                        } else if (next1.opcode == OpCodes.Ldfld && next1.operand == m_netSegment) {
                            yield return cur;
                            yield return new CodeInstruction(OpCodes.Ldloca_S, raycastOutput);
                            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(EToolBase.RaycastOutput), nameof(EToolBase.RaycastOutput.m_netSegment)));
                        } else if (next1.opcode == OpCodes.Ldfld && next1.operand == m_citizenInstance) {
                            yield return cur;
                            yield return new CodeInstruction(OpCodes.Ldloca_S, raycastOutput);
                            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(EToolBase.RaycastOutput), nameof(EToolBase.RaycastOutput.m_citizenInstance)));
                        } else if (next1.opcode == OpCodes.Ldfld && next1.operand == m_disaster) {
                            yield return cur;
                            yield return new CodeInstruction(OpCodes.Ldloca_S, raycastOutput);
                            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(EToolBase.RaycastOutput), nameof(EToolBase.RaycastOutput.m_disaster)));
                        } else {
                            yield return cur;
                            yield return new CodeInstruction(OpCodes.Ldloca_S, raycastOutput);
                            yield return next1;
                        }
                    } else if (next.opcode == OpCodes.Ldflda && next.operand == hoverInstance && codes.MoveNext()) {
                        var next1 = codes.Current;
                        if (next1.opcode == OpCodes.Call && next1.operand == getProp) {
                            yield return cur;
                            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(DefaultTool), "m_hoverInstance"));
                            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(InstanceIDExtension), nameof(InstanceIDExtension.GetProp32)));
                        } else {
                            yield return cur;
                            yield return next;
                            yield return next1;
                        }
                    } else {
                        yield return cur;
                        yield return next;
                    }
                } else if (cur.opcode == OpCodes.Call && cur.operand == checkPlacement) {
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EPropTool), nameof(EPropTool.CheckPlacementErrors)));
                } else if (cur.opcode == OpCodes.Ldfld || cur.opcode == OpCodes.Ldflda) {
                    yield return modifyCode(cur);
                } else if (cur.opcode == OpCodes.Stfld) {
                    yield return modifyCode(cur);
                } else if (cur.opcode == OpCodes.Ldelema && cur.operand == typeof(PropInstance)) {
                    yield return new CodeInstruction(OpCodes.Ldelema, typeof(EPropInstance));
                } else if (cur.opcode == OpCodes.Call && cur.operand == getPosition) {
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(EPropInstance), nameof(EPropInstance.Position)));
                } else if (cur.opcode == OpCodes.Call && cur.operand == setProp) {
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(InstanceIDExtension), nameof(InstanceIDExtension.SetProp32ByRef)));
                } else {
                    yield return cur;
                }
            }
        }

        internal void Enable(Harmony harmony) {
            HarmonyMethod replaceUINT16Prop = new HarmonyMethod(AccessTools.Method(typeof(EDefaultToolPatch), nameof(ReplaceUINT16PropAndGetPropTranspiler)));
            harmony.Patch(AccessTools.Method(typeof(DefaultTool).GetNestedType("<EndMoving>c__Iterator2", BindingFlags.Instance | BindingFlags.NonPublic), "MoveNext"),
                transpiler: new HarmonyMethod(AccessTools.Method(typeof(EDefaultToolPatch), nameof(EndMovingTranspiler))));
            harmony.Patch(AccessTools.Method(typeof(DefaultTool).GetNestedType("<EndRotating>c__Iterator3", BindingFlags.Instance | BindingFlags.NonPublic), "MoveNext"),
                transpiler: new HarmonyMethod(AccessTools.Method(typeof(EDefaultToolPatch), nameof(EndRotatingTranspiler))));
            harmony.Patch(AccessTools.Method(typeof(DefaultTool), nameof(DefaultTool.RenderGeometry)), transpiler: replaceUINT16Prop);
            harmony.Patch(AccessTools.Method(typeof(DefaultTool), nameof(DefaultTool.RenderOverlay)), transpiler: replaceUINT16Prop);
            harmony.Patch(AccessTools.Method(typeof(DefaultTool), "SetHoverInstance"), transpiler: replaceUINT16Prop);
            harmony.Patch(AccessTools.Method(typeof(DefaultTool), nameof(DefaultTool.SimulationStep)), transpiler: new HarmonyMethod(AccessTools.Method(typeof(EDefaultToolPatch), nameof(SimulationStepTranspiler))));
            harmony.Patch(AccessTools.Method(typeof(DefaultTool).GetNestedType("<StartMoving>c__Iterator0", BindingFlags.Instance | BindingFlags.NonPublic), "MoveNext"),
                transpiler: replaceUINT16Prop);
            harmony.Patch(AccessTools.Method(typeof(DefaultTool).GetNestedType("<StartRotating>c__Iterator1", BindingFlags.Instance | BindingFlags.NonPublic), "MoveNext"),
                transpiler: replaceUINT16Prop);
        }

        internal void Disable(Harmony harmony) {
            harmony.Unpatch(AccessTools.Method(typeof(DefaultTool).GetNestedType("<EndMoving>c__Iterator2", BindingFlags.Instance | BindingFlags.NonPublic), "MoveNext"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(DefaultTool).GetNestedType("<EndRotating>c__Iterator3", BindingFlags.Instance | BindingFlags.NonPublic), "MoveNext"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(DefaultTool), nameof(DefaultTool.RenderGeometry)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(DefaultTool), nameof(DefaultTool.RenderOverlay)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(DefaultTool), "SetHoverInstance"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(DefaultTool), nameof(DefaultTool.SimulationStep)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(DefaultTool).GetNestedType("<StartMoving>c__Iterator0", BindingFlags.Instance | BindingFlags.NonPublic), "MoveNext"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(DefaultTool).GetNestedType("<StartRotating>c__Iterator1", BindingFlags.Instance | BindingFlags.NonPublic), "MoveNext"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
        }
    }
}
