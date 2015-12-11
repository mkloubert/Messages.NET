namespace MarcelJoachimKloubert.Messages.ChatExample
{
    partial class MainForm
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
            this.Button_CreateWindow = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // Button_CreateWindow
            // 
            this.Button_CreateWindow.Location = new System.Drawing.Point(12, 12);
            this.Button_CreateWindow.Name = "Button_CreateWindow";
            this.Button_CreateWindow.Size = new System.Drawing.Size(260, 23);
            this.Button_CreateWindow.TabIndex = 0;
            this.Button_CreateWindow.Text = "Create window";
            this.Button_CreateWindow.UseVisualStyleBackColor = true;
            this.Button_CreateWindow.Click += new System.EventHandler(this.Button_CreateWindow_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 48);
            this.Controls.Add(this.Button_CreateWindow);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Messages.NET Chat example";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button Button_CreateWindow;
    }
}