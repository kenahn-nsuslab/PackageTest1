using System;
using Bebop.Model.EventParameters;
using Bebop.Protocol;
using Newtonsoft.Json;
using UnityEngine;

namespace Bebop
{
    /// <summary>
    /// 여기에는 서버 push (Notification) 들만 관리한다. - by ken
    /// </summary>
    public partial class GameManager : MonoBehaviour
    {
       

        /// <summary>
        /// 현재 플레이어의 상태가 변경
        /// 두번째 파라미터 : IsHiddenAction? => 캐치업 작업에서 호출한 것인지 여부
        /// </summary>
        public static event Action<PlayerBetState, bool> OnChangePlayerInfo = delegate {} ;


        /// <summary>
        /// 플레이어 입장
        /// 현재 플레이어를 포함 모든 플레이어가 입장할 때 발생한다.
        /// </summary>
        public static event Action<AccountInfo> OnCheckInPlayer = delegate {} ;

        /// <summary>
        /// 플레이어 퇴장
        /// 현재 플레이어 포함 모든 플레이어가 퇴장할 때 발생한다.
        /// </summary>
        public static event Action<int> OnCheckOutPlayer = delegate {};


        /// <summary>
        /// Waiting ...
        /// 핸드 종료 후 start 전에 서버에서 잠시 대기를 보낼 수 있다. 
        /// 핸드 시작전 항상 보내는 것은 아님.
        /// 두번째 파라미터 bool 은 체크인 스탭샷 처리 중인지 여부를 알려준다.false
        /// 이값으로 애니메이션을 생략해도 되는지 여부를 알수있다
        /// </summary>
        public static event Action<HandStatusArgs,bool> OnHandStatusChanged = delegate {};


        public static event Action<NotifyHandStateHandStartIn, bool> OnHandStart = delegate {};

        /// <summary>
        /// 카드 결과 
        /// 두번째 파라미터 : IsHiddenAction? => 캐치업 작업에서 호출한 것인지 여부
        /// </summary>
        public static event Action<NotifyHandStateCardOpenIn,bool> OnCardOpen = delegate {};


        public static event Action<NotifyHandStateBoardUpdateIn,bool> OnBoardUpdated = delegate {};


        /// <summary>
        /// 플레이어(들)이 베팅을 하거나 취소 하거나 할때 발생
        /// </summary>
        //public static event Action<NotifyBettingPlayerParameter> OnBettingAction = delegate {} ;


        public static event Action<BettingEventArgs> OnReceiveBettingMessage = delegate {};

        

        public static event Action<NotifyKickedFromServer> OnKickedFromServer = delegate {};

        /// <summary>
        /// 배팅 발생
        /// </summary>
        /// <param name="res"></param>
        private void RaiseBettingActionEvents(NotifyBettingPlayer res)
        {
            Debug.Log("recevie Noti-BettingPlayer:"+ JsonConvert.SerializeObject(res,Formatting.Indented));

            var args = new BettingEventArgs(res);

            OnReceiveBettingMessage.Invoke(args); //우하단 토스트 메세지 발생


        }

        private void RaiseCheckInPlayerEvents(NotifyCheckInPlayer res)
        {
            Debug.Log("recevie Noti-CheckInPlayer:"+ JsonConvert.SerializeObject(res,Formatting.Indented));

            if (null != res.AccountInfo)
                OnCheckInPlayer.Invoke(res.AccountInfo);
        }

        private void RaiseCheckOutPlayerEvents(NotifyCheckOutPlayer res)
        {
            Debug.Log("recevie Noti-CheckOutPlayer:"+ JsonConvert.SerializeObject(res,Formatting.Indented));

            if (null != res)
            {
                OnCheckOutPlayer.Invoke(res.AccountId);
            }
        }

        /// <summary>
        /// Idle 상태..
        /// </summary>
        /// <param name="res"></param>
        private void RaiseWaitingEvents(NotifyHandStateIdleIn res)
        {
            Debug.Log("recevie Idle:"+ JsonConvert.SerializeObject(res,Formatting.Indented));

            ScheduleInvoke(res.StateInAt, () =>
            {
                var args = new HandStatusArgs(HandState.Idle, res.StateInAt, 0, res.Reason);

                OnHandStatusChanged.Invoke(args, false);
            });
            
          
        }

