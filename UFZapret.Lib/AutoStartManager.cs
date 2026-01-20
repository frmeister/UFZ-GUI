using Microsoft.Win32;
using System;
using System.IO;

namespace UFZapret.Lib // Или UFZapret.Forms - смотри по вашему проекту
{
    public static class AutoStartManager
    {
        private const string RegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
        private const string AppName = "UFZapret";

        public static bool IsEnabled()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, false))
                {
                    return key?.GetValue(AppName) != null;
                }
            }
            catch { return false; }
        }

        public static bool Enable(string arguments = "")
        {
            try
            {
                string exePath = GetExecutablePath();
                if (string.IsNullOrEmpty(exePath)) return false;

                string command = $"\"{exePath}\" {arguments}".Trim();

                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true))
                {
                    key.SetValue(AppName, command);
                    return true;
                }
            }
            catch { return false; }
        }

        public static bool Disable()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true))
                {
                    key.DeleteValue(AppName, false);
                    return true;
                }
            }
            catch { return false; }
        }

        private static string GetExecutablePath()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().Location;
        }
    }
}