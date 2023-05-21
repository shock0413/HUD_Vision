using AsyncSocket;
using EAST_AS_CENTER_HUD.Camera;
using HResult;
using HTool;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EAST_AS_CENTER_HUD
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리Z
    public partial class MainWindow : MetroWindow
    {
        InspectionEngine inspectionEngine;

        public MainWindow()
        {
            InitContext();
            InitializeComponent();
        }

        private void InitContext()
        {
             inspectionEngine = new InspectionEngine(this);
            this.DataContext = inspectionEngine;
            inspectionEngine.Window = this;
        }
    }
}
