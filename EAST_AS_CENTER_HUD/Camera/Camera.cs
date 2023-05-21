using OpenCvSharp;
using OpenCvSharp.CPlusPlus;
using OpenCvSharp.Extensions;
using PylonC.NET;
using PylonC.NETSupportLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Utill;

namespace EAST_AS_CENTER_HUD.Camera
{
    public class Camera
    {
        ImageProvider imageProvider = new ImageProvider();
        private DeviceEnumerator.Device device;

        public BitmapImage BitmapImage { get { return bitmapImage; } }
        private BitmapImage bitmapImage = null;

        ImageProvider.Image lastGetImage;

        private bool shotFinish = false;

        public string SerialNum { get { return device.SerialNum; } }

        public int GainMin { get { return GetGainMin(); } }
        public int GainMax { get { return GetGainMax(); } }
        public int GainInterval { get { return GetGainInterval(); } }

        public int ExposureMin { get { return GetExposureMin(); } }
        public int ExposureMax { get { return GetExposureMax(); } }
        public int ExposureInterval { get { return GetExposureInterval(); } }

        public Camera(DeviceEnumerator.Device device, ImageProvider imageProvider)
        {
            this.device = device;
            InitImageProviderEvent();
        }

        private void InitImageProviderEvent()
        {
            imageProvider.ImageReadyEvent += new ImageProvider.ImageReadyEventHandler(OnImageReadyEventCallback);
            imageProvider.GrabErrorEvent += new ImageProvider.GrabErrorEventHandler(OnGrabErrorEventCallback);
            imageProvider.DeviceOpenedEvent += new ImageProvider.DeviceOpenedEventHandler(OnDeviceOpenedEventCallback);
        }

        private void OnImageReadyEventCallback()
        {
            try
            {
                lastGetImage = imageProvider.GetLatestImage();
                shotFinish = true;
            }
            catch(Exception e)
            {
                LogManager.Write("이미지 획득 실패 : " + e.Message);
            }
        }

        private void OnGrabErrorEventCallback(Exception grabException, string additionalErrorMessage)
        {

        }


        private void OnDeviceOpenedEventCallback()
        {

        }

        public void Open()
        {
            if (imageProvider.IsOpen == false)
            {
                imageProvider.Open(device.Index);
            }
        }

        public void Close()
        {
            imageProvider.Close();
        }

        private void SetGain(int bright)
        {
            NODE_HANDLE hNode = imageProvider.GetNodeFromDevice("GainRaw");
            int min = (int)GenApi.IntegerGetMin(hNode);
            int max = (int)GenApi.IntegerGetMax(hNode);

            if (min > bright)
            {
                bright = min;
            }

            if (max < bright)
            {
                bright = max;
            }

            GenApi.IntegerSetValue(hNode, bright);
        }

        private int GetGainMin()
        {
            Open();
            NODE_HANDLE hNode = imageProvider.GetNodeFromDevice("GainRaw");
            int min = (int)GenApi.IntegerGetMin(hNode);
            Close();
            return min;
        }


        private int GetGainMax()
        {
            Open();
            NODE_HANDLE hNode = imageProvider.GetNodeFromDevice("GainRaw");

            int max = (int)GenApi.IntegerGetMax(hNode);
            Close();
            return max;
        }

        private int GetGainInterval()
        {
            Open();
            NODE_HANDLE hNode = imageProvider.GetNodeFromDevice("GainRaw");

            int inc = (int)GenApi.IntegerGetInc(hNode);
            Close();

            return inc;
        }


        private void SetExposure(int exposure)
        {
            NODE_HANDLE hNode = imageProvider.GetNodeFromDevice("ExposureTimeRaw");
            GenApi.IntegerSetValue(hNode, exposure);
        }

        private int GetExposureMin()
        {
            Open();
            NODE_HANDLE hNode = imageProvider.GetNodeFromDevice("ExposureTimeRaw");
            int min = (int)GenApi.IntegerGetMin(hNode);
            Close();
            return min;
        }


        private int GetExposureMax()
        {
            Open();
            NODE_HANDLE hNode = imageProvider.GetNodeFromDevice("ExposureTimeRaw");

            int max = (int)GenApi.IntegerGetMax(hNode);
            Close();
            return max;
        }

        private int GetExposureInterval()
        {
            Open();
            NODE_HANDLE hNode = imageProvider.GetNodeFromDevice("ExposureTimeRaw");

            int inc = (int)GenApi.IntegerGetInc(hNode);
            Close();

            return inc;
        }


