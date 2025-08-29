
namespace FZ4P
{
    partial class F_Start
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(F_Start));
            this.StartLog = new System.Windows.Forms.TextBox();
            this.TilteText = new System.Windows.Forms.TextBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // StartLog
            // 
            this.StartLog.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.StartLog.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.StartLog.ForeColor = System.Drawing.Color.LemonChiffon;
            this.StartLog.Location = new System.Drawing.Point(528, 770);
            this.StartLog.Multiline = true;
            this.StartLog.Name = "StartLog";
            this.StartLog.ReadOnly = true;
            this.StartLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.StartLog.Size = new System.Drawing.Size(901, 277);
            this.StartLog.TabIndex = 331;
            // 
            // TilteText
            // 
            this.TilteText.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.TilteText.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.TilteText.Font = new System.Drawing.Font("맑은 고딕", 72F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.TilteText.ForeColor = System.Drawing.Color.LemonChiffon;
            this.TilteText.Location = new System.Drawing.Point(509, 449);
            this.TilteText.Multiline = true;
            this.TilteText.Name = "TilteText";
            this.TilteText.ReadOnly = true;
            this.TilteText.Size = new System.Drawing.Size(901, 144);
            this.TilteText.TabIndex = 332;
            this.TilteText.Text = "6 Axis Tester";
            this.TilteText.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // textBox1
            // 
            this.textBox1.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox1.Font = new System.Drawing.Font("맑은 고딕", 48F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.textBox1.ForeColor = System.Drawing.Color.RoyalBlue;
            this.textBox1.Location = new System.Drawing.Point(509, 599);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(901, 144);
            this.textBox1.TabIndex = 333;
            this.textBox1.Text = "Copy Right to ActRo";
            this.textBox1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // F_Start
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
            this.ClientSize = new System.Drawing.Size(1920, 1080);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.TilteText);
            this.Controls.Add(this.StartLog);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "F_Start";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox StartLog;
        private System.Windows.Forms.TextBox TilteText;
        private System.Windows.Forms.TextBox textBox1;
    }
}

