using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System;
using System.Linq;
//using Bebop.Protocol;
using Bebop.Model.EventParameters;

using DG.Tweening;
using TMPro;

using UnityEngine.Networking;

using Common.Scripts.Managers;

using Common.Scripts.Define;

namespace Bebop.UI
{
    public class PlayerAvatarItem : MonoBehaviour
    {
        public Image imgAvatar;
        public Image imgFrame;
        public Image imgBG;
        public TextMeshProUGUI textWinCount;
        public TextMeshProUGUI textCoin;

        public Bebop.Protocol.PlayerThumbnail playerInfo { get; private set; }

        private Sequence itemMoveSequence = null;
        private Sequence bettingMoveSequence = null;

        private Vector3 startPosition = Vector3.zero;
        private Vector3 startTextCoinPos = Vector3.zero;

        private void Awake()
        {
            startPosition = GetComponent<RectTransform>().localPosition;
            startTextCoinPos = textCoin.transform.localPosition;

            
        }

        private void Start()
        {
            Clear();

            

            var rt = textCoin.GetComponent<RectTransform>();

            //계산 하기 편하게, 센터 상단으로 기준점을 지정한다.
            rt.pivot = new Vector2(0.5f, 1f);
        }

        public bool IsSeatPlayer()
        {
            return playerInfo != null ? true : false;
        }

        public void SetData(Bebop.Protocol.PlayerThumbnail data)
        {
            Clear();
            playerInfo = data;

            Common.Scripts.Localization.LocalizationManager.LanguageChanged -= UpdateWinCount;
            Common.Scripts.Localization.LocalizationManager.LanguageChanged += UpdateWinCount;

            UpdatePlayer();
        }

        private void UpdatePlayer()
        {
            if (playerInfo == null)
                return;

            if(imgFrame != null)
            {
                imgFrame.gameObject.SetActive(true);
            }

            if (imgBG != null)
                imgBG.gameObject.SetActive(true);

            if (textWinCount != null)
            {
                textWinCount.gameObject.SetActive(false);

                const int LimitSuceessive = 3;

                if (playerInfo.Successive >= LimitSuceessive)
                {
                    textWinCount.gameObject.SetActive(true);

                    UpdateWinCount();
                }
            }

            WebImageDownloadManager.Instance.GetImage(playerInfo.AccountInfo.Avatar, (sprite) =>
            {
                imgAvatar.sprite = sprite;
                imgAvatar.gameObject.SetActive(true);
            });

            //-> 190703 국기 숨김
            //if(!string.IsNullOrEmpty(playerInfo.AccountInfo.NationalFlag))
            //{
            //    imgNational.gameObject.SetActive(true);
            //    imgNational.sprite = Common.Scripts.Utils.ResourceManager.Instance.GetSpriteInAtlas(CommonResourcePath.AtlasFlagPath, $"Table_flag_{playerInfo.AccountInfo.NationalFlag}");
            //}
        }

        public void PlayBetting()
        {
            if(bettingMoveSequence != null && bettingMoveSequence.IsPlaying())
            {
                bettingMoveSequence.Complete(true);
            }

            const float MoveLength = 10.0f;
            const float Movespeed = 0.1f;

            bettingMoveSequence = DOTween.Sequence();

            var targetPosition = startPosition + ( transform.right * MoveLength);
            bettingMoveSequence.Append(transform.DOLocalMoveX(targetPosition.x, Movespeed));
            bettingMoveSequence.Append(transform.DOLocalMoveX(startPosition.x, Movespeed)).OnComplete(ResetPosition);
            bettingMoveSequence.Play();
        }

        public void PlayWinning(double amount)
        {
            if (amount <= 0)
                return;

            if (itemMoveSequence != null && itemMoveSequence.IsPlaying())
                itemMoveSequence.Complete(true);

            textCoin.gameObject.SetActive(true);

            textCoin.text = $"+{amount.ToString("#,0.##")}";

            textCoin.transform.localPosition = startTextCoinPos;
            var rt = GetComponent<RectTransform>();

            itemMoveSequence = DOTween.Sequence();
            itemMoveSequence.timeScale = 0.9f;
            itemMoveSequence.Append(textCoin.transform.DOLocalMoveY(rt.sizeDelta.y * 0.5f, 2.5f, true));
            itemMoveSequence.Join(DOTween.To(() => textCoin.alpha, x => textCoin.alpha = x, 0, 2.5f)).OnComplete(ResetCoin);
            itemMoveSequence.Play();
        }

        private void ResetCoin()
        {
            textCoin.alpha = 1;
            textCoin.text = "";
            textCoin.gameObject.SetActive(false);
            textCoin.transform.localPosition = startTextCoinPos;
            textCoin.transform.localScale = Vector3.one;
        }

        private void ResetPosition()
        {
            transform.localPosition = startPosition;
        }

        public void Clear()
        {
            ResetCoin();
            ResetPosition();

            imgAvatar.gameObject.SetActive(false);
            

            if (textWinCount != null)
                textWinCount.gameObject.SetActive(false);

            if (imgFrame != null)
                imgFrame.gameObject.SetActive(false);

            if (imgBG != null)
                imgBG.gameObject.SetActive(false);


            playerInfo = null;
            Common.Scripts.Localization.LocalizationManager.LanguageChanged -= UpdateWinCount;
        }

        private void UpdateWinCount()
        {
            if (playerInfo == null)
                return;

            if (textWinCount == null)
                return;

            var format = Common.Scripts.Localization.LocalizationManager.Instance.GetText("ava_win");
            textWinCount.text = string.Format(format, playerInfo.Successive);
        }
    }
}