using System;
using System.Collections.Generic;
using System.Linq;
using Bebop.Model.EventParameters;
using Bebop.Protocol;
using Common.Scripts.Localization;
using Common.Scripts.Managers;
using Newtonsoft.Json;
using UnityEngine;

namespace Bebop
{
    /// <summary>
    /// 여기서는 Request/Response 들 만 다룬다. - by ken
    /// </summary>
    public partial class GameManager : MonoBehaviour
    {


        /// <summary>
        /// 네트웍 상태 변경 및 CheckIn 성공(Initialize)
        /// </summary>
        public static event Action<NetworkStatus> OnNetworkStatusChanged = delegate {};


    #region ====== Wallet ==========

    /// <summary>
    /// 월렛 정보 조회
    /// </summary>
    public static void GetWallet()
    {
        var dto = new WalletRequest();
        
        client.Send(BebopProtocolId.Wallet,dto);

    }

    /// <summary>
    /// 월렛 요청에 대한 응답 이벤트
    /// </summary>
    public static event Action<WalletResponse> OnWalletResponse = delegate {};


    #endregion ====== Wallet =========


    #region ====== CheckIn/Out ==============

   


        public static void SendCheckInRequest()
        {
//#if BEBOP
            //var token = "181818";
//#else
            var gameServerInfo = CommonManager.Instance.GetGameServerInfo(E_GameType.CowboyHoldem);

            //reconnect
            if (gameServerInfo != null && gameServerInfo.Token == null)
            {
                GetNewTokenAndCheckInRequest();
                return;
            }

            if (gameServerInfo == null && Application.isEditor)
            {
                gameServerInfo = new GameServerInfo();
                gameServerInfo.Token =  "6";
            }

            var token = gameServerInfo.Token;
//#endif
            var checkIn = new CheckInRequest {Token = token};
            client.Send(BebopProtocolId.CheckIn,checkIn);
            gameServerInfo.Token = null;

            //-> 테스트
            clientTime = DateTime.UtcNow;
        }

        
        private static void GetNewTokenAndCheckInRequest()
        {
#if !BEBOP
            Oasis.Scripts.Network.OasisHttp.LobbyClient.EnterSheriff(response =>
            {
                Debug.Log($"Enter sheriff {response.Token}, {response.HostUrl}");
                CommonManager.Instance.RegisterGameServerInfo(
                    E_GameType.CowboyHoldem,
                    new GameServerInfo
                    {
                        HostPort = 0,
                        HostUrl = response.HostUrl,
                        Token = response.Token,
                    }
                );
                SendCheckInRequest();
            }, error =>
            {
                BebopMessagebox.Ok("Error", LocalizationManager.Instance.GetText("common_error_unknown"));
            });
#else
            Debug.Log("reconnect token error");       
#endif
        }
        

        /// <summary>
        /// 체크인 응답, 체크인 응답은 이밴트로 내보내지 않고 각 개별 단위 이벤트들을 하나씩 발생시킨다.
        /// </summary>
        //public static event Action<CheckInResponse> OnCheckIn = delegate {} ; //TODO: 이 이벤트는 여러가지 단위 이벤트로 나눠서 발생 시킨다.

        public static void SendCheckOutRequest()
        {
            //-> 체크아웃을 보낼 때 delegate를 널 시켜 disconnect 팝업 메세지를 뜨지 못하게 하자.
            client.OnDisconnected = null;
            var checkIn = new CheckOutRequest();
            client.Send(BebopProtocolId.CheckOut,checkIn);
        }

        /// <summary>
        /// 페트아웃 응답
        /// </summary>
        public static event Action<CheckOutResponse> OnCheckOut = delegate {} ;



        public static void SendDirectLogin( DirectLoginRequest dto)
        {
            //if (Edi)
        }

        public static event Action<DirectLoginResponse> OnDirectLoginResponse = delegate {};

        #endregion

        #region ===== Betting ======

        /// <summary>
        /// 배팅하기
        /// </summary>
        /// <param name="type">어디에</param>
        /// <param name="amount">얼마나</param>
        public static void SendBettingRequest(BettingType type, CoinType coinType, int coinCount)
        {
            BettingInfo req = new BettingInfo() { BetType = type, CoinType = coinType, CoinCount = coinCount };
            SendBettingRequest(req);
        }

