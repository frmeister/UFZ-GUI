using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using UFZ.Lib;

namespace UFZapret.Lib
{
    public static class AppUpdater
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        // Скачивание и установка обновления
        public static async Task<bool> UpdateAppAsync(string downloadUrl)
        {
            try
            {
                string tempDir = Path.Combine(Path.GetTempPath(), $"UFZapret_Update_{Guid.NewGuid()}");
                string zipPath = Path.Combine(tempDir, "update.zip");

                // 1. Создаем временную папку
                Directory.CreateDirectory(tempDir);

                // 2. Скачиваем архив
                Debug.WriteLine($"[AppUpdater] Скачиваем обновление: {downloadUrl}");
                using (var stream = await _httpClient.GetStreamAsync(downloadUrl))
                using (var fileStream = new FileStream(zipPath, FileMode.Create))
                {
                    await stream.CopyToAsync(fileStream);
                }

                // 3. Распаковываем
                Debug.WriteLine($"[AppUpdater] Распаковываем архив");
                ZipFile.ExtractToDirectory(zipPath, tempDir, true);

                // 4. Получаем текущую директорию приложения
                string appDir = AppDomain.CurrentDomain.BaseDirectory;
                Debug.WriteLine($"[AppUpdater] Текущая директория: {appDir}");

                // 5. Копируем файлы, заменяя старые
                await CopyFilesWithUpdateAsync(tempDir, appDir);

                // 6. ОБНОВЛЯЕМ ВЕРСИЮ В КОНФИГЕ
                string newVersion = await ReadVersionFromUpdateAsync(tempDir);
                if (!string.IsNullOrEmpty(newVersion))
                {
                    Debug.WriteLine($"[AppUpdater] Обновляем версию в конфиге: {newVersion}");
                    ConfigManager.SetValue("appVersion", newVersion);
                }

                // 7. Очищаем временные файлы
                Directory.Delete(tempDir, true);

                Debug.WriteLine($"[AppUpdater] Обновление завершено");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AppUpdater] Ошибка обновления: {ex.Message}");
                return false;
            }
        }

        private static async Task CopyFilesWithUpdateAsync(string sourceDir, string targetDir)
        {
            // Исключаем файлы, которые не нужно обновлять
            var excludeFiles = new[] {
                "Config.cfg",           // Настройки пользователя
                "app_startup.log",      // Логи
                "config_debug.log"      // Логи
            };

            Debug.WriteLine($"[AppUpdater] Начинаем копирование из {sourceDir} в {targetDir}");

            // Получаем ВСЕ файлы рекурсивно
            foreach (var sourceFile in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
            {
                // Получаем относительный путь от корня sourceDir
                string relativePath = Path.GetRelativePath(sourceDir, sourceFile);
                string targetFile = Path.Combine(targetDir, relativePath);

                // Проверяем, не является ли файл исключенным (только имя файла)
                string fileName = Path.GetFileName(relativePath);
                if (excludeFiles.Contains(fileName, StringComparer.OrdinalIgnoreCase))
                {
                    Debug.WriteLine($"[AppUpdater] Пропускаем исключенный файл: {fileName}");
                    continue;
                }

                // Создаем директорию, если нужно
                string targetDirPath = Path.GetDirectoryName(targetFile);
                if (!Directory.Exists(targetDirPath))
                {
                    Directory.CreateDirectory(targetDirPath);
                    Debug.WriteLine($"[AppUpdater] Создана директория: {targetDirPath}");
                }

                try
                {
                    // Копируем файл с перезаписью
                    File.Copy(sourceFile, targetFile, true);
                    Debug.WriteLine($"[AppUpdater] Обновлен: {relativePath}");

                    // Небольшая задержка для стабильности
                    await Task.Delay(10);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[AppUpdater] Ошибка копирования {relativePath}: {ex.Message}");
                    throw; // Пробрасываем исключение дальше
                }
            }
        }

        // Получение ссылки на архив релиза
        public static async Task<string> GetLatestReleaseDownloadUrl()
        {
            try
            {
                string apiUrl = "https://api.github.com/repos/frmeister/UFZ-GUI/releases/latest";

                // Добавляем заголовок User-Agent (обязательно для GitHub API)
                _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("UFZapret-Update-Agent");
                var response = await _httpClient.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"[AppUpdater] GitHub API error: {response.StatusCode}");
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"[AppUpdater] API Response: {json.Substring(0, Math.Min(500, json.Length))}...");

                if (json.Contains("\"assets\""))
                {
                    // Ищем секцию assets
                    int assetsStart = json.IndexOf("\"assets\":[", StringComparison.OrdinalIgnoreCase);
                    if (assetsStart >= 0)
                    {
                        assetsStart += "\"assets\":[".Length;
                        int assetsEnd = json.IndexOf("]", assetsStart);
                        if (assetsEnd > assetsStart)
                        {
                            string assetsSection = json.Substring(assetsStart, assetsEnd - assetsStart);

                            // Разделяем по assets
                            var assets = assetsSection.Split(new[] { "}," }, StringSplitOptions.RemoveEmptyEntries);

                            foreach (var asset in assets)
                            {
                                // Ищем zip файл
                                if (asset.Contains("\"browser_download_url\"", StringComparison.OrdinalIgnoreCase) &&
                                    asset.Contains(".zip\"", StringComparison.OrdinalIgnoreCase))
                                {
                                    int urlStart = asset.IndexOf("\"browser_download_url\":\"", StringComparison.OrdinalIgnoreCase);
                                    if (urlStart >= 0)
                                    {
                                        urlStart += "\"browser_download_url\":\"".Length;
                                        int urlEnd = asset.IndexOf("\"", urlStart);
                                        if (urlEnd > urlStart)
                                        {
                                            string url = asset.Substring(urlStart, urlEnd - urlStart);
                                            Debug.WriteLine($"[AppUpdater] Found download URL: {url}");
                                            return url;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                Debug.WriteLine("[AppUpdater] No zip asset found");
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AppUpdater] Error in GetLatestReleaseDownloadUrl: {ex.Message}");
                return null;
            }
        }

        // Получение версии последнего релиза (tag_name)
        public static async Task<string> GetLatestReleaseVersionAsync()
        {
            try
            {
                string apiUrl = "https://api.github.com/repos/frmeister/UFZ-GUI/releases/latest";

                _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("UFZapret-Update-Agent");
                var response = await _httpClient.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"[AppUpdater] GitHub API error (version): {response.StatusCode}");
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"[AppUpdater] Version API Response: {json.Substring(0, Math.Min(300, json.Length))}...");

                int tagStart = json.IndexOf("\"tag_name\":\"", StringComparison.OrdinalIgnoreCase);
                if (tagStart >= 0)
                {
                    tagStart += "\"tag_name\":\"".Length;
                    int tagEnd = json.IndexOf("\"", tagStart);
                    if (tagEnd > tagStart)
                    {
                        string tag = json.Substring(tagStart, tagEnd - tagStart).Trim();
                        Debug.WriteLine($"[AppUpdater] Found latest tag_name: {tag}");
                        return tag;
                    }
                }

                Debug.WriteLine("[AppUpdater] tag_name not found in release JSON");
                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AppUpdater] Error in GetLatestReleaseVersionAsync: {ex.Message}");
                return null;
            }
        }

        private static async Task<string> ReadVersionFromUpdateAsync(string tempDir)
        {
            try
            {
                string versionPath = Path.Combine(tempDir, ".service", "version.txt");
                if (File.Exists(versionPath))
                {
                    string version = await File.ReadAllTextAsync(versionPath);
                    return version.Trim();
                }

                // Или ищем в корне архива
                versionPath = Path.Combine(tempDir, "version.txt");
                if (File.Exists(versionPath))
                {
                    string version = await File.ReadAllTextAsync(versionPath);
                    return version.Trim();
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

    }
}