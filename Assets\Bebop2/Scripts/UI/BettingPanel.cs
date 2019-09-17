using Bebop.Protocol;
using Common.Scripts.Localization;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bebop
{
    public class BettingPanel : MonoBehaviour
    {
        [Header("Image")]
        public Sprite fireOn;
        public Sprite fireOff;
        public Sprite starOn;
        public Sprite starOff;

        public Sprite historyWin;
        public Sprite historyLoss;

        [Serializable]
        public class Panel
        {
            public Protocol.BettingType type;
            public Button button;
            public TextMeshProUGUI odds;
            public Image fire;
            public Image star;
            public TextMeshProUGUI text;
            [HideInInspector]
            public long totalAmount;
            public GameObject history;
            [HideInInspector]
            public Image[] arrHistory;
            public TextMeshProUGUI txtHistory;
            [HideInInspector]
            public string lastWin;
            public GameObject fxOutline;
            [HideInInspector]
            public GameObject myBettingAmount;
            [HideInInspector]
            public long myTotalAmount;
        }

        [Header("Panel")]
        public Panel[] arrPanel;

        [Header("Data")]
        public BebopValueData valueData;

        [Header("My Betting Amount")]
        public GameObject prefabMyBettingAmount;

        private Dictionary<Protocol.BettingType, Panel> dicPanel = new Dictionary<Protocol.BettingType, Panel>();
        private Queue<Protocol.BettingType> queBettingType = new Queue<Protocol.BettingType>();

        private BettingController controller;

        private void Start()
        {
            Array.ForEach(arrPanel, panel =>
            {
                dicPanel.Add(panel.type, panel);

                //panel.odds.text = Array.Find(valueData.bettingOdds, data => data.type == panel.type).odds.ToString();

                panel.text.text = "0";
                panel.fire.sprite = fireOff;
                panel.star.sprite = starOff;

                if (panel.history != null)
                {
                    panel.arrHistory = panel.history.GetComponentsInChildren<Image>();
                    Array.ForEach(panel.arrHistory, h => h.sprite = historyLoss);
                }

                if (panel.history != null)
                    panel.history.SetActive(false);

                if (panel.txtHistory != null)
                    panel.txtHistory.gameObject.SetActive(false);

                panel.fxOutline.SetActive(false);

                panel.myBettingAmount = Instantiate<GameObject>(prefabMyBettingAmount);
                var rect = panel.myBettingAmount.GetComponent<RectTransform>();
                rect.pivot = new Vector2(0.5f, 1f);
                panel.myBettingAmount.transform.SetParent(panel.button.transform);
                panel.myBettingAmount.transform.localPosition = Vector3.zero;
                panel.myBettingAmount.transform.localScale = Vector3.one;
                panel.myBettingAmount.SetActive(false);
            });

            prefabMyBettingAmount.SetActive(false);

            SetEnabledAll(false);

            LocalizationManager.LanguageChanged += LanguageChanged;
        }

        private void OnDestroy()
        {
            LocalizationManager.LanguageChanged -= LanguageChanged;
        }

        private void LanguageChanged()
        {
            var itor = dicPanel.GetEnumerator();
            while (itor.MoveNext())
            {
                if (itor.Current.Value.history == null)
                    continue;

                var str = itor.Current.Value.lastWin;
                string strFormat = Common.Scripts.Localization.LocalizationManager.Instance.GetText("rank_hitory");
                itor.Current.Value.txtHistory.text = string.Format(strFormat, str);
            }
        }

        //public void Init()
        //{
        //    Array.ForEach(arrPanel, panel =>
        //    {
        //        panel.text.text = "0";
        //        panel.fire.sprite = fireOff;
        //        panel.star.sprite = starOff;

        //        if (panel.history != null)
        //            panel.history.SetActive(false);

        //        if (panel.txtHistory != null)
        //            panel.txtHistory.gameObject.SetActive(false);

        //        panel.button.enabled = true;
        //    });

        //    queBettingType.Clear();
        //}

        public void Reset()
        {
            this.HideFxOutline();

            Array.ForEach(arrPanel, panel =>
            {
                panel.text.text = "0";
                panel.fire.sprite = fireOff;
                panel.star.sprite = starOff;
                panel.myBettingAmount.SetActive(false);

                panel.totalAmount = 0;
                panel.myTotalAmount = 0;
            });
        }

        private void SetButtonEvent(BettingController controller)
        {
            for (int i=0, imax=arrPanel.Length; i<imax; ++i)
            {
                var el = arrPanel[i];
                el.button.onClick.AddListener(() => controller.OnClickBettingPanel(el.type));
            }
        }

        public void SetController(BettingController controller)
        {
            this.controller = controller;

            SetButtonEvent(controller);
        }

        public IEnumerator<Panel> GetEnumerator()
        {
            return System.Array.AsReadOnly<Panel>(arrPanel).GetEnumerator();
        }

        public void SetMyAmount(BettingType type, long amount)
        {
            amount /= 100;

            dicPanel[type].myTotalAmount = amount;

            SetMyAmountText(type);
        }

        public void AddMyAmount(BettingType type, long amount)
        {
            amount /= 100;

            dicPanel[type].myTotalAmount += amount;

            SetMyAmountText(type);
        }

        public void SubMyAmount(BettingType type, long amount)
        {
            amount /= 100;

            dicPanel[type].myTotalAmount -= amount;

            SetMyAmountText(type);
        }

        private void SetMyAmountText(BettingType type)
        {
            if (dicPanel[type].myTotalAmount > 0)
            {
                var myBettingAmount = dicPanel[type].myBettingAmount;
                myBettingAmount.SetActive(true);
                var text = myBettingAmount.GetComponentInChildren<TextMeshProUGUI>();
                text.text = dicPanel[type].myTotalAmount.ToString("#,0");
                ContentSizeFitter fitter = text.GetComponent<ContentSizeFitter>();
                fitter.SetLayoutHorizontal();
                fitter.SetLayoutVertical();

                fitter = myBettingAmount.GetComponent<ContentSizeFitter>();
                fitter.SetLayoutHorizontal();
                fitter.SetLayoutVertical();
            }
            else
            {
                dicPanel[type].myBettingAmount.SetActive(false);
            }
        }

        public void SetAmount(BettingType type, long amount)
        {
            amount /= 100;

            dicPanel[type].totalAmount = amount;
            dicPanel[type].text.text = amount.ToString("#,0");
        }

        public void AddAmount(BettingType type, long amount)
        {
            amount /= 100;

            dicPanel[type].totalAmount += amount;
            dicPanel[type].text.text = dicPanel[type].totalAmount.ToString("#,0");
        }

        public void SubAmount(BettingType type, long amount)
        {
            amount /= 100;

            dicPanel[type].totalAmount -= amount;
            dicPanel[type].text.text = dicPanel[type].totalAmount.ToString("#,0");
        }

        public void SetHistory(BettingType type, List<int> types, int lastWin, bool isSnap)
        {
            StartCoroutine(CoSetHistory(type, types, lastWin, isSnap));
        }

        private IEnumerator CoSetHistory(BettingType type, List<int> types, int lastWin, bool isSnap)
        {
            float delay = isSnap ? 0f : 2f;
            yield return new WaitForSecondsRealtime(delay);

            var panelData = dicPanel[type];

            if (lastWin > 10)
            {
                panelData.history.SetActive(false);
                panelData.txtHistory.gameObject.SetActive(true);

                int MaxLastWin = Protocol.BettingType.FourSFlushRFlush != type ? 200 : 300;

                panelData.lastWin = lastWin > MaxLastWin ? $"{MaxLastWin}+" : lastWin.ToString();
                string strFormat = Common.Scripts.Localization.LocalizationManager.Instance.GetText("rank_hitory");
                panelData.txtHistory.text = string.Format(strFormat, panelData.lastWin);
            }
            else
            {
                panelData.history.SetActive(true);
                panelData.txtHistory.gameObject.SetActive(false);

                if (isSnap == false)
                {
                    Vector3 startPos = panelData.history.transform.localPosition;
                    Vector3 endPos = startPos - new Vector3(historyWin.rect.width + 4f, 0f, 0f);
                    panelData.history.transform.DOLocalMove(endPos, 0.5f);

                    yield return new WaitForSecondsRealtime(1f);

                    panelData.history.transform.localPosition = startPos;
                }

                for (int i = 0, imax = types.Count; i < imax; ++i)
                {
                    bool isWin = (types[i] & (int)type) != 0;

                    panelData.arrHistory[i].sprite = isWin ? historyWin : historyLoss;
                }

            }
        }

        public void SetSuperNovaImage(BettingType type, bool isActive)
        {
            dicPanel[type].fire.sprite = isActive ? fireOn : fireOff;
        }

        public void SetGodBrainImage(BettingType type, bool isActive)
        {
            dicPanel[type].star.sprite = isActive ? starOn : starOff;
        }

        public void Disable(BettingType type)
        {
            dicPanel[type].button.enabled = false;
            queBettingType.Enqueue(type);
        }

        public void Enable()
        {
            if (queBettingType.Count <= 0)
                return;

            dicPanel[queBettingType.Dequeue()].button.enabled = true;
        }

        public void SetEnabledAll(bool isActive)
        {
            var itor = dicPanel.GetEnumerator();
            while(itor.MoveNext())
            {
                itor.Current.Value.button.enabled = isActive;
            }
        }

        public void Win(int winType)
        {
            //SetEnabledAll(false);

            //Debug.Log("====================================");
            //Debug.Log("Protocol.NotifyHandStateCardOpenIn.WinningBettingTypes : " + winType.ToString());
            //foreach(Protocol.BettingType betType in Enum.GetValues(typeof(Protocol.BettingType)))
            //{
            //    if ((winType & (int)betType) != 0)
            //    {
            //        Debug.Log(betType.ToString());
            //        ShowFxOutline(betType);
            //    }
            //}
            //Debug.Log("====================================");

            StartCoroutine(CoWin(winType));
        }

        private IEnumerator CoWin(int winType)
        {
            SetEnabledAll(false);

            Debug.Log("====================================");
            Debug.Log("Protocol.NotifyHandStateCardOpenIn.WinningBettingTypes : " + winType.ToString());
            foreach (Protocol.BettingType betType in Enum.GetValues(typeof(Protocol.BettingType)))
            {
                if (betType == Protocol.BettingType.None)
                {
                    continue;
                }

                if (betType == Protocol.BettingType.Suited || betType == Protocol.BettingType.HighCardOnePair)
                {
                    yield return new WaitForSecondsRealtime(0.35f);
                }

                if ((winType & (int)betType) != 0)
                {
                    Debug.Log(betType.ToString());
                    ShowFxOutline(betType);
                }
                else
                {
                    dicPanel[betType].myBettingAmount.SetActive(false);
                }
            }
            Debug.Log("====================================");
        }

        public void HideFxOutline()
        {
            var itor = dicPanel.GetEnumerator();
            while(itor.MoveNext())
            {
                itor.Current.Value.fxOutline.SetActive(false);
            }
        }

        private void ShowFxOutline(BettingType type)
        {
            dicPanel[type].fxOutline.SetActive(true);
        }

        //-> 월드 좌표를 리턴한다. (메인 히스토리 이펙트 시작 포지션 때문에..)
        public Vector3 GetPanelPosition(BettingType type)
        {
            return dicPanel[type].button.transform.position;
        }
    } 
}
