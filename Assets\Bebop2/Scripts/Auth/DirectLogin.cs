using Bebop.Networking.WebSocket;
using Bebop.Protocol;
using Common.Scripts;
using UnityEngine;

namespace Bebop
{

    public class DirectLogin : MonoBehaviour
    {

        [SerializeField]
        public bool NeedDirectLogin ;

        [SerializeField]
        public string BandId ="SWT";

        [SerializeField]
        public string ID = "f600";
        [SerializeField]
        public string PWD ="1";
        [SerializeField]
        public string LoginToken ;

        [SerializeField]
        public string AccessToken ;



        [SerializeField]
        public ClientDetail ClientDetail = new ClientDetail();
    }

}