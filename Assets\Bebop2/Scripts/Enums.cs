using System;

namespace Bebop
{

    public enum NetworkStatus
    {
        None =0 ,
        
        Connected ,

        Disconnected,
 
        Retring ,    //재접속중

        Initialized  //CheckIn까지 성공


    }
}