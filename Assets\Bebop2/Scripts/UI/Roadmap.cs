using Bebop.Protocol;
using Common.Scripts.Sound.Managers;
using Common.Scripts.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bebop
{
    public class Roadmap : MonoBehaviour
    {
        public enum E_Color
        {
            None,
            Red,
            Blue,
            Green,
        }

        public interface IItem
        {
            void Init();
            E_Color GetColor();
            void SetColor(E_Color color);
            void SetString(string result);
            void SetDrawCount(int count);
        }

        private const int SimpleRoadmapItemCount = 16;
        private const int BeadRoadmapInitCount = 40;

        [Header("Cancel Button")]
        public Button btnCancel;

        [Header("Simple Road")]
        public Transform simpleRoadParent;
        public GameObject prefabSimpleItem;
        public GameObject simpleItemNewIcon;
        private ObjectContainer simpleItemContainer = new ObjectContainer();

        [Header("Bead Road")]
        public Transform beadRoadParent;
        public GameObject prefabBeadItem;
        public GameObject beadItemNewIcon;
        private ObjectContainer beadItemContainer = new ObjectContainer();

        [Header("Big Road")]
        public Transform bigRoadParent;
        public GameObject prefabBigItem;
        private ObjectContainer bigItemContainer = new ObjectContainer();


        [Serializable]
        public class Gauge
        {
            public E_Color color;
            public Image imgGauge;
            public TextMeshProUGUI txtGauge;
        }
        [Header("Gauge")]
        public Gauge[] arrGauge;

        [Serializable]
        public class ToggleItem
        {
            public Toggle toggle;
            public TextMeshProUGUI text;
            public GameObject objContent;
        }
        [Header("Toggle")]
        public ToggleItem[] arrToggleItem;

        [Header("TextMesh Material")]
        public Material matTglOn;
        public Material matTglOff;

        private int matTglOnIndex = 13;
        private int matTglOffIndex = 9;

        public long HandId { get; private set; }

        private void Awake()
        {
            simpleItemContainer.Set(prefabSimpleItem, simpleRoadParent);
            prefabSimpleItem.SetActive(false);
            simpleItemNewIcon.SetActive(false);

            beadItemContainer.Set(prefabBeadItem, beadRoadParent);
            prefabBeadItem.SetActive(false);
            beadItemNewIcon.SetActive(false);

            bigItemContainer.Set(prefabBigItem, bigRoadParent);
            prefabBigItem.SetActive(false);

            btnCancel.onClick.AddListener(OnClickCancel);

            for (int i=0, imax=arrToggleItem.Length; i<imax; ++i)
            {
                var item = arrToggleItem[i];
                item.toggle.onValueChanged.AddListener(isOn => OnClickToggle(isOn, item));
                item.objContent.SetActive(item.toggle.isOn);
            }

            for (int i = 0, imax = SimpleRoadmapItemCount; i < imax; ++i)
            {
                IItem item = simpleItemContainer.GetItem<IItem>();
            }

            for (int i = 0, imax = 8 * 6; i < imax; ++i)
            {
                IItem item = beadItemContainer.GetItem<IItem>();
            }

            for (int i = 0, imax = 11 * 6; i < imax; ++i)
            {
                IItem item = bigItemContainer.GetItem<IItem>();
            }

            HandId = -1;
        }

        private IEnumerator Start()
        {
            yield return null;

            transform.localPosition = Vector3.zero;
            gameObject.SetActive(false);
        }

#if UNITY_EDITOR
        List<int> test = new List<int>();
        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.C))
            {
                test.Insert(0, (int)(BettingType.Cowboy | BettingType.HighCardOnePair));
                SetRoadmap(test);
            }
            else if(Input.GetKeyUp(KeyCode.B))
            {
                test.Insert(0, (int)(BettingType.Bull | BettingType.HighCardOnePair));
                SetRoadmap(test);
            }
            else if (Input.GetKeyUp(KeyCode.V))
            {
                test.Insert(0, (int)(BettingType.Draw | BettingType.HighCardOnePair));
                SetRoadmap(test);
            }
        }
