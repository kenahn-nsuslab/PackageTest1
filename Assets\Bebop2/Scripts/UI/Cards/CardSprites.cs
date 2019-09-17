using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace  Bebop.UI
{

    /// <summary>
    /// 카드 이미지 헬퍼
    /// </summary>
    public class CardSprites
    {

        private string [] suits = new string[] { "S","D","C","H"};
        private string [] numbers = new string [] {"A","2","3","4","5","6","7","8","9","T","J","Q","K"};

        private Dictionary<string,Sprite> sprites = new Dictionary<string,Sprite>(); //카드 스프라이트 캐시



        public CardSprites()
        {
            //CardBack = Resources.Load<Sprite>("");
        }

        /// <summary>
        /// 카드 이름과 스프라이트를 쌍으로 만든다.
        /// 서버에서 주는 카드 스트링은 {번호}{무늬} 형태임 예) 4H, 8S
        /// 이미지 명은 {무늬}_{번호}_2 형태임.
        ///  이 둘을 연결하는 딕셔너리 구성
        /// </summary>
        public void LoadSprites()
        {
            sprites.Clear();
            sprites.Add("Back",Resources.Load<Sprite>("Sprites/Card/Card_Back2"));

            string keyFormat ="{0}{1}";
            string imageNameFormat ="Sprites/Card/{1}_{0}_2";

            foreach(string s in suits)
            {
                foreach(string num in numbers)   // 키가 4H 인 경우 이미지는 H_4_2 이름을 가르키게 된다.
                {
                    sprites.Add(
                        string.Format(keyFormat,num,s),
                        Resources.Load<Sprite>(string.Format(imageNameFormat,num,s))
                        );
                }
            }


            //Debug.LogWarning(string.Join(",",sprites.Keys));
            
        }



        /// <summary>
        /// 카드 스프라이트 인덱서
        /// </summary>
        /// <value></value>
        public Sprite this[string key]
        {
            get
            {
                return sprites[key];
            }
        }


    }

}