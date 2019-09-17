using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Linq;
using Bebop.Model.EventParameters;
using Common.Scripts.Sound.Managers;
using Common.Scripts.Localization;

namespace Bebop
{
    public enum E_BettingAmount
    {
        Betting_1 = 1,
        Betting_5 = 5,
        Betting_10 = 10,
        Betting_100 = 100,
        Betting_1000 = 1000,
    }

    public class BettingController : MonoBehaviour
    {
        public static BettingController Instance { get; private set; }

        public BettingButton betButton;
        public BettingPanel betPanel;
        public BettingCoins betCoins;
        public MainHistory mainHistory;
        public Roadmap roadMap;

        private E_BettingAmount currentAmount = E_BettingAmount.Betting_1;

        private int myPreBettingCount = 0;
        private int myBettingCount = 0;

        //private Protocol.BetStates myPreBettingInfos = new Protocol.BetStates();
        //private Protocol.BetStates myBettingInfos = new Protocol.BetStates();
        //private Stack<List<long>> stkBettingInfo = new Stack<List<long>>();
        //private Queue<List<long>> queRemoveBettingInfo = new Queue<List<long>>();
        //private Dictionary<Protocol.BettingType, long> dicBettingAmountLimit = new Dictionary<Protocol.BettingType, long>();
        //private Dictionary<Protocol.BettingType, long> dicBettingAmount = new Dictionary<Protocol.BettingType, long>();

        //-> https://ggnetwork.atlassian.net/wiki/spaces/SID/pages/1326925/Cowboy+Holdem
        private readonly long[] arrBettingLimit =
            {
                100000, 100000, 20000,
                100000, 100000, 50000, 20000, 10000,
                100000, 100000, 100000, 20000, 10000,
            };

        private long handId;
        private Protocol.HandState currentHandState;

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            betButton.SetController(this);
            betPanel.SetController(this);
            mainHistory.SetController(this);

            var itor = betPanel.GetEnumerator();
            while(itor.MoveNext())
            {
                betCoins.RegisterBettingPlace(itor.Current.type, itor.Current.button.targetGraphic.GetComponent<RectTransform>());
            }

            RegisterResponseEvent();

            int limitIndex = 0;
            foreach(Protocol.BettingType type in System.Enum.GetValues(typeof(Protocol.BettingType)))
            {
                if (type == Protocol.BettingType.None)
                    continue;

                //dicBettingAmountLimit.Add(type, arrBettingLimit[limitIndex++]);
                //dicBettingAmount.Add(type, 0);
            }
        }

        private void Init()
        {
            //-> 각 패널 별 베팅액 초기화
            //var lstAmountKey = dicBettingAmount.Keys.ToList();
            //for (int index = 0; index < lstAmountKey.Count; ++index)
            //{
            //    var key = lstAmountKey[index];
            //    dicBettingAmount[key] = 0;
            //}

            //stkBettingInfo.Clear();
            //queRemoveBettingInfo.Clear();

            //-> 이전 판의 베팅 기록을 기억한다.
            //if (myBettingInfos.States.Count > 0 )
            //{
            //    myPreBettingInfos.States.Clear();
            //    myPreBettingInfos.States = myBettingInfos.States;
            //}

            //myBettingInfos.States.Clear();

            if (myBettingCount > 0)
            {
                myPreBettingCount = myBettingCount;
            }
            
            myBettingCount = 0;

            //-> 이전 베팅 기록이 있을 때 리핏버튼 활성화
            bool isRepeat = myPreBettingCount > 0;
            betButton.SetBettingOptionButtonState(BettingButton.E_ButtonState.Repeat, isRepeat);

            betPanel.Reset();
            betCoins.Reset();
        }

        private void RegisterResponseEvent()
        {
            GameManager.OnBettingResponse += ResponseBetting;
            GameManager.OnCancelBettingResponse += ResponseCancelBetting;
            GameManager.OnHandStatusChanged += HandStatusChanged;
            GameManager.OnReceiveBettingMessage += ReceiveBettingMessage;
            GameManager.OnHandStart += HandStart;
            GameManager.OnCardOpen += CardOpen;
            GameManager.OnGameHistoryResponse += GameHistoryResponse;
            GameManager.OnBoardUpdated += BoardUpdated;
        }

