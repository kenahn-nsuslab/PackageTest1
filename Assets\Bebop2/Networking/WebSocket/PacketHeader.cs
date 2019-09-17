using System;
using System.Threading;
using Common.Scripts.Network.Tcp.Serialize;

namespace  Bebop.Networking.WebSocket
{

    #region  ===== old uint24 =====
    // /// <summary>
    // /// 서버쪽에서 제공해준 uint24
    // /// </summary>
    //  [StructLayout(LayoutKind.Sequential, Pack = 1)]
    // public struct UInt24 {
    //     private byte _b0;
    //     private byte _b1;
    //     private byte _b2;
    //     public UInt24(UInt32 value)
    //     {
    //         _b0 = (byte)(value & 0xFF); 
    //         _b1 = (byte)(value >> 8); 
    //         _b2 = (byte)(value >> 16);
    //     }

    //     public static byte[] GetBytes(UInt24 ui24)
    //     {
    //         byte[] ret = new byte[3];
    //         var value = ui24.Value;

    //         ret[0] = (byte) value;
    //         ret[1] = (byte)(value >> 8);
    //         ret[2] = (byte)(value >> 16);

    //         return ret;

    //     }

    //     public  static UInt24 GetValue(byte[] value, int startIndex)
    //     {
            
    //         int temp =  value[0] | value[1] <<8 | value[2] <<16;
    //         return new UInt24((UInt32)temp);
    //     }

    //     public  byte[] GetBytes()
    //     {
            
    //         return GetBytes(this);
    //     }

    //     public System.UInt32 Value
    //     {
    //         get { return (uint)_b0 | ( (uint)_b1 << 8 ) | ( (uint)_b2 << 16 ); }
    //     }
    //     public static bool operator == (UInt24 lhs, UInt24 rhs) => lhs.Value == rhs.Value;
    //     public static bool operator != (UInt24 lhs, UInt24 rhs) => !( lhs == rhs );
    //     public static UInt24 Zero = new UInt24(0);
    //     public override bool Equals(object obj) => obj != null && this == (UInt24) obj;

    //     public override int GetHashCode()
    //     {
    //         unchecked
    //         {
    //             int hash = 13 ^ _b0.GetHashCode();
    //             hash = ( hash * 17 ) ^ _b1.GetHashCode();
    //             return ( hash * 17 ) ^ _b2.GetHashCode();
    //         }
    //     }
    // }
    #endregion  


    [Flags]
    public enum SerializationModes : byte
    {
        JsonSerialization = 0x00,          // default: JsonSerialization, NoZip, NoEncryption
        ProtoBufSerialization = 0x01,
        ZipCompression = 0x04,
        TeeEncryption = 0x10,
    };

    /// <summary>
    /// 기존 해더 24byte 를 16으로 줄임
    /// Assets.Scripts.Network.Tcp.Protocol.PacketDefaultHeader 클래스를 수정함.
    /// by ken
    /// </summary>
    public class PacketHeader : IByteArraySerializableObject
    {
        
        public const int CurrentFixedHeaderSize = 16;

        public const UInt16 MagicKey = 0xFEFE; //임시
        private static int _packetId = 1;

        public static int NewPacketId()
        {
            // remove lock
            return Interlocked.Increment(ref _packetId);
        }

    
        private static Random saltRandom = new Random();

        public static UInt16 GetSalt()
        {
            return Convert.ToUInt16( saltRandom.Next(1,65000) ) ;
        }

        /// <summary>
        /// 랜덤값
        /// </summary>
        /// <value></value>
        public UInt16 Salt { get; set; }

        /// <summary>
        /// MagickKey
        /// </summary>
        /// <value></value>
        public UInt16 Magic { get; set; }

        /// <summary>
        /// 페이로드 사이즈
        /// </summary>
        /// <value></value>
        public UInt16 PayloadSize { get; set; }

        /// <summary>
        /// 프로토콜 아이디
        /// </summary>
        /// <value></value>
        public UInt16 ProtocolId { get; set; }


        /// <summary>
        /// 페킷 아이디
        /// </summary>
        /// <value></value>
        public int PacketId { get; set; }

        /// <summary>
        /// 요청/응답 쌍을 구분하기 위한 용도 
        /// 현재 패킷이 어떤 요청 패킷과 과련된 건지 설정된다.
        /// </summary>
        /// <value></value>
        public int RelatesTo { get; set; }

