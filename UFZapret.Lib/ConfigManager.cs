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
        private static string configPath = "Config.cfg";
        private static Dictionary<string, string> _config;
        private static readonly object _lock = new object();
        private static bool _isInitialized = false;

        public static void Initialize()
        {
            if (_isInitialized) return;

            lock (_lock)
            {
                if (_isInitialized) return;

                _config = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                try
                {
                    if (!File.Exists(configPath))
                    {
                        Debug.WriteLine("[ConfigManager] Config file not found, creating default...");
                        CreateDefaultConfig();
                        LogToFile("[ConfigManager] Default config created");
                    }
                    else
                    {
                        Debug.WriteLine($"[ConfigManager] Loading config from: {Path.GetFullPath(configPath)}");
                        LogToFile($"[ConfigManager] Loading config from: {Path.GetFullPath(configPath)}");

                        LoadConfigFromFile();

                        // Логируем ключевые параметры
                        string logMessage = $"[ConfigManager] Loaded config: ";
                        foreach (var key in new[] { "isThisFirstLaunch", "autoStart", "pathOrigin", "currentConfig" })
                        {
                            if (_config.TryGetValue(key, out var value))
                            {
                                logMessage += $"{key}={value}, ";
                            }
                        }
                        Debug.WriteLine(logMessage);
                        LogToFile(logMessage);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[ConfigManager] Initialization error: {ex.Message}");
                    LogToFile($"[ConfigManager] Initialization error: {ex.Message}");
                    CreateDefaultConfig();
                }

                _isInitialized = true;
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

        private static void LoadConfigFromFile()
        {
            try
            {
                // Читаем файл с несколькими попытками
                string[] lines = null;
                for (int i = 0; i < 3; i++)
                {
                    try
                    {
                        lines = File.ReadAllLines(configPath);
                        break;
                    }
                    catch (IOException) when (i < 2)
                    {
                        Thread.Sleep(100);
                    }
                }

                if (lines == null)
                {
                    throw new IOException("Failed to read config file after 3 attempts");
                }

                var newConfig = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                foreach (var line in lines)
                {
                    var trimmed = line.Trim();
                    if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#"))
                        continue;

                    var parts = trimmed.Split('=', 2);
                    if (parts.Length == 2)
                    {
                        newConfig[parts[0].Trim()] = parts[1].Trim();
                    }
                }

                _config = newConfig;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ConfigManager] LoadConfigFromFile error: {ex.Message}");
                LogToFile($"[ConfigManager] LoadConfigFromFile error: {ex.Message}");
                throw;
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
                LogToFile($"[ConfigManager] Config not initialized when getting {key}");
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
                Debug.WriteLine($"[ConfigManager] Config saved");
                LogToFile($"[ConfigManager] Config saved");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ConfigManager] Save error: {ex.Message}");
                LogToFile($"[ConfigManager] Save error: {ex.Message}");
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

        // Проверка валидности конфига для запуска приложения
        public static bool IsConfigValidForStartup()
        {
            if (!_isInitialized) Initialize();

            lock (_lock)
            {
                // Проверяем только autoStart для авто-запуска
                return true; // Всегда возвращаем true, так как pathOrigin не требуется для запуска приложения
            }
        }

        // Вспомогательный метод для логирования в файл
        private static void LogToFile(string message)
        {
            try
            {
                string logPath = "config_debug.log";
                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}\n";
                File.AppendAllText(logPath, logEntry);
            }
            catch
            {
                // Игнорируем ошибки логирования
            }
        }
    }
}