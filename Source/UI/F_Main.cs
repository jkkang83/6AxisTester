using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
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
        [Serializable]
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct sSaveResult
        {
            [MarshalAs(UnmanagedType.I8)]
            public Int64 sTime;             //  sTime = DateTime.Now.ToBinary(), When reading, DateTime now = DateTime.FromBinary(sTime);
            [MarshalAs(UnmanagedType.I4)]
            public int frameCount;
            [MarshalAs(UnmanagedType.R8)]
            public double fps;
            [MarshalAs(UnmanagedType.R8)]
            public double ledLeft;
            [MarshalAs(UnmanagedType.R8)]
            public double ledRight;
            [MarshalAs(UnmanagedType.R8)]
            public double testTime;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10000)]
            public double[] X;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10000)]
            public double[] Y;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10000)]
            public double[] Z;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10000)]
            public double[] TX;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10000)]
            public double[] TY;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10000)]
            public double[] TZ;
        }
        [Serializable]
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct sSaveResult5
        {
            [MarshalAs(UnmanagedType.I8)]
            public Int64 sTime;             //  sTime = DateTime.Now.ToBinary(), When reading, DateTime now = DateTime.FromBinary(sTime);
            [MarshalAs(UnmanagedType.I4)]
            public int frameCount;
            [MarshalAs(UnmanagedType.R8)]
            public double fps;
            [MarshalAs(UnmanagedType.R8)]
            public double ledLeft;
            [MarshalAs(UnmanagedType.R8)]
            public double ledRight;
            [MarshalAs(UnmanagedType.R8)]
            public double testTime;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5000)]
            public double[] X;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5000)]
            public double[] Y;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5000)]
            public double[] Z;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5000)]
            public double[] TX;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5000)]
            public double[] TY;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5000)]
            public double[] TZ;
        }
        [Serializable]
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct sSaveResult3
        {
            [MarshalAs(UnmanagedType.I8)]
            public Int64 sTime;             //  sTime = DateTime.Now.ToBinary(), When reading, DateTime now = DateTime.FromBinary(sTime);
            [MarshalAs(UnmanagedType.I4)]
            public int frameCount;
            [MarshalAs(UnmanagedType.R8)]
            public double fps;
            [MarshalAs(UnmanagedType.R8)]
            public double ledLeft;
            [MarshalAs(UnmanagedType.R8)]
            public double ledRight;
            [MarshalAs(UnmanagedType.R8)]
            public double testTime;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3000)]
            public double[] X;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3000)]
            public double[] Y;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3000)]
            public double[] Z;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3000)]
            public double[] TX;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3000)]
            public double[] TY;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3000)]
            public double[] TZ;
        }
        [Serializable]
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct sSaveResult1
        {
            [MarshalAs(UnmanagedType.I8)]
            public Int64 sTime;             //  sTime = DateTime.Now.ToBinary(), When reading, DateTime now = DateTime.FromBinary(sTime);
            [MarshalAs(UnmanagedType.I4)]
            public int frameCount;
            [MarshalAs(UnmanagedType.R8)]
            public double fps;
            [MarshalAs(UnmanagedType.R8)]
            public double ledLeft;
            [MarshalAs(UnmanagedType.R8)]
            public double ledRight;
            [MarshalAs(UnmanagedType.R8)]
            public double testTime;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1000)]
            public double[] X;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1000)]
            public double[] Y;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1000)]
            public double[] Z;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1000)]
            public double[] TX;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1000)]
            public double[] TY;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1000)]
            public double[] TZ;
        }
        [Serializable]
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct sSaveResult0
        {
            [MarshalAs(UnmanagedType.I8)]
            public Int64 sTime;             //  sTime = DateTime.Now.ToBinary(), When reading, DateTime now = DateTime.FromBinary(sTime);
            [MarshalAs(UnmanagedType.I4)]
            public int frameCount;
            [MarshalAs(UnmanagedType.R8)]
            public double fps;
            [MarshalAs(UnmanagedType.R8)]
            public double ledLeft;
            [MarshalAs(UnmanagedType.R8)]
            public double ledRight;
            [MarshalAs(UnmanagedType.R8)]
            public double testTime;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
            public double[] X;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
            public double[] Y;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
            public double[] Z;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
            public double[] TX;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
            public double[] TY;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 100)]
            public double[] TZ;
        }
        [Serializable]
        public class sSaveResultBin
        {
            public Int64 sTime;
            public int frameCount;
            public double fps;
            public double ledLeft;
            public double ledRight;
            public double testTime;
            public double[] X;
            public double[] Y;
            public double[] Z;
            public double[] TX;
            public double[] TY;
            public double[] TZ;
        }
        public class sSaveResultPos
        {
            public Int64 sTime;
            public int frameCount;
            public double fps;
            public double ledLeft;
            public double ledRight;
            public double testTime;
            public double[] X1 = new double[5000];
            public double[] Y1 = new double[5000];
            public double[] X2 = new double[5000];
            public double[] Y2 = new double[5000];
            public double[] X3 = new double[5000];
            public double[] Y3 = new double[5000];
            public double[] X4 = new double[5000];
            public double[] Y4 = new double[5000];
            public double[] X5 = new double[5000];
            public double[] Y5 = new double[5000];
        }
        public static sSaveResultPos ReadsSaveResultPos(string FileName)
        {
            System.Xml.Serialization.XmlSerializer reader =
                new System.Xml.Serialization.XmlSerializer(typeof(sSaveResultPos));

            System.IO.StreamReader file = new System.IO.StreamReader(FileName);
            sSaveResultPos res = (sSaveResultPos)reader.Deserialize(file);
            file.Close();

            return res;
        }

        public static sSaveResult ReadsSaveResult(string FileName)
        {
            sSaveResult sRes = new sSaveResult();
            int size = Marshal.SizeOf(typeof(sSaveResult));

            Stream iStream = File.OpenRead(FileName);
            BinaryReader reader = new BinaryReader(iStream);

            IntPtr ptr = Marshal.AllocHGlobal(size);
            byte[] buffer = reader.ReadBytes(size);
            Marshal.Copy(buffer, 0, ptr, size);
            sRes = (sSaveResult)Marshal.PtrToStructure(ptr, typeof(sSaveResult));
            reader.Close();
            Marshal.FreeHGlobal(ptr);

            return sRes;
        }
        public static sSaveResult5 ReadsSaveResult5(string FileName)
        {
            sSaveResult5 sRes = new sSaveResult5();
            int size = Marshal.SizeOf(typeof(sSaveResult5));

            Stream iStream = File.OpenRead(FileName);
            BinaryReader reader = new BinaryReader(iStream);

            IntPtr ptr = Marshal.AllocHGlobal(size);
            byte[] buffer = reader.ReadBytes(size);
            Marshal.Copy(buffer, 0, ptr, size);
            sRes = (sSaveResult5)Marshal.PtrToStructure(ptr, typeof(sSaveResult5));
            reader.Close();
            Marshal.FreeHGlobal(ptr);

            return sRes;
        }
        public static sSaveResult3 ReadsSaveResult3(string FileName)
        {
            sSaveResult3 sRes = new sSaveResult3();
            int size = Marshal.SizeOf(typeof(sSaveResult3));

            Stream iStream = File.OpenRead(FileName);
            BinaryReader reader = new BinaryReader(iStream);

            IntPtr ptr = Marshal.AllocHGlobal(size);
            byte[] buffer = reader.ReadBytes(size);
            Marshal.Copy(buffer, 0, ptr, size);
            sRes = (sSaveResult3)Marshal.PtrToStructure(ptr, typeof(sSaveResult3));
            reader.Close();
            Marshal.FreeHGlobal(ptr);

            return sRes;
        }

        public static object ReadsSaveResultBin(string FileName)
        {
            Stream rs = new FileStream(FileName, FileMode.Open);
            BinaryFormatter bf = new BinaryFormatter();

            object sRes = bf.Deserialize(rs);
            rs.Close();

            return sRes;
        }

        public string ReadResultPos(string sFileName)
        {
            //Check if result file is readable
            string filename = Path.GetFileName(sFileName);
            string[] strframeCount = filename.Split('_');

            int framCnt = int.Parse(strframeCount[0]);
            string lstr = "";
            sSaveResultPos result = ReadsSaveResultPos(sFileName);
            int len = result.frameCount;
            DateTimeOffset ltime = DateTimeOffset.FromUnixTimeSeconds(result.sTime);
            DateTime TimeTested = ltime.DateTime;
            if (sFileName.Contains("_0_"))
                lstr = TimeTested.ToLocalTime().ToString() + "\t" + "X1 smt" + "\t" + "Y1 smt" + "\t" + "X2 smt" + "\t" + "Y2 smt" + "\t" + "X3 smt" + "\t" + "Y3 smt" + "\t" + "X4 smt" + "\t" + "Y4 smt" + "\t" + "X5 smt" + "\t" + "Y5 smt" + "\r\n";
            else
                lstr = TimeTested.ToLocalTime().ToString() + "\t" + "X1 std" + "\t" + "Y1 std" + "\t" + "X2 std" + "\t" + "Y2 std" + "\t" + "X3 std" + "\t" + "Y3 std" + "\t" + "X4 std" + "\t" + "Y4 std" + "\t" + "X5 std" + "\t" + "Y5 std" + "\r\n";

            for (int i = 0; i < len; i++)
            {
                lstr += i.ToString() + "\t" + result.X1[i].ToString("F3") + "\t" + result.Y1[i].ToString("F3")
                                     + "\t" + result.X2[i].ToString("F3") + "\t" + result.Y2[i].ToString("F3")
                                     + "\t" + result.X3[i].ToString("F3") + "\t" + result.Y3[i].ToString("F3")
                                     + "\t" + result.X4[i].ToString("F3") + "\t" + result.Y4[i].ToString("F3")
                                     + "\t" + result.X5[i].ToString("F3") + "\t" + result.Y5[i].ToString("F3") + "\r\n";


            }
            return lstr;
        }
        public string WriteResultBin(int modelIndex = 0)
        {
            string sLotName = STATIC.fManage.GetLotName();
            m__G.mNowLotName = sLotName;

            string sLotDir = STATIC.fManage.CheckResultFolder();
            if (sLotName != "")
                sLotDir = sLotDir + sLotName;

            if (!Directory.Exists(sLotDir))
                Directory.CreateDirectory(sLotDir);

            DateTime dtNow = DateTime.Now;   // 현재 날짜, 시간 얻기
            int framCnt = STATIC.fVision.GetTriggerGrabbedFrame();

            if (framCnt > m__G.oCam[0].mTargetTriggerCount)
                framCnt = m__G.oCam[0].mTargetTriggerCount;

            string filename = framCnt.ToString() + "_" + modelIndex.ToString() + "_" + dtNow.ToString("yyMMddHHmmss.fff") + ".dat";
            string sFilePath = sLotDir + filename;

            double umscale = 5.5 / Global.LensMag;                           //  rad to min
            double minscale = 180 / Math.PI * 60;                           //  rad to min
            double preZ = 0;
            int i = 0;

            if (framCnt > 5000)
            {
                sSaveResult sResult = new sSaveResult();

                sResult.X = new double[10000];
                sResult.Y = new double[10000];
                sResult.Z = new double[10000];
                sResult.TX = new double[10000];
                sResult.TY = new double[10000];
                sResult.TZ = new double[10000];

                DateTime startDateTime = m__G.oCam[0].GetLastTriggerTime();
                DateTimeOffset datetimeOffset = new DateTimeOffset(startDateTime);
                long unixTime = datetimeOffset.ToUnixTimeSeconds();
                //sResult.sTime = startDateTime.ToBinary();
                sResult.sTime = unixTime;
                sResult.frameCount = framCnt;
                sResult.fps = STATIC.fVision.GetTriggerGrabbedFPS();
                sResult.ledLeft = STATIC.fVision.mLEDcurrent[0];
                sResult.ledRight = STATIC.fVision.mLEDcurrent[1];
                sResult.testTime = STATIC.fVision.GetHowLongItTook();

                //////  임시  230924
                ////double tx0 = m__G.oCam[0].mC_pTX[0];
                ////double ty0 = m__G.oCam[0].mC_pTY[0];
                ////double tz0 = m__G.oCam[0].mC_pTZ[0];

                for (i = 0; i < 5; i++)
                {
                    sResult.X[0] += m__G.oCam[0].mC_pX[i] * umscale / 5;  //  um
                    sResult.Y[0] += m__G.oCam[0].mC_pY[i] * umscale / 5;  //  um
                    sResult.Z[0] += m__G.oCam[0].mC_pZ[i] * umscale / 5;  //  um
                    //preZ = m__G.oCam[0].mC_pZ[i] * umscale;
                    //sResult.Z[i] += m__G.oCam[0].mC_pZ[i] * umscale / 5;
                    sResult.TX[0] += m__G.oCam[0].mC_pTX[i] * minscale / 5; //  min
                    sResult.TY[0] += m__G.oCam[0].mC_pTY[i] * minscale / 5; //  min
                    sResult.TZ[0] += m__G.oCam[0].mC_pTZ[i] * minscale / 5; //  min
                }

                for (i = 1; i < framCnt; i++)
                {
                    sResult.X[i] = m__G.oCam[0].mC_pX[i] * umscale;  //  um
                    sResult.Y[i] = m__G.oCam[0].mC_pY[i] * umscale;  //  um
                    sResult.Z[i] = m__G.oCam[0].mC_pZ[i] * umscale;  //  um
                    //preZ = m__G.oCam[0].mC_pZ[i] * umscale;
                    //sResult.Z[i] = m__G.fVision.ApplyZLUT(preZ);
                    sResult.TX[i] = m__G.oCam[0].mC_pTX[i] * minscale; //  min
                    sResult.TY[i] = m__G.oCam[0].mC_pTY[i] * minscale; //  min
                    sResult.TZ[i] = m__G.oCam[0].mC_pTZ[i] * minscale; //  min
                }

                int size = 0;
                try
                {
                    size = Marshal.SizeOf(sResult);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                }
                IntPtr wPtr = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(sResult, wPtr, true);
                //byte[] sDataBuff = new byte[size];
                //Marshal.Copy(wPtr, sDataBuff, 0, size);
                //wr.Write(sDataBuff);
                STATIC.fManage.sDataBuff = new byte[size];
                Marshal.Copy(wPtr, STATIC.fManage.sDataBuff, 0, size);

                if (m__G.m_bSaveRawData)
                {
                    BinaryWriter wr = new BinaryWriter(File.OpenWrite(sFilePath));
                    wr.Write(STATIC.fManage.sDataBuff);
                    wr.Flush();
                    wr.Close();
                }

                Marshal.FreeHGlobal(wPtr);
            }
            else if (framCnt > 3000)
            {
                sSaveResult5 sResult = new sSaveResult5();

                sResult.X = new double[5000];
                sResult.Y = new double[5000];
                sResult.Z = new double[5000];
                sResult.TX = new double[5000];
                sResult.TY = new double[5000];
                sResult.TZ = new double[5000];

                DateTime startDateTime = m__G.oCam[0].GetLastTriggerTime();
                DateTimeOffset datetimeOffset = new DateTimeOffset(startDateTime);
                long unixTime = datetimeOffset.ToUnixTimeSeconds();
                //sResult.sTime = startDateTime.ToBinary();
                sResult.sTime = unixTime;
                sResult.frameCount = framCnt;
                sResult.fps = STATIC.fVision.GetTriggerGrabbedFPS();
                sResult.ledLeft = STATIC.fVision.mLEDcurrent[0];
                sResult.ledRight = STATIC.fVision.mLEDcurrent[1];
                sResult.testTime = STATIC.fVision.GetHowLongItTook();

                for (i = 0; i < 5; i++)
                {
                    sResult.X[0] += m__G.oCam[0].mC_pX[i] * umscale / 5;  //  um
                    sResult.Y[0] += m__G.oCam[0].mC_pY[i] * umscale / 5;  //  um
                    sResult.Z[0] += m__G.oCam[0].mC_pZ[i] * umscale / 5;  //  um
                    //preZ = m__G.oCam[0].mC_pZ[i] * umscale;
                    //sResult.Z[i] += m__G.oCam[0].mC_pZ[i] * umscale / 5;
                    sResult.TX[0] += m__G.oCam[0].mC_pTX[i] * minscale / 5; //  min
                    sResult.TY[0] += m__G.oCam[0].mC_pTY[i] * minscale / 5; //  min
                    sResult.TZ[0] += m__G.oCam[0].mC_pTZ[i] * minscale / 5; //  min
                }

                for (i = 1; i < framCnt; i++)
                {
                    sResult.X[i] = m__G.oCam[0].mC_pX[i] * umscale;  //  um
                    sResult.Y[i] = m__G.oCam[0].mC_pY[i] * umscale;  //  um
                    sResult.Z[i] = m__G.oCam[0].mC_pZ[i] * umscale;  //  um     //  zlut 적용 검토
                    //preZ = m__G.oCam[0].mC_pZ[i] * umscale;
                    //sResult.Z[i] = m__G.fVision.ApplyZLUT(preZ);
                    sResult.TX[i] = m__G.oCam[0].mC_pTX[i] * minscale; //  min
                    sResult.TY[i] = m__G.oCam[0].mC_pTY[i] * minscale; //  min
                    sResult.TZ[i] = m__G.oCam[0].mC_pTZ[i] * minscale; //  min
                }

                int size = 0;
                try
                {
                    size = Marshal.SizeOf(sResult);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                }
                IntPtr wPtr = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(sResult, wPtr, true);
                //byte[] sDataBuff = new byte[size];
                //Marshal.Copy(wPtr, sDataBuff, 0, size);
                //wr.Write(sDataBuff);
                STATIC.fManage.sDataBuff = new byte[size];
                Marshal.Copy(wPtr, STATIC.fManage.sDataBuff, 0, size);

                if (m__G.m_bSaveRawData)
                {
                    BinaryWriter wr = new BinaryWriter(File.OpenWrite(sFilePath));
                    wr.Write(STATIC.fManage.sDataBuff);
                    wr.Flush();
                    wr.Close();
                }

                Marshal.FreeHGlobal(wPtr);
            }
            else
            {
                sSaveResult3 sResult = new sSaveResult3();

                sResult.X = new double[3000];
                sResult.Y = new double[3000];
                sResult.Z = new double[3000];
                sResult.TX = new double[3000];
                sResult.TY = new double[3000];
                sResult.TZ = new double[3000];

                DateTime startDateTime = m__G.oCam[0].GetLastTriggerTime();
                DateTimeOffset datetimeOffset = new DateTimeOffset(startDateTime);
                long unixTime = datetimeOffset.ToUnixTimeSeconds();
                //sResult.sTime = startDateTime.ToBinary();
                sResult.sTime = unixTime;
                sResult.frameCount = framCnt;
                sResult.fps = STATIC.fVision.GetTriggerGrabbedFPS();
                sResult.ledLeft = STATIC.fVision.mLEDcurrent[0];
                sResult.ledRight = STATIC.fVision.mLEDcurrent[1];
                sResult.testTime = STATIC.fVision.GetHowLongItTook();

                if (framCnt > 1000)
                {
                    for (i = 0; i < 5; i++)
                    {
                        sResult.X[0] += m__G.oCam[0].mC_pX[i] * umscale / 5;  //  um
                        sResult.Y[0] += m__G.oCam[0].mC_pY[i] * umscale / 5;  //  um
                        sResult.Z[0] += m__G.oCam[0].mC_pZ[i] * umscale / 5;  //  um
                                                                              //preZ = m__G.oCam[0].mC_pZ[i] * umscale;
                                                                              //sResult.Z[i] += m__G.oCam[0].mC_pZ[i] * umscale / 5;
                        sResult.TX[0] += m__G.oCam[0].mC_pTX[i] * minscale / 5; //  min
                        sResult.TY[0] += m__G.oCam[0].mC_pTY[i] * minscale / 5; //  min
                        sResult.TZ[0] += m__G.oCam[0].mC_pTZ[i] * minscale / 5; //  min
                    }

                    for (i = 1; i < framCnt; i++)
                    {
                        sResult.X[i] = m__G.oCam[0].mC_pX[i] * umscale;  //  um
                        sResult.Y[i] = m__G.oCam[0].mC_pY[i] * umscale;  //  um
                        sResult.Z[i] = m__G.oCam[0].mC_pZ[i] * umscale;  //  um     //  zlut 적용 검토
                                                                         //preZ = m__G.oCam[0].mC_pZ[i] * umscale;
                                                                         //sResult.Z[i] = m__G.fVision.ApplyZLUT(preZ);
                        sResult.TX[i] = m__G.oCam[0].mC_pTX[i] * minscale; //  min
                        sResult.TY[i] = m__G.oCam[0].mC_pTY[i] * minscale; //  min
                        sResult.TZ[i] = m__G.oCam[0].mC_pTZ[i] * minscale; //  min
                    }
                }
                else
                {
                    for (i = 0; i < framCnt; i++)
                    {
                        sResult.X[i] = m__G.oCam[0].mC_pX[i] * umscale;  //  um
                        sResult.Y[i] = m__G.oCam[0].mC_pY[i] * umscale;  //  um
                        sResult.Z[i] = m__G.oCam[0].mC_pZ[i] * umscale;  //  um     //  zlut 적용 검토
                                                                         //preZ = m__G.oCam[0].mC_pZ[i] * umscale;
                                                                         //sResult.Z[i] = m__G.fVision.ApplyZLUT(preZ);
                        sResult.TX[i] = m__G.oCam[0].mC_pTX[i] * minscale; //  min
                        sResult.TY[i] = m__G.oCam[0].mC_pTY[i] * minscale; //  min
                        sResult.TZ[i] = m__G.oCam[0].mC_pTZ[i] * minscale; //  min
                    }
                }


                int size = 0;
                try
                {
                    size = Marshal.SizeOf(sResult);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                }
                IntPtr wPtr = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(sResult, wPtr, true);
                //byte[] sDataBuff = new byte[size];
                //Marshal.Copy(wPtr, sDataBuff, 0, size);
                //wr.Write(sDataBuff);
                STATIC.fManage.sDataBuff = new byte[size];
                Marshal.Copy(wPtr, STATIC.fManage.sDataBuff, 0, size);

                if (m__G.m_bSaveRawData)
                {
                    BinaryWriter wr = new BinaryWriter(File.OpenWrite(sFilePath));
                    wr.Write(STATIC.fManage.sDataBuff);
                    wr.Flush();
                    wr.Close();
                }

                Marshal.FreeHGlobal(wPtr);
            }

            //  Verify Read process
            //Thread.Sleep(100);
            //sSaveResult3 sRes = ReadsSaveResult3(sFilePath);
            //DateTime now = DateTime.FromBinary(sRes.sTime);

            return sFilePath;
        }
        public string ReadResultBin(string sFileName)
        {
            //Check if result file is readable
            string filename = Path.GetFileName(sFileName);
            string[] strframeCount = filename.Split('_');

            int framCnt = int.Parse(strframeCount[0]);
            string lstr = "";
            string strHead = "";
            if (sFileName.Contains("_0_"))
                strHead = "\tX SMT\tY SMT\tZ SMT\tTX SMT\tTY SMT\tTZ SMT";
            else
                strHead = "\tX std\tY std\tZ std\tTX std\tTY std\tTZ std";

            if (framCnt > 5000)
            {
                sSaveResult result = ReadsSaveResult(sFileName);
                int len = result.frameCount;
                DateTimeOffset ltime = DateTimeOffset.FromUnixTimeSeconds(result.sTime);
                DateTime TimeTested = ltime.DateTime;
                lstr = TimeTested.ToLocalTime().ToString() + strHead + "\r\n";
                for (int i = 0; i < len; i++)
                {
                    lstr += i.ToString() + "\t" + result.X[i].ToString("F2") + "\t" + result.Y[i].ToString("F2") + "\t" + result.Z[i].ToString("F2") + "\t" + result.TX[i].ToString("F2") + "\t" + result.TY[i].ToString("F2") + "\t" + result.TZ[i].ToString("F2") + "\r\n";
                }
            }
            else if (framCnt > 3000)
            {
                sSaveResult5 result5 = ReadsSaveResult5(sFileName);
                int len = result5.frameCount;
                DateTimeOffset ltime = DateTimeOffset.FromUnixTimeSeconds(result5.sTime);
                DateTime TimeTested = ltime.DateTime;
                lstr = TimeTested.ToLocalTime().ToString() + strHead + "\r\n";
                for (int i = 0; i < len; i++)
                {
                    lstr += i.ToString() + "\t" + result5.X[i].ToString("F2") + "\t" + result5.Y[i].ToString("F2") + "\t" + result5.Z[i].ToString("F2") + "\t" + result5.TX[i].ToString("F2") + "\t" + result5.TY[i].ToString("F2") + "\t" + result5.TZ[i].ToString("F2") + "\r\n";
                }
            }
            else
            {
                sSaveResult3 result3 = F_Main.ReadsSaveResult3(sFileName);
                int len = result3.frameCount;
                DateTimeOffset ltime = DateTimeOffset.FromUnixTimeSeconds(result3.sTime);
                DateTime TimeTested = ltime.DateTime;
                lstr = TimeTested.ToLocalTime().ToString() + strHead + "\r\n";
                for (int i = 0; i < len; i++)
                {
                    lstr += i.ToString() + "\t" + result3.X[i].ToString("F2") + "\t" + result3.Y[i].ToString("F2") + "\t" + result3.Z[i].ToString("F2") + "\t" + result3.TX[i].ToString("F2") + "\t" + result3.TY[i].ToString("F2") + "\t" + result3.TZ[i].ToString("F2") + "\r\n";
                }
            }
            return lstr;
        }
    }
}
