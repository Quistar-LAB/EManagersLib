using ColossalFramework;
using ColossalFramework.IO;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using UnityEngine;
using static EManagersLib.EGameAreaManager;

namespace EManagersLib.Patches {
    internal class EGameAreaManagerPatch {
        internal static IEnumerable<CodeInstruction> ReplaceDefaultConstants(IEnumerable<CodeInstruction> instructions) {
            foreach (var code in instructions) {
                if (code.LoadsConstant(MINAREACOUNT)) {
                    code.operand = CUSTOMAREACOUNT;
                    yield return code;
                } else if (code.LoadsConstant(DEFAULTGRIDSIZE)) {
                    yield return new CodeInstruction(OpCodes.Ldc_I4_S, CUSTOMGRIDSIZE);
                } else if (code.LoadsConstant(DEFAULTGRIDSIZE / 2f)) {
                    code.operand = CUSTOMGRIDSIZE / 2f;
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

        internal static IEnumerable<CodeInstruction> ReplaceGetStartTile(IEnumerable<CodeInstruction> instructions) {
            MethodInfo getStartTile = AccessTools.Method(typeof(GameAreaManager), nameof(GameAreaManager.GetStartTile));
            using (var codes = instructions.GetEnumerator()) {
                while (codes.MoveNext()) {
                    var cur = codes.Current;
                    if (cur.opcode == OpCodes.Ldarg_0 && codes.MoveNext()) {
                        var next = codes.Current;
                        if (next.opcode == OpCodes.Ldloca_S && codes.MoveNext()) {
                            var next1 = codes.Current;
                            if (next1.opcode == OpCodes.Ldloca_S && codes.MoveNext()) {
                                var next2 = codes.Current;
                                if (next2.opcode == OpCodes.Call && next2.operand == getStartTile) {
                                    yield return cur;
                                    yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(GameAreaManager), @"m_startTile"));
                                    yield return next;
                                    yield return next1;
                                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EGameAreaManager), nameof(EGameAreaManager.GetStartTile)));
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

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> GetMaxAreaCountTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceDefaultConstants(instructions);

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> AwakeTranspiler(IEnumerable<CodeInstruction> instructions) {
            const int defTextureSize = 8;
            foreach (var code in instructions) {
                if (code.LoadsConstant(defTextureSize)) {
                    yield return new CodeInstruction(OpCodes.Ldc_I4_S, CUSTOMAREATEXSIZE);
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
                    Color color = Color.white;
                    ToolController toolController = Singleton<ToolManager>.instance.m_properties;
                    if (!(toolController is null)) {
                        ToolBase currentTool = toolController.CurrentTool;
                        if ((currentTool.GetErrors() & ToolBase.ToolErrors.OutOfArea) != ToolBase.ToolErrors.None) {
                            color = Color.red;
                        }
                    }
                    color.a = ___m_borderAlpha;
                    for (int z = 0; z <= CUSTOMGRIDSIZE; z++) {
                        for (int x = 0; x <= CUSTOMGRIDSIZE; x++) {
                            bool flag8 = __instance.GetArea(x, z) > 0;
                            bool flag9 = __instance.GetArea(x, z - 1) > 0;
                            bool flag10 = __instance.GetArea(x - 1, z) > 0;
                            if (flag8 != flag9) {
                                Vector3 vector = new Vector3((x - (CUSTOMGRIDSIZE / 2f) + 0.5f) * 1920f, 0f, (z - (CUSTOMGRIDSIZE / 2f)) * 1920f);
                                Vector3 size = new Vector3(1920f, 1024f, 100f);
                                Bounds bounds = new Bounds(vector + new Vector3(0f, size.y * 0.5f, 0f), size);
                                if (cameraInfo.Intersect(bounds)) {
                                    SetWaterMaterialProperties(vector, ___m_borderMaterial);
                                    ___m_borderMaterial.SetColor(___ID_Color, color);
                                    if (___m_borderMaterial.SetPass(0)) {
                                        __instance.m_drawCallData.m_overlayCalls++;
                                        Graphics.DrawMeshNow(___m_borderMesh, vector, rotation);
                                    }
                                }
                            }
                            if (flag8 != flag10) {
                                Vector3 vector = new Vector3((x - (CUSTOMGRIDSIZE / 2f)) * 1920f, 0f, (z - (CUSTOMGRIDSIZE / 2f) + 0.5f) * 1920f);
                                Vector3 size6 = new Vector3(100f, 1024f, 1920f);
                                Bounds bounds7 = new Bounds(vector + new Vector3(0f, size6.y * 0.5f, 0f), size6);
                                if (cameraInfo.Intersect(bounds7)) {
                                    SetWaterMaterialProperties(vector, ___m_borderMaterial);
                                    ___m_borderMaterial.SetColor(___ID_Color, color);
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
                    vector.z = 1f / (1920f * CUSTOMAREATEXSIZE);
                    vector.x = (CUSTOMGRIDSIZE) / (CUSTOMAREATEXSIZE * 2f);
                    vector.y = (CUSTOMGRIDSIZE) / (CUSTOMAREATEXSIZE * 2f);
                    vector.w = 1f / CUSTOMAREATEXSIZE;
                    ___m_areaMaterial.mainTexture = ___m_areaTex;
                    ___m_areaMaterial.SetColor(___ID_Color, new Color(1f, 1f, 1f, ___m_areaAlpha));
                    ___m_areaMaterial.SetVector(___ID_AreaMapping, vector);
                    Vector3 zero = Vector3.zero;
                    Vector3 zero2 = Vector3.zero;
                    for (int i = 0; i < CUSTOMGRIDSIZE; i++) {
                        for (int j = 0; j < CUSTOMGRIDSIZE; j++) {
                            if (__instance.IsUnlocked(j, i)) {
                                zero.x = EMath.Min(zero.x, ((j - 1) - (CUSTOMGRIDSIZE / 2f)) * 1920f);
                                zero2.x = EMath.Max(zero2.x, ((j + 2) - (CUSTOMGRIDSIZE / 2f)) * 1920f);
                                zero.z = EMath.Min(zero.z, ((i - 1) - (CUSTOMGRIDSIZE / 2f)) * 1920f);
                                zero2.z = EMath.Max(zero2.z, ((i + 2) - (CUSTOMGRIDSIZE / 2f)) * 1920f);
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
                if (code.LoadsConstant(-(DEFAULTRESOLUTION * (DEFAULTGRIDSIZE / 2f)))) {
                    code.operand = -(DEFAULTRESOLUTION * (CUSTOMGRIDSIZE / 2f));
                    yield return code;
                } else if (code.LoadsConstant((DEFAULTRESOLUTION * (DEFAULTGRIDSIZE / 2f)))) {
                    code.operand = DEFAULTRESOLUTION * (CUSTOMGRIDSIZE / 2f);
                    yield return code;
                } else {
                    yield return code;
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> GetAreaTranspiler(IEnumerable<CodeInstruction> instructions) {
            foreach (var code in ReplaceDefaultConstants(instructions)) {
                if (code.opcode == OpCodes.Ldc_I4_2) {
                    code.opcode = OpCodes.Ldc_I4_0;
                    yield return code;
                } else {
                    yield return code;
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> GetAreaBoundsTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceDefaultConstants(instructions);

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> GetAreaIndexTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceDefaultConstants(instructions);

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> GetAreaPositionSmoothTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceDefaultConstants(instructions);

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> GetFreeBoundsTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceDefaultConstants(instructions);

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> GetTileXZTranspiler(IEnumerable<CodeInstruction> instructions) {
            foreach (var code in ReplaceDefaultConstants(instructions)) {
                if (code.opcode == OpCodes.Ldc_I4_4) {
                    yield return new CodeInstruction(OpCodes.Ldc_I4_S, CUSTOMGRIDSIZE - 1);
                } else {
                    yield return code;
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> IsUnlockedTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceDefaultConstants(instructions);

        private static IEnumerable<CodeInstruction> OnLevelLoadedTranspiler(IEnumerable<CodeInstruction> instructions) {
            foreach (var code in ReplaceGetStartTile(instructions)) {
                if (code.opcode == OpCodes.Ldc_I4_2) {
                    yield return new CodeInstruction(OpCodes.Ldc_I4_4);
                } else {
                    yield return code;
                }
            }
        }

        // This one also need special attention for Map Editor
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> PointOutOfAreaTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceDefaultConstants(instructions);

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> SetStartTileTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceDefaultConstants(instructions);

        // This one also need special attention for Map Editor
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> QuadOutOfAreaTranspiler(IEnumerable<CodeInstruction> instructions) => ReplaceDefaultConstants(instructions);

        private static void AdjustCamera(CameraController cameraController, Vector3 pos, Vector3 center, Bounds freeBounds) {
            if (EMath.Abs(pos.x) >= EMath.Abs(pos.z)) {
                if (pos.x > 0f) {
                    center.z -= freeBounds.size.z * 0.01f + 30f;
                } else {
                    center.z += freeBounds.size.z * 0.01f + 30f;
                }
                cameraController.SetOverrideModeOn(center, new Vector2((pos.x <= 0f) ? 180f : 0, 80f), freeBounds.size.z * 1.25f);
            } else {
                if (pos.z > 0f) {
                    center.x += freeBounds.size.x * 0.01f + 30f;
                } else {
                    center.x -= freeBounds.size.x * 0.01f + 30f;
                }
                cameraController.SetOverrideModeOn(center, new Vector2((pos.z <= 0f) ? 90f : -90f, 80f), freeBounds.size.x * 1.25f);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> UpdateAreaMappingTranspiler(IEnumerable<CodeInstruction> instructions) {
            int sigCounter = 0;
            MethodInfo getCenter = AccessTools.PropertyGetter(typeof(Bounds), nameof(Bounds.center));
            MethodInfo setOverrideModeOn = AccessTools.Method(typeof(CameraController), nameof(CameraController.SetOverrideModeOn));
            using (var codes = instructions.GetEnumerator()) {
                while (codes.MoveNext()) {
                    var cur = codes.Current;
                    if (cur.opcode == OpCodes.Ldloca_S && cur.operand is LocalBuilder local && local.LocalIndex == 1 && codes.MoveNext()) {
                        var next = codes.Current;
                        if (next.opcode == OpCodes.Call && next.operand == getCenter) {
                            yield return new CodeInstruction(OpCodes.Ldarg_0);
                            yield return new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(GameAreaManager), "m_cameraController"));
                            yield return new CodeInstruction(OpCodes.Ldloc_0);
                            yield return cur;
                            yield return next;
                            yield return new CodeInstruction(OpCodes.Ldloc_1);
                            yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EGameAreaManagerPatch), nameof(AdjustCamera)));
                            while (codes.MoveNext()) {
                                cur = codes.Current;
                                if (cur.opcode == OpCodes.Callvirt && cur.operand == setOverrideModeOn && ++sigCounter == 2) break;
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

        internal static bool UpdateAreaTexturePrefix(GameAreaManager __instance, ref bool ___m_areasUpdated, int ___m_highlightAreaIndex, Texture2D ___m_areaTex) {
            ___m_areasUpdated = false;
            int num = 0;
            ItemClass.Availability mode = Singleton<ToolManager>.instance.m_properties.m_mode;
            if ((mode & ItemClass.Availability.MapEditor) == ItemClass.Availability.None) {
                Color color;
                color.a = 1f;
                for (int i = 0; i <= CUSTOMAREATEXSIZE; i++) {
                    for (int j = 0; j <= CUSTOMAREATEXSIZE; j++) {
                        int x = j - num;
                        int z = i - num;
                        bool isUnlocked = __instance.IsUnlocked(x, z);
                        bool canUnlock = __instance.CanUnlock(x, z);
                        color.r = !isUnlocked ? 0.0f : 1f;
                        color.g = !canUnlock ? 0.0f : 1f;
                        color.b = ___m_highlightAreaIndex != z * CUSTOMGRIDSIZE + x ? 0.0f : (!canUnlock ? (!isUnlocked ? 0.0f : 0.5f) : 0.5f);
                        ___m_areaTex.SetPixel(j, i, color);
                    }
                }
            }
            ___m_areaTex.Apply(false);
            return false;
        }

        private static float CalculateBuildableArea(int tileX, int tileZ) {
            Singleton<NaturalResourceManager>.instance.GetTileResources(tileX, tileZ, out uint num, out uint num2, out uint num3, out uint num4, out uint num5);
            float tileFlatness = Singleton<TerrainManager>.instance.GetTileFlatness(tileX, tileZ);
            float num6 = 3686400f;
            float num7 = 1139.0625f;
            float num8 = num6 / num7 * 255f;
            return tileFlatness * (1f - num5 / num8);
        }

        // This has to be done with a prefix, don't know why 
        private static bool UpdateDataPrefix(GameAreaManager __instance, ref int ___m_startTile,
            ref float ___m_buildableArea0, ref float ___m_buildableArea1, ref float ___m_buildableArea2, ref float ___m_buildableArea3, SimulationManager.UpdateMode mode) {
            int i, j;
            Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.BeginLoading("GameAreaManager.UpdateData");
            int[] areaGrid = __instance.m_areaGrid;
            switch (mode) {
            case SimulationManager.UpdateMode.NewGameFromMap:
            case SimulationManager.UpdateMode.NewScenarioFromMap:
            case SimulationManager.UpdateMode.UpdateScenarioFromMap:
            case SimulationManager.UpdateMode.LoadAsset:
                __instance.m_areaCount = 0;
                RecalcStartTile(ref ___m_startTile);
                for (i = 0; i < CUSTOMGRIDSIZE; i++) {
                    for (j = 0; j < CUSTOMGRIDSIZE; j++) {
                        int grid = i * CUSTOMGRIDSIZE + j;
                        if (grid == ___m_startTile) {
                            areaGrid[grid] = ++__instance.m_areaCount;
                        } else {
                            areaGrid[grid] = 0;
                        }
                    }
                }
                break;
            }
            GetStartTile(___m_startTile, out int x, out int z);
            if (mode != SimulationManager.UpdateMode.LoadGame || ___m_buildableArea0 < 0f) {
                ___m_buildableArea0 = 0f;
                ___m_buildableArea1 = 0f;
                ___m_buildableArea2 = 0f;
                ___m_buildableArea3 = 0f;
                float buildableArea0Count = 0f;
                float buildableArea1Count = 0f;
                float buildableArea2Count = 0f;
                float buildableArea3Count = 0f;
                for (i = 0; i < CUSTOMGRIDSIZE; i++) {
                    for (j = 0; j < CUSTOMGRIDSIZE; j++) {
                        switch (EMath.Abs(j - x) + EMath.Abs(i - z)) {
                        case 0:
                            ___m_buildableArea0 += CalculateBuildableArea(j, i);
                            buildableArea0Count += 1f;
                            break;
                        case 1:
                            ___m_buildableArea1 += CalculateBuildableArea(j, i);
                            buildableArea1Count += 1f;
                            break;
                        case 2:
                            ___m_buildableArea2 += CalculateBuildableArea(j, i);
                            buildableArea2Count += 1f;
                            break;
                        case 3:
                            ___m_buildableArea3 += CalculateBuildableArea(j, i);
                            buildableArea3Count += 1f;
                            break;
                        }
                    }
                }
                if (buildableArea0Count != 0f) {
                    ___m_buildableArea0 /= buildableArea0Count;
                }
                if (buildableArea1Count != 0f) {
                    ___m_buildableArea1 /= buildableArea1Count;
                }
                if (buildableArea2Count != 0f) {
                    ___m_buildableArea2 /= buildableArea2Count;
                }
                if (buildableArea3Count != 0f) {
                    ___m_buildableArea3 /= buildableArea3Count;
                }
            }
            for (i = 0; i < CUSTOMGRIDSIZE; i++) {
                for (j = 0; j < CUSTOMGRIDSIZE; j++) {
                    if (__instance.GetArea(i, j) > 0) {
                        Singleton<TerrainManager>.instance.SetDetailedPatch(i, j);
                    }
                }
            }
            if (mode == SimulationManager.UpdateMode.NewGameFromMap || mode == SimulationManager.UpdateMode.NewScenarioFromMap ||
                mode == SimulationManager.UpdateMode.UpdateScenarioFromMap || __instance.m_areaNotUnlocked is null) {
                __instance.m_areaNotUnlocked = new GenericGuide();
            }
            Singleton<LoadingManager>.instance.m_loadingProfilerSimulation.EndLoading();
            return false;
        }

        private static IEnumerable<CodeInstruction> UpdateDataTranspiler(IEnumerable<CodeInstruction> instructions) {
            var codes = __UpdateDataTranspiler(instructions);
            foreach (var code in codes) {
                EUtils.ELog(code.ToString());
            }
            return codes;
        }
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static IEnumerable<CodeInstruction> __UpdateDataTranspiler(IEnumerable<CodeInstruction> instructions) {
            bool sigFound = false;
            FieldInfo startTile = AccessTools.Field(typeof(GameAreaManager), @"m_startTile");
            foreach (var code in instructions) {
                if (code.opcode == OpCodes.Ldc_I4_2) {
                    yield return new CodeInstruction(OpCodes.Ldc_I4_0).WithLabels(code.labels);
                } else if (code.opcode == OpCodes.Ldc_I4_5) {
                    yield return new CodeInstruction(OpCodes.Ldc_I4, CUSTOMGRIDSIZE).WithLabels(code.labels);
                } else if (!sigFound && code.opcode == OpCodes.Ldfld && code.operand == startTile) {
                    sigFound = true;
                    yield return new CodeInstruction(OpCodes.Ldflda, startTile);
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(EGameAreaManager), nameof(EGameAreaManager.RecalcStartTile)));
                } else {
                    yield return code;
                }
            }
        }

        private static IEnumerable<CodeInstruction> UnlockAreaTranspiler(IEnumerable<CodeInstruction> instructions) {
            MethodInfo simInstance = AccessTools.PropertyGetter(typeof(Singleton<SimulationManager>), nameof(Singleton<SimulationManager>.instance));
            using (var codes = instructions.GetEnumerator()) {
                while (codes.MoveNext()) {
                    var cur = codes.Current;
                    if (cur.opcode == OpCodes.Ldc_I4_5) {
                        yield return new CodeInstruction(OpCodes.Ldc_I4_S, CUSTOMGRIDSIZE);
                    } else if (cur.opcode == OpCodes.Bne_Un && codes.MoveNext()) {
                        var next = codes.Current;
                        if (next.opcode == OpCodes.Call && next.operand == simInstance) {
                            while (codes.MoveNext()) {
                                if (codes.Current.opcode == OpCodes.Beq) break;
                            }
                            yield return new CodeInstruction(OpCodes.Bne_Un_S, codes.Current.operand);
                        } else {
                            yield return cur;
                            yield return next;
                        }
                    } else if (cur.LoadsConstant(DEFAULTGRIDSIZE / 2f)) {
                        cur.operand = CUSTOMGRIDSIZE / 2f;
                        yield return cur;
                    } else if (cur.opcode == OpCodes.Ldc_I4_2 && codes.MoveNext()) {
                        var next = codes.Current;
                        if (next.opcode != OpCodes.Stloc_S || !(next.operand is LocalBuilder local) || local.LocalIndex != 8) {
                            yield return cur;
                            yield return next;
                        }
                    } else if (cur.opcode == OpCodes.Ldloc_S && cur.operand is LocalBuilder local && local.LocalIndex == 8 && codes.MoveNext()) {
                        var next = codes.Current;
                        if (next.opcode != OpCodes.Add) {
                            yield return cur;
                            yield return next;
                        }
                    } else {
                        yield return cur;
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void SerializeCoroutine(GameAreaManager instance, DataSerializer s, int startTile) {
            int[] tempGrid = new int[DEFAULTAREACOUNT];
            int areaCount = 0;
            for (int i = 0; i < DEFAULTGRIDSIZE; i++) {
                for (int j = 0; j < DEFAULTGRIDSIZE; j++) {
                    int grid = instance.m_areaGrid[(j + 2) * CUSTOMGRIDSIZE + (i + 2)];
                    tempGrid[j * DEFAULTGRIDSIZE + i] = grid;
                    if (grid != 0) {
                        areaCount++;
                    }
                }
            }
            s.WriteUInt8((uint)areaCount);
            s.WriteUInt8((uint)startTile);
            EncodedArray.Byte @byte = EncodedArray.Byte.BeginWrite(s);
            for (int i = 0; i < tempGrid.Length; ++i) {
                @byte.Write((byte)tempGrid[i]);
            }
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
            using (var codes = instructions.GetEnumerator()) {
                while (codes.MoveNext()) {
                    var cur = codes.Current;
                    if (cur.opcode == OpCodes.Ldloc_1 && codes.MoveNext()) {
                        var next = codes.Current;
                        if (next.opcode == OpCodes.Ldlen && codes.MoveNext()) {
                            var next1 = codes.Current;
                            if (next1.opcode == OpCodes.Conv_I4) {
                                yield return new CodeInstruction(OpCodes.Ldc_I4, DEFAULTAREACOUNT);
                            } else {
                                yield return cur;
                                yield return next;
                                yield return next1;
                            }
                        } else {
                            yield return cur;
                            yield return next;
                        }
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
                harmony.Patch(AccessTools.Method(typeof(GameAreaManager), "OnLevelLoaded"),
                    transpiler: new HarmonyMethod(typeof(EGameAreaManagerPatch), nameof(OnLevelLoadedTranspiler)));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch GameAreaManager::OnLevelLoaded");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(GameAreaManager), "OnLevelLoaded"),
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
                harmony.Patch(AccessTools.Method(typeof(GameAreaManager), nameof(GameAreaManager.SetStartTile), new Type[] { typeof(int), typeof(int) }),
                    transpiler: new HarmonyMethod(typeof(EGameAreaManagerPatch), nameof(SetStartTileTranspiler)));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch GameAreaManager::SetStartTile");
                EUtils.ELog(e.Message);
                harmony.Patch(AccessTools.Method(typeof(GameAreaManager), nameof(GameAreaManager.SetStartTile), new Type[] { typeof(int), typeof(int) }),
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
#if false
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
#else
            try {
                harmony.Patch(AccessTools.Method(typeof(GameAreaManager), nameof(GameAreaManager.UpdateData)),
                    prefix: new HarmonyMethod(typeof(EGameAreaManagerPatch), nameof(UpdateDataPrefix)));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch GameAreaManager::UpdateData");
                EUtils.ELog(e.Message);
                throw;
            }
#endif
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
#if FALSE
            harmony.Unpatch(AccessTools.Method(typeof(GameAreaManager), "OnLevelLoaded"), HarmonyPatchType.Transpiler, EModule.HARMONYID);
#else
            harmony.Unpatch(AccessTools.Method(typeof(GameAreaManager), "OnLevelLoaded"), HarmonyPatchType.Prefix, EModule.HARMONYID);
#endif
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
