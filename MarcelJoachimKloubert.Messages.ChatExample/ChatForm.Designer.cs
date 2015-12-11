namespace MarcelJoachimKloubert.Messages.ChatExample
{
    partial class ChatForm
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.TextBox_ChatMessage = new System.Windows.Forms.TextBox();
            this.Button_SendMessage = new System.Windows.Forms.Button();
            this.TextBox_ChatLog = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // TextBox_ChatMessage
            // 
            this.TextBox_ChatMessage.Location = new System.Drawing.Point(12, 12);
            this.TextBox_ChatMessage.Name = "TextBox_ChatMessage";
            this.TextBox_ChatMessage.Size = new System.Drawing.Size(489, 20);
            this.TextBox_ChatMessage.TabIndex = 0;
            // 
            // Button_SendMessage
            // 
            this.Button_SendMessage.Location = new System.Drawing.Point(507, 12);
            this.Button_SendMessage.Name = "Button_SendMessage";
            this.Button_SendMessage.Size = new System.Drawing.Size(75, 20);
            this.Button_SendMessage.TabIndex = 1;
            this.Button_SendMessage.Text = "Send";
            this.Button_SendMessage.UseVisualStyleBackColor = true;
            this.Button_SendMessage.Click += new System.EventHandler(this.Button_SendMessage_Click);
            // 
            // TextBox_ChatLog
            // 
            this.TextBox_ChatLog.Location = new System.Drawing.Point(12, 38);
            this.TextBox_ChatLog.Multiline = true;
            this.TextBox_ChatLog.Name = "TextBox_ChatLog";
            this.TextBox_ChatLog.ReadOnly = true;
            this.TextBox_ChatLog.Size = new System.Drawing.Size(570, 341);
            this.TextBox_ChatLog.TabIndex = 2;
            this.TextBox_ChatLog.WordWrap = false;
            // 
            // ChatForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(594, 391);
            this.Controls.Add(this.TextBox_ChatLog);
            this.Controls.Add(this.Button_SendMessage);
            this.Controls.Add(this.TextBox_ChatMessage);
            this.Name = "ChatForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox TextBox_ChatMessage;
        private System.Windows.Forms.Button Button_SendMessage;
        private System.Windows.Forms.TextBox TextBox_ChatLog;
    }
}

