using System.Diagnostics;
using System.Threading;
using UFZ.Lib;

namespace UFZapret.Forms
{
    internal static class Program
    {
        private static Mutex mutex;
        private static bool forceStopCalled = false;

        [STAThread]
        static void Main(string[] args)
        {
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

            // Логируем аргументы для отладки
            string argsStr = string.Join(" ", args);
            Debug.WriteLine($"[Program] Started with args: {argsStr}");
            Debug.WriteLine($"[Program] Working directory: {Environment.CurrentDirectory}");

            // 1. Определяем режим запуска
            bool startMinimized = args.Contains("--minimized");
            bool isAutoStartMode = startMinimized;

            Debug.WriteLine($"[Program] startMinimized: {startMinimized}, isAutoStartMode: {isAutoStartMode}");

            // 2. Инициализируем ConfigManager с ожиданием если это авто-запуск
            if (isAutoStartMode)
            {
                // Для авто-запуска: инициализируем с ожиданием
                Debug.WriteLine("[Program] Auto-start mode detected, initializing ConfigManager with wait...");

                // Показываем сплеш-экран пока загружаем конфиг
                using (var splash = new FormSplash())
                {
                    splash.Show();
                    Application.DoEvents();

                    splash.UpdateStatus("Загрузка конфигурации...");

                    // Инициализируем с несколькими попытками
                    bool configLoaded = false;
                    for (int i = 0; i < 10; i++)
                    {
                        ConfigManager.Initialize(waitForConfig: true);
                        configLoaded = ConfigManager.WaitForInitialization(1000);

                        if (configLoaded)
                        {
                            Debug.WriteLine($"[Program] Config loaded successfully on attempt {i + 1}");
                            break;
                        }

                        splash.UpdateStatus($"Загрузка конфигурации... (попытка {i + 1}/10)");
                        Thread.Sleep(200);
                        Application.DoEvents();
                    }

                    splash.Close();

                    if (!configLoaded)
                    {
                        Debug.WriteLine("[Program] Failed to load config, exiting");
                        mutex?.ReleaseMutex();
                        return;
                    }
                }
            }
            else
            {
                // Обычный запуск: просто инициализируем
                ConfigManager.Initialize();
            }

            // 3. Проверяем автозапуск в конфиге
            bool autoStartEnabled = ConfigManager.IsAutoStartEnabled();
            Debug.WriteLine($"[Program] autoStartEnabled from config: {autoStartEnabled}");

            // 4. Проверяем, нужно ли показывать FormEntrance
            // Проверяем только КЛЮЧЕВЫЕ параметры для авто-запуска
            string pathOrigin = ConfigManager.GetValue("pathOrigin", "none");
            string currentConfig = ConfigManager.GetValue("currentConfig", "none");

            Debug.WriteLine($"[Program] pathOrigin: {pathOrigin}, currentConfig: {currentConfig}");

            // Определяем, нужно ли показывать приветственный экран
            // Показываем только если ОБА параметра равны "none"
            bool showEntrance = (pathOrigin == "none" && currentConfig == "none");

            Debug.WriteLine($"[Program] showEntrance: {showEntrance}");

            // 5. Логика для авто-запуска
            if (isAutoStartMode)
            {
                Debug.WriteLine("[Program] Auto-start mode processing...");

                if (!autoStartEnabled)
                {
                    Debug.WriteLine("[Program] Auto-start disabled in config, exiting");
                    mutex?.ReleaseMutex();
                    return;
                }

                // Для авто-запуска НЕ показываем FormEntrance, даже если конфиг не полный
                // Вместо этого запускаем главную форму свернутой
                showEntrance = false;
                Debug.WriteLine("[Program] Auto-start: bypassing FormEntrance");
            }

            // 6. Создаем главную форму
            FormMain mainForm = null;

            if (showEntrance)
            {
                Debug.WriteLine("[Program] Showing FormEntrance...");
                // Показываем приветственное окно как диалог
                using (var formEntrance = new FormEntrance())
                {
                    if (formEntrance.ShowDialog() == DialogResult.OK)
                    {
                        // После успешной настройки создаем главную форму
                        mainForm = new FormMain(startMinimized);
                        Debug.WriteLine("[Program] FormEntrance completed successfully");
                    }
                    else
                    {
                        // Пользователь отменил
                        Debug.WriteLine("[Program] FormEntrance was cancelled");
                        mutex?.ReleaseMutex();
                        return;
                    }
                }
            }
            else
            {
                Debug.WriteLine("[Program] Creating FormMain directly...");
                // Обычный запуск или авто-запуск
                mainForm = new FormMain(startMinimized);
            }

            // 7. Подписываемся на события закрытия приложения
            Application.ApplicationExit += OnApplicationExit;
            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            // 8. Запускаем приложение
            if (mainForm != null)
            {
                Debug.WriteLine("[Program] Starting application...");
                try
                {
                    Application.Run(mainForm);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[Program] Critical error: {ex}");
                    MessageBox.Show($"Критическая ошибка: {ex.Message}\n\n{ex.StackTrace}",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    // Гарантированная остановка при выходе
                    Debug.WriteLine("[Program] Application exiting...");
                    ZapretService.ForceStop();
                    mutex?.ReleaseMutex();
                }
            }
            else
            {
                Debug.WriteLine("[Program] No main form created, exiting");
                mutex?.ReleaseMutex();
            }
        }

        // Обработчики событий
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
                    Text = "Загрузка...",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Arial", 10)
                };

                this.Controls.Add(label);

                // Добавляем отладочную информацию
                var debugLabel = new Label
                {
                    Text = $"Директория: {Environment.CurrentDirectory}",
                    Dock = DockStyle.Bottom,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Arial", 8),
                    ForeColor = Color.Gray
                };

                this.Controls.Add(debugLabel);
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