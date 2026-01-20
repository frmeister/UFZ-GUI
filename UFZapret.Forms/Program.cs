using System.Diagnostics;
using System.Threading;
using UFZ.Lib;
using UFZapret.Lib;

namespace UFZapret.Forms
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        /// 

        private static Mutex mutex;

        [STAThread]
        static void Main()
        {
            DataService ds = new DataService();

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

            // ПРОВЕРКА ПЕРВОГО ЗАПУСКА ТОЛЬКО ЗДЕСЬ!
            bool isFirstLaunch = ds.IsFirstLaunch();

            if (isFirstLaunch)
            {
                // Показываем приветственное окно как диалог
                using (var formEntrance = new FormEntrance())
                {
                    if (formEntrance.ShowDialog() == DialogResult.OK)
                    {
                        // Сохраняем, что это уже не первый запуск
                        ConfigManager.SetValue("isThisFirstLaunch", "false");

                        // Запускаем главное окно
                        Application.Run(new FormMain());
                    }
                    else
                    {
                        // Пользователь отменил (например, нажал крестик)
                        Application.Exit();
                    }
                }
            }
            else
            {
                // Обычный запуск
                Application.Run(new FormMain());
            }

            // Подписываемся на события закрытия приложения
            Application.ApplicationExit += OnApplicationExit;
            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            try
            {
                Application.Run(new FormMain());
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

        private static void OnApplicationExit(object sender, EventArgs e)
        {
            Debug.WriteLine("=== ApplicationExit: Принудительная остановка Zapret ===");
            ZapretService.ForceStop();
        }

        private static void OnProcessExit(object sender, EventArgs e)
        {
            Debug.WriteLine("=== ProcessExit: Принудительная остановка Zapret ===");
            ZapretService.ForceStop();
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Debug.WriteLine($"=== UnhandledException: {e.ExceptionObject} ===");
            ZapretService.ForceStop();
        }
    }
}