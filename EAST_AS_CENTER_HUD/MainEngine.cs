using EAST_AS_CENTER_HUD.Camera;
using EAST_AS_CENTER_HUD.Carinfo;
using EAST_AS_CENTER_HUD.Setting;
using EAST_AS_CENTER_HUD.Struct;
using HControl.ChatControl;
using HCore;
using HResult;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Utill;
using static PylonC.NETSupportLibrary.DeviceEnumerator;

namespace EAST_AS_CENTER_HUD
{
    public class MainEngine : INotifyPropertyChanged
    {
        public CameraSettingEngine CameraSettingEngine { get { return cameraSettingEngine; } set { cameraSettingEngine = value; } }
        private CameraSettingEngine cameraSettingEngine;

        public CameraCore cameraCore = new CameraCore();

        public SettingEngine SettingEngine { get { return settingEngine; } set { settingEngine = value; } }
        private SettingEngine settingEngine;

        public CameraManager cameraManager;

        //public SentinelLicenseManager.LicenseManager licenseManager = new SentinelLicenseManager.LicenseManager();

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void NotifyPropertyChanged(String propertyName = "")
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public MainEngine(Window window)
        {
            this.window = window;
            window.Closed += Window_Closed;

            this.dialogCoordinator = DialogCoordinator.Instance;

            //카메라 메니저 설정
            cameraManager = new CameraManager(cameraCore);

            //콘솔 결과값 출력 위치 변경
            //Console.SetOut(ConsoleStr);
            //ConsoleStr.OnWriteStringEvent += ConsoleStr_OnWriteStringEvent;

            //라이센스 매니저 실행
            InitLicenseManager();
        }

        public int StateCheckInterval { get { return iniConfig.GetInt32("CheckInterval", "Tick", 60); } }

        private int LogLimitLine { get { return iniConfig.GetInt32("Display", "logLimit", 100); } }

        public bool ConvertToBigEdian { get { return iniConfig.GetString("Reverse", "ConvertToBigEdian", "TRUE").ToUpper() == "TRUE"; } }

        public bool DistortionMenualTest { get { return iniConfig.GetString("DistortionMenual", "IsUse", "False").ToUpper() == "TRUE"; } }

        public int DistortionMenualPos { get { return iniConfig.GetInt32("DistortionMenual", "Position", 0); } }
        public double DistortionMenualXMove { get { return iniConfig.GetDouble("DistortionMenual", "XMove", 0); } }
        public double DistortionMenualYMove { get { return iniConfig.GetDouble("DistortionMenual", "YMove", 0); } }

        public int ShotDelay { get { return iniConfig.GetInt32("Delay", "Shot", 0); } }

        private string ConsoleStr_OnWriteStringEvent(string savedStr, string str)
        {
            string newStr = "";

            List<string> list = savedStr.Split('\n').ToList();
            list.Add(str);

            list.ForEach(x =>
            {
                x.Replace("\n", "");
            });
            while (list.Count > LogLimitLine)
            {
                list.RemoveAt(0);
            }

            list = list.Where(x => !string.IsNullOrEmpty(x)).ToList();

            list.ForEach(x =>
            {
                newStr += x + "\n";
            });

            return newStr;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            isWindowClosed = true;

            //라이센스 체크 종료
            StopLicenseCheck();
        }

        Window window;
        protected bool isWindowClosed = false;

        IniFile iniConfig = new IniFile(System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + "\\Config.ini");

        public string ImageSavePath { get { return iniConfig.GetString("Image", "Path", @"D:\Result\Image"); } }

        public string CaptureImageSavePath { get { return iniConfig.GetString("Image", "Capture Path", @"D:\Result\Image"); } }

        public bool IsSaveJpeg { get { return iniConfig.GetBoolian("Image", "SaveJpeg", false); } }
        public int JpegQualityLevel { get { return iniConfig.GetInt32("Image", "JpegQualityLevel", 100); } }

        public enum SOCKET_STATE { WAIT, CONNECTED, ERROR };

        public SOCKET_STATE SocketState { get { return socketState; } set { socketState = value; NotifyPropertyChanged("SocketStateStr"); } }
        private SOCKET_STATE socketState = SOCKET_STATE.WAIT;

