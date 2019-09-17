using System;
using UnityEngine;
using Common.Scripts.Managers;
using Common.Scripts.Define;

namespace Bebop
{
    public class ExecutorOpenURL : ExecutorFrame
    {
        public override void Execute(E_LinkType linkType)
        {
            string url = "";
            bool inAppBrowser = false;
            bool hasToken = false;
            bool hasUrl = global::Common.Scripts.Utils.GameWebLinkHelper.GetUrl(linkType, out url, out inAppBrowser, out hasToken);

            if (hasUrl == false)
                return;

#if !BEBOP
            if (hasToken == true)
            {

                //Lobby.WaitIndicator.SetActive(true);
                UI.WaitIndicator.SetActive(true);
                Oasis.Scripts.Network.OasisHttp.LobbyClient.GetUrlToken((res) =>
                {
                    UI.WaitIndicator.SetActive(false);
                    var combineURL = global::Common.Scripts.Utils.GameWebLinkHelper.GetCombineTokenURL(url, res.Token);
                    global::Common.Scripts.Utils.GameWebLinkHelper.OpenWebBrowser(inAppBrowser, combineURL);
                },
                (error) =>
                {
                    BebopMessagebox.Ok("", error.DebugMessage);
                    UI.WaitIndicator.SetActive(false);
                });
            }
            else
            {
                global::Common.Scripts.Utils.GameWebLinkHelper.OpenWebBrowser(inAppBrowser, url);
            }
#endif
        }
    }
}