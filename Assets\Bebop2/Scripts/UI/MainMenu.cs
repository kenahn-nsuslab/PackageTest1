using Common.Scripts.Localization;
using Common.Scripts.Sound.Managers;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Bebop
{
    public class MainMenu : MonoBehaviour
    {
        public interface IMainMenuButtonExecutor
        {
            void Open();
        }

        public enum E_MainMenu
        {
            Logout,
            BettingHistory,
            Setting,
            Question,
        }

        public static MainMenu Instance { get; private set; }
        
        [Serializable]
        public class MenuButton
        {
            public E_MainMenu type;
            public Button button;
        }
        public Button btnOpen;
        public MenuButton[] arrMenuButton;
        public RectTransform slideMask;
        public Image slideImage;

        [Header("WholeButton")]
        public Button btnWhole;

        private Dictionary<E_MainMenu, IMainMenuButtonExecutor> dicExecutor = new Dictionary<E_MainMenu, IMainMenuButtonExecutor>();
        private bool isOpened = false;
        private Tween moveOpen;

        private float slideWidth;

        private void Awake()
        {
            btnOpen.onClick.AddListener(OnClickOpen);
            btnWhole.onClick.AddListener(OnClickOpen);

            Array.ForEach(arrMenuButton, menu =>
            {
                menu.button.onClick.AddListener(() => OnClick(menu.type));
                dicExecutor.Add(menu.type, new BettingHistoryExecutor());

                //if (menu.type == E_MainMenu.BettingHistory)
                //{
                //    menu.button.SetInteractableWithGrayscale(false);
                //}
            });

            slideWidth = slideMask.rect.width;
            btnWhole.gameObject.SetActive(isOpened);
        }
        
        private void Start()
        {
            slideMask.DOSizeDelta(new Vector2(0f, 72f), 0);
            slideImage.DOFade(0f, 0f);

            RegisterExecutor(E_MainMenu.Logout, new LogoutExecutor());
            RegisterExecutor(E_MainMenu.Setting, new SettingExecutor());
            RegisterExecutor(E_MainMenu.Question, new QuestionExecutor());
            RegisterExecutor(E_MainMenu.BettingHistory, new BettingHistoryExecutor());
        }

        private void OnClickOpen()
        {
            if (moveOpen != null && moveOpen.IsPlaying())
                return;

            SoundManager.PlaySFX(SFX.ClickButton);

            moveOpen = slideMask.DOSizeDelta(new Vector2(isOpened == false ? slideWidth : 0f, 72f), 0.2f).OnComplete(OpenComplete);
            slideImage.DOFade(isOpened == false ? 1f : 0f, 0.3f);
        }

        private void OpenComplete()
        {
            isOpened = !isOpened;

            btnWhole.gameObject.SetActive(isOpened);

            //-> 타이머 작동
            if (isOpened == true)
            {
                Invoke("OnClickOpen", 5f);
            }
            else
            {
                CancelInvoke();
            }
        }

        private void OnClick(E_MainMenu type)
        {
            SoundManager.PlaySFX(SFX.ClickButton);

            dicExecutor[type].Open();
        }

        public void RegisterExecutor(E_MainMenu type, IMainMenuButtonExecutor executor)
        {
            dicExecutor[type] = executor;
        }
    } 

    public class NoneMainMenuExecutor : MainMenu.IMainMenuButtonExecutor
    {
        public void Open()
        {
            Debug.Log("Open");
        }
    }

    //-> 테스트
    public class BettingHistoryExecutor : MainMenu.IMainMenuButtonExecutor
    {
        public void Open()
        {
            BettingHistory.Instance.SetActive(true);
        }
    }

    //-> 테스트
    public class LogoutExecutor : MainMenu.IMainMenuButtonExecutor
    {
        public void Open()
        {
            BebopMessagebox.OkCancel("", LocalizationManager.Instance.GetText("text_ApplicationQuit"), () =>
            {
                GameObject.DestroyImmediate(BebopMessagebox.Instance.gameObject);
                GameManager.SendCheckOutRequest();
                Common.Scripts.GameSwitch.Load(Common.Scripts.Managers.E_GameType.Fishing, Common.Scripts.Define.Scene.IntegratedLobby);
            });
        }
    }

    public class SettingExecutor : MainMenu.IMainMenuButtonExecutor
    {
        public void Open()
        {
            BebopSetting.Instance.SetActive(true);
        }
    }

    public class QuestionExecutor : MainMenu.IMainMenuButtonExecutor
    {
        public void Open()
        {
            GameHelp.Instance.Open();
        }
    }
}
