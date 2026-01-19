namespace UFZapret.Forms
{
    partial class FormMain
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            panelGeneral = new Panel();
            textBoxStatus = new TextBox();
            buttonConfiguration = new Button();
            textBoxInfo = new TextBox();
            panelStatus = new Panel();
            buttonStart = new Button();
            splitter1 = new Splitter();
            panelGeneral.SuspendLayout();
            panelStatus.SuspendLayout();
            SuspendLayout();
            // 
            // panelGeneral
            // 
            panelGeneral.BackColor = Color.FromArgb(255, 255, 192);
            panelGeneral.Controls.Add(textBoxStatus);
            panelGeneral.Controls.Add(buttonConfiguration);
            panelGeneral.Controls.Add(textBoxInfo);
            panelGeneral.Dock = DockStyle.Top;
            panelGeneral.Location = new Point(0, 0);
            panelGeneral.Name = "panelGeneral";
            panelGeneral.Size = new Size(784, 96);
            panelGeneral.TabIndex = 0;
            // 
            // textBoxStatus
            // 
            textBoxStatus.Enabled = false;
            textBoxStatus.Font = new Font("Meiryo UI", 12F);
            textBoxStatus.Location = new Point(361, 3);
            textBoxStatus.Multiline = true;
            textBoxStatus.Name = "textBoxStatus";
            textBoxStatus.Size = new Size(303, 87);
            textBoxStatus.TabIndex = 2;
            textBoxStatus.Text = "Status:\r\n";
            // 
            // buttonConfiguration
            // 
            buttonConfiguration.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonConfiguration.Image = (Image)resources.GetObject("buttonConfiguration.Image");
            buttonConfiguration.Location = new Point(695, 3);
            buttonConfiguration.Name = "buttonConfiguration";
            buttonConfiguration.Size = new Size(86, 87);
            buttonConfiguration.TabIndex = 1;
            buttonConfiguration.UseVisualStyleBackColor = true;
            buttonConfiguration.Click += buttonConfiguration_Click;
            // 
            // textBoxInfo
            // 
            textBoxInfo.Anchor = AnchorStyles.Left;
            textBoxInfo.BackColor = Color.White;
            textBoxInfo.Enabled = false;
            textBoxInfo.Font = new Font("Meiryo UI", 12F);
            textBoxInfo.Location = new Point(3, 3);
            textBoxInfo.Multiline = true;
            textBoxInfo.Name = "textBoxInfo";
            textBoxInfo.Size = new Size(352, 87);
            textBoxInfo.TabIndex = 0;
            textBoxInfo.Text = "Тестовая программа, для упрощения работы с Zapret\r\n";
            // 
            // panelStatus
            // 
            panelStatus.BackColor = Color.FromArgb(192, 255, 192);
            panelStatus.Controls.Add(buttonStart);
            panelStatus.Controls.Add(splitter1);
            panelStatus.Dock = DockStyle.Fill;
            panelStatus.Location = new Point(0, 96);
            panelStatus.Name = "panelStatus";
            panelStatus.Size = new Size(784, 465);
            panelStatus.TabIndex = 0;
            // 
            // buttonStart
            // 
            buttonStart.Enabled = false;
            buttonStart.Image = (Image)resources.GetObject("buttonStart.Image");
            buttonStart.Location = new Point(260, 130);
            buttonStart.Name = "buttonStart";
            buttonStart.Size = new Size(247, 188);
            buttonStart.TabIndex = 1;
            buttonStart.UseVisualStyleBackColor = true;
            buttonStart.Click += buttonStart_Click;
            // 
            // splitter1
            // 
            splitter1.Dock = DockStyle.Top;
            splitter1.Location = new Point(0, 0);
            splitter1.Name = "splitter1";
            splitter1.Size = new Size(784, 3);
            splitter1.TabIndex = 0;
            splitter1.TabStop = false;
            // 
            // FormMain
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(784, 561);
            Controls.Add(panelStatus);
            Controls.Add(panelGeneral);
            MaximizeBox = false;
            MaximumSize = new Size(800, 600);
            MinimumSize = new Size(800, 600);
            Name = "FormMain";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Main";
            panelGeneral.ResumeLayout(false);
            panelGeneral.PerformLayout();
            panelStatus.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel panelGeneral;
        private Panel panelStatus;
        private TextBox textBoxInfo;
        private Splitter splitter1;
        private Button buttonConfiguration;
        private TextBox textBoxStatus;
        private Button buttonStart;
    }
}
