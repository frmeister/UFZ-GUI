using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using UFZ.Lib;
using UFZapret.Lib;

namespace UFZapret.Forms
{
    public partial class FormSettings : Form
    {
        public FormSettings()
        {
            InitializeComponent();

            LoadSettings();

            OutputVersion(ConfigManager.GetValue("appVersion", "none"));
        }

        private void LoadSettings()
        {
            // Существующие настройки...

            // Автозапуск
            settings_checkBoxAutoStart.Checked = DataService.GetAutoStart();
            settings_checkBoxStartMinimized.Checked = DataService.GetStartupArguments() == "--minimized";
            settings_checkBoxStartMinimized.Enabled = settings_checkBoxAutoStart.Checked;
        }

        private void settings_buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        #region STATUS

        private void OutputVersion(string message)
        {
            settings_textBoxVersion.Text = $"Currrent version - " + message;
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
    }
}
