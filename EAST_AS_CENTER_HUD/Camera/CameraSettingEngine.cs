using EAST_AS_CENTER_HUD.Struct;
using HanseroDisplay.Struct;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using HTool;
using HCore.HDrawPoints;
using HCore;
using static HCore.HResult;
using Utill;
using System.Diagnostics;

namespace EAST_AS_CENTER_HUD.Camera
{
    public class CameraSettingEngine : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void NotifyPropertyChanged(String propertyName = "")
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        Window window;
        public CameraManager cameraManager;
        private bool isRunningContinuous = false;

        #region Properties
        public BitmapImage Image { get { return image; } set { image = value; NotifyPropertyChanged("Image"); } }
        private BitmapImage image;

        public Result Result { get { return result; } set { result = value; NotifyPropertyChanged("Result"); } }
        public Result result = new Result();

        public ObservableCollection<StructCarkind> ListCarkind { get { return listCarkind; } set { listCarkind = value; NotifyPropertyChanged("ListCarkind"); } }
        private ObservableCollection<StructCarkind> listCarkind = new ObservableCollection<StructCarkind>();

        public StructCarkind SelectedCarkind
        {
            get
            {
                return selectedCarkind;
            }
            set
            {
                selectedCarkind = value;
                NotifyPropertyChanged("SelectedCarkind");
                GetInspection();
            }
        }
        private StructCarkind selectedCarkind = null;

        public ObservableCollection<StructInspection> ListInspection { get { return listInspection; } set { listInspection = value; NotifyPropertyChanged("ListInspection"); } }
        private ObservableCollection<StructInspection> listInspection = new ObservableCollection<StructInspection>();

        public StructInspection SelectedInspection
        {
            get
            {
                return selectedInspection;
            }
            set
            {

                NotifyPropertyChanged("SelectedInspection");

                if (value != null)
                {
                    selectedInspection = value;
                    NotifyPropertyChanged("SelectedCamera");

                    GainMax = cameraManager.GetGainMax(selectedInspection.StructCamera);
                    GainMin = cameraManager.GetGainMin(selectedInspection.StructCamera);
                    GainInterval = cameraManager.GetGainInterval(selectedInspection.StructCamera);

                    ExposureMax = cameraManager.GetExposureMax(selectedInspection.StructCamera);
                    ExposureMin = cameraManager.GetExposureMin(selectedInspection.StructCamera);
                    ExposureInterval = cameraManager.GetExposureInterval(selectedInspection.StructCamera);

                    IsRotateCamera = SelectedCarkind.IsRotateCamera;
                }
                NotifyPropertyChanged("IsShowCameraGroupBox");
            }
        }
        private StructInspection selectedInspection = null;

        public StructCamera SelectedCamera
        {
            get
            {
                if (SelectedInspection == null)
                {
                    return new StructCamera("", 0, 0);
                }
                else
                {
                    return selectedInspection.StructCamera;
                }
            }
        }

        public int GainMax { get { return gainMax; } set { gainMax = value; NotifyPropertyChanged("GainMax"); } }
        private int gainMax = 0;

        public int GainMin { get { return gainMin; } set { gainMin = value; NotifyPropertyChanged("GainMin"); } }
        private int gainMin = 0;

        public int GainInterval { get { return gainInterval; } set { gainInterval = value; NotifyPropertyChanged("GainInterval"); } }
        private int gainInterval = 1;

        public int ExposureMax { get { return exposureMax; } set { exposureMax = value; NotifyPropertyChanged("ExposureMax"); } }
        private int exposureMax = 0;

        public int ExposureMin { get { return exposureMin; } set { exposureMin = value; NotifyPropertyChanged("ExposureMin"); } }
        private int exposureMin = 0;

        public int ExposureInterval { get { return exposureInterval; } set { exposureInterval = value; NotifyPropertyChanged("ExposureInterval"); } }
        private int exposureInterval = 1;

        private bool isShowCameraGroupBox = true;
        public bool IsShowCameraGroupBox
        {
            get
            {
                return (SelectedInspection != null && isShowCameraGroupBox);
            }

            set
            {
                isShowCameraGroupBox = value;
                NotifyPropertyChanged("IsShowCameraGroupBox");
            }
        }

        private bool isRoateCamera = false;
        public bool IsRotateCamera { get { return isRoateCamera; } set { isRoateCamera = value; NotifyPropertyChanged("IsRotateCamera"); } }

        private bool runContiniusShot = false;

        public double Calib_mmPerPixel { get { return calib_mmPerPixel; } set { calib_mmPerPixel = value; NotifyPropertyChanged("Calib_mmPerPixel"); } }
        private double calib_mmPerPixel;

