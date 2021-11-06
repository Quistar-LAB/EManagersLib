using ColossalFramework;
using ColossalFramework.IO;
using ColossalFramework.Math;
using EManagersLib.API;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using static EManagersLib.API.EPropManager;

namespace EManagersLib {
    internal class EPropManagerPatch {
        private static IEnumerable<CodeInstruction> AwakeTranspiler(IEnumerable<CodeInstruction> instructions) {
            using (IEnumerator<CodeInstruction> codes = instructions.GetEnumerator()) {
                while (codes.MoveNext()) {
                    var cur = codes.Current;
                    if (cur.opcode == OpCodes.Ldarg_0 && codes.MoveNext()) {
                        var next = codes.Current;
                        if (next.LoadsConstant(EPropManager.DEFAULT_PROP_LIMIT) && codes.MoveNext()) {
                            var next1 = codes.Current;
                            if (next1.opcode == OpCodes.Newobj && codes.MoveNext()) {
                                codes.MoveNext(); /* LDARG_0 */
                                codes.MoveNext(); /* LDC_I4 */
                                codes.MoveNext(); /* NEWARR */
                                codes.MoveNext(); /* STFLD m_updatedProp */
                                codes.MoveNext(); /* LDARG_0 */
                                codes.MoveNext(); /* LDC_I4 */
                                codes.MoveNext(); /* NEWARR */
                                codes.MoveNext(); /* STFLD */
                            } else {
                                yield return cur;
                                yield return next;
                                yield return next1;
                            }
                        } else if (next.opcode == OpCodes.Ldfld && codes.MoveNext()) {
                            var next1 = codes.Current;
                            if (next1.opcode == OpCodes.Ldloca_S && codes.MoveNext()) {
                                var next2 = codes.Current;
                                if (next2.opcode == OpCodes.Callvirt && codes.MoveNext()) {
                                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EPropManager), nameof(EPropManager.Awake)));
                                } else {
                                    yield return cur;
                                    yield return next;
                                    yield return next1;
                                    yield return next2;
                                }
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
        }

        /* This method is completely overriden, do not touch */
        private static IEnumerable<CodeInstruction> AfterTerrainUpdateTranspiler(IEnumerable<CodeInstruction> instructions) {
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EPropManager), nameof(EPropManager.AfterTerrainUpdate)));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        private static IEnumerable<CodeInstruction> CalculateGroupDataTranspiler(IEnumerable<CodeInstruction> instructions) {
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Ldarg_2);
            yield return new CodeInstruction(OpCodes.Ldarg_3);
            yield return new CodeInstruction(OpCodes.Ldarga_S, 4);
            yield return new CodeInstruction(OpCodes.Ldarga_S, 5);
            yield return new CodeInstruction(OpCodes.Ldarga_S, 6);
            yield return new CodeInstruction(OpCodes.Ldarga_S, 7);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EPropManager), nameof(EPropManager.CalculateGroupData)));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        private static IEnumerable<CodeInstruction> CheckLimitsTranspiler(IEnumerable<CodeInstruction> instructions) {
            foreach (var code in instructions) {
                if (code.LoadsConstant(EPropManager.DEFAULT_MAP_PROPS)) {
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(EPropManager), nameof(EPropManager.MAX_MAP_PROPS_LIMIT)));
                } else if (code.LoadsConstant(EPropManager.DEFAULT_GAME_PROPS_LIMIT)) {
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(EPropManager), nameof(EPropManager.MAX_GAME_PROPS_LIMIT)));
                } else {
                    yield return code;
                }
            }
        }

        private static IEnumerable<CodeInstruction> EndRenderingImplTranspiler(IEnumerable<CodeInstruction> instructions) {
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EPropManager), nameof(EPropManager.EndRenderingImpl)));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        private static IEnumerable<CodeInstruction> PopulateGroupDataTranspiler(IEnumerable<CodeInstruction> instructions) {
            yield return new CodeInstruction(OpCodes.Ldarg_1); /* groupX */
            yield return new CodeInstruction(OpCodes.Ldarg_2); /* groupZ */
            yield return new CodeInstruction(OpCodes.Ldarg_3); /* layer */
            yield return new CodeInstruction(OpCodes.Ldarg_S, 4); /* vertexIndex */
            yield return new CodeInstruction(OpCodes.Ldarg_S, 5); /* triangleIndex */
            yield return new CodeInstruction(OpCodes.Ldarg_S, 6); /* groupPosition */
            yield return new CodeInstruction(OpCodes.Ldarg_S, 7); /* data */
            yield return new CodeInstruction(OpCodes.Ldarg_S, 8); /* min */
            yield return new CodeInstruction(OpCodes.Ldarg_S, 9); /* max */
            yield return new CodeInstruction(OpCodes.Ldarg_S, 10); /* maxRenderDistance */
            yield return new CodeInstruction(OpCodes.Ldarg_S, 11); /* maxInstanceDistance */
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EPropManager), nameof(EPropManager.PopulateGroupData)));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        private static IEnumerable<CodeInstruction> SampleSmoothHeightTranspiler(IEnumerable<CodeInstruction> instructions) {
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EPropManager), nameof(EPropManager.SampleSmoothHeight)));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        private static IEnumerable<CodeInstruction> SimulationStepImplTranspiler(IEnumerable<CodeInstruction> instructions) {
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EPropManager), nameof(EPropManager.SimulationStepImpl)));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        private static IEnumerable<CodeInstruction> TerrainUpdatedTranspiler(IEnumerable<CodeInstruction> instructions) {
            yield return new CodeInstruction(OpCodes.Ldarg_2);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EPropManager), nameof(EPropManager.TerrainUpdated)));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        private static IEnumerable<CodeInstruction> UpdateDataTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
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
            FieldInfo propsArray = AccessTools.Field(typeof(PropManager), nameof(PropManager.m_props));
            FieldInfo propsBuffer = AccessTools.Field(typeof(Array16<PropInstance>), nameof(Array16<PropInstance>.m_buffer));
            MethodInfo beginLoading = AccessTools.Method(typeof(LoadingProfiler), nameof(LoadingProfiler.BeginLoading));
            MethodInfo releaseProp = AccessTools.Method(typeof(PropManager), nameof(PropManager.ReleaseProp));
            LocalBuilder props = il.DeclareLocal(typeof(PropInstance[]));
            using (IEnumerator<CodeInstruction> codes = instructions.GetEnumerator()) {
                while (codes.MoveNext()) {
                    var cur = codes.Current;
                    if (cur.opcode == OpCodes.Callvirt && cur.operand == beginLoading) {
                        yield return cur;
                        yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(EPropManager), nameof(EPropManager.m_props)));
                        yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Array32<EPropInstance>), nameof(Array32<EPropInstance>.m_buffer)));
                        yield return newStLoc(props);
                    } else if (cur.opcode == OpCodes.Ldarg_0 && codes.MoveNext()) {
                        var next = codes.Current;
                        if (next.opcode == OpCodes.Ldfld && next.operand == propsArray && codes.MoveNext()) {
                            var next1 = codes.Current;
                            if (next1.opcode == OpCodes.Ldfld && next1.operand == propsBuffer) {
                                yield return newLdLoc(props).WithLabels(cur.labels);
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
                    } else if (cur.opcode == OpCodes.Conv_U2 && codes.MoveNext()) {
                        var next = codes.Current;
                        if (next.opcode == OpCodes.Call && next.operand == releaseProp) {
                            yield return new CodeInstruction(OpCodes.Conv_U4);
                            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EPropManager), nameof(EPropManager.ReleaseProp)));
                        } else {
                            yield return cur;
                            yield return next;
                        }
                    } else if (cur.LoadsConstant(EPropManager.DEFAULT_PROP_LIMIT) && codes.MoveNext()) {
                        var next = codes.Current;
                        if (next.opcode == OpCodes.Blt) {
                            //yield return new CodeInstruction(newLdLoc(props));
                            //yield return new CodeInstruction(OpCodes.Ldlen);
                            //yield return new CodeInstruction(OpCodes.Conv_I4);
                            yield return new CodeInstruction(OpCodes.Ldc_I4, EPropManager.MAX_PROP_LIMIT);
                            yield return next;
                        } else {
                            yield return cur;
                            yield return next;
                        }
                    } else {
                        yield return cur;
                    }
                }
            }
        }

        private static IEnumerable<CodeInstruction> OverlapQuadTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
            var codes = __OverlapQuadTranspiler(instructions, il);
            foreach (var code in codes) {
                EUtils.ELog(code.ToString());
            }
            return codes;
        }
        private static IEnumerable<CodeInstruction> __OverlapQuadTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
            LocalBuilder propID = il.DeclareLocal(typeof(uint));
            FieldInfo propGrid = AccessTools.Field(typeof(PropManager), nameof(PropManager.m_propGrid));
            FieldInfo m_props = AccessTools.Field(typeof(PropManager), nameof(PropManager.m_props));
            FieldInfo m_buffer = AccessTools.Field(typeof(Array16<PropInstance>), nameof(Array16<PropInstance>.m_buffer));
            FieldInfo m_nextGridProp = AccessTools.Field(typeof(PropInstance), nameof(PropInstance.m_nextGridProp));
            MethodInfo getPosition = AccessTools.PropertyGetter(typeof(PropInstance), nameof(PropInstance.Position));
            MethodInfo overlapQuad = AccessTools.Method(typeof(PropInstance), nameof(PropInstance.OverlapQuad));
            using (IEnumerator<CodeInstruction> codes = instructions.GetEnumerator()) {
                while (codes.MoveNext()) {
                    var cur = codes.Current;
                    if (cur.opcode == OpCodes.Ldarg_0 && codes.MoveNext()) {
                        var next = codes.Current;
                        if (next.opcode == OpCodes.Ldfld && next.operand == propGrid) {
                            yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(EPropManager), nameof(m_propGrid))).WithLabels(cur.labels);
                        } else if (next.opcode == OpCodes.Ldfld && next.operand == m_props && codes.MoveNext()) {
                            yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(EPropManager), nameof(EPropManager.m_props))).WithLabels(cur.labels);
                            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Array32<EPropInstance>), nameof(Array32<EPropInstance>.m_buffer)));
                        } else if ((next.opcode == OpCodes.Stloc_S || next.opcode == OpCodes.Ldloc_S) && (next.operand as LocalBuilder).LocalType == typeof(ushort)) {
                            yield return new CodeInstruction(next.opcode, propID).WithLabels(cur.labels);
                        } else {
                            yield return cur;
                            yield return next;
                        }
                    } else if ((cur.opcode == OpCodes.Stloc_S || cur.opcode == OpCodes.Ldloc_S) && (cur.operand as LocalBuilder).LocalType == typeof(ushort)) {
                        yield return new CodeInstruction(cur.opcode, propID).WithLabels(cur.labels);
                    } else if (cur.opcode == OpCodes.Ldelem_U2) {
                        yield return new CodeInstruction(OpCodes.Ldelem_U4);
                    } else if (cur.opcode == OpCodes.Ldelema && cur.operand == typeof(PropInstance)) {
                        yield return new CodeInstruction(OpCodes.Ldelema, typeof(EPropInstance));
                    } else if (cur.opcode == OpCodes.Call && cur.operand == getPosition) {
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(EPropInstance), nameof(EPropInstance.Position)));
                    } else if (cur.opcode == OpCodes.Call && cur.operand == overlapQuad) {
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EPropInstance), nameof(EPropInstance.OverlapQuad)));
                    } else if (cur.opcode == OpCodes.Ldfld && cur.operand == propGrid) {
                        yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(EPropManager), nameof(m_propGrid))).WithLabels(cur.labels);
                    } else if (cur.opcode == OpCodes.Ldfld && cur.operand == m_nextGridProp) {
                        yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(EPropInstance), nameof(EPropInstance.m_nextGridProp)));
                    } else if (cur.opcode == OpCodes.Ldc_I4 && (cur.operand as int?).Value == DEFAULT_PROP_LIMIT) {
                        yield return new CodeInstruction(OpCodes.Ldc_I4, MAX_PROP_LIMIT);
                    } else {
                        yield return cur;
                    }
                }
            }
        }

        private static IEnumerable<CodeInstruction> RayCastTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
            LocalBuilder propID = il.DeclareLocal(typeof(uint));
            FieldInfo propGrid = AccessTools.Field(typeof(PropManager), nameof(PropManager.m_propGrid));
            FieldInfo m_props = AccessTools.Field(typeof(PropManager), nameof(PropManager.m_props));
            FieldInfo m_buffer = AccessTools.Field(typeof(Array16<PropInstance>), nameof(Array16<PropInstance>.m_buffer));
            FieldInfo m_flags = AccessTools.Field(typeof(PropInstance), nameof(PropInstance.m_flags));
            FieldInfo m_nextGridProp = AccessTools.Field(typeof(PropInstance), nameof(PropInstance.m_nextGridProp));
            MethodInfo getPosition = AccessTools.PropertyGetter(typeof(PropInstance), nameof(PropInstance.Position));
            MethodInfo overlapQuad = AccessTools.Method(typeof(PropInstance), nameof(PropInstance.OverlapQuad));
            MethodInfo getInfo = AccessTools.PropertyGetter(typeof(PropInstance), nameof(PropInstance.Info));
            MethodInfo propRaycast = AccessTools.Method(typeof(PropInstance), nameof(PropInstance.RayCast));
            using (IEnumerator<CodeInstruction> codes = instructions.GetEnumerator()) {
                while (codes.MoveNext()) {
                    var cur = codes.Current;
                    if (cur.opcode == OpCodes.Ldarg_0 && codes.MoveNext()) {
                        var next = codes.Current;
                        if (next.opcode == OpCodes.Ldfld && next.operand == propGrid) {
                            yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(EPropManager), nameof(m_propGrid))).WithLabels(cur.labels);
                        } else if (next.opcode == OpCodes.Ldfld && next.operand == m_props && codes.MoveNext()) {
                            yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(EPropManager), nameof(EPropManager.m_props))).WithLabels(cur.labels);
                            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Array32<EPropInstance>), nameof(Array32<EPropInstance>.m_buffer)));
                        } else if ((next.opcode == OpCodes.Stloc_S || next.opcode == OpCodes.Ldloc_S) && (next.operand as LocalBuilder).LocalType == typeof(ushort)) {
                            yield return new CodeInstruction(next.opcode, propID).WithLabels(cur.labels);
                        } else {
                            yield return cur;
                            yield return next;
                        }
                    } else if ((cur.opcode == OpCodes.Stloc_S || cur.opcode == OpCodes.Ldloc_S) && (cur.operand as LocalBuilder).LocalType == typeof(ushort)) {
                        yield return new CodeInstruction(cur.opcode, propID).WithLabels(cur.labels);
                    } else if (cur.opcode == OpCodes.Ldelem_U2) {
                        yield return new CodeInstruction(OpCodes.Ldelem_U4);
                    } else if (cur.opcode == OpCodes.Ldelema && cur.operand == typeof(PropInstance)) {
                        yield return new CodeInstruction(OpCodes.Ldelema, typeof(EPropInstance));
                    } else if (cur.opcode == OpCodes.Call && cur.operand == getPosition) {
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(EPropInstance), nameof(EPropInstance.Position)));
                    } else if (cur.opcode == OpCodes.Call && cur.operand == overlapQuad) {
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EPropInstance), nameof(EPropInstance.OverlapQuad)));
                    } else if (cur.opcode == OpCodes.Ldfld && cur.operand == propGrid) {
                        yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(EPropManager), nameof(m_propGrid))).WithLabels(cur.labels);
                    } else if (cur.opcode == OpCodes.Ldfld && cur.operand == m_flags) {
                        yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(EPropInstance), nameof(EPropInstance.m_flags)));
                    } else if (cur.opcode == OpCodes.Call && cur.operand == getInfo) {
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(EPropInstance), nameof(EPropInstance.Info)));
                    } else if (cur.opcode == OpCodes.Call && cur.operand == propRaycast) {
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EPropInstance), nameof(EPropInstance.RayCast)));
                    } else if (cur.opcode == OpCodes.Ldfld && cur.operand == m_nextGridProp) {
                        yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(EPropInstance), nameof(EPropInstance.m_nextGridProp)));
                    } else if (cur.opcode == OpCodes.Ldc_I4 && (cur.operand as int?).Value == DEFAULT_PROP_LIMIT) {
                        yield return new CodeInstruction(OpCodes.Ldc_I4, MAX_PROP_LIMIT);
                    } else {
                        yield return cur;
                    }
                }
            }
        }

        private static IEnumerable<CodeInstruction> UpdatePropsTranspiler(IEnumerable<CodeInstruction> instructions) {
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Ldarg_2);
            yield return new CodeInstruction(OpCodes.Ldarg_3);
            yield return new CodeInstruction(OpCodes.Ldarg_S, 4);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EPropManager), nameof(EPropManager.UpdateProps)));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        public static void SetPropScaleColor(uint id, ref EPropInstance prop, PropInfo info) {
            if (prop.m_scale == 0f) {
                Randomizer rand = new Randomizer((int)id);
                prop.m_scale = info.m_minScale + rand.Int32(10000u) * (info.m_maxScale - info.m_minScale) * 0.0001f;
                prop.m_color = info.GetColor(ref rand);
            }
        }

        private static IEnumerable<CodeInstruction> AfterDeserializeTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
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
            LocalBuilder buffer = il.DeclareLocal(typeof(EPropInstance[]));
            FieldInfo m_props = AccessTools.Field(typeof(PropManager), nameof(PropManager.m_props));
            FieldInfo m_buffer = AccessTools.Field(typeof(Array16<PropInstance>), nameof(Array16<PropInstance>.m_buffer));
            FieldInfo m_infoIndex = AccessTools.Field(typeof(PropInstance), nameof(PropInstance.m_infoIndex));
            FieldInfo m_flags = AccessTools.Field(typeof(PropInstance), nameof(PropInstance.m_flags));
            FieldInfo m_propCount = AccessTools.Field(typeof(PropManager), nameof(PropManager.m_propCount));
            MethodInfo getInfo = AccessTools.PropertyGetter(typeof(PropInstance), nameof(PropInstance.Info));
            MethodInfo getBlocked = AccessTools.PropertyGetter(typeof(PropInstance), nameof(PropInstance.Blocked));
            MethodInfo getPosition = AccessTools.PropertyGetter(typeof(PropInstance), nameof(PropInstance.Position));
            MethodInfo getItemCount = AccessTools.Method(typeof(Array16<PropInstance>), nameof(Array16<PropInstance>.ItemCount));
            using (IEnumerator<CodeInstruction> codes = instructions.GetEnumerator()) {
                while (codes.MoveNext()) {
                    var cur = codes.Current;
                    if (cur.opcode == OpCodes.Ldloc_0 && codes.MoveNext()) {
                        var next = codes.Current;
                        if (next.opcode == OpCodes.Ldfld && next.operand == m_props && codes.MoveNext()) {
                            var next1 = codes.Current;
                            if (next1.opcode == OpCodes.Ldfld && next1.operand == m_buffer && codes.MoveNext()) {
                                var next2 = codes.Current;
                                if (next2.opcode == OpCodes.Stloc_1) {
                                    yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(EPropManager), nameof(EPropManager.m_props)));
                                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Array32<EPropInstance>), nameof(Array32<EPropInstance>.m_buffer)));
                                    yield return newStLoc(buffer);
                                    codes.MoveNext(); /* ldloc1 */
                                    codes.MoveNext(); /* ldlen */
                                    codes.MoveNext(); /* convi4 */
                                    yield return new CodeInstruction(OpCodes.Ldc_I4, EPropManager.MAX_PROP_LIMIT);
                                } else {
                                    yield return cur;
                                    yield return next;
                                    yield return next1;
                                    yield return next2;
                                }
                            } else {
                                yield return cur;
                                yield return next;
                                yield return next1;
                            }
                        } else if (next.opcode == OpCodes.Ldloc_0 && codes.MoveNext()) {
                            var next1 = codes.Current;
                            if (next1.opcode == OpCodes.Ldfld && next1.operand == m_props && codes.MoveNext()) {
                                var next2 = codes.Current;
                                if (next2.opcode == OpCodes.Callvirt && next2.operand == getItemCount) {
                                    yield return cur;
                                    yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(EPropManager), nameof(EPropManager.m_props)));
                                    yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Array32<EPropInstance>), nameof(Array32<EPropInstance>.ItemCount)));
                                } else {
                                    yield return cur;
                                    yield return next;
                                    yield return next1;
                                    yield return next2;
                                }
                            } else {
                                yield return cur;
                                yield return next;
                                yield return next1;
                            }
                        } else {
                            yield return cur;
                            yield return next;
                        }
                    } else if (cur.opcode == OpCodes.Ldloc_1 && codes.MoveNext()) {
                        var next = codes.Current;
                        if (next.opcode == OpCodes.Ldloc_3 && codes.MoveNext()) {
                            var next1 = codes.Current;
                            if (next1.opcode == OpCodes.Ldelema && codes.MoveNext()) {
                                var next2 = codes.Current;
                                yield return newLdLoc(buffer).WithLabels(cur.labels);
                                yield return next;
                                yield return new CodeInstruction(OpCodes.Ldelema, typeof(EPropInstance));
                                if (next2.opcode == OpCodes.Call && next2.operand == getInfo) {
                                    yield return new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(EPropInstance), nameof(EPropInstance.Info)));
                                } else if (next2.opcode == OpCodes.Call && next2.operand == getBlocked) {
                                    yield return new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(EPropInstance), nameof(EPropInstance.Blocked)));
                                } else if (next2.opcode == OpCodes.Call && next2.operand == getPosition) {
                                    yield return new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(EPropInstance), nameof(EPropInstance.Position)));
                                } else if (next2.opcode == OpCodes.Ldfld && next2.operand == m_flags) {
                                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(EPropInstance), nameof(EPropInstance.m_flags)));
                                } else {
                                    yield return next2;
                                }
                            } else {
                                yield return cur;
                                yield return next;
                                yield return next1;
                            }
                        } else {
                            yield return cur;
                            yield return next;
                        }
                    } else if (cur.opcode == OpCodes.Stfld && cur.operand == m_infoIndex) {
                        yield return new CodeInstruction(OpCodes.Stfld, AccessTools.Field(typeof(EPropInstance), nameof(EPropInstance.m_infoIndex)));
                        yield return new CodeInstruction(OpCodes.Ldloc_3);
                        yield return newLdLoc(buffer);
                        yield return new CodeInstruction(OpCodes.Ldloc_3);
                        yield return new CodeInstruction(OpCodes.Ldelema, typeof(EPropInstance));
                        yield return new CodeInstruction(OpCodes.Ldloc_S, 4);
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EPropManagerPatch), nameof(SetPropScaleColor)));
                    } else if (cur.opcode == OpCodes.Ldfld && cur.operand == m_flags) {
                        yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(EPropInstance), nameof(EPropInstance.m_flags)));
                    } else {
                        yield return cur;
                    }
                }
            }
        }

        private static IEnumerable<CodeInstruction> DeserializeTranspiler(IEnumerable<CodeInstruction> instructions) {
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EPropManagerPatch), nameof(Deserialize)));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        private static IEnumerable<CodeInstruction> SerializeTranspiler(IEnumerable<CodeInstruction> instructions) {
            yield return new CodeInstruction(OpCodes.Ldarg_1);
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EPropManagerPatch), nameof(Serialize)));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        public static void Deserialize(DataSerializer s) {
            Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.BeginDeserialize(s, "PropManager");
            PropManager instance = Singleton<PropManager>.instance;
            EnsureCapacity(instance);
            EPropInstance[] buffer = m_props.m_buffer;
            uint[] propGrid = m_propGrid;
            m_props.ClearUnused();
            SimulationManager.UpdateMode updateMode = Singleton<SimulationManager>.instance.m_metaData.m_updateMode;
            bool assetEditor = updateMode == SimulationManager.UpdateMode.NewAsset || updateMode == SimulationManager.UpdateMode.LoadAsset;
            for (int i = 0; i < propGrid.Length; i++) {
                propGrid[i] = 0;
            }
            EncodedArray.UShort uShort = EncodedArray.UShort.BeginRead(s);
            for (int i = 1; i < DEFAULT_PROP_LIMIT; i++) {
                buffer[i].m_flags = uShort.Read();
            }
            uShort.EndRead();
            PrefabCollection<PropInfo>.BeginDeserialize(s);
            for (int i = 1; i < DEFAULT_PROP_LIMIT; i++) {
                if (buffer[i].m_flags != 0) {
                    buffer[i].m_infoIndex = (ushort)PrefabCollection<PropInfo>.Deserialize(true);
                }
            }
            PrefabCollection<PropInfo>.EndDeserialize(s);
            EncodedArray.Short @short = EncodedArray.Short.BeginRead(s);
            for (int i = 1; i < DEFAULT_PROP_LIMIT; i++) {
                if (buffer[i].m_flags != 0) {
                    buffer[i].m_posX = @short.Read();
                } else {
                    buffer[i].m_posX = 0;
                }
            }
            @short.EndRead();
            EncodedArray.Short short2 = EncodedArray.Short.BeginRead(s);
            for (int i = 1; i < DEFAULT_PROP_LIMIT; i++) {
                if (buffer[i].m_flags != 0) {
                    buffer[i].m_posZ = short2.Read();
                } else {
                    buffer[i].m_posZ = 0;
                }
            }
            short2.EndRead();
            EncodedArray.UShort uShort2 = EncodedArray.UShort.BeginRead(s);
            for (int i = 1; i < DEFAULT_PROP_LIMIT; i++) {
                if (buffer[i].m_flags != 0) {
                    buffer[i].m_angle = uShort2.Read();
                } else {
                    buffer[i].m_angle = 0;
                }
            }
            uShort2.EndRead();
            ESerializableData.IntegratedPropDeserialize(buffer);
            buffer = m_props.m_buffer;
            int len = buffer.Length;
            for (int i = 1; i < len; i++) {
                buffer[i].m_nextGridProp = 0;
                if (buffer[i].m_flags != 0) {
                    InitializeProp((uint)i, ref buffer[i], assetEditor);
                } else {
                    m_props.ReleaseItem((uint)i);
                }
            }
            Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.EndDeserialize(s, "PropManager");
        }

        public static void Serialize(DataSerializer s) {
            Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.BeginSerialize(s, "PropManager");
            EPropInstance[] buffer = m_props.m_buffer;
            int num = DEFAULT_PROP_LIMIT;
            EncodedArray.UShort uShort = EncodedArray.UShort.BeginWrite(s);
            for (int i = 1; i < num; i++) {
                uShort.Write(buffer[i].m_flags);
            }
            uShort.EndWrite();
            try {
                PrefabCollection<PropInfo>.BeginSerialize(s);
                for (int i = 1; i < num; i++) {
                    if (buffer[i].m_flags != 0) {
                        PrefabCollection<PropInfo>.Serialize(buffer[i].m_infoIndex);
                    }
                }
            } finally {
                PrefabCollection<PropInfo>.EndSerialize(s);
            }
            EncodedArray.Short @short = EncodedArray.Short.BeginWrite(s);
            for (int i = 1; i < num; i++) {
                if (buffer[i].m_flags != 0) {
                    @short.Write(buffer[i].m_posX);
                }
            }
            @short.EndWrite();
            EncodedArray.Short short2 = EncodedArray.Short.BeginWrite(s);
            for (int i = 1; i < num; i++) {
                if (buffer[i].m_flags != 0) {
                    short2.Write(buffer[i].m_posZ);
                }
            }
            short2.EndWrite();
            EncodedArray.UShort uShort2 = EncodedArray.UShort.BeginWrite(s);
            for (int i = 1; i < num; i++) {
                if (buffer[i].m_flags != 0) {
                    uShort2.Write(buffer[i].m_angle);
                }
            }
            uShort2.EndWrite();
            Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.EndSerialize(s, "PropManager");
        }



        internal void Enable(Harmony harmony) {
            harmony.Patch(AccessTools.Method(typeof(PropManager), "Awake"), transpiler: new HarmonyMethod(AccessTools.Method(typeof(EPropManagerPatch), nameof(AwakeTranspiler))));
            harmony.Patch(AccessTools.Method(typeof(PropManager), nameof(PropManager.AfterTerrainUpdate)), transpiler: new HarmonyMethod(AccessTools.Method(typeof(EPropManagerPatch), nameof(AfterTerrainUpdateTranspiler))));
            harmony.Patch(AccessTools.Method(typeof(PropManager), nameof(PropManager.CalculateGroupData)), transpiler: new HarmonyMethod(AccessTools.Method(typeof(EPropManagerPatch), nameof(CalculateGroupDataTranspiler))));
            harmony.Patch(AccessTools.Method(typeof(PropManager), nameof(PropManager.CheckLimits)), transpiler: new HarmonyMethod(AccessTools.Method(typeof(EPropManagerPatch), nameof(CheckLimitsTranspiler))));
            harmony.Patch(AccessTools.Method(typeof(PropManager), "EndRenderingImpl"), transpiler: new HarmonyMethod(AccessTools.Method(typeof(EPropManagerPatch), nameof(EndRenderingImplTranspiler))));
            harmony.Patch(AccessTools.Method(typeof(PropManager), nameof(PropManager.OverlapQuad)), transpiler: new HarmonyMethod(AccessTools.Method(typeof(EPropManagerPatch), nameof(OverlapQuadTranspiler))));
            harmony.Patch(AccessTools.Method(typeof(PropManager), nameof(PropManager.PopulateGroupData)), transpiler: new HarmonyMethod(AccessTools.Method(typeof(EPropManagerPatch), nameof(PopulateGroupDataTranspiler))));
            harmony.Patch(AccessTools.Method(typeof(PropManager), nameof(PropManager.RayCast)), transpiler: new HarmonyMethod(AccessTools.Method(typeof(EPropManagerPatch), nameof(RayCastTranspiler))));
            harmony.Patch(AccessTools.Method(typeof(PropManager), nameof(PropManager.SampleSmoothHeight)), transpiler: new HarmonyMethod(AccessTools.Method(typeof(EPropManagerPatch), nameof(SampleSmoothHeightTranspiler))));
            harmony.Patch(AccessTools.Method(typeof(PropManager), "SimulationStepImpl"), transpiler: new HarmonyMethod(AccessTools.Method(typeof(EPropManagerPatch), nameof(SimulationStepImplTranspiler))));
            harmony.Patch(AccessTools.Method(typeof(PropManager), nameof(PropManager.TerrainUpdated)), transpiler: new HarmonyMethod(AccessTools.Method(typeof(EPropManagerPatch), nameof(TerrainUpdatedTranspiler))));
            harmony.Patch(AccessTools.Method(typeof(PropManager), nameof(PropManager.UpdateData)), transpiler: new HarmonyMethod(AccessTools.Method(typeof(EPropManagerPatch), nameof(UpdateDataTranspiler))));
            harmony.Patch(AccessTools.Method(typeof(PropManager), nameof(PropManager.UpdateProps)), transpiler: new HarmonyMethod(AccessTools.Method(typeof(EPropManagerPatch), nameof(UpdatePropsTranspiler))));
            harmony.Patch(AccessTools.Method(typeof(PropManager.Data), nameof(PropManager.Data.Deserialize)), transpiler: new HarmonyMethod(AccessTools.Method(typeof(EPropManagerPatch), nameof(DeserializeTranspiler))));
            harmony.Patch(AccessTools.Method(typeof(PropManager.Data), nameof(PropManager.Data.AfterDeserialize)), transpiler: new HarmonyMethod(AccessTools.Method(typeof(EPropManagerPatch), nameof(AfterDeserializeTranspiler))));
            harmony.Patch(AccessTools.Method(typeof(PropManager.Data), nameof(PropManager.Data.Serialize)), transpiler: new HarmonyMethod(AccessTools.Method(typeof(EPropManagerPatch), nameof(SerializeTranspiler))));
        }

        internal void Disable(Harmony harmony) {
            harmony.Unpatch(AccessTools.Method(typeof(PropManager), "Awake"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(PropManager), nameof(PropManager.AfterTerrainUpdate)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(PropManager), nameof(PropManager.CalculateGroupData)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(PropManager), nameof(PropManager.CheckLimits)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(PropManager), "EndRenderingImpl"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(PropManager), nameof(PropManager.OverlapQuad)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(PropManager), nameof(PropManager.RayCast)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(PropManager), nameof(PropManager.PopulateGroupData)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(PropManager), nameof(PropManager.SampleSmoothHeight)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(PropManager), "SimulationStepImpl"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(PropManager), nameof(PropManager.TerrainUpdated)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(PropManager), nameof(PropManager.UpdateData)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(PropManager), nameof(PropManager.UpdateProps)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(PropManager.Data), nameof(PropManager.Data.Deserialize)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(PropManager.Data), nameof(PropManager.Data.AfterDeserialize)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(PropManager.Data), nameof(PropManager.Data.Serialize)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
        }
    }
}
