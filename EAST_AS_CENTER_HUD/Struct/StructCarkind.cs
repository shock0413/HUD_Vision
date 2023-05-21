using EAST_AS_CENTER_HUD.Camera;
using HCore;
using HHUDTool;
using HTool;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Utill;

namespace EAST_AS_CENTER_HUD.Struct
{
    public class StructCarkind : StructCarkindPart, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void NotifyPropertyChanged( String propertyName = "")
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public List<StructCamera> Cameras { get { return cameras; } set { cameras = value; } }
        private List<StructCamera> cameras;

        public List<StructInspection> Inspections { get { return inspections; } set { inspections = value; } }
        private List<StructInspection> inspections;

        public double HudWidth { get { return hudWidth; } set { hudWidth = value; NotifyPropertyChanged("HudWidth"); IsChangedValue = true; } }
        public double hudWidth;
        public double HudHeight { get { return hudHeight; } set { hudHeight = value; NotifyPropertyChanged("HudHeight"); IsChangedValue = true; } }
        public double hudHeight;
        public double MMPerPixel { get { return mmPerPixel; } set { mmPerPixel = value; NotifyPropertyChanged("MMPerPixel"); IsChangedValue = true; } }
        public double mmPerPixel;
        public double DotHorizentalCount { get { return dotHorizentalCount; } set { dotHorizentalCount = value; NotifyPropertyChanged("DotHorizentalCount"); IsChangedValue = true; } }
        public double dotHorizentalCount;
        public double DotVerticalCount { get { return dotVerticalCount; } set { dotVerticalCount = value; NotifyPropertyChanged("DotVerticalCount"); IsChangedValue = true; } }
        public double dotVerticalCount;
        public double DotHorizentalInterval { get { return dotHorizentalInterval; } set { dotHorizentalInterval = value; NotifyPropertyChanged("DotHorizentalInterval"); IsChangedValue = true; } }
        public double dotHorizentalInterval;
        public double DotVerticalInterval { get { return dotVerticalInterval; } set { dotVerticalInterval = value; NotifyPropertyChanged("DotVerticalInterval"); IsChangedValue = true; } }
        public double dotVerticalInterval;

        public bool IsChangedValue { get { return isChangedValue; } set { isChangedValue = value; NotifyPropertyChanged("IsChangedValue"); } }
        public bool isChangedValue;

        public bool IsRotateCamera { get { return isRotateCamera; } set { isRotateCamera = value; NotifyPropertyChanged("IsRotateCamera"); } }
        private bool isRotateCamera = false;


        public StructCarkind(string name) : base(name)
        {
            GetInspection();
            GetCamera();
            LoadData();
        }

        public new void LoadData()
        {
            base.LoadData();

            HudWidth = GetHudWidth();
            HudHeight = GetHudHeight();
            MMPerPixel = GetHudMMPerPixel();
            DotHorizentalCount = GetHudDotHorizentalCount();
            DotVerticalCount = GetHudDotVerticalCount();
            DotHorizentalInterval = GetHudDotHorizentalInterval();
            DotVerticalInterval = GetHudDotVerticalInterval();
            IsRotateCamera = GetIsRotateCamera();
        }

        public new void SaveData()
        {
            base.SaveData();

            if (isNewCarkind)
            {
                CreateInspection(1, "잘림검사", "Cutoff");
                CreateInspection(2, "왜곡검사", "Distortion");
                CreateInspection(3, "중심검사", "Center");
                CreateInspection(4, "풀컨텐츠", "FullContents");
            }
            else
            {
                SetHudWidth(HudWidth);
                SetHudHeight(HudHeight);
                SetHudMMPerPixel(MMPerPixel);
                SetHudDotHorizentalCount(DotHorizentalCount);
                SetHudDotVerticalCount(DotVerticalCount);
                SetHudDotHorizentalInterval(DotHorizentalInterval);
                SetHudDotVerticalInterval(DotVerticalInterval);
                SetRotateCamera(IsRotateCamera);

                if (cutoffTool != null)
                {
                    cutoffTool.SaveBrightLimit(CutoffToolParams.BrightLimit);
                    cutoffTool.SaveMinBlobCount(CutoffToolParams.MinBlobCount);
                    cutoffTool.SaveMaxBlobCount(CutoffToolParams.MaxBlobCount);
                    cutoffTool.SavePassRangeBottom(CutoffToolParams.PassRangeBottom);
                    cutoffTool.SavePassRangeTop(CutoffToolParams.PassRangeTop);
                    cutoffTool.SaveTransmissionFactor(CutoffToolParams.TransmissionFactor);
                }
            }

            isNewCarkind = false;
        }