        private void OnDestroy()
        {
            GameManager.OnBettingResponse -= ResponseBetting;
            GameManager.OnCancelBettingResponse -= ResponseCancelBetting;
            GameManager.OnHandStatusChanged -= HandStatusChanged;
            GameManager.OnReceiveBettingMessage -= ReceiveBettingMessage;
            GameManager.OnHandStart -= HandStart;
            GameManager.OnCardOpen -= CardOpen;
            GameManager.OnGameHistoryResponse -= GameHistoryResponse;
            GameManager.OnBoardUpdated -= BoardUpdated;
        }

        public void OnClickBettingButton(E_BettingAmount type)
        {
            Debug.Log(type);

            SoundManager.PlaySFX(SFX.ClickButton);

            currentAmount = type;
        }

        //-> 테스트용
        //private Protocol.BettingResponse GetBettingInfo(Protocol.BettingType type, long amount)
        //{
        //    var res = new Protocol.BettingResponse();
        //    var bettingInfo = new Protocol.BettingInfo();
        //    bettingInfo.BettingType = type;
        //    bettingInfo.Amount = amount;
        //    res.BettingInfo = bettingInfo;
        //    res.MyBettingInfos = new List<Protocol.BettingInfo>();
        //    res.MyBettingInfos.Add(bettingInfo);

        //    return res;
        //}

        public void OnClickMainHistory()
        {
            SoundManager.PlaySFX(SFX.ClickButton);

            //-> 이미 이번판 히스토리 셋팅했다
            if (roadMap.HandId == handId)
            {
                roadMap.SetActive(true);
                return;
            }

            Bebop.UI.WaitIndicator.SetActive(true, 0f);
            GameManager.GetGameHistory();
        }

        public void OnClickBettingPanel(Protocol.BettingType type)
        {
            Debug.Log(type);

            SoundManager.PlaySFX(SFX.ClickButton);

            //-> 서버에서 응답 받을 때까지 락
            betPanel.Disable(type);

            GameManager.SendBettingRequest(type, (Protocol.CoinType)((int)currentAmount * 100), 1);

            betButton.SetEnableSideButtons(false);

            //-> 테스트용
            //this.ResponseBetting(GetBettingInfo(type, (long)currentBetting));
        }

        public void OnClickCancel()
        {
            Debug.Log("Cancel Betting");

            SoundManager.PlaySFX(SFX.ClickButton);

            GameManager.CancelBettingRequest();

            //betButton.SetInteractableCancelButton(false);
            //betButton.SetInteractableBettingOptionButton(false);

            betButton.SetEnableSideButtons(false);
        }

        public void OnClickRepeat()
        {
            Debug.Log("Repeat");

            SoundManager.PlaySFX(SFX.ClickButton);

            if (myPreBettingCount > 0)
            {
                GameManager.SendReBettingRequest();
                betButton.SetEnableSideButtons(false);
            }
        }

        public void OnClickDouble()
        {
            Debug.Log("Double Betting");

            SoundManager.PlaySFX(SFX.ClickButton);
            GameManager.SendReBettingRequest();

            betButton.SetEnableSideButtons(false);
        }