        public static void SendBettingRequest(BettingInfo bettingReqs)
        {
            var dto = new BettingRequest();

            dto.BetInfo = bettingReqs;

            client.Send(BebopProtocolId.Betting, dto);
        }

        /// <summary>
        /// 내 배팅 요청에 대한 응답
        /// </summary>
        public static event Action<BettingResponse> OnBettingResponse = delegate {};


        /// <summary>
        /// 배팅 취소
        /// </summary>
        /// <param name="id">배팅 응답(BettingResponse) 안에 BettingInfo 객체가 갖고 있는 id 값을 취소 요청 때 보낸다.</param>
        public static void CancelBettingRequest()
        {
            var dto = new BettingCancelRequest();

            client.Send(BebopProtocolId.BettingCancel,dto);
        }

        /// <summary>
        /// 배팅 취소 요청에 대한 응답
        /// </summary>
        public static event Action<BettingCancelResponse> OnCancelBettingResponse = delegate {};



        /// <summary>
        /// 리배팅, 더블 배팅
        /// </summary>
        public static void SendReBettingRequest()
        {
            var dto = new ReBetRequest();

            client.Send(BebopProtocolId.ReBet, dto);
        }

        /// <summary>
        /// 리배팅, 더블 배팅 요청에 대한 응답
        /// </summary>
        public static event Action<ReBetResponse> OnReBettingResponse = delegate { };

        #endregion

        #region ====== Betting-history , hand history  =====



        /// <summary>
        /// 내 배팅 기록을 조회
        /// </summary>
        public static void GetMyBettingHistory(long startHandId, int count)
        {
            var dto = new MyBettingHistoriesRequest() { StartHandId = startHandId, Count = count };
            client.Send(BebopProtocolId.MyBettingHistory, dto);
        }

        /// <summary>
        /// 내 배팅 기록 조회에 대한 응답
        /// </summary>
        public static event Action<MyBettingHistoriesResponse> OnMyBettingHistoryResponse = delegate {};


        public static void GetHandHistory()
        {
            var dto = new HandHistoryRequest();
            client.Send(BebopProtocolId.HandHistory, dto);
        }


        public static event Action<HandHistoryResponse> OnHandHistoryResponse = delegate {};

    #endregion

    #region ====== 각종 팝업 창 ======

         /// <summary>
        /// 매 정보 요청(Roadmap)
        /// </summary>
        public static void GetGameHistory()
        {
            var dto = new GameHistoryRequest();
            
            client.Send(BebopProtocolId.GameHistory,dto);
        }

        /// <summary>
        /// 매 정보 응답 이벤트
        /// </summary>
        public static event Action<GameHistoryResponse> OnGameHistoryResponse = delegate {};


        /// <summary>
        /// 게임 통계 정보 조회
        /// 로드맵 창에 통계 탭 구현을 위한...
        /// </summary>
        public static void GetGameStatistics()
        {
            var dto = new GameStatisticsRequest();

            client.Send(BebopProtocolId.GameStatistics,dto);
        }
      
        /// <summary>
        /// 게임 통계 응답
        /// </summary>
        public static event Action<GameStatisticsResponse> OnGameStatisticsResponse = delegate {};

        /// <summary>
        /// 현재 접속중인 플레이어 리스트
        /// 페이징을 위한 파라미터를 전달해 줘야 한다.
        /// OnReceiveCurrentPlayerList 이벤트로 응답을 받는다.
        /// </summary>
        /// <param name="page">요청할 페이지 , 0부터 시작</param>
        /// <param name="size">한 페이지 당 사이즈, 기본값은 10</param>
        public static void GetCurrentPlayerList(int page, int size=16)
        {
            var dto = new PlayerListRequest();
            
            dto.Page = page;
            dto.PageSize = size;

            client.Send(BebopProtocolId.PlayerList,dto);
        }
        
