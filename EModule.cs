using CitiesHarmony.API;
using ColossalFramework.UI;
using ICities;

namespace EManagersLib {
    public class EModule : IUserMod, ILoadingExtension {
        internal const string m_modVersion = "0.6.0";
        internal const string m_assemblyVersion = m_modVersion + ".*";
        internal const string m_modName = "Extended Managers";
        internal const string m_modDesc = "A library that extends the existing framework in Cities Skylines";
        internal const string HARMONYID = "quistar.EManagersLib.mod";
        private static UIPanel m_statsPanel;
        public string Name => m_modName;
        public string Description => m_modDesc;

        public void OnEnabled() {
            EUtils.CreateDebugFile();
            HarmonyHelper.DoOnHarmonyReady(() => EUtils.EnablePropPatches());
        }

        public void OnDisabled() {
            if (HarmonyHelper.IsHarmonyInstalled) {
                EUtils.DisablePropPatches();
            }
        }

        public void OnCreated(ILoading loading) { }

        public void OnReleased() { }

        public void OnLevelLoaded(LoadMode mode) {
            m_statsPanel = UIView.GetAView().AddUIComponent(typeof(EStatsPanel)) as UIPanel;
        }

        public void OnLevelUnloading() {
            if (!(m_statsPanel is null)) UnityEngine.Object.Destroy(m_statsPanel);
        }
    }
}
