using HCore;
using HCore.HDrawPoints;
using HOVLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Utill;
using static HCore.HResult;

namespace HHUDTool
{
    public class HFullContentsTool : HudBase, IHTool
    {
        public HFullContentsParams RunParams
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
        private HFullContentsParams runParams;

        public List<HTemplateMatching.TemplateMatch> Match { get { return match; } set { match = value; } }
        private List<HTemplateMatching.TemplateMatch> match = new List<HTemplateMatching.TemplateMatch>();

        public string TemplitPath { get { return ToolPath + ItemIndex + "\\Templit"; } }

        /// <summary>
        /// HUD의 풀컨텐츠 툴
        /// </summary>
        /// <param name="itemIndex"></param>
        /// <param name="structCarkindPart"></param>
        public HFullContentsTool(int itemIndex, StructCarkindPart structCarkindPart) : base(itemIndex, structCarkindPart, "FullContents")
        {

        }

        //검사 설정 불러오기
        /// <summary>
        /// 저장된 기본 파라미터 값 로드, 디폴트 설정 파라미터
        /// </summary>
        public void LoadParams()
        {
            RunParams = new HFullContentsParams();

            RunParams.Score = GetScoreLimit();
        }

        /// <summary>
        /// 수동으로 파라미터 값 저장
        /// </summary>
        /// <param name="toolParams">HTool.HCutoffParams</param>
        public void LoadParams(IHToolParams toolParams)
        {
             
        }

        /// <summary>
        /// 검사 진행
        /// </summary>
        /// <param name="bitmapImage"></param>
        /// <returns></returns>
        public IHResult Run(BitmapSource bitmapImage)
        {
            GC.Collect();

            Stopwatch sw = new Stopwatch();
            sw.Start();

            HTemplateMatching hMatchingTool = new HTemplateMatching();

            //검사 변수들 초기화
            HFullContentsResult result = new HFullContentsResult();
            result.DrawManager = new DrawManager();

            //이미지가 있을 경우 검사 진행
            if(bitmapImage != null)
            {
                //템플릿 로드
                Match = HTemplateMatching.LoadTemplit(TemplitPath);
                //탬플릿 흑백 전환
                match.ForEach(x =>
                {
                    x.TemplitImage = ConvertGray(x.TemplitImage);
                });

                /*
                //이미지 설정
                HMat mat = ImageConverter.ToHMat((BitmapImage)bitmapImage);

                //흑백 이미지 전환
                mat = ConvertGray(mat);
                */

                //매칭 검사 진행 및 최고 점수 결과 가져오기
                HTemplateMatching.TemplateMatchResult bestMatch = null;
                
                /*
                for(int i = 0; i < Match.Count(); i++)
                {
                    HTemplateMatching.TemplateMatchResult currentResult = hMatchingTool.Run(mat, Match[i]);
                    
                    if(bestMatch == null)
                    {
                        bestMatch = currentResult;
                    }
                    else if(bestMatch.Score < currentResult.Score)
                    {
                        bestMatch = currentResult;
                    }
                }
                */

                HTemplateMatching.TemplateMatchResult currentResult = new HTemplateMatching.TemplateMatchResult();
                currentResult.Location = new HPoint(0, 0);
                currentResult.Score = RunParams.Score;
                currentResult.Size = new HSizeD(0, 0);
                bestMatch = currentResult;

                Brush resultColor = Brushes.Green;

                //패턴 매칭 결과 판정
                if (bestMatch != null)
                {
                    if (bestMatch.Score < RunParams.Score)
                    {
                        result.Result = RESULT.NG;
                        resultColor = Brushes.Red;
                    }
                    else
                    {
                        result.Result = RESULT.OK;
                    }
                }

                //검사 결과 화면 표시
                if(bestMatch != null)
                {
                    result.DrawManager.DrawRectangle.Add(new DrawRectangle()
                    {
                        Width = bestMatch.Size.Width,
                        Height = bestMatch.Size.Height,
                        CenterX = bestMatch.Location.X + bestMatch.Size.Width / 2,
                        CenterY = bestMatch.Location.Y + bestMatch.Size.Height / 2,
                        StrokeColor = resultColor,
                        Size = 1
                    });

                    //이동량 표시
                    DrawLabel mmLabel = new DrawLabel();
                    mmLabel.Text = "점수 : " + bestMatch.Score.ToString();
                    mmLabel.X = 0;
                    mmLabel.Y = 100;
                    mmLabel.Size = 30;
                    mmLabel.Foreground = Brushes.White;
                    mmLabel.Background = Brushes.Black;

                    result.DrawManager.DrawLabels.Add(mmLabel);
                }

                result.DrawManager = CreateDrawPoints(result, bitmapImage);
            }
            this.Result = result;

            sw.Stop();

            LogManager.Write("점수 : " + result.Score);
            LogManager.Write("결과 : " + result.Result);
            LogManager.Write("검사 소요시간 : " + sw.ElapsedMilliseconds + "ms");

            return result;
        }

        private DrawManager CreateDrawPoints(HFullContentsResult result, BitmapSource image)
        {
            if (result.DrawManager == null)
            {
                result.DrawManager = new DrawManager();
            }

            if (result.Result == RESULT.OK)
            {
                DrawLabel drawLabel = new DrawLabel();
                drawLabel.Text = "OK";
                drawLabel.X = image.Width;
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
                drawLabel.X = image.Width;
                drawLabel.Y = 0;
                drawLabel.Size = 30;
                drawLabel.TextAlign = DrawLabel.DrawLabelAlign.RIGHT;
                drawLabel.Foreground = Brushes.Red;
                drawLabel.Background = Brushes.Black;

                result.DrawManager.DrawLabels.Add(drawLabel);
            }

            return result.DrawManager;
        }

        public void AddTemplit(CroppedBitmap croppedBitmap)
        {
            HTemplateMatching.AddTemplit(croppedBitmap, TemplitPath);
        }

        #region 검사 파라미터 불러오는 함수들

        public int GetScoreLimit()
        {
            return iniFile.GetInt32("Params", "Score", 70);
        }
        #endregion

        #region 검사 파라미터 저장하는 함수들
         
        public void SaveScoreLimit(int value)
        {
            iniFile.WriteValue("Params", "Score", value);
        }
        #endregion
    }

    public class HFullContentsResult : IHResult
    {
        public RESULT Result { get { return result; } set { result = value; } }
        private RESULT result = RESULT.NG;
        public DrawManager DrawManager { get { return drawManager; } set { drawManager = value; } }
        private DrawManager drawManager = new DrawManager();

        public int Score { get; set; }

        public DrawManager GetDrawManager()
        {
            return DrawManager;
        }

        public RESULT GetResult()
        {
            return result;
        }
    }

    public class HFullContentsParams :IHToolParams
    {
        public int Score { get; set; }
    }
}
