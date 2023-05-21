using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace HDrawPoints
{
    public class DrawLine
    {
        public double StartX { get; set; }
        public double StartY { get; set; }
        public double EndX { get; set; }
        public double EndY { get; set; }
        public string ToolTip { get; set; }
        public double Size { get; set; }
        public Brush StrokeColor { get; set; }
        public Brush Fill { get; set; }
    }
}
