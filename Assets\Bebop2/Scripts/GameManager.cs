using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Text;
using Bebop.Networking.WebSocket;
using Bebop.Protocol;
using Common.Scripts;
using Common.Scripts.Localization;
using Common.Scripts.Managers;
using Newtonsoft.Json;
using UnityEngine;

namespace Bebop
{
    /// <summary>
    /// 게임의 진행을 담당한다.
    /// 이벤트 발생기 역할을 한다.
    /// 
    /// 아직 프로토콜이 정해지지 않아서 셈플을 보고 싶은 경우 Test/scripts/GameManager.cs 를 참고하는 것이 좋다.
    /// </summary>
    public partial class GameManager : MonoBehaviour
    {

        private static SocketComponent client = null;

        private enum E_ApplicationState
        {
            None,
            Deactivate,
        }

        //private E_ApplicationState applicationState = E_ApplicationState.None;


        // [SerializeField]
        // private bool NeedDirectLogin= false;
        private void Awake()
        {
            //applicationState = E_ApplicationState.None;

            //TODO: 연결방식은 논의해서...다시
            client = GetComponent<SocketComponent>();
            //Mockup 서버로 붙는 걸로 설정.
            //serverEndpoint = new ServerType("127.0.0.1","3001");
            //serverEndpoint = new ServerType("192.168.0.187","7942");
            
            //client.ConnectResultSubject.Subscribe(i=> Debug.Log("connect result:"+i.ToString()));
            //binder.DisconnectResultSubject.Subscribe(i=> Debug.Log("disconnect result:"+i.ToString()));

            BindPingAndActiveStatus();// 핑, 활성화 관련

            BindResponseToEvents();  //요청에 대한 응답을 이벤트로...
            BindNotificationToEvents();  //서버 푸시들을 이벤트로..
            
        }

        private void OnEnable() {
            
            GameManager.OnCheckOut += OnCheckoutHandler;

        }

        private void OnDisable() {

            GameManager.OnCheckOut -= OnCheckoutHandler;
            client.UnBindAll();
        }

        private void OnDestroy()
        {
            //GameManager.OnDirectLoginResponse -= OnDirectLoginResponseHandler;

            OnPingResponse = delegate { };
            OnActivatePlayerResponse = delegate { };
            OnDeactivatePlayerResponse = delegate { };
            OnChangePlayerInfo = delegate { };
            OnCheckInPlayer = delegate { };
            OnCheckOutPlayer = delegate { };
            OnHandStatusChanged = delegate { };
            OnHandStart = delegate { };
            OnCardOpen = delegate { };
            OnBoardUpdated = delegate { };
            OnReceiveBettingMessage = delegate { };
            OnKickedFromServer = delegate { };
            OnNetworkStatusChanged = delegate { };
            OnWalletResponse = delegate { };
            OnCheckOut = delegate { };
            OnDirectLoginResponse = delegate { };
            OnBettingResponse = delegate { };
            OnCancelBettingResponse = delegate { };
            OnReBettingResponse = delegate { };
            OnMyBettingHistoryResponse = delegate { };
            OnHandHistoryResponse = delegate { };
            OnGameHistoryResponse = delegate { };
            OnGameStatisticsResponse = delegate { };
            OnReceiveCurrentPlayerList = delegate { };
            OnPlayerSuccessiveRecordResponse = delegate { };
            OnPlayerWinningRateRecordResponse = delegate { };
            OnPlayerBettingSumRecordResponse = delegate { };
            OnPlayerProfitSumRecordResponse = delegate { };
        }

        //private void OnApplicationPause(bool pause)
        //{
        //    if (Application.isMobilePlatform == false)
        //        return;

        //    if (pause == false)
        //    {
        //        //SendDeactivatePlayer();
        //        SendCheckOutRequest();
        //    }
        //    else
        //    {
        //        Bebop.UI.SceneController.Load(Common.Scripts.Define.Scene.BebopLoading, () => { CloseSocket(); });
        //        //UnityEngine.SceneManagement.SceneManager.LoadScene("BebopLoader");
        //    }
        //}

        private void OnActive()
        {
            //client.OnDisconnected = null;
            SendCheckOutRequest();
            Bebop.UI.SceneController.Load(Common.Scripts.Define.Scene.Bebop);
        }

        private void OnDeactive()
        {
            
        }

        private void OnApplicationFocus(bool isFocused)
        {
//#if UNITY_EDITOR && UNITY_ANDROID

//            if (isFocused)
//            {
//                Debug.Log("===============Global OnPause ingame (Active)================");
//                OnActive();
//            }
//            else
//            {
//                Debug.Log("===============Global OnPause ingame (deactive)================");
//                //applicationState = E_ApplicationState.Deactivate;
//                OnDeactive();
//            }
//#endif
            if (isFocused)
            {
#if UNITY_IPHONE
               StopTask();
               OnActive();
#endif
                Debug.Log("===============InGame Focus true================");
            }
            else
            {

#if UNITY_IPHONE
               StartTask();
               //applicationState = E_ApplicationState.Deactivate;
               OnDeactive();
#endif
                Debug.Log("===============InGame Focus false================");
            }
        }

