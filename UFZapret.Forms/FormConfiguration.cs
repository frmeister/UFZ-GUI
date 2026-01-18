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
using UFZapret.Lib;

namespace UFZapret.Forms
{
    public partial class FormConfiguration : Form
    {
        string folderPath;
        private List<Button> configButtons; // Список всех кнопок
        private List<string> batFiles; // Список найденных .bat файлов
        public FormConfiguration()
        {
            InitializeComponent();

            InitializeButtonsList();

            CfgButtonsDisable();
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
                config_button21, config_button22, config_button23, config_button24
            };
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
            // Сначала отключаем все
            CfgButtonsDisable();

            // Включаем и заполняем только нужное количество
            for (int i = 0; i < Math.Min(files.Count, configButtons.Count); i++)
            {
                string filePath = files[i];
                string fileName = Path.GetFileNameWithoutExtension(filePath);

                configButtons[i].Text = fileName;
                configButtons[i].Tag = filePath; // Сохраняем полный путь в Tag
                configButtons[i].Visible = true;
                configButtons[i].Enabled = true;
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
        #endregion

        private void config_buttonSave_Click(object sender, EventArgs e)
        {
            try
            {
                bool isCfgSelected = false;
                string fileName;
                foreach (var button in configButtons)
                {
                    if (button.Enabled == false)
                    {
                        isCfgSelected = true;

                        fileName = button.Text;
                        DataService.SaveCurrentConfig(fileName);
                    }
                }

                // Saving the folder path into Config.cfg
                DataService.SaveFolderPath(folderPath);


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
            string pathDirectory = config_folderBrowserDialogCfg.SelectedPath;

            folderPath = pathDirectory;

            CfgButtonsEnableAndFill(GetBatFiles(pathDirectory));
        }

        #region ConfigButtons

        #endregion
    }
}
