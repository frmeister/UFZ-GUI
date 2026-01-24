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
                    return key.GetValue(AppName) != null;
                }
            }
            catch
            {
                return false;
            }
        }

        public static bool Enable(string arguments = "")
        {
            try
            {
                // Получаем путь к exe через Assembly
                string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;

                if (string.IsNullOrEmpty(exePath))
                {
                    Debug.WriteLine("[AutoStart] Не удалось получить путь к программе");
                    return false;
                }

                // Формируем команду с АБСОЛЮТНЫМ путем
                string command = $"\"{exePath}\" {arguments}";

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

                Debug.WriteLine($"[AutoStart] Успешно добавлен в реестр с аргументами: '{arguments}'");
                return true;
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
                    if (key == null) return true;
                    key.DeleteValue(AppName, false);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        // В FormEntrance вызывайте так:
        // AutoStartManager.Enable("--minimized");
    }
}