using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bebop
{
    public class BettingHistoryItem : MonoBehaviour
    {
        public Button btnDetail;
        public TextMeshProUGUI txtData;
        public TextMeshProUGUI txtBettingAmount;
        public TextMeshProUGUI txtProfit;

        [Header("Arrow Sprite")]
        public Sprite imgUp;
        public Sprite imgDown;

        [Header("Profit Color")]
        public Color colRed;
        public Color colGreen;

        [Header("Content")]
        public GameObject objContent;
        public GameObject prefabContentItem;
    } 
}
