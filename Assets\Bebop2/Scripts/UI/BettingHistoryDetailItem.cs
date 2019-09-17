using Common.Scripts.Localization;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bebop
{
    public class BettingHistoryDetailItem : MonoBehaviour
    {
        public Image imgBetResult;
        public TextMeshProUGUI txtBet;
        public TextMeshProUGUI txtOdds;
        public TextMeshProUGUI txtBetAmount;
        public TextMeshProUGUI txtReward;
        public TextMeshProUGUI txtProfit;

        [Header("Bet Result Sprites")]
        public Sprite imgWin;
        public Sprite imgLoss;

        [Header("Profit Color")]
        public Color colGreen;
        public Color colRed;

        [Header("Odds Data")]
        public BebopValueData oddsData;

        public void Set(Protocol.BettingHistory data)
        {
            imgBetResult.sprite = data.Payout > 0 ? imgWin : imgLoss;
            //txtBet.text = data.BettingType.ToString();
            txtBet.text = LocalizationManager.Instance.GetText(GetBettingTypeStringKey(data.BettingType));

            var oddData = System.Array.Find(oddsData.bettingOdds, odd => odd.type == data.BettingType);
            txtOdds.text = "X" + (oddData != null ? oddData.odds : 0);

            txtBetAmount.text = BettingHistoryUtil.GetDisplayValueString(data.Amount);
            txtReward.text = BettingHistoryUtil.GetDisplayValueString(data.Payout);

            long profit = data.Payout - data.Amount;
            if (profit > 0)
            {
                txtProfit.color = colGreen;
                txtProfit.text = "+" + BettingHistoryUtil.GetDisplayValueString(profit);
            }
            else
            {
                txtProfit.color = colRed;
                txtProfit.text = BettingHistoryUtil.GetDisplayValueString(profit);
            }
        }

        private string GetBettingTypeStringKey(Protocol.BettingType type)
        {
            if (type == Protocol.BettingType.Cowboy)
                return "main_1";
            else if (type == Protocol.BettingType.Bull)
                return "main_2";
            else if (type == Protocol.BettingType.Draw)
                return "main_3";
            else if (type == Protocol.BettingType.Suited)
                return "side1_1";
            else if (type == Protocol.BettingType.Connectors)
                return "side1_2";
            else if (type == Protocol.BettingType.Pair)
                return "side1_3";
            else if (type == Protocol.BettingType.SuitedConnectors)
                return "side1_4";
            else if (type == Protocol.BettingType.PocketAces)
                return "side1_5";
            else if (type == Protocol.BettingType.HighCardOnePair)
                return "side2_1";
            else if (type == Protocol.BettingType.TwoPair)
                return "side2_2";
            else if (type == Protocol.BettingType.TripleStraightFlush)
                return "side2_3";
            else if (type == Protocol.BettingType.FullHouse)
                return "side2_4";
            else if (type == Protocol.BettingType.FourSFlushRFlush)
                return "side2_5";

            return "none";
        }
    } 
}
