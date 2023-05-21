using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace HCore
{
    public class StructInspectionInfo : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void NotifyPropertyChanged( String propertyName = "")
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public string CarkindFullName {
            get {
                if(string.IsNullOrEmpty(subCarkind))
                {
                    return carkind;
                }
                else
                {   
                    return carkind + " " + subCarkind;
                }
            }
        }

        public string Carkind { get { return carkind; } set { carkind = value; NotifyPropertyChanged("Carkind"); NotifyPropertyChanged("CarkindFullName"); } }
        private string carkind;

        public string SubCarkind { get { return subCarkind; } set { subCarkind = value; NotifyPropertyChanged("SubCarkind"); NotifyPropertyChanged("CarkindFullName"); } }
        private string subCarkind;

        public string Info1 { get { return info1; } set { info1 = value; NotifyPropertyChanged("Info1"); } }
        private string info1;

        public string Info2 { get { return info2; } set { info2 = value; NotifyPropertyChanged("Info2"); } }
        private string info2;

        public string Direction { get; set; }

        public string InspectionTimeStr
        {
            get
            {
                if (inspectionTime == null)
                {
                    return "-";
                }
                else
                {
                    return inspectionTime.Value.ToString("yyyy-MM-dd HH:mm:ss");
                }
            }
        }
        public DateTime? InspectionTime { get { return inspectionTime; } set { inspectionTime = value; NotifyPropertyChanged("InspectionTime"); NotifyPropertyChanged("InspectionTimeStr"); } }
        private DateTime? inspectionTime = null;

        public bool IsNotRegistedCarkind { get { return isNotRegistedCarkind; } set { isNotRegistedCarkind = value; } }
        private bool isNotRegistedCarkind = false;

        public int InspectionCountCutoff { get; set; }
        public int InspectionCountDistortion { get; set; }
        public int InspectionCountCenter { get; set; }
        public int InspectionCountFullContent { get; set; }


        public int CaptureCountCutoff { get; set; }
        public int CaptureCountDistortion { get; set; }
        public int CaptureCountFullContent { get; set; }

        public int InspectionCaptureCountDistortion { get; set; }

        public long DBResultIndex { get; set; }

        public List<System.Windows.Point> DistortionBeforeMoveData { get; set; }
        public double? DistortionBeforeVerticalInterval { get; set; }
        public double? DistortionBeforeHorizentalInterval { get; set; }
    }
}
