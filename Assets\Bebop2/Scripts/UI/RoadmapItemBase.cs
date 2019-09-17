using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Bebop
{
    public abstract class RoadmapItemBase : MonoBehaviour, Roadmap.IItem
    {
        [Serializable]
        public class ColorSprite
        {
            public Roadmap.E_Color color;
            public Sprite sprite;
        }
        public ColorSprite[] arrSprite;
        public Image imgBead;
        private Roadmap.E_Color color = Roadmap.E_Color.None;

        public virtual void Init()
        {
            color = Roadmap.E_Color.None;
        }

        public Roadmap.E_Color GetColor()
        {
            return color;
        }

        public virtual void SetColor(Roadmap.E_Color color)
        {
            this.color = color;
            imgBead.sprite = Array.Find(arrSprite, data => data.color == color).sprite;
        }

        public virtual void SetString(string result) { }
        public virtual void SetDrawCount(int count) { }
    } 
}
