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

namespace EAST_AS_CENTER_HUD.Setting
{
    /// <summary>
    /// Window_Setting.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Window_Setting : MetroWindow
    {
        public Window_Setting()
        {
            InitializeComponent();
        }

        private void HDisplay_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ((MainEngine)this.DataContext).SettingEngine.CopyMasterImageToMainDisplay();
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ((MainEngine)this.DataContext).SettingEngine.DialogCoordinator = MahApps.Metro.Controls.Dialogs.DialogCoordinator.Instance;
        }
    }
}
