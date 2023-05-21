using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using HCore.HDrawPoints;

namespace HCore
{
    public class Result : IHResult, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void NotifyPropertyChanged( string propertyName = "")
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public DrawManager DrawManager { get { return drawManager; } set { drawManager = value; NotifyPropertyChanged("DrawManager"); } }
        private DrawManager drawManager = new DrawManager();
        public HResult.RESULT Results { get { return results; } set { results = value; NotifyPropertyChanged("Results"); } }
        private HResult.RESULT results = new HResult.RESULT();

        public DrawManager GetDrawManager()
        {
            return DrawManager;
        }

        public HResult.RESULT GetResult()
        {
            return Results;
        }
    }
}
