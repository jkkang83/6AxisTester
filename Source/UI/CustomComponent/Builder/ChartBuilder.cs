using FZ4P.UI.CustomComponenet.Builder.Params;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Forms.VisualStyles;

namespace FZ4P.UI.CustomComponenet.Builder
{
    public class ChartBuilder
    {
        public ChartBuilder CraeteChartAreas(Chart buildChart, ChartAreasParams parmas)
        {
            var area = new ChartArea()
            {
                BackColor = parmas.BackColor,
                Position = parmas.Position,
                AxisX = parmas.AxisX,
                AxisY= parmas.AxisY,
            };

            buildChart.ChartAreas.Add(area);
            return this;
        }
        public ChartBuilder CreateSerrise(Chart buildChart, SeriesParams parmas)
        {
            //Series.Add("X Code Stroke"); //0
            //Series[numSeries].Label = "X Code Stroke";
            //Series[numSeries].ChartType = SeriesChartType.FastLine;
            //Series[numSeries].Color = Color.Red;
            //Series[numSeries].IsVisibleInLegend = true;
            //Series[numSeries].Font = new Font("Calibri", 4, FontStyle.Regular);
            //Series[numSeries].BorderWidth = 3;

            var serries = new Series()
            {
               Color = parmas.SeriesColor,
               Label= parmas.LabelName,
               BorderWidth = parmas.BorderWidth,
               ChartType = parmas.ChartType,
               Font = parmas.Font,
               IsVisibleInLegend = parmas.IsVisibleInLegend,
            };
            buildChart.Series.Add(serries);

            return this;
        }

        public ChartBuilder CreateLegend(Chart buildChart, LegendParams parmas)
        {
            var legend = new Legend()
            {
               Position = parmas.Position,
               BackColor = parmas.BackColor,
               ForeColor = parmas.ForeColor,
            };
            buildChart.Series.Add(legend);

            return this;
        }
    }
}
