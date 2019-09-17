
using System;
using Common.Scripts.Network.Tcp.Socket;
using UniRx;
using UnityEngine;
using Common.Scripts.Utils;

namespace Bebop.Networking.WebSocket
{

    /// <summary>
    /// Assets.Scripts.Network.Tcp.Socket.SocketConnector 에 해당한다.
    /// Update 에서 task 를 디큐해서 소켓 메세지를 처리하는 동작을 구현함. by ken   
    /// 
    /// 기존 SocketModel 에서 제공하던 BindProtocol 메서드를 이곳으로 옮김. by ken
    /// </summary>
    public class SocketComponent : MonoBehaviour
    {
        private readonly SocketDataProvider socket = new SocketDataProvider();

		public SocketDataProvider.DispatchEventFunctor OnConnected
		{
			set { socket.OnConnectedCallback = value; }
		}

		public SocketDataProvider.DispatchEventFunctor OnDisconnected
		{
			set { socket.OnDisconnectedCallback = value; }
		}

		internal SocketDataProvider Socket
		{
			get { return socket; }
		}

		internal bool IsConnected
		{
			get { return socket != null && socket.IsConnected(); }
		}

		public bool IsLockNetworkTask
		{
			get { return socket.IsLockNetworkTask; }
			set { socket.IsLockNetworkTask = value; }
		}

		private IDisposable dispatcherUpdaterDisposable = null;
		// Update is called once per frame
		void Update()
		{
			if (IsConnected && IsLockNetworkTask)
			{
				if (null == dispatcherUpdaterDisposable)
				{
					dispatcherUpdaterDisposable = Observable.IntervalFrame(10).Subscribe(_ => socket.ExecuteTask()).AddTo(gameObject);
				}
			}
		}

		public void Connect(string ipAddress, SocketDataProvider.DispatchEventFunctor onConnected, SocketDataProvider.DispatchEventFunctor onDisconnected)
		{
			socket.Connect(ipAddress, onConnected, onDisconnected);
		}

        public void CloseTask()
        {
            if(socket != null)
            {
                socket.ClearTask();
            }
        }

		public void Close()
		{
			socket.Close();

			if (null != dispatcherUpdaterDisposable)
			{
				dispatcherUpdaterDisposable.Dispose();
				dispatcherUpdaterDisposable = null;
			}
		}


#region === 기존 SocketModel 역할 이전 ========

        public Action<byte[], object> ResponseCallBack = null;

        public void BindProtocol<T>(
            Enum protocolId,
            SocketCallbackBinder<T>.CurrentDispatchFunctor callbackBinder) where T : class
        {
            ISocketCallbackBinder binder;

            if(ResponseCallBack != null)
                binder = new SocketCallbackBinder<T> { DispatchFunctor = callbackBinder, ResponseCallBack = ResponseDispatchCallBack };
            else
                binder = new SocketCallbackBinder<T> { DispatchFunctor = callbackBinder};

            Socket.BindProtocol(protocolId.GetHashCode(), binder);
        }

        public void UnBindAll()
        {
            socket.UnbindProtocolAll();
        }

        public void Send(Enum protocolId, object request)
        {
            if (IsConnected == false)
            {
                //WaitIndicator.SetActive(false);
                //MessageBox.Instance.Ok(ConnectorName + " : currently not connected");

                // Scheduler.MainThread.Schedule(() =>
                // {
                //     OasisDebug.Log(
                //         ConnectorName + " : currently not connected", 
                //         OasisDebug.E_LogKey.Packet, 
                //         OasisDebug.E_Color.Red);
                    
                    
                //    //WaitIndicator.SetActive(false);
                // });
                
                return;
            }

            Scheduler.MainThread.Schedule(() =>
            {
                OasisDebug.Log(
                    string.Format(
                        "[TCP] ======> [{0}] ID [{1}]", request.GetType().Name, protocolId.ToString()), 
                    OasisDebug.E_LogKey.Packet
                );                
            });

            socket.Send((ushort)protocolId.GetHashCode(), request);

            Debug.Log( string.Format(
                        "[TCP] sent ======> [{0}] ID [{1}]", request.GetType().Name, protocolId.ToString()) );
        }

        public void ResponseDispatchCallBack<T>(byte[]payload, T res)  where T : class
        {
            if (ResponseCallBack != null)
                ResponseCallBack(payload, res);
        }


#endregion  == 기존 SocketModel 역할 =====

		public void OnApplicationQuit()
		{
			Logger.Log(new { Event = "OnApplicationQuit", Message = "App Close", Socket = (socket != null ? socket.ToString() : null) });
			Close();
		}
    }

    
}
