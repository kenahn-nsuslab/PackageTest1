using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

using System;

using Bebop.Protocol;
using Bebop.Model.EventParameters;

using Common.Scripts.Utils;
using Common.Scripts.Localization;
using UnityEngine.Networking;
using TMPro;

namespace Bebop.UI
{
    public class Playerlist : IRecord
    {
        public PlayerlistItem prefabItem;
        public TextMeshProUGUI txtCurrentPlayerCount;
        public TextMeshProUGUI txtPlayCount;

        public ScrollRect scrollRect;
        public ScrollRectOptimizing scrollViewOpt;

        private const int PageSize = 10;
        private List<PlayRecordData> lstPlayRecord = new List<PlayRecordData>();

        private int currentPage = 0;
        private bool isLastPage = false;
        private bool isUpdating = false;

        private void Awake()
        {
            scrollRect.onValueChanged.AddListener(OnScrollRectValueChanged);
        }

        protected override void FocusIn()
        {
            Clear();
            OnRequestPlayerList();
        }
        protected override void FocusOut()
        {
            Clear();
            scrollViewOpt.Reset();
        }

        protected override void AddEvent()
        {
            GameManager.OnReceiveCurrentPlayerList += OnPlayStatisticsRecordsResponse;
        }
        protected override void RemoveEvent()
        {
            GameManager.OnReceiveCurrentPlayerList -= OnPlayStatisticsRecordsResponse;
        }

        private void OnRequestPlayerList()
        {
            isUpdating = true;
            

            GameManager.GetCurrentPlayerList(currentPage, PageSize);

            //테스트코드
            //GetTestCurrentPlayerList(currentPage, PageSize, OnPlayStatisticsRecordsResponse);
        }

        private void OnPlayStatisticsRecordsResponse(PlayerListResponse res)
        {
            WaitIndicator.SetActive(false);

            if (res.GodBrain != null)
                lstPlayRecord.Add(new PlayRecordData(res.GodBrain, AvatarSlotTemplet.E_AvatarSlotType.GobBrain));

            if (res.Supernova != null)
                lstPlayRecord.Add(new PlayRecordData(res.Supernova, AvatarSlotTemplet.E_AvatarSlotType.Supernova));

            var playerCount = string.Format("{0} : {1}", LocalizationManager.Instance.GetText("plist_players"), res.PlayerCount);

            txtCurrentPlayerCount.text = playerCount ;

            if (res.PlayerList != null)
            {
                var listRecord = res.PlayerList.Select(r =>
                {
                    PlayRecordData prd = new PlayRecordData(r, AvatarSlotTemplet.E_AvatarSlotType.Third);
                    return prd;

                }).ToList();

                if (listRecord.Count > PageSize)
                    ++currentPage;
                else
                    isLastPage = true;

                Bind(listRecord.Take(PageSize).ToList());
                isUpdating = false;
            }
        }

        private void Bind(List<PlayRecordData> lstData)
        {
            lstPlayRecord.AddRange(lstData);

            var lstItems = scrollViewOpt.GetActiveContentItems().Where(d => d.gameObject.activeSelf == true).ToList();

            if (lstItems.Any() == false)
                scrollViewOpt.OptimizeScrollRect(lstPlayRecord.Count, OnUpdateItem);
            else
                scrollViewOpt.OptimizeScrollRect(lstPlayRecord.Count);
        }

        private void OnUpdateItem(int index, GameObject obj)
        {
            var item = obj.GetComponent<PlayerlistItem>();

            if (item == null)
                return;

            var recordData = lstPlayRecord[index];

            item.SetData(recordData.AvatarType, recordData.RecordData);

            if( recordData.AvatarType == AvatarSlotTemplet.E_AvatarSlotType.Third)
            {
                int topAvatarCount = lstPlayRecord.Where(d => d.AvatarType == AvatarSlotTemplet.E_AvatarSlotType.GobBrain ||
                                                        d.AvatarType == AvatarSlotTemplet.E_AvatarSlotType.Supernova).Count();

                item.avatarSlot.SetRank((index - topAvatarCount) + 1);
            }
        }

