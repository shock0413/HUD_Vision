using System;
using System.Collections.Generic;
using System.Linq;
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
    /// TemplateGalleryToolTip.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class TemplateGalleryToolTip : UserControl
    {
        public TemplateGalleryToolTip()
        {
            InitializeComponent();
        }

        public BitmapImage BitmapImage
        {
            get { return (BitmapImage)GetValue(BitmapImageProperty); }
            set
            {


                if (value != null)
                {

                    SetValue(BitmapImageProperty, value);
                }
                else
                {
                    SetValue(BitmapImageProperty, value);
                }
            }
        }

        public static readonly DependencyProperty BitmapImageProperty = DependencyProperty.Register(
            "BitmapImage",
            typeof(BitmapImage),
            typeof(TemplateGalleryToolTip),
            new PropertyMetadata(OnBitmapImageChanged));

        static void OnBitmapImageChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (BitmapImageChanged != null)
            {
                BitmapImageChanged(obj);
            }
        }

        public delegate void BitmapImageChangeHandler(object sender);
        public static BitmapImageChangeHandler BitmapImageChanged = delegate { };

    }
}
