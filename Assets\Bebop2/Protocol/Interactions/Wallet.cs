using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bebop.Protocol
{
    public class WalletRequest 
    {
    }

    public class WalletResponse : Response
    {
        public ResultCode Result { get; set; }

        public Wallet MainWallet { get; set; }
    }
}

