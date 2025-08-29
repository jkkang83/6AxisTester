
namespace FZ4P
{
    partial class TestItemOnOff
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            this.TestItemSaveBtn = new System.Windows.Forms.Button();
            this.TestItemGrid = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.TestItemGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // TestItemSaveBtn
            // 
            this.TestItemSaveBtn.BackColor = System.Drawing.Color.DeepSkyBlue;
            this.TestItemSaveBtn.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TestItemSaveBtn.Location = new System.Drawing.Point(0, -2);
            this.TestItemSaveBtn.Name = "TestItemSaveBtn";
            this.TestItemSaveBtn.Size = new System.Drawing.Size(474, 36);
            this.TestItemSaveBtn.TabIndex = 1;
            this.TestItemSaveBtn.Text = "Save Test Item Setting";
            this.TestItemSaveBtn.UseVisualStyleBackColor = false;
            this.TestItemSaveBtn.Click += new System.EventHandler(this.TestItemSaveBtn_Click);
            // 
            // TestItemGrid
            // 
            this.TestItemGrid.AllowUserToAddRows = false;
            this.TestItemGrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.TestItemGrid.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Gulim", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.TestItemGrid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.TestItemGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.TestItemGrid.Location = new System.Drawing.Point(0, 32);
            this.TestItemGrid.Name = "TestItemGrid";
            this.TestItemGrid.RowTemplate.Height = 23;
            this.TestItemGrid.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.TestItemGrid.Size = new System.Drawing.Size(474, 800);
            this.TestItemGrid.TabIndex = 79;
            this.TestItemGrid.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.TestItemGrid_CellDoubleClick);
            // 
            // TestItemOnOff
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(474, 835);
            this.ControlBox = false;
            this.Controls.Add(this.TestItemGrid);
            this.Controls.Add(this.TestItemSaveBtn);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "TestItemOnOff";
            this.Text = "Test Item Selection";
            this.Load += new System.EventHandler(this.TestItemOnOff_Load);
            ((System.ComponentModel.ISupportInitialize)(this.TestItemGrid)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button TestItemSaveBtn;
        private System.Windows.Forms.DataGridView TestItemGrid;
    }
}