        private bool IsResultSuccess(Protocol.ResultCode resultCode)
        {
            Bebop.UI.WaitIndicator.SetActive(false);

            if (resultCode == Protocol.ResultCode.Success)
                return true;

            //-> 배팅시간 이후 배팅 했을 때
            //-> 배팅시간 이후 캔슬 했을 때
            if (resultCode == Protocol.ResultCode.Locked)
            {
                //BebopMessagebox.Ok("ResultCode", LocalizationManager.Instance.GetText("betcancel_fail"));
                return false;
            }

            //-> 월렛 못 가져왔을 때
            if (resultCode == Protocol.ResultCode.RequestTimeout)
            {
                //BebopMessagebox.Ok("ResultCode", LocalizationManager.Instance.GetText("betcancel_fail"));
                return false;
            }

            //-> 배팅 횟수 제한 초과 했을 때
            if (resultCode == Protocol.ResultCode.PreconditionFailed)
            {
                SoundManager.PlaySFX(SFX.Noti_Error);
                BebopMessagebox.Ok("ResultCode", LocalizationManager.Instance.GetText("bet_over"));
                return false;
            }

            //-> 배팅 한도 초과
            //-> 판돈 한계 넘었을 때
            if (resultCode == Protocol.ResultCode.NotAcceptable)
            {
                SoundManager.PlaySFX(SFX.Noti_Error);
                BebopMessagebox.Ok("ResultCode", LocalizationManager.Instance.GetText("bet_limitover"));
                return false;
            }

            //-> 금액 부족
            if (resultCode == Protocol.ResultCode.PaymentRequired)
            {
                SoundManager.PlaySFX(SFX.Noti_Error);
                BebopMessagebox.Ok("ResultCode", LocalizationManager.Instance.GetText("popup_insufficientcoin"));
                return false;
            }

            Debug.Log(resultCode);
            
            BebopMessagebox.Ok("ResultCode", resultCode.ToString());

            return false;
        }

        private void SetBettingPanelAmount(Protocol.BetStates betStates, bool isCancel)
        {
            if (betStates == null) return;

            foreach (var bet in betStates.States)
            {
                Protocol.BettingType type = bet.Key;
                
                foreach (var coinState in bet.Value.States)
                {
                    if (isCancel == true)
                    {
                        betPanel.SubAmount(type, (int)coinState.Key * coinState.Value.CoinCount);
                    }
                    else
                    {
                        betPanel.AddAmount(type, (int)coinState.Key * coinState.Value.CoinCount);
                    }
                }
            }
        }

        private void SetBettingPanelAmountSnap(Protocol.BetStates betStates)
        {
            if (betStates == null) return;

            foreach (var betState in betStates.States)
            {
                Protocol.BettingType type = betState.Key;

                long total = 0;
                foreach (var coinState in betState.Value.States)
                {
                    total += (int)coinState.Key * coinState.Value.CoinCount;
                }

                total /= 100;
                betPanel.SetAmount(type, total);
            }

            //var itor = TableBettingStates.GetEnumerator();
            //while (itor.MoveNext())
            //{
            //    if (itor.Current.Key == Protocol.BettingType.None)
            //        continue;

            //    betPanel.SetAmount(itor.Current.Key, itor.Current.Value.Sum(info=>info.Amount));
            //}
        }

