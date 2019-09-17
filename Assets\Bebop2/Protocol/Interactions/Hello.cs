using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Bebop.Protocol
{
    public class Hello : Response
    {
        // Start is called before the first frame update
        public ResultCode Result { get;set; }

        public string Message = string.Empty;
    }

}
