using Common.Scripts;
using Common.Scripts.Model;

namespace Bebop.Networking
{
    /// <summary>
    /// SocketModel 구현체
    /// Bind 메서드를 제공하여 프로토콜 메세지에 대한 콜백 호출이 가능하게 해준다.
    /// </summary>
    public class ProtocolBinder : SocketModel
    {
        // Start is called before the first frame update public ServerType TargetServerType {get; private set;}
    public ServerType TargetServerType {get; private set;}
        protected override string ConnectorName
        {   
            get
            {
                return "TestConnetion";
            }
        }

        public new void Connect(ServerType serverType)
        {
            TargetServerType = serverType;
            base.Connect(serverType);
        }
    }

}
