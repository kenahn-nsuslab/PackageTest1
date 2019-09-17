using Bebop.Protocol;
using Common.Scripts.Localization;
using Common.Scripts.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Bebop
{
    public class BettingHistoryItemDetailItem : MonoBehaviour
    {
        public Button btnDetail;

        public TextMeshProUGUI txtDate;
        public TextMeshProUGUI txtBettingAmount;
        public TextMeshProUGUI txtProfit;

        [Header("Cards")]
        public Image[] arrImgCard;

        [Header("Profit Color")]
        public Color colGreen;
        public Color colRed;

        private System.Action<HandBettingHistory> onClickDetail;
        private HandBettingHistory bettingHistoryData;

        private void Awake()
        {
            btnDetail.onClick.AddListener(OnClickDetail);
        }

        public void Set(HandBettingHistory handHistory, System.Action<HandBettingHistory> onClickDetail)
        {
            this.onClickDetail = onClickDetail;

            //var historyTime = handHistory.StartAt.ToLocalTime();
            //txtDate.text = string.Format("{0:00}:{1:00}:{2:00}", historyTime.Hour, historyTime.Minute, historyTime.Second);

            var historyTime = handHistory.StartAt.ToLocalTime();
            string month = System.Convert.ToString(historyTime.Month);

            if (LocalizationManager.CurrentLanguage == SystemLanguage.English)
            {
                month = historyTime.ToString("MMM", System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));
            }

            txtDate.text = string.Format(LocalizationManager.Instance.GetText("Format_Date2"), month, historyTime.Day, historyTime.Hour, historyTime.Minute, historyTime.Second);

            long amount = handHistory.BettingHistories.Sum(i => i.Amount);
            txtBettingAmount.text = BettingHistoryUtil.GetDisplayValueString(amount);

            long payout = handHistory.BettingHistories.Sum(i => i.Payout) - amount;
            if (payout > 0)
            {
                txtProfit.color = colGreen;
                txtProfit.text = "+" + BettingHistoryUtil.GetDisplayValueString(payout);
            }
            else if (payout < 0)
            {
                txtProfit.color = colRed;
                txtProfit.text = BettingHistoryUtil.GetDisplayValueString(payout);
            }
            else
            {
                txtProfit.color = colGreen;
                txtProfit.text = "0";
            }

            SetCard(handHistory.DealCards.CowboyHoleCards, 0);
            SetCard(handHistory.DealCards.CommunityCards, 2);
            SetCard(handHistory.DealCards.BullHoleCards, 7);

            SetBestCard(handHistory.BestCards);

            bettingHistoryData = handHistory;
        }

        private void SetCard(List<string> lstCardName, int startIndex)
        {
            for (int i=0, imax=lstCardName.Count; i<imax; ++i)
            {
                arrImgCard[i + startIndex].sprite = GetSprite(lstCardName[i].ToUpper());
                arrImgCard[i + startIndex].name = lstCardName[i].ToUpper();
            }
        }

        private Sprite GetSprite(string cardName)
        {
            return ResourceManager.Instance.GetSprite(string.Format("Sprites/HistoryCard/{0}_{1}_2", cardName[1], cardName[0]));
        }

        private void SetBestCard(List<string> lstName)
        {
            foreach (var card in arrImgCard)
            {
                var toggle = card.GetComponent<Toggle>();
                toggle.isOn = true;
                toggle.transform.localScale = Vector3.one * 0.9f;
            }

            foreach (var best in lstName)
            {
                var imgCard = System.Array.Find(arrImgCard, card => card.name == best.ToUpper());
                if (imgCard != null)
                {
                    var toggle = imgCard.GetComponent<Toggle>();
                    toggle.isOn = false;
                    toggle.transform.localScale = Vector3.one;
                }
            }
        }

        private void OnClickDetail()
        {
            Debug.Log("OnClickDetail");

            if (onClickDetail != null)
                onClickDetail.Invoke(bettingHistoryData);
        }
    } 
}
