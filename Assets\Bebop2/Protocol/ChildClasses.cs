using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace Bebop.Protocol
{
    public class AccountInfo
    {
        public int AccountId { get; set; }
        public string NickName { get; set; }
        public string Avatar { get; set; }
        public string NationalFlag { get; set; }
    }

   // public class PlayerInfo
   //{
   //    public AccountInfo AccountInfo { get; set; }
   //    public BetStates BetStates { get; set; }
   //}
    
// 통계 기록
     // Player 통계
    public class PlayStatistics
    {
        public int Successive { get; set; }
        public long BettingSum { get; set; }
        public long Profit { get; set; }
        public int WinningRate { get; set; }
    }

    public class PlayerStatistics
    {
        public AccountInfo AccountInfo { get; set; }
        public PlayStatistics PlayStatistics { get; set; }
        public DateTime CausedAt { get; set; }
    }

    // 연승 기록
    public class SuccessiveRecord
    {
        
        public int Successive { get; set; }
        public DateTime CausedAt { get; set; }
    }

    public class PlayerSuccessiveRecord
    {
        public AccountInfo AccountInfo { get; set; }
        public SuccessiveRecord SuccessiveRecord { get; set; }
    }

    // Player 승률 기록
    public class WinningRateRecord
    {
        public int WinningRate { get; set; }
        public DateTime CausedAt { get; set; }
    }

    public class PlayerWinningRateRecord
    {
        public AccountInfo AccountInfo { get; set; }
        public WinningRateRecord WinningRateRecord { get; set; }
    }

    // Player 배팅금 합계 기록
    public class BettingSumRecord
    {
        public long BettingSum { get; set; }
        public DateTime CausedAt { get; set; }
    }

    public class PlayerBettingInfo
    {
        public int AccountId { get; set; }
        public long BetId { get; set; }
        public long Amount { get; set; }
    }

    public class PlayerBettingSumRecord
    {
        public AccountInfo AccountInfo { get; set; }
        public BettingSumRecord BettingSumRecord { get; set; }
    }

    // Player Profit 합계 기록
    public class ProfitSumRecord
    {
        public long ProfitSum { get; set; }
        public DateTime CausedAt { get; set; }
    }

    public class PlayerProfitSumRecord
    {
        public AccountInfo AccountInfo { get; set; }
        public ProfitSumRecord ProfitSumRecord { get; set; }
    }

    public class PlayerThumbnail
    {
        public AccountInfo AccountInfo { get; set; }
        public int Successive { get; set; }
    }

#region  ===== 서버쪽 contentsModel======

    public class HandSnapShot
    {
        public DateTime HandStartAt { get; set; }

        // TopPlayers 6명
        //public List<PlayerThumbnail> TopPlayers { get; set; }
        //// 초신성
        //public PlayerThumbnail Supernova { get; set; }
        //// 신산자
        //public PlayerThumbnail GodBrain { get; set; }

        public PlayersBetState PlayersBetState { get; set; }

        // 현재 핸드 ID
        public long HandId { get; set; }
        // 현재 핸드 상태(스텝)
        public HandState HandState { get; set; }
        // 현재 핸드의 상태 진입 시간(UTC)
        public DateTime StateInAt { get; set; }
        // 현재 시간(UTC)
        public int Period { get; set; }
        public DateTime Current { get; set; }

        // 딜링 된 카드
        public DealCards DealCards { get; set; }
        public BestCardInfo CowboyBestCardInfo { get; set; }
        public BestCardInfo BullBestCardInfo { get; set; }
        public int WinningBettingTypes { get; set; }

        // Mea Summary
        public List<int> BetResultHistories { get; set; }

        //Board WinningHistories
        public Dictionary<BettingType, int> LastWinningIndex { get; set; }
    }

    public class DealCards
    {
        // 첫번째 카드가 스퀴즈 할 카드
        public List<string> CowboyHoleCards = new List<string>();
        public List<string> BullHoleCards = new List<string>();
        public List<string> CommunityCards = new List<string>();
    }

    public class BestCardInfo
    {
        // Best Cards
        public List<string> Cards { get; set; }
        public RankCode RankCode { get; set; }
        public int Rank { get; set; }
        public List<string> HighNumbers { get; set; } // "23456789JQKA"
    }

    public class BettingInfo
    {
        // 배팅 타입
        public BettingType BetType { get; set; }
        // CoinType
        public CoinType CoinType { get; set; }
        public int CoinCount { get; set; }
    }

    public class BettingResult
    {
        // 배팅시 생성 되는 BattingHistory Id
        public BettingInfo BettingInfo { get; set; }

        // 판정 금액
        public long Payout { get; private set; }

    }

    public class HandHistory
    {
        public long HandId { get; set; }
        public DealCards DealCards { get; set; }
        public int BetResultFlag { get; set; }
        public List<string> BestCards { get; set; }
        public DateTime StartAt { get; set; }
    }

    public class HandBettingHistory : HandHistory
    {
        public List<BettingHistory> BettingHistories { get; set; }
    }

    public class BettingHistory
    {
        public long BetId { get; set; }
        public BettingType BettingType { get; set; }
        public long Amount { get; set; }
        public long Payout { get; set; }
        public DateTime CauseAt { get; set; }
    }

    public class Wallet
    {
        public string Currency { get; set; }
        public decimal Balance { get; set; }
        public long CoinValue { get; set; }
    }

    public class CoinNPayout
    {
        public int CoinCount { get; set; }
        public long Payout { get; set; }

    }

    public class CoinStates
    {
        public Dictionary<CoinType, CoinNPayout> States { get; set; } = new Dictionary<CoinType, CoinNPayout>();

    }
    public class BetStates
    {
        public Dictionary<BettingType, CoinStates> States { get; set; } = new Dictionary<BettingType, CoinStates>();

    }
    public class PlayerBetState
    {
        public PlayerThumbnail PlayerThumbnail { get; set; }
        public BetStates BetStates { get; set; }
    }

    public class PlayersBetState
    {
        public Dictionary<PlayerType, PlayerBetState> States { get; set; } = new Dictionary<PlayerType, PlayerBetState>();
    }

    #endregion

}

