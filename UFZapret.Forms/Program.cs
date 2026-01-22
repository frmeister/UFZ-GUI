using System.Diagnostics;
using System.Threading;
using UFZ.Lib;

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
            // Начинаем логирование
            LogToFile("========================================");
            LogToFile($"[{DateTime.Now}] Application starting");
            LogToFile($"Args: {string.Join(" ", args)}");
            LogToFile($"Working directory: {Environment.CurrentDirectory}");

            string isFirstLaunch = null;
            string pathOrigin = null;
            string currentConfig = null;

            // Создаем мьютекс для предотвращения запуска нескольких копий
            bool createdNew;
            mutex = new Mutex(true, "UFZapret.Forms.SingleInstance", out createdNew);

            if (!createdNew)
            {
                LogToFile("Application already running, exiting");
                MessageBox.Show("Приложение уже запущено!", "UFZapret",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Инициализируем ConfigManager сразу
            LogToFile("Initializing ConfigManager...");
            ConfigManager.Initialize();
            LogToFile("ConfigManager initialized");

            // Определяем режим запуска
            bool startMinimized = args.Contains("--minimized");
            bool isAutoStart = ConfigManager.IsAutoStartEnabled();

            LogToFile($"startMinimized: {startMinimized}");
            LogToFile($"autoStart from config: {isAutoStart}");

            // Если это автозапуск, но autoStart=false в конфиге, выходим
            if (startMinimized && !isAutoStart)
            {
                LogToFile("Auto-start disabled in config, exiting");
                mutex?.ReleaseMutex();
                return;
            }

            // Проверяем, нужен ли сплеш-экран для автозапуска
            if (startMinimized)
            {
                LogToFile("Auto-start mode detected, showing splash screen");

                using (var splash = new FormSplash())
                {
                    splash.Show();
                    Application.DoEvents();

                    DateTime startTime = DateTime.Now;
                    int maxWaitSeconds = 10;
                    bool configValid = false;

                    // Ждем, пока конфиг станет полностью загруженным
                    while (!configValid && (DateTime.Now - startTime).TotalSeconds < maxWaitSeconds)
                    {
                        int secondsLeft = maxWaitSeconds - (int)(DateTime.Now - startTime).TotalSeconds;
                        splash.UpdateStatus($"Загрузка конфигурации... {secondsLeft} сек");
                        Application.DoEvents();
                        Thread.Sleep(200);

                        // Перезагружаем конфиг
                        ConfigManager.Reload();

                        // Проверяем ключевые параметры
                        isFirstLaunch = ConfigManager.GetValue("isThisFirstLaunch", "true");
                        pathOrigin = ConfigManager.GetValue("pathOrigin", "none");
                        currentConfig = ConfigManager.GetValue("currentConfig", "none");

                        LogToFile($"Check: isFirstLaunch={isFirstLaunch}, pathOrigin={pathOrigin}, currentConfig={currentConfig}");

                        // Конфиг считается валидным, если он загружен (isFirstLaunch имеет значение)
                        configValid = (isFirstLaunch != "true" || (pathOrigin != "none" && currentConfig != "none"));

                        LogToFile($"Config valid: {configValid}");
                    }

                    splash.Close();
                }
            }

            // Проверяем, нужно ли показывать приветственный экран
            isFirstLaunch = ConfigManager.GetValue("isThisFirstLaunch", "true");
            pathOrigin = ConfigManager.GetValue("pathOrigin", "none");
            currentConfig = ConfigManager.GetValue("currentConfig", "none");

            LogToFile($"Final check: isFirstLaunch={isFirstLaunch}, pathOrigin={pathOrigin}, currentConfig={currentConfig}");

            bool showEntrance = (isFirstLaunch == "true") || (pathOrigin == "none" && currentConfig == "none");
            LogToFile($"Show entrance form: {showEntrance}");

            FormMain mainForm = null;

            if (showEntrance)
            {
                LogToFile("Showing FormEntrance...");

                // Если это авто-запуск, не показываем FormEntrance, просто запускаем FormMain
                if (startMinimized)
                {
                    LogToFile("Auto-start mode: bypassing FormEntrance, creating FormMain minimized");
                    mainForm = new FormMain(true);
                }
                else
                {
                    // Показываем приветственное окно как диалог
                    using (var formEntrance = new FormEntrance())
                    {
                        if (formEntrance.ShowDialog() == DialogResult.OK)
                        {
                            // Сохраняем, что это уже не первый запуск
                            ConfigManager.SetValue("isThisFirstLaunch", "false");
                            LogToFile("FormEntrance completed successfully");

                            // Конфиг теперь должен быть валидным, создаем главную форму
                            mainForm = new FormMain(startMinimized);
                        }
                        else
                        {
                            // Пользователь отменил
                            LogToFile("FormEntrance cancelled by user");
                            mutex?.ReleaseMutex();
                            return;
                        }
                    }
                }
            }
            else
            {
                LogToFile("Creating FormMain directly...");
                // Обычный запуск
                mainForm = new FormMain(startMinimized);
            }

            // Подписываемся на события закрытия приложения
            Application.ApplicationExit += OnApplicationExit;
            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            // Запускаем приложение
            if (mainForm != null)
            {
                LogToFile("Starting application...");
                try
                {
                    Application.Run(mainForm);
                    LogToFile("Application run completed");
                }
                catch (Exception ex)
                {
                    LogToFile($"Critical error: {ex.Message}\n{ex.StackTrace}");
                    MessageBox.Show($"Критическая ошибка: {ex.Message}\n\n{ex.StackTrace}",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    // Гарантированная остановка при выходе
                    LogToFile("Application exiting, cleaning up...");
                    ZapretService.ForceStop();
                    mutex?.ReleaseMutex();
                }
            }
            else
            {
                LogToFile("Main form not created, exiting");
                mutex?.ReleaseMutex();
            }

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