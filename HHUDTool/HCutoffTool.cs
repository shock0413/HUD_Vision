using HCore;
using HCore.HDrawPoints;
using HOVLib;
using HResult;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Utill;
using static HCore.HResult;

namespace HHUDTool
{
    public class HCutoffTool : HudBase, IHTool
    {
        HBlob hBlobTool = new HBlob();

        public HCutoffParams RunParams {
            get
            {
                if(runParams == null)
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
        private HCutoffParams runParams;

        /// <summary>
        /// HUD의 잘림 검사 툴
        /// </summary>
        /// <param name="itemIndex"></param>
        /// <param name="structCarkindPart"></param>
        public HCutoffTool(int itemIndex, StructCarkindPart structCarkindPart) : base(itemIndex, structCarkindPart, "Cutoff")
        {

        }

        //검사 설정 불러오기
        /// <summary>
        /// 저장된 기본 파라미터 값 로드, 디폴트 설정 파라미터
        /// </summary>
        public void LoadParams()
        {
            RunParams = new HCutoffParams();

            RunParams.BrightLimit = GetBrightLimit();
            RunParams.MaxBlobCount = GetMaxBlobCount();
            RunParams.MinBlobCount= GetMinBlobCount();
            RunParams.PassRangeTop= GetPassRangeTop();
            RunParams.PassRangeBottom = GetPassRangeBottom();
            runParams.TransmissionFactor = GetTransmissionFactor();
            runParams.TransmissionFactor1 = GetTransmissionFactor1();
            runParams.TransmissionFactor2 = GetTransmissionFactor2();
            runParams.TransmissionFactor3 = GetTransmissionFactor3();
            runParams.TransmissionFactorOver40 = GetTransmissionFactorOver40();
        }

        /// <summary>
        /// 수동으로 파라미터 값 저장
        /// </summary>
        /// <param name="toolParams">HTool.HCutoffParams</param>
        public void LoadParams(IHToolParams toolParams)
        {
            if(toolParams.GetType() == typeof(HCutoffParams))
            {
                RunParams = (HCutoffParams)toolParams;
            }
        }

        /// <summary>
        /// 검사 진행
        /// </summary>
        /// <param name="bitmapImage"></param>
        /// <returns></returns>
        public IHResult Run(BitmapSource bitmapImage)
        {
            GC.Collect();

            HCutoffResult result = new HCutoffResult();

            List<HPoint> leftPoints = new List<HPoint>();
            List<HPoint> rightPoints = new List<HPoint>();

            result.DrawManager = new DrawManager();

            if (bitmapImage != null)
            {
                //이미지 설정
                HMat mat = HOVLib.ImageConverter.ToHMat((BitmapSource)bitmapImage);
                //흑백 이미지로 변환
                mat = ConvertGray(mat);
                //이진화 이미지로 변환
                mat = ConvertBinary(mat, RunParams.BrightLimit, 255);

                //필터값 설정
                hBlobTool.Filter.MinArea = RunParams.MinBlobCount;
                hBlobTool.Filter.MaxArea = RunParams.MaxBlobCount;

                //블랍 합치기 옵션 값 설정
                hBlobTool.MergeMode = HBlob.MergeModeConstant.Biggest;
                hBlobTool.MergeBlob = true;
                hBlobTool.MergeThreshold = 100;

                //블랍 검사 진행
                hBlobTool.Run(mat);

                //결과 정리
                List<KeyValuePair<int, HBlob.Blob>> allBlobList = hBlobTool.Blobs;

                //찾은 블랍 개수가 0이 아닐 경우 진행
                if (allBlobList.Count != 0)
                {

                    //x, y의 최대 최소 찾기

                    int minX = int.MaxValue;
                    int maxX = 0;
                    int minY = int.MaxValue;
                    int maxY = 0;

                    allBlobList.ForEach(x =>
                    {
                        try
                        {
                            if (minX > x.Value.MinX)
                            {
                                minX = x.Value.MinX;
                            }

                            if (maxX < x.Value.MaxX)
                            {
                                maxX = x.Value.MaxX;
                            }

                            if (minY > x.Value.MinY)
                            {
                                minY = x.Value.MinY;
                            }

                            if (maxY < x.Value.MaxY)
                            {
                                maxY = x.Value.MaxY;
                            }

                        }
                        catch

                        {

                        }
                    });

                    float centerX = minX + (maxX - minX) / 2;
                    float centerY = minY + (maxY - minY) / 2;

                    //검사 결과 그리기

                    int size = maxY - minY;
                    result = new HCutoffResult();
                    result.KeyPoints.Add(new HKeyPoint(centerX, centerY, size));

                    double? moveMMValue = null;
                    if (result.KeyPoints.Count > 0)
                    {
                        moveMMValue = GetMoveMMValue(result.KeyPoints[0], mat.Height);
                    }

                    result.Result = GetResult(moveMMValue);
                    ///*
                    //#region 상세 검사
                    ////검증
                    ////윤곽 찾기
                    HPoint[][] contours = hBlobTool.FindContours(mat);

                    HPoint[] approx;

                    for (int i = 0; i < contours.Count(); i++)
                    {

                        approx = hBlobTool.ApproxPolyDP(contours[i], 0.03);

                        int contourSize = approx.Count();

                        if (hBlobTool.ContourArea(approx) <= RunParams.MinBlobCount)
                        {
                            continue;
                        }

                        //Contour를 근사화한 직선을 그린다.
                        if (contourSize % 1 == 0)
                        {
                            double degree = GetDegree(approx[approx.Count() - 1].X, approx[approx.Count() - 1].Y, approx[0].X, approx[0].Y);
                            HPoint drawPoint = GetDreeDrawPoint(approx[approx.Count() - 1].X, approx[approx.Count() - 1].Y, approx[0].X, approx[0].Y);
                            SolidColorBrush findedLineBrush = Brushes.Transparent;

                            int degreeRange = 20;

                            if (
                                (degree < -135 + degreeRange && degree > -135 - degreeRange) ||
                                (degree < 45 + degreeRange && degree > 45 - degreeRange)
                                )
                            {
                                findedLineBrush = Brushes.Orange;
                                if (approx[0].Y < approx[approx.Count() - 1].Y)
                                {
                                    rightPoints.Add(approx[0]);
                                }
                                else
                                {
                                    rightPoints.Add(approx[approx.Count() - 1]);
                                }


                                Console.WriteLine("degree" + degree);
                            }
                            else if (
                                (degree < 135 + degreeRange && degree > 135 - degreeRange) ||
                                (degree < -45 + degreeRange && degree > -45 - degreeRange)
                                )
                            {
                                findedLineBrush = Brushes.Red;
                                if (approx[0].Y < approx[approx.Count() - 1].Y)
                                {
                                    leftPoints.Add(approx[0]);
                                }
                                else
                                {
                                    leftPoints.Add(approx[approx.Count() - 1]);
                                }



                                Console.WriteLine("degree" + degree);
                            }
/*
                            result.DrawManager.DrawLines.Add(
                                new DrawLine()
                                {
                                    StartX = approx[0].X,
                                    EndX = approx[approx.Count() - 1].X,
                                    StartY = approx[0].Y,
                                    EndY = approx[approx.Count() - 1].Y,
                                    StrokeColor = findedLineBrush,
                                    Size = 10
                                });
                                */
                            int leftCount = 0;

                            for (int k = 0; k < contourSize - 1; k++)
                            {
                                //각도 표시
                                degree = GetDegree(approx[k].X, approx[k].Y, approx[k + 1].X, approx[k + 1].Y);
                                drawPoint = GetDreeDrawPoint(approx[k].X, approx[k].Y, approx[k + 1].X, approx[k + 1].Y);

                                findedLineBrush = Brushes.Transparent;

                                if (
                               (degree < -135 + degreeRange && degree > -135 - degreeRange) ||
                               (degree < 45 + degreeRange && degree > 45 - degreeRange)
                               )
                                {
                                    leftCount++;
                                    findedLineBrush = Brushes.Orange;

                                    if (approx[k].Y < approx[k + 1].Y)
                                    {
                                        rightPoints.Add(approx[k]);
                                    }
                                    else
                                    {
                                        rightPoints.Add(approx[k + 1]);
                                    }
                                    //찾음
                                   
                                }
                                else if (
                                    (degree < 135 + degreeRange && degree > 135 - degreeRange) ||
                                    (degree < -45 + degreeRange && degree > -45 - degreeRange)
                                    )
                                {
                                    findedLineBrush = Brushes.Red;

                                    if (approx[k].Y < approx[k + 1].Y)
                                    {
                                        leftPoints.Add(approx[k]);
                                    }
                                    else
                                    {
                                        leftPoints.Add(approx[k + 1]);
                                    }
                                    //찾음
                                }


                                /*
                                result.DrawManager.DrawLines.Add(
                                   new DrawLine()
                                   {
                                       StartX = approx[k].X,
                                       EndX = approx[k + 1].X,
                                       StartY = approx[k].Y,
                                       EndY = approx[k + 1].Y,
                                       StrokeColor = findedLineBrush,
                                       Size = 10
                                   });
                                   */


                                Console.WriteLine("degree" + degree);

                                /*
                                result.DrawManager.DrawLabels.Add(
                                        new DrawLabel()
                                        {
                                            Text = degree + "˚",
                                            X = drawPoint.X,
                                            Y = drawPoint.Y,
                                            Foreground = Brushes.Green,
                                            Background = Brushes.Black,
                                            Size = 30
                                        });
                                        */
                                //각도 표시 끝
                            }

                        }

                    }

                    //오름차순으로 정렬

                    leftPoints = leftPoints.OrderBy(x => x.X).ToList();
                    rightPoints = rightPoints.OrderBy(y => y.X).ToList();

                    //센터의 좌우 포인트 설정
                    if (leftPoints.Count == 6 && rightPoints.Count == 6)
                    {
                        HPoint leftPibot = leftPoints[1];
                        HPoint rightPibot = rightPoints[4];

                        HPoint centerPoint = GetDreeDrawPoint(leftPibot.X, leftPibot.Y, rightPibot.X, rightPibot.Y);
                        result.DrawManager.DrawCross.Add(new DrawCross()
                        {
                            X = centerPoint.X,
                            Y = centerPoint.Y,
                            Size = 10,
                            StrokeColor = Brushes.Green
                        });

                        centerX = centerPoint.X;
                        centerY = centerPoint.Y;

                        moveMMValue = GetMoveMMValue(new HKeyPoint(centerX, centerY, 1), mat.Height);
                        result.Result = GetResult(moveMMValue);
                    }
                    
                    result.MoveMMValue = moveMMValue;
                }
                result.TransmissionFactor = GetTransmissionFactor();
                result.TransmissionFactor1 = GetTransmissionFactor1();
                result.TransmissionFactor2 = GetTransmissionFactor2();
                result.TransmissionFactor3 = GetTransmissionFactor3();
                result.TransmissionFactorOver40 = GetTransmissionFactorOver40();

                result.DrawManager = CreateDrawPoints(result, bitmapImage);

                //OK 범위 표시
                result.DrawManager.DrawRectangle.Add(DrawOKRange(mat.Height, mat.Width));

                this.Result = result;
            }

            return result;
        }

        private DrawManager CreateDrawPoints(HCutoffResult result, BitmapSource image)
        {
            if (result.DrawManager == null)
            {
                result.DrawManager = new DrawManager();
            }

            result.KeyPoints.ForEach(x =>
            {
                DrawPoint drawPoint = new DrawPoint();
                drawPoint.X = x.X;
                drawPoint.Y = x.Y;
                drawPoint.Size = x.Size;
                drawPoint.StrokeColor = Brushes.Green;
                drawPoint.Fill = Brushes.Transparent;

                result.DrawManager.DrawPoints.Add(drawPoint);
            });

            if (result.Result == RESULT.OK)
            {
                DrawLabel drawLabel = new DrawLabel();
                drawLabel.Text = "OK";
                drawLabel.X = image.PixelWidth;
                drawLabel.Y = 0;
                drawLabel.Size = 30;
                drawLabel.TextAlign = DrawLabel.DrawLabelAlign.RIGHT;
                drawLabel.Foreground = Brushes.Green;
                drawLabel.Background = Brushes.Black;

                result.DrawManager.DrawLabels.Add(drawLabel);
            }
            else if (result.Result == RESULT.NG)
            {
                DrawLabel drawLabel = new DrawLabel();
                drawLabel.Text = "NG";
                drawLabel.X = image.PixelWidth;
                drawLabel.Y = 0;
                drawLabel.Size = 30;
                drawLabel.TextAlign = DrawLabel.DrawLabelAlign.RIGHT;
                drawLabel.Foreground = Brushes.Red;
                drawLabel.Background = Brushes.Black;

                result.DrawManager.DrawLabels.Add(drawLabel);
            }

            //이동량 표시
            DrawLabel mmLabel = new DrawLabel();
            mmLabel.Text = "이동량 : " + result.MoveMMValue.ToString();
            mmLabel.X = 0;
            mmLabel.Y = 0;
            mmLabel.Size = 30;
            mmLabel.Foreground = Brushes.White;
            mmLabel.Background = Brushes.Black;

            result.DrawManager.DrawLabels.Add(mmLabel);

            return result.DrawManager;
        }

        private double? GetMoveMMValue(HKeyPoint keyPoint, double imageHeight)
        {
            if (keyPoint != null)
            {
                double moveValue = 0;

                double targetPointY = keyPoint.Y;

                double centerY = imageHeight / 2;
                //File.AppendAllText("hud.result.txt", targetPointY.ToString()+"\n");

                double rangeMMTop = RunParams.PassRangeTop;
                double rangeMMBottom = RunParams.PassRangeBottom;
                double mmPerPixel = GetHudMMPerPixel();

                double topOK = centerY - ((rangeMMTop) / mmPerPixel);
                double bottomOK = centerY + ((rangeMMBottom) / mmPerPixel);

                if (targetPointY > topOK && targetPointY < bottomOK)
                {
                    moveValue = 0;
                }
                else
                {
                    //ng
                    moveValue = (targetPointY - (topOK + ((bottomOK - topOK) / 2))) * mmPerPixel;
                }

                if(GetIsReverseMoveDirection())
                {
                    moveValue = moveValue * -1;
                }

                LogManager.Write("IsReverse : " + GetIsReverseMoveDirection());
                LogManager.Write("targetPointY : " + targetPointY);
                LogManager.Write("centerY : " + centerY);
                LogManager.Write("mmperpixel : " + mmPerPixel);
                LogManager.Write("movevalue : " + moveValue);

                return moveValue;
            }
            else
            {
                return null;
            }
        }

        private DrawRectangle DrawOKRange(double imageHeight, double imageWidth)
        {
            double centerY = imageHeight / 2;
            double centerX = imageWidth / 2;
            double rangeMMTop = RunParams.PassRangeTop;
            double rangeMMBottom = RunParams.PassRangeBottom;
            double mmPerPixel = GetHudMMPerPixel();

            double topOK = centerY - ((rangeMMTop) / mmPerPixel);
            double bottomOK = centerY + ((rangeMMBottom) / mmPerPixel);

            DrawRectangle rec = new DrawRectangle()
            {
                CenterX = centerX,
                CenterY = topOK + ((bottomOK - topOK) / 2),
                Width = imageWidth - 20,
                Height = bottomOK - topOK,
                StrokeColor = Brushes.Orange,
                Size = 1
            };

            return rec;
        }

        private RESULT GetResult(double? moveValue)
        {
            if (moveValue.HasValue)
            {
                if (moveValue.Value == 0)
                {
                    return RESULT.OK;
                }
                else
                {
                    return RESULT.NG;
                }
            }
            else
            {
                return RESULT.NG;
            }
        }

        private double GetDegree(int startX, int startY, int endX, int endY)
        {
            double dx = startX - endX;
            double dy = startY - endY;

            double rad = Math.Atan2(dy, dx);
            double degree = (rad * 180) / Math.PI;
            degree = Math.Round(degree, 0);

            return degree;
        }

        private HPoint GetDreeDrawPoint(int startX, int startY, int endX, int endY)
        {
            double drawX = 0;

            if (startX < endX)
            {
                drawX = startX + ((endX - startX) / 2);
            }
            else
            {
                drawX = endX + ((startX - endX) / 2);
            }

            double drawY = 0;

            if (startY > endY)
            {
                drawY = endY + ((startY - endY) / 2);

            }
            else
            {
                drawY = startY + ((endY - startY) / 2);
            }

            return new HPoint((int)drawX, (int)drawY);
        }

        #region 검사 파라미터 불러오는 함수
        public double GetPassRangeTop()
        {
            return iniFile.GetDouble("Params", "Pass Range Top", 5);
        }

        public double GetPassRangeBottom()
        {
            return iniFile.GetDouble("Params", "Pass Range Bottom", 5);
        }

        public int GetMinBlobCount()
        {
            return iniFile.GetInt32("Params", "Min Blob", 1000);
        }

        public int GetMaxBlobCount()
        {
            return iniFile.GetInt32("Params", "Max Blob", 500000);
        }

        public int GetBrightLimit()
        {
            return iniFile.GetInt32("Params", "Bright Limit", 200);
        }

        public double GetTransmissionFactor()
        {
            return iniFile.GetDouble("Params", "Transmission Factor", 1);
        }

        public double GetTransmissionFactor1()
        {
            return iniFile.GetDouble("Params", "Transmission Factor 1", 1);
        }

        public double GetTransmissionFactor2()
        {
            return iniFile.GetDouble("Params", "Transmission Factor 2", 1);
        }

        public double GetTransmissionFactor3()
        {
            return iniFile.GetDouble("Params", "Transmission Factor 3", 1);
        }

        public double GetTransmissionFactorOver40()
        {
            return iniFile.GetDouble("Params", "Transmission Factor Over 40", 1);
        }

        public bool GetIsReverseMoveDirection()
        {
            return iniFile.GetBoolian("Params", "Reverse Move Value", false);
        }
        #endregion

        #region 검사 파라미터 저장하는 함수들
        public void SavePassRangeTop(double value)
        {
            iniFile.WriteValue("Params", "Pass Range Top", value);
        }

        public void SavePassRangeBottom(double value)
        {
            iniFile.WriteValue("Params", "Pass Range Bottom", value);
        }

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

        public void SaveTransmissionFactor(double value)
        {
            iniFile.WriteValue("Params", "Transmission Factor", value);
        }

        public void SaveTransmissionFactor1(double value)
        {
            iniFile.WriteValue("Params", "Transmission Factor 1", value);
        }

        public void SaveTransmissionFactor2(double value)
        {
            iniFile.WriteValue("Params", "Transmission Factor 2", value);
        }

        public void SaveTransmissionFactor3(double value)
        {
            iniFile.WriteValue("Params", "Transmission Factor 3", value);
        }
        public void SaveTransmissionFactorOver40(double value)
        {
            iniFile.WriteValue("Params", "Transmission Factor Over 40", value);
        }

        public void SaveIsReverseMoveDirection(bool value)
        {
            iniFile.WriteValue("Params", "Reverse Move Value", value);
        }

        #endregion
    }

    public class HCutoffResult : IHResult
    {
        public RESULT Result { get { return result; } set { result = value; } }
        private RESULT result = RESULT.NG;
        public List<HKeyPoint> KeyPoints { get { if (keyPoints == null) keyPoints = new List<HKeyPoint>(); return keyPoints; } set { keyPoints = value; } }
        private List<HKeyPoint> keyPoints;
        public DrawManager DrawManager { get { return drawManager; } set { drawManager = value; } }
        private DrawManager drawManager = new DrawManager();
        public double? MoveMMValue { get { return moveMMValue; } set { moveMMValue = value; } }
        private double? moveMMValue = null;

        public double TransmissionFactor { get; set; }
        public double TransmissionFactor1 { get; set; }
        public double TransmissionFactor2 { get; set; }
        public double TransmissionFactor3 { get; set; }
        public double TransmissionFactorOver40 { get; set; }

        public DrawManager GetDrawManager()
        {
            return DrawManager;
        }

        public RESULT GetResult()
        {
            return result;
        }
    }

    public class HCutoffParams : IHToolParams
    {
        public double PassRangeTop { get; set; }

        public double PassRangeBottom { get; set; }

        public int MinBlobCount { get; set; }

        public int MaxBlobCount { get; set; }
         
        public int BrightLimit { get; set; }

        public double TransmissionFactor { get; set; }

        public double TransmissionFactor1 { get; set; }

        public double TransmissionFactor2 { get; set; }

        public double TransmissionFactor3 { get; set; }

        public double TransmissionFactorOver40 { get; set; }
    }
}
