using Common.Scripts.Managers;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

namespace Bebop.Scripts.UI
{
    public class MyInfo : MonoBehaviour
    {
        public static MyInfo Instance { get; private set; }

        public Text labelBalance;
        public Text labelNickname;
        public TextMeshProUGUI labelCoin;

        public Image userAvatar;
        public Button btnCasher;

        public Image imgNational;

        private Vector3 startTextCoinPos = Vector3.zero;

        private void Awake()
        {
            Instance = this;
            labelNickname.text = "";
            labelBalance.text = "";
            labelCoin.text = "";

            startTextCoinPos = labelCoin.transform.localPosition;

            btnCasher.onClick.AddListener(OnClickCasher);

            //-> 190703 ±¹±â ¼û±è
            if(imgNational != null)
                imgNational.gameObject.SetActive(false);
        }

        private void Start()
        {
            GameManager.OnWalletResponse += UpdateWallet;
        }

        private void OnDestroy()
        {
            GameManager.OnWalletResponse -= UpdateWallet;
        }

        public void SetNational(string code)
        {
            //imgNational.gameObject.SetActive(true);

            //imgNational.sprite =
            //    Common.Scripts.Utils.ResourceManager.Instance.GetSpriteInAtlas(Common.Scripts.Define.CommonResourcePath.AtlasFlagPath, $"Table_flag_{code}");

            //if (imgNational.sprite == null)
            //    imgNational.gameObject.SetActive(false);
        }

        private void OnClickCasher()
        {
            var executor = Common.Scripts.Managers.CommonManager.Instance.GetExecutor(E_ExecuteType.OpenURL);
            executor.Execute(Common.Scripts.Define.E_LinkType.CASHIER);
        }

        private void UpdateWallet(Protocol.WalletResponse res)
        {
            if (res.Result == Protocol.ResultCode.Success)
            {
                if (res.MainWallet == null)
                    return;

                UserData.Instance.Wallet = res.MainWallet;
                UpdateWallet(res.MainWallet);
            }
        }

        public void UpdateWallet(Protocol.Wallet wallet)
        {
            double coinValue = 0;
            if (wallet.CoinValue > 0)
            {
                coinValue = (double)wallet.CoinValue / 100f;
            }
            
            labelBalance.text = coinValue.ToString("#,0.00");
        }

        private Sequence itemMoveSequence;
        
        public void PlayAmount(double amount)
        {
             if (amount <= 0)
                return;

            if (itemMoveSequence != null && itemMoveSequence.IsPlaying())
                itemMoveSequence.Complete(true);

            labelCoin.gameObject.SetActive(true);

            labelCoin.text = $"+{amount.ToString("#,0.##")}";

            labelCoin.transform.localPosition = startTextCoinPos;
            var rt = GetComponent<RectTransform>();

            labelCoin.transform.localScale = new Vector3(1.0f, 0.0f, 1.0f);
            var rtCoint =  labelCoin.GetComponent<RectTransform>();

            itemMoveSequence = DOTween.Sequence();
            itemMoveSequence.timeScale = 0.9f;
            itemMoveSequence.Append(labelCoin.transform.DOLocalMoveY(rt.sizeDelta.y * 0.5f, 2.5f, true));
            itemMoveSequence.Join(labelCoin.transform.DOScaleY(1, 0.2f));
            itemMoveSequence.Join(DOTween.To(() => labelCoin.alpha, x => labelCoin.alpha = x, 0, 2.5f)).OnComplete(ResetCoin);
            itemMoveSequence.Play();
        }

        private void ResetCoin()
        {
            labelCoin.text = "";
            labelCoin.transform.localScale = Vector3.one;
            labelCoin.alpha = 1;
            labelCoin.gameObject.SetActive(false);
        }
    }
}