        private void OnApplicationPause(bool isPause)
        {
#if UNITY_ANDROID
	        
	        if (isPause == false)
	        {
	            Debug.Log("===============Global OnPause ingame (Active)================");    
                OnActive();
	        }
	        else
	        {
	            Debug.Log("===============Global OnPause ingame (deactive)================");
                //applicationState = E_ApplicationState.Deactivate;
                OnDeactive();
	        }
#endif
        }

        public static void CloseSocket()
        {
            if (client == null)
                return;

            client.Close();
        }

        /// <summary>
        /// 체크아웃 요청이 없이 중복로그인인 경우 체크아웃 응답이 온다.
        /// </summary>
        /// <param name="res"></param>
        private void OnCheckoutHandler(CheckOutResponse res)
        {
            if (res.DisconnectType == DisconnectType.Duplicate)
            {

                if (client.IsConnected)
                {
                    Debug.Log("Duplaicated Login");
                    client.Socket.Close();
                }
                else
                {
                    Debug.Log( "Checkout type : " + Enum.GetName(typeof(DisconnectType), res.DisconnectType));
                }

                //TODO: 중복로그인 처리 프로세스 추가
            }
        }


        //todo : 이 타입을 확장해서 쓸것인지...
        private ServerType serverEndpoint;
        // Start is called before the first frame update
        void Start()
        {
            Debug.Log("connecting...");
            //GameManager.OnConnected += OnConnectionHandler; //연결에 서공하면 체크인 시도한다.
            //client.Connect("ws://127.0.0.1:3001", this.onConnected,this.onDisconnected);

            var gameServerInfo = CommonManager.Instance.GetGameServerInfo(E_GameType.CowboyHoldem);

            var host = (gameServerInfo == null && Application.isEditor)
                //? "ws://192.168.0.222:7942/api/server/connect"
                ? "ws://52.68.60.29:7942/api/server/connect"
                : gameServerInfo.HostUrl;

            client.Connect(host, onConnected, onDisconnected);
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        // void OnConnectionHandler(int result){
            
        //    // if (result == (int)ResultCode.Success )
        //     //{
        //         SendCheckInRequest();
        //     //}

        // }

       


#region ==== connect  / disconnect event =====
 //       public static event Action<int> OnConnected;

//        public static event Action<int> OnDisconnected;
        /// <summary>
        /// TODO: result 가 항상 0 이 온다. 조사해서 해결해야 함.
        /// </summary>
        /// <param name="result"></param>
        void onConnected(int result)
        {
            Debug.Log("OnConnected fired :"+ result.ToString());
            OnNetworkStatusChanged.Invoke(NetworkStatus.Connected);

            if (Application.isEditor)
            {

                var directLoginSetting = GetComponent<DirectLogin>();
                if (directLoginSetting.NeedDirectLogin)
                {
                    GameManager.OnDirectLoginResponse += OnDirectLoginResponseHandler;
                    
                    var dto = new Bebop.Protocol.DirectLoginRequest();

                    dto.BrandId = directLoginSetting.BandId;
                    //dto.RememberMeMinutes = 100;
                    dto.ClientDetail = new Protocol.ClientDetail();

                    dto.ClientDetail.Locale = Application.systemLanguage.ToString();
                    dto.ClientDetail.OsType = SystemInfo.operatingSystemFamily.ToString();
                    dto.ClientDetail.OsVersion = SystemInfo.operatingSystem;
                    dto.ClientDetail.ClientId = SystemInfo.deviceUniqueIdentifier;
                    dto.ClientDetail.ClientType = SystemInfo.deviceType.ToString();
                    dto.ClientDetail.ClientVersion = Application.version;
                    dto.ClientDetail.DeviceId = SystemInfo.deviceUniqueIdentifier;
                    dto.ClientDetail.DeviceType = SystemInfo.deviceType.ToString();
                    dto.ClientDetail.DeviceModel = SystemInfo.deviceModel;
                    dto.ClientDetail.SystemMemorySize = SystemInfo.systemMemorySize;

                    object loginValue;
                    
                    if (directLoginSetting.AccessToken != "")
                    {
                        loginValue = new {AccessToken = directLoginSetting.AccessToken};
                    }
                    else if (!string.IsNullOrWhiteSpace(directLoginSetting.ID )&&
                            !string.IsNullOrWhiteSpace(directLoginSetting.PWD ))
                    {
                        loginValue = new { Username = directLoginSetting.ID, Password = directLoginSetting.PWD };
                        //loginValue = new { Username = TempDirectLoginUser.Id, Password = 1 };
                    }
                    else
                    {
                        throw new InvalidOperationException("에디터 로그인 실패");
                    }
                
                    
                    string tokenValue = JsonConvert.SerializeObject(loginValue);
                    string token = Convert.ToBase64String(Encoding.UTF8.GetBytes(tokenValue));

                    Debug.Log("DirectLogin LoginObject:"+ tokenValue);
                    Debug.Log("DirectLogin Token:"+ token);

                    dto.Token = token;

                    client.Send(Bebop.Protocol.BebopProtocolId.DirectLogin,dto);
                }
                else
                {
                    SendCheckInRequest(); 
                }
            }
            else
                SendCheckInRequest(); // 연결되면 바로 체크인을 시도한다.

            // if (result ==200)
            // {
            //     OnNetworkStatusChanged.Invoke(Bebop.NetworkStatus.Connected);
            //     SendCheckInRequest(); // 연결되면 바로 체크인을 시도한다.
            // }
            // else
            // {
            //     //TODO: retry 혹은 오류 메세지 처리..
            //     throw new Exception("result is "+ result.ToString() + ", it's mean "+ Enum.GetName(typeof(ResultCode),result));
            // }
        }

        private void OnDirectLoginResponseHandler(DirectLoginResponse res)
        {
            if (res.Result == ResultCode.Success)
            {
                var checkIn = new CheckInRequest {Token = res.GameToken};
                client.Send(BebopProtocolId.CheckIn,checkIn);

                //-> 테스트
                clientTime = DateTime.UtcNow;
            }
            else
            {
                Debug.LogError( Enum.GetName( typeof(ResultCode),res.Result));
                BebopMessagebox.Ok("Error", LocalizationManager.Instance.GetText("common_error_unknown"), ()=>
                {
                    GameObject.DestroyImmediate(BebopMessagebox.Instance.gameObject);
                    //GameManager.SendCheckOutRequest();
                    Common.Scripts.GameSwitch.Load(Common.Scripts.Managers.E_GameType.Fishing, Common.Scripts.Define.Scene.IntegratedLobby);
                });
            }
            
        }

        void onDisconnected(int result)
        {
            Debug.Log("OnDisconnectd fired :"+result.ToString());

            //if (applicationState == E_ApplicationState.Deactivate)
            //{
            //    applicationState = E_ApplicationState.None;
            //    return;
            //}

            if (BebopMessagebox.Instance != null)
                BebopMessagebox.Ok("", LocalizationManager.Instance.GetText("DisconnectedFromServer"), ()=>
                {
                    GameObject.DestroyImmediate(BebopMessagebox.Instance.gameObject);
                    //GameManager.SendCheckOutRequest();
                    Common.Scripts.GameSwitch.Load(Common.Scripts.Managers.E_GameType.Fishing, Common.Scripts.Define.Scene.IntegratedLobby);
                });

            OnNetworkStatusChanged.Invoke(NetworkStatus.Disconnected);
        }

#endregion

        //-> 동기화 문제로 급히 만들었습니다.
        //-> 서버에서 1초 먼저 상태변화 패킷을 줍니다.
#region server timer
        static DateTime serverTime;
        static DateTime clientTime;

        //-> 서버 스냅샷, 핑 의 커런트 타임 + 패킷 왕복시간 * 0.5 로 서버시간 셋팅
        //-> 서버 스타트 시간
        void SetServerTime(DateTime serverTime)
        {
            GameManager.serverTime = serverTime;
            GameManager.clientTime = DateTime.UtcNow;
        }

        void ScheduleInvoke(DateTime stateInAt, System.Action execute)
        {
            StartCoroutine(ScheculeExecute(stateInAt, execute));
        }

        IEnumerator ScheculeExecute(DateTime stateInAt, System.Action execute)
        {
            //-> 클라에서 흐른 시간만큼 서버 스타트 시간에 더해 시작 딜레이를 계산한다.
            var timeTerm = DateTime.UtcNow - clientTime;
            TimeSpan serverTimeTerm = stateInAt - (serverTime + timeTerm);

            float delay = Convert.ToSingle(serverTimeTerm.TotalSeconds);
            Debug.Log("delay : " + delay);
            if (delay > 0)
                yield return new WaitForSecondsRealtime(delay);

            execute.Invoke();
        }

        public static DateTime GetCurrentServerTime()
        {
            var timeTerm = DateTime.UtcNow - clientTime;
            return serverTime + timeTerm;
        }
#endregion

#if UNITY_IPHONE

        [DllImport("__Internal")]
        private static extern void backgroundLaunch();
        [DllImport("__Internal")]
        private static extern void backgroundStop();

        // Start background task
        public static void StartTask()
        {
            if (Application.isEditor == false)
            {
                Debug.Log("============= BackgroundLaunch ===============");
                backgroundLaunch();    
            }
            
        }

        // Stop background task
        public static void StopTask()
        {
            if (Application.isEditor == false)
            {
                Debug.Log("============= BackgroundStop ===============");
                backgroundStop();
            }
        }

#endif
    }





}

