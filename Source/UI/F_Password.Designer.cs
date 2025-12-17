namespace FZ4P
{
    partial class F_Password
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

        private void InitializeComponent()
        {
            this.pnlHeader = new System.Windows.Forms.Panel();
            this.lbMCtype = new System.Windows.Forms.Label();
            this.tlpMain = new System.Windows.Forms.TableLayoutPanel();
            this.lblPassword = new System.Windows.Forms.Label();
            this.tbPW = new System.Windows.Forms.TextBox();
            this.btnLogIn = new System.Windows.Forms.Button();
            this.chkChangePW = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tbCurrPW = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tbChangePW = new System.Windows.Forms.TextBox();
            this.btnChangePW = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.pnlHeader.SuspendLayout();
            this.tlpMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlHeader
            // 
            this.pnlHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(30)))), ((int)(((byte)(34)))));
            this.pnlHeader.Controls.Add(this.button1);
            this.pnlHeader.Controls.Add(this.lbMCtype);
            this.pnlHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlHeader.Margin = new System.Windows.Forms.Padding(0);
            this.pnlHeader.Name = "pnlHeader";
            this.pnlHeader.Size = new System.Drawing.Size(498, 48);
            this.pnlHeader.TabIndex = 0;
            // 
            // lbMCtype
            // 
            this.lbMCtype.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lbMCtype.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(222)))), ((int)(((byte)(222)))));
            this.lbMCtype.Location = new System.Drawing.Point(0, 0);
            this.lbMCtype.Name = "lbMCtype";
            this.lbMCtype.Size = new System.Drawing.Size(446, 48);
            this.lbMCtype.TabIndex = 0;
            this.lbMCtype.Text = "LOG IN";
            this.lbMCtype.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tlpMain
            // 
            this.tlpMain.ColumnCount = 3;
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 28F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 52F));
            this.tlpMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tlpMain.Controls.Add(this.lblPassword, 0, 0);
            this.tlpMain.Controls.Add(this.tbPW, 1, 0);
            this.tlpMain.Controls.Add(this.btnLogIn, 2, 0);
            this.tlpMain.Controls.Add(this.chkChangePW, 0, 1);
            this.tlpMain.Controls.Add(this.label1, 0, 2);
            this.tlpMain.Controls.Add(this.tbCurrPW, 1, 2);
            this.tlpMain.Controls.Add(this.label2, 0, 3);
            this.tlpMain.Controls.Add(this.tbChangePW, 1, 3);
            this.tlpMain.Controls.Add(this.btnChangePW, 2, 3);
            this.tlpMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tlpMain.Location = new System.Drawing.Point(0, 48);
            this.tlpMain.Margin = new System.Windows.Forms.Padding(8);
            this.tlpMain.Name = "tlpMain";
            this.tlpMain.Padding = new System.Windows.Forms.Padding(20, 14, 20, 18);
            this.tlpMain.RowCount = 6;
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 34F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 44F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 12F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.tlpMain.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tlpMain.Size = new System.Drawing.Size(498, 213);
            this.tlpMain.TabIndex = 1;
            // 
            // lblPassword
            // 
            this.lblPassword.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.lblPassword.AutoSize = true;
            this.lblPassword.Font = new System.Drawing.Font("Arial", 10F, System.Drawing.FontStyle.Bold);
            this.lblPassword.Location = new System.Drawing.Point(23, 31);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(75, 16);
            this.lblPassword.TabIndex = 0;
            this.lblPassword.Text = "Password";
            // 
            // tbPW
            // 
            this.tbPW.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbPW.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(255)))), ((int)(((byte)(230)))));
            this.tbPW.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.tbPW.Location = new System.Drawing.Point(154, 26);
            this.tbPW.Margin = new System.Windows.Forms.Padding(6, 3, 6, 3);
            this.tbPW.Name = "tbPW";
            this.tbPW.Size = new System.Drawing.Size(226, 25);
            this.tbPW.TabIndex = 1;
            this.tbPW.UseSystemPasswordChar = true;
            // 
            // btnLogIn
            // 
            this.btnLogIn.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnLogIn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(76)))), ((int)(((byte)(175)))), ((int)(((byte)(80)))));
            this.btnLogIn.FlatAppearance.BorderSize = 0;
            this.btnLogIn.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(56)))), ((int)(((byte)(145)))), ((int)(((byte)(60)))));
            this.btnLogIn.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(86)))), ((int)(((byte)(185)))), ((int)(((byte)(90)))));
            this.btnLogIn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLogIn.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnLogIn.ForeColor = System.Drawing.Color.White;
            this.btnLogIn.Location = new System.Drawing.Point(400, 24);
            this.btnLogIn.Margin = new System.Windows.Forms.Padding(6, 3, 6, 3);
            this.btnLogIn.Name = "btnLogIn";
            this.btnLogIn.Size = new System.Drawing.Size(72, 30);
            this.btnLogIn.TabIndex = 2;
            this.btnLogIn.Text = "Log In";
            this.btnLogIn.UseVisualStyleBackColor = false;
            this.btnLogIn.Click += new System.EventHandler(this.btnLogIn_Click);
            // 
            // chkChangePW
            // 
            this.chkChangePW.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.chkChangePW.AutoSize = true;
            this.tlpMain.SetColumnSpan(this.chkChangePW, 3);
            this.chkChangePW.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.chkChangePW.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.chkChangePW.Location = new System.Drawing.Point(23, 71);
            this.chkChangePW.Margin = new System.Windows.Forms.Padding(3, 6, 3, 6);
            this.chkChangePW.Name = "chkChangePW";
            this.chkChangePW.Size = new System.Drawing.Size(119, 19);
            this.chkChangePW.TabIndex = 3;
            this.chkChangePW.Text = "Change Password";
            this.chkChangePW.UseVisualStyleBackColor = true;
            this.chkChangePW.CheckedChanged += new System.EventHandler(this.chkChangePW_CheckedChanged);
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.label1.Location = new System.Drawing.Point(23, 112);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(50, 15);
            this.label1.TabIndex = 4;
            this.label1.Text = "Current";
            this.label1.Visible = false;
            // 
            // tbCurrPW
            // 
            this.tbCurrPW.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbCurrPW.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(255)))), ((int)(((byte)(230)))));
            this.tbCurrPW.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.tbCurrPW.Location = new System.Drawing.Point(154, 107);
            this.tbCurrPW.Margin = new System.Windows.Forms.Padding(6, 3, 6, 3);
            this.tbCurrPW.Name = "tbCurrPW";
            this.tbCurrPW.Size = new System.Drawing.Size(226, 25);
            this.tbCurrPW.TabIndex = 5;
            this.tbCurrPW.UseSystemPasswordChar = true;
            this.tbCurrPW.Visible = false;
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.label2.Location = new System.Drawing.Point(23, 156);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(33, 15);
            this.label2.TabIndex = 6;
            this.label2.Text = "New";
            this.label2.Visible = false;
            // 
            // tbChangePW
            // 
            this.tbChangePW.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.tbChangePW.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(230)))), ((int)(((byte)(255)))), ((int)(((byte)(230)))));
            this.tbChangePW.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.tbChangePW.Location = new System.Drawing.Point(154, 151);
            this.tbChangePW.Margin = new System.Windows.Forms.Padding(6, 3, 6, 3);
            this.tbChangePW.Name = "tbChangePW";
            this.tbChangePW.Size = new System.Drawing.Size(226, 25);
            this.tbChangePW.TabIndex = 7;
            this.tbChangePW.UseSystemPasswordChar = true;
            this.tbChangePW.Visible = false;
            // 
            // btnChangePW
            // 
            this.btnChangePW.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.btnChangePW.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(33)))), ((int)(((byte)(150)))), ((int)(((byte)(243)))));
            this.btnChangePW.FlatAppearance.BorderSize = 0;
            this.btnChangePW.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(23)))), ((int)(((byte)(130)))), ((int)(((byte)(223)))));
            this.btnChangePW.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(43)))), ((int)(((byte)(160)))), ((int)(((byte)(253)))));
            this.btnChangePW.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnChangePW.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnChangePW.ForeColor = System.Drawing.Color.White;
            this.btnChangePW.Location = new System.Drawing.Point(400, 149);
            this.btnChangePW.Margin = new System.Windows.Forms.Padding(6, 3, 6, 3);
            this.btnChangePW.Name = "btnChangePW";
            this.btnChangePW.Size = new System.Drawing.Size(72, 30);
            this.btnChangePW.TabIndex = 8;
            this.btnChangePW.Text = "Change";
            this.btnChangePW.UseVisualStyleBackColor = false;
            this.btnChangePW.Visible = false;
            this.btnChangePW.Click += new System.EventHandler(this.btnChangePW_Click);
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.Red;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Font = new System.Drawing.Font("Arial", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.Location = new System.Drawing.Point(447, 0);
            this.button1.Margin = new System.Windows.Forms.Padding(0);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(51, 48);
            this.button1.TabIndex = 1;
            this.button1.Text = "X";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // F_Password
            // 
            this.AcceptButton = this.btnLogIn;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(498, 261);
            this.ControlBox = false;
            this.Controls.Add(this.tlpMain);
            this.Controls.Add(this.pnlHeader);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "F_Password";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.pnlHeader.ResumeLayout(false);
            this.tlpMain.ResumeLayout(false);
            this.tlpMain.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlHeader;
        private System.Windows.Forms.Label lbMCtype;
        private System.Windows.Forms.TableLayoutPanel tlpMain;
        private System.Windows.Forms.Label lblPassword;
        private System.Windows.Forms.TextBox tbPW;
        private System.Windows.Forms.Button btnLogIn;
        private System.Windows.Forms.CheckBox chkChangePW;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbCurrPW;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox tbChangePW;
        private System.Windows.Forms.Button btnChangePW;
        private System.Windows.Forms.Button button1;
    }
}