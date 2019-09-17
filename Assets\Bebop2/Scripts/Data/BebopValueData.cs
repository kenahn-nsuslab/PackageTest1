using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bebop
{
    [CreateAssetMenu]
    public class BebopValueData : ScriptableObject
    {
        [Serializable]
        public class BettingOdds
        {
            public Protocol.BettingType type;
            public float odds;
        }

        public BettingOdds[] bettingOdds;
    } 
}
