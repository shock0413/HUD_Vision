using EAST_AS_CENTER_HUD.Camera;
using EAST_AS_CENTER_HUD.Carinfo;
using EAST_AS_CENTER_HUD.Struct;
using HanseroDisplay;
using HCore;
using HHUDTool;
using HResult;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static HCore.HResult;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace EAST_AS_CENTER_HUD.Setting
{
    public class SettingEngine : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void NotifyPropertyChanged( String propertyName = "")
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public BitmapImage Image { get { return image; } set { image = value; NotifyPropertyChanged("Image"); } }
        private BitmapImage image;

        public BitmapImage MasterImage { get { return masterImage; } set { masterImage = value; NotifyPropertyChanged("MasterImage"); } }
        private BitmapImage masterImage;

        public IHResult Result { get { return result; } set { result = value; NotifyPropertyChanged("Result"); } }
        private IHResult result;

        public IHResult MasterResult { get { return masterResult; } set { masterResult = value; NotifyPropertyChanged("MasterResult"); } }
        private IHResult masterResult;

        public string MainResult { get { return mainResult; }  set { mainResult = value; NotifyPropertyChanged("MainResult"); } }
        private string mainResult;

        public string MainResultCommant { get { return mainResultComment; } set { mainResultComment = value; NotifyPropertyChanged("MainResultCommant"); } }
        private string mainResultComment;

        public SolidColorBrush MainResultColor { get { return mainResultColor; } set { mainResultColor = value; NotifyPropertyChanged("MainResultColor"); } }
        public SolidColorBrush mainResultColor;

        private CameraManager cameraManager;

        public ObservableCollection<StructCarkind> ListStructCarkind { get { return listStructCarkind; } set { listStructCarkind = value; NotifyPropertyChanged("ListStructCarkind"); } }
        private ObservableCollection<StructCarkind> listStructCarkind;

        public ObservableCollection<StructInspection> ListStructInspection { get { return listStructInspection; } set { listStructInspection = value; NotifyPropertyChanged("ListStructInspection"); } }
        private ObservableCollection<StructInspection> listStructInspection;

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
                NotifyPropertyChanged("IsEnabledSetting");

                //검사항목 설정
                ListStructInspection = new ObservableCollection<StructInspection>(SelectedCarkind.Inspections.Cast<StructInspection>());

                //파라미터 설정
                CurrentHudWidth = SelectedCarkind.HudWidth;
                CurrentHudHeight = SelectedCarkind.HudHeight;
                CurrentHudHorizentalCount = SelectedCarkind.DotHorizentalCount;
                CurrentHudVerticalCount = SelectedCarkind.DotVerticalCount;
                CurrentHudHorizentalInterval = SelectedCarkind.DotHorizentalInterval;
                CurrentHudVerticalInterval = SelectedCarkind.DotVerticalInterval;

                LoadCutoffParams();
                LoadDistortionParams();
                LoadCenterParams();
                LoadFullContentsParams();
            }
        }
        private StructCarkind selectedCarkind;

        public StructInspection SelectedInspection
        {
            get
            {
                return selectedInspection;
            }
            set
            {
                selectedInspection = value;
                LoadMasterImage();
                NotifyPropertyChanged("SelectedInspection");
                NotifyPropertyChanged("IsEnabledDisplay");

            }
        }
        private StructInspection selectedInspection;

        //Hud Spec

        public double CurrentHudWidth { get { return currentHudWidht; } set { currentHudWidht = value; NotifyPropertyChanged("CurrentHudWidth"); } }
        private double currentHudWidht;

        public double CurrentHudHeight{ get { return currentHudHeight; } set { currentHudHeight = value; NotifyPropertyChanged("CurrentHudHeight"); } }
        private double currentHudHeight;

        public double CurrentHudHorizentalCount { get { return currentHudHorizentalCount; } set { currentHudHorizentalCount = value; NotifyPropertyChanged("CurrentHudHorizentalCount"); } }
        private double currentHudHorizentalCount;

        public double CurrentHudVerticalCount { get { return currentHudVerticalCount; } set { currentHudVerticalCount = value; NotifyPropertyChanged("CurrentHudVerticalCount"); } }
        private double currentHudVerticalCount;

        public double CurrentHudHorizentalInterval { get { return currentHudHorizentalInterval; } set { currentHudHorizentalInterval = value; NotifyPropertyChanged("CurrentHudHorizentalInterval"); } }
        private double currentHudHorizentalInterval;

        public double CurrentHudVerticalInterval { get { return currentHudVerticalInterval; } set { currentHudVerticalInterval = value; NotifyPropertyChanged("CurrentHudVerticalInterval"); } }
        private double currentHudVerticalInterval;

        //Cutoff
        public double CurrentPassRangeTop { get { return currentPassRangeTop; } set { currentPassRangeTop = value; NotifyPropertyChanged("CurrentPassRangeTop"); NotifyPropertyChanged("IsChangedCutoffParams"); } }
        private double currentPassRangeTop;

        public double CurrentPassRangeBottom { get { return currentPassRangeBottom; } set { currentPassRangeBottom = value; NotifyPropertyChanged("CurrentPassRangeBottom"); NotifyPropertyChanged("IsChangedCutoffParams"); } }
        private double currentPassRangeBottom;

        public int CurrentCutoffMinBlob { get { return currentCutoffMinBlob; } set { currentCutoffMinBlob = value; NotifyPropertyChanged("CurrentCutoffMinBlob"); NotifyPropertyChanged("IsChangedCutoffParams"); } }
        public int currentCutoffMinBlob;

        public int CurrentCutoffMaxBlob { get { return currentCutoffMaxBlob; } set { currentCutoffMaxBlob = value; NotifyPropertyChanged("CurrentCutoffMaxBlob"); NotifyPropertyChanged("IsChangedCutoffParams"); } }
        public int currentCutoffMaxBlob;

        public int CurrentCutoffBrightLimit { get { return currentCutoffBrightLimit; } set { currentCutoffBrightLimit = value; NotifyPropertyChanged("CurrentCutoffBrightLimit"); NotifyPropertyChanged("IsChangedCutoffParams"); } }
        public int currentCutoffBrightLimit;

        public double CurrentCutoffTransmissionFactor { get { return currentCutoffTransmissionFactor; } set { currentCutoffTransmissionFactor = value; NotifyPropertyChanged("CurrentCutoffTransmissionFactor"); NotifyPropertyChanged("IsChangedCutoffParams"); } }
        public double currentCutoffTransmissionFactor;

        public double CurrentCutoffTransmissionFactor1 { get { return currentCutoffTransmissionFactor1; } set { currentCutoffTransmissionFactor1 = value; NotifyPropertyChanged("CurrentCutoffTransmissionFactor1"); NotifyPropertyChanged("IsChangedCutoffParams"); } }
        public double currentCutoffTransmissionFactor1;

        public double CurrentCutoffTransmissionFactor2 { get { return currentCutoffTransmissionFactor2; } set { currentCutoffTransmissionFactor2 = value; NotifyPropertyChanged("CurrentCutoffTransmissionFactor2"); NotifyPropertyChanged("IsChangedCutoffParams"); } }
        public double currentCutoffTransmissionFactor2;

        public double CurrentCutoffTransmissionFactor3 { get { return currentCutoffTransmissionFactor3; } set { currentCutoffTransmissionFactor3 = value; NotifyPropertyChanged("CurrentCutoffTransmissionFactor3"); NotifyPropertyChanged("IsChangedCutoffParams"); } }
        public double currentCutoffTransmissionFactor3;

        public double CurrentCutoffTransmissionFactorOver40 { get { return currentCutoffTransmissionFactorOver40; } set { currentCutoffTransmissionFactorOver40 = value; NotifyPropertyChanged("CurrentCutoffTransmissionFactorOver40"); NotifyPropertyChanged("IsChangedCutoffParams"); } }
        public double currentCutoffTransmissionFactorOver40;

        public bool IsReverseCutOffValue { get { return isReverseCutOffValue; } set { isReverseCutOffValue = value; NotifyPropertyChanged("IsReverseCutOffValue"); NotifyPropertyChanged("IsChangedCutoffParams"); } }
        private bool isReverseCutOffValue = false;

        //Distortion
        public int CurrentDistortionMinBlob { get { return currentDistortionMinBlob; } set { currentDistortionMinBlob = value; NotifyPropertyChanged("CurrentDistortionMinBlob"); NotifyPropertyChanged("IsChangedDistortionParams"); } }
        private int currentDistortionMinBlob;

        public int CurrentDistortionMaxBlob { get { return currentDistortionMaxBlob; } set { currentDistortionMaxBlob = value; NotifyPropertyChanged("CurrentDistortionMaxBlob"); NotifyPropertyChanged("IsChangedDistortionParams"); } }
        private int currentDistortionMaxBlob;

        public int CurrentDistortionBrightLimit { get { return currentDistortionBrightLimit; } set { currentDistortionBrightLimit = value; NotifyPropertyChanged("CurrentDistortionBrightLimit"); NotifyPropertyChanged("IsChangedDistortionParams"); } }
        private int currentDistortionBrightLimit;

        public double CurrentDistortionMasterVerticalMMPerPixel { get { return currentDistortionMasterVerticalMMPerPixel; }
            set { currentDistortionMasterVerticalMMPerPixel = value; NotifyPropertyChanged("CurrentDistortionMasterVerticalMMPerPixel"); }
        }
        private double currentDistortionMasterVerticalMMPerPixel;

        public double CurrentDistortionMasterHorizentalMMPerPixel
        {
            get { return currentDistortionMasterHorizentalMMPerPixel; }
            set { currentDistortionMasterHorizentalMMPerPixel = value; NotifyPropertyChanged("CurrentDistortionMasterHorizentalMMPerPixel"); }
        }
        private double currentDistortionMasterHorizentalMMPerPixel;

        //Center
        public int CurrentCenterMinBlob { get { return currentCenterMinBlob; } set { currentCenterMinBlob = value; NotifyPropertyChanged("CurrentCenterMinBlob"); NotifyPropertyChanged("IsChangedCenterParams"); } }
        private int currentCenterMinBlob;

        public int CurrentCenterMaxBlob { get { return currentCenterMaxBlob; } set { currentCenterMaxBlob = value; NotifyPropertyChanged("CurrentCenterMaxBlob"); NotifyPropertyChanged("IsChangedCenterParams"); } }
        private int currentCenterMaxBlob;

        public int CurrentCenterBrightLimit { get { return currentCenterBrightLimit; } set { currentCenterBrightLimit = value; NotifyPropertyChanged("CurrentCenterBrightLimit"); NotifyPropertyChanged("IsChangedCenterParams"); } }
        private int currentCenterBrightLimit;

        public bool IsCenterReverseXValue { get { return isCenterReverseXValue; } set { isCenterReverseXValue = value; NotifyPropertyChanged("IsCenterReverseXValue"); NotifyPropertyChanged("IsChangedCenterParams"); } }
        private bool isCenterReverseXValue = false;

        public bool IsCenterReverseYValue { get { return isCenterReverseYValue; } set { isCenterReverseYValue = value; NotifyPropertyChanged("IsCenterReverseYValue"); NotifyPropertyChanged("IsChangedCenterParams"); } }
        private bool isCenterReverseYValue = false;

        //Fullcontents
        public int CurrentFullContentsScore { get { return currentFullContentsScore; } set { currentFullContentsScore = value; NotifyPropertyChanged("CurrentFullContentsScore"); NotifyPropertyChanged("IsChangedFullContentsParams");  } }
        private int currentFullContentsScore;

        public string CurrentFullContentsTemplitPath { get { return currentFullContentsTemplitPath; } set { currentFullContentsTemplitPath = value; NotifyPropertyChanged("CurrentFullContentsTemplitPath"); NotifyPropertyChanged("IsChangedFullContentsParams"); } }
        private string currentFullContentsTemplitPath;

        //Enable
        public bool IsEnabledSetting
        {
            get
            {
                return SelectedCarkind != null;
            }
        }

        public bool IsEnabledDisplay
        {
            get
            {
                return SelectedInspection != null;
            }
        }

        //폴더 불러오기
        public ObservableCollection<ImageResult> ListFolderImage { get { return listFolderImage; } set { listFolderImage = value; NotifyPropertyChanged("ListFolderImage"); } }
        private ObservableCollection<ImageResult> listFolderImage;

        public bool IsGetSubFile { get { return isGetSubFile; } set { isGetSubFile = value; NotifyPropertyChanged("IsGetSubFile"); ListFolderImage = new ObservableCollection<ImageResult>(GetChildImage(SelectedFolderPath, IsGetSubFile)); } }
        private bool isGetSubFile;

        //Command
        private ICommand openImageFile;
        public ICommand OpenImageFileCommand
        {
            get { return (this.openImageFile) ?? (this.openImageFile = new DelegateCommand(OpenImageFile)); }
        }

        public void OpenImageFile()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Image Files (jpg, jpeg, bmp) | *.jpg;*.jpeg;*.bmp";
            bool? result = dialog.ShowDialog();
            if(result.HasValue && result.Value)
            {
                if (File.Exists(dialog.FileName))
                {
                    BitmapImage image = new BitmapImage();
                    using (FileStream stream = File.OpenRead(dialog.FileName))
                    {
                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.StreamSource = stream;
                        image.EndInit();
                    }
                    Image = image;
                }
            }
        }

        private ICommand setMasterImage;
        public ICommand SetMasterImageCommand
        {
            get { return (this.setMasterImage) ?? (this.setMasterImage = new DelegateCommand(SetMasterImage)); }
        }

        public void SetMasterImage()
        {
            if (image != null)
            {
                string imagePath = SelectedInspection.GetMasterImagePath();

                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(Image));

                using (var fileStream = new System.IO.FileStream(imagePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    encoder.Save(fileStream);
                }


                LoadMasterImage();
            }
        }

        private ICommand oneShot;
        public ICommand OneShotCommand
        {
            get { return (this.oneShot) ?? (this.oneShot = new DelegateCommand(OneShot)); }
        }

        public void OneShot()
        {
            Image = cameraManager.OneShot(SelectedInspection.StructCamera);
        }


        private ICommand openCarinfo;
        public ICommand OpenCarinfoCommand
        {
            get { return (this.openCarinfo) ?? (this.openCarinfo = new DelegateCommand(OpenCarinfo)); }
        }

        public void OpenCarinfo()
        {
            Window_Carinfo window = new Window_Carinfo();
            window.Owner = System.Windows.Application.Current.MainWindow;
            window.ShowDialog();
        }

        private ICommand removeMasterImage;
        public ICommand RemoveMasterImageCommand
        {
            get { return (this.removeMasterImage) ?? (this.removeMasterImage = new DelegateCommand(RemoveMasterImage)); }
        }

        public void RemoveMasterImage()
        {
            if(File.Exists(SelectedInspection.GetMasterImagePath()))
            {
                File.Delete(SelectedInspection.GetMasterImagePath());
            }

            LoadMasterImage();
        }


        private ICommand runInspection;
        public ICommand RunInspectionCommand
        {
            get { return (this.runInspection) ?? (this.runInspection = new DelegateCommand(RunInspection)); }
        }

        public void RunInspection()
        {
            //일반 검사
            IHTool tool = (SelectedInspection.GetTool() as IHTool);
            tool.LoadParams(LoadToolParams(tool));
            
            if (tool.GetType() == typeof(HDistortionTool))
            {
                ((HDistortionTool)tool).IsSetting = true;
            }
            else if (tool.GetType() == typeof(HCenterTool))
            {
                ((HCenterTool)tool).IsSetting = true;
            }

            IHResult result = tool.Run(Image);

            Result = result;

            /*
            //마스터 검사
            tool = (SelectedInspection.GetTool() as IHTool);
            tool.LoadParams(LoadToolParams(tool));
            MasterResult = tool.Run(MasterImage);
            */
        }

        private IHToolParams LoadToolParams(IHTool tool)
        {

            IHToolParams toolParams = null;

            Type toolType = tool.GetType();
            if (toolType == typeof(HCutoffTool))
            {
                HCutoffParams cutoffParams = new HCutoffParams();
                cutoffParams.BrightLimit = CurrentCutoffBrightLimit;
                cutoffParams.MaxBlobCount = CurrentCutoffMaxBlob;
                cutoffParams.MinBlobCount = CurrentCutoffMinBlob;
                cutoffParams.PassRangeTop = CurrentPassRangeTop;
                cutoffParams.PassRangeBottom = CurrentPassRangeBottom;
                cutoffParams.TransmissionFactor = CurrentCutoffTransmissionFactor;
                cutoffParams.TransmissionFactor1 = CurrentCutoffTransmissionFactor1;
                cutoffParams.TransmissionFactor2 = CurrentCutoffTransmissionFactor2;
                cutoffParams.TransmissionFactor3 = CurrentCutoffTransmissionFactor3;
                cutoffParams.TransmissionFactorOver40 = CurrentCutoffTransmissionFactorOver40;

                toolParams = cutoffParams;
            }
            else if(toolType == typeof(HDistortionTool))
            {
            
                HDistortionParams distortionParams = new HDistortionParams();
                distortionParams.BrightLimit = CurrentDistortionBrightLimit;
                distortionParams.MaxBlobCount = CurrentDistortionMaxBlob;
                distortionParams.MinBlobCount = CurrentDistortionMinBlob;
                distortionParams.MasterDotVerticalMMPerPixel = CurrentDistortionMasterVerticalMMPerPixel;
                distortionParams.MasterDotHorizentalMMPerPixel = CurrentDistortionMasterHorizentalMMPerPixel;

                toolParams = distortionParams;
            }
            else if(toolType == typeof(HCenterTool))
            {
                HCenterParams centerParams = new HCenterParams();
                centerParams.BrightLimit = CurrentCenterBrightLimit;
                centerParams.MaxBlobCount = CurrentCenterMaxBlob;
                centerParams.MinBlobCount = CurrentCenterMinBlob;
                centerParams.ReverseX = IsCenterReverseXValue;
                centerParams.ReverseY = IsCenterReverseYValue;

                toolParams = centerParams;
            }
            else if(toolType == typeof(HFullContentsTool))
            {
                
            }

            return toolParams;
        }

        private ICommand saveCutoffSet;
        public ICommand SaveCutoffSetCommand
        {
            get { return (this.saveCutoffSet) ?? (this.saveCutoffSet = new DelegateCommand(SaveCutoffSet)); }
        }

        public void SaveCutoffSet()
        {
            SelectedCarkind.Inspections.ForEach(x =>
            {
                if(x.GetToolType() == typeof(HCutoffTool))
                {
                    HCutoffTool tool = (HCutoffTool)x.GetTool();
                    tool.SavePassRangeBottom(CurrentPassRangeBottom);
                    tool.SavePassRangeTop(CurrentPassRangeTop);
                    tool.SaveMinBlobCount(CurrentCutoffMinBlob);
                    tool.SaveMaxBlobCount(CurrentCutoffMaxBlob);
                    tool.SaveBrightLimit(CurrentCutoffBrightLimit);
                    tool.SaveTransmissionFactor(CurrentCutoffTransmissionFactor);
                    tool.SaveTransmissionFactor1(CurrentCutoffTransmissionFactor1);
                    tool.SaveTransmissionFactor2(CurrentCutoffTransmissionFactor2);
                    tool.SaveTransmissionFactor3(CurrentCutoffTransmissionFactor3);
                    tool.SaveTransmissionFactorOver40(CurrentCutoffTransmissionFactorOver40);
                    tool.SaveIsReverseMoveDirection(IsReverseCutOffValue);

                    NotifyPropertyChanged("IsChangedCutoffParams");
                }
            });
             
        }

        private ICommand saveDistortionSet;
        public ICommand SaveDistortionSetCommand
        {
            get { return (this.saveDistortionSet) ?? (this.saveDistortionSet = new DelegateCommand(SaveDistortionSet)); }
        }

        public void SaveDistortionSet()
        {
            SelectedCarkind.Inspections.ForEach(x =>
            {
                if (x.GetToolType() == typeof(HDistortionTool))
                {
                    HDistortionTool tool = (HDistortionTool)x.GetTool();
                    tool.SaveBrightLimit(CurrentDistortionBrightLimit);
                    tool.SaveMinBlobCount(CurrentDistortionMinBlob);
                    tool.SaveMaxBlobCount(CurrentDistortionMaxBlob);
                    tool.SaveMasterDotVerticalMMperPixel(CurrentDistortionMasterVerticalMMPerPixel);
                    tool.SaveMasterDotHorizentalMMperPixel(CurrentDistortionMasterHorizentalMMPerPixel);

                    NotifyPropertyChanged("IsChangedDistortionParams");
                }
            });
        }

        private ICommand saveCenterSet;
        public ICommand SaveCenterSetCommand
        {
            get { return (this.saveCenterSet) ?? (this.saveCenterSet = new DelegateCommand(SaveCenterSet)); }
        }

        public void SaveCenterSet()
        {
            SelectedCarkind.Inspections.ForEach(x =>
            {
                if (x.GetToolType() == typeof(HCenterTool))
                {
                    HCenterTool tool = (HCenterTool)x.GetTool();
                    tool.SaveBrightLimit(CurrentCenterBrightLimit);
                    tool.SaveMinBlobCount(CurrentCenterMinBlob);
                    tool.SaveMaxBlobCount(CurrentCenterMaxBlob);
                    tool.SaveReverseX(IsCenterReverseXValue);
                    tool.SaveReverseY(IsCenterReverseYValue);


                    NotifyPropertyChanged("IsChangedCenterParams");
                }
            });
        }


        private ICommand saveFullContentsSet;
        public ICommand SaveFullContentsSetCommand
        {
            get { return (this.saveFullContentsSet) ?? (this.saveFullContentsSet = new DelegateCommand(SaveFullContentsSet)); }
        }

        public void SaveFullContentsSet()
        {
            SelectedCarkind.Inspections.ForEach(x =>
            {
                if (x.GetToolType() == typeof(HFullContentsTool))
                {
                    HFullContentsTool tool = (HFullContentsTool)x.GetTool();
                    tool.SaveScoreLimit(CurrentFullContentsScore);

                    NotifyPropertyChanged("IsChangedFullContentsParams");
                }
            });

        }


        private ICommand restoreCutoffParams;
        public ICommand RestoreCutoffParamsCommand
        {
            get { return (this.restoreCutoffParams) ?? (this.restoreCutoffParams = new DelegateCommand(LoadCutoffParams)); }
        }
        /// <summary>
        /// 잘림검사 설정값 복원
        /// </summary>
        public void LoadCutoffParams()
        {
            selectedCarkind.Inspections.Cast<StructInspection>().ToList().ForEach(x =>
            {
                if (x.GetToolType() == typeof(HCutoffTool))
                {
                    HCutoffTool hCutoff = (HCutoffTool)x.GetTool();

                    CurrentPassRangeTop = hCutoff.GetPassRangeTop();
                    CurrentPassRangeBottom = hCutoff.GetPassRangeBottom();
                    CurrentCutoffMinBlob = hCutoff.GetMinBlobCount();
                    CurrentCutoffMaxBlob = hCutoff.GetMaxBlobCount();
                    CurrentCutoffBrightLimit = hCutoff.GetBrightLimit();
                    CurrentCutoffTransmissionFactor = hCutoff.GetTransmissionFactor();
                    CurrentCutoffTransmissionFactor1 = hCutoff.GetTransmissionFactor1();
                    CurrentCutoffTransmissionFactor2 = hCutoff.GetTransmissionFactor2();
                    CurrentCutoffTransmissionFactor3 = hCutoff.GetTransmissionFactor3();
                    CurrentCutoffTransmissionFactorOver40 = hCutoff.GetTransmissionFactorOver40();

                    IsReverseCutOffValue = hCutoff.GetIsReverseMoveDirection();
                }
            });

        }

        private ICommand restoreDistortionParams;
        public ICommand RestoreDistortionParamsCommand
        {
            get { return (this.restoreDistortionParams) ?? (this.restoreDistortionParams = new DelegateCommand(LoadDistortionParams)); }
        }
        /// <summary>
        /// 왜곡검사 설정값 복원
        /// </summary>
        public void LoadDistortionParams()
        {
            selectedCarkind.Inspections.Cast<StructInspection>().ToList().ForEach(x =>
            {
                if (x.GetToolType() == typeof(HDistortionTool))
                {
                    HDistortionTool hDistortion = (HDistortionTool)x.GetTool();

                    CurrentDistortionMinBlob = hDistortion.GetMinBlob();
                    CurrentDistortionMaxBlob = hDistortion.GetMaxBlob();
                    CurrentDistortionBrightLimit = hDistortion.GetBrightLimit();
                    CurrentDistortionMasterVerticalMMPerPixel = hDistortion.GetMasterDotVerticalMMperPixel();
                    CurrentDistortionMasterHorizentalMMPerPixel = hDistortion.GetMasterDotHorizentalMMperPixel();
                }

            });
        }


        private ICommand restoreCenterParams;
        public ICommand RestoreCenterParamsCommand
        {
            get { return (this.restoreCenterParams) ?? (this.restoreCenterParams = new DelegateCommand(LoadCenterParams)); }
        }
        /// <summary>
        /// 중김검사 설정값 복원
        /// </summary>
        public void LoadCenterParams()
        {
            selectedCarkind.Inspections.Cast<StructInspection>().ToList().ForEach(x =>
            {
                if (x.GetToolType() == typeof(HCenterTool))
                {
                    HCenterTool hCenter = (HCenterTool)x.GetTool();

                    CurrentCenterMinBlob = hCenter.GetMinBlob();
                    CurrentCenterMaxBlob = hCenter.GetMaxBlob();
                    CurrentCenterBrightLimit = hCenter.GetBrightLimit();
                    IsCenterReverseXValue = hCenter.GetReverseX();
                    IsCenterReverseYValue = hCenter.GetReverseY();
                }

            });
        }

        private ICommand restoreFullContentsParams;
        public ICommand RestoreFullContentsParamsCommand
        {
            get { return (this.restoreFullContentsParams) ?? (this.restoreFullContentsParams = new DelegateCommand(LoadFullContentsParams)); }
        }
        /// <summary>
        /// 풀컨텐츠검사 설정값 복원
        /// </summary>
        public void LoadFullContentsParams()
        {
            selectedCarkind.Inspections.Cast<StructInspection>().ToList().ForEach(x =>
            {
                if (x.GetToolType() == typeof(HFullContentsTool))
                {
                    HFullContentsTool tool = (HFullContentsTool)x.GetTool();

                    CurrentFullContentsTemplitPath = tool.TemplitPath;
                    CurrentFullContentsScore = tool.GetScoreLimit();
                }
            });
        }

        private ICommand cutOffBlobMinSetting;
        public ICommand CutOffBlobMinSettingCommand
        {
            get { return (this.cutOffBlobMinSetting) ?? (this.cutOffBlobMinSetting = new DelegateCommand(CutOffBlobMinSetting)); }
        }

        public void CutOffBlobMinSetting()
        {
            IsShowCircle = true;
        }

        //변경된 내용 체크
        public bool IsChangedCutoffParams { get { return CheckCutoffParamChanged(); } }
        public bool IsChangedDistortionParams { get { return CheckDistortionParamChanged(); } }
        public bool IsChangedCenterParams { get { return CheckCenterParamChanged(); } }
        public bool IsChangedFullContentsParams { get { return CheckFullContentsParamChanged(); } }

        /// <summary>
        /// 잘림검사 파라미터 변동 체크
        /// </summary>
        /// <returns></returns>
        public bool CheckCutoffParamChanged()
        {
            bool isChanged = false;

            if (SelectedCarkind != null)
            {
                selectedCarkind.Inspections.Cast<StructInspection>().ToList().ForEach(x =>
                {
                    if (x.GetToolType() == typeof(HCutoffTool))
                    {
                        HCutoffTool hCutoff = (HCutoffTool)x.GetTool();

                        if (
                            hCutoff.GetPassRangeTop() != CurrentPassRangeTop ||
                            hCutoff.GetPassRangeBottom() != CurrentPassRangeBottom ||
                            hCutoff.GetMinBlobCount() != CurrentCutoffMinBlob ||
                            hCutoff.GetMaxBlobCount() != CurrentCutoffMaxBlob ||
                            hCutoff.GetBrightLimit() != CurrentCutoffBrightLimit ||
                            hCutoff.GetTransmissionFactor() != CurrentCutoffTransmissionFactor ||
                            hCutoff.GetTransmissionFactor1() != CurrentCutoffTransmissionFactor1 ||
                            hCutoff.GetTransmissionFactor2() != CurrentCutoffTransmissionFactor2 ||
                            hCutoff.GetTransmissionFactor3() != CurrentCutoffTransmissionFactor3 ||
                            hCutoff.GetTransmissionFactorOver40() != CurrentCutoffTransmissionFactorOver40 ||
                            hCutoff.GetIsReverseMoveDirection() != IsReverseCutOffValue
                        )
                        {
                            isChanged = true;
                        }
                    }
                });
            }

            return isChanged;
        }

        /// <summary>
        /// 왜곡검사 파라미터 변동 체크
        /// </summary>
        /// <returns></returns>
        public bool CheckDistortionParamChanged()
        {
            bool isChanged = false;


            if (SelectedCarkind != null)
            {
                selectedCarkind.Inspections.Cast<StructInspection>().ToList().ForEach(x =>
            {
                if (x.GetToolType() == typeof(HDistortionTool))
                {
                    HDistortionTool tool = (HDistortionTool)x.GetTool();

                    if (
                        tool.GetBrightLimit() != CurrentDistortionBrightLimit ||
                        tool.GetMinBlob() != CurrentDistortionMinBlob ||
                        tool.GetMaxBlob() != CurrentDistortionMaxBlob ||
                        tool.GetMasterDotVerticalMMperPixel() != CurrentDistortionMasterVerticalMMPerPixel ||
                        tool.GetMasterDotHorizentalMMperPixel() != CurrentDistortionMasterHorizentalMMPerPixel
                    )
                    {
                        isChanged = true;
                    }
                }
            });
            }

            return isChanged;
        }

        /// <summary>
        /// 중심검사 파라미터 변동 체크
        /// </summary>
        /// <returns></returns>
        public bool CheckCenterParamChanged()
        {
            bool isChanged = false;

            if (SelectedCarkind != null)
            {
                selectedCarkind.Inspections.Cast<StructInspection>().ToList().ForEach(x =>
                {
                    if (x.GetToolType() == typeof(HCenterTool))
                    {
                        HCenterTool tool = (HCenterTool)x.GetTool();

                        if (
                            tool.GetBrightLimit() != CurrentCenterBrightLimit ||
                            tool.GetMinBlob() != CurrentCenterMinBlob ||
                            tool.GetMaxBlob() != CurrentCenterMaxBlob ||
                            tool.GetReverseX() != IsCenterReverseXValue ||
                            tool.GetReverseY() != IsCenterReverseYValue
                        )
                        {
                            isChanged = true;
                        }
                    }
                });
            }

            return isChanged;
        }

        public bool CheckFullContentsParamChanged()
        {
            bool isChanged = false;

            if (SelectedCarkind != null)
            {
                selectedCarkind.Inspections.Cast<StructInspection>().ToList().ForEach(x =>
                {
                    if (x.GetToolType() == typeof(HFullContentsTool))
                    {
                        HFullContentsTool hFullContents = (HFullContentsTool)x.GetTool();

                        if (
                            hFullContents.GetScoreLimit() != CurrentFullContentsScore
                        )
                        {
                            isChanged = true;
                        }
                    }
                });
            }

            return isChanged;
        }

        //CheckFullContentsParamChanged

        //폴더 내용 불러오기
        public string SelectedFolderPath { get { return selectedFolderPath; } set { selectedFolderPath = value; NotifyPropertyChanged("SelectedFolderPath"); } }
        private string selectedFolderPath;

        private ICommand loadFolderImage;
        public ICommand LoadFolderImageCommand
        {
            get { return (this.loadFolderImage) ?? (this.loadFolderImage = new DelegateCommand(LoadFolderImage)); }
        }

        public void LoadFolderImage()
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            if(dialog.ShowDialog() == DialogResult.OK)
            {
                string path = dialog.SelectedPath;
                ListFolderImage = new ObservableCollection<ImageResult>();
                ListFolderImage =  new ObservableCollection<ImageResult>(GetChildImage(path, IsGetSubFile));

                SelectedFolderPath = path;
            }
        }

        private List<ImageResult> GetChildImage(string path, bool isContainSubDir)
        {
            if(string.IsNullOrEmpty( path))
            {
                return new List<ImageResult>();
            }

            List<ImageResult> result = new List<ImageResult>();

            try
            {
                Directory.GetFiles(path).ToList().ForEach(x =>
                {
                    try
                    {
                        if (x.ToUpper().EndsWith(".BMP") || x.ToUpper().EndsWith(".JPG") || x.ToUpper().EndsWith(".JPEG"))
                        {
                            ImageResult imageResult = new ImageResult();
                            imageResult.Path = x;

                            result.Add(imageResult);
                        }
                    }
                    catch
                    {

                    }
                });
            }
            catch
            {

            }

            if (isContainSubDir)
            {
                Directory.GetDirectories(path).ToList().ForEach(x =>
                {
                    try
                    {
                        result.AddRange(GetChildImage(x, isContainSubDir));
                    }
                    catch
                    {

                    }
                });
            }

            return result;
        }

        //폴더 내용 이미지 선택시
        private ICommand selectFolderImage;
        public ICommand SelectFolderImageCommand
        {
            get { return (this.selectFolderImage) ?? (this.selectFolderImage = new DelegateCommand(SelectFolderImage)); }
        }

        public void SelectFolderImage()
        {
            if(SelectedFolderItem != null)
            {
                string path = SelectedFolderItem.Path;
                if (File.Exists(path))
                {
                    BitmapImage image = new BitmapImage();
                    using (FileStream stream = File.OpenRead(path))
                    {
                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.StreamSource = stream;
                        image.EndInit();
                    }
                    Image = image;

                    RunInspection();
                }
            }
            
        }

        public ImageResult SelectedFolderItem { get; set; }

        //전체 검사
        private ICommand inspectFolderList;
        public ICommand InspectFolderListCommand
        {
            get { return (this.inspectFolderList) ?? (this.inspectFolderList = new DelegateCommand(InspectFolderList)); }
        }

        public void InspectFolderList()
        {
            if(ListFolderImage != null)
            {
                if(ListFolderImage.Count > 0)
                {
                    ShowInspectionAsync();
                }
            }

        }

        private void ShowInspectionAsync()
        {
            
        }

        //마스터 이미지 불러오기
        public void LoadMasterImage()
        {
            MasterImage = null;

            if (SelectedInspection != null)
            {
                string path = SelectedInspection.GetMasterImagePath();
                if (File.Exists(path))
                {
                    BitmapImage image = new BitmapImage();
                    using (FileStream stream = File.OpenRead(path))
                    {
                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.StreamSource = stream;
                        image.EndInit();
                    }
                    MasterImage = image;
                }
            }
            else
            {
                MasterImage = null;
            }
        }

        public void CopyMasterImageToMainDisplay()
        {
            Image = MasterImage;
        }

        //다이어로그 세팅
        public IDialogCoordinator DialogCoordinator { get; set; }

        public SettingEngine(CameraCore cameraCore)
        {
            ListStructCarkind = new ObservableCollection<StructCarkind>(StructCarkind.GetCarkind().Cast<StructCarkind>());
            cameraManager = new CameraManager(cameraCore);
        }

        //사각형 표시 버튼
        public bool IsShowRectangle { get { return isShowRectangle; } set { isShowRectangle = value; NotifyPropertyChanged("IsShowRectangle"); } }
        private bool isShowRectangle = false;

        //원 표시 버튼
        public bool IsShowCircle { get { return isShowCircle; } set { isShowCircle = value; NotifyPropertyChanged("IsShowCircle"); } }
        private bool isShowCircle = false;

        //폴더 내용 이미지 선택시
        private ICommand addTemplit;
        public ICommand AddTemplitCommand
        {
            get { return (this.addTemplit) ?? (this.addTemplit = new DelegateCommand(AddTemplit)); }
        }

        public void AddTemplit()
        {
            if (Image != null)
            {
                IsShowRectangle = true;
            }
        }

        //완료 버튼 선택시
        private ICommand confirm;
        public ICommand ConfirmCommand
        {
            get { return (this.confirm) ?? (this.confirm = new DelegateCommand(Confirm)); }
        }

        public void Confirm()
        {
            IsShowRectangle = false;
            int width = (int)SelectRectangle.SelectedWidth;
            int height = (int)SelectRectangle.SelectedHeight;
            int startX = (int)SelectRectangle.StartX;
            int startY = (int)SelectRectangle.StartY;

            CroppedBitmap croppedBitmap = new CroppedBitmap(Image, new System.Windows.Int32Rect(startX, startY, width, height));

            selectedCarkind.Inspections.Cast<StructInspection>().ToList().ForEach(x =>
            {
                if (x.GetToolType() == typeof(HFullContentsTool))
                {
                    HFullContentsTool hFullContents = (HFullContentsTool)x.GetTool();
                    hFullContents.AddTemplit(croppedBitmap);
                }
            });

            string temp = currentFullContentsTemplitPath;
            CurrentFullContentsTemplitPath = "";
            CurrentFullContentsTemplitPath = temp;
        }

        //선택형 사각형
        private HRectangle selectRectangle;
        public HRectangle SelectRectangle
        {
            get { return selectRectangle; }
            set {
                selectRectangle = value;
                NotifyPropertyChanged("SelectRectangle");
            }
        }

        //선택형 원
        private HCircle selectCircle;
        public HCircle SelectCircle
        {
            get { return selectCircle; }
            set
            {
                selectCircle = value;
                NotifyPropertyChanged("SelectCircle");
            }
        }
    }
}
