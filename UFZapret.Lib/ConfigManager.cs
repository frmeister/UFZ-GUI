using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace UFZ.Lib
{
    public static class ConfigManager
    {
        private static string configPath = "Config.cfg";
        private static Dictionary<string, string> _config;
        private static readonly object _lock = new object();
        private static bool _isInitialized = false;
        private static ManualResetEventSlim _initEvent = new ManualResetEventSlim(false);

        // Инициализация с гарантированной загрузкой
        public static void Initialize(bool waitForConfig = false)
        {
            if (_isInitialized)
            {
                _initEvent.Set();
                return;
            }

            lock (_lock)
            {
                if (_isInitialized)
                {
                    _initEvent.Set();
                    return;
                }

                try
                {
                    _config = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                    if (!File.Exists(configPath))
                    {
                        Debug.WriteLine("[ConfigManager] Config file not found, creating default...");
                        CreateDefaultConfig();
                    }
                    else
                    {
                        Debug.WriteLine($"[ConfigManager] Loading config from: {Path.GetFullPath(configPath)}");

                        // Пытаемся загрузить несколько раз с ожиданием
                        bool loaded = false;
                        int attempts = waitForConfig ? 10 : 3;

                        for (int i = 0; i < attempts; i++)
                        {
                            try
                            {
                                LoadConfigFromFile();
                                loaded = true;
                                break;
                            }
                            catch (IOException ex) when (i < attempts - 1)
                            {
                                Debug.WriteLine($"[ConfigManager] Attempt {i + 1}/{attempts} failed: {ex.Message}");
                                Thread.Sleep(100 * (i + 1)); // Увеличиваем задержку
                            }
                        }

                        if (!loaded)
                        {
                            Debug.WriteLine("[ConfigManager] Failed to load config, creating default");
                            CreateDefaultConfig();
                        }
                    }

                    // Логируем ключевые параметры для отладки (БЕЗ использования GetValue!)
                    Debug.WriteLine($"[ConfigManager] Config loaded successfully. Entries: {_config.Count}");

                    // Просто выводим содержимое словаря без рекурсии
                    foreach (var kvp in _config)
                    {
                        Debug.WriteLine($"[ConfigManager]   {kvp.Key} = {kvp.Value}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[ConfigManager] Initialization error: {ex.Message}");
                    CreateDefaultConfig();
                }

                _isInitialized = true;
                _initEvent.Set();
            }
        }

        // Ожидание инициализации
        public static bool WaitForInitialization(int timeoutMs = 5000)
        {
            return _initEvent.Wait(timeoutMs);
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
                Debug.WriteLine($"[ConfigManager] Warning: Config not initialized when getting {key}");
                Initialize();
            }

            lock (_lock)
            {
                return _config.TryGetValue(key, out var value) ? value : defaultValue;
            }
        }

        public static void SetValue(string key, string value)
        {
            if (!_isInitialized)
            {
                Initialize();
            }

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
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ConfigManager] Save error: {ex.Message}");
            }
        }

        public static bool IsAutoStartEnabled()
        {
            if (!_isInitialized)
            {
                Initialize();
            }

            lock (_lock)
            {
                // Получаем значение напрямую из словаря, без вызова GetValue
                return _config.TryGetValue("autoStart", out var autoStart) && autoStart == "true";
            }
        }

        // Новый метод для безопасного получения значений при инициализации
        public static string GetValueDirect(string key, string defaultValue = "")
        {
            lock (_lock)
            {
                return _config.TryGetValue(key, out var value) ? value : defaultValue;
            }
        }
    }
}