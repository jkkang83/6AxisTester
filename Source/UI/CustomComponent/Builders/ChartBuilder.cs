using FZ4P.UI.CustomComponenet.Builder.Params;
using OpenCvSharp.Internal.Vectors;
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
        private readonly Chart _chart;

        private ChartAreasParams _areaParams;
        private List<SeriesParams> _seriesParams = new List<SeriesParams>();
        private LegendParams _legendParams;

        public ChartBuilder(Chart chart)
        {
            _chart = chart;
        }

        public ChartBuilder SetChartArea(ChartAreasParams p)
        {
            _areaParams = p;
            return this;
        }

        public ChartBuilder AddSeries(SeriesParams p)
        {
            _seriesParams.Add(p);
            return this;
        }

        public ChartBuilder SetLegend(LegendParams p)
        {
            _legendParams = p;
            return this;
        }

        public Chart Build()
        {
            ChartClear();
            CraeteChartAreas();
            foreach (var param in _seriesParams)
            {
                CreateSerise(param);
            }
            CreateLegend();
            return _chart;
        }

        private void ChartClear()
        {
            _chart.ChartAreas.Clear();
            _chart.Series.Clear();
            _chart.Legends.Clear();
        }

        private void CraeteChartAreas()
        {
            var area = new ChartArea()
            {
                BackColor = _areaParams.BackColor,
                Position = _areaParams.Position,
                AxisX = _areaParams.AxisX,
                AxisY= _areaParams.AxisY,
                AxisY2= _areaParams.AxisY2,
            };

            _chart.ChartAreas.Add(area);
        }
        private void CreateSerise(SeriesParams seriesParam)
        {
            var serries = new Series()
            {
               Color = seriesParam.SeriesColor,
               Label= seriesParam.LabelName,
               BorderWidth = seriesParam.BorderWidth,
               ChartType = seriesParam.ChartType,
               Font = seriesParam.Font,
               IsVisibleInLegend = seriesParam.IsVisibleInLegend,
            };
            _chart.Series.Add(serries);
        }
        private void CreateLegend()
        {
            var legend = new Legend()
            {
               Position = _legendParams.Position,
               BackColor = _legendParams.BackColor,
               ForeColor = _legendParams.ForeColor,
            };
            _chart.Legends.Add(legend);
        }
    }
}
