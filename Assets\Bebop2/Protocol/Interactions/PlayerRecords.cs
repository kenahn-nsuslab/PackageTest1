using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bebop.Protocol
{
    //플레이어 관련 정보를 조회하는 요청과 응답들..

    /// <summary>
    /// 현재 접속중인 플레이어 리스트를 요청
    /// </summary>
    public class PlayerListRequest : PagedRequest
    {
       
    }

     public class PlayerListResponse : Response
    {
        public ResultCode Result { get; set; } = ResultCode.Success;

        // 초신성
        public PlayerStatistics Supernova { get; set; }
        // 신산자
        public PlayerStatistics GodBrain { get; set; }
        public List<PlayerStatistics> PlayerList { get; set; }

        // 목록의 전체 갯수 (초신성, 신산자 포함)
        public int PlayerCount { get; set; }
    }

    // Player 연승 기록 요청
    public class PlayerSuccessiveRecordRequest
    {
        public InqueryType InqueryType { get; set; }
    }

    public class PlayerSuccessiveRecordResponse : Response
    {
        public ResultCode Result { get; set; } = ResultCode.Success;

        public List<PlayerSuccessiveRecord> PlayerSuccessiveRecords { get; set; }
        public SuccessiveRecord MySuccessiveRecord { get; set; }
        public int MyRanking { get; set; }
    }

    // Player 승률 기록 요청
    public class PlayerWinningRateRecordRequest
    {
        public InqueryType InqueryType { get; set; }
    }

    public class PlayerWinningRateRecordResponse : Response
    {
        public ResultCode Result { get; set; } = ResultCode.Success;

        public List<PlayerWinningRateRecord> PlayerWinningRateRecords { get; set; }
        public WinningRateRecord MyWinningRateRecord { get; set; }
        public int MyRanking { get; set; }
    }

    // Player 배팅금 합계 요청
    public class PlayerBettingSumRecordRequest
    {
        public InqueryType InqueryType { get; set; }
    }

    public class PlayerBettingSumRecordResponse : Response
    {
        public ResultCode Result { get; set; } = ResultCode.Success;

        public List<PlayerBettingSumRecord> PlayerBettingSumRecords { get; set; }
        public BettingSumRecord MyBettingSumRecord { get; set; }
        public int MyRanking { get; set; }
    }

    // Player Profit 합계 요청
    public class PlayerProfitSumRecordRequest
    {
        public InqueryType InqueryType { get; set; }
    }

    public class PlayerProfitSumRecordResponse : Response
    {
        public ResultCode Result { get; set; } = ResultCode.Success;

        public List<PlayerProfitSumRecord> PlayerProfitSumRecords { get; set; }
        public ProfitSumRecord MyProfitSumRecord { get; set; }
        public int MyRanking { get; set; }
    }



    
}