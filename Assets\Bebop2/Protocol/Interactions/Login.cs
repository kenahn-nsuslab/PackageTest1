using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bebop.Protocol
{
    public class DirectLoginRequest 
    {
        public string BrandId { get; set; }

        // ----------------------------------------------
        // now we have 4 kinds of login types
        // ----------------------------------------------
        // id,password login
        //      new { Username = "gshock", Password="1" }
        // token from poker server
        //      new { Token = "token" }
        // -=<<NEW>>=- auth_code from OAuth2
        //      new { Code = "auth_code_from_brand" }
        // -=<<NEW>>=- accessTokens from previous 'auth_code login';
        //      new { AccessToken = "accessToken" }
       
        public string Token { get; set; }
       
        public ClientDetail ClientDetail { get; set; }
        public int RememberMeMinutes { get; set; } = 20160; // 2 Weeks by default
    }

    public class DirectLoginResponse : Response
    {
        
        public ResultCode Result { get; set; }

        public string GameToken { get; set; }
    }

    [SerializeField]
    public class ClientDetail
    {
       
        public string ClientId { get; set; }
        public string ClientType { get; set; }
        public string ClientVersion { get; set; }
        public string Address { get; set; }
        public string Locale { get; set; }
        
        public string OsType { get; set; }
       
        public string OsVersion { get; set; }
        
        public string DeviceType { get; set; }
       
        public string DeviceModel { get; set; }
        public string DeviceId { get; set; }
        public int SystemMemorySize { get; set; }

        // btag: http://docskr.sknow.net/pages/viewpage.action?pageId=31867802#id-3.1.1GPServiceAPIForFishingGameServer-2.1Login
        public string btag1 { get; set; }
        public string btag2 { get; set; }
        public string btag3 { get; set; }
        public string btag4 { get; set; }
        public string btag5 { get; set; }

        // public ClientOsType ClientOsType()
        // {
        //     // // ex) iPhone: ClientDetail
        //     // //            Locale: Korean
        //     // //            OsType: Other
        //     // //            OsVersion: iOS 12.1.4
        //     // //            ClientId: xxxx
        //     // //            ClientType: Handheld
        //     // //            ClientVersion: 1.4.7
        //     // //            DeviceId: xxxx
        //     // //            DeviceType: Handheld
        //     // //            DeviceModel: iPhone9,4
        //     // //            SystemMemorySize: yyyy

        //     // if (string.IsNullOrEmpty(OsVersion)) return Type.ClientOsType.Any;

        //     // if (OsVersion.Trim().ToLower().StartsWith("ios")) return Type.ClientOsType.IOS;
        //     // if (OsVersion.Trim().ToLower().StartsWith("and")) return Type.ClientOsType.Android;
        //     // if (OsVersion.Trim().ToLower().StartsWith("mac")) return Type.ClientOsType.Mac;
        //     // if (OsVersion.Trim().ToLower().StartsWith("win")) return Type.ClientOsType.Windows;
        //     return Type.ClientOsType.Any;
        // }
   }

    public enum ClientOsType
    {
        Any,
        IOS,
        Android,
        Windows,
        Mac,
        Linux
    }
    
}