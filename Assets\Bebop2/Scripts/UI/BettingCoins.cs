using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Common.Scripts.Utils;
using Bebop.Protocol;

using CoinList = System.Collections.Generic.Dictionary<Bebop.Protocol.BettingType, System.Collections.Generic.List<UnityEngine.GameObject>>;

namespace Bebop
{
    public class BettingCoins : MonoBehaviour
    {
        public interface IBettingCoinData
        {
            BettingType BettingType { get; set; }
            CoinType CoinType { get; set; }
            Vector3 StartPosition { get; set; }

            bool IsMine { get; set; }

            Material DissolveMaterial { set; }
        }

        public Canvas rootCanvas;
        public GameObject prefabCoin;

        //-> 0번째는 유저(나) 전용으로 하자
        public List<Transform> lstOutPosition;

        [Header("Effect")]
        public GameObject fxWin;
        public GameObject fxSuperNova;
        public GameObject fxGodBrain;

        [Header("Material")]
        public Material matDissolve;

        [Header("Betting Odds Data")]
        public BebopValueData bebopValueData;

        private ObjectContainer objContainer = new ObjectContainer();
        private ObjectContainer fxWinContainer = new ObjectContainer();
        private ObjectContainer fxSuperNovaContainer = new ObjectContainer();
        private ObjectContainer fxGodBrainContainer = new ObjectContainer();


        private CoinList myCoins = new CoinList();
        private Dictionary<int, CoinList> playerCoins = new Dictionary<int, CoinList>();

        private Dictionary<BettingType, RectTransform> dicBettingTrans = new Dictionary<BettingType, RectTransform>();

        private Rect coinRect;
        //private List<GameObject> lstMyCoin = new List<GameObject>();

        private void Awake()
        {
            objContainer.Set(prefabCoin, transform);
            fxWinContainer.Set(fxWin, null);
            fxSuperNovaContainer.Set(fxSuperNova, null);
            fxGodBrainContainer.Set(fxGodBrain, null);

            coinRect = prefabCoin.GetComponent<RectTransform>().rect;

            prefabCoin.SetActive(false);
            //prefabCoinMine.SetActive(false);
            fxWin.SetActive(false);
            fxSuperNova.SetActive(false);
            fxGodBrain.SetActive(false);

            foreach (Protocol.BettingType enumValue in Enum.GetValues(typeof(Protocol.BettingType)))
            {
                if (enumValue == Protocol.BettingType.None)
                    continue;

                myCoins.Add(enumValue, new List<GameObject>());
            }

            //dicCoinData.Clear();
        }

        //public void ClearCoin()
        //{
        //    dicCoinData.Clear();
        //}

        //-> 내가 배팅한다.
        //-> 1 : SuperNova
        //-> 2 : GodBrain
        public void MyBetting(BettingType bettingType, CoinType coinType, int coinCount, int fxType)
        {
            //this.PushCoin(accountId, bettingInfo);
            this.Betting(bettingType, coinType, coinCount, fxType);
        }

        public void MyBettingSnap(BettingType bettingType, CoinType coinType, int coinCount)
        {
            for (int i = 0; i < coinCount; ++i)
            {
                var coin = objContainer.GetItem();
                coin.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 180f));
                coin.transform.GetComponent<Image>().color = Color.white;

                var coinData = coin.GetComponent<IBettingCoinData>();
                coinData.IsMine = true;
                coinData.BettingType = bettingType;
                coinData.CoinType = coinType;
                coinData.DissolveMaterial = null;

                Transform outPos = lstOutPosition[0];
                coinData.StartPosition = outPos.localPosition;

                RectTransform inPos = dicBettingTrans[bettingType];

                coin.transform.localPosition = GetAdjustPos(inPos, true);

                KeepCoin(bettingType, coin);
            }

