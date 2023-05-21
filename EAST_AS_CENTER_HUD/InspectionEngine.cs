using AsyncSocket;
using EAST_AS_CENTER_HUD.Camera;
using EAST_AS_CENTER_HUD.Struct;
using HanseroDisplay;
using HCore;
using HCore.HDrawPoints;
using HHUDTool;
using HTool;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Utill;
using static PylonC.NETSupportLibrary.DeviceEnumerator;

namespace EAST_AS_CENTER_HUD
{
    public class InspectionEngine : MainEngine
    {
        private AsyncSocketServer m_SocketServer;
        private List<AsyncSocketClient> m_SockClientList;

        public Window Window { get; set; }

        private AsyncSocketClient CurrentSocketClient { get; set; }

        public InspectionEngine(Window mainWindow) : base(mainWindow)
        {
            InitSystem();
            InitCameraCheckThread();
        }

        /// <summary>
        /// 비전 시스템 이니셜라이즈
        /// 1. 소켓 서버
        /// </summary>
        private void InitSystem()
        {
            LogManager.Write("비전 시스템 이니셜라이즈");
            try
            {
                int portNum = ServerPort;
                string serverIP = ServerIP;

                m_SocketServer = new AsyncSocketServer(portNum);
                m_SocketServer.OnAccept += new AsyncSocketAcceptEventHandler(OnSocketServerAccept);
                m_SocketServer.OnError += new AsyncSocketErrorEventHandler(OnClientError);
                // 2019.11.04
                m_SocketServer.OnClose += M_SocketServer_OnClose;


                m_SockClientList = new List<AsyncSocketClient>(1);

                string ip = serverIP;
                m_SocketServer.Listen(IPAddress.Parse(ip));

                LogManager.Write("서버 IP : " + serverIP + " / 포트 : " + portNum);

                ServerState = SOCKET_STATE.CONNECTED;
                LogManager.Write("서버 생성 성공");

                new Thread(new ThreadStart(() =>
                {
                    int tick = 0;

                    while (!isWindowClosed)
                    {
                        try
                        {
                            int max = StateCheckInterval;

                            if (CurrentSocketClient != null)
                            {
                                if (CurrentSocketClient.IsAliveSocket())
                                {

                                }
                                else
                                {
                                    CurrentSocketClient.Close();
                                    LogManager.Write("클라이언트 접속 종료 / 소켓 해제");
                                    CurrentSocketClient = null;
                                }
                            }

                            Thread.Sleep(1000);
                            tick++;

                            if (tick > max)
                            {
                                tick = 0;
                                try
                                {
                                    LogManager.Write("Program State : Alive");
                                }
                                catch
                                {

                                }
                            }
                        }
                        catch
                        {

                        }
                    }
                })).Start();
            }
            catch
            {
                MessageBox.Show("서버 생성 실패");
            }
        }

        private void M_SocketServer_OnClose(object sender, AsyncSocketConnectionEventArgs e)
        {
            LogManager.Write("서버 종료됨 : " + e.ToString());

            for (int i = 0; i < 5; i++)
            {
                try
                {
                    InitSystem();
                    break;

                }
                catch
                {
                    Thread.Sleep(1000);
                }
            }
        }

        private void OnSocketServerAccept(object sender, AsyncSocketAcceptEventArgs e)
        {
            // 클라이언트 1개만 유지
            AsyncSocketClient worker = new AsyncSocketClient(0, e.Worker);

            // 데이터 수신을 대기한다.
            worker.Receive();

            //이벤트 설정
            worker.OnConnet += new AsyncSocketConnectEventHandler(OnClientConnect);
            worker.OnAccept += Worker_OnAccept;
            worker.OnClose += new AsyncSocketCloseEventHandler(OnClientClose);
            worker.OnError += new AsyncSocketErrorEventHandler(OnClientError);
            worker.OnSend += new AsyncSocketSendEventHandler(OnClientSend);
            worker.OnReceive += new AsyncSocketReceiveEventHandler(OnClientReceive);

            m_SockClientList.ForEach(x => x.Close());
            m_SockClientList.Clear();

            // 접속한 클라이언트를 List에 포함한다.
            m_SockClientList.Add(worker);

            if(CurrentSocketClient != null)
            {
                if(CurrentSocketClient.IsAliveSocket())
                {
                    CurrentSocketClient.Close();
                }
            }

            CurrentSocketClient = worker;

            //서버 접속
            SocketState = SOCKET_STATE.CONNECTED;

            LogManager.Write("클라이언트 접속 완료");
        }

        private void Worker_OnAccept(object sender, AsyncSocketAcceptEventArgs e)
        {

        }

        private void OnClientError(object sender, AsyncSocketErrorEventArgs e)
        {
            try
            {
                m_SockClientList.Remove(m_SockClientList[0]);
            }
            catch
            {

            }
        }

        private void OnClientConnect(object sender, AsyncSocketConnectionEventArgs e)
        {

        }

        private void OnClientClose(object sender, AsyncSocketConnectionEventArgs e)
        {

        }

        private void OnClientSend(object sender, AsyncSocketSendEventArgs e)
        {

        }

