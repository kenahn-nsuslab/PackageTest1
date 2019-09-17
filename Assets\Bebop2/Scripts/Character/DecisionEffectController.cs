using System;
using System.Collections.Generic;
using UnityEngine;
using Bebop.UI;
using Bebop.Model.EventParameters;
using System.Collections;
using Common.Scripts.Sound.Managers;

namespace Bebop.Character
{


    public class DecisionEffectController : MonoBehaviour
    {
        public GameObject BoyWinEffect;

        public GameObject BoyLoseEffect;


        public GameObject BullWinEffect;


        public GameObject BullLoseEffect;


        private void Awake() {
            
        }

        private void OnEnable() {

            GameManager.OnHandStatusChanged += OnHandStatusChangedHandler;
            CardController.OnWinnerDecision += OnWinnerDecisionHandler;

        }

        private void OnHandStatusChangedHandler(HandStatusArgs arg, bool fromSnapshot)
        {
            if (arg.HandStatus == Protocol.HandState.HandStart || arg.HandStatus == Protocol.HandState.Idle)
            {
                //리셋
                BoyLoseEffect.SetActive(false);
                BoyWinEffect.SetActive(false);
                BullLoseEffect.SetActive(false);
                BullWinEffect.SetActive(false);
            }
        }

        private void OnWinnerDecisionHandler(WinnerType type)
        {
            if (type == WinnerType.Boy)
            {
                BoyWinEffect.SetActive(true);
                BullLoseEffect.SetActive(true);

                SoundManager.PlaySFX(SFX.Result_Win_Cowboy);
            }
            else if (type == WinnerType.Bull)
            {
                BullWinEffect.SetActive(true);
                BoyLoseEffect.SetActive(true);

                SoundManager.PlaySFX(SFX.Result_Win_Bull);
            }
        }

        private void OnDisable() {
            
            CardController.OnWinnerDecision -= OnWinnerDecisionHandler;
            GameManager.OnHandStatusChanged -= OnHandStatusChangedHandler;

        }



    }
}