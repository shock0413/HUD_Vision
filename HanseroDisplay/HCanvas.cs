﻿using HanseroDisplay.Struct;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace HanseroDisplay
{
    public class HCanvas : Canvas
    {
        public delegate void OnRectangleMoveEventHandler();
        public event OnRectangleMoveEventHandler OnRectangleMoveEvent = delegate { };

        public delegate void OnCircleMoveEventHandler();
        public event OnCircleMoveEventHandler OnCircleMoveEvent = delegate { };

        public Point startPoint;

        public BitmapSource Bitmap;

        public Point margin = new Point(0, 0);

        private double zoom = 1;
        private Double zoomSpeed = 0.001;

        public bool StopMove = false;

        public UIElement SelectedElement;

        public StackPanel sp_Comment;

        public double drawWidth;
        public double drawHeight;

        public double startX;
        public double startY;

        public List<StructEllipse> ListPoint = new List<StructEllipse>();
        public List<StructLabel> ListLabel = new List<StructLabel>();
        public List<StructLine> ListLine = new List<StructLine>();
        public List<StructRectangle> ListRectangle = new List<StructRectangle>();

        public HCanvas()
        {
            MouseMove += HCanvas_MouseMove;
            MouseLeftButtonDown += HCanvas_MouseLeftButtonDown;
            MouseLeftButtonUp += HCanvas_MouseLeftButtonUp;
            MouseWheel += HCanvas_MouseWheel;

            SizeChanged += HCanvas_SizeChanged;

            this.ClipToBounds = true;

            CreateContextMenu();

            sp_Comment = new StackPanel();
            sp_Comment.HorizontalAlignment = HorizontalAlignment.Right;
            this.Children.Add(sp_Comment);
        }



        private void HCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Fit();
        }

        private void CreateContextMenu()
        {
            this.ContextMenu = new ContextMenu();
            MenuItem menuItem = new MenuItem();
            menuItem.Header = "이미지 맞추기";
            menuItem.Click += MenuItem_Fit_Click;

            ContextMenu.Items.Add(menuItem);

            menuItem = new MenuItem();
            menuItem.Header = "원본 이미지 저장";
            menuItem.Click += MenuItem_ImageSave_Click; ;

            ContextMenu.Items.Add(menuItem);

            menuItem = new MenuItem();
            menuItem.Header = "현재 화면 저장";
            menuItem.Click += MenuItem_InspectionImageSave_Click;

            ContextMenu.Items.Add(menuItem);
        }

        private void MenuItem_ImageSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            if (dialog.ShowDialog() == true)
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(Bitmap));

                using (var fileStream = new System.IO.FileStream(dialog.FileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    encoder.Save(fileStream);
                }
            }
        }

        private void MenuItem_InspectionImageSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Bitmap(*.bmp)|*.bmp";
            if (dialog.ShowDialog() == true)
            { 

                RenderTargetBitmap bitmap = new RenderTargetBitmap((int)this.ActualWidth, (int)this.ActualHeight, 96d,96d, PixelFormats.Pbgra32);
                bitmap.Render(this);

                PngBitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));

                using (var fileStream = new System.IO.FileStream(dialog.FileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    encoder.Save(fileStream);
                }
            }
        }

        private void MenuItem_Fit_Click(object sender, RoutedEventArgs e)
        {
            Fit();
        }

        public void Fit()
        {
            try
            {
                if (Bitmap != null)
                {
                    BeginInit();
                    double widthPer = (this.ActualWidth / Bitmap.PixelWidth);
                    double heightPer = (this.ActualHeight / Bitmap.PixelHeight);

                    if (widthPer > heightPer)
                    {
                        zoom = heightPer;

                        margin.X = (this.ActualWidth - (Bitmap.PixelWidth * zoom)) / 2;
                        margin.Y = 0;
                    }
                    else
                    {
                        zoom = widthPer;

                        margin.X = 0;
                        margin.Y = (this.ActualHeight - (Bitmap.PixelHeight * zoom)) / 2;
                    }

                    EndInit();

                    InvalidateVisual();
                }
            }
            catch(Exception e)
            {

            }
        }

        public void RemoveSelectRectangle()
        {
            Children.Clear();
            InvalidateVisual();
        }

        public void RemoveSelectCircle()
        {
            Children.Clear();
            InvalidateVisual();
        }

        double beforeZoom = 1;
        Point mousePos;


        private void HCanvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Point mousePos = e.GetPosition(this);

            this.mousePos = mousePos;

            beforeZoom = zoom;
            zoom += zoomSpeed * (e.Delta / 5);
             
            this.InvalidateVisual();
        }

        private bool isMouseDown = false;

        private void HCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isMouseDown = false;
        }

        private void HCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(this);
            isMouseDown = true;
        }

        private void HCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isMouseDown)
            {
                return;
            }

            if (!StopMove)
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    Point currentPoint = e.GetPosition(this);

                    margin.X = margin.X + currentPoint.X - startPoint.X;
                    margin.Y = margin.Y + currentPoint.Y - startPoint.Y;

                    this.InvalidateVisual();

                    startPoint = currentPoint;
                }
            }
            else
            {
                if (e.LeftButton == MouseButtonState.Pressed)
                {
                    if (SelectedElement != null)
                    {
                        if (SelectedElement.GetType() == typeof(HRectangle))
                        {
                            HRectangle rec = (SelectedElement as HRectangle);

                            Point currentPoint = e.GetPosition(this);

                            if (rec.Mode == HRectangle.RectangleModeConstant.MOVE_TOP)
                            {
                                rec.TopMoveValue = (rec.TopMoveValue + (startPoint.Y - currentPoint.Y) / zoom);
                            }
                            else if (rec.Mode == HRectangle.RectangleModeConstant.MOVE)
                            {
                                rec.MoveXValue = (rec.MoveXValue + (startPoint.X - currentPoint.X) / zoom);
                                rec.MoveYValue = (rec.MoveYValue + (startPoint.Y - currentPoint.Y) / zoom);
                            }
                            else if (rec.Mode == HRectangle.RectangleModeConstant.MOVE_BOTTOM)
                            {
                                rec.BottomMoveValue = (rec.BottomMoveValue + (currentPoint.Y - startPoint.Y) / zoom);
                            }
                            else if (rec.Mode == HRectangle.RectangleModeConstant.MOVE_RIGHT)
                            {
                                rec.RightMoveValue = (rec.RightMoveValue + (currentPoint.X - startPoint.X) / zoom);
                            }
                            else if (rec.Mode == HRectangle.RectangleModeConstant.MOVE_Left)
                            {
                                rec.LeftMoveValue = (rec.LeftMoveValue + (startPoint.X - currentPoint.X) / zoom);
                            }
                            else if (rec.Mode == HRectangle.RectangleModeConstant.MOVE_BOTTOM_RIGHT)
                            {
                                rec.RightMoveValue = (rec.RightMoveValue + (currentPoint.X - startPoint.X) / zoom);
                                rec.BottomMoveValue = (rec.BottomMoveValue + (currentPoint.Y - startPoint.Y) / zoom);
                            }
                            else if (rec.Mode == HRectangle.RectangleModeConstant.MOVE_BOTTOM_LEFT)
                            {
                                rec.BottomMoveValue = (rec.BottomMoveValue + (currentPoint.Y - startPoint.Y) / zoom);
                                rec.LeftMoveValue = (rec.LeftMoveValue + (startPoint.X - currentPoint.X) / zoom);
                            }
                            else if (rec.Mode == HRectangle.RectangleModeConstant.MOVE_TOP_RIGHT)
                            {
                                rec.TopMoveValue = (rec.TopMoveValue + (startPoint.Y - currentPoint.Y) / zoom);
                                rec.RightMoveValue = (rec.RightMoveValue + (currentPoint.X - startPoint.X) / zoom);
                            }
                            else if (rec.Mode == HRectangle.RectangleModeConstant.MOVE_TOP_LEFT)
                            {
                                rec.TopMoveValue = (rec.TopMoveValue + (startPoint.Y - currentPoint.Y) / zoom);
                                rec.LeftMoveValue = (rec.LeftMoveValue + (startPoint.X - currentPoint.X) / zoom);
                            }

                            OnRectangleMoveEvent();
                            this.InvalidateVisual();
                            startPoint = currentPoint;
                        }
                        else if(SelectedElement.GetType() == typeof(HCircle))
                        {
                            HCircle circle = (SelectedElement as HCircle);

                            Point currentPoint = e.GetPosition(this);

                            if (circle.Mode == HCircle.CircleModeConstant.MOVE_TOP)
                            {
                                double moveValue = (startPoint.Y - currentPoint.Y) / zoom;
                                circle.TopMoveValue = (circle.TopMoveValue + moveValue);
                                circle.BottomMoveValue = (circle.BottomMoveValue + moveValue);
                                circle.LeftMoveValue = (circle.LeftMoveValue + moveValue);
                                circle.RightMoveValue = (circle.RightMoveValue + moveValue);
                            }
                            else if (circle.Mode == HCircle.CircleModeConstant.MOVE)
                            {
                                circle.MoveXValue = (circle.MoveXValue + (startPoint.X - currentPoint.X) / zoom);
                                circle.MoveYValue = (circle.MoveYValue + (startPoint.Y - currentPoint.Y) / zoom);
                            }
                            else if (circle.Mode == HCircle.CircleModeConstant.MOVE_BOTTOM)
                            {
                                double moveValue = (startPoint.Y - currentPoint.Y) / zoom;
                                circle.TopMoveValue = (circle.TopMoveValue - moveValue);
                                circle.BottomMoveValue = (circle.BottomMoveValue - moveValue);
                                circle.LeftMoveValue = (circle.LeftMoveValue - moveValue);
                                circle.RightMoveValue = (circle.RightMoveValue - moveValue);
                            }
                            else if (circle.Mode == HCircle.CircleModeConstant.MOVE_RIGHT)
                            {
                                double moveValue = (currentPoint.X - startPoint.X) / zoom;
                                circle.TopMoveValue = (circle.TopMoveValue + moveValue);
                                circle.BottomMoveValue = (circle.BottomMoveValue + moveValue);
                                circle.LeftMoveValue = (circle.LeftMoveValue + moveValue);
                                circle.RightMoveValue = (circle.RightMoveValue + moveValue);
                            }
                            else if (circle.Mode == HCircle.CircleModeConstant.MOVE_Left)
                            {
                                double moveValue = (currentPoint.X - startPoint.X) / zoom;
                                circle.TopMoveValue = (circle.TopMoveValue - moveValue);
                                circle.BottomMoveValue = (circle.BottomMoveValue - moveValue);
                                circle.LeftMoveValue = (circle.LeftMoveValue - moveValue);
                                circle.RightMoveValue = (circle.RightMoveValue - moveValue);
                            }

                            OnRectangleMoveEvent();
                            this.InvalidateVisual();
                            startPoint = currentPoint;
                        }
                    }
                }
                else
                {
                    SelectedElement = null;
                    StopMove = false;
                }
            }
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            try
            {
                if (Bitmap != null)
                {

                    double currentImagePosX = (mousePos.X - startX);
                    double afterImagePosX = currentImagePosX / beforeZoom * zoom;
                    double xBias = (afterImagePosX - currentImagePosX);

                    double currentImagePosY = (mousePos.Y - startY);
                    double afterImagePosY = currentImagePosY / beforeZoom * zoom;
                    double YBias = (afterImagePosY - currentImagePosY);


                    drawWidth = Bitmap.PixelWidth * zoom;
                    drawHeight = Bitmap.PixelHeight * zoom;

                    startX = margin.X;
                    startY = margin.Y;
                    //margin.X -= xBias;
                    //margin.Y -= YBias;

                    //startX = margin.X;
                    //startY = margin.Y;

                    if (drawWidth < 0)
                    {
                        drawWidth = 1;
                    }
                    if (drawHeight < 0)
                    {
                        drawHeight = 1;
                    }

                    dc.DrawImage(Bitmap, new Rect(startX, startY, drawWidth, drawHeight));

                    //포인트 표시
                    ListPoint.ForEach(x =>
                    {
                        Ellipse ellipse = new Ellipse();
                        double height = x.ellipse.Height * zoom;
                        double width = x.ellipse.Width * zoom;
                        if (height < 0)
                        {
                            height = 0;
                        }
                        if (width < 0)
                        {
                            width = 0;
                        }

                        ellipse.Height = height;
                        ellipse.Width = width;
                        ellipse.Fill = x.ellipse.Fill;
                        ellipse.StrokeThickness = x.ellipse.StrokeThickness;
                        ellipse.Stroke = x.ellipse.Stroke;
                        ellipse.ToolTip = x.ellipse.ToolTip;

                        Point point = new Point() { X = x.RealPosition.X * zoom + startX, Y = x.RealPosition.Y * zoom + startY };

                        dc.DrawEllipse(ellipse.Fill, new Pen(ellipse.Stroke, ellipse.StrokeThickness), point, ellipse.Width / 2, ellipse.Height / 2);
                    });

                    //라인 표시
                    ListLine.ForEach(x =>
                    {
                        double penSize = x.Pen.Thickness * zoom;
                        if (penSize <= 0)
                        {
                            penSize = 1;
                        }

                        dc.DrawLine(new Pen(x.Pen.Brush, penSize), new Point(x.Point1.X * zoom + startX, x.Point1.Y * zoom + startY), new Point(x.Point2.X * zoom + startX, x.Point2.Y * zoom + startY));
                    });

                    //글자 표시
                    ListLabel.ForEach(x =>
                    {
                        if(x.DrawLabelAlign == HCore.HDrawPoints.DrawLabel.DrawLabelAlign.LEFT)
                        {
                            dc.DrawText(x.Text, new Point(x.RealPosition.X * zoom + startX, x.RealPosition.Y * zoom + startY));
                        }
                        else if(x.DrawLabelAlign == HCore.HDrawPoints.DrawLabel.DrawLabelAlign.RIGHT)
                        {
                            dc.DrawText(x.Text, new Point(x.RealPosition.X * zoom + startX - x.Text.Width, x.RealPosition.Y * zoom + startY));
                        }
                        
                    });

                    //사각형 표시
                    ListRectangle.ForEach(x =>
                    {
                        double recStartX = (x.CenterX - x.Width / 2) * zoom + startX;
                        double recStartY = (x.CenterY - x.Height / 2) * zoom + startY;

                        double width = x.Width * zoom;
                        double height = x.Height * zoom;

                        if (width > 0 && height > 0)
                        {
                            dc.DrawRectangle(x.Brush, x.Pen, new Rect(recStartX, recStartY, width, height));
                        }
                    });

                    Children.Cast<UIElement>().ToList().ForEach(x =>
                    {

                        if (x.GetType() == typeof(HRectangle))
                        {
                            HRectangle rec = x as HRectangle;

                            //가로 좌표
                            double leftValue = margin.X;
                            leftValue -= rec.MoveXValue * zoom;
                            leftValue -= rec.LeftMoveValue * zoom;

                            Canvas.SetLeft(x, leftValue);
                            Canvas.SetTop(x, margin.Y - rec.TopMoveValue * zoom - rec.MoveYValue * zoom);

                            //높이
                            double recHeight = (rec.originHeight) * zoom;
                            recHeight += rec.TopMoveValue * zoom;
                            recHeight += rec.BottomMoveValue * zoom;

                            if (recHeight > 0)
                            {
                                rec.Height = recHeight;
                            }
                            else
                            {
                                rec.Height = 1;
                            }

                            //가로

                            double recWidth = rec.originWidth * zoom;
                            recWidth += rec.RightMoveValue * zoom;
                            recWidth += rec.LeftMoveValue * zoom;

                            if (recWidth > 0)
                            {
                                rec.Width = recWidth;
                            }
                            else
                            {
                                rec.Width = 1;
                            }
                        }
                        else if(x.GetType() == typeof(HCircle))
                        {
                            HCircle circle = x as HCircle;

                            //가로 좌표
                            double leftValue = margin.X;
                            leftValue -= circle.MoveXValue * zoom;
                            leftValue -= circle.LeftMoveValue * zoom;

                            Canvas.SetLeft(x, leftValue);
                            Canvas.SetTop(x, margin.Y - circle.TopMoveValue * zoom - circle.MoveYValue * zoom);

                            //높이
                            double recHeight = (circle.originHeight) * zoom;
                            recHeight += circle.TopMoveValue * zoom;
                            recHeight += circle.BottomMoveValue * zoom;

                            if (recHeight > 0)
                            {
                                circle.Height = recHeight;
                            }
                            else
                            {
                                circle.Height = 1;
                            }

                            //가로

                            double recWidth = circle.originWidth * zoom;
                            recWidth += circle.RightMoveValue * zoom;
                            recWidth += circle.LeftMoveValue * zoom;

                            if (recWidth > 0)
                            {
                                circle.Width = recWidth;
                            }
                            else
                            {
                                circle.Width = 1;
                            }
                        }
                    });
                }
            }
            catch
            {

            }
        }
    }
}