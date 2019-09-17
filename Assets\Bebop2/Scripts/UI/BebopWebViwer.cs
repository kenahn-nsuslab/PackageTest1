using UnityEngine;
using UnityEngine.UI;

namespace Bebop.UI
{
    public class BebopWebViwer : MonoBehaviour
    {
        public static BebopWebViwer Instance { get; private set; }

        public GameObject PopupRoot;
        public Button BtnClose;
        public RectTransform WebViewRect;

        private UniWebView _webview;
        private bool _isShow = false;

        private void Awake()
        {
            Instance = this;
            transform.localPosition = Vector3.zero;
            gameObject.SetActive(false);

            _webview = WebViewRect.GetComponent<UniWebView>();

            if (_webview == null)
                _webview = WebViewRect.gameObject.AddComponent<UniWebView>();


            BtnClose.onClick.AddListener(OnClickClose);
        }

        public void OpenWebview(string url)
        {
            if (_isShow == true)
                CloseWebview();

            transform.SetAsLastSibling();
            gameObject.SetActive(true);
            PopupRoot.SetActivePopup(true, () =>
            {
                _webview.ReferenceRectTransform = WebViewRect;

                _isShow = _webview.Show();
                _webview.Load(url);
            });
        }

        public void OnClickClose()
        {
            _webview.Hide();

            PopupRoot.SetActivePopup(false, () =>
            {
                CloseWebview(true);
                gameObject.SetActive(false);
            });
        }

        private void CloseWebview(bool isHide = false)
        {
            if (_webview == null)
                return;

            if (isHide == false)
                _webview.Hide();

            _isShow = false;
        }
    }
}