        public string SocketStateStr
        {
            get
            {
                switch (SocketState)
                {
                    case SOCKET_STATE.CONNECTED:
                        return "접속 완료";
                    case SOCKET_STATE.ERROR:
                        return "에러";
                    case SOCKET_STATE.WAIT:
                        return "대기 중";
                    default:
                        return "";
                }
            }
        }

        public SOCKET_STATE ServerState { get { return serverState; } set { serverState = value; NotifyPropertyChanged("ServerStateStr"); } }
        private SOCKET_STATE serverState = SOCKET_STATE.WAIT;

        public string ServerStateStr
        {
            get
            {
                switch (ServerState)
                {
                    case SOCKET_STATE.CONNECTED:
                        return "생성 완료";
                    case SOCKET_STATE.ERROR:
                        return "에러";
                    case SOCKET_STATE.WAIT:
                        return "대기 중";
                    default:
                        return "";
                }
            }
        }

        //통신 이력
        public ObservableCollection<StructChatMessage> ListCommunicationRecord
        {
            get
            {
                return listCommunicationRecord;
            }
            set
            {
                listCommunicationRecord = value;
                NotifyPropertyChanged("ListCommunicationRecord");
            }
        }
        public ObservableCollection<StructChatMessage> listCommunicationRecord = new ObservableCollection<StructChatMessage>();

        public int CommunicationSelectedPosition { get { return communicationSelectedPosition; } set { communicationSelectedPosition = value; NotifyPropertyChanged("CommunicationSelectedPosition"); } }
        private int communicationSelectedPosition = 0;

        public SolidColorBrush StateCarinfo
        {
            get { return stateCarinfo; }
            set { stateCarinfo = value; NotifyPropertyChanged("StateCarinfo"); }
        }

        public SolidColorBrush StateCutOff
        {
            get { return stateCutOff; }
            set { stateCutOff = value; NotifyPropertyChanged("StateCutOff"); }
        }

        public SolidColorBrush StateDistorion
        {
            get { return stateDistorion; }
            set { stateDistorion = value; NotifyPropertyChanged("StateDistorion"); }
        }

        public SolidColorBrush StateCenter
        {
            get { return stateCenter; }
            set { stateCenter = value; NotifyPropertyChanged("StateCenter"); }
        }

        public SolidColorBrush StateFullContent
        {
            get { return stateFullContent; }
            set { stateFullContent = value; NotifyPropertyChanged("StateFullContent"); }
        }

        public SolidColorBrush StateFinish
        {
            get { return stateFinish; }
            set { stateFinish = value; NotifyPropertyChanged("StateFinish"); }
        }

        private SolidColorBrush stateCarinfo = Brushes.Gray;
        private SolidColorBrush stateCutOff = Brushes.Gray;
        private SolidColorBrush stateDistorion = Brushes.Gray;
        private SolidColorBrush stateCenter = Brushes.Gray;
        private SolidColorBrush stateFullContent = Brushes.Gray;
        private SolidColorBrush stateFinish = Brushes.Gray;

        public enum ProgressState { Carinfo, Cutoff, Distortion, Center, FullContent, Finish }

        public void SetProgressState(ProgressState progress, HCore.HResult.RESULT result)
        {
            switch (progress)
            {
                case ProgressState.Carinfo:
                    StateCarinfo = Brushes.Green;
                    break;
                case ProgressState.Cutoff:
                    if (result == HCore.HResult.RESULT.OK)
                    {
                        StateCutOff = Brushes.Green;
                    }
                    else
                    {
                        StateCutOff = Brushes.Red;
                    }

                    break;
                case ProgressState.Distortion:
                    if (result == HCore.HResult.RESULT.OK)
                    {
                        StateDistorion = Brushes.Green;
                    }
                    else
                    {
                        StateDistorion = Brushes.Red;
                    }
                    break;
                case ProgressState.Center:
                    if (result == HCore.HResult.RESULT.OK)
                    {
                        StateCenter = Brushes.Green;
                    }
                    else
                    {
                        StateCenter = Brushes.Red;
                    }
                    break;
                case ProgressState.FullContent:
                    if (result == HCore.HResult.RESULT.OK)
                    {
                        StateFullContent = Brushes.Green;
                    }
                    else
                    {
                        StateFullContent = Brushes.Red;
                    }
                    break;
                case ProgressState.Finish:
                    StateFinish = Brushes.Green;
                    break;
            }
        }

