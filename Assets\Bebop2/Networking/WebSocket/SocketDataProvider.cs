using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using Common.Scripts.Network.Tcp.PacketPackage;
using Common.Scripts.Network.Tcp.Serialize;
using Common.Scripts.Network.Tcp.Socket;
using UniRx;
using UnityEngine;

namespace Bebop.Networking.WebSocket
{
    public class StateObject
    {
        public const int MaxBuffSize = 8192 * 16;

        public PacketHeader Header;
        public byte[] Buffer = new byte[MaxBuffSize];
    }

    internal enum DispatchResult
    {
        Error,
        ProtocolIdNotFound,
        Success,
    }
    
    /// <summary>
    /// 소켓 데이터 처리 작업
    /// Assets.Scripts.Network.Tcp.Socket.ClientSocket 을 변경함. by ken
    /// </summary>
    public class SocketDataProvider
    {
        enum ConnectEventType
        {
            OnConnected = 0,
            OnDisconnected = 1,
            MaxEvent = 2,
        }

        protected IProtocolPackage ProtocolPackage { get; set; }

        // connect/disconnect callback delegate
        public delegate void DispatchEventFunctor(int result);

        // Connect Events
        private readonly DispatchEventFunctor[] _callbackNetworkEventFunctor =
            new DispatchEventFunctor[(int) ConnectEventType.MaxEvent] {null, null};

        public DispatchEventFunctor OnConnectedCallback
        {
            set { _callbackNetworkEventFunctor[(int) ConnectEventType.OnConnected] = value; }
        }

        public DispatchEventFunctor OnDisconnectedCallback
        {
            set { _callbackNetworkEventFunctor[(int) ConnectEventType.OnDisconnected] = value; }
        }

        // socket impl
        private SocketClient _socket = null;

        // protobufserialize does not work
        private ISerializer _serializer = new NewtonJsonSerializer();

        // thread block 문제�?별도�?메인?�레?�에??polling??task queue
        private readonly Queue<ProtocolDispatchTask> _taskQueue = new Queue<ProtocolDispatchTask>();
        private readonly Queue<ProtocolDispatchTask> _taskIgnoreQueue = new Queue<ProtocolDispatchTask>();

        // I/O lock - Input�?output??별개???�레??루틴?�로 처리가 ?�으�?locking???�요?? 
        // ?�라?�언?�이므�???io lock??비용?� 무시 ?�도 ?�다.
        private readonly object _queueLock = new object();

        // dispatch impl
        private readonly Dictionary<Int32, ISocketCallbackBinder> _packetDispatchList =
            new Dictionary<Int32, ISocketCallbackBinder>();

        public SocketDataProvider()
        {
            ProtocolPackage = new BebopProtocolPackage(OnPacketBodyParseCompletedHandlerCallback,
                OnPacketHeaderReadCompletedHandlerCallback);
        }

        protected void OnPacketHeaderReadCompletedHandlerCallback(PacketHeader header)
        {
        }

        protected void OnPacketBodyParseCompletedHandlerCallback(PacketHeader header, byte[] readBytes)
        {
            EnqueueTask(header.ProtocolId, readBytes);
        }

        internal void BindProtocol(int protocolId, ISocketCallbackBinder callbackBinder)
        {
            if (true == _packetDispatchList.ContainsKey(protocolId))
            {
                return;
            }

            _packetDispatchList.Add(protocolId, callbackBinder);
        }

        internal void BindProtocol<TResponse>(int protocolId,
            SocketCallbackBinder<TResponse>.CurrentDispatchFunctor functor, bool isNotify = false)
            where TResponse : class
        {
            
            if (_packetDispatchList.ContainsKey(protocolId))
            {
                UnbindProtocol(protocolId);
            }

            _packetDispatchList.Add(protocolId,
                new SocketCallbackBinder<TResponse> {DispatchFunctor = functor, IsNotify = isNotify});
        }

        internal void UnbindProtocol(int protocolId)
        {
            if (true == _packetDispatchList.ContainsKey(protocolId))
            {
                _packetDispatchList.Remove(protocolId);
            }
        }

        internal void UnbindProtocolAll()
        {
            _packetDispatchList.Clear();
        }

        private readonly int[] _ignoreNetworPacket =
        {
/*
			(int)RhymeClientTableCallbackEnum.NotifyVoiceChat, 
			(int)RhymeClientTableCallbackEnum.NotifyTableChat, 
			(int)RhymeClientTableCallbackEnum.NotifyBubbleMessage, 
			(int)RhymeClientSessionCallbackEnum.NotifyBuddyMessage,
			(int)RhymeClientSessionEnum.Ping,
 **/
        };

