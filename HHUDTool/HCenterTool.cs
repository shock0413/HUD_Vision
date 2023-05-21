using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using HCore;
using HCore.HDrawPoints;
using HOVLib;
using Utill;
using static HCore.HResult;

namespace HHUDTool
{
    public class HCenterTool : HudBase, IHTool
    {
        public bool IsSetting = false;


        public HCenterParams RunParams
        {
            get
            {
                if (runParams == null)
                {
                    LoadParams();
                }

                return runParams;
            }
            set
            {
                runParams = value;
            }
        }

        private HCenterParams runParams;

        public HCenterTool(int itemIndex, StructCarkindPart structCarkindPart) : base(itemIndex, structCarkindPart, "Center")
        {

        }



        public void LoadParams()
        {
            RunParams = new HCenterParams
            {
                BrightLimit = GetBrightLimit(),
                MaxBlobCount = GetMaxBlob(),
                MinBlobCount = GetMinBlob(),
                ReverseX = GetReverseX(),
                ReverseY = GetReverseY()
                
            };
        }

        public void LoadParams(IHToolParams toolParams)
        {
            if (toolParams.GetType() == typeof(HCenterParams))
            {
                RunParams = (HCenterParams)toolParams;
            }
        }

        public IHResult Run(BitmapSource bitmapImage)
        {
            GC.Collect();

            Stopwatch sw = new Stopwatch();
            sw.Start();

            HBlob hBlobTool = new HBlob();

            //검사 변수들 초기화
            HCenterResult result = new HCenterResult
            {
                DrawManager = new DrawManager(),
            };

            //이미지가 존재할 경우 검사 진행
            if (bitmapImage != null)
            {
                //이미지 설정
                HMat mat = HOVLib.ImageConverter.ToHMat((BitmapSource)bitmapImage);
                //흑백 이미지로 변환
                mat = ConvertGray(mat);
                //이진화 이미지로 변환
                int brightLimit = RunParams.BrightLimit;
                mat = ConvertBinary(mat, brightLimit, 255);

                //블랍 검사 진행
                hBlobTool.Filter.MinArea = RunParams.MinBlobCount;
                hBlobTool.Filter.MaxArea = RunParams.MaxBlobCount;
                hBlobTool.Run(mat);


                //가운데 사각형 제거
                HPoint[][] contours = hBlobTool.FindContours(mat);

                HPoint[] approx = new HPoint[4];
                List<HPoint[]> approxList = new List<HPoint[]>();

                List<KeyValuePair<int, HBlob.Blob>> allBlobList = new List<KeyValuePair<int, HBlob.Blob>>();

                bool isMinApprox = false;
                int _minX = int.MaxValue;
                int _maxX = int.MinValue;
                int _minY = int.MaxValue;
                int _maxY = int.MinValue;

                List<HPoint> xPointList = new List<HPoint>();

                for (int i = 0; i < contours.Count(); i++)
                {
                    approx = hBlobTool.ApproxPolyDP(contours[i], 0.10);

                    int contourSize = approx.Count();

                    if (hBlobTool.ContourArea(approx) <= RunParams.MinBlobCount)
                    {
                        continue;
                    }

                    int minX = int.MaxValue;
                    int maxX = int.MinValue;
                    int minY = int.MaxValue;
                    int maxY = int.MinValue;
                    double size = hBlobTool.ContourArea(approx);

                    for (int k = 0; k < contourSize; k++)
                    {
                        if (approx[k].X < minX)
                        {
                            minX = approx[k].X;
                        }
                        if (approx[k].X > maxX)
                        {
                            maxX = approx[k].X;
                        }
                        if (approx[k].Y < minY)
                        {
                            minY = approx[k].Y;
                        }
                        if (approx[k].Y > maxY)
                        {
                            maxY = approx[k].Y;
                        }
                    }


                    if (maxX - minX > 30 && maxY - minY > 30)
                    {
                        result.DrawManager.DrawLines.Add(
                            new DrawLine()
                            {
                                StartX = approx[0].X,
                                EndX = approx[approx.Count() - 1].X,
                                StartY = approx[0].Y,
                                EndY = approx[approx.Count() - 1].Y,
                                StrokeColor = Brushes.Red,
                                Size = 3
                            });

                        for (int k = 0; k < contourSize; k++)
                        {
                            xPointList.Add(approx[k]);

                            result.DrawManager.DrawPoints.Add(new DrawPoint()
                            {
                                X = approx[k].X,
                                Y = approx[k].Y,
                                Size = 5,
                                StrokeColor = Brushes.Blue
                            });
                        }

                        for (int k = 0; k < contourSize - 1; k++)
                        {
                            result.DrawManager.DrawLines.Add(
                               new DrawLine()
                               {
                                   StartX = approx[k].X,
                                   EndX = approx[k + 1].X,
                                   StartY = approx[k].Y,
                                   EndY = approx[k + 1].Y,
                                   StrokeColor = Brushes.Red,
                                   ToolTip = "test",
                                   Size = 3
                               });
                        }

                        if (minX < _minX)
                        {
                            _minX = minX;
                        }
                        if (maxX > _maxX)
                        {
                            _maxX = maxX;
                        }
                        if (minY < _minY)
                        {
                            _minY = minY;
                        }
                        if (maxY > _maxY)
                        {
                            _maxY = maxY;
                        }

                        if (!isMinApprox)
                        {
                            isMinApprox = true;
                        }

                        approxList.Add(approx);
                    }
                    else
                    {
                        allBlobList.Add(new KeyValuePair<int, HBlob.Blob>(i, new HBlob.Blob((int)size, maxX, minX, maxY, minY)));
                    }
                }

                int direction = 0;

                if (isMinApprox)
                {
                    for (int i = 0; i < approxList.Count; i++)
                    {
                        for (int j = 0; j < approxList[i].Length; j++)
                        {
                            for (int k = 0; k < approxList[i].Length; k++)
                            {
                                if (j == k)
                                {
                                    continue;
                                }

                                if (Math.Abs(approxList[i][j].X - approxList[i][k].X) < 10 && Math.Abs(approxList[i][j].Y - approxList[i][k].Y) < 10)
                                {
                                    List<HPoint> list = approxList[i].ToList();
                                    list.RemoveAt(k);
                                    approxList[i] = list.ToArray();
                                    // xPointList = list;
                                }
                            }
                        }
                    };

                    if (xPointList.Count >= 3)
                    {
                        int midIndex = 0;
                        int leftRight = 0;
                        int topBottom = 0;

                        for (int i = 0; i < xPointList.Count; i++)
                        {
                            int isMidChecked = 0;

                            for (int j = 0; j < xPointList.Count; j++)
                            {
                                isMidChecked = 0;

                                if (i == j)
                                {
                                    continue;
                                }

                                if (isMidChecked != 2 && Math.Abs(xPointList[i].X - xPointList[j].X) < 10)
                                {
                                    isMidChecked++;
                                }

                                if (isMidChecked != 2 && Math.Abs(xPointList[i].Y - xPointList[j].Y) < 10)
                                {
                                    isMidChecked++;
                                }

                                if (isMidChecked == 2)
                                {
                                    midIndex = i;
                                }
                            }
                        }

                        for (int i = 0; i < xPointList.Count; i++)
                        {
                            if (i == midIndex)
                            {
                                continue;
                            }

                            if (Math.Abs(xPointList[i].X - xPointList[midIndex].X) > 10)
                            {
                                if (xPointList[i].X > xPointList[midIndex].X)
                                {
                                    leftRight = 1;
                                }
                                else
                                {
                                    leftRight = 2;
                                }
                            }

                            if (Math.Abs(xPointList[i].Y - xPointList[midIndex].Y) > 10)
                            {
                                if (xPointList[i].Y > xPointList[midIndex].Y)
                                {
                                    topBottom = 1;
                                }
                                else
                                {
                                    topBottom = 2;
                                }
                            }
                        }

                        if (leftRight == 1 && topBottom == 1)
                        {
                            direction = 1;
                        }
                        else if (leftRight == 2 && topBottom == 1)
                        {
                            direction = 2;
                        }
                        else if (leftRight == 1 && topBottom == 2)
                        {
                            direction = 3;
                        }
                        else if (leftRight == 2 && topBottom == 2)
                        {
                            direction = 4;
                        }
                    }

                    iniFile = new IniFile(iniFile.Path);

                    double limitX = iniFile.GetDouble("Params", "LimitX", 75);
                    double limitY = iniFile.GetDouble("Params", "LimitY", 70);
                    double lengthX = iniFile.GetDouble("Params", "LengthX", 75);
                    double lengthY = iniFile.GetDouble("Params", "LengthY", 70);

                    if (Math.Abs(_minX - _maxX) < limitX)
                    {
                        // 좌상단
                        if (direction == 1)
                        {
                            _maxX += (int)lengthX - Math.Abs(_minX - _maxX);
                        }
                        // 우상단
                        else if (direction == 2)
                        {
                            _minX -= (int)lengthX - Math.Abs(_minX - _maxX);
                        }
                        // 좌하단
                        else if (direction == 3)
                        {
                            _maxX += (int)lengthX - Math.Abs(_minX - _maxX);
                        }
                        // 우하단
                        else if (direction == 4)
                        {
                            _minX -= (int)lengthX - Math.Abs(_minX - _maxX);
                        }
                    }

                    if (Math.Abs(_minY - _maxY) < limitY)
                    {
                        if (direction == 1)
                        {
                            _maxY += (int)lengthY - Math.Abs(_minY - _maxY);
                        }
                        else if (direction == 2)
                        {
                            _maxY += (int)lengthY - Math.Abs(_minY - _maxY);
                        }
                        else if (direction == 3)
                        {
                            _minY -= (int)lengthY - Math.Abs(_minY - _maxY);
                        }
                        else if (direction == 4)
                        {
                            _minY -= (int)lengthY - Math.Abs(_minY - _maxY);
                        }
                    }

                    _minX += 10;
                    _maxX -= 10;
                    _minY += 10;
                    _maxY -= 10;

                    result.DrawManager.DrawLines.Add(
                           new DrawLine()
                           {
                               StartX = _minX,
                               EndX = _maxX,
                               StartY = _minY,
                               EndY = _minY,
                               StrokeColor = Brushes.LimeGreen,
                               ToolTip = "test",
                               Size = 3
                           });

                    result.DrawManager.DrawLines.Add(
                           new DrawLine()
                           {
                               StartX = _minX,
                               EndX = _minX,
                               StartY = _minY,
                               EndY = _maxY,
                               StrokeColor = Brushes.LimeGreen,
                               ToolTip = "test",
                               Size = 3
                           });

                    result.DrawManager.DrawLines.Add(
                           new DrawLine()
                           {
                               StartX = _maxX,
                               EndX = _maxX,
                               StartY = _minY,
                               EndY = _maxY,
                               StrokeColor = Brushes.LimeGreen,
                               ToolTip = "test",
                               Size = 3
                           });

                    result.DrawManager.DrawLines.Add(
                           new DrawLine()
                           {
                               StartX = _minX,
                               EndX = _maxX,
                               StartY = _maxY,
                               EndY = _maxY,
                               StrokeColor = Brushes.LimeGreen,
                               ToolTip = "test",
                               Size = 3
                           });
                }

                for (int i = 0; i < allBlobList.Count; i++)
                {
                    HBlob.Blob blob = allBlobList[i].Value;
                    if (isMinApprox)
                    {
                        if (Math.Abs(_minX - _maxX) > 50 && Math.Abs(_minX - _maxX) > 50)
                        {
                            if (_minX >= blob.MinX || _maxX <= blob.MaxX || _minY >= blob.MinY || _maxY <= blob.MaxY)
                            {
                                allBlobList.RemoveAt(i);
                                i--;
                            }
                        }
                    }
                }

                List<KeyValuePair<int, HBlob.Blob>> temp = new List<KeyValuePair<int, HBlob.Blob>>();

                double avgWidth = 0;
                double avgHeight = 0;
                for (int i = 0; i < allBlobList.Count; i++)
                {
                    avgWidth += Math.Abs(allBlobList[i].Value.MaxX - allBlobList[i].Value.MinX);
                    avgHeight += Math.Abs(allBlobList[i].Value.MaxY - allBlobList[i].Value.MinY);
                }
                avgWidth /= allBlobList.Count;
                avgHeight /= allBlobList.Count;

                // 잘림검사 센터 Y 기준 ROI 설정
                IniFile config = new IniFile(AppDomain.CurrentDomain.BaseDirectory + "\\Config.ini");
                bool isUseROI = config.GetBoolian("Info", "검사영역설정", false);
                double m_ROI_Ratio = config.GetDouble("Info", "검사영역계수", 1.5);
                int m_ROI_MinY = 0;
                int m_ROI_MaxY = 0;

                if (isUseROI)
                {
                    int centerY = Convert.ToInt32(mat.Height / 2);
                    int bias = Convert.ToInt32(GetHudDotVerticalInterval() / GetHudMMPerPixel() * (GetHudDotVerticalCount() / 2) * m_ROI_Ratio);
                    m_ROI_MinY = centerY - bias;
                    m_ROI_MaxY = centerY + bias;

                    // 세팅 창에서 검사 시 범위 표시
                    if (IsSetting)
                    {
                        result.DrawManager.DrawLines.Add(new DrawLine() { StartX = 0, EndX = mat.Width - 1, StartY = m_ROI_MinY, EndY = m_ROI_MinY, Size = 3, StrokeColor = Brushes.LimeGreen });
                        result.DrawManager.DrawLines.Add(new DrawLine() { StartX = 0, EndX = mat.Width - 1, StartY = m_ROI_MaxY, EndY = m_ROI_MaxY, Size = 3, StrokeColor = Brushes.LimeGreen });
                    }
                }

                for (int i = 0; i < allBlobList.Count; i++)
                {
                    double width = Math.Abs(allBlobList[i].Value.MaxX - allBlobList[i].Value.MinX);
                    double height = Math.Abs(allBlobList[i].Value.MaxY - allBlobList[i].Value.MinY);

                    if (avgWidth * 2 > width && avgHeight * 2 > height)
                    {
                        int blob_centerX = allBlobList[i].Value.CenterX;
                        int blob_centerY = allBlobList[i].Value.CenterY;
                        int blob_minX = allBlobList[i].Value.MinX;
                        int blob_maxX = allBlobList[i].Value.MaxX;
                        int blob_minY = allBlobList[i].Value.MinY;
                        int blob_maxY = allBlobList[i].Value.MaxY;

                        if (isUseROI)
                        {
                            if (m_ROI_MinY < blob_minX && m_ROI_MinY < blob_maxX && m_ROI_MaxY > blob_minY && m_ROI_MaxY > blob_maxY)
                            {
                                temp.Add(allBlobList[i]);
                            }
                        }
                        else
                        {
                            temp.Add(allBlobList[i]);
                        }
                    }
                    else
                    {
                        LogManager.Write("블랍 제거 / width : " + width + " / height : " + height);
                    }
                }

                allBlobList.ForEach(x =>
                {
                    result.DrawManager.DrawPoints.Add(new DrawPoint() { StrokeColor = Brushes.Green, Size = x.Value.MaxY - x.Value.MinY, X = x.Value.CenterX, Y = x.Value.CenterY });
                    //result.DrawManager.DrawLabels.Add(new DrawLabel() { Foreground = Brushes.Red, Size = 10, X = x.Value.CenterX, Y = x.Value.CenterY, Text = x.Key.ToString() + "/" + Math.Round((double)x.Value.CenterX, 2)  + " / " + Math.Round((double)x.Value.CenterY, 2) });
                });

                allBlobList = temp;

                result.Result = GetResult(allBlobList);

                if(result.Result == RESULT.OK)
                {
                    HBlob.Blob centerBlob = allBlobList[0].Value;

                    double width = bitmapImage.PixelWidth;
                    double height = bitmapImage.PixelHeight;

                    double desX = width / 2;
                    double desY = height / 2;

                    double moveX = (desX - centerBlob.CenterX) * GetHudMMPerPixel();
                    double moveY = (desY - centerBlob.CenterY) * GetHudMMPerPixel();
                    
                    if(RunParams.ReverseX)
                    {
                        moveX *= -1;
                    }

                    if (RunParams.ReverseY)
                    {
                        moveY *= -1;
                    }

                    if(Math.Abs(moveX) > 300)
                    {
                        if(moveX < 0)
                        {
                            moveX = -300;
                        }
                        else
                        {
                            moveX = 300;
                        }
                    }

                    if (Math.Abs(moveY) > 300)
                    {
                        if (moveY < 0)
                        {
                            moveY = -300;
                        }
                        else
                        {
                            moveY = 300;
                        }
                    }

                    moveX = Math.Round(moveX, 2);
                    moveY = Math.Round(moveY, 2);

                    result.MoveX = moveX;
                    result.MoveY = moveY;

                    if(Math.Abs(moveX) > 10)
                    {
                        result.Result = RESULT.NG;
                    }

                    result.DrawManager.DrawLabels.Add(new DrawLabel() { Y = 100, X = 0, Size = 30, Foreground = Brushes.White, Text = "중심 이동 값 : " + moveX + " / " + moveY });
                }

                result.DrawManager.DrawLines.Add(new DrawLine()
                {
                    StartX = bitmapImage.Width / 2,
                    EndX = bitmapImage.Width / 2,
                    StartY = 0,
                    EndY = bitmapImage.Height,
                    StrokeColor = Brushes.Yellow,
                    Size = 2

                });

                result.DrawManager.DrawLines.Add(new DrawLine()
                {
                    StartX = 0,
                    EndX = bitmapImage.Width,
                    StartY = bitmapImage.Height / 2,
                    EndY = bitmapImage.Height / 2,
                    StrokeColor = Brushes.Yellow,
                    Size = 2

                });

                result.DrawManager = CreateDrawPoints(result, bitmapImage);
            }

            this.Result = result;

            sw.Stop();

            return result;
        }

