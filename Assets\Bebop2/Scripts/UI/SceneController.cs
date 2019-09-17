using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

using Common.Scripts.Managers;

namespace Bebop.UI
{

    public class SceneController : Common.Scripts.ISceneLoad
    {
        public Image imgBar;
        private ImageFillEnd imgFillEnd;

        public float Duration = 1.0f;

        //private static string nextSceneName = Common.Scripts.Define.Scene.Bebop;
        private static System.Action nextCallBack = null;

        private void Start()
        {
            imgFillEnd = imgBar.GetComponent<ImageFillEnd>();
            imgFillEnd.ResetFillEnd();

            StartCoroutine(LoadScene());
        }


        IEnumerator LoadScene()
        {
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(_nextSceneName);
            asyncOperation.completed += OnComplete;

            asyncOperation.allowSceneActivation = false;

            //while (!asyncOperation.isDone)
            while (asyncOperation.progress < 0.9f)
            {
                imgBar.fillAmount = asyncOperation.progress;
                imgFillEnd.UpdateFillEnd(imgBar.fillAmount);
                yield return null;
            }

            imgBar.fillAmount = 1.0f;
            imgFillEnd.UpdateFillEnd(imgBar.fillAmount);

            yield return new WaitForSeconds(Duration);

            PrevSceneActivation?.Invoke();

            asyncOperation.allowSceneActivation = true;
        }

        private void OnComplete(AsyncOperation async)
        {
            async.completed -= OnComplete;
            nextCallBack?.Invoke();
        }

        public static void Load(string sceneName, System.Action prev = null , System.Action next = null)
        {
            prev?.Invoke();
            nextCallBack = next;
            _nextSceneName = sceneName;

            SceneManager.LoadScene(sceneName);
            
        }
    }
}