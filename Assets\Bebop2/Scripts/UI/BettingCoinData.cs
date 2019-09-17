using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Bebop
{
    public class BettingCoinData : MonoBehaviour, BettingCoins.IBettingCoinData
    {
        [Header("Sprite")]
        [SerializeField] private Sprite mine;
        [SerializeField] private Sprite player;

        public Protocol.BettingType BettingType { get; set; }
        public Protocol.CoinType CoinType { get; set; }
        public Vector3 StartPosition { get; set; }

        private bool isMine;
        public bool IsMine
        {
            get
            {
                return isMine;
            }
            set
            {
                isMine = value;
                image.sprite = isMine ? mine : player;
            }
        }

        public Material DissolveMaterial
        {
            set
            {
                if (value != null)
                {
                    image.material = value;
                }
                else
                {
                    image.material = matDefault;
                }
                
            }
        }

        private Image image;
        private Material matDefault;

        private void Awake()
        {
            image = GetComponent<Image>();
            matDefault = image.material;
        }

        
    } 
}
