using ColossalFramework;
using ColossalFramework.Math;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using EManagersLib.API;
using static EManagersLib.API.EPropManager;

namespace EManagersLib {
    public class EPropToolPatch {
        public static float CalcSeedPropScale(PropInfo info, ref Randomizer defRandom) {
            Randomizer randomizer = new Randomizer((int)m_props.NextFreeItem(ref defRandom));
            return info.m_minScale + randomizer.Int32(10000u) * (info.m_maxScale - info.m_minScale) * 0.0001f;
        }

        private static IEnumerable<CodeInstruction> RenderGeometryTranspiler(IEnumerable<CodeInstruction> instructions) {
            FieldInfo m_props = AccessTools.Field(typeof(PropManager), nameof(PropManager.m_props));
            MethodInfo getPMInstance = AccessTools.PropertyGetter(typeof(Singleton<PropManager>), nameof(Singleton<PropManager>.instance));
            MethodInfo getNextFreeItem = AccessTools.Method(typeof(Array16<PropInstance>), nameof(Array16<PropInstance>.NextFreeItem));
            using (IEnumerator<CodeInstruction> codes = instructions.GetEnumerator()) {
                while (codes.MoveNext()) {
                    var cur = codes.Current;
                    if (cur.opcode == OpCodes.Call && cur.operand == getPMInstance && codes.MoveNext()) {
                        var next = codes.Current;
                        if (next.opcode == OpCodes.Ldfld && next.operand == m_props && codes.MoveNext()) {
                            var next1 = codes.Current;
                            if (next1.opcode == OpCodes.Ldloca_S) {
                                while (codes.MoveNext()) if (codes.Current.opcode == OpCodes.Stloc_S) break;
                                yield return new CodeInstruction(OpCodes.Ldloc_0);
                                yield return next1;
                                yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EPropToolPatch), nameof(CalcSeedPropScale)));
                                yield return codes.Current;
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

        private static IEnumerable<CodeInstruction> RenderOverlayTranspiler(IEnumerable<CodeInstruction> instructions) {
            FieldInfo m_props = AccessTools.Field(typeof(PropManager), nameof(PropManager.m_props));
            MethodInfo getPMInstance = AccessTools.PropertyGetter(typeof(Singleton<PropManager>), nameof(Singleton<PropManager>.instance));
            MethodInfo getNextFreeItem = AccessTools.Method(typeof(Array16<PropInstance>), nameof(Array16<PropInstance>.NextFreeItem));
            using (IEnumerator<CodeInstruction> codes = instructions.GetEnumerator()) {
                while (codes.MoveNext()) {
                    var cur = codes.Current;
                    if (cur.opcode == OpCodes.Call && cur.operand == getPMInstance && codes.MoveNext()) {
                        var next = codes.Current;
                        if (next.opcode == OpCodes.Ldfld && next.operand == m_props && codes.MoveNext()) {
                            var next1 = codes.Current;
                            if (next1.opcode == OpCodes.Ldloca_S) {
                                while (codes.MoveNext()) {
                                    if (codes.Current.opcode == OpCodes.Stloc_S) break;
                                }
                                yield return new CodeInstruction(OpCodes.Ldloc_0);
                                yield return next1;
                                yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EPropToolPatch), nameof(CalcSeedPropScale)));
                                yield return codes.Current;
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

        public static void SetRaycastInput(ref ToolBase.RaycastInput input) {
            input.m_ignoreBuildingFlags = Building.Flags.None;
            input.m_ignoreNodeFlags = NetNode.Flags.None;
            input.m_ignoreSegmentFlags = NetSegment.Flags.None;
            input.m_buildingService = new ToolBase.RaycastService(ItemClass.Service.None, ItemClass.SubService.None, ItemClass.Layer.Default);
            input.m_netService = new ToolBase.RaycastService(ItemClass.Service.None, ItemClass.SubService.None, ItemClass.Layer.Default);
            input.m_netService2 = new ToolBase.RaycastService(ItemClass.Service.None, ItemClass.SubService.None, ItemClass.Layer.Default);
        }

        public static bool CheckFixedHeight(ref Vector3 position, bool currentEditObject) {
            if (UsePropSnapping) {
                float terrainHeight = Singleton<TerrainManager>.instance.SampleDetailHeight(position);
                if (position.y > terrainHeight) return true;
                return false;
            }
            return currentEditObject;
        }

        private static IEnumerable<CodeInstruction> SimulationStepTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
            const int origRayCastOutputIndex = 3;
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
            bool firstModeSig = false, firstToolControllerSig = false;
            Label endBranch1 = il.DefineLabel();
            LocalBuilder raycastOutput = il.DeclareLocal(typeof(EToolBase.RaycastOutput)), randomizer = default, seed = il.DeclareLocal(typeof(uint));
            FieldInfo m_mode = AccessTools.Field(typeof(PropTool), nameof(PropTool.m_mode));
            FieldInfo m_toolController = AccessTools.Field(typeof(ToolBase), "m_toolController");
            FieldInfo m_placementError = AccessTools.Field(typeof(PropTool), "m_placementError");
            FieldInfo m_fixedHeight = AccessTools.Field(typeof(PropTool), "m_fixedHeight");
            MethodInfo getPMInstance = AccessTools.PropertyGetter(typeof(Singleton<PropManager>), nameof(Singleton<PropManager>.instance));
            MethodInfo getNextFreeItem = AccessTools.Method(typeof(Array16<PropInstance>), nameof(Array16<PropInstance>.NextFreeItem));
            MethodInfo checkPlacementError = AccessTools.Method(typeof(PropTool), nameof(PropTool.CheckPlacementErrors));
            MethodInfo raycast = AccessTools.Method(typeof(ToolBase), "RayCast");
            using (IEnumerator<CodeInstruction> codes = instructions.GetEnumerator()) {
                while (codes.MoveNext()) {
                    var cur = codes.Current;
                    if (randomizer is null && cur.opcode == OpCodes.Stloc_S && codes.MoveNext()) {
                        var next = codes.Current;
                        if (next.opcode == OpCodes.Call && next.operand == getPMInstance) {
                            randomizer = cur.operand as LocalBuilder;
                            while (codes.MoveNext()) {
                                var next1 = codes.Current;
                                if (next1.opcode == OpCodes.Call && next1.operand == checkPlacementError) break;
                            }
                            Label snappingEnabled = il.DefineLabel(), originalCode = il.DefineLabel();
                            yield return cur;
                            yield return new CodeInstruction(OpCodes.Ldloca_S, randomizer);
                            yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(EPropManager), nameof(m_props)));
                            yield return new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(Array32<EPropInstance>), nameof(Array32<EPropInstance>.NextFreeItem)));
                            yield return newStLoc(seed);
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PropTool), "m_propInfo"));
                            yield return new CodeInstruction(OpCodes.Ldloca_S, raycastOutput);
                            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(EToolBase.RaycastOutput), nameof(EToolBase.RaycastOutput.m_hitPos)));
                            yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(EPropManager), nameof(UsePropSnapping)));
                            yield return new CodeInstruction(OpCodes.Brtrue, snappingEnabled);
                            yield return new CodeInstruction(OpCodes.Ldloca_S, raycastOutput);
                            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(EToolBase.RaycastOutput), nameof(EToolBase.RaycastOutput.m_currentEditObject)));
                            yield return new CodeInstruction(OpCodes.Br, originalCode);
                            yield return new CodeInstruction(OpCodes.Ldc_I4_0).WithLabels(snappingEnabled);
                            yield return newLdLoc(seed).WithLabels(originalCode);
                            yield return new CodeInstruction(OpCodes.Ldloc_1);
                            yield return new CodeInstruction(OpCodes.Ldloc_2);
                            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EPropTool), nameof(EPropTool.CheckPlacementErrors)));
                        } else {
                            yield return cur;
                            yield return next;
                        }
                    } else if (cur.opcode == OpCodes.Ldarg_0 && codes.MoveNext()) {
                        var next = codes.Current;
                        if (!firstToolControllerSig && next.opcode == OpCodes.Ldfld && next.operand == m_toolController) {
                            firstToolControllerSig = true;
                            yield return new CodeInstruction(OpCodes.Br, endBranch1);
                            yield return new CodeInstruction(OpCodes.Ldloca_S, 0).WithLabels(cur.labels);
                            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EPropToolPatch), nameof(EPropToolPatch.SetRaycastInput)));
                            yield return new CodeInstruction(OpCodes.Ldarg_0).WithLabels(endBranch1);
                            yield return next;
                        } else if (!firstModeSig && next.opcode == OpCodes.Ldfld && next.operand == m_mode && codes.MoveNext()) {
                            var next1 = codes.Current;
                            if (next1.opcode == OpCodes.Ldc_I4_1 && codes.MoveNext()) {
                                var next2 = codes.Current;
                                if (next2.opcode == OpCodes.Bne_Un) {
                                    firstModeSig = true;
                                    yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(EPropManager), nameof(UsePropSnapping)));
                                    yield return new CodeInstruction(OpCodes.Brtrue, next2.operand);
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
                        } else if (next.opcode == OpCodes.Ldloca_S && (next.operand as LocalBuilder).LocalIndex == origRayCastOutputIndex && codes.MoveNext()) {
                            var next1 = codes.Current;
                            if (next1.opcode == OpCodes.Ldfld && next1.operand == m_currentEditObject && codes.MoveNext()) {
                                var next2 = codes.Current;
                                if (next2.opcode == OpCodes.Stfld && next2.operand == m_fixedHeight) {
                                    Label isSnappingEnabled = il.DefineLabel(), originalCode = il.DefineLabel();
                                    yield return cur;
                                    yield return new CodeInstruction(OpCodes.Ldarg_0);
                                    yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(PropTool), "m_mousePosition"));
                                    yield return newLdLoc(raycastOutput);
                                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(EToolBase.RaycastOutput), nameof(EToolBase.RaycastOutput.m_currentEditObject)));
                                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EPropToolPatch), nameof(EPropToolPatch.CheckFixedHeight)));
                                    yield return next2;
                                } else {
                                    yield return cur;
                                    yield return new CodeInstruction(OpCodes.Ldloca_S, raycastOutput);
                                    yield return modifyCode(next1);
                                    yield return next2;
                                }
                            } else {
                                yield return cur;
                                yield return new CodeInstruction(OpCodes.Ldloca_S, raycastOutput);
                                yield return modifyCode(next1);
                            }
                        } else {
                            yield return cur;
                            yield return next;
                        }
                    } else if (cur.opcode == OpCodes.Ldloca_S && (cur.operand as LocalBuilder).LocalIndex == origRayCastOutputIndex && codes.MoveNext()) {
                        var next = codes.Current;
                        yield return new CodeInstruction(OpCodes.Ldloca_S, raycastOutput);
                        if (next.opcode == OpCodes.Ldfld && next.operand == m_currentEditObject && codes.MoveNext()) {
                            var next1 = codes.Current;
                            yield return modifyCode(next);
                            yield return next1;
                            if (next1.opcode == OpCodes.Brtrue) {
                                yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(EPropManager), nameof(UsePropSnapping)));
                                yield return new CodeInstruction(OpCodes.Brtrue, next1.operand);
                            }
                        } else if (next.opcode == OpCodes.Ldfld || next.opcode == OpCodes.Ldflda) {
                            yield return modifyCode(next);
                        } else if (next.opcode == OpCodes.Call && next.operand == raycast) {
                            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EToolBase), nameof(EToolBase.RayCast)));
                        } else {
                            yield return next;
                        }
                    } else {
                        yield return cur;
                    }
                }
            }
        }

        private static IEnumerable<CodeInstruction> CreatePropTranspiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
            LocalBuilder seed = il.DeclareLocal(typeof(uint));
            FieldInfo m_props = AccessTools.Field(typeof(PropManager), nameof(PropManager.m_props));
            FieldInfo m_buffer = AccessTools.Field(typeof(Array16<PropInstance>), nameof(Array16<PropInstance>.m_buffer));
            FieldInfo m_propInfo = AccessTools.Field(typeof(PropTool), "m_propInfo");
            MethodInfo getPMInstance = AccessTools.PropertyGetter(typeof(Singleton<PropManager>), nameof(Singleton<PropManager>.instance));
            MethodInfo createProp = AccessTools.Method(typeof(PropManager), nameof(PropManager.CreateProp));
            MethodInfo setFixedHeight = AccessTools.PropertySetter(typeof(PropInstance), nameof(PropInstance.FixedHeight));
            using (IEnumerator<CodeInstruction> codes = instructions.GetEnumerator()) {
                while (codes.MoveNext()) {
                    var cur = codes.Current;
                    if (cur.opcode == OpCodes.Call && cur.operand == getPMInstance && codes.MoveNext()) {
                        var next = codes.Current;
                        if (next.opcode == OpCodes.Ldloca_S && (next.operand as LocalBuilder).LocalIndex == 4) {
                            yield return cur;
                            yield return new CodeInstruction(OpCodes.Ldloca_S, seed);
                        } else if (next.opcode == OpCodes.Ldfld && next.operand == m_props && codes.MoveNext()) {
                            var next1 = codes.Current;
                            if (next1.opcode == OpCodes.Ldfld && next1.operand == m_buffer && codes.MoveNext() && codes.MoveNext()) {
                                yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(EPropManager), nameof(EPropManager.m_props)));
                                yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Array32<EPropInstance>), nameof(Array32<EPropInstance>.m_buffer)));
                                yield return new CodeInstruction(OpCodes.Ldloc_S, seed);
                                yield return new CodeInstruction(OpCodes.Ldelema, typeof(EPropInstance));
                            } else {
                                yield return cur;
                                yield return next;
                                yield return next1;
                            }
                        } else {
                            yield return cur;
                            yield return next;
                        }
                    } else if (cur.opcode == OpCodes.Callvirt && cur.operand == createProp) {
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EPropManager), nameof(EPropManager.CreateProp)));
                    } else if (cur.opcode == OpCodes.Call && cur.operand == setFixedHeight) {
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.PropertySetter(typeof(EPropInstance), nameof(EPropInstance.FixedHeight)));
                    } else if (cur.opcode == OpCodes.Stfld && cur.operand == m_propInfo) {
                        yield return cur;
                        yield return new CodeInstruction(OpCodes.Ldsfld, AccessTools.Field(typeof(EPropManager), nameof(EPropManager.m_props)));
                        yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(Array32<EPropInstance>), nameof(Array32<EPropInstance>.m_buffer)));
                        yield return new CodeInstruction(OpCodes.Ldloc_S, seed);
                        yield return new CodeInstruction(OpCodes.Ldelema, typeof(EPropInstance));
                        yield return new CodeInstruction(OpCodes.Ldarg_0);
                        yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PropTool).GetNestedType("<CreateProp>c__Iterator0", BindingFlags.Instance | BindingFlags.NonPublic), "$this"));
                        yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PropTool), "m_fixedHeight"));
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.PropertySetter(typeof(EPropInstance), nameof(EPropInstance.FixedHeight)));
                    } else {
                        yield return cur;
                    }
                }
            }
        }

        public static void ApplyBrush(PropTool propTool, PropInfo prefabInfo, PropInfo info, ToolController toolController, ref Randomizer randomizer, Vector3 mousePosition, bool mouseLeftDown, bool mouseRightDown) {
            int Max(int a, int b) => (a <= b) ? b : a;
            int Min(int a, int b) => (a >= b) ? b : a;
            float Maxf(float a, float b) => (a <= b) ? b : a;
            int Clamp(int value, int min, int max) {
                value = (value < min) ? min : value;
                return (value > max) ? max : value;
            }
            float[] brushData = toolController.BrushData;
            float brushSize = propTool.m_brushSize * 0.5f;
            EPropInstance[] buffer = m_props.m_buffer;
            PropManager pmInstance = Singleton<PropManager>.instance;
            uint[] propGrid = m_propGrid;
            float strength = propTool.m_strength;
            int startX = Max((int)((mousePosition.x - brushSize) / PROPGRID_CELL_SIZE + PROPGRID_RESOLUTION * 0.5f), 0);
            int startZ = Max((int)((mousePosition.z - brushSize) / PROPGRID_CELL_SIZE + PROPGRID_RESOLUTION * 0.5f), 0);
            int endX = Min((int)((mousePosition.x + brushSize) / PROPGRID_CELL_SIZE + PROPGRID_RESOLUTION * 0.5f), PROPGRID_RESOLUTION - 1);
            int endZ = Min((int)((mousePosition.z + brushSize) / PROPGRID_CELL_SIZE + PROPGRID_RESOLUTION * 0.5f), PROPGRID_RESOLUTION - 1);
            for (int i = startZ; i <= endZ; i++) {
                float offsetZ = ((i - PROPGRID_RESOLUTION * 0.5f + 0.5f) * PROPGRID_CELL_SIZE - mousePosition.z + brushSize) / propTool.m_brushSize * PROPGRID_CELL_SIZE - 0.5f;
                int minOffsetZ = Clamp((int)Math.Floor(offsetZ), 0, 63);
                int maxOffsetZ = Clamp((int)Math.Ceiling(offsetZ), 0, 63);
                for (int j = startX; j <= endX; j++) {
                    float offsetX = ((j - PROPGRID_RESOLUTION * 0.5f + 0.5f) * PROPGRID_CELL_SIZE - mousePosition.x + brushSize) / propTool.m_brushSize * PROPGRID_CELL_SIZE - 0.5f;
                    int minOffsetX = Clamp((int)Math.Floor(offsetX), 0, 63);
                    int maxOffsetX = Clamp((int)Math.Ceiling(offsetX), 0, 63);
                    float strength1 = brushData[minOffsetZ * 64 + minOffsetX];
                    float strength2 = brushData[minOffsetZ * 64 + maxOffsetX];
                    float strength3 = brushData[maxOffsetZ * 64 + minOffsetX];
                    float strength4 = brushData[maxOffsetZ * 64 + maxOffsetX];
                    float overStrength1 = strength1 + (strength2 - strength1) * (offsetX - minOffsetX);
                    float overStrength2 = strength3 + (strength4 - strength3) * (offsetX - minOffsetX);
                    float finalStrength = overStrength1 + (overStrength2 - overStrength1) * (offsetZ - minOffsetZ);
                    int brushStrength = (int)(strength * (finalStrength * 1.2f - 0.2f) * 10000f);
                    if (mouseLeftDown && !(propTool.m_prefab is null)) {
                        if (randomizer.Int32(10000u) < brushStrength) {
                            PropInfo propInfo;
                            if ((Singleton<ToolManager>.instance.m_properties.m_mode & ItemClass.Availability.AssetEditor) != ItemClass.Availability.None) {
                                propInfo = prefabInfo;
                            } else {
                                propInfo = prefabInfo.GetVariation(ref randomizer);
                            }
                            Vector3 vector;
                            vector.x = (j - PROPGRID_RESOLUTION * 0.5f) * PROPGRID_CELL_SIZE;
                            vector.z = (i - PROPGRID_RESOLUTION * 0.5f) * PROPGRID_CELL_SIZE;
                            vector.x += (randomizer.Int32(10000u) + 0.5f) * (PROPGRID_CELL_SIZE / 10000f);
                            vector.z += (randomizer.Int32(10000u) + 0.5f) * (PROPGRID_CELL_SIZE / 10000f);
                            vector.y = 0f;
                            vector.y = Singleton<TerrainManager>.instance.SampleDetailHeight(vector, out float f, out float f2);
                            float angle = randomizer.Int32(10000u) * 0.0006283185f;
                            if (Maxf(Math.Abs(f), Math.Abs(f2)) < randomizer.Int32(10000u) * 5E-05f) {
                                Randomizer newRandomizer = new Randomizer((int)m_props.NextFreeItem(ref randomizer));
                                float height = info.m_generatedInfo.m_size.y * (info.m_minScale + newRandomizer.Int32(10000u) * (info.m_maxScale - info.m_minScale) * 0.0001f);
                                Vector2 vector2 = VectorUtils.XZ(vector);
                                Quad2 quad = default;
                                quad.a = vector2 + new Vector2(-4.5f, -4.5f);
                                quad.b = vector2 + new Vector2(-4.5f, 4.5f);
                                quad.c = vector2 + new Vector2(4.5f, 4.5f);
                                quad.d = vector2 + new Vector2(4.5f, -4.5f);
                                Quad2 quad2 = default;
                                quad2.a = vector2 + new Vector2(-8f, -8f);
                                quad2.b = vector2 + new Vector2(-8f, 8f);
                                quad2.c = vector2 + new Vector2(8f, 8f);
                                quad2.d = vector2 + new Vector2(8f, -8f);
                                float y = mousePosition.y;
                                float maxY = mousePosition.y + height;
                                ItemClass.CollisionType collisionType = ItemClass.CollisionType.Terrain;
                                if (!OverlapQuad(quad2, y, maxY, collisionType, 0, 0)) {
                                    if (!Singleton<TreeManager>.instance.OverlapQuad(quad, y, maxY, collisionType, 0, 0u)) {
                                        if (!Singleton<NetManager>.instance.OverlapQuad(quad, y, maxY, collisionType, propInfo.m_class.m_layer, 0, 0, 0)) {
                                            if (!Singleton<BuildingManager>.instance.OverlapQuad(quad, y, maxY, collisionType, propInfo.m_class.m_layer, 0, 0, 0)) {
                                                if (propInfo.m_requireWaterMap || !Singleton<TerrainManager>.instance.HasWater(vector2)) {
                                                    if (!Singleton<GameAreaManager>.instance.QuadOutOfArea(quad)) {
                                                        if (pmInstance.CreateProp(out uint _, ref randomizer, propInfo, vector, angle, false)) {
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    } else if (mouseRightDown || prefabInfo is null) {
                        uint propID = propGrid[i * PROPGRID_RESOLUTION + j];
                        while (propID != 0) {
                            if (randomizer.Int32(10000u) < brushStrength) {
                                pmInstance.ReleaseProp(propID);
                            }
                            propID = buffer[propID].m_nextGridProp;
                        }
                    }
                }
            }
        }

        private static IEnumerable<CodeInstruction> ApplyBrushTranspiler(IEnumerable<CodeInstruction> instructions) {
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PropTool), nameof(PropTool.m_prefab)));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PropTool), "m_propInfo"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PropTool), "m_toolController"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldflda, AccessTools.Field(typeof(PropTool), "m_randomizer"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PropTool), "m_mousePosition"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PropTool), "m_mouseLeftDown"));
            yield return new CodeInstruction(OpCodes.Ldarg_0);
            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PropTool), "m_mouseRightDown"));
            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EPropToolPatch), nameof(ApplyBrush)));
            yield return new CodeInstruction(OpCodes.Ret);
        }

        internal void Enable(Harmony harmony) {
            harmony.Patch(AccessTools.Method(typeof(PropTool), nameof(PropTool.RenderOverlay), new Type[] { typeof(RenderManager.CameraInfo) }),
                transpiler: new HarmonyMethod(AccessTools.Method(typeof(EPropToolPatch), nameof(RenderOverlayTranspiler))));
            harmony.Patch(AccessTools.Method(typeof(PropTool), nameof(PropTool.RenderGeometry)), transpiler: new HarmonyMethod(AccessTools.Method(typeof(EPropToolPatch), nameof(RenderGeometryTranspiler))));
            harmony.Patch(AccessTools.Method(typeof(PropTool), nameof(PropTool.SimulationStep)),
                transpiler: new HarmonyMethod(AccessTools.Method(typeof(EPropToolPatch), nameof(SimulationStepTranspiler))));
            harmony.Patch(AccessTools.Method(typeof(PropTool).GetNestedType("<CreateProp>c__Iterator0", BindingFlags.Instance | BindingFlags.NonPublic), "MoveNext"),
                transpiler: new HarmonyMethod(AccessTools.Method(typeof(EPropToolPatch), nameof(CreatePropTranspiler))));
            harmony.Patch(AccessTools.Method(typeof(PropTool), "ApplyBrush"),
                transpiler: new HarmonyMethod(AccessTools.Method(typeof(EPropToolPatch), nameof(ApplyBrushTranspiler))));
        }

        internal void Disable(Harmony harmony) {
            harmony.Unpatch(AccessTools.Method(typeof(PropTool), nameof(PropTool.RenderOverlay), new Type[] { typeof(RenderManager.CameraInfo) }), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(PropTool), nameof(PropTool.RenderGeometry)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(PropTool), nameof(PropTool.SimulationStep)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(PropTool).GetNestedType("<CreateProp>c__Iterator0", BindingFlags.Instance | BindingFlags.NonPublic), "MoveNext"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(PropTool), "ApplyBrush"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
        }

    }
}
