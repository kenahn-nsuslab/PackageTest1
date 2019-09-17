using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Bebop
{
    public class BeadRoadmapItem : RoadmapItemBase
    {
        [Serializable]
        public class ColorMaterialPreset
        {
            public Roadmap.E_Color color;
            public Material material;
        }

        public ColorMaterialPreset[] arrPreset;
        public TextMeshProUGUI txtResult;

        public override void Init()
        {
            base.Init();

            SetColor(Roadmap.E_Color.None);
            SetString("");
        }

        public override void SetColor(Roadmap.E_Color color)
        {
            base.SetColor(color);
            txtResult.fontSharedMaterial = Array.Find(arrPreset, data=>data.color==color).material;
        }

        public override void SetString(string result)
        {
            var lr = txtResult.gameObject.GetComponent<Common.Scripts.Localization.LocalizationResponder>();
            lr.SetKeyAndUpdate(result);
            //txtResult.text = result;
        }
    } 
}
