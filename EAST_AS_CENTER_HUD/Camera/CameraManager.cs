using EAST_AS_CENTER_HUD.Struct;
using PylonC.NET;
using PylonC.NETSupportLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Utill;
using static PylonC.NETSupportLibrary.DeviceEnumerator;

namespace EAST_AS_CENTER_HUD.Camera
{
    public class CameraManager
    {
        List<Camera> cameras;
        CameraCore cameraCore;

        public CameraManager(CameraCore cameraCore)
        {
            this.cameraCore = cameraCore;

            UpdateDeviceList();
        }

        public static List<DeviceEnumerator.Device> GetAllDevices()
        {
            return DeviceEnumerator.EnumerateAllDevices();
        }

        public static List<DeviceEnumerator.Device> GetConnectedDevices()
        {
            return DeviceEnumerator.EnumerateDevices();
        }

        public static List<StructNetworkInterface> GetNetworkInterface()
        {
            NetworkInterface[] networks = NetworkInterface.GetAllNetworkInterfaces();

            List<StructNetworkInterface> result = new List<StructNetworkInterface>();

            networks.ToList().ForEach(x =>
            {
                if (x.NetworkInterfaceType == NetworkInterfaceType.Ethernet && x.OperationalStatus == OperationalStatus.Up)
                {
                    result.Add(new StructNetworkInterface(x));
                }
            });

            return result;
        }

        public static List<StructCameraInterface> GetCameraInterfaces(List<StructNetworkInterface> listNetworkInterface)
        {
            List<Device> listDevice = GetAllDevices();

            List<StructCameraInterface> cameraInterfaces = new List<StructCameraInterface>();

            listNetworkInterface.ForEach(n => {
                n.ListCameraInterface.Clear();
                List<Device> findDevice = listDevice.Where(x => x.Interface == n.IpAddress).ToList();
                if (findDevice != null)
                {
                    findDevice.ForEach(x =>
                    {
                        StructCameraInterface structCameraInterface = new StructCameraInterface(x, n);
                        n.ListCameraInterface.Add(structCameraInterface);
                        cameraInterfaces.Add(structCameraInterface);
                    });
                }
            });

            return cameraInterfaces;
        }

        public static void SetCameraIP(string macAddr, string ipAddress, string subnetMask, string defaultGateway)
        {
            Pylon.GigEForceIp(macAddr, ipAddress, subnetMask, defaultGateway);
        }

        void UpdateDeviceList()
        {
            cameras = new List<Camera>();

            for (int i = 0; i < 5; i++)
            {
                try
                {
                    List<DeviceEnumerator.Device> list = DeviceEnumerator.EnumerateDevices();

                    list.ForEach(x =>
                    {
                        cameras.Add(new Camera(x, cameraCore.ImageProvider));
                    });

                    break;
                }
                catch (Exception e)
                {
                    LogManager.Write("카메라 목록 불러오기 실패 : " + (i+1).ToString() + " " + e.Message);
                }
            }
        }


        public BitmapImage OneShot(StructCamera structCamera)
        {

            try
            {
                LogManager.Write("이미지 촬영");
                UpdateDeviceList();

                LogManager.Write("카메라 SN : " + structCamera.SerialNum);
                LogManager.Write("카메라 Gain : " + structCamera.Gain);
                LogManager.Write("카메라 Exposure : " + structCamera.Exposure);
                Camera camera = cameras.Where(x => x.SerialNum == structCamera.SerialNum).ToArray()[0];
                camera.OneShot(structCamera.Gain, structCamera.Exposure);

                LogManager.Write("이미지 촬영 성공");
                return camera.BitmapImage;
            }
            catch (Exception e)
            {
                LogManager.Write("이미지 획득 실패 : " + e.Message);
                return null;
            }
        }

        public BitmapImage OneShot(StructCamera structCamera, bool isRotate)
        {

            try
            {
                LogManager.Write("이미지 촬영");
                UpdateDeviceList();

                LogManager.Write("카메라 SN : " + structCamera.SerialNum);
                LogManager.Write("카메라 Gain : " + structCamera.Gain);
                LogManager.Write("카메라 Exposure : " + structCamera.Exposure);
                Camera camera = cameras.Where(x => x.SerialNum == structCamera.SerialNum).ToArray()[0];
                camera.OneShot(structCamera.Gain, structCamera.Exposure, isRotate);

                LogManager.Write("이미지 촬영 성공");
                return camera.BitmapImage;
            }
            catch(Exception e)
            {
                LogManager.Write("이미지 획득 실패 : " + e.Message);
                return null;
            }
        }

        Camera GetCamera(string serialNum)
        {
            List<Camera> findCameraList = cameras.Where(x => x.SerialNum == serialNum).ToList();
            if (findCameraList.Count == 0)
            {
                return null;
            }
            else
            {
                return findCameraList[0];
            }
        }

        public int GetGainMin(StructCamera structCamera)
        {
            Camera camera = GetCamera(structCamera.SerialNum);
            if(camera != null)
            {
                return camera.GainMin;
            }
            else
            {
                return 0;
            }
        }

        public int GetGainMax(StructCamera structCamera)
        {
            Camera camera = GetCamera(structCamera.SerialNum);
            if (camera != null)
            {
                return camera.GainMax;
            }
            else
            {
                return 0;
            }
        }

        public int GetGainInterval(StructCamera structCamera)
        {
            Camera camera = GetCamera(structCamera.SerialNum);
            if (camera != null)
            {
                return camera.GainInterval;
            }
            else
            {
                return 1;
            }
        }


        public int GetExposureMin(StructCamera structCamera)
        {
            Camera camera = GetCamera(structCamera.SerialNum);
            if (camera != null)
            {
                return camera.ExposureMin;
            }
            else
            {
                return 0;
            }
        }

        public int GetExposureMax(StructCamera structCamera)
        {
            Camera camera = GetCamera(structCamera.SerialNum);
            if (camera != null)
            {
                return camera.ExposureMax;
            }
            else
            {
                return 0;
            }
        }

        public int GetExposureInterval(StructCamera structCamera)
        {
            Camera camera = GetCamera(structCamera.SerialNum);
            if (camera != null)
            {
                return camera.ExposureInterval;
            }
            else
            {
                return 0;
            }
        }

        public BitmapImage SnapShot(StructCamera structCamera, bool isRoate)
        {
            try
            {
                Camera camera = cameras.Where(x => x.SerialNum == structCamera.SerialNum).ToArray()[0];

                camera.OneShot(isRoate, false);
                return camera.BitmapImage;
            }
            catch (Exception e)
            {
                LogManager.Write("이미지 스냅샷 실패 : " + e.Message);
                return null;
            }
        }

        public void StopSnapShot(StructCamera structCamera)
        {
            try
            {
                Camera camera = cameras.Where(x => x.SerialNum == structCamera.SerialNum).ToArray()[0];
                camera.Close();
            }
            catch (Exception e)
            {
                LogManager.Write("이미지 스냅샷 종료 실패 : " + e.Message);
                
            }
        }
    }
}
