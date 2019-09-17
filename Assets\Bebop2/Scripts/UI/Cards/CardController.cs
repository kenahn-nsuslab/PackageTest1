using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Bebop.Model.EventParameters;
using System;
using UnityEngine.UI;
using Bebop.Protocol;
using System.Linq;
using TMPro;
using Bebop.Character;
using Common.Scripts.Sound.Managers;

namespace  Bebop.UI
{

    public enum HoldemCards
    {
        BoyHoleCard1=0,
        BoyHoleCard2,
        CommunityCard1,
        CommunityCard2,
        CommunityCard3,
        CommunityCard4,
        CommunityCard5,
        BullHoleCard1,
        BullHoleCard2

    }

   
    /// <summary>
    /// Cards 빈객체에 연결해 카드의 전반적인 컨트롤을 담당한다.
    /// </summary>
    public partial class CardController : MonoBehaviour 
    {

        

        /// <summary>
        /// Placehodler0~8 까지 연결
        /// 카드가 딜링될 위치를 표시
        /// </summary>
        public List<Transform> PlaceHolders ;


        /// <summary>
        /// Card0~8까지 연결
        /// 카드이미지 오브젝트 리스트
        /// 애니메이셔을 위해 리스트에 담아둔다.
        /// </summary>
        public List<GameObject> Cards ;


        /// <summary>
        /// 카드 이미지 컴포넌트 리스트
        /// 카드 이미지 교체를 위해 미리 리스트에 담아둔다.
        /// </summary>
        [SerializeField]
        private List<Image> CardImages;

        /// <summary>
        /// 카우보이 쪽 핸드 족보 스트링
        /// </summary>
        public TextMeshProUGUI CowboyRankText;

        /// <summary>
        /// 소 쪽 핸드 족보 스트링
        /// </summary>
        public TextMeshProUGUI BullRankText;


        //private Sprite CardBack
        /// <summary>
        /// 홀카드 인덱스
        /// 커뮤니티 카드와 분리해서 처리 해야 해서...나눔
        /// </summary>
        /// <value></value>
        private HoldemCards [] holecardsIndex = new HoldemCards [] { HoldemCards.BoyHoleCard1, 
                                                                    HoldemCards.BoyHoleCard2,
                                                                    HoldemCards.BullHoleCard1,
                                                                    HoldemCards.BullHoleCard2 };
        /// <summary>
        /// 커뮤니티 카드 인덱스
        /// </summary>
        /// <value></value>
        private HoldemCards [] commcardIndex = new HoldemCards [] {
                                                            HoldemCards.CommunityCard1,
                                                            HoldemCards.CommunityCard2,
                                                            HoldemCards.CommunityCard3,
                                                            HoldemCards.CommunityCard4,
                                                            HoldemCards.CommunityCard5
                                                    };

        private bool DealingFinished = false; //딜링 애니메이션이 끝났는지 여부

        
        /// <summary>
        /// 카드 스프라이트 캐시 객체
        /// cardSprites["7C"] 처럼 서버가 보내준 카드 스트링을 키로 조회하면 해당 카드의 스프라이트를 리턴해준다.
        /// </summary>
        private CardSprites cardSprites;

        /// <summary>
        /// 베스트 5 카드가 아닌 카드에 적용할 컬러
        /// </summary>
        /// <returns></returns>
        private Color NoBestCardColor = new Color(150,150,150,255);


        /// <summary>
        /// 캐릭터 애니메이션 발생 이벤트
        /// </summary>
        public static event Action<Bebop.Character.WinnerType> OnWinnerDecision = delegate {};


         private void Awake() {
                
            // DOTween.Clear();
            // DOTween.SetTweensCapacity(200, 200);
            cardSprites = new CardSprites();
            cardSprites.LoadSprites();

            //이미지 컴포넌트 캐싱
            CardImages = new List<Image>();

            foreach(var obj in Cards)
            {
                CardImages.Add(obj.GetComponent<Image>());
            }
            
        }

        private void OnEnable() {
            
            GameManager.OnHandStatusChanged+= OnHandStatusChangedHanlders ;
            GameManager.OnCardOpen += OnCardOopenHandeler ;
        }

       

        private void OnDisable() {
            GameManager.OnHandStatusChanged-= OnHandStatusChangedHanlders ;
            GameManager.OnCardOpen -= OnCardOopenHandeler;
        }

