using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EAST_AS_CENTER_HUD.Camera
{
    /// <summary>
    /// Window_CameraSetting.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Window_CameraSetting : MetroWindow
    {
        private CameraSettingEngine engine;

        public Window_CameraSetting()
        {
            InitializeComponent();
        }

        private void MetroWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            engine = ((MainEngine)e.NewValue).CameraSettingEngine;
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            engine.StopContiniusShot();
        }
         
    }
}
