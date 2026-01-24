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
    public partial class FormEntrance : Form
    {
        string folderPath;

        public FormEntrance()
        {
            InitializeComponent();
        }

        #region BUTTON LOGIC

        private void entrance_buttonClose_Click(object sender, EventArgs e)
        {
            entrance_folderBrowserDialogHello.ShowDialog();

            folderPath = entrance_folderBrowserDialogHello.SelectedPath;

            string[] batFiles = Directory.GetFiles(folderPath, "*.bat");

            bool gitDirectoryIsThere = File.Exists(folderPath + ".gitattributes");

            if (batFiles.Length == 0)
            {
                MessageBox.Show($"В папке не найдены .bat файлы:\n{folderPath}",
                    "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                if (gitDirectoryIsThere)
                {

                    ConfigManager.SetValue("pathOrigin", folderPath);

                    FormConfiguration formconfig = new FormConfiguration();
                    formconfig.ShowDialog();

                    this.DialogResult = DialogResult.OK;
                }
                else
                {
                    var result = MessageBox.Show("$Папка установлена не через git (необходимо для авто-обновлений). Установить?\n" +
                        "Если нет, то функция авто-обновлений не будет работать (будет работать некорректно)",
                        "$Непраильный путь",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    switch (result)
                    {
                        case DialogResult.Yes:

                            MessageBox.Show("Укажите путь куда установить zapret",
                                "Новый zapret",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Exclamation);

                            entrance_folderBrowserDialogHello.ShowDialog();

                            folderPath = entrance_folderBrowserDialogHello.SelectedPath;

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

                                new FormConfiguration().ShowDialog();

                                this.DialogResult = DialogResult.OK;
                            }

                            break;

                        case DialogResult.No:
                            ConfigManager.SetValue("pathOrigin", folderPath);

                            FormConfiguration formconfig = new FormConfiguration();
                            formconfig.ShowDialog();

                            this.DialogResult = DialogResult.OK;
                            break;
                    }
                }
            }

        }



        // Обработчик закрытия формы через крестик
        private void Entrance_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Если пользователь нажал крестик - считаем это отменой
            if (this.DialogResult == DialogResult.None)
            {
                this.DialogResult = DialogResult.Cancel;
            }
        }

        #endregion


        private void entrance_buttonDownload_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Куда хотите установить Zapret (лучше установить в папку приложения)",
                "Установка",
                MessageBoxButtons.OK,
                MessageBoxIcon.Exclamation);

            entrance_folderBrowserDialogHello.ShowDialog();
            folderPath = entrance_folderBrowserDialogHello.SelectedPath;

            DataService.CreateNewGitClone_Zapret(folderPath);

            folderPath += "\\zapret-discord-youtube";

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

                new FormConfiguration().ShowDialog();

                this.DialogResult = DialogResult.OK;
            }
        }
    }
}
