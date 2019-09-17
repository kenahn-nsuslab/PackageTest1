
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bebop.Protocol
{

    public class NotifyCheckInPlayer
    {
        public AccountInfo AccountInfo { get; set; }
    }

    public class NotifyCheckOutPlayer 
    {
        public int AccountId { get; set; }
    }


    /// <summary>
    /// 플레이어가 배팅을 하거나 취소했을 때 브로드캐스팅 되는 메세지
    /// 캐치업의 경우 배팅 판 정보를 셋팅하기 위해 
    /// AcountId =0 , BettyingType = none 인 인벤트를 한번 발생시켜준다.
    /// </summary>
    public class NotifyBettingPlayer
    {
        // 배팅한 Player AccountId
        public int AccountId { get; set; }
        // 배팅인지, 취소인지
        public bool Cancel { get; set; } = false;

        // 배팅 내역
        public BetStates BetStates { get; set; }

        public DateTime BettingAt { get; set; }

        //-> 스냅샷 용
        public bool IsSnapShot { get; set; }
    }

    public class NotifyShowEmoticon
    {
        public int AccountInfo { get; set; }
        public Emoticon Emoticon { get; set; }
    }

    public class NotifyHandStateIdleIn
    {
        public WaitingReason Reason { get; set; }
        public DateTime StateInAt { get; set; }
    }

    public class NotifyHandStateHandStartIn
    {
        public DateTime StateInAt { get; set; }
        public int Period { get; set; }
        public long HandId { get; set; }

    }

    public class NotifyHandStateBettingIn
    {
        public DateTime StateInAt { get; set; }
        public int Period { get; set; }

        public Wallet Wallet { get; set; }

        public long HandId { get; set; }
    }

    public class NotifyHandStateCardOpenIn {
        public DateTime StateInAt { get; set; }
        public int Period { get; set; }

        public DealCards DealCards { get; set; }

        public BetStates MyBetStates { get; set; }
        public PlayersBetState PlayersBetState { get; set; }

        public BestCardInfo CowboyBestCardInfo { get; set; }
        public BestCardInfo BullBestCardInfo { get; set; }

        /// <summary>
        /// 이긴 배팅들...
        /// </summary>
        /// <value></value>
        public int WinningBettingTypes { get; set; }

        public Wallet Wallet { get; set; }

        public long HandId { get; set; }
    }

    public class NotifyHandStateBoardUpdateIn
    {
        public DateTime StateInAt { get; set; }
        public int Period { get; set; }

        //public List<PlayerThumbnail> TopPlayers { get; set; }
        //// 초신성
        //public PlayerThumbnail Supernova { get; set; }
        //// 신산자
        //public PlayerThumbnail GodBrain { get; set; }

        public Dictionary<PlayerType, PlayerThumbnail> PlayersRank { get; set; }

        // Mea Summary
        public List<int> BetResultHistories { get; set; }
        public Dictionary<BettingType, int> LastWinningIndex { get; set; }

        public long HandId { get; set; }
    }

    public class NotifyKickedFromServer
    {
        public int DisconnectType;
        public string DisconnectTypeAsString;
    }
}