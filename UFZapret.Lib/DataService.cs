using System.IO;
using UFZ.Lib;

namespace UFZapret.Lib
{
    public class DataService
    {
        
        // Prohibits program of starting on entrance form every launch
        public bool IsFirstLaunch()
        {
            string value = ConfigManager.GetValue("isThisFirstLaunch", "true");

            if (value == "true")
            {
                ConfigManager.SetValue("isThisFirstLaunch", "false");
                return true;
            }
            else return false;
        }

        public static void SaveFolderPath(string path)
        {
            string value = ConfigManager.GetValue("pathOrigin", "none");

            ConfigManager.SetValue("pathOrigin", path);

        }

        public static void SaveCurrentConfig(string name)
        {
            string value = ConfigManager.GetValue("currentConfig", "none");

            ConfigManager.SetValue("currentConfig", name);
        }

        public static bool GetAutoStart()
        {
            return ConfigManager.GetValue("autoStart", "false") == "true";
        }

        public static void SetAutoStart(bool enabled)
        {
            ConfigManager.SetValue("autoStart", enabled ? "true" : "false");
        }

        public static string GetStartupArguments()
        {
            return ConfigManager.GetValue("startupArgs", "--minimized");
        }
    }
}