﻿
using HanseroDisplay.Struct;
using HCore;
using HCore.HDrawPoints;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HanseroDisplay
{
    /// <summary>
    /// HDiaplay.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class HDisplay : System.Windows.Controls.UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void NotifyPropertyChanged( String propertyName = "")
        {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public Visibility ConfirmButtonVisibility{ get { return confirmButtonVisibility; } set { confirmButtonVisibility = value;  NotifyPropertyChanged("ConfirmButtonVisibility"); } }
        private Visibility confirmButtonVisibility = Visibility.Collapsed;

        public BitmapSource BitmapImage
        {
            get { return (BitmapSource)GetValue(BitmapImageProperty); }
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
                RemoveSelectRectangle();
                this.Result = null;
                cv.Bitmap = BitmapImage;

                if (IsAutoFit)
                {
                    cv.Fit();
                }
            }
        }

        public static readonly DependencyProperty BitmapImageProperty = DependencyProperty.Register(
            "BitmapImage",
            typeof(BitmapSource),
            typeof(HDisplay),
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

        public delegate void OnDoubleClickEventHandler();
        public event OnDoubleClickEventHandler OnDoubleClickEvent = delegate { };

        //Rectangle
        public ObservableCollection<StructRectangle> Rectangles
        {
            get { return (ObservableCollection<StructRectangle>)GetValue(RectanglesProperty); }
            set
            {
                if (value != null)
                {
                    cv.ListRectangle.Clear();
                    value.CollectionChanged += Rectangle_CollectionChanged;
                    SetValue(RectanglesProperty, value);
                }
                
                cv.InvalidateVisual();
            }
        }

        private void Rectangle_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if(e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach(StructRectangle obj in e.NewItems)
                {
                    cv.ListRectangle.Add(obj);
                }
                cv.InvalidateVisual();
            }
            else if(e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (StructRectangle obj in e.NewItems)
                {
                    cv.ListRectangle.Remove(obj);
                }
                cv.InvalidateVisual();
            }
            else if(e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                cv.ListRectangle.Clear();
                cv.InvalidateVisual();
            }
        }

        public static readonly DependencyProperty RectanglesProperty = DependencyProperty.Register(
            "Rectangles",
            typeof(ObservableCollection<StructRectangle>),
            typeof(HDisplay),
            new PropertyMetadata(OnRectanglesChanged));

        static void OnRectanglesChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (RectanglesChanged != null)
            {
                RectanglesChanged(obj);
            }
        }

        public delegate void RectanglesChangeHandler(object sender);
        public static RectanglesChangeHandler RectanglesChanged = delegate { };





        public ObservableCollection<StructLine> Lines
        {
            get { return (ObservableCollection<StructLine>)GetValue(LinesProperty); }
            set
            {
                if (value != null)
                {
                    cv.ListLine.Clear();
                    value.CollectionChanged += Lines_CollectionChanged;
                    SetValue(LinesProperty, value);
                }

                cv.InvalidateVisual();
            }
        }


        private void Lines_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (StructLine obj in e.NewItems)
                {
                    cv.ListLine.Add(obj);
                }
                cv.InvalidateVisual();
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (StructLine obj in e.NewItems)
                {
                    cv.ListLine.Remove(obj);
                }
                cv.InvalidateVisual();
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                cv.ListLine.Clear();
                cv.InvalidateVisual();
            }
        }

        public static readonly DependencyProperty LinesProperty = DependencyProperty.Register(
            "Lines",
            typeof(ObservableCollection<StructLine>),
            typeof(HDisplay),
            new PropertyMetadata(OnLinesChanged));

        static void OnLinesChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (LinesChanged != null)
            {
                LinesChanged(obj);
            }
        }

        public delegate void LinesChangeHandler(object sender);
        public static LinesChangeHandler LinesChanged = delegate { };


        public bool IsAutoFit
        {
            get { return (bool)GetValue(IsAutoFitProperty); }
            set
            {
                if (value != null)
                {
                    SetValue(IsAutoFitProperty, value);
                }
            }
        }


        private void IsAutoFit_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
         
        }

        public static readonly DependencyProperty IsAutoFitProperty = DependencyProperty.Register(
            "IsAutoFit",
            typeof(bool),
            typeof(HDisplay),
            new PropertyMetadata(true, OnIsAutoFitChanged));

        static void OnIsAutoFitChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (IsAutoFitChanged != null)
            {
                IsAutoFitChanged(obj, args);
            }
        }

        public delegate void IsAutoFitChangeHandler(object sender, DependencyPropertyChangedEventArgs args);
        public static IsAutoFitChangeHandler IsAutoFitChanged = delegate { };

        public void DrawDrawPoints()
        {
            cv.ListPoint.Clear();
            cv.ListLabel.Clear();
            cv.ListLine.Clear();
            cv.ListRectangle.Clear();

            cv.InvalidateVisual();

            if (Result != null)
            {
                DrawManager drawManager = Result.GetDrawManager();
                if (drawManager != null)
                {
                    drawManager.DrawPoints.ToList().ForEach(x =>
                    {
                        DrawPoint("", new System.Drawing.Point((int)x.X, (int)x.Y), x.Size, x.StrokeColor, x.ToolTip);
                    });

                    drawManager.DrawLabels.ToList().ForEach(x =>
                    {
                        DrawLabel(
                                "",
                                new System.Drawing.Point((int)x.X, (int)x.Y),
                                new FormattedText(x.Text, CultureInfo.CurrentCulture, System.Windows.FlowDirection.LeftToRight, new Typeface("돋움"), x.Size, x.Foreground),
                                x.TextAlign);
                    });

                    drawManager.DrawLines.ToList().ForEach(x =>
                    {
                        DrawLine("",
                            new Pen(x.StrokeColor, x.Size),
                            new System.Drawing.Point((int)x.StartX, (int)x.StartY),
                            new System.Drawing.Point((int)x.EndX, (int)x.EndY));
                    });

                    drawManager.DrawCross.ToList().ForEach(x =>
                    {
                        DrawLine("",
                              new Pen(x.StrokeColor, x.Size),
                              new System.Drawing.Point((int)(x.X - x.Size*3), (int)(x.Y)),
                              new System.Drawing.Point((int)(x.X + x.Size*3), (int)(x.Y)));

                        DrawLine("",
                            new Pen(x.StrokeColor, x.Size),
                            new System.Drawing.Point((int)x.X, (int)(x.Y - x.Size*3)),
                            new System.Drawing.Point((int)x.X, (int)(x.Y + x.Size*3)));
                    });

                    drawManager.DrawRectangle.ToList().ForEach(x =>
                    {
                        DrawRectangle("",
                            x.Fill,
                            new Pen(x.StrokeColor, x.Size),
                            x.CenterX,
                            x.CenterY,
                            x.Height,
                            x.Width
                            );
                    });
                }
            }
        }

        public HCanvas canvas { get { return cv; } }

        public HDisplay()
        {
            InitializeComponent();

            sp.DataContext = this;

            BitmapImageChanged += new BitmapImageChangeHandler((object sender) =>
            {
                if (this == ((HDisplay)sender))
                {
                    BitmapImage = this.BitmapImage;
                }
            });

            ResultChanged += new ResultChangeHandler((object sender) =>
            {
                if (this == ((HDisplay)sender))
                {
                    Result = this.Result;
                }
            });

            RectanglesChanged += new RectanglesChangeHandler((object sender) =>
            {
                if (this == ((HDisplay)sender))
                {
                    Rectangles = this.Rectangles;
                }
            });

            LinesChanged += new LinesChangeHandler((object sender) =>
            {
                if (this == ((HDisplay)sender))
                {
                    Lines = this.Lines;
                }
            });

            IsShowRectangleChanged += new IsShowRectangleChangeHandler((sender, args) =>
            {
                if (sender == this)
                {
                    if ((bool)args.NewValue)
                    {
                        ConfirmButtonVisibility = Visibility.Visible;
                        ShowRectangle();
                    }
                    else if (!(bool)args.NewValue)
                    {
                        ConfirmButtonVisibility = Visibility.Collapsed;
                        RemoveSelectRectangle();
                    }
                }
            });

            IsShowCircleChanged += new IsShowCircleChangeHandler((sender, args) =>
            {
                if (sender == this)
                {
                    if ((bool)args.NewValue)
                    {
                        ShowCircle();
                    }
                    else
                    {
                        RemoveSelectRectangle();
                    }
                }
            });

            IsAutoFitChanged += new IsAutoFitChangeHandler((sender, args) =>
            {
                if(sender == this)
                {
                    IsAutoFit = (bool)args.NewValue;
                }
            });


            /*
            ConfirmedChanged += new ConfirmedHandler((sender, args) =>
            {
                if(sender == this)
                {
                    RemoveSelectRectangle();
                    ConfirmButtonVisibility = Visibility.Collapsed;
                }
            });*/
        }

        public void RemoveSelectRectangle()
        {
            cv.RemoveSelectRectangle();
        }

        public void RemoveSelectCircle()
        {
            cv.RemoveSelectCircle();
        }

        public void LoadImagePath(string path)
        {
            try
            {
                BitmapImage = new BitmapImage(new Uri(path));
            }
            catch (Exception e)
            {

            }
        }

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //cv.Fit();
        }

       
        public HRectangle ShowRectangle()
        {
            cv.RemoveSelectRectangle();
            if(SelectRectangle == null)
            {
                int width = (int)BitmapImage.PixelWidth / 5;
                int height = (int)BitmapImage.PixelHeight / 5;
                int x = (int)((BitmapImage.PixelWidth / 2) - (width / 2)) * -1;
                int y = (int)((BitmapImage.PixelHeight / 2) - (height / 2)) * -1;
                SelectRectangle = new HRectangle(width, height, x, y , cv);
            }

            cv.Children.Add(SelectRectangle);

            return SelectRectangle;
        }

        public HRectangle ShowRectangle(int width, int height, int x, int y)
        {
            cv.RemoveSelectRectangle();
            HRectangle rec = new HRectangle(width, height, x * -1, y * -1, cv);

            cv.Children.Add(rec);

            return rec;
        }

        public HCircle ShowCircle()
        {
            cv.RemoveSelectRectangle();
            if (SelectCircle == null)
            {
                int width = (int)BitmapImage.PixelWidth / 5;
                int height = width;
                int x = (int)((BitmapImage.PixelWidth / 2) - (width / 2)) * -1;
                int y = (int)((BitmapImage.PixelHeight / 2) - (height / 2)) * -1;
                SelectCircle = new HCircle(width, height, x, y, cv);
            }

            cv.Children.Add(SelectCircle);

            return SelectCircle;
        }

        public void ShowOK()
        {
            cv.RemoveSelectRectangle();

            TextBlock textBlock = new TextBlock();
            textBlock.Background = Brushes.Black;
            textBlock.FontSize = this.ActualHeight / 10;
            textBlock.Foreground = Brushes.LightGreen;
            textBlock.Text = "OK";
            textBlock.TextAlignment = TextAlignment.Center;

            cv.Children.Add(textBlock);

            textBlock.Loaded += (s, e) =>
            {
                textBlock.Margin = new Thickness(cv.ActualWidth - textBlock.ActualWidth, 0, 0, 0);
            };
        }


        public void ShowNG()
        {
            cv.RemoveSelectRectangle();

            TextBlock textBlock = new TextBlock();
            textBlock.Background = Brushes.Black;
            textBlock.FontSize = this.ActualHeight / 10;
            textBlock.Foreground = Brushes.Red;
            textBlock.Text = "NG";
            textBlock.TextAlignment = TextAlignment.Center;

            cv.Children.Add(textBlock);

            textBlock.Loaded += (s, e) =>
            {
                textBlock.Margin = new Thickness(cv.ActualWidth - textBlock.ActualWidth, 0, 0, 0);
            };
        }

        public void ClearComment()
        {
            cv.sp_Comment.Children.Clear();
        }

        public void ShowComment(string str, int left, int top, int right, int bottom)
        {
            TextBlock textBlock = new TextBlock();
            textBlock.Background = Brushes.Black;
            textBlock.FontSize = this.ActualHeight / 10;
            textBlock.Foreground = Brushes.White;
            textBlock.Text = str;
            textBlock.TextAlignment = TextAlignment.Center;

            cv.Children.Add(textBlock);
        }

        public void DrawPoint(string tag, System.Drawing.Point realPosition, Ellipse ellipse)
        {
            canvas.ListPoint.Add(new StructEllipse() { ellipse = ellipse, RealPosition = realPosition });
            canvas.InvalidateVisual();
        }

        public void DrawPoint(string tag, System.Drawing.Point realPosition, double size, Brush brush, string toolTip)
        {
            Ellipse ellipse = new Ellipse();
            ellipse.ToolTip = toolTip;
            ellipse.Height = size;
            ellipse.Width = size;
            ellipse.Stroke = brush;
            ellipse.StrokeThickness = 1;
            canvas.ListPoint.Add(new StructEllipse() { ellipse = ellipse, RealPosition = realPosition });
            canvas.InvalidateVisual();
        }

        public void DrawLabel(string tag, System.Drawing.Point realPosition, FormattedText text, DrawLabel.DrawLabelAlign align)
        {
            canvas.ListLabel.Add(new StructLabel() { Tag = tag, RealPosition = realPosition, Text = text, DrawLabelAlign = align });
            canvas.InvalidateVisual();
        }

        public void DrawLine(string tag, System.Windows.Media.Pen pen, System.Drawing.Point point1, System.Drawing.Point point2)
        {
            canvas.ListLine.Add(new StructLine() { Tag = tag, Point1 = point1, Point2 = point2, Pen = pen });
            canvas.InvalidateVisual();
        }

        public void DrawRectangle(string tag, SolidColorBrush brush, System.Windows.Media.Pen pen, double centerX, double centerY, double height, double width)
        {
            canvas.ListRectangle.Add(new StructRectangle() { Tag = tag, Brush = brush, Pen = pen, CenterX = centerX, CenterY = centerY, Height = height, Width = width });
            canvas.InvalidateVisual();
        }

        public void SaveImage(string filePath)
        {
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(BitmapImage));

            using (var fileStream = new System.IO.FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                encoder.Save(fileStream);
            }
        }

        private void Cv_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.ClickCount == 2)
            {
                OnDoubleClickEvent();
            }
        }

        private void Btn_Confirm_Click(object sender, RoutedEventArgs e)
        {
            Confirmed.Execute(null);
        }

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            IsShowRectangle= false;
            ConfirmButtonVisibility = Visibility.Collapsed;
        }


        //Result
        public IHResult Result
        {
            get { return (IHResult)GetValue(ResultProperty); }
            set
            {
                SetValue(ResultProperty, value);

                DrawDrawPoints();
            }
        }
        public static readonly DependencyProperty ResultProperty = DependencyProperty.Register(
            "Result",
            typeof(IHResult),
            typeof(HDisplay),
            new PropertyMetadata(OnResultChanged));

        static void OnResultChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (ResultChanged != null)
            {
                ResultChanged(obj);
            }
        }


        public delegate void ResultChangeHandler(object sender);
        public static ResultChangeHandler ResultChanged = delegate { };

        //사각형 표시
        public bool IsShowRectangle
        {
            get { return (bool)GetValue(IsShowRectangleProperty); }
            set
            {
                SetValue(IsShowRectangleProperty, value);
            }
        }

        public static readonly DependencyProperty IsShowRectangleProperty = DependencyProperty.Register(
            "IsShowRectangle",
            typeof(bool),
            typeof(HDisplay),
            new PropertyMetadata(OnIsShowRectangleChanged)
            );

        static void OnIsShowRectangleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (IsShowRectangleChanged != null)
            {
                IsShowRectangleChanged(obj, args);
            }
        }

        public delegate void IsShowRectangleChangeHandler(object sender, DependencyPropertyChangedEventArgs args);
        public static IsShowRectangleChangeHandler IsShowRectangleChanged = delegate { };

        //원 표시
        public bool IsShowCircle
        {
            get { return (bool)GetValue(IsShowCircleProperty); }
            set
            {
                SetValue(IsShowCircleProperty, value);
            }
        }

        public static readonly DependencyProperty IsShowCircleProperty = DependencyProperty.Register(
            "IsShowCircle",
            typeof(bool),
            typeof(HDisplay),
            new PropertyMetadata(OnIsShowCircleChanged)
            );

        static void OnIsShowCircleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (IsShowCircleChanged != null)
            {
                IsShowCircleChanged(obj, args);
            }
        }

        public delegate void IsShowCircleChangeHandler(object sender, DependencyPropertyChangedEventArgs args);
        public static IsShowCircleChangeHandler IsShowCircleChanged = delegate { };

        //Confirm

        public ICommand Confirmed
        {
            get { return (ICommand)GetValue(ConfirmedProperty); }
            set
            {
                SetValue(ConfirmedProperty, value);
            }
        }

        public static readonly DependencyProperty ConfirmedProperty = DependencyProperty.Register(
            "Confirmed",
            typeof(ICommand),
            typeof(HDisplay),
            new PropertyMetadata(OnConfirmedChanged)
            );

        static void OnConfirmedChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (ConfirmedChanged != null)
            {
                ConfirmedChanged(obj, args);
            }
        }

        public delegate void ConfirmedHandler(object sender, DependencyPropertyChangedEventArgs args);
        public static ConfirmedHandler ConfirmedChanged = delegate { };
        //
        //

        public HRectangle SelectRectangle
        {
            get { return (HRectangle)GetValue(SelectRectangleProperty); }
            set
            {
                SetValue(SelectRectangleProperty, value);
            }
        }

        public static readonly DependencyProperty SelectRectangleProperty = DependencyProperty.Register(
            "SelectRectangle",
            typeof(HRectangle),
            typeof(HDisplay),
            new PropertyMetadata(OnSelectRectangleChanged)
            );

        static void OnSelectRectangleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (SelectRectangleChanged != null)
            {
                SelectRectangleChanged(obj, args);
            }
        }

        public delegate void SelectRectangleHandler(object sender, DependencyPropertyChangedEventArgs args);
        public static SelectRectangleHandler SelectRectangleChanged = delegate { };

        //Circle
        public HCircle SelectCircle
        {
            get { return (HCircle)GetValue(SelectCircleProperty); }
            set
            {
                SetValue(SelectCircleProperty, value);
            }
        }

        public static readonly DependencyProperty SelectCircleProperty = DependencyProperty.Register(
            "SelectCircle",
            typeof(HCircle),
            typeof(HDisplay),
            new PropertyMetadata(OnSelectCircleChanged)
            );

        static void OnSelectCircleChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (SelectCircleChanged != null)
            {
                SelectCircleChanged(obj, args);
            }
        }

        public delegate void SelectCircleHandler(object sender, DependencyPropertyChangedEventArgs args);
        public static SelectCircleHandler SelectCircleChanged = delegate { };

    }
}