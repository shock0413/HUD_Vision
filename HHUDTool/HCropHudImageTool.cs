using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using HCore;
using HCore.HDrawPoints;
using HOVLib;
using static HCore.HResult;

namespace HHUDTool
{
    public class HCropHudImageTool : HudBase, IHTool
    {
        HBlob hBlobTool = new HBlob();

        public HCropHudImageParams RunParams
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
        private HCropHudImageParams runParams;

        public HCropHudImageTool(int itemIndex, StructCarkindPart structCarkindPart, string toolTypeName) : base(itemIndex, structCarkindPart, toolTypeName)
        {

        }

        public void LoadParams()
        {
            RunParams = new HCropHudImageParams();

            RunParams.BrightLimit = GetBrightLimit();
            RunParams.MaxBlobCount = GetMaxBlobCount();
            RunParams.MinBlobCount = GetMinBlobCount();
            RunParams.Margin = GetMargin();
        }

        public void LoadParams(IHToolParams toolParams)
        {
            if (toolParams.GetType() == typeof(HCropHudImageParams))
            {
                RunParams = (HCropHudImageParams)toolParams;
            }
        }

        public IHResult Run(BitmapSource bitmapImage)
        {
            HCroppedImageResult result = new HCroppedImageResult();

            if (bitmapImage != null)
            {
                //이미지 설정
                HMat mat = HOVLib.ImageConverter.ToHMat((BitmapSource)bitmapImage);
                //흑백 이미지로 변
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

                    //크롭해야할 가로세로 길이
                    double cropWidth = maxX - minX;
                    double cropHeight = maxY - minY;

                    //크롭할 사각형
                    System.Windows.Int32Rect rec = new System.Windows.Int32Rect(minX - RunParams.Margin, minY - RunParams.Margin, (int)cropWidth + RunParams.Margin * 2, (int)cropHeight + RunParams.Margin * 2);

                    if(rec.X < 0)
                    {
                        rec.X = 0;
                    }
                    if(rec.Y < 0)
                    {
                        rec.Y = 0;
                    }

                    if(rec.X + rec.Width > bitmapImage.PixelWidth)
                    {
                        rec.Width = (int)(bitmapImage.Width - rec.X);
                    }
                    if((rec.Y + rec.Height > bitmapImage.PixelHeight))
                    {
                        rec.Height = (int)(bitmapImage.Height - rec.Y);
                    }

                    CroppedBitmap cropImage = new CroppedBitmap(bitmapImage, rec);

                    result = new HCroppedImageResult();


                    result.CroppedRegion = rec;

                    result.Result = RESULT.OK;
                    result.CroppedImage = (BitmapSource)cropImage;

                    this.Result = result;
                }
            }

            return result;
        }

        #region 검사 파라미터 불러오는 함수
        public int GetMinBlobCount()
        {
            return iniFile.GetInt32("Params", "Min Blob", 0);
        }

        public int GetMaxBlobCount()
        {
            return iniFile.GetInt32("Params", "Max Blob", 500000);
        }

        public int GetBrightLimit()
        {
            return iniFile.GetInt32("Params", "Bright Limit", 100);
        }

        public int GetMargin()
        {
            return iniFile.GetInt32("Params", "Margin", 50);
        }

        #endregion

        public class HCropHudImageParams : IHToolParams
        {
            public int MinBlobCount { get; set; }

            public int MaxBlobCount { get; set; }

            public int BrightLimit { get; set; }

            public int Margin { get; set; }
        }

        public class HCroppedImageResult : IHResult
        {
            public RESULT Result { get { return result; } set { result = value; } }
            private RESULT result = RESULT.NG;

            public DrawManager DrawManager { get { return drawManager; } set { drawManager = value; } }
            private DrawManager drawManager = new DrawManager();

            public BitmapSource CroppedImage { get { return croppedImage; } set { croppedImage = value; } }
            private BitmapSource croppedImage = null;


            public System.Windows.Int32Rect CroppedRegion
            {
                get; set;
            }


            public DrawManager GetDrawManager()
            {
                return DrawManager;
            }

            public RESULT GetResult()
            {
                return result;
            }
        }
    }
}