        public ObservableCollection<StructRectangle> DisplayRectangles
        {
            get { return displayRectangles; }
            set { displayRectangles = value; NotifyPropertyChanged("DisplayRectangles"); }
        }
        private ObservableCollection<StructRectangle> displayRectangles = new ObservableCollection<StructRectangle>();


        public ObservableCollection<StructLine> DisplayLines
        {
            get { return displayLines; }
            set { displayLines = value; NotifyPropertyChanged("DisplayLines"); }
        }
        private ObservableCollection<StructLine> displayLines = new ObservableCollection<StructLine>();

        private bool isFitDisplay = true;
        public bool IsFitDisplay { get{ return isFitDisplay; } set { isFitDisplay = value; NotifyPropertyChanged("IsFitDisplay"); } }

        private int continuousGuideRectangleStrokeSize = 50;
        public int ContinuousGuideRectangleStrokeSize { get { return continuousGuideRectangleStrokeSize; } set { continuousGuideRectangleStrokeSize = value; NotifyPropertyChanged("ContinuousGuideRectangleStrokeSize"); } }
        #endregion

        //Command
        #region Command
        private ICommand oneShot;
        public ICommand OneShotCommand
        {
            get { return (this.oneShot) ?? (this.oneShot = new DelegateCommand(OneShot)); }
        }

        private ICommand continiusShot;
        public ICommand ContiniusShotCommand
        {
            get { return (this.continiusShot) ?? (this.continiusShot = new DelegateCommand(ContiniusShot)); }
        }

        private ICommand stopContiniusShot;
        public ICommand StopContiniusShotCommand
        {
            get { return (this.stopContiniusShot) ?? (this.stopContiniusShot = new DelegateCommand(StopContiniusShot)); }
        }

        private ICommand saveCameraParams;
        public ICommand SaveCameraParamsCommand
        {
            get { return (this.saveCameraParams) ?? (this.saveCameraParams = new DelegateCommand(SaveCameraParams)); }
        }

        private ICommand saveCalibrationParams;
        public ICommand SaveCalibrationParamsCommand
        {
            get { return (this.saveCalibrationParams) ?? (this.saveCalibrationParams = new DelegateCommand(SaveCalibrationParams)); }
        }

        private ICommand saveImage;
        public ICommand SaveImageCommand
        {
            get { return (this.saveImage) ?? (this.saveImage = new DelegateCommand(SaveImage)); }
        }

        #endregion
        //Constructor
        public CameraSettingEngine(Window window, CameraCore cameraCore)
        {
            this.window = window;
            window.Closing += Window_Closing;
            this.cameraManager = new CameraManager(cameraCore);
            GetCarkind();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            StopContiniusShot();
        }

        //Method
        public void GetCarkind()
        {
            ListCarkind = new ObservableCollection<StructCarkind>(StructCarkind.GetCarkind());
        }

        public void GetInspection()
        {
            ListInspection = new ObservableCollection<StructInspection>(SelectedCarkind.Inspections.Cast<StructInspection>());
        }

        public void DrawCalibrationRectangle()
        {
            if (Image != null)
            {
                try
                {
                    DrawRectangle rectangle = new DrawRectangle();

                    int centerX = (int)Image.PixelWidth / 2;
                    int centerY = (int)image.PixelHeight / 2;

                    rectangle.CenterX = centerX;
                    rectangle.CenterY = centerY;
                    rectangle.StrokeColor = Brushes.Orange;
                    rectangle.Size = ContinuousGuideRectangleStrokeSize * 1.0 / 10;


                    rectangle.Height = SelectedCarkind.HudHeight / SelectedCarkind.MMPerPixel;
                    rectangle.Width = SelectedCarkind.HudWidth / SelectedCarkind.MMPerPixel;

                    DrawLine verticalLine = new DrawLine();
                    verticalLine.StrokeColor = Brushes.Red;
                    verticalLine.StartX = centerX;
                    verticalLine.Size = ContinuousGuideRectangleStrokeSize * 1.0 / 10;
                    verticalLine.StartY = 0;
                    verticalLine.EndX = centerX;
                    verticalLine.EndY = Image.PixelHeight;

                    DrawLine horizentalLine = new DrawLine();
                    horizentalLine.StrokeColor = Brushes.Red;
                    horizentalLine.Size = ContinuousGuideRectangleStrokeSize * 1.0 / 10;
                    horizentalLine.StartX = 0;
                    horizentalLine.StartY = centerY;
                    horizentalLine.EndX = image.PixelWidth;
                    horizentalLine.EndY = centerY;
                     
                    DrawLabel label = new DrawLabel();
                    label.Text = "LIVE  ";
                    label.TextAlign = DrawLabel.DrawLabelAlign.RIGHT;
                    label.X = Image.PixelWidth;
                    label.Y = 100;
                    label.Size = 40;
                    label.Foreground = Brushes.Green;
                    label.Background = Brushes.Black;

                    Result result = new Result();
                    result.DrawManager.DrawLabels.Add(label);
                    result.DrawManager.DrawLines.Add(horizentalLine);
                    result.DrawManager.DrawLines.Add(verticalLine);
                    result.DrawManager.DrawRectangle.Add(rectangle);
                    this.Result = result;
                }
                catch
                {

                }
            }
        }