        private void OnClientReceive(object sender, AsyncSocketReceiveEventArgs e)
        {
            try
            {
                //수신받은 명령어 처리
                string receivedString = Encoding.Default.GetString(e.ReceiveData, 0, e.ReceiveBytes);
                string[] arg = receivedString.Split(',');

                //커맨드 최신화
                InitCommand();

                Window.Dispatcher.Invoke(new Action(() =>
                {
                    try
                    {
                        //커맨드에 따른 비교 처리
                        CurrentCommand = arg[0].ToUpper();
                        LogManager.Write("수신된 커맨드 : " + arg[0]);

                        ////라이센스 체크
                        //if (!licenseManager.CurrentState)
                        //{
                        //    //LogManager.Write("라이센스 오류 발생");
                        //    //라이센스 상태 불량시 검사 종료
                        //    //return;
                        //}

                        //차량 정보 수신
                        if (CurrentCommand == Command_Carinfo)
                        {
                            //Carinfo "CARINFO,CARKIND,INFO1,INFO2,SUB CARKIND"
                            LogManager.Write("////////////////////////////////////////////////////////");
                            LogManager.Write("////////////////////////////////////////////////////////");
                            LogManager.Write("기종 정보 수신");
                            LogManager.Write("////////////////////////////////////////////////////////");
                            LogManager.Write("////////////////////////////////////////////////////////");
                            //화면 클리어
                            ClearDisplay();

                            //검사 정보
                            InspectionInfo = new StructInspectionInfo();
                            InspectionInfo.InspectionTime = DateTime.Now;
                            InspectionInfo.Carkind = arg[3];
                            InspectionInfo.Info1 = arg[1];
                            InspectionInfo.Info2 = arg[2];
                            InspectionInfo.SubCarkind = arg[4];

                            //DB에 검사 기종 등록
                            long resultIdx = -1;

                            //DB에 등록된 Index 가져오기
                            InspectionInfo.DBResultIndex = resultIdx;

                            //LED 컨트롤 초기화
                            ClearProgressState();

                            //커뮤니케이트 메시지 초기화
                            RemoveCommunicateMessage();


                            //현재 LED
                            //LED 패널 표시
                            SetProgressState(ProgressState.Carinfo, HCore.HResult.RESULT.OK);

                            //로그 남기기
                            LogManager.Write("기종 정보 " + arg[1] + "/" + arg[2] + "/" + arg[3] + "/" + arg[4]);

                            //등록된 기종인지 체크
                            List<StructCarkind> carkinds = StructCarkind.GetCarkind();

                            if (StructCarkind.GetCarkind().Where(x => x.Name == InspectionInfo.CarkindFullName).ToList().Count() == 0)
                            {

                                IsVisibleCodeCheck = true;
                                InspectionInfo.IsNotRegistedCarkind = true;
                                AddCommunicationMsg("기종 정보", "등록되지 않은 기종입니다.", HorizontalAlignment.Left);
                                LogManager.Write("등록되지 않은 기종");

                                //차량 정보 오류 내용 전송
                                SendDataToSocket(Command_Carinfo + ",Error:2");

                            }
                            else
                            {
                                IsVisibleCodeCheck = false;
                                LogManager.Write("정상적인 기종");

                                //Capture 경로
                                string capturePath = CaptureImageSavePath + "\\" +
                                    InspectionInfo.InspectionTime.Value.ToString("yyyyMMdd") + "\\" +
                                    InspectionInfo.InspectionTime.Value.ToString("HHmmss") + "_" + InspectionInfo.Carkind + "\\";

                                LogManager.Write("capture 이미지 경로 : " + capturePath);

                                //차량 정보 정상 수신 내용 전송
                                SendDataToSocket(receivedString);
                                //차량 정보 정상 수신 내용 전송
                                //SendDataToSocket(Command_Carinfo + "," + capturePath);

                            }

                            AddCommunicationMsg("기종 정보", "기종 정보 수신", HorizontalAlignment.Left);
                            AddCommunicationMsg("기종 정보", "기종 : " + InspectionInfo.Carkind + " " + InspectionInfo.SubCarkind, HorizontalAlignment.Left);
                            AddCommunicationMsg("기종 정보", "정보 : " + InspectionInfo.Info1 + " / " + InspectionInfo.Info2, HorizontalAlignment.Left);
                        }


                        //카메라 정보 수신
                        else if (CurrentCommand == Command_CameraCheck)
                        {
                            List<Device> AllDeviceList = CameraManager.GetAllDevices();
                            List<Device> connectedCameraList = CameraManager.GetConnectedDevices();

                            if (AllDeviceList.Count == connectedCameraList.Count)
                            {
                                SendCameraCheckResult(HCore.HResult.RESULT.OK);
                            }
                            else
                            {
                                SendCameraCheckResult(HCore.HResult.RESULT.NG);
                            }

                        }
                        //검사 진행
                        else if (
                                CurrentCommand == Command_CutOff ||
                                CurrentCommand == Command_Distortion ||
                                CurrentCommand == Command_Center ||
                                CurrentCommand == Command_FullContents ||
                                CurrentCommand == Command_CutOffCapture ||
                                CurrentCommand == Command_DistortionCapture ||
                                CurrentCommand == Command_FullContentsCapture ||
                                CurrentCommand == Command_DistortionInspectionCapture
                                )
                        {
                            bool isCaptureMode = false;

                            if (
                            CurrentCommand == Command_CutOffCapture ||
                            CurrentCommand == Command_DistortionCapture ||
                            CurrentCommand == Command_FullContentsCapture)
                            {
                                isCaptureMode = true;
                                LogManager.Write("캡쳐모드");
                            }

                            if (InspectionInfo == null || InspectionInfo.IsNotRegistedCarkind)
                            {
                                //기종 정보가 없을 경우 코드 확인 창 표시
                                IsVisibleCodeCheck = true;
                                LogManager.Write("기종 정보 없음 / 검사 종료");
                            }
                            else
                            {
                                IHResult result = null;

                                //차종 정보 불러오기
                                StructCarkind carkind = new StructCarkind(InspectionInfo.CarkindFullName);
                                InspectionInfo.Direction = carkind.Direction;
                                LogManager.Write("기종 정보 불러오기 완료");

                                Type toolType = null;
                                if (CurrentCommand == Command_CutOff || CurrentCommand == Command_CutOffCapture)
                                {
                                    AddCommunicationMsg("잘림검사", "잘림검사 시작", HorizontalAlignment.Left);
                                    LogManager.Write("잘림검사 수신");
                                    LogManager.Write("////////////////////////////////////////////////////////");
                                    AddCommunicationMsg("잘림검사", "잘림검사 시작", HorizontalAlignment.Right);
                                    LogManager.Write("잘림검사 시작");
                                    toolType = typeof(HCutoffTool);
                                }
                                else if (CurrentCommand == Command_Distortion || CurrentCommand == Command_DistortionCapture || CurrentCommand == Command_DistortionInspectionCapture)
                                {
                                    AddCommunicationMsg("왜곡검사", "왜곡검사 시작", HorizontalAlignment.Left);
                                    LogManager.Write("왜곡검사 수신");
                                    LogManager.Write("////////////////////////////////////////////////////////");
                                    AddCommunicationMsg("왜곡검사", "왜곡검사 시작", HorizontalAlignment.Right);
                                    LogManager.Write("왜곡검사 시작");
                                    toolType = typeof(HDistortionTool);
                                }
                                else if (CurrentCommand == Command_Center)
                                {
                                    AddCommunicationMsg("센터검사", "센터검사 시작", HorizontalAlignment.Left);
                                    LogManager.Write("센터검사 시작");
                                    LogManager.Write("////////////////////////////////////////////////////////");
                                    AddCommunicationMsg("센터검사", "센터검사 시작", HorizontalAlignment.Right);
                                    LogManager.Write("센터검사 시작");
                                    toolType = typeof(HCenterTool);
                                }
                                else if (CurrentCommand == Command_FullContents || CurrentCommand == Command_FullContentsCapture)
                                {
                                    AddCommunicationMsg("풀컨텐츠", "풀컨텐츠 시작", HorizontalAlignment.Left);
                                    LogManager.Write("풀컨텐츠 수신");
                                    LogManager.Write("////////////////////////////////////////////////////////");
                                    AddCommunicationMsg("풀컨텐츠", "풀컨텐츠 시작", HorizontalAlignment.Right);
                                    LogManager.Write("풀컨텐츠 시작");
                                    toolType = typeof(HFullContentsTool);
                                }

                                //검사항목 불러오기
                                StructInspection part = GetInspectionPart(carkind, toolType);

                                BitmapImage image = null;
                                if (CurrentCommand == Command_CutOff || CurrentCommand == Command_CutOffCapture)
                                {
                                    InspectionInfo.InspectionCountCutoff++;

                                    //기존 이미지 제거
                                    ImageCutoff = null;
                                    

                                    //이미지 촬영
                                    image = ShotImage(part, carkind);


                                    //이미지 촬영 여부 확인
                                    IsShotFailed(image);
                                    //이미지 표시
                                    ImageCutoff = image;

                                    if (!isCaptureMode)
                                    {
                                        //검사 진행
                                        result = RunInspection(part, toolType, image);
                                        
                                        //결과 표시
                                        ResultCutoff = result;


                                        //LED 패널 표시
                                        SetProgressState(ProgressState.Cutoff, result.GetResult());
                                    }

                                    AddCommunicationMsg("잘림검사", "잘림검사 완료", HorizontalAlignment.Right);

                                    ////////////////////
                                    //잘림 검사 결과 전송
                                    ////////////////////
                                    if (!isCaptureMode)
                                    {
                                        SendCutoffResult((HCutoffResult)result);
                                        //이미지 저장
                                        try
                                        {
                                            string savePath = SaveImage(ProgressState.Cutoff, InspectionInfo.InspectionCountCutoff, ImageCutoff, result.GetResult());
                                        }
                                        catch
                                        {
                                            LogManager.Write("잘림 검사 이미지 저장 실패");
                                        }

 
                                    }
                                    else
                                    {
                                        try
                                        {
                                            //캡쳐 이미지 저장
                                            LogManager.Write("잘림검사 캡쳐 시도");
                                            SaveCaptureImage(InspectionInfo, InspectionInfo.CaptureCountCutoff, Command_CutOffCapture.ToString(), ImageCutoff);
                                            SendCutoffCaptureResult();

                                            InspectionInfo.CaptureCountCutoff++;
                                        }
                                        catch
                                        {

                                        }
                                    }
                                }
                                else if (CurrentCommand == Command_Distortion || CurrentCommand == Command_DistortionCapture || CurrentCommand == Command_DistortionInspectionCapture)
                                {
                                    InspectionInfo.InspectionCountDistortion++;

                                    //기존 이미지 제거
                                    ImageDistortion = null;
                                    //이미지 촬영
                                    image = ShotImage(part, carkind);

                                    //이미지 촬영 여부 확인
                                    IsShotFailed(image);
                                    //이미지 표시
                                    ImageDistortion = image;

                                    //검사 진행
                                    if (!isCaptureMode)
                                    {
                                        result = RunInspection(part, toolType, image);
                                        //result.GetDrawManager().DrawLabels.Clear();
                                        //결과 표시
                                        ResultDistortion = result;

                                        //LED 패널 표시
                                        SetProgressState(ProgressState.Distortion, result.GetResult());
                                    }

                                    AddCommunicationMsg("왜곡검사", "왜곡검사 완료", HorizontalAlignment.Right);

                                    ////////////////////
                                    //왜곡 검사 결과 전송
                                    ////////////////////
                                    if (!isCaptureMode && CurrentCommand != Command_DistortionInspectionCapture)
                                    {
                                        SendDistortionResult((HDistortionResult)result);

                                        //이미지 저장
                                        try
                                        {
                                            string savePath = SaveImage(ProgressState.Distortion, InspectionInfo.InspectionCountDistortion, ImageDistortion, result.GetResult());
                                        }
                                        catch
                                        {
                                            LogManager.Write("왜곡 검사 이미지 저장 실패");
                                        }
 
                                    }
                                    else if (CurrentCommand == Command_DistortionInspectionCapture)
                                    {
                                        try
                                        {
                                            //왜곡 검사 검사 이미지 저장
                                            LogManager.Write("왜곡 검사 캡쳐 시도 캡쳐 시도");

                                            try
                                            {
                                                HDisplay dp = ((MainWindow)Window).dp_Distortion;

                                                dp.canvas.ListLabel.Clear();
                                                dp.canvas.InvalidateVisual();

                                                RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
                                                    (int)dp.canvas.ActualWidth,
                                                    (int)dp.canvas.ActualHeight,
                                                    1 / 96,
                                                    1 / 96,
                                                    PixelFormats.Pbgra32);

                                                double widthZoom = dp.canvas.ActualWidth / ImageDistortion.PixelWidth;
                                                double heightZoom = dp.canvas.ActualHeight / ImageDistortion.PixelHeight;

                                                renderBitmap.Render(dp.canvas);

                                                BitmapSource s = (BitmapSource)new CroppedBitmap(renderBitmap, new Int32Rect((int)(dp.canvas.startX), (int)(dp.canvas.startY), (int)(renderBitmap.PixelWidth - (dp.canvas.startX * 2)), (int)(renderBitmap.PixelHeight - (dp.canvas.startY * 2))));

                                                FormatConvertedBitmap newFormatedBitmapSource = new FormatConvertedBitmap();



                                                newFormatedBitmapSource.BeginInit();
                                                newFormatedBitmapSource.Source = s;

                                                newFormatedBitmapSource.DestinationFormat = PixelFormats.Bgr24;
                                                newFormatedBitmapSource.EndInit();

                                                SaveCaptureImage(InspectionInfo, InspectionInfo.InspectionCaptureCountDistortion, Command_DistortionInspectionCapture.ToString(), newFormatedBitmapSource);

                                                HCropHudImageTool tool = new HCropHudImageTool(part.ItemIndex, carkind, typeof(HCropHudImageTool).ToString());
                                                tool.LoadParams();
                                                tool.Run(newFormatedBitmapSource);
                                                HCropHudImageTool.HCroppedImageResult cropResult = (HCropHudImageTool.HCroppedImageResult)tool.Result;

                                                try
                                                {
                                                    SaveCaptureImage(InspectionInfo, InspectionInfo.InspectionCaptureCountDistortion, Command_DistortionInspectionCapture.ToString(), cropResult.CroppedImage);
                                                }
                                                catch (Exception exc)
                                                {
                                                    LogManager.Write("왜곡 검사 캡쳐 실패 : " + exc.Message);
                                                }

                                                SendDistortionInspectionCaptureResult(tool.Result.GetResult());
                                                InspectionInfo.InspectionCaptureCountDistortion++;
                                            }
                                            catch (Exception ex)
                                            {
                                                LogManager.Write("왜곡 검사 캡쳐 실패");
                                            }
                                        }
                                        catch
                                        {

                                        }
                                    }
                                    else
                                    {
                                        try
                                        {
                                            //캡쳐 이미지 저장
                                            LogManager.Write("왜곡 캡쳐 시도");
                                            SaveCaptureImage(InspectionInfo, InspectionInfo.CaptureCountDistortion, Command_DistortionCapture.ToString(), ImageDistortion);
                                            SendDistortionCaptureResult();

                                            InspectionInfo.CaptureCountDistortion++;
                                        }
                                        catch
                                        {

                                        }
                                    }
                                }

                                else if (CurrentCommand == Command_Center)
                                {
                                    InspectionInfo.InspectionCountCenter++;

                                    //기존 이미지 제거
                                    ImageCenter = null;

                                    //이미지 촬영
                                    image = ShotImage(part, carkind);

                                    //이미지 촬영 여부 확인
                                    IsShotFailed(image);
                                    //이미지 표시
                                    ImageCenter = image;

                                    if (!isCaptureMode)
                                    {
                                        //검사 진행
                                        result = RunInspection(part, toolType, image);
                                        //결과 표시
                                        ResultCenter = result;

                                        //LED 패널 표시
                                        SetProgressState(ProgressState.Center, result.GetResult());
                                    }

                                    AddCommunicationMsg("센터검사", "센터검사 완료", HorizontalAlignment.Right);

                                    ///////////////////////
                                    //센터 검사 결과 전송
                                    ///////////////////////
                                    if (!isCaptureMode)
                                    {
                                        SendCenterResult((HCenterResult)result);

                                        //이미지 저장
                                        try
                                        {
                                            string savePath = SaveImage(ProgressState.Center, InspectionInfo.InspectionCountCenter, ImageCenter, result.GetResult());
                                        }
                                        catch
                                        {
                                            LogManager.Write("센터 검사 이미지 저장 실패");
                                        }
                                    }
                                }

                                else if (CurrentCommand == Command_FullContents || CurrentCommand == Command_FullContentsCapture)
                                {

                                    InspectionInfo.InspectionCountFullContent++;

                                    //기존 이미지 제거
                                    ImageFullContents = null;
                                    //이미지 촬영
                                    image = ShotImage(part, carkind);

                                    //이미지 촬영 여부 확인
                                    IsShotFailed(image);
                                    //이미지 표시
                                    ImageFullContents = image;

                                    if (!isCaptureMode)
                                    {
                                        //검사 진행
                                        result = RunInspection(part, toolType, image);
                                        //결과 표시
                                        ResultFullContents = result;

                                        //LED 패널 표시
                                        SetProgressState(ProgressState.FullContent, result.GetResult());
                                    }


                                    AddCommunicationMsg("풀컨텐츠", "풀컨텐츠 완료", HorizontalAlignment.Right);

                                    ///////////////////////
                                    //풀컨텐츠 검사 결과 전송
                                    ///////////////////////
                                    if (!isCaptureMode)
                                    {
                                        SendFullContentResult((HFullContentsResult)result);

                                        //이미지 저장
                                        try
                                        {
                                            string savePath = SaveImage(ProgressState.FullContent, InspectionInfo.InspectionCountFullContent, ImageFullContents, result.GetResult());
                                        }
                                        catch
                                        {
                                            LogManager.Write("풀컨텐츠 이미지 저장 실패");
                                        }
                                    }
                                    else
                                    {
                                        //캡쳐 이미지 저장
                                        LogManager.Write("풀컨텐츠 캡쳐 시도");

                                        try
                                        {
                                            HCropHudImageTool tool = new HCropHudImageTool(part.ItemIndex, carkind, typeof(HCropHudImageTool).ToString());
                                            tool.LoadParams();
                                            tool.Run(ImageFullContents);
                                            HCropHudImageTool.HCroppedImageResult cropResult = (HCropHudImageTool.HCroppedImageResult)tool.Result;
                                            try
                                            {
                                                SaveCaptureImage(InspectionInfo, InspectionInfo.CaptureCountFullContent, Command_FullContentsCapture.ToString(), cropResult.CroppedImage);
                                            }
                                            catch
                                            {
                                                LogManager.Write("풀컨텐츠 캡쳐 실패");
                                            }

                                            SendFullContentCaptureResult(tool.Result.GetResult());

                                            InspectionInfo.CaptureCountFullContent++;
                                        }
                                        catch
                                        {
                                            LogManager.Write("풀컨텐츠 캡쳐 실패");
                                        }
                                    }
                                }
                            }
                        }
                        //킵얼라이브
                        else if (CurrentCommand == Command_KeepAlive)
                        {
                            //재수신 내용 설정
                            if (arg[1] == "1")
                            {
                                SendKeepAlive();
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        LogManager.Write("receive Invoke error 발생 : " + ex.Message);
                    }

                }));

                NotifyPropertyChanged("ConsoleStr");
            }
            catch (Exception ex)
            {
                LogManager.Write(ex.Message);
            }
        }

        private StructInspection GetInspectionPart(StructCarkind carkind, Type toolType)
        {
            IHTool tool = null;
            StructInspection part = null;

            carkind.Inspections.Cast<StructInspection>().ToList().ForEach(x =>
            {
                if (toolType == x.GetToolType())
                {
                    tool = x.GetTool();
                    part = x;
                }
            });

            LogManager.Write("검사항목 불러오기 완료");

            return part;
        }

        private IHResult RunInspection(StructInspection part, Type toolType, BitmapImage image)
        {
            LogManager.Write("검사 정보");
            LogManager.Write("Part : " + part);
            LogManager.Write("Tool Type : " + toolType);


            IHTool tool = part.GetTool();
            LogManager.Write("불러오기 완료");
            tool.LoadParams();
            LogManager.Write("검사 파라미터 불러오기 완료");

            if (toolType == typeof(HDistortionTool))
            {
                ((HDistortionTool)tool).InspectionCount = InspectionInfo.InspectionCountDistortion;

                if (InspectionInfo.DistortionBeforeHorizentalInterval != null &&
                    InspectionInfo.DistortionBeforeVerticalInterval != null &&
                    InspectionInfo.DistortionBeforeMoveData != null)
                {
                    ((HDistortionTool)tool).BeforeVerticalInterval = InspectionInfo.DistortionBeforeVerticalInterval;
                    ((HDistortionTool)tool).BeforeHorizentalInterval = InspectionInfo.DistortionBeforeHorizentalInterval;
                    ((HDistortionTool)tool).BeforeMoveData = InspectionInfo.DistortionBeforeMoveData;
                    if (InspectionInfo.Direction == "Conti" || InspectionInfo.Direction == "Mobis")
                    {
                        ((HDistortionTool)tool).IsOverLimitCheck = true;
                    }
                }
            }

            IHResult result = tool.Run(image);
            LogManager.Write("검사 완료");

            if (toolType == typeof(HDistortionTool))
            {
                InspectionInfo.DistortionBeforeVerticalInterval = ((HDistortionTool)tool).BeforeVerticalInterval;
                InspectionInfo.DistortionBeforeHorizentalInterval = ((HDistortionTool)tool).BeforeHorizentalInterval;
                InspectionInfo.DistortionBeforeMoveData = ((HDistortionTool)tool).BeforeMoveData;
            }

            return result;
        }

        private BitmapImage ShotImage(StructInspection part, StructCarkind carkind)
        {

            Thread.Sleep(ShotDelay);
            
            BitmapImage image = null;

            //image = new BitmapImage(new Uri(AppDomain.CurrentDomain.BaseDirectory + "\\center\\" + "test.jpg"));
            
            for(int i = 0; i < 3 && image == null; i++)
            {
                try
                {
                    image = cameraManager.OneShot(part.StructCamera, carkind.GetIsRotateCamera());
                    if (image == null)
                    {
                        Thread.Sleep(500);
                        LogManager.Write("영상 취득 실패");
                    }
                }
                catch(Exception e)
                {
                    LogManager.Write("영상 취득 실패 : " + e.Message);
                    Thread.Sleep(500);
                }
            }

            return image;
        }

        private void IsShotFailed(BitmapImage image)
        {
            if (image == null)
            {
                IsVisibleCamCheck = true;
            }
            else
            {
                IsVisibleCamCheck = false;
            }
        }

        /// <summary>
        /// 이미지 저장
        /// </summary>
        /// <param name="progress">검사 단계</param>
        /// <param name="count">검사 회수</param>
        /// <param name="image">이미지 파일</param>
        /// <param name="result">결과</param>
        /// <returns>파일 경로</returns>
        private string SaveImage(ProgressState progress, int count, BitmapSource image, HCore.HResult.RESULT result)
        {
            try
            {
                string dir = ImageSavePath + "\\" +
                    InspectionInfo.InspectionTime.Value.ToString("yyyyMMdd") + "\\" +
                    InspectionInfo.Carkind + "_" + InspectionInfo.SubCarkind + "_" + InspectionInfo.InspectionTime.Value.ToString("HHmmss") + "_" + InspectionInfo.Info1 + "_" + InspectionInfo.Info1;

                Directory.CreateDirectory(dir);

                string fileName = progress.ToString() + "_" + count + "_" + result;

                string path = dir + "\\" + fileName;

                if (IsSaveJpeg)
                {
                    SaveJpegImage(image, path + ".jpeg");
                }
                else
                {
                    SaveBitmapImage(image, path + ".bmp");
                }

                return path;
            }
            catch
            {
                LogManager.Write("이미지 저장 실패");
            }

            return null;
        }

        private string SaveCaptureImage(StructInspectionInfo inspectionInfo, int count, string name, BitmapSource image)
        {
            try
            {
                string dir = CaptureImageSavePath + "\\" +
                    InspectionInfo.InspectionTime.Value.ToString("yyyyMMdd") + "\\" +
                    InspectionInfo.InspectionTime.Value.ToString("HHmmss") + "_" + InspectionInfo.Carkind + "\\";

                Directory.CreateDirectory(dir);

                string fileName = name;
                if (count == 0)
                {
                    fileName += "_Before";
                }
                else
                {
                    fileName += "_After";
                }

                string path = dir + "\\" + fileName + ".bmp";

                SaveBitmapImage(image, path);

                LogManager.Write("캡쳐 이미지 저장 성공");

                return path;
            }
            catch (Exception e)
            {
                LogManager.Write("캡쳐 이미지 저장 실패 : " + e.Message);
            }

            return null;
        }

        private void SendDataToSocket(string str)
        {
            try
            {
                CurrentSocketClient.Send(Encoding.ASCII.GetBytes(str));
                LogManager.Write("소켓 데이터 전송 성공");
            }
            catch(Exception ex)
            {
                LogManager.Write("소켓 데이터 전송 실패 : " + ex.Message);
            }
        }

        private void SendDataToSocket(byte[] byteArr)
        {
            try
            {
                CurrentSocketClient.Send(byteArr);
            }
            catch (Exception ex)
            {
                LogManager.Write("소켓 데이터 전송 실패 : " + ex.Message);
            }
        }

        /// <summary>
        /// 잘림검사 결과값 전송
        /// </summary>
        /// <param name="result"></param>
        private void SendCutoffResult(HCutoffResult result)
        {
            string sendStr = Command_CutOffFinish + "," + result.Result + ",";

            if (result.MoveMMValue == null)
            {
                result.MoveMMValue = 0;
            }

            if (GetCutOffIsReverseMinus)
            {
                if (result.MoveMMValue < 0)
                {
                    sendStr += "01,";
                }
                else
                {
                    sendStr += "00,";
                }
            }
            else
            {
                if (result.MoveMMValue < 0)
                {
                    sendStr += "00,";
                }
                else
                {
                    sendStr += "01,";
                }
            }

            LogManager.Write("보내야할 Move Value : " + result.MoveMMValue);

            int convertValue;
            if(result.MoveMMValue.HasValue && Math.Abs(result.MoveMMValue.Value) >= 40)
            {
                double mul = result.TransmissionFactorOver40;
                LogManager.Write("40이상 변환 값 적용 :" + mul);
                convertValue = (int)(result.MoveMMValue * mul);
            }
            else if (InspectionInfo.InspectionCountCutoff == 1)
            {
                double mul = result.TransmissionFactor1;
                LogManager.Write("1번쨰 시도 값 적용 :" + mul);
                convertValue = (int)(result.MoveMMValue * mul);
            }
            else if(InspectionInfo.InspectionCountCutoff == 2)
            {
                double mul = result.TransmissionFactor2;
                LogManager.Write("2번쨰 시도 값 적용 :" + mul);
                convertValue = (int)(result.MoveMMValue * mul);
            }
            else if (InspectionInfo.InspectionCountCutoff == 3)
            {
                double mul = result.TransmissionFactor3;
                LogManager.Write("3번쨰 시도 값 적용 :" + mul);
                convertValue = (int)(result.MoveMMValue * mul);
            }
            else
            {
                double mul = result.TransmissionFactor;
                LogManager.Write("기타 시도 값 적용 :" + mul);
                convertValue = (int)(result.MoveMMValue * mul);
            }

            LogManager.Write("변환된 Move Value : " + convertValue);

            byte[] byteMoveData = IntToBigEdian((Math.Abs(convertValue)));

            StringBuilder sb = new StringBuilder();
            for (int i = 1; i < byteMoveData.Length; i++)
            {
                sb.AppendFormat("{0:x1}", byteMoveData[i]);
            }

            if (sb.ToString().ToUpper().Length == 1)
                sendStr += "0" + sb.ToString().ToUpper();
            else
            {
                sendStr += sb.ToString().ToUpper();
            }

            LogManager.Write("Send : " + sendStr);

            SendDataToSocket(sendStr);
        }

        /// <summary>
        /// 왜곡검사 결과값 전송
        /// </summary>
        /// <param name="result"></param>
        private void SendDistortionResult(HDistortionResult result)
        {

            string sendStr = Command_DistortionFinish + ",";

            //Data Size
            int size = (result.MoveDataList.Count() * 2 * 2 + 2);
            LogManager.Write("Result.MoveDataList Length : " + result.MoveDataList.Count());
            LogManager.Write("보내야할 Stream length : " + size);
            sendStr += result.Result + ",";
            sendStr += size + ",";


            if (result.Result == HCore.HResult.RESULT.OK)
            {
                List<byte> sendByteList = new List<byte>();
                sendByteList.AddRange(Encoding.ASCII.GetBytes(sendStr));
                byte[] byteMoveData = Make_MoveData(result);
                StringBuilder sb = new StringBuilder(byteMoveData.Length);
                for (int i = 0; i < byteMoveData.Length; i++)
                {
                    sb.AppendFormat("{0:x2}", byteMoveData[i]);
                }

                string sendMoveData = sb.ToString().ToUpper();
                sendByteList.AddRange(Encoding.ASCII.GetBytes(sendMoveData));

                LogManager.Write("Send : " + sendStr + sendMoveData);
                SendDataToSocket(sendByteList.ToArray());
            }
            else
            {
                LogManager.Write("Send : " + sendStr);
                SendDataToSocket(sendStr);
            }
        }

        private void SendCenterResult(HCenterResult result)
        {
            double sendMoveX = result.MoveX;
            double sendMoveY = result.MoveY;

            sendMoveX *= 100;
            sendMoveY *= 100;
            
            sendMoveX += 32767;
            sendMoveY += 32767;


            string sendStr = Command_CenterFinish + "," + result.Result + ",";

            if (result.MoveX < 0)
            {
                sendStr += "01,";
            }
            else
            {
                sendStr += "00,";
            }


            double tempX1 = sendMoveX / 256;
            double tempX2 = sendMoveX % 256;

            byte[] byteMoveData1 = IntToBigEdian((int)(Math.Abs(tempX1)));
            byte[] byteMoveData2 = IntToBigEdian((int)(Math.Abs(tempX2)));

            StringBuilder sb = new StringBuilder();
            for (int i = 1; i < byteMoveData1.Length; i++)
            {
                sb.AppendFormat("{0:x1}", byteMoveData1[i]);
            }

            if (sb.ToString().ToUpper().Length == 1)
            {
                sendStr += "0" + sb.ToString().ToUpper();
            }
            else
            {
                sendStr += sb.ToString().ToUpper();
            }

            sb = new StringBuilder();

            for (int i = 1; i < byteMoveData2.Length; i++)
            {
                sb.AppendFormat("{0:x1}", byteMoveData2[i]);
            }

            if (sb.ToString().ToUpper().Length == 1)
            {
                sendStr += "0" + sb.ToString().ToUpper();
            }
            else
            {
                sendStr += sb.ToString().ToUpper();
            }

            sendStr += ",";


            //Y 전송
            if (result.MoveY < 0)
            {
                sendStr += "01,";
            }
            else
            {
                sendStr += "00,";
            }

            tempX1 = sendMoveY / 256;
            tempX2 = sendMoveY % 256;

            byteMoveData1 = IntToBigEdian((int)(Math.Abs(tempX1)));
            byteMoveData2 = IntToBigEdian((int)(Math.Abs(tempX2)));
            
            sb = new StringBuilder();
            for (int i = 1; i < byteMoveData1.Length; i++)
            {
                sb.AppendFormat("{0:x1}", byteMoveData1[i]);
            }

            if (sb.ToString().ToUpper().Length == 1)
            {
                sendStr += "0" + sb.ToString().ToUpper();
            }
            else
            {
                sendStr += sb.ToString().ToUpper();
            }

            sb = new StringBuilder();

            for (int i = 1; i < byteMoveData2.Length; i++)
            {
                sb.AppendFormat("{0:x1}", byteMoveData2[i]);
            }

            if (sb.ToString().ToUpper().Length == 1)
            {
                sendStr += "0" + sb.ToString().ToUpper();
            }
            else
            {
                sendStr += sb.ToString().ToUpper();
            }

            sendStr += ",";


            LogManager.Write("Send : " + sendStr);

            SendDataToSocket(sendStr);
        }

        /// <summary>
        /// 풀컨텐츠 결과값 전송
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private void SendFullContentResult(HFullContentsResult result)
        {
            string sendStr = Command_FullContentsFinish + ",";
            sendStr += result.Result;

            LogManager.Write("Send : " + sendStr);

            SendDataToSocket(sendStr);
        }

        private void SendCutoffCaptureResult()
        {
            string sendStr = Command_CutOffCapture + ",OK";
            SendDataToSocket(sendStr);
        }

        /// <summary>
        /// 왜곡검사 결과값 전송
        /// </summary>
        /// <param name="result"></param>
        private void SendDistortionCaptureResult()
        {
            string sendStr = Command_DistortionCapture + ",OK";
            SendDataToSocket(sendStr);
        }

        /// <summary>
        /// 풀컨텐츠 결과값 전송
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private void SendFullContentCaptureResult(HCore.HResult.RESULT result)
        {
            string sendStr = Command_FullContentsCapture + ",OK";
            SendDataToSocket(sendStr);
        }

        /// <summary>
        /// 왜곡검사 촬영 결과값 전송
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private void SendDistortionInspectionCaptureResult(HCore.HResult.RESULT result)
        {
            string sendStr = Command_DistortionInspectionCapture + ",OK";
            SendDataToSocket(sendStr);
        }

        /// <summary>
        /// 킵얼라이브 재전송
        /// </summary>
        private void SendKeepAlive()
        {
            string sendStr = Command_KeepAlive + ",1";
            SendDataToSocket(sendStr);
        }

        public void SendCameraCheckResult(HCore.HResult.RESULT result)
        {
            string sendStr = Command_CameraCheck + "," + result;
            SendDataToSocket(sendStr);
        }

        private byte[] IntToBigEdian(int data)
        {
            if (ConvertToBigEdian)
            {
                byte[] b = new byte[2];
                b[1] = (byte)data;
                b[0] = (byte)(((int)data >> 8) & 0xFF);
                return b;
            }
            else
            {
                return BitConverter.GetBytes(data);
            }
        }

        private byte[] Make_MoveData(HDistortionResult result)
        { 
            if (DistortionMenualTest)
            {
                if (result.MoveDataList == null || result.MoveDataList.Count == 0)
                {
                    for (int i = 0; i < result.VerticalDotCount; i++)
                    {
                        for (int j = 0; j < result.HorizentalDotCount; j++)
                        {
                            result.MoveDataList.Add(new Point(0, 0));
                        }
                    }
                }
            }

            List<byte> moveByteArr = new List<byte>();

            List<double> reversedXArr = new List<double>();
            List<double> reversedYArr = new List<double>();

            List<Point> listTempPos = new List<Point>();

            //X축으로 반전인지 확인.
            if (GetDistionIsReverseXArray)
            {
                listTempPos.Clear();

                for (int i = 0; i < result.VerticalDotCount; i++)
                {
                    for (int j = 1; j <= result.HorizentalDotCount; j++)
                    {
                        Point tempMoveValue = result.MoveDataList[(result.VerticalDotCount * i) + (result.HorizentalDotCount - j)];

                        listTempPos.Add(tempMoveValue);
                    }
                }
                result.MoveDataList = new List<Point>(listTempPos);
            }
             

            //Y축으로 반전인지 확인.
            if (GetDistionIsReverseYArray)
            {
                listTempPos.Clear();

                for (int i = result.VerticalDotCount - 1; i >= 0; i--)
                {
                    for (int j = 0; j < result.HorizentalDotCount; j++)
                    {
                        Point tempMoveValue = result.MoveDataList[(result.HorizentalDotCount * i) + j];

                        listTempPos.Add(tempMoveValue);
                    }
                }

                result.MoveDataList = new List<Point>(listTempPos);
            }
             
            /////////////////////테스트
            ////

            if (DistortionMenualTest)
            {
                for (int i = 0; i < result.MoveDataList.Count; i++)
                {
                    if (i == DistortionMenualPos)
                    {
                        result.MoveDataList[i] = new Point(DistortionMenualXMove, DistortionMenualYMove);
                    }
                    else
                    {
                        result.MoveDataList[i] = new Point(0, 0);
                    }
                }
            }

            //체크썸
            int checkSum = 0;

            List<Point> copy = new List<Point>(result.MoveDataList);
             
            StructCarkind carkind = StructCarkind.GetCarkind().Where(x => x.Name == InspectionInfo.CarkindFullName).ToList()[0];
            for (int i = 0; i < result.MoveDataList.Count; i++)
            {
                double currentX = result.MoveDataList[i].X;
                double currentY = result.MoveDataList[i].Y;

                if (carkind != null)
                {

                    if (carkind.IsXMinus)
                    {
                        currentX *= -1.0;
                    }

                    if (carkind.IsYMinus)
                    {
                        currentY *= -1.0;
                    }
                }

                result.MoveDataList[i] = new Point(currentX, currentY);
            }



            ////이동 값 화면 표시
            //try
            //{
            //    for (int i = 0; i < result.MoveDataList.Count; i++)
            //    {

            //        LogManager.Write(i + " : x = " + result.MoveDataList[i].X);
            //        LogManager.Write(i + " : y = " + result.MoveDataList[i].Y);
            //    }
            //}
            //catch
            //{
            //    LogManager.Write("이동량 화면 표시 실패");
            //}

            //각 좌표 접근
            for (int i = 0; i < result.MoveDataList.Count; i++)
            {
                Int16 xValue = (Int16)(result.MoveDataList[i].X * 100);
                Int16 yValue = (Int16)(result.MoveDataList[i].Y * 100);

                moveByteArr.AddRange(IntToBigEdian(xValue));
                moveByteArr.AddRange(IntToBigEdian(yValue));

                checkSum += xValue + yValue;
            }
             
            checkSum = 0xFFFF - checkSum;

            //체크썸 추가 2byte
            moveByteArr.AddRange(IntToBigEdian(checkSum));

            return moveByteArr.ToArray();
        }

        private BitmapImage RotateBitmapImgae(BitmapImage image)
        {
            
            image.Rotation = Rotation.Rotate180;
            return image;
        }
    }
}
