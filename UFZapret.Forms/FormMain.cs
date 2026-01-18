using UFZ.Lib;
using UFZapret.Lib;

namespace UFZapret.Forms
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            
        }

        private void buttonConfiguration_Click(object sender, EventArgs e)
        {
            FormConfiguration formconfig = new FormConfiguration();
            formconfig.ShowDialog();
        }
    }
}
