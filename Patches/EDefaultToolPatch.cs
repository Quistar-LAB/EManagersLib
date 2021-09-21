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
            FieldInfo m_props = AccessTools.Field(typeof(PropManager), nameof(PropManager.m_props));
            MethodInfo getProp = AccessTools.PropertyGetter(typeof(InstanceID), nameof(InstanceID.Prop));
            MethodInfo getPMInstance = AccessTools.PropertyGetter(typeof(Singleton<PropManager>), nameof(Singleton<PropManager>.instance));
            MethodInfo moveProp = AccessTools.Method(typeof(PropManager), nameof(PropManager.MoveProp));
            MethodInfo setFixedHeight = AccessTools.PropertySetter(typeof(PropInstance), nameof(PropInstance.FixedHeight));
            var codes = instructions.GetEnumerator();
            while(codes.MoveNext()) {
                var cur = codes.Current;
                if (cur.opcode == OpCodes.Call && cur.operand == getProp && codes.MoveNext()) {
                    var next = codes.Current;
                    if (next.opcode == OpCodes.Stloc_2) {
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(InstanceIDExtension), nameof(InstanceIDExtension.GetProp32ByRef)));
                        yield return newStLoc(prop32);
                    } else {
                        yield return cur;
                        yield return next;
                    }
                } else if (cur.opcode == OpCodes.Ldloc_2) {
                    yield return newLdLoc(prop32);
                } else if (cur.opcode == OpCodes.Call && cur.operand == getPMInstance && codes.MoveNext()) {
                    var next = codes.Current;
                    if (next.opcode == OpCodes.Ldfld && next.operand == m_props && codes.MoveNext()) {
                        yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(EPropManager), nameof(EPropManager.m_props))).WithLabels(cur.labels);
                        yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Array32<EPropInstance>), nameof(Array32<EPropInstance>.m_buffer)));
                    } else if (next.opcode == OpCodes.Ldloc_2) {
                        yield return cur;
                        yield return newLdLoc(prop32);
                    } else {
                        yield return cur;
                        yield return next;
                    }
                } else if (cur.opcode == OpCodes.Callvirt && cur.operand == moveProp) {
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EPropManager), nameof(EPropManager.MoveProp)));
                } else if (cur.opcode == OpCodes.Ldelema && cur.operand == typeof(PropInstance)) {
                    yield return new CodeInstruction(OpCodes.Ldelema, typeof(EPropInstance));
                } else if (cur.opcode == OpCodes.Call && cur.operand == setFixedHeight) {
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.PropertySetter(typeof(EPropInstance), nameof(EPropInstance.FixedHeight)));
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
            FieldInfo m_props = AccessTools.Field(typeof(PropManager), nameof(PropManager.m_props));
            MethodInfo getProp = AccessTools.PropertyGetter(typeof(InstanceID), nameof(InstanceID.Prop));
            MethodInfo getPMInstance = AccessTools.PropertyGetter(typeof(Singleton<PropManager>), nameof(Singleton<PropManager>.instance));
            MethodInfo setAngle = AccessTools.PropertySetter(typeof(PropInstance), nameof(PropInstance.Angle));
            var codes = instructions.GetEnumerator();
            while (codes.MoveNext()) {
                var cur = codes.Current;
                if (cur.opcode == OpCodes.Call && cur.operand == getProp && codes.MoveNext()) {
                    var next = codes.Current;
                    if (next.opcode == OpCodes.Stloc_2) {
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(InstanceIDExtension), nameof(InstanceIDExtension.GetProp32ByRef)));
                        yield return newStLoc(prop32);
                    } else {
                        yield return cur;
                        yield return next;
                    }
                } else if (cur.opcode == OpCodes.Ldloc_2) {
                    yield return newLdLoc(prop32);
                } else if (cur.opcode == OpCodes.Call && cur.operand == getPMInstance && codes.MoveNext()) {
                    var next = codes.Current;
                    if (next.opcode == OpCodes.Ldfld && next.operand == m_props && codes.MoveNext()) {
                        yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(EPropManager), nameof(EPropManager.m_props))).WithLabels(cur.labels);
                        yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Array32<EPropInstance>), nameof(Array32<EPropInstance>.m_buffer)));
                    } else {
                        yield return cur;
                        yield return next;
                    }
                } else if (cur.opcode == OpCodes.Ldelema && cur.operand == typeof(PropInstance)) {
                    yield return new CodeInstruction(OpCodes.Ldelema, typeof(EPropInstance));
                } else if (cur.opcode == OpCodes.Call && cur.operand == setAngle) {
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.PropertySetter(typeof(EPropInstance), nameof(EPropInstance.Angle)));
                } else {
                    yield return cur;
                }
            }
        }

        private static IEnumerable<CodeInstruction> RenderGeometryTranspiler(IEnumerable<CodeInstruction> instructions) {
            MethodInfo renderInstance = AccessTools.Method(typeof(PropInstance), nameof(PropInstance.RenderInstance),
                new System.Type[] { typeof(RenderManager.CameraInfo), typeof(PropInfo), typeof(InstanceID), typeof(UnityEngine.Vector3),
                                    typeof(float), typeof(float), typeof(UnityEngine.Color), typeof(UnityEngine.Vector4), typeof(bool)
                });
            MethodInfo getPMInstance = AccessTools.PropertyGetter(typeof(Singleton<PropManager>), nameof(Singleton<PropManager>.instance));
            MethodInfo getProp = AccessTools.PropertyGetter(typeof(InstanceID), nameof(InstanceID.Prop));
            var codes = instructions.GetEnumerator();
            while(codes.MoveNext()) {
                var cur = codes.Current;
                if (cur.opcode == OpCodes.Call && cur.operand == getProp) {
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(InstanceIDExtension), nameof(InstanceIDExtension.GetProp32ByRef)));
                } else if (cur.opcode == OpCodes.Call && cur.operand == getPMInstance) {
                    while(codes.MoveNext()) {
                        cur = codes.Current;
                        if(cur.opcode == OpCodes.Call && cur.operand == renderInstance) break;
                    }
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Ldloca_S, 0);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(DefaultTool), "m_mousePosition"));
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(DefaultTool), "m_angle"));
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EDefaultTool), nameof(EDefaultTool.RenderPropGeometry)));
                } else {
                    yield return cur;
                }
            }
        }

        private static IEnumerable<CodeInstruction> RenderOverlayTranspiler(IEnumerable<CodeInstruction> instructions) {
            bool firstSig = false;
            List<Label> labels = default;
            MethodInfo getProp = AccessTools.PropertyGetter(typeof(InstanceID), nameof(InstanceID.Prop));
            MethodInfo getPMInstance = AccessTools.PropertyGetter(typeof(Singleton<PropManager>), nameof(Singleton<PropManager>.instance));
            MethodInfo renderOverlay = AccessTools.Method(typeof(PropTool), nameof(PropTool.RenderOverlay),
                new System.Type[] { typeof(RenderManager.CameraInfo), typeof(PropInfo), typeof(UnityEngine.Vector3), typeof(float), typeof(float), typeof(UnityEngine.Color) });
            var codes = instructions.GetEnumerator();
            while(codes.MoveNext()) {
                var cur = codes.Current;
                if (cur.opcode == OpCodes.Ldloca_S && (cur.operand as LocalBuilder).LocalIndex == 0 && codes.MoveNext()) {
                    var next = codes.Current;
                    if (!firstSig && next.opcode == OpCodes.Call && next.operand == getProp) {
                        yield return cur;
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(InstanceIDExtension), nameof(InstanceIDExtension.GetProp32ByRef)));
                    } else if (firstSig && next.opcode == OpCodes.Call && next.operand == getProp && codes.MoveNext()) {
                        labels = cur.ExtractLabels();
                    } else {
                        yield return cur;
                        yield return next;
                    }
                } else if (!firstSig && cur.opcode == OpCodes.Call && cur.operand == getPMInstance) {
                    firstSig = true;
                    while (codes.MoveNext()) if (codes.Current.opcode == OpCodes.Call && codes.Current.operand == renderOverlay) break;
                    yield return new CodeInstruction(OpCodes.Ldarg_1);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(DefaultTool), "m_toolController"));
                    yield return new CodeInstruction(OpCodes.Ldloca_S, 0);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(DefaultTool), "m_mousePosition"));
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(DefaultTool), "m_angle"));
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(DefaultTool), "m_selectErrors"));
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EDefaultTool), nameof(EDefaultTool.RenderPropOverlay)));
                } else if (firstSig && cur.opcode == OpCodes.Call && cur.operand == getPMInstance) {
                    while (codes.MoveNext()) if (codes.Current.opcode == OpCodes.Call && codes.Current.operand == renderOverlay) break;
                    yield return new CodeInstruction(OpCodes.Ldarg_1).WithLabels(labels);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(DefaultTool), "m_toolController"));
                    yield return new CodeInstruction(OpCodes.Ldloca_S, 0);
                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(DefaultTool), "m_selectErrors"));
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EDefaultTool), nameof(EDefaultTool.RenderPropTypeOverlay)));
                } else {
                    yield return cur;
                }
            }
        }

        private static IEnumerable<CodeInstruction> SetHoverInstanceTranspiler(IEnumerable<CodeInstruction> instructions) {
            FieldInfo m_props = AccessTools.Field(typeof(PropManager), nameof(PropManager.m_props));
            FieldInfo m_buffer = AccessTools.Field(typeof(Array16<PropInstance>), nameof(Array16<PropInstance>.m_buffer));
            MethodInfo getProp = AccessTools.PropertyGetter(typeof(InstanceID), nameof(InstanceID.Prop));
            MethodInfo getPMInstance = AccessTools.PropertyGetter(typeof(Singleton<PropManager>), nameof(Singleton<PropManager>.instance));
            MethodInfo getHidden = AccessTools.PropertyGetter(typeof(PropInstance), nameof(PropInstance.Hidden));
            MethodInfo setHidden = AccessTools.PropertySetter(typeof(PropInstance), nameof(PropInstance.Hidden));
            var codes = instructions.GetEnumerator();
            while(codes.MoveNext()) {
                var cur = codes.Current;
                if (cur.opcode == OpCodes.Call && cur.operand == getProp) {
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(InstanceIDExtension), nameof(InstanceIDExtension.GetProp32ByRef)));
                } else if (cur.opcode == OpCodes.Call && cur.operand == getPMInstance && codes.MoveNext()) {
                    var next = codes.Current;
                    if (next.opcode == OpCodes.Ldfld && next.operand == m_props && codes.MoveNext()) {
                        var next1 = codes.Current;
                        if (next1.opcode == OpCodes.Ldfld && next1.operand == m_buffer) {
                            yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(EPropManager), nameof(EPropManager.m_props)));
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
                } else if (cur.opcode == OpCodes.Ldelema && cur.operand == typeof(PropInstance)) {
                    yield return new CodeInstruction(OpCodes.Ldelema, typeof(EPropInstance));
                } else if (cur.opcode == OpCodes.Call && cur.operand == getHidden) {
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(EPropInstance), nameof(EPropInstance.Hidden)));
                } else if (cur.opcode == OpCodes.Call && cur.operand == setHidden) {
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.PropertySetter(typeof(EPropInstance), nameof(EPropInstance.Hidden)));
                } else {
                    yield return cur;
                }
            }
        }

        private static IEnumerable<CodeInstruction> StartMovingRotatingTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
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
            MethodInfo getProp = AccessTools.PropertyGetter(typeof(InstanceID), nameof(InstanceID.Prop));
            MethodInfo getPMInstance = AccessTools.PropertyGetter(typeof(Singleton<PropManager>), nameof(Singleton<PropManager>.instance));
            MethodInfo getHidden = AccessTools.PropertyGetter(typeof(PropInstance), nameof(PropInstance.Hidden));
            MethodInfo setHidden = AccessTools.PropertySetter(typeof(PropInstance), nameof(PropInstance.Hidden));
            var codes = instructions.GetEnumerator();
            while(codes.MoveNext()) {
                var cur = codes.Current;
                if (cur.opcode == OpCodes.Call && cur.operand == getProp && codes.MoveNext()) {
                    var next = codes.Current;
                    if (next.opcode == OpCodes.Stloc_1) {
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(InstanceIDExtension), nameof(InstanceIDExtension.GetProp32ByRef)));
                        yield return newStLoc(prop32);
                    } else {
                        yield return cur;
                        yield return next;
                    }
                } else if (cur.opcode == OpCodes.Ldloc_1) {
                    yield return newLdLoc(prop32);
                } else if (cur.opcode == OpCodes.Call && cur.operand == getPMInstance) {
                    while (codes.MoveNext()) if (codes.Current.opcode == OpCodes.Call && codes.Current.operand == setHidden) break;
                    yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(DefaultTool), "m_angle"));
                    yield return newLdLoc(prop32);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EDefaultTool), nameof(EDefaultTool.StartMovingRotating)));
                } else {
                    yield return cur;
                }
            }
        }

        private static IEnumerable<CodeInstruction> SimulationStepTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
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
            IEnumerable<CodeInstruction> yieldPropBuffer(CodeInstruction cur, IEnumerator<CodeInstruction> __codes) {
                var next = __codes.Current;
                if (next.opcode == OpCodes.Ldfld && next.operand == m_props && __codes.MoveNext()) {
                    yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(EPropManager), nameof(EPropManager.m_props)));
                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Array32<EPropInstance>), nameof(Array32<EPropInstance>.m_buffer)));
                } else {
                    yield return cur;
                    yield return next;
                }
            }
            LocalBuilder raycastOutput = il.DeclareLocal(typeof(EToolBase.RaycastOutput));
            MethodInfo raycast = AccessTools.Method(typeof(ToolBase), "RayCast");
            MethodInfo getProp = AccessTools.PropertyGetter(typeof(InstanceID), nameof(InstanceID.Prop));
            MethodInfo setProp = AccessTools.PropertySetter(typeof(InstanceID), nameof(InstanceID.Prop));
            MethodInfo getPMInstance = AccessTools.PropertyGetter(typeof(Singleton<PropManager>), nameof(Singleton<PropManager>.instance));
            MethodInfo checkPlacement = AccessTools.Method(typeof(PropTool), nameof(PropTool.CheckPlacementErrors));
            MethodInfo getInfo = AccessTools.PropertyGetter(typeof(PropInstance), nameof(PropInstance.Info));
            MethodInfo getPosition = AccessTools.PropertyGetter(typeof(PropInstance), nameof(PropInstance.Position));
            MethodInfo checkProp = AccessTools.Method(typeof(DefaultTool), "CheckProp");
            var codes = instructions.GetEnumerator();
            while(codes.MoveNext()) {
                var cur = codes.Current;
                if (cur.opcode == OpCodes.Ldloca_S && (cur.operand as LocalBuilder).LocalIndex == 2 && codes.MoveNext()) {
reYield:
                    yield return new CodeInstruction(OpCodes.Ldloca_S, raycastOutput).WithLabels(cur.labels);
                    var next = codes.Current;
                    if (next.opcode == OpCodes.Ldfld || next.opcode == OpCodes.Ldflda) {
                        yield return modifyCode(next);
                    } else if (next.opcode == OpCodes.Call && next.operand == raycast) {
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EToolBase), nameof(EToolBase.RayCast)));
                    } else if (next.opcode == OpCodes.Ldloca_S && (next.operand as LocalBuilder).LocalIndex == 2 && codes.MoveNext()) {
                        goto reYield;
                    } else if (next.opcode == OpCodes.Call && next.operand == getPMInstance && codes.MoveNext()) {
                        foreach (var code in yieldPropBuffer(cur, codes)) yield return code;
                    } else if (next.opcode == OpCodes.Initobj && next.operand == typeof(ToolBase.RaycastOutput)) {
                        yield return new CodeInstruction(OpCodes.Initobj, typeof(EToolBase.RaycastOutput));
                    } else {
                        yield return next;
                    }
                } else if (cur.opcode == OpCodes.Call && cur.operand == raycast) {
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EToolBase), nameof(EToolBase.RayCast)));
                } else if (cur.opcode == OpCodes.Stfld) {
                    yield return modifyCode(cur);
                } else if (cur.opcode == OpCodes.Call && cur.operand == getProp) {
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(InstanceIDExtension), nameof(InstanceIDExtension.GetProp32ByRef)));
                } else if (cur.opcode == OpCodes.Call && cur.operand == getPMInstance && codes.MoveNext()) {
                    foreach (var code in yieldPropBuffer(cur, codes)) yield return code;
                } else if (cur.opcode == OpCodes.Ldelema && cur.operand == typeof(PropInstance)) {
                    yield return new CodeInstruction(OpCodes.Ldelema, typeof(EPropInstance));
                } else if (cur.opcode == OpCodes.Call && cur.operand == getInfo) {
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(EPropInstance), nameof(EPropInstance.Info)));
                } else if (cur.opcode == OpCodes.Call && cur.operand == checkPlacement) {
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EPropTool), nameof(EPropTool.CheckPlacementErrors)));
                } else if (cur.opcode == OpCodes.Ldarg_0 && codes.MoveNext()) {
                    var next = codes.Current;
                    if (next.opcode == OpCodes.Ldloca_S && (next.operand as LocalBuilder).LocalIndex == 2 && codes.MoveNext()) {
                        var next1 = codes.Current;
                        if (next1.opcode == OpCodes.Ldfld && next1.operand == m_propInstance && codes.MoveNext()) {
                            var next2 = codes.Current;
                            if (next2.opcode == OpCodes.Ldloca_S && codes.MoveNext()) {
                                var next3 = codes.Current;
                                if (next3.opcode == OpCodes.Callvirt && next3.operand == checkProp) {
                                    yield return cur;
                                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(DefaultTool), "m_toolController"));
                                    yield return new CodeInstruction(OpCodes.Ldloca_S, raycastOutput);
                                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(EToolBase.RaycastOutput), nameof(EToolBase.RaycastOutput.m_propInstance)));
                                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EDefaultTool), nameof(EDefaultTool.CheckProp)));
                                } else {
                                    yield return cur;
                                    yield return next;
                                    yield return next1;
                                    yield return next2;
                                    yield return next3;
                                }
                            } else {
                                yield return cur;
                                yield return next;
                                yield return next1;
                                yield return next2;
                            }
                        } else if (next1.opcode == OpCodes.Ldfld && next1.operand == m_hitPos) {
                            yield return cur;
                            yield return new CodeInstruction(OpCodes.Ldloca_S, raycastOutput);
                            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(EToolBase.RaycastOutput), nameof(EToolBase.RaycastOutput.m_hitPos)));
                        } else {
                            yield return cur;
                            yield return next;
                            yield return next1;
                        }
                    } else {
                        yield return cur;
                        yield return next;
                    }
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
            harmony.Patch(AccessTools.Method(typeof(DefaultTool).GetNestedType("<EndMoving>c__Iterator2", BindingFlags.Instance | BindingFlags.NonPublic), "MoveNext"),
                transpiler: new HarmonyMethod(AccessTools.Method(typeof(EDefaultToolPatch), nameof(EndMovingTranspiler))));
            harmony.Patch(AccessTools.Method(typeof(DefaultTool).GetNestedType("<EndRotating>c__Iterator3", BindingFlags.Instance | BindingFlags.NonPublic), "MoveNext"),
                transpiler: new HarmonyMethod(AccessTools.Method(typeof(EDefaultToolPatch), nameof(EndRotatingTranspiler))));
            harmony.Patch(AccessTools.Method(typeof(DefaultTool), nameof(DefaultTool.RenderGeometry)), transpiler: new HarmonyMethod(AccessTools.Method(typeof(EDefaultToolPatch), nameof(RenderGeometryTranspiler))));
            harmony.Patch(AccessTools.Method(typeof(DefaultTool), nameof(DefaultTool.RenderOverlay)), transpiler: new HarmonyMethod(AccessTools.Method(typeof(EDefaultToolPatch), nameof(RenderOverlayTranspiler))));
            harmony.Patch(AccessTools.Method(typeof(DefaultTool), "SetHoverInstance"), transpiler: new HarmonyMethod(AccessTools.Method(typeof(EDefaultToolPatch), nameof(SetHoverInstanceTranspiler))));
            harmony.Patch(AccessTools.Method(typeof(DefaultTool), nameof(DefaultTool.SimulationStep)), transpiler: new HarmonyMethod(AccessTools.Method(typeof(EDefaultToolPatch), nameof(SimulationStepTranspiler))));
            harmony.Patch(AccessTools.Method(typeof(DefaultTool).GetNestedType("<StartMoving>c__Iterator0", BindingFlags.Instance | BindingFlags.NonPublic), "MoveNext"),
                transpiler: new HarmonyMethod(AccessTools.Method(typeof(EDefaultToolPatch), nameof(StartMovingRotatingTranspiler))));
            harmony.Patch(AccessTools.Method(typeof(DefaultTool).GetNestedType("<StartRotating>c__Iterator1", BindingFlags.Instance | BindingFlags.NonPublic), "MoveNext"),
                transpiler: new HarmonyMethod(AccessTools.Method(typeof(EDefaultToolPatch), nameof(StartMovingRotatingTranspiler))));
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
