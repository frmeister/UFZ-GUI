namespace UFZapret.Forms
{
    partial class FormEntrance
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormEntrance));
            entrance_buttonClose = new Button();
            entrance_folderBrowserDialogHello = new FolderBrowserDialog();
            entrance_buttonDownload = new Button();
            textBox1 = new TextBox();
            SuspendLayout();
            // 
            // entrance_buttonClose
            // 
            entrance_buttonClose.Location = new Point(317, 393);
            entrance_buttonClose.Name = "entrance_buttonClose";
            entrance_buttonClose.Size = new Size(134, 50);
            entrance_buttonClose.TabIndex = 0;
            entrance_buttonClose.Text = "Путь";
            entrance_buttonClose.UseVisualStyleBackColor = true;
            entrance_buttonClose.Click += entrance_buttonClose_Click;
            // 
            // entrance_buttonDownload
            // 
            entrance_buttonDownload.Location = new Point(305, 267);
            entrance_buttonDownload.Name = "entrance_buttonDownload";
            entrance_buttonDownload.Size = new Size(157, 120);
            entrance_buttonDownload.TabIndex = 1;
            entrance_buttonDownload.Text = "Установить";
            entrance_buttonDownload.UseVisualStyleBackColor = true;
            entrance_buttonDownload.Click += entrance_buttonDownload_Click;
            // 
            // textBox1
            // 
            textBox1.Font = new Font("Segoe UI Emoji", 12F, FontStyle.Regular, GraphicsUnit.Point, 0);
            textBox1.Location = new Point(183, 73);
            textBox1.Multiline = true;
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(416, 188);
            textBox1.TabIndex = 2;
            textBox1.Text = resources.GetString("textBox1.Text");
            // 
            // FormEntrance
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(784, 561);
            Controls.Add(textBox1);
            Controls.Add(entrance_buttonDownload);
            Controls.Add(entrance_buttonClose);
            MaximizeBox = false;
            MaximumSize = new Size(800, 600);
            MinimumSize = new Size(800, 600);
            Name = "FormEntrance";
            Text = "Hello";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button entrance_buttonClose;
        private FolderBrowserDialog entrance_folderBrowserDialogHello;
        private Button entrance_buttonDownload;
        private TextBox textBox1;
    }
}