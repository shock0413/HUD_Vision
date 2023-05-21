using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HCore
{
    public class ImageResult : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void NotifyPropertyChanged( String propertyName = "")
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public string Path { get; set; }
        public string Name {
            get
            {
                if (name == null && Path != null && Path.Contains("\\"))
                {
                    return Path.Split('\\')[Path.Split('\\').Length - 1];
                }
                else
                {
                    return name;
                }
            }
            set
            {
                name = value;
            }
        }
        public string name = null;

        public HResult.RESULT Result
        {
            get { return result; }
            set { result = value;
                NotifyPropertyChanged("Result");
                NotifyPropertyChanged("ResultStr");
                NotifyPropertyChanged("RowBackgroundColor");
            }
        }
        private HResult.RESULT result = HResult.RESULT.NONE;

        public string ResultStr
        {
            get {
                if(result == HResult.RESULT.OK)
                {
                    return HResult.RESULT.OK.ToString();
                }
                else if(result == HResult.RESULT.NG)
                {
                    return HResult.RESULT.NG.ToString();
                }
                else
                {
                    return " ";
                }
            }
        }

        public SolidColorBrush RowBackgroundColor {
            get
            {
                if (result == HResult.RESULT.OK)
                {
                    return Brushes.Green;
                }
                else if(result == HResult.RESULT.NG)
                {
                    return Brushes.Red;
                }
                else
                {
                    return Brushes.Transparent;
                }
            }
        }

    }
}
