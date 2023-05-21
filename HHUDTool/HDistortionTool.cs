using HCore;
using HCore.HDrawPoints;
using HOVLib;
using HResult;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Utill;
using static HCore.HResult;

namespace HHUDTool
{
    public class HDistortionTool : HudBase, IHTool
    {
        public List<System.Windows.Point> BeforeMoveData { get { return beforeMoveData; } set { beforeMoveData = value; } }
        private List<System.Windows.Point> beforeMoveData = null;

        public double? BeforeVerticalInterval { get; set; }
        public double? BeforeHorizentalInterval { get; set; }

        public bool IsOverLimitCheck { get { return isOverLimitCheck; } set { isOverLimitCheck = value; } }
        private bool isOverLimitCheck;

        public int InspectionCount { get; set; }
        public bool IsSetting = false;

        public HDistortionParams RunParams
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

        private HDistortionParams runParams;

        public HDistortionTool(int itemIndex, StructCarkindPart structCarkindPart) : base(itemIndex, structCarkindPart, "Distortion")
        {

        }
        //검사 설정 불러오기
        /// <summary>
        /// 저장된 기본 파라미터 값 로드, 디폴트 설정 파라미터
        /// </summary>
        public void LoadParams()
        {
            RunParams = new HDistortionParams
            {
                BrightLimit = GetBrightLimit(),
                MaxBlobCount = GetMaxBlob(),
                MinBlobCount = GetMinBlob(),
                MasterDotVerticalMMPerPixel = GetMasterDotVerticalMMperPixel(),
                MasterDotHorizentalMMPerPixel = GetMasterDotHorizentalMMperPixel()

            };
        }

        /// <summary>
        /// 수동으로 파라미터 값 저장
        /// </summary>
        /// <param name="toolParams">HTool.HDistortionParams</param>
        public void LoadParams(IHToolParams toolParams)
        {
            if (toolParams.GetType() == typeof(HDistortionParams))
            {
                RunParams = (HDistortionParams)toolParams;
            }
        }

