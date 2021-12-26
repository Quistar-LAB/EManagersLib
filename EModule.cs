using CitiesHarmony.API;
using ColossalFramework;
using ColossalFramework.UI;
using ICities;
using System;

namespace EManagersLib {
    public sealed class EModule : IUserMod, ILoadingExtension {
        internal const string m_modVersion = "0.9.7";
        internal const string m_assemblyVersion = m_modVersion + ".*";
        internal const string m_modName = "Extended Managers";
        internal const string m_modDesc = "A library that extends the existing framework in Cities Skylines";
        internal const string HARMONYID = "quistar.EManagersLib.mod";
        internal const string m_settingsFile = "EManagersLibKeyBind";

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
            GameSettings.AddSettingsFile(new SettingsFile[] {
                        new SettingsFile() { fileName = m_settingsFile }
                    });
            EUtils.CheckIncompatibleMods();
            HarmonyHelper.DoOnHarmonyReady(() => EUtils.EnablePropPatches());
        }

        public void OnDisabled() {
            if (HarmonyHelper.IsHarmonyInstalled) {
                EUtils.DisablePropPatches();
            }
        }

        public void OnSettingsUI(UIHelperBase helper) {
            UIPanel root = (helper.AddGroup(m_modName + " " + m_modVersion) as UIHelper).self as UIPanel;
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
            EStatsPanel.Initialize();
        }

        public void OnLevelUnloading() {

        }
    }
}
