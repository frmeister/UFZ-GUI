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
            entrance_buttonClose = new Button();
            entrance_folderBrowserDialogHello = new FolderBrowserDialog();
            SuspendLayout();
            // 
            // entrance_buttonClose
            // 
            entrance_buttonClose.Location = new Point(541, 310);
            entrance_buttonClose.Name = "entrance_buttonClose";
            entrance_buttonClose.Size = new Size(151, 131);
            entrance_buttonClose.TabIndex = 0;
            entrance_buttonClose.Text = "Yes";
            entrance_buttonClose.UseVisualStyleBackColor = true;
            entrance_buttonClose.Click += entrance_buttonClose_Click;
            // 
            // FormEntrance
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(784, 561);
            Controls.Add(entrance_buttonClose);
            MaximizeBox = false;
            MaximumSize = new Size(800, 600);
            MinimumSize = new Size(800, 600);
            Name = "FormEntrance";
            Text = "Hello";
            ResumeLayout(false);
        }

        #endregion

        private Button entrance_buttonClose;
        private FolderBrowserDialog entrance_folderBrowserDialogHello;
    }
}