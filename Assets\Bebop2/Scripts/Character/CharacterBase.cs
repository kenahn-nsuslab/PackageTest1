
using System;
using System.Collections.Generic;
using UnityEngine;
using Bebop.UI;
using Bebop.Model.EventParameters;
using System.Collections;

namespace Bebop.Character
{

    [System.Serializable]
    public enum WinnerType 
    {
        None = -1,
        Draw=0,
        Bull,
        Boy
    }

    /// <summary>
    /// 카우보이와 카우의 공통 부분을 담당하는 베이스 클래스
    /// </summary>
    public class CharacterBase :MonoBehaviour
    {
        protected Animation anim;

        [SerializeField]
        protected CharacterAction Clips ;

        /// <summary>
        /// 내 캐릭의 속성을 정함.
        /// </summary>
        /// <value></value>
        protected  readonly WinnerType CharacterType ;

        protected bool IsIdle = true;

        protected Coroutine IdleCoroutine;


        float crossfadeTime = 0.3f;

        public CharacterBase( WinnerType characterType)
        {
            CharacterType = characterType;
        }

        protected virtual void Awake() {

            anim = gameObject.GetComponent<Animation>();
            //anim["idle2"].layer = 10;
        }

        protected virtual void OnEnable() {
            
            GameManager.OnHandStatusChanged += OnHandStatusChangedHandler;
            ProgressPanelController.OnBettingStop += OnBettingStop;
            CardController.OnWinnerDecision += OnWinnerDecisionHandler;

            StartCoroutine(PlayIdleAnimation());

        }

        protected virtual void OnDisable() {

            GameManager.OnHandStatusChanged -= OnHandStatusChangedHandler;
            ProgressPanelController.OnBettingStop -= OnBettingStop;
            CardController.OnWinnerDecision -= OnWinnerDecisionHandler;
        }

        protected virtual void PlayWinSound() { }

        private void OnHandStatusChangedHandler(HandStatusArgs args, bool fromSnapshot)
        {
            //if (!fromSnapshot)
            //{
                if (args.HandStatus.CompareTo(Bebop.Protocol.HandState.CardOpen) < 0) //카드오픈 이전 상태가 아이들임. 아이들로 초기화.
                {
                    if (IsIdle == false)
                    {
                        IsIdle = true;
                        IdleCoroutine = StartCoroutine(PlayIdleAnimation());
                    }
                   
                }
                else
                {
                    IsIdle= false;
                }
            //}
        }

        private void OnBettingStop()
        {
            IsIdle = false;

             if (anim.clip.name == "idle2" && CharacterType == WinnerType.Bull)
            {
                //anim.CrossFade(nextClip.name, 0.3F);
                anim.clip = Clips.Waiting;
                anim.Play();
        
            }
            else
            {
                anim.CrossFade(Clips.Waiting.name, crossfadeTime);

            }


        }



        private IEnumerator PlayIdleAnimation()
        {

            var currentClip = Clips.IdleList[0];
            //currentClip.wrapMode = WrapMode.ClampForever;
            AnimationClip nextClip = null;

            

            anim.CrossFade(currentClip.name , crossfadeTime); //첫번째 idle 애니메이션은 
            yield return new WaitForSeconds(currentClip.length);

            while(IsIdle)
            {
                
                //2,3번이 현재 플레이 중이면 다음 클립은 무조건 1번
                if (currentClip.name != Clips.IdleList[0].name)
                {
                    nextClip = Clips.IdleList[0];
                }
                else //현재 실행은 1번이다.
                {
                    var random = UnityEngine.Random.Range(1,11); // 70% 1번, 30% 는 2,3 번이 재생

                    // 5이하 인 경우 idle 에서 다시 idle 이므로 5 초과인 경우만 2, 3번으로 바꿔준다.
                    if (random > 6) 
                    {

                        //nextClip = Clips.IdleList[0];
                        nextClip = random < 9 ? Clips.IdleList[1]: Clips.IdleList[2]; 

                    }
                    else //계속 아이들 이므로 ...그냥 두자.
                    {
                        yield return new WaitForSeconds(currentClip.length);
                        continue;
                    }
                    
                }
              
                //여기서 부터는 소의 2번인  경우 Crossfade 안하게 하는 코드..연결이 부자연 스러워서...

                if (currentClip.name == "idle2" && CharacterType == WinnerType.Bull)
                {
                    //anim.CrossFade(nextClip.name, 0.3F);
                    anim.clip = nextClip;
                    anim.Play();
            
                }
                else
                {
                    anim.CrossFade(nextClip.name, crossfadeTime);

                }
                    
                currentClip = nextClip;


                    
                
                //Debug.Log("Waiting : "+ currentClip.length.ToString());
                yield return new WaitForSeconds(currentClip.length);

            }
        }

        private void OnWinnerDecisionHandler(WinnerType type)
        {
            Debug.LogWarning("Recevied Winnder Decision");
            
             //StopCoroutine(IdleCoroutine);

            if (CharacterType == type) //이긴경우
            {
                anim.CrossFade(Clips.Win.name , 0.2f);
                PlayWinSound();
            }
            else if ( WinnerType.Draw == type) // 비긴 경우
            {
                anim.CrossFade(Clips.Draw.name , 0.2f);
            }
            else // 진 경우
            {
                anim.CrossFade(Clips.Lose.name, 0.2f);
            }
        }

        
       
    }


    /// <summary>
    /// 캐릭터 애니메이션 컨테이너 Editor 에서 지정한다.
    /// Animation 에 등록된 클립의 이름을 얻기 위해 추상화 한 클래스
    /// 애니메이션 파일 이름이 변경된 경우 대응하기 위함.
    /// 연결된 클립은 Animation 컴포넌트에 존재 해야 하고 여기서는 name 을 가져오기 위해 클립 파일을 연결한다.
    /// </summary>
    [Serializable]
    public class CharacterAction
    {
        public List<AnimationClip> IdleList;


        public AnimationClip Waiting;

        public AnimationClip Win;

        public AnimationClip Lose;


        public AnimationClip Draw;

    }
}