        public IHResult Run(BitmapSource bitmapImage)
        {

            GC.Collect();

            Stopwatch sw = new Stopwatch();
            sw.Start();

            HBlob hBlobTool = new HBlob();

            //검사 변수들 초기화
            HDistortionResult result = new HDistortionResult
            {
                DrawManager = new DrawManager(),

                VerticalDotCount = GetHudDotVerticalCount(),
                HorizentalDotCount = GetHudDotHorizentalCount()
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
                List<HPoint[]> approxList = new List<HPoint[]>();

                HPoint[] approx;

                List<KeyValuePair<int, HBlob.Blob>> allBlobList = new List<KeyValuePair<int, HBlob.Blob>>();

                bool isMinApprox = false;
                int _insideMinX = int.MaxValue;
                int _insideMaxX = int.MinValue;
                int _insideMinY = int.MaxValue;
                int _insideMaxY = int.MinValue;
                int _outsideMinX = int.MaxValue;
                int _outsideMaxX = int.MinValue;
                int _outsideMinY = int.MaxValue;
                int _outsideMaxY = int.MinValue;

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

                        if (minX < _insideMinX)
                        {
                            _insideMinX = minX;
                        }
                        if (maxX > _insideMaxX)
                        {
                            _insideMaxX = maxX;
                        }
                        if (minY < _insideMinY)
                        {
                            _insideMinY = minY;
                        }
                        if (maxY > _insideMaxY)
                        {
                            _insideMaxY = maxY;
                        }

                        if (minX < _outsideMinX)
                        {
                            _outsideMinX = minX;
                        }
                        if (maxX > _outsideMaxX)
                        {
                            _outsideMaxX = maxX;
                        }
                        if (minY < _outsideMinY)
                        {
                            _outsideMinY = minY;
                        }
                        if (maxY > _outsideMaxY)
                        {
                            _outsideMaxY = maxY;
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

                    double insideLimitX = iniFile.GetDouble("Params", "InsideLimitX", 75);
                    double insideLimitY = iniFile.GetDouble("Params", "InsideLimitY", 70);
                    double insideXLength = iniFile.GetDouble("Params", "InsideXLength", 75);
                    double insideYLength = iniFile.GetDouble("Params", "InsideYLength", 70);
                    double outsideLimitX = iniFile.GetDouble("Params", "OutsideLimitX", 75);
                    double outsideLimitY = iniFile.GetDouble("Params", "OutsideLimitY", 70);
                    double outsideXLength = iniFile.GetDouble("Params", "OutsideXLength", 75);
                    double outsideYLength = iniFile.GetDouble("Params", "OutsideYLength", 70);

                    // 내부 사각형
                    if (Math.Abs(_insideMinX - _insideMaxX) < insideLimitX)
                    {
                        // 좌상단
                        if (direction == 1)
                        {
                            _insideMaxX += (int)insideXLength - Math.Abs(_insideMinX - _insideMaxX);
                        }
                        // 우상단
                        else if (direction == 2)
                        {
                            _insideMinX -= (int)insideXLength - Math.Abs(_insideMinX - _insideMaxX);
                        }
                        // 좌하단
                        else if (direction == 3)
                        {
                            _insideMaxX += (int)insideXLength - Math.Abs(_insideMinX - _insideMaxX);
                        }
                        // 우하단
                        else if (direction == 4)
                        {
                            _insideMinX -= (int)insideXLength - Math.Abs(_insideMinX - _insideMaxX);
                        }
                    }

                    if (Math.Abs(_insideMinY - _insideMaxY) < insideLimitY)
                    {
                        if (direction == 1)
                        {
                            _insideMaxY += (int)insideYLength - Math.Abs(_insideMinY - _insideMaxY);
                        }
                        else if (direction == 2)
                        {
                            _insideMaxY += (int)insideYLength - Math.Abs(_insideMinY - _insideMaxY);
                        }
                        else if (direction == 3)
                        {
                            _insideMinY -= (int)insideYLength - Math.Abs(_insideMinY - _insideMaxY);
                        }
                        else if (direction == 4)
                        {
                            _insideMinY -= (int)insideYLength - Math.Abs(_insideMinY - _insideMaxY);
                        }
                    }

                    _insideMinX += 10;
                    _insideMaxX -= 10;
                    _insideMinY += 10;
                    _insideMaxY -= 10;

                    result.DrawManager.DrawLines.Add(
                           new DrawLine()
                           {
                               StartX = _insideMinX,
                               EndX = _insideMaxX,
                               StartY = _insideMinY,
                               EndY = _insideMinY,
                               StrokeColor = Brushes.LimeGreen,
                               ToolTip = "test",
                               Size = 3
                           });

                    result.DrawManager.DrawLines.Add(
                           new DrawLine()
                           {
                               StartX = _insideMinX,
                               EndX = _insideMinX,
                               StartY = _insideMinY,
                               EndY = _insideMaxY,
                               StrokeColor = Brushes.LimeGreen,
                               ToolTip = "test",
                               Size = 3
                           });

                    result.DrawManager.DrawLines.Add(
                           new DrawLine()
                           {
                               StartX = _insideMaxX,
                               EndX = _insideMaxX,
                               StartY = _insideMinY,
                               EndY = _insideMaxY,
                               StrokeColor = Brushes.LimeGreen,
                               ToolTip = "test",
                               Size = 3
                           });

                    result.DrawManager.DrawLines.Add(
                           new DrawLine()
                           {
                               StartX = _insideMinX,
                               EndX = _insideMaxX,
                               StartY = _insideMaxY,
                               EndY = _insideMaxY,
                               StrokeColor = Brushes.LimeGreen,
                               ToolTip = "test",
                               Size = 3
                           });

                    // 외부 사각형
                    if (Math.Abs(_outsideMinX - _outsideMaxX) < outsideLimitX)
                    {
                        // 좌상단
                        if (direction == 1)
                        {
                            _outsideMaxX += (int)outsideXLength - Math.Abs(_outsideMinX - _outsideMaxX);
                        }
                        // 우상단
                        else if (direction == 2)
                        {
                            _outsideMinX -= (int)outsideXLength - Math.Abs(_outsideMinX - _outsideMaxX);
                        }
                        // 좌하단
                        else if (direction == 3)
                        {
                            _outsideMaxX += (int)outsideXLength - Math.Abs(_outsideMinX - _outsideMaxX);
                        }
                        // 우하단
                        else if (direction == 4)
                        {
                            _outsideMinX -= (int)outsideXLength - Math.Abs(_outsideMinX - _outsideMaxX);
                        }
                    }

                    if (Math.Abs(_outsideMinY - _outsideMaxY) < outsideLimitY)
                    {
                        if (direction == 1)
                        {
                            _outsideMaxY += (int)outsideYLength - Math.Abs(_outsideMinY - _outsideMaxY);
                        }
                        else if (direction == 2)
                        {
                            _outsideMaxY += (int)outsideYLength - Math.Abs(_outsideMinY - _outsideMaxY);
                        }
                        else if (direction == 3)
                        {
                            _outsideMinY -= (int)outsideYLength - Math.Abs(_outsideMinY - _outsideMaxY);
                        }
                        else if (direction == 4)
                        {
                            _outsideMinY -= (int)outsideYLength - Math.Abs(_outsideMinY - _outsideMaxY);
                        }
                    }

                    _outsideMinX -= 5;
                    _outsideMaxX += 5;
                    _outsideMinY -= 5;
                    _outsideMaxY += 5;

                    result.DrawManager.DrawLines.Add(
                           new DrawLine()
                           {
                               StartX = _outsideMinX,
                               EndX = _outsideMaxX,
                               StartY = _outsideMinY,
                               EndY = _outsideMinY,
                               StrokeColor = Brushes.LimeGreen,
                               ToolTip = "test",
                               Size = 3
                           });

                    result.DrawManager.DrawLines.Add(
                           new DrawLine()
                           {
                               StartX = _outsideMinX,
                               EndX = _outsideMinX,
                               StartY = _outsideMinY,
                               EndY = _outsideMaxY,
                               StrokeColor = Brushes.LimeGreen,
                               ToolTip = "test",
                               Size = 3
                           });

                    result.DrawManager.DrawLines.Add(
                           new DrawLine()
                           {
                               StartX = _outsideMaxX,
                               EndX = _outsideMaxX,
                               StartY = _outsideMinY,
                               EndY = _outsideMaxY,
                               StrokeColor = Brushes.LimeGreen,
                               ToolTip = "test",
                               Size = 3
                           });

                    result.DrawManager.DrawLines.Add(
                           new DrawLine()
                           {
                               StartX = _outsideMinX,
                               EndX = _outsideMaxX,
                               StartY = _outsideMaxY,
                               EndY = _outsideMaxY,
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
                        if (Math.Abs(_insideMinX - _insideMaxX) > 50 && Math.Abs(_insideMinX - _insideMaxX) > 50)
                        {
                            int blobMinX = blob.MinX;
                            int blobMaxX = blob.MaxX;
                            int blobMinY = blob.MinY;
                            int blobMaxY = blob.MaxY;

                            if (_insideMinX >= blobMinX || _insideMaxX <= blobMaxX || _insideMinY >= blobMinY || _insideMaxY <= blobMaxY)
                            {
                                if (_outsideMinX <= blobMinX && _outsideMaxX >= blobMaxX && _outsideMinY <= blobMinY && _outsideMaxY >= blobMaxY)
                                {
                                    allBlobList.RemoveAt(i);
                                    i--;
                                }
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
                        result.DrawManager.DrawLines.Add(new DrawLine() { StartX = 0, EndX = mat.Width - 1, StartY = m_ROI_MinY, EndY = m_ROI_MinY, Size = 3, StrokeColor = Brushes.Yellow });
                        result.DrawManager.DrawLines.Add(new DrawLine() { StartX = 0, EndX = mat.Width - 1, StartY = m_ROI_MaxY, EndY = m_ROI_MaxY, Size = 3, StrokeColor = Brushes.Yellow });
                    }
                }

                /*
                int m_MinX = int.MaxValue;
                int m_MinY = int.MaxValue;
                int m_MaxX = int.MinValue;
                int m_MaxY = int.MinValue;
                */

                for (int i = 0; i < allBlobList.Count; i++)
                {
                    double width = Math.Abs(allBlobList[i].Value.MaxX - allBlobList[i].Value.MinX);
                    double height = Math.Abs(allBlobList[i].Value.MaxY - allBlobList[i].Value.MinY);
                    int minY = allBlobList[i].Value.MinY;
                    int maxY = allBlobList[i].Value.MaxY;

                    /*
                    if (m_MinX > allBlobList[i].Value.CenterX)
                    {
                        m_MinX = allBlobList[i].Value.CenterX;
                    }
                    if (m_MaxX < allBlobList[i].Value.CenterX)
                    {
                        m_MaxX = allBlobList[i].Value.CenterX;
                    }
                    if (m_MinY > allBlobList[i].Value.CenterY)
                    {
                        m_MinY = allBlobList[i].Value.CenterY;
                    }
                    if (m_MaxY < allBlobList[i].Value.CenterY)
                    {
                        m_MaxY = allBlobList[i].Value.CenterY;
                    }
                    */

                    if (avgWidth * 2 > width && avgHeight * 2 > height)
                    {
                        if (isUseROI)
                        {
                            if (m_ROI_MinY < minY && m_ROI_MinY < maxY && m_ROI_MaxY > minY && m_ROI_MaxY > maxY)
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

                /*
                result.DrawManager.DrawLines.Add(new DrawLine() { StartX = m_MinX, StartY = m_MinY, EndX = m_MaxX, EndY = m_MinY, Size = 3, StrokeColor = Brushes.Red });
                result.DrawManager.DrawLines.Add(new DrawLine() { StartX = m_MinX, StartY = m_MaxY, EndX = m_MaxX, EndY = m_MaxY, Size = 3, StrokeColor = Brushes.Red });
                result.DrawManager.DrawLines.Add(new DrawLine() { StartX = m_MinX, StartY = m_MinY, EndX = m_MinX, EndY = m_MaxY, Size = 3, StrokeColor = Brushes.Red });
                result.DrawManager.DrawLines.Add(new DrawLine() { StartX = m_MaxX, StartY = m_MinY, EndX = m_MaxX, EndY = m_MaxY, Size = 3, StrokeColor = Brushes.Red });
                result.DrawManager.DrawLines.Add(new DrawLine() { StartX = m_MinX, StartY = m_MinY, EndX = m_MaxX, EndY = m_MaxY, Size = 3, StrokeColor = Brushes.Red });
                result.DrawManager.DrawLines.Add(new DrawLine() { StartX = m_MinX, StartY = m_MaxY, EndX = m_MaxX, EndY = m_MinY, Size = 3, StrokeColor = Brushes.Red });
                */

                allBlobList = temp;

                if (allBlobList.Count != 0)
                {
                    //블랍 정렬
                    List<KeyValuePair<int, HBlob.Blob>> blobResult = new List<KeyValuePair<int, HBlob.Blob>>();

                    if (allBlobList.Count == GetHudDotVerticalCount() * GetHudDotHorizentalCount())
                    {
                        int j = 0;
                        List<HBlob.Blob> sortedBlobList = SortAllBlobList(allBlobList);

                        sortedBlobList.ForEach(x =>
                        {
                            blobResult.Add(new KeyValuePair<int, HBlob.Blob>(j, x));
                            j++;
                        });

                        double rise = Math.Abs(tmp[tmp.GetLength(0) - 1, 0].CenterY - tmp[tmp.GetLength(0) - 1, tmp.GetLength(1) - 1].CenterY);
                        double run = Math.Abs(tmp[tmp.GetLength(0) - 1, 0].CenterX - tmp[tmp.GetLength(0) - 1, tmp.GetLength(1) - 1].CenterX);
                        double m = rise / run;
                        Console.WriteLine("상단 기울기 : " + m);

                        rise = Math.Abs(tmp[0, 0].CenterY - tmp[0, tmp.GetLength(1) - 1].CenterY);
                        run = Math.Abs(tmp[0, 0].CenterX - tmp[0, tmp.GetLength(1) - 1].CenterX);
                        m = rise / run;
                        Console.WriteLine("하단 기울기 : " + m);

                        /*
                        result.DrawManager.DrawLines.Add(new DrawLine() { StartX = tmp[0, 0].CenterX, StartY = tmp[0,0].CenterY, EndX = tmp[0, tmp.GetLength(1) - 1].CenterX, EndY = tmp[0, tmp.GetLength(1) - 1].CenterY, Size = 3, StrokeColor = Brushes.Red });
                        result.DrawManager.DrawLines.Add(new DrawLine() { StartX = tmp[tmp.GetLength(0) - 1, 0].CenterX, StartY = tmp[tmp.GetLength(0) - 1, 0].CenterY, EndX = tmp[tmp.GetLength(0) - 1, tmp.GetLength(1) - 1].CenterX, EndY = tmp[tmp.GetLength(0) - 1, tmp.GetLength(1) - 1].CenterY, Size = 3, StrokeColor = Brushes.Red });
                        result.DrawManager.DrawLines.Add(new DrawLine() { StartX = tmp[0, 0].CenterX, StartY = tmp[0, 0].CenterY, EndX = tmp[0, 0].CenterX, EndY = tmp[0, tmp.GetLength(1) - 1].CenterY, Size = 3, StrokeColor = Brushes.Red });
                        */

                        //결과 정리
                        result.Result = GetResult(sortedBlobList);
                        List<HPoint2D> moveDataList = GetMoveData(sortedBlobList, result);
                        result.MoveDataList = moveDataList.Select(x => x.GetPoint()).ToList();

                        //검증 데이터
                        if(result.Result == RESULT.OK)
                        {
                            int horizentalCount = GetHudDotHorizentalCount();
                            int verticalCount = GetHudDotVerticalCount();

                            double hudWidth = GetHudWidth();
                            double hudHeight = GetHudHeight();

                            double mmPerPixel = GetHudMMPerPixel();

                            {
                                //X-Distortion 계산
                                int maxValue = 0;
                                for (int i = 0; i < horizentalCount; i++)
                                {
                                    HBlob.Blob topBlob = sortedBlobList[i];
                                    HBlob.Blob bottomBlob = sortedBlobList[horizentalCount * (verticalCount - 1) + i];

                                    int currentMaxValue = topBlob.CenterX - bottomBlob.CenterX;
                                    currentMaxValue = Math.Abs(currentMaxValue);
                                    if (maxValue < currentMaxValue)
                                    {
                                        maxValue = currentMaxValue;
                                    }
                                }

                                double value = maxValue * 1.0 * mmPerPixel / hudWidth;
                                Console.WriteLine(value);
                            }

                            {
                                //Y-Distortion 계산
                                int maxValue = 0;
                                for (int i = 0; i < verticalCount; i++)
                                {
                                    HBlob.Blob topBlob = sortedBlobList[horizentalCount * i];
                                    HBlob.Blob bottomBlob = sortedBlobList[horizentalCount * i + horizentalCount - 1];

                                    int currentMaxValue = topBlob.CenterY - bottomBlob.CenterY;
                                    currentMaxValue = Math.Abs(currentMaxValue);
                                    if (maxValue < currentMaxValue)
                                    {
                                        maxValue = currentMaxValue;
                                    }
                                }

                                double value = maxValue * 1.0 * mmPerPixel / hudHeight;
                                Console.WriteLine(value);
                            }

                            
                            {
                                //smile
                                int maxValue = 0;
                                for (int i = 0; i < verticalCount; i++)
                                {
                                    HBlob.Blob topBlob = sortedBlobList[horizentalCount * i];
                                    HBlob.Blob middleBLob = sortedBlobList[horizentalCount * i + horizentalCount / 2];

                                    int currentMaxValue = topBlob.CenterY - middleBLob.CenterY;
                                    currentMaxValue = Math.Abs(currentMaxValue);
                                    if (maxValue < currentMaxValue)
                                    {
                                        maxValue = currentMaxValue;
                                    }
                                }

                                for (int i = 0; i < verticalCount; i++)
                                {
                                    HBlob.Blob middleBLob = sortedBlobList[horizentalCount * i + horizentalCount / 2];
                                    HBlob.Blob bottomBlob = sortedBlobList[horizentalCount * i + horizentalCount - 1];

                                    int currentMaxValue = middleBLob.CenterY - bottomBlob.CenterY;
                                    currentMaxValue = Math.Abs(currentMaxValue);
                                    if (maxValue < currentMaxValue)
                                    {
                                        maxValue = currentMaxValue;
                                    }
                                }

                                double value = maxValue * 1.0 * mmPerPixel / hudHeight;
                                Console.WriteLine(value);
                            }

                            {
                                //X Degree
                                double maxValue = 0;
                                for (int i = 0; i < verticalCount; i++)
                                {
                                    HBlob.Blob topBlob = sortedBlobList[horizentalCount * i];
                                    HBlob.Blob bottomBlob = sortedBlobList[horizentalCount * i + horizentalCount - 1];

                                    double degree = (topBlob.CenterY * 1.0 - bottomBlob.CenterY) / (topBlob.CenterX - bottomBlob.CenterX);

                                    double radian = Math.Atan2((bottomBlob.CenterY - topBlob.CenterY), (bottomBlob.CenterX - topBlob.CenterX));
                                    degree = (radian * (180d / Math.PI) + 360d);


                                    if (degree > 180)
                                    {
                                        degree = Math.Abs(360 - degree);
                                    }

                                    double currentMaxValue = degree;
                                    currentMaxValue = Math.Abs(currentMaxValue);
                                    if (maxValue < currentMaxValue)
                                    {
                                        maxValue = currentMaxValue;
                                    }
                                }

                                double value = maxValue;
                                Console.WriteLine(value);
                            }

                            {
                                //Y Degree
                                double maxValue = 0;
                                for (int i = 0; i < horizentalCount; i++)
                                {
                                    HBlob.Blob topBlob = sortedBlobList[i];
                                    HBlob.Blob bottomBlob = sortedBlobList[horizentalCount * (verticalCount - 1) + i];

                                    double degree = (topBlob.CenterY * 1.0 - bottomBlob.CenterY) / (topBlob.CenterX - bottomBlob.CenterX);

                                    double radian = Math.Atan2((bottomBlob.CenterY - topBlob.CenterY), (bottomBlob.CenterX - topBlob.CenterX));
                                    degree = (radian * (180d / Math.PI) + 360d) % 360d;


                                    if (degree > 90)
                                    {
                                        degree = Math.Abs(degree - 90);
                                    }
                                    else
                                    {
                                        degree = Math.Abs(90 - degree);
                                    }

                                    double currentMaxValue = degree;
                                    currentMaxValue = Math.Abs(currentMaxValue);
                                    if (maxValue < currentMaxValue)
                                    {
                                        maxValue = currentMaxValue;
                                    }
                                }

                                double value = maxValue;
                                Console.WriteLine(value);
                            }
                        }
                    }
                    else
                    {
                        blobResult = allBlobList;
                    }
                    

                    Stopwatch sw1 = new Stopwatch();
                    sw1.Start();
                    //결과 화면 표시
                    blobResult.ForEach(x =>
                    {
                        result.DrawManager.DrawPoints.Add(new DrawPoint() { StrokeColor = Brushes.Green, Size = x.Value.MaxY - x.Value.MinY, X = x.Value.CenterX, Y = x.Value.CenterY });
                        //result.DrawManager.DrawLabels.Add(new DrawLabel() { Foreground = Brushes.Red, Size = 10, X = x.Value.CenterX, Y = x.Value.CenterY, Text = x.Key.ToString() + "/" + Math.Round((double)x.Value.CenterX, 2)  + " / " + Math.Round((double)x.Value.CenterY, 2) });
                    });
                    Console.WriteLine("결과 화면 표시 1 소요 시간 : " + sw1.ElapsedMilliseconds);
                }

                result.DrawManager.DrawLabels.Add(new DrawLabel() { Y = 100, X = 0, Size = 30, Foreground = Brushes.White, Text = "찾은 블랍 개수 : " + allBlobList.Count() + " / " + (GetHudDotHorizentalCount() * GetHudDotVerticalCount()).ToString() });
                result.DrawManager = CreateDrawPoints(result, bitmapImage);
            }

            this.Result = result;

            sw.Stop();

            return result;
        }

        private DrawManager CreateDrawPoints(HDistortionResult result, BitmapSource image)
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

        HBlob.Blob[,] tmp = null;

        private List<HBlob.Blob> SortAllBlobList(List<KeyValuePair<int, HBlob.Blob>> blobList)
        {
            int verticalCount = GetHudDotVerticalCount();
            int horizentalCount = GetHudDotHorizentalCount();

            List<HBlob.Blob> result = new List<HBlob.Blob>();
            List<HBlob.Blob> xSortedList = blobList.Select(x => x.Value).ToList();
            //List<HBlob.Blob> ySortedList = blobList.Select(x => x.Value).ToList();
            //전체 도트 X축으로 정렬
            xSortedList = xSortedList.OrderBy(x => x.CenterX).ToList();
            //전체 도트 Y축으로 정렬
            // ySortedList = ySortedList.OrderBy(x => x.CenterY).ToList();

            tmp = new HBlob.Blob[verticalCount, horizentalCount];

            //전체 도트 각 X축 열 마다 Y값으로 정렬
            for (int i = 0; i < horizentalCount; i++)
            {
                int startPibot = verticalCount * i;
                List<HBlob.Blob> currentList = xSortedList.GetRange(startPibot, verticalCount);
                currentList = currentList.OrderBy(x => x.CenterY).ToList();

                // result.AddRange(currentList);

                for (int j = 0; j < verticalCount; j++)
                {
                    tmp[j, i] = currentList[j];
                }
            }

            for (int v = 0; v < verticalCount; v++)
            {
                for (int h = 0; h < horizentalCount; h++)
                {
                    result.Add(tmp[v, h]);
                    // Console.Write("(" + tmp[v,h].CenterX + "," + tmp[v,h].CenterY + ")  ");
                }

                // Console.WriteLine();
            }

            return result;
        }

        private List<HPoint2D> GetMoveData(List<HBlob.Blob> blobList, HDistortionResult distortionResult)
        {
            double moveXLimit = GetMoveLimit();
            double moveYLimit = GetMoveLimit();

            List<HPoint2D> result = new List<HPoint2D>();

            int verticalCount = GetHudDotVerticalCount();
            int horizentalCount = GetHudDotHorizentalCount();
            double verticalInterval = GetHudDotVerticalInterval();
            double horizentalInterval = GetHudDotHorizentalInterval();
            double mmPerPixel = GetHudMMPerPixel();

            if (BeforeHorizentalInterval.HasValue && isOverLimitCheck)
            {
                horizentalInterval = BeforeHorizentalInterval.Value;
            }

            if (BeforeVerticalInterval.HasValue && isOverLimitCheck)
            {
                verticalInterval = BeforeVerticalInterval.Value;
            }

            int centerIndex = blobList.Count / 2;

            int centerPibotMoveX = blobList[centerIndex].CenterX;
            int centerPibotMoveY = blobList[centerIndex].CenterY;

            //배수
            double mul = iniFile.GetDouble("Bias", "Mul" + InspectionCount, 1);
            LogManager.Write("Distortion Mul : " + InspectionCount + " / " + mul);

            int _v = 0;
            
            for (int k = 0; k < 100; k++)
            {
                for (int i = 0; i < blobList.Count; i++)
                {
                    HBlob.Blob blob = blobList[i];

                    double masterDotVerticalMMPerPixel = RunParams.MasterDotVerticalMMPerPixel;
                    double masterDotHorizentalMMPerPixel = RunParams.MasterDotHorizentalMMPerPixel;

                    // 속도 저하 요소
                    // LogManager.Write("Master Dot Vertical MM Per Pixel : " + masterDotVerticalMMPerPixel);
                    // LogManager.Write("Master Dot Horizental MM Per Pixel : " + masterDotHorizentalMMPerPixel);

                    // 마스터 위치 y축 기준 정렬
                    
                    double masterX = ((i % horizentalCount) - horizentalCount / 2) * (horizentalInterval / masterDotHorizentalMMPerPixel);
                    double masterY = ((i / horizentalCount) - (verticalCount / 2)) * (verticalInterval / masterDotVerticalMMPerPixel);

                    masterX += centerPibotMoveX;
                    masterY += centerPibotMoveY;
                    
                    DrawPoint point = new DrawPoint()
                    {
                        X = masterX,
                        Y = masterY,
                        StrokeColor = Brushes.Red,
                        Size = 5
                    };

                    distortionResult.DrawManager.DrawPoints.Add(point);

                    double moveX = (masterX - blob.CenterX) * mmPerPixel;
                    double moveY = (masterY - blob.CenterY) * mmPerPixel;

                    // Console.Write("(" + Math.Round(masterX - blob.CenterX, 2) + "," + Math.Round(masterY - blob.CenterY, 2) + ")  ");
                    //Console.Write("(" + Convert.ToInt32(masterX) + "," + Convert.ToInt32(masterY) + ")  ");

                    _v++;

                    if (_v % 21 == 0)
                    {
                       // Console.WriteLine();
                    }

                    moveX *= mul;
                    moveY *= mul;

                    if (beforeMoveData != null && IsOverLimitCheck)
                    {
                        double currentMoveX = BeforeMoveData[i].X + moveX;
                        double currentMoveY = BeforeMoveData[i].Y + moveY;
                        result.Add(new HPoint2D(currentMoveX, currentMoveY));
                    }
                    else
                    {
                        result.Add(new HPoint2D(moveX, moveY));
                    }
                }

                if (isOverLimitCheck)
                {
                    if (BeforeMoveData == null)
                    {
                        bool isOverLimit = false;

                        for (int j = 0; j < result.Count; j++)
                        {
                            if (Math.Abs(result[j].X) > moveXLimit || Math.Abs(result[j].Y) > moveYLimit)
                            {
                                isOverLimit = true;
                                break;
                            }
                        }

                        if (!isOverLimit)
                        {
                            break;
                        }
                        else
                        {
                            result = new List<HPoint2D>();
                            horizentalInterval -= (horizentalInterval * 0.01);
                            verticalInterval -= (verticalInterval * 0.01);
                        }
                    }
                }
                else
                {
                    break;
                }

                if (BeforeMoveData != null)
                {
                    break;
                }
            }

            BeforeHorizentalInterval = horizentalInterval;
            BeforeVerticalInterval = verticalInterval;

            BeforeMoveData = new List<System.Windows.Point>();

            result.ForEach(x =>
            {
                BeforeMoveData.Add(new System.Windows.Point(x.X, x.Y));
            });

            result.ForEach(x =>
            {

                if (x.X > moveXLimit)
                {
                    x.X = moveXLimit;
                }
                if (x.Y > moveYLimit)
                {
                    x.Y = moveYLimit;
                }

                if (x.X < moveXLimit * -1.0)
                {
                    x.X = moveXLimit * -1.0;
                }
                if (x.Y < moveYLimit * -1.0)
                {
                    x.Y = moveYLimit * -1.0;
                }

            });

            if (result == null || result.Count == 0)
            {
                LogManager.Write("블랍 생성 실패");
            }

            return result;
        }

        private RESULT GetResult(List<HBlob.Blob> blobList)
        {
            int verticalCount = GetHudDotVerticalCount();
            int horizentalCount = GetHudDotHorizentalCount();

            if (blobList.Count == verticalCount * horizentalCount)
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

        public double GetMoveLimit()
        {
            return iniFile.GetDouble("Params", "MoveLimit", 4.5);
        }

        public double GetMasterDotVerticalMMperPixel()
        {
            return iniFile.GetDouble("Params", "VerticalDotMMPerPixel", 0.65);
        }


        public double GetMasterDotHorizentalMMperPixel()
        {
            return iniFile.GetDouble("Params", "HorizentalDotMMPerPixel", 0.65);
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


        public void SaveMasterDotVerticalMMperPixel(double value)
        {
            iniFile.WriteValue("Params", "VerticalDotMMPerPixel", value);
        }


        public void SaveMasterDotHorizentalMMperPixel(double value)
        {
            iniFile.WriteValue("Params", "HorizentalDotMMPerPixel", value);
        }
        #endregion
    }

    public class HDistortionResult : IHResult
    {
        public RESULT Result { get { return result; } set { result = value; } }
        private RESULT result = RESULT.NG;
        public List<HKeyPoint> KeyPoints { get { if (keyPoints == null) keyPoints = new List<HKeyPoint>(); return keyPoints; } set { keyPoints = value; } }
        private List<HKeyPoint> keyPoints;
        public DrawManager DrawManager { get { return drawManager; } set { drawManager = value; } }
        private DrawManager drawManager = new DrawManager();
        public List<System.Windows.Point> MoveDataList { get { return moveDataList; } set { moveDataList = value; } }
        private List<System.Windows.Point> moveDataList = new List<System.Windows.Point>();

        public int VerticalDotCount { get { return verticalDotCount; } internal set { verticalDotCount = value; } }
        private int verticalDotCount;
        public int HorizentalDotCount { get { return horizentalDotCount; } internal set { horizentalDotCount = value; } }
        private int horizentalDotCount;

        public DrawManager GetDrawManager()
        {
            return DrawManager;
        }

        public RESULT GetResult()
        {
            return result;
        }
    }

    public class HDistortionParams : IHToolParams
    {
        public int MinBlobCount { get; set; }

        public int MaxBlobCount { get; set; }

        public int BrightLimit { get; set; }

        public double MasterDotVerticalMMPerPixel { get; set; }

        public double MasterDotHorizentalMMPerPixel { get; set; }
    }
}
