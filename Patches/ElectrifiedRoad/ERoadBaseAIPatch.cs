using ColossalFramework;
using HarmonyLib;
using System;
using UnityEngine;

namespace EManagersLib.Patches.ElectrifiedRoad {
    internal readonly struct ERoadBaseAIPatch {
        private static void GetColorSegment(ref Color __result, ref NetSegment data, InfoManager.InfoMode infoMode) {
            if (infoMode == InfoManager.InfoMode.Electricity) {
                NetNode.Flags flags = Singleton<NetManager>.instance.m_nodes.m_buffer[data.m_startNode].m_flags;
                NetNode.Flags flags2 = Singleton<NetManager>.instance.m_nodes.m_buffer[data.m_endNode].m_flags;
                if ((flags & flags2 & NetNode.Flags.Electricity) != NetNode.Flags.None) {
                    __result = Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_activeColor;
                    return;
                }
                Color inactiveColor = Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_inactiveColor;
                inactiveColor.a = 0f;
                __result = inactiveColor;
            }
        }

        private static void GetColorNode(ref Color __result, ref NetNode data, InfoManager.InfoMode infoMode) {
            if (infoMode == InfoManager.InfoMode.Electricity) {
                if ((data.m_flags & NetNode.Flags.Electricity) != NetNode.Flags.None) {
                    __result = Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_activeColor;
                    return;
                }
                Color inactiveColor = Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_inactiveColor;
                inactiveColor.a = 0f;
                __result = inactiveColor;
            }
        }

        internal void Enable(Harmony harmony) {
            try {
                harmony.Patch(AccessTools.Method(typeof(RoadBaseAI), nameof(RoadBaseAI.GetColor),
                    new Type[] { typeof(ushort), typeof(NetSegment).MakeByRefType(), typeof(InfoManager.InfoMode) }),
                    postfix: new HarmonyMethod(AccessTools.Method(typeof(ERoadBaseAIPatch), nameof(GetColorSegment))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch PlayerNetAI::GetColor(ushort, NetSegment, InfoManager.InfoMode)");
                EUtils.ELog(e.Message);
                throw;
            }
            try {
                harmony.Patch(AccessTools.Method(typeof(RoadBaseAI), nameof(RoadBaseAI.GetColor),
                    new Type[] { typeof(ushort), typeof(NetNode).MakeByRefType(), typeof(InfoManager.InfoMode) }),
                    postfix: new HarmonyMethod(AccessTools.Method(typeof(ERoadBaseAIPatch), nameof(GetColorNode))));
            } catch (Exception e) {
                EUtils.ELog("Failed to patch PlayerNetAI::GetColor(ushort, NetNode, InfoManager.InfoMode)");
                EUtils.ELog(e.Message);
                throw;
            }
        }

        internal void Disable(Harmony harmony) {
            harmony.Unpatch(AccessTools.Method(typeof(RoadBaseAI), nameof(RoadBaseAI.GetColor),
                new Type[] { typeof(ushort), typeof(NetSegment).MakeByRefType(), typeof(InfoManager.InfoMode) }),
                HarmonyPatchType.Postfix, EModule.HARMONYID);
            harmony.Unpatch(AccessTools.Method(typeof(RoadBaseAI), nameof(RoadBaseAI.GetColor),
                new Type[] { typeof(ushort), typeof(NetNode).MakeByRefType(), typeof(InfoManager.InfoMode) }),
                HarmonyPatchType.Postfix, EModule.HARMONYID);
        }
    }
}
