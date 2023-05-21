using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

namespace HControl.ChatControl
{
    /// <summary>
    /// HChatControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class HChatControl : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void NotifyPropertyChanged( String propertyName = "")
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }


        //좌측 타이틀
        public string LeftTitle
        {
            get { return (string)GetValue(LeftTitleProperty); }
            set
            {
                if (value != null)
                {

                    SetValue(LeftTitleProperty, value);
                }
                else
                {
                    SetValue(LeftTitleProperty, value);
                }
            }
        }

        public static readonly DependencyProperty LeftTitleProperty = DependencyProperty.Register(
            "LeftTitle",
            typeof(string),
            typeof(HChatControl),
            new PropertyMetadata(OnLeftTitleChanged));

        static void OnLeftTitleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (LeftTitleChanged != null)
            {
                LeftTitleChanged(obj);
            }
        }

        public delegate void LeftTitleChangeHandler(object sender);
        public static LeftTitleChangeHandler LeftTitleChanged = delegate { };

        //우측 타이틀
        public string RightTitle
        {
            get { return (string)GetValue(RightTitleProperty); }
            set
            {


                if (value != null)
                {

                    SetValue(RightTitleProperty, value);
                }
                else
                {
                    SetValue(RightTitleProperty, value);
                }
            }
        }

        public static readonly DependencyProperty RightTitleProperty = DependencyProperty.Register(
            "RightTitle",
            typeof(string),
            typeof(HChatControl),
            new PropertyMetadata(OnRightTitleChanged));

        static void OnRightTitleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (RightTitleChanged != null)
            {
                RightTitleChanged(obj);
            }
        }

        public delegate void RightTitleChangeHandler(object sender);
        public static RightTitleChangeHandler RightTitleChanged = delegate { };

      

        //현재 위치
        public int SelectedPosition
        {
            get { return (int)GetValue(SelectedPositionProperty); }
            set
            {
                SetValue(SelectedPositionProperty, value);
            }
        }

        public static readonly DependencyProperty SelectedPositionProperty = DependencyProperty.Register(
            "SelectedPosition",
            typeof(int),
            typeof(HChatControl),
            new PropertyMetadata(OnSelectedPositionChanged));

        static void OnSelectedPositionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (SelectedPositionChanged != null)
            {
                SelectedPositionChanged(obj, args);
            }
        }

        public delegate void SelectedPositionChangeHandler(object sender, DependencyPropertyChangedEventArgs args);
        public static SelectedPositionChangeHandler SelectedPositionChanged = delegate { };

        //메시지
        public ObservableCollection<StructChatMessage> Items
        {
            get { return (ObservableCollection<StructChatMessage>)GetValue(ItemsProperty); }
            set
            {

                if (value != null)
                {

                    SetValue(ItemsProperty, value);
                }
                else
                {
                    SetValue(ItemsProperty, value);
                }
            }
        }

        public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register(
            "Items",
            typeof(ObservableCollection<StructChatMessage>),
            typeof(HChatControl),
            new PropertyMetadata(OnItemsChanged));

        static void OnItemsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (ItemsChanged != null)
            {
                ItemsChanged(obj);
            }
        }

        public delegate void ItemsChangeHandler(object sender);
        public static ItemsChangeHandler ItemsChanged = delegate { };

        //내부 리스트 박스 
        public int ListBoxSelectedIndex { get { return listBoxSelectedIndex; } set { listBoxSelectedIndex = value; NotifyPropertyChanged("ListBoxSelectedIndex");  } }
        private int listBoxSelectedIndex = 0;

        //생성자
        public HChatControl()
        {
            InitializeComponent();

            tb_LeftTitle.DataContext = this;
            tb_RightTitle.DataContext = this;
            lb.DataContext = this;

            SelectedPositionChanged += new SelectedPositionChangeHandler((sender, args) =>
            {
                lb.SelectedIndex = (int)args.NewValue;
                lb.ScrollIntoView(lb.Items[lb.Items.Count - 1]);
            });
        }
    }
}