        private void SetBettingCoinsSnap(int accountId, Protocol.BetStates betStates)
        {
            if (betStates == null) return;

            foreach (var betState in betStates.States)
            {
                Protocol.BettingType betType = betState.Key;

                foreach (var coinState in betState.Value.States)
                {
                    if (UserData.Instance.AccountId == accountId)
                    {
                        betCoins.MyBettingSnap(betType, coinState.Key, coinState.Value.CoinCount);
                    }
                    else
                    {
                        betCoins.PlayerBettingSnap(accountId, betType, coinState.Key, coinState.Value.CoinCount);
                    }

                    UI.PlayerAvatar.E_ItemType specialType;
                    bool isSpecial = UI.PlayerAvatar.Instance.GetSpecialPlayer(accountId, out specialType);
                    if (isSpecial == true)
                    {
                        if (specialType == UI.PlayerAvatar.E_ItemType.Supernova)
                        {
                            betPanel.SetSuperNovaImage(betType, true);
                        }
                        else if (specialType == UI.PlayerAvatar.E_ItemType.GodBrain)
                        {
                            betPanel.SetGodBrainImage(betType, true);
                        }
                    }
                }
            }

            //List<Protocol.BettingInfo> lstMyBettingInfo = new List<Protocol.BettingInfo>();

            //var itor = TableBettingStates.GetEnumerator();
            //while (itor.MoveNext())
            //{
            //    if (itor.Current.Key == Protocol.BettingType.None)
            //        continue;

            //    var bettingType = itor.Current.Key;

            //    itor.Current.Value.ForEach(info =>
            //    {
            //        if (UserData.Instance.AccountId == info.AccountId)
            //        {
            //            betCoins.MyBettingSnap(info.AccountId, bettingType, info.BetId, info.Amount);
            //            lstMyBettingInfo.Add(new Protocol.BettingInfo() {BettingType = bettingType, Id = info.BetId, Amount = info.Amount });
            //        }
            //        else
            //        {
            //            betCoins.PlayerBettingSnap(info.AccountId, bettingType, info.BetId, info.Amount);
            //        }

            //        UI.PlayerAvatar.E_ItemType specialType;
            //        bool isSpecial = UI.PlayerAvatar.Instance.GetSpecialPlayer(info.AccountId, out specialType);
            //        if (isSpecial == true)
            //        {
            //            if (specialType == UI.PlayerAvatar.E_ItemType.Supernova)
            //            {
            //                betPanel.SetSuperNovaImage(bettingType, true);
            //            }
            //            else if (specialType == UI.PlayerAvatar.E_ItemType.GodBrain)
            //            {
            //                betPanel.SetGodBrainImage(bettingType, true);
            //            }
            //        }
            //    });
            //}

            //if (lstMyBettingInfo.Count > 0)
            //{
            //    stkBettingInfo.Push(lstMyBettingInfo.Select(info => info.Id).ToList());
            //    myBettingInfos.Clear();
            //    myBettingInfos.AddRange(lstMyBettingInfo);

            //    ChangeBettingButtonState();
            //}
        }

        private void ChangeBettingButtonState()
        {
            betButton.SetInteractableCancelButton(myBettingCount > 0);

            if (myBettingCount > 0)
            {
                betButton.SetBettingOptionButtonState(BettingButton.E_ButtonState.Double, true);
            }
            else
            {
                bool isEnable = myPreBettingCount > 0;
                betButton.SetBettingOptionButtonState(BettingButton.E_ButtonState.Repeat, isEnable);
            }
        }

        private void ResponseBetting(Protocol.BettingResponse res)
        {
            betPanel.Enable();

            if (currentHandState == Protocol.HandState.Betting)
                betButton.SetEnableSideButtons(true);

            if (IsResultSuccess(res.Result) == false)
                return;

            if (res.IsSanpShot == false)
                ++myBettingCount;

            UI.PlayerAvatar.E_ItemType specialType;
            bool isSpecial = UI.PlayerAvatar.Instance.GetSpecialPlayer(UserData.Instance.AccountId, out specialType);

            SetBettingPanelAmount(res.BetStates, false);

            int bettingCount = 0;
            foreach (var betState in res.BetStates.States)
            {
                Protocol.BettingType betType = betState.Key;
                foreach (var coinState in betState.Value.States)
                {
                    bettingCount += coinState.Value.CoinCount;

                    //-> 코인 이동
                    if (res.IsSanpShot == true)
                    {
                        betCoins.MyBettingSnap(betType, coinState.Key, coinState.Value.CoinCount);
                    }
                    else
                    {
                        betCoins.MyBetting(betType, coinState.Key, coinState.Value.CoinCount, (int)specialType);
                    }

                    //-> 패널 스페셜타입 체크
                    if (isSpecial == true)
                    {
                        if (specialType == UI.PlayerAvatar.E_ItemType.Supernova)
                        {
                            betPanel.SetSuperNovaImage(betType, true);
                        }
                        else if (specialType == UI.PlayerAvatar.E_ItemType.GodBrain)
                        {
                            betPanel.SetGodBrainImage(betType, true);
                        }
                    }

                    //-> 패널 내 배팅 금액
                    betPanel.AddMyAmount(betType, (int)coinState.Key * coinState.Value.CoinCount);
                }
            }

            if (bettingCount > 1 && res.IsSanpShot == false)
            {
                SoundManager.PlaySFX(SFX.Chip_Betting_Multiple);
            }
            else
            {
                SoundManager.PlaySFX(SFX.Chip_Betting, 0.7f);
            }

            ChangeBettingButtonState();
        }