        public BitmapImage OneShot(int bright, int exposure, bool isRotate)
        {
            GC.Collect();
            shotFinish = false;
            bitmapImage = null;
            bitmapImage = new BitmapImage();

            for (int j = 0; j < 5; j++)
            {
                try
                {
                    Open();

                    SetGain(bright);
                    SetExposure(exposure);

                    imageProvider.OneShot();

                    for (int i = 0; i < 100 && !shotFinish; i++)
                    {
                        Thread.Sleep(5);
                    }

                    Close();

                    Bitmap bitmap;

                    BitmapFactory.CreateBitmap(out bitmap, lastGetImage.Width, lastGetImage.Height, lastGetImage.Color);
                    BitmapFactory.UpdateBitmap(bitmap, lastGetImage.Buffer, lastGetImage.Width, lastGetImage.Height, lastGetImage.Color);

                    if (isRotate)
                    {
                        bitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
                    }

                    bitmapImage = (BitmapImage)ToWpfBitmap(bitmap);

                    break;
                }
                catch (Exception e)
                {
                    LogManager.Write("이미지 획득 실패 : " + e.Message);
                    Close();
                }

                Thread.Sleep(400);

                LogManager.Write("카메라 재접속 시도");
            }

            if (shotFinish)
            {
                return bitmapImage;
            }
            else
            {
                return null;
            }
        }


        public BitmapImage OneShot(bool isRotate, bool isClose)
        {
            try
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();

                shotFinish = false;

                
                    Open();
               

                bitmapImage = new BitmapImage();
                imageProvider.OneShot();

                for (int i = 0; i < 100 && !shotFinish; i++)
                {
                    Thread.Sleep(5);
                }

                if (isClose)
                {
                    Close();
                }

                sw.Stop();
                Console.WriteLine("연속 촬영 소요 시간" + sw.ElapsedMilliseconds);

                Bitmap bitmap;

                BitmapFactory.CreateBitmap(out bitmap, lastGetImage.Width, lastGetImage.Height, lastGetImage.Color);
                BitmapFactory.UpdateBitmap(bitmap, lastGetImage.Buffer, lastGetImage.Width, lastGetImage.Height, lastGetImage.Color);
                if (isRotate)
                {
                    bitmap.RotateFlip(RotateFlipType.Rotate180FlipNone);
                }

                bitmapImage = (BitmapImage)ToWpfBitmap(bitmap);

                if (shotFinish)
                {
                    return bitmapImage;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                LogManager.Write("이미지 획득 실패 : " + e.Message);
                Close();
            }

            return null;
        }


        public BitmapImage OneShot(int bright, int exposure)
        {
            try
            {
                shotFinish = false;

                for (int j = 0; j < 5; j++)
                {
                    Open();

                    SetGain(bright);
                    SetExposure(exposure);

                    bitmapImage = new BitmapImage();
                    imageProvider.OneShot();

                    for (int i = 0; i < 400 && !shotFinish; i++)
                    {
                        Thread.Sleep(5);
                    }

                    Close();

                    if(lastGetImage != null)
                    {
                        break;
                    }
                }

                Bitmap bitmap;

                BitmapFactory.CreateBitmap(out bitmap, lastGetImage.Width, lastGetImage.Height, lastGetImage.Color);
                BitmapFactory.UpdateBitmap(bitmap, lastGetImage.Buffer, lastGetImage.Width, lastGetImage.Height, lastGetImage.Color);

                bitmapImage = (BitmapImage)ToWpfBitmap(bitmap);

                if (shotFinish)
                {
                    return bitmapImage;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                LogManager.Write("이미지 획득 실패 : " + e.Message);
                Close();
            }

            return null;
        }

        private static Mat createAGrayScaleClone(Mat src)
        {
            var srcCopy = new Mat();


            src.CopyTo(srcCopy);
            Cv2.CvtColor(srcCopy, srcCopy, ColorConversion.BgraToGray);


            return srcCopy;
        }


        public BitmapSource ToWpfBitmap(Bitmap bitmap)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Bmp);

                stream.Position = 0;
                BitmapImage result = new BitmapImage();
                result.BeginInit();
                result.CacheOption = BitmapCacheOption.OnLoad;
                result.StreamSource = stream;
                result.EndInit();
                result.Freeze();
                return result;
            }
        }

        public void ContiniusShot()
        {
            imageProvider.ContinuousShot();
        }
    }
}
