using Common.Scripts.Localization;
using Common.Scripts.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bebop
{
    public class BettingHistoryDetail : MonoBehaviour
    {
        public Button btnCancel;

        public Image imgCowboyResult;
        public Image imgBullResult;

        public Image[] arrImgCard;

        public TextMeshProUGUI txtTime;
        public TextMeshProUGUI txtTotalBetAmount;
        public TextMeshProUGUI txtTotalReward;
        public TextMeshProUGUI txtProfit;

        public Transform contentParent;
        public GameObject prefabItem;

        [Header("Result Sprites")]
        public Sprite imgWin;
        public Sprite imgDraw;

        [Header("Profit Color")]
        public Color colGreen;
        public Color colRed;

        private ObjectContainer objContainer;

        private void Awake()
        {
            objContainer = new ObjectContainer();
            objContainer.Set(prefabItem, contentParent);

            prefabItem.SetActive(false);

            btnCancel.onClick.AddListener(OnClickCancel);

            var scrollRect = contentParent.parent.GetComponentInParent<ScrollRect>();
            BettingHistoryUtil.SetScrollSensitivity(scrollRect);

            gameObject.SetActive(false);
        }

        private void OnClickCancel()
        {
            gameObject.SetActive(false);
        }

        public void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);
        }

        public void Set(Protocol.HandBettingHistory data)
        {
            SetCard(data.DealCards.CowboyHoleCards, 0);
            SetCard(data.DealCards.CommunityCards, 2);
            SetCard(data.DealCards.BullHoleCards, 7);

            SetBestCard(data.BestCards);

            SetResult(data.BetResultFlag);

            var historyTime = data.StartAt.ToLocalTime();
            string month = System.Convert.ToString(historyTime.Month);

            if (LocalizationManager.CurrentLanguage == SystemLanguage.English)
            {
                month = historyTime.ToString("MMM", System.Globalization.CultureInfo.CreateSpecificCulture("en-US"));
            }

            txtTime.text = string.Format(LocalizationManager.Instance.GetText("Format_Date"), month, historyTime.Day, historyTime.Hour, historyTime.Minute, historyTime.Second);

            long totalAmount = data.BettingHistories.Sum(bet => bet.Amount);
            long totalPayout = data.BettingHistories.Sum(bet => bet.Payout);
            long totalProfit = totalPayout - totalAmount;

            txtTotalBetAmount.text = BettingHistoryUtil.GetDisplayValueString(totalAmount);
            txtTotalReward.text = BettingHistoryUtil.GetDisplayValueString(totalPayout);
            
            if (totalProfit > 0)
            {
                txtProfit.color = colGreen;
                txtProfit.text = "+" + BettingHistoryUtil.GetDisplayValueString(totalProfit);
            }
            else if (totalProfit < 0)
            {
                txtProfit.color = colRed;
                txtProfit.text = BettingHistoryUtil.GetDisplayValueString(totalProfit);
            }
            else
            {
                txtProfit.color = colGreen;
                txtProfit.text = "0";
            }

            objContainer.HideAll();
            data.BettingHistories = data.BettingHistories.OrderBy(h => (int)h.BettingType).ToList();
            foreach (var history in data.BettingHistories)
            {
                var item = objContainer.GetItem<BettingHistoryDetailItem>();
                item.Set(history);
            }

            var scroll = contentParent.parent.GetComponentInParent<ScrollRect>();
            scroll.verticalNormalizedPosition = 1f;
        }

        private void SetCard(List<string> lstCardName, int startIndex)
        {
            for (int i = 0, imax = lstCardName.Count; i < imax; ++i)
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

        private void SetResult(int resultFlag)
        {
            if ((resultFlag & (int)Protocol.BettingType.Cowboy) != 0)
            {
                imgCowboyResult.gameObject.SetActive(true);
                imgBullResult.gameObject.SetActive(false);

                imgCowboyResult.sprite = imgWin;
                
            }
            else if ((resultFlag & (int)Protocol.BettingType.Bull) != 0)
            {
                imgCowboyResult.gameObject.SetActive(false);
                imgBullResult.gameObject.SetActive(true);

                imgBullResult.sprite = imgWin;
            }
            else
            {
                imgCowboyResult.gameObject.SetActive(true);
                imgBullResult.gameObject.SetActive(true);

                imgCowboyResult.sprite = imgDraw;
                imgBullResult.sprite = imgDraw;
            }

            imgCowboyResult.SetNativeSize();
            imgBullResult.SetNativeSize();
        }
    } 
}