        /// <summary>
        /// 핸드의 상태 변경 옵저버
        /// </summary>
        /// <param name="arg">상태정보</param>
        /// <param name="fromShapshsot">첫 접속후 캐치업에서 호출된 것인지 여부</param>
        private void OnHandStatusChangedHanlders(HandStatusArgs arg, bool fromSnapshot)
        {

            
           
            //카드 딜링제외 각 상태마다 처리할 작업
            if ( arg.HandStatus == Protocol.HandState.HandStart)
            {
                ResetHand();
                //TODO: 배팅 준비 표시  => ProgressPanelController.cs 에서 처리 하는 것으로..
            }
            else if (arg.HandStatus == Protocol.HandState.Betting)
            {
             
                //TODO: 배팅 타이머 처리
            }
            else if (arg.HandStatus == Protocol.HandState.CardOpen)
            {
                //TODO 카드오픈 처리 ..CardOpen 이벤트는 별로도 처리된다. 여기서는 상태변경에 따른 부가 처리만..
                //타이머 사라지기..결과 알림.
            }
            else if (arg.HandStatus == Protocol.HandState.Idle)
            {
                ResetHand();
            }

            //캐치업 및 핸드스타트 공통부분
            // 핸드스타트 이후 스냅샷에 대해 딜링이 안된경우 딜링을 먼저 수행하기 위한용도와
            // 정상적인 상태에서 핸드스타트시 딜링 모두 여기서 처리됨.
            if (arg.HandStatus.CompareTo(Protocol.HandState.HandStart)> -1)
            {
                if (DealingFinished== false)
                {
                    
                    StartCoroutine(DoDealCards(fromSnapshot));
                }
                
            }

        

        }

        private void OnCardOopenHandeler(NotifyHandStateCardOpenIn dto, bool fromSnapshot)
        {
           
            StartCoroutine(OpenCard(dto, fromSnapshot));
        }

        private void Start() {
            
            //ThrowCard(Cards[0].transform, GetPlaceHolder(HoldemCards.BoyHoleCard1));
           // StartCoroutine(DealCards());

           //Test = cardSprites["Back"];
        }

        private Transform GetPlaceHolder(HoldemCards index)
        {
            return PlaceHolders[(int)index];
        }

        /// <summary>
        /// 카드 생성시 로테이션을 180도 돌리고 target 이 되는 플레이스 홀더는 0 도로 두어 180도 회전하도록 한다.
        /// </summary>
        /// <param name="card"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        private void ThrowCard(Transform card, Transform target,float rotTime, float movTime)
        {
           var rot = target.rotation;
            card.DORotateQuaternion(rot,rotTime);
            card.DOMove(target.position, movTime).SetEase(Ease.OutCirc);
            SoundManager.PlaySFX(SFX.Card_Deal);
        }


        /// <summary>
        /// 카드를 초기상태로(딜링가능한 상태) 돌린다.
        /// TODO: 카드오픈 애니메이션으로 변경된 프로퍼티도 초기화 시켜야 함.
        /// </summary>
        /// <param name="card"></param>
        private void ResetCards()
        {
            //for(int i=8 ; i > -1 ; i--)
            for ( int i =0; i < 9 ; i++)
            {
                var card = Cards[i];
                card.GetComponent<ParticleSystem>().Stop();
                card.SetActive(false);
                card.transform.localPosition = new Vector3(0,80,0);
                card.transform.localRotation = Quaternion.Euler(0,0,180f);
                var cardImage = CardImages[i];
                cardImage.sprite = cardSprites["Back"];
                cardImage.color = Color.white;

                //card.GetComponent<Outline>().enabled =false;
                card.GetComponent<Canvas>().sortingOrder= 11;
                
                card.transform.SetAsLastSibling();
               
                //Debug.Log(i.ToString()+"번째 :"+card.transform.GetSiblingIndex().ToString());
            
            }
          
        }


