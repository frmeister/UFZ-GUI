using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using UFZ.Lib;
using UFZapret.Lib;

namespace UFZapret.Forms
{
    public partial class FormConfiguration : Form
    {
        string folderPath;
        string configName;

        private List<Button> configButtons; // Список всех кнопок
        private List<string> batFiles; // Список найденных .bat файлов
        public FormConfiguration()
        {
            InitializeComponent();

            InitializeButtonsList();

            CfgButtonsDisable();

            DrawCurrentPath();

            Updater_Origin();

            StatusDownload_Origin();
        }

        #region LOGIC
        private void InitializeButtonsList()
        {
            configButtons = new List<Button>
            {
                config_button1, config_button2, config_button3, config_button4,
                config_button5, config_button6, config_button7, config_button8,
                config_button9, config_button10, config_button11, config_button12,
                config_button13, config_button14, config_button15, config_button16,
                config_button17, config_button18, config_button19, config_button20,
                config_button21, config_button22, config_button23, config_button24,
            };

            foreach (var button in configButtons)
            {
                button.Click += ConfigButton_Click;
            }
        }

        // Disabling all func buttons for configs inside zapret
        private void CfgButtonsDisable()
        {
            foreach (var button in configButtons)
            {
                button.Enabled = false;
                button.Visible = false;
                button.Text = ""; // Очищаем текст
                button.Tag = null; // Очищаем Tag (там будет путь к файлу)
            }
        }

        private void CfgButtonsEnableAndFill(List<string> files)
        {
            CfgButtonsDisable();

            for (int i = 0; i < Math.Min(files.Count, configButtons.Count); i++)
            {
                string filePath = files[i];
                string fileName = Path.GetFileNameWithoutExtension(filePath);

                configButtons[i].Text = fileName;
                configButtons[i].Tag = filePath;
                configButtons[i].Visible = true;
                configButtons[i].Enabled = true;

                // Проверяем, должна ли эта кнопка быть выбрана
                if (configName != "none")
                {
                    string configNameWithoutExtension = configName.Replace(".bat", "");
                    if (fileName == configNameWithoutExtension)
                    {
                        configButtons[i].Enabled = false;
                        configButtons[i].BackColor = Color.LightGray;
                    }
                }
            }
        }

