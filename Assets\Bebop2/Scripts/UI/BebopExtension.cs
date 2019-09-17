using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Bebop
{
	public static class Extension
	{
	    public static void SetGrayscale<T>(this T obj, bool isGray, bool withChild = false) where T : Graphic
	    {
	        T img = obj.GetComponent<T>();
	        img.material = new Material(Shader.Find("Grayscale"));
	
	        img.material.SetFloat("_GrayscaleAmount", isGray == true ? 1 : 0);
	
	        if (withChild == true)
	        {
	            for (int i = 0, imax = obj.transform.childCount; i < imax; ++i)
	            {
	                Transform child = obj.transform.GetChild(i);
	                if (child != null && child.gameObject.activeSelf == true)
	                {
	                    var comp = child.GetComponent<MaskableGraphic>();
	                    if (comp != null)
	                    {
	                        comp.material = new Material(Shader.Find("Grayscale"));
	                        comp.material.SetFloat("_GrayscaleAmount", isGray == true ? 1 : 0);
	                    }
	                }
	
	            }
	        }
	    }

	    public static void SetInteractableWithGrayscale(this Button obj, bool isInteractable, bool withChild = false)
	    {
            // UI를 생성할 때 disabledColor기본값 알파가 0.5 이므로 투명한 상태를 피하고자 추가한다.
	        ColorBlock colorBlock = obj.colors;
            colorBlock.disabledColor = new Color(1f, 1f, 1f, 1f);
	        obj.colors = colorBlock;

	        obj.interactable = isInteractable;
            SetGrayscale(obj.image, !isInteractable, withChild);
	    }

        public static void SetActivePopup(this GameObject obj, bool isActive, System.Action callback=null)
        {
            if( isActive == true )
            {
                obj.SetActive(true);
                var originScale = obj.transform.localScale;
                obj.transform.localScale = Vector3.zero;
                obj.transform.DOScale(originScale, 0.1f).OnComplete( () => 
                {
                    if (callback != null)
                        callback.Invoke();
                });
            }
            else
            {
                var originScale = obj.transform.localScale;
                obj.transform.DOScale(0f, 0.1f).OnComplete(() =>
                {
                    obj.SetActive(false);
                    obj.transform.localScale = originScale;
                    if (callback != null)
                        callback.Invoke();
                });
            }
            
        }
	}
}
