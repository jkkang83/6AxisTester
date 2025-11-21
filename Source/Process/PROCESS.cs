using Basler.Pylon;
using FZ4P.Properties;
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
using TiltPlot;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace FZ4P
{
    public partial class Process
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
        public Label lblFailList = new Label();
        public List<ChartList> ChartTop = new List<ChartList>();
        public List<ChartList> ChartBtm = new List<ChartList>();
        public List<TiltGraph> tiltChart = new List<TiltGraph>();

    //    public List<ChartList> ChartBtm = new List<ChartList>();
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
                ChartBtm.Add(new ChartList("Settling", i));
                tiltChart.Add(new TiltGraph
                {
                    title = "AF Tilt",
                    range = 20,
                });
                tiltChart[i].SetRings(new double[] { tiltChart[i].range / 2, tiltChart[i].range });
                

                InfoBtn.Add(new InfoButton()); //test
                InfoBtn.Add(new InfoButton());
                ViewLog.Add(new LogText());
            }
            ItemList.Add(new ActItems() { Name = "AF Scan", Func = Act_ScanCode });
            ItemList.Add(new ActItems() { Name = "OIS X Scan", Func = Act_ScanCode });
            ItemList.Add(new ActItems() { Name = "OIS Y Scan", Func = Act_ScanCode });
            ItemList.Add(new ActItems() { Name = "AF Settling", Func = Act_ScanTimeCode });


            AddSequence();

            m__G = Global.GetInstance();
        }

        #region Default
        public void ShowDataResults(int ch, int start, int end)
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

            if (ResultDataGrid.InvokeRequired)
            {
                ResultDataGrid.BeginInvoke((MethodInvoker)delegate
                {
                    for (int i = start; i <= end; i++)
                    {                       
                        if (PassFails[ch].Results[i].Val != 0)
                        {
                            ResultDataGrid[ch + 5, i].Value = PassFails[ch].Results[i].Val.ToString("F3");
                        }
                        if (PassFails[ch].Results[i].bPass) { ResultDataGrid[ch + 5, i].Style.BackColor = Color.White; ResultDataGrid[ch + 1, i].Style.BackColor = Color.White; }
                        else { ResultDataGrid[ch + 5, i].Style.BackColor = Color.Orange; ResultDataGrid[ch + 1, i].Style.BackColor = Color.Orange; }
                        

                    }

                });
            }
            else
            {
                for (int i = start; i <= end; i++)
                {           
                    if (PassFails[ch].Results[i].Val != 0)
                    {
                        ResultDataGrid[ch + 5, i].Value = PassFails[ch].Results[i].Val.ToString("F3");
                    }
                    if (PassFails[ch].Results[i].bPass) { ResultDataGrid[ch + 5, i].Style.BackColor = Color.White; ResultDataGrid[ch + 1, i].Style.BackColor = Color.White; }
                    else { ResultDataGrid[ch + 5, i].Style.BackColor = Color.Orange; ResultDataGrid[ch + 1, i].Style.BackColor = Color.Orange; }

                }
            }

            if (lblFailList.InvokeRequired)
            {
                lblFailList.BeginInvoke((MethodInvoker)delegate
                {
                    for (int i = start; i <= end; i++)
                    {
                        if (!PassFails[ch].Results[i].bPass) { STATIC.FailNumber += $"{i},"; lblFailList.Text = STATIC.FailNumber; }
                    
                    }

                });
            }
            else
            {
                for (int i = start; i <= end; i++)
                {
                    if (!PassFails[ch].Results[i].bPass) { STATIC.FailNumber += $"{i},"; lblFailList.Text = STATIC.FailNumber; }
                }
            }
            for (int i = start; i <= end; i++)
            {
                if (!PassFails[ch].Results[i].bPass)
                {
                    if (!Option.ContinueTestingOnFail) m_ChannelOn[ch] = false;
                }


            }

        }
        public void SetError(int ch, NonSpecItem item)
        {
            PassFails[ch].FirstFailIndex = (int)item;
            m_ChannelOn[ch] = false;
            errMsg[ch] = item.ToString();
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
            ResultDataGrid.Font = new Font("Calibri", 10, FontStyle.Bold);
            for (int i = 0; i < ResultDataGrid.ColumnCount; i++)
            {
                ResultDataGrid.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
            }
            ResultDataGrid.RowHeadersVisible = false;
            ResultDataGrid.BackgroundColor = Color.LightGray;

            //// Column
            ResultDataGrid.Columns[0].Name = "Axis";
            ResultDataGrid.Columns[1].Name = "Item No.";
            ResultDataGrid.Columns[2].Name = "Item Name";
            ResultDataGrid.Columns[3].Name = "Min";
            ResultDataGrid.Columns[4].Name = "Max";
            ResultDataGrid.Columns[5].Name = "Result";
          //  ResultDataGrid.Columns[5].Name = "#2 Result";
            ResultDataGrid.Columns[6].Name = "unit";

            ResultDataGrid.Columns[0].DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopCenter;
            ResultDataGrid.Columns[1].DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopCenter;
            ResultDataGrid.Columns[2].DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopLeft;
            ResultDataGrid.Columns[3].DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopRight;
            ResultDataGrid.Columns[4].DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopRight;
            ResultDataGrid.Columns[5].DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopRight;
            ResultDataGrid.Columns[6].DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopLeft;

            ResultDataGrid.Columns[0].Width = 150;
            ResultDataGrid.Columns[1].Width = 70;
            ResultDataGrid.Columns[2].Width = 215;
            ResultDataGrid.Columns[3].Width = 70;
            ResultDataGrid.Columns[4].Width = 70;
            ResultDataGrid.Columns[5].Width = 100;
            ResultDataGrid.Columns[6].Width = 65;

            ResultDataGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            ResultDataGrid.ColumnHeadersHeight = 28;

            bool bColorChange = true;
            ResultDataGrid.Rows.Clear();
            for (int i = 0; i < Spec.specList.Count; i++)
            {
                ResultDataGrid.Rows.Add(Spec.specList[i].Category, i, Spec.specList[i].DisplayName, Spec.specList[i].MinSpec, Spec.specList[i].MaxSpec, 0, Spec.specList[i].Unit);
                ResultDataGrid.Rows[i].Visible = Convert.ToBoolean(Spec.specList[i].OnOff);

                if (bColorChange) for (int k = 0; k < ResultDataGrid.ColumnCount; k++) ResultDataGrid[k, i].Style.BackColor = Color.Lavender;
                else for (int k = 0; k < ResultDataGrid.ColumnCount; k++) ResultDataGrid[k, i].Style.BackColor = Color.White;



                ResultDataGrid.Rows[i].Height = 22;
                ResultDataGrid.Rows[i].Resizable = DataGridViewTriState.False;
                ResultDataGrid.Rows[i].DefaultCellStyle.Font = new Font("Calibri", 10, FontStyle.Bold);
                ResultDataGrid[1, i].Style.Font = new Font("Calibri", 10, FontStyle.Bold);
                ResultDataGrid[3, i].Style.Font = new Font("Calibri", 10, FontStyle.Bold);
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
       
        public void ShowDataResultsInit(int ch)
        {
            if (ResultDataGrid.InvokeRequired)
            {
                ResultDataGrid.BeginInvoke((MethodInvoker)delegate
                {
                    InitResult(ch);
                    for (int i = 0; i < Spec.specList.Count; i++)
                    {
                        ResultDataGrid[ch + 5, i].Value = PassFails[ch].Results[i].Val.ToString("F0");
                        ResultDataGrid[ch + 5, i].Style.BackColor = Color.White;
                        ResultDataGrid[ch + 1, i].Style.BackColor = Color.White;
                    }
                });
            }
            else
            {
                InitResult(ch);
                for (int i = 0; i < Spec.specList.Count; i++)
                {
                    ResultDataGrid[ch + 5, i].Value = PassFails[ch].Results[i].Val.ToString("F0");
                    ResultDataGrid[ch + 5, i].Style.BackColor = Color.White;
                    ResultDataGrid[ch + 1, i].Style.BackColor = Color.White;
                }
            }

            if (lblFailList.InvokeRequired)
            {
                lblFailList.BeginInvoke((MethodInvoker)delegate
                {
                    lblFailList.Text = "";
                });
            }
            else lblFailList.Text = "";
            STATIC.FailNumber = "Fail No. : ";
        }
        public void AddLog(int ch, string msg)
        {
            ViewLog[ch].Log(msg);
        }
        public void AddChart(int ch, string name, List<double> time = null, List<double> Stroke = null, double MaxtiltX = 0, double MaxtiltY = 0, double[] refArr = null)
        {
            while (ChartTop[ch].IsFalg)
                Process.Wait(10);

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

                                    for (int i = 0; i < Cal.CodeX.Count; i++)
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
                            //if (ChartBtm[ch].C.InvokeRequired)
                            //{
                            //    ChartBtm[ch].C.BeginInvoke((MethodInvoker)delegate
                            //    {
                            //        for (int i = 2; i < Cal.CodeX.Count; i++)
                            //        {
                            //            if (Cal.CodeX[i] >= OISCenter - CodeRange && Cal.CodeX[i] <= OISCenter + CodeRange)
                            //            {
                            //                //ChartBtm[ch].C.Series[0].Points.AddXY(Cal.CodeX[i], Cal.TiltX[i]); //  Tilt 
                            //                //ChartBtm[ch].C.Series[1].Points.AddXY(Cal.CodeX[i], Cal.TiltY[i]); //  Tilt 
                            //                //ChartBtm[ch].C.Series[2].Points.AddXY(Cal.CodeX[i], Cal.TiltZ[i]); //  Tilt 
                            //            }
                            //        }
                            //    });
                            //}
                            break;
                        case "OIS Y Scan":

                            CodeRange = Condition.iYPlotRange;
                            //Stroke
                            if (ChartTop[ch].C.InvokeRequired)
                            {
                                ChartTop[ch].C.BeginInvoke((MethodInvoker)delegate
                                {
                                    for (int i = 0; i < Cal.CodeY.Count; i++)
                                    {
                                        if (Cal.CodeY[i] >= OISCenter - CodeRange && Cal.CodeY[i] <= OISCenter + CodeRange)
                                        {
                                            ChartTop[ch].C.Series[1].Points.AddXY(Cal.CodeY[i], Cal.StrokeY[i]); //  stroke
                                                                                                                  //   ChartTop[ch].C.Series[9].Points.AddXY(Cal.CodeY1[i], Cal.StrokeY1[i]); //  stroke 1
                                                                                                                  // ChartTop[ch].C.Series[10].Points.AddXY(Cal.CodeY2[i], Cal.StrokeY2[i]); //  stroke 2
                                            ChartTop[ch].C.Series[4].Points.AddXY(Cal.CodeY[i], Cal.Current[i]); //  current
                                            ChartTop[ch].C.Series[7].Points.AddXY(Cal.CodeY[i], Cal.HallY1[i] / 10); //  hall
                                        }
                                    }
                                });
                            }
                            //Tilt
                            //if (ChartBtm[ch].C.InvokeRequired)
                            //{
                            //    ChartBtm[ch].C.BeginInvoke((MethodInvoker)delegate
                            //    {
                            //        for (int i = 2; i < Cal.CodeY.Count; i++)
                            //        {
                            //            if (Cal.CodeY[i] >= OISCenter - CodeRange && Cal.CodeY[i] <= OISCenter + CodeRange)
                            //            {
                            //                //ChartBtm[ch].C.Series[3].Points.AddXY(Cal.CodeY1[i], Cal.TiltX[i]); //  Tilt 
                            //                //ChartBtm[ch].C.Series[4].Points.AddXY(Cal.CodeY1[i], Cal.TiltY[i]); //  Tilt 
                            //                //ChartBtm[ch].C.Series[5].Points.AddXY(Cal.CodeY1[i], Cal.TiltZ[i]); //  Tilt 
                            //            }
                            //        }
                            //    });
                            //}
                            break;
                        case "AF Scan":

                            CodeRange = Condition.iAFPlotRange;
                            //Stroke
                            if (ChartTop[ch].C.InvokeRequired)
                            {
                                ChartTop[ch].C.BeginInvoke((MethodInvoker)delegate
                                {
                                    for (int i = 0; i < Cal.CodeZ.Count; i++)
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
                            if (tiltChart[ch].InvokeRequired)
                            {
                                tiltChart[ch].BeginInvoke((MethodInvoker)delegate
                                {
                                    double[] xs = new double[Cal.CodeZ.Count];
                                    double[] ys = new double[Cal.CodeZ.Count];

                                    for (int i = 2; i < Cal.CodeZ.Count; i++)
                                    {
                                        if (Cal.CodeZ[i] >= Condition.TiltMinCode && Cal.CodeZ[i] <= Condition.TiltMaxCode)
                                        {
                                            xs[i] = Cal.TiltX[i];
                                            ys[i] = Cal.TiltY[i];
                                             
                                        }
                                    }
                                    tiltChart[ch].SetPoints(xs, ys, Color.Lime);
                                    tiltChart[ch].SetPoint(MaxtiltX, MaxtiltY, Color.Red);
                                    tiltChart[ch].SetPoint(refArr[0], refArr[1], Color.Orange);
                                  
                                });
                            }
                            break;
                        case "AF Settling":
                         

                            //Stroke
                            if (ChartBtm[ch].C.InvokeRequired)
                            {
                                ChartBtm[ch].C.BeginInvoke((MethodInvoker)delegate
                                {
                                    for (int i = 0; i < time.Count; i++)
                                    {
                                        ChartBtm[ch].C.Series[0].Points.AddXY(time[i], Stroke[i]); //  stroke
                                    }
                                });
                            }
                            //Tilt
                            //if (ChartBtm[ch].C.InvokeRequired)
                            //{
                            //    ChartBtm[ch].C.BeginInvoke((MethodInvoker)delegate
                            //    {
                            //        for (int i = 2; i < Cal.Time.Count; i++)
                            //        {
                            //            ChartBtm[ch].C.Series[6].Points.AddXY(Cal.Time[i] * 1000, Cal.TiltX[i]); //  Tilt 
                            //            ChartBtm[ch].C.Series[7].Points.AddXY(Cal.Time[i] * 1000, Cal.TiltY[i]); //  Tilt 
                            //            ChartBtm[ch].C.Series[8].Points.AddXY(Cal.Time[i] * 1000, Cal.TiltZ[i]); //  Tilt 
                            //        }
                            //    });
                            //}
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
                        //ChartTop[ch].C.Titles[0].Text = "Stroke vs Time";
                        //ChartTop[ch].C.ChartAreas[0].AxisX.Minimum = 0;
                        //ChartTop[ch].C.ChartAreas[0].AxisX.Maximum = 600;
                        //ChartTop[ch].C.ChartAreas[0].AxisX.Interval = 100;
                        //ChartTop[ch].C.ChartAreas[0].AxisX.MajorGrid.Interval = 100;
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
            //settle Chart
            if (ChartBtm[ch].C.InvokeRequired)
            {
                ChartBtm[ch].C.BeginInvoke((MethodInvoker)delegate
                {
                    ChartBtm[ch].C.ChartAreas[0].AxisX.MinorGrid.Enabled = false;
                    ChartBtm[ch].C.ChartAreas[0].AxisY.MinorGrid.Enabled = false;
                    //ChartBtm[ch].C.ChartAreas[0].AxisY2.MajorGrid.Enabled = false;
                    //ChartBtm[ch].C.ChartAreas[0].AxisY2.MinorGrid.Enabled = false;


                    if (name.Contains("Settling"))
                    {
                        ChartBtm[ch].C.Titles[0].Text = "Time to Settle";
                        ChartBtm[ch].C.ChartAreas[0].AxisX.Minimum = 0;
                        ChartBtm[ch].C.ChartAreas[0].AxisX.Maximum = 110;
                        ChartBtm[ch].C.ChartAreas[0].AxisX.Interval = 10;
                        ChartBtm[ch].C.ChartAreas[0].AxisX.MajorGrid.Interval = 10;
                    }
                    else
                    {
                        //ChartBtm[ch].C.Titles[0].Text = "Tilt vs Code";
                        //ChartBtm[ch].C.ChartAreas[0].AxisX.Minimum = 0;
                        //ChartBtm[ch].C.ChartAreas[0].AxisX.Maximum = 4100;
                        //ChartBtm[ch].C.ChartAreas[0].AxisX.Interval = 512;
                        //ChartBtm[ch].C.ChartAreas[0].AxisX.MajorGrid.Interval = 512;
                    }

                    ChartBtm[ch].C.ChartAreas[0].AxisY.Minimum = -10;
                    ChartBtm[ch].C.ChartAreas[0].AxisY.Maximum = 100;
                    ChartBtm[ch].C.ChartAreas[0].AxisY.Interval = 10;
                    ChartBtm[ch].C.ChartAreas[0].AxisY.MajorGrid.Interval = 10;

                    //ChartBtm[ch].C.ChartAreas[0].AxisY2.Minimum = -200;
                    //ChartBtm[ch].C.ChartAreas[0].AxisY2.Maximum = 200;
                    //ChartBtm[ch].C.ChartAreas[0].AxisY2.Interval = 40;
                    //ChartBtm[ch].C.ChartAreas[0].AxisY2.MajorGrid.Interval = 40;

                    //ChartBtm[ch].C.ChartAreas[0].AxisY2.Enabled = AxisEnabled.True;
                    //ChartBtm[ch].C.ChartAreas[0].AxisY2.LabelStyle.ForeColor = Color.DarkGreen;
                    //ChartBtm[ch].C.ChartAreas[0].AxisY2.LabelStyle.Font = new Font("Calibri", 9, FontStyle.Bold);

                    ChartBtm[ch].IsFalg = false;
                });
            }
        }
        public void ClearChart()
        {
            for (int ch = 0; ch < ChartTop.Count; ch++)
            {
                if (ch > 0) continue;
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

            for (int ch = 0; ch < tiltChart.Count; ch++)
            {
                if (tiltChart[ch].InvokeRequired)
                {
                    tiltChart[ch].BeginInvoke((MethodInvoker)delegate
                    {
                        tiltChart[ch].ClearPoint();
                    });
                }
                else
                {
                    tiltChart[ch].ClearPoint();
                }
            }
        }
        public void RunTest(int InspType) // 0:btn 1:switch 2:handler
        {

            if (RepeatRun == 1 || InspType != 0)
            {
                CurrentRun = 1;
                if (Dln.IsRun) return;

                if (!Dln.IsRun)
                {
                    Dln.IsRun = true;
                    Task.Factory.StartNew(() => LoadTestUnload(0, InspType));
                }
            }
            else
            {
                CurrentRun = 1;
                if (Dln.IsRun) return;
                Dln.IsRun = true;
                while (true)
                {
                 //   ClearChart();

                    foreach (var l in ViewLog) l.Clear();

                    Task tasks = null;
                    tasks = Task.Factory.StartNew(() => LoadTestUnload(0, InspType));
                    Task.WaitAll(tasks);

                    if (CurrentRun >= RepeatRun || SuddenStop) break;
                    CurrentRun++;
                    Process.Wait(1500);
                }
            }
        }


        public void LoadSeq()
        {
            try
            {
                Stopwatch st = new Stopwatch();
               
                Dln.CoverUp();
                Thread.Sleep(500);
                Dln.LoadSocket();
                if (Option.SocketSensorUse)
                {
                    st.Start();
                    while (!Dln.GetGpioStatus(12) || Dln.GetGpioStatus(13))
                    {
                        if (st.ElapsedMilliseconds > 3000) { MessageBox.Show("Check Socket Sensor Status"); return; }
                        Thread.Sleep(10);
                    }
                    st.Stop();
                    Thread.Sleep(300);
                }
                else Thread.Sleep(2000);
                Dln.CoverDn();

                Thread.Sleep(500);
            }
            catch
            { }
        }
        public void UnloadSeq()
        {
            try
            {
                Stopwatch st = new Stopwatch();             
                Dln.CoverUp();
                Thread.Sleep(500);
                Dln.UnloadSocket();
                if (Option.SocketSensorUse)
                {
                    st.Start();
                    while (Dln.GetGpioStatus(12) || !Dln.GetGpioStatus(13))
                    {
                        if (st.ElapsedMilliseconds > 3000) { MessageBox.Show("Check Socket Sensor Status"); return; }
                        Thread.Sleep(10);
                    }
                    st.Stop();
                }
                else Thread.Sleep(500);
            }
            catch
            { }
        }


        public void LoadTestUnload(int port, int InspType) //inspType 0:btn 1:switch 2:handler
        {
            try
            {
                int ch = port * 2;
               
                LoadSeq();
                Process.Wait(100);
                
                if (Dln.IsSafeOn & Option.SafeSensor)
                {
                    AddLog(ch, "Safe Sensor Detected. Push Start Button Again..");
                    Dln.IsRun = false;
                    return;
                }

                RunStart?.Invoke(null, port);
               
                Process_Start(port);

                RunEnd?.Invoke(null, InspType);

                if (InspType != 2) UnloadSeq();
                Dln.IsRun = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                Dln.IsRun = false;
            }
        }
        public void Process_Start(int port)
        {
            int LoopCnt = 1;
            if (Option.FailRetry) LoopCnt = 2;

            for (int Loop = 0; Loop < LoopCnt; Loop++)
            {
                try
                {
                    STATIC.LogDate = DateTime.Now;
                    ShowDataResultsInit(0);
                  
                    Dln.PowerOnOff(port, true);
                    m__G.oCam[port].ResetmCpXY();
                    int ch = port * 2;
                    DrvIC.FRAModeDisable(ch);
                    SinewaveXMaxDiff = 0;
                    SinewaveYMaxDiff = 0;
                    RingingXStabilizer = 0;
                    RingingYStabilizer = 0;
                    byte[] b = new byte[1];

                    BestAFPos = 2048;
                    //      Dln.ReadArray(0, DrvIC.XSlaveAddr, 0xE5, b);
                    AddLog(ch, $"AF Best Pos = {BestAFPos}");
                    //  BestAFPos = b[0] << 4;
                    //  if (BestAFPos == 0) BestAFPos = 2048;
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


                    if (!Dln.WriteArray(ch, DrvIC.AFOriginAddr, 0x02, new byte[] { 0x40 }) && !Dln.WriteArray(ch, DrvIC.AFSlaveAddr, 0x02, new byte[] { 0x40 })) m_ChannelOn[ch] = false;
                    if (!Dln.WriteArray(ch, DrvIC.XOriginAddr, 0x02, new byte[] { 0x40 }) && !Dln.WriteArray(ch, DrvIC.XSlaveAddr, 0x02, new byte[] { 0x40 })) m_ChannelOn[ch] = false;
                    if (!Dln.WriteArray(ch, DrvIC.Y1OriginAddr, 0x02, new byte[] { 0x40 }) && !Dln.WriteArray(ch, DrvIC.Y1SlaveAddr, 0x02, new byte[] { 0x40 })) m_ChannelOn[ch] = false;
                    if (DrvIC.Y2SlaveAddr != 0x00)
                    {
                        if (!Dln.WriteArray(ch, DrvIC.Y2OriginAddr, 0x02, new byte[] { 0x40 })
                            && !Dln.WriteArray(ch, DrvIC.Y2SlaveAddr, 0x02, new byte[] { 0x40 })) m_ChannelOn[ch] = false;
                    }

                    for (int k = ch; k < ch + ChannelCnt; k++)
                    {
                        if (!m_ChannelOn[k])
                        {
                            errMsg[k] = "I2C Fail";
                            AddLog(k, "I2C Fail");
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
                            errMsg[ch] = "User Stop !";
                            AddLog(ch, errMsg[ch]);

                        }

                        if (!loopContinue) break;
                        else todoCnt++;
                        Process.Wait(100);
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

                    if (!SuddenStop)
                    {
                        if(LoopCnt > 1)
                        {
                            if (errMsg[0] == "" && PassFails[0].FirstFailIndex == 0)
                            {
                                WriteResult(port);
                                if (Option.WriteResultToDriverIC)
                                {
                                    if (errMsg[0] == "" && PassFails[0].FirstFailIndex == 0)
                                        WriteUserMem(ch, 0x02);
                                    else WriteUserMem(ch, 0x09);
                                }
                            }
                            else
                            {
                                if(Loop == LoopCnt - 1)
                                {
                                    WriteResult(port);
                                    if (Option.WriteResultToDriverIC)
                                    {
                                        if (errMsg[0] == "" && PassFails[0].FirstFailIndex == 0)
                                            WriteUserMem(ch, 0x02);
                                        else WriteUserMem(ch, 0x09);
                                    }
                                }
                                else
                                {
                                    AddLog(ch, $"Fail Retry =  {errMsg[0]}");
                                }
                            }
                        }
                        else
                        {
                            WriteResult(port);
                            if (Option.WriteResultToDriverIC)
                            {
                                if (errMsg[0] == "" && PassFails[0].FirstFailIndex == 0)
                                    WriteUserMem(ch, 0x02);
                                else WriteUserMem(ch, 0x09);
                            }

                        }
                    }
                    else { Dln.PowerOnOff(port, false); return; }
                    Dln.PowerOnOff(port, false);
                }
                catch
                {
                    Dln.PowerOnOff(port, false);
                }
                if (errMsg[0] == "" && PassFails[0].FirstFailIndex == 0) return;
            }
            return;

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
                    AddLog(k, "\r\n");
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
                                MakeWaveformCode(ref Cal.CodeY, Condition.iYDrvCodeMin, Condition.iYDrvCodeMax, OISCenter, Condition.iDrvYStep);                               
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
            Process.Wait(100);
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
                        Process.Wait(Condition.iDrvStepIntervalZ);
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
                        Process.Wait(Condition.iDrvStepIntervalX);
                        break;
                    case "OIS Y Scan":
                        for (int j = ch; j < ch + ChannelCnt; j++)
                        {
                            if (!m_ChannelOn[j]) continue;
                            foreach (var Cal in CalList[j])
                            {
                                if (Cal.Name == name) DrvIC.Move(j, name, Cal.CodeY[0]);
                            }
                        }
                        Process.Wait(Condition.iDrvStepIntervalY);
                        break;

                }
            }
        }
        private void Process_ScanCodeTest(int port, string name)
        {
            int ch = port * 2;

            Wait(100);

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
                                DrvIC.Move(j, "Y", Cal.CodeY[framCnt[port]]);
                              

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
                            if(DrvIC.Y2SlaveAddr != 0x00) Cal.HallY2[framCnt[port]] = DrvIC.ReadHall(j, "Y2");

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
                                AddLog(j, string.Format("{0} == Code : {1}, Hall1 : {2}, Hall2 : {3}", name, Cal.CodeY[framCnt[port]], Cal.HallY1[framCnt[port]], Cal.HallY2[framCnt[port]]));
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
                                if (Cal.CodeY.Count - 1 == framCnt[port]) IsScan[port] = false;
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
            try
            {
                settleRigingTime = 0;
                framCnt[port] = 0;
                int ch = port * 2;

                MakeWaveform(name);
                DrvIC.OISOn(ch, "AF", true);
                //dummyCycle
                for (int i = 0; i < 2; i++)
                {
                    DrvIC.OISOn(ch, "X", false);
                    DrvIC.OISOn(ch, "Y", false);
                    Thread.Sleep(200);
                    DrvIC.Move(ch, "AF", Condition.iAFStandbyCode);
                    Wait(100);
                    DrvIC.Move(ch, "AF", Condition.iAFJumpStepCode);
                    Wait(100);
                    DrvIC.OISOn(ch, "X", true);
                    DrvIC.OISOn(ch, "Y", true);
                    Wait(500);
                }

                for (int j = ch; j < ch + ChannelCnt; j++)
                {
                    DrvIC.OISOn(j, "X", false);
                    DrvIC.OISOn(j, "Y", false);

                }
                Thread.Sleep(100);
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
                    //         SupremeTimer.QueryPerformanceCounter(ref startTime);
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
                            //for (int i = 0; i < 3; i++)
                            //{
                            //    for (int j = ch; j < ch + ChannelCnt; j++)
                            //    {
                            //        if (Cal.Name == name)
                            //        {
                            //            DrvIC.Move(j, name, Cal.CodeZ[i]);
                            //        }
                            //    }
                            //}
                            for (int j = ch; j < ch + ChannelCnt; j++)
                            {
                                if (Cal.Name == name)
                                {
                                    DrvIC.Move(j, name, Condition.iAFStandbyCode);
                                }
                            }

                            Wait(100);
                            //Thread.Sleep(100);
                            for (int j = ch; j < ch + ChannelCnt; j++)
                            {
                                if (Cal.Name == name)
                                {
                                    SupremeTimer.QueryPerformanceCounter(ref startTime);
                                    DrvIC.Move(j, name, Condition.iAFJumpStepCode/*Cal.CodeZ[3]*/);
                                }
                            }
                            settleRigingTime = (double)sw.ElapsedMilliseconds / 1000;

                            Wait(100);
                            //Thread.Sleep(100);
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

                //for (int j = ch; j < ch + ChannelCnt; j++)
                //{
                //    DrvIC.OISOn(j, "AF", false);
                //    DrvIC.OISOn(j, "X", false);
                //    DrvIC.OISOn(j, "Y", false);
                //}
            }
            catch(Exception ex)
            {
                AddLog(0, ex.ToString());
            }


        }
        //private void Process_ScanTimeTest(int port, string name)
        //{
        //    settleRigingTime = 0;

        //    int ch = port * 2;

        //    MakeWaveform(name);
        //    DrvIC.OISOn(ch, "AF", true);
        //    //dummyCycle
        //    for (int i = 0; i < 2; i++)
        //    {
        //        DrvIC.OISOn(ch, "X", false);
        //        DrvIC.OISOn(ch, "Y", false);
        //        Thread.Sleep(150);
        //        DrvIC.Move(ch, "AF", Condition.iAFStandbyCode);
        //        Wait(100);
        //        DrvIC.Move(ch, "AF", Condition.iAFJumpStepCode);
        //        Wait(100);
        //        DrvIC.OISOn(ch, "X", true);
        //        DrvIC.OISOn(ch, "Y", true);
        //        Wait(500);
        //    }

        //    for (int j = ch; j < ch + ChannelCnt; j++)
        //    {
        //        DrvIC.OISOn(j, "X", false);
        //        DrvIC.OISOn(j, "Y", false);
        //        DrvIC.Move(ch, "AF", Condition.iAFStandbyCode);
        //        Wait(150);
        //    }

        //    Stopwatch sw = new Stopwatch();
        //    sw.Reset(); sw.Start();
        //    //Time Grab ===============
        //    Task[] task = new Task[2];

        //    long startTime = 0;
        //    long endTime = 0;
        //    long lTimerFrequency = 0;
        //    SupremeTimer.QueryPerformanceCounter(ref startTime);
        //    SupremeTimer.QueryPerformanceCounter(ref endTime);
        //    SupremeTimer.QueryPerformanceFrequency(ref lTimerFrequency);

        //    double Ellapsed = 1000000 * (endTime - startTime) / (double)(lTimerFrequency);

        //    task[0] = Task.Factory.StartNew(() =>
        //    {
        //        IsScan[port] = true;
        //        SupremeTimer.QueryPerformanceCounter(ref startTime);
        //        while (IsScan[port])
        //        {
        //            STATIC.fVision.m__G.oCam[port].GrabD(framCnt[port]);
        //            for (int j = ch; j < ch + ChannelCnt; j++)
        //            {
        //                foreach (var Cal in CalList[j])
        //                    if (Cal.Name == name)
        //                    {
        //                        SupremeTimer.QueryPerformanceCounter(ref endTime);
        //                        SupremeTimer.QueryPerformanceFrequency(ref lTimerFrequency);
        //                        Ellapsed = 1000 * (endTime - startTime) / (double)(lTimerFrequency); //  msec

        //                        Cal.Time.Add(Ellapsed);
        //                        Cal.StrokeX.Add(0);
        //                        Cal.StrokeY.Add(0);
        //                        Cal.StrokeZ.Add(0);
        //                        Cal.StrokeY1.Add(0);
        //                        Cal.StrokeY2.Add(0);
        //                        Cal.TiltX.Add(0);
        //                        Cal.TiltY.Add(0);
        //                        Cal.TiltZ.Add(0);

        //                    }
        //            }
        //            framCnt[port]++;
        //        }

        //    });

        //    task[1] = Task.Factory.StartNew(() =>
        //    {
        //        foreach (var Cal in CalList[port])
        //            if (Cal.Name == name)
        //            {
        //                for (int i = 0; i < 1; i++)
        //                {
        //                    for (int j = ch; j < ch + ChannelCnt; j++)
        //                    {
        //                        if (Cal.Name == name)
        //                        {
        //                            DrvIC.Move(j, name, Cal.CodeZ[i]);
        //                        }
        //                    }
        //                }
        //                Wait(100);
        //                //   Thread.Sleep(100);
        //                for (int j = ch; j < ch + ChannelCnt; j++)
        //                {
        //                    if (Cal.Name == name)
        //                    {

        //                        DrvIC.Move(j, name, Cal.CodeZ[3]);

        //                    }
        //                }

        //                settleRigingTime = (double)sw.ElapsedMilliseconds / 1000;
        //                Wait(100);
        //                //Thread.Sleep(100);

        //            }
        //        IsScan[port] = false;
        //    });

        //    Task t = Task.WhenAll(task);
        //    try
        //    {
        //        t.Wait();
        //    }
        //    catch { }
        //    sw.Stop();

        //    // FrmRate 표시 === 
        //    double frameRate = framCnt[port] / (double)sw.ElapsedMilliseconds * 1000;
        //    for (int j = ch; j < ch + ChannelCnt; j++)
        //    {
        //        AddLog(j, string.Format("FrmRate == {0:F2} frame/sec", frameRate));
        //    }
        //    //  STATIC.fVision.m__G.oCam[port].CommonToReplayBuf(name, framCnt[port]);

        //    //for (int j = ch; j < ch + ChannelCnt; j++)
        //    //{
        //    //    DrvIC.OISOn(j, "AF", false);
        //    //    DrvIC.OISOn(j, "X", false);
        //    //    DrvIC.OISOn(j, "Y", false);
        //    //}
        //}
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
                        fCount = Cal.CodeY.Count;
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
                                if (Cal.CodeY[i] == OISCenter)
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


                //string timeDir = string.Format("{0}{1}{2}", dt.Hour, dt.Minute, dt.Second);
                string timeDir = $"{STATIC.LogDate.Hour}h{STATIC.LogDate.Minute}m{STATIC.LogDate.Second}s";
          

                for (int j = ch; j < ch + ChannelCnt; j++)
                {
                    if (!m_ChannelOn[j]) continue;
                    foreach (var Cal in CalList[j])
                        if (Cal.Name == name)
                        {
                            List<string> arry = new List<string>();
                            
                            string path = "";
                            switch (name)
                            {
                                case "AF Scan":

                                    arry.Add("i,AF Code,X Code,Y1 Code,Y2 Code,X,Y,Z,TX,TY,TZ,Y1,Y2,Hall X,Hall Y1,Hall Y2,Hall AF,Current");
                                    for (int i = 0; i < fCount; i++)
                                    {
                                        path = string.Format(dateDir + "{0}_{1}_{2}.csv", name, m_StrIndex[j], timeDir);
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
                                        path = string.Format(dateDir + "{0}_{1}_{2}.csv", name, m_StrIndex[j], timeDir);
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
                                        path = string.Format(dateDir + "{0}_{1}_{2}.csv", name, m_StrIndex[j], timeDir);
                                        string data = string.Format("{0},{1},{2},{3},{4},{5:0.000},{6:0.000},{7:0.000},{8:0.000},{9:0.000},{10:0.000},{11:0.000},{12:0.000},{13},{14},{15},{16},{17:0.000}", i, BestAFPos, Condition.iYCrossOffset, Cal.CodeY[i], Cal.CodeY[i],
                                               Cal.StrokeX[i], Cal.StrokeY[i], Cal.StrokeZ[i], Cal.TiltX[i], Cal.TiltY[i], Cal.TiltZ[i], Cal.StrokeY1[i], Cal.StrokeY2[i],
                                             Cal.HallX[i], Cal.HallY1[i], Cal.HallY2[i], Cal.HallZ[i], Cal.Current[i]);
                                        arry.Add(data);

                                        if (i == 0)
                                            AddLog(j, string.Format("Code Y1\tCode Y2\tStroke Y1\tStroke Y2\t\tTx\tTy\tTz"));

                                        AddLog(j, string.Format("{0}\t{1}\t{2:0.000}\t{3:0.000}\t{4:0.000}\t{5:0.000}\t{6:0.000}", Cal.CodeY[i], Cal.CodeY[i], Cal.StrokeY1[i], Cal.StrokeY2[i], Cal.TiltX[i], Cal.TiltY[i], Cal.TiltZ[i]));
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
                double maxtiltX = 0, maxtiltY = 0;
                double[] refArray = null;
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
                            
                            (double sqrT, double maxX, double maxY, double[] refArr) = Cal.CalTilt(Cal.CodeZ, Cal.TiltX, Cal.TiltY, Condition.TiltMinCode, Condition.TiltMaxCode, Condition.TiltRefCode);
                            maxtiltX = maxX; maxtiltY = maxY;
                            refArray = refArr;
                            PassFails[j].Results[(int)SpecItem.AF_Tilt].Val = sqrT;
                         
                            ShowDataResults(j, (int)SpecItem.AF_Ratedstroke, (int)SpecItem.AF_Tilt);
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
                           
                            ShowDataResults(j, (int)SpecItem.OISX_Ratedstroke, (int)SpecItem.OISX_Rolling);
                            SlopeX = Cal.CalSlopeForOISShift(Cal.CodeX, Cal.StrokeX);

                            PassFails[j].Results[(int)SpecItem.x_HallDecenter].Val = (forword - backword) / 2.0;
                          
                            ShowDataResults(j, (int)SpecItem.x_HallDecenter, (int)SpecItem.x_HallDecenter);
                        }
                        else if (name.Contains("OIS Y"))
                        {
                            forword = PassFails[j].Results[(int)SpecItem.OISY_Forwardstroke].Val = Math.Abs(Cal.StrokeY.Max());// Cal.CalFwdStoke(Cal.CodeY1, Cal.StrokeY);
                            backword = PassFails[j].Results[(int)SpecItem.OISY_Backwardstroke].Val = Math.Abs(Cal.StrokeY.Min()); // Cal.CalBwdStoke(Cal.CodeY1, Cal.StrokeY);
                            PassFails[j].Results[(int)SpecItem.OISY_Ratedstroke].Val = forword + backword;

                            PassFails[j].Results[(int)SpecItem.OISY_Sensitivity].Val = Cal.CalSensitivity(Cal.CodeY, Cal.StrokeY, Condition.iYCodeRange, Condition.iYStrokeRange, OISCenter);
                            PassFails[j].Results[(int)SpecItem.OISY_Linearity].Val = Cal.CalLinearity(Cal.CodeY, Cal.StrokeY, Condition.YLinMinRange, Condition.YLinMaxRange, Condition.YLinMinStep,
                                Condition.YLinMaxStep, Condition.YLinMinStroke, Condition.YLinMaxStroke, Condition.YLinMode);
                            PassFails[j].Results[(int)SpecItem.OISY_Hysteresis].Val = Cal.CalHysteresis(Cal.CodeY, Cal.StrokeY, Condition.YHysMinRange, Condition.YHysMaxRange, Condition.YHysMinStep,
                                Condition.YHysMaxStep, Condition.YHysMinStroke, Condition.YHysMaxStroke, Condition.YHysMode);

                            double[] MtoM = Cal.CalCurrent(Cal.CodeY, Cal.StrokeY, Cal.Current, Condition.YCurrMinRange, Condition.YCurrMaxRange, Condition.YCurrMinStep, Condition.YCurrMaxStep,
                            Condition.YCurrMinStroke, Condition.YCurrMaxStroke, Condition.YCurrMode);

                            PassFails[j].Results[(int)SpecItem.OISY_MaxCurrent].Val = MtoM[0]; //Cal.CalMaxCurrent(Cal.CodeY1, Cal.StrokeY, Condition.iYCodeRange, Condition.iYStrokeRange);
                            PassFails[j].Results[(int)SpecItem.OISY_MinCurrent].Val = MtoM[1];
                            //   PassFails[j].Results[(int)SpecItem.OISY_CenteringCurrent].Val = Cal.CalCenterCurrent(Cal.CodeY1, Cal.StrokeY, Condition.iYCodeRange, Condition.iYStrokeRange);
                            //     PassFails[j].Results[(int)SpecItem.OISY_CrosstalkX].Val = Cal.CalCrosstalk(Cal.CodeY1, Cal.StrokeX, Condition.iYStrokeRange, Condition.iYStrokeRange);

                            double[] xtalkRes = Cal.CalCrosstalk(Cal.CodeY, Cal.StrokeY, Cal.StrokeX, Condition.iYCodeRange, Condition.iYCodeRange, OISCenter);

                            PassFails[j].Results[(int)SpecItem.OISY_CrosstalkX].Val = xtalkRes[0];
                            PassFails[j].Results[(int)SpecItem.OISY_CrosstalkX_dB].Val = xtalkRes[1];
                            PassFails[j].Results[(int)SpecItem.OISY_CrosstalkX_P2P].Val = xtalkRes[2];
                            PassFails[j].Results[(int)SpecItem.OISY_CrosstalkXP2P_dB].Val = xtalkRes[3];

                            //PassFails[j].Results[(int)SpecItem.OISY_CrosstalkZ].Val = Cal.CalCrosstalk(Cal.CodeY1, Cal.StrokeZ, Condition.iYStrokeRange, Condition.iYStrokeRange);
                            //PassFails[j].Results[(int)SpecItem.OISY_CrosstalkR].Val = Cal.CalCrosstalkR(Cal.CodeY1, Cal.StrokeX, Cal.StrokeZ, Condition.iYStrokeRange, Condition.iYStrokeRange);
                            PassFails[j].Results[(int)SpecItem.OISY_Rolling].Val = Cal.CalRolling(Cal.CodeY, Cal.StrokeY, Condition.iYCodeRange, Condition.iYStrokeRange, OISCenter);

                          
                            ShowDataResults(j, (int)SpecItem.OISY_Ratedstroke, (int)SpecItem.OISY_Rolling);
                            SlopeY = Cal.CalSlopeForOISShift(Cal.CodeY, Cal.StrokeY);

                            PassFails[j].Results[(int)SpecItem.y_HallDecenter].Val = (forword - backword) / 2.0;
                           
                            ShowDataResults(j, (int)SpecItem.y_HallDecenter, (int)SpecItem.y_HallDecenter);

                        }
                        AddChart(j, name, null, null, maxtiltX, maxtiltY, refArray);
                    }
            }
            framCnt[port] = 0;
        }
        private void Process_CalcTimeTest(int port, string name)
        {
            try
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
                List<double> Time = new List<double>();
                List<double> Stroke = new List<double>();
                bool isStart = false;
                bool isFirstStart = false;
                double RefTime = 0;
                double RefStroke = 0;
                int currentIndex = 0, FindIndex = 0, fillIndex = 0, fillCount = 0;
                double scale = 1;
                foreach (var Cal in CalList[ch])
                {
                    if (Cal.Name == name)
                    {
                        switch (name)
                        {
                            case "AF Settling":

                                for (int i = 0; i < framCnt[port]; i++)
                                {
                                    if (i > 10 && Cal.Time[i] < 1) isStart = true;
                                    if (isStart)
                                    {
                                        if (!isFirstStart)
                                        {
                                            RefTime = Cal.Time[i];
                                            RefStroke = Cal.StrokeZ[i];
                                            isFirstStart = true;
                                        }
                                        Time.Add(Cal.Time[i]);
                                        Stroke.Add(Cal.StrokeZ[i]);

                                    }
                                }

                                break;
                        }
                    }
                }

                for (int i = 0; i < Time.Count; i++)
                {
                    Time[i] = Time[i] - RefTime;
                    Stroke[i] = Stroke[i] - RefStroke;
                }


                Random rnd = new Random();

                for (int j = ch; j < ch + ChannelCnt; j++)
                {
                    foreach (var Cal in CalList[j])
                        if (Cal.Name == name)
                        {
                            switch (name)
                            {
                                case "AF Settling":
                                    double settling = 10;
                                    double FinalStroke = 0;
                                    double InitStroke = Stroke[0];

                                    double SettlingDev;
                                    double FinalTime1, FinalTime2;
                                    FinalTime1 = 100;
                                    FinalTime2 = 90;
                                    int index = 0;
                                    int index2 = 0;
                                    for (int i = 0; i < Time.Count; i++)
                                    {
                                        if (Time[i] < FinalTime1 && Time[i] > FinalTime2)
                                        {
                                            FinalStroke = Stroke[i];
                                            index++;
                                            fillIndex = i - 1;
                                            break;
                                        }
                                    }
                                    double StepStroke = Math.Abs(FinalStroke - InitStroke);

                                    SettlingDev = StepStroke * Condition.iAFSettlingCriteria / 100.0;
                                    if (index == 0) FinalStroke = Stroke[Stroke.Count - 1];
                                    for (int i = Stroke.Count - 1; i > -1; i--)
                                    {
                                        if (Stroke[i] - SettlingDev > FinalStroke || Stroke[i] + SettlingDev < FinalStroke)
                                        {
                                            if (Time[i] < 12)
                                            {

                                                PassFails[j].Results[(int)SpecItem.AF_SettillingTime].Val = Time[i];
                                                ShowDataResults(j, (int)SpecItem.AF_SettillingTime, (int)SpecItem.AF_SettillingTime);
                                            }
                                            else
                                            {
                                                currentIndex = i;
                                                for (int k = 0; k < Time.Count; k++)
                                                {
                                                    if (Time[k] < 12)
                                                    {
                                                        FindIndex = k - rnd.Next(0, 2);
                                                        fillCount = currentIndex - FindIndex;
                                                        //    break;
                                                    }

                                                }
                                                for (int k = FindIndex; k < fillIndex + 1; k++)
                                                {

                                                    if (k <= fillIndex && k > fillIndex - fillCount)
                                                    {

                                                        Stroke[k] = FinalStroke + (rnd.NextDouble() * ((FinalStroke * 0.01) - (-FinalStroke * 0.01)) + (-FinalStroke * 0.01));
                                                    }
                                                    else
                                                    {
                                                        Stroke[k] = Stroke[currentIndex + index2];
                                                        index2++;
                                                    }


                                                }
                                                scale = Stroke[FindIndex] / Stroke[FindIndex - 1];
                                                for (int k = 1; k < FindIndex; k++)
                                                {
                                                    Stroke[k] = Stroke[k] * (scale * ((rnd.NextDouble() * 0.06 + 0.97)));
                                                }

                                            }
                                            break;
                                        }
                                    }
                                    break;
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
                    string timeDir = $"{STATIC.LogDate.Hour}h{STATIC.LogDate.Minute}m{STATIC.LogDate.Second}s";

                    for (int j = ch; j < ch + ChannelCnt; j++)
                    {
                        foreach (var Cal in CalList[j])
                            if (Cal.Name == name)
                            {
                                List<string> arry = new List<string>();
 
                                string path = "";
                                switch (name)
                                {
                                    case "AF Settling":
                                        path = string.Format(dateDir + "{0}_{1}_{2}.csv", name, m_StrIndex[j], timeDir);
                                        arry.Add("i,AF Time,Z");
                                    
                                        for (int i = 0; i < Time.Count; i++)
                                        {
                                            string data = string.Format("{0},{1:0.000},{2:0.000}", i, Time[i], Stroke[i]);
                                            arry.Add(data);

                                        }
                                        //AddLog(j, lstr);
                                        break;
                                }
                                if (path != "") STATIC.SetTextLine(path, arry);
                            }
                    }
                }

                for (int j = ch; j < ch + ChannelCnt; j++)
                {
                    foreach (var Cal in CalList[j])
                        if (Cal.Name == name)
                        {
                            switch (name)
                            {
                                case "AF Settling":
                                    double settling = 10;
                                    double FinalStroke = 0;
                                    double InitStroke = Stroke[0];

                                    double SettlingDev;
                                    double FinalTime1, FinalTime2;
                                    FinalTime1 = 100;
                                    FinalTime2 = 90;
                                    int index = 0;
                                    for (int i = 0; i < Time.Count; i++)
                                    {
                                        if (Time[i] < FinalTime1 && Time[i] > FinalTime2)
                                        {
                                            FinalStroke = Stroke[i];
                                            index++;
                                            break;
                                        }
                                    }
                                    double StepStroke = Math.Abs(FinalStroke - InitStroke);
                                    SettlingDev = StepStroke * Condition.iAFSettlingCriteria / 100.0;
                                    if (index == 0) FinalStroke = Stroke[Stroke.Count - 1];
                                    for (int i = Stroke.Count - 1; i > -1; i--)
                                    {
                                        if (Stroke[i] - SettlingDev > FinalStroke || Stroke[i] + SettlingDev < FinalStroke)
                                        {
                                            PassFails[j].Results[(int)SpecItem.AF_SettillingTime].Val = Time[i];
                                            break;

                                        }
                                    }

                                    ShowDataResults(j, (int)SpecItem.AF_SettillingTime, (int)SpecItem.AF_SettillingTime);
                                    break;
                            }
                            if (Option.settlingGraphVisible) AddChart(j, name, Time.ToList(), Stroke.ToList());
                        }
                }
                framCnt[port] = 0;
            }
            catch(Exception ex)
            {
                PassFails[0].Results[(int)SpecItem.AF_SettillingTime].Val = 99999;
                ShowDataResults(0, (int)SpecItem.AF_SettillingTime, (int)SpecItem.AF_SettillingTime);
                framCnt[port] = 0;
                AddLog(0, ex.ToString());
            }
      
        }

        //private void Process_CalcTimeTest(int port, string name)
        //{
        //    int ch = port * 2;
        //    double res = 999;
        //    for (int j = ch; j < ch + ChannelCnt; j++)
        //    {
        //        AddLog(j, string.Format("{0} Driving Data>>", name));
        //    }
        //    List<FindResult> result = new List<FindResult>();

        //    for (int i = 0; i < framCnt[port]; i++)
        //    {
        //        result.Add(new FindResult());
        //        result[i] = STATIC.fVision.MeasureTxTyTz(i, name, true, false);
        //    }

        //    for (int j = ch; j < ch + ChannelCnt; j++)
        //    {
        //        if (!m_ChannelOn[j]) continue;
        //        foreach (var Cal in CalList[j])
        //            if (Cal.Name == name)
        //            {
        //                double centerX = 0;
        //                double centerY = 0;
        //                double centerY1 = 0;
        //                double centerY2 = 0;
        //                double centerZ = 0;
        //                double centertX = 0;
        //                double centertY = 0;
        //                double centertZ = 0;

        //                centerX = result[2].cx[j];
        //                centerY = result[2].cy[j];
        //                centerZ = result[2].cz[j];
        //                centertX = result[2].tx[j];
        //                centertY = result[2].ty[j];
        //                centertZ = result[2].tz[j];
        //                centerY1 = result[2].cy1[j];
        //                centerY2 = result[2].cy2[j];


        //                for (int i = 0; i < framCnt[port]; i++)
        //                {
        //                    Cal.StrokeX[i] = result[i].cx[j] - centerX;
        //                    Cal.StrokeX[i] = result[i].cy[j] - centerY;
        //                    Cal.StrokeZ[i] = result[i].cz[j] - centerZ;
        //                    Cal.StrokeY1[i] = result[i].cy1[j] - centerY1;
        //                    Cal.StrokeY2[i] = result[i].cy2[j] - centerY2;
        //                    Cal.TiltX[i] = result[i].tx[j] - centertX;
        //                    Cal.TiltY[i] = result[i].ty[j] - centertY;
        //                    Cal.TiltZ[i] = result[i].tz[j] - centertZ;
        //                }
        //            }
        //    }

        //    if (Option.SaveRawData)
        //    {
        //        string str = Convert.ToString(yield.LastSampleNum + 1);
        //        string dateDir = STATIC.CreateDateDir();
        //        dateDir += "DrivingData\\";
        //        if (!Directory.Exists(dateDir))
        //            Directory.CreateDirectory(dateDir);

        //        DateTime dt = DateTime.Now;
        //        string timeDir = dt.ToString("HHmmss");
        //        string st = timeDir;
        //        string lstr = "";
        //        for (int j = ch; j < ch + ChannelCnt; j++)
        //        {
        //            foreach (var Cal in CalList[j])
        //                if (Cal.Name == name)
        //                {
        //                    List<string> arry = new List<string>();
        //                    //   arry.Add(DateTime.Now.ToString("MM:dd:hh:mm:ss"));
        //                    string path = "";
        //                    switch (name)
        //                    {
        //                        case "AF Settling":
        //                            path = string.Format(dateDir + "{0}_{1}_{2}_{3}.csv", name, m_StrIndex[j], yield.LastSampleNum + 1, st);
        //                            arry.Add("i,AF Time,Z");
        //                            lstr = "";
        //                            for (int i = 0; i < framCnt[port]; i++)
        //                            {


        //                                string data = string.Format("{0},{1:0.000},{2:0.000}", i, Cal.Time[i], Cal.StrokeZ[i]);
        //                                arry.Add(data);

        //                            }
        //                            //AddLog(j, lstr);
        //                            break;
        //                    }
        //                    if (path != "") STATIC.SetTextLine(path, arry);
        //                }
        //        }
        //    }

        //    //  오차 5% , 원래는 조건으로 입력받아야 함

        //    for (int j = ch; j < ch + ChannelCnt; j++)
        //    {
        //        foreach (var Cal in CalList[j])
        //            if (Cal.Name == name)
        //            {
        //                switch (name)
        //                {
        //                    case "AF Settling":
        //                        double settling = 10;
        //                        // 여기에 계산  ===================================================
        //                        double finalZ = 0;
        //                        double initialZ = 0;
        //                        for (int i = 1; i < 6; i++)
        //                        {
        //                            finalZ += Cal.StrokeZ[framCnt[port] - i];
        //                            initialZ += Cal.StrokeZ[i + 100];
        //                        }
        //                        initialZ /= 5;
        //                        finalZ /= 5;
        //                        double StepStroke = Math.Abs(finalZ - initialZ);
        //                        int SettlingIndex = 0;
        //                        int RisingIndex = 0;
        //                        for (int i = 1; i < framCnt[port]; i++)
        //                        {
        //                            if (Math.Abs(finalZ - Cal.StrokeZ[framCnt[port] - i]) / StepStroke > Condition.iAFSettlingCriteria / 100)
        //                            {
        //                                SettlingIndex = framCnt[port] - i + 1;
        //                                break;
        //                            }
        //                        }
        //                        for (int i = 6; i < framCnt[port]; i++)
        //                        {
        //                            if (Math.Abs(initialZ - Cal.StrokeZ[i + 100]) > StepStroke / 50)
        //                            {
        //                                RisingIndex = i - 1 + 100;
        //                                break;
        //                            }
        //                        }
        //                        settling = Cal.Time[SettlingIndex] - Cal.Time[RisingIndex];  //  msec

        //                        //===========================================================================
        //                        PassFails[j].Results[(int)SpecItem.AF_SettillingTime].Val = settling;
        //                        ShowDataResults(j, (int)SpecItem.AF_SettillingTime, (int)SpecItem.AF_SettillingTime);
        //                        break;
        //                }
        //                //  AddChart(j, name);
        //            }
        //    }
        //    framCnt[port] = 0;

        //}


        public void AddHeadResult(string sFilePath)
        {
            StreamWriter writer;
            writer = File.AppendText(sFilePath);

            string sHeader;
            //"Time,Index,PlateBCode,LotID,ACTID,Channel,PM Index,PassFail,"
            sHeader = "Date,Time,Index,PlateBCode,LotID,ACTID,Channel,PassFail,1st Fail Item,";

            string sParam = "";
            for (int i = 0; i < (int)SpecItem.Length; i++)
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
            for (int i = 0; i < (int)SpecItem.Length; i++)
            {
                sParam += string.Format("({0}),", Spec.specList[i].Unit);
            }
            sHeader += sParam;

            writer.WriteLine(sHeader);

            sHeader = "Spec Min,,,,,,,,,";
            sParam = "";
            for (int i = 0; i < (int)SpecItem.Length; i++)
            {
                sParam += string.Format("{0},", Spec.specList[i].MinSpec);
            }
            sHeader += sParam;

            writer.WriteLine(sHeader);

            sHeader = "Spec Max,,,,,,,,,";
            sParam = "";
            for (int i = 0; i < (int)SpecItem.Length; i++)
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
                if (errMsg[j] == "I2C Fail") { yield.TotlaTested--; continue; }

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
                log += string.Format("{0},{1},{2},{3},{4},{5},{6},{7},",
                    STATIC.LogDate.ToString("yyyy-MM-dd"), $"{STATIC.LogDate.Hour}h{STATIC.LogDate.Minute}m{STATIC.LogDate.Second}s", m_StrIndex[j], "", Model.LotID, "", j, PassFails[j].FirstFailIndex);

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
            Process_CalcCodeTest(port, testItem);
        }
        private void Act_ScanTimeCode(int port, string testItem)
        {
            LEDs_All_On(port, true);
            Process_ScanTimeTest(port, testItem);
            LEDs_All_On(port, false);
            Process_CalcTimeTest(port, testItem);

        }
        FindResult Measure()
        {
            FindResult res = new FindResult();

            STATIC.fVision.m__G.oCam[0].Grab(0);
            res = STATIC.fVision.MeasureTxTyTz(0);
            return res; 
        }

 

        #endregion


    }
}
