using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 테스트용 프로토콜 객체들.. 기존 생선겜 프로토콜 설계를 모방함.
/// </summary>
namespace FakeProtocol
{

#region  ========= Enum ==========
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

    public enum FakeProtocolId
    {
        Hello = 9000,
        Ping = 9001,

        Deal =9010,
        HandResult = 9012
    }

#endregion ========= Enum ==========


    /**
     * 프로토콜 인터페이스
     */
    public interface Response {
         FakeProtocol.ResultCode Result { get; set; }
    }
    
    /**
     * Hello 응답 테스트용
     */
    public class HelloResponse : Response
    {
        public string Message ="Hello";

        public ResultCode Result { get; set; }
    }

    /**
     * 핑 응답
     */
    public class PingResponse : Response
    {
        public PingType PingType;
        public long Timestamp;
        public long RequestTime;
        public string Version;


        public ResultCode Result { get; set; }
    }

    public class DealResponse : Response
    {


        public ResultCode Result { get; set; }
    }

    public class HandResultResponse: Response 
    {
        public HandInfo  CowHand;
        public HandInfo BoyHand ;

        public string [] CommunityCards;

        public ResultCode Result { get; set; }
    }


    public class HandInfo
    { 
        public string Name ;

        public string [] HoleCard ={}; //"h1,d3"

        public int HandRank ; 

        public string [] Best5Cards;


    }
}