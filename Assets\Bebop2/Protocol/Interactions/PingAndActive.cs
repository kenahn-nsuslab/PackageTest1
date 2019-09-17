using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bebop.Protocol
{
    public class PingRequest 
   {
       public DateTime RequestAt;
   }

   public class PingResponse : Response
   {
       public ResultCode Result { get; set; }

       public DateTime Current;
       public DateTime RequestAt;
   }

   public class ActivatePlayerRequest 
   {
   }

   public class ActivatePlayerResponse : Response
   {
       public ResultCode Result { get; set; }
       public PlayerBetState  MyInfo { get; set; }

       public HandSnapShot HandSnapShot { get; set; }
   }

   public class DeactivatePlayerRequest 
   {
   }

   public class DeactivatePlayerResponse : Response
   {
       public ResultCode Result { get; set; }
   }

}