using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System;
using System.Linq;

using TMPro;

using UnityEngine.Networking;

using Common.Scripts.Managers;

using Common.Scripts.Define;

namespace Bebop.UI
{
    public class AvatarSlotTemplet : MonoBehaviour
    {
        public enum E_AvatarSlotType
        {
            None,
            Supernova, //초신성
            GobBrain, //신산자
            Third, //??3번재?
        }

        [System.Serializable]
        public struct FrameInfo
        {
            public E_AvatarSlotType type;
            public Image imgFrame;
            public Image imgSymbol;
            public Image imgRibbon;
        }

        public List<FrameInfo> lstFrame = new List<FrameInfo>();

        public GameObject objFrameRoot;
        public GameObject objSymbolRoot;
        public GameObject RibbonRoot;

        public Image imgAvatar;

        private Coroutine avatarImgeCoru;
        private E_AvatarSlotType slotType = E_AvatarSlotType.None;

        private void Awake()
        {
            AllHideFrame();
        }

        public void SetAvatar(E_AvatarSlotType type, string avatarUrl, string nationalFlag)
        {
            slotType = type;
            if (avatarImgeCoru != null)
            {
                StopCoroutine(avatarUrl);
                avatarImgeCoru = null;
            }

            AllHideFrame();
            EnableFrame(type);
            WebImageDownloadManager.Instance.GetImage(avatarUrl, (sprite) => 
            {
                imgAvatar.sprite = sprite;
                imgAvatar.gameObject.SetActive(true);
            });
        }

        public void SetRank(int rank)
        {
            var findData = lstFrame.Find(d => d.type == E_AvatarSlotType.Third);

            if (rank <= 0)
                return;

            var label = findData.imgRibbon.GetComponentInChildren<TextMeshProUGUI>(true);

            var format = Common.Scripts.Localization.LocalizationManager.Instance.GetText("plist_rich");

            label.text = string.Format(format, rank);
            //label.text = $"No.{rank}";
        }

        private void EnableFrame(E_AvatarSlotType type)
        {
            var findData = lstFrame.Find(d => d.type == type);
            findData.imgFrame?.gameObject.SetActive(true);
            findData.imgRibbon?.gameObject.SetActive(true);
            findData.imgSymbol?.gameObject.SetActive(true);
        }

        private void AllHideFrame()
        {
            System.Action<GameObject> allHide = (GameObject obj) =>
            {
                if (obj == null)
                    return;

                Image[] images = obj.GetComponentsInChildren<Image>(true);

                for(int index = 0; index < images.Length; ++index)
                    images[index].gameObject.SetActive(false);
            };

            allHide(objFrameRoot);
            allHide(objSymbolRoot);
            allHide(RibbonRoot);

            imgAvatar.gameObject.SetActive(false);

            slotType = E_AvatarSlotType.None;
        }
    }
}