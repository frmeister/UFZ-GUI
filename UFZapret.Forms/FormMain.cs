using System.Diagnostics;
using UFZ.Lib;
using UFZapret.Lib;

namespace UFZapret.Forms
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();

            CheckStatusBar();
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            LaunchSelectedBat();
        }

        private void buttonConfiguration_Click(object sender, EventArgs e)
        {
            FormConfiguration formconfig = new FormConfiguration();
            formconfig.ShowDialog();
        }

        #region BAT EXECUTABLE

        // Метод для запуска выбранного .bat файла
        public static bool LaunchSelectedBat()
        {
            try
            {
                // 1. Получаем путь к выбранному конфигу из Config.cfg
                string folderPath = ConfigManager.GetValue("pathOrigin", "none");
                string configName = ConfigManager.GetValue("currentConfig", "none");
                string configPath = folderPath+'\\'+configName; // Нужно добавить этот метод

                if (string.IsNullOrEmpty(configPath) || configPath == "none")
                {
                    MessageBox.Show("Конфиг не выбран! Настройте конфигурацию.", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }


                if (!File.Exists(configPath))
                {
                MessageBox.Show($"Файл не найден:\n{configPath}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
                }

                // 2. Запускаем .bat файл
                return LaunchBatFile(configPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка запуска:\n{ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        // Метод для запуска конкретного .bat файла
        public static bool LaunchBatFile(string batFilePath, string workingDirectory = null)
        {
            try
            {
                // Проверяем существование файла
                if (!File.Exists(batFilePath))
                {
                    MessageBox.Show($"Файл не найден:\n{batFilePath}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                // Настройки процесса
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c \"{batFilePath}\"",
                    WorkingDirectory = workingDirectory ?? Path.GetDirectoryName(batFilePath),
                    CreateNoWindow = false,          // Не показывать окно CMD
                    UseShellExecute = false,        // Не использовать системную оболочку
                    RedirectStandardOutput = true,  // Перенаправляем вывод
                    RedirectStandardError = true,   // Перенаправляем ошибки
                    // WindowStyle = ProcessWindowStyle.Hidden // Скрыть окно
                };

                // Создаем и запускаем процесс
                using (Process process = new Process())
                {
                    process.StartInfo = startInfo;

                    // Подписываемся на события вывода
                    process.OutputDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                            Console.WriteLine($"[Zapret] {e.Data}");
                    };

                    process.ErrorDataReceived += (sender, e) =>
                    {
                        if (!string.IsNullOrEmpty(e.Data))
                            Console.WriteLine($"[Zapret ERROR] {e.Data}");
                    };

                    // Запускаем процесс
                    process.Start();

                    // Начинаем асинхронное чтение вывода
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    // Можно подождать немного или запустить асинхронно
                    // process.WaitForExit(); // Блокирующий вызов

                    MessageBox.Show($"Zapret запущен:\n{Path.GetFileName(batFilePath)}",
                        "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);



                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка запуска .bat файла:\n{ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        #endregion

        #region STATUS

        private void CheckStatusBar()
        {
            string directory = ConfigManager.GetValue("pathOrigin", "none");
            string config = ConfigManager.GetValue("currentConfig", "none");

            if (directory != "none")
            {
                if (config != "none")
                {
                    textBoxStatus.Text += "Ready!";
                }
                else
                {
                    textBoxStatus.Text += "Config = none";
                }
            }
            else
            {
                textBoxStatus.Text += "Directory = none";
            }
        }

        #endregion
    }
}
