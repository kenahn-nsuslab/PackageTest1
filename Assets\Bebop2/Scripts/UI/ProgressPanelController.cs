using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Bebop.Model.EventParameters;
using TMPro;
using Common.Scripts.Sound.Managers;
using UnityEngine.UI;
using Common.Scripts.Localization;
using System.Linq;
using System;

namespace Bebop.UI
{
    public class ProgressPanelController : MonoBehaviour
    {

        public enum PanelType
        {
            //Versus,
            StartBetting,
            StopBetting
        }

        /// <summary>
        /// 게임 진행 판떼기
        /// </summary>
        //public GameObject NotiPanel;

        /// <summary>
        /// 시계
        /// </summary>
        public GameObject Clock;

        /// <summary>
        ///  시계 안에 남은 시간 표시
        /// </summary>
        public TextMeshProUGUI ClockText;

        /// <summary>
        /// 게임 진행 판떼기 안에 텍스트
        /// </summary>
        //public TextMeshProUGUI NotiText;

        public GameObject sfxVersus;

        private Transform ClockImage ;

        [SerializeField]
        public GameObject Title;

        private GameObject VersusImage ;

        //private GameObject BettingStartImage;

        //private GameObject BettingStopImage;

        private Animator BettingAnim ;

        [SerializeField]
        private List<GameObject> ContentImages ;

        private GameObject WatchEffect;



        /// <summary>
        /// 배팅 타이머 정지 후 결과가 나오기 전까지 별도의 애니메이션 처리등을 위해 배팅 정지 이벤트를 발생시킨다.
        /// </summary>
        public static event Action OnBettingStop= delegate {};

        //private RectTransform NotiPanelOriginPosition;

        private void Awake() {
            //NotiPanelOriginPosition =  NotiPanel.GetComponent<RectTransform>();
            ClockImage = Clock.transform.Find("Watch").transform;
            WatchEffect = Clock.transform.Find("watchEffect").gameObject;

            BettingAnim = Title.GetComponent<Animator>();

            //VersusImage = Title.transform.Find("VersusImage").gameObject;
            //BettingStartImage = Title.transform.Find("BettingStartImage").gameObject;
            //BettingStopImage = Title.transform.Find("BettingStopImage").gameObject;

            sfxVersus.SetActive(false);

            Title.SetActive(false);
        }

         private void OnEnable() {
            
            GameManager.OnHandStatusChanged+= OnHandStatusChangedHanlders ;
           
        }

       

        private void OnDisable() {
            GameManager.OnHandStatusChanged-= OnHandStatusChangedHanlders ;
           
        }

        private void OnHandStatusChangedHanlders(HandStatusArgs arg, bool fromSnapshot)
        {

            
           
            //카드 딜링제외 각 상태마다 처리할 작업
            if ( arg.HandStatus == Protocol.HandState.HandStart)
            {
                //ResetHand();
                //TODO: 배팅 준비 표시
                ShowHandStartPanel(fromSnapshot);
            }
            else if (arg.HandStatus == Protocol.HandState.Betting)
            {
                ShowBettingStartPanel(fromSnapshot);

                //캐치업인 경우 남은 시간 만큼, 정상 상태인 경우 period 밀리세컨드를 초로 바꿔 전달한다.
                //int startTime = fromSnapshot ? (int)(arg.StartInAt - arg.Current.Value).TotalSeconds : arg.Period / 1000;

                float durationTime = arg.Period / 1000f;
                if (fromSnapshot == true)
                {
                    durationTime = durationTime - (float)(GameManager.GetCurrentServerTime() - arg.StartInAt).TotalSeconds;
                }

                ShowTimerClock(durationTime);
            }
            else if (arg.HandStatus == Protocol.HandState.CardOpen)
            {
                //ShowCardOpenPanel(fromSnapshot);
                //TODO 카드오픈 처리 ..CardOpen 이벤트는 별로도 처리된다. 여기서는 상태변경에 따른 부가 처리만..

                //배팅 중지 배널을 제거해 준다.
               
               StartCoroutine(HideBettingStopTitle());
            }
            else if (arg.HandStatus == Protocol.HandState.Idle)
            {
                //ResetHand();
            }

       
        }

        private void Start() {
            //ShowHandStartPanel(false);

            //ShowTimerClock(15);
        }

