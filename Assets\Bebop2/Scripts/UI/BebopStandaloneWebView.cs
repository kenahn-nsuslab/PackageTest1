#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
using UnityEngine;
using UnityEngine.UI;

using ZenFulcrum.EmbeddedBrowser;

namespace Bebop.UI
{
    public class BebopStandaloneWebView : MonoBehaviour
    {
        public static BebopStandaloneWebView Instance { get; private set; }

        public Browser browser;
        public Button btnClose;

        public void Awake()
        {
            Instance = this;

            gameObject.SetActive(false);
            transform.localPosition = Vector3.zero;
            btnClose.onClick.AddListener(OnClickClose);
            transform.SetAsLastSibling();
        }

        public void OpenWebView(string url)
        {
            WaitIndicator.SetActive(true);
            gameObject.SetActive(true);
            browser.LoadURL(url, true);
            browser.onLoad += OnLoad;
        }

        private void OnLoad(JSONNode node)
        {
            WaitIndicator.SetActive(false);
            browser.onLoad -= OnLoad;
        }

        private void OnClickClose()
        {
            gameObject.SetActive(false);
        }
    }
}
#endif