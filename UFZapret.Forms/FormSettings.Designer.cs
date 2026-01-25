namespace UFZapret.Forms
{
    partial class FormSettings
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            settings_panelGeneral = new Panel();
            settings_textBoxInfo = new TextBox();
            settings_panelMain = new Panel();
            settings_buttonTheme = new Button();
            settings_checkBoxStartMinimized = new CheckBox();
            settings_checkBoxAutoStart = new CheckBox();
            settings_textBoxAutoStart = new TextBox();
            settings_textBoxVersion = new TextBox();
            settings_buttonCancel = new Button();
            settings_buttonSave = new Button();
            settings_buttonUpdate = new Button();
            settings_splitter = new Splitter();
            settings_panelGeneral.SuspendLayout();
            settings_panelMain.SuspendLayout();
            SuspendLayout();
            // 
            // settings_panelGeneral
            // 
            settings_panelGeneral.BackColor = SystemColors.AppWorkspace;
            settings_panelGeneral.Controls.Add(settings_textBoxInfo);
            settings_panelGeneral.Dock = DockStyle.Top;
            settings_panelGeneral.Location = new Point(0, 0);
            settings_panelGeneral.Name = "settings_panelGeneral";
            settings_panelGeneral.Size = new Size(634, 100);
            settings_panelGeneral.TabIndex = 0;
            // 
            // settings_textBoxInfo
            // 
            settings_textBoxInfo.Enabled = false;
            settings_textBoxInfo.Location = new Point(3, 3);
            settings_textBoxInfo.Multiline = true;
            settings_textBoxInfo.Name = "settings_textBoxInfo";
            settings_textBoxInfo.Size = new Size(237, 94);
            settings_textBoxInfo.TabIndex = 0;
            settings_textBoxInfo.Text = "Info =D";
            // 
            // settings_panelMain
            // 
            settings_panelMain.BackColor = SystemColors.Control;
            settings_panelMain.Controls.Add(settings_buttonTheme);
            settings_panelMain.Controls.Add(settings_checkBoxStartMinimized);
            settings_panelMain.Controls.Add(settings_checkBoxAutoStart);
            settings_panelMain.Controls.Add(settings_textBoxAutoStart);
            settings_panelMain.Controls.Add(settings_textBoxVersion);
            settings_panelMain.Controls.Add(settings_buttonCancel);
            settings_panelMain.Controls.Add(settings_buttonSave);
            settings_panelMain.Controls.Add(settings_buttonUpdate);
            settings_panelMain.Controls.Add(settings_splitter);
            settings_panelMain.Dock = DockStyle.Fill;
            settings_panelMain.Location = new Point(0, 100);
            settings_panelMain.Name = "settings_panelMain";
            settings_panelMain.Size = new Size(634, 661);
            settings_panelMain.TabIndex = 0;
            // 
            // settings_buttonTheme
            // 
            settings_buttonTheme.Location = new Point(532, 95);
            settings_buttonTheme.Name = "settings_buttonTheme";
            settings_buttonTheme.Size = new Size(90, 68);
            settings_buttonTheme.TabIndex = 7;
            settings_buttonTheme.UseVisualStyleBackColor = true;
            settings_buttonTheme.Click += settings_buttonTheme_Click;
            // 
            // settings_checkBoxStartMinimized
            // 
            settings_checkBoxStartMinimized.AutoSize = true;
            settings_checkBoxStartMinimized.Location = new Point(346, 99);
            settings_checkBoxStartMinimized.Name = "settings_checkBoxStartMinimized";
            settings_checkBoxStartMinimized.Size = new Size(82, 19);
            settings_checkBoxStartMinimized.TabIndex = 6;
            settings_checkBoxStartMinimized.Text = "Minimized";
            settings_checkBoxStartMinimized.UseVisualStyleBackColor = true;
            // 
            // settings_checkBoxAutoStart
            // 
            settings_checkBoxAutoStart.AutoSize = true;
            settings_checkBoxAutoStart.Location = new Point(264, 99);
            settings_checkBoxAutoStart.Name = "settings_checkBoxAutoStart";
            settings_checkBoxAutoStart.Size = new Size(76, 19);
            settings_checkBoxAutoStart.TabIndex = 5;
            settings_checkBoxAutoStart.Text = "AutoStart";
            settings_checkBoxAutoStart.UseVisualStyleBackColor = true;
            settings_checkBoxAutoStart.CheckedChanged += settings_checkBoxAutoStart_CheckedChanged_1;
            // 
            // settings_textBoxAutoStart
            // 
            settings_textBoxAutoStart.Enabled = false;
            settings_textBoxAutoStart.Location = new Point(3, 97);
            settings_textBoxAutoStart.Multiline = true;
            settings_textBoxAutoStart.Name = "settings_textBoxAutoStart";
            settings_textBoxAutoStart.Size = new Size(237, 81);
            settings_textBoxAutoStart.TabIndex = 4;
            settings_textBoxAutoStart.Text = "Добавление программы в автозапуск:\r\n(НЕ РЕКОМЕНДУЕТСЯ ОТКЛЮЧАТЬ АВТОЗАПУСК ЧЕРЕЗ ДИСПЕТЧЕР ЗАДАЧ)";
            // 
            // settings_textBoxVersion
            // 
            settings_textBoxVersion.Enabled = false;
            settings_textBoxVersion.Location = new Point(3, 9);
            settings_textBoxVersion.Multiline = true;
            settings_textBoxVersion.Name = "settings_textBoxVersion";
            settings_textBoxVersion.Size = new Size(237, 82);
            settings_textBoxVersion.TabIndex = 3;
            // 
            // settings_buttonCancel
            // 
            settings_buttonCancel.Location = new Point(460, 578);
            settings_buttonCancel.Name = "settings_buttonCancel";
            settings_buttonCancel.Size = new Size(78, 71);
            settings_buttonCancel.TabIndex = 2;
            settings_buttonCancel.Text = "Cancel";
            settings_buttonCancel.UseVisualStyleBackColor = true;
            settings_buttonCancel.Click += settings_buttonCancel_Click;
            // 
            // settings_buttonSave
            // 
            settings_buttonSave.Location = new Point(544, 578);
            settings_buttonSave.Name = "settings_buttonSave";
            settings_buttonSave.Size = new Size(78, 71);
            settings_buttonSave.TabIndex = 2;
            settings_buttonSave.Text = "Save";
            settings_buttonSave.UseVisualStyleBackColor = true;
            settings_buttonSave.Click += settings_buttonSave_Click;
            // 
            // settings_buttonUpdate
            // 
            settings_buttonUpdate.Location = new Point(532, 9);
            settings_buttonUpdate.Name = "settings_buttonUpdate";
            settings_buttonUpdate.Size = new Size(90, 82);
            settings_buttonUpdate.TabIndex = 1;
            settings_buttonUpdate.Text = "Update";
            settings_buttonUpdate.UseVisualStyleBackColor = true;
            // 
            // settings_splitter
            // 
            settings_splitter.Dock = DockStyle.Top;
            settings_splitter.Location = new Point(0, 0);
            settings_splitter.Name = "settings_splitter";
            settings_splitter.Size = new Size(634, 3);
            settings_splitter.TabIndex = 0;
            settings_splitter.TabStop = false;
            // 
            // FormSettings
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(634, 761);
            Controls.Add(settings_panelMain);
            Controls.Add(settings_panelGeneral);
            MaximumSize = new Size(650, 800);
            MinimumSize = new Size(650, 800);
            Name = "FormSettings";
            Text = "Settings";
            settings_panelGeneral.ResumeLayout(false);
            settings_panelGeneral.PerformLayout();
            settings_panelMain.ResumeLayout(false);
            settings_panelMain.PerformLayout();
            ResumeLayout(false);
        }

        private void Settings_checkBoxAutoStart_CheckedChanged(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        #endregion

        private Panel settings_panelGeneral;
        private TextBox settings_textBoxInfo;
        private Panel settings_panelMain;
        private TextBox settings_textBoxVersion;
        private Button settings_buttonCancel;
        private Button settings_buttonSave;
        private Button settings_buttonUpdate;
        private Splitter settings_splitter;
        private CheckBox settings_checkBoxAutoStart;
        private TextBox settings_textBoxAutoStart;
        private CheckBox settings_checkBoxStartMinimized;
        private Button settings_buttonTheme;
    }
}