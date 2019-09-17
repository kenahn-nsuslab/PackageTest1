using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Bebop
{
    public class BigRoadmapItem : RoadmapItemBase
    {
        public TextMeshProUGUI txtCount;

        public override void Init()
        {
            base.Init();

            txtCount.gameObject.SetActive(false);
            SetColor(Roadmap.E_Color.None);
        }

        public override void SetColor(Roadmap.E_Color color)
        {
            base.SetColor(color);
            base.imgBead.enabled = true;
        }

        public override void SetDrawCount(int count)
        {
            txtCount.gameObject.SetActive(true);
            txtCount.text = count.ToString();
        }
    } 
}
