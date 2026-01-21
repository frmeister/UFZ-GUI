using System.Diagnostics;
using System.Threading;
using UFZ.Lib;
using UFZapret.Lib;

namespace UFZapret.Forms
{
    internal static class Program
    {
        private static Mutex mutex;
        private static bool forceStopCalled = false;

        [STAThread]
        static void Main(string[] args)
        {
            DataService ds = new DataService();

            bool isConfigValid = IsConfigValid();
            bool startMinimized = args.Contains("--minimized");

            // Создаем мьютекс для предотвращения запуска нескольких копий
            bool createdNew;
            mutex = new Mutex(true, "UFZapret.Forms.SingleInstance", out createdNew);

            if (!createdNew)
            {
                MessageBox.Show("Приложение уже запущено!", "UFZapret",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            using (var splash = new FormSplash())
            {
                splash.Show();
                Application.DoEvents();

                // Загрузка конфига
                ConfigManager.LoadConfig();

                Thread.Sleep(1000); // Минимум 1 секунда для показа сплеша
            }

            // Подписываемся на события закрытия приложения
            Application.ApplicationExit += OnApplicationExit;
            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            FormMain mainForm = null;

            if (!isConfigValid)
            {
                // Показываем приветственное окно как диалог
                using (var formEntrance = new FormEntrance())
                {
                    if (formEntrance.ShowDialog() == DialogResult.OK)
                    {
                        // Сохраняем, что это уже не первый запуск
                        ConfigManager.SetValue("isThisFirstLaunch", "false");
                        // Конфиг теперь должен быть валидным, создаем главную форму
                        mainForm = new FormMain();
                    }
                    else
                    {
                        // Пользователь отменил (например, нажал крестик)
                        // Освобождаем мьютекс и выходим
                        mutex?.ReleaseMutex();
                        return;
                    }
                }
            }
            else
            {
                // Обычный запуск
                mainForm = new FormMain();
            }

            // Если форма создана, запускаем приложение
            if (mainForm != null)
            {
                try
                {
                    Application.Run(mainForm);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Критическая ошибка: {ex.Message}\n\n{ex.StackTrace}",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    // Гарантированная остановка при выходе
                    ZapretService.ForceStop();
                    mutex?.ReleaseMutex();
                }
            }
        }

        private static void OnApplicationExit(object sender, EventArgs e)
        {
            if (forceStopCalled) return;
            forceStopCalled = true;

            Debug.WriteLine("=== ApplicationExit: Принудительная остановка Zapret ===");
            ZapretService.ForceStop();
        }

        private static void OnProcessExit(object sender, EventArgs e)
        {
            if (forceStopCalled) return;
            forceStopCalled = true;

            Debug.WriteLine("=== ProcessExit: Принудительная остановка Zapret ===");
            ZapretService.ForceStop();
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Debug.WriteLine($"=== UnhandledException: {e.ExceptionObject} ===");
            ZapretService.ForceStop();
        }

        static bool IsConfigValid()
        {
            try
            {
                string pathOrigin = ConfigManager.GetValue("pathOrigin", "none");
                string currentConfig = ConfigManager.GetValue("currentConfig", "none");

                // Проверяем, что путь существует и конфиг выбран
                if (pathOrigin == "none" || currentConfig == "none")
                    return false;

                // Проверяем существование папки zapret
                if (!Directory.Exists(pathOrigin))
                    return false;

                // Проверяем существование файла конфига
                string configPath = Path.Combine(pathOrigin, currentConfig);
                if (!File.Exists(configPath))
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}