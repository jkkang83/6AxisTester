//using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace FZ4P
{
    public partial class F_Manage : Form
    {
        public Condition Condition { get { return STATIC.Rcp.Condition; } }
        public Spec Spec { get { return STATIC.Rcp.Spec; } }
        public Option Option { get { return STATIC.Rcp.Option; } }
        public Model Model { get { return STATIC.Rcp.Model; } }
        public CurrentPath Current { get { return STATIC.Rcp.Current; } }
        public Process Process { get { return STATIC.Process; } }

        public static int bestpos;

        public F_Manage()
        {
            InitializeComponent();
        }
        private void F_Manage_Load(object sender, EventArgs e)
        {
            RunProgress.SizeMode = PictureBoxSizeMode.StretchImage;
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            RunProgress.BackColor = Color.Transparent;
            RunProgress.Hide();

            InitYield();

            if (Process != null)
            {
                Process.RunStart += Process_RunStart;
                Process.RunEnd += Process_RunEnd;
            }

            Process.Dln.SwitchOn += DriverIC_SwitchOn;
            Model.Changed += Model_Changed;
            BindingUI();

            TestStartBtn.Visible = Process.IsVirtual;
            TestStopBtn.Visible = Process.IsVirtual;
            TestCountText.Visible = Process.IsVirtual;

        }

        private void Process_RunEnd(object sender, int e)
        {
            SafeControlView(RunProgress, false);
            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    SetInforView(0);
                });
            }
            else
            {
                SetInforView(0);
            }
            SafeControlView(Process.InfoBtn[0].btn, true);
            if (Process.ChannelCnt > 1) SafeControlView(Process.InfoBtn[1].btn, true);

            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    //CurrentRunCnt.Text = Process.CurrentRun.ToString();
                    LastSampleNum.Text = Spec.LastSampleNum.ToString();
                    NewSampleNumber.Text = (Spec.LastSampleNum + 1).ToString();

                    SafeInitYield();

                    string dateDir = STATIC.CreateDateDir();
                    dateDir += "LogData\\";
                    if (!Directory.Exists(dateDir))
                        Directory.CreateDirectory(dateDir);
                    for (int j = 0; j < Process.ChannelCnt; j++)
                    {
                        List<string> arry = new List<string>();
                        arry.Add(Process.ViewLog[j].box.Text);

                        string path = string.Format("{0}{1}_Ch{2}.txt", dateDir, DateTime.Now.ToString("yyMMddhhmmss"), j);

                        if (path != "") STATIC.SetTextLine(path, arry);
                    }
                });
            }
        }

        private void Process_RunStart(object sender, int e)
        {
            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    if (RepeatRunCnt.Text == "") RepeatRunCnt.Text = "1";
                    CurrentRunCnt.Text = Process.CurrentRun.ToString();
                    RepeatRunCnt.Text = Process.RepeatRun.ToString();
                });
            }
            else
            {
                if (RepeatRunCnt.Text == "") RepeatRunCnt.Text = "1";
                CurrentRunCnt.Text = Process.CurrentRun.ToString();
                RepeatRunCnt.Text = Process.RepeatRun.ToString();
            }

            SafeControlView(Process.InfoBtn[0].btn, false);
            if (Process.ChannelCnt > 1) SafeControlView(Process.InfoBtn[1].btn, false);
            Process.ShowDataResultsInit(0);
            if (Process.ChannelCnt > 1) Process.ShowDataResultsInit(1);

            SafeControlView(RunProgress, true);
        }

        private void BindingUI()
        {
            for (int i = 0; i < Process.ViewLog.Count; i++)
            {
                Process.ViewLog[i].box.Location = new Point(3 + 478 * i, 44);
                Controls.Add(Process.ViewLog[i].box);
            }
            for (int i = 0; i < Process.ChartTop.Count; i++)
            {
                Process.ChartTop[i].C.Location = new Point(3 + 478 * i, 117);
                Controls.Add(Process.ChartTop[i].C);
            }
            for (int i = 0; i < Process.ChartBtm.Count; i++)
            {
                Process.ChartBtm[i].C.Location = new Point(3 + 478 * i, 400);
                Controls.Add(Process.ChartBtm[i].C);
            }
            for (int i = 0; i < Process.InfoBtn.Count; i++)
            {
                Process.InfoBtn[i].btn.Location = new Point(3 + 478 * i, 291);
                Controls.Add(Process.InfoBtn[i].btn);
                Process.InfoBtn[i].btn.BringToFront();
            }
            p_Result.Controls.Add(Process.ResultDataGrid);
            Process.InitResultData();
            //Process.ResultDataGrid.CellMouseDoubleClick += new DataGridViewCellMouseEventHandler(ResultDataGrid_CellMouseDoubleClick);
            for (int i = 0; i < Option.Param.Count; i++)
            {
                int width = 0;
                int hCal = 40 * i;

                Label Chk = new Label
                {
                    Text = Option.Param[i][0].ToString(),
                    Font = new Font("Calibri", 10, FontStyle.Bold),
                    ForeColor = Color.Black,
                    Location = new Point(6 + width, 50 + hCal),
                    Size = new Size(164, 38),
                    TextAlign = ContentAlignment.MiddleCenter,
                    BorderStyle = BorderStyle.FixedSingle,
                };
                if (Convert.ToBoolean(Option.Param[i][1]))
                {
                    Chk.BackColor = Color.Red;
                }
                else Chk.BackColor = Color.Transparent;

                ModelGroup.Controls.Add(Chk);
            }
            //MoveToVision();

        }
        private void ResultDataGrid_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.RowIndex < 0)
            {
                if (Process.ResultDataGrid.Tag.ToString() == "S")
                {
                    p_Result.Location = new Point(479, 400);
                    p_Result.Size = new Size(956, 614);
                    Process.ResultDataGrid.Size = new Size(956, 614);
                    p_Result.BringToFront();
                    Process.ResultDataGrid.Tag = "L";
                }
                else
                {
                    p_Result.Location = new Point(479, 701);
                    p_Result.Size = new Size(956, 312);
                    Process.ResultDataGrid.Size = new Size(956, 330);
                    Process.ResultDataGrid.Tag = "S";
                }
            }
        }
        private void Model_Changed(object sender, EventArgs e)
        {
        }

        private void DriverIC_SwitchOn(object sender, EventArgs e)
        {
            Process.RepeatRun = 1;
            Process.m_StrIndex[0] = textBox1.Text;
            Process.m_StrIndex[1] = textBox2.Text;
            Process.ClearChart();
            foreach (var l in Process.ViewLog) l.Clear();
            Process.RunTest();
        }


        private void SafeInitYield()
        {
            if (InvokeRequired)
            {
                InitYield();
            }
            else
            {
                InitYield();
            }
        }

        private void InitYield()
        {
            LastSampleNum.Text = Spec.LastSampleNum.ToString();
            NewSampleNumber.Text = (Spec.LastSampleNum + 1).ToString();
            List<string> litem = new List<string>();
            List<double> lratio = new List<double>();

            for (int i = 0; i < Spec.Param.Count; i++)
            {
                int Failed = Convert.ToInt32(Spec.Param[i][8]);
                if (Failed > 0)
                {
                    litem.Add(string.Format("{0} {1}", Spec.Param[i][0], Spec.Param[i][1]));
                    lratio.Add(Failed / (double)Spec.TotlaTested);
                }
            }
            double lyield = 100;
            if (Spec.TotlaTested > 0)
            {
                lyield = (1 - Spec.TotlaFailed / (double)Spec.TotlaTested) * 100;
                if (litem.Count > 0) YieldChart.Series[0].Points.DataBindXY(litem, lratio);
                YieldChart.DataManipulator.Sort(PointSortOrder.Descending, YieldChart.Series[0]);
            }
            else
            {
                YieldChart.Series[0].Points.Clear();
            }
            YieldChart.Titles[0].Text = "Yield " + lyield.ToString("F2") + "% \t" + (Spec.TotlaTested - Spec.TotlaFailed).ToString() + " / " + Spec.TotlaTested.ToString();

        }
        private async void RepeatStartTest_Click(object sender, EventArgs e)
        {
            Process.RepeatRun = int.Parse(RepeatRunCnt.Text);
            Process.CurrentRun = 1;

            Process.m_StrIndex[0] = textBox1.Text;
            //Process.m_StrIndex[1] = textBox2.Text;

            Process.ClearChart();
            foreach (var l in Process.ViewLog) l.Clear();

            await Task.Factory.StartNew(() => Process.RunTest());
        }

        private void SaveScreenAction()
        {
            if (Option.ScreenCapture)
            {
                DateTime dtNow = DateTime.Now;   // 현재 날짜, 시간 얻기
                string pngname = "Screen" + "_" + dtNow.ToString("dd_hh_mm_ss") + ".png";
                string sScreenCapturePath = STATIC.BaseDir + "\\Result\\ScreenCapture\\" + pngname;
                string sDir = STATIC.BaseDir + "\\Result\\ScreenCapture";
                Bitmap memoryImage;
                memoryImage = new Bitmap(1906, 1080);
                Size s = new Size(memoryImage.Width, memoryImage.Height);
                Graphics memoryGraphics = Graphics.FromImage(memoryImage);


                if (!Directory.Exists(sDir))
                    Directory.CreateDirectory(sDir);

                Thread.Sleep(300);
                if (!Process.IsVirtual)
                {
                    memoryGraphics.CopyFromScreen(7, 31, 0, 0, s);
                    memoryImage.Save(sScreenCapturePath);
                }
            }
        }

        private void SafeControlView(Control con, bool bShow)
        {
            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    if (bShow) con.Show(); else con.Hide();
                });
            }
            else
            {
                if (bShow) con.Show(); else con.Hide();
            }
        }

        private void SetInforView(int port)
        {
            if (port == 0)
            {
                for (int i = 0; i < Process.ChannelCnt; i++)
                {
                    if (Process.errMsg[i] == "")
                    {
                        Process.InfoBtn[i].btn.Text = "PASS";
                        Process.InfoBtn[i].btn.Font = new Font("Malgun Gothic", 60, FontStyle.Bold);
                        Process.InfoBtn[i].btn.ForeColor = Color.Cyan;
                    }
                    else
                    {
                        Process.InfoBtn[i].btn.Text = Process.errMsg[i];
                        Process.InfoBtn[i].btn.Font = new Font("Malgun Gothic", 24, FontStyle.Bold);
                        Process.InfoBtn[i].btn.ForeColor = Color.OrangeRed;
                    }
                }
            }
            else
            {
                for (int i = 2; i < 2* Process.ChannelCnt; i++)
                {
                    if (Process.errMsg[i] == "")
                    {
                        Process.InfoBtn[i].btn.Text = "PASS";
                        Process.InfoBtn[i].btn.Font = new Font("Malgun Gothic", 60, FontStyle.Bold);
                        Process.InfoBtn[i].btn.ForeColor = Color.Cyan;
                    }
                    else
                    {
                        Process.InfoBtn[i].btn.Text = Process.errMsg[i];
                        Process.InfoBtn[i].btn.Font = new Font("Malgun Gothic", 24, FontStyle.Bold);
                        Process.InfoBtn[i].btn.ForeColor = Color.OrangeRed;
                    }
                }
            }

        }

        private void SetSampleNumber_Click(object sender, EventArgs e)
        {
            int NewNum = Convert.ToInt32(NewSampleNumber.Text);
            if (NewNum > 0)
            {
                Spec.LastSampleNum = NewNum - 1;
                LastSampleNum.Text = Spec.LastSampleNum.ToString();
            }
            else NewSampleNumber.Text = "1";
        }

        private void ToAdmin_Click(object sender, EventArgs e)
        {
            STATIC.State = (int)STATIC.STATE.Main;
        }

        private void ToVision_Click(object sender, EventArgs e)
        {
            STATIC.fVision.mLEDcurrent[0] = Condition.LedCurrentL;
            STATIC.fVision.mLEDcurrent[1] = Condition.LedCurrentR;
            //STATIC.fVision.m_ChannelOn[0] = Process.m_ChannelOn[0];
            //STATIC.fVision.m_ChannelOn[1] = Process.m_ChannelOn[1];
            STATIC.State = (int)STATIC.STATE.Vision;
        }

        private void SaveScreenOperator_Click(object sender, EventArgs e)
        {
            if (Option.ScreenCapture)
            {
                DateTime dtNow = DateTime.Now;   // 현재 날짜, 시간 얻기
                string pngname = "Screen" + "_" + dtNow.ToString("dd_hh_mm_ss") + ".png";
                string sScreenCapturePath = STATIC.BaseDir + "\\Result\\ScreenCapture\\" + pngname;
                string sDir = STATIC.BaseDir + "\\Result\\ScreenCapture";
                Bitmap memoryImage;
                memoryImage = new Bitmap(1906, 1080);
                Size s = new Size(memoryImage.Width, memoryImage.Height);
                Graphics memoryGraphics = Graphics.FromImage(memoryImage);


                if (!Directory.Exists(sDir))
                    Directory.CreateDirectory(sDir);

                Thread.Sleep(300);
                if (!Process.IsVirtual)
                {
                    memoryGraphics.CopyFromScreen(7, 31, 0, 0, s);
                    memoryImage.Save(sScreenCapturePath);
                }
            }
        }

        private void YieldChart_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            DialogResult result = MessageBox.Show("Do you wan to Reset and Save Yield Data?", "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            if (result == DialogResult.OK)
            {
                Spec.TotlaTested = 0;
                Spec.TotlaFailed = 0;
                Spec.TotlaPassed = 0;
                for (int i = 0; i < Spec.Param.Count; i++)
                {
                    Spec.Param[i][8] = 0;
                }
                InitYield();
            }
        }

        private void btnCheckContact_Click(object sender, EventArgs e)
        {
            //int[] centerCode = new int[6];  //  Hall 값이 (2048,2048,2048) 에 가장 가까와지는  code 값
            //                                //  ch1_x, ch1_y1, ch1_y2, ch2_x, ch2_y1, ch2_y2

            ////  centerCode 위치에서 FWD 방향 구동거리와 BWD 방향 구동거리가 같아지는 Code 값
            //int[] epaCodeMin = new int[6];  //  ch1_x, ch1_y1, ch1_y2,     ch2_x, ch2_y1, ch2_y2

            //int[] epaCodeMax = new int[6];  //  ch1_x, ch1_y1, ch1_y2,     ch2_x, ch2_y1, ch2_y2\

            ////  X EPA
            //Task taskFindEPAX = Task.Run(() => { Process.Process_FindEPA("OIS X Scan", ref centerCode, ref epaCodeMin, ref epaCodeMax); });
            //taskFindEPAX.Wait();

            ////  Y EPA
            //Task taskFindEPAY = Task.Run(() => { Process.Process_FindEPA("OIS Y Scan", ref centerCode, ref epaCodeMin, ref epaCodeMax); });
            //taskFindEPAY.Wait();
            //Process.CalList[0][0].CalLinearity(0,330);
        }

        private void SuddenStop_Click(object sender, EventArgs e)
        {
            //Process.SuddenStop = true;
            STATIC.fVision.m__G.oCam[0].GrabB();
        }
        public bool IsTestOn = false;
        private void button2_Click(object sender, EventArgs e)
        {
            IsTestOn = true;
            double count = 0;
            Task.Factory.StartNew(() => {
                while (IsTestOn)
                {
                    Process.LEDs_All_On(0, true);
                    Process.LEDs_All_On(0, false);
                    count += 0.0001;
                    //if (InvokeRequired)
                    //{
                    //    BeginInvoke((MethodInvoker)delegate
                    //    {
                    //        textBox3.Text = count.ToString("F4");
                    //    });
                    //}

                    Thread.Sleep(1);
                }
            });
        }

        private void button3_Click(object sender, EventArgs e)
        {
            IsTestOn = false;
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            bestpos = Convert.ToInt32(tbBestpos.Text);
            STATIC.DrvIC.OISOn(0, "AF", true);
            STATIC.DrvIC.Move(0, "AF", bestpos);
        }

        private void button3_Click_1(object sender, EventArgs e)
        {

            STATIC.DrvIC.OISOn(0, "AF", true);
            STATIC.DrvIC.Move(0, "AF", bestpos);

            STATIC.DrvIC.Move(0, "X", 2048);
            STATIC.DrvIC.Move(0, "Y", 2048);
            STATIC.DrvIC.OISOn(0, "X", true);
            STATIC.DrvIC.OISOn(0, "Y", true);
        }
        public byte[] sDataBuff = null;
        public int RunNum = 1;
        public string CheckResultFolder()
        {
            DateTime dt = DateTime.Now;
            string resDirectory = STATIC.BaseDir + dt.Year + "\\" + dt.Month + "\\" + dt.Day + "\\";
            if (!Directory.Exists(resDirectory))
                Directory.CreateDirectory(resDirectory);
            return resDirectory;
        }
        public string GetLotName()
        {

            //return tbLotName.Text;
            return "";
        }
        public void SettbUncalibratedInfoVisible(bool visible)
        {
            //tbUncalibratedInfo.BeginInvoke(new Action(() => { tbUncalibratedInfo.Visible = visible; }));
        }
    }
}
