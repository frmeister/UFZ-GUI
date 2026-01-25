using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace UFZapret.Lib
{
    public static class UpdateManager
    {
        private static readonly HttpClient _httpClient = new HttpClient();
        private const string ZapretRepo = "Flowseal/zapret-discord-youtube";
        private const string ApplicationRepo = "frmeister/UFZ-GUI";

        static UpdateManager()
        {
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "UFZapret-App");
            _httpClient.Timeout = TimeSpan.FromSeconds(10);
        }

        // Получение онлайн-версии (разная логика для zapret и GUI)
        public static async Task<string> GetOnlineVersionAsync(bool appOrOrigin)
        {
            try
            {
                if (appOrOrigin)
                {
                    // Для ZAPRET: читаем version.txt из репозитория
                    return await GetZapretVersionFromFileAsync();
                }
                else
                {
                    // Для GUI: получаем версию из GitHub Releases
                    return await GetGuiVersionFromReleasesAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[UpdateManager] Ошибка GetOnlineVersionAsync: {ex.Message}");
                return "error";
            }
        }

        // Для zapret: читаем version.txt из .service папки
        private static async Task<string> GetZapretVersionFromFileAsync()
        {
            try
            {
                string fileUrl = $"https://raw.githubusercontent.com/{ZapretRepo}/main/.service/version.txt";
                var response = await _httpClient.GetAsync(fileUrl);

                if (!response.IsSuccessStatusCode)
                    return "error";

                string version = await response.Content.ReadAsStringAsync();
                version = version.Trim();

                Debug.WriteLine($"[UpdateManager] Версия zapret: {version}");
                return CleanVersionString(version);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[UpdateManager] Ошибка zapret: {ex.Message}");
                return "error";
            }
        }

        // Для GUI: получаем версию из GitHub Releases API
        private static async Task<string> GetGuiVersionFromReleasesAsync()
        {
            try
            {
                string apiUrl = $"https://api.github.com/repos/{ApplicationRepo}/releases/latest";
                var response = await _httpClient.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"[UpdateManager] API вернул: {response.StatusCode}");
                    return "error";
                }

                var json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);

                // Получаем тег релиза (версию)
                if (doc.RootElement.TryGetProperty("tag_name", out var tagName))
                {
                    string version = tagName.GetString();
                    Debug.WriteLine($"[UpdateManager] Версия GUI из релиза: {version}");
                    return CleanVersionString(version);
                }

                return "unknown";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[UpdateManager] Ошибка GUI релиза: {ex.Message}");
                return "error";
            }
        }

        // Основная функция проверки обновлений
        public static async Task<bool> CheckUpdateNeededAsync(string localVersion, bool appOrOrigin)
        {
            try
            {
                if (string.IsNullOrEmpty(localVersion) || localVersion == "none")
                {
                    Debug.WriteLine($"[UpdateManager] Локальная версия не указана ({(appOrOrigin ? "zapret" : "app")})");
                    return false;
                }

                Debug.WriteLine($"[UpdateManager] Проверка обновлений {(appOrOrigin ? "zapret" : "app")}. Локальная: '{localVersion}'");

                string onlineVersion = await GetOnlineVersionAsync(appOrOrigin);

                if (onlineVersion == "error" || onlineVersion == "unknown")
                {
                    Debug.WriteLine($"[UpdateManager] Не удалось получить онлайн-версию");
                    return false;
                }

                Debug.WriteLine($"[UpdateManager] Сравниваем: '{localVersion}' vs '{onlineVersion}'");

                // Очищаем версии для сравнения
                localVersion = CleanVersionString(localVersion);
                onlineVersion = CleanVersionString(onlineVersion);

                bool needUpdate = !string.Equals(localVersion, onlineVersion,
                    StringComparison.OrdinalIgnoreCase);

                Debug.WriteLine($"[UpdateManager] Нужно обновление: {needUpdate}");
                return needUpdate;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[UpdateManager] Исключение: {ex.Message}");
                return false;
            }
        }

        private static string CleanVersionString(string version)
        {
            if (string.IsNullOrEmpty(version))
                return version;

            version = version.Trim();

            // Убираем "v" в начале, если есть
            if (version.StartsWith("v", StringComparison.OrdinalIgnoreCase))
            {
                return version.Substring(1);
            }

            return version;
        }
    }
}