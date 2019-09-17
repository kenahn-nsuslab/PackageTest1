using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System;
using System.Linq;
using Bebop.Protocol;
using Bebop.Model.EventParameters;
using Common.Scripts.Utils;

using DG.Tweening;

namespace Bebop.UI
{
    public class BettingNotice : MonoBehaviour
    {
        private const float ItemOffset = 2;
        private const float FadeoutContentSpeed = 0.1f;
        private const float MoveItemSpeed = 0.4f;
        private const float MoveItemFadeOutSpeed = 0.2f;

        public static BettingNotice Instance { get; private set; }

        public BettingNoticeItem PrefabItem;
        public RectTransform ViewTransform;
        public RectTransform ContentRoot;

        private Vector2 viewSize = Vector2.one;
        private Sequence itemMoveSequence;

        private bool isFadeOutContent = false;
        private float currentFadeOutValue = 1;

        private SimpleGameObjectPool<BettingNoticeItem> itemPool = new SimpleGameObjectPool<BettingNoticeItem>(false);

        private void Awake()
        {
            Instance = this;

            PrefabItem?.gameObject.SetActive(false);
            ContentRoot?.gameObject.SetActive(false);
            viewSize = ViewTransform.sizeDelta;
        }

        private void OnEnable()
        {
            GameManager.OnReceiveBettingMessage += BindNotice;
        }

        private void OnDisable()
        {
            GameManager.OnReceiveBettingMessage -= BindNotice;
        }

        public void BindNotice(BettingEventArgs bettingEvent)
        {
            PlayFadeInOutContent();

            //연출이 끝나지 않았을 경우, 즉시 완료.
            if (itemMoveSequence != null && itemMoveSequence.IsPlaying())
                itemMoveSequence.Complete();

           // var newItem = AddItem(bettingEvent.DTO.AccountInfo.NickName, bettingEvent.DTO.Amount);
           // PlayMoveBettingItem(newItem);
        }

        private BettingNoticeItem AddItem(string userName, long coin)
        {
            var newItem = CreateItem();
            newItem.SetData(null, userName, coin, ReturnItem);
            return newItem;
        }

        private void PlayMoveBettingItem(BettingNoticeItem newItem)
        {
            var lstChildItem = GetEnableItem();

            itemMoveSequence = DOTween.Sequence();
            itemMoveSequence.Append(newItem.transform.DOLocalMoveY(0, MoveItemSpeed, true));

            foreach (var item in lstChildItem)
            {
                var rt = item.transform.GetComponent<RectTransform>();
                float localPosY = rt.localPosition.y + rt.sizeDelta.y + ItemOffset;
                float alpha = 1.0f - (localPosY / viewSize.y);

                itemMoveSequence.Join(item.transform.DOLocalMoveY(localPosY, MoveItemSpeed, true));

                var itemImage = item.GetComponent<Image>();
                var color = itemImage.color;

                var canvasGroup = item.GetComponent<CanvasGroup>();
                canvasGroup.alpha = alpha;

                itemMoveSequence.Join(itemImage.DOColor(new Color(color.r, color.g, color.b, alpha), MoveItemFadeOutSpeed).
                    OnComplete(item.GetComponent<BettingNoticeItem>().CompleteMove));
            }

            itemMoveSequence.Play();
        }

        private void PlayFadeInOutContent()
        {
            const string InvokeMethodName = "FadeOutContent";
            const float StartInvokeTime = 3.0f;
            const float UpdateInvokeTime = 0.03f;

            if (isFadeOutContent == true)
                ContentRoot.gameObject.SetActive(false);

            if (ContentRoot.gameObject.activeSelf == false)
                FadeInContent();

            if (IsInvoking(InvokeMethodName) == true)
                CancelInvoke(InvokeMethodName);

            InvokeRepeating(InvokeMethodName, StartInvokeTime, UpdateInvokeTime);
        }

        private void FadeInContent()
        {
            ContentRoot.gameObject.SetActive(true);
            var canvasGroup = ContentRoot.GetComponent<CanvasGroup>();
            canvasGroup.alpha = currentFadeOutValue = 1;
            isFadeOutContent = false;
        }

        private void FadeOutContent()
        {
            isFadeOutContent = true;

            var canvasGroup = ContentRoot.GetComponent<CanvasGroup>();

            currentFadeOutValue -= FadeoutContentSpeed;
            canvasGroup.alpha = currentFadeOutValue;

            if (canvasGroup.alpha <= 0.0f)
            {
                AllReturnItem();
                ContentRoot.gameObject.SetActive(false);
                CancelInvoke("FadeOutContent");
            }
        }

        private List<BettingNoticeItem> GetEnableItem()
        {
            List<BettingNoticeItem> lstItem = new List<BettingNoticeItem>();
            for (int index = 0; index < ContentRoot.childCount; ++index)
            {
                var childItem = ContentRoot.GetChild(index);

                if (childItem.gameObject.activeSelf == true)
                    lstItem.Add(childItem.GetComponent<BettingNoticeItem>());
            }

            return lstItem;
        }

        private void AllReturnItem()
        {
            for (int index = 0; index < ContentRoot.childCount; ++index)
            {
                var childItem = ContentRoot.GetChild(index);

                if (childItem.gameObject.activeSelf == false)
                    continue;

                itemPool.Return(childItem.GetComponent<BettingNoticeItem>());
                childItem.gameObject.SetActive(false);
            }
        }

        private BettingNoticeItem CreateItem()
        {
            PrefabItem?.gameObject.SetActive(true);
            var rt = PrefabItem?.GetComponent<RectTransform>();
            float posX = rt.localPosition.x;
            float itemSizeHeight = rt.sizeDelta.y;

            var newItem = itemPool.Request(PrefabItem);
            newItem.transform.SetParent(ContentRoot);
            newItem.transform.localPosition = new Vector2(posX, -itemSizeHeight);
            newItem.transform.localScale = Vector3.one;
            newItem.gameObject.SetActive(true);

            PrefabItem?.gameObject.SetActive(false);

            return newItem;
        }

        private void ReturnItem(BettingNoticeItem returnItem)
        {
            itemPool.Return(returnItem);

            var rt = returnItem.GetComponent<RectTransform>();
            returnItem.GetComponent<CanvasGroup>().alpha = 1;
            returnItem.transform.localPosition = new Vector3(returnItem.transform.localPosition.x, -rt.sizeDelta.y);

            var image = returnItem.GetComponent<Image>();
            image.color = new Color(image.color.r, image.color.g, image.color.b, 1);

            returnItem.gameObject.SetActive(false);
        }

        //베팅 알림 테스트 코드
        //public static event Action<string, long> OnBettingNotice;
        //IEnumerator TestStartBettingNotification()
        //{
        //    string[] userNickName = { "Ka12n3danma", "12n3danmaKA", "CowBoy2", "QWER123", "CowBoy5" };
        //    while (true)
        //    {
        //        int randomidx = UnityEngine.Random.Range(0, userNickName.Length - 1);
        //        var userName = userNickName[randomidx];

        //        int randomCoin = UnityEngine.Random.Range(10000, 100000);

        //        OnBettingNotice?.Invoke(userName, randomCoin);

        //        yield return new WaitForSecondsRealtime(UnityEngine.Random.Range(0.1f, 0.8f));
        //    }
        //}
    }
}