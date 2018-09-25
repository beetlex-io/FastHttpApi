using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi.WebSockets
{
    public class WebSocketToken
    {
        public WebSocketToken()
        {

        }

        public HttpRequest ConnectionRequest { get; set; }

        public bool Connected { get; set; }


    }
}
