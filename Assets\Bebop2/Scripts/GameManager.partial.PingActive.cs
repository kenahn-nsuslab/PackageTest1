using System;
using Bebop.Model.EventParameters;
using Bebop.Protocol;
using Newtonsoft.Json;
using UnityEngine;

namespace Bebop
{

    /// <summary>
    /// 핑과 Active / Deactive 상태 이벤트
    /// </summary>
    public partial class GameManager : MonoBehaviour
    {

        public static void SendPing()
        {
            var dto = new PingRequest();
            dto.RequestAt = DateTime.UtcNow;

            client.Send(BebopProtocolId.Ping, dto);
        }

        public static event Action<PingResponse> OnPingResponse = delegate {};



        public static void SendAcivatePlayer()
        {
          
            var dto = new ActivatePlayerRequest();
            
            client.Send(BebopProtocolId.ActivatePlayer,dto);
       
         
        }

        public static event Action<ActivatePlayerResponse> OnActivatePlayerResponse = delegate {};

        public static void SendDeactivatePlayer()
        {
            var dto = new DeactivatePlayerRequest();
            client.Send(BebopProtocolId.DeactivatePlayer, dto);
            
        }

    
        public static event Action<DeactivatePlayerResponse> OnDeactivatePlayerResponse = delegate {};

        public void RaisePingResponse(PingResponse res)
        {
            var rtt = DateTime.UtcNow - res.RequestAt;
            SetServerTime(res.Current + TimeSpan.FromSeconds(rtt.TotalSeconds * 0.5f));

            OnPingResponse.Invoke(res);
        }

        public void RaiseActivatePlayerResponse(ActivatePlayerResponse res)
        {
            OnActivatePlayerResponse.Invoke(res);
        }

        public void RaiseDeactivatePlayerResponse(DeactivatePlayerResponse res)
        {
            OnDeactivatePlayerResponse.Invoke(res);
        }


        private void BindPingAndActiveStatus()
        {
            client.BindProtocol<PingResponse>(BebopProtocolId.Ping, RaisePingResponse);
            client.BindProtocol<ActivatePlayerResponse>(BebopProtocolId.ActivatePlayer, RaiseActivatePlayerResponse);
            client.BindProtocol<DeactivatePlayerResponse>(BebopProtocolId.DeactivatePlayer, RaiseDeactivatePlayerResponse);
        }
    }


}