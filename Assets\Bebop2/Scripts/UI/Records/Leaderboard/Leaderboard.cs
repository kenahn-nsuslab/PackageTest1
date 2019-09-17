using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System;
using System.Linq;
using Bebop.Protocol;
using Bebop.Model.EventParameters;

using Common.Scripts.Utils;
using UnityEngine.Networking;
using TMPro;

namespace Bebop.UI
{
    public class Leaderboard : IRecord
    {
        public enum E_MainType
        {
            Betting,
            Successive,
            WinningRate,
        }

        public enum E_SubType
        {
            Day = InqueryType.Today,
            Month = InqueryType.Days30,
        }

        [System.Serializable]
        public struct MainBarToggleGroup
        {
            public E_MainType type;
            public Toggle toggle;
            public TextMeshProUGUI toggleLabel;
        }

        public List<MainBarToggleGroup> lstMainBar = new List<MainBarToggleGroup>();

        [System.Serializable]
        public struct SubBarToggleGroup
        {
            public E_SubType inqueryType;
            public Toggle toggle;
            public TextMeshProUGUI toggleLabel;
        }

        public LeaderboardItem prefabItem;
        public TextMeshProUGUI txtCurrentGameCount;
        public List<SubBarToggleGroup> lstSubBar = new List<SubBarToggleGroup>();

        public ScrollRect scrollRect;
        public ScrollRectOptimizing scrollViewOpt;

        [Header("[ MyRank Info ]")]
        public MedalGroup medalGroup;
        public AvatarSlotTemplet avatarSlot;
        public TextMeshProUGUI txtNickName;
        public TextMeshProUGUI txtRecordValue;
        public TextMeshProUGUI txtDate;

        private bool isFocus = false;
        private E_MainType currentMainType = E_MainType.Betting;
        private E_SubType currentSubType = E_SubType.Day;

        private const int PageSize = 10;
        private int currentPage = 1;
        private bool isLastPage = false;
        private bool isUpdating = false;

        private List<IResponseRecord> lstRecordData = new List<IResponseRecord>();

        private void Awake()
        {
            scrollRect.onValueChanged.AddListener(OnScrollRectValueChanged);

            lstMainBar.ForEach(d =>
            {
                d.toggle.onValueChanged.AddListener(isOn => OnClickMainToggle(isOn, d.type));
            });

            lstSubBar.ForEach(d =>
            {
                d.toggle.onValueChanged.AddListener(isOn => OnClickSubToggle(isOn, d.inqueryType));
            });
        }

        protected override void FocusIn()
        {
            DisableMyRecord();
            DefaultToggleSet(true);
            isFocus = true;

            OnRequestRecord(currentMainType, currentSubType);
        }
        protected override void FocusOut()
        {
            isFocus = false;
            DefaultToggleSet(false);
            ResetData();
        }

        protected override void AddEvent()
        {
            GameManager.OnPlayerBettingSumRecordResponse += OnPlayerBettingSumRecordResponse;
            GameManager.OnPlayerSuccessiveRecordResponse += OnPlayerSuccessiveRecordResponse;
            GameManager.OnPlayerWinningRateRecordResponse += OnPlayerWinningRateRecordResponse;
        }

        protected override void RemoveEvent()
        {
            GameManager.OnPlayerBettingSumRecordResponse -= OnPlayerBettingSumRecordResponse;
            GameManager.OnPlayerSuccessiveRecordResponse -= OnPlayerSuccessiveRecordResponse;
            GameManager.OnPlayerWinningRateRecordResponse -= OnPlayerWinningRateRecordResponse;
        }

        private void OnClickMainToggle(bool isOn, E_MainType mainType)
        {
            var findToggle = lstMainBar.Find(d => d.type == mainType);
            var lable = findToggle.toggleLabel;

            if (isOn == false)
            {
                lable.color = Records.Instance.DisableToggleTextColor;
                return;
            }

            lable.color = Records.Instance.EnableToggleTextColor;

            if (isFocus == false)
                return;

            if (currentMainType == mainType)
                return;

            currentMainType = mainType;

            ResetData();
            OnRequestRecord(currentMainType, currentSubType);
        }