#endif

        public void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);

            Array.ForEach(arrToggleItem, (d) => 
            {
                UpdateTlgFontMat(d.toggle.isOn, d);
            });
        }

        public void SetRoadmap(long handId, List<int> lstBettingType)
        {
            if (HandId == handId)
                return;

            HandId = handId;

            SetRoadmap(lstBettingType);
        }

        private void SetRoadmap(List<int> lstBettingType)
        {
            ResetItem();

            if ( lstBettingType == null || lstBettingType.Count <= 0)
                return;

            AdjustBettingList(lstBettingType);

            //-> 리스트 0 부터가 최신
            SetGauge(lstBettingType);

            //-> 밑의 로드맵들은 예전 기록부터 계산한다.
            lstBettingType = lstBettingType.ToList();
            lstBettingType.Reverse();

            SetSimpleRoadmap(lstBettingType);
            SetBeadRoadmap(lstBettingType);
            SetBigRoadmap(lstBettingType);
        }

        private void ResetItem()
        {
            simpleItemContainer.HideAll();

            foreach (var item in beadItemContainer.GetActiveItemAll())
            {
                item.GetComponent<IItem>().Init();
            }

            foreach (var item in bigItemContainer.GetActiveItemAll())
            {
                item.GetComponent<IItem>().Init();
            }

            SetGauge(E_Color.Blue, 0f);
            SetGauge(E_Color.Red, 0f);

            simpleItemNewIcon.gameObject.SetActive(false);
            beadItemNewIcon.gameObject.SetActive(false);
        }

        private void SetSimpleRoadmap(List<int> lstBettingType)
        {
            int count = Mathf.Min(SimpleRoadmapItemCount, lstBettingType.Count);
            var types = lstBettingType.GetRange(lstBettingType.Count - count, count);

            GameObject obj = null;
            for (int i = 0, imax = types.Count; i < imax; ++i)
            {
                var type = types[i];

                obj = simpleItemContainer.GetItem();
                IItem item = obj.GetComponent<IItem>();

                item.SetColor(GetColor(type));
            }

            if (obj != null)
            {
                simpleItemNewIcon.SetActive(true);

                Vector3 pos = simpleRoadParent.parent.InverseTransformPoint(obj.transform.position);
                simpleItemNewIcon.transform.localPosition = pos;
            }
        }

        private int beadItemAddCount = 5;
        private void SetBeadRoadmap(List<int> lstBettingType)
        {
            int count = Mathf.Min(BeadRoadmapInitCount + beadItemAddCount, lstBettingType.Count);

            var types = lstBettingType.GetRange(lstBettingType.Count-count, count);

            var allItem = beadItemContainer.GetActiveItemAll();

            GameObject obj = null;
            for (int i=0, imax= types.Count; i<imax; ++i)
            {
                var type = types[i];

                obj = allItem.ElementAt(i);
                IItem item = obj.GetComponent<IItem>();

                item.SetColor(GetColor(type));
                item.SetString(GetResult(type));
            }

            //-> 마지막 비드아이템 위치로 셋팅
            if (obj != null)
            {
                beadItemNewIcon.SetActive(true);

                Vector3 pos = beadRoadParent.parent.InverseTransformPoint(obj.transform.position);
                beadItemNewIcon.transform.localPosition = pos;
            }

            ++beadItemAddCount;
            if (beadItemAddCount > 6)
            {
                beadItemAddCount = 0;
            }
        }

        private void SetBigRoadmap(List<int> lstBettingType)
        {
            // 앞쪽에 비김이 있나 확인한다.
            // 앞쪽부터 비김이면 이것이 어떤 색상인지 알 수 없음
            int startIndex = 0;
            if (GetWinType(lstBettingType[0]) == BettingType.Draw)
            {
                for (int i = 0, imax = lstBettingType.Count; i < imax; ++i)
                {
                    var type = GetWinType(lstBettingType[i]);
                    if (type == BettingType.Draw)
                    {
                        ++startIndex;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            var allItem = bigItemContainer.GetActiveItemAll();
            Func<int, int, IItem> GetCalIndexItem = (r, c) =>
            {
                return allItem.ElementAt(r * 6 + c).GetComponent<IItem>();
            };

            //-> 이전과 다른 아이템이면 오른쪽으로 한칸 옮기기 위함
            int row = 0;
            //-> 이전과 같은 아이템일 때 제일 밑에서는 오른쪽으로 옮기기 위함
            int sameColorRow = row;
            int column = 0;

            //-> 첫번째 아이템을 셋팅한다.
            //-> 밑의 for문에서 이전과 비교를 하기 위함
            IItem firstItem = GetCalIndexItem(row, column);
            BettingType beforeWinType = GetWinType(lstBettingType[startIndex]);
            firstItem.SetColor(GetColor(beforeWinType));

            int drawCount = 0;
            for (int i = startIndex + 1, imax = lstBettingType.Count; i < imax; ++i)
            {
                BettingType preType = beforeWinType;
                BettingType type = GetWinType(lstBettingType[i]);

                //-> 비겼을 때 이전 아이템에 숫자 기입
                if (type == BettingType.Draw)
                {
                    IItem item = GetCalIndexItem(sameColorRow, column);

                    ++drawCount;
                    item.SetDrawCount(drawCount);
                    continue;
                }

                {
                    drawCount = 0;

                    //-> 같은 색상이다
                    if (preType == type)
                    {
                        //-> 맨 아래이면 오른쪽으로 채우기 시작한다.
                        if (column >= 5)
                        {
                            ++sameColorRow;
                        }
                        else
                        {
                            //-> 바로 밑에 다른 색깔 아이템이 있는지 확인
                            //-> 있다면 오른쪽으로 이동
                            //-> 제일 윗칸인데 바로 아래에 다른 아이템이 있다면 row(다른 색상용)도 이동
                            int checkColumn = column + 1;
                            if (GetCalIndexItem(sameColorRow, checkColumn).GetColor() != E_Color.None)
                            {
                                ++sameColorRow;

                                if (column <= 0)
                                {
                                    ++row;
                                }
                            }
                            else
                            {
                                ++column;
                            }
                        }
                    }
                    else
                    {
                        //-> 이전 아이템과 다르기에 오른쪽 윗칸으로 이동한다.
                        ++row;
                        sameColorRow = row;
                        column = 0;

                        beforeWinType = type;
                    }

                    //-> 제일 오른쪽 한계를 벗어났다.
                    //-> 제일 왼쪽 한줄을 오른쪽 끝으로 붙이면서 초기화
                    if (sameColorRow > 10)
                    {
                        //-> 최소값이 -1 인 이유는 이전과 다른색상일때 ++row를 하게 되는데 0으로 만들기 위함
                        row = Mathf.Max(row-1, -1);
                        sameColorRow = Mathf.Max(sameColorRow-1, -1);

                        for (int j = 0; j < 6; ++j)
                        {
                            var obj = allItem.ElementAt(0);
                            bigItemContainer.PutToLast(obj);
                            obj.GetComponent<IItem>().Init();

                            allItem = bigItemContainer.GetActiveItemAll();
                        }
                    }

                    IItem item = GetCalIndexItem(sameColorRow, column);
                    item.SetColor(GetColor(type));
                }
            }
        }

        private void SetGauge(List<int> lstBettingType)
        {
            int count = Mathf.Min(20, lstBettingType.Count);

            int redCount = lstBettingType.GetRange(0, count).Select(type => (type & (int)BettingType.Cowboy) != 0).Count(result => result == true);
            SetGauge(E_Color.Red, redCount / (float)count);

            int blueCount = lstBettingType.GetRange(0, count).Select(type => (type & (int)BettingType.Bull) != 0).Count(result => result == true);
            SetGauge(E_Color.Blue, blueCount / (float)count);
        }

        private void SetGauge(E_Color color, float percent)
        {
            Gauge info = Array.Find(arrGauge, data => data.color == color);

            float gauge = Mathf.Lerp(60f, 295f, percent);

            RectTransform rect = info.imgGauge.GetComponent<RectTransform>();
            Vector2 size = rect.sizeDelta;
            size.x = gauge;
            rect.sizeDelta = size;

            percent *= 100f;
            info.txtGauge.text = percent.ToString("##0") + "%";
        }

        private E_Color GetColor(int bettingFlag)
        {
            if ((bettingFlag & (int)BettingType.Cowboy) != 0)
            {
                return E_Color.Red;
            }

            if ((bettingFlag & (int)BettingType.Bull) != 0)
            {
                return E_Color.Blue;
            }

            if ((bettingFlag & (int)BettingType.Draw) != 0)
            {
                return E_Color.Green;
            }

            return E_Color.None;
        }

        private E_Color GetColor(BettingType type)
        {
            if (type == BettingType.Cowboy)
                return E_Color.Red;
            else if (type == BettingType.Bull)
                return E_Color.Blue;
            else
                return E_Color.Green;
        }

        private BettingType GetWinType(int bettingFlag)
        {
            BettingType type = BettingType.Draw;
            if ((bettingFlag & (int)BettingType.Cowboy) != 0)
            {
                type = BettingType.Cowboy;
            }
            else if ((bettingFlag & (int)BettingType.Bull) != 0)
            {
                type = BettingType.Bull;
            }

            return type;
        }

        private string GetResult(int bettingFlag)
        {
            BettingType returnType = BettingType.None;
            System.Func<BettingType, bool> checkResult = (bettingType) =>
             {
                 if ((bettingFlag & (int)bettingType) != 0)
                 {
                     returnType = bettingType;
                     return true;
                 }

                 return false;
             };

            string stringKey = "";
            if (checkResult(BettingType.HighCardOnePair))
            {
                stringKey = "mea_hc";
            }
            else if (checkResult(BettingType.TwoPair))
            {
                stringKey = "mea_2p";
            }
            else if (checkResult(BettingType.TripleStraightFlush))
            {
                stringKey = "mea_tok";
            }
            else if (checkResult(BettingType.FullHouse))
            {
                stringKey = "mea_fh";
            }
            else if (checkResult(BettingType.FourSFlushRFlush))
            {
                stringKey = "mea_fok";
            }

            return stringKey;
            //return returnType.ToString();
            //return Common.Scripts.Localization.LocalizationManager.Instance.GetText(stringKey);
        }

        //-> 서버값이 잘못될 경우 대비
        private void AdjustBettingList(List<int> lstBettingType)
        {
            for (int i=0; i<lstBettingType.Count; ++i)
            {
                var type = lstBettingType[i];

                if( (type & (int)BettingType.Cowboy) == 0 &&
                    (type & (int)BettingType.Bull) == 0 &&
                    (type & (int)BettingType.Draw) == 0)
                {
                    lstBettingType.RemoveAt(i);
                    --i;
                }
            }
        }

        #region Button
        private void OnClickCancel()
        {
            SoundManager.PlaySFX(SFX.ClickButton);

            gameObject.SetActive(false);
        }

        private void OnClickToggle(bool isOn, ToggleItem item)
        {
            if (isOn == true)
            {
                SoundManager.PlaySFX(SFX.ClickButton);
            }

            UpdateTlgFontMat(isOn, item);
        }

        private void UpdateTlgFontMat(bool isOn, ToggleItem item)
        {
            var fontData = Common.Scripts.Localization.LocalizationManager.Instance.GetCurrentLanguageFontData();

            item.text.fontSharedMaterial = isOn ? fontData.FontMaterial[matTglOnIndex] : fontData.FontMaterial[matTglOffIndex];
            item.objContent.SetActive(isOn);
        }
        #endregion
    }
}