        /// <summary>
        /// GetCurrentPlayerList() 요청에 대한 응답.
        /// </summary>
        public static event Action<PlayerListResponse> OnReceiveCurrentPlayerList = delegate {};

        
        /// <summary>
        /// 플레이어 연승기록 리스트 조회
        /// </summary>
        /// <param name="type">오늘 or 한달</param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        public static void GetPlayerSuccessiveRecord(InqueryType type, int page, int size=16)
        {
            var dto = new PlayerSuccessiveRecordRequest();
            //dto.Page = page;
            //dto.PageSize = size;
            dto.InqueryType = type;

            client.Send(BebopProtocolId.PlayerSuccessiveRecord,dto);
        }

        /// <summary>
        /// 플레이어 연승기록 리스트 조회 응답
        /// </summary>
        public static event Action<PlayerSuccessiveRecordResponse> OnPlayerSuccessiveRecordResponse = delegate {};

        /// <summary>
        /// 플레이어 승률기록 요청
        /// </summary>
        /// <param name="type"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        public static void GetPlayerWinningRateRecord(InqueryType type ,int page, int size=16)
        {
            var dto = new PlayerWinningRateRecordRequest();
            dto.InqueryType = type;
            //dto.Page = page;
            //dto.PageSize = size;

            client.Send(BebopProtocolId.PlayerWinningRateRecord,dto);
        }

        /// <summary>
        /// 플레이어 승률 기록 응답
        /// </summary>
        public static event Action<PlayerWinningRateRecordResponse> OnPlayerWinningRateRecordResponse = delegate {};

        /// <summary>
        /// 플레이어 배팅금 합계 리스트 요청
        /// </summary>
        /// <param name="type"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        public static void GetPlayerBettingSumRecord(InqueryType type, int page, int size=16)
        {
            var dto = new PlayerBettingSumRecordRequest();
            dto.InqueryType = type;
            //dto.Page = page;
            //dto.PageSize = size;

            client.Send(BebopProtocolId.PlayerBettingSumRecord,dto);
        }

        /// <summary>
        /// 플레이어 배팅금 합계 리스트 응답
        /// </summary>
        public static event Action<PlayerBettingSumRecordResponse> OnPlayerBettingSumRecordResponse = delegate {};

        /// <summary>
        /// 플레이어 Profit합계 요청
        /// </summary>
        /// <param name="type"></param>
        /// <param name="page"></param>
        /// <param name="size"></param>
        public static void GetPlayerProfitSumRecord(InqueryType type , int page, int size=16)
        {
            var dto = new PlayerProfitSumRecordRequest();
            dto.InqueryType = type;
            //dto.Page = page;
            //dto.PageSize = size;

            client.Send(BebopProtocolId.PlayerProfitSumRecord,dto);
        }

        /// <summary>
        /// 플레이어 profit 합계 응답
        /// </summary>
        public static event Action<PlayerProfitSumRecordResponse> OnPlayerProfitSumRecordResponse = delegate {};


        

    #endregion
       