        private void OnClickSubToggle(bool isOn, E_SubType subType)
        {
            if (isFocus == false)
                return;

            var findToggle = lstSubBar.Find(d => d.inqueryType == subType);
            var lable = findToggle.toggleLabel;

            if (isOn == false)
            {
                lable.color = Records.Instance.DisableToggleTextColor;
                return;
            }

            lable.color = Records.Instance.EnableToggleTextColor;

            if (isFocus == false)
                return;

            if (currentSubType == subType)
                return;

            currentSubType = subType;

            ResetData();
            OnRequestRecord(currentMainType, currentSubType);
        }

        private void OnScrollRectValueChanged(Vector2 normal)
        {
            if (isUpdating == true)
                return;

            if (isLastPage == true)
                return;

            if (normal.y <= 0)
            {
                OnRequestRecord(currentMainType, currentSubType);
            }
        }

        private void OnRequestRecord(E_MainType type, E_SubType subType)
        {
            isUpdating = true;

            switch (type)
            {
                case E_MainType.Betting:
                    {
                        GameManager.GetPlayerBettingSumRecord((InqueryType)subType, currentPage, PageSize);
                        //GetTestBettingResponse(currentPage, PageSize, OnPlayerBettingSumRecordResponse);
                        break;
                    }
                case E_MainType.Successive:
                    {
                        GameManager.GetPlayerSuccessiveRecord((InqueryType)subType, currentPage, PageSize);
                        //GetTestSuccessiveResponse(currentPage, PageSize, OnPlayerSuccessiveRecordResponse);
                        break;
                    }
                case E_MainType.WinningRate:
                    {
                        GameManager.GetPlayerWinningRateRecord((InqueryType)subType, currentPage, PageSize);
                        //GetTestWinningResponse(currentPage, PageSize, OnPlayerWinningRateRecordResponse);
                        break;
                    }
            }
        }

        private void OnPlayerBettingSumRecordResponse(PlayerBettingSumRecordResponse res)
        {
            WaitIndicator.SetActive(false);

            if (res == null || res.PlayerBettingSumRecords == null || res.MyBettingSumRecord == null)
                return;

            var lstData = res.PlayerBettingSumRecords.Select(r => 
            {
                var data = new BettingData(r);
                return data as IResponseRecord;

            }).ToList();

            if (lstData.Count > PageSize)
                ++currentPage;
            else
                isLastPage = true;

            Bind(lstData);

            UpdateMyRecord(res.MyRanking, res.MyBettingSumRecord.CausedAt, bettingSum: res.MyBettingSumRecord.BettingSum);

            isUpdating = false;
        }

        private void OnPlayerSuccessiveRecordResponse(PlayerSuccessiveRecordResponse res)
        {
            WaitIndicator.SetActive(false);

            if (res == null || res.PlayerSuccessiveRecords == null || res.MySuccessiveRecord == null)
                return;

            var lstData = res.PlayerSuccessiveRecords.Select(r =>
            {
                var data = new SuccessiveData(r);
                return data as IResponseRecord;

            }).ToList();

            if (lstData.Count > PageSize)
                ++currentPage;
            else
                isLastPage = true;

            Bind(lstData);

            UpdateMyRecord(res.MyRanking, res.MySuccessiveRecord.CausedAt, successive: res.MySuccessiveRecord.Successive);

            isUpdating = false;
        }

        private void OnPlayerWinningRateRecordResponse(PlayerWinningRateRecordResponse res)
        {
            WaitIndicator.SetActive(false);

            if (res == null || res.MyWinningRateRecord == null || res.PlayerWinningRateRecords == null)
                return;

            var lstData = res.PlayerWinningRateRecords.Select(r =>
            {
                var data = new WinningRateData(r);
                return data as IResponseRecord;

            }).ToList();

            if (lstData.Count > PageSize)
                ++currentPage;
            else
                isLastPage = true;

            Bind(lstData);

            UpdateMyRecord(res.MyRanking, res.MyWinningRateRecord.CausedAt, winning: res.MyWinningRateRecord.WinningRate);

            isUpdating = false;
        }

        private void Bind(List<IResponseRecord> lstData)
        {
            lstRecordData.AddRange(lstData);

            var lstItem = scrollViewOpt.GetActiveContentItems().Where(d => d.gameObject.activeSelf == true).ToList();

            if (lstItem.Any() == false)
                scrollViewOpt.OptimizeScrollRect(lstRecordData.Count, OnUpdateItem);
            else
                scrollViewOpt.OptimizeScrollRect(lstRecordData.Count);
        }

