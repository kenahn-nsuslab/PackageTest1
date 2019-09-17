using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bebop.Protocol
{
    public class CheckInRequest 
    {
        public string Token;
    }

    
    public class CheckInResponse : Response
    {
        public ResultCode Result { get; set; } 

        public PlayerBetState MyInfo { get; set; }

        public Wallet MainWallet { get; set; }

        public HandSnapShot HandSnapShot { get; set; }
    }

    public class CheckOutRequest
    {

    }

    public class CheckOutResponse : Response
    {
        public ResultCode Result { get; set; }

        public DisconnectType DisconnectType { get; set; }
    }


}