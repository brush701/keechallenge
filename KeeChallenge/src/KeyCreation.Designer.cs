namespace KeeChallenge
{
    partial class KeyCreation
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
            this.promptLabel = new System.Windows.Forms.Label();
            this.secretTextBox = new System.Windows.Forms.TextBox();
            this.OKButton = new System.Windows.Forms.Button();
            this.CancelButton1 = new System.Windows.Forms.Button();
            this.LT64_cb = new System.Windows.Forms.CheckBox();
            this.helpProvider1 = new System.Windows.Forms.HelpProvider();
            this.Slot_cb = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // promptLabel
            // 
            this.promptLabel.AutoSize = true;
            this.helpProvider1.SetHelpString(this.promptLabel, "Enter the HMAC-SHA1 secret key you generated while setting up Yubikey for challen" +
        "ge-response mode.");
            this.promptLabel.Location = new System.Drawing.Point(18, 46);
            this.promptLabel.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.promptLabel.Name = "promptLabel";
            this.helpProvider1.SetShowHelp(this.promptLabel, true);
            this.promptLabel.Size = new System.Drawing.Size(518, 25);
            this.promptLabel.TabIndex = 0;
            this.promptLabel.Text = "Enter Your Yubkey Challenge-Response Secret Key:";
            // 
            // secretTextBox
            // 
            this.helpProvider1.SetHelpString(this.secretTextBox, "Enter the HMAC-SHA1 secret key you generated while setting up Yubikey for challen" +
        "ge-response mode.");
            this.secretTextBox.Location = new System.Drawing.Point(24, 77);
            this.secretTextBox.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.secretTextBox.Name = "secretTextBox";
            this.helpProvider1.SetShowHelp(this.secretTextBox, true);
            this.secretTextBox.Size = new System.Drawing.Size(646, 31);
            this.secretTextBox.TabIndex = 1;
            // 
            // OKButton
            // 
            this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKButton.Location = new System.Drawing.Point(362, 208);
            this.OKButton.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(150, 44);
            this.OKButton.TabIndex = 3;
            this.OKButton.Text = "&OK";
            this.OKButton.UseVisualStyleBackColor = true;
            // 
            // CancelButton1
            // 
            this.CancelButton1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelButton1.Location = new System.Drawing.Point(524, 208);
            this.CancelButton1.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.CancelButton1.Name = "CancelButton1";
            this.CancelButton1.Size = new System.Drawing.Size(150, 44);
            this.CancelButton1.TabIndex = 4;
            this.CancelButton1.Text = "&Cancel";
            this.CancelButton1.UseVisualStyleBackColor = true;
            // 
            // LT64_cb
            // 
            this.LT64_cb.AutoSize = true;
            this.LT64_cb.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.LT64_cb.Location = new System.Drawing.Point(362, 127);
            this.LT64_cb.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.LT64_cb.Name = "LT64_cb";
            this.LT64_cb.Size = new System.Drawing.Size(310, 29);
            this.LT64_cb.TabIndex = 2;
            this.LT64_cb.Text = "Variable Length Challenge?";
            this.LT64_cb.UseVisualStyleBackColor = true;
            // 
            // Slot_cb
            // 
            this.Slot_cb.FormattingEnabled = true;
            this.Slot_cb.Items.AddRange(new object[] {
            "Slot 1",
            "Slot 2"});
            this.Slot_cb.Location = new System.Drawing.Point(23, 127);
            this.Slot_cb.Name = "Slot_cb";
            this.Slot_cb.Size = new System.Drawing.Size(121, 33);
            this.Slot_cb.TabIndex = 5;
            // 
            // KeyCreation
            // 
            this.AcceptButton = this.OKButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(12F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(698, 275);
            this.Controls.Add(this.Slot_cb);
            this.Controls.Add(this.LT64_cb);
            this.Controls.Add(this.CancelButton1);
            this.Controls.Add(this.OKButton);
            this.Controls.Add(this.secretTextBox);
            this.Controls.Add(this.promptLabel);
            this.HelpButton = true;
            this.Margin = new System.Windows.Forms.Padding(6, 6, 6, 6);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "KeyCreation";
            this.Text = "Secret Key Entry";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label promptLabel;
        private System.Windows.Forms.TextBox secretTextBox;
        private System.Windows.Forms.Button OKButton;
        private System.Windows.Forms.Button CancelButton1;
        private System.Windows.Forms.CheckBox LT64_cb;
        private System.Windows.Forms.HelpProvider helpProvider1;
        private System.Windows.Forms.ComboBox Slot_cb;
    }
}

