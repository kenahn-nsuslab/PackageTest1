using Common.Scripts.Sound.Managers;
using System.Linq;
using UnityEngine;

using Common.Scripts.Managers;
using DG.Tweening;

namespace Bebop
{
    public class GameResourcePreLoader : MonoBehaviour
    {
        public BebopSoundData soundData;

        private void Awake()
        {
            RegisterExecutor();

            GameObject localizationManager = GameObject.Find("LocalizationManager");
            if (localizationManager == null)
            {
                localizationManager = new GameObject("LocalizationManager", typeof(Common.Scripts.Localization.LocalizationManager));
            }

            DontDestroyOnLoad(localizationManager);

            //SetCursor();
        }

        private void Start()
        {
            LoadSound();
        }

#if UNITY_EDITOR
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                SoundManager.PlaySFX(SFX.Chip_Betting, 0.7f);
            }
        }
#endif

        private void LoadSound()
        {
            if (SoundManager.SoundConnectionsContainsThisLevel("Bebop") < 0)
            {
                SoundManager.SetDefaultResourcesPath("Sound");
                AudioClip clip = SoundManager.Load("cowboy35s");
                SoundManager.AddSoundConnection(SoundManager.CreateSoundConnection("Bebop", clip));
                SoundManager.SetCrossDuration(0f);
                SoundManager.PlayConnection("Bebop");
            }

            SoundManager.SaveSFX(soundData.data.Select(data =>
            {
                data.clip.name = data.name;
                return data.clip;
            }).ToArray());
        }

        private void RegisterExecutor()
        {
            Common.Scripts.Managers.CommonManager.Instance.RegisterExecutor(E_GameType.CowboyHoldem, E_ExecuteType.OpenURL, new Bebop.ExecutorOpenURL());
            Common.Scripts.Managers.CommonManager.Instance.RegisterExecutor(E_GameType.CowboyHoldem, E_ExecuteType.WebViewer, new Bebop.ExecutorWebViewer());
        }

        private void SetCursor()
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.ForceSoftware);
        }
    } 
}
