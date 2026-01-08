using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace FZ4P.UI.CustomComponenet.Builder.Params
{
    public class ChartAreasParams
    {
        public Color BackColor { get; set; }
        public ElementPosition Position { get; set; }

        public Axis AxisX { get; set; }
        public Axis AxisY { get; set; }
        public Axis AxisY2 { get; set; }
    }
}
