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
    }
}
