using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UFZ.Lib;
using UFZapret.Lib;
using static System.Net.Mime.MediaTypeNames;

namespace UFZapret.Forms
{
    public partial class FormSettings : Form
    {
        public FormSettings()
        {
            InitializeComponent();

            LoadSettings();

            // Показываем текущую версию
            string currentVersion = DataService.GetLocalVersion_Gui();
            UpdateStatus_Version($"Текущая версия: {currentVersion}", 0);

            // Инициализируем кнопку обновления
            settings_buttonUpdate.Enabled = false;

            // Запускаем проверку обновлений при загрузке формы
            this.Load += async (sender, e) => await CheckForUpdatesAsync();
        }

        private void LoadSettings()
        {
            // Автозапуск
            settings_checkBoxAutoStart.Checked = DataService.GetAutoStart();
            settings_checkBoxStartMinimized.Checked = DataService.GetStartupArguments() == "--minimized";
            settings_checkBoxStartMinimized.Enabled = settings_checkBoxAutoStart.Checked;
        }

        private void settings_buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        #region LOGIC

        private async Task CheckForUpdatesAsync()
        {
            try
            {
                UpdateStatus_Version("\nПроверка обновлений...", 1);
                System.Windows.Forms.Application.DoEvents(); // Обновляем UI

                // Получаем текущую версию приложения
                string currentVersion = DataService.GetLocalVersion_Gui();

                Debug.WriteLine($"[FormSettings] Проверка обновлений GUI. Текущая версия: {currentVersion}");

                // Проверяем обновление для GUI (false = проверка обновлений GUI)
                bool updateAvailable = await DataService.IsThereUpdateZapret_OriginAsync("", false);

                Debug.WriteLine($"[FormSettings] Результат проверки: {updateAvailable}");

                if (updateAvailable)
                {
                    UpdateStatus_Version("\n✅ Доступно обновление!", 1);
                    settings_buttonUpdate.Enabled = true;
                }
                else
                {
                    UpdateStatus_Version("\n✅ Установлена последняя версия", 1);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[FormSettings] Ошибка в CheckForUpdatesAsync: {ex.Message}");
                UpdateStatus_Version($"\n❌ Ошибка: {ex.Message}", 1);
            }
        }

        #endregion

        #region STATUS

        private void UpdateStatus_Version(string text, int row)
        {
            if (settings_textBoxVersion.InvokeRequired)
            {
                settings_textBoxVersion.Invoke(new Action(() => UpdateStatus_Version(text, row)));
                return;
            }

            switch (row)
            {
                case 0:
                    settings_textBoxVersion.Text = text;
                    break;
                case 1:
                    settings_textBoxVersion.Text += text;
                    break;
            }
        }

        #endregion

        private void settings_buttonSave_Click(object sender, EventArgs e)
        {
            bool autoStartEnabled = settings_checkBoxAutoStart.Checked;
            DataService.SetAutoStart(autoStartEnabled);

            string args = settings_checkBoxStartMinimized.Checked ? "--minimized" : "";
            DataService.SetStartupArguments(args);

            // Применяем изменения в реестре
            bool success;
            if (autoStartEnabled)
            {
                success = AutoStartManager.Enable(args);
            }
            else
            {
                success = AutoStartManager.Disable();
            }

            // Показываем результат
            if (autoStartEnabled && !success)
            {
                MessageBox.Show("Не удалось включить автозапуск.\n" +
                              "Попробуйте запустить программу от имени администратора.",
                              "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void settings_checkBoxAutoStart_CheckedChanged_1(object sender, EventArgs e)
        {
            settings_checkBoxStartMinimized.Enabled = settings_checkBoxAutoStart.Checked;
        }

        // Обработчик кнопки обновления GUI
        private void settings_buttonUpdate_Click(object sender, EventArgs e)
        {
            // Здесь будет логика обновления GUI приложения
            MessageBox.Show("Функция обновления GUI будет реализована позже",
                "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}