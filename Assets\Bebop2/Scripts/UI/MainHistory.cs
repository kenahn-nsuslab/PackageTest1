using Bebop.Protocol;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Bebop
{
    public class MainHistory : MonoBehaviour
    {
        [Header("Sprite")]
        public Sprite cowboy;
        public Sprite cow;
        public Sprite draw;
        public Sprite etc;

        [Header("Bead Item")]
        public Image[] arrWin;

        [Header("Layout Group")]
        public HorizontalLayoutGroup layoutGroup;

        [Header("Effect")]
        public GameObject fxCircle;

        [Serializable]
        public class EffectTrail
        {
            public Protocol.BettingType type;
            public GameObject fxTrail;
        }
        public EffectTrail[] arrFxTrail;

        [Header("Bead Sprite for FxTrail")]
        public Image imgWinBead;

        private Button btnSelf;
        private BettingController controller;

        private void Start()
        {
            arrWin = arrWin.Reverse().ToArray();
            //layoutGroup.enabled = false;

            fxCircle.SetActive(false);
            Array.ForEach(arrFxTrail, fx => fx.fxTrail.SetActive(false));

            imgWinBead.gameObject.SetActive(false);
        }

        public void SetHistorySnap(List<int> types)
        {
            Array.ForEach(arrWin, win => win.gameObject.SetActive(false));

            for (int i=0, imax=types.Count; i<imax; ++i)
            {
                foreach (Protocol.BettingType betType in Enum.GetValues(typeof(Protocol.BettingType)))
                {
                    if ((types[i] & (int)betType) != 0)
                    {
                        ChangeSprite(betType, arrWin[i]);
                        arrWin[i].gameObject.SetActive(true);
                    }
                }
            }
        }

        private void ChangeSprite(Protocol.BettingType type, Image image)
        {
            if (type == Protocol.BettingType.Cowboy)
            {
                image.sprite = cowboy;
            }
            else if (type == Protocol.BettingType.Bull)
            {
                image.sprite = cow;
            }
            else if (type == Protocol.BettingType.Draw)
            {
                image.sprite = draw;
            }
        }

        Coroutine coSetHistory = null;
        public void SetHistory(List<int> types, float delay)
        {
            if (coSetHistory != null)
            {
                return;
            }

            coSetHistory = StartCoroutine(CoSetHistory(types, delay));
        }

        private IEnumerator CoSetHistory(List<int> types, float delay)
        {
            yield return new WaitForSecondsRealtime(delay);

            var deactive = Array.Find(arrWin, win => win.gameObject.activeSelf == false);

            //-> 서버에서 시작한지 10게임이 되지 않았다.
            if (deactive != null)
            {
                Array.ForEach(arrWin, win => win.gameObject.SetActive(false));

                for (int i = 1, imax = types.Count; i < imax; ++i)
                {
                    BettingType type = GetWinType(types[i]);
                    arrWin[i].gameObject.SetActive(true);
                    ChangeSprite(type, arrWin[i]);
                }

                yield return null;

                //-> 트레일 이펙트
                BettingType winType = GetWinType(types[0]);
                var startPos = transform.InverseTransformPoint(controller.GetPanelPosition(winType));
                var endPos = transform.InverseTransformPoint(arrWin[1].transform.position);
                endPos += new Vector3(cowboy.rect.width, 0f, 0f);

                var fxTrail = Array.Find(arrFxTrail, fx => fx.type == winType).fxTrail;
                fxTrail.SetActive(true);
                fxTrail.GetComponent<ParticleSystem>().Play();
                fxTrail.transform.localPosition = startPos;
                fxTrail.transform.DOLocalMove(endPos, 0.5f);

                yield return new WaitForSecondsRealtime(0.5f);

                fxCircle.transform.localPosition = endPos;
                fxCircle.SetActive(true);
                var particles = fxCircle.GetComponentsInChildren<ParticleSystem>();
                Array.ForEach(particles, p => p.Play());

                arrWin[0].gameObject.SetActive(true);
                ChangeSprite(winType, arrWin[0]);
            }
            else
            {
                //-> 트레일 이펙트
                BettingType winType = GetWinType(types[0]);
                var fxStartPos = transform.InverseTransformPoint(controller.GetPanelPosition(winType));
                var fxEndPos = transform.InverseTransformPoint(arrWin[0].transform.position);

                var fxTrail = Array.Find(arrFxTrail, fx => fx.type == winType).fxTrail;
                fxTrail.SetActive(true);

                //-> 움직일 이미지
                ChangeSprite(winType, imgWinBead);
                imgWinBead.gameObject.SetActive(true);
                imgWinBead.transform.SetParent(fxTrail.transform);
                imgWinBead.transform.localPosition = Vector3.zero;

                fxTrail.GetComponent<ParticleSystem>().Play();
                fxTrail.transform.localPosition = fxStartPos;
                fxTrail.transform.DOLocalMove(fxEndPos, 0.7f).OnComplete(()=>fxTrail.gameObject.SetActive(false));

                //-> 메인 히스토리 이동
                //layoutGroup.enabled = false;

                Vector3 startPos = layoutGroup.transform.localPosition;
                yield return layoutGroup.transform.DOLocalMoveX(startPos.x - cowboy.rect.width, 0.7f).OnComplete(()=>
                {
                    layoutGroup.transform.localPosition = startPos;

                    fxCircle.transform.localPosition = fxEndPos;
                    fxCircle.SetActive(true);
                    var particles = fxCircle.GetComponentsInChildren<ParticleSystem>();
                    Array.ForEach(particles, p => p.Play());

                    for (int i = 0, imax = types.Count; i < imax; ++i)
                    {
                        BettingType type = GetWinType(types[i]);
                        ChangeSprite(type, arrWin[i]);
                    }
                });

                //yield return new WaitForSecondsRealtime(0.55f);

                //layoutGroup.transform.localPosition = startPos;

                

                //layoutGroup.enabled = true;
            }

            coSetHistory = null;
        }

        public void SetController(BettingController controller)
        {
            this.controller = controller;

            btnSelf = GetComponent<Button>()?? gameObject.AddComponent<Button>();

            btnSelf.onClick.AddListener(controller.OnClickMainHistory);

            btnSelf.transition = Selectable.Transition.None;
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
    } 
}