        /// <summary>
        /// Serailize 옵션 플래그, 플래그 연산으로 현재 
        /// </summary>
        /// <value></value>
        public Byte SerializeMode { get; set; }
        public Byte Reserved1 { get; set; }


        public PacketHeader()
		{
            Salt = 0;
			PayloadSize = 0;
			ProtocolId = 0;
			Magic = MagicKey;
			SerializeMode = 0;
			PacketId = 0;
			RelatesTo = 0;
		}

        public PacketHeader(UInt16 payloadSize, UInt16 protocolId, byte serializeMode =0)
        {
            Salt = GetSalt();
            Magic = MagicKey;
            PayloadSize = payloadSize;
            ProtocolId =  protocolId;
            SerializeMode = serializeMode; //필요한 경우 Flag 연산으로...값을 셋팅하거나 읽는다.
            PacketId = NewPacketId();
            
            Reserved1 = 0;

        }

        public bool IsValid
        {
            get { return Magic == MagicKey; }
        }
    
    

        public byte[] Serialize()
        {
            var buffer = new byte[CurrentFixedHeaderSize];
            Buffer.BlockCopy(BitConverter.GetBytes(Salt), 0, buffer, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(Magic), 0, buffer, 2, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(PayloadSize), 0, buffer, 4, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(ProtocolId), 0, buffer, 6, 2);

            
            Buffer.BlockCopy(new UInt24((uint)PacketId).GetBytes(), 0, buffer, 8, 3);
            Buffer.BlockCopy(BitConverter.GetBytes(SerializeMode), 0, buffer,11, 1);
            Buffer.BlockCopy(new UInt24((uint)RelatesTo).GetBytes(), 0, buffer, 12,3);
            
            

            ThrowIfNotValidateHeader();

            return buffer;
        }

        public void Deserialize(byte[] byteArray)
        {
            Salt = BitConverter.ToUInt16(byteArray,0);
            Magic = BitConverter.ToUInt16(byteArray,2);
            PayloadSize = BitConverter.ToUInt16(byteArray,4);
            ProtocolId = BitConverter.ToUInt16(byteArray,6);
            PacketId = (int)UInt24.GetValue(byteArray,8); //UInt24 는 요렇게..
            SerializeMode = byteArray[11];
            RelatesTo =(int) UInt24.GetValue(byteArray,12);

        }

        private void ThrowIfNotValidateHeader()
		{
			// *** check, invalid packet header ***

			// *** all exceptions are INVALID DATA EXCEPTION

			// Body size is must 0 ~ 1 Mega byte. (1024 Kilo byte, 1,048,576 byte)
			if (PayloadSize < 0)
				throw new Exception("Payload (body) size must greater than or equal to 0.");

            //요 아래쪽은 서버쪽에서 쓰는거인듯 하여 주석처리 함 - by ken
			// if (PayloadSize > SocketSetting.MaxAllowBlockSize)
			// 	throw new Exception(string.Format("Payload (body) size is too large. (Max allow byte is {0}.)", SocketSetting.MaxAllowBlockSize));

			// // Protocol id is must 1 ~ 4999. (not start 0)
			// if (ProtocolId < SocketSetting.MinAllowProtocolId)
			// 	throw new Exception(string.Format("Protocol id not less than {0}.", SocketSetting.MinAllowProtocolId));
			// if (ProtocolId > SocketSetting.MaxAllowProtocolId)
			// 	throw new Exception(string.Format("Protocol id can not be greater than {0}.", SocketSetting.MaxAllowProtocolId));

			if (false == IsValid)
				throw new Exception("Current processing header is invalid.");

			// 0 or 1
			// if (SerializeMode != (int)PacketSerializeMode.ProtoBuf &&
			// 	SerializeMode != (int)PacketSerializeMode.JsonText)
			// 	throw new Exception(string.Format("[{0}] serialize mode is not support.", SerializeMode));


            //TODO : 시리얼라즈 모드는 현재 default 임. 바뀌는 경우 검사방식을 바꿔야 함.
            if (SerializeMode != 0)
                throw new Exception(string.Format("serialize mode is not valid ,value: {0}",SerializeMode));

			// Not use 0 and minus.
			if (PacketId <= 0)
				throw new Exception("Packet id is must greater than 0.");

			// Not use minus.
			if (RelatesTo < 0)
				throw new Exception("Relates to is must greater than or equal to 0.");
		}
    }
}
