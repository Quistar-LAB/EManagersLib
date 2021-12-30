using ColossalFramework;
using ColossalFramework.IO;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace EManagersLib.Patches {
    internal class EGameAreaManagerPatch {
        internal static IEnumerable<CodeInstruction> ReplaceDefaultConstants(IEnumerable<CodeInstruction> instructions) {
            foreach (var code in instructions) {
                if (code.LoadsConstant(EGameAreaManager.MINAREACOUNT)) {
                    code.operand = EGameAreaManager.CUSTOMAREACOUNT;
                    yield return code;
                } else if (code.LoadsConstant(EGameAreaManager.DEFAULTGRIDSIZE)) {
                    code.operand = EGameAreaManager.CUSTOMGRIDSIZE;
                    yield return code;
                } else if (code.LoadsConstant((float)EGameAreaManager.DEFAULTGRIDSIZE / 2)) {
                    code.operand = (float)EGameAreaManager.CUSTOMGRIDSIZE / 2;
                    yield return code;
                } else {
                    yield return code;
                }
            }
        }

        internal static IEnumerable<CodeInstruction> ReplaceGetTileXZ(IEnumerable<CodeInstruction> instructions) {
            MethodInfo getTileXZ = AccessTools.Method(typeof(GameAreaManager), nameof(GameAreaManager.GetTileXZ), new Type[] { typeof(int), typeof(int).MakeByRefType(), typeof(int).MakeByRefType() });
            foreach (var code in instructions) {
                if (code.opcode == OpCodes.Callvirt && code.operand == getTileXZ) {
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EGameAreaManager), nameof(EGameAreaManager.GetTileXZ)));
                } else {
                    yield return code;
                }
            }
        }

        internal static IEnumerable<CodeInstruction> ReplaceGetTileIndex(IEnumerable<CodeInstruction> instructions) {
            MethodInfo getTileIndex = AccessTools.Method(typeof(GameAreaManager), nameof(GameAreaManager.GetTileIndex));
            foreach (var code in instructions) {
                if (code.opcode == OpCodes.Callvirt && code.operand == getTileIndex) {
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EGameAreaManager), nameof(EGameAreaManager.GetTileIndex)));
                } else {
                    yield return code;
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> GetMaxAreaCountTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceDefaultConstants(instructions);

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> AwakeTranspiler(IEnumerable<CodeInstruction> instructions) {
            foreach (var code in instructions) {
                if (code.LoadsConstant(EGameAreaManager.DEFAULTAREACOUNT)) {
                    code.operand = EGameAreaManager.CUSTOMAREACOUNT;
                    yield return code;
                } else {
                    yield return code;
                }
            }
        }

        private delegate void SetWaterMaterialPropertiesAPI(Vector3 worldPos, Material material);
        internal static bool BeginOverlayImplPrefix(GameAreaManager __instance, RenderManager.CameraInfo cameraInfo, Texture2D ___m_areaTex,
                                                    float ___m_borderAlpha, Material ___m_borderMaterial, Mesh ___m_borderMesh, int ___ID_Color,
                                                    float ___m_areaAlpha, Material ___m_areaMaterial, int ___ID_AreaMapping) {
            ItemClass.Availability mode = Singleton<ToolManager>.instance.m_properties.m_mode;
            if ((mode & ItemClass.Availability.Editors) == ItemClass.Availability.None) {
                if (___m_borderAlpha >= 0.001f && !(___m_borderMaterial is null)) {
                    SetWaterMaterialPropertiesAPI SetWaterMaterialProperties = Singleton<TerrainManager>.instance.SetWaterMaterialProperties;
                    Quaternion rotation = Quaternion.AngleAxis(90f, Vector3.up);
                    Color value5 = Color.white;
                    ToolController properties = Singleton<ToolManager>.instance.m_properties;
                    if (!(properties is null)) {
                        ToolBase currentTool = properties.CurrentTool;
                        if ((currentTool.GetErrors() & ToolBase.ToolErrors.OutOfArea) != ToolBase.ToolErrors.None) {
                            value5 = Color.red;
                        }
                    }
                    value5.a = ___m_borderAlpha;
                    for (int k = 0; k <= EGameAreaManager.CUSTOMGRIDSIZE; k++) {
                        for (int l = 0; l <= EGameAreaManager.CUSTOMGRIDSIZE; l++) {
                            bool flag8 = __instance.GetArea(l, k) > 0;
                            bool flag9 = __instance.GetArea(l, k - 1) > 0;
                            bool flag10 = __instance.GetArea(l - 1, k) > 0;
                            if (flag8 != flag9) {
                                Vector3 vector = new Vector3((l - (EGameAreaManager.CUSTOMGRIDSIZE / 2f) + 0.5f) * 1920f, 0f, (k - (EGameAreaManager.CUSTOMGRIDSIZE / 2f)) * 1920f);
                                Vector3 size = new Vector3(1920f, 1024f, 100f);
                                Bounds bounds = new Bounds(vector + new Vector3(0f, size.y * 0.5f, 0f), size);
                                if (cameraInfo.Intersect(bounds)) {
                                    SetWaterMaterialProperties(vector, ___m_borderMaterial);
                                    ___m_borderMaterial.SetColor(___ID_Color, value5);
                                    if (___m_borderMaterial.SetPass(0)) {
                                        __instance.m_drawCallData.m_overlayCalls++;
                                        Graphics.DrawMeshNow(___m_borderMesh, vector, rotation);
                                    }
                                }
                            }
                            if (flag8 != flag10) {
                                Vector3 vector = new Vector3((l - (EGameAreaManager.CUSTOMGRIDSIZE / 2f)) * 1920f, 0f, (k - (EGameAreaManager.CUSTOMGRIDSIZE / 2f) + 0.5f) * 1920f);
                                Vector3 size6 = new Vector3(100f, 1024f, 1920f);
                                Bounds bounds7 = new Bounds(vector + new Vector3(0f, size6.y * 0.5f, 0f), size6);
                                if (cameraInfo.Intersect(bounds7)) {
                                    SetWaterMaterialProperties(vector, ___m_borderMaterial);
                                    ___m_borderMaterial.SetColor(___ID_Color, value5);
                                    if (___m_borderMaterial.SetPass(0)) {
                                        __instance.m_drawCallData.m_overlayCalls++;
                                        Graphics.DrawMeshNow(___m_borderMesh, vector, Quaternion.identity);
                                    }
                                }
                            }
                        }
                    }
                }
                if (___m_areaAlpha >= 0.001f && !(___m_areaMaterial is null)) {
                    Vector4 vector;
                    vector.z = 1.0f / (1920.0f * EGameAreaManager.CUSTOMAREATEXSIZE);
                    vector.x = (EGameAreaManager.CUSTOMGRIDSIZE + 0.0f) / (EGameAreaManager.CUSTOMAREATEXSIZE * 2.0f);
                    vector.y = (EGameAreaManager.CUSTOMGRIDSIZE + 0.0f) / (EGameAreaManager.CUSTOMAREATEXSIZE * 2.0f);
                    vector.w = 1.0f / (1.0f * EGameAreaManager.CUSTOMAREATEXSIZE);
                    //value6.z = 6.510417E-05f;
                    //value6.x = 0.4375f;
                    //value6.y = 0.4375f;
                    //value6.w = 0.125f;
                    ___m_areaMaterial.mainTexture = ___m_areaTex;
                    ___m_areaMaterial.SetColor(___ID_Color, new Color(1f, 1f, 1f, ___m_areaAlpha));
                    ___m_areaMaterial.SetVector(___ID_AreaMapping, vector);
                    Vector3 zero = Vector3.zero;
                    Vector3 zero2 = Vector3.zero;
                    for (int i = 0; i < EGameAreaManager.CUSTOMGRIDSIZE; i++) {
                        for (int j = 0; j < EGameAreaManager.CUSTOMGRIDSIZE; j++) {
                            if (__instance.IsUnlocked(j, i)) {
                                zero.x = EMath.Min(zero.x, ((j - 1) - (EGameAreaManager.CUSTOMGRIDSIZE / 2f)) * 1920f);
                                zero2.x = EMath.Max(zero2.x, ((j + 2) - (EGameAreaManager.CUSTOMGRIDSIZE / 2f)) * 1920f);
                                zero.z = EMath.Min(zero.z, ((i - 1) - (EGameAreaManager.CUSTOMGRIDSIZE / 2f)) * 1920f);
                                zero2.z = EMath.Max(zero2.z, ((i + 2) - (EGameAreaManager.CUSTOMGRIDSIZE / 2f)) * 1920f);
                                zero2.y = EMath.Max(zero2.y, 1024f);
                            }
                        }
                    }
                    Bounds freeBounds = default;
                    freeBounds.SetMinMax(zero, zero2);
                    freeBounds.size += new Vector3(100f, 1f, 100f);
                    __instance.m_drawCallData.m_overlayCalls++;
                    Singleton<RenderManager>.instance.OverlayEffect.DrawEffect(cameraInfo, ___m_areaMaterial, 0, freeBounds);
                }
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> CalculateTilePriceTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceDefaultConstants(instructions);

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> CanUnlockTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceDefaultConstants(instructions);

        // Need to fix when in map editor
        private static IEnumerable<CodeInstruction> ClampPointTranspiler(IEnumerable<CodeInstruction> instructions) {
            foreach (var code in ReplaceDefaultConstants(instructions)) {
                if (code.LoadsConstant(-(EGameAreaManager.DEFAULTRESOLUTION * ((float)EGameAreaManager.DEFAULTGRIDSIZE / 2)))) {
                    code.operand = -(EGameAreaManager.DEFAULTRESOLUTION * ((float)EGameAreaManager.CUSTOMGRIDSIZE / 2));
                    yield return code;
                } else if (code.LoadsConstant((EGameAreaManager.DEFAULTRESOLUTION * ((float)EGameAreaManager.DEFAULTGRIDSIZE / 2)))) {
                    code.operand = EGameAreaManager.DEFAULTRESOLUTION * ((float)EGameAreaManager.CUSTOMGRIDSIZE / 2);
                    yield return code;
                } else {
                    yield return code;
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> GetAreaTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceDefaultConstants(instructions);

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> GetAreaBoundsTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceDefaultConstants(instructions);

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> GetAreaIndexTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceDefaultConstants(instructions);

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> GetAreaPositionSmoothTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceDefaultConstants(instructions);

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> GetFreeBoundsTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceDefaultConstants(instructions);

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> GetTileXZTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceDefaultConstants(instructions);

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> IsUnlockedTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceDefaultConstants(instructions);

        // This one also need special attention for Map Editor
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> PointOutOfAreaTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceDefaultConstants(instructions);

        // This one also need special attention for Map Editor
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> QuadOutOfAreaTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceDefaultConstants(instructions);

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> UpdateAreaMappingTranspiler(IEnumerable<CodeInstruction> instructions) {
            foreach (var code in instructions) {
                if (code.LoadsConstant(0.1f)) {
                    code.operand = 0.02f;
                    yield return code;
                } else if (code.LoadsConstant(200f)) {
                    code.operand = 50f;
                    yield return code;
                } else if (code.LoadsConstant(60f)) {
                    code.operand = 80f;
                    yield return code;
                } else {
                    yield return code;
                }
            }
        }

        internal static bool UpdateAreaTexturePrefix(GameAreaManager __instance, ref bool ___m_areasUpdated, int ___m_highlightAreaIndex, Texture2D ___m_areaTex) {
            ___m_areasUpdated = false;
            int num = 0;
            ItemClass.Availability mode = Singleton<ToolManager>.instance.m_properties.m_mode;
            if ((mode & ItemClass.Availability.MapEditor) == ItemClass.Availability.None) {
                Color color;
                color.a = 1f;
                for (int i = 0; i <= EGameAreaManager.CUSTOMAREATEXSIZE; i++) {
                    for (int j = 0; j <= EGameAreaManager.CUSTOMAREATEXSIZE; j++) {
                        int x = j - num;
                        int z = i - num;
                        bool isUnlocked = __instance.IsUnlocked(x, z);
                        bool canUnlock = __instance.CanUnlock(x, z);
                        color.r = !isUnlocked ? 0.0f : 1f;
                        color.g = !canUnlock ? 0.0f : 1f;
                        color.b = ___m_highlightAreaIndex != z * EGameAreaManager.CUSTOMGRIDSIZE + x ? 0.0f : (!canUnlock ? (!isUnlocked ? 0.0f : 0.5f) : 0.5f);
                        ___m_areaTex.SetPixel(j, i, color);
                    }
                }
            }
            ___m_areaTex.Apply(false);
            return false;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> UpdateDataTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceDefaultConstants(instructions);

        // Special attention required!
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> UnlockAreaTranspiler(IEnumerable<CodeInstruction> instructions) {
            foreach (var code in instructions) {
                if (code.LoadsConstant(EGameAreaManager.DEFAULTGRIDSIZE)) {
                    code.operand = EGameAreaManager.CUSTOMGRIDSIZE;
                    yield return code;
                } else if (code.LoadsConstant((float)EGameAreaManager.DEFAULTGRIDSIZE / 2)) {
                    code.operand = (float)EGameAreaManager.CUSTOMGRIDSIZE / 2;
                    yield return code;
                } else {
                    yield return code;
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void SerializeCoroutine(GameAreaManager instance, DataSerializer s, int startTile) {
            int[] tempGrid = new int[EGameAreaManager.DEFAULTAREACOUNT];
            int areaCount = 0;
            for (var i = 0; i < EGameAreaManager.DEFAULTGRIDSIZE; i++) {
                for (var j = 0; j < EGameAreaManager.DEFAULTGRIDSIZE; j++) {
                    var grid = GameAreaManager.instance.m_areaGrid[(j + 2) * EGameAreaManager.CUSTOMGRIDSIZE + (i + 2)];
                    tempGrid[j * EGameAreaManager.DEFAULTGRIDSIZE + i] = grid;
                    if (grid != 0) {
                        areaCount++;
                    }
                }
            }
            s.WriteUInt8((uint)areaCount);
            s.WriteUInt8((uint)startTile);
            EncodedArray.Byte @byte = EncodedArray.Byte.BeginWrite(s);
            for (int index = 0; index < tempGrid.Length; ++index)
                @byte.Write((byte)tempGrid[index]);
            @byte.EndWrite();
        }

        private static IEnumerable<CodeInstruction> SerializeTranspiler(IEnumerable<CodeInstruction> instructions) {
            bool sigFound = false;
            MethodInfo byteEndWrite = AccessTools.Method(typeof(EncodedArray.Byte), nameof(EncodedArray.Byte.EndWrite));
            using (var codes = instructions.GetEnumerator()) {
                while (codes.MoveNext()) {
                    var cur = codes.Current;
                    if (!sigFound && cur.opcode == OpCodes.Ldloc_0) {
                        sigFound = true;
                        while (codes.MoveNext()) {
                            if (codes.Current.opcode == OpCodes.Callvirt && codes.Current.operand == byteEndWrite) break;
                        }
                        yield return cur;
                        yield return new CodeInstruction(OpCodes.Ldarg_1);
                        yield return new CodeInstruction(OpCodes.Ldloc_0);
                        yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(GameAreaManager), "m_startTile"));
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EGameAreaManagerPatch), nameof(SerializeCoroutine)));
                    } else {
                        yield return cur;
                    }
                }
            }
        }

        private static IEnumerable<CodeInstruction> DeserializeTranspiler(IEnumerable<CodeInstruction> instructions) {
            FieldInfo cameraMode = AccessTools.Field(typeof(CameraController.SavedCameraView), nameof(CameraController.SavedCameraView.m_mode));
            bool firstSigFound = false;
            using (var codes = instructions.GetEnumerator()) {
                while (codes.MoveNext()) {
                    var cur = codes.Current;
                    if (!firstSigFound && cur.opcode == OpCodes.Ldloc_0) {
                        firstSigFound = true;
                        while (codes.MoveNext()) {
                            if (codes.Current.opcode == OpCodes.Stloc_2) break;
                        }
                        yield return new CodeInstruction(OpCodes.Ldc_I4, EGameAreaManager.DEFAULTAREACOUNT);
                        yield return codes.Current;
                    } else if (cur.opcode == OpCodes.Stfld && cur.operand == cameraMode && codes.MoveNext()) {
                        yield return cur;
                        yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EGameAreaManager), nameof(EGameAreaManager.IntegratedDeserialize))).WithLabels(codes.Current.labels);
                        yield return new CodeInstruction(codes.Current.opcode, codes.Current.operand);
                    } else {
                        yield return cur;
                    }
                }
            }
        }


        internal void Enable(Harmony harmony) {
            try {
                harmony.Patch(AccessTools.PropertyGetter(typeof(GameAreaManager), nameof(GameAreaManager.MaxAreaCount)),
                    transpiler: new HarmonyMethod(typeof(EGameAreaManagerPatch), nameof(GetMaxAreaCountTranspiler)));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch GameAreaManager::get_MaxAreaCount");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.PropertyGetter(typeof(GameAreaManager), nameof(GameAreaManager.MaxAreaCount)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(GameAreaManager), "Awake"),
                    transpiler: new HarmonyMethod(typeof(EGameAreaManagerPatch), nameof(AwakeTranspiler)));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch GameAreaManager::Awake");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(GameAreaManager), "Awake"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(GameAreaManager), "BeginOverlayImpl"),
                    prefix: new HarmonyMethod(typeof(EGameAreaManagerPatch), nameof(BeginOverlayImplPrefix)));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch GameAreaManager::BeginOverlayImpl");
                EUtils.ELog(e.Message);
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(GameAreaManager), nameof(GameAreaManager.CalculateTilePrice), new Type[] { typeof(int) }),
                    transpiler: new HarmonyMethod(typeof(EGameAreaManagerPatch), nameof(CalculateTilePriceTranspiler)));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch GameAreaManager::CalculateTilePrice");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(GameAreaManager), nameof(GameAreaManager.CalculateTilePrice), new Type[] { typeof(int) }),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(GameAreaManager), nameof(GameAreaManager.CanUnlock)),
                    transpiler: new HarmonyMethod(typeof(EGameAreaManagerPatch), nameof(CanUnlockTranspiler)));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch GameAreaManager::CanUnlock");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(GameAreaManager), nameof(GameAreaManager.CanUnlock)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(GameAreaManager), nameof(GameAreaManager.ClampPoint)),
                    transpiler: new HarmonyMethod(typeof(EGameAreaManagerPatch), nameof(ClampPointTranspiler)));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch GameAreaManager::ClampPoint");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(GameAreaManager), nameof(GameAreaManager.ClampPoint)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(GameAreaManager), nameof(GameAreaManager.GetArea)),
                    transpiler: new HarmonyMethod(typeof(EGameAreaManagerPatch), nameof(GetAreaTranspiler)));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch GameAreaManager::GetArea");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(GameAreaManager), nameof(GameAreaManager.GetArea)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(GameAreaManager), nameof(GameAreaManager.GetAreaBounds)),
                    transpiler: new HarmonyMethod(typeof(EGameAreaManagerPatch), nameof(GetAreaBoundsTranspiler)));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch GameAreaManager::GetAreaBounds");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(GameAreaManager), nameof(GameAreaManager.GetAreaBounds)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(GameAreaManager), nameof(GameAreaManager.GetAreaIndex)),
                    transpiler: new HarmonyMethod(typeof(EGameAreaManagerPatch), nameof(GetAreaIndexTranspiler)));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch GameAreaManager::GetAreaIndex");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(GameAreaManager), nameof(GameAreaManager.GetAreaIndex)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(GameAreaManager), nameof(GameAreaManager.GetAreaPositionSmooth)),
                    transpiler: new HarmonyMethod(typeof(EGameAreaManagerPatch), nameof(GetAreaPositionSmoothTranspiler)));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch GameAreaManager::GetAreaPositionSmooth");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(GameAreaManager), nameof(GameAreaManager.GetAreaPositionSmooth)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(GameAreaManager), @"GetFreeBounds"),
                    transpiler: new HarmonyMethod(typeof(EGameAreaManagerPatch), nameof(GetFreeBoundsTranspiler)));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch GameAreaManager::GetFreeBounds");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(GameAreaManager), @"GetFreeBounds"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(GameAreaManager), nameof(GameAreaManager.GetTileXZ),
                    new Type[] { typeof(Vector3), typeof(int).MakeByRefType(), typeof(int).MakeByRefType() }),
                    transpiler: new HarmonyMethod(typeof(EGameAreaManagerPatch), nameof(GetTileXZTranspiler)));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch GameAreaManager::GetTileXZ");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(GameAreaManager), nameof(GameAreaManager.GetTileXZ),
                    new Type[] { typeof(Vector3), typeof(int).MakeByRefType(), typeof(int).MakeByRefType() }),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(GameAreaManager), nameof(GameAreaManager.IsUnlocked)),
                    transpiler: new HarmonyMethod(typeof(EGameAreaManagerPatch), nameof(IsUnlockedTranspiler)));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch GameAreaManager::IsUnlocked");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(GameAreaManager), nameof(GameAreaManager.IsUnlocked)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(GameAreaManager), nameof(GameAreaManager.PointOutOfArea), new Type[] { typeof(Vector3) }),
                    transpiler: new HarmonyMethod(typeof(EGameAreaManagerPatch), nameof(PointOutOfAreaTranspiler)));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch GameAreaManager::PointOutOfArea(Vector3)");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(GameAreaManager), nameof(GameAreaManager.PointOutOfArea), new Type[] { typeof(Vector3) }),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(GameAreaManager), nameof(GameAreaManager.PointOutOfArea), new Type[] { typeof(Vector3), typeof(float) }),
                    transpiler: new HarmonyMethod(typeof(EGameAreaManagerPatch), nameof(PointOutOfAreaTranspiler)));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch GameAreaManager::PointOutOfArea(Vector3, float)");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(GameAreaManager), nameof(GameAreaManager.PointOutOfArea), new Type[] { typeof(Vector3), typeof(float) }),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(GameAreaManager), nameof(GameAreaManager.QuadOutOfArea)),
                    transpiler: new HarmonyMethod(typeof(EGameAreaManagerPatch), nameof(QuadOutOfAreaTranspiler)));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch GameAreaManager::QuadOutOfArea");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(GameAreaManager), nameof(GameAreaManager.QuadOutOfArea)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(GameAreaManager), @"UpdateAreaMapping"),
                    transpiler: new HarmonyMethod(typeof(EGameAreaManagerPatch), nameof(UpdateAreaMappingTranspiler)));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch GameAreaManager::UpdateAreaMapping");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(GameAreaManager), @"UpdateAreaMapping"),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(GameAreaManager), @"UpdateAreaTexture"),
                    prefix: new HarmonyMethod(typeof(EGameAreaManagerPatch), nameof(UpdateAreaTexturePrefix)));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch GameAreaManager::UpdateAreaTexture");
                EUtils.ELog(e.Message);
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(GameAreaManager), nameof(GameAreaManager.UpdateData)),
                    transpiler: new HarmonyMethod(typeof(EGameAreaManagerPatch), nameof(UpdateDataTranspiler)));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch GameAreaManager::UpdateData");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(GameAreaManager), nameof(GameAreaManager.UpdateData)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(GameAreaManager), nameof(GameAreaManager.UnlockArea)),
                    transpiler: new HarmonyMethod(typeof(EGameAreaManagerPatch), nameof(UnlockAreaTranspiler)));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch GameAreaManager::UnlockArea");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(GameAreaManager), nameof(GameAreaManager.UnlockArea)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(GameAreaManager.Data), nameof(GameAreaManager.Data.Serialize)),
                    transpiler: new HarmonyMethod(typeof(EGameAreaManagerPatch), nameof(SerializeTranspiler)));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch GameAreaManager::Data::Serialize");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(GameAreaManager.Data), nameof(GameAreaManager.Data.Serialize)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(GameAreaManager.Data), nameof(GameAreaManager.Data.Deserialize)),
                    transpiler: new HarmonyMethod(typeof(EGameAreaManagerPatch), nameof(DeserializeTranspiler)));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch GameAreaManager::Data::Deserialize");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(GameAreaManager.Data), nameof(GameAreaManager.Data.Deserialize)),
                    transpiler: new HarmonyMethod(AccessTools.Method(typeof(EUtils), nameof(EUtils.DebugPatchOutput))));
                throw;
            }
        }

        internal void Disable(Harmony harmony) {
            harmony.Unpatch(AccessTools.PropertyGetter(typeof(GameAreaManager), nameof(GameAreaManager.MaxAreaCount)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(GameAreaManager), "Awake"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(GameAreaManager), "BeginOverlayImpl"), HarmonyPatchType.Prefix, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(GameAreaManager), nameof(GameAreaManager.CalculateTilePrice), new Type[] { typeof(int) }), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(GameAreaManager), nameof(GameAreaManager.CanUnlock)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(GameAreaManager), nameof(GameAreaManager.ClampPoint)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(GameAreaManager), nameof(GameAreaManager.GetArea)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(GameAreaManager), nameof(GameAreaManager.GetAreaBounds)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(GameAreaManager), nameof(GameAreaManager.GetAreaIndex)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(GameAreaManager), nameof(GameAreaManager.GetAreaPositionSmooth)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(GameAreaManager), "GetFreeBounds"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(GameAreaManager), nameof(GameAreaManager.GetTileXZ),
                new Type[] { typeof(Vector3), typeof(int).MakeByRefType(), typeof(int).MakeByRefType() }), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(GameAreaManager), nameof(GameAreaManager.IsUnlocked)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(GameAreaManager), nameof(GameAreaManager.PointOutOfArea), new Type[] { typeof(Vector3) }),
                HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(GameAreaManager), nameof(GameAreaManager.PointOutOfArea), new Type[] { typeof(Vector3), typeof(float) }),
                HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(GameAreaManager), nameof(GameAreaManager.QuadOutOfArea)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(GameAreaManager), @"UpdateAreaMapping"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(GameAreaManager), @"UpdateAreaTexture"), HarmonyPatchType.Prefix, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(GameAreaManager), nameof(GameAreaManager.UpdateData)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(GameAreaManager), nameof(GameAreaManager.UnlockArea)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(GameAreaManager.Data), nameof(GameAreaManager.Data.Serialize)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(GameAreaManager.Data), nameof(GameAreaManager.Data.Deserialize)), HarmonyPatchType.Transpiler, EModule.HARMONYID);
        }
    }
}