        private DrawManager CreateDrawPoints(HCenterResult result, BitmapSource image)
        {
            if (result.DrawManager == null)
            {
                result.DrawManager = new DrawManager();
            }

            if (result.Result == RESULT.OK)
            {
                DrawLabel drawLabel = new DrawLabel
                {
                    Text = "OK",
                    X = image.Width,
                    Y = 0,
                    Size = 30,
                    TextAlign = DrawLabel.DrawLabelAlign.RIGHT,
                    Foreground = Brushes.Green,
                    Background = Brushes.Black
                };

                result.DrawManager.DrawLabels.Add(drawLabel);
            }
            else if (result.Result == RESULT.NG)
            {
                DrawLabel drawLabel = new DrawLabel
                {
                    Text = "NG",
                    X = image.Width,
                    Y = 0,
                    Size = 30,
                    TextAlign = DrawLabel.DrawLabelAlign.RIGHT,
                    Foreground = Brushes.Red,
                    Background = Brushes.Black
                };

                result.DrawManager.DrawLabels.Add(drawLabel);
            }

            return result.DrawManager;
        }

        public RESULT GetResult(List<KeyValuePair<int, HBlob.Blob>> blobList)
        {
            if (blobList.Count == 1)
            {
                return RESULT.OK;
            }
            else
            {
                return RESULT.NG;
            }
        }