        private bool IsIgnoreNetworkDelayPacket(int protocolId)
        {
            return _ignoreNetworPacket.FirstOrDefault(i => i == protocolId) != 0;
        }

        private void EnqueueTask(int protocolId, byte[] buf)
        {
            Debug.Log("Enqueue :"+protocolId.ToString());
            
            var task = new ProtocolDispatchTask(protocolId, buf);

            // queue?� 무�??�게 즉시 처리 ?�야 ??것들
/*
			if (protocolId == (int)RhymeClientSessionEnum.Ping)
			{
				MainThreadDispatcher.Post(() => ImmediateDispatchTask(task));
				return;
			}
/**/
            lock (_queueLock)
            {
                
                if (IsIgnoreNetworkDelayPacket(protocolId))
                {
                    _taskIgnoreQueue.Enqueue(task);
                }
                else
                {
                    _taskQueue.Enqueue(task);
                }
            }
        }

        public bool IsLockNetworkTask = true;

        public void ExecuteTask()
        {
            lock (_queueLock)
            {
                while (_taskIgnoreQueue.Count > 0)
                {
                    var task = _taskIgnoreQueue.Dequeue();
                    DispatchTask(task);
                }

                while (_taskQueue.Count > 0)
                {
                    if (false == IsLockNetworkTask)
                    {
                        return;
                    }

                    var task = _taskQueue.Dequeue();
                    DispatchTask(task);
                }
            }
        }

        private void ImmediateDispatchTask(ProtocolDispatchTask task)
        {
            DispatchTask(task);
        }
        IDisposable _timerdisposable;
        private void DispatchTask(ProtocolDispatchTask task)
        {
            if (task == null)
                return;

            var result = Dispatch(task.Command, task.Buffer);
            if (result == DispatchResult.ProtocolIdNotFound)
            {
                var refTask = task;
                _timerdisposable = Observable.Timer(TimeSpan.FromMilliseconds(100)).Subscribe(_ =>
                {
                    Debug.Log("Try one more for dispatch by protocol id not bind.(" + task.Command + ")");
                    //_taskQueue.Enqueue(refTask);
                });

                return;
            }
            else if (result == DispatchResult.Error)
            {
                Debug.LogFormat("Dispatch error: {0}", task.Command);
            }

            task.Dispose();
            task = null;
        }

        private DispatchResult Dispatch(Int32 protocolId, byte[] payload)
        {
            if (false == _packetDispatchList.ContainsKey(protocolId))
            {
                //Logger.Warn(new { Event = "clientsocket_dispatch_protocol_id_not_found", ProtocolId = protocolId, });
                return DispatchResult.ProtocolIdNotFound;
            }

            //if (Debug.isDebugBuild)
            //{
            //    string typeName = _packetDispatchList[protocolId].GetName();
            //    string json = Encoding.UTF8.GetString(payload);
            
            //    OasisDebug.Log(
            //        string.Format(
            //            "[TCP] <====== [{0}] ID [{1}]\n{2}", 
            //            typeName, protocolId, json),
            //        OasisDebug.E_LogKey.Packet,
            //        OasisDebug.E_Color.Green
            //    );                
            //}

            _packetDispatchList[protocolId].Dispatch(ref _serializer, payload);
            return DispatchResult.Success;
        }

        // TODO: need ?
        public bool HasProtocol(int protocolId)
        {
            if (false == _packetDispatchList.ContainsKey(protocolId))
            {
                return false;
            }

            return true;
        }

        private void OnError(Exception excetion)
        {
            Logger.Error(new {Event = "socket_exception", Exception = excetion,});
            Close();
        }

        private void OnClosed()
        {
            Logger.Log(new {Event = "socket_closed",});
            Close();
        }

        private void OnConnected(int result)
        {
            Logger.Log(new {Event = "socket_connected.", Message = "result=" + result,});
//			MainThreadDispatcher.Post(_ => ConnectCompleteInternal(result), null);
            ConnectCompleteInternal(result);
        }

        private void OnSent(int size)
        {
        }

        private void OnReceived(int dataLength, byte[] dataBytes)
        {
            if (ProtocolPackage.Process(dataLength, dataBytes) == false)
            {
                Logger.Error(new {Event = "socket_received", Message = "failed to process, length=" + dataLength,});
            }
        }


