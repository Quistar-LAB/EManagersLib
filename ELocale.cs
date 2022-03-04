using ColossalFramework;
using ColossalFramework.Globalization;
using ColossalFramework.IO;
using ColossalFramework.PlatformServices;
using EManagersLib.Localization;
using System;
using System.Globalization;
using System.IO;
using System.Xml;

namespace EManagersLib {
    internal static class ELocale {
        private const ulong m_betaModID = 2607980391uL;
        private const ulong m_thisModID = 2696146165uL;
        private const string m_fileNameTemplate = @"EManagersLib.";
        private const string m_defaultFile = @"EManagersLib.en.locale";
        private static XmlDocument m_xmlLocale;
        private static string m_directory;
        private static bool isInitialized = false;

        public static void OnLocaleChanged() {
            string locale = SingletonLite<LocaleManager>.instance.language;
            if (locale == @"zh") {
                if (CultureInfo.InstalledUICulture.Name == @"zh-TW") {
                    locale = @"zh-TW";
                } else {
                    locale = @"zh-CN";
                }
            } else if (locale == @"pt") {
                if (CultureInfo.InstalledUICulture.Name == @"pt-BR") {
                    locale = @"pt-BR";
                }
            } else {
                switch (CultureInfo.InstalledUICulture.Name) {
                case @"ms":
                case @"ms-MY":
                    locale = @"ms";
                    break;
                case @"ja":
                case @"ja-JP":
                    locale = @"ja";
                    break;
                }
            }
            LoadLocale(locale);
        }

        private static void LoadLocale(string culture) {
            XmlDocument locale = new XmlDocument {
                XmlResolver = null
            };
            try {
                string localeFile = m_directory + m_fileNameTemplate + culture + @".locale";
                locale.Load(localeFile);
            } catch {
                /* Load default english locale stored in dll */
                using (MemoryStream ms = new MemoryStream(DefaultLocale.EManagersLib_en)) {
                    locale.Load(ms);
                }
            } finally {
                m_xmlLocale = locale;
            }
        }

        internal static void Init() {
            if (!isInitialized) {
                try {
                    foreach (PublishedFileId fileID in PlatformService.workshop.GetSubscribedItems()) {
                        if (fileID.AsUInt64 == m_thisModID || fileID.AsUInt64 == m_betaModID) {
                            string dir = PlatformService.workshop.GetSubscribedItemPath(fileID) + Path.DirectorySeparatorChar + @"Locale" + Path.DirectorySeparatorChar;
                            if (Directory.Exists(dir) && File.Exists(dir + m_defaultFile)) {
                                m_directory = dir;
                                break;
                            }
                        }
                    }
                    if (m_directory is null) {
                        string dir = DataLocation.modsPath + Path.DirectorySeparatorChar + @"EManagersLib" + Path.DirectorySeparatorChar + @"Locale" + Path.DirectorySeparatorChar;
                        if (Directory.Exists(dir) && File.Exists(dir + m_defaultFile)) {
                            m_directory = dir;
                        }
                    }
                    isInitialized = true;
                } catch (Exception e) {
                    UnityEngine.Debug.LogException(e);
                }
            }
        }

        internal static void Destroy() {
            LocaleManager.eventLocaleChanged -= OnLocaleChanged;
        }

        internal static string GetLocale(string name) => m_xmlLocale.GetElementById(name).InnerText;

    }
}