        #region 검사 파라미터 불러오는 함수들
        public int GetMinBlob()
        {
            return iniFile.GetInt32("Params", "Min Blob", 50);
        }

        public int GetMaxBlob()
        {
            return iniFile.GetInt32("Params", "Max Blob", 1000);
        }


        public int GetBrightLimit()
        {
            return iniFile.GetInt32("Params", "Bright Limit", 150);
        }
         
        public bool GetReverseX()
        {
            return iniFile.GetBoolian("Params", "Reverse X", false);
        }

        public bool GetReverseY()
        {
            return iniFile.GetBoolian("Params", "Reverse Y", false);
        }
        #endregion


        #region 검사 파라미터 저장하는 함수들
        public void SaveMinBlobCount(int value)
        {
            iniFile.WriteValue("Params", "Min Blob", value);
        }

        public void SaveMaxBlobCount(int value)
        {
            iniFile.WriteValue("Params", "Max Blob", value);
        }

        public void SaveBrightLimit(int value)
        {
            iniFile.WriteValue("Params", "Bright Limit", value);
        }

        public void SaveReverseX(bool value)
        {
            iniFile.WriteValue("Params", "Reverse X", value);
        }

        public void SaveReverseY(bool value)
        {
            iniFile.WriteValue("Params", "Reverse Y", value);
        }
        #endregion
    }


    public class HCenterResult : IHResult
    {
        public RESULT Result { get { return result; } set { result = value; } }
        private RESULT result = RESULT.NG;
        public List<HKeyPoint> KeyPoints { get { if (keyPoints == null) keyPoints = new List<HKeyPoint>(); return keyPoints; } set { keyPoints = value; } }
        private List<HKeyPoint> keyPoints;
        public DrawManager DrawManager { get { return drawManager; } set { drawManager = value; } }
        private DrawManager drawManager = new DrawManager();

        public double MoveX { get; set; }
        public double MoveY { get; set; }

        public DrawManager GetDrawManager()
        {
            return DrawManager;
        }

        public RESULT GetResult()
        {
            return result;
        }
    }


    public class HCenterParams : IHToolParams
    {
        public int MinBlobCount { get; set; }

        public int MaxBlobCount { get; set; }

        public int BrightLimit { get; set; }

        public bool ReverseX { get; set; }

        public bool ReverseY { get; set; }
    }
}
