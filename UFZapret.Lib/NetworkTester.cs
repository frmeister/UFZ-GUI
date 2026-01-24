using System;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace UFZapret.Lib
{
    public static class NetworkTester
    {
        // Проверка доступности через HTTP (самый надежный способ)
        public static async Task<bool> TestWebsiteAccessAsync(string url = "https://www.youtube.com", int timeoutMs = 5000)
        {
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "HEAD";
                request.Timeout = timeoutMs;
                request.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36";

                using (var response = (HttpWebResponse)await request.GetResponseAsync())
                {
                    return response.StatusCode == HttpStatusCode.OK;
                }
            }
            catch (WebException ex)
            {
                // Для отладки можно логировать ошибки
                Debug.WriteLine($"[NetworkTester] WebException: {ex.Status} - {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[NetworkTester] Exception: {ex.Message}");
                return false;
            }
        }

        // Проверка через ping (быстрее, но может быть заблокирован)
        public static async Task<bool> TestPingAsync(string host = "8.8.8.8", int timeoutMs = 2000)
        {
            try
            {
                using (var ping = new Ping())
                {
                    var reply = await ping.SendPingAsync(host, timeoutMs);
                    return reply.Status == IPStatus.Success;
                }
            }
            catch
            {
                return false;
            }
        }

        // Комплексная проверка через несколько методов
        public static async Task<bool> TestNetworkConnectivityAsync()
        {
            // Пробуем несколько тестов для надежности
            var testUrls = new[]
            {
                "https://www.youtube.com",
                "https://www.google.com",
                "https://www.github.com"
            };

            // Сначала пробуем ping (быстро)
            if (await TestPingAsync("8.8.8.8", 1500))
                return true;

            // Пробуем разные сайты
            foreach (var url in testUrls)
            {
                if (await TestWebsiteAccessAsync(url, 3000))
                    return true;
            }

            return false;
        }
    }
}