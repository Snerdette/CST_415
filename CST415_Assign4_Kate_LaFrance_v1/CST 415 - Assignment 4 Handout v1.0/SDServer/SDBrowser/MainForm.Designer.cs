namespace SDBrowser
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
            this.textboxAddress = new System.Windows.Forms.TextBox();
            this.buttonGo = new System.Windows.Forms.Button();
            this.textboxContent = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // textboxAddress
            // 
            this.textboxAddress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textboxAddress.Location = new System.Drawing.Point(16, 15);
            this.textboxAddress.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textboxAddress.Name = "textboxAddress";
            this.textboxAddress.Size = new System.Drawing.Size(292, 22);
            this.textboxAddress.TabIndex = 0;
            // 
            // buttonGo
            // 
            this.buttonGo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonGo.Location = new System.Drawing.Point(317, 12);
            this.buttonGo.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.buttonGo.Name = "buttonGo";
            this.buttonGo.Size = new System.Drawing.Size(45, 28);
            this.buttonGo.TabIndex = 1;
            this.buttonGo.Text = "Go!";
            this.buttonGo.UseVisualStyleBackColor = true;
            this.buttonGo.Click += new System.EventHandler(this.buttonGo_Click);
            // 
            // textboxContent
            // 
            this.textboxContent.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textboxContent.Location = new System.Drawing.Point(16, 47);
            this.textboxContent.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textboxContent.Multiline = true;
            this.textboxContent.Name = "textboxContent";
            this.textboxContent.Size = new System.Drawing.Size(345, 259);
            this.textboxContent.TabIndex = 2;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(379, 321);
            this.Controls.Add(this.textboxContent);
            this.Controls.Add(this.buttonGo);
            this.Controls.Add(this.textboxAddress);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "MainForm";
            this.Text = "SD Browser";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textboxAddress;
        private System.Windows.Forms.Button buttonGo;
        private System.Windows.Forms.TextBox textboxContent;
    }
}