        public void ClearProgressState()
        {
            StateCarinfo = Brushes.Gray;
            StateCutOff = Brushes.Gray;
            StateDistorion = Brushes.Gray;
            StateCenter = Brushes.Gray;
            StateFullContent = Brushes.Gray;
            StateFinish = Brushes.Gray;
        }

        public HStringWriter ConsoleStr
        {
            get
            {
                return consoleStr;
            }
            set
            {
                consoleStr = value;
                NotifyPropertyChanged("ConsoleStr");
            }
        }
        public HStringWriter consoleStr = new HStringWriter();

        //검사 정보
        public StructInspectionInfo InspectionInfo { get { return inspectionInfo; } set { inspectionInfo = value; NotifyPropertyChanged("InspectionInfo"); } }
        private StructInspectionInfo inspectionInfo;

        //검사 정보 초기화
        public void ClearInspectionInfo()
        {
            InspectionInfo = null;
        }

        //화면 이미지
        public BitmapSource ImageCutoff { get { return imageCutoff; } set { imageCutoff = value; NotifyPropertyChanged("ImageCutoff"); } }
        private BitmapSource imageCutoff;

        public BitmapSource ImageDistortion { get { return imageDistortion; } set { imageDistortion = value; NotifyPropertyChanged("ImageDistortion"); } }
        private BitmapSource imageDistortion;

        public BitmapSource ImageCenter { get { return imageCenter; } set { imageCenter = value; NotifyPropertyChanged("ImageCenter"); } }
        private BitmapSource imageCenter;

        public BitmapSource ImageFullContents { get { return imageFullContents; } set { imageFullContents = value; NotifyPropertyChanged("ImageFullContents"); } }
        private BitmapSource imageFullContents;

        public void SaveBitmapImage(BitmapSource image, string path)
        {
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));

