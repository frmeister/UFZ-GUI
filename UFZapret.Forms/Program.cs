using UFZ.Lib;
using UFZapret.Lib;

namespace UFZapret.Forms
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            DataService ds = new DataService();

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
        }
    }
}