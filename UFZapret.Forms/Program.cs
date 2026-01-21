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

            // 1. Инициализируем ConfigManager ПЕРВЫМ делом
            ConfigManager.Initialize();

            // 2. Проверяем флаги автозапуска
            bool startMinimized = args.Contains("--minimized");
            bool isAutoStart = ConfigManager.IsAutoStartEnabled();

            // 3. Если это автозапуск, но autoStart=false, выходим
            if (startMinimized && !isAutoStart)
            {
                mutex?.ReleaseMutex();
                return;
            }

            // 4. Если это первый запуск, показываем приветствие
            bool isFirstLaunch = ConfigManager.GetValue("isThisFirstLaunch", "true") == "true";

            if (isFirstLaunch)
            {
                ShowWelcomeScreen();
                // После приветствия устанавливаем флаг
                ConfigManager.SetValue("isThisFirstLaunch", "false");
            }

            // 5. Подписываемся на события закрытия приложения
            Application.ApplicationExit += OnApplicationExit;
            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            // 6. Создаем главную форму
            FormMain mainForm = new FormMain();

            // 7. Запускаем приложение
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

        private static void ShowWelcomeScreen()
        {
            using (var formEntrance = new FormEntrance())
            {
                if (formEntrance.ShowDialog() != DialogResult.OK)
                {
                    // Пользователь отменил, выходим
                    mutex?.ReleaseMutex();
                    Environment.Exit(0);
                }
            }
        }

        // Обработчики событий (оставляем без изменений)
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

        // Класс сплеш-экрана (оставляем на всякий случай)
        public class FormSplash : Form
        {
            private Label label;

            public FormSplash()
            {
                this.FormBorderStyle = FormBorderStyle.None;
                this.StartPosition = FormStartPosition.CenterScreen;
                this.TopMost = true;
                this.Size = new Size(300, 100);
                this.BackColor = Color.LightBlue;

                label = new Label
                {
                    Text = "Загрузка...",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Arial", 12)
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