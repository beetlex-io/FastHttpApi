using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi
{
    public class HttpToken
    {
        public HttpToken()
        {
            FirstRequest = true;
        }

        public bool KeepAlive { get; set; }

        internal StaticResurce.FileBlock File { get; set; }

        public bool WebSocket { get; set; }

        public HttpRequest WebSocketRequest { get; set; }

        public bool FirstRequest { get; set; }
    }
}