        private void ResponseCancelBetting(Protocol.BettingCancelResponse res)
        {
            if (IsResultSuccess(res.Result) == false)
            {
                return;
            }

            //-> 배팅 상태가 끝나고 오는 캔슬이면 데빗문제로 서버에서 캔슬하는것임.
            //-> 이미 캔슬이 온 상태이다.
            if (res.Reason != Protocol.BetCancelReason.PlayerRequest && res.Reason != Protocol.BetCancelReason.None)
            {
                if (myBettingCount <= 0)
                    return;

                string reason = "popup_erroebet";
                if (res.Reason == Protocol.BetCancelReason.BalanceLack)
                {
                    reason = "popup_cancelbet";
                }
                //else if (res.Reason == Protocol.BetCancelReason.DebitFail || res.Reason == Protocol.BetCancelReason.DebitTimeout)
                //{
                //    reason = "popup_erroebet";
                //}

                //-> 메세지 박스를 띄어 유저에게 알려주자
                BebopMessagebox.Ok("", LocalizationManager.Instance.GetText(reason));
                myBettingCount = 0;
            }

            if (currentHandState == Protocol.HandState.Betting)
                betButton.SetEnableSideButtons(true);

            --myBettingCount;

            SetBettingPanelAmount(res.BetStates, true);

            UI.PlayerAvatar.E_ItemType specialType;
            bool isSpecial = UI.PlayerAvatar.Instance.GetSpecialPlayer(UserData.Instance.AccountId, out specialType);

            foreach (var betState in res.BetStates.States)
            {
                Protocol.BettingType betType = betState.Key;
                foreach (var coinState in betState.Value.States)
                {
                    //-> 코인 이동
                    betCoins.CancelMyBetting(betType, coinState.Key, coinState.Value.CoinCount);

                    //-> 패널 스페셜타입 체크
                    if (isSpecial == true)
                    {
                        if (specialType == UI.PlayerAvatar.E_ItemType.Supernova)
                        {
                            betPanel.SetSuperNovaImage(betType, false);
                        }
                        else if (specialType == UI.PlayerAvatar.E_ItemType.GodBrain)
                        {
                            betPanel.SetGodBrainImage(betType, false);
                        }
                    }

                    //-> 패널 내 배팅 금액
                    betPanel.SubMyAmount(betType, (int)coinState.Key * coinState.Value.CoinCount);
                }
            }

            ChangeBettingButtonState();
        }