        private void ShowHandStartPanel(bool fromSnapshot)
        {
            if (fromSnapshot==false)
                //StartCoroutine(ShowPanel("Ready",0.5f,1.0f,0.5f));
                StartCoroutine(ShowVersusImage());
        }

        private void ShowBettingStartPanel(bool fromSnapshot)
        {
            if (fromSnapshot==false)
                //StartCoroutine(ShowPanel("Let's Bet",0.5f,1.0f,0.5f));
                StartCoroutine(ShowBettingStartTitle());
        }


        

        // private void ShowCardOpenPanel(bool fromSnapshot)
        // {
        //     //if (fromSnapshot==false)
        //         //StartCoroutine(ShowPanel("Open Cards!",0.5f,1.0f,0.5f));
        // }

        /// <summary>
        /// 초시간 타이머 보이기
        /// </summary>
        /// <param name="startTime">현재 상태의 남은 시간(Period 에서 남은시간)</param>
        private void ShowTimerClock(float durationTime)
        {

            StartCoroutine(StartTimer(durationTime));
        }

        /// <summary>
        /// 버티컬 그라디언트 인 경우 top 좌우 색이 같고 bottom 좌주 색이 같다.
        /// </summary>
        /// <param name="Color(255"></param>
        /// <returns></returns>
        private  readonly VertexGradient DefaultTimerTextColor = 
            new VertexGradient(new Color(1,0.9337f,0.2688f,1),new Color(1,0.9337f,0.2688f,1), 
                                new Color(0.1676f,0.7264f,0.1610f,1),new Color(0.1676f,0.7264f,0.1610f,1));

        private readonly VertexGradient AlertTimerTextColor = 
            new VertexGradient(new Color(1,1,0.5661f,1),
                                new Color(1,1,0.5661f,1),
                                new Color(1,0,0,1),
                                new Color(1,0,0,1)
                                );

        //public Image BettingStopImage1 { get => BettingStopImage2; set => BettingStopImage2 = value; }
        //public Image BettingStopImage2 { get => BettingStopImage; set => BettingStopImage = value; }

        private IEnumerator StartTimer(float durationTime)
        {
            // 19초에서 앞뒤 2초를 마진으로 두고 표시 숫자를 하나씩 적게 보여준다.
            durationTime -= 2f;

            var timerTime = 15.0f; //초기값 , 타이머 시작 시간

            if (durationTime >= timerTime )
            {
                var waitingTime = durationTime - timerTime; //15초 타이머 시간을 빼고 나머지 시간을 웨이팅으로..(캐치업을 위해..), 단 3초를 넘기기 않게 한다.
                yield return new WaitForSecondsRealtime(waitingTime);
            }
            else
            {
                timerTime = durationTime; //타이머 남은 시간.
            }

            
            Clock.SetActive(true);

            while ( timerTime > 0f) 
            {
                int remainingTime = Mathf.CeilToInt(timerTime);
                ClockText.text = remainingTime.ToString("0");

                if (remainingTime <= 5)
                {
                    WatchEffect.SetActive(true);
                    ClockText.colorGradient = AlertTimerTextColor; 
                   
                    SoundManager.PlaySFX(SFX.Timer_Count2);         
                }
                else
                {
                    SoundManager.PlaySFX(SFX.Timer_Count1);
                }

                ClockImage.DOShakeScale(0.3f,0.1f,5,1,true); //시계 흔들기

                yield return new WaitForSecondsRealtime(1.0f); // 유니티 내부 시간을 늘리거나 빠르게 하는 것에 영향을 받지 않음.

                timerTime--;
            }

            SoundManager.PlaySFX(SFX.Timer_Finish);
            ClockImage.DOShakeScale(0.3f, 0.1f, 5, 1, true); //시계 흔들기
            ClockText.text = "0";

            //타이머 정지 후 배팅중지 판넬을 보여준다.
            yield return new WaitForSecondsRealtime(0.5f);

            ClockText.colorGradient = DefaultTimerTextColor;
            Clock.SetActive(false);
            WatchEffect.SetActive(false);

            StartCoroutine(ShowBettingStopTitle());
        }


