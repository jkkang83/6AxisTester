using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace FZ4P
{
    public class ActroChart : Chart
    {
        public string Title = "";
        public string Type = "";
        public int Ch = 0;
        public bool IsFalg = false;

        public Rectangle OldPt;

        public ActroChart(string type, int ch)
        {
            Type = type;
            Ch = ch;

            BackColor = Color.Black;

            ChartAreas.Add(new ChartArea()
            {
                BackColor = Color.Black,

                Position =
                {
                    X = 0,
                    Y = 0,
                    Height = 99 ,
                    Width = 100
                },
                AxisX =
                {
                    LabelStyle = { Font = new Font("Calibri", 7, FontStyle.Bold) } ,
                    ScaleView = { Position = 0 },
                    MajorGrid = { LineColor =  Color.FromArgb(30, 30, 30)  },
                    MinorGrid = { LineColor =  Color.FromArgb(15, 15, 15)  },
                    LineColor = Color.FromArgb(30, 30, 30),
                    LineDashStyle = ChartDashStyle.Solid,
                },
                AxisY =
                {
                    LabelStyle = { Font = new Font("Calibri", 7, FontStyle.Bold) },
                    ScaleView = { Position = 0 },
                    MajorGrid = { LineColor =  Color.FromArgb(30, 30, 30)  },
                    MinorGrid = { LineColor =  Color.FromArgb(15, 15, 15)  },
                    LineColor = Color.FromArgb(30, 30, 30),
                    LineDashStyle = ChartDashStyle.Solid,
                },
            });
            //ChartAreas[0].Position.X = 0;
            //ChartAreas[0].Position.Y = 0;
            //ChartAreas[0].Position.Height = 99;
            //ChartAreas[0].Position.Width = 100;
            //ChartAreas[0].AxisX.LabelStyle.Font = new Font("Calibri", 7, FontStyle.Bold);
            //ChartAreas[0].AxisY.LabelStyle.Font = new Font("Calibri", 7, FontStyle.Bold);
            //ChartAreas[0].AxisX.ScaleView.Position = 0;
            //ChartAreas[0].AxisY.ScaleView.Position = 0;
            //ChartAreas[0].AxisX.MajorGrid.LineColor = Color.FromArgb(30, 30, 30);             //라이트 그레이
            //ChartAreas[0].AxisY.MajorGrid.LineColor = Color.FromArgb(30, 30, 30);             //라이트 그레이
            //ChartAreas[0].AxisX.MinorGrid.LineColor = Color.FromArgb(15, 15, 15);             //화이트스모크
            //ChartAreas[0].AxisY.MinorGrid.LineColor = Color.FromArgb(15, 15, 15);             //화이트스모크
            //ChartAreas[0].AxisX.LineColor = Color.Black;                                      //다크 그레이
            //ChartAreas[0].AxisY.LineColor = Color.Black;                                      //다크 그레이
            //ChartAreas[0].BackColor = Color.Black;
            //ChartAreas[0].AxisX.LineDashStyle = ChartDashStyle.Solid;
            //ChartAreas[0].AxisX.LineColor = Color.FromArgb(30, 30, 30);
            //ChartAreas[0].AxisY.LineDashStyle = ChartDashStyle.Solid;
            //ChartAreas[0].AxisY.LineColor = Color.FromArgb(30, 30, 30);
            //C.BackColor = SystemColors.ControlLightLight;                                       //
            Legends.Add(new Legend());
            Legends[0].Position = new ElementPosition(5, 0, 40, 18);
            Legends[0].BackColor = Color.Transparent;
            Legends[0].ForeColor = Color.White;
            Size = new Size(475, 280);
            
            Tag = "S";
            int numSeries = 0;
            if (type == "Stroke")
            {
                ChartAreas[0].AxisY.MinorGrid.Interval = .1;
                ChartAreas[0].AxisX.MinorGrid.Enabled = false;
                ChartAreas[0].AxisY.MinorGrid.Enabled = false;
                ChartAreas[0].AxisY.Minimum = -4;
                ChartAreas[0].AxisY.Maximum = 4;
                ChartAreas[0].AxisY.Interval = 1;
                ChartAreas[0].AxisY2.LabelStyle.Font = new Font("Calibri", 7, FontStyle.Bold);
                ChartAreas[0].AxisX.LabelStyle.Format = "#";
                ChartAreas[0].AxisX.IsLabelAutoFit = false;
                ChartAreas[0].AxisX.LabelStyle.Angle = 45;
                ChartAreas[0].AxisY.LabelStyle.Format = "0.00";
                ChartAreas[0].AxisY2.LabelStyle.Format = "0.0";
                ChartAreas[0].AxisY.LabelStyle.Format = "0.00";
                ChartAreas[0].AxisY2.LabelStyle.Format = "0.0";

                ChartAreas[0].AxisX.LabelStyle.ForeColor = Color.White;
                ChartAreas[0].AxisY.LabelStyle.ForeColor = Color.White;


                Titles.Add("Stroke vs Code");

                Series.Add("X Code Stroke"); //0
                Series[numSeries].Label = "X Code Stroke";
                Series[numSeries].ChartType = SeriesChartType.FastLine;
                Series[numSeries].Color = Color.Red;
                Series[numSeries].IsVisibleInLegend = true;
                Series[numSeries].Font = new Font("Calibri", 4, FontStyle.Regular);
                Series[numSeries].BorderWidth = 3;
                numSeries++;
                Series.Add("Y Code Stroke"); //1
                Series[numSeries].Label = "Y Code Stroke";
                Series[numSeries].ChartType = SeriesChartType.FastLine;
                Series[numSeries].Color = Color.Blue;
                Series[numSeries].IsVisibleInLegend = true;
                Series[numSeries].Font = new Font("Calibri", 4, FontStyle.Regular);
                Series[numSeries].BorderWidth = 3;

                numSeries++;
                Series.Add("X Current"); //2
                Series[numSeries].Label = "X Current";
                Series[numSeries].ChartType = SeriesChartType.FastLine;
                Series[numSeries].Color = Color.LightPink;
                Series[numSeries].IsVisibleInLegend = true;
                Series[numSeries].YAxisType = AxisType.Secondary;
                Series[numSeries].Font = new Font("Calibri", 4, FontStyle.Regular);
                Series[numSeries].BorderWidth = 3;

                numSeries++;
                Series.Add("Y Current"); //3
                Series[numSeries].Label = "Y Current";
                Series[numSeries].ChartType = SeriesChartType.FastLine;
                Series[numSeries].Color = Color.Turquoise;
                Series[numSeries].IsVisibleInLegend = true;
                Series[numSeries].YAxisType = AxisType.Secondary;
                Series[numSeries].Font = new Font("Calibri", 4, FontStyle.Regular);
                Series[numSeries].BorderWidth = 3;

                numSeries++;
                Series.Add("X Hall"); //4 
                Series[numSeries].Label = "X Hall";
                Series[numSeries].ChartType = SeriesChartType.FastLine;
                Series[numSeries].Color = Color.Bisque;
                Series[numSeries].IsVisibleInLegend = true;
                Series[numSeries].YAxisType = AxisType.Secondary;
                Series[numSeries].Font = new Font("Calibri", 4, FontStyle.Regular);
                Series[numSeries].BorderWidth = 3;

                numSeries++;
                Series.Add("Y Hall"); //5
                Series[numSeries].Label = "Y Hall";
                Series[numSeries].ChartType = SeriesChartType.FastLine;
                Series[numSeries].Color = Color.LightSkyBlue;
                Series[numSeries].IsVisibleInLegend = true;
                Series[numSeries].YAxisType = AxisType.Secondary;
                Series[numSeries].Font = new Font("Calibri", 4, FontStyle.Regular);
                Series[numSeries].BorderWidth = 3;
            }
            else if (type == "Step")
            {
                ChartAreas[0].AxisY.MinorGrid.Interval = .1;
                ChartAreas[0].AxisX.MinorGrid.Enabled = false;
                ChartAreas[0].AxisY.MinorGrid.Enabled = false;
                ChartAreas[0].AxisX.Minimum = 0;
                ChartAreas[0].AxisX.Maximum = 0.3;
                ChartAreas[0].AxisX.Interval = 0.02;
                ChartAreas[0].AxisY.Minimum = 0;
                ChartAreas[0].AxisY.Maximum = 2;
                ChartAreas[0].AxisY.Interval = 0.25;
                ChartAreas[0].AxisX.LabelStyle.Format = "0.00";     // Time
                ChartAreas[0].AxisY.LabelStyle.Format = "0.0";

                Titles.Add("Step Response");

                for (numSeries = 0; numSeries < 5; numSeries++)
                {
                    Series.Add("X" + numSeries.ToString());
                    Series[numSeries].Label = "X" + numSeries.ToString();
                    Series[numSeries].ChartType = SeriesChartType.FastLine;
                    Series[numSeries].Color = GetColorForSeries(numSeries);
                    Series[numSeries].IsVisibleInLegend = true;
                    Series[numSeries].Font = new Font("Calibri", 4, FontStyle.Regular);
                }

                for (; numSeries < 10; numSeries++)
                {
                    Series.Add("Y" + numSeries.ToString());
                    Series[numSeries].Label = "Y" + numSeries.ToString();
                    Series[numSeries].ChartType = SeriesChartType.FastLine;
                    Series[numSeries].IsVisibleInLegend = true;
                    Series[numSeries].Font = new Font("Calibri", 4, FontStyle.Regular);
                }
            }
        }
        private Color GetColorForSeries(int seriesIndex)
        {

            Color[] colors = { Color.Red, Color.Blue, Color.Green, Color.Orange, Color.Purple,
                                   Color.Cyan, Color.Magenta, Color.Yellow, Color.Brown, Color.Gray };
            return colors[seriesIndex % colors.Length];
        }
    }

    public class ActroChartList : ActroChart
    {
        public ActroChartList(string type, int ch) :
            base(type, ch)
        {
            base.MouseDoubleClick += MouseDoubleClick;
        }

        private void MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (Tag.ToString() == "S")
            {
                OldPt.Width = Width;
                OldPt.Height = Height;
                OldPt.X = Left;
                OldPt.Y = Top;
                Width = 953;
                Height = 616;
                Left = 3 + (Ch / 2) * 956;
                Top = 81;
                Title = Titles[0].Text;
                Titles[0].Text = Title + " Ch " + Ch.ToString();
                Titles[0].Font = new Font("Malgun Gothic", 14, FontStyle.Bold); ;
                ChartAreas[0].AxisY.MinorGrid.Enabled = true;
                ChartAreas[0].AxisY.LabelStyle.Format = "0";
                Legends[0].Position = new ElementPosition(5, 0, 20, 9);
                BringToFront();
                Tag = "L";
            }
            else
            {
                Width = OldPt.Width;
                Height = OldPt.Height;
                Left = OldPt.X;
                Top = OldPt.Y;
                SendToBack();
                Titles[0].Text = Title;
                Titles[0].Font = new Font("Malgun Gothic", 9, FontStyle.Bold); ;
                ChartAreas[0].AxisY.MinorGrid.Enabled = false;
                ChartAreas[0].AxisY.LabelStyle.Format = "0";
                Legends[0].Position = new ElementPosition(5, 0, 40, 18);
                Tag = "S";
            }
        }
    }


    public class ChartList
    {
        public Chart C = new Chart();
        public string Title = "";
        public string Type = "";
        public int Ch = 0;
        public bool IsFalg = false;

        public Rectangle OldPt;
        public ChartList(string type, int ch)
        {
            Type = type;
            Ch = ch;

            C.BackColor = Color.Black;

            C.ChartAreas.Add(new ChartArea());
            C.ChartAreas[0].Position.X = 0;
            C.ChartAreas[0].Position.Y = 0;
            C.ChartAreas[0].Position.Height = 99;
            C.ChartAreas[0].Position.Width = 100;
            C.ChartAreas[0].AxisX.LabelStyle.Font = new Font("Calibri", 7, FontStyle.Bold);
            C.ChartAreas[0].AxisY.LabelStyle.Font = new Font("Calibri", 7, FontStyle.Bold);
            C.ChartAreas[0].AxisX.ScaleView.Position = 0;
            C.ChartAreas[0].AxisY.ScaleView.Position = 0;
            C.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.FromArgb(30, 30, 30);             //라이트 그레이
            C.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.FromArgb(30, 30, 30);             //라이트 그레이
            C.ChartAreas[0].AxisX.MinorGrid.LineColor = Color.FromArgb(15, 15, 15);             //화이트스모크
            C.ChartAreas[0].AxisY.MinorGrid.LineColor = Color.FromArgb(15, 15, 15);             //화이트스모크
            C.ChartAreas[0].AxisX.LineColor = Color.Black;                                      //다크 그레이
            C.ChartAreas[0].AxisY.LineColor = Color.Black;                                      //다크 그레이
            C.ChartAreas[0].BackColor = Color.Black;
            C.ChartAreas[0].AxisX.LineDashStyle = ChartDashStyle.Solid;
            C.ChartAreas[0].AxisX.LineColor = Color.FromArgb(30, 30, 30);
            C.ChartAreas[0].AxisY.LineDashStyle = ChartDashStyle.Solid;
            C.ChartAreas[0].AxisY.LineColor = Color.FromArgb(30, 30, 30);
            //C.BackColor = SystemColors.ControlLightLight;                                       //
            C.Legends.Add(new Legend());
            C.Legends[0].Position = new ElementPosition(5, 0, 40, 18);
            C.Legends[0].BackColor = Color.Transparent;
            C.Legends[0].ForeColor = Color.White;
            C.Size = new Size(475, 280);
            C.MouseDoubleClick += new MouseEventHandler(MouseDoubleClick);
            C.Tag = "S";
            int numSeries = 0;
            if (type == "Stroke")
            {
                C.ChartAreas[0].AxisY.MinorGrid.Interval = .1;
                C.ChartAreas[0].AxisX.MinorGrid.Enabled = false;
                C.ChartAreas[0].AxisY.MinorGrid.Enabled = false;
                C.ChartAreas[0].AxisY.Minimum = -4;
                C.ChartAreas[0].AxisY.Maximum = 4;
                C.ChartAreas[0].AxisY.Interval = 1;
                C.ChartAreas[0].AxisY2.LabelStyle.Font = new Font("Calibri", 7, FontStyle.Bold);
                C.ChartAreas[0].AxisX.LabelStyle.Format = "#";
                C.ChartAreas[0].AxisX.IsLabelAutoFit = false;
                C.ChartAreas[0].AxisX.LabelStyle.Angle = 45;
                C.ChartAreas[0].AxisY.LabelStyle.Format = "0.00";
                C.ChartAreas[0].AxisY2.LabelStyle.Format = "0.0";
                C.ChartAreas[0].AxisY.LabelStyle.Format = "0.00";
                C.ChartAreas[0].AxisY2.LabelStyle.Format = "0.0";

                C.ChartAreas[0].AxisX.LabelStyle.ForeColor = Color.White;
                C.ChartAreas[0].AxisY.LabelStyle.ForeColor = Color.White;


                C.Titles.Add("Stroke vs Code");

                C.Series.Add("X Code Stroke"); //0
                C.Series[numSeries].Label = "X Code Stroke";
                C.Series[numSeries].ChartType = SeriesChartType.FastLine;
                C.Series[numSeries].Color = Color.Red;
                C.Series[numSeries].IsVisibleInLegend = true;
                C.Series[numSeries].Font = new Font("Calibri", 4, FontStyle.Regular);
                C.Series[numSeries].BorderWidth = 3;
                numSeries++;
                C.Series.Add("Y Code Stroke"); //1
                C.Series[numSeries].Label = "Y Code Stroke";
                C.Series[numSeries].ChartType = SeriesChartType.FastLine;
                C.Series[numSeries].Color = Color.Blue;
                C.Series[numSeries].IsVisibleInLegend = true;
                C.Series[numSeries].Font = new Font("Calibri", 4, FontStyle.Regular);
                C.Series[numSeries].BorderWidth = 3;

                numSeries++;
                C.Series.Add("X Current"); //2
                C.Series[numSeries].Label = "X Current";
                C.Series[numSeries].ChartType = SeriesChartType.FastLine;
                C.Series[numSeries].Color = Color.LightPink;
                C.Series[numSeries].IsVisibleInLegend = true;
                C.Series[numSeries].YAxisType = AxisType.Secondary;
                C.Series[numSeries].Font = new Font("Calibri", 4, FontStyle.Regular);
                C.Series[numSeries].BorderWidth = 3;

                numSeries++;
                C.Series.Add("Y Current"); //3
                C.Series[numSeries].Label = "Y Current";
                C.Series[numSeries].ChartType = SeriesChartType.FastLine;
                C.Series[numSeries].Color = Color.Turquoise;
                C.Series[numSeries].IsVisibleInLegend = true;
                C.Series[numSeries].YAxisType = AxisType.Secondary;
                C.Series[numSeries].Font = new Font("Calibri", 4, FontStyle.Regular);
                C.Series[numSeries].BorderWidth = 3;

                numSeries++;
                C.Series.Add("X Hall"); //4 
                C.Series[numSeries].Label = "X Hall";
                C.Series[numSeries].ChartType = SeriesChartType.FastLine;
                C.Series[numSeries].Color = Color.Bisque;
                C.Series[numSeries].IsVisibleInLegend = true;
                C.Series[numSeries].YAxisType = AxisType.Secondary;
                C.Series[numSeries].Font = new Font("Calibri", 4, FontStyle.Regular);
                C.Series[numSeries].BorderWidth = 3;

                numSeries++;
                C.Series.Add("Y Hall"); //5
                C.Series[numSeries].Label = "Y Hall";
                C.Series[numSeries].ChartType = SeriesChartType.FastLine;
                C.Series[numSeries].Color = Color.LightSkyBlue;
                C.Series[numSeries].IsVisibleInLegend = true;
                C.Series[numSeries].YAxisType = AxisType.Secondary;
                C.Series[numSeries].Font = new Font("Calibri", 4, FontStyle.Regular);
                C.Series[numSeries].BorderWidth = 3;
            }
            else if (type == "Step")
            {
                C.ChartAreas[0].AxisY.MinorGrid.Interval = .1;
                C.ChartAreas[0].AxisX.MinorGrid.Enabled = false;
                C.ChartAreas[0].AxisY.MinorGrid.Enabled = false;
                C.ChartAreas[0].AxisX.Minimum = 0;
                C.ChartAreas[0].AxisX.Maximum = 0.3;
                C.ChartAreas[0].AxisX.Interval = 0.02;
                C.ChartAreas[0].AxisY.Minimum = 0;
                C.ChartAreas[0].AxisY.Maximum = 2;
                C.ChartAreas[0].AxisY.Interval = 0.25;
                C.ChartAreas[0].AxisX.LabelStyle.Format = "0.00";     // Time
                C.ChartAreas[0].AxisY.LabelStyle.Format = "0.0";

                C.Titles.Add("Step Response");

                for (numSeries = 0; numSeries < 5; numSeries++)
                {
                    C.Series.Add("X" + numSeries.ToString());
                    C.Series[numSeries].Label = "X" + numSeries.ToString();
                    C.Series[numSeries].ChartType = SeriesChartType.FastLine;
                    C.Series[numSeries].Color = GetColorForSeries(numSeries);
                    C.Series[numSeries].IsVisibleInLegend = true;
                    C.Series[numSeries].Font = new Font("Calibri", 4, FontStyle.Regular);
                }

                for (; numSeries < 10; numSeries++)
                {
                    C.Series.Add("Y" + numSeries.ToString());
                    C.Series[numSeries].Label = "Y" + numSeries.ToString();
                    C.Series[numSeries].ChartType = SeriesChartType.FastLine;
                    C.Series[numSeries].IsVisibleInLegend = true;
                    C.Series[numSeries].Font = new Font("Calibri", 4, FontStyle.Regular);
                }
            }
        }

        private Color GetColorForSeries(int seriesIndex)
        {

            Color[] colors = { Color.Red, Color.Blue, Color.Green, Color.Orange, Color.Purple,
                                   Color.Cyan, Color.Magenta, Color.Yellow, Color.Brown, Color.Gray };
            return colors[seriesIndex % colors.Length];
        }

        private void MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (C.Tag.ToString() == "S")
            {
                OldPt.Width = C.Width;
                OldPt.Height = C.Height;
                OldPt.X = C.Left;
                OldPt.Y = C.Top;
                C.Width = 953;
                C.Height = 616;
                C.Left = 3 + (Ch / 2) * 956;
                C.Top = 81;
                Title = C.Titles[0].Text;
                C.Titles[0].Text = Title + " Ch " + Ch.ToString();
                C.Titles[0].Font = new Font("Malgun Gothic", 14, FontStyle.Bold); ;
                C.ChartAreas[0].AxisY.MinorGrid.Enabled = true;
                C.ChartAreas[0].AxisY.LabelStyle.Format = "0";
                C.Legends[0].Position = new ElementPosition(5, 0, 20, 9);
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
                C.Legends[0].Position = new ElementPosition(5, 0, 40, 18);
                C.Tag = "S";
            }
        }
    }
}
