﻿namespace KeeChallenge
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
            this.SuspendLayout();
            // 
            // promptLabel
            // 
            this.promptLabel.AutoSize = true;
            this.promptLabel.Location = new System.Drawing.Point(9, 24);
            this.promptLabel.Name = "promptLabel";
            this.promptLabel.Size = new System.Drawing.Size(255, 13);
            this.promptLabel.TabIndex = 0;
            this.promptLabel.Text = "Enter Your Yubkey Challenge-Response Secret Key:";
            // 
            // secretTextBox
            // 
            this.secretTextBox.Location = new System.Drawing.Point(12, 40);
            this.secretTextBox.Name = "secretTextBox";
            this.secretTextBox.Size = new System.Drawing.Size(325, 20);
            this.secretTextBox.TabIndex = 1;
            // 
            // OKButton
            // 
            this.OKButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OKButton.Location = new System.Drawing.Point(181, 108);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(75, 23);
            this.OKButton.TabIndex = 2;
            this.OKButton.Text = "&OK";
            this.OKButton.UseVisualStyleBackColor = true;
            // 
            // CancelButton1
            // 
            this.CancelButton1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.CancelButton1.Location = new System.Drawing.Point(262, 108);
            this.CancelButton1.Name = "CancelButton1";
            this.CancelButton1.Size = new System.Drawing.Size(75, 23);
            this.CancelButton1.TabIndex = 3;
            this.CancelButton1.Text = "&Cancel";
            this.CancelButton1.UseVisualStyleBackColor = true;
            // 
            // LT64_cb
            // 
            this.LT64_cb.AutoSize = true;
            this.LT64_cb.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.LT64_cb.Location = new System.Drawing.Point(181, 66);
            this.LT64_cb.Name = "LT64_cb";
            this.LT64_cb.Size = new System.Drawing.Size(156, 17);
            this.LT64_cb.TabIndex = 4;
            this.LT64_cb.Text = "Variable Length Challenge?";
            this.LT64_cb.UseVisualStyleBackColor = true;
            this.LT64_cb.CheckedChanged += new System.EventHandler(this.LT64_cb_CheckedChanged);
            // 
            // KeyCreation
            // 
            this.AcceptButton = this.OKButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(349, 143);
            this.Controls.Add(this.LT64_cb);
            this.Controls.Add(this.CancelButton1);
            this.Controls.Add(this.OKButton);
            this.Controls.Add(this.secretTextBox);
            this.Controls.Add(this.promptLabel);
            this.MaximizeBox = false;
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
    }
}

