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
            // Создаем мьютекс для предотвращения запуска нескольких копий
            bool createdNew;
            mutex = new Mutex(true, "UFZapret.Forms.SingleInstance", out createdNew);

            if (!createdNew)
            {
                MessageBox.Show("Приложение уже запущено!", "UFZapret",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DataService ds = new DataService();

            ConfigManager.LoadConfig();

            bool isConfigValid = ConfigManager.CheckConfigFileExistsAndValid();
            bool startMinimized = args.Contains("--minimized");

            // Если это автозапуск и конфиг невалиден, ждем в сплеш-скрине
            if (startMinimized && !isConfigValid)
            {
                using (var splash = new FormSplash())
                {
                    splash.Show();
                    Application.DoEvents(); // Обновляем UI

                    DateTime startTime = DateTime.Now;
                    int maxWaitSeconds = 10; // Максимум 10 секунд ожидания

                    // Ждем, пока конфиг станет валидным
                    while (!isConfigValid && (DateTime.Now - startTime).TotalSeconds < maxWaitSeconds)
                    {
                        splash.UpdateStatus($"Загрузка... {maxWaitSeconds - (int)(DateTime.Now - startTime).TotalSeconds} сек");
                        Application.DoEvents();
                        Thread.Sleep(200); // Проверяем каждые 200 мс

                        // Перезагружаем конфиг и обновляем флаг
                        ConfigManager.LoadConfig();
                        isConfigValid = ConfigManager.CheckConfigFileExistsAndValid(); // Это ключевая строка!
                    }

                    // Закрываем сплеш-скрин
                    splash.Close();
                }
            }

            // ПЕРЕЗАГРУЖАЕМ конфиг ПОСЛЕ ожидания (важно!)
            ConfigManager.LoadConfig();
            isConfigValid = ConfigManager.CheckConfigFileExistsAndValid(); // Обновляем состояние после ожидания

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

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