using EAST_AS_CENTER_HUD.Struct;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EAST_AS_CENTER_HUD.Carinfo
{
    public class CarinfoEngine : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void NotifyPropertyChanged( String propertyName = "")
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public ObservableCollection<StructCarkind> StructCarkinds { get { return structCarkinds; } set { structCarkinds = value; NotifyPropertyChanged("StructCarkinds"); } }
        private ObservableCollection<StructCarkind> structCarkinds = new ObservableCollection<StructCarkind>(StructCarkind.GetCarkind());

        public StructCarkind SelectedCarinfo { get { return selectedCarinfo; } set { selectedCarinfo = value; NotifyPropertyChanged("SelectedCarinfo"); } }
        private StructCarkind selectedCarinfo;

        public CarinfoEngine()
        {
             
        }

        private ICommand saveCarinfo;
        public ICommand SaveCarinfoCommand
        {
            get { return (this.saveCarinfo) ?? (this.saveCarinfo = new DelegateCommand(SaveCarinfo)); }
        }

        public void SaveCarinfo()
        {
            MessageBoxResult result = MessageBox.Show("변경된 내용을 저장하시겠습니까?", "확인", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if(result == MessageBoxResult.Yes)
            {
                StructCarkinds.ToList().ForEach(x =>
                {
                    x.SaveData();
                });
            }
        }

        private ICommand reloadCarinfo;
        public ICommand ReloadCarinfoCommand
        {
            get { return (this.reloadCarinfo) ?? (this.reloadCarinfo = new DelegateCommand(ReloadCarinfo)); }
        }

        public void ReloadCarinfo()
        {
            StructCarkinds = new ObservableCollection<StructCarkind>(StructCarkind.GetCarkind());
        }

        private ICommand addCarinfo;
        public ICommand AddCarinfoCommand
        {
            get { return (this.addCarinfo) ?? (this.addCarinfo = new DelegateCommand(AddCarinfo)); }
        }

        public void AddCarinfo()
        {
            if(StructCarkinds.Where(x=>x.Name == "new").ToList().Count == 0)
            {
                StructCarkind carkind = new StructCarkind("new");
                carkind.SaveData();
                StructCarkinds.Add(carkind);
            }
        }

        private ICommand removeCarinfo;
        public ICommand RemoveCarinfoCommand
        {
            get { return (this.removeCarinfo) ?? (this.removeCarinfo = new DelegateCommand(RemoveCarinfo)); }
        }

        public void RemoveCarinfo()
        {
            if(SelectedCarinfo != null)
            {
                MessageBoxResult result = MessageBox.Show("\"" + SelectedCarinfo.ToString() + "\" 기종을 제거 하시겠습니까?", "확인", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if(result == MessageBoxResult.Yes)
                {
                    selectedCarinfo.Delete();
                    StructCarkinds.Remove(SelectedCarinfo);
                }
            }
        }


        private ICommand loadCSV;
        public ICommand LoadCSVCommand
        {
            get { return (this.loadCSV) ?? (this.loadCSV = new DelegateCommand(LoadCSV)); }
        }

        public void LoadCSV()
        {
            System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.Filter = "*.csv|*.csv";
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if(result == System.Windows.Forms.DialogResult.OK)
            {
                StreamReader sr = new StreamReader(dialog.FileName, Encoding.GetEncoding("euc-kr"));
                string s = sr.ReadLine();
                while (!sr.EndOfStream)
                {
                    s = sr.ReadLine();
                    string[] temp = s.Split(',');
                }
            }
        }
    }
}
