using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using DG.Tweening;

using TMPro;
using System.Text;

using Common.Scripts.Localization;

namespace Bebop
{
    public class GameHelp : MonoBehaviour
    {
        public static GameHelp Instance { get; private set; }

        public GameObject root;
        public TextMeshProUGUI textBasic;
        public TextMeshProUGUI textlimit;
        public Button btnClose;

        private void Awake()
        {
            Instance = this;

            root.SetActive(false);

            transform.localPosition = Vector3.zero;

            btnClose.onClick.AddListener(OnClose);
        }

        private void Start()
        {
            Init();
        }

        //private void Update()
        //{
        //    if(Input.GetKeyUp(KeyCode.P))
        //    {
        //        this.Open();
        //    }
        //}

        public void Open()
        {
            Init();
            root.SetActive(true);
        }

        private void OnClose()
        {
            root.SetActive(false);
        }

        private void Init()
        {
            StringBuilder sb = new StringBuilder();

            for( int index = 1; index <= 5; ++index)
            {
                var text = LocalizationManager.Instance.GetText($"rule_basic{index}");

                if (string.IsNullOrEmpty(text) == true)
                    continue;

                sb.Append(text);
                sb.Append("\n");
            }

            this.textBasic.text = sb.ToString();

            sb = new StringBuilder();
            string strlimit = LocalizationManager.Instance.GetText("rule_limit10");
            sb.Append(strlimit);
            sb.Append("\n");
            strlimit = LocalizationManager.Instance.GetText("rule_limit1099");
            sb.Append(strlimit);
            sb.Append("\n");
            strlimit = LocalizationManager.Instance.GetText("rule_limit100");
            sb.Append(strlimit);

            textlimit.text = sb.ToString();
        }
    }
}