        private void ReceiveBettingMessage(BettingEventArgs args)
        {
            Protocol.NotifyBettingPlayer noti = args.DTO;

            RectTransform rectTransform;
            bool isTopPlayer = UI.PlayerAvatar.Instance.GetAvatarPosition(noti.AccountId, out rectTransform);

            UI.PlayerAvatar.E_ItemType specialType;
            bool isSpecialPlayer = UI.PlayerAvatar.Instance.GetSpecialPlayer(noti.AccountId, out specialType);

            int bettingCount = 0;
            foreach (var betState in noti.BetStates.States)
            {
                Protocol.BettingType betType = betState.Key;

                foreach (var coinState in betState.Value.States)
                {
                    bettingCount += coinState.Value.CoinCount;

                    //-> 동전 던지기
                    if (noti.Cancel == true)
                    {
                        betCoins.CancelPlayerBetting( isTopPlayer ? noti.AccountId : 0, betType, coinState.Key, coinState.Value.CoinCount);
                    }
                    else
                    {
                        if (noti.IsSnapShot == true)
                        {
                            betCoins.PlayerBettingSnap(noti.AccountId, betType, coinState.Key, coinState.Value.CoinCount);
                        }
                        else
                        {
                            if (isTopPlayer == true)
                            {
                                betCoins.PlayerBetting(noti.AccountId, betType, coinState.Key, coinState.Value.CoinCount, rectTransform, (int)specialType);
                                UI.PlayerAvatar.Instance.PlayBetting(noti.AccountId);
                            }
                            else
                            {
                                betCoins.PlayerBetting(0, betType, coinState.Key, coinState.Value.CoinCount, null, (int)specialType);
                            }
                        }
                    }

                    // 패널에 스페셜 불 들어오기
                    if (isSpecialPlayer == true)
                    {
                        if (specialType == UI.PlayerAvatar.E_ItemType.Supernova)
                        {
                            betPanel.SetSuperNovaImage(betType, !noti.Cancel);
                        }
                        else
                        {
                            betPanel.SetGodBrainImage(betType, !noti.Cancel);
                        }
                    }

                    
                }
            }

            if (noti.Cancel == false && bettingCount > 0 && noti.IsSnapShot == false)
            {
                if (bettingCount > 1)
                {
                    SoundManager.PlaySFX(SFX.Chip_Betting_Multiple);
                }
                else
                {
                    SoundManager.PlaySFX(SFX.Chip_Betting, 0.7f);
                }
            }

            SetBettingPanelAmount(noti.BetStates, noti.Cancel);

            //if (isSpecialPlayer)
            //{
            //    noti.BettingInfos.ForEach(info =>
            //    {
            //        if (specialType == UI.PlayerAvatar.E_ItemType.Supernova)
            //        {
            //            betPanel.SetSuperNovaImage(info.BettingType, !noti.Cancel);
            //        }
            //        else
            //        {
            //            betPanel.SetGodBrainImage(info.BettingType, !noti.Cancel);
            //        }
            //    });
            //}



            //noti.BettingInfos.ForEach(info =>
            //{
            //    if (noti.Cancel == true)
            //    {
            //        //betCoins.NotifyCancelBetting(info.BettingType, noti.AccountId, rectTransform);
            //        betCoins.CancelBetting(noti.AccountId, info);
            //    }
            //    else
            //    {
            //        if (isTopPlayer == true)
            //        {
            //            betCoins.PlayerBetting(noti.AccountId, info.BettingType, info.Id, info.Amount, rectTransform, (int)specialType);
            //            UI.PlayerAvatar.Instance.PlayBetting(noti.AccountId);
            //        }
            //        else
            //        {
            //            betCoins.PlayerBetting(noti.AccountId, info.BettingType, info.Id, info.Amount, null, (int)specialType);
            //        }
            //    }
            //});

            //if (noti.Cancel == false && noti.BettingInfos.Count > 0)
            //{
            //    if (noti.BettingInfos.Count > 1)
            //    {
            //        SoundManager.PlaySFX(SFX.Chip_Betting_Multiple);
            //    }
            //    else
            //    {
            //        SoundManager.PlaySFX(SFX.Chip_Betting);
            //    }
            //}

            //SetBettingPanelAmount(noti.BetStates, noti.Cancel);
        }

        private void HandStatusChanged(Model.EventParameters.HandStatusArgs args, bool isSnapShot)
        {
            currentHandState = args.HandStatus;

            if (args.HandStatus == Protocol.HandState.Idle || args.HandStatus == Protocol.HandState.HandStart || isSnapShot)
            {
                Init();

                //-> Idle 상태에서 Reason이 이거라면 서버점검이다. 체크아웃하자.
                if (args.WaitingReason == Protocol.WaitingReason.ServiceCheckup)    
                {
                    BebopMessagebox.Error("", LocalizationManager.Instance.GetText("maintenance_announcealam"), () =>
                    {
                        GameObject.DestroyImmediate(BebopMessagebox.Instance.gameObject);
                        GameManager.SendCheckOutRequest();
                        Common.Scripts.GameSwitch.Load(Common.Scripts.Managers.E_GameType.Fishing, Common.Scripts.Define.Scene.IntegratedLobby);
                    }, 10f);
                }
            }

            betPanel.SetEnabledAll(args.HandStatus == Protocol.HandState.Betting);
            betButton.SetInteractableCancelButton(args.HandStatus == Protocol.HandState.Betting);
            betButton.SetInteractableBettingOptionButton(args.HandStatus == Protocol.HandState.Betting);
            betButton.SetInteractableBettingButton(args.HandStatus == Protocol.HandState.Betting);

            if (args.HandStatus == Protocol.HandState.Betting)
            {
                ChangeBettingButtonState();
            }
        }

