
namespace FZ4P
{
    partial class F_Manage
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(F_Manage));
            System.Windows.Forms.DataVisualization.Charting.ChartArea chartArea1 = new System.Windows.Forms.DataVisualization.Charting.ChartArea();
            System.Windows.Forms.DataVisualization.Charting.Legend legend1 = new System.Windows.Forms.DataVisualization.Charting.Legend();
            System.Windows.Forms.DataVisualization.Charting.Series series1 = new System.Windows.Forms.DataVisualization.Charting.Series();
            System.Windows.Forms.DataVisualization.Charting.Title title1 = new System.Windows.Forms.DataVisualization.Charting.Title();
            this.lblRepeatLoadingUnloading = new System.Windows.Forms.Label();
            this.RepeatRunCnt = new System.Windows.Forms.TextBox();
            this.CurrentRunCnt = new System.Windows.Forms.TextBox();
            this.SetSampleNumber = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.NewSampleNumber = new System.Windows.Forms.TextBox();
            this.LastSampleNum = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.btnCheckContact = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.SuddenStop = new System.Windows.Forms.Button();
            this.RepeatStartTest = new System.Windows.Forms.Button();
            this.ToAdmin = new System.Windows.Forms.Button();
            this.ToVision = new System.Windows.Forms.Button();
            this.p_Result = new System.Windows.Forms.Panel();
            this.RunProgress = new System.Windows.Forms.PictureBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label2 = new System.Windows.Forms.Label();
            this.lblMCnum = new System.Windows.Forms.Label();
            this.lblMcNo = new System.Windows.Forms.Label();
            this.lblCspec = new System.Windows.Forms.Label();
            this.lblSpec = new System.Windows.Forms.Label();
            this.lblCrecipe = new System.Windows.Forms.Label();
            this.lblRecipe = new System.Windows.Forms.Label();
            this.lblPGMver = new System.Windows.Forms.Label();
            this.lblPGver = new System.Windows.Forms.Label();
            this.YieldChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.ModelGroup = new System.Windows.Forms.Panel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.lbMcConstatus = new System.Windows.Forms.Label();
            this.lbMCtype = new System.Windows.Forms.Label();
            this.lblCheckPoint = new System.Windows.Forms.Label();
            this.pResult2 = new System.Windows.Forms.Panel();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RunProgress)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.YieldChart)).BeginInit();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblRepeatLoadingUnloading
            // 
            this.lblRepeatLoadingUnloading.AutoSize = true;
            this.lblRepeatLoadingUnloading.Font = new System.Drawing.Font("맑은 고딕", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblRepeatLoadingUnloading.Location = new System.Drawing.Point(6, 16);
            this.lblRepeatLoadingUnloading.Name = "lblRepeatLoadingUnloading";
            this.lblRepeatLoadingUnloading.Size = new System.Drawing.Size(274, 25);
            this.lblRepeatLoadingUnloading.TabIndex = 150;
            this.lblRepeatLoadingUnloading.Text = "Repeat Loading/Unloading #";
            // 
            // RepeatRunCnt
            // 
            this.RepeatRunCnt.Font = new System.Drawing.Font("맑은 고딕", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.RepeatRunCnt.Location = new System.Drawing.Point(286, 13);
            this.RepeatRunCnt.Name = "RepeatRunCnt";
            this.RepeatRunCnt.Size = new System.Drawing.Size(80, 33);
            this.RepeatRunCnt.TabIndex = 149;
            this.RepeatRunCnt.Text = "1";
            this.RepeatRunCnt.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // CurrentRunCnt
            // 
            this.CurrentRunCnt.Font = new System.Drawing.Font("맑은 고딕", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.CurrentRunCnt.ForeColor = System.Drawing.Color.Red;
            this.CurrentRunCnt.Location = new System.Drawing.Point(384, 13);
            this.CurrentRunCnt.Name = "CurrentRunCnt";
            this.CurrentRunCnt.ReadOnly = true;
            this.CurrentRunCnt.Size = new System.Drawing.Size(80, 33);
            this.CurrentRunCnt.TabIndex = 151;
            this.CurrentRunCnt.Text = "1";
            this.CurrentRunCnt.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // SetSampleNumber
            // 
            this.SetSampleNumber.BackColor = System.Drawing.Color.DarkOliveGreen;
            this.SetSampleNumber.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("SetSampleNumber.BackgroundImage")));
            this.SetSampleNumber.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.SetSampleNumber.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.SetSampleNumber.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold);
            this.SetSampleNumber.ForeColor = System.Drawing.Color.Black;
            this.SetSampleNumber.Location = new System.Drawing.Point(0, 68);
            this.SetSampleNumber.Name = "SetSampleNumber";
            this.SetSampleNumber.Size = new System.Drawing.Size(174, 45);
            this.SetSampleNumber.TabIndex = 156;
            this.SetSampleNumber.Text = "Set Sample No.";
            this.SetSampleNumber.UseVisualStyleBackColor = false;
            this.SetSampleNumber.Click += new System.EventHandler(this.SetSampleNumber_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold);
            this.label3.Location = new System.Drawing.Point(246, 83);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(162, 21);
            this.label3.TabIndex = 155;
            this.label3.Text = "Last Tested SPL No.";
            // 
            // NewSampleNumber
            // 
            this.NewSampleNumber.BackColor = System.Drawing.Color.Cornsilk;
            this.NewSampleNumber.Font = new System.Drawing.Font("맑은 고딕", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.NewSampleNumber.ForeColor = System.Drawing.Color.Olive;
            this.NewSampleNumber.Location = new System.Drawing.Point(183, 77);
            this.NewSampleNumber.Name = "NewSampleNumber";
            this.NewSampleNumber.Size = new System.Drawing.Size(52, 33);
            this.NewSampleNumber.TabIndex = 154;
            this.NewSampleNumber.Text = "9999";
            this.NewSampleNumber.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // LastSampleNum
            // 
            this.LastSampleNum.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.LastSampleNum.Font = new System.Drawing.Font("맑은 고딕", 16F, System.Drawing.FontStyle.Bold);
            this.LastSampleNum.Location = new System.Drawing.Point(411, 76);
            this.LastSampleNum.Name = "LastSampleNum";
            this.LastSampleNum.ReadOnly = true;
            this.LastSampleNum.Size = new System.Drawing.Size(53, 29);
            this.LastSampleNum.TabIndex = 153;
            this.LastSampleNum.Text = "9999";
            this.LastSampleNum.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // groupBox2
            // 
            this.groupBox2.BackColor = System.Drawing.Color.Gainsboro;
            this.groupBox2.Controls.Add(this.SetSampleNumber);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.NewSampleNumber);
            this.groupBox2.Controls.Add(this.LastSampleNum);
            this.groupBox2.Location = new System.Drawing.Point(7, 747);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(470, 122);
            this.groupBox2.TabIndex = 194;
            this.groupBox2.TabStop = false;
            // 
            // label4
            // 
            this.label4.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.label4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label4.Font = new System.Drawing.Font("Arial", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.Color.Yellow;
            this.label4.Location = new System.Drawing.Point(0, 4);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(474, 55);
            this.label4.TabIndex = 259;
            this.label4.Text = "SU2810";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnCheckContact
            // 
            this.btnCheckContact.BackColor = System.Drawing.Color.MidnightBlue;
            this.btnCheckContact.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnCheckContact.BackgroundImage")));
            this.btnCheckContact.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnCheckContact.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnCheckContact.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold);
            this.btnCheckContact.ForeColor = System.Drawing.Color.Black;
            this.btnCheckContact.Location = new System.Drawing.Point(1739, 784);
            this.btnCheckContact.Name = "btnCheckContact";
            this.btnCheckContact.Size = new System.Drawing.Size(178, 45);
            this.btnCheckContact.TabIndex = 165;
            this.btnCheckContact.Text = "Check Pin Contact";
            this.btnCheckContact.UseVisualStyleBackColor = false;
            this.btnCheckContact.Click += new System.EventHandler(this.btnCheckContact_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lblRepeatLoadingUnloading);
            this.groupBox1.Controls.Add(this.RepeatRunCnt);
            this.groupBox1.Controls.Add(this.CurrentRunCnt);
            this.groupBox1.Location = new System.Drawing.Point(7, 869);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(470, 53);
            this.groupBox1.TabIndex = 193;
            this.groupBox1.TabStop = false;
            // 
            // SuddenStop
            // 
            this.SuddenStop.BackColor = System.Drawing.Color.DarkRed;
            this.SuddenStop.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("SuddenStop.BackgroundImage")));
            this.SuddenStop.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.SuddenStop.Font = new System.Drawing.Font("맑은 고딕", 32.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SuddenStop.ForeColor = System.Drawing.Color.White;
            this.SuddenStop.Location = new System.Drawing.Point(241, 923);
            this.SuddenStop.Name = "SuddenStop";
            this.SuddenStop.Size = new System.Drawing.Size(236, 89);
            this.SuddenStop.TabIndex = 196;
            this.SuddenStop.Text = "Halt";
            this.SuddenStop.UseVisualStyleBackColor = false;
            this.SuddenStop.Click += new System.EventHandler(this.SuddenStop_Click);
            // 
            // RepeatStartTest
            // 
            this.RepeatStartTest.BackColor = System.Drawing.Color.RoyalBlue;
            this.RepeatStartTest.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("RepeatStartTest.BackgroundImage")));
            this.RepeatStartTest.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.RepeatStartTest.Font = new System.Drawing.Font("맑은 고딕", 32F, System.Drawing.FontStyle.Bold);
            this.RepeatStartTest.ForeColor = System.Drawing.Color.White;
            this.RepeatStartTest.Location = new System.Drawing.Point(3, 923);
            this.RepeatStartTest.Name = "RepeatStartTest";
            this.RepeatStartTest.Size = new System.Drawing.Size(236, 89);
            this.RepeatStartTest.TabIndex = 176;
            this.RepeatStartTest.Text = "Repeat";
            this.RepeatStartTest.UseVisualStyleBackColor = false;
            this.RepeatStartTest.Click += new System.EventHandler(this.RepeatStartTest_Click);
            // 
            // ToAdmin
            // 
            this.ToAdmin.BackColor = System.Drawing.Color.DeepSkyBlue;
            this.ToAdmin.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ToAdmin.BackgroundImage")));
            this.ToAdmin.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ToAdmin.Font = new System.Drawing.Font("Calibri", 18F, System.Drawing.FontStyle.Bold);
            this.ToAdmin.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.ToAdmin.Location = new System.Drawing.Point(309, 0);
            this.ToAdmin.Name = "ToAdmin";
            this.ToAdmin.Size = new System.Drawing.Size(320, 37);
            this.ToAdmin.TabIndex = 179;
            this.ToAdmin.Text = "Admin Mode";
            this.ToAdmin.UseVisualStyleBackColor = false;
            this.ToAdmin.Click += new System.EventHandler(this.ToAdmin_Click);
            // 
            // ToVision
            // 
            this.ToVision.BackColor = System.Drawing.Color.DodgerBlue;
            this.ToVision.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ToVision.BackgroundImage")));
            this.ToVision.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ToVision.Font = new System.Drawing.Font("Calibri", 18F, System.Drawing.FontStyle.Bold);
            this.ToVision.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.ToVision.Location = new System.Drawing.Point(635, 0);
            this.ToVision.Name = "ToVision";
            this.ToVision.Size = new System.Drawing.Size(320, 37);
            this.ToVision.TabIndex = 178;
            this.ToVision.Text = "Vision";
            this.ToVision.UseVisualStyleBackColor = false;
            this.ToVision.Click += new System.EventHandler(this.ToVision_Click);
            // 
            // p_Result
            // 
            this.p_Result.Location = new System.Drawing.Point(961, 43);
            this.p_Result.Name = "p_Result";
            this.p_Result.Size = new System.Drawing.Size(772, 788);
            this.p_Result.TabIndex = 175;
            // 
            // RunProgress
            // 
            this.RunProgress.BackColor = System.Drawing.Color.Transparent;
            this.RunProgress.Image = ((System.Drawing.Image)(resources.GetObject("RunProgress.Image")));
            this.RunProgress.Location = new System.Drawing.Point(7, 684);
            this.RunProgress.Name = "RunProgress";
            this.RunProgress.Size = new System.Drawing.Size(470, 57);
            this.RunProgress.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.RunProgress.TabIndex = 244;
            this.RunProgress.TabStop = false;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.Transparent;
            this.tableLayoutPanel1.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 19.56067F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 80.43933F));
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblMCnum, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.lblMcNo, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.lblCspec, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.lblSpec, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.lblCrecipe, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.lblRecipe, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.lblPGMver, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblPGver, 0, 1);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(961, 835);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 6;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 19.99998F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20.00001F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 5F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(951, 176);
            this.tableLayoutPanel1.TabIndex = 254;
            // 
            // label2
            // 
            this.label2.BackColor = System.Drawing.Color.YellowGreen;
            this.tableLayoutPanel1.SetColumnSpan(this.label2, 2);
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.Black;
            this.label2.Location = new System.Drawing.Point(4, 1);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(943, 32);
            this.label2.TabIndex = 192;
            this.label2.Text = "Driving Information";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblMCnum
            // 
            this.lblMCnum.BackColor = System.Drawing.Color.White;
            this.lblMCnum.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblMCnum.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMCnum.ForeColor = System.Drawing.Color.Blue;
            this.lblMCnum.Location = new System.Drawing.Point(190, 133);
            this.lblMCnum.Name = "lblMCnum";
            this.lblMCnum.Size = new System.Drawing.Size(757, 32);
            this.lblMCnum.TabIndex = 192;
            this.lblMCnum.Text = "MC Number";
            this.lblMCnum.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblMcNo
            // 
            this.lblMcNo.BackColor = System.Drawing.Color.Thistle;
            this.lblMcNo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblMcNo.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMcNo.Location = new System.Drawing.Point(4, 133);
            this.lblMcNo.Name = "lblMcNo";
            this.lblMcNo.Size = new System.Drawing.Size(179, 32);
            this.lblMcNo.TabIndex = 189;
            this.lblMcNo.Text = "Tester No.";
            this.lblMcNo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblCspec
            // 
            this.lblCspec.BackColor = System.Drawing.Color.White;
            this.lblCspec.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCspec.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCspec.ForeColor = System.Drawing.Color.Blue;
            this.lblCspec.Location = new System.Drawing.Point(190, 100);
            this.lblCspec.Name = "lblCspec";
            this.lblCspec.Size = new System.Drawing.Size(757, 32);
            this.lblCspec.TabIndex = 184;
            this.lblCspec.Text = "CurrentSpec";
            this.lblCspec.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblSpec
            // 
            this.lblSpec.BackColor = System.Drawing.Color.Thistle;
            this.lblSpec.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSpec.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSpec.Location = new System.Drawing.Point(4, 100);
            this.lblSpec.Name = "lblSpec";
            this.lblSpec.Size = new System.Drawing.Size(179, 32);
            this.lblSpec.TabIndex = 187;
            this.lblSpec.Text = "Spec";
            this.lblSpec.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblCrecipe
            // 
            this.lblCrecipe.BackColor = System.Drawing.Color.White;
            this.lblCrecipe.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCrecipe.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCrecipe.ForeColor = System.Drawing.Color.Blue;
            this.lblCrecipe.Location = new System.Drawing.Point(190, 67);
            this.lblCrecipe.Name = "lblCrecipe";
            this.lblCrecipe.Size = new System.Drawing.Size(757, 32);
            this.lblCrecipe.TabIndex = 190;
            this.lblCrecipe.Text = "CurrentRecipe";
            this.lblCrecipe.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblRecipe
            // 
            this.lblRecipe.BackColor = System.Drawing.Color.Thistle;
            this.lblRecipe.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblRecipe.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRecipe.Location = new System.Drawing.Point(4, 67);
            this.lblRecipe.Name = "lblRecipe";
            this.lblRecipe.Size = new System.Drawing.Size(179, 32);
            this.lblRecipe.TabIndex = 186;
            this.lblRecipe.Text = "Recipe";
            this.lblRecipe.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblPGMver
            // 
            this.lblPGMver.BackColor = System.Drawing.Color.White;
            this.lblPGMver.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPGMver.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPGMver.ForeColor = System.Drawing.Color.Blue;
            this.lblPGMver.Location = new System.Drawing.Point(190, 34);
            this.lblPGMver.Name = "lblPGMver";
            this.lblPGMver.Size = new System.Drawing.Size(757, 32);
            this.lblPGMver.TabIndex = 191;
            this.lblPGMver.Text = "Pogram Ver";
            this.lblPGMver.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblPGver
            // 
            this.lblPGver.BackColor = System.Drawing.Color.Thistle;
            this.lblPGver.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPGver.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPGver.Location = new System.Drawing.Point(4, 34);
            this.lblPGver.Name = "lblPGver";
            this.lblPGver.Size = new System.Drawing.Size(179, 32);
            this.lblPGver.TabIndex = 185;
            this.lblPGver.Text = "Pogram Ver.";
            this.lblPGver.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // YieldChart
            // 
            this.YieldChart.AllowDrop = true;
            this.YieldChart.BackImageAlignment = System.Windows.Forms.DataVisualization.Charting.ChartImageAlignmentStyle.Center;
            this.YieldChart.BorderlineColor = System.Drawing.Color.Black;
            chartArea1.BackColor = System.Drawing.Color.Transparent;
            chartArea1.Name = "ChartArea1";
            chartArea1.ShadowColor = System.Drawing.Color.White;
            this.YieldChart.ChartAreas.Add(chartArea1);
            legend1.Alignment = System.Drawing.StringAlignment.Center;
            legend1.BackColor = System.Drawing.Color.Transparent;
            legend1.BackSecondaryColor = System.Drawing.Color.Transparent;
            legend1.BorderColor = System.Drawing.Color.Transparent;
            legend1.BorderWidth = 0;
            legend1.DockedToChartArea = "ChartArea1";
            legend1.Docking = System.Windows.Forms.DataVisualization.Charting.Docking.Bottom;
            legend1.Font = new System.Drawing.Font("맑은 고딕", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            legend1.IsTextAutoFit = false;
            legend1.Name = "Legend1";
            legend1.Position.Auto = false;
            legend1.Position.Height = 25F;
            legend1.Position.Width = 98F;
            legend1.Position.Y = 75F;
            legend1.ShadowColor = System.Drawing.Color.White;
            legend1.TitleBackColor = System.Drawing.Color.Transparent;
            legend1.TitleFont = new System.Drawing.Font("Calibri", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.YieldChart.Legends.Add(legend1);
            this.YieldChart.Location = new System.Drawing.Point(480, 684);
            this.YieldChart.Name = "YieldChart";
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie;
            series1.Legend = "Legend1";
            series1.Name = "Series5";
            this.YieldChart.Series.Add(series1);
            this.YieldChart.Size = new System.Drawing.Size(475, 324);
            this.YieldChart.TabIndex = 255;
            this.YieldChart.Text = "YieldChart";
            title1.Alignment = System.Drawing.ContentAlignment.TopLeft;
            title1.BackColor = System.Drawing.Color.Transparent;
            title1.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            title1.Name = "Title1";
            title1.Position.Auto = false;
            title1.Position.Height = 8F;
            title1.Position.Width = 55F;
            title1.Text = "Yield";
            title1.TextStyle = System.Windows.Forms.DataVisualization.Charting.TextStyle.Shadow;
            this.YieldChart.Titles.Add(title1);
            this.YieldChart.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.YieldChart_MouseDoubleClick);
            // 
            // ModelGroup
            // 
            this.ModelGroup.BackColor = System.Drawing.SystemColors.ControlLight;
            this.ModelGroup.Location = new System.Drawing.Point(1739, 43);
            this.ModelGroup.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ModelGroup.Name = "ModelGroup";
            this.ModelGroup.Size = new System.Drawing.Size(178, 734);
            this.ModelGroup.TabIndex = 256;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 80F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.tableLayoutPanel2.Controls.Add(this.lbMcConstatus, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.lbMCtype, 0, 0);
            this.tableLayoutPanel2.Location = new System.Drawing.Point(7, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(296, 33);
            this.tableLayoutPanel2.TabIndex = 257;
            // 
            // lbMcConstatus
            // 
            this.lbMcConstatus.BackColor = System.Drawing.Color.Red;
            this.lbMcConstatus.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lbMcConstatus.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbMcConstatus.Location = new System.Drawing.Point(239, 0);
            this.lbMcConstatus.Name = "lbMcConstatus";
            this.lbMcConstatus.Size = new System.Drawing.Size(54, 33);
            this.lbMcConstatus.TabIndex = 1;
            this.lbMcConstatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lbMCtype
            // 
            this.lbMCtype.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.lbMCtype.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbMCtype.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lbMCtype.Font = new System.Drawing.Font("Arial", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbMCtype.ForeColor = System.Drawing.Color.Yellow;
            this.lbMCtype.Location = new System.Drawing.Point(3, 0);
            this.lbMCtype.Name = "lbMCtype";
            this.lbMCtype.Size = new System.Drawing.Size(230, 33);
            this.lbMCtype.TabIndex = 0;
            this.lbMCtype.Text = "Normal";
            this.lbMCtype.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblCheckPoint
            // 
            this.lblCheckPoint.BackColor = System.Drawing.Color.MediumBlue;
            this.lblCheckPoint.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblCheckPoint.Font = new System.Drawing.Font("Calibri", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCheckPoint.ForeColor = System.Drawing.Color.White;
            this.lblCheckPoint.Location = new System.Drawing.Point(1739, 1);
            this.lblCheckPoint.Name = "lblCheckPoint";
            this.lblCheckPoint.Size = new System.Drawing.Size(178, 38);
            this.lblCheckPoint.TabIndex = 258;
            this.lblCheckPoint.Text = "Check Points";
            this.lblCheckPoint.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pResult2
            // 
            this.pResult2.Location = new System.Drawing.Point(961, 3);
            this.pResult2.Name = "pResult2";
            this.pResult2.Size = new System.Drawing.Size(772, 34);
            this.pResult2.TabIndex = 259;
            // 
            // F_Manage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1920, 1080);
            this.Controls.Add(this.pResult2);
            this.Controls.Add(this.btnCheckContact);
            this.Controls.Add(this.lblCheckPoint);
            this.Controls.Add(this.tableLayoutPanel2);
            this.Controls.Add(this.RunProgress);
            this.Controls.Add(this.ModelGroup);
            this.Controls.Add(this.YieldChart);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.SuddenStop);
            this.Controls.Add(this.RepeatStartTest);
            this.Controls.Add(this.ToAdmin);
            this.Controls.Add(this.ToVision);
            this.Controls.Add(this.p_Result);
            this.Controls.Add(this.groupBox2);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "F_Manage";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Load += new System.EventHandler(this.F_Manage_Load);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RunProgress)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.YieldChart)).EndInit();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Label lblRepeatLoadingUnloading;
        private System.Windows.Forms.TextBox RepeatRunCnt;
        private System.Windows.Forms.TextBox CurrentRunCnt;
        private System.Windows.Forms.Button SetSampleNumber;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox NewSampleNumber;
        private System.Windows.Forms.TextBox LastSampleNum;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnCheckContact;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button SuddenStop;
        private System.Windows.Forms.Button RepeatStartTest;
        private System.Windows.Forms.Button ToAdmin;
        private System.Windows.Forms.Button ToVision;
        private System.Windows.Forms.Panel p_Result;
        private System.Windows.Forms.PictureBox RunProgress;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        public System.Windows.Forms.Label label2;
        public System.Windows.Forms.Label lblMCnum;
        private System.Windows.Forms.Label lblMcNo;
        public System.Windows.Forms.Label lblCspec;
        private System.Windows.Forms.Label lblSpec;
        public System.Windows.Forms.Label lblCrecipe;
        private System.Windows.Forms.Label lblRecipe;
        public System.Windows.Forms.Label lblPGMver;
        private System.Windows.Forms.Label lblPGver;
        private System.Windows.Forms.DataVisualization.Charting.Chart YieldChart;
        private System.Windows.Forms.Panel ModelGroup;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label lbMcConstatus;
        private System.Windows.Forms.Label lbMCtype;
        private System.Windows.Forms.Label lblCheckPoint;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Panel pResult2;
    }
}