            using (var fileStream = new System.IO.FileStream(path, System.IO.FileMode.Create))
            {
                encoder.Save(fileStream);
            }
        }

        public void SaveJpegImage(BitmapSource image, string path)
        {
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(image));
            encoder.QualityLevel = JpegQualityLevel;

            using (var fileStream = new System.IO.FileStream(path, System.IO.FileMode.Create))
            {
                encoder.Save(fileStream);
            }
        }

        //잘못된 기종
        public bool IsVisibleCodeCheck { get { return isVisibleCodeCheck; } set { isVisibleCodeCheck = value; NotifyPropertyChanged("IsVisibleCodeCheck"); } }
        private bool isVisibleCodeCheck = false;

        //카메라 확인
        public bool IsVisibleCamCheck { get { return isVisibleCamCheck; } set { isVisibleCamCheck = value; NotifyPropertyChanged("IsVisibleCamCheck"); } }
        private bool isVisibleCamCheck = false;

        //검사 결과
        public IHResult ResultCutoff { get { return resultCutoff; } set { resultCutoff = value; NotifyPropertyChanged("ResultCutoff"); } }
        private IHResult resultCutoff;

        public IHResult ResultDistortion { get { return resultDistortion; } set { resultDistortion = value; NotifyPropertyChanged("ResultDistortion"); } }
        private IHResult resultDistortion;

        public IHResult ResultCenter { get { return resultCenter; } set { resultCenter = value; NotifyPropertyChanged("ResultCenter"); } }
        private IHResult resultCenter;

        public IHResult ResultFullContents { get { return resultFullContents; } set { resultFullContents = value; NotifyPropertyChanged("ResultFullContents"); } }
        private IHResult resultFullContents;

        #region InspectionInfo

        public bool GetDistionIsReverseXArray
        {
            get
            {
                if (iniConfig.GetString("Reverse", "DistortionXArray", "False").ToUpper() == "TRUE")
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
        }

        public bool GetDistionIsReverseYArray
        {
            get
            {
                if (iniConfig.GetString("Reverse", "DistortionYArray", "False").ToUpper() == "TRUE")
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
        }


        public bool GetCutOffIsReverseMinus
        {
            get
            {
                if (iniConfig.GetString("Reverse", "CutoffMinus", "False").ToUpper() == "TRUE")
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
        }
        public int ServerPort
        {
            get
            {
                return iniConfig.GetInt32("Server", "Port", 9988);
            }
        }

        public string ServerIP
        {
            get
            {
                return iniConfig.GetString("Server", "IP", "127.0.0.1");
            }
        }


        #endregion


        #region Command
        public string Command_Carinfo
        {
            get
            {
                if (command_Carinfo == null)
                {
                    InitCommand();
                }
                return command_Carinfo;
            }
        }
        private string command_Carinfo = null;


        public string Command_CutOff
        {
            get
            {
                if (command_CutOff == null)
                {
                    InitCommand();
                }
                return command_CutOff;
            }
        }
        private string command_CutOff = null;

        public string Command_Distortion
        {
            get
            {
                if (command_Distortion == null)
                {
                    InitCommand();
                }
                return command_Distortion;
            }
        }
        private string command_Distortion = null;

        public string Command_Center
        {
            get
            {
                if (command_Center == null)
                {
                    InitCommand();
                }
                return command_Center;
            }
        }
        private string command_Center = null;

        public string Command_FullContents
        {
            get
            {
                if (command_FullContents == null)
                {
                    InitCommand();
                }
                return command_FullContents;
            }
        }
        private string command_FullContents = null;

        //Command Finish
        public string Command_CutOffFinish
        {
            get
            {
                if (command_CutOffFinish == null)
                {
                    InitCommand();
                }
                return command_CutOffFinish;
            }
        }
        private string command_CutOffFinish = null;

        public string Command_DistortionFinish
        {
            get
            {
                if (command_DistortionFinish == null)
                {
                    InitCommand();
                }
                return command_DistortionFinish;
            }
        }
        private string command_DistortionFinish = null;

        public string Command_CenterFinish
        {
            get
            {
                if (command_CenterFinish == null)
                {
                    InitCommand();
                }
                return command_CenterFinish;
            }
        }
        private string command_CenterFinish = null;


        public string Command_FullContentsFinish
        {
            get
            {
                if (command_FullContentsFinish == null)
                {
                    InitCommand();
                }
                return command_FullContentsFinish;
            }
        }
        private string command_FullContentsFinish = null;


        //Capture Command
        public string Command_CutOffCapture
        {
            get
            {
                if (command_CutOffCapture == null)
                {
                    InitCommand();
                }
                return command_CutOffCapture;
            }
        }
        private string command_CutOffCapture = null;

        public string Command_DistortionCapture
        {
            get
            {
                if (command_DistortionCapture == null)
                {
                    InitCommand();
                }
                return command_DistortionCapture;
            }
        }
        private string command_DistortionCapture = null;

        public string Command_FullContentsCapture
        {
            get
            {
                if (command_FullContentsCapture == null)
                {
                    InitCommand();
                }
                return command_FullContentsCapture;
            }
        }
        private string command_FullContentsCapture = null;

        //킵얼라이브
        public string Command_KeepAlive
        {
            get
            {
                if (command_KeepAlive == null)
                {
                    InitCommand();
                }
                return command_KeepAlive;
            }
        }
        private string command_KeepAlive = null;

        //카메라 촬영
        public string Command_CameraCheck
        {
            get
            {
                if (command_CameraCheck == null)
                {
                    InitCommand();
                }
                return command_CameraCheck;
            }
        }
        private string command_CameraCheck = null;

        //검사 화면 캡쳐
        public string Command_DistortionInspectionCapture
        {
            get
            {
                if (command_DistortionInspectionCapture == null)
                {
                    InitCommand();
                }
                return command_DistortionInspectionCapture;
            }
        }
        private string command_DistortionInspectionCapture = null;

        //현재 커맨드
        public string CurrentCommand { get { return currentCommand; } set { currentCommand = value; } }
        private string currentCommand;

        #endregion

        #region Camera
        public int AllCameraCount { get { return allCameraCount; } set { allCameraCount = value; NotifyPropertyChanged("AllCameraCount"); } }
        private int allCameraCount;

        public int RequiedCameraCount { get { return requiedCameraCount; } set { requiedCameraCount = value; NotifyPropertyChanged("RequiedCameraCount"); } }
        private int requiedCameraCount;

        public int ConnectedCameraCount { get { return connectedCameraCount; } set { connectedCameraCount = value; NotifyPropertyChanged("ConnectedCameraCount"); } }
        private int connectedCameraCount;

        public bool IsVisibleCameraErrorIcon { get { return isVisibleCameraErrorIcon; } set { isVisibleCameraErrorIcon = value; NotifyPropertyChanged("IsVisibleCameraErrorIcon"); } }
        private bool isVisibleCameraErrorIcon = false;

        public string CameraErrorMsg { get { return cameraErrorMsg; } set { cameraErrorMsg = value; NotifyPropertyChanged("CameraErrorMsg"); } }
        private string cameraErrorMsg;
        #endregion

        public void InitCommand()
        {
            command_Carinfo = iniConfig.GetString("Command", "차량정보", "CARINFO");
            command_CutOff = iniConfig.GetString("Command", "잘림검사", "BHS");
            command_Distortion = iniConfig.GetString("Command", "왜곡검사", "HWS");
            command_Center = iniConfig.GetString("Command", "센터검사", "CWS");
            command_FullContents = iniConfig.GetString("Command", "풀컨텐츠", "BGW");

            command_CutOffFinish = iniConfig.GetString("Command", "잘림검사 완료", "BHS"); ;
            command_DistortionFinish = iniConfig.GetString("Command", "왜곡검사 완료", "HWS");
            command_CenterFinish = iniConfig.GetString("Command", "센터검사 완료", "CWS");
            command_FullContentsFinish = iniConfig.GetString("Command", "풀컨텐츠 완료", "BGW");

            command_CutOffCapture = iniConfig.GetString("Command", "잘림검사 촬영", "BHC");
            command_DistortionCapture = iniConfig.GetString("Command", "왜곡검사 촬영", "HWC");
            command_FullContentsCapture = iniConfig.GetString("Command", "풀컨텐츠 촬영", "BGC");

            command_DistortionInspectionCapture = iniConfig.GetString("Command", "왜곡검사 검사촬영", "HWI");

            command_CameraCheck = iniConfig.GetString("Command", "카메라 상태 확인", "CAMCHECK");

            command_KeepAlive = iniConfig.GetString("Command", "킵얼라이브", "KEEPALIVE");

            LogManager.Write("커맨트 최신화");
        }

        public void InitLicenseManager()
        {
            //licenseManager.OnLicenseErrorEvent += LicenseManager_OnLicenseErrorEvent;
            //licenseManager.StartCheckAsync(1000);
        }

        public void StopLicenseCheck()
        {
            //licenseManager.StopCheck();
        }

        private void LicenseManager_OnLicenseErrorEvent()
        {
            //MessageBox.Show("라이센스 에러가 발생하였습니다.", "라이센스", MessageBoxButton.OK, MessageBoxImage.Error);
            //Environment.Exit(0);
        }

        /// <summary>
        /// 카메라 설정 쓰레드 시작
        /// </summary>
        public void InitCameraCheckThread()
        {
            new Thread(new ThreadStart(() =>
            {
                try
                {
                    //while (!isWindowClosed)
                    //{
                    List<Device> AllDeviceList = CameraManager.GetAllDevices();
                    List<Device> connectedCameraList = CameraManager.GetConnectedDevices();

                    List<KeyValuePair<string, string>> requiedCameraList = iniConfig.GetSectionValuesAsList("Camera Index");

                    for (int i = 0; i < connectedCameraList.Count; i++)
                    {
                        Device x = connectedCameraList[i];

                        if (requiedCameraList.Where(y => y.Value == x.SerialNum).ToList().Count() == 0)
                        {
                            connectedCameraList.Remove(x);
                        }
                    }

                    AllCameraCount = AllDeviceList.Count;
                    ConnectedCameraCount = connectedCameraList.Count;
                    RequiedCameraCount = requiedCameraList.Count;

                    if (ConnectedCameraCount != RequiedCameraCount)
                    {
                        if (allCameraCount - connectedCameraCount > 0)
                        {
                            window.Dispatcher.Invoke(new Action(() =>
                            {
                                IsVisibleCameraErrorIcon = true;
                                CameraErrorMsg = "연결되지 않은 카메라가 존재합니다.";
                                ShowCameraAutoConnectDialogAsync();
                            }));
                        }
                        else
                        {
                            window.Dispatcher.Invoke(new Action(() =>
                            {
                                IsVisibleCameraErrorIcon = true;
                                CameraErrorMsg = "카메라가 정상적으로 연결되지 않았습니다.";
                            }));
                        }
                    }
                    else
                    {
                        window.Dispatcher.Invoke(new Action(() =>
                        {
                            IsVisibleCameraErrorIcon = false;
                            CameraErrorMsg = "";
                        }));
                    }

                    Thread.Sleep(5000);
                    //}
                }
                catch (Exception e)
                {
                    LogManager.Write("카메라 접속 상태 체크 쓰레드 종료 : " + e.Message);
                }
            })).Start();
        }

        public void ClearDisplay()
        {
            ImageCutoff = null;
            ImageDistortion = null;
            ImageCenter = null;
            ImageFullContents = null;

            LogManager.Write("디스플레이 초기화");
        }

        private ICommand openSetting;
        public ICommand OpenSettingCommand
        {
            get { return (this.openSetting) ?? (this.openSetting = new DelegateCommand(OpenSetting)); }
        }

        public void OpenSetting()
        {
            Window_Setting window = new Window_Setting();
            SettingEngine = new SettingEngine(cameraCore);
            window.DataContext = this;
            window.Owner = Application.Current.MainWindow;
            window.ShowDialog();
        }

        private ICommand openCamera;
        public ICommand OpenCameraCommand
        {
            get { return (this.openCamera) ?? (this.openCamera = new DelegateCommand(OpenCamera)); }
        }

        public void OpenCamera()
        {
            Window_CameraSetting window = new Window_CameraSetting();
            CameraSettingEngine = new CameraSettingEngine(window, cameraCore);
            window.DataContext = this;
            window.Owner = Application.Current.MainWindow;
            window.ShowDialog();
        }

        private ICommand openCameraSetting;
        public ICommand OpenCameraSettingCommand
        {
            get { return (this.openCameraSetting) ?? (this.openCameraSetting = new DelegateCommand(OpenCameraSetting)); }
        }

        public void OpenCameraSetting()
        {
            Window_CameraConnectSetting window = new Window_CameraConnectSetting();
            window.Owner = Application.Current.MainWindow;
            window.ShowDialog();
        }



        private ICommand openCarinfoSetting;
        public ICommand OpenCarinfoSettingCommand
        {
            get { return (this.openCarinfoSetting) ?? (this.openCarinfoSetting = new DelegateCommand(OpenCarinfoSetting)); }
        }

        public void OpenCarinfoSetting()
        {
            Window_Carinfo window = new Window_Carinfo();
            window.Owner = Application.Current.MainWindow;
            window.ShowDialog();
        }

        public void AddCommunicationMsg(string title, string msg, HorizontalAlignment align)
        {
            StructChatMessage message = new StructChatMessage();
            message.Align = align;
            message.Title = title;
            message.Message = msg;
            message.BorderColor = Brushes.White;
            message.Foreground = Brushes.White;
            message.ShowTitle = true;

            if (ListCommunicationRecord.Count > 0)
            {
                if (ListCommunicationRecord.Last().Align == align && ListCommunicationRecord.Last().Title == title)
                {
                    message.ShowTitle = false;
                }
            }

            ListCommunicationRecord.Add(message);

            CommunicationSelectedPosition = ListCommunicationRecord.Count - 1;
        }

        public void RemoveCommunicateMessage()
        {
            ListCommunicationRecord.Clear();
        }

        #region dialog
        private IDialogCoordinator dialogCoordinator;

        public void ShowCameraAutoConnectDialogAsync()
        {

        }
        #endregion
    }
}

