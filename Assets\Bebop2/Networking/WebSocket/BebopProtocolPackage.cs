using System;
using Common.Scripts.Network.Tcp.PacketPackage;

namespace  Bebop.Networking.WebSocket
{
    /// <summary>
    /// RhymeTcpProtocolPackage 를 수정함.
    /// TODO: PacketHeader 를 injection 하도록 해서 재활용이 가능하도록 할 필요가 있음. by Ken
    /// </summary>
    public class BebopProtocolPackage : IProtocolPackage
    {
    // Start is called before the first frame update

        private readonly PacketHeader _currentProcessingHeader = new PacketHeader();
        private readonly IPacketExtractor _headerExtractor = new FixedPacketExtractor(PacketHeader.CurrentFixedHeaderSize);
        private readonly IPacketExtractor _bodyExtractor = new FixedPacketExtractor(0);

        // complete handler
        private event Action<PacketHeader, byte[]> PacketBodyParseCompletedHandlerCallback;
        private event Action<PacketHeader> PacketHeaderReadCompletedHandlerCallback;

        /// <summary>
        /// Assets.Scripts.Network.Tcp.Utils.ByteHelper 메서드 이전함.
        /// </summary>
        /// <param name="firstBytes"></param>
        /// <param name="secondBytes"></param>
        /// <returns></returns>
        public static byte[] MergeBytes(byte[] firstBytes, byte[] secondBytes)
		{
			// combine first bytes and second bytes.
			var buffer = new byte[PacketHeader.CurrentFixedHeaderSize + secondBytes.Length];
			Buffer.BlockCopy(firstBytes, 0, buffer, 0, firstBytes.Length);
			Buffer.BlockCopy(secondBytes, 0, buffer, firstBytes.Length, secondBytes.Length);

			return buffer;
		}

        public BebopProtocolPackage(Action<PacketHeader, byte[]> packetBodyParseCompletedHandlerCallback, Action<PacketHeader> packetHeaderReadCompletedHandlerCallback)
        {
            PacketBodyParseCompletedHandlerCallback += packetBodyParseCompletedHandlerCallback;
			PacketHeaderReadCompletedHandlerCallback += packetHeaderReadCompletedHandlerCallback;
        }

        public bool Process(int bytesTransferred, byte[] buffer)
		{
			var totalReceivedBytes = bytesTransferred;
			var remainingBytesToProcess = bytesTransferred;

			try
			{
				while (true)
				{
					if (_headerExtractor.IsDone && _bodyExtractor.IsDone)
					{
						Dispose();
						return false;
					}

					// NOTE : true  : parse ok. go next ProcessBody.
					// NOTE : false : get more body bytes. (stop this method. -> StartReceive(); again !)
					if (ProcessHeader(buffer, totalReceivedBytes, ref remainingBytesToProcess) == false)
						break;

					// NOTE : true  : parse ok. well done.
					// NOTE : false : something wrong.
					if (ProcessBody(buffer, totalReceivedBytes, ref remainingBytesToProcess))
						break;

					// NOTE : loop again
				}

				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

        private bool ProcessHeader(byte[] buffer, int totalReceivedBytes, ref int remainingBytesToProcess)
		{
			// start read header
			if (false == _headerExtractor.IsDone)
			{
				_headerExtractor.CurrentSocketBufferOffSet(totalReceivedBytes - remainingBytesToProcess);
				remainingBytesToProcess = _headerExtractor.Process(buffer, remainingBytesToProcess);

				if (_headerExtractor.IsDone)
				{
					// NOTE : deserialize then validate check.
					_currentProcessingHeader.Deserialize(_headerExtractor.GetBuffer());

					// start read body
					_bodyExtractor.Reset(_currentProcessingHeader.PayloadSize);

					if (null != PacketHeaderReadCompletedHandlerCallback)
					{
						PacketHeaderReadCompletedHandlerCallback(_currentProcessingHeader);
					}
				}
			}

			if (remainingBytesToProcess == 0 && _currentProcessingHeader.PayloadSize > 0)
				return false;

			// ex) Task<PingResponse> Ping();
			if (remainingBytesToProcess == 0 && _currentProcessingHeader.PayloadSize == 0 && _headerExtractor.IsDone)
				return true;

			return remainingBytesToProcess > 0;
		}

		private bool ProcessBody(byte[] buffer, int totalReceivedBytes, ref int remainingBytesToProcess)
		{
			// start read payload(body)
			_bodyExtractor.CurrentSocketBufferOffSet(totalReceivedBytes - remainingBytesToProcess);
			remainingBytesToProcess = _bodyExtractor.Process(buffer, remainingBytesToProcess);

			if (_bodyExtractor.IsDone)
			{
				try
				{
					if (PacketBodyParseCompletedHandlerCallback != null)
						PacketBodyParseCompletedHandlerCallback(_currentProcessingHeader, _bodyExtractor.GetBuffer());
				}
				catch (Exception)
				{
					// NOTE : not throw
				}

				// start read header
				_headerExtractor.Reset(PacketHeader.CurrentFixedHeaderSize);
			}

			return remainingBytesToProcess == 0;
		}

        public void Reset()
		{
			_headerExtractor.Reset(PacketHeader.CurrentFixedHeaderSize, true);
			_bodyExtractor.Reset(0, true);
		}

		public void Dispose()
		{
			Reset();
			PacketBodyParseCompletedHandlerCallback = null;
			PacketHeaderReadCompletedHandlerCallback = null;
        }
		
    }

}