        private void OnUpdateItem(int index, GameObject obj)
        {
            var item = obj.GetComponent<LeaderboardItem>();

            if (item == null)
                return;

            var data = lstRecordData[index];
            item.SetRecordData(index + 1, data);
        }

        private void UpdateMyRecord(int rank, DateTime date, long bettingSum = -1, long profit = -1, int winning = -1, int successive = -1)
        {
            var avatarUrl = UserData.Instance.AccountInfo.Avatar;
            var nationalFlag = UserData.Instance.AccountInfo.NationalFlag;

            avatarSlot.SetAvatar(AvatarSlotTemplet.E_AvatarSlotType.None, avatarUrl, nationalFlag);
            medalGroup.UpdateRank(rank, true);
            this.txtDate.text = date.ToString("yyy.MM.dd");
            this.txtNickName.text = UserData.Instance.AccountInfo.NickName;

            this.txtRecordValue.text = "-";

            if (bettingSum > 0)
                this.txtRecordValue.text = bettingSum.ToString();
            else if (profit > 0)
                this.txtRecordValue.text = profit.ToString();
            else if (winning > 0)
                this.txtRecordValue.text = winning.ToString();
            else if (successive > 0)
                this.txtRecordValue.text = successive.ToString();
        }

        private void DisableMyRecord()
        {
            this.txtDate.text = "";
            this.txtNickName.text = "";
            this.txtRecordValue.text = "";
            this.medalGroup.AllHide();
        }

        private void DefaultToggleSet(bool isOn)
        {
            lstMainBar.ForEach(d =>
            {
                if( d.type == currentMainType )
                    d.toggle.isOn = isOn;
                else
                    d.toggleLabel.color = Records.Instance.DisableToggleTextColor;
            });

            lstSubBar.ForEach(d =>
            {
                if( d.inqueryType == currentSubType )
                    d.toggle.isOn = isOn;
                else
                    d.toggleLabel.color = Records.Instance.DisableToggleTextColor;
            });
        }

        private void ResetData()
        {
            currentPage = 1;
            isLastPage = false;
            isUpdating = false;
            scrollViewOpt.Reset();
            lstRecordData.Clear();
        }


        //테스트 코드
        private void GetTestSuccessiveResponse(int page, int size, Action<PlayerSuccessiveRecordResponse> onResponse)
        {
            const int TotalCount = 200;
            int curretTotalSize = page * size;

            int loopCount = curretTotalSize > TotalCount ? curretTotalSize - TotalCount : size + 1;

            List<PlayerSuccessiveRecord> lstRecord = new List<PlayerSuccessiveRecord>();

            for (int index = 0; index < loopCount; ++index)
            {
                lstRecord.Add(GetTestRecordSuccessive());
            }

            onResponse(new PlayerSuccessiveRecordResponse
            {
                PlayerSuccessiveRecords = lstRecord,
                MyRanking = UnityEngine.Random.Range(1, 100),
                MySuccessiveRecord = new SuccessiveRecord
                {
                    CausedAt = DateTime.UtcNow,
                    Successive = UnityEngine.Random.Range(0, 15),
                }
            });
        }

        private void GetTestBettingResponse(int page, int size, Action<PlayerBettingSumRecordResponse> onResponse)
        {
            const int TotalCount = 200;
            int curretTotalSize = page * size;

            int loopCount = curretTotalSize > TotalCount ? curretTotalSize - TotalCount : size + 1;

            List<PlayerBettingSumRecord> lstRecord = new List<PlayerBettingSumRecord>();

            for (int index = 0; index < loopCount; ++index)
            {
                lstRecord.Add(GetTestRecordBettingSum());
            }

            onResponse(new PlayerBettingSumRecordResponse
            {
                PlayerBettingSumRecords = lstRecord,
                MyRanking = UnityEngine.Random.Range(1, 100),
                MyBettingSumRecord = new BettingSumRecord
                {
                    CausedAt = DateTime.UtcNow,
                    BettingSum = UnityEngine.Random.Range(1000, 99999),
                }
            });
        }

