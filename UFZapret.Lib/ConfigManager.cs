using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace UFZ.Lib
{
    public static class ConfigManager
    {
        private static string configPath;
        private static Dictionary<string, string> _config;
        private static readonly object _lock = new object();
        private static bool _isInitialized = false;

        public static void Initialize(string appDirectory = null)
        {
            if (_isInitialized) return;

            lock (_lock)
            {
                if (_isInitialized) return;

                // Устанавливаем абсолютный путь к конфигу
                if (string.IsNullOrEmpty(appDirectory))
                {
                    // Используем папку приложения, а не рабочую директорию
                    string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                    appDirectory = Path.GetDirectoryName(exePath);
                }

                configPath = Path.Combine(appDirectory, "Config.cfg");
                Debug.WriteLine($"[ConfigManager] Config path: {configPath}");

                _config = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                try
                {
                    if (!File.Exists(configPath))
                    {
                        Debug.WriteLine("[ConfigManager] Config file not found, creating default...");
                        CreateDefaultConfig();
                    }
                    else
                    {
                        Debug.WriteLine($"[ConfigManager] Loading config from: {configPath}");
                        LoadConfigFromFile();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[ConfigManager] Initialization error: {ex.Message}");
                    CreateDefaultConfig();
                }

                _isInitialized = true;
            }
        }

        private static void LoadConfigFromFile()
        {
            var lines = File.ReadAllLines(configPath);
            foreach (var line in lines)
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

        public static string GetValue(string key, string defaultValue = "")
        {
            if (!_isInitialized)
            {
                Debug.WriteLine($"[ConfigManager] Config not initialized when getting {key}");
                Initialize();
            }

            lock (_lock)
            {
                return _config.TryGetValue(key, out var value) ? value : defaultValue;
            }
        }

        public static void SetValue(string key, string value)
        {
            if (!_isInitialized) Initialize();

            lock (_lock)
            {
                _config[key] = value;
                SaveConfig();
            }
        }

        private static void SaveConfig()
        {
            try
            {
                var lines = _config.Select(kvp => $"{kvp.Key} = {kvp.Value}");
                File.WriteAllLines(configPath, lines);
                Debug.WriteLine($"[ConfigManager] Config saved to {configPath}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ConfigManager] Save error: {ex.Message}");
            }
        }

        public static bool IsAutoStartEnabled()
        {
            if (!_isInitialized) Initialize();

            lock (_lock)
            {
                return _config.TryGetValue("autoStart", out var autoStart) && autoStart == "true";
            }
        }

        public static void Reload()
        {
            lock (_lock)
            {
                _isInitialized = false;
                Initialize();
            }
        }
    }
}