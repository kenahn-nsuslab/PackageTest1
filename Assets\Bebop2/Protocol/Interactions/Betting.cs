using System;
using System.Collections;
using System.Collections.Generic;

namespace Bebop.Protocol
{
    /// <summary>
    /// 배팅 요청 DTO
    /// </summary>
    public class BettingRequest 
    {
        public BettingInfo BetInfo { get; set; }
    }

    /// <summary>
    /// 배팅 응답
    /// </summary>
    public class BettingResponse : Response
    {
        public ResultCode Result { get; set; } = ResultCode.Success;

        public BetStates BetStates { get; set; }

        // 배팅 결과 반영된 Wallet
        public Wallet MainWallet { get; set; }

        public DateTime BettingAt { get; set; }

        public bool IsSanpShot { get; set; }
    }

    /// <summary>
    /// 배팅 취소 요청
    /// </summary>
    public class BettingCancelRequest 
    {
        
    }

    public class BettingCancelResponse : Response
    {
        public ResultCode Result { get; set; } = ResultCode.Success;

        public BetCancelReason Reason { get; set; } = BetCancelReason.None;

        public BetStates BetStates { get; set; }

        // 배팅 결과 반영된 Wallet
        public Wallet MainWallet { get; set; }
    }


    /// <summary>
    /// 리배팅, 더블배팅 요청
    /// </summary>
    public class ReBetRequest
    {

    }

    public class ReBetResponse : Response
    {
        public ResultCode Result { get; set; } = ResultCode.Success;

        // 요청한 BattingInfos
        public BetStates BetStates { get; set; }

        // 배팅 결과 반영된 Wallet
        public Wallet MainWallet { get; set; }

        public DateTime BettingAt { get; set; }
    }
}




