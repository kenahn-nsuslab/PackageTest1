using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Bebop.Model.EventParameters;
using TMPro;
using System;
using UnityEngine.UI;
using Bebop.Protocol;
using Common.Scripts.Localization;
using System.Linq;

namespace Bebop.UI
{
    public class StatisticsViewModel : MonoBehaviour
    {


        
        private Dictionary<SystemLanguage, GameObject> Panels = new Dictionary<SystemLanguage, GameObject>();

        public GameObject detail_CN;

        public GameObject detail_EN;

        public GameObject bar;

        //private BarGraphViewModel graph;
        private RectTransform Red;


        /// <summary>
        /// 빨간색 바 위에 말풍선 숫자 값
        /// </summary>
        private TextMeshProUGUI RedText;

        private RectTransform Blue;

        private TextMeshProUGUI BlueText;

        private RectTransform Green;

        private TextMeshProUGUI GreenText;


        private GameObject SelectedPanel;

        /// <summary>
        /// Ui 패널마다 값을 반영할 text 객체가 따로 존재하므로 현재 활성화된 패널의 텍스트 객체들만 모은다.
        /// </summary>
        /// <typeparam name="BettingType"></typeparam>
        /// <typeparam name="TextMeshProUGUI"></typeparam>
        /// <returns></returns>
        public Dictionary<BettingType, TextMeshProUGUI> UITexts = new Dictionary<BettingType, TextMeshProUGUI>();
        private void Awake() {
            
           
            Panels.Add(SystemLanguage.English, detail_EN);
            Panels.Add(SystemLanguage.ChineseSimplified , detail_CN);

            //graph = bar.GetComponent<BarGraphViewModel>();

            Red =  bar.transform.Find("Red").GetComponent<RectTransform>();
            RedText = Red.GetComponentInChildren<TextMeshProUGUI>();

            Blue = bar.transform.Find("Blue").GetComponent<RectTransform>();
            BlueText = Blue.GetComponentInChildren<TextMeshProUGUI>();

            Green = bar.transform.Find("Green").GetComponent<RectTransform>();
            GreenText = Green.GetComponentInChildren<TextMeshProUGUI>();


            RedText.text ="-";
            BlueText.text ="-";
            GreenText.text ="-";
        
        }

        private void OnEnable() {
            var lang = LocalizationManager.CurrentLanguage;

            SelectPanelByLanguage(lang);

            
        }

        private void SelectPanelByLanguage(SystemLanguage lang)
        {
            //언어에 맞는 패널만 활성화 시킨다.
            Panels.ToList().ForEach(p=>p.Value.SetActive(false));
            this.SelectedPanel = Panels.ContainsKey(lang)? Panels[lang]: Panels[SystemLanguage.English];
            this.SelectedPanel.SetActive(true);

            UITexts.Clear();

            //CowValue = this.SelectedPanel.transform.Find("Cow").GetComponent<TextMeshProUGUI>();
            // foreach ( string objName in Enum.GetNames(typeof(BettingType)))
            // {
                
            // }

            //언어에 따른 패널이 정해지면 값을 할당할 UI 객체들을 준비한다.

            var TMPUs = this.SelectedPanel.GetComponentsInChildren<TextMeshProUGUI>();

            foreach( var tmpu in TMPUs)
            {
                BettingType type = (BettingType) Enum.Parse(typeof(BettingType), tmpu.gameObject.name); // 객체명이 enum 명과 같으므로...

                UITexts.Add(type, tmpu);
            }



        }

        /// <summary>
        /// UI text 에 값을 반영한다.
        /// </summary>
        /// <param name="data"></param>
        public void ApplyData(Dictionary<BettingType, int> data)
        {

            //graph.ApplyData(data[BettingType.Bull], data[BettingType.Cowboy], data[BettingType.Draw]);

            data.ToList().ForEach(item=> {

                if (UITexts.ContainsKey(item.Key))
                    UITexts[item.Key].text = item.Value.ToString("N0");

            });

            var boyCount = data[BettingType.Cowboy];
            var bullCount = data[BettingType.Bull];
            var drawCount = data[BettingType.Draw];

            //graph.ApplyData(bullValue,boyValue,drawValue);

            double total = bullCount + boyCount + drawCount;

            double bullNumber = Convert.ToDouble(bullCount) / total;
            double boyNumber = Convert.ToDouble(boyCount) / total;
            double drawNumber = Convert.ToDouble(drawCount) / total;

            float boySize = (float) boyNumber * 500.0f + 8.0f;
            float bullSize = (float) bullNumber * 500.0f + 8.0f;
            float drawSize = (float) drawNumber * 500.0f ;

            Debug.Log( boySize.ToString()+ " // "+ bullSize.ToString() +" // "+ drawSize.ToString());

            Red.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, boySize );
            Blue.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, bullSize );
            Green.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,drawSize);

            var greenX = boySize + drawSize /2  - 258.0f ; //  왼쪽에 맞춰 녹색의 중심을 이동시키자.

            var greenTransform = Green.GetComponent<RectTransform>();
            greenTransform.localPosition = new Vector3(greenX, 0,0);            //Green.SetPositionAndRotation()

            //"P포멧에서 % 앞에 띄어쓰기 삭제"
            RedText.text = boyNumber.ToString("P1").Replace(" ", "");
            BlueText.text = bullNumber.ToString("P1").Replace(" ", "");
            GreenText.text = drawNumber.ToString("P1").Replace(" ", "");
        }
    }

}