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

            foreach (var sourceFile in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
            {
                string relativePath = Path.GetRelativePath(sourceDir, sourceFile);
                string targetFile = Path.Combine(targetDir, relativePath);

                // Пропускаем исключенные файлы
                if (excludeFiles.Any(f => relativePath.EndsWith(f, StringComparison.OrdinalIgnoreCase)))
                {
                    Debug.WriteLine($"[AppUpdater] Пропускаем: {relativePath}");
                    continue;
                }

                // Создаем директорию, если нужно
                string targetDirPath = Path.GetDirectoryName(targetFile);
                if (!Directory.Exists(targetDirPath))
                    Directory.CreateDirectory(targetDirPath);

                // Копируем файл
                Debug.WriteLine($"[AppUpdater] Обновляем: {relativePath}");
                File.Copy(sourceFile, targetFile, true);
            }
        }

        // Получение ссылки на архив релиза
        public static async Task<string> GetLatestReleaseDownloadUrl()
        {
            try
            {
                string apiUrl = "https://api.github.com/repos/frmeister/UFZ-GUI/releases/latest";
                var response = await _httpClient.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                    return null;

                var json = await response.Content.ReadAsStringAsync();

                // Ищем asset с zip архивом
                if (json.Contains("\"browser_download_url\""))
                {
                    int start = json.IndexOf("\"browser_download_url\":\"") + "\"browser_download_url\":\"".Length;
                    int end = json.IndexOf("\"", start);
                    if (start > 0 && end > start)
                    {
                        string url = json.Substring(start, end - start);
                        if (url.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
                            return url;
                    }
                }

                return null;
            }
            catch
            {
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