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
            // Временный лог для отладки авто-запуска
            string debugLogPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "ufzapret_debug.txt");
            File.AppendAllText(debugLogPath, $"[{DateTime.Now}] Запуск с аргументами: {string.Join(" ", args)}\n");
            File.AppendAllText(debugLogPath, $"[{DateTime.Now}] Текущая директория: {Environment.CurrentDirectory}\n");

            string isFirstLaunch, pathOrigin, currentConfig;
            bool startMinimized;

            // Создаем мьютекс для предотвращения запуска нескольких копий
            bool createdNew;
            mutex = new Mutex(true, "UFZapret.Forms.SingleInstance", out createdNew);

            if (!createdNew)
            {
                File.AppendAllText(debugLogPath, $"[{DateTime.Now}] Приложение уже запущено\n");
                MessageBox.Show("Приложение уже запущено!", "UFZapret",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Сразу покажем сплеш-экран при любом запуске для диагностики
            using (var splash = new FormSplash())
            {
                splash.Show();
                Application.DoEvents();

                File.AppendAllText(debugLogPath, $"[{DateTime.Now}] Показан сплеш-экран\n");

                // Ждем 2 секунды для отладки
                splash.UpdateStatus("Диагностика...");
                Thread.Sleep(2000);

                // Инициализируем ConfigManager
                splash.UpdateStatus("Загрузка конфигурации...");
                ConfigManager.Initialize();

                // Проверяем аргументы
                startMinimized = args.Contains("--minimized");
                bool isAutoStart = ConfigManager.IsAutoStartEnabled();

                File.AppendAllText(debugLogPath, $"[{DateTime.Now}] startMinimized: {startMinimized}, isAutoStart: {isAutoStart}\n");
                File.AppendAllText(debugLogPath, $"[{DateTime.Now}] Проверка реестра авто-запуска: {AutoStartManager.IsEnabled()}\n");

                splash.UpdateStatus($"Аргументы: {string.Join(" ", args)}");
                Thread.Sleep(2000);

                splash.Close();
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