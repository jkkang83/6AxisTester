using OpenCvSharp;
using OpenCvSharp.Dnn;
using OpenCvSharp.Flann;
using OpenCvSharp.XImgProc;
using S2System.Vision;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace FZ4P
{
    public class Process
    {
        public DLN Dln { get { return STATIC.Dln; } }
        public AK73XX DrvIC { get { return STATIC.DrvIC; } }
        public Recipe Rcp { get { return STATIC.Rcp; } }
        public Condition Condition { get { return STATIC.Rcp.Condition; } }
        public Spec Spec { get { return STATIC.Rcp.Spec; } }
        public Option Option { get { return STATIC.Rcp.Option; } }
        public Model Model { get { return STATIC.Rcp.Model; } }
        public CurrentPath Current { get { return STATIC.Rcp.Current; } }
        public List<PassFail> PassFails { get { return STATIC.Rcp.PassFails; } }
        public TotalYield yield { get { return STATIC.Rcp.yield; } }

        Global m__G = null; 


        public ObservableCollection<ActItems> ItemList = new ObservableCollection<ActItems>();
        public List<NVMHallParam> HallParam = new List<NVMHallParam>();
        public List<Task> RunTasks = new List<Task>();
        public int RunTaskId1 = 0;
        public int RunTaskId2 = 0;

        public bool m_bAllLEDOn = false;
        public bool IsVirtual = false;
        public bool SuddenStop = false;
        public int RepeatRun = 0;
        public int CurrentRun = 0;
        public bool IsHallComplete = false;
        public int PortCnt { get; set; }
        public int ChannelCnt { get; set; }

        public List<bool> IsRun = new List<bool>();
        public List<string> errMsg = new List<string>();
        public List<bool> m_ChannelOn = new List<bool>();
        public List<string> m_StrIndex = new List<string>();
        public List<bool> IsScan = new List<bool>();
        public List<int> framCnt = new List<int>();

        public List<byte[]> FWCode = new List<byte[]>();

        public event EventHandler<int> RunStart = null;
        public event EventHandler<int> RunEnd = null;

        public List<LogText> ViewLog = new List<LogText>();

        public List<InfoButton> InfoBtn = new List<InfoButton>();

        public List<DrvParam> DrvValue = new List<DrvParam>();

        public List<List<CalResult>> CalList = new List<List<CalResult>>();

        public DataGridView ResultDataGrid = new DataGridView()
        { Size = new System.Drawing.Size(780, 828) };

        public List<ChartList> ChartTop = new List<ChartList>();

        public List<ChartList> ChartBtm = new List<ChartList>();
        public int BestAFPos = 2048;
        public int OISCenter = 2048;
        public int AFCenter = 2048;
        double SlopeX = 0;
        double SlopeY = 0;
        public Process()
        {
            PortCnt = 1;
            ChannelCnt = 1;

            for (int i = 0; i < PortCnt; i++)
            {
                IsRun.Add(false);
                IsScan.Add(false);
                framCnt.Add(0);
            }
            for (int i = 0; i < ChannelCnt; i++)
            {
                errMsg.Add("");
                m_ChannelOn.Add(false);
                m_StrIndex.Add("");
                HallParam.Add(new NVMHallParam());
                DrvValue.Add(new DrvParam());

                CalList.Add(new List<CalResult>());
                CalList[i].Add(new CalResult("AF Scan"));              
                CalList[i].Add(new CalResult("AF Settling"));
                CalList[i].Add(new CalResult("OIS X Scan"));              
                CalList[i].Add(new CalResult("OIS Y Scan"));


                ChartTop.Add(new ChartList("Stroke", i));
                ChartBtm.Add(new ChartList("Tilt", i));

                InfoBtn.Add(new InfoButton()); //test
                InfoBtn.Add(new InfoButton());
                ViewLog.Add(new LogText());
            }

            ItemList.Add(new ActItems() { Name = "AF OpenLoopAging", Func = Act_AFOpenLoopAging, IsMulti = true });
            ItemList.Add(new ActItems() { Name = "AF Initial", Func = Act_AFInit, IsMulti = true });
            ItemList.Add(new ActItems() { Name = "AF EPA", Func = Act_AFEPA, IsMulti = true });
            ItemList.Add(new ActItems() { Name = "AF Linearity Comp", Func = Act_AFLinComp, IsMulti = true });
            ItemList.Add(new ActItems() { Name = "AF Scan", Func = Act_ScanCode });
            ItemList.Add(new ActItems() { Name = "Find AF Best Position", Func = Act_FindBestAFPosition });
            ItemList.Add(new ActItems() { Name = "OIS Init", Func = Act_OISInit });
            ItemList.Add(new ActItems() { Name = "OIS EPA", Func = Act_OISEPA, IsMulti = true });
            ItemList.Add(new ActItems() { Name = "Close Loop Aging", Func = Act_CloseLoopAging });
            ItemList.Add(new ActItems() { Name = "OIS X LinComp", Func = Act_OISLinComp });
            ItemList.Add(new ActItems() { Name = "OIS Y LinComp", Func = Act_OISLinComp });
            ItemList.Add(new ActItems() { Name = "Servo Decenter", Func = ServoDecenter, IsMulti = true });
            ItemList.Add(new ActItems() { Name = "OIS X Scan", Func = Act_ScanCode });
            ItemList.Add(new ActItems() { Name = "OIS Y Scan", Func = Act_ScanCode });
            ItemList.Add(new ActItems() { Name = "Gain@10Hz", Func = Act_GaindB10Hz, IsMulti = true });
            ItemList.Add(new ActItems() { Name = "Phase Margin", Func = Act_Phase_Margin, IsMulti = true });
      //      ItemList.Add(new ActItems() { Name = "Gain Margin", Func = Act_Gain_Margin, IsMulti = true });
            ItemList.Add(new ActItems() { Name = "AF Settling", Func = Act_ScanTimeCode });
            ItemList.Add(new ActItems() { Name = "AF ScanAging", Func = Act_AFScanAging });
            ItemList.Add(new ActItems() { Name = "AF PreDriving", Func = Act_PreAFDriving });
            ItemList.Add(new ActItems() { Name = "OIS Shift", Func = Act_OISShift, IsMulti = true });

            m__G = Global.GetInstance();
        }


        #region Default
        public void ShowDataResults(int ch, string key, int start, int end)
        {
            if (ResultDataGrid.InvokeRequired)
            {
                ResultDataGrid.BeginInvoke((MethodInvoker)delegate
                {
                    for (int i = start; i <= end; i++)
                    {
                        if (Spec.specList[i].Category != key) continue;
                        if (PassFails[ch].Results[i].Val != 0)
                        {
                            if (key.Contains("FRA") || key.Contains("Gyro"))
                            {
                                ResultDataGrid[ch + 4, i].Value = PassFails[ch].Results[i].Val.ToString("F1");
                            }
                            else
                            {
                                ResultDataGrid[ch + 4, i].Value = PassFails[ch].Results[i].Val.ToString("F3");
                            }
                        }
                        if (PassFails[ch].Results[i].bPass) ResultDataGrid[ch + 4, i].Style.BackColor = Color.White;
                        else ResultDataGrid[ch + 4, i].Style.BackColor = Color.Orange;

                    }

                });
            }
            else
            {
                for (int i = start; i <= end; i++)
                {
                    if (Spec.specList[i].Category != key) continue;
                    if (PassFails[ch].Results[i].Val != 0)
                    {
                        if (key.Contains("FRA") || key.Contains("Gyro"))
                        {
                            ResultDataGrid[ch + 4, i].Value = PassFails[ch].Results[i].Val.ToString("F1");
                        }
                        else
                        {
                            ResultDataGrid[ch + 4, i].Value = PassFails[ch].Results[i].Val.ToString("F3");
                        }
                    }
                    if (PassFails[ch].Results[i].bPass) ResultDataGrid[ch + 4, i].Style.BackColor = Color.White;
                    else ResultDataGrid[ch + 4, i].Style.BackColor = Color.Orange;

                }
            }
        }
        public void InitResultData()
        {
            Type dgvType = ResultDataGrid.GetType();
            PropertyInfo pi = dgvType.GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
            pi.SetValue(ResultDataGrid, true, null);

            ResultDataGrid.AllowUserToAddRows = false;
            ResultDataGrid.AllowUserToDeleteRows = false;
            ResultDataGrid.AllowUserToResizeColumns = false;
            ResultDataGrid.AllowUserToResizeRows = false;
            ResultDataGrid.Tag = "S";
            ResultDataGrid.ColumnCount = 7; //  Group, Item, min, max, r0, r1, r2, r3, unit, Fratio
            ResultDataGrid.Font = new Font("Calibri", 14, FontStyle.Bold);
            for (int i = 0; i < ResultDataGrid.ColumnCount; i++)
            {
                ResultDataGrid.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
            }
            ResultDataGrid.RowHeadersVisible = false;
            ResultDataGrid.BackgroundColor = Color.LightGray;

            //// Column
            ResultDataGrid.Columns[0].Name = "Axis";
            ResultDataGrid.Columns[1].Name = "Items";
            ResultDataGrid.Columns[2].Name = "Min";
            ResultDataGrid.Columns[3].Name = "Max";
            ResultDataGrid.Columns[4].Name = "#1 Result";
            ResultDataGrid.Columns[5].Name = "#2 Result";
            ResultDataGrid.Columns[6].Name = "unit";

            ResultDataGrid.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopCenter;
            ResultDataGrid.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopLeft;
            ResultDataGrid.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopRight;
            ResultDataGrid.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopRight;
            ResultDataGrid.Columns[4].DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopRight;
            ResultDataGrid.Columns[5].DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopRight;
            ResultDataGrid.Columns[6].DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopLeft;

            ResultDataGrid.Columns[0].Width = 160;
            ResultDataGrid.Columns[1].Width = 215;
            ResultDataGrid.Columns[2].Width = 70;
            ResultDataGrid.Columns[3].Width = 70;
            ResultDataGrid.Columns[4].Width = 90;
            ResultDataGrid.Columns[5].Width = 90;
            ResultDataGrid.Columns[6].Width = 65;

            ResultDataGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            ResultDataGrid.ColumnHeadersHeight = 28;

            bool bColorChange = true;
            ResultDataGrid.Rows.Clear();
            for (int i = 0; i < Spec.specList.Count; i++)
            {
                ResultDataGrid.Rows.Add(Spec.specList[i].Category, Spec.specList[i].DisplayName, Spec.specList[i].MinSpec, Spec.specList[i].MaxSpec, 0, 0, Spec.specList[i].Unit);
                ResultDataGrid.Rows[i].Visible = Convert.ToBoolean(Spec.specList[i].OnOff);

                if (bColorChange) for (int k = 0; k < ResultDataGrid.ColumnCount; k++) ResultDataGrid[k, i].Style.BackColor = Color.Lavender;
                else for (int k = 0; k < ResultDataGrid.ColumnCount; k++) ResultDataGrid[k, i].Style.BackColor = Color.White;



                ResultDataGrid.Rows[i].Height = 22;
                ResultDataGrid.Rows[i].Resizable = DataGridViewTriState.False;
                ResultDataGrid.Rows[i].DefaultCellStyle.Font = new Font("Calibri", 10, FontStyle.Bold);
                ResultDataGrid[1, i].Style.Font = new Font("Calibri", 10, FontStyle.Bold);
                ResultDataGrid[2, i].Style.Font = new Font("Calibri", 10, FontStyle.Bold);
                ResultDataGrid[6, i].Style.Font = new Font("Calibri", 10, FontStyle.Italic);

                ResultDataGrid.ReadOnly = true;
            }

            string old = string.Empty;/*ResultGrid.Rows[0].Cells[0].Value.ToString();*/
            for (int i = 0; i < Spec.specList.Count; i++)
            {
                if (ResultDataGrid.Rows[i].Visible)
                {
                    string newKey = ResultDataGrid.Rows[i].Cells[0].Value.ToString();

                    if (old != newKey)
                        bColorChange = !bColorChange;
                    if (bColorChange) for (int k = 0; k < ResultDataGrid.ColumnCount; k++) ResultDataGrid[k, i].Style.BackColor = Color.Lavender;
                    else for (int k = 0; k < ResultDataGrid.ColumnCount; k++) ResultDataGrid[k, i].Style.BackColor = Color.White;

                    if (old == newKey)
                        ResultDataGrid.Rows[i].Cells[0].Style.ForeColor = ResultDataGrid.Rows[i].Cells[0].Style.BackColor;
                    old = newKey;
                }
            }
        }
        public void InitResult(int ch)
        {
            PassFails[ch].TotalFail = "";
            PassFails[ch].FirstFail = "";
            PassFails[ch].FirstFailIndex = 0;
            for (int i = 0; i < (int)SpecItem.Length; i++)
            {
                PassFails[ch].Results[i].Val = 0;
                PassFails[ch].Results[i].msg = ""; PassFails[ch].Results[i].bPass = true;
            }
        }
        public void SetResult(int ch, int start, int end)
        {
            for (int i = start; i < end + 1; i++)
            {
                if (!Spec.specList[i].OnOff) continue;

                double lmin, lmax;
                lmin = Convert.ToDouble(Spec.specList[i].MinSpec);
                lmax = Convert.ToDouble(Spec.specList[i].MaxSpec);

                if (PassFails[ch].Results[i].Val < lmin || PassFails[ch].Results[i].Val > lmax || double.IsNaN(PassFails[ch].Results[i].Val))
                {
                    PassFails[ch].Results[i].msg = Spec.specList[i].Category + "_" + Spec.specList[i].DisplayName;
                    PassFails[ch].Results[i].bPass = false;
                    PassFails[ch].TotalFail += string.Format("{0}'", i + 1);
                }
                else
                {
                    PassFails[ch].Results[i].msg = "";
                    PassFails[ch].Results[i].bPass = true;

                }
            }
            for (int i = start; i < end + 1; i++)
            {
                if (!PassFails[ch].Results[i].bPass)
                {
                    if (PassFails[ch].FirstFailIndex == 0)
                    {
                        PassFails[ch].FirstFailIndex = (i + 1);
                        PassFails[ch].FirstFail = PassFails[ch].Results[i].msg;
                    }

                    int failCnt = Convert.ToInt32(Spec.specList[i].FailCnt); failCnt++;
                    Spec.specList[i].FailCnt = failCnt;
                }
            }
        }
        public void ShowDataResultsInit(int ch)
        {
            if (ResultDataGrid.InvokeRequired)
            {
                ResultDataGrid.BeginInvoke((MethodInvoker)delegate
                {
                    InitResult(ch);
                    for (int i = 0; i < Spec.specList.Count; i++)
                    {
                        ResultDataGrid[ch + 4, i].Value = PassFails[ch].Results[i].Val.ToString("F0");
                        ResultDataGrid[ch + 4, i].Style.BackColor = Color.White;
                    }
                });
            }
            else
            {
                InitResult(ch);
                for (int i = 0; i < Spec.specList.Count; i++)
                {
                    ResultDataGrid[ch + 4, i].Value = PassFails[ch].Results[i].Val.ToString("F0");
                    ResultDataGrid[ch + 4, i].Style.BackColor = Color.White;
                }
            }
        }
        public void AddLog(int ch, string msg)
        {
            ViewLog[ch].Log(msg);
        }
        public void AddChart(int ch, string name)
        {
            while (ChartTop[ch].IsFalg)
                Thread.Sleep(10);

            int CodeRange = 0;

            foreach (var Cal in CalList[ch])
            {
                if (Cal.Name == name)
                {
                    switch (name)
                    {
                        case "OIS X Scan":

                            CodeRange = Condition.iXPlotRange;
                            //Stroke
                            if (ChartTop[ch].C.InvokeRequired)
                            {
                                ChartTop[ch].C.BeginInvoke((MethodInvoker)delegate
                                {
                                    ChartTop[ch].C.Series[0].Points.Clear();

                                    for (int i = 2; i < Cal.CodeX.Count; i++)
                                    {
                                        if (Cal.CodeX[i] >= OISCenter - CodeRange && Cal.CodeX[i] <= OISCenter + CodeRange)
                                        {
                                            ChartTop[ch].C.Series[0].Points.AddXY(Cal.CodeX[i], Cal.StrokeX[i]); //  stroke
                                            ChartTop[ch].C.Series[3].Points.AddXY(Cal.CodeX[i], Cal.Current[i]); //  current
                                            ChartTop[ch].C.Series[6].Points.AddXY(Cal.CodeX[i], Cal.HallX[i] / 10); //  hall
                                        }
                                    }
                                });
                            }
                            //Tilt
                            if (ChartBtm[ch].C.InvokeRequired)
                            {
                                ChartBtm[ch].C.BeginInvoke((MethodInvoker)delegate
                                {
                                    for (int i = 2; i < Cal.CodeX.Count; i++)
                                    {
                                        if (Cal.CodeX[i] >= OISCenter - CodeRange && Cal.CodeX[i] <= OISCenter + CodeRange)
                                        {
                                            //ChartBtm[ch].C.Series[0].Points.AddXY(Cal.CodeX[i], Cal.TiltX[i]); //  Tilt 
                                            //ChartBtm[ch].C.Series[1].Points.AddXY(Cal.CodeX[i], Cal.TiltY[i]); //  Tilt 
                                            //ChartBtm[ch].C.Series[2].Points.AddXY(Cal.CodeX[i], Cal.TiltZ[i]); //  Tilt 
                                        }
                                    }
                                });
                            }
                            break;
                        case "OIS Y Scan":

                            CodeRange = Condition.iYPlotRange;
                            //Stroke
                            if (ChartTop[ch].C.InvokeRequired)
                            {
                                ChartTop[ch].C.BeginInvoke((MethodInvoker)delegate
                                {
                                    for (int i = 2; i < Cal.CodeY1.Count; i++)
                                    {
                                        if (Cal.CodeY1[i] >= OISCenter - CodeRange && Cal.CodeY1[i] <= OISCenter + CodeRange)
                                        {
                                            ChartTop[ch].C.Series[1].Points.AddXY(Cal.CodeY1[i], Cal.StrokeY[i]); //  stroke
                                                                                                                  //   ChartTop[ch].C.Series[9].Points.AddXY(Cal.CodeY1[i], Cal.StrokeY1[i]); //  stroke 1
                                                                                                                  // ChartTop[ch].C.Series[10].Points.AddXY(Cal.CodeY2[i], Cal.StrokeY2[i]); //  stroke 2
                                            ChartTop[ch].C.Series[4].Points.AddXY(Cal.CodeY1[i], Cal.Current[i]); //  current
                                            ChartTop[ch].C.Series[7].Points.AddXY(Cal.CodeY1[i], Cal.HallY1[i] / 10); //  hall
                                        }
                                    }
                                });
                            }
                            //Tilt
                            if (ChartBtm[ch].C.InvokeRequired)
                            {
                                ChartBtm[ch].C.BeginInvoke((MethodInvoker)delegate
                                {
                                    for (int i = 2; i < Cal.CodeY1.Count; i++)
                                    {
                                        if (Cal.CodeY1[i] >= OISCenter - CodeRange && Cal.CodeY1[i] <= OISCenter + CodeRange)
                                        {
                                            //ChartBtm[ch].C.Series[3].Points.AddXY(Cal.CodeY1[i], Cal.TiltX[i]); //  Tilt 
                                            //ChartBtm[ch].C.Series[4].Points.AddXY(Cal.CodeY1[i], Cal.TiltY[i]); //  Tilt 
                                            //ChartBtm[ch].C.Series[5].Points.AddXY(Cal.CodeY1[i], Cal.TiltZ[i]); //  Tilt 
                                        }
                                    }
                                });
                            }
                            break;
                        case "AF Scan":

                            CodeRange = Condition.iAFPlotRange;
                            //Stroke
                            if (ChartTop[ch].C.InvokeRequired)
                            {
                                ChartTop[ch].C.BeginInvoke((MethodInvoker)delegate
                                {
                                    for (int i = 2; i < Cal.CodeZ.Count; i++)
                                    {
                                        if (Cal.CodeZ[i] >= AFCenter - CodeRange && Cal.CodeZ[i] <= AFCenter + CodeRange)
                                        {
                                            ChartTop[ch].C.Series[2].Points.AddXY(Cal.CodeZ[i], Cal.StrokeZ[i]); //  stroke
                                            ChartTop[ch].C.Series[5].Points.AddXY(Cal.CodeZ[i], Cal.Current[i]); //  current
                                            ChartTop[ch].C.Series[8].Points.AddXY(Cal.CodeZ[i], Cal.HallZ[i] / 10); //  hall
                                        }
                                    }
                                });
                            }
                            //Tilt
                            if (ChartBtm[ch].C.InvokeRequired)
                            {
                                ChartBtm[ch].C.BeginInvoke((MethodInvoker)delegate
                                {
                                    for (int i = 2; i < Cal.CodeZ.Count; i++)
                                    {
                                        if (Cal.CodeZ[i] >= AFCenter - CodeRange && Cal.CodeZ[i] <= AFCenter + CodeRange)
                                        {
                                            ChartBtm[ch].C.Series[6].Points.AddXY(Cal.CodeZ[i], Cal.TiltX[i]); //  Tilt 
                                            ChartBtm[ch].C.Series[7].Points.AddXY(Cal.CodeZ[i], Cal.TiltY[i]); //  Tilt 
                                                                                                               //  ChartBtm[ch].C.Series[8].Points.AddXY(Cal.CodeZ[i], Cal.TiltZ[i]); //  Tilt 
                                        }
                                    }
                                });
                            }
                            break;
                        case "AF Settling":
                            //Stroke
                            if (ChartTop[ch].C.InvokeRequired)
                            {
                                ChartTop[ch].C.BeginInvoke((MethodInvoker)delegate
                                {
                                    for (int i = 2; i < Cal.Time.Count; i++)
                                    {
                                        ChartTop[ch].C.Series[2].Points.AddXY(Cal.Time[i] * 1000, Cal.StrokeZ[i]); //  stroke
                                    }
                                });
                            }
                            //Tilt
                            if (ChartBtm[ch].C.InvokeRequired)
                            {
                                ChartBtm[ch].C.BeginInvoke((MethodInvoker)delegate
                                {
                                    for (int i = 2; i < Cal.Time.Count; i++)
                                    {
                                        ChartBtm[ch].C.Series[6].Points.AddXY(Cal.Time[i] * 1000, Cal.TiltX[i]); //  Tilt 
                                        ChartBtm[ch].C.Series[7].Points.AddXY(Cal.Time[i] * 1000, Cal.TiltY[i]); //  Tilt 
                                        ChartBtm[ch].C.Series[8].Points.AddXY(Cal.Time[i] * 1000, Cal.TiltZ[i]); //  Tilt 
                                    }
                                });
                            }
                            break;

                    }
                    ChartSet(ch, name);
                }
            }
        }
        private void ChartSet(int ch, string name)
        {
            //StrokeChart
            if (ChartTop[ch].C.InvokeRequired)
            {
                ChartTop[ch].C.BeginInvoke((MethodInvoker)delegate
                {
                    ChartTop[ch].C.ChartAreas[0].AxisX.MinorGrid.Enabled = false;
                    ChartTop[ch].C.ChartAreas[0].AxisY.MinorGrid.Enabled = false;
                    ChartTop[ch].C.ChartAreas[0].AxisY2.MajorGrid.Enabled = false;
                    ChartTop[ch].C.ChartAreas[0].AxisY2.MinorGrid.Enabled = false;

                    if (name.Contains("Settling"))
                    {
                        ChartTop[ch].C.Titles[0].Text = "Stroke vs Time";
                        ChartTop[ch].C.ChartAreas[0].AxisX.Minimum = 0;
                        ChartTop[ch].C.ChartAreas[0].AxisX.Maximum = 600;
                        ChartTop[ch].C.ChartAreas[0].AxisX.Interval = 100;
                        ChartTop[ch].C.ChartAreas[0].AxisX.MajorGrid.Interval = 100;
                    }
                    else
                    {
                        ChartTop[ch].C.Titles[0].Text = "Stroke vs Code";
                        ChartTop[ch].C.ChartAreas[0].AxisX.Minimum = 0;
                        ChartTop[ch].C.ChartAreas[0].AxisX.Maximum = 4100;
                        ChartTop[ch].C.ChartAreas[0].AxisX.Interval = 512;
                        ChartTop[ch].C.ChartAreas[0].AxisX.MajorGrid.Interval = 512;
                    }


                    ChartTop[ch].C.ChartAreas[0].AxisY.Minimum = -500;
                    ChartTop[ch].C.ChartAreas[0].AxisY.Maximum = 500;
                    ChartTop[ch].C.ChartAreas[0].AxisY.Interval = 100;
                    ChartTop[ch].C.ChartAreas[0].AxisY.MajorGrid.Interval = 100;

                    ChartTop[ch].C.ChartAreas[0].AxisY2.Minimum = -50;
                    ChartTop[ch].C.ChartAreas[0].AxisY2.Maximum = 410;
                    ChartTop[ch].C.ChartAreas[0].AxisY2.Interval = 45;

                    ChartTop[ch].C.ChartAreas[0].AxisY2.Enabled = AxisEnabled.True;
                    ChartTop[ch].C.ChartAreas[0].AxisY2.LabelStyle.ForeColor = Color.DarkGreen;
                    ChartTop[ch].C.ChartAreas[0].AxisY2.LabelStyle.Font = new Font("Calibri", 9, FontStyle.Bold);

                    ChartTop[ch].IsFalg = false;
                });
            }
            //Tilt Chart
            if (ChartBtm[ch].C.InvokeRequired)
            {
                ChartBtm[ch].C.BeginInvoke((MethodInvoker)delegate
                {
                    ChartBtm[ch].C.ChartAreas[0].AxisX.MinorGrid.Enabled = false;
                    ChartBtm[ch].C.ChartAreas[0].AxisY.MinorGrid.Enabled = false;
                    ChartBtm[ch].C.ChartAreas[0].AxisY2.MajorGrid.Enabled = false;
                    ChartBtm[ch].C.ChartAreas[0].AxisY2.MinorGrid.Enabled = false;


                    if (name.Contains("Settling"))
                    {
                        ChartBtm[ch].C.Titles[0].Text = "Tilt vs Time";
                        ChartBtm[ch].C.ChartAreas[0].AxisX.Minimum = 0;
                        ChartBtm[ch].C.ChartAreas[0].AxisX.Maximum = 600;
                        ChartBtm[ch].C.ChartAreas[0].AxisX.Interval = 100;
                        ChartBtm[ch].C.ChartAreas[0].AxisX.MajorGrid.Interval = 100;
                    }
                    else
                    {
                        ChartBtm[ch].C.Titles[0].Text = "Tilt vs Code";
                        ChartBtm[ch].C.ChartAreas[0].AxisX.Minimum = 0;
                        ChartBtm[ch].C.ChartAreas[0].AxisX.Maximum = 4100;
                        ChartBtm[ch].C.ChartAreas[0].AxisX.Interval = 512;
                        ChartBtm[ch].C.ChartAreas[0].AxisX.MajorGrid.Interval = 512;
                    }

                    ChartBtm[ch].C.ChartAreas[0].AxisY.Minimum = -50;
                    ChartBtm[ch].C.ChartAreas[0].AxisY.Maximum = 50;
                    ChartBtm[ch].C.ChartAreas[0].AxisY.Interval = 10;
                    ChartBtm[ch].C.ChartAreas[0].AxisY.MajorGrid.Interval = 10;

                    //ChartBtm[ch].C.ChartAreas[0].AxisY2.Minimum = -200;
                    //ChartBtm[ch].C.ChartAreas[0].AxisY2.Maximum = 200;
                    //ChartBtm[ch].C.ChartAreas[0].AxisY2.Interval = 40;
                    //ChartBtm[ch].C.ChartAreas[0].AxisY2.MajorGrid.Interval = 40;

                    ChartBtm[ch].C.ChartAreas[0].AxisY2.Enabled = AxisEnabled.True;
                    ChartBtm[ch].C.ChartAreas[0].AxisY2.LabelStyle.ForeColor = Color.DarkGreen;
                    ChartBtm[ch].C.ChartAreas[0].AxisY2.LabelStyle.Font = new Font("Calibri", 9, FontStyle.Bold);

                    ChartBtm[ch].IsFalg = false;
                });
            }
        }
        public void ClearChart()
        {
            for (int ch = 0; ch < ChartTop.Count; ch++)
            {
                if (ChartTop[ch].C.InvokeRequired)
                {
                    ChartTop[ch].C.BeginInvoke((MethodInvoker)delegate
                    {
                        for (int i = 0; i < ChartTop[ch].C.Series.Count; i++)
                        {
                            ChartTop[ch].C.Series[i].Points.Clear();
                        }
                        ChartTop[ch].C.Series[0].Points.AddXY(0, 0);
                    });
                }
                else
                {
                    for (int i = 0; i < ChartTop[ch].C.Series.Count; i++)
                    {
                        ChartTop[ch].C.Series[i].Points.Clear();
                    }
                    ChartTop[ch].C.Series[0].Points.AddXY(0, 0);
                }
            }
            for (int ch = 0; ch < ChartBtm.Count; ch++)
            {
                if (ChartBtm[ch].C.InvokeRequired)
                {
                    ChartBtm[ch].C.BeginInvoke((MethodInvoker)delegate
                    {
                        for (int i = 0; i < ChartBtm[ch].C.Series.Count; i++)
                        {
                            ChartBtm[ch].C.Series[i].Points.Clear();
                        }
                        ChartBtm[ch].C.Series[0].Points.AddXY(0, 0);
                    });
                }
                else
                {
                    for (int i = 0; i < ChartBtm[ch].C.Series.Count; i++)
                    {
                        ChartBtm[ch].C.Series[i].Points.Clear();
                    }
                    ChartBtm[ch].C.Series[0].Points.AddXY(0, 0);
                }
            }
        }
        public void RunTest()
        {
            if (RepeatRun == 1)
            {
                CurrentRun = 1;
                if (IsRun[0]) return;

                if (!IsRun[0])
                {
                    IsRun[0] = true;
                    Task.Factory.StartNew(() => LoadTestUnload(0));
                }
            }
            else
            {
                CurrentRun = 1;
                if (IsRun[0]) return;
                IsRun[0] = true;
                while (true)
                {
                    ClearChart();
                    foreach (var l in ViewLog) l.Clear();

                    Task tasks = null;
                    tasks = Task.Factory.StartNew(() => LoadTestUnload(0));
                    Task.WaitAll(tasks);

                    if (CurrentRun >= RepeatRun || SuddenStop) break;
                    CurrentRun++;
                    Thread.Sleep(1500);
                }
            }
        }
        public void LoadTestUnload(int port)
        {
            try
            {
                int ch = port * 2;
                Thread.Sleep(100);

                if (Dln.IsSafeOn & Option.SafeSensor)
                {
                    AddLog(ch, "Safe Sensor Detected. Push Start Button Again..");
                    IsRun[port] = false;
                    return;
                }

                RunStart?.Invoke(null, port);

                Process_Start(port);

                RunEnd?.Invoke(null, port);

                IsRun[port] = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                IsRun[port] = false;
            }
        }
        public void Process_Start(int port)
        {
            try
            {
                m__G.oCam[port].ResetmCpXY();
                int ch = port * 2;
                DrvIC.FRAModeDisable(ch);
                byte[] b = new byte[1];
                Dln.ReadArray(0, DrvIC.XSlaveAddr, 1, 0xE5, b);
                AddLog(ch, $"AF Best Pos = {b[0] << 2}");
                BestAFPos = b[0] << 4;
                int count = Condition.ToDoList.Count;
                if (count == 0)
                {
                    for (int i = ch; i < ch + ChannelCnt; i++)
                        errMsg[i] = "Test Item is Empty";
                    return;
                }
                for (int k = ch; k < ch + ChannelCnt; k++)
                {
                    m_ChannelOn[k] = true;
                    errMsg[k] = "";
                    PassFails[k].FirstFailIndex = 0;
                }
                Dln.PowerOnOff(0, false);
                Thread.Sleep(200);
                Dln.PowerOnOff(0, true);
                Thread.Sleep(200);

                if (!Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x40 })) m_ChannelOn[ch] = false;
                if (!Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x40 })) m_ChannelOn[ch] = false;
                if (!Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x40 })) m_ChannelOn[ch] = false;
                if (DrvIC.Y2SlaveAddr != 0x00) { if (!Dln.WriteArray(ch, DrvIC.Y2SlaveAddr, 0x02, new byte[] { 0x40 })) m_ChannelOn[ch] = false; }
                //m_ChannelOn[1] = false; // 1ch Test

                for (int k = ch; k < ch + ChannelCnt; k++)
                {
                    if (!m_ChannelOn[k])
                    {
                        errMsg[k] = "Socket Empty";
                        AddLog(k, "Socket Empty");
                    }
                }
                if (errMsg[ch] != "" /*&& errMsg[ch + 1] != ""*/)
                {
                    return;
                }

                Stopwatch sw = new Stopwatch();
                sw.Start();

                bool loopContinue = true;

                int todoCnt = 0;
                SuddenStop = false;

                for (int i = 0; i < Condition.ToDoList.Count; i++)
                {
                    MakeWaveform(Condition.ToDoList[i]);
                }


                while (todoCnt < count)
                {
                    string testItem = Condition.ToDoList[todoCnt];

                    Process_Function(port, testItem);

                    if (errMsg[ch] != "")
                    {
                        loopContinue = false;
                        AddLog(ch, errMsg[ch]);

                    }
                    if (SuddenStop)
                    {
                        loopContinue = false;
                        errMsg[ch] = errMsg[ch + 1] = "SuddenStop !";
                        AddLog(ch, errMsg[ch]);

                    }

                    if (!loopContinue) break;
                    else todoCnt++;
                    Thread.Sleep(100);
                }
                LEDs_All_On(port, false);

                double ellipse = (double)sw.ElapsedMilliseconds / 1000;
                sw.Stop();

                yield.LastSampleNum++;

                for (int k = ch; k < ch + ChannelCnt; k++)
                {
                    AddLog(k, string.Format("Total Test Time\t{0:0.000} sec", ellipse));
                    PassFails[k].TotalTime = ellipse.ToString("F3");
                }

                if (!SuddenStop) WriteResult(port);

                return;
            }
            catch
            {

            }
        }
        public void Process_Function(int port, string testItem)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            int index = 0;
            for (int i = 0; i < ItemList.Count; i++)
            {
                if (testItem == ItemList[i].Name)
                {
                    index = i; break;
                }
            }

            int ch = port * 2;
            if (!m_ChannelOn[ch])
                return;

            for (int k = ch; k < ch + ChannelCnt; k++)
            {
                if (m_ChannelOn[k])
                {
                    m_StrIndex[k] = (yield.LastSampleNum + k + 1).ToString();
                    AddLog(k, m_StrIndex[k] + ">> " + testItem + " Start");
                }
            }

            try
            {
                Task Func1 = null, Func2 = null;

                if (!ItemList[index].IsMulti)
                {
                    Func1 = new Task(() => ItemList[index].Func(port, testItem));
                    Func1.Start();

                    if (Func1 != null) Task.WaitAll(Func1);
                }
                else
                {
                    if (m_ChannelOn[ch])
                    {
                        Func1 = new Task(() => ItemList[index].Func(ch, testItem));
                        Func1.Start();
                        AddLog(ch, testItem + " Start");
                    }
                    if (ChannelCnt > 1)
                    {
                        if (m_ChannelOn[ch + 1])
                        {
                            Func2 = new Task(() => ItemList[index].Func(ch + 1, testItem));
                            Func2.Start();
                            AddLog(ch + 1, testItem + " Start");
                        }
                    }

                    if (Func1 != null && Func2 != null) Task.WaitAll(Func1, Func2);
                    else
                    {
                        if (Func1 != null) Task.WaitAll(Func1);
                        if (Func2 != null) Task.WaitAll(Func2);
                    }
                }
            }
            catch (Exception e)
            {
                for (int k = ch; k < ch + ChannelCnt; k++)
                {
                    AddLog(k, testItem + " Exception : " + e.ToString() + " ch : " + k.ToString());
                    errMsg[k] = testItem + " Error";
                    m_ChannelOn[k] = false;
                }
            }

            for (int k = ch; k < ch + ChannelCnt; k++)
            {
                if (m_ChannelOn[k])
                {
                    double ellipse = (double)sw.ElapsedMilliseconds / 1000;
                    AddLog(k, string.Format("{0}\t{1:0.000} sec", testItem, ellipse));
                    ItemList[index].Time = ellipse.ToString("F3");
                }
            }
            sw.Stop();
        }
        public void LEDs_All_On(int port, bool isOn, List<double> volt = null)
        {
            int ch = port * 2;

            if (volt == null)
            {
                volt = new List<double>
                {
                    Condition.LedCurrentL,
                    Condition.LedCurrentR
                };
            }

            if (m_bAllLEDOn = isOn)
            {
                //  CSH035 적용 시 
                Dln.SetLEDpower(1, (int)(Condition.LedCurrentL * 500));
                Dln.SetLEDpower(2, (int)(Condition.LedCurrentR * 500));
            }
            else
                for (int k = ch; k < ch + ChannelCnt; k++)
                {
                    Dln.SetLEDpower(1, 0);
                    Dln.SetLEDpower(2, 0);
                }
        }

        public void MakeWaveform(string name)
        {
            for (int k = 0; k < ChannelCnt; k++)
            {
                foreach (var Cal in CalList[k])
                {
                    if (Cal.Name == name)
                    {
                        Cal.Clear();

                        int min = 0;
                        int max = 0;
                        int step = 0;
                        int curPos = 0;

                        switch (name)
                        {
                            case "AF Scan":
                                //AF ========
                                MakeWaveformCode(ref Cal.CodeZ, Condition.iAFDrvCodeMin, Condition.iAFDrvCodeMax, AFCenter, Condition.iDrvAFStep);
                                break;
                            case "OIS X Scan":
                                //X =========
                                MakeWaveformCode(ref Cal.CodeX, Condition.iXDrvCodeMin, Condition.iXDrvCodeMax, OISCenter, Condition.iDrvXStep);
                                break;
                            case "OIS Y Scan":
                                //Y1 ===========================
                                MakeWaveformCode(ref Cal.CodeY1, Condition.iYDrvCodeMin, Condition.iYDrvCodeMax, OISCenter, Condition.iDrvYStep);
                                //Y2 ===========================
                                MakeWaveformCode(ref Cal.CodeY2, Condition.iY2DrvCodeMin, Condition.iY2DrvCodeMax, OISCenter, Condition.iDrvYStep);
                                break;

                            case "AF Settling":
                                min = Condition.iAFStandbyCode;
                                max = Condition.iAFJumpStepCode;
                                Cal.CodeZ.Add(min);
                                Cal.CodeZ.Add(min);
                                Cal.CodeZ.Add(min);
                                Cal.CodeZ.Add(max);
                                break;
                        }
                    }
                }

            }
        }
        private void MakeWaveformCode(ref List<int> code, int min, int max, int mid, int step)
        {
            int curPos = 0;

            curPos = mid;
            do
            {
                code.Add(curPos);
                curPos += step;
            } while (curPos < max);
            if (max >= 4095) max = 4095;
            code.Add(max);
            curPos -= step;
            do
            {
                code.Add(curPos);
                curPos -= step;
            } while (curPos > mid);

            int lastCode = 0;
            do
            {
                code.Add(curPos);
                curPos -= step;
            } while (curPos > min);
            lastCode = code[code.Count - 1];
            code.Add(min);

            curPos = lastCode;
            do
            {
                code.Add(curPos);
                curPos += step;
            } while (curPos < mid);
            code.Add(mid);

        }

        private void CrossOffsetMove(int port, string name)
        {
            int ch = port * 2;
            //Cross Offset Pos Move 
            for (int j = ch; j < ch + ChannelCnt; j++)
            {
                if (!m_ChannelOn[j]) continue;
                DrvIC.OISOn(j, name, true);

                switch (name)
                {
                    case "AF Scan":
                        Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x40 });
                        Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x40 });
                        if (DrvIC.Y2SlaveAddr != 0x00)
                            Dln.WriteArray(ch, DrvIC.Y2SlaveAddr, 0x02, new byte[] { 0x40 });
                        break;
                    case "OIS X Scan":
                        DrvIC.Move(j, "X", OISCenter);
                        DrvIC.OISOn(j, "Y", true);
                        DrvIC.Move(j, "Y", Condition.iXCrossOffset);
                        DrvIC.OISOn(j, "AF", true);
                        DrvIC.Move(j, "AF", BestAFPos);
                        AddLog(ch, $"Move AF Best Position : {BestAFPos}");
                        break;
                    case "OIS Y Scan":
                        DrvIC.Move(j, "Y", OISCenter);
                        DrvIC.OISOn(j, "X", true);
                        DrvIC.Move(j, "X", Condition.iYCrossOffset);
                        DrvIC.OISOn(j, "AF", true);
                        DrvIC.Move(j, "AF", BestAFPos);
                        AddLog(ch, $"Move AF Best Position : {BestAFPos}");
                        break;

                }
            }
            Thread.Sleep(100);
            //Initial Pos Move 

            for (int k = 0; k < 2; k++)
            {
                switch (name)
                {
                    case "AF Scan":
                        for (int j = ch; j < ch + ChannelCnt; j++)
                        {
                            if (!m_ChannelOn[j]) continue;
                            foreach (var Cal in CalList[j])
                            {
                                if (Cal.Name == name) DrvIC.Move(j, name, Cal.CodeZ[0]);
                            }
                        }
                        Thread.Sleep(Condition.iDrvStepIntervalZ);
                        break;
                    case "OIS X Scan":
                        for (int j = ch; j < ch + ChannelCnt; j++)
                        {
                            if (!m_ChannelOn[j]) continue;
                            foreach (var Cal in CalList[j])
                            {
                                if (Cal.Name == name) DrvIC.Move(j, name, Cal.CodeX[0]);
                            }
                        }
                        Thread.Sleep(Condition.iDrvStepIntervalX);
                        break;
                    case "OIS Y Scan":
                        for (int j = ch; j < ch + ChannelCnt; j++)
                        {
                            if (!m_ChannelOn[j]) continue;
                            foreach (var Cal in CalList[j])
                            {
                                if (Cal.Name == name) DrvIC.Move(j, name, Cal.CodeY1[0]);
                            }
                        }
                        Thread.Sleep(Condition.iDrvStepIntervalY);
                        break;

                }
            }
        }
        private void Process_ScanCodeTest(int port, string name)
        {
            int ch = port * 2;

            Thread.Sleep(100);

            CrossOffsetMove(port, name);

            IsScan[port] = true;
            framCnt[port] = 0;
            int curPos = 0;

            Stopwatch sw = new Stopwatch();
            sw.Reset(); sw.Start();
            while (IsScan[port])
            {
                for (int j = ch; j < ch + ChannelCnt; j++)
                {
                    if (!m_ChannelOn[j]) continue;
                    foreach (var Cal in CalList[j])
                        if (Cal.Name == name)
                        {
                            if (name.Contains("X"))
                            {
                                DrvIC.Move(j, "X", Cal.CodeX[framCnt[port]]);
                                DrvIC.Move(j, "Y", OISCenter);
                            }
                            else if (name.Contains("Y"))
                            {
                                DrvIC.Move(j, "X", OISCenter);
                                DrvIC.Move(j, "Y1", Cal.CodeY1[framCnt[port]]);
                                if(DrvIC.Y2SlaveAddr != 0x00) DrvIC.Move(j, "Y2", Cal.CodeY1[framCnt[port]]);

                            }
                            else if (name.Contains("AF"))
                            {
                                DrvIC.Move(j, name, Cal.CodeZ[framCnt[port]]);
                            }

                            Cal.StrokeX.Add(0);
                            Cal.StrokeY.Add(0);
                            Cal.StrokeZ.Add(0);
                            Cal.StrokeY1.Add(0);
                            Cal.StrokeY2.Add(0);
                            Cal.HallX.Add(0);
                            Cal.HallY.Add(0);
                            Cal.HallZ.Add(0);
                            Cal.HallY1.Add(0);
                            Cal.HallY2.Add(0);
                            Cal.Current.Add(0);
                            Cal.TiltX.Add(0);
                            Cal.TiltY.Add(0);
                            Cal.TiltZ.Add(0);
                        }
                }
                if (name.Contains("X"))
                {
                    Thread.Sleep(Condition.iDrvStepIntervalX);
                }
                else if (name.Contains("Y"))
                {
                    Thread.Sleep(Condition.iDrvStepIntervalY);
                }
                else if (name.Contains("AF"))
                {
                    Thread.Sleep(Condition.iDrvStepIntervalZ);
                }

                for (int j = ch; j < ch + ChannelCnt; j++)
                {
                    if (!m_ChannelOn[j]) continue;
                    foreach (var Cal in CalList[j])
                        if (Cal.Name == name)
                        {
                            Cal.HallX[framCnt[port]] = DrvIC.ReadHall(j, "X");
                            Cal.HallY1[framCnt[port]] = DrvIC.ReadHall(j, "Y1");
                            // Cal.HallY2[framCnt[port]] = DrvIC.ReadHall(j, "Y2");
                            Cal.HallZ[framCnt[port]] = DrvIC.ReadHall(j, "AF");
                            //Get Hall
                            if (name.Contains("X"))
                            {
                                Cal.Current[framCnt[port]] = Dln.GetCurrent(j, 1);
                                AddLog(j, string.Format("{0} == Code : {1}, Hall : {2}", name, Cal.CodeX[framCnt[port]], Cal.HallX[framCnt[port]]));
                            }
                            else if (name.Contains("Y"))
                            {
                                Cal.Current[framCnt[port]] = Dln.GetCurrent(j, 1);
                                AddLog(j, string.Format("{0} == Code : {1}, Hall1 : {2}, Hall2 : {3}", name, Cal.CodeY1[framCnt[port]], Cal.HallY1[framCnt[port]], Cal.HallY2[framCnt[port]]));
                            }
                            else if (name.Contains("AF"))
                            {
                                Cal.Current[framCnt[port]] = Dln.GetCurrent(j, 0);
                                AddLog(j, string.Format("{0} == Code : {1}, Hall : {2}", name, Cal.CodeZ[framCnt[port]], Cal.HallZ[framCnt[port]]));
                            }

                        }
                }
                STATIC.fVision.m__G.oCam[port].GrabA(framCnt[port]);

                for (int j = ch; j < ch + ChannelCnt; j++)
                {
                    if (!m_ChannelOn[j]) continue;
                    foreach (var Cal in CalList[j])
                        if (Cal.Name == name)
                        {
                            if (name.Contains("X"))
                            {
                                if (Cal.CodeX.Count - 1 == framCnt[port]) IsScan[port] = false;
                            }
                            else if (name.Contains("Y"))
                            {
                                if (Cal.CodeY1.Count - 1 == framCnt[port]) IsScan[port] = false;
                            }
                            else if (name.Contains("AF"))
                            {
                                if (Cal.CodeZ.Count - 1 == framCnt[port]) IsScan[port] = false;
                            }

                        }
                }
                framCnt[port]++;
            }
            long esec = sw.ElapsedMilliseconds;
            sw.Stop();

            double fps = 0;
            if (name.Contains("X"))
            {
                fps = esec - Condition.iDrvStepIntervalX * framCnt[port];
            }
            else if (name.Contains("Y"))
            {
                fps = esec - Condition.iDrvStepIntervalY * framCnt[port];
            }
            else if (name.Contains("AF"))
            {
                fps = esec - Condition.iDrvStepIntervalZ * framCnt[port];
            }

            fps = fps / 1000;
            fps = framCnt[port] / fps * 2.4;

            for (int j = ch; j < ch + ChannelCnt; j++)
            {
                DrvIC.OISOn(j, "AF", false);
                DrvIC.OISOn(j, "X", false);
                DrvIC.OISOn(j, "Y", false);
            }
            for (int j = ch; j < ch + ChannelCnt; j++)
                AddLog(j, string.Format("framCnt {0}", framCnt[port]));

            STATIC.fVision.m__G.oCam[port].CommonToReplayBuf(name, framCnt[port]);
        }
        public double settleRigingTime = 0;
        private void Process_ScanTimeTest(int port, string name)
        {
            settleRigingTime = 0;

            int ch = port * 2;

            MakeWaveform(name);

            for (int j = ch; j < ch + ChannelCnt; j++)
            {
                DrvIC.OISOn(j, "X", false);
                DrvIC.OISOn(j, "Y", false);
                DrvIC.OISOn(j, "AF", true);
                DrvIC.Move(j, "AF", BestAFPos);
                AddLog(ch, $"Move AF Best Position : {BestAFPos}");
            }

            Stopwatch sw = new Stopwatch();
            sw.Reset(); sw.Start();
            //Time Grab ===============
            Task[] task = new Task[2];

            long startTime = 0;
            long endTime = 0;
            long lTimerFrequency = 0;
            SupremeTimer.QueryPerformanceCounter(ref startTime);
            SupremeTimer.QueryPerformanceCounter(ref endTime);
            SupremeTimer.QueryPerformanceFrequency(ref lTimerFrequency);

            double Ellapsed = 1000000 * (endTime - startTime) / (double)(lTimerFrequency);
            task[0] = Task.Factory.StartNew(() =>
            {
                IsScan[port] = true;
                SupremeTimer.QueryPerformanceCounter(ref startTime);
                while (IsScan[port])
                {
                    STATIC.fVision.m__G.oCam[port].GrabD(framCnt[port]);
                    for (int j = ch; j < ch + ChannelCnt; j++)
                    {
                        foreach (var Cal in CalList[j])
                            if (Cal.Name == name)
                            {
                                SupremeTimer.QueryPerformanceCounter(ref endTime);
                                SupremeTimer.QueryPerformanceFrequency(ref lTimerFrequency);
                                Ellapsed = 1000 * (endTime - startTime) / (double)(lTimerFrequency); //  msec

                                Cal.Time.Add(Ellapsed);
                                Cal.StrokeX.Add(0);
                                Cal.StrokeY.Add(0);
                                Cal.StrokeZ.Add(0);
                                Cal.StrokeY1.Add(0);
                                Cal.StrokeY2.Add(0);
                                Cal.TiltX.Add(0);
                                Cal.TiltY.Add(0);
                                Cal.TiltZ.Add(0);
                            }
                    }
                    framCnt[port]++;
                }
            });

            task[1] = Task.Factory.StartNew(() =>
            {
                foreach (var Cal in CalList[port])
                    if (Cal.Name == name)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            for (int j = ch; j < ch + ChannelCnt; j++)
                            {
                                if (Cal.Name == name)
                                {
                                    DrvIC.Move(j, name, Cal.CodeZ[i]);
                                }
                            }
                        }
                        Thread.Sleep(100);
                        for (int j = ch; j < ch + ChannelCnt; j++)
                        {
                            if (Cal.Name == name)
                            {
                                DrvIC.Move(j, name, Cal.CodeZ[3]);
                            }
                        }
                        settleRigingTime = (double)sw.ElapsedMilliseconds / 1000;
                        Thread.Sleep(400);
                    }
                IsScan[port] = false;
            });

            Task t = Task.WhenAll(task);
            try
            {
                t.Wait();
            }
            catch { }
            sw.Stop();

            // FrmRate 표시 === 
            double frameRate = framCnt[port] / (double)sw.ElapsedMilliseconds * 1000;
            for (int j = ch; j < ch + ChannelCnt; j++)
            {
                AddLog(j, string.Format("FrmRate == {0:F2} frame/sec", frameRate));
            }
            STATIC.fVision.m__G.oCam[port].CommonToReplayBuf(name, framCnt[port]);

            for (int j = ch; j < ch + ChannelCnt; j++)
            {
                DrvIC.OISOn(j, "AF", false);
                DrvIC.OISOn(j, "X", false);
                DrvIC.OISOn(j, "Y", false);
            }
        }
        public void Process_CalcCodeTest(int port, string name)
        {
            int ch = port * 2;

            for (int j = ch; j < ch + ChannelCnt; j++)
            {
                AddLog(j, string.Format("{0} Driving Data>>", name));
            }
            List<FindResult> result = new List<FindResult>();
            int fCount = 0;
            foreach (var Cal in CalList[port])
                if (Cal.Name == name)
                {
                    if (name.Contains("X"))
                    {
                        fCount = Cal.CodeX.Count;
                    }
                    else if (name.Contains("Y"))
                    {
                        fCount = Cal.CodeY1.Count;
                    }
                    else if (name.Contains("AF"))
                    {
                        fCount = Cal.CodeZ.Count;
                    }

                }

            for (int i = 0; i < fCount; i++)
            {
                result.Add(new FindResult());

                result[i] = STATIC.fVision.MeasureTxTyTz(i, name, true, false);

            }

            //////////////////////////////////////////////////////////////////////////////
            //////////////////////////////////////////////////////////////////////////////


            for (int j = ch; j < ch + ChannelCnt; j++)
            {
                if (!m_ChannelOn[j]) continue;
                foreach (var Cal in CalList[j])
                    if (Cal.Name == name)
                    {
                        double centerX = 0;
                        double centerY = 0;
                        double centerY1 = 0;
                        double centerY2 = 0;
                        double centerZ = 0;
                        double centertX = 0;
                        double centertY = 0;
                        double centertZ = 0;

                        bool isCentered = false;
                        for (int i = 2; i < fCount; i++)
                        {
                            if (name.Contains("X"))
                            {
                                if (Cal.CodeX[i] == OISCenter)
                                {
                                    centerX = result[i].cx[j];
                                    centerY = result[i].cy[j];
                                    centerZ = result[i].cz[j];
                                    centertX = result[i].tx[j];
                                    centertY = result[i].ty[j];
                                    centertZ = result[i].tz[j];
                                    centerY1 = result[i].cy1[j];
                                    centerY2 = result[i].cy2[j];
                                    isCentered = true;
                                    break;
                                }
                            }
                            else if (name.Contains("Y"))
                            {
                                if (Cal.CodeY1[i] == OISCenter)
                                {
                                    centerX = result[i].cx[j];
                                    centerY = result[i].cy[j];
                                    centerZ = result[i].cz[j];
                                    centertX = result[i].tx[j];
                                    centertY = result[i].ty[j];
                                    centertZ = result[i].tz[j];
                                    centerY1 = result[i].cy1[j];
                                    centerY2 = result[i].cy2[j];
                                    isCentered = true;
                                    break;
                                }
                            }
                            else if (name.Contains("AF"))
                            { 
                                if (Cal.CodeZ[i] == AFCenter)
                                {
                                    centerX = result[i].cx[j];
                                    centerY = result[i].cy[j];
                                    centerZ = result[i].cz[j];
                                    centertX = result[i].tx[j];
                                    centertY = result[i].ty[j];
                                    centertZ = result[i].tz[j];
                                    centerY1 = result[i].cy1[j];
                                    centerY2 = result[i].cy2[j];
                                    isCentered = true;
                                    break;
                                }
                            }


                        }
                        if (!isCentered)
                        {
                            AddLog(j, string.Format("Center Code Data Failed"));
                        }

                        //if (Option.FixedCenter)
                        //{
                        //    int centerPoint = 0;
                        //    for (int i = 0; i < fCount; i++)
                        //    {
                        //        if (name.Contains("X"))
                        //        {
                        //            centerPoint = HallParam[j].XHmid;
                        //        }
                        //        else if (name.Contains("Y"))
                        //        {
                        //            centerPoint = HallParam[j].YHmid;
                        //        }

                        //    }
                        //    centerX = result[centerPoint].cx[j]; centerY = result[centerPoint].cy[j];
                        //    centerY1 = result[centerPoint].cy1[j]; centerY2 = result[centerPoint].cy2[j];
                        //}
                        //else
                        //{
                        //}
                        for (int i = 0; i < fCount; i++)
                        {
                            Cal.StrokeX[i] = result[i].cx[j] - centerX;
                            Cal.StrokeY[i] = result[i].cy[j] - centerY;
                            Cal.StrokeZ[i] = result[i].cz[j] - centerZ;
                            Cal.StrokeY1[i] = result[i].cy1[j] - centerY1;
                            Cal.StrokeY2[i] = result[i].cy2[j] - centerY2;
                            Cal.TiltX[i] = result[i].tx[j] - centertX;
                            Cal.TiltY[i] = result[i].ty[j] - centertY;
                            Cal.TiltZ[i] = result[i].tz[j] - centertZ;
                        }
                    }
            }
            if (Option.SaveRawData)
            {
                string str = Convert.ToString(yield.LastSampleNum + 1);
                string dateDir = STATIC.CreateDateDir();
                dateDir += "DrivingData\\";
                if (!Directory.Exists(dateDir))
                    Directory.CreateDirectory(dateDir);

                DateTime dt = DateTime.Now;
                //string timeDir = string.Format("{0}{1}{2}", dt.Hour, dt.Minute, dt.Second);
                string timeDir = dt.ToString("HHmmss");
                string st = timeDir;

                for (int j = ch; j < ch + ChannelCnt; j++)
                {
                    if (!m_ChannelOn[j]) continue;
                    foreach (var Cal in CalList[j])
                        if (Cal.Name == name)
                        {
                            List<string> arry = new List<string>();
                            arry.Add(dt.ToString("MM:dd:hh:mm:ss"));
                            string path = "";
                            switch (name)
                            {
                                case "AF Scan":

                                    arry.Add("i,AF Code,X Code,Y1 Code,Y2 Code,X,Y,Z,TX,TY,TZ,Y1,Y2,Hall X,Hall Y1,Hall Y2,Hall AF,Current");
                                    for (int i = 0; i < fCount; i++)
                                    {
                                        path = string.Format(dateDir + "{0}_{1}_{2}_{3}.csv", name, m_StrIndex[j], yield.LastSampleNum + 1, st);
                                        string data = string.Format("{0},{1},{2},{3},{4},{5:0.000},{6:0.000},{7:0.000},{8:0.000},{9:0.000},{10:0.000},{11:0.000},{12:0.000},{13},{14},{15},{16},{17:0.000}", i, Cal.CodeZ[i], Condition.iAFCrossOffsetX, Condition.iAFCrossOffsetY, Condition.iAFCrossOffsetY,
                                            Cal.StrokeX[i], Cal.StrokeY[i], Cal.StrokeZ[i], Cal.TiltX[i], Cal.TiltY[i], Cal.TiltZ[i], Cal.StrokeY1[i], Cal.StrokeY2[i],
                                            Cal.HallX[i], Cal.HallY1[i], Cal.HallY2[i], Cal.HallZ[i], Cal.Current[i]);
                                        arry.Add(data);
                                        if (i == 0)
                                            AddLog(j, string.Format("Code AF\tStroke AF\tTx\tTy\tTz"));
                                        AddLog(j, string.Format("{0}\t{1:0.000}\t{2:0.000}\t{3:0.000}\t{4:0.000}", Cal.CodeZ[i], Cal.StrokeZ[i], Cal.TiltX[i], Cal.TiltY[i], Cal.TiltZ[i]));
                                    }
                                    break;
                                case "OIS X Scan":

                                    arry.Add("i,AF Code,X Code,Y1 Code,Y2 Code,X,Y,Z,TX,TY,TZ,Y1,Y2,Hall X,Hall Y1,Hall Y2,Hall AF,Current");
                                    for (int i = 0; i < fCount; i++)
                                    {
                                        if (name.Contains("Linearity"))
                                            path = string.Format(dateDir + "{0}_{1}_{2}_{3}_Lin.csv", name, m_StrIndex[j], yield.LastSampleNum + 1, st);
                                        else path = string.Format(dateDir + "{0}_{1}_{2}_{3}.csv", name, m_StrIndex[j], yield.LastSampleNum + 1, st);
                                        string data = string.Format("{0},{1},{2},{3},{4},{5:0.000},{6:0.000},{7:0.000},{8:0.000},{9:0.000},{10:0.000},{11:0.000},{12:0.000},{13},{14},{15},{16},{17:0.000}", i, BestAFPos, Cal.CodeX[i], Condition.iXCrossOffset, Condition.iXCrossOffset,
                                            Cal.StrokeX[i], Cal.StrokeY[i], Cal.StrokeZ[i], Cal.TiltX[i], Cal.TiltY[i], Cal.TiltZ[i], Cal.StrokeY1[i], Cal.StrokeY2[i],
                                            Cal.HallX[i], Cal.HallY1[i], Cal.HallY2[i], Cal.HallZ[i], Cal.Current[i]);
                                        arry.Add(data);

                                        if (i == 0)
                                            AddLog(j, string.Format("Code X\tStroke X\tTx\tTy\tTz"));
                                        AddLog(j, string.Format("{0}\t{1:0.000}\t{2:0.000}\t{3:0.000}\t{4:0.000}", Cal.CodeX[i], Cal.StrokeX[i], Cal.TiltX[i], Cal.TiltY[i], Cal.TiltZ[i]));
                                    }

                                    AddLog(j, string.Format("Cross Y1 Hall Max {0:00} Y1 Hall Min {1:00}", Cal.HallY1.Max(), Cal.HallY1.Min()));
                                    AddLog(j, string.Format("Cross Y1 Hall Diff {0:00}", Math.Abs(Cal.HallY1.Max() - Cal.HallY1.Min())));
                                    AddLog(j, string.Format("Cross Y2 Hall Max {0:00} Y2 Hall Min {1:00}", Cal.HallY2.Max(), Cal.HallY2.Min()));
                                    AddLog(j, string.Format("Cross Y2 Hall Diff {0:00}", Math.Abs(Cal.HallY2.Max() - Cal.HallY2.Min())));

                                    AddLog(j, string.Format("Rotation Max {0:00} Min {1:00}", Cal.TiltZ.Max(), Cal.TiltZ.Min()));
                                    AddLog(j, string.Format("Rotation Diff {0:00}", Math.Abs(Cal.TiltZ.Max() - Cal.TiltZ.Min())));

                                    break;
                                case "OIS Y Scan":

                                    arry.Add("i,AF Code,X Code,Y1 Code,Y2 Code,X,Y,Z,TX,TY,TZ,Y1,Y2,Hall X,Hall Y1,Hall Y2,Hall AF,Current");
                                    for (int i = 0; i < fCount; i++)
                                    {
                                        if (name.Contains("Linearity"))
                                            path = string.Format(dateDir + "{0}_{1}_{2}_{3}_Lin.csv", name, m_StrIndex[j], yield.LastSampleNum + 1, st);
                                        else path = string.Format(dateDir + "{0}_{1}_{2}_{3}.csv", name, m_StrIndex[j], yield.LastSampleNum + 1, st);
                                        string data = string.Format("{0},{1},{2},{3},{4},{5:0.000},{6:0.000},{7:0.000},{8:0.000},{9:0.000},{10:0.000},{11:0.000},{12:0.000},{13},{14},{15},{16},{17:0.000}", i, BestAFPos, Condition.iYCrossOffset, Cal.CodeY1[i], Cal.CodeY1[i],
                                               Cal.StrokeX[i], Cal.StrokeY[i], Cal.StrokeZ[i], Cal.TiltX[i], Cal.TiltY[i], Cal.TiltZ[i], Cal.StrokeY1[i], Cal.StrokeY2[i],
                                             Cal.HallX[i], Cal.HallY1[i], Cal.HallY2[i], Cal.HallZ[i], Cal.Current[i]);
                                        arry.Add(data);

                                        if (i == 0)
                                            AddLog(j, string.Format("Code Y1\tCode Y2\tStroke Y1\tStroke Y2\t\tTx\tTy\tTz"));

                                        AddLog(j, string.Format("{0}\t{1}\t{2:0.000}\t{3:0.000}\t{4:0.000}\t{5:0.000}\t{6:0.000}", Cal.CodeY1[i], Cal.CodeY1[i], Cal.StrokeY1[i], Cal.StrokeY2[i], Cal.TiltX[i], Cal.TiltY[i], Cal.TiltZ[i]));
                                    }

                                    AddLog(j, string.Format("Cross X Hall Max {0:00} X Hall Min {1:00}", Cal.HallY2.Max(), Cal.HallY2.Min()));
                                    AddLog(j, string.Format("Cross X Hall Diff {0:00}", Math.Abs(Cal.HallY2.Max() - Cal.HallY2.Min())));

                                    AddLog(j, string.Format("Rotation Max {0:00} Min {1:00}", Cal.TiltZ.Max(), Cal.TiltZ.Min()));
                                    AddLog(j, string.Format("Rotation Diff {0:00}", Math.Abs(Cal.TiltZ.Max() - Cal.TiltZ.Min())));

                                    break;

                            }
                            if (path != "") STATIC.SetTextLine(path, arry);
                        }
                }
            }

            for (int j = ch; j < ch + ChannelCnt; j++)
            {
                if (!m_ChannelOn[j]) continue;
                foreach (var Cal in CalList[j])
                    if (Cal.Name == name)
                    {
                        double forword = 0, backword = 0;
                        if (name.Contains("Linearity")) return;
                        if (name.Contains("AF"))
                        {
                            forword = PassFails[j].Results[(int)SpecItem.AF_Forwardstroke].Val = Math.Abs(Cal.StrokeZ.Max()); //Cal.CalFwdStoke(Cal.CodeZ, Cal.StrokeZ);
                            backword = PassFails[j].Results[(int)SpecItem.AF_Backwardstroke].Val = Math.Abs(Cal.StrokeZ.Min()); //Cal.CalBwdStoke(Cal.CodeZ, Cal.StrokeZ);
                            PassFails[j].Results[(int)SpecItem.AF_Ratedstroke].Val = forword + backword;
                            PassFails[j].Results[(int)SpecItem.AF_Sensitivity].Val = Cal.CalSensitivity(Cal.CodeZ, Cal.StrokeZ, Condition.iAFCodeRange, Condition.iAFStrokeRange, AFCenter);
                            PassFails[j].Results[(int)SpecItem.AF_Linearity].Val = Cal.CalLinearity(Cal.CodeZ, Cal.StrokeZ, Condition.AFLinMinRange, Condition.AFLinMaxRange, Condition.AFLinMinStep,
                                Condition.AFLinMaxStep, Condition.AFLinMinStroke, Condition.AFLinMaxStroke, Condition.AFLinMode);
                            PassFails[j].Results[(int)SpecItem.AF_Hysteresis].Val = Cal.CalHysteresis(Cal.CodeZ, Cal.StrokeZ, Condition.AFHysMinRange, Condition.AFHysMaxRange, Condition.AFHysMinStep,
                                Condition.AFhysMaxStep, Condition.AFHysMinStroke, Condition.AFHysMaxStroke, Condition.AFHysMode);


                            double[] MtoM = Cal.CalCurrent(Cal.CodeZ, Cal.StrokeZ, Cal.Current, Condition.AFCurrMinRange, Condition.AFCurrMaxRange, Condition.AFCurrMinStep, Condition.AFCurrMaxStep,
                                Condition.AFCurrMinStroke, Condition.AFCurrMaxStroke, Condition.AFCurrMode);

                            PassFails[j].Results[(int)SpecItem.AF_MaxCurrent].Val = MtoM[0]; //Cal.CalMaxCurrent(Cal.CodeZ, Cal.StrokeZ, Condition.iAFCodeRange, Condition.iAFStrokeRange);
                            PassFails[j].Results[(int)SpecItem.AF_MinCurrent].Val = MtoM[1];
                            //     PassFails[j].Results[(int)SpecItem.AF_HoldingCurrent].Val = Cal.CalHoldingCurrent(Cal.CodeZ, Cal.StrokeZ, Condition.iAFCodeRange, Condition.iAFStrokeRange);
                            PassFails[j].Results[(int)SpecItem.AF_CrosstalkX].Val = Cal.CalCrosstalkAF(Cal.CodeZ, Cal.StrokeZ, Cal.StrokeX, Condition.iAFCodeRange, Condition.iAFStrokeRange, AFCenter);
                            PassFails[j].Results[(int)SpecItem.AF_CrosstalkY].Val = Cal.CalCrosstalkAF(Cal.CodeZ, Cal.StrokeZ, Cal.StrokeY, Condition.iAFCodeRange, Condition.iAFStrokeRange, AFCenter);
                            PassFails[j].Results[(int)SpecItem.AF_CrosstalkR].Val = Cal.CalCrosstalkR(Cal.CodeZ, Cal.StrokeX, Cal.StrokeY, Condition.iAFCodeRange, Condition.iAFStrokeRange, AFCenter);
                            PassFails[j].Results[(int)SpecItem.AF_Rolling].Val = Cal.CalRolling(Cal.CodeZ, Cal.StrokeZ, Condition.iAFCodeRange, Condition.iAFStrokeRange, AFCenter);
                            PassFails[j].Results[(int)SpecItem.AF_Tilt].Val = Cal.CalTilt(Cal.CodeZ, Cal.TiltX, Cal.TiltY, Condition.TiltMinCode, Condition.TiltMaxCode, Condition.TiltRefCode);
                            SetResult(j, (int)SpecItem.AF_Ratedstroke, (int)SpecItem.AF_Tilt);
                            ShowDataResults(j, "AF", (int)SpecItem.AF_Ratedstroke, (int)SpecItem.AF_Tilt);
                        }
                        else if (name.Contains("OIS X"))
                        {
                            forword = PassFails[j].Results[(int)SpecItem.OISX_Forwardstroke].Val = Math.Abs(Cal.StrokeX.Max());// Cal.CalFwdStoke(Cal.CodeX, Cal.StrokeX);
                            backword = PassFails[j].Results[(int)SpecItem.OISX_Backwardstroke].Val = Math.Abs(Cal.StrokeX.Min());//Cal.CalBwdStoke(Cal.CodeX, Cal.StrokeX);
                            PassFails[j].Results[(int)SpecItem.OISX_Ratedstroke].Val = forword + backword;
                            PassFails[j].Results[(int)SpecItem.OISX_Sensitivity].Val = Cal.CalSensitivity(Cal.CodeX, Cal.StrokeX, Condition.iXCodeRange, Condition.iXStrokeRange, OISCenter);
                            PassFails[j].Results[(int)SpecItem.OISX_Linearity].Val = Cal.CalLinearity(Cal.CodeX, Cal.StrokeX, Condition.XLinMinRange, Condition.XLinMaxRange, Condition.XLinMinStep,
                                Condition.XLinMaxStep, Condition.XLinMinStroke, Condition.XLinMaxStroke, Condition.XLinMode);
                            PassFails[j].Results[(int)SpecItem.OISX_Hysteresis].Val = Cal.CalHysteresis(Cal.CodeX, Cal.StrokeX, Condition.XHysMinRange, Condition.XHysMaxRange, Condition.XHysMinStep,
                                Condition.XHysMaxStep, Condition.XHysMinStroke, Condition.XHysMaxStroke, Condition.XHysMode);

                            double[] MtoM = Cal.CalCurrent(Cal.CodeX, Cal.StrokeX, Cal.Current, Condition.XCurrMinRange, Condition.XCurrMaxRange, Condition.XCurrMinStep, Condition.XCurrMaxStep,
                              Condition.XCurrMinStroke, Condition.XCurrMaxStroke, Condition.XCurrMode);
                            PassFails[j].Results[(int)SpecItem.OISX_MaxCurrent].Val = MtoM[0]; //Cal.CalMaxCurrent(Cal.CodeX, Cal.StrokeX, Condition.iXCodeRange, Condition.iXStrokeRange);
                            PassFails[j].Results[(int)SpecItem.OISX_MinCurrent].Val = MtoM[1];
                            // PassFails[j].Results[(int)SpecItem.OISX_CenteringCurrent].Val = Cal.CalCenterCurrent(Cal.CodeX, Cal.StrokeX, Condition.iXCodeRange, Condition.iXCodeRange);

                            double[] xtalkRes = Cal.CalCrosstalk(Cal.CodeX, Cal.StrokeX, Cal.StrokeY, Condition.iXCodeRange, Condition.iXStrokeRange, OISCenter);

                            PassFails[j].Results[(int)SpecItem.OISX_CrosstalkY].Val = xtalkRes[0];
                            PassFails[j].Results[(int)SpecItem.OISX_CrosstalkY_dB].Val = xtalkRes[1];
                            PassFails[j].Results[(int)SpecItem.OISX_CrosstalkY_P2P].Val = xtalkRes[2];
                            PassFails[j].Results[(int)SpecItem.OISX_CrosstalkYP2P_dB].Val = xtalkRes[3];
                            //PassFails[j].Results[(int)SpecItem.OISX_CrosstalkZ].Val = Cal.CalCrosstalk(Cal.CodeX, Cal.StrokeZ, Condition.iXCodeRange, Condition.iXCodeRange);
                            //PassFails[j].Results[(int)SpecItem.OISX_CrosstalkR].Val = Cal.CalCrosstalkR(Cal.CodeX, Cal.StrokeY, Cal.StrokeZ, Condition.iXCodeRange, Condition.iXCodeRange);
                            PassFails[j].Results[(int)SpecItem.OISX_Rolling].Val = Cal.CalRolling(Cal.CodeX, Cal.StrokeX, Condition.iXCodeRange, Condition.iXStrokeRange, OISCenter);
                            SetResult(j, (int)SpecItem.OISX_Ratedstroke, (int)SpecItem.OISX_Rolling);
                            ShowDataResults(j, "X", (int)SpecItem.OISX_Ratedstroke, (int)SpecItem.OISX_Rolling);
                            SlopeX = Cal.CalSlopeForOISShift(Cal.CodeX, Cal.StrokeX);

                            PassFails[j].Results[(int)SpecItem.x_HallDecenter].Val = (forword - backword) / 2.0;
                            SetResult(j, (int)SpecItem.x_HallDecenter, (int)SpecItem.x_HallDecenter);
                            ShowDataResults(j, "Hall Decenter", (int)SpecItem.x_HallDecenter, (int)SpecItem.x_HallDecenter);
                        }
                        else if (name.Contains("OIS Y"))
                        {
                            forword = PassFails[j].Results[(int)SpecItem.OISY_Forwardstroke].Val = Math.Abs(Cal.StrokeY.Max());// Cal.CalFwdStoke(Cal.CodeY1, Cal.StrokeY);
                            backword = PassFails[j].Results[(int)SpecItem.OISY_Backwardstroke].Val = Math.Abs(Cal.StrokeY.Min()); // Cal.CalBwdStoke(Cal.CodeY1, Cal.StrokeY);
                            PassFails[j].Results[(int)SpecItem.OISY_Ratedstroke].Val = forword + backword;

                            PassFails[j].Results[(int)SpecItem.OISY_Sensitivity].Val = Cal.CalSensitivity(Cal.CodeY1, Cal.StrokeY, Condition.iYCodeRange, Condition.iYStrokeRange, OISCenter);
                            PassFails[j].Results[(int)SpecItem.OISY_Linearity].Val = Cal.CalLinearity(Cal.CodeY1, Cal.StrokeY, Condition.YLinMinRange, Condition.YLinMaxRange, Condition.YLinMinStep,
                                Condition.YLinMaxStep, Condition.YLinMinStroke, Condition.YLinMaxStroke, Condition.YLinMode);
                            PassFails[j].Results[(int)SpecItem.OISY_Hysteresis].Val = Cal.CalHysteresis(Cal.CodeY1, Cal.StrokeY, Condition.YHysMinRange, Condition.YHysMaxRange, Condition.YHysMinStep,
                                Condition.YHysMaxStep, Condition.YHysMinStroke, Condition.YHysMaxStroke, Condition.YHysMode);

                            double[] MtoM = Cal.CalCurrent(Cal.CodeY1, Cal.StrokeY, Cal.Current, Condition.YCurrMinRange, Condition.YCurrMaxRange, Condition.YCurrMinStep, Condition.YCurrMaxStep,
                            Condition.YCurrMinStroke, Condition.YCurrMaxStroke, Condition.YCurrMode);

                            PassFails[j].Results[(int)SpecItem.OISY_MaxCurrent].Val = MtoM[0]; //Cal.CalMaxCurrent(Cal.CodeY1, Cal.StrokeY, Condition.iYCodeRange, Condition.iYStrokeRange);
                            PassFails[j].Results[(int)SpecItem.OISY_MinCurrent].Val = MtoM[1];
                            //   PassFails[j].Results[(int)SpecItem.OISY_CenteringCurrent].Val = Cal.CalCenterCurrent(Cal.CodeY1, Cal.StrokeY, Condition.iYCodeRange, Condition.iYStrokeRange);
                            //     PassFails[j].Results[(int)SpecItem.OISY_CrosstalkX].Val = Cal.CalCrosstalk(Cal.CodeY1, Cal.StrokeX, Condition.iYStrokeRange, Condition.iYStrokeRange);

                            double[] xtalkRes = Cal.CalCrosstalk(Cal.CodeY1, Cal.StrokeY, Cal.StrokeX, Condition.iYCodeRange, Condition.iYCodeRange, OISCenter);

                            PassFails[j].Results[(int)SpecItem.OISY_CrosstalkX].Val = xtalkRes[0];
                            PassFails[j].Results[(int)SpecItem.OISY_CrosstalkX_dB].Val = xtalkRes[1];
                            PassFails[j].Results[(int)SpecItem.OISY_CrosstalkX_P2P].Val = xtalkRes[2];
                            PassFails[j].Results[(int)SpecItem.OISY_CrosstalkXP2P_dB].Val = xtalkRes[3];

                            //PassFails[j].Results[(int)SpecItem.OISY_CrosstalkZ].Val = Cal.CalCrosstalk(Cal.CodeY1, Cal.StrokeZ, Condition.iYStrokeRange, Condition.iYStrokeRange);
                            //PassFails[j].Results[(int)SpecItem.OISY_CrosstalkR].Val = Cal.CalCrosstalkR(Cal.CodeY1, Cal.StrokeX, Cal.StrokeZ, Condition.iYStrokeRange, Condition.iYStrokeRange);
                            PassFails[j].Results[(int)SpecItem.OISY_Rolling].Val = Cal.CalRolling(Cal.CodeY1, Cal.StrokeY, Condition.iYCodeRange, Condition.iYStrokeRange, OISCenter);

                            SetResult(j, (int)SpecItem.OISY_Ratedstroke, (int)SpecItem.OISY_Rolling);
                            ShowDataResults(j, "Y", (int)SpecItem.OISY_Ratedstroke, (int)SpecItem.OISY_Rolling);
                            SlopeY = Cal.CalSlopeForOISShift(Cal.CodeY1, Cal.StrokeY);

                            PassFails[j].Results[(int)SpecItem.y_HallDecenter].Val = (forword - backword) / 2.0;
                            SetResult(j, (int)SpecItem.y_HallDecenter, (int)SpecItem.y_HallDecenter);
                            ShowDataResults(j, "Hall Decenter", (int)SpecItem.y_HallDecenter, (int)SpecItem.y_HallDecenter);

                        }
                        AddChart(j, name);
                    }
            }
            framCnt[port] = 0;
        }
        private void Process_CalcTimeTest(int port, string name)
        {
            int ch = port * 2;

            for (int j = ch; j < ch + ChannelCnt; j++)
            {
                AddLog(j, string.Format("{0} Driving Data>>", name));
            }
            List<FindResult> result = new List<FindResult>();

            for (int i = 0; i < framCnt[port]; i++)
            {
                result.Add(new FindResult());
                result[i] = STATIC.fVision.MeasureTxTyTz(i, name, true, false);
            }

            for (int j = ch; j < ch + ChannelCnt; j++)
            {
                if (!m_ChannelOn[j]) continue;
                foreach (var Cal in CalList[j])
                    if (Cal.Name == name)
                    {
                        double centerX = 0;
                        double centerY = 0;
                        double centerY1 = 0;
                        double centerY2 = 0;
                        double centerZ = 0;
                        double centertX = 0;
                        double centertY = 0;
                        double centertZ = 0;

                        centerX = result[2].cx[j];
                        centerY = result[2].cy[j];
                        centerZ = result[2].cz[j];
                        centertX = result[2].tx[j];
                        centertY = result[2].ty[j];
                        centertZ = result[2].tz[j];
                        centerY1 = result[2].cy1[j];
                        centerY2 = result[2].cy2[j];


                        for (int i = 0; i < framCnt[port]; i++)
                        {
                            Cal.StrokeX[i] = result[i].cx[j] - centerX;
                            Cal.StrokeX[i] = result[i].cy[j] - centerY;
                            Cal.StrokeZ[i] = result[i].cz[j] - centerZ;
                            Cal.StrokeY1[i] = result[i].cy1[j] - centerY1;
                            Cal.StrokeY2[i] = result[i].cy2[j] - centerY2;
                            Cal.TiltX[i] = result[i].tx[j] - centertX;
                            Cal.TiltY[i] = result[i].ty[j] - centertY;
                            Cal.TiltZ[i] = result[i].tz[j] - centertZ;
                        }
                    }
            }

            if (Option.SaveRawData)
            {
                string str = Convert.ToString(yield.LastSampleNum + 1);
                string dateDir = STATIC.CreateDateDir();
                dateDir += "DrivingData\\";
                if (!Directory.Exists(dateDir))
                    Directory.CreateDirectory(dateDir);

                DateTime dt = DateTime.Now;
                string timeDir = dt.ToString("HHmmss");
                string st = timeDir;

                string lstr = "";
                for (int j = ch; j < ch + ChannelCnt; j++)
                {
                    foreach (var Cal in CalList[j])
                        if (Cal.Name == name)
                        {
                            List<string> arry = new List<string>();
                            arry.Add(DateTime.Now.ToString("MM:dd:hh:mm:ss"));
                            string path = "";
                            switch (name)
                            {
                                case "AF Settling":
                                    arry.Add("i,AF Time,X,Y,Z,TX,TY,TZ,Y1,Y2");
                                    lstr = "";
                                    for (int i = 0; i < framCnt[port]; i++)
                                    {
                                        path = string.Format(dateDir + "{0}_{1}_{2}_{3}.csv", name, m_StrIndex[j], yield.LastSampleNum + 1, st);
                                        string data = string.Format("{0},{1:0.000},{2:0.000},{3:0.000},{4:0.000},{5:0.000},{6:0.000},{7:0.000},{8:0.000},{9:0.000}", i, Cal.Time[i],
                                               Cal.StrokeX[i], Cal.StrokeY[i], Cal.StrokeZ[i], Cal.TiltX[i], Cal.TiltY[i], Cal.TiltZ[i], Cal.StrokeY1[i], Cal.StrokeY2[i]);
                                        arry.Add(data);
                                    }
                                    //AddLog(j, lstr);
                                    break;
                            }
                            if (path != "") STATIC.SetTextLine(path, arry);
                        }
                }
            }

            //  오차 5% , 원래는 조건으로 입력받아야 함

            for (int j = ch; j < ch + ChannelCnt; j++)
            {
                foreach (var Cal in CalList[j])
                    if (Cal.Name == name)
                    {
                        switch (name)
                        {
                            case "AF Settling":
                                double settling = 10;
                                // 여기에 계산  ===================================================
                                double finalZ = 0;
                                double initialZ = 0;
                                for (int i = 1; i < 6; i++)
                                {
                                    finalZ += Cal.StrokeZ[framCnt[port] - i];
                                    initialZ += Cal.StrokeZ[i + 100];
                                }
                                initialZ /= 5;
                                finalZ /= 5;
                                double StepStroke = Math.Abs(finalZ - initialZ);
                                int SettlingIndex = 0;
                                int RisingIndex = 0;
                                for (int i = 1; i < framCnt[port]; i++)
                                {
                                    if (Math.Abs(finalZ - Cal.StrokeZ[framCnt[port] - i]) / StepStroke > Condition.iAFSettlingCriteria)
                                    {
                                        SettlingIndex = framCnt[port] - i + 1;
                                        break;
                                    }
                                }
                                for (int i = 6; i < framCnt[port]; i++)
                                {
                                    if (Math.Abs(initialZ - Cal.StrokeZ[i + 100]) > StepStroke / 50)
                                    {
                                        RisingIndex = i - 1 + 100;
                                        break;
                                    }
                                }
                                settling = Cal.Time[SettlingIndex] - Cal.Time[RisingIndex];  //  msec
                                //===========================================================================
                                PassFails[j].Results[(int)SpecItem.AF_SettillingTime].Val = settling;

                                SetResult(j, (int)SpecItem.AF_SettillingTime, (int)SpecItem.AF_SettillingTime);
                                ShowDataResults(j, "AF", (int)SpecItem.AF_SettillingTime, (int)SpecItem.AF_SettillingTime);
                                break;
                        }
                        AddChart(j, name);
                    }
            }
            framCnt[port] = 0;
        }

        public void AddHeadResult(string sFilePath)
        {
            StreamWriter writer;
            writer = File.AppendText(sFilePath);

            string sHeader;
            sHeader = "Time,Index,PlateBCode,LotID,ACTID,Channel,PM Index,PassFail,1st Fail Item,";

            string sParam = "";
            for (int i = (int)SpecItem.OISX_Ratedstroke; i < (int)SpecItem.Length; i++)
            {
                sParam += string.Format("{0} {1},", Spec.specList[i].Category, Spec.specList[i].DisplayName);
            }
            sHeader += sParam;


            //Time
            sParam = "";
            for (int i = 0; i < ItemList.Count; i++)
            {
                sParam += string.Format("{0} Time ,", ItemList[i].Name);
            }
            sParam += "Total Test Time";

            sHeader += sParam;

            writer.WriteLine(sHeader);

            //"Time,Index,PlateBCode,LotID,ACTID,Channel,PM Index,PassFail,1st Fail Item,";

            sHeader = "uint,,,,,,,,,";

            sParam = "";
            for (int i = (int)SpecItem.OISX_Ratedstroke; i < (int)SpecItem.Length; i++)
            {
                sParam += string.Format("({0}),", Spec.specList[i].Unit);
            }
            sHeader += sParam;

            writer.WriteLine(sHeader);

            sHeader = "Spec Min,,,,,,,,,";
            sParam = "";
            for (int i = (int)SpecItem.OISX_Ratedstroke; i < (int)SpecItem.Length; i++)
            {
                sParam += string.Format("{0},", Spec.specList[i].MinSpec);
            }
            sHeader += sParam;

            writer.WriteLine(sHeader);

            sHeader = "Spec Max,,,,,,,,,";
            sParam = "";
            for (int i = (int)SpecItem.OISX_Ratedstroke; i < (int)SpecItem.Length; i++)
            {
                sParam += string.Format("{0},", Spec.specList[i].MaxSpec);
            }
            sHeader += sParam;

            writer.WriteLine(sHeader);

            writer.Close();
        }
        public void WriteResult(int port)
        {
            string dateDir = STATIC.CreateDateDir();
            if (!Directory.Exists(dateDir))
                Directory.CreateDirectory(dateDir);

            string path = string.Format("{0}res_{1}.csv", dateDir, DateTime.Now.ToString("yyMMdd"));

            if (!File.Exists(path))
            {
                AddHeadResult(path);
            }

            int ch = port * 2;

            StreamWriter sw = File.AppendText(path);

            for (int j = ch; j < ch + ChannelCnt; j++)
            {
                string log = "";
                if (errMsg[j] == "Socket Empty") { yield.TotlaTested--; continue; }

                if (PassFails[j].FirstFailIndex > 0)
                {
                    for (int k = 0; k < ItemList.Count; k++)
                    {
                        if (errMsg[j].Contains(ItemList[k].Name))
                        {
                            PassFails[j].FirstFailIndex = (-(k + 2));
                        }
                    }
                }

                AddLog(j, string.Format("ch : {0}, msg : {1}, PassFail : {2}", j, errMsg[j], PassFails[j].FirstFailIndex));

                //"Time,Index,PlateBCode,LotID,ACTID,Channel,PM Index,PassFail,"
                log += string.Format("'{0},{1},{2},{3},{4},{5},{6},{7},",
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), m_StrIndex[j], "", Model.LotID, "", j, "", PassFails[j].FirstFailIndex);

                yield.TotlaTested++;
                //1st Fail Item
                if (PassFails[j].FirstFailIndex > 0)
                {
                    errMsg[j] = PassFails[j].FirstFail;
                    yield.TotlaFailed++;
                    AddLog(j, "Fail : " + errMsg[j]);
                    log += errMsg[j] + ",";
                }
                else if (PassFails[j].FirstFailIndex < 0)
                {

                    log += errMsg[j] + ",";
                }
                else
                {
                    if (m_ChannelOn[j])
                    {
                        yield.TotlaPassed++;
                        log += "PASS" + ",";
                    }
                    else
                    {
                        log += "NONE" + ",";
                    }
                }

                //  X Results
                for (int i = (int)SpecItem.OISX_Ratedstroke; i < (int)SpecItem.OISY_Ratedstroke; i++)
                {
                    log += string.Format("{0:0.000},", PassFails[j].Results[i].Val);
                }

                //  Y Results
                for (int i = (int)SpecItem.OISY_Ratedstroke; i < (int)SpecItem.AF_Ratedstroke; i++)
                {
                    log += string.Format("{0:0.000},", PassFails[j].Results[i].Val);
                }

                //  AF Results
                for (int i = (int)SpecItem.AF_Ratedstroke; i < (int)SpecItem.FRAX_PMFreq; i++)
                {
                    log += string.Format("{0:0.000},", PassFails[j].Results[i].Val);
                }

                for (int i = (int)SpecItem.FRAX_PMFreq; i < (int)SpecItem.Length; i++)
                {
                    log += string.Format("{0:0.000},", PassFails[j].Results[i].Val);
                }

                //Time
                for (int i = 0; i < ItemList.Count; i++)
                {
                    log += string.Format("{0:0.000},", ItemList[i].Time);
                }

                log += string.Format("{0:0.000},", PassFails[ch].TotalTime);

                sw.WriteLine(log);
            }
            sw.Close();
        }
        private void Act_ScanCode(int port, string testItem)
        {
            LEDs_All_On(port, true);
            Process_ScanCodeTest(port, testItem);
            LEDs_All_On(port, false);
            if (!Option.WriteResultToDriverIC) Process_CalcCodeTest(port, testItem);
        }
        private void Act_ScanTimeCode(int port, string testItem)
        {
            LEDs_All_On(port, true);
            Process_ScanTimeTest(port, testItem);
            LEDs_All_On(port, false);
            if (!Option.WriteResultToDriverIC) Process_CalcTimeTest(port, testItem);
        }
        #endregion

        #region AddSeq
        private void Act_AFOpenLoopAging(int ch, string testItem)
        {
            AFOpenLoopAging(0);
        }
        void Act_AFScanAging(int ch, string testItem)
        {
            AddLog(ch, "<<<  AF Scan aging Start  >>>");
            AddLog(ch, $"Start aging {Condition.AFSCanAgingCount} cycle for AF Driving");
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x40 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x40 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x00 });
            DrvIC.Move(ch, "AF", AFCenter);
            Thread.Sleep(100);

            int curPos = 2047;
            List<int> code = new List<int>();

            do
            {
                code.Add(curPos);
                curPos -= Condition.AFScanAgingStep;
            } while (curPos > Condition.AFScanAgingMin);
            code.Add(Condition.AFScanAgingMin);
            curPos += Condition.AFScanAgingStep;
            do
            {
                code.Add(curPos);
                curPos += Condition.AFScanAgingStep;
            } while (curPos < Condition.AFScanAgingMax);
            code.Add(Condition.AFScanAgingMax);
            curPos -= Condition.AFScanAgingStep;

            do
            {
                code.Add(curPos);
                curPos -= Condition.AFScanAgingStep;
            } while (curPos > 2047);
            code.Add(2047);

            for (int i = 0; i < Condition.AFSCanAgingCount; i++)
            {
                for (int j = 0; j < code.Count; j++)
                {
                    DrvIC.Move(ch, "AF", code[j]);
                    Thread.Sleep(Condition.AFScanAgingDelay);
                }

            }
            AddLog(ch, "<<<  AF Scan aging End  >>>");
        }
        void Act_PreAFDriving(int ch, string testItem)
        {
            LEDs_All_On(0, true);
            AddLog(ch, "AF Pre Driving");
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x40 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x40 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x00 });
            FindResult res = new FindResult();


            int[] code = new int[] { 2048, 1600, 320, 160, 0, 3995, 4075, 4085, 4095 }; //4, 8

            for (int i = 0; i < Condition.AFPReDrvCount; i++)
            {
                double[] MtoM = new double[2];
                for (int j = 0; j < code.Length; j++)
                {
                    DrvIC.Move(ch, "AF", code[j]);
                    Thread.Sleep(Condition.AFPreDrvDelay);
                    if (j == 4)
                    {
                        STATIC.fVision.m__G.oCam[0].GrabA(0);
                        res = STATIC.fVision.MeasureTxTyTz(0, "AF", true);
                        MtoM[0] = res.cz[0];
                    }
                    if (j == 8)
                    {
                        STATIC.fVision.m__G.oCam[0].GrabA(0);
                        res = STATIC.fVision.MeasureTxTyTz(0, "AF", true);
                        MtoM[1] = res.cz[0];
                    }
                }
                AddLog(ch, $"{i + 1} scan stroke : {Math.Abs(MtoM[1] - MtoM[0]).ToString("F3")}");
            }
            LEDs_All_On(0, false);
        }

        private void Act_AFInit(int ch, string testItem)
        {
            byte[] rbuf = new byte[1];


            AddLog(ch, "Setting register setting");
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x40 });
            Thread.Sleep(5);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xAE, new byte[] { 0x3B });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x0B, new byte[] { 0xE2 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x0A, new byte[] { 0x73 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x08, new byte[] { 0x85 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x09, new byte[] { 0x8C });

            AF_EPA_Reset(ch);
            AF_LinearityComp_Reset(ch);

            //PID Update - 나중에 파일로 처리
            AddLog(ch, "PID parameter setting");
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x10, new byte[] { 0x2C });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x11, new byte[] { 0x47 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x12, new byte[] { 0x96 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x13, new byte[] { 0x24 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x14, new byte[] { 0x18 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x15, new byte[] { 0x26 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x16, new byte[] { 0x20 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x17, new byte[] { 0x4B });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x18, new byte[] { 0x14 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x1A, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x1B, new byte[] { 0x6E });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x1C, new byte[] { 0xDC });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x1D, new byte[] { 0xCD });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x1E, new byte[] { 0xCD });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x1F, new byte[] { 0x1F });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x20, new byte[] { 0x11 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x21, new byte[] { 0x10 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x22, new byte[] { 0x0A });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x23, new byte[] { 0x32 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x24, new byte[] { 0xC4 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x25, new byte[] { 0xF5 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x26, new byte[] { 0xCD });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x27, new byte[] { 0xC3 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x28, new byte[] { 0x71 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x29, new byte[] { 0xDF });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x2A, new byte[] { 0x34 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x2B, new byte[] { 0x88 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x2C, new byte[] { 0x8E });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x2D, new byte[] { 0x21 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x2E, new byte[] { 0x3D });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x2F, new byte[] { 0xB5 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xC0, new byte[] { 0x10 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xC1, new byte[] { 0x6E });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xC2, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xC3, new byte[] { 0xBA });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xC4, new byte[] { 0xD0 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xC5, new byte[] { 0x46 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xC6, new byte[] { 0xD7 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xC7, new byte[] { 0x50 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xC8, new byte[] { 0x09 });

            AddLog(ch, "Function register setting");
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xCA, new byte[] { 0x46 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xCB, new byte[] { 0xD8 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xCC, new byte[] { 0x40 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xCD, new byte[] { 0x32 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xCE, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x3D, new byte[] { 0x06 });

            AddLog(ch, "Temp register setting");
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xC9, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x80 });
            Thread.Sleep(50);
            Dln.ReadArray(ch, DrvIC.AFSlaveAddr, 0x70, rbuf);
            AddLog(ch, $"Read 0x70 : 0x{rbuf[0].ToString("X")}");


            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xC9, rbuf);

            AddLog(ch, "Calibration instruction");
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x0C, new byte[] { 0x62 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x18 });
            Thread.Sleep(150);
            Dln.ReadArray(ch, DrvIC.AFSlaveAddr, 0x19, rbuf);
            AddLog(ch, $"Read 0x19 : 0x{rbuf[0].ToString("X")}");

            byte tmpData = (byte)(rbuf[0] * 0.75);
            AddLog(ch, $"CalcData : 0x{tmpData.ToString("X")}");

            if (tmpData >= 0x00 && tmpData <= 0x30)
            {
                Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x19, new byte[] { tmpData });
            }
            else
            {
                PassFails[ch].FirstFailIndex = (int)NonSpecItem.AF_Init;
                m_ChannelOn[ch] = false;
                errMsg[ch] = NonSpecItem.AF_Init.ToString();
                return;
                //Error처리
            }
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xF3, new byte[] { 0x1E });
            Thread.Sleep(30);
            Store(ch, 0);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xAE, new byte[] { 0x00 });
            Dln.PowerOnOff(0, false);
            Thread.Sleep(200);
            Dln.PowerOnOff(0, true);
            Thread.Sleep(100);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x40 });
            CheckData(ch, 0);
        }

        void Act_CloseLoopAging(int ch, string testitem)
        {
            CloseLoopAging(0, Condition.CLAgingMode);
        }
        private void Act_AFEPA(int ch, string testItem)
        {


            LEDs_All_On(0, true);
            FindResult res = new FindResult();


            double Target = Condition.AFEPATarget;
            int InfCut = 10;
            int macCut = 6;
            byte[] rbuf2 = new byte[2];
            byte[] rbuf = new byte[1];
            byte backData = 0;
            double InitPos = 0; double EndPos = 0;

            //move 0 code Position
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x00, new byte[] { 0x80, 0x00 });
            Thread.Sleep(50);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x00, new byte[] { 0x19, 0x00 });
            Thread.Sleep(50);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x00, new byte[] { 0x05, 0x00 });
            Thread.Sleep(50);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x00, new byte[] { 0x02, 0x80 });
            Thread.Sleep(50);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x00, new byte[] { 0x00, 0x00 });
            Thread.Sleep(100);
            //측정하고 값 초기화         
            for (int i = 0; i < 5; i++)
            {
                AddLog(ch, $"af pos(t, c) : {0},{DrvIC.ReadHall(ch, "AF")}");
                Thread.Sleep(50);
            }

            STATIC.fVision.m__G.oCam[0].GrabA(0);
            res = STATIC.fVision.MeasureTxTyTz(0, "AF", true);

            InitPos = res.cz[0];
            int dir = 1;

            int step = 512;
            int pos = step;
            InfCut = (int)(InitPos + 10);
            while (true)
            {
                DrvIC.Move(ch, "AF", pos);
                Thread.Sleep(100);
                STATIC.fVision.m__G.oCam[0].GrabA(0);
                res = STATIC.fVision.MeasureTxTyTz(0, "AF", true);

                AddLog(ch, $"Pos:{(int)(res.cz[0] - InitPos)}, Code:{pos}, Step:{step}");

                if (res.cz[0] > InfCut + 1)
                {
                    if (dir == 1)
                    {
                        dir = 0;
                        step = step / 2;
                        pos = pos - step;
                    }
                    else
                    {
                        dir = 0;
                        pos = pos - step;
                    }

                }
                else if (res.cz[0] < InfCut - 1)
                {
                    if (dir == 1)
                    {
                        dir = 1;
                        pos = pos + step;
                    }
                    else
                    {
                        dir = 1;
                        step = step / 2;
                        pos = pos + step;
                    }

                }
                else { break; }

            }

            int InfPos = pos;
            AddLog(ch, $"Inf Code : {InfPos}");

            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x00, new byte[] { 0x80, 0x00 });
            Thread.Sleep(50);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x00, new byte[] { 0xE6, 0xF0 });
            Thread.Sleep(50);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x00, new byte[] { 0xFA, 0xF0 });
            Thread.Sleep(50);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x00, new byte[] { 0xFD, 0x70 });
            Thread.Sleep(50);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x00, new byte[] { 0xFF, 0xF8 });
            Thread.Sleep(100);
            //측정하고 값 초기화, Measure Stroke 구해서 담음
            double measureStroke = 0;


            Dln.ReadArray(ch, DrvIC.AFSlaveAddr, 0x84, rbuf2); // check AF Current Hall
            for (int i = 0; i < 5; i++)
            {
                AddLog(ch, $"af pos(t, c) : {4095},{DrvIC.ReadHall(ch, "AF")}");
                Thread.Sleep(50);
            }
            STATIC.fVision.m__G.oCam[0].GrabA(0);
            res = STATIC.fVision.MeasureTxTyTz(0, "AF", true);

            EndPos = res.cz[0];
            measureStroke = Math.Abs(EndPos - InitPos);
            AddLog(ch, $"Full Stroke = {measureStroke.ToString("F3")}");
            if (measureStroke - Target - 10 > 6) macCut = (int)(measureStroke - Target - 10);
            AddLog(ch, $"Find macCut = {macCut}");

            dir = 0;
            step = 512;
            pos = 4095 - step;
            macCut = (int)(EndPos - macCut);
            while (true)
            {

                DrvIC.Move(ch, "AF", pos);
                Thread.Sleep(100);
                STATIC.fVision.m__G.oCam[0].GrabA(0);
                res = STATIC.fVision.MeasureTxTyTz(0, "AF", true);

                AddLog(ch, $"Pos:{(int)(res.cz[0] - EndPos)}, Code:{pos}, Step:{step}");
                //측정하고 값 기입
                if (res.cz[0] > macCut + 1)
                {
                    if (dir == 1)
                    {
                        dir = 0;
                        step = step / 2;
                        pos = pos - step;
                    }
                    else
                    {
                        dir = 0;
                        pos = pos - step;
                    }

                }
                else if (res.cz[0] < macCut - 1)
                {
                    if (dir == 1)
                    {
                        dir = 1;
                        pos = pos + step;
                    }
                    else
                    {
                        dir = 1;
                        step = step / 2;
                        pos = pos + step;
                    }

                }
                else { break; }

            }
            int macPos = pos;
            AddLog(ch, $"Mac Code : {macPos}");
            //   Inf, Mac EPA 기입 계산

            byte POSVT = (byte)((4096 - macPos) / 16); byte NEGVT = (byte)(InfPos / 16);

            //   byte POSVT = (byte)((-Condition.AFPOSVT) / 16); byte NEGVT = (byte)(Condition.AFNEGVT / 16);

            //     AddLog(ch, $"POSVT = {Condition.AFPOSVT}, NEGVT = {Condition.AFNEGVT}");
            AddLog(ch, $"0x0E : 0x{POSVT.ToString("X")}, 0x0F : 0x{NEGVT.ToString("X")}");


            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x40 });
            Thread.Sleep(5);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xAE, new byte[] { 0x3B });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x0E, new byte[] { POSVT });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x0F, new byte[] { NEGVT });
            Dln.ReadArray(ch, DrvIC.AFSlaveAddr, 0x0B, rbuf);
            backData = rbuf[0];
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x0B, new byte[] { (byte)(rbuf[0] | 0x80) });//0x0B값 읽어서 백업해야하는지 확인

            DrvIC.Move(ch, "AF", AFCenter);

            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x03, new byte[] { 0x01 });
            Thread.Sleep(100);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x03, new byte[] { 0x10 });
            Thread.Sleep(200);
            Dln.ReadArray(ch, DrvIC.AFSlaveAddr, 0x4B, rbuf);
            if ((byte)(rbuf[0] & 0x04) == 0x00)
            { }
            else
            {
                m__G.m_ChannelOn[ch] = false;
                PassFails[ch].FirstFailIndex = (int)NonSpecItem.AF_EPA;
                errMsg[ch] = NonSpecItem.AF_EPA.ToString();
                return;
            }
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xAE, new byte[] { 0x00 });
        }
        private void Act_OISEPA(int ch, string testItem)
        {
            byte[] rbuf = new byte[1];
            byte backData = 0;

            int Xposvt = -Condition.XPOSVT, Xnegvt = Condition.XNEGVT, Yposvt = -Condition.YPOSVT, Ynegvt = Condition.YNEGVT;
            AddLog(ch, $"X POSVT = {Xposvt}, X NEGVT = {Xnegvt}");
            AddLog(ch, $"Y POSVT = {Yposvt}, Y NEGVT = {Ynegvt}");

            AddLog(ch, $"X = 0x0E : 0x{((Xposvt / 4) >> 2).ToString("X")}, 0x0F : 0x{((Xnegvt / 4) & 0x03).ToString("X")}");
            AddLog(ch, $"Y = 0x0E : 0x{((Yposvt / 4) >> 2).ToString("X")}, 0x0F : 0x{((Ynegvt / 4) & 0x03).ToString("X")}");

            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x40 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x40 });
            Thread.Sleep(5);
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xAE, new byte[] { 0x3B });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0xAE, new byte[] { 0x3B });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x0E, new byte[] { (byte)((Xposvt / 4) >> 2) });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x0F, new byte[] { (byte)((Xnegvt / 4) >> 2) });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x0D, new byte[] { (byte)(((Xposvt / 4) & 0x03 << 2) | ((Xnegvt) & 0x03)) });

            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x0E, new byte[] { (byte)((Yposvt / 4) >> 2) });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x0F, new byte[] { (byte)((Ynegvt / 4) >> 2) });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x0D, new byte[] { (byte)(((Yposvt / 4) & 0x03 << 2) | ((Ynegvt) & 0x03)) });


            Dln.ReadArray(ch, DrvIC.XSlaveAddr, 0x0B, rbuf);
            backData = rbuf[0];
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x0B, new byte[] { (byte)(rbuf[0] | 0X80) });//0x0B값 읽어서 백업해야하는지 확인
            Dln.ReadArray(ch, DrvIC.Y1SlaveAddr, 0x0B, rbuf);
            backData = rbuf[0];
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x0B, new byte[] { (byte)(rbuf[0] | 0X80) });//0x0B값 읽어서 백업해야하는지 확인
            Thread.Sleep(120);

            Store(ch, 1);
            Store(ch, 2);
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xAE, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0xAE, new byte[] { 0x00 });
        }

        void Store(int ch, int Axis)
        {

            AddLog(ch, "Store Start");
            byte[] rbuf = new byte[1];
            if (Axis == 0)
            {
                Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x03, new byte[] { 0x01 });
                Thread.Sleep(100);
                Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x03, new byte[] { 0x02 });
                Thread.Sleep(200);
                Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x03, new byte[] { 0x04 });
                Thread.Sleep(200);
                Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x03, new byte[] { 0x08 });
                Thread.Sleep(100);
                Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x03, new byte[] { 0x10 });
                Thread.Sleep(200);
                Dln.ReadArray(ch, DrvIC.AFSlaveAddr, 0x4B, rbuf);
                if ((byte)(rbuf[0] & 0x04) == 0x00)
                {

                }
                else
                {
                    m__G.m_ChannelOn[ch] = false;
                    PassFails[ch].FirstFailIndex = (int)NonSpecItem.Store_Fail;
                    errMsg[ch] = NonSpecItem.Store_Fail.ToString();
                    AddLog(ch, "Store fail");
                    return;
                }

            }
            else
            {
                int addr = Axis == 1 ? DrvIC.XSlaveAddr : DrvIC.Y1SlaveAddr;

                Dln.WriteArray(ch, addr, 0x03, new byte[] { 0x01 });
                Thread.Sleep(150);
                Dln.WriteArray(ch, addr, 0x03, new byte[] { 0x02 });
                Thread.Sleep(230);
                Dln.WriteArray(ch, addr, 0x03, new byte[] { 0x04 });
                Thread.Sleep(120);
                Dln.WriteArray(ch, addr, 0x03, new byte[] { 0x08 });
                Thread.Sleep(100);
                Dln.WriteArray(ch, addr, 0x03, new byte[] { 0x10 });
                Thread.Sleep(50);
                Dln.ReadArray(ch, addr, 0x4B, rbuf);
                if ((byte)(rbuf[0] & 0x04) == 0x00)
                {

                }
                else
                {
                    m__G.m_ChannelOn[ch] = false;
                    PassFails[ch].FirstFailIndex = (int)NonSpecItem.Store_Fail;
                    errMsg[ch] = NonSpecItem.Store_Fail.ToString();
                    AddLog(ch, "Store fail");
                    return;
                }
            }
            AddLog(ch, "Store finish");
        }
        void AF_EPA_Reset(int ch)
        {
            AddLog(ch, "AF EPA Reset");
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x0E, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x0F, new byte[] { 0x00 });
        }
        void AF_LinearityComp_Reset(int ch)
        {
            AddLog(ch, "AF Linearity Comp Reset");
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x30, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x31, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x32, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x33, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x34, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x35, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x36, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x37, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x38, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x39, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x3A, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x3B, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x3C, new byte[] { 0x00 });
        }

        void OIS_EPA_Reset(int ch)
        {
            AddLog(ch, "OIS EPA Reset");
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x0E, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x0E, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x0F, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x0F, new byte[] { 0x00 });
        }
        void OIS_LinearityComp_Reset(int ch)
        {
            AddLog(ch, "OIS Linearity Comp Reset");
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x2A, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x2B, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x2C, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x2D, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x2E, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x2F, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x30, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x31, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x32, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x33, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x34, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x35, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x36, new byte[] { 0x00 });

            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x2A, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x2B, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x2C, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x2D, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x2E, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x2F, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x30, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x31, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x32, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x33, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x34, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x35, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x36, new byte[] { 0x00 });
        }
        void AFOpenLoopAging(int ch)
        {
            byte[] rbuf = new byte[1];
            byte DataBackup = 0x00;
            int delay = 1000000 / Condition.AFOpenLoopFreq / 2 / 1000;

            //OIS On
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x00, new byte[] { 0x80, 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x00, new byte[] { 0x80, 0x00 });

            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x00 });

            //AF OpenLoop Aging Seq
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x40 });
            Thread.Sleep(5);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xAE, new byte[] { 0x3B });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x1A, new byte[] { 0x00 });
            Dln.ReadArray(ch, DrvIC.AFSlaveAddr, 0x0B, rbuf);
            DataBackup = rbuf[0];
            rbuf[0] = (byte)(rbuf[0] & 0x7F);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xA6, new byte[] { 0x7B });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x00 });

            AddLog(ch, $"OpenLoop Range : {0} - {4095}");
            AddLog(ch, $"OpenLoop Freq : {Condition.AFOpenLoopFreq}");
            AddLog(ch, $"OpenLoop Count : {Condition.AFOpenLoopCount}");
            for (int i = 0; i < Condition.AFOpenLoopCount; i++)
            {
                Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x00, new byte[] { 0xFF, 0xF0 });
                Thread.Sleep(delay);
                Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x00, new byte[] { 0x00, 0x00 });
                Thread.Sleep(delay);
            }

            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x40 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xA6, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x0B, new byte[] { DataBackup });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xAE, new byte[] { 0x00 });
        }
        void CloseLoopAging(int ch, int mode)
        {
            int AFMin = Condition.CLAgingAFMin, AFMax = Condition.CLAgingAFMax, OISMin = Condition.CLAgingOISMin, OISMax = Condition.CLAgingOISMax, count = Condition.CLAgingCount;
            int delay = 1000000 / Condition.CLAgingFreq / 2 / 1000;

            AddLog(ch, $"AF Range : {AFMin} - {AFMax}");
            AddLog(ch, $"OIS Range : {OISMin} - {OISMax}");
            AddLog(ch, $"Aging Count : {count}, Freq : {Condition.CLAgingFreq}");

            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x00 });

            DrvIC.Move(ch, "AF", AFCenter);
            DrvIC.Move(ch, "X", OISCenter);
            DrvIC.Move(ch, "Y", OISCenter);
            Thread.Sleep(100);
            if (mode == 0)
            {
                for (int i = 0; i < count; i++)
                {
                    DrvIC.Move(ch, "AF", AFMin);
                    DrvIC.Move(ch, "X", OISMin);
                    DrvIC.Move(ch, "Y", OISMin);
                    Thread.Sleep(delay);
                    DrvIC.Move(ch, "AF", AFMax);
                    DrvIC.Move(ch, "X", OISMax);
                    DrvIC.Move(ch, "Y", OISMax);
                    Thread.Sleep(delay);
                }
            }
            else
            {
                Random rnd = new Random();
                for (int i = 0; i < count; i++)
                {
                    DrvIC.Move(ch, "AF", AFMin);
                    DrvIC.Move(ch, "X", rnd.Next(OISMin, OISMax));
                    DrvIC.Move(ch, "Y", rnd.Next(OISMin, OISMax));
                    Thread.Sleep(delay);
                    DrvIC.Move(ch, "AF", AFMax);
                    DrvIC.Move(ch, "X", rnd.Next(OISMin, OISMax));
                    DrvIC.Move(ch, "Y", rnd.Next(OISMin, OISMax));
                    Thread.Sleep(delay);
                }
            }


            DrvIC.Move(ch, "AF", AFCenter);
            DrvIC.Move(ch, "X", OISCenter);
            DrvIC.Move(ch, "Y", OISCenter);

            //   Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x40 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x40 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x40 });

        }
        void CheckData(int ch, int axis)
        {
            byte[] data = new byte[256];
            byte[] rbuf = new byte[1];
            int addr = 0x00;
            string s = string.Empty;
            switch (axis)
            {
                case 0:
                    addr = DrvIC.AFSlaveAddr;
                    break;
                case 1:
                    addr = DrvIC.XSlaveAddr;
                    break;
                case 2:
                    addr = DrvIC.Y1SlaveAddr;
                    break;
            }
            for (int i = 0; i < 256; i++)
            {
                Dln.ReadArray(ch, addr, 0x00 + i, rbuf);
                data[i] = rbuf[0];

            }
            for (int i = 0; i < 16; i++)
            {
                s += $"0x{(16 * i).ToString("X2")}~0x{(16 * i + 15).ToString("X2")} : " +
                     $"{data[16 * i].ToString("X2")}{data[16 * i + 1].ToString("X2")}{data[16 * i + 2].ToString("X2")}{data[16 * i + 3].ToString("X2")}  " +
                     $"{data[16 * i + 4].ToString("X2")}{data[16 * i + 5].ToString("X2")}{data[16 * i + 6].ToString("X2")}{data[16 * i + 7].ToString("X2")}  " +
                     $"{data[16 * i + 8].ToString("X2")}{data[16 * i + 9].ToString("X2")}{data[16 * i + 10].ToString("X2")}{data[16 * i + 11].ToString("X2")}  " +
                     $"{data[16 * i + 12].ToString("X2")}{data[16 * i + 13].ToString("X2")}{data[16 * i + 14].ToString("X2")}{data[16 * i + 15].ToString("X2")}\r\n";
            }

            AddLog(ch, s);

        }
        void Act_OISLinComp(int ch, string testitem)
        {
            int addr = testitem.Contains("X") ? DrvIC.XSlaveAddr : DrvIC.Y1SlaveAddr;
            string Axis = testitem.Contains("X") ? "X" : "Y";
            int axisint = testitem.Contains("X") ? 1 : 2;

            int start = 0, end = 0, step = 0, delay = 0;
            List<float> target = new List<float>();
            List<float> data = new List<float>();
            List<float> ReadHall = new List<float>();
            float RefData = 0;


            if (Axis == "X") { start = Condition.XLinCompStart; end = Condition.XLinCompEnd; step = Condition.XLinCompStep; delay = Condition.XLinCompMoveDelay; }
            else { start = Condition.YLinCompStart; end = Condition.YLinCompEnd; step = Condition.YLinCompStep; delay = Condition.YLinCompMoveDelay; }



            LEDs_All_On(0, true);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x00 });
            DrvIC.Move(ch, "AF", BestAFPos);
            AddLog(ch, $"Move AF Best Position : {BestAFPos}");

            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x00 });
            DrvIC.Move(ch, "X", OISCenter);
            DrvIC.Move(ch, "Y", OISCenter);
            Thread.Sleep(100);

            FindResult tmpres = new FindResult();

            byte pvt = 0, nvt = 0;
            byte[] rbuf = new byte[1];

            Dln.ReadArray(ch, addr, 0x0E, rbuf);
            pvt = rbuf[0];
            Dln.ReadArray(ch, addr, 0x0F, rbuf);
            nvt = rbuf[0];

            AddLog(ch, $"POSVT = {pvt}, NEGVT = {nvt}");


            Dln.WriteArray(ch, addr, 0x02, new byte[] { 0x00 });
            DrvIC.Move(ch, Axis, start);
            int index = 0;
            AddLog(ch, $"Target\tPos\tReadHall");
            while (true)
            {
                int currCode = start + (index * step);
                if (currCode > end)
                    currCode = end;
                STATIC.DrvIC.Move(0, Axis, currCode);
                Thread.Sleep(delay);
                STATIC.fVision.m__G.oCam[0].GrabA(0);
                tmpres = STATIC.fVision.MeasureTxTyTz(0, Axis, true);
                target.Add(currCode);
                ReadHall.Add(DrvIC.ReadHall(ch, Axis));
                if (Axis == "X")
                {
                    if (index != 0)
                        data.Add((float)tmpres.cx[0] - RefData);
                    else { data.Add(0); RefData = (float)tmpres.cx[0]; }
                }
                else
                {
                    if (index != 0)
                        data.Add((float)tmpres.cy[0] - RefData);
                    else { data.Add(0); RefData = (float)tmpres.cy[0]; }
                }

                AddLog(ch, $"{target[index]}\t{data[index].ToString("F2")}\t{ReadHall[index]}");
                if (currCode >= end) break;
                index++;
            }


            DrvIC.Move(ch, Axis, OISCenter);
            OISLinCompCoef coef = new OISLinCompCoef();
            int[] lincoef = new int[OISLinCompCoef.NUM_COEF];
            float resError = 0;
            int res = coef.LinCompMain(target.ToArray(), data.ToArray(), data.Count, pvt, nvt, 0, 0, ref lincoef, ref resError);
            if (res != 0)
            {
                AddLog(ch, $"Linearity Comp Fail");
                m__G.m_ChannelOn[ch] = false;
                if (Axis == "X")
                {
                    PassFails[ch].FirstFailIndex = (int)NonSpecItem.X_LinearityComp;
                    errMsg[ch] = NonSpecItem.X_LinearityComp.ToString();
                }
                else
                {
                    PassFails[ch].FirstFailIndex = (int)NonSpecItem.Y_LinearityComp;
                    errMsg[ch] = NonSpecItem.Y_LinearityComp.ToString();
                }

            }


            Dln.WriteArray(ch, addr, 0xAE, new byte[] { 0x3B });
            Dln.WriteArray(ch, addr, 0x2A, new byte[] { (byte)lincoef[0] });
            Dln.WriteArray(ch, addr, 0x2B, new byte[] { (byte)lincoef[1] });
            Dln.WriteArray(ch, addr, 0x2C, new byte[] { (byte)lincoef[2] });
            Dln.WriteArray(ch, addr, 0x2D, new byte[] { (byte)lincoef[3] });
            Dln.WriteArray(ch, addr, 0x2E, new byte[] { (byte)lincoef[4] });
            Dln.WriteArray(ch, addr, 0x2F, new byte[] { (byte)lincoef[5] });
            Dln.WriteArray(ch, addr, 0x30, new byte[] { (byte)lincoef[6] });
            Dln.WriteArray(ch, addr, 0x31, new byte[] { (byte)lincoef[7] });
            Dln.WriteArray(ch, addr, 0x32, new byte[] { (byte)lincoef[8] });
            Dln.WriteArray(ch, addr, 0x33, new byte[] { (byte)lincoef[9] });
            Dln.WriteArray(ch, addr, 0x34, new byte[] { (byte)lincoef[10] });
            Dln.WriteArray(ch, addr, 0x35, new byte[] { (byte)lincoef[11] });
            Dln.WriteArray(ch, addr, 0x36, new byte[] { (byte)lincoef[12] });

            string s = $"0x2A : 0x{lincoef[0].ToString("X")}, 0x2B : 0x{lincoef[1].ToString("X")}, 0x2C : 0x{lincoef[2].ToString("X")}, 0x2D : 0x{lincoef[3].ToString("X")}, 0x2E : 0x{lincoef[4].ToString("X")}\r\n" +
             $"0x2F : 0x{lincoef[5].ToString("X")}, 0x30 : 0x{lincoef[6].ToString("X")}, 0x31 : 0x{lincoef[7].ToString("X")}, 0x32 : 0x{lincoef[8].ToString("X")}, 0x33 : 0x{lincoef[9].ToString("X")}\r\n" +
             $"0x34 : 0x{lincoef[10].ToString("X")}, 0x35 : 0x{lincoef[11].ToString("X")}, 0x36 : 0x{lincoef[12].ToString("X")}";

            AddLog(ch, s);

            Store(ch, axisint);
            Dln.WriteArray(ch, addr, 0xAE, new byte[] { 0x00 });
            LEDs_All_On(0, false);
        }
        void Act_AFLinComp(int ch, string testitem)
        {
            int start = Condition.AfLinCompStart, end = Condition.AfLinCompEnd, step = Condition.AFLinCompStep, delay = Condition.AFLinCompMoveDelay;
            LEDs_All_On(0, true);
            FindResult tmpres = new FindResult();

            List<float> target = new List<float>();
            List<float> data = new List<float>();
            List<float> ReadHall = new List<float>();
            float RefData = 0;
            byte[] rbuf = new byte[1];
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x40 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x40 });
            Thread.Sleep(10);
            Dln.ReadArray(ch, DrvIC.AFSlaveAddr, 0x0E, rbuf);
            byte pvt = rbuf[0];
            Dln.ReadArray(ch, DrvIC.AFSlaveAddr, 0x0F, rbuf);
            byte nvt = rbuf[0];

            AddLog(ch, $"POSVT = {pvt}, NEGVT = {nvt}");

            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x00 });
            DrvIC.Move(ch, "AF", start);
            int index = 0;
            AddLog(ch, $"Target\tPos\tReadHall");
            while (true)
            {
                int currCode = start + (index * step);
                if (currCode > end)
                    currCode = end;
                STATIC.DrvIC.Move(0, "AF", currCode);
                Thread.Sleep(delay);
                STATIC.fVision.m__G.oCam[0].GrabA(0);
                tmpres = STATIC.fVision.MeasureTxTyTz(0, "AF", true);
                target.Add(currCode);
                ReadHall.Add(DrvIC.ReadHall(ch, "AF"));
                if (index != 0)
                    data.Add((float)tmpres.cz[0] - RefData);
                else { data.Add(0); RefData = (float)tmpres.cz[0]; }

                AddLog(ch, $"{target[index]}\t{data[index].ToString("F2")}\t{ReadHall[index]}");
                if (currCode >= end) break;
                index++;
            }

            DrvIC.Move(ch, "AF", AFCenter);
            AFLinCompCoef coef = new AFLinCompCoef();
            int[] lincoef = new int[AFLinCompCoef.NUM_COEF];
            float resError = 0;
            int res = coef.LinCompMain(target.ToArray(), data.ToArray(), data.Count, pvt, nvt, 0, 0, ref lincoef, ref resError);

            if (res != 0)
            {
                AddLog(ch, $"Linearity Comp Fail");
                m__G.m_ChannelOn[ch] = false;
                PassFails[ch].FirstFailIndex = (int)NonSpecItem.AF_LinearityComp;
                errMsg[ch] = NonSpecItem.AF_LinearityComp.ToString();
            }


            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xAE, new byte[] { 0x3B });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x30, new byte[] { (byte)lincoef[0] });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x31, new byte[] { (byte)lincoef[1] });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x32, new byte[] { (byte)lincoef[2] });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x33, new byte[] { (byte)lincoef[3] });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x34, new byte[] { (byte)lincoef[4] });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x35, new byte[] { (byte)lincoef[5] });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x36, new byte[] { (byte)lincoef[6] });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x37, new byte[] { (byte)lincoef[7] });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x38, new byte[] { (byte)lincoef[8] });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x39, new byte[] { (byte)lincoef[9] });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x3A, new byte[] { (byte)lincoef[10] });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x3B, new byte[] { (byte)lincoef[11] });
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x3C, new byte[] { (byte)lincoef[12] });

            string s = $"0x30 : 0x{lincoef[0].ToString("X")}, 0x31 : 0x{lincoef[1].ToString("X")}, 0x32 : 0x{lincoef[2].ToString("X")}, 0x33 : 0x{lincoef[3].ToString("X")}, 0x34 : 0x{lincoef[4].ToString("X")}\r\n" +
                       $"0x35 : 0x{lincoef[5].ToString("X")}, 0x36 : 0x{lincoef[6].ToString("X")}, 0x37 : 0x{lincoef[7].ToString("X")}, 0x38 : 0x{lincoef[8].ToString("X")}, 0x39 : 0x{lincoef[9].ToString("X")}\r\n" +
                       $"0x3A : 0x{lincoef[10].ToString("X")}, 0x3B : 0x{lincoef[11].ToString("X")}, 0x3C : 0x{lincoef[12].ToString("X")}";

            AddLog(ch, s);
            Store(ch, 0);
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0xAE, new byte[] { 0x00 });

            LEDs_All_On(0, false);
        }
        void Act_FindBestAFPosition(int ch, string testitem)
        {

            int[] step = new int[9] { 0, 511, 1023, 1535, 2047, 2559, 3071, 3585, 4095 };
            int[] hallX = new int[9];
            int[] hallY = new int[9];

            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x00 });
            DrvIC.Move(ch, "AF", 200);
            Thread.Sleep(50);
            DrvIC.Move(ch, "AF", 0);

            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xAE, new byte[] { 0x3B });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0xAE, new byte[] { 0x3B });

            //중간 셋팅값 확인 

            //
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x40 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x40 });

            Thread.Sleep(100);

            for (int i = 0; i < 9; i++)
            {
                int[] tmphallX = new int[6];
                int[] tmphallY = new int[6];
                DrvIC.Move(ch, "AF", step[i]);
                Thread.Sleep(100);
                for (int j = 0; j < 6; j++)
                {
                    tmphallX[j] = DrvIC.ReadHall(ch, "X");
                    tmphallY[j] = DrvIC.ReadHall(ch, "Y");
                    hallX[i] += tmphallX[j];
                    hallY[i] += tmphallY[j];
                }
                hallX[i] /= 6;
                hallY[i] /= 6;

                AddLog(ch, $"Pos = {step[i]}, DataX[{i}] = {hallX[i]}, DataY[{i}] = {hallY[i]}");
            }
            int xMin = hallX.Min(); int xMax = hallX.Max();
            int yMin = hallY.Min(); int yMax = hallY.Max();
            int xCenter = (xMin + xMax) / 2;
            int yCenter = (yMin + yMax) / 2;
            int xMinIndex = 0; int yMinIndex = 0;
            int xMaxIndex = 0; int yMaxIndex = 0;
            bool XMinFind = false; bool YMinFind = false;
            bool XMaxFind = false; bool YMaxFind = false;
            int xBestPos = 0; int yBestPos = 0;
            for (int i = 0; i < 9; i++)
            {
                if (xMin == hallX[i] && !XMinFind) { XMinFind = true; xMinIndex = i; }
                if (xMax == hallX[i] && !XMaxFind) { XMaxFind = true; xMaxIndex = i; }
                if (yMin == hallY[i] && !YMinFind) { YMinFind = true; yMinIndex = i; }
                if (yMax == hallY[i] && !YMaxFind) { YMaxFind = true; yMaxIndex = i; }
            }
            int startXIndex = 0; int endXIndex = 0; int startYIndex = 0; int endYIndex = 0;
            if (xMinIndex > xMaxIndex)
            {
                startXIndex = xMaxIndex;
                endXIndex = xMinIndex;
            }
            else
            {
                startXIndex = xMinIndex;
                endXIndex = xMaxIndex;
            }
            if (yMinIndex > yMaxIndex)
            {
                startYIndex = yMaxIndex;
                endYIndex = yMinIndex;
            }
            else
            {
                startYIndex = yMinIndex;
                endYIndex = yMaxIndex;
            }
            string s = $"[MAX/MIN Index] 0, start:{startXIndex}, end:{endXIndex}\r\n" +
                       $"[MAX/MIN Index] 1, start:{startYIndex}, end:{endYIndex}\r\n" +
                       $"X Min : {xMin}, X Max : {xMax} ({xMax - xMin})\r\n" +
                       $"Y Min : {yMin}, Y Max : {yMax} ({yMax - yMin})\r\n" +
                       $"X Center :{xCenter}, Y Center : {yCenter}\r\n";
            AddLog(ch, s);

            for (int i = startXIndex; i <= endXIndex; i++)
            {
                if (i == 0) continue;
                if (hallX[i - 1] <= xCenter && hallX[i] >= xCenter || hallX[i - 1] >= xCenter && hallX[i] <= xCenter)
                {

                    xBestPos = (int)(step[i - 1] + (step[i] - step[i - 1]) * (xCenter - hallX[i - 1]) / (hallX[i] - hallX[i - 1]));


                    break;
                }
            }
            for (int i = startYIndex; i <= endYIndex; i++)
            {
                if (i == 0) continue;
                if (hallY[i - 1] <= yCenter && hallY[i] >= yCenter || hallY[i - 1] >= yCenter && hallY[i] <= yCenter)
                {
                    yBestPos = (int)(step[i - 1] + (step[i] - step[i - 1]) * (yCenter - hallY[i - 1]) / (hallY[i] - hallY[i - 1]));

                    break;
                }
            }
            AddLog(ch, $"X_AF : {xBestPos}, Y_AF : {yBestPos}");
            if (xMax - xMin > yMax - yMin)
                BestAFPos = xBestPos;
            else BestAFPos = yBestPos;
            AddLog(ch, $"Chosen Best AF : {BestAFPos}");
        }

        void Act_OISInit(int ch, string testitem)
        {

            #region PID

            List<byte[]> PID = new List<byte[]>();
            PID.Add(new byte[3] { 0x10, 0x55, 0x50 });
            PID.Add(new byte[3] { 0x11, 0x2D, 0x2D });
            PID.Add(new byte[3] { 0x12, 0xFA, 0xFA });
            PID.Add(new byte[3] { 0x13, 0x19, 0x19 });
            PID.Add(new byte[3] { 0x14, 0x1E, 0x1E });
            PID.Add(new byte[3] { 0x15, 0x50, 0x50 });
            PID.Add(new byte[3] { 0x16, 0x25, 0x25 });
            PID.Add(new byte[3] { 0x17, 0x6E, 0x6E });
            PID.Add(new byte[3] { 0x18, 0xF3, 0xF4 });
            PID.Add(new byte[3] { 0x1A, 0xC2, 0xC3 });
            PID.Add(new byte[3] { 0x1B, 0xA0, 0xEE });
            PID.Add(new byte[3] { 0x1C, 0x7D, 0x7C });
            PID.Add(new byte[3] { 0x1D, 0x5C, 0x0B });
            PID.Add(new byte[3] { 0x1E, 0x39, 0x3D });
            PID.Add(new byte[3] { 0x1F, 0x9B, 0x00 });
            PID.Add(new byte[3] { 0x20, 0x8B, 0x83 });
            PID.Add(new byte[3] { 0x21, 0x8A, 0xF5 });
            PID.Add(new byte[3] { 0x22, 0x3A, 0x3F });
            PID.Add(new byte[3] { 0x23, 0xDF, 0x12 });
            PID.Add(new byte[3] { 0x27, 0x92, 0x92 });
            PID.Add(new byte[3] { 0x28, 0x92, 0x92 });
            PID.Add(new byte[3] { 0x29, 0x18, 0x18 });
            PID.Add(new byte[3] { 0x2A, 0x00, 0x00 });
            PID.Add(new byte[3] { 0x2B, 0x00, 0x00 });
            PID.Add(new byte[3] { 0x2C, 0x00, 0x00 });
            PID.Add(new byte[3] { 0x2D, 0x00, 0x00 });
            PID.Add(new byte[3] { 0x2E, 0x00, 0x00 });
            PID.Add(new byte[3] { 0x2F, 0x00, 0x00 });
            PID.Add(new byte[3] { 0x30, 0x00, 0x00 });
            PID.Add(new byte[3] { 0x31, 0x00, 0x00 });
            PID.Add(new byte[3] { 0x32, 0x00, 0x00 });
            PID.Add(new byte[3] { 0x33, 0x00, 0x00 });
            PID.Add(new byte[3] { 0x34, 0x00, 0x00 });
            PID.Add(new byte[3] { 0x35, 0x00, 0x00 });
            PID.Add(new byte[3] { 0x36, 0x00, 0x00 });
            PID.Add(new byte[3] { 0x37, 0xFC, 0x04 });
            PID.Add(new byte[3] { 0x50, 0xEF, 0xEF });
            PID.Add(new byte[3] { 0x51, 0xFF, 0xFF });
            PID.Add(new byte[3] { 0x52, 0x40, 0x40 });
            PID.Add(new byte[3] { 0x53, 0x28, 0x1E });
            PID.Add(new byte[3] { 0x54, 0x01, 0x01 });
            PID.Add(new byte[3] { 0x55, 0x78, 0x50 });
            PID.Add(new byte[3] { 0x56, 0x7D, 0x8C });
            PID.Add(new byte[3] { 0x57, 0xFA, 0xFA });
            PID.Add(new byte[3] { 0x58, 0xFA, 0xFA });
            PID.Add(new byte[3] { 0x59, 0x2D, 0x2D });
            PID.Add(new byte[3] { 0x5A, 0x50, 0x3C });
            PID.Add(new byte[3] { 0x5B, 0xFF, 0xFF });
            PID.Add(new byte[3] { 0x5C, 0x32, 0x32 });

            #endregion


            byte[] rbuf = new byte[2];
            Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x00 });

            //Set I2C Volt = 1.8V
            DrvIC.Move(ch, "AF", BestAFPos);
            AddLog(ch, $"Move AF Best Position : {BestAFPos}");
            Thread.Sleep(100);

            AddLog(ch, $"X/Y Setting register setting");
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xAE, new byte[] { 0x3B });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x0b, new byte[] { 0x02 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x0A, new byte[] { 0x59 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x08, new byte[] { 0x08 });


            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0xAE, new byte[] { 0x3B });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x0b, new byte[] { 0x04 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x0A, new byte[] { 0x59 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x08, new byte[] { 0x08 });

            //set i2c volt = 1.2V
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x0B, new byte[] { 0x12 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x0B, new byte[] { 0x14 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x0A, new byte[] { 0x59 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x0A, new byte[] { 0x59 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x0C, new byte[] { 0x62 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x0C, new byte[] { 0x62 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x08, new byte[] { 0x09 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x08, new byte[] { 0x09 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x09, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x09, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x24, new byte[] { 0x6C });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x24, new byte[] { 0x6C });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x25, new byte[] { 0x2F });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x25, new byte[] { 0x2F });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x5D, new byte[] { 0x60 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x5D, new byte[] { 0x60 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x5E, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x5E, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x5F, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x5F, new byte[] { 0x04 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x60, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x60, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x61, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x61, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x6B, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x6B, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x6C, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x6C, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x6D, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x6D, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x6E, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x6E, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x6F, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x6F, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xD8, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0xD8, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xD9, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0xD9, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xDA, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0xDA, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xDB, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0xDB, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xDC, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0xDC, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xDD, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0xDD, new byte[] { 0x00 });

            AddLog(ch, $"X/Y Register initial setting");
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x0D, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x0D, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x0E, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x0E, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x0F, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x0F, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x3E, new byte[] { 0x85 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x3E, new byte[] { 0x85 });
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xFE, new byte[] { 0x0A });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0xFE, new byte[] { 0x0A });
            Thread.Sleep(30);
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xFF, new byte[] { 0x33 });
            Thread.Sleep(30);


            OIS_EPA_Reset(ch);
            OIS_LinearityComp_Reset(ch);


            AddLog(ch, $"X/Y PID parameter setting");
            for (int i = 0; i < PID.Count; i++)
            {
                Dln.WriteArray(ch, DrvIC.XSlaveAddr, PID[i][0], new byte[] { PID[i][1] });
                Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, PID[i][0], new byte[] { PID[i][2] });
            }

            AddLog(ch, $"X/Y Calibration instruction");
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x09 });
            Thread.Sleep(150);
            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x19, new byte[] { 0x88 });
            Dln.ReadArray(ch, DrvIC.XSlaveAddr, 0x04, rbuf);
            Dln.ReadArray(ch, DrvIC.XSlaveAddr, 0x06, rbuf);

            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x09 });
            Thread.Sleep(150);
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x19, new byte[] { 0x88 });
            Dln.ReadArray(ch, DrvIC.Y1SlaveAddr, 0x04, rbuf);
            Dln.ReadArray(ch, DrvIC.Y1SlaveAddr, 0x06, rbuf);

            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x5D, new byte[] { 0x68 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x5D, new byte[] { 0x68 });
            Store(ch, 1);
            Store(ch, 2);

            Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0xAE, new byte[] { 0x00 });
            Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0xAE, new byte[] { 0x00 });
            Dln.PowerOnOff(0, false);
            Thread.Sleep(200);
            Dln.PowerOnOff(0, true);
            Thread.Sleep(100);
            CheckData(ch, 1);
            CheckData(ch, 2);
        }

        private void Act_GaindB10Hz(int ch, string testItem)
        {
            int amp;

            if (!Dln.WriteArray(ch, DrvIC.XSlaveAddr, 1, 0x02, new byte[] { 0x40 })) return;
            if (!Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 1, 0x02, new byte[] { 0x40 })) return;

            //X
            amp = (int)Condition.iLoppgainXAmp;
            AddLog(ch, string.Format("X FRA =="));

            List<double> freq = new List<double>();
            List<double> gain = new List<double>();
            List<double> phase = new List<double>();
            freq.Add(10);

            if (!DrvIC.FRA_Single(ch, "X", amp, 2, freq, ref gain, ref phase))
            {
                errMsg[ch] = string.Format("{0} Error", testItem);
                m_ChannelOn[ch] = false;
            }
            else
            {
                AddLog(ch, string.Format("FRA X Gain10Hz = {0:0.000}",
                    PassFails[ch].Results[(int)SpecItem.FRAX_Gain10Hz].Val = gain[0]));

                SetResult(ch, (int)SpecItem.FRAX_Gain10Hz, (int)SpecItem.FRAX_Gain10Hz);
                ShowDataResults(ch, "FRA X", (int)SpecItem.FRAX_Gain10Hz, (int)SpecItem.FRAX_Gain10Hz);
            }
            //Y1
            amp = (int)Condition.iLoppgainYAmp;
            AddLog(ch, string.Format("Y1 FRA =="));

            freq = new List<double>();
            gain = new List<double>();
            phase = new List<double>();
            freq.Add(10);

            if (!DrvIC.FRA_Single(ch, "Y1", amp, 2, freq, ref gain, ref phase))
            {
                errMsg[ch] = string.Format("{0} Error", testItem);
                m_ChannelOn[ch] = false;
            }
            else
            {
                AddLog(ch, string.Format("FRA Y1 Gain10Hz = {0:0.000}",
                PassFails[ch].Results[(int)SpecItem.FRAY1_Gain10Hz].Val = gain[0]));

                SetResult(ch, (int)SpecItem.FRAY1_Gain10Hz, (int)SpecItem.FRAY1_Gain10Hz);
                ShowDataResults(ch, "FRA Y1", (int)SpecItem.FRAY1_Gain10Hz, (int)SpecItem.FRAY1_Gain10Hz);
            }
          //  Y2
            amp = (int)Condition.iLoppgainYAmp;
            AddLog(ch, string.Format("Y2 FRA =="));

            freq = new List<double>();
            gain = new List<double>();
            phase = new List<double>();
            freq.Add(10);

            if (!DrvIC.FRA_Single(ch, "Y2", amp, 2, freq, ref gain, ref phase))
            {
                errMsg[ch] = string.Format("{0} Error", testItem);
                m_ChannelOn[ch] = false;
            }
            else
            {
                AddLog(ch, string.Format("FRA Y2 Gain10Hz = {0:0.000}",
                PassFails[ch].Results[(int)SpecItem.FRAY2_Gain10Hz].Val = gain[0]));

                SetResult(ch, (int)SpecItem.FRAY2_Gain10Hz, (int)SpecItem.FRAY2_Gain10Hz);
                ShowDataResults(ch, "FRA Y2", (int)SpecItem.FRAY2_Gain10Hz, (int)SpecItem.FRAY2_Gain10Hz);
            }
        }
        //private void Act_Phase_Margin(int ch, string testItem)
        //{

        //    if (!Dln.WriteArray(ch, DrvIC.XSlaveAddr, 1, 0x02, new byte[] { 0x40 })) return;
        //    if (!Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 1, 0x02, new byte[] { 0x40 })) return;
        //    if (!Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 1, 0x02, new byte[] { 0x40 })) return;
        //    string axis;
        //    int startFreq;
        //    int EndFreq;
        //    int amp;

        //    int phaseIndex = 0;

        //    List<double> freq = new List<double>();
        //    List<double> gain = new List<double>();
        //    List<double> phase = new List<double>();

        //    //DrvIC.Move(ch, "AF", 2045);

        //    #region X PM
        //    axis = "X";
        //    startFreq = Condition.iXChirpFrom;
        //    EndFreq = Condition.iXChirpTo;
        //    amp = (int)Condition.iXAmplitude;

        //    AddLog(ch, string.Format("{0} FRA ==", axis));

        //    freq = new List<double>();
        //    gain = new List<double>();
        //    phase = new List<double>();

        //    for (int i = 0; i < Condition.iFRAloop; i++)
        //    {
        //        while (true)
        //        {
        //            freq.Add(startFreq);
        //            startFreq -= Condition.iFRAstep;
        //            if (startFreq < EndFreq) break;
        //        }
        //    }

        //    if (!DrvIC.FRA_Single(ch, axis, amp, 0, freq, ref gain, ref phase))
        //    {
        //        errMsg[ch] = string.Format("{0} Error", testItem);
        //        m_ChannelOn[ch] = false;
        //    }

        //    phaseIndex = FindPhaseIndex(gain);
        //    if (phaseIndex < 1)
        //    {
        //        AddLog(ch, "X Find Phase Margin Failed.. Freq Range Check Please.");
        //        errMsg[ch] = string.Format("{0} Error", testItem);
        //        m_ChannelOn[ch] = false;
        //    }
        //    else
        //    {
        //        double phaseRes = 0, freqRes = 0;
        //        phaseRes = ((gain[phaseIndex + 1] * phase[phaseIndex]) - (gain[phaseIndex] * phase[phaseIndex + 1])) / (gain[phaseIndex + 1] - gain[phaseIndex]);
        //        freqRes = (int)(((gain[phaseIndex + 1] * freq[phaseIndex]) - (gain[phaseIndex] * freq[phaseIndex + 1])) / (gain[phaseIndex + 1] - gain[phaseIndex]));

        //        AddLog(ch, string.Format("FRA X Freq = {0} PM = {1}",
        //        PassFails[ch].Results[(int)SpecItem.FRAX_PMFreq].Val = freqRes, PassFails[ch].Results[(int)SpecItem.FRAX_PhaseMargin].Val = phaseRes));

        //        SetResult(ch, (int)SpecItem.FRAX_PMFreq, (int)SpecItem.FRAX_PhaseMargin);
        //        ShowDataResults(ch, "FRA X", (int)SpecItem.FRAX_PMFreq, (int)SpecItem.FRAX_PhaseMargin);
        //    }
        //    #endregion
        //    #region Y PM
        //    //Y1
        //    axis = "Y1";
        //    startFreq = Condition.iYChirpFrom;
        //    EndFreq = Condition.iYChirpTo;
        //    amp = (int)Condition.iYAmplitude;

        //    AddLog(ch, string.Format("{0} FRA ==", axis));

        //    freq = new List<double>();
        //    gain = new List<double>();
        //    phase = new List<double>();

        //    for (int i = 0; i < Condition.iFRAloop; i++)
        //    {
        //        while (true)
        //        {
        //            freq.Add(startFreq);
        //            startFreq -= Condition.iFRAstep;
        //            if (startFreq < EndFreq) break;

        //        }
        //    }

        //    if (!DrvIC.FRA_Single(ch, axis, amp, 0, freq, ref gain, ref phase))
        //    {
        //        errMsg[ch] = string.Format("{0} Error", testItem);
        //        m_ChannelOn[ch] = false;
        //    }

        //    phaseIndex = FindPhaseIndex(gain);
        //    if (phaseIndex < 1)
        //    {
        //        AddLog(ch, "Y1 Find Phase Margin Failed.. Freq Range Check Please.");
        //        errMsg[ch] = string.Format("{0} Error", testItem);
        //        m_ChannelOn[ch] = false;
        //    }
        //    else
        //    {
        //        double phaseRes = 0, freqRes = 0;
        //        phaseRes = ((gain[phaseIndex + 1] * phase[phaseIndex]) - (gain[phaseIndex] * phase[phaseIndex + 1])) / (gain[phaseIndex + 1] - gain[phaseIndex]);
        //        freqRes = (int)(((gain[phaseIndex + 1] * freq[phaseIndex]) - (gain[phaseIndex] * freq[phaseIndex + 1])) / (gain[phaseIndex + 1] - gain[phaseIndex]));


        //        AddLog(ch, string.Format("FRA Y1 Freq = {0} PM = {1}",
        //        PassFails[ch].Results[(int)SpecItem.FRAY1_PMFreq].Val = freqRes, PassFails[ch].Results[(int)SpecItem.FRAY1_PhaseMargin].Val = phaseRes));

        //        SetResult(ch, (int)SpecItem.FRAY1_PMFreq, (int)SpecItem.FRAY1_PhaseMargin);
        //        ShowDataResults(ch, "FRA Y1", (int)SpecItem.FRAY1_PMFreq, (int)SpecItem.FRAY1_PhaseMargin);
        //    }
        //    #endregion
        //    #region Y2 PM
        //    //Y2
        //    //axis = "Y2";
        //    //startFreq = Condition.iYChirpFrom;
        //    //EndFreq = Condition.iYChirpTo;
        //    //amp = (int)Condition.iYAmplitude;

        //    //AddLog(ch, string.Format("{0} FRA ==", axis));

        //    //freq = new List<double>();
        //    //gain = new List<double>();
        //    //phase = new List<double>();

        //    //for (int i = 0; i < Condition.iFRAloop; i++)
        //    //{
        //    //    while (true)
        //    //    {
        //    //        freq.Add(startFreq);
        //    //        startFreq -= Condition.iFRAstep;
        //    //        if (startFreq < EndFreq) break;

        //    //    }
        //    //}

        //    //if (!DrvIC.FRA_Single(ch, axis, amp, freq, ref gain, ref phase))
        //    //{
        //    //    errMsg[ch] = string.Format("{0} Error", testItem);
        //    //    m_ChannelOn[ch] = false;
        //    //}
        //    //phaseIndex = FindPhaseIndex(gain);
        //    //if (phaseIndex < 1)
        //    //{
        //    //    AddLog(ch, "Y2 Find Phase Margin Failed.. Freq Range Check Please.");
        //    //    errMsg[ch] = string.Format("{0} Error", testItem);
        //    //    m_ChannelOn[ch] = false;
        //    //}
        //    //else
        //    //{
        //    //    AddLog(ch, string.Format("FRA Y2 Freq = {0} PM = {1}",
        //    //          Spec.PassFails[ch].Results[(int)SpecItem.FRAY2_PMFreq].Val = freq[(int)phaseIndex], Spec.PassFails[ch].Results[(int)SpecItem.FRAY2_PhaseMargin].Val = 180 + (phase[(int)phaseIndex])));

        //    //    Spec.SetResult(ch, (int)SpecItem.FRAY2_PMFreq, (int)SpecItem.FRAY2_PhaseMargin);
        //    //    ShowDataResults(ch, "FRA Y2");
        //    //}
        //    #endregion
        //    #region AF PM
        //    //AF
        //    axis = "AF";
        //    startFreq = Condition.iAFChirpFrom;
        //    EndFreq = Condition.iAFChirpTo;
        //    amp = (int)Condition.iAFAmplitude;

        //    AddLog(ch, string.Format("{0} FRA ==", axis));

        //    freq = new List<double>();
        //    gain = new List<double>();
        //    phase = new List<double>();

        //    for (int i = 0; i < Condition.iFRAloop; i++)
        //    {
        //        while (true)
        //        {
        //            freq.Add(startFreq);
        //            startFreq -= Condition.iFRAstep;
        //            if (startFreq < EndFreq) break;
        //        }
        //    }

        //    if (!DrvIC.FRA_Single(ch, axis, amp, 0, freq, ref gain, ref phase))
        //    {
        //        errMsg[ch] = string.Format("{0} Error", testItem);
        //        m_ChannelOn[ch] = false;
        //    }
        //    phaseIndex = FindPhaseIndex(gain);
        //    if (phaseIndex < 1)
        //    {
        //        AddLog(ch, "AF Find Phase Margin Failed.. Freq Range Check Please.");
        //        errMsg[ch] = string.Format("{0} Error", testItem);
        //        m_ChannelOn[ch] = false;
        //    }
        //    else
        //    {
        //        double phaseRes = 0, freqRes = 0;
        //        phaseRes = ((gain[phaseIndex + 1] * phase[phaseIndex]) - (gain[phaseIndex] * phase[phaseIndex + 1])) / (gain[phaseIndex + 1] - gain[phaseIndex]);
        //        freqRes = (int)(((gain[phaseIndex + 1] * freq[phaseIndex]) - (gain[phaseIndex] * freq[phaseIndex + 1])) / (gain[phaseIndex + 1] - gain[phaseIndex]));


        //        AddLog(ch, string.Format("FRA AF Freq = {0} PM = {1}",
        //              PassFails[ch].Results[(int)SpecItem.FRAAF_PMFreq].Val = freqRes, PassFails[ch].Results[(int)SpecItem.FRAAF_PhaseMargin].Val = phaseRes));

        //        SetResult(ch, (int)SpecItem.FRAAF_PMFreq, (int)SpecItem.FRAAF_PhaseMargin);
        //        ShowDataResults(ch, "FRA AF", (int)SpecItem.FRAAF_PMFreq, (int)SpecItem.FRAAF_PhaseMargin);
        //    }
        //    #endregion

        //}

        private void Act_Phase_Margin(int ch, string testItem)
        {

            if (!Dln.WriteArray(ch, DrvIC.XSlaveAddr, 1, 0x02, new byte[] { 0x40 })) return;
            if (!Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 1, 0x02, new byte[] { 0x40 })) return;
            if (!Dln.WriteArray(ch, DrvIC.Y2SlaveAddr, 1, 0x02, new byte[] { 0x40 })) return;
            if (!Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 1, 0x02, new byte[] { 0x00 })) return;
            DrvIC.Move(ch, "AF", BestAFPos);
            //DrvIC.Move(ch, "X", 2048);
            //DrvIC.Move(ch, "Y1", 2048);
            //DrvIC.Move(ch, "Y2", 2048);
            Thread.Sleep(200);

            string axis;
            int startFreq;
            int EndFreq;
            int amp;

            int phaseIndex = 0;

            List<double> freq = new List<double>();
            List<double> gain = new List<double>();
            List<double> phase = new List<double>();

            #region X PM Low
            axis = "X";
            startFreq = Condition.iXChirpFrom;
            EndFreq = Condition.iXChirpTo;
            amp = Condition.iXAmplitude;

            AddLog(ch, string.Format("{0} FRA ==", axis));

            freq = new List<double>();
            gain = new List<double>();
            phase = new List<double>();

            for (int i = 0; i < Condition.iFRAloop; i++)
            {
                while (true)
                {
                    freq.Add(startFreq);
                    startFreq -= (int)(startFreq * (Condition.iFRAstep / 100f));
                    if (startFreq < EndFreq) break;
                }
            }

            if (!DrvIC.FRA_Single(ch, axis, amp, 0, freq, ref gain, ref phase))
            {
                errMsg[ch] = string.Format("{0} Error", testItem);
                m_ChannelOn[ch] = false;
            }

            phaseIndex = FindPhaseIndex(gain);
            if (phaseIndex < 1)
            {
                AddLog(ch, "X Find Phase Margin Failed.. Freq Range Check Please.");
                errMsg[ch] = string.Format("{0} Error", testItem);
                m_ChannelOn[ch] = false;
            }
            else
            {
                double phaseRes = 0, freqRes = 0;
             
                if(phaseIndex == gain.Count - 1)
                {
                    phaseRes = phase[phaseIndex];
                    freqRes = freq[phaseIndex];
                }
                else
                {
                    phaseRes = ((gain[phaseIndex + 1] * phase[phaseIndex]) - (gain[phaseIndex] * phase[phaseIndex + 1])) / (gain[phaseIndex + 1] - gain[phaseIndex]);
                    freqRes = (int)(((gain[phaseIndex + 1] * freq[phaseIndex]) - (gain[phaseIndex] * freq[phaseIndex + 1])) / (gain[phaseIndex + 1] - gain[phaseIndex]));
                }

                AddLog(ch, string.Format("FRA X Freq = {0} PM = {1}",
                PassFails[ch].Results[(int)SpecItem.FRAX_PMFreq].Val = freqRes, PassFails[ch].Results[(int)SpecItem.FRAX_PhaseMargin].Val = phaseRes));

                SetResult(ch, (int)SpecItem.FRAX_PMFreq, (int)SpecItem.FRAX_PhaseMargin);
                ShowDataResults(ch, "FRA X", (int)SpecItem.FRAX_PMFreq, (int)SpecItem.FRAX_PhaseMargin);

            }
            #endregion
            #region Y PM Low
            //Y1
            axis = "Y1";
            startFreq = Condition.iYChirpFrom;
            EndFreq = Condition.iYChirpTo;
            amp = Condition.iYAmplitude;

            AddLog(ch, string.Format("{0} FRA ==", axis));

            freq = new List<double>();
            gain = new List<double>();
            phase = new List<double>();

            for (int i = 0; i < Condition.iFRAloop; i++)
            {
                while (true)
                {
                    freq.Add(startFreq);
                    startFreq -= (int)(startFreq * (Condition.iFRAstep / 100f));
                    if (startFreq < EndFreq) break;
                }
            }

            if (!DrvIC.FRA_Single(ch, axis, amp, 0, freq, ref gain, ref phase))
            {
                errMsg[ch] = string.Format("{0} Error", testItem);
                m_ChannelOn[ch] = false;
            }

            phaseIndex = FindPhaseIndex(gain);
            if (phaseIndex < 1)
            {
                AddLog(ch, "Y1 Find Phase Margin Failed.. Freq Range Check Please.");
                errMsg[ch] = string.Format("{0} Error", testItem);
                m_ChannelOn[ch] = false;
            }
            else
            {
                double phaseRes = 0, freqRes = 0;
                if (phaseIndex == gain.Count - 1)
                {
                    phaseRes = phase[phaseIndex];
                    freqRes = freq[phaseIndex];
                }
                else
                {
                    phaseRes = ((gain[phaseIndex + 1] * phase[phaseIndex]) - (gain[phaseIndex] * phase[phaseIndex + 1])) / (gain[phaseIndex + 1] - gain[phaseIndex]);
                    freqRes = (int)(((gain[phaseIndex + 1] * freq[phaseIndex]) - (gain[phaseIndex] * freq[phaseIndex + 1])) / (gain[phaseIndex + 1] - gain[phaseIndex]));
                }



                AddLog(ch, string.Format("FRA Y1 Freq = {0} PM = {1}",
                PassFails[ch].Results[(int)SpecItem.FRAY1_PMFreq].Val = freqRes, PassFails[ch].Results[(int)SpecItem.FRAY1_PhaseMargin].Val = phaseRes));

                SetResult(ch, (int)SpecItem.FRAY1_PMFreq, (int)SpecItem.FRAY1_PhaseMargin);
                ShowDataResults(ch, "FRA Y1", (int)SpecItem.FRAY1_PMFreq, (int)SpecItem.FRAY1_PhaseMargin);

            }
            #endregion
            #region Y2 PM Low
            //Y2
            axis = "Y2";
            startFreq = Condition.iYChirpFrom;
            EndFreq = Condition.iYChirpTo;
            amp = Condition.iYAmplitude;

            AddLog(ch, string.Format("{0} FRA ==", axis));

            freq = new List<double>();
            gain = new List<double>();
            phase = new List<double>();

            for (int i = 0; i < Condition.iFRAloop; i++)
            {
                while (true)
                {
                    freq.Add(startFreq);
                    startFreq -= (int)(startFreq * (Condition.iFRAstep / 100f));
                    if (startFreq < EndFreq) break;
                }
            }

            if (!DrvIC.FRA_Single(ch, axis, amp, 0, freq, ref gain, ref phase))
            {
                errMsg[ch] = string.Format("{0} Error", testItem);
                m_ChannelOn[ch] = false;
            }
            phaseIndex = FindPhaseIndex(gain);
            if (phaseIndex < 1)
            {
                AddLog(ch, "Y2 Find Phase Margin Failed.. Freq Range Check Please.");
                errMsg[ch] = string.Format("{0} Error", testItem);
                m_ChannelOn[ch] = false;
            }
            else
            {
                double phaseRes = 0, freqRes = 0;
                if (phaseIndex == gain.Count - 1)
                {
                    phaseRes = phase[phaseIndex];
                    freqRes = freq[phaseIndex];
                }
                else
                {
                    phaseRes = ((gain[phaseIndex + 1] * phase[phaseIndex]) - (gain[phaseIndex] * phase[phaseIndex + 1])) / (gain[phaseIndex + 1] - gain[phaseIndex]);
                    freqRes = (int)(((gain[phaseIndex + 1] * freq[phaseIndex]) - (gain[phaseIndex] * freq[phaseIndex + 1])) / (gain[phaseIndex + 1] - gain[phaseIndex]));
                }

                AddLog(ch, string.Format("FRA Y2 Freq = {0} PM = {1}",
                      PassFails[ch].Results[(int)SpecItem.FRAY2_PMFreq].Val = freqRes, PassFails[ch].Results[(int)SpecItem.FRAY2_PhaseMargin].Val = phaseRes));

                SetResult(ch, (int)SpecItem.FRAY2_PMFreq, (int)SpecItem.FRAY2_PhaseMargin);
                ShowDataResults(ch, "FRA Y2", (int)SpecItem.FRAY2_PMFreq, (int)SpecItem.FRAY2_PhaseMargin);
            }
            #endregion

     

            #region X PM
            axis = "X";
            startFreq = Condition.iHighXChirpFrom;
            EndFreq = Condition.iHighXChirpTo;
            amp = (int)Condition.iHighXAmplitude;

            AddLog(ch, string.Format("{0} FRA ==", axis));

            freq = new List<double>();
            gain = new List<double>();
            phase = new List<double>();

            for (int i = 0; i < Condition.iFRAloop; i++)
            {
                while (true)
                {
                    freq.Add(startFreq);
                    startFreq -= (int)(startFreq * (Condition.iHighFRAstep / 100f));
                    if (startFreq < EndFreq) break;
                }
            }

            if (!DrvIC.FRA_Single(ch, axis, amp, 0, freq, ref gain, ref phase))
            {
                errMsg[ch] = string.Format("{0} Error", testItem);
                m_ChannelOn[ch] = false;
            }

            phaseIndex = FindPhaseIndex(gain);
            if (phaseIndex < 1)
            {
                AddLog(ch, "X Find Phase Margin Failed.. Freq Range Check Please.");
                errMsg[ch] = string.Format("{0} Error", testItem);
                m_ChannelOn[ch] = false;
            }
            else
            {
                double phaseRes = 0, freqRes = 0;
                if (phaseIndex == gain.Count - 1)
                {
                    phaseRes = phase[phaseIndex];
                    freqRes = freq[phaseIndex];
                }
                else
                {
                    phaseRes = ((gain[phaseIndex + 1] * phase[phaseIndex]) - (gain[phaseIndex] * phase[phaseIndex + 1])) / (gain[phaseIndex + 1] - gain[phaseIndex]);
                    freqRes = (int)(((gain[phaseIndex + 1] * freq[phaseIndex]) - (gain[phaseIndex] * freq[phaseIndex + 1])) / (gain[phaseIndex + 1] - gain[phaseIndex]));
                }

                AddLog(ch, string.Format("FRA X Freq = {0} PM = {1}",
                PassFails[ch].Results[(int)SpecItem.FRAX_PMFreq_High].Val = freqRes, PassFails[ch].Results[(int)SpecItem.FRAX_PhaseMargin_High].Val = phaseRes));

                SetResult(ch, (int)SpecItem.FRAX_PMFreq_High, (int)SpecItem.FRAX_PhaseMargin_High);
                ShowDataResults(ch, "FRA X", (int)SpecItem.FRAX_PMFreq_High, (int)SpecItem.FRAX_PhaseMargin_High);

            }
            #endregion
            #region Y PM
            //Y1
            axis = "Y1";
            startFreq = Condition.iHighYChirpFrom;
            EndFreq = Condition.iHighYChirpTo;
            amp = (int)Condition.iHighYAmplitude;

            AddLog(ch, string.Format("{0} FRA ==", axis));

            freq = new List<double>();
            gain = new List<double>();
            phase = new List<double>();

            for (int i = 0; i < Condition.iFRAloop; i++)
            {
                while (true)
                {
                    freq.Add(startFreq);
                    startFreq -= (int)(startFreq * (Condition.iHighFRAstep / 100f));
                    if (startFreq < EndFreq) break;
                }
            }

            if (!DrvIC.FRA_Single(ch, axis, amp, 0, freq, ref gain, ref phase))
            {
                errMsg[ch] = string.Format("{0} Error", testItem);
                m_ChannelOn[ch] = false;
            }

            phaseIndex = FindPhaseIndex(gain);
            if (phaseIndex < 1)
            {
                AddLog(ch, "Y1 Find Phase Margin Failed.. Freq Range Check Please.");
                errMsg[ch] = string.Format("{0} Error", testItem);
                m_ChannelOn[ch] = false;
            }
            else
            {
                double phaseRes = 0, freqRes = 0;
                if (phaseIndex == gain.Count - 1)
                {
                    phaseRes = phase[phaseIndex];
                    freqRes = freq[phaseIndex];
                }
                else
                {
                    phaseRes = ((gain[phaseIndex + 1] * phase[phaseIndex]) - (gain[phaseIndex] * phase[phaseIndex + 1])) / (gain[phaseIndex + 1] - gain[phaseIndex]);
                    freqRes = (int)(((gain[phaseIndex + 1] * freq[phaseIndex]) - (gain[phaseIndex] * freq[phaseIndex + 1])) / (gain[phaseIndex + 1] - gain[phaseIndex]));
                }


                AddLog(ch, string.Format("FRA Y1 Freq = {0} PM = {1}",
                PassFails[ch].Results[(int)SpecItem.FRAY1_PMFreq_High].Val = freqRes, PassFails[ch].Results[(int)SpecItem.FRAY1_PhaseMargin_High].Val = phaseRes));

                SetResult(ch, (int)SpecItem.FRAY1_PMFreq_High, (int)SpecItem.FRAY1_PhaseMargin_High);
                ShowDataResults(ch, "FRA Y1", (int)SpecItem.FRAY1_PMFreq_High, (int)SpecItem.FRAY1_PhaseMargin_High);

            }
            #endregion
            #region Y2 PM
            //Y2
            axis = "Y2";
            startFreq = Condition.iHighYChirpFrom;
            EndFreq = Condition.iHighYChirpTo;
            amp = (int)Condition.iHighYAmplitude;

            AddLog(ch, string.Format("{0} FRA ==", axis));

            freq = new List<double>();
            gain = new List<double>();
            phase = new List<double>();

            for (int i = 0; i < Condition.iFRAloop; i++)
            {
                while (true)
                {
                    freq.Add(startFreq);
                    startFreq -= (int)(startFreq * (Condition.iHighFRAstep / 100f));
                    if (startFreq < EndFreq) break;
                }
            }

            if (!DrvIC.FRA_Single(ch, axis, amp, 0, freq, ref gain, ref phase))
            {
                errMsg[ch] = string.Format("{0} Error", testItem);
                m_ChannelOn[ch] = false;
            }
            phaseIndex = FindPhaseIndex(gain);
            if (phaseIndex < 1)
            {
                AddLog(ch, "Y2 Find Phase Margin Failed.. Freq Range Check Please.");
                errMsg[ch] = string.Format("{0} Error", testItem);
                m_ChannelOn[ch] = false;
            }
            else
            {
                double phaseRes = 0, freqRes = 0;
                if (phaseIndex == gain.Count - 1)
                {
                    phaseRes = phase[phaseIndex];
                    freqRes = freq[phaseIndex];
                }
                else
                {
                    phaseRes = ((gain[phaseIndex + 1] * phase[phaseIndex]) - (gain[phaseIndex] * phase[phaseIndex + 1])) / (gain[phaseIndex + 1] - gain[phaseIndex]);
                    freqRes = (int)(((gain[phaseIndex + 1] * freq[phaseIndex]) - (gain[phaseIndex] * freq[phaseIndex + 1])) / (gain[phaseIndex + 1] - gain[phaseIndex]));
                }
                AddLog(ch, string.Format("FRA Y2 Freq = {0} PM = {1}",
                      PassFails[ch].Results[(int)SpecItem.FRAY2_PMFreq_High].Val = freqRes, PassFails[ch].Results[(int)SpecItem.FRAY2_PhaseMargin_High].Val = phaseRes));

                SetResult(ch, (int)SpecItem.FRAY2_PMFreq_High, (int)SpecItem.FRAY2_PhaseMargin_High);
                ShowDataResults(ch, "FRA Y2", (int)SpecItem.FRAY2_PMFreq_High, (int)SpecItem.FRAY2_PhaseMargin_High);
            }
            #endregion
            #region AF PM
            //AF
            axis = "AF";
            startFreq = Condition.iAFChirpFrom;
            EndFreq = Condition.iAFChirpTo;
            amp = (int)Condition.iAFAmplitude;

            AddLog(ch, string.Format("{0} FRA ==", axis));

            freq = new List<double>();
            gain = new List<double>();
            phase = new List<double>();

            for (int i = 0; i < Condition.iFRAloop; i++)
            {
                while (true)
                {
                    freq.Add(startFreq);
                    startFreq -= (int)(startFreq * (0.2));
                    if (startFreq < EndFreq) break;
                }
            }

            if (!DrvIC.FRA_Single(ch, axis, amp, 0, freq, ref gain, ref phase))
            {
                errMsg[ch] = string.Format("{0} Error", testItem);
                m_ChannelOn[ch] = false;
            }
            phaseIndex = FindPhaseIndex(gain);
            if (phaseIndex < 1)
            {
                AddLog(ch, "AF Find Phase Margin Failed.. Freq Range Check Please.");
                errMsg[ch] = string.Format("{0} Error", testItem);
                m_ChannelOn[ch] = false;
            }
            else
            {
                double phaseRes = 0, freqRes = 0;
                if (phaseIndex == gain.Count - 1)
                {
                    phaseRes = phase[phaseIndex];
                    freqRes = freq[phaseIndex];
                }
                else
                {
                    phaseRes = ((gain[phaseIndex + 1] * phase[phaseIndex]) - (gain[phaseIndex] * phase[phaseIndex + 1])) / (gain[phaseIndex + 1] - gain[phaseIndex]);
                    freqRes = (int)(((gain[phaseIndex + 1] * freq[phaseIndex]) - (gain[phaseIndex] * freq[phaseIndex + 1])) / (gain[phaseIndex + 1] - gain[phaseIndex]));
                }
                AddLog(ch, string.Format("FRA AF Freq = {0} PM = {1}",
                      PassFails[ch].Results[(int)SpecItem.FRAAF_PMFreq].Val = freqRes, PassFails[ch].Results[(int)SpecItem.FRAAF_PhaseMargin].Val = phaseRes));

                SetResult(ch, (int)SpecItem.FRAAF_PMFreq, (int)SpecItem.FRAAF_PhaseMargin);
                ShowDataResults(ch, "FRA AF", (int)SpecItem.FRAAF_PMFreq, (int)SpecItem.FRAAF_PhaseMargin);

            }
            #endregion



        }


        public int FindPhaseIndex(List<double> gain)
        {
            bool isNeg = false;
            for (int i = 0; i < gain.Count; i++)
            {
                if (gain[i] >= 0 && !isNeg)
                {
                    continue;
                }
                isNeg = true;
                if (gain[i] >= 0)
                {
                    if (i == 0) return 0;
                    return i - 1;
                }
            }
            return gain.Count - 1;
        }
        //public int FindGainIndex(List<double> phase)
        //{
        //    for (int i = 0; i < phase.Count; i++)
        //    {
        //        if (phase[i] >= 0)
        //        {
        //            if (i == 0) return 0;
        //            return i - 1;
        //        }
        //    }
        //    return 0;
        //}
        //private void Act_Gain_Margin(int ch, string testItem)
        //{
        //    string axis;
        //    int startFreq;
        //    int EndFreq;
        //    int amp;

        //    DrvIC.OISOn(ch, testItem, false);
        //    //X
        //    axis = "X";
        //    startFreq = Condition.iXGainFrom;
        //    EndFreq = Condition.iXGainTo;
        //    amp = (int)Condition.iXAmplitudeGain;

        //    AddLog(ch, string.Format("{0} FRA ==", axis));

        //    List<double> freq = new List<double>();
        //    List<double> gain = new List<double>();
        //    List<double> phase = new List<double>();

        //    for (int i = 0; i < Condition.iGainLoop; i++)
        //    {
        //        while (true)
        //        {
        //            freq.Add(startFreq);
        //            startFreq -= Condition.iGainStep;
        //            if (startFreq < EndFreq) break;

        //        }
        //    }
        //    if (!DrvIC.FRA_Single(ch, axis, amp, 1, freq, ref gain, ref phase))
        //    {
        //        errMsg[ch] = string.Format("{0} Error", testItem);
        //        m_ChannelOn[ch] = false;
        //    }
        //    int gainIndex = FindGainIndex(phase);
        //    if (gainIndex < 1)
        //    {
        //        AddLog(ch, "X Find Gain Margin Failed.. Freq Range Check Please.");
        //        errMsg[ch] = string.Format("{0} Error", testItem);
        //        m_ChannelOn[ch] = false;
        //    }
        //    else
        //    {
        //        AddLog(ch, string.Format("FRA X GM = {0}", PassFails[ch].Results[(int)SpecItem.FRAX_GainMargin].Val = Math.Abs(gain[gainIndex])));
        //        SetResult(ch, (int)SpecItem.FRAX_GainMargin, (int)SpecItem.FRAX_GainMargin);
        //        ShowDataResults(ch, "FRA X", (int)SpecItem.FRAX_GainMargin, (int)SpecItem.FRAX_GainMargin);
        //    }

        //    //Y1
        //    axis = "Y1";
        //    startFreq = Condition.iYGainFrom;
        //    EndFreq = Condition.iYGainTo;
        //    amp = (int)Condition.iYAmplitudeGain;
        //    AddLog(ch, string.Format("{0} FRA ==", axis));

        //    gain = new List<double>();
        //    phase = new List<double>();

        //    if (!DrvIC.FRA_Single(ch, axis, amp, 1, freq, ref gain, ref phase))
        //    {
        //        errMsg[ch] = string.Format("{0} Error", testItem);
        //        m_ChannelOn[ch] = false;
        //    }
        //    gainIndex = FindGainIndex(phase);
        //    if (gainIndex < 1)
        //    {
        //        AddLog(ch, "Y1 Find Gain Margin Failed.. Freq Range Check Please.");
        //        errMsg[ch] = string.Format("{0} Error", testItem);
        //        m_ChannelOn[ch] = false;
        //    }
        //    else
        //    {
        //        AddLog(ch, string.Format("FRA Y1 GM = {0}", PassFails[ch].Results[(int)SpecItem.FRAY1_GainMargin].Val = Math.Abs(gain[gainIndex])));

        //        SetResult(ch, (int)SpecItem.FRAY1_PMFreq, (int)SpecItem.FRAY1_GainMargin);
        //        ShowDataResults(ch, "FRA Y1", (int)SpecItem.FRAY1_PMFreq, (int)SpecItem.FRAY1_GainMargin);
        //    }

        //    //Y2
        //    //axis = "Y2";
        //    //AddLog(ch, string.Format("{0} FRA ==", axis));

        //    //gain = new List<double>();
        //    //phase = new List<double>();

        //    //if (!DrvIC.FRA_Single(ch, axis, amp, freq, ref gain, ref phase))
        //    //{
        //    //    errMsg[ch] = string.Format("{0} Error", testItem);
        //    //    m_ChannelOn[ch] = false;
        //    //}
        //    //gainIndex = FindGainIndex(phase);
        //    //if (gainIndex < 1)
        //    //{
        //    //    AddLog(ch, "Y2 Find Gain Margin Failed.. Freq Range Check Please.");
        //    //    errMsg[ch] = string.Format("{0} Error", testItem);
        //    //    m_ChannelOn[ch] = false;
        //    //}
        //    //else
        //    //{

        //    //    AddLog(ch, string.Format("FRA Y2 GM = {0}", Spec.PassFails[ch].Results[(int)SpecItem.FRAY2_GainMargin].Val = Math.Abs(gain[gainIndex])));

        //    //    Spec.SetResult(ch, (int)SpecItem.FRAY2_GainMargin, (int)SpecItem.FRAY2_GainMargin);
        //    //    ShowDataResults(ch, "FRA Y2");
        //    //}
        //}

        public void ServoDecenter(int port, string name)
        {
            int ch = port * 2;
            LEDs_All_On(port, true);
            FindResult[] fX = new FindResult[2] { new FindResult(), new FindResult() };
            FindResult[] fY = new FindResult[2] { new FindResult(), new FindResult() };
            STATIC.DrvIC.OISOn(0, "X", true);
            STATIC.DrvIC.OISOn(0, "Y", true);

            STATIC.DrvIC.Move(0, "X", OISCenter);
            STATIC.DrvIC.Move(0, "Y", OISCenter);

            Thread.Sleep(500);
            STATIC.fVision.m__G.oCam[0].GrabA(0);
            fX[0] = STATIC.fVision.MeasureTxTyTz(0, "X", true);

            STATIC.DrvIC.OISOn(0, "X", false);
            Thread.Sleep(500);

            STATIC.fVision.m__G.oCam[0].GrabA(0);
            fX[1] = STATIC.fVision.MeasureTxTyTz(0, "X", true);


            PassFails[0].Results[(int)SpecItem.x_ServoDecenter].Val = fX[0].cx[0] - fX[1].cx[0];


            STATIC.DrvIC.OISOn(0, "X", true);
            STATIC.DrvIC.OISOn(0, "Y", true);

            STATIC.DrvIC.Move(0, "X", OISCenter);
            STATIC.DrvIC.Move(0, "Y", OISCenter);

            Thread.Sleep(500);
            STATIC.fVision.m__G.oCam[0].GrabA(0);
            fY[0] = STATIC.fVision.MeasureTxTyTz(0, "Y", true);

            STATIC.DrvIC.OISOn(0, "Y", false);

            Thread.Sleep(500);
            STATIC.fVision.m__G.oCam[0].GrabA(0);
            fY[1] = STATIC.fVision.MeasureTxTyTz(0, "Y", true);

            PassFails[0].Results[(int)SpecItem.y_ServoDecenter].Val = fY[0].cy[0] - fY[1].cy[0];

            SetResult(0, (int)SpecItem.x_ServoDecenter, (int)SpecItem.y_ServoDecenter);
            ShowDataResults(0, "Servo Decenter", (int)SpecItem.x_ServoDecenter, (int)SpecItem.y_ServoDecenter);

            LEDs_All_On(port, false);
        }

        private void Act_OISShift(int port, string testItem)
        {
         
       
            //      Dln.ReadArray(0, DrvIC.Y1SlaveAddr, 1, 0xE5, b);

            LEDs_All_On(port, true);
            FindResult res = new FindResult();

            List<FindResult> resList = new List<FindResult>();
            List<FindResult> resList2 = new List<FindResult>();
            List<double> diffx = new List<double>();
            List<double> diffy = new List<double>();
            List<double> shiftX = new List<double>();
            List<double> shiftY = new List<double>();
            List<int> hallcompx = new List<int>();
            List<int> hallcompy = new List<int>();
            double RefX;
            double RefY;

            double slopeX = SlopeX; //F_Manage.xSlope;
            double slopeY = SlopeY;//F_Manage.ySlope;
            //double slopeX = F_Manage.xSlope;
            //double slopeY = F_Manage.ySlope;

            AddLog(0, $"X Slope : {slopeX.ToString("F4")}, Y Slope : {slopeY.ToString("F4")}");

            Dln.WriteArray(0, DrvIC.AFSlaveAddr, 1, 0x02, new byte[] { 0x00 });
            Dln.WriteArray(0, DrvIC.XSlaveAddr, 1, 0x02, new byte[] { 0x00 });
            Dln.WriteArray(0, DrvIC.Y1SlaveAddr, 1, 0x02, new byte[] { 0x00 });
            Dln.WriteArray(0, DrvIC.Y2SlaveAddr, 1, 0x02, new byte[] { 0x00 });

            DrvIC.Move(0, "X", 2047);
            DrvIC.Move(0, "Y", 2047);
            Thread.Sleep(100);

          
            int[] code = new int[] { 0, 512, 1024, 1536, 2048, 2560, 3072, 3584, 4092 };


            DrvIC.Move(0, "AF", BestAFPos - 100);
            Thread.Sleep(100);
            DrvIC.Move(0, "AF", BestAFPos - 50);
            Thread.Sleep(100);
            DrvIC.Move(0, "AF", BestAFPos);
            Thread.Sleep(100);
            STATIC.fVision.m__G.oCam[port].GrabA(0);
            res = STATIC.fVision.MeasureTxTyTz(0, "X", true, false);

            RefX = res.cx[0];
            RefY = res.cy[0];



            DrvIC.Move(0, "AF", 100);
            Thread.Sleep(100);
            DrvIC.Move(0, "AF", 50);
            Thread.Sleep(100);
            DrvIC.Move(0, "AF", 0);
            Thread.Sleep(100);
            for (int i = 0; i < code.Length; i++)
            {
                resList.Add(new FindResult());
                DrvIC.Move(0, "AF", code[i]);
                Thread.Sleep(100);
                STATIC.fVision.m__G.oCam[port].GrabA(0);
                resList[i] = STATIC.fVision.MeasureTxTyTz(0, "X", true, false);
            }

            for (int i = 0; i < resList.Count; i++)
            {
                diffx.Add(resList[i].cx[0] - RefX);
                diffy.Add(resList[i].cy[0] - RefY);
                AddLog(0, $"{code[i]}\t{diffx[i].ToString("F3")}\t{diffy[i].ToString("F3")}\t{resList[i].cz[0].ToString("F3")}");
            }

            for (int i = 0; i < resList.Count; i++)
            {
                hallcompx.Add((int)(-1 * (diffx[i] / slopeX)));
                hallcompy.Add((int)(-1 * (diffy[i] / slopeY)));
                AddLog(0, $"Hall Comp X : {(int)hallcompx[i]}, Hall Comp Y : {(int)hallcompy[i]}");
            }

            Dln.WriteArray(0, DrvIC.AFSlaveAddr, 1, 0x02, new byte[] { 0x00 });
            DrvIC.Move(0, "X", OISCenter);
            DrvIC.Move(0, "Y", OISCenter);

            DrvIC.Move(0, "AF", 100);
            Thread.Sleep(100);
            DrvIC.Move(0, "AF", 50);
            Thread.Sleep(100);
            DrvIC.Move(0, "AF", 0);
            Thread.Sleep(100);


            for (int i = 0; i < code.Length; i++)
            {
                resList2.Add(new FindResult());

                DrvIC.Move(0, "AF", code[i]);
                DrvIC.Move(0, "X", OISCenter + hallcompx[i]);
                DrvIC.Move(0, "Y", OISCenter + hallcompy[i]);
                Thread.Sleep(100);

                STATIC.fVision.m__G.oCam[port].GrabA(0);
                resList2[i] = STATIC.fVision.MeasureTxTyTz(0, "X", true, false);


            }

            for (int i = 0; i < resList2.Count; i++)
            {
                shiftX.Add(resList2[i].cx[0] - RefX);
                shiftY.Add(resList2[i].cy[0] - RefY);
                AddLog(0, $"{code[i]}\t{shiftX[i].ToString("F3")}\t{shiftY[i].ToString("F3")}\t{resList2[i].cz[0].ToString("F3")}");
            }

            double xValMax = double.MinValue;
            double yValMax = double.MinValue;
            double xLimitMax = double.MinValue;
            double yLimitMax = double.MinValue;

            int xValMaxIndex = 0;
            int yValMaxIndex = 0;
            int xLimitMaxIndex = 0;
            int yLimitMaxIndex = 0;



            for (int i = 0; i < resList2.Count; i++)
            {
                if (Math.Abs(shiftX[i]) > xValMax) { xValMax = Math.Abs(shiftX[i]); xValMaxIndex = i; }
                if (Math.Abs(shiftY[i]) > yValMax) { yValMax = Math.Abs(shiftY[i]); yValMaxIndex = i; }
                if (Math.Abs(hallcompx[i]) > xLimitMax) { xLimitMax = Math.Abs(hallcompx[i]); xLimitMaxIndex = i; }
                if (Math.Abs(hallcompy[i]) > yLimitMax) { yLimitMax = Math.Abs(hallcompy[i]); yLimitMaxIndex = i; }
            }

            PassFails[0].Results[(int)SpecItem.x_Shift].Val = shiftX[xValMaxIndex];
            PassFails[0].Results[(int)SpecItem.y_Shift].Val = shiftY[yValMaxIndex];
            PassFails[0].Results[(int)SpecItem.x_Limit].Val = hallcompx[xLimitMaxIndex];
            PassFails[0].Results[(int)SpecItem.y_Limit].Val = hallcompy[yLimitMaxIndex];

            SetResult(0, (int)SpecItem.x_Shift, (int)SpecItem.y_Limit);
            ShowDataResults(0, "OIS Shift", (int)SpecItem.x_Shift, (int)SpecItem.y_Limit);

            LEDs_All_On(port, false);
        }

        #endregion



        //===============================================================================================================================












    }
}
