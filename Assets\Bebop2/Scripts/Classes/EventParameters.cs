using System;
using Bebop.Protocol;

namespace Bebop.Model.EventParameters
{

    /// <summary>
    /// 우하단 토스트 정보용 
    /// </summary>
    public class BettingEventArgs 
    {

        public BettingEventArgs(NotifyBettingPlayer dto)
        {
            // Player = player;
            // BettingAmount = bettingAmount;
            // BettingType = type;

            // this.TableBettingStates = new Dictionary<BettingType, long>();

            // //copy dictionary
            // foreach (var item in tableStatus)
            // {
            //     this.TableBettingStates.Add(item.Key,item.Value);
            // }
            DTO = dto;
           

        }

        public NotifyBettingPlayer DTO { get; private set;}
    }



    public class HandStatusArgs 
    {
        public HandStatusArgs(HandState state, DateTime startInAt , int period = 0,
                    WaitingReason reason = WaitingReason.None,DateTime? current = null)
        {

            HandStatus = state ;
            StartInAt = startInAt;
            Period = period;
            WaitingReason = reason;
            Current = current;
        
        }

        /// <summary>
        /// 핸드 아이디
        /// TODO: 핸드아이디가 각 이벤트마다 일치하는지 여부로 Validation 한다.
        /// </summary>
        /// <value></value>
        public long HandId {get;set;}

        /// <summary>
        /// 현재 서버 시간 , Catchup 인 경우 만 이 값이 셋팅 된고 다른 정상 상태에서는 null 이다.
        /// StartAt 과 비교하면 핸드 상태 변경 후 얼마나 시간이 경과 한 것인지 알 수 있다.
        /// 
        /// </summary>
        /// <value></value>
        public DateTime? Current { get; private set;}

        /// <summary>
        /// 현재 핸드 상태
        /// </summary>
        /// <value></value>
        public HandState HandStatus  {get; private set;}

        /// <summary>
        /// Idle 상태인 경우 사유가 셋팅된다.
        /// </summary>
        /// <value></value>
        public WaitingReason WaitingReason {get; private set ;} 

        /// <summary>
        /// 현재 핸드 상태로 바뀐 서버 시간
        /// </summary>
        /// <value></value>
        public DateTime StartInAt {get; private set;}

        /// <summary>
        /// 핸드 상태가 유지되는 시간 , 밀리 세컨드
        /// </summary>
        /// <value></value>
        public int Period {get; private set;} 
    }
}