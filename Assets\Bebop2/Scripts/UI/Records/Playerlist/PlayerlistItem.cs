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

namespace Bebop.UI
{
    public class PlayerlistItem : MonoBehaviour
    {
        public AvatarSlotTemplet avatarSlot;
        public TextMeshProUGUI txtNickName;
        public TextMeshProUGUI txtBettingAmount; //베팅금
        public TextMeshProUGUI txtWinningRate; //승률   
        public TextMeshProUGUI txtSuccessive; //연승

        public void SetData(AvatarSlotTemplet.E_AvatarSlotType slotType, PlayerStatistics recordData )
        {

            txtNickName.text = recordData.AccountInfo.NickName;
            txtWinningRate.text = $"{recordData.PlayStatistics.WinningRate}%";
            txtSuccessive.text = recordData.PlayStatistics.Successive.ToString();

            double bettingSum = (double)recordData.PlayStatistics.BettingSum / 100f;
            txtBettingAmount.text = bettingSum.ToString("#,0.##");
            avatarSlot.SetAvatar(slotType, recordData.AccountInfo.Avatar, recordData.AccountInfo.NationalFlag);
        }
    }
}