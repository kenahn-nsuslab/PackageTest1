using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace  Bebop.UI
{

    public enum  CardType
    {
        FirstHoleCard,
        SecondHoleCard,
        CommunityCard
    }
    
    /// <summary>
    /// 카드 플레이스 홀더... 카드가 위치할 곳을 표시하는 용도로 쓰이며
    /// 애니메이션의 타겟 역할을 한다.
    /// </summary>
    public class CardPlaceHolder : MonoBehaviour
    {
       
        public CardType Type = CardType.CommunityCard;
        private void OnDrawGizmos() {

       
            
            // Gizmos.color = Color.green;
            // Gizmos.DrawWireCube(transform.position,new Vector3(1.1f,1.5f,0));

            Gizmos.color = Color.green;
           var curMat = Gizmos.matrix;

           var rt = this.GetComponent<RectTransform>();
           var worldMat = rt.localToWorldMatrix;

           Gizmos.matrix = worldMat;
           Gizmos.DrawWireCube(Vector3.zero, new Vector3(rt.sizeDelta.x, rt.sizeDelta.y, 0));
           //Gizmos.DrawWireCube(rt.position, new Vector3(rt.sizeDelta.x, rt.sizeDelta.y, 0));

           Gizmos.matrix = curMat;

        }
    }
}