        private void GetTestWinningResponse(int page, int size, Action<PlayerWinningRateRecordResponse> onResponse)
        {
            const int TotalCount = 200;
            int curretTotalSize = page * size;

            int loopCount = curretTotalSize > TotalCount ? curretTotalSize - TotalCount : size + 1;

            List<PlayerWinningRateRecord> lstRecord = new List<PlayerWinningRateRecord>();

            for (int index = 0; index < loopCount; ++index)
            {
                lstRecord.Add(GetTestRecordWinningRate());
            }

            onResponse(new PlayerWinningRateRecordResponse
            {
                PlayerWinningRateRecords = lstRecord,
                MyRanking = UnityEngine.Random.Range(1, 100),
                MyWinningRateRecord = new WinningRateRecord
                {
                    CausedAt = DateTime.UtcNow,
                    WinningRate = UnityEngine.Random.Range(10, 100),
                }
            });
        }


        private PlayerSuccessiveRecord GetTestRecordSuccessive()
        {
            return new PlayerSuccessiveRecord
            {
                AccountInfo = GetTestAccountInfo(),
                SuccessiveRecord = new SuccessiveRecord()
                {
                    Successive = UnityEngine.Random.Range(2, 10),
                    CausedAt = DateTime.UtcNow,
        }
            };
        }

        private PlayerBettingSumRecord GetTestRecordBettingSum()
        {
            return new PlayerBettingSumRecord
            {
                AccountInfo = GetTestAccountInfo(),
                BettingSumRecord = new BettingSumRecord()
                {
                    BettingSum = UnityEngine.Random.Range(1000, 99999),
                    CausedAt = DateTime.UtcNow,
                }
            };
        }

        private PlayerWinningRateRecord GetTestRecordWinningRate()
        {
            return new PlayerWinningRateRecord
            {
                AccountInfo = GetTestAccountInfo(),
                WinningRateRecord = new WinningRateRecord()
                {
                    WinningRate = UnityEngine.Random.Range(0, 100),
                    CausedAt = DateTime.UtcNow,
                }
            };
        }

        private AccountInfo GetTestAccountInfo()
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

            return accountInfo;
        }

    }

    //Wrapper class (분산된 Record 클래스를 통합하여 쓴다.)
    public interface IResponseRecord
    {
        long GetRecordValue();

        AccountInfo GetAccountInfo();

        DateTime GetCausedAt();

        Leaderboard.E_MainType GetContentType();
    }

    public class BettingData : IResponseRecord
    {
        private PlayerBettingSumRecord recordData; 
        public BettingData(PlayerBettingSumRecord data)
        {
            recordData = data;
        }

        public long GetRecordValue()
        {
           return recordData.BettingSumRecord.BettingSum;
        }

        public DateTime GetCausedAt()
        {
            return recordData.BettingSumRecord.CausedAt;
        }

        public AccountInfo GetAccountInfo()
        {
            return recordData.AccountInfo;
        }

        public Leaderboard.E_MainType GetContentType()
        {
            return Leaderboard.E_MainType.Betting;
        }
    }

    public class SuccessiveData : IResponseRecord
    {
        private PlayerSuccessiveRecord recordData;
        public SuccessiveData(PlayerSuccessiveRecord data)
        {
            recordData = data;
        }

        public long GetRecordValue()
        {
            return recordData.SuccessiveRecord.Successive;
        }

        public DateTime GetCausedAt()
        {
            return recordData.SuccessiveRecord.CausedAt;
        }

        public AccountInfo GetAccountInfo()
        {
            return recordData.AccountInfo;
        }
        public Leaderboard.E_MainType GetContentType()
        {
            return Leaderboard.E_MainType.Successive;
        }

    }
    public class WinningRateData : IResponseRecord
    {
        private PlayerWinningRateRecord recordData;
        public WinningRateData(PlayerWinningRateRecord data)
        {
            recordData = data;
        }

        public long GetRecordValue()
        {
            return recordData.WinningRateRecord.WinningRate;
        }

        public DateTime GetCausedAt()
        {
            return recordData.WinningRateRecord.CausedAt;
        }

        public AccountInfo GetAccountInfo()
        {
            return recordData.AccountInfo;
        }

        public Leaderboard.E_MainType GetContentType()
        {
            return Leaderboard.E_MainType.WinningRate;
        }
    }

}