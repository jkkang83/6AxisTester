using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FZ4P
{
    public partial class F_Main : Form
    {
        private Global m__G = null;
        public Condition Condition { get { return STATIC.Rcp.Condition; } }
        public Recipe Rcp { get { return STATIC.Rcp; } }
        public Spec Spec { get { return STATIC.Rcp.Spec; } }
        public Option Option { get { return STATIC.Rcp.Option; } }
        public Model Model { get { return STATIC.Rcp.Model; } }
        public CurrentPath Current { get { return STATIC.Rcp.Current; } }
        public Process Process { get { return STATIC.Process; } }
        public F_Main()
        {
            try
            {
                Task.Factory.StartNew(() =>
                {
                    Application.Run(STATIC.fStart);
                });

                InitializeComponent();

                STATIC.fStart.Log("Program Start !!");
                STATIC.StateChange += Form_StateChange;

                m__G = Global.GetInstance();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        private void Form_StateChange(object sender, EventArgs e)
        {
            switch (STATIC.State)
            {
                case (int)STATIC.STATE.Manage:
                    P_Main.Hide();
                    P_Manager.Location = new Point(0, 0);
                    P_Manager.Size = new Size(1920, 1080);
                    P_Manager.Show();
                    P_Vision.Location = new Point(59, 1026);
                    P_Vision.Size = new Size(50, 31);
                    P_Vision.Hide();
                    break;
                case (int)STATIC.STATE.Main:
                    //InitCondition();
                    //InitDataSpec();
                    P_Main.Show();
                    P_Manager.Location = new Point(3, 1026);
                    P_Manager.Size = new Size(50, 31);
                    P_Manager.Hide();
                    P_Vision.Location = new Point(59, 1026);
                    P_Vision.Size = new Size(50, 31);
                    P_Vision.Hide();
                    break;
                case (int)STATIC.STATE.Vision:
                    P_Main.Hide();
                    P_Vision.Location = new Point(0, 0);
                    P_Vision.Size = new Size(1920, 1080);
                    P_Vision.Show();
                    break;
            }
        }
        private void F_Main_Load(object sender, EventArgs e)
        {
            List<Form> fList = new List<Form>() { STATIC.fManage, STATIC.fVision };
            for (int i = 0; i < fList.Count; i++)
            {
                fList[i].TopLevel = false;
                fList[i].BackColor = SystemColors.ControlLight;
                fList[i].FormBorderStyle = FormBorderStyle.None;
            }

            P_Manager.Controls.Add(STATIC.fManage);
            P_Vision.Controls.Add(STATIC.fVision);

            STATIC.fStart.Log("Vision Initial Prossess..");
            if (!Process.IsVirtual)
            {
                m__G.Initial_Vision(2);  //  SOLIOS = 1, RADIENT = 2, ...
                STATIC.fVision.Show();
            }
            STATIC.fManage.Show();

            InitCondition();

            InitDataSpec();

            InitOption();

            InitTodoList();

            InitModel();

            InitScripPath();

            LoadLastModelFileList();

            if (!Process.IsVirtual) Task.WaitAll(Task.Factory.StartNew(() => { while (!STATIC.fVision.mLoaded) { Thread.Sleep(100); } }));

            STATIC.fStart.Log("Vision Initial Complete.");

            if (!Process.IsVirtual)
            {
                STATIC.fVision.BufferInit();

                STATIC.fVision.StartLive();

                Thread.Sleep(100);

                STATIC.fVision.GrabHalt();

            }

            STATIC.State = (int)STATIC.STATE.Manage;

            if (!Process.IsVirtual) STATIC.fVision.SetExposure(0, Condition.iExposure);

            STATIC.fStart.Invoke(new MethodInvoker(STATIC.fStart.Close));

        }

        public string mFile4ModelFileList = "ModelFileList.txt";
        bool bLoadLastModelFile = true;

        public bool LoadLastModelFileList()
        {
            if (!File.Exists(m__G.m_RootDirectory + "\\DoNotTouch\\" + mFile4ModelFileList))
                return false;

            StreamReader rd = new StreamReader(m__G.m_RootDirectory + "\\DoNotTouch\\" + mFile4ModelFileList);
            string allstring = rd.ReadToEnd();
            rd.Close();
            string[] eachLine = allstring.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            List<string> lPreparedModelFile = new List<string>();
            foreach (string filename in eachLine)
            {
                lPreparedModelFile.Add(filename);
                lbxModelFiles.Items.Add(filename);
            }
            lbxModelFiles.SelectedIndex = eachLine.Length - 1;
            STATIC.fVision.SetModelFileList(lPreparedModelFile.ToArray());
            bLoadLastModelFile = false;
            return true;
        }
        //============================================================
        private void F_Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            Spec.Save();
            if (!Process.IsVirtual)
            {
                Process.LEDs_All_On(0, false);
            }
        }
        private void InitCondition(bool isClear = true)
        {
            if (isClear)
            {
                Actionbox.Items.Clear();
                for (int i = 0; i < Process.ItemList.Count; i++)
                {
                    Actionbox.Items.Add(Process.ItemList[i].Name);
                }

                TodoBox.Items.Clear();
                for (int i = 0; i < Condition.ToDoList.Count; i++)
                {
                    TodoBox.Items.Add(Condition.ToDoList[i]);
                }
                RecipeFileName.Text = Condition.CurrentName;
            }
            AFPidSetPath.BackColor = Color.White;
            XPidSetPath.BackColor = Color.White;
            YPidSetPath.BackColor = Color.White;
            CodeScriptPath.BackColor = Color.White;

            ConditinGrid.ColumnCount = 5;
            ConditinGrid.Font = new Font("Calibri", 10, FontStyle.Bold);
            for (int i = 0; i < ConditinGrid.ColumnCount; i++)
            {
                ConditinGrid.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
            }
            ConditinGrid.RowHeadersVisible = false;
            ConditinGrid.BackgroundColor = Color.LightGray;
            ConditinGrid.Columns[0].Name = "Class";
            ConditinGrid.Columns[1].Name = "Condition Item";
            ConditinGrid.Columns[2].Name = "Value";
            ConditinGrid.Columns[3].Name = "Unit";

            ConditinGrid.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            ConditinGrid.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            ConditinGrid.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            ConditinGrid.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            ConditinGrid.Columns[0].Width = 80;
            ConditinGrid.Columns[1].Width = 150;
            ConditinGrid.Columns[2].Width = 75;
            ConditinGrid.Columns[3].Width = 70;

            ConditinGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            ConditinGrid.ColumnHeadersHeight = 22;

            int effRowNum = 0;
            string colTitle;
            bool bColorChange = false;
            ConditinGrid.Rows.Clear();

            for (int i = 0; i < Condition.Param.Count; i++)
            {
                if (Condition.Param[i][1].ToString().Length >= 9)
                {
                    string compare = Condition.Param[i][1].ToString().Substring(0, 9);
                }

                if (i == 0) colTitle = Condition.Param[i][0].ToString();
                else
                {
                    if (Condition.Param[i - 1][0].ToString() == Condition.Param[i][0].ToString()) colTitle = "";
                    else
                    {
                        colTitle = Condition.Param[i][0].ToString();
                        bColorChange = !bColorChange;
                    }
                }
                ConditinGrid.Rows.Add(colTitle, Condition.Param[i][1], Condition.Param[i][2], Condition.Param[i][3]);

                if (bColorChange)
                {

                    ConditinGrid[0, effRowNum].Style.BackColor = Color.Lavender;
                    ConditinGrid[1, effRowNum].Style.BackColor = Color.Lavender;
                    ConditinGrid[3, effRowNum].Style.BackColor = Color.Lavender;
                }
                else
                {
                    ConditinGrid[0, effRowNum].Style.BackColor = Color.White;
                    ConditinGrid[1, effRowNum].Style.BackColor = Color.White;
                    ConditinGrid[3, effRowNum].Style.BackColor = Color.White;
                }
                ConditinGrid.Rows[effRowNum].Visible = Convert.ToBoolean(Condition.Param[i][4]);
                effRowNum++;
            }
            ConditinGrid.Rows.Add("", "", "", "", "");

            for (int i = 0; i < effRowNum; i++)
            {
                ConditinGrid.Rows[i].Height = 15;
                ConditinGrid.Rows[i].Resizable = DataGridViewTriState.False;
                ConditinGrid.Rows[i].DefaultCellStyle.Font = new Font("Calibri", 9, FontStyle.Bold);
                ConditinGrid[1, i].Style.Font = new Font("Calibri", 9, FontStyle.Bold);
                ConditinGrid[2, i].Style.Font = new Font("Calibri", 9, FontStyle.Bold);
                ConditinGrid[3, i].Style.Font = new Font("Calibri", 9, FontStyle.Italic);
            }

            for (int colum = 2; colum < 3; colum++)
            {
                for (int row = 0; row < effRowNum; row++)
                {
                    ConditinGrid[colum, row].Style.BackColor = Color.LightGray;
                    ConditinGrid.ReadOnly = true;
                }
            }

            IsEdit();
        }
        private void IsEdit()
        {
            if (EditCondition.Checked)
            {
                ConditinGrid.ReadOnly = false;
                for (int row = 0; row < ConditinGrid.Rows.Count; row++)
                {
                    {
                        ConditinGrid[2, row].Style.BackColor = Color.White;
                        ConditinGrid[0, row].ReadOnly = true;
                        ConditinGrid[1, row].ReadOnly = true;
                        ConditinGrid[3, row].ReadOnly = true;
                        ConditinGrid[4, row].ReadOnly = true;
                    }
                }
            }
            else
            {
                ConditinGrid.ReadOnly = true;
                for (int row = 0; row < ConditinGrid.Rows.Count; row++)
                {
                    ConditinGrid[2, row].Style.BackColor = Color.LightGray;
                }
            }
            if (EditSpec.Checked == true)
            {
                SpecGrid.ReadOnly = false;
                for (int row = 0; row < SpecGrid.Rows.Count; row++)
                {
                    {
                        SpecGrid[2, row].Style.BackColor = Color.White;
                        SpecGrid[3, row].Style.BackColor = Color.White;
                        SpecGrid[0, row].ReadOnly = true;
                        SpecGrid[1, row].ReadOnly = true;
                        SpecGrid[4, row].ReadOnly = true;
                    }
                }
            }
            else
            {
                SpecGrid.ReadOnly = true;
                for (int row = 0; row < SpecGrid.Rows.Count; row++)
                {
                    SpecGrid[2, row].Style.BackColor = Color.LightGray;
                    SpecGrid[3, row].Style.BackColor = Color.LightGray;
                }
            }
        }
        private void InitDataSpec()
        {
            SpecGrid.ColumnCount = 5;
            SpecGrid.Font = new Font("Calibri", 10, FontStyle.Bold);
            for (int i = 0; i < SpecGrid.ColumnCount; i++)
            {
                SpecGrid.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
            }
            SpecGrid.RowHeadersVisible = false;
            SpecGrid.BackgroundColor = Color.LightGray;

            SpecFileName.Text = Spec.CurrentName;
            // Column
            SpecGrid.Columns[0].Name = "Axis";
            SpecGrid.Columns[1].Name = "Test Item";
            SpecGrid.Columns[2].Name = "Min";
            SpecGrid.Columns[3].Name = "Max";
            SpecGrid.Columns[4].Name = "unit";
            for (int i = 0; i < 5; i++)
                SpecGrid.Columns[i].DefaultCellStyle.Font = new Font("Calibri", 10, FontStyle.Bold);

            SpecGrid.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            SpecGrid.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;
            SpecGrid.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            SpecGrid.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            SpecGrid.Columns[4].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleLeft;

            SpecGrid.Columns[0].Width = 80;
            SpecGrid.Columns[1].Width = 140;
            SpecGrid.Columns[2].Width = 70;
            SpecGrid.Columns[3].Width = 70;
            SpecGrid.Columns[4].Width = 80;

            // Row
            int effRowNum = 0;
            bool bColorChange = false;
            SpecGrid.Rows.Clear();

            for (int i = 1; i < Spec.Param.Count; i++)
            {
                if (Spec.Param[i - 1][0].ToString() != Spec.Param[i][0].ToString())
                    bColorChange = !bColorChange;

                SpecGrid.Rows.Add(Spec.Param[i][0], Spec.Param[i][1], Spec.Param[i][2], Spec.Param[i][3], Spec.Param[i][9]);
                if (bColorChange)
                {
                    SpecGrid[0, effRowNum].Style.BackColor = Color.Lavender;
                    SpecGrid[1, effRowNum].Style.BackColor = Color.Lavender;
                    SpecGrid[4, effRowNum].Style.BackColor = Color.Lavender;
                }
                else
                {
                    SpecGrid[0, effRowNum].Style.BackColor = Color.White;
                    SpecGrid[1, effRowNum].Style.BackColor = Color.White;
                    SpecGrid[4, effRowNum].Style.BackColor = Color.White;
                }
                SpecGrid.Rows[effRowNum].Visible = Convert.ToBoolean(Spec.Param[i][10]);
                effRowNum++;
            }
            

            string oldkey = "";
            for (int i = 0; i < effRowNum; i++)
            {
                if (SpecGrid.Rows[i].Visible)
                {
                    string newKey = SpecGrid.Rows[i].Cells[0].Value.ToString();
                    if (oldkey == newKey) SpecGrid.Rows[i].Cells[0].Value = "";
                    oldkey = newKey;
                }
            }

            SpecGrid.Rows.Add("", "", "", "", "");

            SpecGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            SpecGrid.ColumnHeadersHeight = 22;

            for (int i = 0; i < effRowNum; i++)
            {
                SpecGrid.Rows[i].Height = 15;     // spec 높이조절A
                SpecGrid.Rows[i].Resizable = DataGridViewTriState.False;
                SpecGrid.Rows[i].DefaultCellStyle.Font = new Font("Calibri", 9, FontStyle.Bold);
                SpecGrid[1, i].Style.Font = new Font("Calibri", 9, FontStyle.Bold);
                SpecGrid[2, i].Style.Font = new Font("Calibri", 9, FontStyle.Bold);
                SpecGrid[4, i].Style.Font = new Font("Calibri", 9, FontStyle.Italic);
            }

            for (int colum = 2; colum < 4; colum++)
            {
                for (int row = 0; row < SpecGrid.Rows.Count; row++)
                {
                    SpecGrid[colum, row].Style.BackColor = Color.LightGray;
                    SpecGrid.ReadOnly = true;
                }
            }
            SpecGrid.ReadOnly = true;
            IsEdit();
        }
        public void UpdateUI()
        {
            Condition.ToDoList.Clear();
            for (int i = 0; i < TodoBox.Items.Count; i++)
            {
                Condition.ToDoList.Add(TodoBox.Items[i].ToString());
            }
            for (int i = 0; i < Condition.Param.Count; i++)
            {
                Condition.Param[i][2] = ConditinGrid[2, i].Value.ToString();
            }
            for (int i = 0; i < Spec.Param.Count - 1; i++)
            {
                Spec.Param[i + 1][2] = SpecGrid[2, i].Value.ToString();
                Spec.Param[i + 1][3] = SpecGrid[3, i].Value.ToString();
            }

            //  sRecipe 변수가 동작하지 않고 있음.
            STATIC.fVision.SetExposure(0, Condition.iExposure);
            STATIC.fVision.SetRawGainNGamma(Condition.iRawGain, Condition.iGamma);

            STATIC.fVision.SetExposure(0, Condition.iExposure);
            STATIC.fVision.SetRawGainNGamma(Condition.iRawGain, Condition.iGamma);

        }
        public List<CheckBox> ListChk = new List<CheckBox>();
        private void InitOption()
        {
            for (int i = 0; i < Option.Param.Count; i++)
            {
                int width = 0;
                int hCal = 40 * i;

                CheckBox Chk = new CheckBox
                {
                    Text = Option.Param[i][0].ToString(),
                    Checked = Convert.ToBoolean(Option.Param[i][1]),
                    Font = new Font("Calibri", 12, FontStyle.Bold),
                    ForeColor = Color.DarkBlue,
                    Location = new Point(300 + width, 30 + hCal),
                    AutoSize = true,
                };
                ModelGroup.Controls.Add(Chk);
                ListChk.Add(Chk);
            }
        }
        private void InitTodoList()
        {
            TodoBox.Items.Clear();
            for (int i = 0; i < Condition.ToDoList.Count; i++)
                TodoBox.Items.Add(Condition.ToDoList[i]);
        }
        private void InitModel()
        {
            LotMaker.Items.Clear();
            for (int i = 0; i < Model.MakerList.Count; i++)
                LotMaker.Items.Add(Model.MakerList[i]);
            LotMaker.SelectedItem = Model.Maker;

            RevisionNo.Text = Model.RevisionNo;
            TesterNo.Text = Model.TesterNo;
            ProductLine.Text = Model.ProductLine;

            SupplierList.Items.Clear();
            for (int i = 0; i < Model.SupplierList.Count; i++)
                SupplierList.Items.Add(Model.SupplierList[i]);
            SupplierList.SelectedItem = Model.Supplier;

            ICList.Items.Clear();
            for (int i = 0; i < Model.ICList.Count; i++)
                ICList.Items.Add(Model.ICList[i]);
            ICList.SelectedItem = Model.DriverIC;

            ModelList.Items.Clear();
            for (int i = 0; i < Model.ModelList.Count; i++)
                ModelList.Items.Add(Model.ModelList[i]);
            ModelList.SelectedItem = Model.ModelName;
        }
        private void InitScripPath()
        {
            AFPidSetPath.Text = Current.AFPidPath;
            XPidSetPath.Text = Current.XPidPath;
            YPidSetPath.Text = Current.YPidPath;
            CodeScriptPath.Text = Current.CodeScriptPath;
        }
        private void ToOperator_Click(object sender, EventArgs e)
        {
            STATIC.State = (int)STATIC.STATE.Manage;
        }

        private void ToVision_Click(object sender, EventArgs e)
        {
            //STATIC.fVision.mLEDcurrent[0] = Condition.LedCurrentL;
            //STATIC.fVision.mLEDcurrent[1] = Condition.LedCurrentR;
            //STATIC.fVision.m_ChannelOn[0] = Process.m_ChannelOn[0];
            //STATIC.fVision.m_ChannelOn[1] = Process.m_ChannelOn[1];
            STATIC.State = (int)STATIC.STATE.Vision;
        }

        private void ToMotion_Click(object sender, EventArgs e)
        {

        }

        private void OpenCondition_Click(object sender, EventArgs e)
        {
            string result = STATIC.OpenFile(Condition.InitDir, Condition.Ext);
            if (result == null) return;
            Condition.Read(result);
            Current.ConditionName = Condition.CurrentName;
            Current.Save();
            InitCondition();
        }

        private void SaveCondition_Click(object sender, EventArgs e)
        {
            UpdateUI();
            Condition.Save();
        }

        private void SaveAsCondition_Click(object sender, EventArgs e)
        {
            string result = STATIC.OpenFile(Condition.InitDir, Condition.Ext, true);
            UpdateUI();
            Condition.Save(result);
            Condition.Read(result);
            Current.ConditionName = Condition.CurrentName;
            InitCondition();
        }

        private void OpenSpec_Click(object sender, EventArgs e)
        {
            string result = STATIC.OpenFile(Spec.InitDir, Spec.Ext);
            if (result == null) return;
            Spec.Read(result);
            Current.SpecName = Spec.CurrentName;
            Current.Save();
            InitDataSpec();
            Process.InitResultData();
        }

        private void SaveSpec_Click(object sender, EventArgs e)
        {
            UpdateUI();
            Spec.Save();
            Process.InitResultData();
        }

        private void SaveAsSpec_Click(object sender, EventArgs e)
        {
            string result = STATIC.OpenFile(Spec.InitDir, Spec.Ext, true);
            if (result == null) return;
            UpdateUI();
            Spec.Save(result);

            Spec.Read(result);
            Current.SpecName = Spec.CurrentName;
            InitDataSpec();
            Process.InitResultData();
        }

        private void ApplyTester_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < Option.Param.Count; i++)
                Option.Param[i][1] = ListChk[i].Checked;
            Option.Save();
            //Model ==
            if (LotMaker.SelectedItem != null) Model.Maker = LotMaker.SelectedItem.ToString();
            Model.RevisionNo = RevisionNo.Text;
            Model.TesterNo = TesterNo.Text;
            Model.ProductLine = ProductLine.Text;
            if (SupplierList.SelectedItem != null) Model.Supplier = SupplierList.SelectedItem.ToString();
            Model.MCNumber = MCNumber.Text;
            if (ICList.SelectedItem != null) Model.DriverIC = ICList.SelectedItem.ToString();
            if (ModelList.SelectedItem != null) Model.ModelName = ModelList.SelectedItem.ToString();
            Model.Save();

            InitModel();
        }

        private void RemoveItem_Click(object sender, EventArgs e)
        {
            if (TodoBox.SelectedItems == null) return;
            for (int i = 0; i < TodoBox.SelectedItems.Count; i++)
            {
                string sName = TodoBox.SelectedItems[i].ToString();
                foreach (var l in Process.ItemList)
                    Condition.ToDoList.Remove(sName);

            }
            InitTodoList();
        }

        private void AddItem_Click(object sender, EventArgs e)
        {
            if (Actionbox.SelectedItems == null) return;
            for (int i = 0; i < Actionbox.SelectedItems.Count; i++)
            {
                string target = Actionbox.SelectedItems[i].ToString();
                foreach (var l in Process.ItemList)
                    if (l.Name == target)
                    {
                        bool isExist = false;
                        foreach (var t in Condition.ToDoList) if (t == target) isExist = true;
                        if (!isExist) Condition.ToDoList.Add(l.Name);
                    }
            }

            InitTodoList();
        }

        private void Move_Up_Click(object sender, EventArgs e)
        {
            MoveTodo(true);
        }

        private void Move_Down_Click(object sender, EventArgs e)
        {
            MoveTodo(false);
        }

        public void MoveTodo(bool dir) // true : up, false : down
        {
            if (TodoBox.SelectedItems.Count > 1) return;
            int cIndex = TodoBox.SelectedIndex;
            if (cIndex < 0) return;
            if (cIndex <= 0 && dir) return;
            if ((cIndex + 1 >= TodoBox.Items.Count) && !dir) return;

            int target = 0;
            for (int i = 0; i < TodoBox.SelectedItems.Count; i++)
            {
                if (dir)
                    Condition.ToDoList.Move(cIndex + i, target = (cIndex + i - 1));
                else
                    Condition.ToDoList.Move(cIndex + i, target = (cIndex + i + 1));
            }

            TodoBox.Items.Clear();
            for (int i = 0; i < Condition.ToDoList.Count; i++)
            {
                TodoBox.Items.Add(Condition.ToDoList[i]);
            }
            TodoBox.SelectedIndex = target;
        }

        private void SpecGrid_CellMouseDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                new TestItemOnOff().ShowDialog();
                InitDataSpec();
            }
        }

        private void EditCondition_CheckedChanged(object sender, EventArgs e)
        {
            IsEdit();
        }

        private void EditSpec_CheckedChanged(object sender, EventArgs e)
        {
            IsEdit();
        }

        private void SetAFPIDUpdate_Click(object sender, EventArgs e)
        {
            string result = STATIC.OpenFile(Rcp.AfPidSet.InitDir, Rcp.AfPidSet.Ext);
            if (result == null) return;
            Rcp.AfPidSet.Read(result);
            Current.AFPidPath = Rcp.AfPidSet.CurrentName;
            Current.Save();
            AFPidSetPath.Text = Current.AFPidPath;
        }

        private void SetXPIDUpdate_Click(object sender, EventArgs e)
        {
            string result = STATIC.OpenFile(Rcp.XPidSet.InitDir, Rcp.XPidSet.Ext);
            if (result == null) return;
            Rcp.XPidSet.Read(result);
            Current.XPidPath = Rcp.XPidSet.CurrentName;
            Current.Save();
            XPidSetPath.Text = Current.XPidPath;
        }

        private void SetYPIDUpdate_Click(object sender, EventArgs e)
        {
            string result = STATIC.OpenFile(Rcp.YPidSet.InitDir, Rcp.YPidSet.Ext);
            if (result == null) return;
            Rcp.YPidSet.Read(result);
            Current.YPidPath = Rcp.YPidSet.CurrentName;
            Current.Save();
            YPidSetPath.Text = Current.YPidPath;
        }

        private void SetCodeScript_Click(object sender, EventArgs e)
        {
            string result = STATIC.OpenFile(Rcp.CodeScript.InitDir, Rcp.CodeScript.Ext);
            if (result == null) return;
            Rcp.CodeScript.Read(result);
            Current.CodeScriptPath = Rcp.CodeScript.CurrentName;
            Current.Save();
            CodeScriptPath.Text = Current.CodeScriptPath;
        }

        private void Actionbox_SelectedIndexChanged(object sender, EventArgs e)
        {
            InitCondition(false);
            ListBox b = (ListBox)sender;
            if (b.SelectedItem == null) return;
            switch(b.SelectedItem.ToString())
            {
                case "PID Setting":
                    AFPidSetPath.BackColor = Color.Orange;
                    XPidSetPath.BackColor = Color.Orange;
                    YPidSetPath.BackColor = Color.Orange;
                    break;
                case "Aging OpenLoop":
                    for (int i = 0; i < Condition.Param.Count; i++)
                    {
                        if (Condition.Param[i][0].ToString() == ("Aging"))
                        {
                            ConditinGrid[0, i].Style.BackColor = Color.Orange;
                            ConditinGrid[1, i].Style.BackColor = Color.Orange;
                        }
                    }
                    break;
                case "Hall Calibration":
                    for (int i = 0; i < Condition.Param.Count; i++)
                    {
                        if (Condition.Param[i][0].ToString() == ("Hall Cal"))
                        {
                            ConditinGrid[0, i].Style.BackColor = Color.Orange;
                            ConditinGrid[1, i].Style.BackColor = Color.Orange;
                        }
                    }
                    break;
                case "AF Scan":
                case "AF Scan2":
                case "AF Scan3":
                case "AF Scan4":
                case "AF Settling":
                    for (int i = 0; i < Condition.Param.Count; i++)
                    {
                        if (Condition.Param[i][0].ToString() == ("AF"))
                        {
                            ConditinGrid[0, i].Style.BackColor = Color.Orange;
                            ConditinGrid[1, i].Style.BackColor = Color.Orange;
                        }
                    }
                    break;
                case "OIS X Scan":
                case "OIS X Scan2":
                case "OIS X Scan3":
                case "OIS X Scan4":
                    for (int i = 0; i < Condition.Param.Count; i++)
                    {
                        if (Condition.Param[i][0].ToString() == ("X"))
                        {
                            ConditinGrid[0, i].Style.BackColor = Color.Orange;
                            ConditinGrid[1, i].Style.BackColor = Color.Orange;
                        }
                    }
                    break;
                case "OIS Y Scan":
                case "OIS Y Scan2":
                case "OIS Y Scan3":
                case "OIS Y Scan4":
                    for (int i = 0; i < Condition.Param.Count; i++)
                    {
                        if (Condition.Param[i][0].ToString() == ("Y") ||
                            Condition.Param[i][0].ToString() == ("Y1") ||
                            Condition.Param[i][0].ToString() == ("Y2"))
                        {
                            ConditinGrid[0, i].Style.BackColor = Color.Orange;
                            ConditinGrid[1, i].Style.BackColor = Color.Orange;
                        }
                    }
                    break;
                case "OIS Matrix Scan":
                    CodeScriptPath.BackColor = Color.Orange;
                    for (int i = 0; i < Condition.Param.Count; i++)
                    {
                        if (Condition.Param[i][0].ToString() == ("Matrix"))
                        {
                            ConditinGrid[0, i].Style.BackColor = Color.Orange;
                            ConditinGrid[1, i].Style.BackColor = Color.Orange;
                        }
                    }
                    break;
                case "OIS X EPA":
                case "OIS Y EPA":
                case "OIS X EPA Recipe":
                case "OIS Y EPA Recipe":
                case "OIS X Ex EPA Recipe":
                case "OIS Y Ex EPA Recipe":
                    for (int i = 0; i < Condition.Param.Count; i++)
                    {
                        if (Condition.Param[i][0].ToString() == ("EPA"))
                        {
                            ConditinGrid[0, i].Style.BackColor = Color.Orange;
                            ConditinGrid[1, i].Style.BackColor = Color.Orange;
                        }
                    }
                    break;
                case "OIS X Linearity Comp":
                case "OIS Y Linearity Comp":
                case "OIS X Linearity Comp2":
                case "OIS Y Linearity Comp2":
                    for (int i = 0; i < Condition.Param.Count; i++)
                    {
                        if (Condition.Param[i][0].ToString() == ("Linearity Comp"))
                        {
                            ConditinGrid[0, i].Style.BackColor = Color.Orange;
                            ConditinGrid[1, i].Style.BackColor = Color.Orange;
                        }
                    }
                    break;
                case "Gain@10Hz":
                    for (int i = 0; i < Condition.Param.Count; i++)
                    {
                        if (Condition.Param[i][0].ToString() == ("LG @ 10Hz"))
                        {
                            ConditinGrid[0, i].Style.BackColor = Color.Orange;
                            ConditinGrid[1, i].Style.BackColor = Color.Orange;
                        }
                    }
                    break;
                case "Phase Margin":
                    for (int i = 0; i < Condition.Param.Count; i++)
                    {
                        if (Condition.Param[i][0].ToString().Contains("Phase Margin"))
                        {
                            ConditinGrid[0, i].Style.BackColor = Color.Orange;
                            ConditinGrid[1, i].Style.BackColor = Color.Orange;
                        }
                    }
                    break;
                case "Gain Margin":
                    for (int i = 0; i < Condition.Param.Count; i++)
                    {
                        if (Condition.Param[i][0].ToString().Contains("Gain Margin"))
                        {
                            ConditinGrid[0, i].Style.BackColor = Color.Orange;
                            ConditinGrid[1, i].Style.BackColor = Color.Orange;
                        }
                    }
                    break;
                case "OIS Hall Test":
                    for (int i = 0; i < Condition.Param.Count; i++)
                    {
                        if (Condition.Param[i][0].ToString().Contains("Sine Wave") ||
                            Condition.Param[i][0].ToString().Contains("Ringing"))
                        {
                            ConditinGrid[0, i].Style.BackColor = Color.Orange;
                            ConditinGrid[1, i].Style.BackColor = Color.Orange;
                        }
                    }
                    break;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //var folder = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, @"..\\Training"));
            //string sFilePath = folder;
            string sFilePath = m__G.m_RootDirectory + "\\DoNotTouch\\Training";
            if (!Directory.Exists(sFilePath))
                Directory.CreateDirectory(sFilePath);

            List<string> lPreparedModelFile = new List<string>();

            OpenFileDialog openFile = new OpenFileDialog();
            openFile.DefaultExt = "xml";
            openFile.InitialDirectory = sFilePath;
            openFile.Multiselect = true;
            openFile.Filter = "XML(*.xml)|*.xml";
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                lbxModelFiles.Items.Clear();
                StreamWriter wr = new StreamWriter(m__G.m_RootDirectory + "\\DoNotTouch\\" + mFile4ModelFileList);
                foreach (string filename in openFile.FileNames)
                {
                    lPreparedModelFile.Add(filename);
                    lbxModelFiles.Items.Add(filename);
                    wr.WriteLine(filename);
                }
                wr.Close();
                STATIC.fVision.SetModelFileList(lPreparedModelFile.ToArray());
                STATIC.fVision.TransferModelFileList();
            }
            else
                return;
        }

        private void lbxModelFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (m__G == null)
                return;
            if (m__G.oCam[0] == null)
                return;
            if (m__G.oCam[0].mFAL == null)
                return;
            if (lbxModelFiles.SelectedItem == null)
                return;

            if (!m__G.mFAL.mFAutoLearnLoaded)
            {
                m__G.mFAL.Show();
                m__G.mFAL.WindowState = System.Windows.Forms.FormWindowState.Maximized;
                //mFAL.Size = new Size(1920, 1045);
                m__G.mFAL.Location = new Point(0, 0);
                m__G.mFAL.Hide();
            }

            if (!bLoadLastModelFile)
            {
                string sFile = lbxModelFiles.SelectedItem.ToString();
                int lModelScale = m__G.oCam[0].mFAL.ExternalLoadFMIFile(sFile);
                if (m__G != null)
                    if (m__G.oCam[0] != null)
                        m__G.oCam[0].ResetModelScale(1.0 / lModelScale);

                lblDefaultModel.Text = sFile;
            }
        }
    }
}
