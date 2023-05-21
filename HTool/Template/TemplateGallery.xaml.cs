using HOVLib;
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
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HTool.Template
{
    /// <summary>
    /// TemplateGallery.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class TemplateGallery : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void NotifyPropertyChanged( String propertyName = "")
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public delegate void OnAddTemplitClickEventHandler();
        public event OnAddTemplitClickEventHandler OnAddTemplitClickEvent = delegate { };

        //추가
        public ICommand AddCommand
        {
            get { return (ICommand)GetValue(AddCommandProperty); }
            set
            {
                if (value != null)
                {

                    SetValue(AddCommandProperty, value);
                }
                else
                {
                    SetValue(AddCommandProperty, value);
                }
            }
        }

        public static readonly DependencyProperty AddCommandProperty = DependencyProperty.Register(
            "AddCommand",
            typeof(ICommand),
            typeof(TemplateGallery));

        //경로
        public string TemplitePath
        {
            get { return (string)GetValue(TemplitePathProperty); }
            set
            {
                if (value != null)
                {

                    SetValue(TemplitePathProperty, value);
                }
                else
                {
                    SetValue(TemplitePathProperty, value);
                }
            }
        }

        public static readonly DependencyProperty TemplitePathProperty = DependencyProperty.Register(
            "TemplitePath",
            typeof(string),
            typeof(TemplateGallery),
            new PropertyMetadata(OnTemplitePathChanged));

        static void OnTemplitePathChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (TemplitePathChanged != null)
            {
                TemplitePathChanged(obj, args);
            }
        }

        public delegate void TemplitePathChangeHandler(object sender, DependencyPropertyChangedEventArgs args);
        public static TemplitePathChangeHandler TemplitePathChanged = delegate { };

        //템플릿 목록
        public ObservableCollection<GalleryItemData> Templits { get { return templits; } set { templits = value; NotifyPropertyChanged("Templits"); } }
        private ObservableCollection<GalleryItemData> templits = new ObservableCollection<GalleryItemData>();

        public GalleryItemData SelectedTemplit { get { return selectedTemplit; } set { selectedTemplit = value; NotifyPropertyChanged("SelectedTemplit"); } }
        private GalleryItemData selectedTemplit = null;


        public TemplateGallery()
        {
            InitializeComponent();

            lb.DataContext = this;

            TemplitePathChanged += delegate
            {
                RefleshTemplit();
            };
        }

        public void RefleshTemplit()
        {
            templits.Clear();

            int i = 0;
            (HTemplateMatching.LoadTemplit(TemplitePath)).OrderBy(x=> new FileInfo(x.Path).CreationTime).ToList().ForEach(x =>
            {
                i++;
                BitmapSource image = ImageConverter.MatToBitmapSource(x.TemplitImage);
                string title = i.ToString();

                GalleryItemData data = new GalleryItemData();
                data.BitmapImage = image;
                data.Path = x.Path;
                data.Title = title;
                data.Match = x;

                templits.Add(data);
            });
        }

        public class GalleryItemData
        {
            public HTemplateMatching.TemplateMatch Match { get; set; }
            public BitmapSource BitmapImage { get; set;}
            public string Title { get; set; }
            public string Path { get; set; }
        }

        private void Btn_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedTemplit != null)
            {
                MessageBoxResult result = MessageBox.Show("해당 템플릿을 제거 하시겠습니까?", "확인", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    HTemplateMatching.RemoveTemplit(SelectedTemplit.Path);
                    RefleshTemplit();
                }
            }
        }

        private void Btn_Add_Click(object sender, RoutedEventArgs e)
        {
            AddCommand.Execute(null);
        }
    }
}
