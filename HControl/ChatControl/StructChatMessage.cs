using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace HControl.ChatControl
{
    public class StructChatMessage
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public SolidColorBrush Background { get; set; }
        public SolidColorBrush Foreground { get; set; }
        public int TitleFontSize { get { return titleFontSize; } set { titleFontSize = value; } }
        private int titleFontSize = 10;
        public int MessageFontSize { get { return messageFontSize; } set { messageFontSize = value; } }
        private int messageFontSize = 10;
        public SolidColorBrush BorderColor { get; set; }
        public bool ShowTitle { get; set; }
        public DateTime Time;

        public HorizontalAlignment Align { get; set; }
    }
}
