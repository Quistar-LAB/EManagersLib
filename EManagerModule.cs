using ICities;

namespace EManagersLib {
    public class EManagerModule : IUserMod {
        internal const string m_modVersion = "0.1.0";
        internal const string m_assemblyVersion = m_modVersion + ".*";
        internal const string m_modName = "Extended Managers Lib";
        internal const string m_modDesc = "A library that extends the existing framework in Cities Skylines";
        public string Name => m_modName;
        public string Description => m_modDesc;
    }
}
