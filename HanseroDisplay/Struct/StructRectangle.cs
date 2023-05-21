using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace HanseroDisplay.Struct
{
    public class StructRectangle
    {
        public double Width { get; set; }
        public double Height { get; set; }
        public double CenterX { get; set; }
        public double CenterY { get; set; }

        public SolidColorBrush Brush { get; set; }
        public Pen Pen { get; set; }
        public string Tag { get; set; }
    }
}