        public static List<StructCarkind> GetCarkind()
        {
            string dir = System.IO.Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory) + "\\Carkind\\";

            if (Directory.Exists(dir) == false)
            {
                Directory.CreateDirectory(dir);
            }

            List<StructCarkind> list = new List<StructCarkind>();

            string[] paths = Directory.GetFiles(System.IO.Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName) + "\\Carkind\\").Where(x => x.EndsWith(".ini")).ToArray();
            if (paths != null)
            {
                paths.ToList().ForEach(x =>
                {
                    list.Add(new StructCarkind(x.Split('\\')[x.Split('\\').Length - 1].Replace(".ini", "")));
                });
            }

            return list;
        }


        private void GetInspection()
        {
            if (isNewCarkind)
            {
                SaveData();
            }

            List<StructInspection> list = new List<StructInspection>();
            List<KeyValuePair<string, string>> pairs = IniFileCarkind.GetSectionValuesAsList("Inspection");
            pairs.ForEach(x =>
            {
                StructInspection inspection = new StructInspection(this, x.Value, Convert.ToInt32(x.Key));
                list.Add(inspection);
            });

            Inspections = list;
        }

        public HCutoffTool CutOffTool {
            get
            {
                if (cutoffTool == null)
                {
                    cutoffTool = (HCutoffTool)(Inspections.Where(x => x.GetToolType() == typeof(HCutoffTool)).ToArray()[0].GetTool());
                }

                return cutoffTool;
            }
        }
        public HDistortionTool DistortionTool {
            get
            {
                if(distortionTool == null)
                {
                    distortionTool = (HDistortionTool)(Inspections.Where(x => x.GetToolType() == typeof(HDistortionTool)).ToArray()[0].GetTool());
                    
                }
                return distortionTool;
            }
        }
        public HCenterTool CenterTool
        {
            get
            {
                if (centerTool == null)
                {
                    centerTool = (HCenterTool)(Inspections.Where(x => x.GetToolType() == typeof(HCenterTool)).ToArray()[0].GetTool());

                }
                return centerTool;
            }
        }
        public HFullContentsTool FullContentsTool {
            get
            {
                if (fullContentsTool == null)
                {
                    fullContentsTool = (HFullContentsTool)(Inspections.Where(x => x.GetToolType() == typeof(HFullContentsTool)).ToArray()[0].GetTool());
                }
                return fullContentsTool;
            }
        }

        private HCutoffTool cutoffTool;
        private HDistortionTool distortionTool;
        private HCenterTool centerTool;
        private HFullContentsTool fullContentsTool;

        public HCutoffParams CutoffToolParams {
            get
            {
                return CutOffTool.RunParams;
            }
        }

        public HDistortionParams DistortionToolParams
        {
            get
            {
                return DistortionTool.RunParams;
            }
        }

        public HCenterParams CenterToolParams
        {
            get
            {
                return CenterTool.RunParams;
            }
        }

        public HFullContentsParams FullContentsToolParams
        {
            get
            {
                return FullContentsTool.RunParams;
            }
        }

        private void GetCamera()
        {
            List<StructCamera> list = new List<StructCamera>();
            Inspections.ForEach(x =>
            {
                string cameraIndex = IniFileCarkind.GetString(x.Name, "Camera Index", "");
                int gain = IniFileCarkind.GetInt32(x.Name, "Camera Gain", 0);
                int exposure = IniFileCarkind.GetInt32(x.Name, "Camera Exposure", 0);

                string sn = "";
                IniFileConfig.GetSectionValuesAsList("Camera Index").ForEach(y =>
                {
                    if (y.Key == cameraIndex)
                    {
                        sn = y.Value;
                    }
                });

                StructCamera structCamera = new StructCamera(sn, gain, exposure);
                ((StructInspection)x).StructCamera = structCamera;
            });

            Cameras = list;
        }


        void CreateInspection(int pos, string inspectionName, string toolType)
        {
            IniFileCarkind.WriteValue("Inspection", pos.ToString(), inspectionName);
            IniFileCarkind.WriteValue("ToolType", pos.ToString(), toolType);
            IniFileCarkind.WriteValue(inspectionName, "Camera Index", "1");
            IniFileCarkind.WriteValue(inspectionName, "Camera Gain", 0);
            IniFileCarkind.WriteValue(inspectionName, "Camera Exposure", 35000.0);


            string toolPath = HCore.IniManager.GetToolPath(toolType);

            if(Directory.Exists(toolPath) == false)
            {
                Directory.CreateDirectory(toolPath);
            }

            int max = 1;
            Directory.GetFiles(toolPath).ToList().ForEach(x =>
            {
                if (x.EndsWith(".ini"))
                {
                    try
                    {
                        string fileName = x.Split('\\')[x.Split('\\').Length - 1];

                        int currentValue = Convert.ToInt32(fileName.Replace(".ini", ""));
                        if (max < currentValue)
                        {
                            max = currentValue;
                        }
                    }
                    catch
                    {

                    }
                }
            });

            max++;

            IniFileCarkind.WriteValue("Item", pos.ToString(), max.ToString());
            IniFile iniFile = new IniFile("Tool" + "\\" + toolType + "\\" + max + ".ini");
            iniFile.WriteValue("Info", "Name", inspectionName);
        }

        internal double GetHudWidth()
        {
            return IniFileCarkind.GetDouble("HUD Spec", "Width", 176);
        }

        internal double GetHudHeight()
        {
            return IniFileCarkind.GetDouble("HUD Spec", "Height", 65);
        }

        internal double GetHudMMPerPixel()
        {
            return IniFileCarkind.GetDouble("HUD Spec", "mmPerPiexel", 0.225);
        }

        internal double GetHudDotHorizentalCount()
        {
            return IniFileCarkind.GetDouble("HUD Spec", "Dot Horizental Count", 15);
        }

        internal double GetHudDotVerticalCount()
        {
            return IniFileCarkind.GetDouble("HUD Spec", "Dot Vertical Count", 7);
        }

        internal double GetHudDotHorizentalInterval()
        {
            return IniFileCarkind.GetDouble("HUD Spec", "Dot Horizental Interval", 0.005);
        }

        internal double GetHudDotVerticalInterval()
        {
            return IniFileCarkind.GetDouble("HUD Spec", "Dot Vertical Interval", 0.005);
        }

        public string Direction { get { return IniFileCarkind.GetString("Info", "SubCarkind", "Denso"); } }

        internal bool GetIsRotateCamera()
        {
            return IniFileCarkind.GetBoolian("Camera", "Rotate", false);
        }

        public bool IsXMinus { get { return IniFileCarkind.GetString("Reverse", "IsDistortionXMinus", "FALSE").ToUpper() == "TRUE"; } }
        public bool IsYMinus { get { return IniFileCarkind.GetString("Reverse", "IsDistortionYMinus", "FALSE").ToUpper() == "TRUE"; } }

        internal void SetRotateCamera(bool value)
        {
            IniFileCarkind.WriteValue("Camera", "Rotate", value);
        }

        internal void SetHudWidth(double value)
        {
            IniFileCarkind.WriteValue("HUD Spec", "Width", value);
        }

        internal void SetHudHeight(double value)
        {
            IniFileCarkind.WriteValue("HUD Spec", "Height", value);
        }

        internal void SetHudMMPerPixel(double value)
        {
            IniFileCarkind.WriteValue("HUD Spec", "mmPerPiexel", value);
        }

        internal void SetHudDotHorizentalCount(double value)
        {
            IniFileCarkind.WriteValue("HUD Spec", "Dot Horizental Count", value);
        }

        internal void SetHudDotVerticalCount(double value)
        {
            IniFileCarkind.WriteValue("HUD Spec", "Dot Vertical Count", value);
        }

        internal void SetHudDotHorizentalInterval(double value)
        {
            IniFileCarkind.WriteValue("HUD Spec", "Dot Horizental Interval", value);
        }

        internal void SetHudDotVerticalInterval(double value)
        {
            IniFileCarkind.WriteValue("HUD Spec", "Dot Vertical Interval", value);
        }
    }
}
