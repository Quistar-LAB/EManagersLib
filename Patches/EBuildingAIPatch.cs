using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace EManagersLib {
    internal class EBuildingAIPatch {
        private static IEnumerable<CodeInstruction> CalculatePropGroupDataTranspiler(IEnumerable<CodeInstruction> instructions) {
            MethodInfo calculateGroupData = typeof(PropInstance).GetMethod(nameof(PropInstance.CalculateGroupData), BindingFlags.Public | BindingFlags.Static);
            foreach (var code in instructions) {
                if (code.opcode == OpCodes.Call && code.operand == calculateGroupData) {
                    yield return new CodeInstruction(OpCodes.Call, typeof(EPropInstance).GetMethod(nameof(EPropInstance.CalculateGroupData), BindingFlags.Public | BindingFlags.Static));
                } else {
                    yield return code;
                }
            }
        }

        private static IEnumerable<CodeInstruction> PopulatePropGroupDataTranspiler(IEnumerable<CodeInstruction> instructions) {
            MethodInfo populateGroupData = typeof(PropInstance).GetMethod(nameof(PropInstance.PopulateGroupData), BindingFlags.Public | BindingFlags.Static);
            foreach (var code in instructions) {
                if (code.opcode == OpCodes.Call && code.operand == populateGroupData) {
                    yield return new CodeInstruction(OpCodes.Call, typeof(EPropInstance).GetMethod(nameof(EPropInstance.PopulateGroupData), BindingFlags.Public | BindingFlags.Static));
                } else {
                    yield return code;
                }
            }
        }

        private static IEnumerable<CodeInstruction> RenderDestroyedPropsTranspiler(IEnumerable<CodeInstruction> instructions) {
            MethodInfo renderInstanceWatermap = AccessTools.Method(typeof(PropInstance), nameof(PropInstance.RenderInstance), new Type[] {
                typeof(RenderManager.CameraInfo), typeof(PropInfo), typeof(InstanceID), typeof(Vector3), typeof(float), typeof(float), typeof(Color),
                typeof(Vector4), typeof(bool), typeof(Texture), typeof(Vector4), typeof(Vector4), typeof(Texture), typeof(Vector4), typeof(Vector4)
            });
            MethodInfo renderInstanceHeightmap = AccessTools.Method(typeof(PropInstance), nameof(PropInstance.RenderInstance), new Type[] {
                typeof(RenderManager.CameraInfo), typeof(PropInfo), typeof(InstanceID), typeof(Vector3), typeof(float),
                typeof(float), typeof(Color), typeof(Vector4), typeof(bool), typeof(Texture), typeof(Vector4), typeof(Vector4)
            });
            MethodInfo renderInstance = AccessTools.Method(typeof(PropInstance), nameof(PropInstance.RenderInstance), new Type[] {
                typeof(RenderManager.CameraInfo), typeof(PropInfo), typeof(InstanceID), typeof(Vector3), typeof(float), typeof(float), typeof(Color), typeof(Vector4), typeof(bool)
            });
            foreach (var code in instructions) {
                if (code.opcode == OpCodes.Call && code.operand == renderInstanceWatermap) {
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EPropInstance), nameof(EPropInstance.RenderInstance), new Type[] {
                        typeof(RenderManager.CameraInfo), typeof(PropInfo), typeof(InstanceID), typeof(Vector3), typeof(float), typeof(float), typeof(Color),
                        typeof(Vector4), typeof(bool), typeof(Texture), typeof(Vector4), typeof(Vector4), typeof(Texture), typeof(Vector4), typeof(Vector4)
                    }));
                } else if (code.opcode == OpCodes.Call && code.operand == renderInstanceHeightmap) {
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EPropInstance), nameof(EPropInstance.RenderInstance), new Type[] {
                        typeof(RenderManager.CameraInfo), typeof(PropInfo), typeof(InstanceID), typeof(Vector3), typeof(float),
                        typeof(float), typeof(Color), typeof(Vector4), typeof(bool), typeof(Texture), typeof(Vector4), typeof(Vector4)
                    }));
                } else if (code.opcode == OpCodes.Call && code.operand == renderInstance) {
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EPropInstance), nameof(EPropInstance.RenderInstance), new Type[] {
                        typeof(RenderManager.CameraInfo), typeof(PropInfo), typeof(InstanceID), typeof(Vector3), typeof(float), typeof(float), typeof(Color), typeof(Vector4), typeof(bool)
                    }));
                } else {
                    yield return code;
                }
            }
        }

        private static IEnumerable<CodeInstruction> RenderPropsTranspiler(IEnumerable<CodeInstruction> instructions) {
            MethodInfo renderInstanceWatermap = AccessTools.Method(typeof(PropInstance), nameof(PropInstance.RenderInstance), new Type[] {
                typeof(RenderManager.CameraInfo), typeof(PropInfo), typeof(InstanceID), typeof(Vector3), typeof(float), typeof(float), typeof(Color),
                typeof(Vector4), typeof(bool), typeof(Texture), typeof(Vector4), typeof(Vector4), typeof(Texture), typeof(Vector4), typeof(Vector4)
            });
            MethodInfo renderInstanceHeightmap = AccessTools.Method(typeof(PropInstance), nameof(PropInstance.RenderInstance), new Type[] {
                typeof(RenderManager.CameraInfo), typeof(PropInfo), typeof(InstanceID), typeof(Vector3), typeof(float),
                typeof(float), typeof(Color), typeof(Vector4), typeof(bool), typeof(Texture), typeof(Vector4), typeof(Vector4)
            });
            MethodInfo renderInstance = typeof(PropInstance).GetMethod(nameof(PropInstance.RenderInstance), BindingFlags.Public | BindingFlags.Static, null, CallingConventions.Standard, new Type[] {
                typeof(RenderManager.CameraInfo), typeof(PropInfo), typeof(InstanceID), typeof(Vector3), typeof(float), typeof(float), typeof(Color), typeof(Vector4), typeof(bool)
            }, null);
            foreach (var code in instructions) {
                if (code.opcode == OpCodes.Call && code.operand == renderInstanceWatermap) {
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EPropInstance), nameof(EPropInstance.RenderInstance), new Type[] {
                        typeof(RenderManager.CameraInfo), typeof(PropInfo), typeof(InstanceID), typeof(Vector3), typeof(float), typeof(float), typeof(Color),
                        typeof(Vector4), typeof(bool), typeof(Texture), typeof(Vector4), typeof(Vector4), typeof(Texture), typeof(Vector4), typeof(Vector4)
                    }));
                } else if (code.opcode == OpCodes.Call && code.operand == renderInstanceHeightmap) {
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EPropInstance), nameof(EPropInstance.RenderInstance), new Type[] {
                        typeof(RenderManager.CameraInfo), typeof(PropInfo), typeof(InstanceID), typeof(Vector3), typeof(float),
                        typeof(float), typeof(Color), typeof(Vector4), typeof(bool), typeof(Texture), typeof(Vector4), typeof(Vector4)
                    }));
                } else if (code.opcode == OpCodes.Call && code.operand == renderInstance) {
                    yield return new CodeInstruction(OpCodes.Call, typeof(EPropInstance).GetMethod(nameof(EPropInstance.RenderInstance), BindingFlags.Public | BindingFlags.Static, null, CallingConventions.Standard, new Type[] {
                        typeof(RenderManager.CameraInfo), typeof(PropInfo), typeof(InstanceID), typeof(Vector3), typeof(float), typeof(float), typeof(Color), typeof(Vector4), typeof(bool)
                    }, null));
                } else {
                    yield return code;
                }
            }
        }

        internal void Enable(Harmony harmony) {
            harmony.Patch(AccessTools.Method(typeof(BuildingAI), "CalculatePropGroupData"), transpiler: new HarmonyMethod(AccessTools.Method(typeof(EBuildingAIPatch), nameof(CalculatePropGroupDataTranspiler))));
            harmony.Patch(AccessTools.Method(typeof(BuildingAI), "PopulatePropGroupData"), transpiler: new HarmonyMethod(AccessTools.Method(typeof(EBuildingAIPatch), nameof(PopulatePropGroupDataTranspiler))));
            harmony.Patch(AccessTools.Method(typeof(BuildingAI), "RenderDestroyedProps"), transpiler: new HarmonyMethod(AccessTools.Method(typeof(EBuildingAIPatch), nameof(RenderDestroyedPropsTranspiler))));
            harmony.Patch(AccessTools.Method(typeof(BuildingAI), "RenderProps", new Type[] {
                typeof(RenderManager.CameraInfo), typeof(ushort), typeof(Building).MakeByRefType(), typeof(int), typeof(RenderManager.Instance).MakeByRefType(), typeof(bool), typeof(bool), typeof(bool)
            }), transpiler: new HarmonyMethod(AccessTools.Method(typeof(EBuildingAIPatch), nameof(RenderPropsTranspiler))));
        }

        internal void Disable(Harmony harmony) {
            harmony.Unpatch(AccessTools.Method(typeof(BuildingAI), "CalculatePropGroupData"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(BuildingAI), "PopulatePropGroupData"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(BuildingAI), "RenderDestroyedProps"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(BuildingAI), "RenderProps", new Type[] {
                typeof(RenderManager.CameraInfo), typeof(ushort), typeof(Building).MakeByRefType(), typeof(int), typeof(RenderManager.Instance).MakeByRefType(), typeof(bool), typeof(bool), typeof(bool)
            }), HarmonyPatchType.Transpiler, EModule.HARMONYID);
        }
    }
}
