namespace AlarmTo
{
    partial class FormHelp
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormHelp));
            this.HelpTxt = new System.Windows.Forms.TextBox();
            this.CloseBTN = new System.Windows.Forms.Button();
            this.GoGitBTN = new System.Windows.Forms.Button();
            this.BTN_tips = new System.Windows.Forms.Button();
            this.BTN_Lic = new System.Windows.Forms.Button();
            this.LicenseTxt = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // HelpTxt
            // 
            this.HelpTxt.BackColor = System.Drawing.Color.White;
            this.HelpTxt.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.HelpTxt.Location = new System.Drawing.Point(12, 12);
            this.HelpTxt.Multiline = true;
            this.HelpTxt.Name = "HelpTxt";
            this.HelpTxt.ReadOnly = true;
            this.HelpTxt.Size = new System.Drawing.Size(644, 468);
            this.HelpTxt.TabIndex = 1;
            this.HelpTxt.Text = resources.GetString("HelpTxt.Text");
            // 
            // CloseBTN
            // 
            this.CloseBTN.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.CloseBTN.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.CloseBTN.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.CloseBTN.Location = new System.Drawing.Point(335, 486);
            this.CloseBTN.Name = "CloseBTN";
            this.CloseBTN.Size = new System.Drawing.Size(119, 39);
            this.CloseBTN.TabIndex = 0;
            this.CloseBTN.Text = "Close";
            this.CloseBTN.UseVisualStyleBackColor = false;
            this.CloseBTN.Click += new System.EventHandler(this.CloseBTN_Click);
            // 
            // GoGitBTN
            // 
            this.GoGitBTN.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.GoGitBTN.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.GoGitBTN.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.GoGitBTN.Location = new System.Drawing.Point(460, 486);
            this.GoGitBTN.Name = "GoGitBTN";
            this.GoGitBTN.Size = new System.Drawing.Size(119, 39);
            this.GoGitBTN.TabIndex = 2;
            this.GoGitBTN.Text = "Go github";
            this.GoGitBTN.UseVisualStyleBackColor = false;
            this.GoGitBTN.Click += new System.EventHandler(this.GoGitBTN_Click);
            // 
            // BTN_tips
            // 
            this.BTN_tips.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.BTN_tips.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.BTN_tips.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BTN_tips.Location = new System.Drawing.Point(85, 486);
            this.BTN_tips.Name = "BTN_tips";
            this.BTN_tips.Size = new System.Drawing.Size(119, 39);
            this.BTN_tips.TabIndex = 3;
            this.BTN_tips.Text = "Help";
            this.BTN_tips.UseVisualStyleBackColor = false;
            this.BTN_tips.Click += new System.EventHandler(this.BTN_tips_Click);
            // 
            // BTN_Lic
            // 
            this.BTN_Lic.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.BTN_Lic.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.BTN_Lic.Font = new System.Drawing.Font("Consolas", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BTN_Lic.Location = new System.Drawing.Point(210, 486);
            this.BTN_Lic.Name = "BTN_Lic";
            this.BTN_Lic.Size = new System.Drawing.Size(119, 39);
            this.BTN_Lic.TabIndex = 4;
            this.BTN_Lic.Text = "License";
            this.BTN_Lic.UseVisualStyleBackColor = false;
            this.BTN_Lic.Click += new System.EventHandler(this.BTN_Lic_Click);
            // 
            // LicenseTxt
            // 
            this.LicenseTxt.BackColor = System.Drawing.Color.White;
            this.LicenseTxt.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LicenseTxt.Location = new System.Drawing.Point(12, 12);
            this.LicenseTxt.Multiline = true;
            this.LicenseTxt.Name = "LicenseTxt";
            this.LicenseTxt.ReadOnly = true;
            this.LicenseTxt.Size = new System.Drawing.Size(644, 468);
            this.LicenseTxt.TabIndex = 5;
            this.LicenseTxt.Text = resources.GetString("LicenseTxt.Text");
            this.LicenseTxt.Visible = false;
            // 
            // FormHelp
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.Color.Black;
            this.ClientSize = new System.Drawing.Size(668, 532);
            this.ControlBox = false;
            this.Controls.Add(this.BTN_Lic);
            this.Controls.Add(this.BTN_tips);
            this.Controls.Add(this.GoGitBTN);
            this.Controls.Add(this.CloseBTN);
            this.Controls.Add(this.HelpTxt);
            this.Controls.Add(this.LicenseTxt);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormHelp";
            this.ShowIcon = false;
            this.Text = "Quick Help";
            this.Load += new System.EventHandler(this.FormHelp_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox HelpTxt;
        private System.Windows.Forms.Button CloseBTN;
        private System.Windows.Forms.Button GoGitBTN;
        private System.Windows.Forms.Button BTN_tips;
        private System.Windows.Forms.Button BTN_Lic;
        private System.Windows.Forms.TextBox LicenseTxt;
    }
}