        private void HandStart(Protocol.NotifyHandStateHandStartIn res, bool isSnap)
        {
            handId = res.HandId;
        }

        private void CardOpen(Protocol.NotifyHandStateCardOpenIn res, bool isSnap)
        {
            if (isSnap == true)
                return;

            if (0 < res.HandId && res.HandId < handId)
                return;

            StartCoroutine(CoCardOpen(res));
        }

        private IEnumerator CoCardOpen(Protocol.NotifyHandStateCardOpenIn res)
        {
            yield return new WaitForSecondsRealtime(4f);

            betCoins.Win(res.WinningBettingTypes, ()=>
            {
                long total = 0;
                foreach (var betState in res.MyBetStates.States)
                {
                    foreach (var coinState in betState.Value.States)
                    {
                        total += coinState.Value.Payout;
                    }
                }

                if (total > 0)
                {
                    Bebop.Scripts.UI.MyInfo.Instance.PlayAmount((double)total / 100f);
                    Scripts.UI.MyInfo.Instance.UpdateWallet(res.Wallet);
                }

                foreach (var playerState in res.PlayersBetState.States)
                {
                    if (playerState.Key == Protocol.PlayerType.NormalPlayers)
                        continue;

                    int accountId = playerState.Value.PlayerThumbnail.AccountInfo.AccountId;
                    total = 0;
                    foreach (var betState in playerState.Value.BetStates.States)
                    {
                        foreach (var coinState in betState.Value.States)
                        {
                            total += coinState.Value.Payout;
                        }
                    }

                    if (total > 0)
                    {
                        UI.PlayerAvatar.Instance.PlayWinning(accountId, (double)total / 100f);
                    }
                }

            });
            betPanel.Win(res.WinningBettingTypes);

            if (res.MyBetStates.States.Count() > 0)
            {
                Common.Scripts.Sound.Managers.SoundManager.PlaySFX(SFX.Chip_Getting);
                Common.Scripts.Sound.Managers.SoundManager.PlaySFX(SFX.Result_Win);
            }
        }

        private void BoardUpdated(Protocol.NotifyHandStateBoardUpdateIn res, bool isSnap)
        {
            if (0 < res.HandId && res.HandId < handId)
                return;

            handId = ++handId;

            //-> 로드맵이 켜진 상태면 업데이트
            if (roadMap.gameObject.activeSelf == true)
            {
                OnClickMainHistory();
            }

            if (isSnap == true)
            {
                mainHistory.SetHistorySnap(res.BetResultHistories);
            }
            else
            {
                mainHistory.SetHistory(res.BetResultHistories, 1.5f);
            }

            var itor = res.LastWinningIndex.GetEnumerator();
            while (itor.MoveNext())
            {
                Protocol.BettingType type = itor.Current.Key;
                if (type == Protocol.BettingType.Cowboy ||
                    type == Protocol.BettingType.Bull ||
                    type == Protocol.BettingType.Draw )
                {
                    continue;
                }

                betPanel.SetHistory(itor.Current.Key, res.BetResultHistories, itor.Current.Value, isSnap);
            }
        }

        private void GameHistoryResponse(Protocol.GameHistoryResponse res)
        {
            Bebop.UI.WaitIndicator.SetActive(false);

            if (IsResultSuccess(res.Result) == false)
                return;

            roadMap.SetActive(true);
            roadMap.SetRoadmap(handId, res.GameHistories);
        }

        public Vector3 GetPanelPosition(Protocol.BettingType type)
        {
            return betPanel.GetPanelPosition(type);
        }

        public static void StopBetting()
        {
            Instance.betPanel.SetEnabledAll(false);
            Instance.betButton.SetInteractableCancelButton(false);
            Instance.betButton.SetInteractableBettingOptionButton(false);
            Instance.betButton.SetInteractableBettingButton(false);
        }
    } 
}
