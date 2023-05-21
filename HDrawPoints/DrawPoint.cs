﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace HDrawPoints
{
    public class DrawPoint
    {
        public double X { get; set; }
        public double Y { get; set; }
        public string ToolTip { get; set; }
        public double Size { get; set; }
        public Brush StrokeColor { get; set; }
        public Brush Fill { get; set; }
    }
}
