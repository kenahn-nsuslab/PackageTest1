using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System;
using System.Linq;
using Bebop.Protocol;
using Bebop.Model.EventParameters;

using UnityEngine.Networking;
using TMPro;

using Common.Scripts.Localization;


namespace Bebop.UI
{
    [System.Serializable]
    public struct MedalGroup
    {
        public Image medalG;
        public Image medalS;
        public Image medalB;
        public TextMeshProUGUI txtValue;

        public void UpdateRank(int rank, bool isLimitRank = false)
        {
            const int Max_Rank = 50;
            AllHide();


            switch (rank)
            {
                case 1:
                    medalG?.gameObject.SetActive(true);
                    break;
                case 2:
                    medalS?.gameObject.SetActive(true);
                    break;
                case 3:
                    medalB?.gameObject.SetActive(true);
                    break;
                default:
                    {
                        txtValue.gameObject.SetActive(true);

                        if (isLimitRank == true)
                        {
                            string strRank = rank <= 0 ? "-" : rank.ToString();

                            txtValue.text = rank < Max_Rank ? $"{strRank}" : $"+{Max_Rank}";
                        }
                        else
                            txtValue.text = $"{rank}";
                        break;
                    }
                    
            }
        }

        public void AllHide()
        {
            medalG.gameObject.SetActive(false);
            medalS.gameObject.SetActive(false);
            medalB.gameObject.SetActive(false);
            txtValue.gameObject.SetActive(false);
        }
    }

    public class LeaderboardItem : MonoBehaviour
    {
        public MedalGroup medalGroup;
        public AvatarSlotTemplet avatarSlot;
        public TextMeshProUGUI txtNickName;
        public TextMeshProUGUI txtRecordValue;
        public TextMeshProUGUI txtDate;

        public void SetRecordData(int rank, IResponseRecord recordData)
        {
            var accountInfo = recordData.GetAccountInfo();
            SetDefault(rank, accountInfo.NickName, accountInfo.Avatar, accountInfo.NationalFlag, recordData.GetCausedAt());

            var format =  GetFormatRecordValue(recordData.GetContentType());
            txtRecordValue.text = string.Format(format, recordData.GetRecordValue().ToString());
        }

        private void SetDefault(int rank, string nickName, string avatarUrl, string flag, DateTime date)
        {
            medalGroup.UpdateRank(rank);

            txtNickName.text = nickName;

            //DateTime.Now.ToString("yyyy.MM.dd.HH.mm.ss");
            txtDate.text = date.ToString("yyy.MM.dd");
            avatarSlot.SetAvatar(AvatarSlotTemplet.E_AvatarSlotType.None, avatarUrl, flag);
        }

        private string GetFormatRecordValue(Leaderboard.E_MainType type)
        {
            if (type == Leaderboard.E_MainType.Betting)
                return LocalizationManager.Instance.GetText("plist_winrec");
            else if (type == Leaderboard.E_MainType.WinningRate)
                return LocalizationManager.Instance.GetText("plist_raterec");
            else
                return "{0}%";
        }
    }
}