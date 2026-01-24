using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace UFZapret.Lib
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
                    if (key == null) return false;

                    object value = key.GetValue(AppName);
                    return value != null && !string.IsNullOrEmpty(value.ToString());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AutoStart] Ошибка проверки: {ex.Message}");
                return false;
            }
        }

        public static bool Enable(string arguments = "")
        {
            try
            {
                // Получаем путь к EXE файлу
                string exePath = GetExecutablePath();
                if (string.IsNullOrEmpty(exePath) || !File.Exists(exePath))
                {
                    Debug.WriteLine($"[AutoStart] Не удалось найти exe файл: {exePath}");
                    return false;
                }

                Debug.WriteLine($"[AutoStart] Путь к exe: {exePath}");

                // Формируем команду для реестра
                string command = $"\"{exePath}\"";
                if (!string.IsNullOrWhiteSpace(arguments))
                {
                    command += $" {arguments}";
                }

                Debug.WriteLine($"[AutoStart] Команда для реестра: {command}");

                // Записываем в реестр
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true))
                {
                    if (key == null)
                    {
                        // Создаем ключ, если его нет
                        using (RegistryKey newKey = Registry.CurrentUser.CreateSubKey(RegistryKeyPath, true))
                        {
                            newKey.SetValue(AppName, command, RegistryValueKind.String);
                        }
                    }
                    else
                    {
                        key.SetValue(AppName, command, RegistryValueKind.String);
                    }
                }

                // Проверяем результат
                bool success = IsEnabled();
                Debug.WriteLine(success
                    ? "[AutoStart] Успешно добавлен в реестр"
                    : "[AutoStart] Не удалось добавить в реестр");

                return success;
            }
            catch (UnauthorizedAccessException ex)
            {
                Debug.WriteLine($"[AutoStart] Нет прав на запись в реестр: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AutoStart] Ошибка: {ex.Message}");
                return false;
            }
        }

        private static string GetExecutablePath()
        {
            try
            {
                // Метод 1: Получаем путь через Entry Assembly
                string assemblyPath = Assembly.GetEntryAssembly().Location;
                Debug.WriteLine($"[AutoStart] Assembly path: {assemblyPath}");

                // Если это dll, пробуем найти exe с тем же именем
                if (assemblyPath.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                {
                    // Заменяем расширение .dll на .exe
                    string exePath = Path.ChangeExtension(assemblyPath, ".exe");

                    if (File.Exists(exePath))
                    {
                        Debug.WriteLine($"[AutoStart] Found exe: {exePath}");
                        return exePath;
                    }

                    // Ищем в директории публикации
                    string publishPath = GetPublishExecutablePath();
                    if (!string.IsNullOrEmpty(publishPath) && File.Exists(publishPath))
                    {
                        Debug.WriteLine($"[AutoStart] Found publish exe: {publishPath}");
                        return publishPath;
                    }
                }

                // Если это уже exe, возвращаем как есть
                if (assemblyPath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                {
                    return assemblyPath;
                }

                // Метод 2: Пробуем через Process
                using (Process process = Process.GetCurrentProcess())
                {
                    string processPath = process.MainModule.FileName;
                    Debug.WriteLine($"[AutoStart] Process path: {processPath}");

                    if (processPath.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                    {
                        return processPath;
                    }
                }

                Debug.WriteLine("[AutoStart] Could not find exe path");
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AutoStart] Ошибка получения пути: {ex.Message}");
                return null;
            }
        }

        private static string GetPublishExecutablePath()
        {
            try
            {
                // Получаем базовую директорию
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                Debug.WriteLine($"[AutoStart] Base directory: {baseDir}");

                // Ищем exe файлы в директории
                string[] exeFiles = Directory.GetFiles(baseDir, "*.exe", SearchOption.TopDirectoryOnly);

                foreach (string exeFile in exeFiles)
                {
                    Debug.WriteLine($"[AutoStart] Found exe in directory: {exeFile}");

                    // Пытаемся найти основной exe файл (не vshost, не .config, не .manifest)
                    string fileName = Path.GetFileName(exeFile);
                    if (!fileName.Contains("vshost") &&
                        !fileName.EndsWith(".config.exe") &&
                        !fileName.EndsWith(".manifest.exe"))
                    {
                        return exeFile;
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        public static bool Disable()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true))
                {
                    if (key == null) return true;
                    key.DeleteValue(AppName, false);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AutoStart] Ошибка отключения: {ex.Message}");
                return false;
            }
        }

        public static void SyncWithConfig()
        {
            try
            {
                bool autoStartInConfig = DataService.GetAutoStart();
                bool autoStartInRegistry = IsEnabled();

                if (autoStartInConfig != autoStartInRegistry)
                {
                    Debug.WriteLine($"[AutoStart] Расхождение: Config={autoStartInConfig}, Registry={autoStartInRegistry}");

                    if (autoStartInConfig)
                    {
                        Enable(DataService.GetStartupArguments());
                    }
                    else
                    {
                        Disable();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AutoStart] Ошибка синхронизации: {ex.Message}");
            }
        }
    }
}