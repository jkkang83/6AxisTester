using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Xml.Linq;

namespace FZ4P
{
    public delegate void FunctionPointer(int ch, string testItem);
    public class ActItems
    {
        public string Name { get; set; }
        public FunctionPointer Func { get; set; }
        public bool IsMulti { get; set; }
        public string Time { get; set; }
    }
    public class FindResult
    {
        public double[] cx = new double[2];
        public double[] cy = new double[2];
        public double[] cz = new double[2];
        public long nFound = 0;
        public double[] tx = new double[2];
        public double[] ty = new double[2];
        public double[] tz = new double[2];

        public double[] cy1 = new double[2];
        public double[] cy2 = new double[2];
    }
    public class NVMHallParam
    {
        public int XHOffset;
        public int YHOffset;
        public int XHBias;
        public int YHBias;
        public int XHmin;
        public int XHmax;
        public int YHmin;
        public int YHmax;
        public int XHmid;
        public int YHmid;
        public int XDrv_Min;
        public int XDrv_Max;
        public int YDrv_Min;
        public int YDrv_Max;
        public int XNEPAmin;
        public int XNEPAmax;
        public int YNEPAmin;
        public int YNEPAmax;
        public void Clear()
        {
            XHOffset = 0;
            YHOffset = 0;
            XHBias = 0;
            YHBias = 0;
            XHmin = 0;
            XHmax = 0;
            YHmin = 0;
            YHmax = 0;
            XHmid = 0;
            YHmid = 0;
            XDrv_Min = 0;
            XDrv_Max = 0;
            YDrv_Min = 0;
            YDrv_Max = 0;
            XNEPAmin = 0;
            XNEPAmax = 0;
            YNEPAmin = 0;
            YNEPAmax = 0;
        }
    };
    public class LogText
    {
        public TextBox box = new TextBox();
        public LogText()
        {
            box.BackColor = Color.Black;
            box.BorderStyle = BorderStyle.FixedSingle;
            box.Font = new Font("맑은 고딕", 9F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            box.ForeColor = Color.LemonChiffon;
            box.Multiline = true;
            box.ReadOnly = true;
            box.ScrollBars = ScrollBars.Vertical;
            box.Size = new System.Drawing.Size(474, 70);
            box.TabIndex = 127;
            box.Tag = "S";
            box.MouseDoubleClick += new MouseEventHandler(MouseDoubleClick);
        }
        private void MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (box.Tag.ToString() == "S")
            {
                box.Size = new System.Drawing.Size(475, 638);
                box.Tag = "L";
            }
            else
            {
                box.Size = new System.Drawing.Size(475, 70);
                box.Tag = "S";
            }
        }
        public void Log(string e)
        {
            if (box.InvokeRequired)
            {
                box.BeginInvoke((MethodInvoker)delegate
                {
                    box.AppendText(e + "\r\n");
                });
            }
            else
                box.AppendText(e + "\r\n");
        }
        public void Clear()
        {
            box.Text = "";
        }
    }
    public class InfoButton
    {
        public Button btn = new Button();
        public InfoButton()
        {
            btn.BackColor = Color.Black;
            btn.Font = new Font("Microsoft Sans Serif", 60F, System.Drawing.FontStyle.Bold);
            btn.ForeColor = Color.Transparent;
            btn.Size = new System.Drawing.Size(470, 200);
            btn.Text = "Start Test";
            btn.UseVisualStyleBackColor = false;
            btn.Click += new EventHandler(this.Info_Click);
        }
        private void Info_Click(object sender, EventArgs e)
        {
            btn.Hide();
        }
    }
    public class DrvParam
    {
        public int XPCAL;
        public int XNCAL;
        public int YPCAL;
        public int YNCAL;

        public int XPOSVT;
        public int XNEGVT;
        public int YPOSVT;
        public int YNEGVT;

        public int XLC2C;
        public int XLC2D;
        public int XLC2E;

        public int YLC2C;
        public int YLC2D;
        public int YLC2E;

