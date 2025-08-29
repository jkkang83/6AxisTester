using OpenCvSharp.Flann;
using S2System.Vision;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
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
                CalList[i].Add(new CalResult("AF Scan2"));
                CalList[i].Add(new CalResult("AF Scan3"));
                CalList[i].Add(new CalResult("AF Scan4"));
                CalList[i].Add(new CalResult("AF Settling"));
                CalList[i].Add(new CalResult("OIS X Scan"));
                CalList[i].Add(new CalResult("OIS X Scan2"));
                CalList[i].Add(new CalResult("OIS X Scan3"));
                CalList[i].Add(new CalResult("OIS X Scan4"));
                CalList[i].Add(new CalResult("OIS Y Scan"));
                CalList[i].Add(new CalResult("OIS Y Scan2"));
                CalList[i].Add(new CalResult("OIS Y Scan3"));
                CalList[i].Add(new CalResult("OIS Y Scan4"));
                CalList[i].Add(new CalResult("OIS X Linearity Comp"));
                CalList[i].Add(new CalResult("OIS Y Linearity Comp"));
                CalList[i].Add(new CalResult("OIS X Linearity Comp2"));
                CalList[i].Add(new CalResult("OIS Y Linearity Comp2"));
                CalList[i].Add(new CalResult("OIS X EPA"));
                CalList[i].Add(new CalResult("OIS Y EPA"));
                CalList[i].Add(new CalResult("OIS Matrix Scan"));

                ChartTop.Add(new ChartList("Stroke", i));
                ChartBtm.Add(new ChartList("Tilt", i));

                InfoBtn.Add(new InfoButton());
                ViewLog.Add(new LogText());
            }

            ItemList.Add(new ActItems() { Name = "Change Slave Addr", Func = Act_ChangeSlaveAddr, IsMulti = true });
            ItemList.Add(new ActItems() { Name = "Data Check", Func = Act_Data_Check, IsMulti = true });
            ItemList.Add(new ActItems() { Name = "PID Setting", Func = Act_PIDSetting, IsMulti = true });
            ItemList.Add(new ActItems() { Name = "Aging OpenLoop", Func = Act_AgingOpenLoop, IsMulti = true });
            ItemList.Add(new ActItems() { Name = "Hall Calibration", Func = Act_HallCalibration, IsMulti = true });
            ItemList.Add(new ActItems() { Name = "Linearity Save", Func = Act_LinearitySave, IsMulti = true });
            ItemList.Add(new ActItems() { Name = "Linearity Delete", Func = Act_LinearityDelete, IsMulti = true });
            ItemList.Add(new ActItems() { Name = "Linearity Restore", Func = Act_LinearityRestore, IsMulti = true });

            ItemList.Add(new ActItems() { Name = "AF Scan", Func = Act_ScanCode });
            ItemList.Add(new ActItems() { Name = "AF Scan", Func = Act_ScanCode });
            ItemList.Add(new ActItems() { Name = "AF Scan2", Func = Act_ScanCode });
            ItemList.Add(new ActItems() { Name = "AF Scan3", Func = Act_ScanCode });
            ItemList.Add(new ActItems() { Name = "AF Scan4", Func = Act_ScanCode });
            ItemList.Add(new ActItems() { Name = "AF Settling", Func = Act_ScanTimeCode });
            ItemList.Add(new ActItems() { Name = "OIS X Scan", Func = Act_ScanCode });
            ItemList.Add(new ActItems() { Name = "OIS X Scan2", Func = Act_ScanCode });
            ItemList.Add(new ActItems() { Name = "OIS X Scan3", Func = Act_ScanCode });
            ItemList.Add(new ActItems() { Name = "OIS X Scan4", Func = Act_ScanCode });
            ItemList.Add(new ActItems() { Name = "OIS Y Scan", Func = Act_ScanCode });
            ItemList.Add(new ActItems() { Name = "OIS Y Scan2", Func = Act_ScanCode });
            ItemList.Add(new ActItems() { Name = "OIS Y Scan3", Func = Act_ScanCode });
            ItemList.Add(new ActItems() { Name = "OIS Y Scan4", Func = Act_ScanCode });
            ItemList.Add(new ActItems() { Name = "OIS Matrix Scan", Func = Act_ScanCode });
            ItemList.Add(new ActItems() { Name = "OIS X EPA", Func = Act_OIS_EPA });
            ItemList.Add(new ActItems() { Name = "OIS Y EPA", Func = Act_OIS_EPA });
            ItemList.Add(new ActItems() { Name = "OIS X EPA Recipe", Func = Act_OIS_EPA_Recipe, IsMulti = true });
            ItemList.Add(new ActItems() { Name = "OIS Y EPA Recipe", Func = Act_OIS_EPA_Recipe, IsMulti = true });
            ItemList.Add(new ActItems() { Name = "OIS X Ex EPA Recipe", Func = Act_OIS_ExEPA_Recipe, IsMulti = true });
            ItemList.Add(new ActItems() { Name = "OIS Y Ex EPA Recipe", Func = Act_OIS_ExEPA_Recipe, IsMulti = true });
            ItemList.Add(new ActItems() { Name = "OIS X Linearity Comp", Func = Act_OISLineartitycomp });
            ItemList.Add(new ActItems() { Name = "OIS Y Linearity Comp", Func = Act_OISLineartitycomp });
            ItemList.Add(new ActItems() { Name = "OIS X Linearity Comp2", Func = Act_OISLineartitycomp });
            ItemList.Add(new ActItems() { Name = "OIS Y Linearity Comp2", Func = Act_OISLineartitycomp });
            ItemList.Add(new ActItems() { Name = "Gain@10Hz", Func = Act_GaindB10Hz, IsMulti = true });
            ItemList.Add(new ActItems() { Name = "Phase Margin", Func = Act_Phase_Margin, IsMulti = true });
            ItemList.Add(new ActItems() { Name = "Gain Margin", Func = Act_Gain_Margin, IsMulti = true });
            ItemList.Add(new ActItems() { Name = "OIS Hall Test", Func = Act_OISHallTest, IsMulti = true });
            ItemList.Add(new ActItems() { Name = "Hall Decenter", Func = HallDecenter, IsMulti = true });
            ItemList.Add(new ActItems() { Name = "Servo Decenter", Func = ServoDecenter, IsMulti = true });
        }
        public void ShowDataResults(int ch, string key)
        {
            if (ResultDataGrid.InvokeRequired)
            {
                ResultDataGrid.BeginInvoke((MethodInvoker)delegate
                {
                    for (int i = 0; i < Spec.Param.Count; i++)
                    {
                        if (Spec.Param[i][0].ToString() != key) continue;

                        if (Spec.PassFails[ch].Results[i].Val != 0)
                        {
                            if (key.Contains("FRA") || key.Contains("Gyro"))
                            {
                                ResultDataGrid[ch + 4, i].Value = Spec.PassFails[ch].Results[i].Val.ToString("F1");
                            }
                            else
                            {
                                ResultDataGrid[ch + 4, i].Value = Spec.PassFails[ch].Results[i].Val.ToString("F3");
                            }
                        }
                        if (Spec.PassFails[ch].Results[i].bPass) ResultDataGrid[ch + 4, i].Style.BackColor = Color.White;
                        else ResultDataGrid[ch + 4, i].Style.BackColor = Color.Orange;
                    }
                });
            }
            else
            {
                for (int i = 0; i < Spec.Param.Count; i++)
                {
                    if (Spec.Param[i][0].ToString() != key) continue;

                    if (Spec.PassFails[ch].Results[i].Val != 0)
                    {
                        if (key.Contains("FRA") || key.Contains("Gyro"))
                        {
                            ResultDataGrid[ch + 4, i].Value = Spec.PassFails[ch].Results[i].Val.ToString("F1");
                        }
                        else
                        {
                            ResultDataGrid[ch + 4, i].Value = Spec.PassFails[ch].Results[i].Val.ToString("F3");
                        }
                    }
                    if (Spec.PassFails[ch].Results[i].bPass) ResultDataGrid[ch + 4, i].Style.BackColor = Color.White;
                    else ResultDataGrid[ch + 4, i].Style.BackColor = Color.Orange;
                }
            }
        }
        public void InitResultData()
        {
            ResultDataGrid.Tag = "S";
            ResultDataGrid.ColumnCount = 9; //  Group, Item, min, max, r0, r1, r2, r3, unit, Fratio
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
            ResultDataGrid.Columns[6].DefaultCellStyle.Alignment = DataGridViewContentAlignment.TopRight;

            ResultDataGrid.Columns[0].Width = 80;
            ResultDataGrid.Columns[1].Width = 180;
            ResultDataGrid.Columns[2].Width = 70;
            ResultDataGrid.Columns[3].Width = 70;
            ResultDataGrid.Columns[4].Width = 90;
            ResultDataGrid.Columns[5].Width = 90;
            ResultDataGrid.Columns[6].Width = 90;

            ResultDataGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            ResultDataGrid.ColumnHeadersHeight = 28;

            bool bColorChange = true;
            ResultDataGrid.Rows.Clear();
            for (int i = 0; i < Spec.Param.Count; i++)
            {
                if (i > 0)
                    if (Spec.Param[i - 1][0].ToString() != Spec.Param[i][0].ToString())
                        bColorChange = !bColorChange;

                ResultDataGrid.Rows.Add(Spec.Param[i][0], Spec.Param[i][1], Spec.Param[i][2], Spec.Param[i][3], Spec.Param[i][4], Spec.Param[i][5], Spec.Param[i][9]);

                if (bColorChange) for (int k = 0; k < ResultDataGrid.ColumnCount; k++) ResultDataGrid[k, i].Style.BackColor = Color.Lavender;
                else for (int k = 0; k < ResultDataGrid.ColumnCount; k++) ResultDataGrid[k, i].Style.BackColor = Color.White;

                ResultDataGrid.Rows[i].Visible = Convert.ToBoolean(Spec.Param[i][10]);

                ResultDataGrid.Rows[i].Height = 22;
                ResultDataGrid.Rows[i].Resizable = DataGridViewTriState.False;
                ResultDataGrid.Rows[i].DefaultCellStyle.Font = new Font("Calibri", 12, FontStyle.Bold);
                ResultDataGrid[1, i].Style.Font = new Font("Calibri", 12, FontStyle.Bold);
                ResultDataGrid[2, i].Style.Font = new Font("Calibri", 12, FontStyle.Bold);
                ResultDataGrid[6, i].Style.Font = new Font("Calibri", 12, FontStyle.Italic);

                ResultDataGrid.ReadOnly = true;
            }

            string oldkey = "";
            for (int i = 0; i < Spec.Param.Count; i++)
            {
                if (ResultDataGrid.Rows[i].Visible)
                {
                    string newKey = ResultDataGrid.Rows[i].Cells[0].Value.ToString();
                    if (oldkey == newKey) ResultDataGrid.Rows[i].Cells[0].Value = "";
                    oldkey = newKey;
                }
            }
        }
        public void ShowDataResultsInit(int ch)
        {
            if (ResultDataGrid.InvokeRequired)
            {
                ResultDataGrid.BeginInvoke((MethodInvoker)delegate
                {
                    Spec.InitResult(ch);
                    for (int i = 0; i < Spec.Param.Count; i++)
                    {
                        ResultDataGrid[ch + 4, i].Value = Spec.PassFails[ch].Results[i].Val.ToString("F0");
                        ResultDataGrid[ch + 4, i].Style.BackColor = Color.White;
                    }
                });
            }
            else
            {
                Spec.InitResult(ch);
                for (int i = 0; i < Spec.Param.Count; i++)
                {
                    ResultDataGrid[ch + 4, i].Value = Spec.PassFails[ch].Results[i].Val.ToString("F0");
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
                        case "OIS X Scan2":
                        case "OIS X Scan3":
                        case "OIS X Scan4":
                            CodeRange = Condition.iXPlotRange;
                            //Stroke
                            if (ChartTop[ch].C.InvokeRequired)
                            {
                                ChartTop[ch].C.BeginInvoke((MethodInvoker)delegate
                                {
                                    ChartTop[ch].C.Series[0].Points.Clear();

                                    for (int i = 2; i < Cal.CodeX.Count; i++)
                                    {
                                        if (Cal.CodeX[i] >= 2048 - CodeRange && Cal.CodeX[i] <= 2048 + CodeRange)
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
                                        if (Cal.CodeX[i] >= 2048 - CodeRange && Cal.CodeX[i] <= 2048 + CodeRange)
                                        {
                                            ChartBtm[ch].C.Series[0].Points.AddXY(Cal.CodeX[i], Cal.TiltX[i]); //  Tilt 
                                            ChartBtm[ch].C.Series[1].Points.AddXY(Cal.CodeX[i], Cal.TiltY[i]); //  Tilt 
                                            ChartBtm[ch].C.Series[2].Points.AddXY(Cal.CodeX[i], Cal.TiltZ[i]); //  Tilt 
                                        }
                                    }
                                });
                            }
                            break;
                        case "OIS Y Scan":
                        case "OIS Y Scan2":
                        case "OIS Y Scan3":
                        case "OIS Y Scan4":
                            CodeRange = Condition.iYPlotRange;
                            //Stroke
                            if (ChartTop[ch].C.InvokeRequired)
                            {
                                ChartTop[ch].C.BeginInvoke((MethodInvoker)delegate
                                {
                                    for (int i = 2; i < Cal.CodeY1.Count; i++)
                                    {
                                        if (Cal.CodeY1[i] >= 2048 - CodeRange && Cal.CodeY1[i] <= 2048 + CodeRange)
                                        {
                                            ChartTop[ch].C.Series[1].Points.AddXY(Cal.CodeY1[i], Cal.StrokeY[i]); //  stroke
                                            ChartTop[ch].C.Series[9].Points.AddXY(Cal.CodeY1[i], Cal.StrokeY1[i]); //  stroke 1
                                            ChartTop[ch].C.Series[10].Points.AddXY(Cal.CodeY2[i], Cal.StrokeY2[i]); //  stroke 2
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
                                        if (Cal.CodeY1[i] >= 2048 - CodeRange && Cal.CodeY1[i] <= 2048 + CodeRange)
                                        {
                                            ChartBtm[ch].C.Series[3].Points.AddXY(Cal.CodeY1[i], Cal.TiltX[i]); //  Tilt 
                                            ChartBtm[ch].C.Series[4].Points.AddXY(Cal.CodeY1[i], Cal.TiltY[i]); //  Tilt 
                                            ChartBtm[ch].C.Series[5].Points.AddXY(Cal.CodeY1[i], Cal.TiltZ[i]); //  Tilt 
                                        }
                                    }
                                });
                            }
                            break;
                        case "AF Scan":
                        case "AF Scan2":
                        case "AF Scan3":
                        case "AF Scan4":
                            CodeRange = Condition.iAFPlotRange;
                            //Stroke
                            if (ChartTop[ch].C.InvokeRequired)
                            {
                                ChartTop[ch].C.BeginInvoke((MethodInvoker)delegate
                                {
                                    for (int i = 2; i < Cal.CodeZ.Count; i++)
                                    {
                                        if (Cal.CodeZ[i] >= 2048 - CodeRange && Cal.CodeZ[i] <= 2048 + CodeRange)
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
                                        if (Cal.CodeZ[i] >= 2048 - CodeRange && Cal.CodeZ[i] <= 2048 + CodeRange)
                                        {
                                            ChartBtm[ch].C.Series[6].Points.AddXY(Cal.CodeZ[i], Cal.TiltX[i]); //  Tilt 
                                            ChartBtm[ch].C.Series[7].Points.AddXY(Cal.CodeZ[i], Cal.TiltY[i]); //  Tilt 
                                            ChartBtm[ch].C.Series[8].Points.AddXY(Cal.CodeZ[i], Cal.TiltZ[i]); //  Tilt 
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
                        case "OIS Matrix Scan":
                            CodeRange = Condition.iXPlotRange;
                            //Stroke
                            if (ChartTop[ch].C.InvokeRequired)
                            {
                                ChartTop[ch].C.BeginInvoke((MethodInvoker)delegate
                                {
                                    ChartTop[ch].C.Series[0].Points.Clear();

                                    for (int i = 2; i < Cal.CodeX.Count; i++)
                                    {
                                        if (Cal.CodeX[i] >= 2048 - CodeRange && Cal.CodeX[i] <= 2048 + CodeRange)
                                        {
                                            ChartTop[ch].C.Series[0].Points.AddXY(Cal.CodeX[i], Cal.StrokeX[i]); //  stroke
                                            ChartTop[ch].C.Series[3].Points.AddXY(Cal.CodeX[i], Cal.Current[i]); //  current
                                            ChartTop[ch].C.Series[6].Points.AddXY(Cal.CodeX[i], Cal.HallX[i] / 10); //  hall
                                        }
                                    }
                                });
                            }

                            CodeRange = Condition.iYPlotRange;
                            //Stroke
                            if (ChartTop[ch].C.InvokeRequired)
                            {
                                ChartTop[ch].C.BeginInvoke((MethodInvoker)delegate
                                {
                                    for (int i = 2; i < Cal.CodeY1.Count; i++)
                                    {
                                        if (Cal.CodeY1[i] >= 2048 - CodeRange && Cal.CodeY1[i] <= 2048 + CodeRange)
                                        {
                                            ChartTop[ch].C.Series[9].Points.AddXY(Cal.CodeY1[i], Cal.StrokeY1[i]); //  stroke 1
                                            ChartTop[ch].C.Series[10].Points.AddXY(Cal.CodeY2[i], Cal.StrokeY2[i]); //  stroke 2
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
                                        if (Cal.CodeY1[i] >= 2048 - CodeRange && Cal.CodeY1[i] <= 2048 + CodeRange)
                                        {
                                            ChartBtm[ch].C.Series[0].Points.AddXY(Cal.CodeY1[i], Cal.TiltX[i]); //  Tilt 
                                            ChartBtm[ch].C.Series[1].Points.AddXY(Cal.CodeY1[i], Cal.TiltY[i]); //  Tilt 
                                            ChartBtm[ch].C.Series[2].Points.AddXY(Cal.CodeY1[i], Cal.TiltZ[i]); //  Tilt 
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
                                        if (Cal.CodeY1[i] >= 2048 - CodeRange && Cal.CodeY1[i] <= 2048 + CodeRange)
                                        {
                                            ChartBtm[ch].C.Series[3].Points.AddXY(Cal.CodeY1[i], Cal.TiltX[i]); //  Tilt 
                                            ChartBtm[ch].C.Series[4].Points.AddXY(Cal.CodeY1[i], Cal.TiltY[i]); //  Tilt 
                                            ChartBtm[ch].C.Series[5].Points.AddXY(Cal.CodeY1[i], Cal.TiltZ[i]); //  Tilt 
                                        }
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
                while (true)
                {
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
            }
        }
        public void Process_Start(int port)
        {
            try
            {
                int ch = port * 2;
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
                    Spec.PassFails[k].FirstFailIndex = 0;
                }

                //m_ChannelOn[1] = false; // 1ch Test

                for (int k = ch; k < ch + ChannelCnt; k++)
                {
                    if (!m_ChannelOn[k])
                    {
                        errMsg[k] = "Socket Empty";
                        AddLog(k, "Socket Empty");
                    }
                }
                if (errMsg[ch] != "" && errMsg[ch + 1] != "")
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

                    if (errMsg[ch] != "" && errMsg[ch + 1] != "")
                    {
                        loopContinue = false;
                        AddLog(ch, errMsg[ch]);
                        AddLog(ch + 1, errMsg[ch + 1]);
                    }
                    if (SuddenStop)
                    {
                        loopContinue = false;
                        errMsg[ch] = errMsg[ch + 1] = "SuddenStop !";
                        AddLog(ch, errMsg[ch]);
                        AddLog(ch + 1, errMsg[ch + 1]);
                    }

                    if (!loopContinue) break;
                    else todoCnt++;
                    Thread.Sleep(100);
                }
                LEDs_All_On(port, false);

                double ellipse = (double)sw.ElapsedMilliseconds / 1000;
                sw.Stop();

                Spec.LastSampleNum++;

                for (int k = ch; k < ch + ChannelCnt; k++)
                {
                    AddLog(k, string.Format("Total Test Time\t{0:0.000} sec", ellipse));
                    Spec.PassFails[k].TotalTime = ellipse.ToString("F3");
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
            if (!m_ChannelOn[ch] && !m_ChannelOn[ch + 1])
                return;

            for (int k = ch; k < ch + ChannelCnt; k++)
            {
                if (m_ChannelOn[k])
                {
                    //m_StrIndex[k] = (Spec.LastSampleNum + k + 1).ToString();
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
        //===============================================================================================================================
        private void Act_ChangeSlaveAddr(int ch, string testItem)
        {
            //if (ch == 1) return;
            if (!DrvIC.ChangeSlaveAddr(ch))
            {
                errMsg[ch] = string.Format("{0} Error", testItem);
                m_ChannelOn[ch] = false;
            }
        }
        private void Act_PIDSetting(int ch, string testItem)
        {
            if (!DrvIC.PIDSetting(ch, Rcp.AfPidSet.Param, 0))
            {
                errMsg[ch] = string.Format("{0} Error", testItem);
                m_ChannelOn[ch] = false;
            }
            if (!DrvIC.PIDSetting(ch, Rcp.XPidSet.Param, 1))
            {
                errMsg[ch] = string.Format("{0} Error", testItem);
                m_ChannelOn[ch] = false;
            }
            if (!DrvIC.PIDSetting(ch, Rcp.YPidSet.Param, 2))
            {
                errMsg[ch] = string.Format("{0} Error", testItem);
                m_ChannelOn[ch] = false;
            }
        }
        private void Act_AgingOpenLoop(int ch, string testItem)
        {
            if (!DrvIC.AgingOpenLoop(ch, testItem))
            {
                errMsg[ch] = string.Format("{0} Error", testItem);
                m_ChannelOn[ch] = false;
            }
        }
        private void Act_LinearitySave(int ch, string testItem)
        {
            if (!DrvIC.LinearitySave(ch))
            {
                errMsg[ch] = string.Format("{0} Error", testItem);
                m_ChannelOn[ch] = false;
            }
        }
        private void Act_LinearityDelete(int ch, string testItem)
        {
            if (!DrvIC.LinearityDelete(ch))
            {
                errMsg[ch] = string.Format("{0} Error", testItem);
                m_ChannelOn[ch] = false;
            }
        }
        private void Act_LinearityRestore(int ch, string testItem)
        {
            if (!DrvIC.LinearityRestore(ch))
            {
                errMsg[ch] = string.Format("{0} Error", testItem);
                m_ChannelOn[ch] = false;
            }
        }
        private void Act_HallCalibration(int ch, string testItem)
        {
            if (!DrvIC.EPA_Set(ch, "X", 0, 0))
            {
                errMsg[ch] = string.Format("Reset EPA Error");
                m_ChannelOn[ch] = false;
            }
            if (!DrvIC.EPA_Set(ch, "Y1", 0, 0))
            {
                errMsg[ch] = string.Format("Reset EPA Error");
                m_ChannelOn[ch] = false;
            }
            if (!DrvIC.EPA_Set(ch, "Y2", 0, 0))
            {
                errMsg[ch] = string.Format("Reset EPA Error");
                m_ChannelOn[ch] = false;
            }
            if (!m_ChannelOn[ch]) return;
            if (!DrvIC.HallCalibration(ch, Condition.HallCalMode))
            {
                errMsg[ch] = string.Format("{0} Error", testItem);
                m_ChannelOn[ch] = false;
            }
        }
        private void Act_Data_Check(int ch, string testItem)
        {
            if (!DrvIC.Data_Check(ch))
            {
                errMsg[ch] = string.Format("{0} Error", testItem);
                m_ChannelOn[ch] = false;
            }
        }
        private void Act_OIS_EPA(int port, string testItem)
        {
            int[] centerCode = new int[6];  //  Hall 값이 (2048,2048,2048) 에 가장 가까와지는  code 값
                                            //  ch1_x, ch1_y1, ch1_y2, ch2_x, ch2_y1, ch2_y2
                                            //  centerCode 위치에서 FWD 방향 구동거리와 BWD 방향 구동거리가 같아지는 Code 값
            int[] epaCodeMin = new int[6];  //  ch1_x, ch1_y1, ch1_y2,     ch2_x, ch2_y1, ch2_y2

            int[] epaCodeMax = new int[6];  //  ch1_x, ch1_y1, ch1_y2,     ch2_x, ch2_y1, ch2_y2\

            Process_FindEPA(port, testItem, ref centerCode, ref epaCodeMin, ref epaCodeMax);

            int ch = port * 2;
            for (int j = ch; j < ch + ChannelCnt; j++)
            {
                if (!m_ChannelOn[j]) continue;
                AddLog(j, string.Format("epaCodeMin X : {0}, Y1 : {1}, Y2 : {2}", epaCodeMin[0 + 3 * j], epaCodeMin[1 + 3 * j], epaCodeMin[2 + 3 * j]));
                AddLog(j, string.Format("epaCodeMax X : {0}, Y1 : {1}, Y2 : {2}", 4096 - epaCodeMax[0 + 3 * j], 4096 - epaCodeMax[1 + 3 * j], 4096 - epaCodeMax[2 + 3 * j]));
            }

            if (testItem.Contains("X"))
            {
                for (int j = ch; j < ch + ChannelCnt; j++)
                {
                    if (!m_ChannelOn[j]) continue;
                    if (!DrvIC.EPA_Set(j, "X", 4096 - epaCodeMax[0 + 3 * j], epaCodeMin[0 + 3 * j]))
                    {
                        errMsg[j] = string.Format("{0} Error", testItem);
                        m_ChannelOn[j] = false;
                    }
                }
            }
            else
            {
                for (int j = ch; j < ch + ChannelCnt; j++)
                {
                    if (!m_ChannelOn[j]) continue;
                    if (!DrvIC.EPA_Set(j, "Y1", 4096 - epaCodeMax[1 + 3 * j], epaCodeMin[1 + 3 * j]))
                    {
                        errMsg[j] = string.Format("{0} Error", testItem);
                        m_ChannelOn[j] = false;
                    }
                    if (!DrvIC.EPA_Set(j, "Y2", 4096 - epaCodeMax[2 + 3 * j], epaCodeMin[2 + 3 * j]))
                    {
                        errMsg[j] = string.Format("{0} Error", testItem);
                        m_ChannelOn[j] = false;
                    }
                }
            }


        }
        private void Act_OIS_EPA_Recipe(int ch, string testItem)
        {
            if (testItem.Contains("X"))
            {
                if (!DrvIC.EPA_Set(ch, "X", Condition.iXEPACutTop, Condition.iXEPACutBottom))
                {
                    errMsg[ch] = string.Format("{0} Error", testItem);
                    m_ChannelOn[ch] = false;
                }
            }
            else
            {
                if (!DrvIC.EPA_Set(ch, "Y1", Condition.iY1EPACutTop, Condition.iY1EPACutBottom))
                {
                    errMsg[ch] = string.Format("{0} Error", testItem);
                    m_ChannelOn[ch] = false;
                }
                if (!DrvIC.EPA_Set(ch, "Y2", Condition.iY2EPACutTop, Condition.iY2EPACutBottom))
                {
                    errMsg[ch] = string.Format("{0} Error", testItem);
                    m_ChannelOn[ch] = false;
                }
            }
        }
        private void Act_OIS_ExEPA_Recipe(int ch, string testItem)
        {
            if (testItem.Contains("X"))
            {
                if (!DrvIC.Ex_EPA_Set(ch, "X", Condition.iXEPAExTop, Condition.iXEPAExBottom))
                {
                    errMsg[ch] = string.Format("{0} Error", testItem);
                    m_ChannelOn[ch] = false;
                }
            }
            else
            {
                if (!DrvIC.Ex_EPA_Set(ch, "Y1", Condition.iY1EPAExTop, Condition.iY1EPAExBottom))
                {
                    errMsg[ch] = string.Format("{0} Error", testItem);
                    m_ChannelOn[ch] = false;
                }
                if (!DrvIC.Ex_EPA_Set(ch, "Y2", Condition.iY2EPAExTop, Condition.iY2EPAExBottom))
                {
                    errMsg[ch] = string.Format("{0} Error", testItem);
                    m_ChannelOn[ch] = false;
                }
            }
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
        private void Act_GaindB10Hz(int ch, string testItem)
        {
            int amp;

            DrvIC.OISOn(ch, testItem, false);

            //X
            amp = (int)Condition.iLoppgainXAmp;
            AddLog(ch, string.Format("X FRA =="));

            List<double> freq = new List<double>();
            List<double> gain = new List<double>();
            List<double> phase = new List<double>();
            freq.Add(10);

            if (!DrvIC.FRA_Single(ch, "X", amp, freq, ref gain, ref phase))
            {
                errMsg[ch] = string.Format("{0} Error", testItem);
                m_ChannelOn[ch] = false;
            }
            else
            {
                AddLog(ch, string.Format("FRA X Gain10Hz = {0:0.000}",
                    Spec.PassFails[ch].Results[(int)SpecItem.FRAX_Gain10Hz].Val = gain[0]));

                Spec.SetResult(ch, (int)SpecItem.FRAX_Gain10Hz, (int)SpecItem.FRAX_Gain10Hz);
                ShowDataResults(ch, "FRA X");
            }
            //Y1
            amp = (int)Condition.iLoppgainYAmp;
            AddLog(ch, string.Format("Y1 FRA =="));

            freq = new List<double>();
            gain = new List<double>();
            phase = new List<double>();
            freq.Add(10);

            if (!DrvIC.FRA_Single(ch, "Y1", amp, freq, ref gain, ref phase))
            {
                errMsg[ch] = string.Format("{0} Error", testItem);
                m_ChannelOn[ch] = false;
            }
            else
            {
                AddLog(ch, string.Format("FRA Y1 Gain10Hz = {0:0.000}",
                Spec.PassFails[ch].Results[(int)SpecItem.FRAY1_Gain10Hz].Val = gain[0]));

                Spec.SetResult(ch, (int)SpecItem.FRAY1_Gain10Hz, (int)SpecItem.FRAY1_Gain10Hz);
                ShowDataResults(ch, "FRA Y1");
            }
            //Y2
            amp = (int)Condition.iLoppgainYAmp;
            AddLog(ch, string.Format("Y2 FRA =="));

            freq = new List<double>();
            gain = new List<double>();
            phase = new List<double>();
            freq.Add(10);

            if (!DrvIC.FRA_Single(ch, "Y2", amp, freq, ref gain, ref phase))
            {
                errMsg[ch] = string.Format("{0} Error", testItem);
                m_ChannelOn[ch] = false;
            }
            else
            {
                AddLog(ch, string.Format("FRA Y2 Gain10Hz = {0:0.000}",
                Spec.PassFails[ch].Results[(int)SpecItem.FRAY2_Gain10Hz].Val = gain[0]));

                Spec.SetResult(ch, (int)SpecItem.FRAY2_Gain10Hz, (int)SpecItem.FRAY2_Gain10Hz);
                ShowDataResults(ch, "FRA Y2");
            }
        }
        private void Act_Phase_Margin(int ch, string testItem)
        {
            string axis;
            int startFreq;
            int EndFreq;
            int amp;

            double phaseIndex = 0;

            List<double> freq = new List<double>();
            List<double> gain = new List<double>();
            List<double> phase = new List<double>();

            //DrvIC.Move(ch, "AF", 2045);

            #region X PM
            axis = "X";
            startFreq = Condition.iXChirpFrom;
            EndFreq = Condition.iXChirpTo;
            amp = (int)Condition.iXAmplitude;

            AddLog(ch, string.Format("{0} FRA ==", axis));

            freq = new List<double>();
            gain = new List<double>();
            phase = new List<double>();

            for (int i = 0; i < Condition.iFRAloop; i++)
            {
                while (true)
                {
                    freq.Add(startFreq);
                    startFreq -= Condition.iFRAstep;
                    if (startFreq < EndFreq) break;
                }
            }

            if (!DrvIC.FRA_Single(ch, axis, amp, freq, ref gain, ref phase))
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
                AddLog(ch, string.Format("FRA X Freq = {0} PM = {1}",
                Spec.PassFails[ch].Results[(int)SpecItem.FRAX_PMFreq].Val = freq[(int)phaseIndex], Spec.PassFails[ch].Results[(int)SpecItem.FRAX_PhaseMargin].Val = 180 + (phase[(int)phaseIndex])));

                Spec.SetResult(ch, (int)SpecItem.FRAX_PMFreq, (int)SpecItem.FRAX_PhaseMargin);
                ShowDataResults(ch, "FRA X");
            }
            #endregion
            #region Y PM
            //Y1
            axis = "Y1";
            startFreq = Condition.iYChirpFrom;
            EndFreq = Condition.iYChirpTo;
            amp = (int)Condition.iYAmplitude;

            AddLog(ch, string.Format("{0} FRA ==", axis));

            freq = new List<double>();
            gain = new List<double>();
            phase = new List<double>();

            for (int i = 0; i < Condition.iFRAloop; i++)
            {
                while (true)
                {
                    freq.Add(startFreq);
                    startFreq -= Condition.iFRAstep;
                    if (startFreq < EndFreq) break;

                }
            }

            if (!DrvIC.FRA_Single(ch, axis, amp, freq, ref gain, ref phase))
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
                AddLog(ch, string.Format("FRA Y1 Freq = {0} PM = {1}",
                Spec.PassFails[ch].Results[(int)SpecItem.FRAY1_PMFreq].Val = freq[(int)phaseIndex], Spec.PassFails[ch].Results[(int)SpecItem.FRAY1_PhaseMargin].Val = 180 + (phase[(int)phaseIndex])));

                Spec.SetResult(ch, (int)SpecItem.FRAY1_PMFreq, (int)SpecItem.FRAY1_PhaseMargin);
                ShowDataResults(ch, "FRA Y1");
            }
            #endregion
            #region Y2 PM
            //Y2
            axis = "Y2";
            startFreq = Condition.iYChirpFrom;
            EndFreq = Condition.iYChirpTo;
            amp = (int)Condition.iYAmplitude;

            AddLog(ch, string.Format("{0} FRA ==", axis));

            freq = new List<double>();
            gain = new List<double>();
            phase = new List<double>();

            for (int i = 0; i < Condition.iFRAloop; i++)
            {
                while (true)
                {
                    freq.Add(startFreq);
                    startFreq -= Condition.iFRAstep;
                    if (startFreq < EndFreq) break;

                }
            }

            if (!DrvIC.FRA_Single(ch, axis, amp, freq, ref gain, ref phase))
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
                AddLog(ch, string.Format("FRA Y2 Freq = {0} PM = {1}",
                      Spec.PassFails[ch].Results[(int)SpecItem.FRAY2_PMFreq].Val = freq[(int)phaseIndex], Spec.PassFails[ch].Results[(int)SpecItem.FRAY2_PhaseMargin].Val = 180 + (phase[(int)phaseIndex])));

                Spec.SetResult(ch, (int)SpecItem.FRAY2_PMFreq, (int)SpecItem.FRAY2_PhaseMargin);
                ShowDataResults(ch, "FRA Y2");
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
                    startFreq -= Condition.iFRAstep;
                    if (startFreq < EndFreq) break;
                }
            }

            if (!DrvIC.FRA_Single(ch, axis, amp, freq, ref gain, ref phase))
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
                AddLog(ch, string.Format("FRA AF Freq = {0} PM = {1}",
                      Spec.PassFails[ch].Results[(int)SpecItem.FRAAF_PMFreq].Val = freq[(int)phaseIndex], Spec.PassFails[ch].Results[(int)SpecItem.FRAAF_PhaseMargin].Val = 180 + (phase[(int)phaseIndex])));

                Spec.SetResult(ch, (int)SpecItem.FRAAF_PMFreq, (int)SpecItem.FRAAF_PhaseMargin);
                ShowDataResults(ch, "FRA AF");
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
            return 0;
        }

        public int FindGainIndex(List<double> phase)
        {
            for (int i = 0; i < phase.Count; i++)
            {
                if (phase[i] >= 0)
                {
                    if (i == 0) return 0;
                    return i - 1;
                }
            }
            return 0;
        }
        private void Act_Gain_Margin(int ch, string testItem)
        {
            string axis;
            int startFreq;
            int EndFreq;
            int amp;

            DrvIC.OISOn(ch, testItem, false);
            //X
            axis = "X";
            startFreq = Condition.iXGainFrom;
            EndFreq = Condition.iXGainTo;
            amp = (int)Condition.iXAmplitudeGain;

            AddLog(ch, string.Format("{0} FRA ==", axis));

            List<double> freq = new List<double>();
            List<double> gain = new List<double>();
            List<double> phase = new List<double>();

            for (int i = 0; i < Condition.iGainLoop; i++)
            {
                while (true)
                {
                    freq.Add(startFreq);
                    startFreq -= Condition.iGainStep;
                    if (startFreq < EndFreq) break;

                }
            }
            if (!DrvIC.FRA_Single(ch, axis, amp, freq, ref gain, ref phase))
            {
                errMsg[ch] = string.Format("{0} Error", testItem);
                m_ChannelOn[ch] = false;
            }
            int gainIndex = FindGainIndex(phase);
            if (gainIndex < 1)
            {
                AddLog(ch, "X Find Gain Margin Failed.. Freq Range Check Please.");
                errMsg[ch] = string.Format("{0} Error", testItem);
                m_ChannelOn[ch] = false;
            }
            else
            {
                AddLog(ch, string.Format("FRA X GM = {0}", Spec.PassFails[ch].Results[(int)SpecItem.FRAX_GainMargin].Val = Math.Abs(gain[gainIndex])));
                Spec.SetResult(ch, (int)SpecItem.FRAX_GainMargin, (int)SpecItem.FRAX_GainMargin);
                ShowDataResults(ch, "FRA X");
            }

            //Y1
            axis = "Y1";
            AddLog(ch, string.Format("{0} FRA ==", axis));

            gain = new List<double>();
            phase = new List<double>();

            if (!DrvIC.FRA_Single(ch, axis, amp, freq, ref gain, ref phase))
            {
                errMsg[ch] = string.Format("{0} Error", testItem);
                m_ChannelOn[ch] = false;
            }
            gainIndex = FindGainIndex(phase);
            if (gainIndex < 1)
            {
                AddLog(ch, "Y1 Find Gain Margin Failed.. Freq Range Check Please.");
                errMsg[ch] = string.Format("{0} Error", testItem);
                m_ChannelOn[ch] = false;
            }
            else
            {
                AddLog(ch, string.Format("FRA Y1 GM = {0}", Spec.PassFails[ch].Results[(int)SpecItem.FRAY1_GainMargin].Val = Math.Abs(gain[gainIndex])));

                Spec.SetResult(ch, (int)SpecItem.FRAY1_PMFreq, (int)SpecItem.FRAY1_GainMargin);
                ShowDataResults(ch, "FRA Y1");
            }

            //Y2
            axis = "Y2";
            AddLog(ch, string.Format("{0} FRA ==", axis));

            gain = new List<double>();
            phase = new List<double>();

            if (!DrvIC.FRA_Single(ch, axis, amp, freq, ref gain, ref phase))
            {
                errMsg[ch] = string.Format("{0} Error", testItem);
                m_ChannelOn[ch] = false;
            }
            gainIndex = FindGainIndex(phase);
            if (gainIndex < 1)
            {
                AddLog(ch, "Y2 Find Gain Margin Failed.. Freq Range Check Please.");
                errMsg[ch] = string.Format("{0} Error", testItem);
                m_ChannelOn[ch] = false;
            }
            else
            {

                AddLog(ch, string.Format("FRA Y2 GM = {0}", Spec.PassFails[ch].Results[(int)SpecItem.FRAY2_GainMargin].Val = Math.Abs(gain[gainIndex])));

                Spec.SetResult(ch, (int)SpecItem.FRAY2_GainMargin, (int)SpecItem.FRAY2_GainMargin);
                ShowDataResults(ch, "FRA Y2");
            }
        }
        private void Act_OISHallTest(int ch, string testItem)
        {
            List<int> result;
            //SineWave Test
            if (Condition.SIN_AXIS == 0 || Condition.SIN_AXIS == 2)
            {
                result = new List<int>();
                if (!DrvIC.Sine_Wave_Test(ch, "X", Condition.SIN_AXIS, Condition.SIN_THD,
                    Condition.SIN_CNT_ERR, Condition.SIN_FREQ, Condition.SIN_AMP, Condition.SIN_CYCL, ref result))
                {
                    errMsg[ch] = string.Format("{0} Error", testItem);
                    m_ChannelOn[ch] = false;
                }
                else
                {
                    Spec.PassFails[ch].Results[(int)SpecItem.SineWaveX_Count].Val = result[0];
                    Spec.PassFails[ch].Results[(int)SpecItem.SineWaveX_Result].Val = result[1];

                    Spec.SetResult(ch, (int)SpecItem.SineWaveX_Result, (int)SpecItem.SineWaveX_Count);
                    ShowDataResults(ch, "FRA X");
                }
            }

            if (Condition.SIN_AXIS == 1 || Condition.SIN_AXIS == 2)
            {
                result = new List<int>();
                if (!DrvIC.Sine_Wave_Test(ch, "Y1", Condition.SIN_AXIS, Condition.SIN_THD,
                    Condition.SIN_CNT_ERR, Condition.SIN_FREQ, Condition.SIN_AMP, Condition.SIN_CYCL, ref result))
                {
                    errMsg[ch] = string.Format("{0} Error", testItem);
                    m_ChannelOn[ch] = false;
                }
                else
                {
                    Spec.PassFails[ch].Results[(int)SpecItem.SineWaveY1_Count].Val = result[0];
                    Spec.PassFails[ch].Results[(int)SpecItem.SineWaveY1_Result].Val = result[1];

                    Spec.SetResult(ch, (int)SpecItem.SineWaveY1_Result, (int)SpecItem.SineWaveY1_Count);
                    ShowDataResults(ch, "FRA Y1");
                }


                result = new List<int>();
                if (!DrvIC.Sine_Wave_Test(ch, "Y2", Condition.SIN_AXIS, Condition.SIN_THD,
                    Condition.SIN_CNT_ERR, Condition.SIN_FREQ, Condition.SIN_AMP, Condition.SIN_CYCL, ref result))
                {
                    errMsg[ch] = string.Format("{0} Error", testItem);
                    m_ChannelOn[ch] = false;
                }
                else
                {
                    Spec.PassFails[ch].Results[(int)SpecItem.SineWaveY2_Count].Val = result[0];
                    Spec.PassFails[ch].Results[(int)SpecItem.SineWaveY2_Result].Val = result[1];

                    Spec.SetResult(ch, (int)SpecItem.SineWaveY2_Result, (int)SpecItem.SineWaveY2_Count);
                    ShowDataResults(ch, "FRA Y2");
                }
            }
            //Ringing Test
            if (Condition.RNG_AXIS == 0 || Condition.RNG_AXIS == 2)
            {
                result = new List<int>();
                if (!DrvIC.Ringing_Test(ch, "X", Condition.RNG_AXIS, Condition.RNG_THD, Condition.RNG_STVT,
                    Condition.RNG_METM, Condition.RNG_WSEC, ref result))
                {
                    errMsg[ch] = string.Format("{0} Error", testItem);
                    m_ChannelOn[ch] = false;
                }
                else
                {
                    Spec.PassFails[ch].Results[(int)SpecItem.RingingX_Time].Val = result[0];
                    Spec.PassFails[ch].Results[(int)SpecItem.RingingX_Result].Val = result[1];

                    Spec.SetResult(ch, (int)SpecItem.RingingX_Result, (int)SpecItem.RingingX_Time);
                    ShowDataResults(ch, "FRA X");
                }
            }

            if (Condition.RNG_AXIS == 1 || Condition.RNG_AXIS == 2)
            {
                result = new List<int>();
                if (!DrvIC.Ringing_Test(ch, "Y1", Condition.RNG_AXIS, Condition.RNG_THD, Condition.RNG_STVT,
                    Condition.RNG_METM, Condition.RNG_WSEC, ref result))
                {
                    errMsg[ch] = string.Format("{0} Error", testItem);
                    m_ChannelOn[ch] = false;
                }
                else
                {
                    Spec.PassFails[ch].Results[(int)SpecItem.RingingY1_Time].Val = result[0];
                    Spec.PassFails[ch].Results[(int)SpecItem.RingingY1_Result].Val = result[1];

                    Spec.SetResult(ch, (int)SpecItem.RingingY1_Result, (int)SpecItem.RingingY1_Time);
                    ShowDataResults(ch, "FRA Y1");
                }

                result = new List<int>();
                if (!DrvIC.Ringing_Test(ch, "Y2", Condition.RNG_AXIS, Condition.RNG_THD, Condition.RNG_STVT,
                    Condition.RNG_METM, Condition.RNG_WSEC, ref result))
                {
                    errMsg[ch] = string.Format("{0} Error", testItem);
                    m_ChannelOn[ch] = false;
                }
                else
                {
                    Spec.PassFails[ch].Results[(int)SpecItem.RingingY2_Time].Val = result[0];
                    Spec.PassFails[ch].Results[(int)SpecItem.RingingY2_Result].Val = result[1];

                    Spec.SetResult(ch, (int)SpecItem.RingingY2_Result, (int)SpecItem.RingingY2_Time);
                    ShowDataResults(ch, "FRA Y2");
                }
            }
        }
        private void Act_OISLineartitycomp(int port, string testItem)
        {
            LEDs_All_On(port, true);
            Process_ScanCodeTest(port, testItem);
            LEDs_All_On(port, false);
            if (!Option.WriteResultToDriverIC) Process_CalcCodeTest(port, testItem);

            int ch = port * 2;

            for (int j = ch; j < ch + ChannelCnt; j++)
            {
                if (!m_ChannelOn[j]) continue;
                foreach (var Cal in CalList[j])
                    if (Cal.Name == testItem)
                    {
                        List<byte> result = new List<byte>();

                        if (testItem.Contains("X"))
                        {
                            Cal.CodeX.RemoveAt(0);
                            Cal.CodeX.RemoveAt(0);
                            Cal.StrokeX.RemoveAt(0);
                            Cal.StrokeX.RemoveAt(0);
                            if (!DrvIC.LinearityComp(j, testItem, Cal.CodeX, Cal.StrokeX, ref result))
                            {
                                errMsg[j] = string.Format("{0} Error", testItem);
                                m_ChannelOn[j] = false;
                            }

                            DrvValue[j].XLC2C = result[0];
                            DrvValue[j].XLC2D = result[1];
                            DrvValue[j].XLC2E = result[2];
                        }
                        else
                        {
                            Cal.CodeY1.RemoveAt(0);
                            Cal.CodeY1.RemoveAt(0);
                            Cal.StrokeY.RemoveAt(0);
                            Cal.StrokeY.RemoveAt(0);
                            if (!DrvIC.LinearityComp(j, testItem, Cal.CodeY1, Cal.StrokeY, ref result))
                            {
                                errMsg[j] = string.Format("{0} Error", testItem);
                                m_ChannelOn[j] = false;
                            }

                            DrvValue[j].YLC2C = result[0];
                            DrvValue[j].YLC2D = result[1];
                            DrvValue[j].YLC2E = result[2];
                        }
                    }
            }

            LEDs_All_On(port, false);
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
                            case "AF Scan2":
                            case "AF Scan3":
                            case "AF Scan4":
                                //AF ========
                                MakeWaveformCode(ref Cal.CodeZ, Condition.iAFDrvCodeMin, Condition.iAFDrvCodeMax, 2048, Condition.iDrvAFStep);
                                break;
                            case "OIS X Scan":
                            case "OIS X Scan2":
                            case "OIS X Scan3":
                            case "OIS X Scan4":
                                //X =========
                                MakeWaveformCode(ref Cal.CodeX, Condition.iXDrvCodeMin, Condition.iXDrvCodeMax, 2048, Condition.iDrvXStep);
                                break;
                            case "OIS Y Scan":
                            case "OIS Y Scan2":
                            case "OIS Y Scan3":
                            case "OIS Y Scan4":
                                //Y1 ===========================
                                MakeWaveformCode(ref Cal.CodeY1, Condition.iYDrvCodeMin, Condition.iYDrvCodeMax, 2048, Condition.iDrvYStep);
                                //Y2 ===========================
                                MakeWaveformCode(ref Cal.CodeY2, Condition.iY2DrvCodeMin, Condition.iY2DrvCodeMax, 2048, Condition.iDrvYStep);
                                break;
                            case "OIS Matrix Scan":
                                for (int i = 0; i < Rcp.CodeScript.Param.Count; i++)
                                {
                                    Cal.CodeX.Add(int.Parse(Rcp.CodeScript.Param[i][1].ToString()));
                                    Cal.CodeY1.Add(int.Parse(Rcp.CodeScript.Param[i][2].ToString()));
                                    Cal.CodeY1.Add(int.Parse(Rcp.CodeScript.Param[i][3].ToString()));
                                }
                                break;
                            case "OIS X Linearity Comp":
                            case "OIS Y Linearity Comp":
                            case "OIS X Linearity Comp2":
                            case "OIS Y Linearity Comp2":
                                if (name.Contains("X"))
                                {
                                    min = Condition.LinStart;
                                    max = Condition.LinEnd;
                                    step = Condition.LinSamplingSize;

                                    curPos = min;
                                    Cal.CodeX.Add(curPos);
                                    Cal.CodeX.Add(curPos);
                                    do
                                    {
                                        Cal.CodeX.Add(curPos);
                                        curPos += step;
                                    } while (curPos < max);
                                    Cal.CodeX.Add(max);
                                }
                                else if (name.Contains("Y"))
                                {
                                    min = Condition.LinStart;
                                    max = Condition.LinEnd;
                                    step = Condition.LinSamplingSize;

                                    curPos = min;
                                    Cal.CodeY1.Add(curPos);
                                    Cal.CodeY1.Add(curPos);
                                    do
                                    {
                                        Cal.CodeY1.Add(curPos);
                                        curPos += step;
                                    } while (curPos < max);
                                    Cal.CodeY1.Add(max);

                                    min = Condition.LinStart;
                                    max = Condition.LinEnd;
                                    step = Condition.LinSamplingSize;

                                    curPos = min;
                                    Cal.CodeY2.Add(curPos);
                                    Cal.CodeY2.Add(curPos);
                                    do
                                    {
                                        Cal.CodeY2.Add(curPos);
                                        curPos += step;
                                    } while (curPos < max);
                                    Cal.CodeY2.Add(max);
                                }
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
            code.Add(curPos);
            code.Add(curPos);
            do
            {
                code.Add(curPos);
                curPos += step;
            } while (curPos < max);
            code.Add(max);
            //code.Add(max);
            //code.Add(max);
            curPos -= step;
            do
            {
                code.Add(curPos);
                curPos -= step;
            } while (curPos > mid);

            //code.Add(mid);
            //code.Add(mid);
            do
            {
                code.Add(curPos);
                curPos -= step;
            } while (curPos > min);

            //code.Add(min);
            //code.Add(min);
            curPos = min;
            do
            {
                code.Add(curPos);
                curPos += step;
            } while (curPos < mid);
            code.Add(mid);
            //code.Add(mid);
            //code.Add(mid);
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
                    case "AF Scan2":
                    case "AF Scan3":
                    case "AF Scan4":
                        DrvIC.Move(j, "AF", 2048);
                        if (Condition.iAFCrossOffsetCntl == 1)
                        {
                            DrvIC.OISOn(j, "X", true);
                            DrvIC.Move(j, "X", Condition.iAFCrossOffsetX);
                            DrvIC.OISOn(j, "Y", true);
                            DrvIC.Move(j, "Y", Condition.iAFCrossOffsetY);
                        }
                        break;
                    case "OIS X Scan":
                    case "OIS X Scan2":
                    case "OIS X Scan3":
                    case "OIS X Scan4":
                    case "OIS X Linearity Comp":
                    case "OIS X Linearity Comp2":
                        DrvIC.Move(j, "X", 2048);
                        if (Condition.iXCrossOffsetCntl == 1)
                        {
                            DrvIC.OISOn(j, "Y", true);
                            DrvIC.Move(j, "Y", Condition.iXCrossOffset);
                        }
                        if (Condition.iXCrossOffsetCntlAf == 1)
                        {
                            DrvIC.OISOn(j, "AF", true);
                            //DrvIC.Move(j, "AF", Condition.iXCrossOffsetAf);

                            DrvIC.Move(j, "AF", F_Manage.bestpos);
                        }
                        break;
                    case "OIS Y Scan":
                    case "OIS Y Scan2":
                    case "OIS Y Scan3":
                    case "OIS Y Scan4":
                    case "OIS Y Linearity Comp":
                    case "OIS Y Linearity Comp2":
                        DrvIC.Move(j, "Y", 2048);
                        if (Condition.iYCrossOffsetCntl == 1)
                        {
                            DrvIC.OISOn(j, "X", true);
                            DrvIC.Move(j, "X", Condition.iYCrossOffset);
                        }
                        if (Condition.iYCrossOffsetCntlAf == 1)
                        {
                            DrvIC.OISOn(j, "AF", true);
                            //DrvIC.Move(j, "AF", Condition.iYCrossOffsetAf);

                            DrvIC.Move(j, "AF", F_Manage.bestpos);
                        }
                        break;
                    case "OIS Matrix Scan":
                        DrvIC.OISOn(j, "X", true);
                        DrvIC.OISOn(j, "Y", true);
                        DrvIC.Move(j, "X", 2048);
                        DrvIC.Move(j, "Y", 2048);
                        if (Condition.iMCrossOffsetCntlAf == 1)
                        {
                            DrvIC.OISOn(j, "AF", true);
                            DrvIC.Move(j, "AF", Condition.iMCrossOffsetAf);
                        }
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
                    case "AF Scan2":
                    case "AF Scan3":
                    case "AF Scan4":
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
                    case "OIS X Scan2":
                    case "OIS X Scan3":
                    case "OIS X Scan4":
                    case "OIS X Linearity Comp":
                    case "OIS X Linearity Comp2":
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
                    case "OIS Y Scan2":
                    case "OIS Y Scan3":
                    case "OIS Y Scan4":
                    case "OIS Y Linearity Comp":
                    case "OIS Y Linearity Comp2":
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
                    case "OIS Matrix Scan":
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
                            if (name.Contains("Matrix"))
                            {
                                DrvIC.Move(j, "X", Cal.CodeX[framCnt[port]]);
                                DrvIC.Move(j, "Y1", Cal.CodeY1[framCnt[port]]);
                                DrvIC.Move(j, "Y2", Cal.CodeY2[framCnt[port]]);
                            }
                            else
                            {
                                if (name.Contains("X"))
                                {
                                    DrvIC.Move(j, "X", Cal.CodeX[framCnt[port]]);
                                }
                                else if (name.Contains("Y"))
                                {
                                    DrvIC.Move(j, "Y1", Cal.CodeY1[framCnt[port]]);
                                    DrvIC.Move(j, "Y2", Cal.CodeY2[framCnt[port]]);
                                }
                                else if (name.Contains("AF"))
                                {
                                    DrvIC.Move(j, name, Cal.CodeZ[framCnt[port]]);
                                }

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
                            Cal.HallY2[framCnt[port]] = DrvIC.ReadHall(j, "Y2");
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
                            else if (name.Contains("Matrix"))
                            {
                                Cal.Current[framCnt[port]] = Dln.GetCurrent(j, 1);
                                AddLog(j, string.Format("{0} == X Code : {1}, Hall : {2}", name, Cal.CodeX[framCnt[port]], Cal.HallX[framCnt[port]]));
                                AddLog(j, string.Format("{0} == Y1 Code : {1},  Y2Code : {2}, Hall1 : {3}, Hall2 : {4}", name, Cal.CodeY1[framCnt[port]], Cal.CodeY2[framCnt[port]], Cal.HallY1[framCnt[port]], Cal.HallY2[framCnt[port]]));
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
                DrvIC.Move(j, "AF", F_Manage.bestpos);
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

            if (Option.XDirReverse)
            {
                for (int i = 0; i < fCount; i++)
                    for (int j = ch; j < ch + ChannelCnt; j++)
                    {
                        if (!m_ChannelOn[j]) continue;
                        result[i].cx[j] = result[i].cx[j] * -1;
                    }
            }
            if (Option.YDirReverse)
            {
                for (int i = 0; i < fCount; i++)
                    for (int j = ch; j < ch + ChannelCnt; j++)
                    {
                        result[i].cy[j] = result[i].cy[j] * -1;
                        result[i].cy1[j] = result[i].cy1[j] * -1;
                        result[i].cy2[j] = result[i].cy2[j] * -1;
                    }
            }
            if (Option.AFDirReverse)
            {
                for (int i = 0; i < fCount; i++)
                    for (int j = ch; j < ch + ChannelCnt; j++)
                    {
                        if (!m_ChannelOn[j]) continue;
                        result[i].cz[j] = result[i].cz[j] * -1;
                    }
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
                        if (Option.FixedCenter)
                        {
                            int centerPoint = 0;
                            for (int i = 0; i < fCount; i++)
                            {
                                if (name.Contains("X"))
                                {
                                    centerPoint = HallParam[j].XHmid;
                                }
                                else if (name.Contains("Y"))
                                {
                                    centerPoint = HallParam[j].YHmid;
                                }

                            }
                            centerX = result[centerPoint].cx[j]; centerY = result[centerPoint].cy[j];
                            centerY1 = result[centerPoint].cy1[j]; centerY2 = result[centerPoint].cy2[j];
                        }
                        else
                        {
                            bool isCentered = false;
                            for (int i = 2; i < fCount; i++)
                            {
                                if (name.Contains("X"))
                                {
                                    if (Cal.CodeX[i] == 2048)
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
                                    if (Cal.CodeY1[i] == 2048)
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
                                    if (Cal.CodeZ[i] == 2048)
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

                        }
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
                string str = Convert.ToString(Spec.LastSampleNum + 1);
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
                                case "AF Scan2":
                                case "AF Scan3":
                                case "AF Scan4":
                                    arry.Add("i,AF Code,X Code,Y1 Code,Y2 Code,X,Y,Z,TX,TY,TZ,Y1,Y2,Hall X,Hall Y1,Hall Y2,Hall AF,Current");
                                    for (int i = 0; i < fCount; i++)
                                    {
                                        path = string.Format(dateDir + "{0}_{1}_{2}_{3}.csv", name, m_StrIndex[j], Spec.LastSampleNum + 1, st);
                                        string data = string.Format("{0},{1},{2},{3},{4},{5:0.000},{6:0.000},{7:0.000},{8:0.000},{9:0.000},{10:0.000},{11:0.000},{12:0.000},{13},{14},{15},{16},{17:0.000}", i, Cal.CodeZ[i], Cal.CodeZ[i], Cal.CodeZ[i], Cal.CodeZ[i],
                                            Cal.StrokeX[i], Cal.StrokeY[i], Cal.StrokeZ[i], Cal.TiltX[i], Cal.TiltY[i], Cal.TiltZ[i], Cal.StrokeY1[i], Cal.StrokeY2[i],
                                            Cal.HallX[i], Cal.HallY1[i], Cal.HallY2[i], Cal.HallZ[i], Cal.Current[i]);
                                        arry.Add(data);
                                        if (i == 0)
                                            AddLog(j, string.Format("Code AF\tStroke AF\tTx\tTy\tTz"));
                                        AddLog(j, string.Format("{0}\t{1:0.000}\t{2:0.000}\t{3:0.000}\t{4:0.000}", Cal.CodeZ[i], Cal.StrokeZ[i], Cal.TiltX[i], Cal.TiltY[i], Cal.TiltZ[i]));
                                    }
                                    break;
                                case "OIS X Scan":
                                case "OIS X Scan2":
                                case "OIS X Scan3":
                                case "OIS X Scan4":
                                    //case "OIS X Linearity Comp":
                                    //case "OIS X Linearity Comp2":
                                    arry.Add("i,AF Code,X Code,Y1 Code,Y2 Code,X,Y,Z,TX,TY,TZ,Y1,Y2,Hall X,Hall Y1,Hall Y2,Hall AF,Current");
                                    for (int i = 0; i < fCount; i++)
                                    {
                                        if (name.Contains("Linearity"))
                                            path = string.Format(dateDir + "{0}_{1}_{2}_{3}_Lin.csv", name, m_StrIndex[j], Spec.LastSampleNum + 1, st);
                                        else path = string.Format(dateDir + "{0}_{1}_{2}_{3}.csv", name, m_StrIndex[j], Spec.LastSampleNum + 1, st);
                                        string data = string.Format("{0},{1},{2},{3},{4},{5:0.000},{6:0.000},{7:0.000},{8:0.000},{9:0.000},{10:0.000},{11:0.000},{12:0.000},{13},{14},{15},{16},{17:0.000}", i, Cal.CodeX[i], Cal.CodeX[i], Cal.CodeX[i], Cal.CodeX[i],
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
                                case "OIS Y Scan2":
                                case "OIS Y Scan3":
                                case "OIS Y Scan4":
                                    //case "OIS Y Linearity Comp":
                                    // case "OIS Y Linearity Comp2":
                                    arry.Add("i,AF Code,X Code,Y1 Code,Y2 Code,X,Y,Z,TX,TY,TZ,Y1,Y2,Hall X,Hall Y1,Hall Y2,Hall AF,Current");
                                    for (int i = 0; i < fCount; i++)
                                    {
                                        if (name.Contains("Linearity"))
                                            path = string.Format(dateDir + "{0}_{1}_{2}_{3}_Lin.csv", name, m_StrIndex[j], Spec.LastSampleNum + 1, st);
                                        else path = string.Format(dateDir + "{0}_{1}_{2}_{3}.csv", name, m_StrIndex[j], Spec.LastSampleNum + 1, st);
                                        string data = string.Format("{0},{1},{2},{3},{4},{5:0.000},{6:0.000},{7:0.000},{8:0.000},{9:0.000},{10:0.000},{11:0.000},{12:0.000},{13},{14},{15},{16},{17:0.000}", i, Cal.CodeY1[i], Cal.CodeY1[i], Cal.CodeY1[i], Cal.CodeY1[i],
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
                                case "OIS Matrix Scan":
                                    arry.Add("i,X Code,Y1 Code,Y2 Code,X Stroke,Y1 Stroke,Y2 Stroke,Current,Hall X,Hall Y1,Hall Y2,Tx,Ty,Tz");
                                    for (int i = 0; i < fCount; i++)
                                    {
                                        path = string.Format("{0}_{1}_{2}_{3}.csv", name, m_StrIndex[j], Spec.LastSampleNum + 1, st);
                                        string data = string.Format("{0},{1},{2},{3},{4:0.000},{5:0.000},{6:0.000},{7:0.0},{8},{9},{10},{11:0.000},{12:0.000},{13:0.000}", i, Cal.CodeZ[i],
                                           Cal.CodeX[i], Cal.CodeY1[i],
                                           Cal.StrokeX[i], Cal.StrokeY1[i], Cal.StrokeY2[i], Cal.Current[i], Cal.HallX[i], Cal.HallY1[i], Cal.HallY2[i], Cal.TiltX[i], Cal.TiltY[i], Cal.TiltZ[i]);
                                        arry.Add(data);

                                        if (i == 0)
                                            AddLog(j, string.Format("X Code\tY1 Code\tY2 Code\tStroke X\tStroke Y1\tStroke Y2\tTx\tTy\tTz"));

                                        AddLog(j, string.Format("{0}\t{1}\t{2}\t{3:0.000}\t{4:0.000}\t{5:0.000}\t{6:0.000}\t{7:0.000}\t{8:0.000}", Cal.CodeX[i], Cal.CodeY1[i], Cal.CodeY2[i], Cal.StrokeX[i], Cal.StrokeY1[i], Cal.StrokeY2[i], Cal.TiltX[i], Cal.TiltY[i], Cal.TiltZ[i]));
                                    }

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
                            forword = Spec.PassFails[j].Results[(int)SpecItem.AF_Forwardstroke].Val = Cal.CalFwdStoke(Cal.CodeZ, Cal.StrokeZ);
                            backword = Spec.PassFails[j].Results[(int)SpecItem.AF_Backwardstroke].Val = Cal.CalBwdStoke(Cal.CodeZ, Cal.StrokeZ);
                            Spec.PassFails[j].Results[(int)SpecItem.AF_Ratedstroke].Val = forword + backword;
                            Spec.PassFails[j].Results[(int)SpecItem.AF_Sensitivity].Val = Cal.CalSensitivity(Cal.CodeZ, Cal.StrokeZ, Condition.iAFCodeRange, Condition.iAFStrokeRange);
                            Spec.PassFails[j].Results[(int)SpecItem.AF_Linearity].Val = Cal.CalLinearity(Cal.CodeZ, Cal.StrokeZ, Condition.iAFCodeRange, Condition.iAFStrokeRange);
                            Spec.PassFails[j].Results[(int)SpecItem.AF_Hysteresis].Val = Cal.CalHysteresis(Cal.CodeZ, Cal.StrokeZ, Condition.iAFCodeRange, Condition.iAFStrokeRange);
                            Spec.PassFails[j].Results[(int)SpecItem.AF_MaxCurrent].Val = Cal.CalMaxCurrent(Cal.CodeZ, Cal.StrokeZ, Condition.iAFCodeRange, Condition.iAFStrokeRange);
                            Spec.PassFails[j].Results[(int)SpecItem.AF_HoldingCurrent].Val = Cal.CalHoldingCurrent(Cal.CodeZ, Cal.StrokeZ, Condition.iAFCodeRange, Condition.iAFStrokeRange);
                            Spec.PassFails[j].Results[(int)SpecItem.AF_CrosstalkX].Val = Cal.CalCrosstalk(Cal.CodeZ, Cal.StrokeX, Condition.iAFCodeRange, Condition.iAFCodeRange);
                            Spec.PassFails[j].Results[(int)SpecItem.AF_CrosstalkY].Val = Cal.CalCrosstalk(Cal.CodeZ, Cal.StrokeY, Condition.iAFCodeRange, Condition.iAFCodeRange);
                            Spec.PassFails[j].Results[(int)SpecItem.AF_CrosstalkR].Val = Cal.CalCrosstalkR(Cal.CodeZ, Cal.StrokeX, Cal.StrokeY, Condition.iAFCodeRange, Condition.iAFCodeRange);
                            Spec.PassFails[j].Results[(int)SpecItem.AF_Rolling].Val = Cal.CalRolling(Cal.CodeZ, Cal.StrokeZ, Condition.iAFCodeRange, Condition.iAFStrokeRange);

                            Spec.SetResult(j, (int)SpecItem.AF_Ratedstroke, (int)SpecItem.AF_Rolling);
                            ShowDataResults(j, "AF");
                        }
                        else if (name.Contains("OIS X"))
                        {
                            forword = Spec.PassFails[j].Results[(int)SpecItem.OISX_Forwardstroke].Val = Cal.CalFwdStoke(Cal.CodeX, Cal.StrokeX);
                            backword = Spec.PassFails[j].Results[(int)SpecItem.OISX_Backwardstroke].Val = Cal.CalBwdStoke(Cal.CodeX, Cal.StrokeX);
                            Spec.PassFails[j].Results[(int)SpecItem.OISX_Ratedstroke].Val = forword + backword;
                            Spec.PassFails[j].Results[(int)SpecItem.OISX_Sensitivity].Val = Cal.CalSensitivity(Cal.CodeX, Cal.StrokeX, Condition.iXCodeRange, Condition.iXStrokeRange);
                            Spec.PassFails[j].Results[(int)SpecItem.OISX_Linearity].Val = Cal.CalLinearity(Cal.CodeX, Cal.StrokeX, Condition.iXCodeRange, Condition.iXStrokeRange);
                            Spec.PassFails[j].Results[(int)SpecItem.OISX_Hysteresis].Val = Cal.CalHysteresis(Cal.CodeX, Cal.StrokeX, Condition.iXCodeRange, Condition.iXStrokeRange);
                            Spec.PassFails[j].Results[(int)SpecItem.OISX_MaxCurrent].Val = Cal.CalMaxCurrent(Cal.CodeX, Cal.StrokeX, Condition.iXCodeRange, Condition.iXStrokeRange);
                            Spec.PassFails[j].Results[(int)SpecItem.OISX_CenteringCurrent].Val = Cal.CalCenterCurrent(Cal.CodeX, Cal.StrokeX, Condition.iXCodeRange, Condition.iXCodeRange);
                            Spec.PassFails[j].Results[(int)SpecItem.OISX_CrosstalkY].Val = Cal.CalCrosstalk(Cal.CodeX, Cal.StrokeY, Condition.iXCodeRange, Condition.iXCodeRange);
                            Spec.PassFails[j].Results[(int)SpecItem.OISX_CrosstalkZ].Val = Cal.CalCrosstalk(Cal.CodeX, Cal.StrokeZ, Condition.iXCodeRange, Condition.iXCodeRange);
                            Spec.PassFails[j].Results[(int)SpecItem.OISX_CrosstalkR].Val = Cal.CalCrosstalkR(Cal.CodeX, Cal.StrokeY, Cal.StrokeZ, Condition.iXCodeRange, Condition.iXCodeRange);
                            Spec.PassFails[j].Results[(int)SpecItem.OISX_Rolling].Val = Cal.CalRolling(Cal.CodeX, Cal.StrokeX, Condition.iXCodeRange, Condition.iXStrokeRange);

                            Spec.SetResult(j, (int)SpecItem.OISX_Ratedstroke, (int)SpecItem.OISX_Rolling);
                            ShowDataResults(j, "X");
                        }
                        else if (name.Contains("OIS Y"))
                        {
                            forword = Spec.PassFails[j].Results[(int)SpecItem.OISY_Forwardstroke].Val = Cal.CalFwdStoke(Cal.CodeY1, Cal.StrokeY);
                            backword = Spec.PassFails[j].Results[(int)SpecItem.OISY_Backwardstroke].Val = Cal.CalBwdStoke(Cal.CodeY1, Cal.StrokeY);
                            Spec.PassFails[j].Results[(int)SpecItem.OISY_Ratedstroke].Val = forword + backword;

                            Spec.PassFails[j].Results[(int)SpecItem.OISY_Sensitivity].Val = Cal.CalSensitivity(Cal.CodeY1, Cal.StrokeY, Condition.iYCodeRange, Condition.iYStrokeRange);
                            Spec.PassFails[j].Results[(int)SpecItem.OISY_Linearity].Val = Cal.CalLinearity(Cal.CodeY1, Cal.StrokeY, Condition.iYCodeRange, Condition.iYStrokeRange);
                            Spec.PassFails[j].Results[(int)SpecItem.OISY_Hysteresis].Val = Cal.CalHysteresis(Cal.CodeY1, Cal.StrokeY, Condition.iYCodeRange, Condition.iYStrokeRange);
                            Spec.PassFails[j].Results[(int)SpecItem.OISY_MaxCurrent].Val = Cal.CalMaxCurrent(Cal.CodeY1, Cal.StrokeY, Condition.iYCodeRange, Condition.iYStrokeRange);
                            Spec.PassFails[j].Results[(int)SpecItem.OISY_CenteringCurrent].Val = Cal.CalCenterCurrent(Cal.CodeY1, Cal.StrokeY, Condition.iYCodeRange, Condition.iYStrokeRange);
                            Spec.PassFails[j].Results[(int)SpecItem.OISY_CrosstalkX].Val = Cal.CalCrosstalk(Cal.CodeY1, Cal.StrokeX, Condition.iYStrokeRange, Condition.iYStrokeRange);
                            Spec.PassFails[j].Results[(int)SpecItem.OISY_CrosstalkZ].Val = Cal.CalCrosstalk(Cal.CodeY1, Cal.StrokeZ, Condition.iYStrokeRange, Condition.iYStrokeRange);
                            Spec.PassFails[j].Results[(int)SpecItem.OISY_CrosstalkR].Val = Cal.CalCrosstalkR(Cal.CodeY1, Cal.StrokeX, Cal.StrokeZ, Condition.iYStrokeRange, Condition.iYStrokeRange);
                            Spec.PassFails[j].Results[(int)SpecItem.OISY_Rolling].Val = Cal.CalRolling(Cal.CodeY1, Cal.StrokeY, Condition.iYCodeRange, Condition.iYStrokeRange);

                            Spec.SetResult(j, (int)SpecItem.OISY_Ratedstroke, (int)SpecItem.OISY_Rolling);
                            ShowDataResults(j, "Y");
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
            if (Option.XDirReverse)
            {
                for (int i = 0; i < framCnt[port]; i++)
                    for (int j = ch; j < ch + ChannelCnt; j++)
                    {
                        if (!m_ChannelOn[j]) continue;
                        result[i].cx[j] = result[i].cx[j] * -1;
                    }
            }
            if (Option.YDirReverse)
            {
                for (int i = 0; i < framCnt[port]; i++)
                    for (int j = ch; j < ch + ChannelCnt; j++)
                    {
                        result[i].cy[j] = result[i].cy[j] * -1;
                        result[i].cy1[j] = result[i].cy1[j] * -1;
                        result[i].cy2[j] = result[i].cy2[j] * -1;
                    }
            }
            if (Option.AFDirReverse)
            {
                for (int i = 0; i < framCnt[port]; i++)
                    for (int j = ch; j < ch + ChannelCnt; j++)
                    {
                        if (!m_ChannelOn[j]) continue;
                        result[i].cz[j] = result[i].cz[j] * -1;
                    }
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
                string str = Convert.ToString(Spec.LastSampleNum + 1);
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
                                        path = string.Format(dateDir + "{0}_{1}_{2}_{3}.csv", name, m_StrIndex[j], Spec.LastSampleNum + 1, st);
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
                                Spec.PassFails[j].Results[(int)SpecItem.AF_SettillingTime].Val = settling;

                                Spec.SetResult(j, (int)SpecItem.AF_SettillingTime, (int)SpecItem.AF_SettillingTime);
                                ShowDataResults(j, "AF");
                                break;
                        }
                        AddChart(j, name);
                    }
            }
            framCnt[port] = 0;
        }
        public void Process_FindEPA(int port, string name, ref int[] centerCode, ref int[] epaCodeMin, ref int[] epaCodeMax)
        {
            int ch = port * 2;

            int[] xcode = new int[ChannelCnt];
            int[] y1code = new int[ChannelCnt];
            int[] y2code = new int[ChannelCnt];
            for (int j = ch; j < ch + ChannelCnt; j++)
            {
                if (!m_ChannelOn[j]) continue;
                xcode[j] = 2048;
                y1code[j] = 2048;
                y2code[j] = 2048;
            }

            int[] xHall = new int[ChannelCnt];
            int[] y1Hall = new int[ChannelCnt];
            int[] y2Hall = new int[ChannelCnt];

            int[] errX = new int[ChannelCnt];
            int[] errY1 = new int[ChannelCnt];
            int[] errY2 = new int[ChannelCnt];

            //  구동오차 이력, 구동 코드이력 저장
            List<int>[] errXold = new List<int>[ChannelCnt];
            List<int>[] errY1old = new List<int>[ChannelCnt];
            List<int>[] errY2old = new List<int>[ChannelCnt];

            List<int>[] xcodeold = new List<int>[ChannelCnt];
            List<int>[] y1codeold = new List<int>[ChannelCnt];
            List<int>[] y2codeold = new List<int>[ChannelCnt];

            //  초기 위치는 (2048, 2048, 2048)
            for (int j = ch; j < ch + ChannelCnt; j++)
            {
                if (!m_ChannelOn[j]) continue;
                errXold[j] = new List<int>();
                errY1old[j] = new List<int>();
                errY2old[j] = new List<int>();

                xcodeold[j] = new List<int>();
                y1codeold[j] = new List<int>();
                y2codeold[j] = new List<int>();

                if (name.Contains("X"))
                {
                    DrvIC.OISOn(j, "X", true);
                    Thread.Sleep(10);
                    DrvIC.Move(j, "X", xcode[j]);
                    AddLog(j, string.Format("Move X : {0}", xcode[j]));
                    xcodeold[j].Add(Math.Abs(xcode[j]));
                }
                else
                {
                    DrvIC.OISOn(j, "Y", true);
                    Thread.Sleep(10);
                    DrvIC.Move(j, "Y1", y1code[j]);
                    DrvIC.Move(j, "Y2", y2code[j]);
                    AddLog(j, string.Format("Move Y1 : {0}, Y2 : {1}", y1code[j], y2code[j]));
                    y1codeold[j].Add(Math.Abs(y1code[j]));
                    y2codeold[j].Add(Math.Abs(y2code[j]));
                }
            }

            Thread.Sleep(100);

            for (int j = ch; j < ch + ChannelCnt; j++)
            {
                if (!m_ChannelOn[j]) continue;
                if (name.Contains("X"))
                {
                    xHall[j] = DrvIC.ReadHall(j, "X");
                    AddLog(j, string.Format("Read Hall X : {0}", xHall[j]));
                    errX[j] = xHall[j] - 2048;
                    errXold[j].Add(Math.Abs(errX[j]));
                }
                else
                {
                    y1Hall[j] = DrvIC.ReadHall(j, "Y1");
                    y2Hall[j] = DrvIC.ReadHall(j, "Y2");
                    AddLog(j, string.Format("Read Hall Y1 : {0}, Y2 : {1}", y1Hall[j], y2Hall[j]));
                    errY1[j] = y1Hall[j] - 2048;
                    errY2[j] = y2Hall[j] - 2048;
                    errY1old[j].Add(Math.Abs(errY1[j]));
                    errY2old[j].Add(Math.Abs(errY2[j]));
                }
            }

            int itrCnt = 0;
            bool[] chFinish = new bool[2] { false, false };

            //
            LEDs_All_On(0, true);
            //Process.LEDs_All_On(1, true);


            while (true)
            {
                for (int j = ch; j < ch + ChannelCnt; j++)
                {
                    if (!m_ChannelOn[j]) continue;
                    if (name.Contains("X"))
                    {
                        xcode[j] -= errX[j];
                        DrvIC.Move(j, "X", xcode[j]);
                        AddLog(j, string.Format("Move X : {0}", xcode[j]));
                        xcodeold[j].Add(Math.Abs(xcode[j]));
                    }
                    else
                    {
                        y1code[j] -= errY1[j];
                        y2code[j] -= errY2[j];
                        DrvIC.Move(j, "Y1", y1code[j]);
                        DrvIC.Move(j, "Y2", y2code[j]);
                        AddLog(j, string.Format("Move Y1 : {0}, Y2 : {1}", y1code[j], y2code[j]));
                        y1codeold[j].Add(Math.Abs(y1code[j]));
                        y2codeold[j].Add(Math.Abs(y2code[j]));
                    }
                }

                Thread.Sleep(100);
                for (int j = ch; j < ch + ChannelCnt; j++)
                {
                    if (!m_ChannelOn[j]) continue;
                    if (name.Contains("X"))
                    {
                        xHall[j] = DrvIC.ReadHall(j, "X");
                        AddLog(j, string.Format("Read Hall X : {0}", xHall[j]));
                        errX[j] = xHall[j] - 2048;
                        errXold[j].Add(Math.Abs(errX[j]));
                        //  2048 을 찾은 경우 종료
                        if (errX[j] == 0)
                            chFinish[j] = true;

                        if (itrCnt > 5)
                        {
                            if (!m_ChannelOn[j]) continue;
                            //  더이상 오차가 줄어들지 않는 경우 종료
                            if (errXold[j][itrCnt - 1] >= Math.Abs(errX[j]))
                                chFinish[j] = true;

                            if (errXold[j][itrCnt - 1] >= Math.Abs(errX[j]))
                                chFinish[j] = true;
                        }
                    }
                    else
                    {
                        y1Hall[j] = DrvIC.ReadHall(j, "Y1");
                        y2Hall[j] = DrvIC.ReadHall(j, "Y2");
                        AddLog(j, string.Format("Read Hall Y1 : {0}, Y2 : {1}", y1Hall[j], y2Hall[j]));
                        errY1[j] = y1Hall[j] - 2048;
                        errY2[j] = y2Hall[j] - 2048;
                        errY1old[j].Add(Math.Abs(errY1[j]));
                        errY2old[j].Add(Math.Abs(errY2[j]));
                        //  2048, 2048 을 찾은 경우 종료
                        if (errY1[j] == 0 && errY2[j] == 0)
                            chFinish[j] = true;

                        if (itrCnt > 5)
                        {
                            if (!m_ChannelOn[j]) continue;
                            //  더이상 오차가 줄어들지 않는 경우 종료
                            if (errY1old[j][itrCnt - 1] >= Math.Abs(errY1[j]) && errY2old[j][itrCnt - 1] >= Math.Abs(errY2[j]))
                                chFinish[j] = true;

                            if (errY1old[j][itrCnt - 1] >= Math.Abs(errY1[j]) && errY2old[j][itrCnt - 1] >= Math.Abs(errY2[j]))
                                chFinish[j] = true;
                        }
                    }
                }

                if (chFinish[0] && chFinish[1])
                    break;

                itrCnt++;

                //for (int j = ch; j < ch + ChannelCnt; j++)
                //{
                //    if (!m_ChannelOn[j]) continue;
                //    AddLog(j, string.Format("Read Hall X : {0}, Y1 : {1}, Y2 : {2}", xHall[j], y1Hall[j], y2Hall[j]));
                //}

                if (itrCnt == 50)
                {
                    for (int j = ch; j < ch + ChannelCnt; j++)
                    {
                        if (!m_ChannelOn[j]) continue;
                        AddLog(j, string.Format("Not Found Center Hall.."));
                    }
                    break;
                }
            }

            for (int j = ch; j < ch + ChannelCnt; j++)
            {
                if (!m_ChannelOn[j]) continue;
                if (name.Contains("X"))
                {
                    AddLog(j, string.Format("Center Final Code X : {0}", xcode[j]));
                }
                else
                {
                    AddLog(j, string.Format("Center Final Code Y1 : {0}, Y2 : {1}", y1code[j], y2code[j]));
                }
            }

            centerCode[0] = xcode[0];
            centerCode[3] = xcode[1];
            centerCode[1] = y1code[0];
            centerCode[2] = y2code[0];
            centerCode[4] = y1code[1];
            centerCode[5] = y2code[1];

            //  Center Position
            //STATIC.fVision.oCam[0].GrabB(0);
            //List<double[]> measure = STATIC.fVision.MeasureTxTyTz(0, m_ChannelOn, false, "default");

            List<FindResult> result = new List<FindResult>();
            result.Add(new FindResult());
            //result[0].cy = measure[0];
            //result[0].cx = measure[1];
            //result[0].cz = measure[2];
            //result[0].ty = measure[3];
            //result[0].tx = measure[4];
            //result[0].tz = measure[5];
            //result[0].cy1 = measure[6]; //  Y1 변위
            //result[0].cy2 = measure[7]; //  Y2 변위

            //  Driver To 4095
            for (int j = ch; j < ch + ChannelCnt; j++)
            {
                chFinish[j] = false;
                if (!m_ChannelOn[j]) chFinish[j] = true;
            }

            itrCnt = 0;
            for (int j = ch; j < ch + ChannelCnt; j++)
            {
                if (!m_ChannelOn[j]) continue;
                xcodeold[j].Clear();
                y1codeold[j].Clear();
                y2codeold[j].Clear();
                xcodeold[j].Add(Math.Abs(xcode[j]));
                y1codeold[j].Add(Math.Abs(y1code[j]));
                y2codeold[j].Add(Math.Abs(y2code[j]));
            }

            while (true)
            {
                for (int j = ch; j < ch + ChannelCnt; j++)
                {
                    if (!m_ChannelOn[j]) continue;
                    if (name.Contains("X"))
                    {
                        xcode[j] = xcode[j] < 3996 ? (xcode[j] + 100) : 4095;
                    }
                    else if (name.Contains("Y"))
                    {
                        if (Math.Max(y1code[j], y2code[j]) < 3996)
                        {
                            y1code[j] += 100;
                            y2code[j] += 100;
                        }
                        else
                        {
                            if (y1code[j] >= y2code[j])
                            {
                                int delta = 4095 - y1code[j];
                                y1code[j] += delta;
                                y2code[j] += delta;
                            }
                            else
                            {
                                int delta = 4095 - y2code[j];
                                y1code[j] += delta;
                                y2code[j] += delta;
                            }
                        }
                    }
                    if (name.Contains("X"))
                    {
                        DrvIC.Move(j, "X", xcode[j]);
                        AddLog(j, string.Format("Move X : {0:0.00}", xcode[j]));
                        xcodeold[j].Add(Math.Abs(xcode[j]));
                    }
                    else
                    {

                        DrvIC.Move(j, "Y1", y1code[j]);
                        DrvIC.Move(j, "Y2", y2code[j]);
                        AddLog(j, string.Format("Move Y1 : {0:0.00}, Y2 : {1:0.00}", y1code[j], y2code[j]));

                        y1codeold[j].Add(Math.Abs(y1code[j]));
                        y2codeold[j].Add(Math.Abs(y2code[j]));
                    }

                }
                Thread.Sleep(100);
                //STATIC.fVision.oCam[0].GrabB(0);

                //measure = STATIC.fVision.MeasureTxTyTz(0, m_ChannelOn, false, "default");
                //itrCnt++;
                //result.Add(new FindResult());
                //result[itrCnt].cy = measure[0];
                //result[itrCnt].cx = measure[1];
                //result[itrCnt].cz = measure[2];
                //result[itrCnt].ty = measure[3];
                //result[itrCnt].tx = measure[4];
                //result[itrCnt].tz = measure[5];
                //result[itrCnt].cy1 = measure[6]; //  Y1 변위
                //result[itrCnt].cy2 = measure[7]; //  Y2 변위

                for (int j = ch; j < ch + ChannelCnt; j++)
                {
                    if (!m_ChannelOn[j]) continue;
                    if (xcode[j] == 4095 || y1code[j] == 4095 || y2code[j] == 4095)
                        chFinish[j] = true;

                    if (name.Contains("X"))
                    {
                        AddLog(j, string.Format("Stroke X : {0:0.00}", result[itrCnt].cx[j]));
                    }
                    else
                    {
                        AddLog(j, string.Format("Stroke Y1 : {0:0.00}, Y2 : {1:0.00}", result[itrCnt].cy1[j], result[itrCnt].cy2[j]));
                    }
                }
                if (chFinish[0] && chFinish[1])
                    break;
            }
            for (int j = ch; j < ch + ChannelCnt; j++)
            {
                if (!m_ChannelOn[j]) continue;
                AddLog(j, string.Format("======================================================="));
            }
            int fwdLast = itrCnt;
            //  Driver To 0
            for (int j = ch; j < ch + ChannelCnt; j++)
            {
                chFinish[j] = false;
                if (!m_ChannelOn[j]) chFinish[j] = true;
            }
            while (true)
            {
                for (int j = ch; j < ch + ChannelCnt; j++)
                {
                    if (!m_ChannelOn[j]) continue;
                    if (name.Contains("X"))
                    {
                        xcode[j] = xcode[j] > 99 ? (xcode[j] - 100) : 0;
                    }
                    else if (name.Contains("Y"))
                    {
                        if (Math.Min(y1code[j], y2code[j]) > 99)
                        {
                            y1code[j] -= 100;
                            y2code[j] -= 100;
                        }
                        else
                        {
                            if (y1code[j] < y2code[j])
                            {
                                int delta = y1code[j];
                                y1code[j] -= delta;
                                y2code[j] -= delta;
                            }
                            else
                            {
                                int delta = y2code[j];
                                y1code[j] -= delta;
                                y2code[j] -= delta;
                            }
                        }
                    }
                    if (name.Contains("X"))
                    {
                        DrvIC.Move(j, "X", xcode[j]);
                        AddLog(j, string.Format("Move X : {0}", xcode[j]));
                        xcodeold[j].Add(Math.Abs(xcode[j]));
                    }
                    else
                    {

                        DrvIC.Move(j, "Y1", y1code[j]);
                        DrvIC.Move(j, "Y2", y2code[j]);
                        AddLog(j, string.Format("Move Y1 : {0}, Y2 : {1}", y1code[j], y2code[j]));

                        y1codeold[j].Add(Math.Abs(y1code[j]));
                        y2codeold[j].Add(Math.Abs(y2code[j]));
                    }
                }
                Thread.Sleep(100);
                //STATIC.fVision.oCam[0].GrabB(0);


                //measure = STATIC.fVision.MeasureTxTyTz(0, m_ChannelOn, false, "default");
                //itrCnt++;
                //result.Add(new FindResult());
                //result[itrCnt].cy = measure[0];
                //result[itrCnt].cx = measure[1];
                //result[itrCnt].cz = measure[2];
                //result[itrCnt].ty = measure[3];
                //result[itrCnt].tx = measure[4];
                //result[itrCnt].tz = measure[5];
                //result[itrCnt].cy1 = measure[6]; //  Y1 변위
                //result[itrCnt].cy2 = measure[7]; //  Y2 변위

                for (int j = ch; j < ch + ChannelCnt; j++)
                {
                    if (!m_ChannelOn[j]) continue;
                    if (xcode[j] == 0 || y1code[j] == 0 || y2code[j] == 0)
                        chFinish[j] = true;
                    if (name.Contains("X"))
                    {
                        AddLog(j, string.Format("Stroke X : {0:0.00}", result[itrCnt].cx[j]));
                    }
                    else
                    {
                        AddLog(j, string.Format("Stroke Y1 : {0:0.00}, Y2 : {1:0.00}", result[itrCnt].cy1[j], result[itrCnt].cy2[j]));
                    }
                }
                if (chFinish[0] && chFinish[1])
                    break;
            }


            int bwdLast = itrCnt;
            double[] fwdStroke = new double[2];
            double[] bwdStroke = new double[2];
            for (int j = ch; j < ch + ChannelCnt; j++)
            {
                if (!m_ChannelOn[j]) continue;
                if (name.Contains("X"))
                {
                    fwdStroke[j] = Math.Abs(result[0].cx[j] - result[fwdLast].cx[j]);
                    bwdStroke[j] = Math.Abs(result[0].cx[j] - result[bwdLast].cx[j]);

                    AddLog(j, string.Format("fwdStroke X : {0:0.00}, bwdStroke X  : {1:0.00}", fwdStroke[j], bwdStroke[j]));
                }
                else if (name.Contains("Y"))
                {
                    fwdStroke[j] = Math.Abs(result[0].cy[j] - result[fwdLast].cy[j]);
                    bwdStroke[j] = Math.Abs(result[0].cy[j] - result[bwdLast].cy[j]);

                    AddLog(j, string.Format("fwdStroke Y : {0:0.00}, bwdStroke Y  : {1:0.00}", fwdStroke[j], bwdStroke[j]));
                }

                int i = 0;
                double lstroke = 0;
                if (fwdStroke[j] > bwdStroke[j])
                {
                    //  정방향 Stroke 가 더 큰 경우
                    i = fwdLast - 1;
                    while (true)
                    {
                        if (name.Contains("X"))
                            lstroke = Math.Abs(result[0].cx[j] - result[i].cx[j]);
                        else if (name.Contains("Y"))
                            lstroke = Math.Abs(result[0].cy[j] - result[i].cy[j]);

                        if (lstroke < bwdStroke[j])
                            break;
                        i--;
                    }
                    int targetXCode = 0;
                    int targetY1Code = 0;
                    int targetY2Code = 0;
                    if (name.Contains("X"))
                    {
                        targetXCode = (int)(xcodeold[j][i] + (bwdStroke[j] - lstroke) * (xcodeold[j][i + 1] - xcodeold[j][i]) / Math.Abs(result[i + 1].cx[j] - result[i].cx[j]));
                        epaCodeMin[j * 3 + 0] = 0;
                        epaCodeMax[j * 3 + 0] = targetXCode;
                    }
                    else if (name.Contains("Y"))
                    {
                        targetY1Code = (int)(y1codeold[j][i] + (bwdStroke[j] - lstroke) * (y1codeold[j][i + 1] - y1codeold[j][i]) / Math.Abs(result[i + 1].cy[j] - result[i].cy[j]));
                        targetY2Code = (int)(y2codeold[j][i] + (bwdStroke[j] - lstroke) * (y2codeold[j][i + 1] - y2codeold[j][i]) / Math.Abs(result[i + 1].cy[j] - result[i].cy[j]));

                        epaCodeMin[j * 3 + 1] = 0;
                        epaCodeMin[j * 3 + 2] = 0;

                        epaCodeMax[j * 3 + 1] = targetY1Code;
                        epaCodeMax[j * 3 + 2] = targetY2Code;
                    }
                }
                else
                {
                    //  역방향 Stroke 가 더 큰 경우
                    i = bwdLast - 1;
                    while (true)
                    {
                        if (name.Contains("X"))
                            lstroke = Math.Abs(result[0].cx[j] - result[i].cx[j]);
                        else if (name.Contains("Y"))
                            lstroke = Math.Abs(result[0].cy[j] - result[i].cy[j]);

                        if (lstroke < fwdStroke[j])
                            break;
                        i--;
                    }
                    int targetXCode = 0;
                    int targetY1Code = 0;
                    int targetY2Code = 0;
                    if (name.Contains("X"))
                    {
                        targetXCode = (int)(xcodeold[j][i] + (fwdStroke[j] - lstroke) * (xcodeold[j][i + 1] - xcodeold[j][i]) / Math.Abs(result[i + 1].cx[j] - result[i].cx[j]));
                        epaCodeMin[j * 3 + 0] = targetXCode;
                        epaCodeMax[j * 3 + 0] = 0;
                    }
                    else if (name.Contains("Y"))
                    {
                        targetY1Code = (int)(y1codeold[j][i] + (fwdStroke[j] - lstroke) * (y1codeold[j][i + 1] - y1codeold[j][i]) / Math.Abs(result[i + 1].cy[j] - result[i].cy[j]));
                        targetY2Code = (int)(y2codeold[j][i] + (fwdStroke[j] - lstroke) * (y2codeold[j][i + 1] - y2codeold[j][i]) / Math.Abs(result[i + 1].cy[j] - result[i].cy[j]));

                        epaCodeMin[j * 3 + 1] = targetY1Code;
                        epaCodeMin[j * 3 + 2] = targetY2Code;

                        epaCodeMax[j * 3 + 1] = 0;
                        epaCodeMax[j * 3 + 2] = 0;
                    }
                }
            }

            LEDs_All_On(0, false);

            for (int j = ch; j < ch + ChannelCnt; j++)
            {
                if (!m_ChannelOn[j]) continue;
                if (name.Contains("X"))
                {
                    DrvIC.OISOn(j, "X", false);
                    DrvIC.Move(j, "X", xcode[j]);
                }
                else if (name.Contains("Y"))
                {
                    DrvIC.OISOn(j, "Y", false);
                    DrvIC.Move(j, "Y1", y1code[j]);
                    DrvIC.Move(j, "Y2", y2code[j]);
                }
            }
        }

        public void HallDecenter(int port, string name)
        {
            LEDs_All_On(port, true);
            FindResult[] fX = new FindResult[3] { new FindResult(), new FindResult(), new FindResult() };
            FindResult[] fY = new FindResult[3] { new FindResult(), new FindResult(), new FindResult() };
            STATIC.DrvIC.OISOn(0, "X", true);
            STATIC.DrvIC.OISOn(0, "Y", true);
            STATIC.DrvIC.OISOn(0, "AF", true);

            STATIC.DrvIC.Move(0, "X", 2048);
            STATIC.DrvIC.Move(0, "Y", 2048);
            STATIC.DrvIC.Move(0, "AF", F_Manage.bestpos);
            Thread.Sleep(500);
            STATIC.fVision.m__G.oCam[0].GrabA(0);
            fX[0] = STATIC.fVision.MeasureTxTyTz(0, "X", true);
            fY[0] = STATIC.fVision.MeasureTxTyTz(0, "Y", true);

            STATIC.DrvIC.Move(0, "X", 100);
            Thread.Sleep(100);
            STATIC.DrvIC.Move(0, "X", 0);
            Thread.Sleep(500);
            STATIC.fVision.m__G.oCam[0].GrabA(0);
            fX[1] = STATIC.fVision.MeasureTxTyTz(0, "X", true);
            STATIC.DrvIC.Move(0, "X", 3995);
            Thread.Sleep(100);
            STATIC.DrvIC.Move(0, "X", 4095);
            Thread.Sleep(500);
            STATIC.fVision.m__G.oCam[0].GrabA(0);
            fX[2] = STATIC.fVision.MeasureTxTyTz(0, "X", true);
            ////X 측정
            STATIC.DrvIC.Move(0, "X", 2048);
            Thread.Sleep(200);

            STATIC.DrvIC.Move(0, "Y", 100);
            Thread.Sleep(100);
            STATIC.DrvIC.Move(0, "Y", 0);
            Thread.Sleep(500);
            STATIC.fVision.m__G.oCam[0].GrabA(0);
            fY[1] = STATIC.fVision.MeasureTxTyTz(0, "Y", true);
            STATIC.DrvIC.Move(0, "Y", 3995);
            Thread.Sleep(100);
            STATIC.DrvIC.Move(0, "Y", 4095);
            Thread.Sleep(500);
            //Y측정
            STATIC.fVision.m__G.oCam[0].GrabA(0);
            fY[2] = STATIC.fVision.MeasureTxTyTz(0, "Y", true);


            Spec.PassFails[0].Results[(int)SpecItem.x_HallDecenter].Val = ((fX[1].cx[0] + fX[2].cx[0]) / 2.0f) - fX[0].cx[0];
            Spec.PassFails[0].Results[(int)SpecItem.y_HallDecenter].Val = ((fY[1].cy[0] + fY[2].cy[0]) / 2.0f) - fY[0].cy[0];

            Spec.SetResult(0, (int)SpecItem.x_HallDecenter, (int)SpecItem.y_HallDecenter);
            ShowDataResults(0, "Hall Decenter");

            LEDs_All_On(port, false);


        }

        public void ServoDecenter(int port, string name)
        {
            LEDs_All_On(port, true);
            FindResult[] fX = new FindResult[2] { new FindResult(), new FindResult()};
            FindResult[] fY = new FindResult[2] { new FindResult(), new FindResult()};
            STATIC.DrvIC.OISOn(0, "X", true);
            STATIC.DrvIC.OISOn(0, "Y", true);
          

            STATIC.DrvIC.Move(0, "X", 2048);
            STATIC.DrvIC.Move(0, "Y", 2048);
          
            Thread.Sleep(500);
            STATIC.fVision.m__G.oCam[0].GrabA(0);
            fX[0] = STATIC.fVision.MeasureTxTyTz(0, "X", true);

            STATIC.DrvIC.OISOn(0, "X", false);
            Thread.Sleep(500);

            STATIC.fVision.m__G.oCam[0].GrabA(0);
            fX[1] = STATIC.fVision.MeasureTxTyTz(0, "X", true);
            

            Spec.PassFails[0].Results[(int)SpecItem.x_ServoDecenter].Val = fX[0].cx[0] - fX[1].cx[0];
           

            STATIC.DrvIC.OISOn(0, "X", true);
            STATIC.DrvIC.OISOn(0, "Y", true);

            STATIC.DrvIC.Move(0, "X", 2048);
            STATIC.DrvIC.Move(0, "Y", 2048);

            Thread.Sleep(500);
            STATIC.fVision.m__G.oCam[0].GrabA(0);
            fY[0] = STATIC.fVision.MeasureTxTyTz(0, "Y", true);

            STATIC.DrvIC.OISOn(0, "Y", false);

            Thread.Sleep(500);
            STATIC.fVision.m__G.oCam[0].GrabA(0);
            fY[1] = STATIC.fVision.MeasureTxTyTz(0, "Y", true);

            Spec.PassFails[0].Results[(int)SpecItem.y_ServoDecenter].Val = fY[0].cy[0] - fY[1].cy[0];

            Spec.SetResult(0, (int)SpecItem.x_ServoDecenter, (int)SpecItem.y_ServoDecenter);
            ShowDataResults(0, "Servo Decenter");

            LEDs_All_On(port, false);
        }

        public void AddHeadResult(string sFilePath)
        {
            StreamWriter writer;
            writer = File.AppendText(sFilePath);

            string sHeader;
            sHeader = "Time,Index,PlateBCode,LotID,ACTID,Channel,PM Index,PassFail,1st Fail Item,";

            string sParam = "";
            for (int i = (int)SpecItem.OISX_Ratedstroke; i < (int)SpecItem.OISY_Ratedstroke; i++)
            {
                sParam += string.Format("X {0},", Spec.Param[i][1]);
            }
            sHeader += sParam;

            sParam = "";
            for (int i = (int)SpecItem.OISY_Ratedstroke; i < (int)SpecItem.OISY_CrosstalkX; i++)
            {
                sParam += string.Format("Y {0},", Spec.Param[i][1]);
            }
            sHeader += sParam;

            sParam = "";
            for (int i = (int)SpecItem.OISY_CrosstalkX; i < (int)SpecItem.FRAX_PMFreq; i++)
            {
                sParam += string.Format("AF {0},", Spec.Param[i][1]);
            }
            sHeader += sParam;

            sParam = "";
            for (int i = (int)SpecItem.FRAX_PMFreq; i < Spec.Param.Count; i++)
            {
                if (Spec.Param[i][0].Equals("FRA X")) sParam += string.Format("X {0},", Spec.Param[i][1]);
                else if (Spec.Param[i][0].Equals("FRA Y")) sParam += string.Format("Y {0},", Spec.Param[i][1]);
                else sParam += string.Format("{0},", Spec.Param[i][1]);
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
            for (int i = (int)SpecItem.OISX_Ratedstroke; i < Spec.Param.Count; i++)
            {
                sParam += string.Format("({0}),", Spec.Param[i][9]);
            }
            sHeader += sParam;

            writer.WriteLine(sHeader);

            sHeader = "Spec Min,,,,,,,,,";
            sParam = "";
            for (int i = (int)SpecItem.OISX_Ratedstroke; i < Spec.Param.Count; i++)
            {
                sParam += string.Format("{0},", Spec.Param[i][2]);
            }
            sHeader += sParam;

            writer.WriteLine(sHeader);

            sHeader = "Spec Max,,,,,,,,,";
            sParam = "";
            for (int i = (int)SpecItem.OISX_Ratedstroke; i < Spec.Param.Count; i++)
            {
                sParam += string.Format("{0},", Spec.Param[i][3]);
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
                if (errMsg[j].Contains("Hall Cal")) { Spec.PassFails[j].FirstFailIndex = -1; }
                else if (errMsg[j] == "Socket Empty") { Spec.PassFails[j].FirstFailIndex = -2; }
                else
                {
                    for (int k = 0; k < ItemList.Count; k++)
                    {
                        if (errMsg[j].Contains(ItemList[k].Name))
                        {
                            Spec.PassFails[j].FirstFailIndex = (-(k + 2));
                        }
                    }
                }

                AddLog(j, string.Format("ch : {0}, msg : {1}, PassFail : {2}", j, errMsg[j], Spec.PassFails[j].FirstFailIndex));

                //"Time,Index,PlateBCode,LotID,ACTID,Channel,PM Index,PassFail,"
                log += string.Format("'{0},{1},{2},{3},{4},{5},{6},{7},",
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), m_StrIndex[j], "", Model.LotID, "", j, "", Spec.PassFails[j].TotalFail);

                Spec.TotlaTested++;
                //1st Fail Item
                if (Spec.PassFails[j].FirstFailIndex > 0)
                {
                    errMsg[j] = Spec.PassFails[j].FirstFail;
                    Spec.TotlaFailed++;
                    AddLog(j, "Fail : " + errMsg[j]);
                    log += errMsg[j] + ",";
                }
                else if (Spec.PassFails[j].FirstFailIndex < 0)
                {
                    Spec.TotlaTested--;
                    log += errMsg[j] + ",";
                }
                else
                {
                    if (m_ChannelOn[j])
                    {
                        Spec.TotlaPassed++;
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
                    log += string.Format("{0:0.000},", Spec.PassFails[j].Results[i].Val);
                }

                //  Y Results
                for (int i = (int)SpecItem.OISY_Ratedstroke; i < (int)SpecItem.AF_Ratedstroke; i++)
                {
                    log += string.Format("{0:0.000},", Spec.PassFails[j].Results[i].Val);
                }

                //  AF Results
                for (int i = (int)SpecItem.AF_Ratedstroke; i < (int)SpecItem.FRAX_PMFreq; i++)
                {
                    log += string.Format("{0:0.000},", Spec.PassFails[j].Results[i].Val);
                }

                for (int i = (int)SpecItem.FRAX_PMFreq; i < Spec.Param.Count; i++)
                {
                    log += string.Format("{0:0.000},", Spec.PassFails[j].Results[i].Val);
                }

                //Time
                for (int i = 0; i < ItemList.Count; i++)
                {
                    log += string.Format("{0:0.000},", ItemList[i].Time);
                }

                log += string.Format("{0:0.000},", Spec.PassFails[ch].TotalTime);

                sw.WriteLine(log);
            }
            sw.Close();
        }
    }
}
