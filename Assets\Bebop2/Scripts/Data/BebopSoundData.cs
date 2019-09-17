using System;
using UnityEngine;

namespace Bebop
{
    public class SFX
    {
        public const string Card_Deal = "Card_Deal";
        public const string Card_Open = "Card_Open";

        public const string Chip_Betting = "Chip_Betting";
        public const string Chip_Betting_Multiple = "Chip_Betting_Multiple";
        public const string Chip_Getting = "Chip_Getting";

        public const string Timer_Count1 = "Timer_Count1";
        public const string Timer_Count2 = "Timer_Count2";
        public const string Timer_Finish = "Timer_Finish";

        public const string Result_Win = "Result_Win";
        public const string Result_BestCard = "Result_BestCard";

        public const string Hand_Start = "Hand_Start";
        public const string Hand_BettingStart = "Hand_BettingStart";

        public const string Noti_Error = "Noti_Error";

        public const string Result_Win_Cowboy = "Result_Win_Cowboy";
        public const string Result_Win_Bull = "Result_Win_Bull";

        public const string ClickButton = "ClickButton";
    }

    [CreateAssetMenu]
    public class BebopSoundData : ScriptableObject
    {
        [Serializable]
        public class Data
        {
            public string name;
            public AudioClip clip;
        }

        public Data[] data;
    } 
}
