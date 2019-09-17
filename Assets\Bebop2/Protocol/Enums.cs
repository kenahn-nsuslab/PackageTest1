using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bebop.Protocol
{
    public enum BettingType : int
    {
        None = 0,
        Cowboy = 0x1,
        Bull = 0x2,
        Draw = 0x4,

        Suited = 0x8,
        Connectors = 0x10,
        Pair = 0x20,
        SuitedConnectors = 0x40,
        PocketAces = 0x80,

        HighCardOnePair = 0x100,
        TwoPair = 0x200,
        TripleStraightFlush = 0x400,
        FullHouse = 0x800,
        FourSFlushRFlush = 0x1000,
    }
    

    /// <summary>
    /// 서버랑 이름은 다름. 값은 일치
    /// </summary>
    public enum HandState
    {
        Idle = 0,

        HandStart,
        Betting,
        CardOpen,
        BoardUpdate
    }

    public enum Emoticon
    {
        None = 0,
        Smile,
        Cry,
        Anger,
        Comfort
    }

    public enum WaitingReason
    {
        None = 0,
        ServiceCheckup,
        ServiceError
    }

    public enum RankCode
    {
        None = 0,
        SF = 1, // Straight Flush
        FC = 11, // Four Card
        FH = 167, // Full House
        F = 323, // Flush
        S = 1600, // Straight
        TC = 1610, // Three Card
        TP = 2468, // Two Pair
        OP = 3326, // One Pair
        HC = 6186 // High Card
    }


    public enum InqueryType
    {
        Today = 0,
        Days30,
    }

    public enum DisconnectType
    {
        None = 0,
        SocketDisconnect,
        CheckOut,
        TimeOut,
        Ban,
        Duplicate
    }

    public enum PlayerType : int
    {
        NormalPlayers = 0,

        TopPlayer1,
        TopPlayer2,
        TopPlayer3,
        TopPlayer4,
        TopPlayer5,
        TopPlayer6,

        Supernova,
        GodBrain,
    }

    public enum CoinType : int
    {
        Coin1 = 100,
        Coin5 = 500,
        Coin10 = 1000,
        Coin100 = 10000,
        Coin1000 = 100000
    }

    public enum BetCancelReason
    {
        None = 0,
        PlayerRequest, // 사용자 요청
        BalanceLack, // Balance부족
        DebitFail, // 출금 실패
        DebitTimeout, // 출금 시간 초과
        DeductionLack, // 차감액 부족
    }
}