        private void OnScrollRectValueChanged(Vector2 normal)
        {
            if(isUpdating == true)
                return;

            if (isLastPage == true)
                return;

            if (normal.y <= 0)
            {
                OnRequestPlayerList();
            }
        }

        private void Clear()
        {
            isLastPage = false;
            isUpdating = false;
            currentPage = 0;
            lstPlayRecord.Clear();
        }

        //테스트 코드   
        private void GetTestCurrentPlayerList(int page, int size, Action<PlayerListResponse> resCallBack)
        {
            const int TotalCount = 5;
            int curretTotalSize = page * size;

            int loopCount = curretTotalSize > TotalCount ? curretTotalSize - TotalCount : size + 1;

            List<PlayerStatistics> lstRecord = new List<PlayerStatistics>();

            for (int index = 0; index < loopCount; ++index)
            {
                lstRecord.Add(GetTestPlayerInfo());
            }

            PlayerStatistics super = null;
            PlayerStatistics god = null;

            if (page == 1)
            {
                super = GetTestPlayerInfo();
                god = GetTestPlayerInfo();
            }

            var newData = new PlayerListResponse()
            {
                Supernova = super,
                GodBrain = god,
                PlayerList = lstRecord,
                PlayerCount = UnityEngine.Random.Range(5, 99),
            };

            

            resCallBack.Invoke(newData);
        }


        private PlayerStatistics GetTestPlayerInfo()
        {
            string[] nicknames = new string[] { "QWWE123", "sdfsedf", "KOREA123", "ka12n3Danma", "Ka3123DRanm", "USA123" };
            string[] avatarUrl = new string[] { "http://download.ggnet.com/portal/ggfishing3d/bundles/stage/TestAvatar/1/1/Avatar1.png",
                                                "http://download.ggnet.com/portal/ggfishing3d/bundles/stage/TestAvatar/1/1/Avatar2.png",
                                                "http://download.ggnet.com/portal/ggfishing3d/bundles/stage/TestAvatar/1/1/Avatar3.png",
                                                "http://download.ggnet.com/portal/ggfishing3d/bundles/stage/TestAvatar/1/1/Avatar4.png",
                                                "http://download.ggnet.com/portal/ggfishing3d/bundles/stage/TestAvatar/1/1/Avatar5.png" };

            int nickRandomIdx = UnityEngine.Random.Range(0, nicknames.Length);
            int avatarUrlIdx = UnityEngine.Random.Range(0, avatarUrl.Length);

            var accountInfo = new AccountInfo
            {
                AccountId = UnityEngine.Random.Range(123, 500),
                NickName = nicknames[nickRandomIdx],
                Avatar = avatarUrl[avatarUrlIdx],
            };

            return new PlayerStatistics
            {
                AccountInfo = accountInfo,
                PlayStatistics = new PlayStatistics()
                {
                    Successive = UnityEngine.Random.Range(2, 10),
                    WinningRate = UnityEngine.Random.Range(30, 80),
                    BettingSum = UnityEngine.Random.Range(1000, 999999),
                }
                /* Successive = UnityEngine.Random.Range(2, 10), WinStatistics = UnityEngine.Random.Range(30, 80)*/
            };
        }


        private class PlayRecordData
        {
            public AvatarSlotTemplet.E_AvatarSlotType AvatarType { get { return avatarType; } }
            public PlayerStatistics RecordData { get { return recordData; } }

            private AvatarSlotTemplet.E_AvatarSlotType avatarType = AvatarSlotTemplet.E_AvatarSlotType.None;
            private PlayerStatistics recordData;

            public PlayRecordData(PlayerStatistics record, AvatarSlotTemplet.E_AvatarSlotType type = AvatarSlotTemplet.E_AvatarSlotType.None)
            {
                avatarType = type;
                recordData = record;
            }
        }
    }
}