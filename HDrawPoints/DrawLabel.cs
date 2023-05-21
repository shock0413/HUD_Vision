using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace HDrawPoints
{
    public class DrawLabel
    {
        public double X { get; set; }
        public double Y { get; set; }
        public string ToolTip { get; set; }
        public double Size { get; set; }
        public Brush Foreground { get; set; }
        public Brush Background { get; set; }
        public string Text { get; set; }
    }
}
