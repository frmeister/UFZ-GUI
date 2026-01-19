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

            if (batFiles.Length == 0)
            {
                MessageBox.Show($"В папке не найдены .bat файлы:\n{folderPath}",
                    "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                ConfigManager.SetValue("pathOrigin", folderPath);

                FormConfiguration formconfig = new FormConfiguration();
                formconfig.ShowDialog();

                this.DialogResult = DialogResult.OK;
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


    }
}
