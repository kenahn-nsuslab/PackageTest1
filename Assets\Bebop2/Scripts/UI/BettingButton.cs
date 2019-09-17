using Common.Scripts.Localization;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Bebop
{
    public class BettingButton : MonoBehaviour
    {
        public enum E_ButtonState
        {
            Repeat,
            Double
        }

        [Serializable]
        public class Toggles
        {
            public E_BettingAmount type;
            public Toggle button;
            public GameObject targetObject;
        }

        public Toggles[] arrToggle;

        [Header("Buttons")]
        public Button btnCancel;
        public Button btnRepeat;
        public Button btnDouble;
        public Button btnUsers;
        public Image imgRepeat;
        public Image imgDouble;

        [Serializable]
        public class Sprites
        {
            public SystemLanguage langeage;
            public Sprite enableSprite;
            public Sprite disableSprite;
        }
        [Header("Sprites")]
        public Sprites[] arrCancelSprite;
        public Sprites[] arrRepeatSprite;
        public Sprites[] arrRepeatArrowSprite;
        public Sprites[] arrDoubleSprite;

        private BettingController controller;

        //private Sprite enableRepeat;
        //private Sprite enableCancel;
        //private Sprite enableRepeatButton;
        //private Sprite enableDoubleButton;

        private void Awake()
        {
            //enableRepeat = imgRepeat.sprite;
            //enableCancel = btnCancel.image.sprite;
            //enableRepeatButton = btnRepeat.image.sprite;
            //enableDoubleButton = btnDouble.image.sprite;

            SetBettingOptionButtonState(E_ButtonState.Repeat, false);
            SetInteractableCancelButton(false);

            btnUsers.onClick.AddListener(OnClickPlayers);

            //-> 임시
            //btnDouble.gameObject.SetActive(false);
            //btnRepeat.gameObject.SetActive(false);
        }

        private void Start()
        {
            LocalizationManager.LanguageChanged += LanguageChanged;
        }

        private void OnDestroy()
        {
            LocalizationManager.LanguageChanged -= LanguageChanged;
        }

        private void LanguageChanged()
        {
            SetInteractableBettingOptionButton(btnDouble.interactable);
            SetInteractableCancelButton(btnCancel.interactable);

            imgRepeat.SetNativeSize();
        }

        private void SetButtonEvent(BettingController controller)
        {
            for (int i = 0, imax = arrToggle.Length; i < imax; ++i)
            {
                var el = arrToggle[i];
                el.targetObject.SetActive(false);

                el.button.onValueChanged.AddListener((isActive) =>
                    {
                        el.targetObject.SetActive(isActive);
                        if (isActive == false)
                            return;

                        controller.OnClickBettingButton(el.type);
                    });
            }

            var group = arrToggle[0].button.group;
            group.SetAllTogglesOff();
            arrToggle[0].button.isOn = true;

            btnCancel.onClick.AddListener(controller.OnClickCancel);
            btnRepeat.onClick.AddListener(controller.OnClickRepeat);
            btnDouble.onClick.AddListener(controller.OnClickDouble);
        }

        public void SetController(BettingController controller)
        {
            this.controller = controller;

            SetButtonEvent(controller);
        }

        public void SetBettingOptionButtonState(E_ButtonState state, bool isEnable)
        {
            //return;

            btnRepeat.gameObject.SetActive(state == E_ButtonState.Repeat);
            btnDouble.gameObject.SetActive(state == E_ButtonState.Double);

            SetInteractableBettingOptionButton(isEnable);
        }

        public void SetInteractableBettingOptionButton(bool isEnable)
        {
            var currentLanguage = LocalizationManager.CurrentLanguage;

            var repeat = Array.Find(arrRepeatSprite, s => s.langeage == currentLanguage);
            if (repeat == null) repeat = arrRepeatSprite[0];
            var repeatArrow = Array.Find(arrRepeatArrowSprite, s => s.langeage == currentLanguage);
            if (repeatArrow == null) repeatArrow = arrRepeatArrowSprite[0];
            
            btnRepeat.interactable = isEnable;
            btnRepeat.image.sprite = isEnable ? repeat.enableSprite : repeat.disableSprite;
            imgRepeat.sprite = isEnable ? repeatArrow.enableSprite : repeatArrow.disableSprite;

            var doubleButton = Array.Find(arrDoubleSprite, s => s.langeage == currentLanguage);
            if (doubleButton == null) doubleButton = arrDoubleSprite[0];

            btnDouble.interactable = isEnable;
            btnDouble.image.sprite = isEnable ? doubleButton.enableSprite : doubleButton.disableSprite;
            imgDouble.SetGrayscale(!isEnable);
        }

        public void SetInteractableCancelButton(bool isEnable)
        {
            var currentLanguage = LocalizationManager.CurrentLanguage;

            var cancel = Array.Find(arrCancelSprite, s => s.langeage == currentLanguage);
            if (cancel == null) cancel = arrCancelSprite[0];

            btnCancel.interactable = isEnable;
            btnCancel.image.sprite = isEnable ? cancel.enableSprite : cancel.disableSprite;
        }

        public void SetEnableSideButtons(bool isEnable)
        {
            btnCancel.enabled = isEnable;
            btnDouble.enabled = isEnable;
            btnRepeat.enabled = isEnable;
        }

        public void SetInteractableBettingButton(bool isEnable)
        {
            Array.ForEach(arrToggle, toggle =>
            {
                toggle.button.interactable = isEnable;
                toggle.button.image.SetGrayscale(!isEnable, true);
                toggle.button.graphic.SetGrayscale(!isEnable, true);
            });
        }

        public void OnClickPlayers()
        {
            UI.Records.Instance.Open();
        }
    } 
}
