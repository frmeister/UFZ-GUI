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

namespace UFZapret.Forms
{
    public partial class FormSettings : Form
    {
        public FormSettings()
        {
            InitializeComponent();

            OutputVersion(ConfigManager.GetValue("appVersion", "none"));
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
    }
}
