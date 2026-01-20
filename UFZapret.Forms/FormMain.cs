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

            CheckIsConfigAvalible();

            InitializeTrayIcon();

            this.FormClosing += FormMain_FormClosing;
        }

        #region TRAY

        private NotifyIcon trayIcon;
        private ContextMenuStrip trayMenu;

        private void InitializeTrayIcon()
        {
            // Создаем контекстное меню для трея
            trayMenu = new ContextMenuStrip();
            trayMenu.Items.Add("Развернуть", null, OnTrayRestore);
            trayMenu.Items.Add("Выход", null, OnTrayExit);

            // Создаем иконку в трее
            trayIcon = new NotifyIcon
            {
                Icon = SystemIcons.Application, // Можно заменить на свою иконку
                Text = "UFZapret",
                ContextMenuStrip = trayMenu,
                Visible = true
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
            trayIcon.Visible = false; // Скрываем иконку при восстановлении
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

        private async void OnTrayExit(object sender, EventArgs e)
        {
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
                    await ZapretService.Stop();
                }
                // Если "Нет", то выходим, оставляя Zapret работать
            }

            // Корректно закрываем приложение
            trayIcon.Visible = false;
            trayIcon.Dispose();
            Application.Exit();
        }

        

        private bool isExitingFromTray = false;

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

        #endregion

        #region LOGIC

        private async void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Отменяем стандартное закрытие если не это не выход из трея
            if (e.CloseReason == CloseReason.UserClosing && !isExitingFromTray)
            {
                e.Cancel = true;
                MinimizeToTray();
                UpdateStatus("Приложение свернуто в трей");
                return;
            }

            // Если Zapret запущен, предлагаем остановить
            if (ZapretService.IsRunning)
            {
                var result = MessageBox.Show(
                    "Zapret в настоящее время запущен.\n\n" +
                    "Остановить zapret перед выходом?",
                    "Остановить zapret?",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                switch (result)
                {
                    case DialogResult.Yes:
                        UpdateStatus("Останавливаем zapret...");
                        await ZapretService.Stop();
                        break;
                    case DialogResult.No:
                        // Оставляем Zapret работать
                        break;
                    case DialogResult.Cancel:
                        e.Cancel = true;
                        UpdateStatus("Закрытие отменено");
                        return;
                }
            }
        }

        #endregion

        
    }
}
