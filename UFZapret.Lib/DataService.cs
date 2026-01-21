using System.Diagnostics;
using System.IO;
using UFZ.Lib;
using static System.Net.WebRequestMethods;
using File = System.IO.File;

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

        public static void SetStartupArguments(string arguments)
        {
            ConfigManager.SetValue("startupArgs", arguments);
        }

        #region ORIGIN WORKFLOW

        public static void CreateNewGitClone_Zapret(string path)
        {
            string pathValue = path + "\\UFZapretUpdater_Zapret.bat";

            string updateText =
                "cd \\\n"+
                "cd " + path + "\n" +
                "git clone https://github.com/Flowseal/zapret-discord-youtube";

            File.WriteAllText(pathValue, updateText);

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "cmd.exe", // Запускаем командную строку
                Arguments = $"/C \"{pathValue}\"", // /C выполняет команду и завершает cmd
                RedirectStandardOutput = true, // Перенаправляем вывод
                RedirectStandardError = true,  // Перенаправляем ошибки
                UseShellExecute = false,      // Не использовать оболочку Windows
                CreateNoWindow = true         // Не создавать окно
            };

            Debug.WriteLine("===Starting FlowSeal Clone===");
            using (Process process = Process.Start(psi))
            {
                // Читаем вывод асинхронно
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                Debug.WriteLine(output);

                process.WaitForExit(); // Ждем завершения процесса

                if (!string.IsNullOrEmpty(error))
                {
                    Debug.WriteLine("Error: ", error);
                }
            }

            Debug.WriteLine("===Exiting FlowSeal Clone===");

            File.Delete(pathValue);
        }

        public static bool GitExisting_Zapret(string path)
        {
            return Directory.Exists(path + "\\.github");
        }

        public static void UpdateZapret_Origin(string path)
        {
            string pathValue = path + "\\UFZapretUpdater_Zapret.bat";

            string updateText =
                "cd \\\n" +
                "cd " + path + "\n" +
                "git pull";

            File.WriteAllText(pathValue, updateText);

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "cmd.exe", // Запускаем командную строку
                Arguments = $"/C \"{pathValue}\"", // /C выполняет команду и завершает cmd
                RedirectStandardOutput = true, // Перенаправляем вывод
                RedirectStandardError = true,  // Перенаправляем ошибки
                UseShellExecute = false,      // Не использовать оболочку Windows
                CreateNoWindow = true         // Не создавать окно
            };

            Debug.WriteLine("===Starting FlowSeal Pull===");
            using (Process process = Process.Start(psi))
            {
                // Читаем вывод асинхронно
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                Debug.WriteLine(output);

                process.WaitForExit(); // Ждем завершения процесса

                if (!string.IsNullOrEmpty(error))
                {
                    Debug.WriteLine("Error: ", error);
                }
            }

            Debug.WriteLine("===Exiting FlowSeal Pull===");

            File.Delete(pathValue);
        }

        public static bool IsThereUpdateZapret_Origin(string path)
        {

            return false;
        }

        public static string GetLocalVersion_Origin(string path)
        {
            string value = "none";

            if (GitExisting_Zapret(path))
            {
                value = File.ReadAllText(path + "\\.service\\version.txt");

                return value;
            }
            else
            {
                return value;
            }
        }

        #endregion
    }
}