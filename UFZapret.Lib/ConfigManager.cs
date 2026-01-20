// ConfigManager.cs - ОДИН на все приложение
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace UFZ.Lib
{
    public static class ConfigManager
    {
        private static string configPath = "Config.cfg";
        private static Dictionary<string, string> cachedConfig;
        private static DateTime lastReadTime; // Not intended
        private static readonly object lockObject = new object();

        // Инициализация при запуске программы
        static ConfigManager()
        {
            LoadConfig();
        }

        // Загружаем конфигурацию один раз при запуске
        private static void LoadConfig()
        {
            lock (lockObject)
            {
                cachedConfig = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                if (!File.Exists(configPath))
                {
                    CreateDefaultConfig();
                    return;
                }

                var lines = File.ReadAllLines(configPath);
                foreach (var line in lines)
                {
                    var trimmed = line.Trim();
                    if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#"))
                        continue;

                    var parts = trimmed.Split('=', 2);
                    if (parts.Length == 2)
                    {
                        cachedConfig[parts[0].Trim()] = parts[1].Trim();
                    }
                }

                lastReadTime = File.GetLastWriteTime(configPath);
            }
        }

        // DEBUG: Path finder method
        public static string GetCurrentConfigPath()
        {
            // Получаем полный путь к файлу конфигурации
            return Path.GetFullPath(configPath);
        }

        // Получить значение параметра (из кэша)
        public static string GetValue(string key, string defaultValue = "")
        {
            lock (lockObject)
            {
                // Можно добавить проверку обновления файла, если нужно
                return cachedConfig.ContainsKey(key) ? cachedConfig[key] : defaultValue;
            }
        }

        // Установить значение параметра
        public static bool SetValue(string key, string value)
        {
            lock (lockObject)
            {
                cachedConfig[key] = value;

                // Отложенное сохранение (например, через 2 секунды)
                // или сохранение в отдельном потоке
                Task.Run(() => SafeSaveAsync());

                return true;
            }
        }

        private static async Task SafeSaveAsync()
        {
            string tempFile = Path.GetTempFileName();

            try
            {
                // 1. Пишем во временный файл
                var lines = cachedConfig.Select(kvp => $"{kvp.Key} = {kvp.Value}");
                await File.WriteAllLinesAsync(tempFile, lines);

                // 2. Атомарно заменяем оригинальный
                File.Replace(tempFile, configPath, null);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка: {ex.Message}");
            }
            finally
            {
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
        }

        // Сохранение всех настроек в файл
        private static void SaveConfig()
        {
            try
            {
                var lines = cachedConfig.Select(kvp => $"{kvp.Key} = {kvp.Value}");
                File.WriteAllLines(configPath, lines);
                lastReadTime = File.GetLastWriteTime(configPath);
            }
            catch (Exception ex)
            {
                // Логирование ошибки
                System.Diagnostics.Debug.WriteLine($"Ошибка сохранения конфигурации: {ex.Message}");
            }
        }

        private static void CreateDefaultConfig()
        {
            var defaults = new Dictionary<string, string>
            {
                { "isThisFirstLaunch", "true" },
                { "pathOrigin", "none" },
                { "currentConfig", "none" },
                { "autoStart", "false" },
                { "startupArgs", "none" },
                { "appVersion", "0.21" },
                // ... другие параметры по умолчанию
            };

            cachedConfig = defaults;
            SaveConfig();
        }
    }
}