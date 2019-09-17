using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using DG.Tweening;

namespace Bebop
{
    public class CanvasGroupFadeOut : MonoBehaviour
    {
        public CanvasGroup canvasGroup;

        [Range(0.1f, 10)]
        public float speed = 0.7f;

        public void Awake()
        {
            if (canvasGroup == null)
                return;

            canvasGroup.gameObject.SetActive(true);
        }

        private void OnEnable()
        {
            GameManager.OnGameStart += OnStart;
        }

        private void OnDisable()
        {
            GameManager.OnGameStart -= OnStart;
        }

        public void OnStart()
        {
            if (canvasGroup == null)
                return;

            DOTween.To(() => canvasGroup.alpha, d => canvasGroup.alpha = d, 0, speed).OnComplete(ResetCanvas);
        }

        private void ResetCanvas()
        {
            canvasGroup.gameObject.SetActive(false);
            canvasGroup.alpha = 1;
        }
    }
}