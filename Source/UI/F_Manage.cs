//using OpenCvSharp;
using Dln;
using FZ4P.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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

        public string ProgramVer = string.Empty; 
   

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



            if (Model.MCType == "Master") lbMCtype.Text = "PORT 1";
            else if (Model.MCType == "Slave") { lbMCtype.Text = "PORT 2"; }
            else if (Model.MCType == "Handler") { lbMCtype.Text = $"Port {AppHelper.GetEthernetIPv4()} (Handler)"; }
            else lbMCtype.Text = "Normal";

            if (STATIC.Rcp.tt.Count == 0) lbST.Text = $"0.0 sec";
            else lbST.Text = $"{(STATIC.Rcp.tt.St / STATIC.Rcp.tt.Count).ToString("F1")} sec";
            lbCurrentST.Text = $"{(STATIC.Rcp.tt.CurrentST).ToString("F1")} sec";

        }
        public void SetConStatus(bool isCon)
        {
            if (isCon) 
            {
                if (InvokeRequired)
                {
                    BeginInvoke((MethodInvoker)delegate
                    {
                        lbMcConstatus.BackColor = Color.YellowGreen;
                    });
                }
                else
                    lbMcConstatus.BackColor = Color.YellowGreen;
            }
            else
            {
                if (InvokeRequired)
                {
                    BeginInvoke((MethodInvoker)delegate
                    {
                        lbMcConstatus.BackColor = Color.Red;
                    });
                }
                else
                    lbMcConstatus.BackColor = Color.Red;
            }
        }
        private void Process_RunEnd(object sender, int inspType)
        {

            SafeControlView(RunProgress, false);
            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    //CurrentRunCnt.Text = Process.CurrentRun.ToString();
                    LastSampleNum.Text = STATIC.Rcp.yield.LastSampleNum.ToString();
                    NewSampleNumber.Text = (STATIC.Rcp.yield.LastSampleNum + 1).ToString();

                    SafeInitYield();

                
                });
            }
            if (Model.MCType == "Slave")
            {
                string s = Process.errMsg[0] == "" ? "PASS" : Process.errMsg[0];
                STATIC.TcpConn.SendMessage($"Res.{s}");

            }
            if (inspType == 2)
            {
                if (Model.MCType == "Handler")
                {
                    if(Option.DryRunMode)
                    {
                        Random rnd = new Random();
                        int val = rnd.Next(100);

                        string res = string.Empty;
                        if (val < 20)
                            res = "NG";
                        else res = "OK";
                        STATIC.TcpConn.SendMessage(res);
                        Process.errMsg[0] = res == "OK" ? "" : $"NG,{1.ToString("D3")},";
                    }
                    else
                    {
                        string s = Process.errMsg[0] == "" ? "OK" : $"NG,{Process.PassFails[0].FirstFailIndex.ToString("D3")}";
                        if (Process.errMsg[0] != "" && Process.PassFails[0].FirstFailIndex == 0)
                            s = $"NG,{50.ToString("D3")}";
                        STATIC.TcpConn.SendMessage(s);
                    }
                }
            }

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
                    lbCurrentST.Text = $"{(STATIC.Rcp.tt.CurrentST).ToString("F1")} sec";
                    if (STATIC.Rcp.tt.Count == 0) lbST.Text = $"0.0 sec";
                    else lbST.Text = $"{(STATIC.Rcp.tt.St / STATIC.Rcp.tt.Count).ToString("F1")} sec";
                });
            }
            else
            {
                lbCurrentST.Text = $"{(STATIC.Rcp.tt.CurrentST).ToString("F1")} sec";
                if (STATIC.Rcp.tt.Count == 0) lbST.Text = $"0.0 sec";
                else lbST.Text = $"{(STATIC.Rcp.tt.St / STATIC.Rcp.tt.Count).ToString("F1")} sec";
            }
            DataIO.SerializeToXMLFile(STATIC.Rcp.tt, STATIC.appPath.TestTimeDir);

        }

        private void Process_RunStart(object sender, int e)
        {

            for (int i = 0; i < Process.ItemList.Count; i++)
            {
                Process.ItemList[i].Time = "";
            }
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
           

            SafeControlView(RunProgress, true);
            if (Model.MCType == "Slave")
                STATIC.TcpConn.SendMessage("Clear");

        }

        private void BindingUI()
        {

            int infoCnt = 1;
            if(STATIC.Rcp.Model.MCType == "Master")
            {
                infoCnt = 2;
            }

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

            for (int i = 0; i < Process.tiltChart.Count; i++)
            {
             
                Process.tiltChart[i].Size = new Size(475, 475);
                Process.tiltChart[i].Location = new Point(3 + 478 * (i + 1), 117);
                Controls.Add(Process.tiltChart[i]);
            }
            for (int i = 0; i < infoCnt; i++)
            {
                Process.InfoBtn[i].btn.Location = new Point(3 + 478 * i, 291);
                Controls.Add(Process.InfoBtn[i].btn);
                Process.InfoBtn[i].btn.BringToFront();
            }
            //for (int i = 0; i < Process.InfoBtn.Count; i++)
            //{
            //    Process.InfoBtn[i].btn.Location = new Point(3 + 478 * i, 291);
            //    Controls.Add(Process.InfoBtn[i].btn);
            //    Process.InfoBtn[i].btn.BringToFront();
            //}

            Process.ResultDataGrid.Dock = DockStyle.Fill;
            p_Result.Controls.Add(Process.ResultDataGrid);

            Process.lblFailList.AutoSize = false;
            Process.lblFailList.BackColor = Color.White;
            Process.lblFailList.ForeColor = Color.Red;
            Process.lblFailList.Dock = DockStyle.Fill;
            Process.lblFailList.Font = new Font("Arial", 14, FontStyle.Bold);
            Process.lblFailList.TextAlign = ContentAlignment.MiddleCenter;
            pResult2.Controls.Add(Process.lblFailList);

            Process.InitResultData();
            //Process.ResultDataGrid.CellMouseDoubleClick += new DataGridViewCellMouseEventHandler(ResultDataGrid_CellMouseDoubleClick);

            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(Option);

            for (int i = 0; i < props.Count; i++)
            {
                int width = 0;
                int hCal = 40 * i;

                Label Chk = new Label
                {
                    Text = DataIO.GetCustomAttribute<OptionAttribute>(props[i])?.DisplayName,
                    Font = new Font("Calibri", 10, FontStyle.Bold),
                    ForeColor = Color.Black,
                    Location = new Point(6 + width, 50 + hCal),
                    Size = new Size(164, 38),
                    TextAlign = ContentAlignment.MiddleCenter,
                    BorderStyle = BorderStyle.FixedSingle,
                };
                if (Convert.ToBoolean(props[i].GetValue(Option)))
                {
                    Chk.BackColor = Color.Red;
                }
                else Chk.BackColor = Color.Transparent;

                ModelGroup.Controls.Add(Chk);
            }
            lblCrecipe.Text = Current.ConditionName;
            lblCspec.Text = Current.SpecName;
           
        }
        public void BindingUIModel(string PGVer)
        {
            ModelGroup.Controls.Clear();
            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(Option);

            for (int i = 0; i < props.Count; i++)
            {
                int width = 0;
                int hCal = 40 * i;

                Label Chk = new Label
                {
                    Text = DataIO.GetCustomAttribute<OptionAttribute>(props[i])?.DisplayName,
                    Font = new Font("Calibri", 10, FontStyle.Bold),
                    ForeColor = Color.Black,
                    Location = new Point(6 + width, hCal),
                    Size = new Size(164, 38),
                    TextAlign = ContentAlignment.MiddleCenter,
                    BorderStyle = BorderStyle.FixedSingle,
                };
                if (Convert.ToBoolean(props[i].GetValue(Option)))
                {
                    Chk.BackColor = Color.YellowGreen;
                }
                else Chk.BackColor = Color.Transparent;

                ModelGroup.Controls.Add(Chk);
            }
            lblCrecipe.Text = Current.ConditionName;
            lblCspec.Text = Current.SpecName;
            lblPGMver.Text = PGVer;
            lblMCnum.Text = STATIC.Rcp.Model.TesterNo; 
        }
        
        private void Model_Changed(object sender, EventArgs e)
        {
        }

        private void DriverIC_SwitchOn(object sender, EventArgs e)
        {
            if (STATIC.Dln.IsRun) return;
            Process.RepeatRun = 1;
           // Process.m_StrIndex[0] = textBox1.Text;
         //   Process.m_StrIndex[1] = textBox2.Text;

            Process.ClearChart();
            foreach (var l in Process.ViewLog) l.Clear();
            Process.RunTest(1);
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
            LastSampleNum.Text = STATIC.Rcp.yield.LastSampleNum.ToString();
            NewSampleNumber.Text = (STATIC.Rcp.yield.LastSampleNum + 1).ToString();
            List<string> litem = new List<string>();
            List<double> lratio = new List<double>();

            for (int i = 0; i < Spec.specList.Count; i++)
            {
                int Failed = Convert.ToInt32(Spec.specList[i].FailCnt);
                if (Failed > 0)
                {
                    litem.Add(string.Format("{0}", Spec.specList[i].DisplayName));
                    lratio.Add(Failed / (double)STATIC.Rcp.yield.TotlaTested);
                }
            }
            double lyield = 100;
            if (STATIC.Rcp.yield.TotlaTested > 0)
            {
                lyield = (1 - STATIC.Rcp.yield.TotlaFailed / (double)STATIC.Rcp.yield.TotlaTested) * 100;
                if (litem.Count > 0) YieldChart.Series[0].Points.DataBindXY(litem, lratio);
                YieldChart.DataManipulator.Sort(PointSortOrder.Descending, YieldChart.Series[0]);
            }
            else
            {
                YieldChart.Series[0].Points.Clear();
            }
            YieldChart.Titles[0].Text = "Yield " + lyield.ToString("F2") + "% \t" + (STATIC.Rcp.yield.TotlaTested - STATIC.Rcp.yield.TotlaFailed).ToString() + " / " + STATIC.Rcp.yield.TotlaTested.ToString();

        }
        private async void RepeatStartTest_Click(object sender, EventArgs e)
        {
            if (STATIC.Dln.IsRun) return;
            Process.RepeatRun = int.Parse(RepeatRunCnt.Text);
            Process.CurrentRun = 1;

     
            Process.ClearChart();
            foreach (var l in Process.ViewLog) l.Clear();

            await Task.Factory.StartNew(() => Process.RunTest(0));
        }

        private void SaveScreenAction()
        {
            if (Option.ScreenCapture)
            {
                DateTime dtNow = DateTime.Now;   // 현재 날짜, 시간 얻기
                string pngname = "Screen" + "_" + dtNow.ToString("dd_hh_mm_ss") + ".png";
                string sScreenCapturePath = STATIC.appPath.BaseDir + "\\Result\\ScreenCapture\\" + pngname;
                string sDir = STATIC.appPath.BaseDir + "\\Result\\ScreenCapture";
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

        public void SetInforViewOnComm(string msg)
        {
            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    if (msg == "PASS" || msg == "pass")
                    {
                        Process.InfoBtn[1].btn.Text = msg;
                        Process.InfoBtn[1].btn.Font = new Font("Malgun Gothic", 60, FontStyle.Bold);
                        Process.InfoBtn[1].btn.ForeColor = Color.Cyan;
                    }
                    else
                    {
                        Process.InfoBtn[1].btn.Text = msg;
                        Process.InfoBtn[1].btn.Font = new Font("Malgun Gothic", 24, FontStyle.Bold);
                        Process.InfoBtn[1].btn.ForeColor = Color.OrangeRed;
                    }
                    Process.InfoBtn[1].btn.Show();
                });
            }
            else
            {
                if (msg == "PASS" || msg == "pass")
                {
                    Process.InfoBtn[1].btn.Text = msg;
                    Process.InfoBtn[1].btn.Font = new Font("Malgun Gothic", 60, FontStyle.Bold);
                    Process.InfoBtn[1].btn.ForeColor = Color.Cyan;

                }
                else
                {
                    Process.InfoBtn[1].btn.Text = msg;
                    Process.InfoBtn[1].btn.Font = new Font("Malgun Gothic", 24, FontStyle.Bold);
                    Process.InfoBtn[1].btn.ForeColor = Color.OrangeRed;
                    
                }
                Process.InfoBtn[1].btn.Show();
            }

        }
        public void SafeControlViewOnComm()
        {
            if (InvokeRequired)
            {
                BeginInvoke((MethodInvoker)delegate
                {
                    Process.InfoBtn[1].btn.Hide();
                });
            }
            else
            {
                Process.InfoBtn[1].btn.Hide();
            }
        }

        private void SetSampleNumber_Click(object sender, EventArgs e)
        {
            int NewNum = Convert.ToInt32(NewSampleNumber.Text);
            if (NewNum > 0)
            {
                STATIC.Rcp.yield.LastSampleNum = NewNum - 1;
                LastSampleNum.Text = STATIC.Rcp.yield.LastSampleNum.ToString();
            }
            else NewSampleNumber.Text = "1";
            STATIC.Rcp.tt.St = 0;
            STATIC.Rcp.tt.Count = 0;
            STATIC.Rcp.tt.CurrentST = 0;
            DataIO.SerializeToXMLFile(STATIC.Rcp.tt, STATIC.appPath.TestTimeDir);
            lbST.Text = "0.0 sec";
            lbCurrentST.Text = "0.0 sec";
        }

        private void ToAdmin_Click(object sender, EventArgs e)
        {
            STATIC.State = (int)STATE.Main;
        }

        private void ToVision_Click(object sender, EventArgs e)
        {
            STATIC.FVision.mLEDcurrent[0] = STATIC.Rcp.vsFile.LEDCurrentL;
            STATIC.FVision.mLEDcurrent[1] = STATIC.Rcp.vsFile.LEDCurrentR;
            //STATIC.fVision.m_ChannelOn[0] = Process.m_ChannelOn[0];
            //STATIC.fVision.m_ChannelOn[1] = Process.m_ChannelOn[1];
            STATIC.State = (int)STATE.Vision;
        }

        private void SaveScreenOperator_Click(object sender, EventArgs e)
        {
            if (Option.ScreenCapture)
            {
                DateTime dtNow = DateTime.Now;   // 현재 날짜, 시간 얻기
                string pngname = "Screen" + "_" + dtNow.ToString("dd_hh_mm_ss") + ".png";
                string sScreenCapturePath = STATIC.appPath.BaseDir + "\\Result\\ScreenCapture\\" + pngname;
                string sDir = STATIC.appPath.BaseDir + "\\Result\\ScreenCapture";
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
                STATIC.Rcp.yield.TotlaTested = 0;
                STATIC.Rcp.yield.TotlaFailed = 0;
                STATIC.Rcp.yield.TotlaPassed = 0;
                for (int i = 0; i < Spec.specList.Count; i++)
                {
                    Spec.specList[i].FailCnt = 0;
                }
                InitYield();
                DataIO.SerializeToXMLFile(Spec, STATIC.appPath.SpecDir + Current.SpecName);
            }
        }

        private void btnCheckContact_Click(object sender, EventArgs e)
        {
            bool Constate = true;
           
            if (!Process.Dln.WriteArray(0, Process.DrvIC.AFOriginAddr, 0x02, new byte[] { 0x40 }) && !Process.Dln.WriteArray(0, Process.DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x40 })) Constate = false;
            if (!Process.Dln.WriteArray(0, Process.DrvIC.XOriginAddr, 0x02, new byte[] { 0x40 }) && !Process.Dln.WriteArray(0, Process.DrvIC.XSlaveAddr, 0x02, new byte[] { 0x40 })) Constate = false;
            if (!Process.Dln.WriteArray(0, Process.DrvIC.Y1OriginAddr, 0x02, new byte[] { 0x40 }) && !Process.Dln.WriteArray(0, Process.DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x40 })) Constate = false;
            if (Process.DrvIC.Y2SlaveAddr != 0x00)
            {
                if (!Process.Dln.WriteArray(0, Process.DrvIC.Y2OriginAddr, 0x02, new byte[] { 0x40 })
                    && !Process.Dln.WriteArray(0, Process.DrvIC.Y2SlaveAddr, 0x02, new byte[] { 0x40 })) Constate = false;
            }
            if (Constate) Process.AddLog(0, "Contact Pass");
            else Process.AddLog(0, "Contact Fail");
        }

        private void SuddenStop_Click(object sender, EventArgs e)
        {
            Process.SuddenStop = true;
        
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

     
   
        public byte[] sDataBuff = null;
        public int RunNum = 1;
        public string CheckResultFolder()
        {
            DateTime dt = DateTime.Now;
            string resDirectory = STATIC.appPath.BaseDir + dt.Year + "\\" + dt.Month + "\\" + dt.Day + "\\";
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
