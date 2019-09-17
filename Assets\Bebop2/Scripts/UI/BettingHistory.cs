using Bebop.Protocol;
using Common.Scripts.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bebop
{
    public class BettingHistory : MonoBehaviour
    {
        public static BettingHistory Instance { get; private set; }

        public Button btnCancel;
        public Transform contentParent;

        public GameObject prefabItem;

        [Header("Detail Panel")]
        public BettingHistoryDetail bettingHistoryDetail;

        [Header("ScrollRect Optimizing")]
        public ScrollRectOptimizing scrollRectOpti;

        [Header("No Data")]
        public GameObject objNoData;

        //private ObjectContainer itemContainer;
        private List<HandBettingHistory> lstHandData = new List<HandBettingHistory>();
        private List<HandBettingHistory> lstDisplayHandData = new List<HandBettingHistory>();

        private bool isUpdating = false;
        private bool isLastPage = false;

        private void Awake()
        {
            Instance = this;

            transform.localPosition = Vector3.zero;

            //itemContainer = new ObjectContainer();
            //itemContainer.Set(prefabItem, contentParent);

            prefabItem.SetActive(false);

            btnCancel.onClick.AddListener(OnClickCancel);

            var scrollRect = scrollRectOpti.GetComponent<ScrollRect>();
            scrollRect.onValueChanged.AddListener(OnScrollRectValueChanged);
            BettingHistoryUtil.SetScrollSensitivity(scrollRect);

            objNoData.SetActive(false);
            gameObject.SetActive(false);
        }

        private void Start()
        {
            GameManager.OnMyBettingHistoryResponse += MyBettingHistoryResponse;
        }

        private void OnDestroy()
        {
            GameManager.OnMyBettingHistoryResponse -= MyBettingHistoryResponse;
        }

        private void Reset()
        {
            //itemContainer.HideAll();
            scrollRectOpti.Reset();
            lstHandData.Clear();
            lstDisplayHandData.Clear();

            isUpdating = false;
            isLastPage = false;

            objNoData.SetActive(false);
        }

        public void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);

            if (isActive == true)
            {
                Reset();

                UI.WaitIndicator.SetActive(true, 0f);

                isUpdating = true;
                GameManager.GetMyBettingHistory(0, 10);
            }
        }

        private void OnClickCancel()
        {
            gameObject.SetActive(false);
        }

        private void MyBettingHistoryResponse(MyBettingHistoriesResponse res)
        {
            UI.WaitIndicator.SetActive(false);
            isUpdating = false;

            if (res.Result != ResultCode.Success)
            {
                return;
            }

            int min = Mathf.Min(res.HandBettingHistories.Count, 10);

            lstHandData.AddRange(res.HandBettingHistories.GetRange(0, min));
            lstDisplayHandData = lstHandData.Where(data=>data.BestCards.Count > 0).ToList();

            if (lstDisplayHandData.Count >= 50 || res.HandBettingHistories.Count <= 10 || lstHandData.Count != lstDisplayHandData.Count)
                isLastPage = true;

            bool isNoData = lstDisplayHandData.Count <= 0;

            objNoData.SetActive(isNoData);

            if (lstDisplayHandData.Count <= 10)
            {
                scrollRectOpti.OptimizeScrollRect(lstDisplayHandData.Count, OnUpdateItem);
            }
            else
            {
                scrollRectOpti.OptimizeScrollRect(lstDisplayHandData.Count);
            }
        }

        public void ClickDetail(HandBettingHistory data)
        {
            bettingHistoryDetail.SetActive(true);
            bettingHistoryDetail.Set(data);
        }

        private void OnUpdateItem(int index, GameObject item)
        {
            var comp = item.GetComponent<BettingHistoryItemDetailItem>();

            if (comp == null)
                return;

            comp.Set(lstHandData[index], ClickDetail);
        }

        private void OnScrollRectValueChanged(Vector2 normal)
        {
            if (isUpdating == true)
                return;

            if (isLastPage == true)
                return;

            if (normal.y <= 0f)
            {
                isUpdating = true;
                UI.WaitIndicator.SetActive(true, 0f);
                GameManager.GetMyBettingHistory(lstHandData[lstHandData.Count-1].HandId, 10);
            }
        }
    }
    
    public class BettingHistoryUtil
    {
        public static string GetDisplayValueString(long value)
        {
            double val = value;
            if (value != 0)
            {
                val /= 100f;
            }

            return val.ToString("#,0.##");    
        }

        public static void SetScrollSensitivity(ScrollRect scroll)
        {
            if (scroll == null)
                return;

            if (Application.platform == RuntimePlatform.OSXEditor ||
                Application.platform == RuntimePlatform.OSXPlayer)
            {
                scroll.scrollSensitivity = ScrollSensitivity.OSX;
            }
            else
            {
                scroll.scrollSensitivity = ScrollSensitivity.WINDOW;
            }
        }
    }

    public class ScrollSensitivity
    {
        public const float OSX = 2.5f;
        public const float WINDOW = 25f;
    }
}
