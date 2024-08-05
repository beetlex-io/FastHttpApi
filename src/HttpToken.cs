using BeetleX.Buffers;
using BeetleX.EventArgs;
using BeetleX.FastHttpApi.WebSockets;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BeetleX.FastHttpApi
{
    public class HttpToken
    {
        public HttpToken()
        {
            FirstRequest = true;
            CreateTime = DateTime.Now;
            RequestArgs = new EventHttpRequestArgs();
        }

        internal BeetleX.Dispatchs.SingleThreadDispatcher<HttpApiServer.IOQueueProcessArgs> IOQueue { get; set; }

        internal EventHttpRequestArgs RequestArgs { get; set; }

        public bool KeepAlive { get; internal set; }

        internal StaticResurce.FileBlock File { get; set; }

        public bool WebSocket { get; internal set; }

        public HttpRequest Request { get; internal set; }

        internal bool FirstRequest { get; set; }

        public DateTime CreateTime { get; internal set; }

        public PipeStream WebSocketData { get; set; }

        public Newtonsoft.Json.JsonSerializer WebSocketJsonSerializer { get; set; }

        internal RpsLimit WSRpsLimit { get; set; }

        internal RpsLimit HttpRpsLimit { get; set; }

        internal DataPacketType WSLastPacketType { get; set; }
    }
}
