using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using System;
using System.Linq;
using Bebop.Protocol;
using Bebop.Model.EventParameters;

using DG.Tweening;

using UnityEngine.Networking;

namespace Bebop.UI
{
    public class PlayerAvatar : MonoBehaviour
    {
        public enum E_ItemType
        {
            None,
            Supernova, //초신성.
            GodBrain, //신산자
        }

        [System.Serializable]
        public struct AvatarData
        {
            public E_ItemType itemType;
            public PlayerAvatarItem avatarItem;

        }
        public List<AvatarData> lstAvatarData = new List<AvatarData>();

        public static PlayerAvatar Instance { get; private set; }
        
        private void Awake()
        {
            Instance = this;

            var buttonCom = GetComponent<Button>();
            if (buttonCom == null)
                buttonCom = gameObject.AddComponent<Button>();

            buttonCom.onClick.AddListener(OnClickPlayerList);
        }

        private void OnClickPlayerList()
        {
            Records.Instance.Open();
        }

        //private void Start()
        //{
        //}

        //private void Update()
        //{
        //    if(Input.GetKeyUp(KeyCode.A))
        //    {
        //        AllClearitem();
        //        SeatPlayer(TestRandomPlayer());
        //    }
        //}

        private void OnEnable()
        {
            GameManager.OnBoardUpdated += BindPlayerAvatar;
            //GameManager.OnCardOpen += PlayerWinning;
        }

        private void OnDisable()
        {
            GameManager.OnBoardUpdated -= BindPlayerAvatar;
            //GameManager.OnCardOpen -= PlayerWinning;
        }

        public void PlayWinning(int accountId, double amount)
        {
            var findAvatar = GetAvatarData(accountId);
            if (findAvatar.HasValue == false)
                return;

            findAvatar.Value.avatarItem.PlayWinning(amount);
        }

        public void PlayBetting(int accountId)
        {
            var findAvatar = GetAvatarData(accountId);
            if (findAvatar.HasValue == false)
                return;

            findAvatar.Value.avatarItem.PlayBetting();
        }

        //앉자있는 플레이어 accountId로 PlayerInfo 를 얻는다.
        public PlayerThumbnail GetAccountInfo(int accountId)
        {
            var avatarData = GetAvatarData(accountId);
            if (avatarData.HasValue == false)
                return null;

            return avatarData.Value.avatarItem.playerInfo;
        }

        //앉자있는 플레이어 accountId로 위치를 얻는다.
        public bool GetAvatarPosition(int accountId, out RectTransform rt)
        {
            rt = null;

            var avatarData = GetAvatarData(accountId);
            if (avatarData.HasValue == false)
                return false;

            rt = avatarData.Value.avatarItem.GetComponent<RectTransform>();
            return true;
        }

        public bool GetSpecialPlayer(int accountId, out E_ItemType type)
        {
            type = E_ItemType.None;
            AvatarData? findData;
            if (false == GetSpecialPlayer(accountId, out findData))
                return false;

            type = findData.Value.itemType;
            return true;
        }

        public bool GetSpecialPlayer(int accountId, out AvatarData? data)
        {
            data = null;
            var avatarData = GetAvatarData(accountId);
            if (avatarData.HasValue == false)
                return false;

            if (avatarData.Value.itemType == E_ItemType.None)
                return false;

            data = avatarData;
            return true;
        }

        private AvatarData? GetAvatarData(int accountId)
        {
            var findData = lstAvatarData.Where(d => d.avatarItem.IsSeatPlayer() == true && d.avatarItem.playerInfo.AccountInfo.AccountId == accountId);
            if (findData.Any() == false)
                return null;

            return findData.First();
        }

        private void BindPlayerAvatar(Protocol.NotifyHandStateBoardUpdateIn res, bool isSnap)
        {
            AllClearitem();

            SeatPlayer(res);
            //SeatPlayer(TestRandomPlayer());
        }

        private void PlayerWinning(NotifyHandStateCardOpenIn pushData, bool fromShapshot)
        {
            //테스트 코드
            //TestWinningPlayer();
        }

        private void SeatPlayer(NotifyHandStateBoardUpdateIn data)
        {
            if (data.PlayersRank.ContainsKey(PlayerType.Supernova) == true)
            {
                var item = GetItemByType(E_ItemType.Supernova);
                item.SetData(data.PlayersRank[PlayerType.Supernova]);
            }

            if (data.PlayersRank.ContainsKey(PlayerType.GodBrain) == true)
            {
                var item = GetItemByType(E_ItemType.GodBrain);
                item.SetData(data.PlayersRank[PlayerType.GodBrain]);
            }

            //if (data.Supernova != null)
            //{
            //    var item = GetItemByType(E_ItemType.Supernova);
            //    item.SetData(data.Supernova);
            //}

            //if (data.GodBrain != null)
            //{
            //    var item = GetItemByType(E_ItemType.GodBrain);
            //    item.SetData(data.GodBrain);
            //}

            for (PlayerType type = PlayerType.TopPlayer1; type <= PlayerType.TopPlayer6; ++type)
            {
                if (data.PlayersRank.ContainsKey(type) == true)
                {
                    var item = NextEmptySlot();
                    if (item != null)
                        item.SetData(data.PlayersRank[type]);
                }
            }

            //if (data.TopPlayers != null)
            //{
            //    foreach (var player in data.TopPlayers)
            //    {
            //        var item = NextEmptySlot();
            //        if (item != null)
            //            item.SetData(player);
            //    }
            //}
        }