        public List<string> GetBatFiles(string folderPath)
        {
            try
            {
                // Проверяем существование папки
                if (!Directory.Exists(folderPath))
                {
                    MessageBox.Show($"Папка не найдена: {folderPath}", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return new List<string>();
                }

                // Получаем все .bat файлы
                string[] batFiles = Directory.GetFiles(folderPath, "*.bat");

                if (batFiles.Length == 0)
                {
                    MessageBox.Show($"В папке не найдены .bat файлы:\n{folderPath}",
                        "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return new List<string>();
                }

                // Преобразуем в список и сортируем по имени
                return batFiles.OrderBy(f => f).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при чтении файлов:\n{ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return new List<string>();
            }
        }

        private void DrawCurrentPath()
        {
            folderPath = ConfigManager.GetValue("pathOrigin", "none");
            configName = ConfigManager.GetValue("currentConfig", "none"); // ← ДО заполнения кнопок!

            if (folderPath != "none" && Directory.Exists(folderPath))
            {
                // Сначала получаем список файлов
                var files = GetBatFiles(folderPath);

                // Заполняем кнопки
                CfgButtonsEnableAndFill(files);

                UpdateStatus_Config($"Directory found!", 0);

                // Теперь ищем и деактивируем
                if (configName != "none")
                {
                    string configNameWithoutExtension = configName.Replace(".bat", "");

                    foreach (var button in configButtons)
                    {
                        if (button.Visible && button.Text == configNameWithoutExtension)
                        {
                            button.Enabled = false;
                            button.BackColor = Color.LightGray;
                            UpdateStatus_Config($"Выбран конфиг:\n{button.Text}", 1); // NOT WORKING
                            break;
                        }
                    }
                }
            }
            else
            {
                UpdateStatus_Config("Папка Zapret не настроена. Выберите папку.", 0);
            }
        }

        private void Updater_Origin()
        {
            ConfigManager.SetValue("originVersion", DataService.GetLocalVersion_Origin(folderPath));

            if(DataService.IsThereUpdateZapret_Origin(folderPath))
            {
                config_buttonUpdate.Enabled = true;

                UpdateStatus_Info("\n$New version of Zapret is avalible!", 0);
            }
            else
            {
                UpdateStatus_Info("\n$Stable version", 0);
            }
        }

        #endregion

        #region STATUS

        private void UpdateStatus_Info(string text, int row)
        {
            switch (row)
            {
                case 0:
                    config_textBoxInfo.Text = text;
                    break;
                case 1:
                    config_textBoxInfo.Text += text;
                    break;
            }
        }

        private void UpdateStatus_Config(string text, int row)
        {
            switch(row)
            {
                case 0:
                    config_textBoxConfigMaster.Text = text;
                    break;
                case 1:
                    config_textBoxConfigMaster.Text += text;
                    break;
            }
        }

        private void StatusDownload_Origin()
        {
            if (ConfigManager.GetValue("originPath", "none") != "none")
            {
                config_buttonDownload.Enabled = true;
                config_buttonDownload.Visible = true;
            }
        }

        #endregion

        #region BUTTONS
        private void ConfigButton_Click(object sender, EventArgs e)
        {
            Button clickedButton = sender as Button;

            // Проверяем, что это кнопка конфига (по имени)
            if (clickedButton == null || !clickedButton.Name.StartsWith("config_button") ||
                clickedButton.Name.EndsWith("Save") || clickedButton.Name.EndsWith("Cancel") ||
                clickedButton.Name.EndsWith("ChangeCfg") || clickedButton.Name.EndsWith("Update") ||
                clickedButton.Name.EndsWith("AutoCfg"))
                return;

            // 1. Сначала активируем ВСЕ остальные кнопки (снимаем предыдущий выбор)
            foreach (var button in configButtons)
            {
                // Пропускаем нажатую кнопку
                if (button == clickedButton) continue;

                // Активируем все остальные кнопки
                if (button.Visible && !button.Enabled)
                {
                    button.Enabled = true;
                    button.BackColor = SystemColors.Control; // Возвращаем стандартный цвет
                }
            }

            // 2. Деактивируем только нажатую кнопку
            clickedButton.Enabled = false;

            // 3. Сохраняем выбор
            configName = clickedButton.Text + ".bat";

            // 4. Показываем информацию о выборе
            UpdateStatus_Config($"Выбран конфиг:\n{clickedButton.Text}", 1);
        }

        private void config_buttonUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                DataService.UpdateZapret_Origin(folderPath);

                MessageBox.Show(
                    "$Обновление установлено успешно!",
                    "$Обновление",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
            }
            catch
            {
                MessageBox.Show(
                    "$Не удалось установить обновление",
                    "$Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void config_buttonSave_Click(object sender, EventArgs e)
        {
            try
            {
                // Saving the folder path into Config.cfg
                DataService.SaveFolderPath(folderPath);

                DataService.SaveCurrentConfig(configName);

                FormMain formMain = new FormMain();
                formMain.CheckIsConfigAvalible();
            }
            catch
            {
                MessageBox.Show("Error", "Info", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void config_buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void config_buttonChangeCfg_Click(object sender, EventArgs e)
        {
            config_folderBrowserDialogCfg.ShowDialog();
            folderPath = config_folderBrowserDialogCfg.SelectedPath;

            CfgButtonsEnableAndFill(GetBatFiles(folderPath));
        }

        private void config_buttonAutoCfg_Click(object sender, EventArgs e)
        {

        }

        private void config_buttonDownload_Click(object sender, EventArgs e)
        {
            config_folderBrowserDialogCfg.ShowDialog();
            folderPath = config_folderBrowserDialogCfg.SelectedPath;

            DataService.CreateNewGitClone_Zapret(folderPath);

            if (!DataService.GitExisting_Zapret(folderPath))
            {
                MessageBox.Show("Произошла ошибка, файлы не установлены!",
                "Ошибка",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            }
            else
            {
                ConfigManager.SetValue("pathOrigin", folderPath);

                config_buttonDownload.Enabled = false;
                config_buttonDownload.Visible = false;
            }
        }

        #endregion

    }
}
