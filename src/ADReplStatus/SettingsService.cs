using System;
using Microsoft.Win32;

namespace ADReplStatus
{
    public static class SettingsService
    {
        private const string RegistryKeyPath = "SOFTWARE\\ADREPLSTATUS";

        public static void LoadSettings()
        {
            var state = AppState.Instance;

            using (var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, false))
            {
                if (key != null)
                {
                    state.ForestName = key.GetValue("ForestName", string.Empty).ToString();
                    state.DarkMode = Convert.ToBoolean(key.GetValue("DarkMode", false));
                }
            }
        }

        public static void SaveDarkMode(bool darkMode)
        {
            using (var key = Registry.CurrentUser.CreateSubKey(RegistryKeyPath, true))
            {
                if (key != null)
                {
                    key.SetValue("DarkMode", darkMode ? 1 : 0);
                }
            }
        }

        public static void SaveForestName(string forestName)
        {
            using (var key = Registry.CurrentUser.CreateSubKey(RegistryKeyPath, true))
            {
                if (key != null)
                {
                    key.SetValue("ForestName", forestName);
                }
            }
        }

        public static string DetectForestName()
        {
            using (var forest = System.DirectoryServices.ActiveDirectory.Forest.GetCurrentForest())
            {
                return forest.Name;
            }
        }
    }
}