        /// <summary>
        /// 핸드 시작
        /// </summary>
        /// <param name="res"></param>
        private void RaiseHandStartEvents(NotifyHandStateHandStartIn res)
        {
            Debug.Log("recevie Hand-Start:"+ JsonConvert.SerializeObject(res,Formatting.Indented));

            ScheduleInvoke(res.StateInAt, () =>
            {
                var args = new HandStatusArgs(HandState.HandStart, res.StateInAt, res.Period);

                OnHandStatusChanged.Invoke(args, false);
                OnHandStart.Invoke(res, false);
            });
        }

        /// <summary>
        /// 배팅 시작
        /// </summary>
        /// <param name="res"></param>
        private void RaiseBettingStartEvents(NotifyHandStateBettingIn res)
        {
            Debug.Log("recevie Hand-Betting :"+ JsonConvert.SerializeObject(res,Formatting.Indented));

            ScheduleInvoke(res.StateInAt, () =>
            {
                var args = new HandStatusArgs(HandState.Betting, res.StateInAt, res.Period);

                OnHandStatusChanged.Invoke(args, false);
                OnWalletResponse.Invoke(new WalletResponse() { Result = ResultCode.Success, MainWallet = res.Wallet });
            });
        }

        /// <summary>
        /// 카드오픈
        /// </summary>
        /// <param name="res"></param>
        private void RaiseCardOpenEvents(NotifyHandStateCardOpenIn res)
        {
            Debug.Log("recevie Hand-CardOpen :"+ JsonConvert.SerializeObject(res,Formatting.Indented));

            ScheduleInvoke(res.StateInAt, () =>
            {
                var args = new HandStatusArgs(HandState.CardOpen, res.StateInAt, res.Period);

                OnHandStatusChanged.Invoke(args, false); //상태변경 알림
                OnCardOpen.Invoke(res, false); //카드오픈 이벤트 발생
            });
        }

        /// <summary>
        /// 보드 정보 수정 
        /// </summary>
        /// <param name="res"></param>
        private void RaiseBordUpdatedEvents(NotifyHandStateBoardUpdateIn res)
        {
            ScheduleInvoke(res.StateInAt, () =>
            {
                var args = new HandStatusArgs(HandState.BoardUpdate, res.StateInAt, res.Period);
                OnHandStatusChanged.Invoke(args, false); //상태변경 알림
                OnBoardUpdated.Invoke(res, false);
            });

            SendPing();
        }


        private void RasieKickedFromServerEvents(NotifyKickedFromServer res)
        {
            OnKickedFromServer.Invoke(res);
        }






        private void BindNotificationToEvents()
        {

            client.BindProtocol<NotifyCheckInPlayer>(BebopProtocolId.CheckInPlayer,RaiseCheckInPlayerEvents);
            client.BindProtocol<NotifyCheckOutPlayer>(BebopProtocolId.CheckOutPlayer, RaiseCheckOutPlayerEvents);
            
            client.BindProtocol<NotifyBettingPlayer>(BebopProtocolId.BettingPlayer,RaiseBettingActionEvents);
            client.BindProtocol<NotifyHandStateIdleIn>(BebopProtocolId.HandStateIdleIn, RaiseWaitingEvents);
            client.BindProtocol<NotifyHandStateHandStartIn>(BebopProtocolId.HandStateHandStartIn,RaiseHandStartEvents);
            client.BindProtocol<NotifyHandStateBettingIn>(BebopProtocolId.HandStateBettingIn,RaiseBettingStartEvents);
            client.BindProtocol<NotifyHandStateCardOpenIn>(BebopProtocolId.HandStateCardOpenIn,RaiseCardOpenEvents);
            client.BindProtocol<NotifyHandStateBoardUpdateIn>(BebopProtocolId.HandStateBoardUpdateIn,RaiseBordUpdatedEvents);

            client.BindProtocol<NotifyKickedFromServer>(BebopProtocolId.KickedFromServer,RasieKickedFromServerEvents);
        }

        
    }
}