        /// <summary>
        /// 체크인 시 스냅샷 정보를 받아 반영시킨다.
        /// TODO: 델리게이트를 이용해 순차적 프로세스를 정리하자.
        /// </summary>
        /// <param name="res"></param>
        private void CheckInProcess(CheckInResponse res)
        {
            if (res.Result != ResultCode.Success)
            {
                Debug.Log(res.Result.ToString());
                BebopMessagebox.Ok("Error", LocalizationManager.Instance.GetText("common_error_unknown"), ()=>
                {
                    GameObject.DestroyImmediate(BebopMessagebox.Instance.gameObject);
                    //GameManager.SendCheckOutRequest();
                    Common.Scripts.GameSwitch.Load(Common.Scripts.Managers.E_GameType.Fishing, Common.Scripts.Define.Scene.IntegratedLobby);
                });
                return;
            }
                

            var rtt = DateTime.UtcNow - GameManager.clientTime;
            SetServerTime(res.HandSnapShot.Current + TimeSpan.FromSeconds(rtt.TotalSeconds * 0.5f));
            
            Debug.Log("recevie CheckIn:"+ JsonConvert.SerializeObject(res,Formatting.Indented));

            UserData.Instance.AccountInfo = res.MyInfo.PlayerThumbnail.AccountInfo;
            UserData.Instance.Wallet = res.MainWallet;

            Scripts.UI.MyInfo.Instance.UpdateWallet(res.MainWallet);

            //Scripts.UI.MyInfo.Instance.SetNational(res.MyInfo.AccountInfo.NationalFlag);
            Scripts.UI.MyInfo.Instance.labelNickname.text = res.MyInfo.PlayerThumbnail.AccountInfo.NickName;
            WebImageDownloadManager.Instance.GetImage(res.MyInfo.PlayerThumbnail.AccountInfo.Avatar, (sprite) =>
            {
                Scripts.UI.MyInfo.Instance.userAvatar.sprite = sprite;
            });

            //Debug.Log(OnCheckIn?.GetInvocationList().Length);

            //NOTE :  캐치업 정보를 분리해 해당 이벤트들을 호출해 준다.
            OnNetworkStatusChanged.Invoke(NetworkStatus.Initialized); //여기까지를 초기화 완료로 본다.

            if (res.HandSnapShot == null) throw new InvalidOperationException("the Hand Snapshsot is null in Check-in process");

            var handStatusArgs = new HandStatusArgs(
                                            res.HandSnapShot.HandState,
                                            res.HandSnapShot.StateInAt,
                                            res.HandSnapShot.Period,
                                            WaitingReason.None,
                                            res.HandSnapShot.Current  // 캐치업 인  경우 만 값이 할당 됨.
                                            );


            OnHandStatusChanged.Invoke(handStatusArgs,true);

            //-> 배팅 패널을 업데이트 전에 플레이어 목록을 먼저 업데이트
            {
                NotifyHandStateBoardUpdateIn param = new NotifyHandStateBoardUpdateIn();

                //param.GodBrain = res.HandSnapShot.GodBrain;
                //param.Supernova = res.HandSnapShot.Supernova;
                //param.TopPlayers = res.HandSnapShot.TopPlayers;

                param.PlayersRank = new Dictionary<PlayerType, PlayerThumbnail>();
                var itor = res.HandSnapShot.PlayersBetState.States.GetEnumerator();
                while (itor.MoveNext())
                {
                    param.PlayersRank.Add(itor.Current.Key, itor.Current.Value.PlayerThumbnail);
                }

                //param.MeaHistory = res.HandSnapShot.MeaHistory;
                param.Period = res.HandSnapShot.HandState == HandState.HandStart ? res.HandSnapShot.Period : 0; //상태가 일치하는 경우만 period 를 설정하고 아니면 0 으로..
                param.StateInAt = res.HandSnapShot.StateInAt;
                param.BetResultHistories = res.HandSnapShot.BetResultHistories;
                param.LastWinningIndex = res.HandSnapShot.LastWinningIndex;

                OnBoardUpdated.Invoke(param, true);
            }


            //캐치업... 진행된 이벤트들을 만들어 발생시킨다.
            if (res.HandSnapShot.HandState.CompareTo(HandState.HandStart) > -1)
            {
                NotifyHandStateHandStartIn param = new NotifyHandStateHandStartIn();
                param.HandId = res.HandSnapShot.HandId;

                OnHandStart.Invoke(param , true);
            }

            //배팅 판 정보를 셋팅하기 위해 배팅정보 이벤트를 발생시킨다.
            if (res.HandSnapShot.HandState.CompareTo(HandState.Betting) > -1)
            {
                //-> 내 배팅
                var mySnap = new BettingResponse();
                mySnap.Result = ResultCode.Success;
                mySnap.BetStates = res.MyInfo.BetStates;
                mySnap.IsSanpShot = true;
                OnBettingResponse.Invoke(mySnap);

                //-> 플레이어 배팅
                var playersBetStates = res.HandSnapShot.PlayersBetState;

                //-> 내가 탑플레이어인지 확인
                bool isTopPlayer = false;
                foreach (var playerBetState in playersBetStates.States)
                {
                    if (playerBetState.Key == PlayerType.NormalPlayers)
                        continue;

                    if (UserData.Instance.AccountId == playerBetState.Value.PlayerThumbnail.AccountInfo.AccountId)
                    {
                        isTopPlayer = true;
                        break;
                    }
                }

                //-> 내가 일반 플레이어라면 내 동전 개수를 뺀다.
                if (isTopPlayer == false)
                {
                    foreach (var betState in res.MyInfo.BetStates.States)
                    {
                        foreach (var coinState in betState.Value.States)
                        {
                            playersBetStates.States[PlayerType.NormalPlayers].BetStates.States[betState.Key].States[coinState.Key].CoinCount -= coinState.Value.CoinCount;
                        }
                    }
                }

                foreach (var playerBetState in playersBetStates.States.Values)
                {
                    //-> 내동전 빼기
                    if (UserData.Instance.AccountId == playerBetState.PlayerThumbnail.AccountInfo.AccountId)
                        continue;

                    var snapNoti = new NotifyBettingPlayer();
                    snapNoti.AccountId = playerBetState.PlayerThumbnail.AccountInfo.AccountId;
                    snapNoti.BetStates = playerBetState.BetStates;
                    snapNoti.Cancel = false;
                    snapNoti.IsSnapShot = true;

                    var bettingEvent = new BettingEventArgs(snapNoti);

                    OnReceiveBettingMessage.Invoke(bettingEvent);
                }

                //var fakeNotifyBettingPlayer = new NotifyBettingPlayer();
                //fakeNotifyBettingPlayer.AccountId = 0;
                //fakeNotifyBettingPlayer.BetStates = null;

                //var betEventArgs = new BettingEventArgs(fakeNotifyBettingPlayer);

                //OnReceiveBettingMessage.Invoke(betEventArgs, true);
            }
            

            if (res.HandSnapShot.HandState.CompareTo(HandState.CardOpen) > 1)
            {
                NotifyHandStateCardOpenIn cardOpenParam = new NotifyHandStateCardOpenIn();
                cardOpenParam.BullBestCardInfo = res.HandSnapShot.BullBestCardInfo;
                cardOpenParam.CowboyBestCardInfo = res.HandSnapShot.CowboyBestCardInfo;
                cardOpenParam.DealCards = res.HandSnapShot.DealCards;
                cardOpenParam.Period = res.HandSnapShot.Period;
                cardOpenParam.StateInAt = res.HandSnapShot.StateInAt;
                cardOpenParam.WinningBettingTypes = res.HandSnapShot.WinningBettingTypes;

                OnCardOpen.Invoke(cardOpenParam,true);
            }

            OnGameStart.Invoke();
        }

