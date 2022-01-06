using CitiesHarmony.API;
using ColossalFramework;
using ColossalFramework.UI;
using ICities;
using System;
#if EMERGENCYRESCUE
using System.IO;
#endif

namespace EManagersLib {
    public sealed class EModule : IUserMod, ILoadingExtension {
        internal const string m_modVersion = "1.0.3";
        internal const string m_assemblyVersion = m_modVersion + ".*";
        internal const string m_modName = "Extended Managers";
        internal const string m_modDesc = "A library that extends the existing framework in Cities Skylines";
        internal const string HARMONYID = "quistar.EManagersLib.mod";
        internal const string m_settingsFile = "EManagersLibKeyBind";
#if EMERGENCYRESCUE
        private const string ExportedData = "EML_ExportedData.dat";
#endif

        public string Name => m_modName + ' ' + m_modVersion;
        public string Description => m_modDesc;

        public EModule() {
            try {
                EUtils.CreateDebugFile();
            } catch (Exception e) {
                UnityEngine.Debug.LogException(e);
            }
        }

        public void OnEnabled() {
            try {
                if (GameSettings.FindSettingsFileByName(m_settingsFile) is null) {
                    GameSettings.AddSettingsFile(new SettingsFile[] {
                        new SettingsFile() { fileName = m_settingsFile }
                    });
                }
            } catch (Exception e) {
                UnityEngine.Debug.LogException(e);
            }
            EUtils.CheckIncompatibleMods();
            HarmonyHelper.DoOnHarmonyReady(() => EUtils.EnablePropPatches());
        }

        public void OnDisabled() {
            if (HarmonyHelper.IsHarmonyInstalled) {
                EUtils.DisablePropPatches();
            }
        }

        public void OnSettingsUI(UIHelperBase helper) {
            UIHelper helperPanel = helper.AddGroup(m_modName + " " + m_modVersion) as UIHelper;
            UIPanel root = helperPanel.self as UIPanel;
#if EMERGENCYRESCUE
            UIButton exportData = helperPanel.AddButton(@"Import all prop position", () => {
                EPropInstance[] props = EPropManager.m_props.m_buffer;
                try {
                    using (BinaryReader reader = new BinaryReader(File.Open(ExportedData, FileMode.Open))) {
                        for (int i = 1; i < props.Length; i++) {
                            props[i].m_flags = reader.ReadUInt16();
                        }
                        for (int i = 1; i < props.Length; i++) {
                            if (props[i].m_flags != 0) {
                                props[i].m_color.r = reader.ReadSingle();
                                props[i].m_color.g = reader.ReadSingle();
                                props[i].m_color.b = reader.ReadSingle();
                                props[i].m_color.a = reader.ReadSingle();
                                props[i].m_infoIndex = reader.ReadUInt16();
                                props[i].m_scale = reader.ReadSingle();
                                props[i].m_nextGridProp = reader.ReadUInt32();
                                props[i].m_angle = reader.ReadUInt16();
                                props[i].m_posX = reader.ReadInt16();
                                props[i].m_posY = reader.ReadUInt16();
                                props[i].m_posZ = reader.ReadInt16();
                                props[i].m_preciseX = reader.ReadSingle();
                                props[i].m_preciseZ = reader.ReadSingle();
                            }
                        }
                        for(int i = 1; i < props.Length; i++) {
                            Singleton<PropManager>.instance.UpdatePropRenderer((uint)i, true);
                        }
                    }
                } catch(Exception e) {
                    UnityEngine.Debug.LogException(e);
                    throw;
                }
            }) as UIButton;
            helperPanel.AddSpace(20);
#endif
            UILabel msg = root.AddUIComponent<UILabel>();
            msg.width = root.width;
            msg.autoHeight = true;
            msg.wordWrap = true;
            msg.text = "Extended Managers Library is a mod to extend the default Cities Skylines Framework.\n" +
                       "Currently, only prop limit can be extended beyond 65k\n\n" +
                       "This mod also includes a statistic panel, which you can bring out by pressing the following hotkey. This statistics panel is updated every 5 seconds\n";
            root.gameObject.AddComponent<EKeyBinding>();
        }

        public void OnCreated(ILoading loading) {
            EUtils.EnableModPatches();
        }

        public void OnReleased() { }

        public void OnLevelLoaded(LoadMode mode) {
            EUtils.LateEnablePatches();
            EStatsPanel.Initialize();
            EGameAreaManager.OnLevelLoaded();
            EDistrictManager.OnLevelLoaded();
        }

        public void OnLevelUnloading() {
            EUtils.LateDisablePatches();
        }
    }
}
