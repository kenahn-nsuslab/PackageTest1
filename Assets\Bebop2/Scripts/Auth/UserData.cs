using Bebop.Protocol;
using Common.Scripts.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bebop
{
    public class UserData : SingletonClass<UserData>
    {
        public AccountInfo AccountInfo { get; set; }
        public int AccountId { get { return AccountInfo.AccountId; } }
        public Protocol.Wallet Wallet { get; set; }
    } 
}