        private PlayerAvatarItem GetItemByType(E_ItemType type)
        {
            var findData = lstAvatarData.Find(d => d.itemType == type);
            return findData.avatarItem;
        }

        //순차적으로 비어있는 슬롯을 넘겨준다.
        private PlayerAvatarItem NextEmptySlot()
        {
            var item = lstAvatarData.Where(d => (d.itemType == E_ItemType.None && d.avatarItem.IsSeatPlayer() == false));
            if (item.Any() == false)
                return null;

            return item.First().avatarItem;
        }

        private void AllClearitem()
        {
            foreach (var item in lstAvatarData)
            {
                item.avatarItem.Clear();
            }
        }

        private void TestWinningPlayer()
        {
            var findAvatars = lstAvatarData.Where(d => d.avatarItem.IsSeatPlayer() == true);

            if (findAvatars.Any() == false)
                return;

            var lstPlayer = findAvatars.ToList();
            int count = UnityEngine.Random.Range(0, lstPlayer.Count);

            for (int index = 0; index < 0; ++index)
            {
                lstPlayer[index].avatarItem.PlayWinning(UnityEngine.Random.Range(100, 99999));
            }

        }

        //private NotifyHandStateBoardUpdateIn TestRandomPlayer()
        //{
        //    var handStart = new NotifyHandStateBoardUpdateIn();
        //     //int seatGod = UnityEngine.Random.Range(0, 2);
        //     //if (seatGod >= 1)
        //         handStart.GodBrain = GetTestPlayerInfo();

        //     int seatSupernova = UnityEngine.Random.Range(0, 2);
        //     //if (seatSupernova >= 1)
        //         handStart.Supernova = GetTestPlayerInfo();

        //     int randomCount = UnityEngine.Random.Range(0, 6);
        //     handStart.TopPlayers = new List<PlayerThumbnail>();
        //     for (int index = 0; index < randomCount; ++index)
        //     {
        //         handStart.TopPlayers.Add(GetTestPlayerInfo());
        //     }

        //    return handStart;
        //}

        private PlayerThumbnail GetTestPlayerInfo()
        {
            string[] nicknames = new string[] { "QWWE123", "sdfsedf", "KOREA123", "ka12n3Danma", "Ka3123DRanm", "USA123" };
            string[] avatarUrl = new string[] { "http://www.city.kr/files/attach/images/238/409/717/007/f0cd03797244d65fcc46742d9c54aff1.png",
                                                "http://www.city.kr/files/attach/images/238/409/717/007/6d8e26b66c50c11869738e87dad59839.png",
                                                "http://www.city.kr/files/attach/images/238/409/717/007/f2ade6f074547b4216eb6c40ae4dd50f.png",
                                                "http://www.city.kr/files/attach/images/238/409/717/007/5ae100a1ceaf97d8d032d076f7480fbf.png",
                                                "http://www.city.kr/files/attach/images/238/409/717/007/f0cd03797244d65fcc46742d9c54aff1.png",
                                                "http://www.city.kr/files/attach/images/238/409/717/007/c685c583a9f56126605846f6b47d9171.png"};

            int nickRandomIdx = UnityEngine.Random.Range(0, nicknames.Length);
            int avatarUrlIdx = UnityEngine.Random.Range(0, avatarUrl.Length);

            var accountInfo = new AccountInfo
            {
                AccountId = UnityEngine.Random.Range(123, 500),
                NickName = nicknames[nickRandomIdx],
                Avatar = avatarUrl[avatarUrlIdx],
            };

            return new PlayerThumbnail { AccountInfo = accountInfo };
        }

        private IEnumerator TestNotiBettingPlayer()
        {
            while(true)
            {
                var seatPlayers = lstAvatarData.Where(d => d.avatarItem.IsSeatPlayer() == true);

                if(seatPlayers.Any() == true)
                {
                    var bettingPlayer = seatPlayers.ToList();

                    foreach (var player in bettingPlayer)
                    {
                        var isBetting = UnityEngine.Random.Range(0, 5);

                        if(isBetting > 2)
                            player.avatarItem.PlayBetting();
                    }
                }

                yield return new WaitForSecondsRealtime(0.5f);
            }
        }
    }
}