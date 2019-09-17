using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;

namespace Bebop.UI
{
    public class WaitIndicator : MonoBehaviour
    {
        public Image imgRoot;
        public Image imgBg;
        public Image imgLoding;

        private static WaitIndicator _instance;

        private float DurationRot = 1.2f;
        public static WaitIndicator Instance
        {
            get { return _instance; }
        }

        private void Awake()
        {
            _instance = this;
            _instance.transform.localPosition = Vector3.zero;
            _instance.transform.localScale = Vector3.one;
            _instance.transform.SetAsLastSibling();

            _instance.imgLoding.transform.localRotation = Quaternion.identity;

            imgBg.gameObject.SetActive(false);
            imgRoot.gameObject.SetActive(false);
        }

        private void Start()
        {
            imgLoding.transform.DORotate(new Vector3(0, 0, 360), DurationRot, RotateMode.FastBeyond360).
               SetEase(Ease.Linear).
               SetLoops(-1, LoopType.Restart);
        }

        //private void Update()
        //{
        //    if (Input.GetKeyUp(KeyCode.P))
        //    {
        //        WaitIndicator.SetActive(true, 0);
        //    }

        //    if (Input.GetKeyUp(KeyCode.O))
        //    {
        //        WaitIndicator.SetActive(false);
        //    }
        //}

        private void OnDestroy()
        {
            _instance = null;
        }

        private void ShowLoading()
        {
            imgBg.gameObject.SetActive(true);
        }

        private void HideLoading()
        {
            imgBg.gameObject.SetActive(false);
            imgRoot.gameObject.SetActive(false);
        }

        private void ReserveShowLoading(float waitTime)
        {
            CancelReserve("HideLoading");
            CancelReserve("ShowLoading");
            Invoke("ShowLoading", waitTime);
        }

        private void ReserveHideLoading(float exitTime)
        {
            CancelReserve("HideLoading");
            CancelReserve("ShowLoading");

            //Hide 시킬때 바로 끄면, 눈이 아프므로, 약간의 딜레이는 준다.
            Invoke("HideLoading", exitTime);
        }

        private void CancelReserve(string methodName)
        {
            if (IsInvoking(methodName))
                CancelInvoke(methodName);
        }

        public static bool ActiveSelf()
        {
            if (Instance == null)
            {
                return false;
            }

            return Instance.gameObject.activeSelf && Instance.imgBg.gameObject.activeSelf;
        }

        public static void SetActive(bool isActive, float waitTime = 1.0f, float exitTime = 0)
        {
            if (isActive == true)
            {
                Instance.gameObject.SetActive(isActive);
                Instance.imgRoot.gameObject.SetActive(isActive);
                Instance.transform.SetAsLastSibling();
                Instance.ReserveShowLoading(waitTime);
            }
            else
            {
                Instance.ReserveHideLoading(exitTime);
                //Instance.gameObject.SetActive(isActive);
                
            }
                
        }
    }
}
