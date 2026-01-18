using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UFZapret.Forms
{
    public partial class FormEntrance : Form
    {
        public FormEntrance()
        {
            InitializeComponent();
        }

        private void entrance_buttonClose_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
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
    }
}
