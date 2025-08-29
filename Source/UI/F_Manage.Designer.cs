
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
            this.ApplyLotID = new System.Windows.Forms.Button();
            this.btnCheckContact = new System.Windows.Forms.Button();
            this.LotID = new System.Windows.Forms.TextBox();
            this.OperatorName = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.labelCenteringStatus = new System.Windows.Forms.Label();
            this.SuddenStop = new System.Windows.Forms.Button();
            this.RepeatStartTest = new System.Windows.Forms.Button();
            this.ToAdmin = new System.Windows.Forms.Button();
            this.ToVision = new System.Windows.Forms.Button();
            this.p_Result = new System.Windows.Forms.Panel();
            this.TestCountText = new System.Windows.Forms.TextBox();
            this.TestStopBtn = new System.Windows.Forms.Button();
            this.TestStartBtn = new System.Windows.Forms.Button();
            this.btnClearLogs = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.RunProgress = new System.Windows.Forms.PictureBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label2 = new System.Windows.Forms.Label();
            this.lblMES = new System.Windows.Forms.Label();
            this.lblMCnum = new System.Windows.Forms.Label();
            this.lblMcNo = new System.Windows.Forms.Label();
            this.lblCFW = new System.Windows.Forms.Label();
            this.lblFirmware = new System.Windows.Forms.Label();
            this.lblCspec = new System.Windows.Forms.Label();
            this.lblSpec = new System.Windows.Forms.Label();
            this.lblCrecipe = new System.Windows.Forms.Label();
            this.lblRecipe = new System.Windows.Forms.Label();
            this.lblPGMver = new System.Windows.Forms.Label();
            this.lblPGver = new System.Windows.Forms.Label();
            this.lblMESFname = new System.Windows.Forms.Label();
            this.YieldChart = new System.Windows.Forms.DataVisualization.Charting.Chart();
            this.ModelGroup = new System.Windows.Forms.Panel();
            this.lblCheckPoint = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.tbBestpos = new System.Windows.Forms.TextBox();
            this.button2 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.p_Result.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RunProgress)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.YieldChart)).BeginInit();
            this.ModelGroup.SuspendLayout();
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
            this.SetSampleNumber.Location = new System.Drawing.Point(0, 63);
            this.SetSampleNumber.Name = "SetSampleNumber";
            this.SetSampleNumber.Size = new System.Drawing.Size(174, 40);
            this.SetSampleNumber.TabIndex = 156;
            this.SetSampleNumber.Text = "Set Sample No.";
            this.SetSampleNumber.UseVisualStyleBackColor = false;
            this.SetSampleNumber.Click += new System.EventHandler(this.SetSampleNumber_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold);
            this.label3.Location = new System.Drawing.Point(246, 73);
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
            this.NewSampleNumber.Location = new System.Drawing.Point(183, 67);
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
            this.LastSampleNum.Location = new System.Drawing.Point(411, 66);
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
            this.groupBox2.Controls.Add(this.ApplyLotID);
            this.groupBox2.Controls.Add(this.btnCheckContact);
            this.groupBox2.Controls.Add(this.SetSampleNumber);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.NewSampleNumber);
            this.groupBox2.Controls.Add(this.LastSampleNum);
            this.groupBox2.Controls.Add(this.LotID);
            this.groupBox2.Controls.Add(this.OperatorName);
            this.groupBox2.Location = new System.Drawing.Point(7, 752);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(470, 110);
            this.groupBox2.TabIndex = 194;
            this.groupBox2.TabStop = false;
            // 
            // ApplyLotID
            // 
            this.ApplyLotID.BackColor = System.Drawing.Color.MediumBlue;
            this.ApplyLotID.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ApplyLotID.BackgroundImage")));
            this.ApplyLotID.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ApplyLotID.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.ApplyLotID.Font = new System.Drawing.Font("Calibri", 16F, System.Drawing.FontStyle.Bold);
            this.ApplyLotID.ForeColor = System.Drawing.Color.White;
            this.ApplyLotID.Location = new System.Drawing.Point(236, 10);
            this.ApplyLotID.Name = "ApplyLotID";
            this.ApplyLotID.Size = new System.Drawing.Size(73, 45);
            this.ApplyLotID.TabIndex = 189;
            this.ApplyLotID.Text = "Apply";
            this.ApplyLotID.UseVisualStyleBackColor = false;
            // 
            // btnCheckContact
            // 
            this.btnCheckContact.BackColor = System.Drawing.Color.MidnightBlue;
            this.btnCheckContact.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnCheckContact.BackgroundImage")));
            this.btnCheckContact.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnCheckContact.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnCheckContact.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold);
            this.btnCheckContact.ForeColor = System.Drawing.Color.Black;
            this.btnCheckContact.Location = new System.Drawing.Point(311, 10);
            this.btnCheckContact.Name = "btnCheckContact";
            this.btnCheckContact.Size = new System.Drawing.Size(159, 45);
            this.btnCheckContact.TabIndex = 165;
            this.btnCheckContact.Text = "Check Pin Contact";
            this.btnCheckContact.UseVisualStyleBackColor = false;
            this.btnCheckContact.Click += new System.EventHandler(this.btnCheckContact_Click);
            // 
            // LotID
            // 
            this.LotID.BackColor = System.Drawing.Color.GreenYellow;
            this.LotID.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.LotID.ForeColor = System.Drawing.Color.Red;
            this.LotID.Location = new System.Drawing.Point(88, 36);
            this.LotID.Name = "LotID";
            this.LotID.Size = new System.Drawing.Size(142, 23);
            this.LotID.TabIndex = 185;
            // 
            // OperatorName
            // 
            this.OperatorName.BackColor = System.Drawing.Color.GreenYellow;
            this.OperatorName.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.OperatorName.ForeColor = System.Drawing.Color.Red;
            this.OperatorName.Location = new System.Drawing.Point(88, 8);
            this.OperatorName.Name = "OperatorName";
            this.OperatorName.Size = new System.Drawing.Size(142, 23);
            this.OperatorName.TabIndex = 142;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lblRepeatLoadingUnloading);
            this.groupBox1.Controls.Add(this.RepeatRunCnt);
            this.groupBox1.Controls.Add(this.CurrentRunCnt);
            this.groupBox1.Location = new System.Drawing.Point(7, 869);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(470, 46);
            this.groupBox1.TabIndex = 193;
            this.groupBox1.TabStop = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.label1.Location = new System.Drawing.Point(11, 791);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(69, 17);
            this.label1.TabIndex = 186;
            this.label1.Text = "Lot Name";
            // 
            // labelCenteringStatus
            // 
            this.labelCenteringStatus.AutoSize = true;
            this.labelCenteringStatus.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.labelCenteringStatus.ForeColor = System.Drawing.Color.Blue;
            this.labelCenteringStatus.Location = new System.Drawing.Point(10, 763);
            this.labelCenteringStatus.Name = "labelCenteringStatus";
            this.labelCenteringStatus.Size = new System.Drawing.Size(82, 17);
            this.labelCenteringStatus.TabIndex = 181;
            this.labelCenteringStatus.Text = "Operator ID";
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
            this.ToAdmin.Location = new System.Drawing.Point(2, 0);
            this.ToAdmin.Name = "ToAdmin";
            this.ToAdmin.Size = new System.Drawing.Size(475, 45);
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
            this.ToVision.Location = new System.Drawing.Point(478, 0);
            this.ToVision.Name = "ToVision";
            this.ToVision.Size = new System.Drawing.Size(477, 45);
            this.ToVision.TabIndex = 178;
            this.ToVision.Text = "Vision";
            this.ToVision.UseVisualStyleBackColor = false;
            this.ToVision.Click += new System.EventHandler(this.ToVision_Click);
            // 
            // p_Result
            // 
            this.p_Result.Controls.Add(this.TestCountText);
            this.p_Result.Controls.Add(this.TestStopBtn);
            this.p_Result.Controls.Add(this.TestStartBtn);
            this.p_Result.Location = new System.Drawing.Point(961, 1);
            this.p_Result.Name = "p_Result";
            this.p_Result.Size = new System.Drawing.Size(772, 831);
            this.p_Result.TabIndex = 175;
            // 
            // TestCountText
            // 
            this.TestCountText.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.TestCountText.Location = new System.Drawing.Point(327, 7);
            this.TestCountText.Name = "TestCountText";
            this.TestCountText.Size = new System.Drawing.Size(115, 29);
            this.TestCountText.TabIndex = 181;
            this.TestCountText.Text = "0";
            this.TestCountText.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // TestStopBtn
            // 
            this.TestStopBtn.BackColor = System.Drawing.Color.DodgerBlue;
            this.TestStopBtn.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("TestStopBtn.BackgroundImage")));
            this.TestStopBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.TestStopBtn.Font = new System.Drawing.Font("Calibri", 18F, System.Drawing.FontStyle.Bold);
            this.TestStopBtn.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.TestStopBtn.Location = new System.Drawing.Point(189, 3);
            this.TestStopBtn.Name = "TestStopBtn";
            this.TestStopBtn.Size = new System.Drawing.Size(132, 45);
            this.TestStopBtn.TabIndex = 180;
            this.TestStopBtn.Text = "Stop";
            this.TestStopBtn.UseVisualStyleBackColor = false;
            this.TestStopBtn.Click += new System.EventHandler(this.button3_Click);
            // 
            // TestStartBtn
            // 
            this.TestStartBtn.BackColor = System.Drawing.Color.DodgerBlue;
            this.TestStartBtn.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("TestStartBtn.BackgroundImage")));
            this.TestStartBtn.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.TestStartBtn.Font = new System.Drawing.Font("Calibri", 18F, System.Drawing.FontStyle.Bold);
            this.TestStartBtn.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.TestStartBtn.Location = new System.Drawing.Point(51, 3);
            this.TestStartBtn.Name = "TestStartBtn";
            this.TestStartBtn.Size = new System.Drawing.Size(132, 45);
            this.TestStartBtn.TabIndex = 179;
            this.TestStartBtn.Text = "Test";
            this.TestStartBtn.UseVisualStyleBackColor = false;
            this.TestStartBtn.Click += new System.EventHandler(this.button2_Click);
            // 
            // btnClearLogs
            // 
            this.btnClearLogs.BackColor = System.Drawing.Color.Cornsilk;
            this.btnClearLogs.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnClearLogs.BackgroundImage")));
            this.btnClearLogs.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnClearLogs.Font = new System.Drawing.Font("맑은 고딕", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClearLogs.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.btnClearLogs.Location = new System.Drawing.Point(3, 680);
            this.btnClearLogs.Name = "btnClearLogs";
            this.btnClearLogs.Size = new System.Drawing.Size(235, 60);
            this.btnClearLogs.TabIndex = 192;
            this.btnClearLogs.Text = "Clear Log";
            this.btnClearLogs.UseVisualStyleBackColor = false;
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.Gold;
            this.button1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("button1.BackgroundImage")));
            this.button1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.button1.Font = new System.Drawing.Font("맑은 고딕", 24F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.button1.Location = new System.Drawing.Point(243, 680);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(235, 60);
            this.button1.TabIndex = 199;
            this.button1.Text = "Save Log";
            this.button1.UseVisualStyleBackColor = false;
            // 
            // RunProgress
            // 
            this.RunProgress.BackColor = System.Drawing.Color.Transparent;
            this.RunProgress.Image = ((System.Drawing.Image)(resources.GetObject("RunProgress.Image")));
            this.RunProgress.Location = new System.Drawing.Point(3, 682);
            this.RunProgress.Name = "RunProgress";
            this.RunProgress.Size = new System.Drawing.Size(474, 61);
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
            this.tableLayoutPanel1.Controls.Add(this.lblMES, 0, 6);
            this.tableLayoutPanel1.Controls.Add(this.lblMCnum, 1, 5);
            this.tableLayoutPanel1.Controls.Add(this.lblMcNo, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.lblCFW, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.lblFirmware, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.lblCspec, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.lblSpec, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.lblCrecipe, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.lblRecipe, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.lblPGMver, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblPGver, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.lblMESFname, 1, 6);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(961, 838);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 8;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 14.14426F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 14.14427F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 14.14427F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 14.14427F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 14.14427F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 14.14427F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 14.14427F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 0.9900987F));
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
            this.label2.Size = new System.Drawing.Size(943, 23);
            this.label2.TabIndex = 192;
            this.label2.Text = "Driving Information";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblMES
            // 
            this.lblMES.BackColor = System.Drawing.Color.Thistle;
            this.lblMES.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblMES.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMES.Location = new System.Drawing.Point(4, 145);
            this.lblMES.Name = "lblMES";
            this.lblMES.Size = new System.Drawing.Size(179, 23);
            this.lblMES.TabIndex = 194;
            this.lblMES.Text = "MES File Name";
            this.lblMES.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblMCnum
            // 
            this.lblMCnum.BackColor = System.Drawing.Color.White;
            this.lblMCnum.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblMCnum.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMCnum.ForeColor = System.Drawing.Color.Blue;
            this.lblMCnum.Location = new System.Drawing.Point(190, 121);
            this.lblMCnum.Name = "lblMCnum";
            this.lblMCnum.Size = new System.Drawing.Size(757, 23);
            this.lblMCnum.TabIndex = 192;
            this.lblMCnum.Text = "MC Number";
            this.lblMCnum.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblMcNo
            // 
            this.lblMcNo.BackColor = System.Drawing.Color.Thistle;
            this.lblMcNo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblMcNo.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMcNo.Location = new System.Drawing.Point(4, 121);
            this.lblMcNo.Name = "lblMcNo";
            this.lblMcNo.Size = new System.Drawing.Size(179, 23);
            this.lblMcNo.TabIndex = 189;
            this.lblMcNo.Text = "Mc No.";
            this.lblMcNo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblCFW
            // 
            this.lblCFW.BackColor = System.Drawing.Color.White;
            this.lblCFW.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCFW.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCFW.ForeColor = System.Drawing.Color.Blue;
            this.lblCFW.Location = new System.Drawing.Point(190, 97);
            this.lblCFW.Name = "lblCFW";
            this.lblCFW.Size = new System.Drawing.Size(757, 23);
            this.lblCFW.TabIndex = 193;
            this.lblCFW.Text = "CurrentFW";
            this.lblCFW.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblFirmware
            // 
            this.lblFirmware.BackColor = System.Drawing.Color.Thistle;
            this.lblFirmware.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblFirmware.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFirmware.Location = new System.Drawing.Point(4, 97);
            this.lblFirmware.Name = "lblFirmware";
            this.lblFirmware.Size = new System.Drawing.Size(179, 23);
            this.lblFirmware.TabIndex = 188;
            this.lblFirmware.Text = "Firmware";
            this.lblFirmware.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblCspec
            // 
            this.lblCspec.BackColor = System.Drawing.Color.White;
            this.lblCspec.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblCspec.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCspec.ForeColor = System.Drawing.Color.Blue;
            this.lblCspec.Location = new System.Drawing.Point(190, 73);
            this.lblCspec.Name = "lblCspec";
            this.lblCspec.Size = new System.Drawing.Size(757, 23);
            this.lblCspec.TabIndex = 184;
            this.lblCspec.Text = "CurrentSpec";
            this.lblCspec.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblSpec
            // 
            this.lblSpec.BackColor = System.Drawing.Color.Thistle;
            this.lblSpec.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblSpec.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSpec.Location = new System.Drawing.Point(4, 73);
            this.lblSpec.Name = "lblSpec";
            this.lblSpec.Size = new System.Drawing.Size(179, 23);
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
            this.lblCrecipe.Location = new System.Drawing.Point(190, 49);
            this.lblCrecipe.Name = "lblCrecipe";
            this.lblCrecipe.Size = new System.Drawing.Size(757, 23);
            this.lblCrecipe.TabIndex = 190;
            this.lblCrecipe.Text = "CurrentRecipe";
            this.lblCrecipe.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblRecipe
            // 
            this.lblRecipe.BackColor = System.Drawing.Color.Thistle;
            this.lblRecipe.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblRecipe.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRecipe.Location = new System.Drawing.Point(4, 49);
            this.lblRecipe.Name = "lblRecipe";
            this.lblRecipe.Size = new System.Drawing.Size(179, 23);
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
            this.lblPGMver.Location = new System.Drawing.Point(190, 25);
            this.lblPGMver.Name = "lblPGMver";
            this.lblPGMver.Size = new System.Drawing.Size(757, 23);
            this.lblPGMver.TabIndex = 191;
            this.lblPGMver.Text = "Pogram Ver";
            this.lblPGMver.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblPGver
            // 
            this.lblPGver.BackColor = System.Drawing.Color.Thistle;
            this.lblPGver.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPGver.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPGver.Location = new System.Drawing.Point(4, 25);
            this.lblPGver.Name = "lblPGver";
            this.lblPGver.Size = new System.Drawing.Size(179, 23);
            this.lblPGver.TabIndex = 185;
            this.lblPGver.Text = "Pogram Ver.";
            this.lblPGver.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblMESFname
            // 
            this.lblMESFname.BackColor = System.Drawing.Color.White;
            this.lblMESFname.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblMESFname.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMESFname.ForeColor = System.Drawing.Color.Blue;
            this.lblMESFname.Location = new System.Drawing.Point(190, 145);
            this.lblMESFname.Name = "lblMESFname";
            this.lblMESFname.Size = new System.Drawing.Size(757, 23);
            this.lblMESFname.TabIndex = 195;
            this.lblMESFname.Text = "MES File Name";
            this.lblMESFname.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
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
            this.YieldChart.Location = new System.Drawing.Point(480, 682);
            this.YieldChart.Name = "YieldChart";
            series1.ChartArea = "ChartArea1";
            series1.ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Pie;
            series1.Legend = "Legend1";
            series1.Name = "Series5";
            this.YieldChart.Series.Add(series1);
            this.YieldChart.Size = new System.Drawing.Size(436, 276);
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
            this.ModelGroup.Controls.Add(this.lblCheckPoint);
            this.ModelGroup.Location = new System.Drawing.Point(1739, 1);
            this.ModelGroup.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ModelGroup.Name = "ModelGroup";
            this.ModelGroup.Size = new System.Drawing.Size(178, 830);
            this.ModelGroup.TabIndex = 256;
            // 
            // lblCheckPoint
            // 
            this.lblCheckPoint.BackColor = System.Drawing.Color.YellowGreen;
            this.lblCheckPoint.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblCheckPoint.Font = new System.Drawing.Font("Arial", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCheckPoint.Location = new System.Drawing.Point(6, 6);
            this.lblCheckPoint.Name = "lblCheckPoint";
            this.lblCheckPoint.Size = new System.Drawing.Size(164, 38);
            this.lblCheckPoint.TabIndex = 229;
            this.lblCheckPoint.Text = "Check Points";
            this.lblCheckPoint.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // textBox1
            // 
            this.textBox1.Font = new System.Drawing.Font("맑은 고딕", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.textBox1.Location = new System.Drawing.Point(541, 973);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(80, 33);
            this.textBox1.TabIndex = 152;
            this.textBox1.Text = "0";
            this.textBox1.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // textBox2
            // 
            this.textBox2.Font = new System.Drawing.Font("맑은 고딕", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.textBox2.ForeColor = System.Drawing.Color.Black;
            this.textBox2.Location = new System.Drawing.Point(699, 973);
            this.textBox2.Name = "textBox2";
            this.textBox2.Size = new System.Drawing.Size(80, 33);
            this.textBox2.TabIndex = 153;
            this.textBox2.Text = "1";
            this.textBox2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold);
            this.label4.Location = new System.Drawing.Point(483, 979);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(52, 21);
            this.label4.TabIndex = 190;
            this.label4.Text = "SPL 0";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("맑은 고딕", 12F, System.Drawing.FontStyle.Bold);
            this.label5.Location = new System.Drawing.Point(641, 979);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(52, 21);
            this.label5.TabIndex = 191;
            this.label5.Text = "SPL 1";
            // 
            // tbBestpos
            // 
            this.tbBestpos.Location = new System.Drawing.Point(626, 284);
            this.tbBestpos.Name = "tbBestpos";
            this.tbBestpos.Size = new System.Drawing.Size(168, 21);
            this.tbBestpos.TabIndex = 257;
            this.tbBestpos.Text = "0";
            this.tbBestpos.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(626, 311);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(168, 29);
            this.button2.TabIndex = 258;
            this.button2.Text = "set";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click_1);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(575, 83);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(99, 71);
            this.button3.TabIndex = 259;
            this.button3.Text = "button3";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click_1);
            // 
            // F_Manage
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1920, 1080);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.tbBestpos);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.textBox2);
            this.Controls.Add(this.ModelGroup);
            this.Controls.Add(this.YieldChart);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.RunProgress);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.labelCenteringStatus);
            this.Controls.Add(this.SuddenStop);
            this.Controls.Add(this.RepeatStartTest);
            this.Controls.Add(this.ToAdmin);
            this.Controls.Add(this.ToVision);
            this.Controls.Add(this.p_Result);
            this.Controls.Add(this.btnClearLogs);
            this.Controls.Add(this.button1);
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
            this.p_Result.ResumeLayout(false);
            this.p_Result.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.RunProgress)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.YieldChart)).EndInit();
            this.ModelGroup.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

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
        private System.Windows.Forms.Button ApplyLotID;
        private System.Windows.Forms.Button btnCheckContact;
        private System.Windows.Forms.TextBox OperatorName;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox LotID;
        private System.Windows.Forms.Label labelCenteringStatus;
        private System.Windows.Forms.Button SuddenStop;
        private System.Windows.Forms.Button RepeatStartTest;
        private System.Windows.Forms.Button ToAdmin;
        private System.Windows.Forms.Button ToVision;
        private System.Windows.Forms.Panel p_Result;
        private System.Windows.Forms.Button btnClearLogs;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.PictureBox RunProgress;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        public System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblMES;
        public System.Windows.Forms.Label lblMCnum;
        private System.Windows.Forms.Label lblMcNo;
        public System.Windows.Forms.Label lblCFW;
        private System.Windows.Forms.Label lblFirmware;
        public System.Windows.Forms.Label lblCspec;
        private System.Windows.Forms.Label lblSpec;
        public System.Windows.Forms.Label lblCrecipe;
        private System.Windows.Forms.Label lblRecipe;
        public System.Windows.Forms.Label lblPGMver;
        private System.Windows.Forms.Label lblPGver;
        public System.Windows.Forms.Label lblMESFname;
        private System.Windows.Forms.DataVisualization.Charting.Chart YieldChart;
        private System.Windows.Forms.Panel ModelGroup;
        private System.Windows.Forms.Label lblCheckPoint;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button TestStartBtn;
        private System.Windows.Forms.Button TestStopBtn;
        private System.Windows.Forms.TextBox TestCountText;
        public System.Windows.Forms.TextBox tbBestpos;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button3;
    }
}