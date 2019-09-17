using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bebop
{
    public class ImageFillEnd : MonoBehaviour
    {
        public Image targetFrame;
        public Image targetBar;

        public float hideAmountValue = 0.8f;

        public void UpdateFillEnd(float fillAmount)
        {
            if (targetFrame == null)
                return;

            if (false == IsFillType())
                return;

            var rt = targetFrame.GetComponent<RectTransform>();
            var size = rt.sizeDelta;

            //float fillAmount = targetFrame.fillAmount;

            if (fillAmount >= hideAmountValue)
                targetBar.gameObject.SetActive(false);
            else
                targetBar.gameObject.SetActive(true);

            if (targetFrame.fillMethod == Image.FillMethod.Vertical)
            {
                float value = size.y * fillAmount;
                value = targetFrame.fillOrigin == 0 ? -value : value;
                targetBar.transform.localPosition = new Vector2(0, value);
            }
            else
            {
                float value = size.x * fillAmount;
                value = targetFrame.fillOrigin == 0 ? value : -value;
                targetBar.transform.localPosition = new Vector2(value, 0);
            }
        }

        private bool IsFillType()
        {
            if (targetFrame == null)
                return false;

            if (targetFrame.type != Image.Type.Filled)
                return false;

            if( targetFrame.fillMethod == Image.FillMethod.Radial90 || targetFrame.fillMethod == Image.FillMethod.Radial180 ||
                targetFrame.fillMethod == Image.FillMethod.Radial360)
            {
                return false;
            }

            return true;
        }

        public void ResetFillEnd()
        {
            targetBar.transform.localPosition = new Vector2(0, 0);
        }

    }
}