

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bebop.Protocol
{
    public class HandHistoryRequest 
    {
    }

    public class HandHistoryResponse : Response
    {
        public ResultCode Result { get; set; } 
        //* todo : define hand history
        public List<HandHistory> HandHistorys { get; set; }
    }

    public class MyBettingHistoriesRequest
    {
        public long StartHandId { get; set; } = 0;// 0 to get from recently
        public int Count { get; set; }
    }

    public class MyBettingHistoriesResponse : Response
    {
        public ResultCode Result { get; set; } = ResultCode.Success;
        public List<HandBettingHistory> HandBettingHistories { get; set; }
    }


    /// <summary>
    /// 로드맵
    /// </summary>
    public class GameHistoryRequest
    {
    }

    /// <summary>
    /// 로드맵
    /// </summary>
    public class GameHistoryResponse : Response
    {
        public ResultCode Result { get; set; } = ResultCode.Success;

        public List<int> GameHistories { get; set; }
    }


    /// <summary>
    /// 게임 통계 , 어떤 배팅이 얼마나 이겼나..
    /// 최근 1000 핸드 기준.
    /// </summary>
    public class GameStatisticsRequest 
    {
    }

    /// <summary>
    /// 게임 통계 , 어떤 배팅이 얼마나 이겼나..
    /// 최근 1000 핸드 기준.
    /// </summary>
    public class GameStatisticsResponse : Response
    {
        public ResultCode Result { get; set; } = ResultCode.Success;

        public Dictionary<BettingType, int> GameStatistics { get; set; }
    }
}
