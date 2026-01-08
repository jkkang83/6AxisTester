using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace FZ4P.UI.CustomComponenet.Builder.Params
{
    public class SeriesParams
    {
        public Color SeriesColor { get; set; }
        public string LabelName { get; set; }
        public int BorderWidth { get; set; }
        public Font Font { get; set; }
        public bool IsVisibleInLegend { get; set; }
        public SeriesChartType ChartType { get; set; }  
    }
}