        private void FocusAssist()
        {
            try
            {

                window.Dispatcher.Invoke(new Action(() =>
                {
                    float value = HFocusUtill.GetBlurValue(Image);

                    DrawLabel label = new DrawLabel()
                    {
                        Foreground = Brushes.Red,
                        Background = Brushes.Black,
                        Text = Math.Round(value, 2).ToString(),
                        Size = 40,
                        X = 0,
                        Y = 0
                    };

                    Result result = new Result();
                    result.DrawManager.DrawLabels.Add(label);
                    this.Result = result;
                }));
            }
            catch
            {

            }
        }

        public bool IsFocusAssist
        {
            get { return isFocusAssist; }
            set { isFocusAssist = value; NotifyPropertyChanged("IsFocusAssist"); }
        }

        private bool isFocusAssist = false;

        public void ContiniusShot()
        {
            if (SelectedCarkind != null && SelectedCamera != null)
            {
                Image = null;
                IsFitDisplay = true;
                OneShot();
                DrawCalibrationRectangle();

                IsFitDisplay = false;
                runContiniusShot = true;
                IsShowCameraGroupBox = false;
                new Thread(new ThreadStart(() =>
                {
                    isRunningContinuous = true;
                    while (runContiniusShot)
                    {
                        try
                        {
                            BitmapImage currentImage = cameraManager.SnapShot(SelectedCamera, SelectedCarkind.IsRotateCamera);

                            Application.Current.Dispatcher.Invoke(new Action(() =>
                            {
                                if (isRunningContinuous)
                                {
                                //BitmapImage currentImage = new BitmapImage(new Uri(System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + "\\dtest.jpg"));
                                try
                                    {
                                        Stopwatch sw = new Stopwatch();
                                        sw.Start();
                                        this.Image = currentImage;
                                        DrawCalibrationRectangle();

                                        if (IsFocusAssist)
                                        {
                                            FocusAssist();
                                        }

                                        sw.Stop();
                                        Console.WriteLine("화면 표시 소요 시간 : " + sw.ElapsedMilliseconds);
                                    }
                                    catch (Exception e)
                                    {
                                        LogManager.Write("카메라 연속 촬영 실패 : " + e.Message);
                                    }
                                }

                            }));

                        }
                        catch
                        {

                        }
                    }

                    window.Dispatcher.Invoke(new Action(() =>
                    {
                        IsShowCameraGroupBox = true;
                    }));
                    isRunningContinuous = false;
                    IsFitDisplay = true;

                })).Start();
            }
        }

        public void StopContiniusShot()
        {
            if (SelectedCamera != null && SelectedCarkind != null)
            {
                runContiniusShot = false;
                IsFitDisplay = true;
                cameraManager.StopSnapShot(SelectedCamera);
                Result = new Result();
            }
        }

        public void OneShot()
        {
            if (SelectedCamera != null && SelectedCarkind != null)
            {
                StopContiniusShot();

                Image = null;
                BitmapImage image = cameraManager.OneShot(SelectedCamera, SelectedCarkind.IsRotateCamera);
                Image = image;
            }
        }

        public void SaveCameraParams()
        {
            SelectedInspection.SaveCameraParams(SelectedCamera.Gain, SelectedCamera.Exposure);

            SelectedCarkind.SaveData();
        }

        public void SaveCalibrationParams()
        {
            SelectedCarkind.SaveData();
        }

        public void SaveImage()
        {
            if (Image != null)
            {
                SaveFileDialog dialog = new SaveFileDialog();
                bool? isShow = dialog.ShowDialog();
                if (isShow.HasValue && isShow.Value)
                {
                    BitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(Image));

                    using (var fileStream = new System.IO.FileStream(dialog.FileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                    {
                        encoder.Save(fileStream);
                    }
                }
            }
        }
    }
}
