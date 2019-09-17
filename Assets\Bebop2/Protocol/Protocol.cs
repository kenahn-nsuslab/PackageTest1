using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bebop.Protocol
{
    public enum ResultCode
    {
        Success = 200,
        Accepted = 202,
        NoContent = 204,
        AlreadyReported = 208,
        Redirection = 300,
        MultipleChoices = 300,
        TemporaryRedirect = 307,
        BadRequest = 400,
        Unauthorized = 401,
        PaymentRequired = 402,
        Forbidden = 403,
        NotFound = 404,
        MethodNotAllowed = 405,
        NotAcceptable = 406,
        RequestTimeout = 408,
        Conflict = 409,
        Gone = 410,
        PreconditionFailed = 412,
        RequestEntityTooLarge = 413,
        RequestedRangeNotSatisfiable = 416,
        UnprocessableEntity = 422,
        Locked = 423,
        FailedMethod = 424,
        UpgradeRequired = 426,
        PreconditionRequired = 428,
        TemporarilyUnavailable = 480,
        ChannelNotFound = 481,
        BusyHere = 486,
        RequestTerminated = 487,
        InvalidSession = 488,
        InternalServerError = 500,
        NotImplemented = 501,
        BadGateway = 502,
        ServiceUnavailable = 503,
        GatewayTimeout = 504,
        VersionNotSupported = 505,
        BandwidthLimitExceeded = 509
    }

    public enum PingType
    {
        SyncTime = 0,
        HeartBeat = 1
    }

    public enum BebopProtocolId
    {

#region ==== test =====
        // Hello = 9000,
        // CheckInRequest = 9001,
        // CheckInResponse = 9002,

        // Deal =9010,
        // HandResult = 9012,
#endregion

       
       Ping = 1500,

       CheckIn = 1501,
       CheckOut = 1502,

       Wallet = 1503,

       Betting = 1510,
       BettingCancel = 1511,
       ReBet = 1512,

       GameHistory = 1520,
       GameStatistics = 1521,

       PlayerList = 1530,
       PlayerSuccessiveRecord = 1531,
       PlayerWinningRateRecord = 1532,
       PlayerBettingSumRecord = 1533,
       PlayerProfitSumRecord = 1534,

       MyBettingHistory = 1540,
       HandHistory = 1550,

       ShowEmoticon = 1590,

       DirectLogin = 1600,

       ActivatePlayer = 1601,
       DeactivatePlayer = 1602,

#region === Notification =====
       
        CheckInPlayer = 2102,
        CheckOutPlayer = 2106,

        BettingPlayer = 2110,

        HandStateIdleIn = 2120,
        HandStateHandStartIn = 2121,
        HandStateBettingIn = 2122,
        HandStateCardOpenIn = 2123,

        HandStateBoardUpdateIn = 2124,

        KickedFromServer = 2200,
#endregion 

    }

    public interface Response {
         Bebop.Protocol.ResultCode Result { get; set; }
    }

    public class PagedRequest
    {
        public int Page { get; set; } = 0;

        public int PageSize { get; set; } = 16;

    }


}

