using EAST_AS_CENTER_HUD.Struct;
using MahApps.Metro.Controls;
using PylonC.NET;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static PylonC.NETSupportLibrary.DeviceEnumerator;

namespace EAST_AS_CENTER_HUD.Camera
{
    /// <summary>
    /// Window_CameraSetting.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Window_CameraConnectSetting : MetroWindow, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private ICommand updateCameraInformation;
        public ICommand UpdateCameraInformationCommand
        {
            get { return (this.updateCameraInformation) ?? (this.updateCameraInformation = new DelegateCommand(UpdateCameraInformation)); }
        }


        public ObservableCollection<StructNetworkInterface> ListNetworkInterface { get { return listNetworkInterface; } set { listNetworkInterface = value; NotifyPropertyChanged("ListNetworkInterface"); NotifyPropertyChanged("ListCameraInterface"); } }
        private ObservableCollection<StructNetworkInterface> listNetworkInterface;

        public ObservableCollection<StructCameraInterface> ListCameraInterface { get { return listCameraInterface; }  set { listCameraInterface = value; NotifyPropertyChanged("ListNetworkInterface"); NotifyPropertyChanged("ListCameraInterface"); } }
        private ObservableCollection<StructCameraInterface> listCameraInterface;

        public StructNetworkInterface SelectedNetworkInterface { get; set; }
        public StructCameraInterface SelectedCameraInterface { get; set; }
        public object SelectedTreeViewItem { get; set; }

        public Window_CameraConnectSetting()
        {
            InitializeComponent();
            this.DataContext = this;

            InitNetworkInterface();
            InitCameraInterface();
        }

        private void InitNetworkInterface()
        {
            ListNetworkInterface = new ObservableCollection<StructNetworkInterface>(CameraManager.GetNetworkInterface());
        }

        private void InitCameraInterface()
        {
            ListCameraInterface = new ObservableCollection<StructCameraInterface>(CameraManager.GetCameraInterfaces(ListNetworkInterface.ToList()));

            SelectedNetworkInterface = null;
            SelectedCameraInterface = null;

            this.DataContext = this;
        }

        private void TreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            gd_Content.Children.Clear();
            if(e == null || e.NewValue == null)
            {
                return;
            }
            if(e.NewValue.GetType() == typeof(StructNetworkInterface))
            {
                SelectedNetworkInterface = (StructNetworkInterface)e.NewValue;
                gd_Content.Children.Add(new Control_NetworkInterface());
            }
            else if(e.NewValue.GetType() == typeof(StructCameraInterface))
            {
                StructCameraInterface structCameraInterface = (StructCameraInterface)e.NewValue;

                SelectedCameraInterface = structCameraInterface;
                SelectedCameraInterface.SavedIpAddress = SelectedCameraInterface.IpAddress;
                SelectedCameraInterface.SavedSubnetMask= SelectedCameraInterface.SubnetMask;
                gd_Content.Children.Add(new Control_CameraInterface());
            }
        }

        public void UpdateCameraInformation()
        {
            string macAddress = SelectedCameraInterface.Mac;
            string ipAddress = SelectedCameraInterface.SavedIpAddress;
            string subnetMask = SelectedCameraInterface.SubnetMask;
            string gateWay = GetGateway(subnetMask, ipAddress);

            Pylon.GigEForceIp(macAddress, ipAddress, subnetMask, gateWay);

            InitNetworkInterface();
            InitCameraInterface();
        }


        private string GetGateway(string subnetMask, string IpAddress)
        {
            string[] octets = subnetMask.Split('.');
            string[] gateWayOctets = IpAddress.Split('.');
            string gateway = "";

            for (int i = 0; i < octets.Length; i++)
            {
                if (octets[i] == "0")
                {
                    gateWayOctets[i] = "0";
                }
            }

            gateWayOctets.ToList().ForEach(x =>
            {
                if (!string.IsNullOrEmpty(gateway))
                {
                    gateway += ".";
                }
                gateway += x;
            });

            return gateway;
        }
    }
}
