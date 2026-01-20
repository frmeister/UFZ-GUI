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
        }

        // Обработчики для CheckBox
        private void settings_checkBoxAutoStart_CheckedChanged(object sender, EventArgs e)
        {
            settings_checkBoxStartMinimized.Enabled = settings_checkBoxAutoStart.Checked;
        }
    }
}
