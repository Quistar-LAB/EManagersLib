using ColossalFramework;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace EManagersLib.Patches {
    internal class EBuildingDecorationPatch {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void ReleaseProps() {
            PropManager pmInstance = Singleton<PropManager>.instance;
            EPropInstance[] propBuffer = EPropManager.m_props.m_buffer;
            for (uint i = 1; i < propBuffer.Length; i++) {
                if (propBuffer[i].m_flags != 0) {
                    pmInstance.ReleaseProp(i);
                }
            }
        }

        /* BuildingDecoration::ClearDecoration() patch */
        private static IEnumerable<CodeInstruction> BDClearDecorationTranspiler(IEnumerable<CodeInstruction> instructions) {
            bool skipCodes = false, skipOnce = false;
            MethodInfo get_PropInstance = AccessTools.PropertyGetter(typeof(Singleton<PropManager>), nameof(Singleton<PropManager>.instance));
            foreach (var code in instructions) {
                if (code.opcode == OpCodes.Call && code.operand == get_PropInstance) {
                    skipCodes = true;
                } else if (skipCodes && code.opcode == OpCodes.Blt) {
                    skipCodes = false;
                    skipOnce = true;
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EBuildingDecorationPatch), nameof(ReleaseProps)));
                }
                if (!skipCodes && !skipOnce) {
                    yield return code;
                }
                skipOnce = false;
            }
        }

        public static void SaveBuildingProps(Building data, FastList<BuildingInfo.Prop> fastList, Matrix4x4 matrix4x) {
            EPropInstance[] propBuffer = EPropManager.m_props.m_buffer;
            int len = EPropManager.MAX_PROP_LIMIT;
            for (int i = 1; i < len; i++) {
                if ((propBuffer[i].m_flags & (EPropInstance.BLOCKEDFLAG | EPropInstance.CREATEDFLAG | EPropInstance.DELETEDFLAG)) == 1) {
                    BuildingInfo.Prop prop = new BuildingInfo.Prop {
                        m_prop = propBuffer[i].Info,
                        m_position = matrix4x.MultiplyPoint(propBuffer[i].Position),
                        m_radAngle = propBuffer[i].Angle - data.m_angle,
                        m_fixedHeight = propBuffer[i].FixedHeight,
                        m_probability = 100
                    };
                    prop.m_finalProp = prop.m_prop;
                    prop.m_angle = 57.29578f * prop.m_radAngle;
                    fastList.Add(prop);
                }
            }
        }

        /* BuildingDecoration::SaveProps() patch */
        private static IEnumerable<CodeInstruction> BDSavePropsTranspiler(IEnumerable<CodeInstruction> instructions) {
            bool skipCodes = false, skipOnce = false;
            MethodInfo get_PropInstance = AccessTools.PropertyGetter(typeof(Singleton<PropManager>), nameof(Singleton<PropManager>.instance));
            foreach (var code in instructions) {
                if (code.opcode == OpCodes.Call && code.operand == get_PropInstance) {
                    skipCodes = true;
                } else if (skipCodes && code.opcode == OpCodes.Blt) {
                    skipCodes = false;
                    skipOnce = true;
                    yield return new CodeInstruction(OpCodes.Ldarg_2);
                    yield return new CodeInstruction(OpCodes.Ldobj, typeof(Building));
                    yield return new CodeInstruction(OpCodes.Ldloc_0);
                    yield return new CodeInstruction(OpCodes.Ldloc_3);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EBuildingDecorationPatch), nameof(SaveBuildingProps)));
                }
                if (!skipCodes && !skipOnce) {
                    yield return code;
                }
                skipOnce = false;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void LoadBuildingProps(PropManager propManager, PropInfo finalProp, bool fixedHeight, Vector3 vector, float angle) {
            if (propManager.CreateProp(out uint propID, ref Singleton<SimulationManager>.instance.m_randomizer, finalProp, vector, angle, true)) {
                EPropManager.m_props.m_buffer[propID].FixedHeight = fixedHeight;
            }
        }

        /* BuildingDecoration::LoadProps() patch */
        private static IEnumerable<CodeInstruction> BDLoadPropsTranspiler(IEnumerable<CodeInstruction> instructions) {
            LocalBuilder finalProp = default, angle = default, vector = default, index = default;
            MethodInfo setFixedHeight = AccessTools.PropertySetter(typeof(PropInstance), nameof(PropInstance.FixedHeight));
            MethodInfo muliplyPoint = AccessTools.Method(typeof(Matrix4x4), nameof(Matrix4x4.MultiplyPoint));
            FieldInfo m_finalProp = AccessTools.Field(typeof(BuildingInfo.Prop), nameof(BuildingInfo.Prop.m_finalProp));
            FieldInfo m_angle = AccessTools.Field(typeof(BuildingInfo.Prop), nameof(BuildingInfo.Prop.m_angle));
            using (IEnumerator<CodeInstruction> codes = instructions.GetEnumerator()) {
                while (codes.MoveNext()) {
                    var cur = codes.Current;
                    if (cur.opcode == OpCodes.Ldc_I4_0 && codes.MoveNext()) {
                        var next = codes.Current;
                        if (next.opcode == OpCodes.Stloc_S) {
                            index = next.operand as LocalBuilder;
                        }
                        yield return cur;
                        yield return next;
                    } else if (cur.opcode == OpCodes.Ldfld && cur.operand == m_finalProp && codes.MoveNext()) {
                        var next = codes.Current;
                        if (next.opcode == OpCodes.Stloc_S) {
                            finalProp = next.operand as LocalBuilder;
                        }
                        yield return cur;
                        yield return next;
                    } else if (cur.opcode == OpCodes.Call && cur.operand == muliplyPoint && codes.MoveNext()) {
                        var next = codes.Current;
                        if (next.opcode == OpCodes.Stloc_S) {
                            vector = next.operand as LocalBuilder;
                        }
                        yield return cur;
                        yield return next;
                    } else if (cur.opcode == OpCodes.Ldfld && cur.operand == m_angle && codes.MoveNext()) {
                        var next = codes.Current;
                        if (next.opcode == OpCodes.Mul && codes.MoveNext()) {
                            var next1 = codes.Current;
                            if (next1.opcode == OpCodes.Add && codes.MoveNext()) {
                                var next2 = codes.Current;
                                if (next2.opcode == OpCodes.Stloc_S) {
                                    angle = next2.operand as LocalBuilder;
                                }
                                yield return cur;
                                yield return next;
                                yield return next1;
                                yield return next2;
                            } else {
                                yield return cur;
                                yield return next;
                                yield return next1;
                            }
                        } else {
                            yield return cur;
                            yield return next;
                        }
                    } else if (cur.opcode == OpCodes.Ldloc_3) {
                        yield return cur;
                        while (codes.MoveNext()) {
                            var skipCode = codes.Current;
                            if (skipCode.opcode == OpCodes.Call && skipCode.operand == setFixedHeight) break;
                        }
                        yield return new CodeInstruction(OpCodes.Ldloc_S, finalProp);
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(BuildingInfo), nameof(BuildingInfo.m_props)));
                        yield return new CodeInstruction(OpCodes.Ldloc_S, index);
                        yield return new CodeInstruction(OpCodes.Ldelem_Ref);
                        yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(BuildingInfo.Prop), nameof(BuildingInfo.Prop.m_fixedHeight)));
                        yield return new CodeInstruction(OpCodes.Ldloc_S, vector);
                        yield return new CodeInstruction(OpCodes.Ldloc_S, angle);
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EBuildingDecorationPatch), nameof(LoadBuildingProps)));
                    } else {
                        yield return cur;
                    }
                }
            }
        }

        internal void Enable(Harmony harmony) {
            try {
                harmony.Patch(AccessTools.Method(typeof(BuildingDecoration), nameof(BuildingDecoration.ClearDecorations)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EBuildingDecorationPatch), nameof(BDClearDecorationTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch BuildingDecoration::ClearDecorations");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(BuildingDecoration), nameof(BuildingDecoration.ClearDecorations)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(BuildingDecoration), nameof(BuildingDecoration.SaveProps)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EBuildingDecorationPatch), nameof(BDSavePropsTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch BuildingDecoration::SaveProps");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(BuildingDecoration), nameof(BuildingDecoration.SaveProps)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(BuildingDecoration), nameof(BuildingDecoration.LoadProps)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EBuildingDecorationPatch), nameof(BDLoadPropsTranspiler))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch BuildingDecoration::LoadProps");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(BuildingDecoration), nameof(BuildingDecoration.LoadProps)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
        }

        internal void Disable(Harmony harmony) {
            harmony.Unpatch(AccessTools.Method(typeof(BuildingDecoration), nameof(BuildingDecoration.ClearDecorations)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(BuildingDecoration), nameof(BuildingDecoration.SaveProps)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(BuildingDecoration), nameof(BuildingDecoration.LoadProps)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
        }
    }
}