        public void Clear()
        {
            XPCAL = 0;
            XNCAL = 0;
            YPCAL = 0;
            YNCAL = 0;

            XPOSVT = 0;
            XNEGVT = 0;
            YPOSVT = 0;
            YNEGVT = 0;

            XLC2C = 0;
            XLC2D = 0;
            XLC2E = 0;

            YLC2C = 0;
            YLC2D = 0;
            YLC2E = 0;
        }
    };
    public class CalResult
    {
        public string Name { get; set; }
        public List<int> CodeZ = new List<int>();
        public List<int> CodeX = new List<int>();
        public List<int> CodeY1 = new List<int>();
        public List<int> CodeY2 = new List<int>();
        public List<double> StrokeX = new List<double>();
        public List<double> StrokeY = new List<double>();
        public List<double> StrokeZ = new List<double>();
        public List<double> StrokeY1 = new List<double>();
        public List<double> StrokeY2 = new List<double>();
        public List<int> HallX = new List<int>();
        public List<int> HallY = new List<int>();
        public List<int> HallZ = new List<int>();
        public List<int> HallY1 = new List<int>();
        public List<int> HallY2 = new List<int>();
        public List<double> Current = new List<double>();
        public List<double> Time = new List<double>();
        public List<double> TiltX = new List<double>();
        public List<double> TiltY = new List<double>();
        public List<double> TiltZ = new List<double>();
        public struct SPoint
        {
            public double x;
            public double y;
        };
        public struct SLine
        {
            public double dSlope;
            public double dYintercept;
        };
        public SLine Line_fitting(SPoint[] data, int dataSize)
        {
            SLine rtnLine;
            double SUMx = 0; //sum of x values
            double SUMy = 0; //sum of y values
            double SUMxy = 0; //sum of x * y
            double SUMxx = 0; //sum of x^2 
            double slope = 0; //slope of regression line
            double y_intercept = 0; //y intercept of regression line 
            double AVGy = 0; //mean of y
            double AVGx = 0; //mean of x

            //calculate various sums 
            for (int i = 0; i < dataSize; i++)
            {
                SUMx = SUMx + data[i].x;
                SUMy = SUMy + data[i].y;
                SUMxy = SUMxy + data[i].x * data[i].y;
                SUMxx = SUMxx + data[i].x * data[i].x;
            }

            //calculate the means of x and y
            AVGy = SUMy / dataSize;
            AVGx = SUMx / dataSize;

            //slope or a1
            slope = (dataSize * SUMxy - SUMx * SUMy) / (dataSize * SUMxx - SUMx * SUMx);

            //y itercept or a0
            y_intercept = AVGy - slope * AVGx;

            rtnLine.dSlope = slope;
            rtnLine.dYintercept = y_intercept;

            return rtnLine;
        }
        public SLine Line_fitting(List<SPoint> data)
        {
            SLine rtnLine;
            double SUMx = 0; //sum of x values
            double SUMy = 0; //sum of y values
            double SUMxy = 0; //sum of x * y
            double SUMxx = 0; //sum of x^2 
            double slope = 0; //slope of regression line
            double y_intercept = 0; //y intercept of regression line 
            double AVGy = 0; //mean of y
            double AVGx = 0; //mean of x

            //calculate various sums 
            for (int i = 0; i < data.Count; i++)
            {
                SUMx = SUMx + data[i].x;
                SUMy = SUMy + data[i].y;
                SUMxy = SUMxy + data[i].x * data[i].y;
                SUMxx = SUMxx + data[i].x * data[i].x;
            }

            //calculate the means of x and y
            AVGy = SUMy / data.Count;
            AVGx = SUMx / data.Count;

            //slope or a1
            slope = (data.Count * SUMxy - SUMx * SUMy) / (data.Count * SUMxx - SUMx * SUMx);

            //y itercept or a0
            y_intercept = AVGy - slope * AVGx;

            rtnLine.dSlope = slope;
            rtnLine.dYintercept = y_intercept;

            return rtnLine;
        }
        public void Clear()
        {
            CodeZ.Clear();
            CodeX.Clear();
            CodeY1.Clear();
            CodeY2.Clear();
            StrokeX.Clear();
            StrokeY.Clear();
            StrokeZ.Clear();
            StrokeY1.Clear();
            StrokeY2.Clear();
            HallX.Clear();
            HallY.Clear();
            HallZ.Clear();
            HallY1.Clear();
            HallY2.Clear();


            Current.Clear();
            Time.Clear();
            TiltX.Clear();
            TiltY.Clear();
            TiltZ.Clear();
        }
        public CalResult(string name)
        {
            Name = name;
        }
        // mode 0 : code, mode 1 : stroke
        public double CalFwdStoke(List<int> code, List<double> stroke)
        {
            double min = 9999;
            double max = -9999;

            for (int i = 2; i < code.Count / 2; i++)
            {
                if (code[i] >= 2048)
                {
                    if (min > stroke[i])
                        min = stroke[i];
                    if (max < stroke[i])
                        max = stroke[i];
                }
            }
            return Math.Abs(max - min);
        }
        public double CalBwdStoke(List<int> code, List<double> stroke)
        {
            double min = 9999;
            double max = -9999;
            for (int i = code.Count / 2; i < code.Count; i++)
            {
                if (code[i] <= 2048)
                {
                    if (min > stroke[i])
                        min = stroke[i];
                    if (max < stroke[i])
                        max = stroke[i];
                }
            }

            return Math.Abs(max - min);
        }
        public double CalSensitivity(List<int> code, List<double> stroke, int CodeRange, int StrokeRange)
        {
            List<SPoint> point = new List<SPoint>();

            for (int i = 2; i < code.Count; i++)
            {
                if (code[i] >= 2048 - CodeRange && code[i] <= 2048 + CodeRange && stroke[i] >= -StrokeRange && stroke[i] <= StrokeRange)
                {
                    point.Add(new SPoint() { x = code[i], y = stroke[i] });
                }
            }

            SLine result = Line_fitting(point);

            return result.dSlope;
        }
        public double CalLinearity(List<int> code, List<double> stroke, int CodeRange, int StrokeRange)
        {
            List<SPoint> point = new List<SPoint>();
            double max = -9999;

            for (int i = 2; i < code.Count; i++)
            {
                if (code[i] >= 2048 - CodeRange && code[i] <= 2048 + CodeRange && stroke[i] >= -StrokeRange && stroke[i] <= StrokeRange)
                
                {
                    point.Add(new SPoint() { x = code[i], y = stroke[i] });
                }
            }

            SLine result = Line_fitting(point);

            for (int i = 2; i < point.Count; i++)
            {
                double newS = result.dSlope * point[i].x + result.dYintercept;
                if (max < Math.Abs(newS - point[i].y))
                    max = Math.Abs(newS - point[i].y);
            }

            return max;
        }
        public double CalHysteresis(List<int> code, List<double> stroke, int CodeRange, int StrokeRange)
        {
            double max = -9999;
            int fwdIndex = 0;
            int bwdIndex = 0;
            for (int i = 2; i < code.Count - 1; i++)
            {
                if (code[i + 1] < code[i])
                {
                    fwdIndex = i;
                    break;
                }
            }
            for (int i = fwdIndex; i < code.Count - 1; i++)
            {
                if (code[i + 1] <= 2048)
                {
                    bwdIndex = i;
                    break;
                }
            }
            for (int i = 2; i < fwdIndex; i++)
            {
                for (int j = fwdIndex; j < bwdIndex; j++)
                {
                    if (code[i] == code[j])
                    {
                        if (code[i] >= 2048 - CodeRange && code[i] <= 2048 + CodeRange && stroke[i] >= -StrokeRange && stroke[i] <= StrokeRange)
                        {
                            double Hyst = Math.Abs(stroke[i] - stroke[j]);
                            if (max < Hyst)
                                max = Hyst;
                        }
                    }
                }
            }
            for (int i = bwdIndex; i < code.Count - 1; i++)
            {
                if (code[i + 1] > code[i])
                {
                    fwdIndex = i;
                    break;
                }
            }
            for (int i = fwdIndex; i < code.Count - 1; i++)
            {
                if (code[i + 1] > code[i])
                {
                    fwdIndex = i;
                    break;
                }
            }
            for (int i = fwdIndex; i < code.Count - 1; i++)
            {
                if (code[i] >= 2048)
                {
                    bwdIndex = i;
                    break;
                }
            }
            for (int i = 2; i < fwdIndex; i++)
            {
                for (int j = fwdIndex; j < bwdIndex; j++)
                {
                    if (code[i] == code[j])
                    {
                        if (code[i] >= 2048 - CodeRange && code[i] <= 2048 + CodeRange && stroke[i] >= -StrokeRange && stroke[i] <= StrokeRange)
                        {
                            double Hyst = Math.Abs(stroke[i] - stroke[j]);
                            if (max < Hyst)
                                max = Hyst;
                        }
                    }
                }
            }
            return max;
        }
        public double CalResolution(List<int> code, List<double> stroke, int CodeRange, int StrokeRange)
        {
            double Resolution = 0;
            double max = -9999;

            for (int i = 0; i < code.Count; i++)
            {
                if (code[i] >= 2048 - CodeRange && code[i] <= 2048 + CodeRange && stroke[i] >= -StrokeRange && stroke[i] <= StrokeRange)
                {
                    if (i > 1 && (code[i] != code[i - 1]))
                        Resolution = (stroke[i] - stroke[i - 1]) / (code[i] - code[i - 1]);
                    if (max < Resolution)
                        max = Resolution;
                }
            }

            return max;
        }
        public double CalMaxCurrent(List<int> code, List<double> stroke, int CodeRange, int StrokeRange)
        {
            double max = 0;

            for (int i = 2; i < code.Count; i++)
            {
                if (code[i] >= 2048 - CodeRange && code[i] <= 2048 + CodeRange && stroke[i] >= -StrokeRange && stroke[i] <= StrokeRange)
                {
                    if (Math.Abs(max) < Math.Abs(Current[i]))
                        max = Current[i];
                }
            }
            return max;
        }
        public double CalCenterCurrent(List<int> code, List<double> stroke, int CodeRange, int StrokeRange)
        {
            double min = 9999;
            double max = 0;

            for (int i = 2; i < code.Count; i++)
            {
                //if (code[i] >= 2048 - CodeRange && code[i] <= 2048 + CodeRange && stroke[i] >= -StrokeRange && stroke[i] <= StrokeRange)
                if (code[i] >= 480 && code[i] <= 2895)
                {
                    if (min > Math.Abs(Current[i]))
                        min = Math.Abs(Current[i]);
                    if (max < Math.Abs(Current[i]))
                        max = Math.Abs(Current[i]);
                }
            }
            return ((max - min) / 2) + min;
        }
        public double CalHoldingCurrent(List<int> code, List<double> stroke, int CodeRange, int StrokeRange)
        {
            double min = 9999;
            double max = 0;

            for (int i = 2; i < code.Count; i++)
            {
                if (code[i] >= 480 && code[i] <= 2895)
                {
                    //if (min > Math.Abs(Current[i]))
                    //    min = Math.Abs(Current[i]);
                    if (max < Math.Abs(Current[i]))
                        max = Math.Abs(Current[i]);
                }
            }
            return max;
        }
        public double CalCrosstalk(List<int> code, List<double> stroke, int CodeRange, int StrokeRange)
        {
            double minStroke = 9999;
            double maxStroke = -9999;

            for (int i = 0; i < code.Count; i++)
            {
                if (code[i] >= 2048 - CodeRange && code[i] <= 2048 + CodeRange && stroke[i] >= -StrokeRange && stroke[i] <= StrokeRange)
                {
                    if (minStroke > stroke[i])
                        minStroke = stroke[i];
                    if (maxStroke < stroke[i])
                        maxStroke = stroke[i];
                }
            }

            return Math.Abs(maxStroke - minStroke);

        }
        public double CalCrosstalkR(List<int> code, List<double> stroke1, List<double> stroke2, int CodeRange, int StrokeRange)
        {
            double minR = 9999;
            double maxR = -9999;

            for (int i = 0; i < code.Count; i++)
            {
                if (code[i] >= 2048 - CodeRange && code[i] <= 2048 + CodeRange)
                {
                    double news = Math.Sqrt( Math.Pow( stroke1[i], 2) + Math.Pow(stroke2[i], 2));
                    if (minR > news)
                        minR = news;
                    if (maxR < news)
                        maxR = news;
                }
            }

            return Math.Abs(maxR - minR);

        }
        public double CalRolling(List<int> x, List<double> y, int CodeRange, int StrokeRange)
        {
            double minTilt = 9999;
            double maxTilt = -9999;

            for (int i = 2; i < x.Count; i++)
            {
                if (x[i] >= 2048 - CodeRange && x[i] <= 2048 + CodeRange && y[i] >= -StrokeRange && y[i] <= StrokeRange)
                {
                    if (minTilt > TiltZ[i])
                        minTilt = TiltZ[i];
                    if (maxTilt < TiltZ[i])
                        maxTilt = TiltZ[i];
                }
            }

            return Math.Abs(maxTilt - minTilt);

        }
    }
    public class ChartList
    {
        public Chart C = new Chart();
        public string Title = "";
        public string Type = "";
        public int Ch = 0;
        public bool IsFalg = false;
        public bool IsAEnable = true;
        public int StrokeNum = 0;
        public int TiltNum = 0;

