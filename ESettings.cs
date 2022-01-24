using System.IO;
using System.Threading;
using System.Xml;

namespace EManagersLib {
    public readonly struct ESettings {
        private const string ESettingsFileName = @"EMLSettings.xml";
        private static readonly object m_settingsLock = new object();
        /// <summary>
        /// Using a static value for now, and will probably build an interface in the future for
        /// variable limit
        /// </summary>
        public static int m_maxOutsideConnection = 32;
        public static bool m_electrifiedRoad = true;
        public static bool m_wateredRoad = true;

        internal static bool LoadSettings() {
            try {
                if (!File.Exists(ESettingsFileName)) {
                    SaveSettings();
                } else {
                    XmlDocument xmlConfig = new XmlDocument {
                        XmlResolver = null
                    };
                    xmlConfig.Load(ESettingsFileName);
                    m_maxOutsideConnection = int.Parse(xmlConfig.DocumentElement.GetAttribute(@"MaxOutsideConnection"));
                    m_electrifiedRoad = bool.Parse(xmlConfig.DocumentElement.GetAttribute(@"ElectrifiedRoad"));
                    m_wateredRoad = bool.Parse(xmlConfig.DocumentElement.GetAttribute(@"WateredRoad"));
                }
            } catch {
                SaveSettings(); // Most likely a corrupted file if we enter here. Recreate the file
                return false;
            }
            return true;
        }

        internal static void SaveSettings(object _ = null) {
            Monitor.Enter(m_settingsLock);
            try {
                XmlDocument xmlConfig = new XmlDocument {
                    XmlResolver = null
                };
                XmlElement root = xmlConfig.CreateElement(@"EMLConfigs");
                root.Attributes.Append(AddElement(xmlConfig, @"MaxOutsideConnection", m_maxOutsideConnection));
                root.Attributes.Append(AddElement(xmlConfig, @"ElectrifiedRoad", m_electrifiedRoad));
                root.Attributes.Append(AddElement(xmlConfig, @"WateredRoad", m_wateredRoad));
                xmlConfig.AppendChild(root);
                xmlConfig.Save(ESettingsFileName);
            } finally {
                Monitor.Exit(m_settingsLock);
            }
        }

        private static XmlAttribute AddElement<T>(XmlDocument doc, string name, T t) {
            XmlAttribute attr = doc.CreateAttribute(name);
            attr.Value = t.ToString();
            return attr;
        }
    }
}
