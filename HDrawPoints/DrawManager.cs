using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDrawPoints
{
    public class DrawManager
    {
        public ObservableCollection<DrawPoint> DrawPoints { get; private set; }
        public ObservableCollection<DrawLabel> DrawLabels { get; private set; }
        public ObservableCollection<DrawLine> DrawLines { get; private set; }
        public ObservableCollection<DrawCross> DrawCross { get; private set; }
        public ObservableCollection<DrawRectangle> DrawRectangle { get; private set; }

        public DrawManager()
        {
            DrawPoints = new ObservableCollection<DrawPoint>();
            DrawLabels = new ObservableCollection<DrawLabel>();
            DrawLines = new ObservableCollection<DrawLine>();
            DrawCross = new ObservableCollection<DrawCross>();
            DrawRectangle = new ObservableCollection<DrawRectangle>();
        }
    }
}