            SetAsLastSibling();
        }

        private void Betting(BettingType bettingType, CoinType coinType, int coinCount, int fxType)
        {
            for (int i = 0; i < coinCount; i++)
            {
                var coin = objContainer.GetItem();
                coin.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 180f));
                coin.transform.GetComponent<Image>().color = Color.white;

                var coinData = coin.GetComponent<IBettingCoinData>();
                coinData.IsMine = true;
                coinData.BettingType = bettingType;
                coinData.CoinType = coinType;
                coinData.DissolveMaterial = null;

                GameObject fx = null;
                if (fxType > 0)
                {
                    fx = fxType > 1 ? fxGodBrainContainer.GetItem() : fxSuperNovaContainer.GetItem();
                    fx.transform.SetParent(coin.transform);
                    fx.transform.localScale = Vector3.one;
                    fx.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 180f));
                    fx.transform.localPosition = Vector3.zero;
                }

                RectTransform inPos = dicBettingTrans[bettingType];

                Transform outPos = lstOutPosition[0];
                coinData.StartPosition = outPos.localPosition;
                coin.transform.localPosition = coinData.StartPosition;

                float speed = UnityEngine.Random.Range(0.2f, 0.5f);
                float rotateDuration = 0.2f;

                coin.transform.DOLocalMove(GetAdjustPos(inPos, true), speed).OnComplete(() =>
                {
                    coin.transform.DORotate(new Vector3(0f, 0f, 360f), rotateDuration).OnComplete(() =>
                    {
                        if (fx != null)
                            fx.SetActive(false);
                    });
                });

                KeepCoin(bettingType, coin);
            }

            SetAsLastSibling();
        }



        public void PlayerBettingSnap(int accountId, BettingType bettingType, CoinType coinType, int coinCount)
        {
            this.PlayerBetting(accountId, bettingType, coinType, coinCount, null, true, 0);
        }

        public void PlayerBetting(int accountId, BettingType bettingType, CoinType coinType, int coinCount, RectTransform topPlayerTrans, int fxType)
        {
            //this.PushCoin(accountId, bettingInfo);
            this.PlayerBetting(accountId, bettingType, coinType, coinCount, topPlayerTrans, false, fxType);
        }

        private void PlayerBetting(int accountId, BettingType bettingType, CoinType coinType, int coinCount, RectTransform rectTransform, bool isSnap, int fxType)
        {
            for (int i = 0; i < coinCount; i++)
            {
                GameObject coin = objContainer.GetItem();
                //coin.transform.rotation = Quaternion.identity;
                coin.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 180f));
                coin.transform.GetComponent<Image>().color = Color.white;

                var coinData = coin.GetComponent<IBettingCoinData>();
                coinData.IsMine = false;
                coinData.BettingType = bettingType;
                coinData.CoinType = coinType;
                coinData.DissolveMaterial = null;

                RectTransform inPos = dicBettingTrans[bettingType];

                if (rectTransform != null)
                {
                    coinData.StartPosition = rootCanvas.transform.InverseTransformPoint(rectTransform.position);
                    coin.transform.localPosition = coinData.StartPosition;
                }
                else
                {
                    int outIndex = UnityEngine.Random.Range(1, lstOutPosition.Count);
                    Transform outPos = lstOutPosition[outIndex];
                    coinData.StartPosition = outPos.localPosition;
                    coin.transform.localPosition = coinData.StartPosition;
                }

                GameObject fx = null;
                if (fxType > 0)
                {
                    fx = fxType > 1 ? fxGodBrainContainer.GetItem() : fxSuperNovaContainer.GetItem();
                    fx.transform.SetParent(coin.transform);
                    fx.transform.localScale = Vector3.one;
                    fx.transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 180f));
                    fx.transform.localPosition = Vector3.zero;
                }

                float speed = UnityEngine.Random.Range(0.2f, 0.5f);
                float rotateDuration = 0.2f;

                if (isSnap == true)
                {
                    speed = rotateDuration = 0f;
                }

                coin.transform.DOLocalMove(GetAdjustPos(inPos, false), speed).OnComplete(() =>
                {
                    coin.transform.DORotate(new Vector3(0f, 0f, 360f), rotateDuration).OnComplete(() =>
                    {
                        if (fx != null)
                        {
                            fx.SetActive(false);
                        }
                    });
                });

                KeepPlayerCoin(accountId, bettingType, coin);
            }

            SetAsLastSibling();
        }

        public void CancelMyBetting(BettingType bettingType, CoinType coinType, int coinCount)
        {
            for (int i = 0; i < coinCount; ++i)
            {
                GameObject cancelCoin = myCoins[bettingType].Find(coin => coin.GetComponent<IBettingCoinData>().CoinType == coinType);

                if (cancelCoin != null)
                {
                    var data = cancelCoin.GetComponent<IBettingCoinData>();
                    cancelCoin.transform.DOLocalMove(data.StartPosition, 0.5f).OnComplete(() => cancelCoin.SetActive(false));
                    myCoins[bettingType].Remove(cancelCoin);
                }
            }
        }
        public void CancelPlayerBetting(int accountId, BettingType bettingType, CoinType coinType, int coinCount)
        {
            CoinList coinList;
            for (int i = 0; i < coinCount; i++)
            {
                if (playerCoins.TryGetValue(accountId, out coinList))
                {
                    GameObject cancelCoin = coinList[bettingType].Find(coin => coin.GetComponent<IBettingCoinData>().CoinType == coinType);

                    if (cancelCoin != null)
                    {
                        var data = cancelCoin.GetComponent<IBettingCoinData>();
                        cancelCoin.transform.DOLocalMove(data.StartPosition, 0.5f).OnComplete(() => cancelCoin.SetActive(false));
                        coinList[bettingType].Remove(cancelCoin);
                    }
                } 
            }
        }

        public void Win(int winType, DG.Tweening.TweenCallback callback)
        {
            // 테스트 용
            //List<KeyValuePair<int, long>> lstWinPlayer = new List<KeyValuePair<int, long>>();

            //List<BettingType> lstWinType = new List<BettingType>();

            foreach (BettingType type in Enum.GetValues(typeof(BettingType)))
            {
                if ((winType & (int)type) != 0)
                {
                    var lst = myCoins[type];
                    lst.ForEach(coin =>
                    {
                        var coinData = coin.GetComponent<IBettingCoinData>();

                        coin.transform.DOLocalMove(coinData.StartPosition, 0.5f).SetDelay(1.5f);
                        //AttachFxWin(coin);
                    });

                    foreach (var pLst in playerCoins.Values)
                    {
                        if (pLst.ContainsKey(type) == true)
                        {
                            pLst[type].ForEach(coin =>
                            {
                                var coinData = coin.GetComponent<IBettingCoinData>();

                                coin.transform.DOLocalMove(coinData.StartPosition, 0.5f).SetDelay(1.5f);
                                //AttachFxWin(coin);
                            });
                        }
                    }
                    //lstWinType.Add(type);
                }
            }

            float temp = 0f;
            //DOTween.To(() => temp, x => temp = x, 1, 0f).SetDelay(2f).OnComplete(()=>ShowWinAmount(lstWinType));

            DOTween.To(() => temp, x => temp = x, 1, 0f).SetDelay(2f).OnComplete(()=>
            {
                callback.Invoke();
                Reset();
            });

            foreach (Protocol.BettingType enumValue in Enum.GetValues(typeof(Protocol.BettingType)))
            {
                if (enumValue == Protocol.BettingType.None)
                    continue;

                if ((winType & (uint)enumValue) != 0)
                {
                    continue;
                }

                var lst = myCoins[enumValue];
                lst.ForEach(coin =>
                {
                    var coinData = coin.GetComponent<IBettingCoinData>();
                    coinData.DissolveMaterial = matDissolve;
                });

                foreach (var pLst in playerCoins.Values)
                {
                    if (pLst.ContainsKey(enumValue) == true)
                    {
                        pLst[enumValue].ForEach(coin =>
                        {
                            var coinData = coin.GetComponent<IBettingCoinData>();
                            coinData.DissolveMaterial = matDissolve;
                        });
                    }
                }
            }

            matDissolve.SetFloat("_Edges", 0.05f);
            DOTween.To(() => matDissolve.GetFloat("_Level"), x => matDissolve.SetFloat("_Level", x), 1, 2.5f);
        }

        //private void ShowWinAmount(List<Protocol.BettingType> lstWinType)
        //{
        //    var avatarData = UI.PlayerAvatar.Instance.lstAvatarData;
        //    var topPlayerIds = avatarData.Where(aData => aData.avatarItem.IsSeatPlayer()).Select(aData => aData.avatarItem.playerInfo.AccountInfo.AccountId);

        //    List<GameObject> coinList = new List<GameObject>();
        //    lstWinType.ForEach(t =>
        //    {
        //        coinList.AddRange(dicCoins[t]);
        //    });

        //    double totalAmount = 0;
        //    foreach (var id in topPlayerIds)
        //    {
        //        totalAmount = coinList.Sum(coin =>
        //        {
        //            var data = coin.GetComponent<IBettingCoinData>();
        //            if (data.AccountId == id)
        //                return data.Amount * Array.Find(bebopValueData.bettingOdds, oddData => oddData.type == data.BettingType).odds;
        //            else
        //                return 0;
        //        });

        //        totalAmount *= 0.01;

        //        if (totalAmount > 0)
        //        {
        //            UI.PlayerAvatar.Instance.PlayWinning(id, totalAmount);
        //        }
        //    }

        //    //-> 내꺼
        //    totalAmount = coinList.Sum(coin =>
        //    {
        //        var data = coin.GetComponent<IBettingCoinData>();
        //        if (data.AccountId == UserData.Instance.AccountId)
        //            return data.Amount * Array.Find(bebopValueData.bettingOdds, oddData => oddData.type == data.BettingType).odds;
        //        else
        //            return 0;
        //    });

        //    totalAmount *= 0.01;

        //    if (totalAmount > 0)
        //    {
        //        MyInfo.Instance.PlayAmount(totalAmount);
        //    }
        //}

        private void AttachFxWin(GameObject obj)
        {
            var fx = fxWinContainer.GetItem();
            fx.transform.SetParent(obj.transform);
            fx.transform.localPosition = Vector3.zero;
            fx.transform.localScale = Vector3.one;
        }

        private void SetAsLastSibling()
        {
            foreach (var coins in myCoins.Values)
            {
                foreach (var coin in coins)
                {
                    coin.transform.SetAsLastSibling();
                }
            }
        }

        private void KeepCoin(BettingType type, GameObject coin)
        {
            //-> 스택처럼 쓰기 위함
            myCoins[type].Insert(0, coin);
        }

        private void KeepPlayerCoin(int accountId, BettingType type, GameObject coin)
        {
            if (playerCoins.ContainsKey(accountId) == false)
            {
                playerCoins.Add(accountId, new CoinList());

                foreach (Protocol.BettingType enumValue in Enum.GetValues(typeof(Protocol.BettingType)))
                {
                    if (enumValue == Protocol.BettingType.None)
                        continue;

                    playerCoins[accountId].Add(enumValue, new List<GameObject>());
                }
            }

            playerCoins[accountId][type].Insert(0, coin);
        }

        public void Reset()
        {
            objContainer.HideAll();
            fxWinContainer.HideAll();

            var itor = myCoins.GetEnumerator();
            while (itor.MoveNext())
            {
                itor.Current.Value.Clear();
            }

            var pitor = playerCoins.GetEnumerator();
            while (pitor.MoveNext())
            {
                pitor.Current.Value.Clear();
            }
            playerCoins.Clear();

            //lstMyCoin.Clear();
            //this.ClearCoin();

            matDissolve.SetFloat("_Level", 0f);
            matDissolve.SetFloat("_Edges", 0f);
        }

        private Vector3 GetAdjustPos(RectTransform rectTrans, bool isMine)
        {
            Rect rect = rectTrans.rect;

            if (isMine)
            {
                rect.min *= 0.5f;
                rect.max *= 0.5f;
            }

            Vector3 pos = rootCanvas.transform.InverseTransformPoint(rectTrans.position);

            Vector2 coinSize = coinRect.size * 0.5f;
            pos.x += UnityEngine.Random.Range((int)(rect.xMin + coinSize.x), (int)(rect.xMax + 1 - coinSize.x));
            pos.y += UnityEngine.Random.Range((int)(rect.yMin + coinSize.y), (int)(rect.yMax + 1 - coinSize.y));

            return pos;
        }

        public void RegisterBettingPlace(Protocol.BettingType type, RectTransform rectTransform)
        {
            dicBettingTrans.Add(type, rectTransform);
        }
    }
}
