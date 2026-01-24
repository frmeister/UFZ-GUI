using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;

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
                string exePath = GetExecutablePath();
                if (string.IsNullOrEmpty(exePath))
                {
                    Debug.WriteLine("[AutoStart] Не удалось получить путь к программе");
                    return false;
                }

                Debug.WriteLine($"[AutoStart] Путь к exe: {exePath}");
                Debug.WriteLine($"[AutoStart] Аргументы для авто-запуска: '{arguments}'");

                // Формируем команду
                string command = $"\"{exePath}\"";
                if (!string.IsNullOrWhiteSpace(arguments))
                {
                    command += $" {arguments}";
                }

                Debug.WriteLine($"[AutoStart] Команда для реестра: {command}");

                // Создаем или открываем ключ
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

        public static bool Disable()
        {
            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath, true))
                {
                    if (key == null) return true; // Ключа нет - значит уже отключен

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

        private static string GetExecutablePath()
        {
            try
            {
                // Получаем путь через текущий процесс
                using (Process process = Process.GetCurrentProcess())
                {
                    string path = process.MainModule.FileName;

                    if (File.Exists(path) && path.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
                    {
                        return path;
                    }
                }

                // Альтернативный способ
                return System.Reflection.Assembly.GetEntryAssembly().Location;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AutoStart] Ошибка получения пути: {ex.Message}");
                return null;
            }
        }

        // Метод для синхронизации настроек (вызывается из FormMain)
        public static void SyncWithConfig()
        {
            try
            {
                bool autoStartInConfig = DataService.GetAutoStart();
                bool autoStartInRegistry = IsEnabled();

                if (autoStartInConfig != autoStartInRegistry)
                {
                    Debug.WriteLine($"[AutoStart] Расхождение: Config={autoStartInConfig}, Registry={autoStartInRegistry}");

                    // Обновляем реестр в соответствии с конфигом
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