        public static event Action OnGameStart = delegate { };

        //TODO : 제너릭 하게 바꿀껄...
        private void RaiseCheckOut(CheckOutResponse res)
        {
            
           
            OnCheckOut.Invoke(res);
           
        }

        private void RaiseWalletResponse(WalletResponse res)
        {
            OnWalletResponse.Invoke(res);
        }

        private void RaiseBettingResponse(BettingResponse res)
        {
            OnBettingResponse.Invoke(res);
            OnWalletResponse.Invoke(new WalletResponse() { Result = res.Result, MainWallet = res.MainWallet });
        }

        private void RaiseCancelBettingResponse(BettingCancelResponse res)
        {
            OnCancelBettingResponse.Invoke(res);
            OnWalletResponse.Invoke(new WalletResponse() { Result = res.Result, MainWallet = res.MainWallet });
        }

        private void RaiseReBettingResponse(ReBetResponse res)
        {
            //OnReBettingResponse.Invoke(res);
            BettingResponse bettingRes = new BettingResponse()
            {
                BettingAt = res.BettingAt,
                BetStates = res.BetStates,
                MainWallet = res.MainWallet,
                Result = res.Result
            };
            OnBettingResponse.Invoke(bettingRes);
            OnWalletResponse.Invoke(new WalletResponse() { Result = res.Result, MainWallet = res.MainWallet });
        }


        private void RaiseMyBettingHistoryResponse(MyBettingHistoriesResponse res)
        {
            OnMyBettingHistoryResponse.Invoke(res);
        }

