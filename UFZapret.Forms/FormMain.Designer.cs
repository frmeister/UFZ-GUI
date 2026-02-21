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
            buttonConfiguration = new Button();
            buttonSettings = new Button();
            textBoxStatus = new TextBox();
            pictureBoxTheme = new PictureBox();
            pictureBoxThemeHead = new PictureBox();
            panelStatus = new Panel();
            buttonStart = new Button();
            splitter1 = new Splitter();
            panelGeneral.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxTheme).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxThemeHead).BeginInit();
            panelStatus.SuspendLayout();
            SuspendLayout();
            // 
            // panelGeneral
            // 
            panelGeneral.BackColor = SystemColors.AppWorkspace;
            panelGeneral.Controls.Add(buttonConfiguration);
            panelGeneral.Controls.Add(buttonSettings);
            panelGeneral.Controls.Add(textBoxStatus);
            panelGeneral.Controls.Add(pictureBoxTheme);
            panelGeneral.Dock = DockStyle.Top;
            panelGeneral.Location = new Point(0, 0);
            panelGeneral.Name = "panelGeneral";
            panelGeneral.Size = new Size(784, 96);
            panelGeneral.TabIndex = 0;
            // 
            // buttonConfiguration
            // 
            buttonConfiguration.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonConfiguration.BackColor = Color.Transparent;
            buttonConfiguration.Image = (Image)resources.GetObject("buttonConfiguration.Image");
            buttonConfiguration.Location = new Point(695, 3);
            buttonConfiguration.Name = "buttonConfiguration";
            buttonConfiguration.Size = new Size(86, 87);
            buttonConfiguration.TabIndex = 1;
            buttonConfiguration.UseVisualStyleBackColor = false;
            buttonConfiguration.Click += buttonConfiguration_Click;
            // 
            // buttonSettings
            // 
            buttonSettings.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonSettings.BackColor = Color.Transparent;
            buttonSettings.BackgroundImageLayout = ImageLayout.None;
            buttonSettings.Cursor = Cursors.Hand;
            buttonSettings.ForeColor = SystemColors.ControlText;
            buttonSettings.Image = (Image)resources.GetObject("buttonSettings.Image");
            buttonSettings.Location = new Point(603, 3);
            buttonSettings.Name = "buttonSettings";
            buttonSettings.Size = new Size(86, 87);
            buttonSettings.TabIndex = 3;
            buttonSettings.UseVisualStyleBackColor = false;
            buttonSettings.Click += buttonSettings_Click;
            // 
            // textBoxStatus
            // 
            textBoxStatus.Enabled = false;
            textBoxStatus.Font = new Font("Meiryo UI", 12F);
            textBoxStatus.Location = new Point(3, 3);
            textBoxStatus.Multiline = true;
            textBoxStatus.Name = "textBoxStatus";
            textBoxStatus.Size = new Size(303, 87);
            textBoxStatus.TabIndex = 2;
            textBoxStatus.Text = "Status:\r\n";
            // 
            // pictureBoxTheme
            // 
            pictureBoxTheme.Image = (Image)resources.GetObject("pictureBoxTheme.Image");
            pictureBoxTheme.Location = new Point(0, 0);
            pictureBoxTheme.Name = "pictureBoxTheme";
            pictureBoxTheme.Size = new Size(784, 227);
            pictureBoxTheme.TabIndex = 2;
            pictureBoxTheme.TabStop = false;
            // 
            // pictureBoxThemeHead
            // 
            pictureBoxThemeHead.BackgroundImage = (Image)resources.GetObject("pictureBoxThemeHead.BackgroundImage");
            pictureBoxThemeHead.Location = new Point(-18, -27);
            pictureBoxThemeHead.Name = "pictureBoxThemeHead";
            pictureBoxThemeHead.Size = new Size(799, 511);
            pictureBoxThemeHead.TabIndex = 3;
            pictureBoxThemeHead.TabStop = false;
            // 
            // panelStatus
            // 
            panelStatus.BackColor = SystemColors.Control;
            panelStatus.Controls.Add(buttonStart);
            panelStatus.Controls.Add(splitter1);
            panelStatus.Controls.Add(pictureBoxThemeHead);
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
            buttonStart.Location = new Point(294, 170);
            buttonStart.Name = "buttonStart";
            buttonStart.Size = new Size(200, 140);
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
            ((System.ComponentModel.ISupportInitialize)pictureBoxTheme).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxThemeHead).EndInit();
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
        private PictureBox pictureBoxTheme;
        private PictureBox pictureBoxThemeHead;
    }
}