        /// <summary>
        /// 타이틀 배널에 표시될 객체를 활성화 시킨다.
        /// </summary>
        /// <param name="type"></param>
        private void SelectTitle(PanelType type)
        {

            string postfix = LocalizationManager.CurrentLanguage == SystemLanguage.ChineseSimplified  ? "CN" : "EN";

            var contentName = string.Format("{0}_{1}",Enum.GetName(typeof(PanelType), type), postfix);

            Debug.Log("conents name:"+contentName);

            foreach( var image in ContentImages )
            {
                if (string.Compare(contentName,image.name,true)==0) image.SetActive(true);
                else image.SetActive(false);
            }
        }


        /// <summary>
        /// Versus 시작 이미지 보여주기
        /// </summary>
        /// <returns></returns>
        private IEnumerator ShowVersusImage()
        {
            //패널 이미지 선택
            // SelectTitle(PanelType.Versus);

            // var orgin = new Vector3(-14.0f,0,0);

            // var target = new Vector3(14.0f,0,0);

            //SoundManager.PlaySFX(SFX.Hand_Start);
            // yield return Title.transform.DOMove(Vector3.zero ,0.5f).SetEase(Ease.OutBounce).WaitForCompletion();

            // yield return new WaitForSeconds(1.0f);

            // yield return Title.transform.DOMove(target ,0.5f).SetEase(Ease.InBack).WaitForCompletion();

            // Title.transform.position = orgin;

            SoundManager.PlaySFX(SFX.Hand_Start);
            sfxVersus.SetActive(true);

            yield return new WaitForSecondsRealtime(2f);

            sfxVersus.SetActive(false);

        }

        /// <summary>
        /// 배팅 시작 이미지 보여주기
        /// </summary>
        /// <returns></returns>
        private IEnumerator ShowBettingStartTitle()
        {
            //패널 이미지 선택
            SelectTitle(PanelType.StartBetting);
            
            Title.SetActive(true);
            
            SoundManager.PlaySFX(SFX.Hand_BettingStart);
            yield return new WaitForSecondsRealtime(1.5f);
            BettingAnim.SetTrigger("GoHide");
            yield return new WaitForSecondsRealtime(0.5f);
            Title.SetActive(false);
            //BettingAnim.ResetTrigger("GoHide");

        //     var orgin = new Vector3(-1250.0f,0,0);

        //     var target = new Vector3(1250.0f,0,0);
           
        //    SoundManager.PlaySFX(SFX.Hand_BettingStart);
        //     yield return Title.transform.DOLocalMove(Vector3.zero ,0.5f).SetEase(Ease.OutBounce).WaitForCompletion();

        //     yield return new WaitForSeconds(1.0f);

        //     yield return Title.transform.DOLocalMove(target ,0.5f).SetEase(Ease.InBack).WaitForCompletion();

        //    Title.transform.localPosition = orgin;
        }

        /// <summary>
        /// 배팅 중지 이미지 보여주기
        /// </summary>
        /// <returns></returns>
        private IEnumerator ShowBettingStopTitle()
        {
            BettingController.StopBetting();

            //패널 이미지 선택
            SelectTitle(PanelType.StopBetting);
            Title.SetActive(true);
            SoundManager.PlaySFX(SFX.Hand_BettingStart);

            OnBettingStop.Invoke(); //배팅 정지 이벤트 발생

            yield return null;


            //Title.SetActive(false);

            //var target = new Vector3(14.0f,0,0);

            //SoundManager.PlaySFX(SFX.Hand_BettingStart);
            //yield return Title.transform.DOLocalMove(Vector3.zero ,0.5f).SetEase(Ease.OutBounce).WaitForCompletion();

            
        }


        private IEnumerator HideBettingStopTitle()
        {

            BettingAnim.SetTrigger("GoHide");
            yield return new WaitForSecondsRealtime(0.5f);
            Title.SetActive(false);
            //BettingAnim.ResetTrigger("GoHide");
            // var orgin = new Vector3(-1250.0f,0,0);

            // var target = new Vector3(1250.0f,0,0);

            // //if (Title.transform.localPosition ==  Vector3.zero)
            // {
            //     yield return Title.transform.DOLocalMove(target ,0.5f).SetEase(Ease.InBack).WaitForCompletion();
            //     Title.transform.localPosition = orgin;
            // }
            //else
            //{
            //    yield return null;
            //}
        }






        

        
    }
}