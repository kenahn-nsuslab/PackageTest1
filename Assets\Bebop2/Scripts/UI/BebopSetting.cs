using Bebop.UI;
using Common.Scripts.Define;
using Common.Scripts.Localization;
using Common.Scripts.Sound.Managers;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace Bebop
{
    public class BebopSetting : MonoBehaviour
    {
        public static BebopSetting Instance { get; private set; }

        [Flags]
        public enum E_Setting
        {
            None = 0,
            Bgm = 1 << 0,
            Effect = 1 << 1,
            Vibration = 1 << 2,
            Max = int.MaxValue
        }

        [Header("Buttons")]
        public Button btnCancel;
        public Button btnLogout;
        public Button btnExit;

        [Serializable]
        public class CustomToggle
        {
            public E_Setting type;
            public Toggle toggle;
            public Image imgOn;
            public Image imgOff;
            public TextMeshProUGUI txtOn;
            public TextMeshProUGUI txtOff;
        }

        [Header("BGM & Effect")]
        public CustomToggle tglBgm;
        public CustomToggle tglEffect;

        //[Header("BGM & Effect Text Material")]
        //public Material matTextOn;
        //public Material matTextOff;

        private int matTextOnIndex = 13;
        private int matTextOffIndex = 9;

        [Header("Language")]
        public CustomToggle tglChinese;
        public CustomToggle tglEnglish;

        //[Header("Language Text Material")]
        //public Material matLanguageOn;
        //public Material matLanguageOff;

        private int matLanguageOnIndex = 8;
        private int matLanguageOffIndex = 0;

        private E_Setting currentSetting;
        private SystemLanguage currentLanguage = SystemLanguage.ChineseSimplified;

        private void Awake()
        {
            Instance = this;

            btnCancel.onClick.AddListener(OnClickCancel);
            btnLogout.onClick.AddListener(OnClickLogout);
            btnExit.onClick.AddListener(OnClickExit);

            transform.localPosition = Vector3.zero;
        }

        private void Start()
        {
            tglBgm.toggle.onValueChanged.AddListener(isOn => OnClickCustomToggle(isOn, tglBgm));
            tglEffect.toggle.onValueChanged.AddListener(isOn => OnClickCustomToggle(isOn, tglEffect));

            tglChinese.toggle.onValueChanged.AddListener(isOn => OnClickLanguageToggle(isOn, tglChinese));
            tglEnglish.toggle.onValueChanged.AddListener(isOn => OnClickLanguageToggle(isOn, tglEnglish));

            Load();

            gameObject.SetActive(false);
        }

        private void Load()
        {
            currentSetting = (E_Setting)PlayerPrefs.GetInt("OasisSetting", (int)E_Setting.Max);
            bool isBgm = (currentSetting & E_Setting.Bgm) > 0;
            bool isEffect = (currentSetting & E_Setting.Effect) > 0;

            SoundManager.MuteMusic(!isBgm);
            SoundManager.MuteSFX(!isEffect);

            int selectLang = PlayerPrefs.GetInt("OasisSelectLanguage", -1);

            if (selectLang < 0)
            {
                var lang = Application.systemLanguage;
                if (lang == SystemLanguage.ChineseSimplified)
                {
                    currentLanguage = SystemLanguage.ChineseSimplified;
                }
                else if (lang == SystemLanguage.ChineseTraditional)
                {
                    currentLanguage = SystemLanguage.ChineseSimplified;
                }
                else if (lang == SystemLanguage.Chinese)
                {
                    currentLanguage = SystemLanguage.ChineseSimplified;
                }
                else
                {
                    currentLanguage = SystemLanguage.English;
                }
            }
            else
                currentLanguage = (SystemLanguage)selectLang;

            //LocalizationManager.Instance.SetLanguage(currentLanguage);
            tglBgm.toggle.isOn = isBgm;

            tglEffect.toggle.isOn = isEffect;

            if (currentLanguage == SystemLanguage.ChineseSimplified)
                tglChinese.toggle.isOn = true;
            else
                tglEnglish.toggle.isOn = true;
        }

        private void Save()
        {
            PlayerPrefs.SetInt("OasisSetting", (int)currentSetting);
            PlayerPrefs.SetInt("OasisSelectLanguage", (int)currentLanguage);
        }

        private void OnClickCancel()
        {
            SoundManager.PlaySFX(SFX.ClickButton);
            SetActive(false);
        }

        private void OnClickLogout()
        {
            SoundManager.PlaySFX(SFX.ClickButton);

            var exe = Common.Scripts.Managers.CommonManager.Instance.GetExecutor(Common.Scripts.Managers.E_ExecuteType.OpenURL);
            exe.Execute(E_LinkType.CHANGE_PASSWORD);
        }

        private void OnClickExit()
        {
            BebopMessagebox.OkCancel("",LocalizationManager.Instance.GetText("text_ApplicationQuit"), Application.Quit);
        }

        private void OnClickCustomToggle(bool isOn, CustomToggle toggleInfo)
        {
            if (isOn) SoundManager.PlaySFX(SFX.ClickButton);

            toggleInfo.imgOn.gameObject.SetActive(isOn);
            toggleInfo.imgOff.gameObject.SetActive(!isOn);

            UpdateEffectAndBGMFontMaterial(isOn, toggleInfo);

            SetState(toggleInfo.type, isOn);
        }

        private void UpdateEffectAndBGMFontMaterial(bool isOn, CustomToggle toggleInfo)
        {
            var fontData = LocalizationManager.Instance.GetFontDataFromLanguage(LocalizationManager.CurrentLanguage);

            toggleInfo.txtOn.fontSharedMaterial = isOn ? fontData.FontMaterial[matTextOnIndex] : fontData.FontMaterial[matTextOffIndex];
            toggleInfo.txtOff.fontSharedMaterial = isOn ? fontData.FontMaterial[matTextOffIndex] : fontData.FontMaterial[matTextOnIndex];
        }

        private void UpdateLanguageFontMaterial(bool isOn, CustomToggle toggleInfo)
        {   
            var lang = toggleInfo.toggle.name.ToLower().Equals(("English").ToLower()) ? SystemLanguage.English : SystemLanguage.ChineseSimplified;

            var fontData = LocalizationManager.Instance.GetFontDataFromLanguage(lang);

            toggleInfo.txtOn.fontSharedMaterial = isOn ? fontData.FontMaterial[matLanguageOnIndex] : fontData.FontMaterial[matLanguageOffIndex];
            toggleInfo.txtOn.color = isOn ? new Color(171f / 255f, 68f / 255f, 40f / 255f) : new Color(144f / 255f, 144f / 255f, 144f / 255f);
        }

        private void OnClickLanguageToggle(bool isOn, CustomToggle toggleInfo)
        {
            if (isOn) SoundManager.PlaySFX(SFX.ClickButton);

            toggleInfo.imgOn.gameObject.SetActive(isOn);
            toggleInfo.imgOff.gameObject.SetActive(!isOn);

            UpdateLanguageFontMaterial(isOn, toggleInfo);

            //toggleInfo.txtOn.fontSharedMaterial = isOn ? matLanguageOn : matLanguageOff;

            if (isOn)
                SetLanguage(toggleInfo == tglChinese ? SystemLanguage.ChineseSimplified : SystemLanguage.English);
        }

        private void SetState(E_Setting eSetting, bool isOn)
        {
            if (isOn == true)
            {
                currentSetting |= eSetting;
            }
            else
            {
                currentSetting &= ~eSetting;
            }

            if (eSetting == E_Setting.Bgm)
            {
                SoundManager.MuteMusic(!isOn);
            }

            if (eSetting == E_Setting.Effect)
            {
                SoundManager.MuteSFX(!isOn);
            }

            Save();
        }

        private void SetLanguage(SystemLanguage lang)
        {
            currentLanguage = lang;
            LocalizationManager.Instance.SetLanguage(currentLanguage);
            Save();
        }

        public void SetActive(bool isActive)
        {
            gameObject.SetActive(isActive);

            bool isBgm = (currentSetting & E_Setting.Bgm) > 0;
            bool isEffect = (currentSetting & E_Setting.Effect) > 0;

            UpdateEffectAndBGMFontMaterial(isBgm, tglBgm);
            UpdateEffectAndBGMFontMaterial(isEffect, tglEffect);

        }
    } 
}
