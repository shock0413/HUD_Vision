using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace HCore.HDrawPoints
{
    public class DrawRectangle
    {
        public double CenterX { get; set; }
        public double CenterY{ get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public string ToolTip { get; set; }
        public double Size { get; set; }
        public Brush StrokeColor { get; set; }
        public SolidColorBrush Fill { get; set; }
    }
}
