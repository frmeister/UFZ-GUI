using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;

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

        // Получение онлайн-версии через файл version.txt
        public static async Task<string> GetOnlineVersionAsync(bool appOrOrigin)
        {
            try
            {
                string fileUrl = appOrOrigin ?
                    $"https://raw.githubusercontent.com/{ZapretRepo}/main/.service/version.txt" :
                    $"https://raw.githubusercontent.com/{ApplicationRepo}/main/.service/version.txt";

                var response = await _httpClient.GetAsync(fileUrl);

                if (!response.IsSuccessStatusCode)
                    return "error";

                string version = await response.Content.ReadAsStringAsync();
                version = version.Trim();

                Debug.WriteLine($"[UpdateManager] Онлайн-версия ({(appOrOrigin ? "zapret" : "app")}): {version}");
                return CleanVersionString(version);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[UpdateManager] Ошибка: {ex.Message}");
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

                if (onlineVersion == "error")
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