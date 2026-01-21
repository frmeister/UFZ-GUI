using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace UFZ.Lib
{
    public static class ConfigManager
    {
        private static string configPath = "Config.cfg";
        private static Dictionary<string, string> _config;
        private static readonly object _lock = new object();
        private static bool _isLoaded = false;
        private static DateTime _lastLoadTime = DateTime.MinValue;

        // Явная инициализация - вызывается в начале программы
        public static void Initialize()
        {
            if (!_isLoaded)
            {
                LoadConfig();
            }
        }

        // Загружаем конфиг с блокировкой
        private static void LoadConfig()
        {
            lock (_lock)
            {
                _config = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                if (!File.Exists(configPath))
                {
                    CreateDefaultConfig();
                    _isLoaded = true;
                    _lastLoadTime = DateTime.Now;
                    return;
                }

                try
                {
                    // Читаем весь файл за один раз
                    string content = File.ReadAllText(configPath);
                    ParseConfigContent(content);
                    _isLoaded = true;
                    _lastLoadTime = DateTime.Now;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Ошибка загрузки конфига: {ex.Message}");
                    CreateDefaultConfig();
                }
            }
        }

        private static void ParseConfigContent(string content)
        {
            using (var reader = new StringReader(content))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var trimmed = line.Trim();
                    if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#"))
                        continue;

                    var parts = trimmed.Split('=', 2);
                    if (parts.Length == 2)
                    {
                        _config[parts[0].Trim()] = parts[1].Trim();
                    }
                }
            }
        }

        // Принудительная перезагрузка
        public static void Reload()
        {
            lock (_lock)
            {
                _isLoaded = false;
                LoadConfig();
            }
        }

        // Получение значения
        public static string GetValue(string key, string defaultValue = "")
        {
            if (!_isLoaded) Initialize();

            lock (_lock)
            {
                return _config.TryGetValue(key, out var value) ? value : defaultValue;
            }
        }

        // Установка значения с немедленным сохранением
        public static void SetValue(string key, string value)
        {
            if (!_isLoaded) Initialize();

            lock (_lock)
            {
                _config[key] = value;
                SaveConfig();
            }
        }

        // Установка значения с асинхронным сохранением
        public static void SetValueAsync(string key, string value)
        {
            if (!_isLoaded) Initialize();

            lock (_lock)
            {
                _config[key] = value;
            }

            Task.Run(() => SaveConfigAsync());
        }

        // Синхронное сохранение
        private static void SaveConfig()
        {
            lock (_lock)
            {
                try
                {
                    var lines = _config.Select(kvp => $"{kvp.Key} = {kvp.Value}");
                    File.WriteAllLines(configPath, lines);
                    _lastLoadTime = DateTime.Now;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Ошибка сохранения конфига: {ex.Message}");
                }
            }
        }

        // Асинхронное сохранение
        private static async Task SaveConfigAsync()
        {
            Dictionary<string, string> configCopy;

            lock (_lock)
            {
                configCopy = new Dictionary<string, string>(_config);
            }

            try
            {
                var lines = configCopy.Select(kvp => $"{kvp.Key} = {kvp.Value}");
                await File.WriteAllLinesAsync(configPath, lines);

                lock (_lock)
                {
                    _lastLoadTime = DateTime.Now;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка асинхронного сохранения: {ex.Message}");
            }
        }

        private static void CreateDefaultConfig()
        {
            _config = new Dictionary<string, string>
            {
                { "isThisFirstLaunch", "true" },
                { "pathOrigin", "none" },
                { "currentConfig", "none" },
                { "autoStart", "false" },
                { "startupArgs", "none" },
                { "appVersion", "0.21" },
                { "originVersion", "none" }
            };

            SaveConfig();
        }

        // Проверка только параметров автозапуска
        public static bool IsAutoStartEnabled()
        {
            if (!_isLoaded) Initialize();
            return GetValue("autoStart", "false") == "true";
        }

        // Получение аргументов автозапуска
        public static string GetStartupArgs()
        {
            if (!_isLoaded) Initialize();
            return GetValue("startupArgs", "none");
        }
    }
}