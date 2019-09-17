using Common.Scripts.Localization;
using Common.Scripts.Sound.Managers;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bebop
{
    public class BebopMessagebox : MonoBehaviour
    {
        public static BebopMessagebox Instance { get; private set; }

        public Button btnBack;
        public Button btnOk;
        public Button btnCancel;

        public TextMeshProUGUI txtTitle;
        public TextMeshProUGUI txtDesc;
        public TextMeshProUGUI txtOk;

        private System.Action callback;

        private bool isError;

        private void Awake()
        {
            Instance = this;

            btnBack.onClick.AddListener(OnClickBack);
            btnOk.onClick.AddListener(OnClickOk);
            btnCancel.onClick.AddListener(OnClickCancel);

            btnBack.gameObject.SetActive(false);

            transform.localPosition = Vector3.zero;

            isError = false;
        }

        private void Start()
        {
            gameObject.SetActive(false);
        }

        private void OnClickBack()
        {
            SoundManager.PlaySFX(SFX.ClickButton);

            gameObject.SetActive(false);
        }

        private void OnClickOk()
        {
            SoundManager.PlaySFX(SFX.ClickButton);

            gameObject.SetActive(false);

            if (callback != null)
                callback.Invoke();

            isError = false;
        }

        private void OnClickCancel()
        {
            SoundManager.PlaySFX(SFX.ClickButton);

            gameObject.SetActive(false);
        }

        private void SetButtonState(bool isOkCancel)
        {
            Instance.btnOk.gameObject.SetActive(true);
            Instance.btnCancel.gameObject.SetActive(isOkCancel);
        }

        private void SetTitle(string str)
        {
            //if (string.IsNullOrEmpty(str) == true)
            //{
            //    txtTitle.gameObject.SetActive(false);
            //}
            //else
            //{
            //    txtTitle.gameObject.SetActive(true);
            //    txtTitle.text = str;
            //}

            txtTitle.text = str;
        }

        private void SetDesc(string str)
        {
            //bool isShowTitle = txtTitle.gameObject.activeSelf;
            //txtDesc.transform.localPosition = new Vector3(0f, isShowTitle ? 4f : 40f, 0f);

            txtDesc.text = str;
        }

        private void SetOkButtonText(string str)
        {
            txtOk.text = str;
        }

        private void SetCallback(System.Action callback)
        {
            this.callback = callback;
        }

        public static void Ok(string title, string desc, System.Action callback = null)
        {
            if (Instance.isError == true)
                return;

            title = "";
            Ok(title, desc, LocalizationManager.Instance.GetText("ButtonOK"), callback);
        }

        private static void Ok(string title, string desc, string okButtonText, System.Action callback = null)
        {
            Instance.gameObject.SetActive(true);
            Instance.SetButtonState(false);
            Instance.SetTitle(title);
            Instance.SetDesc(desc);
            Instance.SetOkButtonText(okButtonText);
            Instance.SetCallback(callback);
        }

        public static void OkCancel(string title, string desc, System.Action callback = null)
        {
            if (Instance.isError == true)
                return;

            OkCancel(title, desc, LocalizationManager.Instance.GetText("ButtonOK"), callback);
        }

        private static void OkCancel(string title, string desc, string okButtonText, System.Action callback = null)
        {
            Instance.gameObject.SetActive(true);
            Instance.SetButtonState(true);
            Instance.SetTitle(title);
            Instance.SetDesc(desc);
            Instance.SetOkButtonText(okButtonText);
            Instance.SetCallback(callback);
        }

        public static void Error(string title, string desc, System.Action callback = null, float duration = 0f)
        {
            if (Instance.isError == true)
                return;

            Instance.isError = true;

            title = "";
            Ok(title, desc, LocalizationManager.Instance.GetText("ButtonOK"), callback);

            if (duration > 0f)
                Instance.Invoke("OnClickOk", duration);
        }
    } 
}
