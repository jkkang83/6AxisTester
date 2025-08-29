
namespace FZ4P
{
    partial class F_Main
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(F_Main));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.SpecGrid = new System.Windows.Forms.DataGridView();
            this.btnApplyTesterNo2 = new System.Windows.Forms.Button();
            this.ModelGroup = new System.Windows.Forms.GroupBox();
            this.lblDefaultModel = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.lbxModelFiles = new System.Windows.Forms.ListBox();
            this.lblStartModel = new System.Windows.Forms.Label();
            this.ModelList = new System.Windows.Forms.ListBox();
            this.MCNumber = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.ICList = new System.Windows.Forms.ListBox();
            this.SupplierList = new System.Windows.Forms.ListBox();
            this.label17 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.ProductLine = new System.Windows.Forms.TextBox();
            this.label15 = new System.Windows.Forms.Label();
            this.lblRevisionNumber = new System.Windows.Forms.Label();
            this.TesterNo = new System.Windows.Forms.TextBox();
            this.lblMaker = new System.Windows.Forms.Label();
            this.LotMaker = new System.Windows.Forms.ListBox();
            this.RevisionNo = new System.Windows.Forms.TextBox();
            this.ApplyTester = new System.Windows.Forms.Button();
            this.panel4 = new System.Windows.Forms.Panel();
            this.textBox4 = new System.Windows.Forms.TextBox();
            this.OpenSpec = new System.Windows.Forms.Button();
            this.EditSpec = new System.Windows.Forms.CheckBox();
            this.SaveSpec = new System.Windows.Forms.Button();
            this.SaveAsSpec = new System.Windows.Forms.Button();
            this.SpecFileName = new System.Windows.Forms.TextBox();
            this.P_AutoLearn = new System.Windows.Forms.Panel();
            this.P_Vision = new System.Windows.Forms.Panel();
            this.P_Manager = new System.Windows.Forms.Panel();
            this.LotID = new System.Windows.Forms.TextBox();
            this.P_Main = new System.Windows.Forms.Panel();
            this.SetCodeScript = new System.Windows.Forms.Button();
            this.CodeScriptPath = new System.Windows.Forms.RichTextBox();
            this.SetYPIDUpdate = new System.Windows.Forms.Button();
            this.YPidSetPath = new System.Windows.Forms.RichTextBox();
            this.SetXPIDUpdate = new System.Windows.Forms.Button();
            this.XPidSetPath = new System.Windows.Forms.RichTextBox();
            this.SetAFPIDUpdate = new System.Windows.Forms.Button();
            this.AFPidSetPath = new System.Windows.Forms.RichTextBox();
            this.ConditinGrid = new System.Windows.Forms.DataGridView();
            this.panel2 = new System.Windows.Forms.Panel();
            this.ToVision = new System.Windows.Forms.Button();
            this.ToOperator = new System.Windows.Forms.Button();
            this.lblOPName = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.OperatorName = new System.Windows.Forms.TextBox();
            this.P_Motion = new System.Windows.Forms.Panel();
            this.Move_Down = new System.Windows.Forms.Button();
            this.Move_Up = new System.Windows.Forms.Button();
            this.tbToDoList = new System.Windows.Forms.TextBox();
            this.Actionbox = new System.Windows.Forms.ListBox();
            this.TodoBox = new System.Windows.Forms.ListBox();
            this.tbActionList = new System.Windows.Forms.TextBox();
            this.RemoveItem = new System.Windows.Forms.Button();
            this.AddItem = new System.Windows.Forms.Button();
            this.EditCondition = new System.Windows.Forms.CheckBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.panel3 = new System.Windows.Forms.Panel();
            this.SaveAsCondition = new System.Windows.Forms.Button();
            this.SaveCondition = new System.Windows.Forms.Button();
            this.OpenCondition = new System.Windows.Forms.Button();
            this.RecipeFileName = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.SpecGrid)).BeginInit();
            this.ModelGroup.SuspendLayout();
            this.panel4.SuspendLayout();
            this.P_Main.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ConditinGrid)).BeginInit();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // SpecGrid
            // 
            this.SpecGrid.AllowUserToAddRows = false;
            this.SpecGrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.SpecGrid.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.SpecGrid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.SpecGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.SpecGrid.Location = new System.Drawing.Point(769, 152);
            this.SpecGrid.Name = "SpecGrid";
            this.SpecGrid.RowTemplate.Height = 23;
            this.SpecGrid.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.SpecGrid.Size = new System.Drawing.Size(473, 843);
            this.SpecGrid.TabIndex = 78;
            this.SpecGrid.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.SpecGrid_CellMouseDoubleClick);
            // 
            // btnApplyTesterNo2
            // 
            this.btnApplyTesterNo2.BackColor = System.Drawing.Color.MediumBlue;
            this.btnApplyTesterNo2.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("btnApplyTesterNo2.BackgroundImage")));
            this.btnApplyTesterNo2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.btnApplyTesterNo2.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnApplyTesterNo2.Font = new System.Drawing.Font("Calibri", 16F, System.Drawing.FontStyle.Bold);
            this.btnApplyTesterNo2.ForeColor = System.Drawing.Color.White;
            this.btnApplyTesterNo2.Location = new System.Drawing.Point(151, 951);
            this.btnApplyTesterNo2.Name = "btnApplyTesterNo2";
            this.btnApplyTesterNo2.Size = new System.Drawing.Size(159, 36);
            this.btnApplyTesterNo2.TabIndex = 178;
            this.btnApplyTesterNo2.Text = "Apply";
            this.btnApplyTesterNo2.UseVisualStyleBackColor = false;
            // 
            // ModelGroup
            // 
            this.ModelGroup.BackColor = System.Drawing.Color.LightGray;
            this.ModelGroup.Controls.Add(this.lblDefaultModel);
            this.ModelGroup.Controls.Add(this.button1);
            this.ModelGroup.Controls.Add(this.lbxModelFiles);
            this.ModelGroup.Controls.Add(this.lblStartModel);
            this.ModelGroup.Controls.Add(this.ModelList);
            this.ModelGroup.Controls.Add(this.MCNumber);
            this.ModelGroup.Controls.Add(this.label4);
            this.ModelGroup.Controls.Add(this.label18);
            this.ModelGroup.Controls.Add(this.ICList);
            this.ModelGroup.Controls.Add(this.SupplierList);
            this.ModelGroup.Controls.Add(this.label17);
            this.ModelGroup.Controls.Add(this.label1);
            this.ModelGroup.Controls.Add(this.ProductLine);
            this.ModelGroup.Controls.Add(this.label15);
            this.ModelGroup.Controls.Add(this.lblRevisionNumber);
            this.ModelGroup.Controls.Add(this.TesterNo);
            this.ModelGroup.Controls.Add(this.lblMaker);
            this.ModelGroup.Controls.Add(this.LotMaker);
            this.ModelGroup.Controls.Add(this.RevisionNo);
            this.ModelGroup.Controls.Add(this.ApplyTester);
            this.ModelGroup.Location = new System.Drawing.Point(1248, 48);
            this.ModelGroup.Name = "ModelGroup";
            this.ModelGroup.Size = new System.Drawing.Size(647, 524);
            this.ModelGroup.TabIndex = 159;
            this.ModelGroup.TabStop = false;
            // 
            // lblDefaultModel
            // 
            this.lblDefaultModel.AutoSize = true;
            this.lblDefaultModel.Font = new System.Drawing.Font("맑은 고딕", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.lblDefaultModel.Location = new System.Drawing.Point(6, 494);
            this.lblDefaultModel.Name = "lblDefaultModel";
            this.lblDefaultModel.Size = new System.Drawing.Size(126, 17);
            this.lblDefaultModel.TabIndex = 239;
            this.lblDefaultModel.Text = "Default Model File";
            // 
            // button1
            // 
            this.button1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("button1.BackgroundImage")));
            this.button1.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.button1.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold);
            this.button1.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.button1.Location = new System.Drawing.Point(4, 390);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(200, 28);
            this.button1.TabIndex = 238;
            this.button1.Text = "Load Model FIle List";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // lbxModelFiles
            // 
            this.lbxModelFiles.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(242)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.lbxModelFiles.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbxModelFiles.FormattingEnabled = true;
            this.lbxModelFiles.ItemHeight = 15;
            this.lbxModelFiles.Location = new System.Drawing.Point(4, 427);
            this.lbxModelFiles.Name = "lbxModelFiles";
            this.lbxModelFiles.Size = new System.Drawing.Size(643, 64);
            this.lbxModelFiles.TabIndex = 237;
            this.lbxModelFiles.SelectedIndexChanged += new System.EventHandler(this.lbxModelFiles_SelectedIndexChanged);
            // 
            // lblStartModel
            // 
            this.lblStartModel.AutoSize = true;
            this.lblStartModel.Font = new System.Drawing.Font("Calibri", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStartModel.ForeColor = System.Drawing.Color.MediumBlue;
            this.lblStartModel.Location = new System.Drawing.Point(70, 340);
            this.lblStartModel.Name = "lblStartModel";
            this.lblStartModel.Size = new System.Drawing.Size(62, 23);
            this.lblStartModel.TabIndex = 236;
            this.lblStartModel.Text = "Model";
            // 
            // ModelList
            // 
            this.ModelList.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ModelList.ForeColor = System.Drawing.Color.MediumBlue;
            this.ModelList.FormattingEnabled = true;
            this.ModelList.ItemHeight = 19;
            this.ModelList.Location = new System.Drawing.Point(139, 340);
            this.ModelList.Name = "ModelList";
            this.ModelList.Size = new System.Drawing.Size(130, 42);
            this.ModelList.TabIndex = 235;
            // 
            // MCNumber
            // 
            this.MCNumber.BackColor = System.Drawing.Color.White;
            this.MCNumber.Font = new System.Drawing.Font("Calibri", 14F, System.Drawing.FontStyle.Bold);
            this.MCNumber.Location = new System.Drawing.Point(139, 241);
            this.MCNumber.Name = "MCNumber";
            this.MCNumber.Size = new System.Drawing.Size(130, 30);
            this.MCNumber.TabIndex = 234;
            this.MCNumber.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Calibri", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(63, 244);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(69, 23);
            this.label4.TabIndex = 233;
            this.label4.Text = "MC No.";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Font = new System.Drawing.Font("Calibri", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label18.Location = new System.Drawing.Point(45, 282);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(79, 23);
            this.label18.TabIndex = 231;
            this.label18.Text = "Driver IC";
            // 
            // ICList
            // 
            this.ICList.BackColor = System.Drawing.Color.LightCyan;
            this.ICList.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ICList.FormattingEnabled = true;
            this.ICList.ItemHeight = 19;
            this.ICList.Location = new System.Drawing.Point(139, 282);
            this.ICList.Name = "ICList";
            this.ICList.Size = new System.Drawing.Size(130, 42);
            this.ICList.TabIndex = 232;
            // 
            // SupplierList
            // 
            this.SupplierList.BackColor = System.Drawing.Color.LightCyan;
            this.SupplierList.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SupplierList.FormattingEnabled = true;
            this.SupplierList.ItemHeight = 19;
            this.SupplierList.Location = new System.Drawing.Point(139, 193);
            this.SupplierList.Name = "SupplierList";
            this.SupplierList.Size = new System.Drawing.Size(130, 42);
            this.SupplierList.TabIndex = 230;
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Font = new System.Drawing.Font("Calibri", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label17.Location = new System.Drawing.Point(7, 198);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(125, 23);
            this.label17.TabIndex = 229;
            this.label17.Text = "Prism Supplier";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Calibri", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(23, 167);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(109, 23);
            this.label1.TabIndex = 228;
            this.label1.Text = "Product Line";
            // 
            // ProductLine
            // 
            this.ProductLine.BackColor = System.Drawing.Color.White;
            this.ProductLine.Font = new System.Drawing.Font("Calibri", 14F, System.Drawing.FontStyle.Bold);
            this.ProductLine.Location = new System.Drawing.Point(139, 160);
            this.ProductLine.Name = "ProductLine";
            this.ProductLine.Size = new System.Drawing.Size(130, 30);
            this.ProductLine.TabIndex = 227;
            this.ProductLine.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Font = new System.Drawing.Font("Calibri", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label15.Location = new System.Drawing.Point(41, 131);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(91, 23);
            this.label15.TabIndex = 226;
            this.label15.Text = "Tester No.";
            // 
            // lblRevisionNumber
            // 
            this.lblRevisionNumber.AutoSize = true;
            this.lblRevisionNumber.Font = new System.Drawing.Font("Calibri", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRevisionNumber.Location = new System.Drawing.Point(23, 97);
            this.lblRevisionNumber.Name = "lblRevisionNumber";
            this.lblRevisionNumber.Size = new System.Drawing.Size(110, 23);
            this.lblRevisionNumber.TabIndex = 222;
            this.lblRevisionNumber.Text = "Revision No.";
            // 
            // TesterNo
            // 
            this.TesterNo.BackColor = System.Drawing.Color.White;
            this.TesterNo.Font = new System.Drawing.Font("Calibri", 14F, System.Drawing.FontStyle.Bold);
            this.TesterNo.Location = new System.Drawing.Point(139, 124);
            this.TesterNo.Name = "TesterNo";
            this.TesterNo.Size = new System.Drawing.Size(130, 30);
            this.TesterNo.TabIndex = 225;
            this.TesterNo.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // lblMaker
            // 
            this.lblMaker.AutoSize = true;
            this.lblMaker.Font = new System.Drawing.Font("Calibri", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMaker.Location = new System.Drawing.Point(71, 53);
            this.lblMaker.Name = "lblMaker";
            this.lblMaker.Size = new System.Drawing.Size(61, 23);
            this.lblMaker.TabIndex = 221;
            this.lblMaker.Text = "Maker";
            // 
            // LotMaker
            // 
            this.LotMaker.BackColor = System.Drawing.Color.LightCyan;
            this.LotMaker.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LotMaker.FormattingEnabled = true;
            this.LotMaker.ItemHeight = 19;
            this.LotMaker.Location = new System.Drawing.Point(139, 49);
            this.LotMaker.Name = "LotMaker";
            this.LotMaker.Size = new System.Drawing.Size(130, 42);
            this.LotMaker.TabIndex = 224;
            // 
            // RevisionNo
            // 
            this.RevisionNo.BackColor = System.Drawing.Color.LightCyan;
            this.RevisionNo.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold);
            this.RevisionNo.Location = new System.Drawing.Point(139, 93);
            this.RevisionNo.Name = "RevisionNo";
            this.RevisionNo.Size = new System.Drawing.Size(130, 27);
            this.RevisionNo.TabIndex = 223;
            this.RevisionNo.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // ApplyTester
            // 
            this.ApplyTester.BackColor = System.Drawing.Color.MediumBlue;
            this.ApplyTester.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ApplyTester.BackgroundImage")));
            this.ApplyTester.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ApplyTester.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.ApplyTester.Font = new System.Drawing.Font("Calibri", 16F, System.Drawing.FontStyle.Bold);
            this.ApplyTester.ForeColor = System.Drawing.Color.White;
            this.ApplyTester.Location = new System.Drawing.Point(9, 9);
            this.ApplyTester.Name = "ApplyTester";
            this.ApplyTester.Size = new System.Drawing.Size(179, 34);
            this.ApplyTester.TabIndex = 132;
            this.ApplyTester.Text = "Apply";
            this.ApplyTester.UseVisualStyleBackColor = false;
            this.ApplyTester.Click += new System.EventHandler(this.ApplyTester_Click);
            // 
            // panel4
            // 
            this.panel4.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.panel4.Controls.Add(this.textBox4);
            this.panel4.Controls.Add(this.OpenSpec);
            this.panel4.Controls.Add(this.EditSpec);
            this.panel4.Controls.Add(this.SaveSpec);
            this.panel4.Controls.Add(this.SaveAsSpec);
            this.panel4.Location = new System.Drawing.Point(769, 46);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(474, 70);
            this.panel4.TabIndex = 91;
            // 
            // textBox4
            // 
            this.textBox4.BackColor = System.Drawing.Color.DimGray;
            this.textBox4.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox4.Font = new System.Drawing.Font("Calibri", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox4.ForeColor = System.Drawing.Color.White;
            this.textBox4.Location = new System.Drawing.Point(0, -2);
            this.textBox4.Name = "textBox4";
            this.textBox4.ReadOnly = true;
            this.textBox4.Size = new System.Drawing.Size(474, 26);
            this.textBox4.TabIndex = 90;
            this.textBox4.TabStop = false;
            this.textBox4.Text = "Test Spec";
            this.textBox4.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // OpenSpec
            // 
            this.OpenSpec.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("OpenSpec.BackgroundImage")));
            this.OpenSpec.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.OpenSpec.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.OpenSpec.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold);
            this.OpenSpec.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.OpenSpec.Location = new System.Drawing.Point(15, 31);
            this.OpenSpec.Name = "OpenSpec";
            this.OpenSpec.Size = new System.Drawing.Size(120, 28);
            this.OpenSpec.TabIndex = 88;
            this.OpenSpec.Text = "Open";
            this.OpenSpec.UseVisualStyleBackColor = true;
            this.OpenSpec.Click += new System.EventHandler(this.OpenSpec_Click);
            // 
            // EditSpec
            // 
            this.EditSpec.AutoSize = true;
            this.EditSpec.BackColor = System.Drawing.Color.Transparent;
            this.EditSpec.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.EditSpec.ForeColor = System.Drawing.Color.White;
            this.EditSpec.Location = new System.Drawing.Point(401, 37);
            this.EditSpec.Name = "EditSpec";
            this.EditSpec.Size = new System.Drawing.Size(47, 19);
            this.EditSpec.TabIndex = 1;
            this.EditSpec.Text = "Edit";
            this.EditSpec.UseVisualStyleBackColor = false;
            this.EditSpec.CheckedChanged += new System.EventHandler(this.EditSpec_CheckedChanged);
            // 
            // SaveSpec
            // 
            this.SaveSpec.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("SaveSpec.BackgroundImage")));
            this.SaveSpec.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.SaveSpec.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.SaveSpec.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold);
            this.SaveSpec.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.SaveSpec.Location = new System.Drawing.Point(141, 31);
            this.SaveSpec.Name = "SaveSpec";
            this.SaveSpec.Size = new System.Drawing.Size(120, 28);
            this.SaveSpec.TabIndex = 87;
            this.SaveSpec.Text = "Save";
            this.SaveSpec.UseVisualStyleBackColor = true;
            this.SaveSpec.Click += new System.EventHandler(this.SaveSpec_Click);
            // 
            // SaveAsSpec
            // 
            this.SaveAsSpec.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("SaveAsSpec.BackgroundImage")));
            this.SaveAsSpec.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.SaveAsSpec.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.SaveAsSpec.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold);
            this.SaveAsSpec.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.SaveAsSpec.Location = new System.Drawing.Point(267, 31);
            this.SaveAsSpec.Name = "SaveAsSpec";
            this.SaveAsSpec.Size = new System.Drawing.Size(120, 28);
            this.SaveAsSpec.TabIndex = 89;
            this.SaveAsSpec.Text = "Save As";
            this.SaveAsSpec.UseVisualStyleBackColor = true;
            this.SaveAsSpec.Click += new System.EventHandler(this.SaveAsSpec_Click);
            // 
            // SpecFileName
            // 
            this.SpecFileName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.SpecFileName.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.SpecFileName.Font = new System.Drawing.Font("Calibri", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.SpecFileName.ForeColor = System.Drawing.Color.LightGray;
            this.SpecFileName.Location = new System.Drawing.Point(769, 118);
            this.SpecFileName.Name = "SpecFileName";
            this.SpecFileName.ReadOnly = true;
            this.SpecFileName.Size = new System.Drawing.Size(474, 26);
            this.SpecFileName.TabIndex = 89;
            this.SpecFileName.TabStop = false;
            this.SpecFileName.Text = "Spec File Name";
            this.SpecFileName.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // P_AutoLearn
            // 
            this.P_AutoLearn.Location = new System.Drawing.Point(117, 1023);
            this.P_AutoLearn.Name = "P_AutoLearn";
            this.P_AutoLearn.Size = new System.Drawing.Size(50, 31);
            this.P_AutoLearn.TabIndex = 196;
            // 
            // P_Vision
            // 
            this.P_Vision.Location = new System.Drawing.Point(61, 1023);
            this.P_Vision.Name = "P_Vision";
            this.P_Vision.Size = new System.Drawing.Size(50, 31);
            this.P_Vision.TabIndex = 195;
            // 
            // P_Manager
            // 
            this.P_Manager.Location = new System.Drawing.Point(5, 1023);
            this.P_Manager.Name = "P_Manager";
            this.P_Manager.Size = new System.Drawing.Size(50, 31);
            this.P_Manager.TabIndex = 194;
            // 
            // LotID
            // 
            this.LotID.BackColor = System.Drawing.Color.White;
            this.LotID.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.LotID.Font = new System.Drawing.Font("Calibri", 14F, System.Drawing.FontStyle.Bold);
            this.LotID.Location = new System.Drawing.Point(151, 879);
            this.LotID.MaxLength = 50;
            this.LotID.Name = "LotID";
            this.LotID.Size = new System.Drawing.Size(159, 30);
            this.LotID.TabIndex = 170;
            this.LotID.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // P_Main
            // 
            this.P_Main.Controls.Add(this.SetCodeScript);
            this.P_Main.Controls.Add(this.CodeScriptPath);
            this.P_Main.Controls.Add(this.SetYPIDUpdate);
            this.P_Main.Controls.Add(this.YPidSetPath);
            this.P_Main.Controls.Add(this.SetXPIDUpdate);
            this.P_Main.Controls.Add(this.XPidSetPath);
            this.P_Main.Controls.Add(this.SetAFPIDUpdate);
            this.P_Main.Controls.Add(this.AFPidSetPath);
            this.P_Main.Controls.Add(this.ConditinGrid);
            this.P_Main.Controls.Add(this.SpecGrid);
            this.P_Main.Controls.Add(this.btnApplyTesterNo2);
            this.P_Main.Controls.Add(this.ModelGroup);
            this.P_Main.Controls.Add(this.panel2);
            this.P_Main.Controls.Add(this.panel4);
            this.P_Main.Controls.Add(this.lblOPName);
            this.P_Main.Controls.Add(this.SpecFileName);
            this.P_Main.Controls.Add(this.label16);
            this.P_Main.Controls.Add(this.LotID);
            this.P_Main.Controls.Add(this.OperatorName);
            this.P_Main.Location = new System.Drawing.Point(2, 1);
            this.P_Main.Name = "P_Main";
            this.P_Main.Size = new System.Drawing.Size(1898, 998);
            this.P_Main.TabIndex = 197;
            // 
            // SetCodeScript
            // 
            this.SetCodeScript.BackColor = System.Drawing.Color.MediumBlue;
            this.SetCodeScript.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("SetCodeScript.BackgroundImage")));
            this.SetCodeScript.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.SetCodeScript.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.SetCodeScript.Font = new System.Drawing.Font("Calibri", 16F, System.Drawing.FontStyle.Bold);
            this.SetCodeScript.ForeColor = System.Drawing.Color.White;
            this.SetCodeScript.Location = new System.Drawing.Point(1247, 907);
            this.SetCodeScript.Name = "SetCodeScript";
            this.SetCodeScript.Size = new System.Drawing.Size(642, 43);
            this.SetCodeScript.TabIndex = 243;
            this.SetCodeScript.Text = "Code Script File";
            this.SetCodeScript.UseVisualStyleBackColor = false;
            this.SetCodeScript.Click += new System.EventHandler(this.SetCodeScript_Click);
            // 
            // CodeScriptPath
            // 
            this.CodeScriptPath.BackColor = System.Drawing.Color.White;
            this.CodeScriptPath.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.CodeScriptPath.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.CodeScriptPath.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.CodeScriptPath.Location = new System.Drawing.Point(1247, 956);
            this.CodeScriptPath.Name = "CodeScriptPath";
            this.CodeScriptPath.ReadOnly = true;
            this.CodeScriptPath.Size = new System.Drawing.Size(639, 36);
            this.CodeScriptPath.TabIndex = 242;
            this.CodeScriptPath.Text = "";
            // 
            // SetYPIDUpdate
            // 
            this.SetYPIDUpdate.BackColor = System.Drawing.Color.MediumBlue;
            this.SetYPIDUpdate.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("SetYPIDUpdate.BackgroundImage")));
            this.SetYPIDUpdate.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.SetYPIDUpdate.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.SetYPIDUpdate.Font = new System.Drawing.Font("Calibri", 16F, System.Drawing.FontStyle.Bold);
            this.SetYPIDUpdate.ForeColor = System.Drawing.Color.White;
            this.SetYPIDUpdate.Location = new System.Drawing.Point(1247, 814);
            this.SetYPIDUpdate.Name = "SetYPIDUpdate";
            this.SetYPIDUpdate.Size = new System.Drawing.Size(642, 43);
            this.SetYPIDUpdate.TabIndex = 241;
            this.SetYPIDUpdate.Text = "Set Y PID Update File";
            this.SetYPIDUpdate.UseVisualStyleBackColor = false;
            this.SetYPIDUpdate.Click += new System.EventHandler(this.SetYPIDUpdate_Click);
            // 
            // YPidSetPath
            // 
            this.YPidSetPath.BackColor = System.Drawing.Color.White;
            this.YPidSetPath.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.YPidSetPath.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.YPidSetPath.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.YPidSetPath.Location = new System.Drawing.Point(1247, 863);
            this.YPidSetPath.Name = "YPidSetPath";
            this.YPidSetPath.ReadOnly = true;
            this.YPidSetPath.Size = new System.Drawing.Size(639, 36);
            this.YPidSetPath.TabIndex = 240;
            this.YPidSetPath.Text = "";
            // 
            // SetXPIDUpdate
            // 
            this.SetXPIDUpdate.BackColor = System.Drawing.Color.MediumBlue;
            this.SetXPIDUpdate.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("SetXPIDUpdate.BackgroundImage")));
            this.SetXPIDUpdate.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.SetXPIDUpdate.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.SetXPIDUpdate.Font = new System.Drawing.Font("Calibri", 16F, System.Drawing.FontStyle.Bold);
            this.SetXPIDUpdate.ForeColor = System.Drawing.Color.White;
            this.SetXPIDUpdate.Location = new System.Drawing.Point(1247, 723);
            this.SetXPIDUpdate.Name = "SetXPIDUpdate";
            this.SetXPIDUpdate.Size = new System.Drawing.Size(642, 43);
            this.SetXPIDUpdate.TabIndex = 239;
            this.SetXPIDUpdate.Text = "Set X PID Update File";
            this.SetXPIDUpdate.UseVisualStyleBackColor = false;
            this.SetXPIDUpdate.Click += new System.EventHandler(this.SetXPIDUpdate_Click);
            // 
            // XPidSetPath
            // 
            this.XPidSetPath.BackColor = System.Drawing.Color.White;
            this.XPidSetPath.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.XPidSetPath.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.XPidSetPath.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.XPidSetPath.Location = new System.Drawing.Point(1247, 772);
            this.XPidSetPath.Name = "XPidSetPath";
            this.XPidSetPath.ReadOnly = true;
            this.XPidSetPath.Size = new System.Drawing.Size(639, 36);
            this.XPidSetPath.TabIndex = 238;
            this.XPidSetPath.Text = "";
            // 
            // SetAFPIDUpdate
            // 
            this.SetAFPIDUpdate.BackColor = System.Drawing.Color.MediumBlue;
            this.SetAFPIDUpdate.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("SetAFPIDUpdate.BackgroundImage")));
            this.SetAFPIDUpdate.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.SetAFPIDUpdate.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.SetAFPIDUpdate.Font = new System.Drawing.Font("Calibri", 16F, System.Drawing.FontStyle.Bold);
            this.SetAFPIDUpdate.ForeColor = System.Drawing.Color.White;
            this.SetAFPIDUpdate.Location = new System.Drawing.Point(1248, 632);
            this.SetAFPIDUpdate.Name = "SetAFPIDUpdate";
            this.SetAFPIDUpdate.Size = new System.Drawing.Size(642, 43);
            this.SetAFPIDUpdate.TabIndex = 237;
            this.SetAFPIDUpdate.Text = "Set AF PID Update File";
            this.SetAFPIDUpdate.UseVisualStyleBackColor = false;
            this.SetAFPIDUpdate.Click += new System.EventHandler(this.SetAFPIDUpdate_Click);
            // 
            // AFPidSetPath
            // 
            this.AFPidSetPath.BackColor = System.Drawing.Color.White;
            this.AFPidSetPath.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.AFPidSetPath.Font = new System.Drawing.Font("맑은 고딕", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.AFPidSetPath.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.AFPidSetPath.Location = new System.Drawing.Point(1248, 681);
            this.AFPidSetPath.Name = "AFPidSetPath";
            this.AFPidSetPath.ReadOnly = true;
            this.AFPidSetPath.Size = new System.Drawing.Size(639, 36);
            this.AFPidSetPath.TabIndex = 191;
            this.AFPidSetPath.Text = "";
            // 
            // ConditinGrid
            // 
            this.ConditinGrid.AllowUserToAddRows = false;
            this.ConditinGrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ConditinGrid.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.ConditinGrid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.ConditinGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.ConditinGrid.Location = new System.Drawing.Point(364, 46);
            this.ConditinGrid.Name = "ConditinGrid";
            this.ConditinGrid.RowTemplate.Height = 23;
            this.ConditinGrid.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.ConditinGrid.Size = new System.Drawing.Size(399, 951);
            this.ConditinGrid.TabIndex = 87;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.Silver;
            this.panel2.Controls.Add(this.ToVision);
            this.panel2.Controls.Add(this.ToOperator);
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1907, 42);
            this.panel2.TabIndex = 32;
            // 
            // ToVision
            // 
            this.ToVision.BackColor = System.Drawing.Color.DodgerBlue;
            this.ToVision.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ToVision.BackgroundImage")));
            this.ToVision.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ToVision.Font = new System.Drawing.Font("Calibri", 16F, System.Drawing.FontStyle.Bold);
            this.ToVision.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.ToVision.Location = new System.Drawing.Point(363, 0);
            this.ToVision.Name = "ToVision";
            this.ToVision.Size = new System.Drawing.Size(400, 45);
            this.ToVision.TabIndex = 68;
            this.ToVision.Text = "Vision";
            this.ToVision.UseVisualStyleBackColor = false;
            this.ToVision.Click += new System.EventHandler(this.ToVision_Click);
            // 
            // ToOperator
            // 
            this.ToOperator.BackColor = System.Drawing.Color.DeepSkyBlue;
            this.ToOperator.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("ToOperator.BackgroundImage")));
            this.ToOperator.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ToOperator.Font = new System.Drawing.Font("Calibri", 16F, System.Drawing.FontStyle.Bold);
            this.ToOperator.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.ToOperator.Location = new System.Drawing.Point(0, 0);
            this.ToOperator.Name = "ToOperator";
            this.ToOperator.Size = new System.Drawing.Size(357, 45);
            this.ToOperator.TabIndex = 120;
            this.ToOperator.Text = "Operator";
            this.ToOperator.UseVisualStyleBackColor = false;
            this.ToOperator.Click += new System.EventHandler(this.ToOperator_Click);
            // 
            // lblOPName
            // 
            this.lblOPName.AutoSize = true;
            this.lblOPName.Font = new System.Drawing.Font("Calibri", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblOPName.Location = new System.Drawing.Point(17, 915);
            this.lblOPName.Name = "lblOPName";
            this.lblOPName.Size = new System.Drawing.Size(134, 23);
            this.lblOPName.TabIndex = 172;
            this.lblOPName.Text = "Operator Name";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Font = new System.Drawing.Font("Calibri", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label16.Location = new System.Drawing.Point(90, 883);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(61, 23);
            this.label16.TabIndex = 171;
            this.label16.Text = "LOT ID";
            // 
            // OperatorName
            // 
            this.OperatorName.BackColor = System.Drawing.Color.White;
            this.OperatorName.Font = new System.Drawing.Font("Calibri", 14F, System.Drawing.FontStyle.Bold);
            this.OperatorName.Location = new System.Drawing.Point(151, 915);
            this.OperatorName.MaxLength = 50;
            this.OperatorName.Name = "OperatorName";
            this.OperatorName.Size = new System.Drawing.Size(159, 30);
            this.OperatorName.TabIndex = 170;
            this.OperatorName.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // P_Motion
            // 
            this.P_Motion.Location = new System.Drawing.Point(173, 1023);
            this.P_Motion.Name = "P_Motion";
            this.P_Motion.Size = new System.Drawing.Size(50, 31);
            this.P_Motion.TabIndex = 198;
            // 
            // Move_Down
            // 
            this.Move_Down.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Move_Down.Location = new System.Drawing.Point(207, 807);
            this.Move_Down.Name = "Move_Down";
            this.Move_Down.Size = new System.Drawing.Size(101, 37);
            this.Move_Down.TabIndex = 193;
            this.Move_Down.Text = "Dn";
            this.Move_Down.UseVisualStyleBackColor = true;
            this.Move_Down.Click += new System.EventHandler(this.Move_Down_Click);
            // 
            // Move_Up
            // 
            this.Move_Up.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Move_Up.Location = new System.Drawing.Point(38, 807);
            this.Move_Up.Name = "Move_Up";
            this.Move_Up.Size = new System.Drawing.Size(101, 37);
            this.Move_Up.TabIndex = 192;
            this.Move_Up.Text = "Up";
            this.Move_Up.UseVisualStyleBackColor = true;
            this.Move_Up.Click += new System.EventHandler(this.Move_Up_Click);
            // 
            // tbToDoList
            // 
            this.tbToDoList.BackColor = System.Drawing.Color.White;
            this.tbToDoList.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbToDoList.ForeColor = System.Drawing.Color.Black;
            this.tbToDoList.Location = new System.Drawing.Point(4, 501);
            this.tbToDoList.Name = "tbToDoList";
            this.tbToDoList.ReadOnly = true;
            this.tbToDoList.Size = new System.Drawing.Size(355, 23);
            this.tbToDoList.TabIndex = 191;
            this.tbToDoList.TabStop = false;
            this.tbToDoList.Text = "To Do List";
            this.tbToDoList.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // Actionbox
            // 
            this.Actionbox.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Actionbox.FormattingEnabled = true;
            this.Actionbox.ItemHeight = 15;
            this.Actionbox.Location = new System.Drawing.Point(3, 177);
            this.Actionbox.Name = "Actionbox";
            this.Actionbox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.Actionbox.Size = new System.Drawing.Size(356, 259);
            this.Actionbox.TabIndex = 186;
            this.Actionbox.SelectedIndexChanged += new System.EventHandler(this.Actionbox_SelectedIndexChanged);
            // 
            // TodoBox
            // 
            this.TodoBox.BackColor = System.Drawing.Color.PaleGreen;
            this.TodoBox.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TodoBox.FormattingEnabled = true;
            this.TodoBox.ItemHeight = 15;
            this.TodoBox.Location = new System.Drawing.Point(2, 525);
            this.TodoBox.Name = "TodoBox";
            this.TodoBox.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.TodoBox.Size = new System.Drawing.Size(357, 274);
            this.TodoBox.TabIndex = 190;
            this.TodoBox.SelectedIndexChanged += new System.EventHandler(this.Actionbox_SelectedIndexChanged);
            // 
            // tbActionList
            // 
            this.tbActionList.BackColor = System.Drawing.Color.White;
            this.tbActionList.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbActionList.ForeColor = System.Drawing.Color.Black;
            this.tbActionList.Location = new System.Drawing.Point(3, 154);
            this.tbActionList.Name = "tbActionList";
            this.tbActionList.ReadOnly = true;
            this.tbActionList.Size = new System.Drawing.Size(356, 23);
            this.tbActionList.TabIndex = 187;
            this.tbActionList.TabStop = false;
            this.tbActionList.Text = "Action List";
            this.tbActionList.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // RemoveItem
            // 
            this.RemoveItem.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RemoveItem.Location = new System.Drawing.Point(38, 442);
            this.RemoveItem.Name = "RemoveItem";
            this.RemoveItem.Size = new System.Drawing.Size(101, 37);
            this.RemoveItem.TabIndex = 189;
            this.RemoveItem.Text = "↑↑";
            this.RemoveItem.UseVisualStyleBackColor = true;
            this.RemoveItem.Click += new System.EventHandler(this.RemoveItem_Click);
            // 
            // AddItem
            // 
            this.AddItem.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.AddItem.Location = new System.Drawing.Point(207, 442);
            this.AddItem.Name = "AddItem";
            this.AddItem.Size = new System.Drawing.Size(101, 37);
            this.AddItem.TabIndex = 188;
            this.AddItem.Text = "↓↓";
            this.AddItem.UseVisualStyleBackColor = true;
            this.AddItem.Click += new System.EventHandler(this.AddItem_Click);
            // 
            // EditCondition
            // 
            this.EditCondition.AutoSize = true;
            this.EditCondition.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.EditCondition.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.EditCondition.ForeColor = System.Drawing.Color.White;
            this.EditCondition.Location = new System.Drawing.Point(302, 39);
            this.EditCondition.Name = "EditCondition";
            this.EditCondition.Size = new System.Drawing.Size(47, 19);
            this.EditCondition.TabIndex = 0;
            this.EditCondition.Text = "Edit";
            this.EditCondition.UseVisualStyleBackColor = false;
            this.EditCondition.CheckedChanged += new System.EventHandler(this.EditCondition_CheckedChanged);
            // 
            // textBox1
            // 
            this.textBox1.BackColor = System.Drawing.Color.DimGray;
            this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox1.Font = new System.Drawing.Font("Calibri", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox1.ForeColor = System.Drawing.Color.White;
            this.textBox1.Location = new System.Drawing.Point(0, 0);
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(358, 26);
            this.textBox1.TabIndex = 92;
            this.textBox1.TabStop = false;
            this.textBox1.Text = "Recipe";
            this.textBox1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.panel3.Controls.Add(this.textBox1);
            this.panel3.Controls.Add(this.SaveAsCondition);
            this.panel3.Controls.Add(this.SaveCondition);
            this.panel3.Controls.Add(this.OpenCondition);
            this.panel3.Controls.Add(this.EditCondition);
            this.panel3.Location = new System.Drawing.Point(2, 47);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(357, 72);
            this.panel3.TabIndex = 185;
            // 
            // SaveAsCondition
            // 
            this.SaveAsCondition.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("SaveAsCondition.BackgroundImage")));
            this.SaveAsCondition.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.SaveAsCondition.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.SaveAsCondition.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold);
            this.SaveAsCondition.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.SaveAsCondition.Location = new System.Drawing.Point(205, 34);
            this.SaveAsCondition.Name = "SaveAsCondition";
            this.SaveAsCondition.Size = new System.Drawing.Size(89, 28);
            this.SaveAsCondition.TabIndex = 70;
            this.SaveAsCondition.Text = "Save As";
            this.SaveAsCondition.UseVisualStyleBackColor = true;
            this.SaveAsCondition.Click += new System.EventHandler(this.SaveAsCondition_Click);
            // 
            // SaveCondition
            // 
            this.SaveCondition.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("SaveCondition.BackgroundImage")));
            this.SaveCondition.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.SaveCondition.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.SaveCondition.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold);
            this.SaveCondition.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.SaveCondition.Location = new System.Drawing.Point(104, 34);
            this.SaveCondition.Name = "SaveCondition";
            this.SaveCondition.Size = new System.Drawing.Size(89, 28);
            this.SaveCondition.TabIndex = 69;
            this.SaveCondition.Text = "Save";
            this.SaveCondition.UseVisualStyleBackColor = true;
            this.SaveCondition.Click += new System.EventHandler(this.SaveCondition_Click);
            // 
            // OpenCondition
            // 
            this.OpenCondition.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("OpenCondition.BackgroundImage")));
            this.OpenCondition.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.OpenCondition.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.OpenCondition.Font = new System.Drawing.Font("Calibri", 12F, System.Drawing.FontStyle.Bold);
            this.OpenCondition.ForeColor = System.Drawing.Color.WhiteSmoke;
            this.OpenCondition.Location = new System.Drawing.Point(3, 34);
            this.OpenCondition.Name = "OpenCondition";
            this.OpenCondition.Size = new System.Drawing.Size(89, 28);
            this.OpenCondition.TabIndex = 68;
            this.OpenCondition.Text = "Open";
            this.OpenCondition.UseVisualStyleBackColor = true;
            this.OpenCondition.Click += new System.EventHandler(this.OpenCondition_Click);
            // 
            // RecipeFileName
            // 
            this.RecipeFileName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.RecipeFileName.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.RecipeFileName.Font = new System.Drawing.Font("Calibri", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.RecipeFileName.ForeColor = System.Drawing.Color.LightGray;
            this.RecipeFileName.Location = new System.Drawing.Point(2, 119);
            this.RecipeFileName.Name = "RecipeFileName";
            this.RecipeFileName.ReadOnly = true;
            this.RecipeFileName.Size = new System.Drawing.Size(357, 26);
            this.RecipeFileName.TabIndex = 184;
            this.RecipeFileName.TabStop = false;
            this.RecipeFileName.Text = "Recipe File Name";
            this.RecipeFileName.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // F_Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1904, 1041);
            this.Controls.Add(this.P_AutoLearn);
            this.Controls.Add(this.P_Vision);
            this.Controls.Add(this.P_Manager);
            this.Controls.Add(this.P_Motion);
            this.Controls.Add(this.Move_Down);
            this.Controls.Add(this.Move_Up);
            this.Controls.Add(this.tbToDoList);
            this.Controls.Add(this.Actionbox);
            this.Controls.Add(this.TodoBox);
            this.Controls.Add(this.tbActionList);
            this.Controls.Add(this.RemoveItem);
            this.Controls.Add(this.AddItem);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.RecipeFileName);
            this.Controls.Add(this.P_Main);
            this.Name = "F_Main";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "B7Wide_24061701";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.F_Main_FormClosing);
            this.Load += new System.EventHandler(this.F_Main_Load);
            ((System.ComponentModel.ISupportInitialize)(this.SpecGrid)).EndInit();
            this.ModelGroup.ResumeLayout(false);
            this.ModelGroup.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.P_Main.ResumeLayout(false);
            this.P_Main.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ConditinGrid)).EndInit();
            this.panel2.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.DataGridView SpecGrid;
        private System.Windows.Forms.Button btnApplyTesterNo2;
        private System.Windows.Forms.GroupBox ModelGroup;
        private System.Windows.Forms.Button ApplyTester;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.TextBox textBox4;
        private System.Windows.Forms.Button OpenSpec;
        private System.Windows.Forms.CheckBox EditSpec;
        private System.Windows.Forms.Button SaveSpec;
        private System.Windows.Forms.Button SaveAsSpec;
        private System.Windows.Forms.TextBox SpecFileName;
        private System.Windows.Forms.Panel P_AutoLearn;
        private System.Windows.Forms.Panel P_Vision;
        private System.Windows.Forms.Panel P_Manager;
        private System.Windows.Forms.TextBox LotID;
        private System.Windows.Forms.Panel P_Main;
        private System.Windows.Forms.DataGridView ConditinGrid;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button ToVision;
        public System.Windows.Forms.Button ToOperator;
        private System.Windows.Forms.Label lblOPName;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.TextBox OperatorName;
        private System.Windows.Forms.Panel P_Motion;
        private System.Windows.Forms.Button Move_Down;
        private System.Windows.Forms.Button Move_Up;
        private System.Windows.Forms.TextBox tbToDoList;
        private System.Windows.Forms.ListBox Actionbox;
        private System.Windows.Forms.ListBox TodoBox;
        private System.Windows.Forms.TextBox tbActionList;
        private System.Windows.Forms.Button RemoveItem;
        private System.Windows.Forms.Button AddItem;
        private System.Windows.Forms.CheckBox EditCondition;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Button SaveAsCondition;
        private System.Windows.Forms.Button SaveCondition;
        private System.Windows.Forms.Button OpenCondition;
        private System.Windows.Forms.TextBox RecipeFileName;
        private System.Windows.Forms.Label lblStartModel;
        private System.Windows.Forms.ListBox ModelList;
        private System.Windows.Forms.TextBox MCNumber;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.ListBox ICList;
        private System.Windows.Forms.ListBox SupplierList;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox ProductLine;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label lblRevisionNumber;
        private System.Windows.Forms.TextBox TesterNo;
        private System.Windows.Forms.Label lblMaker;
        private System.Windows.Forms.ListBox LotMaker;
        private System.Windows.Forms.TextBox RevisionNo;
        private System.Windows.Forms.Button SetAFPIDUpdate;
        private System.Windows.Forms.RichTextBox AFPidSetPath;
        private System.Windows.Forms.Button SetYPIDUpdate;
        private System.Windows.Forms.RichTextBox YPidSetPath;
        private System.Windows.Forms.Button SetXPIDUpdate;
        private System.Windows.Forms.RichTextBox XPidSetPath;
        private System.Windows.Forms.Button SetCodeScript;
        private System.Windows.Forms.RichTextBox CodeScriptPath;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ListBox lbxModelFiles;
        private System.Windows.Forms.Label lblDefaultModel;
    }
}

