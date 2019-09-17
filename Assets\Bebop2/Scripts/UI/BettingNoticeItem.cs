using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Bebop.UI
{
    public class BettingNoticeItem : MonoBehaviour
    {
        public Image userThumbnail;
        public TextMeshProUGUI userName;
        public TextMeshProUGUI Coin;
        private System.Action<BettingNoticeItem> returnCallBack;

        public void SetData(Image userThumbnail, string userName, long coin, System.Action<BettingNoticeItem> returnCallBack)
        {
            this.userThumbnail = userThumbnail;
            
            this.userName.text = userName;
            this.Coin.text = coin.ToString("#,0.00");
            this.returnCallBack = returnCallBack;

            transform.SetAsLastSibling();
        }

        public void CompleteMove()
        {
            var alpha = GetComponent<Image>().color.a;

            if (alpha <= 0.0f)
                returnCallBack?.Invoke(this);
        }
    }
}   