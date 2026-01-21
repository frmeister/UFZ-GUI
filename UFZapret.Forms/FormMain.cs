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

            AutoStartManager.SyncWithConfig();

            string[] args = Environment.GetCommandLineArgs();
            bool startMinimized = args.Contains("--minimized");

            if (startMinimized)
            {
                this.Load += (s, e) => {
                    MinimizeToTray();
                };
            }

            CheckIsConfigAvalible();

            InitializeTrayIcon();

            CheckAutoStartStatus();

            this.FormClosing += FormMain_FormClosing;
            this.Resize += FormMain_Resize;
        }

        #region TRAY

        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenu;

        private void InitializeTrayIcon()
        {
            // Создаем контекстное меню для трея
            trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("Развернуть", null, OnTrayRestore);
            trayMenu.Items.Add(new ToolStripSeparator());
            trayMenu.Items.Add("Выход", null, OnTrayExit);

            // Создаем иконку в трее
            trayIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application, // Можно заменить на свою иконку
                Text = "UFZapret",
                ContextMenuStrip = trayMenu,
                Visible = false // Изначально скрыта
            };

            // Обработка кликов по иконке
            trayIcon.DoubleClick += (s, e) => RestoreFromTray();
            trayIcon.MouseClick += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    RestoreFromTray();
                }
            };
        }

        private void RestoreFromTray()
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.ShowInTaskbar = true;
            trayIcon.Visible = false;
            this.Activate(); // Активируем окно
        }

        private void MinimizeToTray()
        {
            this.Hide();
            this.ShowInTaskbar = false;
            trayIcon.Visible = true;
        }

        private void OnTrayRestore(object sender, EventArgs e)
        {
            RestoreFromTray();
        }

        private bool isExitingFromTray = false;

        private async void OnTrayExit(object sender, EventArgs e)
        {
            isExitingFromTray = true;

            // Останавливаем Zapret если запущен
            if (ZapretService.IsRunning)
            {
                var result = MessageBox.Show(
                    "Zapret в настоящее время запущен.\n\n" +
                    "Остановить zapret и выйти?",
                    "Выход",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    UpdateStatus("Останавливаем zapret...");
                    await ZapretService.Stop();
                }
                // Если "Нет", то выходим, оставляя Zapret работать
            }

            // Корректно закрываем приложение
            trayIcon.Visible = false;
            Application.Exit();
        }

        #endregion

        #region BUTTONS

        private void buttonSettings_Click(object sender, EventArgs e)
        {
            FormSettings formSettings = new FormSettings();
            formSettings.ShowDialog();
        }

        private async void buttonStart_Click(object sender, EventArgs e)
        {
            buttonStart.Enabled = false;

            try
            {
                string folderPath = ConfigManager.GetValue("pathOrigin", "none");
                string configName = ConfigManager.GetValue("currentConfig", "none");

                if (folderPath == "none" || configName == "none")
                {
                    MessageBox.Show("Сначала настройте конфигурацию!", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Используем единый сервис
                bool success = await ZapretService.Toggle(folderPath, configName);

                if (success)
                {
                    if (ZapretService.IsRunning)
                    {
                        UpdateStatus("Zapret запущен");
                        buttonStart.Text = "Остановить Zapret";
                    }
                    else
                    {
                        UpdateStatus("Zapret остановлен");
                        buttonStart.Text = "Запустить Zapret";
                    }
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Ошибка: {ex.Message}");
            }
            finally
            {
                buttonStart.Enabled = true;
            }
        }
        private void buttonConfiguration_Click(object sender, EventArgs e)
        {
            FormConfiguration formconfig = new FormConfiguration();
            formconfig.ShowDialog();
        }

        #endregion

        #region STATUS

        // MAIN STATUS FUNC
        private void UpdateStatus(string message)
        {
            textBoxStatus.Text = message;
        }

        public void CheckIsConfigAvalible()
        {
            string directory = ConfigManager.GetValue("pathOrigin", "none");
            string config = ConfigManager.GetValue("currentConfig", "none");

            if (directory != "none")
            {
                if (config != "none")
                {
                    UpdateStatus("Ready!");

                    // Enabling start button on ready status
                    buttonStart.Enabled = true;
                }
                else
                {
                    UpdateStatus("Config = none");
                }
            }
            else
            {
                UpdateStatus("Directory = none");
            }
        }

        private void CheckAutoStartStatus()
        {
            bool autoStartInConfig = DataService.GetAutoStart();
            bool autoStartInRegistry = AutoStartManager.IsEnabled();

            // Если настройки не совпадают, обновляем Config
            if (autoStartInConfig != autoStartInRegistry)
            {
                Debug.WriteLine($"Расхождение в автозапуске: Config={autoStartInConfig}, Registry={autoStartInRegistry}");
                DataService.SetAutoStart(autoStartInRegistry);
            }
        }

        #endregion

        #region LOGIC

        private async void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Если уже выходим из трея, пропускаем
            if (isExitingFromTray) return;

            // Если Zapret запущен, показываем предупреждение
            if (ZapretService.IsRunning)
            {
                var result = MessageBox.Show(
                    "Zapret в настоящее время запущен.\n\n" +
                    "Остановить zapret и выйти?\n" +
                    "• Да - остановить zapret и выйти\n" +
                    "• Нет - выйти, оставив zapret работать\n" +
                    "• Отмена - остаться в приложении",
                    "Подтверждение выхода",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button3); // По умолчанию "Отмена"

                switch (result)
                {
                    case DialogResult.Yes:
                        UpdateStatus("Останавливаем zapret...");
                        this.Enabled = false;

                        try
                        {
                            // Останавливаем с таймаутом
                            var stopTask = ZapretService.Stop();
                            var timeoutTask = Task.Delay(3000);

                            var completedTask = await Task.WhenAny(stopTask, timeoutTask);

                            if (completedTask == timeoutTask)
                            {
                                ZapretService.ForceStop();
                            }
                        }
                        finally
                        {
                            this.Enabled = true;
                        }

                        // Закрываем иконку трея
                        trayIcon.Visible = false;
                        break;

                    case DialogResult.No:
                        // Просто выходим, оставляя zapret работать
                        trayIcon.Visible = false;
                        break;

                    case DialogResult.Cancel:
                        e.Cancel = true;
                        UpdateStatus("Закрытие отменено");
                        return;
                }
            }
            else
            {
                // Zapret не запущен - просто закрываем
                trayIcon.Visible = false;
            }
        }

        private void TrayStartup()
        {
            string[] args = Environment.GetCommandLineArgs();
            bool startMinimized = args.Contains("--minimized");

            if (startMinimized)
            {
                this.Load += (s, e) => {
                    // Сразу сворачиваем в трей
                    MinimizeToTray();
                };
            }
        }

        private void FormMain_Resize(object sender, EventArgs e)
        {
            // Сворачиваем в трей при нажатии на кнопку "минус"
            if (this.WindowState == FormWindowState.Minimized)
            {
                MinimizeToTray();
            }
        }

        #endregion


    }
}
