using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

using Bebop.Protocol;
using TMPro;
using System;
using Bebop.Model.EventParameters;

namespace  Bebop.UI
{
    

    public class UI2DController : MonoBehaviour
    {
        // Start is called before the first frame update

        private TextMeshProUGUI blockUIText;
        private CanvasGroup blockUICanvasGroup;

        public Canvas BlockUI ; //BlockUI_Canvas 오브젝트

        private void Awake() {
            //blockUIText = BlockUI.GetComponentInChildren<TextMeshProUGUI>();
            //blockUICanvasGroup = BlockUI.GetComponent<CanvasGroup>();
        }

        private void OnEnable() {
            
            //ShowBlockUI();
            //GameManager.OnNetworkStatusChanged += OnNetworkStatusChangedHandler;
            //GameManager.OnHandStatusChanged += OnHandStatusChangedHandler;
            // GameManager.OnCheckIn += OnCheckInHandler ;d
        }

        private void OnHandStatusChangedHandler(HandStatusArgs args,bool calledByCheckIn)
        {
            Debug.Log( "Hand Status Changed : "+ Enum.GetName(typeof(HandState),args.HandStatus));
        }

        private void OnNetworkStatusChangedHandler(NetworkStatus status)
        {
            if (NetworkStatus.Connected == status)
            {
                blockUIText.text = "Connected!, Tring Check-In...";

            }
            else if (NetworkStatus.Disconnected == status)
            {
                
            }
            else if (NetworkStatus.Initialized == status)
            {
                Debug.Log("hide block");
                blockUIText.text = "CheckIn Success!";
                HideBlockUI();
                 
            }
        }

        /// <summary>
        /// 게임 도중에 dispose 될 객체가 아니면 이벤트 해제를 해줄 필요는 없지만..
        /// 해주는 습관을 들이는게 좋다.
        /// </summary>
        private void OnDisable() {
            
            //GameManager.OnNetworkStatusChanged -= OnNetworkStatusChangedHandler;
           
            
        }

        void Start()
        {
            
           
            
        }


        /// <summary>
        /// Block UI 로 화면을 가린다.
        /// </summary>
        public void ShowBlockUI()
        {
            blockUICanvasGroup.alpha = 1.0f;
            BlockUI.enabled= true;
            
        }

        public void HideBlockUI()
        {
            StartCoroutine(FadeOutBlockUI());
        }
      

        private IEnumerator FadeOutBlockUI()
        {
            if (BlockUI.enabled == false || blockUICanvasGroup.alpha != 1.0f) 
            {
                Debug.LogError( "FadeBlockUI called but BlockUI is disabled or Alpha is not 1");
                yield return null;
            }
        

            var fadeSpeed = 0.5f;

            while(blockUICanvasGroup.alpha > 0.0f)
            {
               
               blockUICanvasGroup.alpha -= Time.deltaTime * fadeSpeed;
                yield return null;

            }

            blockUICanvasGroup.alpha =0;
            BlockUI.enabled = false;



        }
    }
}
