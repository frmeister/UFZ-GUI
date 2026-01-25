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
            if (disposing)
            {
                trayIcon?.Dispose();
                trayMenu?.Dispose();
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
            buttonSettings = new Button();
            textBoxStatus = new TextBox();
            buttonConfiguration = new Button();
            panelStatus = new Panel();
            buttonStart = new Button();
            splitter1 = new Splitter();
            panelGeneral.SuspendLayout();
            panelStatus.SuspendLayout();
            SuspendLayout();
            // 
            // panelGeneral
            // 
            panelGeneral.BackColor = SystemColors.AppWorkspace;
            panelGeneral.Controls.Add(buttonSettings);
            panelGeneral.Controls.Add(textBoxStatus);
            panelGeneral.Controls.Add(buttonConfiguration);
            panelGeneral.Dock = DockStyle.Top;
            panelGeneral.Location = new Point(0, 0);
            panelGeneral.Name = "panelGeneral";
            panelGeneral.Size = new Size(784, 96);
            panelGeneral.TabIndex = 0;
            // 
            // buttonSettings
            // 
            buttonSettings.Image = (Image)resources.GetObject("buttonSettings.Image");
            buttonSettings.Location = new Point(596, 3);
            buttonSettings.Name = "buttonSettings";
            buttonSettings.Size = new Size(86, 87);
            buttonSettings.TabIndex = 3;
            buttonSettings.UseVisualStyleBackColor = true;
            buttonSettings.Click += buttonSettings_Click;
            // 
            // textBoxStatus
            // 
            textBoxStatus.Enabled = false;
            textBoxStatus.Font = new Font("Meiryo UI", 12F);
            textBoxStatus.Location = new Point(226, 3);
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
            // panelStatus
            // 
            panelStatus.BackColor = SystemColors.Control;
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
        private Splitter splitter1;
        private Button buttonConfiguration;
        private TextBox textBoxStatus;
        private Button buttonStart;
        private Button buttonSettings;
    }
}