        public Rectangle OldPt;
        public ChartList(string type, int ch)
        {
            Type = type;
            Ch = ch;
            C.ChartAreas.Add(new ChartArea());
            C.ChartAreas[0].Position.X = 0;
            C.ChartAreas[0].Position.Y = 0;
            C.ChartAreas[0].Position.Height = 99;
            C.ChartAreas[0].Position.Width = 100;
            C.ChartAreas[0].AxisX.LabelStyle.Font = new Font("Calibri", 9, FontStyle.Bold);
            C.ChartAreas[0].AxisY.LabelStyle.Font = new Font("Calibri", 9, FontStyle.Bold);
            C.ChartAreas[0].AxisX.ScaleView.Position = 0;
            C.ChartAreas[0].AxisY.ScaleView.Position = 0;
            C.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.LightGray;
            C.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.LightGray;
            C.ChartAreas[0].AxisX.MinorGrid.LineColor = Color.WhiteSmoke;
            C.ChartAreas[0].AxisY.MinorGrid.LineColor = Color.WhiteSmoke;
            C.ChartAreas[0].AxisX.LineColor = Color.DarkGray;
            C.ChartAreas[0].AxisY.LineColor = Color.DarkGray;
            C.BackColor = SystemColors.ControlLightLight;
            C.Legends.Add(new Legend());
            C.Legends[0].Position = new ElementPosition(5, 0, 30, 20);
            C.Legends[0].BackColor = Color.Transparent;
            C.Size = new System.Drawing.Size(475, 280);
            C.MouseDoubleClick += new MouseEventHandler(MouseDoubleClick);
            C.Tag = "S";
            int numSeries = 0;
            if (type == "Stroke")
            {
                C.ChartAreas[0].AxisY.MinorGrid.Interval = .1;
                C.ChartAreas[0].AxisX.MinorGrid.Enabled = false;
                C.ChartAreas[0].AxisY.MinorGrid.Enabled = false;
                C.ChartAreas[0].AxisX.Minimum = 0;
                C.ChartAreas[0].AxisX.Maximum = 4100;
                C.ChartAreas[0].AxisX.Interval = 400;
                C.ChartAreas[0].AxisY.Minimum = 0;
                C.ChartAreas[0].AxisY.Maximum = 1500;
                C.ChartAreas[0].AxisY.Interval = 150;
                C.Titles.Add("Stroke vs Code");
                //  Stroke, Current, Cross Stroke
                C.Series.Add("X Code Stroke"); //0
                C.Series[numSeries].Label = "X Code Stroke";
                C.Series[numSeries].ChartType = SeriesChartType.FastLine;
                C.Series[numSeries].Color = Color.Red;
                C.Series[numSeries].IsVisibleInLegend = true;
                numSeries++;
                C.Series.Add("Y Code Stroke"); //1
                C.Series[numSeries].Label = "Y Code Stroke";
                C.Series[numSeries].ChartType = SeriesChartType.FastLine;
                C.Series[numSeries].Color = Color.Blue;
                C.Series[numSeries].IsVisibleInLegend = true;
                numSeries++;
                C.Series.Add("AF Code Stroke"); //2
                C.Series[numSeries].Label = "AF Code Stroke";
                C.Series[numSeries].ChartType = SeriesChartType.FastLine;
                C.Series[numSeries].Color = Color.Green;
                C.Series[numSeries].IsVisibleInLegend = true;
                numSeries++;
                C.Series.Add("X Current"); //3
                C.Series[numSeries].Label = "X Current";
                C.Series[numSeries].ChartType = SeriesChartType.FastLine;
                C.Series[numSeries].Color = Color.LightPink;
                C.Series[numSeries].IsVisibleInLegend = true;
                C.Series[numSeries].YAxisType = AxisType.Secondary;
                numSeries++;
                C.Series.Add("Y Current"); //4
                C.Series[numSeries].Label = "Y Current";
                C.Series[numSeries].ChartType = SeriesChartType.FastLine;
                C.Series[numSeries].Color = Color.Turquoise;
                C.Series[numSeries].IsVisibleInLegend = true;
                C.Series[numSeries].YAxisType = AxisType.Secondary;
                numSeries++;
                C.Series.Add("AF Current"); //5
                C.Series[numSeries].Label = "AF Current";
                C.Series[numSeries].ChartType = SeriesChartType.FastLine;
                C.Series[numSeries].Color = Color.Violet;
                C.Series[numSeries].IsVisibleInLegend = true;
                C.Series[numSeries].YAxisType = AxisType.Secondary;
                numSeries++;
                C.Series.Add("X Hall"); //6 
                C.Series[numSeries].Label = "X Hall";
                C.Series[numSeries].ChartType = SeriesChartType.FastLine;
                C.Series[numSeries].Color = Color.Bisque;
                C.Series[numSeries].IsVisibleInLegend = true;
                C.Series[numSeries].YAxisType = AxisType.Secondary;
                numSeries++;
                C.Series.Add("Y Hall"); //7
                C.Series[numSeries].Label = "Y Hall";
                C.Series[numSeries].ChartType = SeriesChartType.FastLine;
                C.Series[numSeries].Color = Color.LightSkyBlue;
                C.Series[numSeries].IsVisibleInLegend = true;
                C.Series[numSeries].YAxisType = AxisType.Secondary;
                numSeries++;
                C.Series.Add("AF Hall"); //8
                C.Series[numSeries].Label = "AF Hall";
                C.Series[numSeries].ChartType = SeriesChartType.FastLine;
                C.Series[numSeries].Color = Color.LightSlateGray;
                C.Series[numSeries].IsVisibleInLegend = true;
                C.Series[numSeries].YAxisType = AxisType.Secondary;
                numSeries++;
                C.Series.Add("Y1 Code Stroke"); //9
                C.Series[numSeries].Label = "Y1 Code Stroke";
                C.Series[numSeries].ChartType = SeriesChartType.FastLine;
                C.Series[numSeries].Color = Color.BlueViolet;
                C.Series[numSeries].IsVisibleInLegend = true;
                numSeries++;
                C.Series.Add("Y2 Code Stroke"); //10
                C.Series[numSeries].Label = "Y2 Code Stroke";
                C.Series[numSeries].ChartType = SeriesChartType.FastLine;
                C.Series[numSeries].Color = Color.DarkBlue;
                C.Series[numSeries].IsVisibleInLegend = true;
            }
            else if (type == "Tilt")
            {
                C.ChartAreas[0].AxisY.MinorGrid.Interval = .1;
                C.ChartAreas[0].AxisX.MinorGrid.Enabled = false;
                C.ChartAreas[0].AxisY.MinorGrid.Enabled = false;
                C.ChartAreas[0].AxisX.Minimum = 0;
                C.ChartAreas[0].AxisX.Maximum = 4100;
                C.ChartAreas[0].AxisX.Interval = 400;
                C.ChartAreas[0].AxisY.Minimum = -40;
                C.ChartAreas[0].AxisY.Maximum = 40;
                C.ChartAreas[0].AxisY.Interval = 8;

                C.Titles.Add("Tilt vs Code");

                C.Series.Add("X Tx");
                C.Series[numSeries].Label = "X Tx";
                C.Series[numSeries].ChartType = SeriesChartType.FastLine;
                C.Series[numSeries].Color = Color.Red;
                C.Series[numSeries].IsVisibleInLegend = true;
                numSeries++;
                C.Series.Add("X Ty");
                C.Series[numSeries].Label = "X Ty";
                C.Series[numSeries].ChartType = SeriesChartType.FastLine;
                C.Series[numSeries].Color = Color.DarkRed;
                C.Series[numSeries].IsVisibleInLegend = true;
                numSeries++;
                C.Series.Add("X Tz");
                C.Series[numSeries].Label = "X Tz";
                C.Series[numSeries].ChartType = SeriesChartType.FastLine;
                C.Series[numSeries].Color = Color.Red;
                C.Series[numSeries].IsVisibleInLegend = true;
                C.Series[numSeries].YAxisType = AxisType.Secondary;
                numSeries++;
                C.Series.Add("Y Tx");
                C.Series[numSeries].Label = "Y Tx";
                C.Series[numSeries].ChartType = SeriesChartType.FastLine;
                C.Series[numSeries].Color = Color.Blue;
                C.Series[numSeries].IsVisibleInLegend = true;
                numSeries++;
                C.Series.Add("Y Ty");
                C.Series[numSeries].Label = "Y Ty";
                C.Series[numSeries].ChartType = SeriesChartType.FastLine;
                C.Series[numSeries].Color = Color.BlueViolet;
                C.Series[numSeries].IsVisibleInLegend = true;
                numSeries++;
                C.Series.Add("Y Tz");
                C.Series[numSeries].Label = "Y Tz";
                C.Series[numSeries].ChartType = SeriesChartType.FastLine;
                C.Series[numSeries].Color = Color.Blue;
                C.Series[numSeries].IsVisibleInLegend = true;
                C.Series[numSeries].YAxisType = AxisType.Secondary;
                numSeries++;
                C.Series.Add("AF Tx");
                C.Series[numSeries].Label = "AF Tx";
                C.Series[numSeries].ChartType = SeriesChartType.FastLine;
                C.Series[numSeries].Color = Color.Green;
                C.Series[numSeries].IsVisibleInLegend = true;
                numSeries++;
                C.Series.Add("AF Ty");
                C.Series[numSeries].Label = "AF Ty";
                C.Series[numSeries].ChartType = SeriesChartType.FastLine;
                C.Series[numSeries].Color = Color.GreenYellow;
                C.Series[numSeries].IsVisibleInLegend = true;
                numSeries++;
                C.Series.Add("AF Tz");
                C.Series[numSeries].Label = "AF Tz";
                C.Series[numSeries].ChartType = SeriesChartType.FastLine;
                C.Series[numSeries].Color = Color.Green;
                C.Series[numSeries].IsVisibleInLegend = true;
                C.Series[numSeries].YAxisType = AxisType.Secondary;
            }//Tilt
        }
        private void MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.X >= (C.Width - 40))
            {
                //if (IsAEnable)
                //{
                //    foreach (var c in C.Series) if (c.YAxisType == AxisType.Primary) c.Enabled = false;
                //    IsAEnable = false;
                //}
                //else
                //{
                //    foreach (var c in C.Series) c.Enabled = true;
                //    IsAEnable = true;
                //}
                foreach (var c in C.Series) c.Enabled = false;
                if (StrokeNum == 0) foreach (var c in C.Series) if (c.Name.Contains("X")) c.Enabled = true;
                if (StrokeNum == 1) foreach (var c in C.Series) if (c.Name.Contains("Y")) c.Enabled = true;
                if (StrokeNum == 2) foreach (var c in C.Series) if (c.Name.Contains("AF")) c.Enabled = true;
                if (StrokeNum == 3) foreach (var c in C.Series) c.Enabled = true;
                StrokeNum++;
                if (StrokeNum > 3) StrokeNum = 0;
                return;
            }
            if (e.X <= 40)
            {
                //if (IsAEnable)
                //{
                //    foreach (var c in C.Series) if (c.YAxisType != AxisType.Primary) c.Enabled = false;
                //    IsAEnable = false;
                //}
                //else
                //{
                //    foreach (var c in C.Series) c.Enabled = true;
                //    IsAEnable = true;
                //}
                return;
            }
            if (C.Tag.ToString() == "S")
            {
                OldPt.Width = C.Width;
                OldPt.Height = C.Height;
                OldPt.X = C.Left;
                OldPt.Y = C.Top;

                C.Width = 953;
                C.Height = 563;
                C.Left = 3 + (Ch / 2) * 953;
                C.Top = 117;
                Title = C.Titles[0].Text;
                C.Titles[0].Text = Title + " Ch " + Ch.ToString();
                C.Titles[0].Font = new Font("Malgun Gothic", 14, FontStyle.Bold); ;
                C.ChartAreas[0].AxisY.MinorGrid.Enabled = true;
                C.ChartAreas[0].AxisY.LabelStyle.Format = "0";
                if (Type == "Stroke")
                    C.Legends[0].Position = new ElementPosition(5, 0, 30, 50);
                else
                    C.Legends[0].Position = new ElementPosition(5, 0, 20, 20);
                C.BringToFront();
                C.Tag = "L";
            }
            else
            {
                C.Width = OldPt.Width;
                C.Height = OldPt.Height;
                C.Left = OldPt.X;
                C.Top = OldPt.Y;
                C.SendToBack();
                C.Titles[0].Text = Title;
                C.Titles[0].Font = new Font("Malgun Gothic", 9, FontStyle.Bold); ;
                C.ChartAreas[0].AxisY.MinorGrid.Enabled = false;
                C.ChartAreas[0].AxisY.LabelStyle.Format = "0";
                C.Legends[0].Position = new ElementPosition(5, 0, 30, 20);
                C.Tag = "S";
            }
        }
    }
}