        private void ResetHand()
        {
            CowboyRankText.transform.parent.gameObject.SetActive(false);
            BullRankText.transform.parent.gameObject.SetActive(false);

            CowboyRankText.text="";
            BullRankText.text="";
           
            ResetCards();

           
            DealingFinished = false;
        }

       
       /// <summary>
       /// 딜링 애니메이션 수행
       /// </summary>
       /// <param name="fromSnapshot"></param>
       /// <returns></returns>
        private IEnumerator DoDealCards(bool fromSnapshot )
        {
            if (DealingFinished) yield return null;
            else 
            {
                //시작하고 1.5 초 기다린 후 딜링.
                yield return fromSnapshot? null: new WaitForSecondsRealtime(2.5f);
            }

            //스냅샷 적용시 애니 시간을 줄인다.
            float rotTime = fromSnapshot ? 0.1f: 0.4f;
            float movTime = fromSnapshot ? 0.1f : 0.3f;

            
            //foreach(HoldemCards i in holecardsIndex.Reverse())
            // foreach(HoldemCards i in holecardsIndex) //홀카드 왼쪽이 아래로가게..
            // {
            //      var card = Cards[(int)i];
            //     card.SetActive(true);
            // }

            //
            foreach ( HoldemCards index in holecardsIndex)
            {
                var card = Cards[(int)index];
                card.SetActive(true);
                ThrowCard(card.transform, GetPlaceHolder( index),rotTime,movTime);
                yield return fromSnapshot?null : new WaitForSeconds(0.05f);
            }
           
            foreach( HoldemCards index in commcardIndex)
            {
                yield return fromSnapshot? null : new WaitForSeconds(0.1f);

                var card = Cards[(int) index];
                card.SetActive(true);

                ThrowCard(card.transform, GetPlaceHolder(index),rotTime,movTime);
                
            }

            yield return fromSnapshot? new WaitForSeconds(0.1f):null; //캐치업 시 딜 애니메이션 대기

            
            DealingFinished = true;
            
        }

