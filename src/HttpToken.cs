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
            CreateTime = DateTime.Now;
        }

        public bool KeepAlive { get; internal set; }

        internal StaticResurce.FileBlock File { get; set; }

        public bool WebSocket { get; internal set; }

        public HttpRequest WebSocketRequest { get; internal set; }

        internal bool FirstRequest { get; set; }

        public DateTime CreateTime { get; internal set; }
    }
}
