using System;
using UnityEngine;
using Bebop.UI;
using Common.Scripts.Sound.Managers;

namespace Bebop.Character
{

    public class Boy : CharacterBase
    {

        private const WinnerType MyType= WinnerType.Boy;

        public Boy() : base(MyType)
        {

        }

        
         protected override void Awake() {
             base.Awake();
        }

        protected override void OnEnable() {
            base.OnEnable();
        }

        protected override void OnDisable() {

            base.OnDisable();
            
        }

        protected override void PlayWinSound()
        {
            SoundManager.PlaySFX(SFX.Result_Win_Cowboy);
        }
    }

   
}