using CitiesHarmony.API;
using ICities;

namespace EManagersLib {
    public class EModule : IUserMod {
        internal const string m_modVersion = "0.1.0";
        internal const string m_assemblyVersion = m_modVersion + ".*";
        internal const string m_modName = "Extended Managers";
        internal const string m_modDesc = "A library that extends the existing framework in Cities Skylines";
        internal const string HARMONYID = "quistar.EManagersLib.mod";

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
    }
}