        /// <summary>
        /// 카드 오픈 애니메이션 수행
        /// </summary>
        /// <param name="fromSnapshot"></param>
        /// <returns></returns>
        private IEnumerator OpenCard(NotifyHandStateCardOpenIn dto, bool fromSnapshot )
        {
        
            yield return new WaitWhile(()=> DealingFinished == false); //딜링이 끝나길 기다린다. 스냅샷인 경우에도 DealCards 코루틴은 호출되어 실행된다.

            yield return fromSnapshot? null : new WaitForSeconds(0.5f); // progress panel 애니메이션을 기다려 준다.
 

            var cards = dto.DealCards;
            List<string> cardStrings = new List<string>();

            //카드 인덱스에 맞춰 묶는다.
            cardStrings.AddRange(cards.CowboyHoleCards);
            cardStrings.AddRange(cards.CommunityCards);
            cardStrings.AddRange(cards.BullHoleCards);

            //판정처리
            // var isBullWin = dto.BullBestCardInfo.Rank <= dto.CowboyBestCardInfo.Rank;// Rank 가 작은 것이 높은 카드임. 비긴것도 이긴것으로..
            // var bestCards = isBullWin? dto.BullBestCardInfo.Cards : dto.CowboyBestCardInfo.Cards; //이긴 쪽의 베스트 카드정보

            List<string> bestCards = null;
            WinnerType winType = WinnerType.None;

            if (dto.BullBestCardInfo.Rank < dto.CowboyBestCardInfo.Rank) // 소 이김, Rank가 낮아야 이기는 것.
            {
                bestCards = dto.BullBestCardInfo.Cards;
                winType = WinnerType.Bull;

            }
            else if (dto.BullBestCardInfo.Rank == dto.CowboyBestCardInfo.Rank) //비긴경우 양쪽 다 하이라이트.
            {
                bestCards = new List<string>();
                bestCards.AddRange(dto.CowboyBestCardInfo.Cards);
                bestCards.AddRange(dto.BullBestCardInfo.Cards);

                winType = WinnerType.Draw;
            }
            else   // 보이가 이김
            {
                bestCards = dto.CowboyBestCardInfo.Cards;
                winType = WinnerType.Boy;
            }


            Debug.LogWarning(string.Join(",",cardStrings));

           
            //홀카드 먼저 뒤집고...
            foreach ( HoldemCards index in holecardsIndex)
            {
                //yield return fromSnapshot?null : new WaitForSeconds(0.05f);

                var intIDX = (int)index;
                var card = Cards[intIDX];
                var image = card.GetComponent<Image>();
                //image.sprite = cardSprites[cardStrings[intIDX]];
                FlipCard(card,image,cardStrings[intIDX],fromSnapshot);

                if (index == HoldemCards.BoyHoleCard2) //보이 카드 먼저 뒤집고 잠시 후 카우 카드 뒤집는다.
                     yield return fromSnapshot?null : new WaitForSeconds(0.7f);
                
            }

            yield return fromSnapshot?null : new WaitForSeconds(0.7f);

            //커무니티 카드를 뒤집는다.
            foreach ( HoldemCards index in commcardIndex)
            {
                //yield return fromSnapshot?null : new WaitForSeconds(0.05f);

                var intIDX = (int)index;
                var card = Cards[intIDX];
                var image = CardImages[intIDX];
                var cardString = cardStrings[intIDX];

                //var isBestCard = bestCards.Contains(cardString);
                //image.sprite = cardSprites[cardStrings[intIDX]];
                FlipCard(card,image,cardString,fromSnapshot);
                
            }

            
            


            float waitTime = fromSnapshot ? 0.0f: 1.0f; //애니메이션을 기다렸다가..0.4 초 였다가 기획 요청으로 늘림
            yield return new WaitForSeconds(waitTime); 

            //TODO: 이쯤에서 승패에 대한 효과 처리가 들어가면 될듯.

            

            //베스트5 카드 효과를 준다.
            var pickTime = fromSnapshot? 0.0f: 0.2f ;

            foreach( var s in cardStrings.Select( (value,i)=> (value,i)))
            {
                if (bestCards.Contains(s.value)) //베스트 카드인 경우
                {
                    var cardObj = Cards[s.i];
                    // var temp = cardObj.GetComponent<Outline>();
                    // temp.enabled = true;
                    cardObj.GetComponent<Canvas>().sortingOrder = 13;
                    cardObj.GetComponent<ParticleSystem>().Play();
                    cardObj.transform.SetAsLastSibling(); //오더링을 변경 ..위에 보이게..
                    //cardObj.transform.DOMoveY(temp+ 20.0, pickTime,true);
                }
                else
                {
                    //var old =  CardImages[s.i].color;
                    //CardImages[s.i].color = new Color(0.5f,0.5f,0.5f, 0.9f);
                    CardImages[s.i].DOColor(new Color(0.4f,0.4f,0.4f, 0.9f),pickTime);
                    //Debug.Log("no best card: "+ s.i);
                }
            }

            yield return fromSnapshot? null: new WaitForSeconds(0.2f);

            //카드 족보 스트링을 보여준다.
            string strKey = CardRankHelper.GetRankTitleKeyByCode(dto.CowboyBestCardInfo.RankCode);
            var textResponder = CowboyRankText.GetComponent<Common.Scripts.Localization.LocalizationResponder>();
            textResponder.SetKeyAndUpdate(strKey);


            strKey = CardRankHelper.GetRankTitleKeyByCode(dto.BullBestCardInfo.RankCode);
            textResponder = BullRankText.GetComponent<Common.Scripts.Localization.LocalizationResponder>();
            textResponder.SetKeyAndUpdate(strKey);

            //CowboyRankText.text = CardRankHelper.GetRankString(dto.CowboyBestCardInfo.RankCode,dto.CowboyBestCardInfo.HighNumbers);
            //BullRankText.text = CardRankHelper.GetRankString(dto.BullBestCardInfo.RankCode,dto.BullBestCardInfo.HighNumbers);

            SoundManager.PlaySFX(SFX.Result_BestCard);
            CowboyRankText.transform.parent.gameObject.SetActive(true);
            BullRankText.transform.parent.gameObject.SetActive(true);

            CowboyRankText.transform.parent.transform.SetAsLastSibling();
            BullRankText.transform.parent.transform.SetAsLastSibling();


            
            OnWinnerDecision.Invoke(winType); // 캐릭터 애니메이션 용 이벤트 발생.

            if (WinnerType.Draw != winType) //상단 결정에서 none 은 배제된다.
            {
                SoundManager.PlaySFX(SFX.Result_Win);
            }
        }

        private void FlipCard(GameObject card, Image image ,string cardString,bool fromSnapshot)
        {
            var origin = card.transform.localRotation;
            var target = Quaternion.Euler(origin.x,origin.y+90.0f,origin.z);

            float time = fromSnapshot ? 0.1f:0.4f;

            //90도 회전 후 이미지를 바꾸고 다시 -90도 회전
            card.transform.DORotateQuaternion(target,time ).SetEase(Ease.OutCirc).OnComplete(()=> { 
                 image.sprite = cardSprites[cardString];
                 card.transform.DORotateQuaternion(origin,time ).SetEase(Ease.OutBounce);
               });
            
            SoundManager.PlaySFX(SFX.Card_Open);
        }
            
    }

  
}