        /// <summary>
        /// TCP 연결시 사용할 것. 웹소켓에서는 사용금지
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <param name="callbackConnectResult"></param>
        /// <param name="callbackDisconnectResult"></param>
        public void Connect(String ip, int port, DispatchEventFunctor callbackConnectResult = null,
            DispatchEventFunctor callbackDisconnectResult = null)
        {
#if UNITY_WEBGL
			String address = String.Format("ws://{0}:{1}", ip, port);
#else
            String address = String.Format("tcp://{0}:{1}", ip, port);
#endif

            Uri uri = new Uri(address);
            ConnectInternal(uri, callbackConnectResult, callbackDisconnectResult);
        }

        /// <summary>
        /// tcp 혹은 웹소켓 연결
        /// </summary>
        /// <param name="address">"wss://아이피:포트" 혹은 "tcp://아이피:포트"형태 스트링</param>
        /// <param name="callbackConnectResult"></param>
        /// <param name="callbackDisconnectResult"></param>
        public void Connect(String address, DispatchEventFunctor callbackConnectResult = null,
            DispatchEventFunctor callbackDisconnectResult = null)
        {
            Uri uri = new Uri(address);
            ConnectInternal(uri, callbackConnectResult, callbackDisconnectResult);
        }

        private void ConnectInternal(Uri uri, DispatchEventFunctor callbackConnectResult,
            DispatchEventFunctor callbackDisconnectResult)
        {
            Debug.Log("ClientSocket.ConnectInternal, enter, uri=" + uri);

            if (null != _socket)
            {
                Close();
                _socket = null;
            }

            try
            {
                if (uri.Scheme.Equals("tcp"))
                {
                    _socket = new TcpSocketClient();
                }
                else if (uri.Scheme.Equals("ws"))
                {
                    _socket = new WebSocketClient();
                }
                else
                {
                    // TODO: exception
                    throw new Exception("invalid uri :"+ uri);
                    
                }

                _callbackNetworkEventFunctor[(int) ConnectEventType.OnConnected] = callbackConnectResult;
                _callbackNetworkEventFunctor[(int) ConnectEventType.OnDisconnected] = callbackDisconnectResult;

                _socket.OnError = OnError;
                _socket.OnConnected = OnConnected;
                _socket.OnClosed = OnClosed;
                _socket.OnSent = OnSent;
                _socket.OnReceived = OnReceived;

                bool ret = _socket.Connect(uri);
            }
            catch (SocketException e)
            {
                Logger.Error(new {Event = "connect_exception", Exception = e,});
            }
        }

        public bool IsConnected()
        {
            return _socket != null && _socket.IsConnected();
        }

        private void ConnectCompleteInternal(int result)
        {
//			if (result == (int)ConnectEventType.OnConnected)
//			{
//				Logger.Log(new
//				{
//					Event = "client_connected",
//					Direction = string.Format("{0} -> {1}", _sock.LocalEndPoint, _sock.RemoteEndPoint),
//				});
//			}

            if (_socket != null && _callbackNetworkEventFunctor[(int) ConnectEventType.OnConnected] != null)
            {
                _callbackNetworkEventFunctor[(int) ConnectEventType.OnConnected].Invoke(result);
            }
        }

        public void Send<T>(ushort protocolId, T requestObject) where T : class
        {
            if (IsConnected())
            {
                var serialize = _serializer.Serialize(requestObject);
                SendInternal(protocolId, serialize);
            }
            else
            {
                Logger.Log(new {Event = "send_failed.", Message = "_sock is null or disconnected.",});
            }
        }

        private void SendInternal(ushort protocolId, byte[] payload)
        {
            var header = new PacketHeader((ushort)payload.Length, protocolId, (int) SerializationModes.JsonSerialization);

            var buffer = BebopProtocolPackage.MergeBytes(header.Serialize(), payload);

            if (buffer != null)
            {
                _socket.Send(buffer, protocolId);
            }
        }

        public void Close()
        {
            //			MainThreadDispatcher.Post(_ => ShutDownInternal(), null);
            ClearTask();

            if (_timerdisposable != null)
                _timerdisposable.Dispose();

            ShutDownInternal();
        }

        public void ClearTask()
        {
            if (_taskIgnoreQueue != null)
                _taskIgnoreQueue.Clear();

            if (_taskQueue != null)
                _taskQueue.Clear();
        }

        private void ShutDownInternal()
        {
            if (_socket != null)
            {
                try
                {
                    // callback to callee
                    if (null != _callbackNetworkEventFunctor[(int) ConnectEventType.OnDisconnected])
                    {
                        ExecuteTask();
                        _callbackNetworkEventFunctor[(int) ConnectEventType.OnDisconnected].Invoke(0);
                    }

                    if (_socket.IsConnected())
                    {
                        _socket.Disconnect();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(new {Event = "ShutdownInternal exception", Message = ex.Message});
                }

                _socket = null;
            }
        }
    }

}
