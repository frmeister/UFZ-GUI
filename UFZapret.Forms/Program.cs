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
        private static string logFilePath = "app_startup.log";

        [STAThread]
        static void Main(string[] args)
        {
            // Создаем мьютекс для предотвращения запуска нескольких копий
            bool createdNew;
            mutex = new Mutex(true, "UFZapret.Forms.SingleInstance", out createdNew);

            string isFirstLaunch, pathOrigin, currentConfig, autoStart;

            if (!createdNew)
            {
                // В авто-запуске не показываем сообщение, просто выходим
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 1. ОПРЕДЕЛЯЕМ РЕЖИМ ЗАПУСКА ПО ФАКТУЧЕСКИМ АРГУМЕНТАМ
            bool startMinimized = args.Contains("--minimized");

            // 2. Показываем сплеш-экран сразу для любой загрузки
            using (var splash = new FormSplash())
            {
                splash.Show();
                Application.DoEvents();

                // 3. Устанавливаем ПРАВИЛЬНУЮ рабочую директорию
                string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
                string appDirectory = Path.GetDirectoryName(exePath);
                Directory.SetCurrentDirectory(appDirectory);

                // 4. Инициализируем ConfigManager с правильным путем
                ConfigManager.Initialize(appDirectory);

                DateTime startTime = DateTime.Now;
                int maxWaitSeconds = 10;
                bool configLoaded = false;
                string status = "";

                // 5. Ждем загрузки конфига
                while (!configLoaded && (DateTime.Now - startTime).TotalSeconds < maxWaitSeconds)
                {
                    int secondsLeft = maxWaitSeconds - (int)(DateTime.Now - startTime).TotalSeconds;

                    // Проверяем ключевые параметры
                    isFirstLaunch = ConfigManager.GetValue("isThisFirstLaunch", "true");
                    pathOrigin = ConfigManager.GetValue("pathOrigin", "none");
                    currentConfig = ConfigManager.GetValue("currentConfig", "none");
                    autoStart = ConfigManager.GetValue("autoStart", "false");

                    status = $"Загрузка: first={isFirstLaunch}, auto={autoStart} ({secondsLeft} сек)";
                    splash.UpdateStatus(status);

                    // Конфиг считается загруженным, если мы можем получить значение autoStart
                    configLoaded = (autoStart == "true" || autoStart == "false");

                    Thread.Sleep(200);
                    Application.DoEvents();
                }

                splash.Close();
            }

            // 6. ПОСЛЕ загрузки конфига определяем, что делать
            bool isAutoStart = ConfigManager.IsAutoStartEnabled();
            isFirstLaunch = ConfigManager.GetValue("isThisFirstLaunch", "true");
            pathOrigin = ConfigManager.GetValue("pathOrigin", "none");
            currentConfig = ConfigManager.GetValue("currentConfig", "none");

            // 7. ОСНОВНАЯ ЛОГИКА ЗАПУСКА

            // Вариант A: Авто-запуск из Windows (даже без аргументов!)
            bool isWindowsAutoStart = !startMinimized && isAutoStart && Environment.CurrentDirectory.Contains("system32");

            // Вариант B: Явный авто-запуск с аргументом --minimized
            bool isExplicitAutoStart = startMinimized && isAutoStart;

            // Если это авто-запуск (любой вариант)
            if (isWindowsAutoStart || isExplicitAutoStart)
            {
                // Показываем FormMain сразу, даже если это первый запуск
                // Авто-запуск не должен показывать FormEntrance
                var mainForm = new FormMain(true);
                Application.Run(mainForm);
                mutex?.ReleaseMutex();
                return;
            }

            // 8. Обычный запуск (не авто-запуск)
            bool showEntrance = (isFirstLaunch == "true") || (pathOrigin == "none" && currentConfig == "none");

            FormMain mainForm2 = null;

            if (showEntrance)
            {
                using (var formEntrance = new FormEntrance())
                {
                    if (formEntrance.ShowDialog() == DialogResult.OK)
                    {
                        ConfigManager.SetValue("isThisFirstLaunch", "false");
                        mainForm2 = new FormMain(false);
                    }
                    else
                    {
                        mutex?.ReleaseMutex();
                        return;
                    }
                }
            }
            else
            {
                mainForm2 = new FormMain(false);
            }

            if (mainForm2 != null)
            {
                Application.Run(mainForm2);
            }

            mutex?.ReleaseMutex();

            LogToFile("Application exit completed");
    }

        private static void LogToFile(string message)
        {
            try
            {
                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] {message}\n";
                File.AppendAllText(logFilePath, logEntry);
                Debug.WriteLine(message);
            }
            catch
            {
                // Игнорируем ошибки логирования
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

        // Класс сплеш-экрана
        public class FormSplash : Form
        {
            private Label label;

            public FormSplash()
            {
                this.FormBorderStyle = FormBorderStyle.None;
                this.StartPosition = FormStartPosition.CenterScreen;
                this.TopMost = true;
                this.Size = new Size(400, 120);
                this.BackColor = Color.LightBlue;
                this.ShowInTaskbar = false;

                label = new Label
                {
                    Text = "Загрузка конфигурации...",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Arial", 10)
                };

                this.Controls.Add(label);
            }

            public void UpdateStatus(string message)
            {
                if (label.InvokeRequired)
                    label.Invoke(new Action(() => label.Text = message));
                else
                    label.Text = message;
            }
        }
    }
}