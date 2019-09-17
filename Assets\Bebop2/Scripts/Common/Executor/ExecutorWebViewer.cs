using System;
using UnityEngine;
using Common.Scripts.Managers;

namespace Bebop
{
    public class ExecutorWebViewer : ExecutorFrame
    {
        public override void Execute(bool isFlag, string value)
        {
#if UNITY_IPHONE || UNITY_ANDROID
            if(isFlag == true)
                Bebop.UI.BebopWebViwer.Instance.OpenWebview(value);
            else
                Application.OpenURL(value);
#elif (UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN ) && !BEBOP
            if (isFlag == true)
                Bebop.UI.BebopStandaloneWebView.Instance.OpenWebView(value);
            else
                Application.OpenURL(value);
#endif
        }
    }
}