        private void RaiseHandHistoryResponse(HandHistoryResponse res)
        {
            OnHandHistoryResponse.Invoke(res);
        }


        private void RaisePlayStatisticsResponse(PlayerListResponse res)
        {
            OnReceiveCurrentPlayerList.Invoke(res);
        }

        private void RaiseGameHistoryResponse(GameHistoryResponse res)
        {
            OnGameHistoryResponse.Invoke(res);
        }

        private void RaiseGameStatisticsResponse(GameStatisticsResponse res)
        {
            OnGameStatisticsResponse.Invoke(res);
        }

        private void RaisePlayerSuccessiveResponse(PlayerSuccessiveRecordResponse res)
        {
            OnPlayerSuccessiveRecordResponse.Invoke(res);
        }

        private void RaisePlayerWinningRateRecordResponse(PlayerWinningRateRecordResponse res)
        {
            OnPlayerWinningRateRecordResponse.Invoke(res);
        }

        private void RaisePlayerBettingSumRecordResponse(PlayerBettingSumRecordResponse res)
        {
            OnPlayerBettingSumRecordResponse.Invoke(res);
        }

        private void RaisePlayerProfitSumRecordResponse(PlayerProfitSumRecordResponse res)
        {
            OnPlayerProfitSumRecordResponse.Invoke(res);
        }

        private void RaiseDirectLoginResponse(DirectLoginResponse res)
        {
            OnDirectLoginResponse.Invoke(res);
        }


        /// <summary>
        /// 요청에 대한 응답들을 이벤트에 연결
        /// TODO: 이런 노가다를 ...쉽게 할 방법은? 숏컷을  만들던가..
        /// </summary>
        private void BindResponseToEvents()
        {
            //client.BindProtocol<Hello>(BebopProtocolId.Hello, RaiseHello);
            client.BindProtocol<CheckInResponse>(BebopProtocolId.CheckIn,CheckInProcess);
            client.BindProtocol<CheckOutResponse>(BebopProtocolId.CheckOut,RaiseCheckOut);

            client.BindProtocol<WalletResponse>(BebopProtocolId.Wallet,RaiseWalletResponse);


            //배팅
            client.BindProtocol<BettingResponse>(BebopProtocolId.Betting, RaiseBettingResponse);
            client.BindProtocol<BettingCancelResponse>(BebopProtocolId.BettingCancel, RaiseCancelBettingResponse);
            client.BindProtocol<ReBetResponse>(BebopProtocolId.ReBet, RaiseReBettingResponse);
            client.BindProtocol<MyBettingHistoriesResponse>(BebopProtocolId.MyBettingHistory, RaiseMyBettingHistoryResponse);

            //handhistory
            client.BindProtocol<HandHistoryResponse>(BebopProtocolId.HandHistory,RaiseHandHistoryResponse);

            //팝업창들
            client.BindProtocol<PlayerListResponse>(BebopProtocolId.PlayerList,RaisePlayStatisticsResponse);
            client.BindProtocol<PlayerSuccessiveRecordResponse>(BebopProtocolId.PlayerSuccessiveRecord,RaisePlayerSuccessiveResponse);
            client.BindProtocol<PlayerWinningRateRecordResponse>(BebopProtocolId.PlayerWinningRateRecord,RaisePlayerWinningRateRecordResponse);
            client.BindProtocol<PlayerBettingSumRecordResponse>(BebopProtocolId.PlayerBettingSumRecord,RaisePlayerBettingSumRecordResponse);
            client.BindProtocol<PlayerProfitSumRecordResponse>(BebopProtocolId.PlayerProfitSumRecord,RaisePlayerProfitSumRecordResponse);


            client.BindProtocol<GameHistoryResponse>(BebopProtocolId.GameHistory,RaiseGameHistoryResponse); //매정보
            client.BindProtocol<GameStatisticsResponse>(BebopProtocolId.GameStatistics,RaiseGameStatisticsResponse); //

            client.BindProtocol<DirectLoginResponse>(BebopProtocolId.DirectLogin,RaiseDirectLoginResponse);

            
        }
    }

}
