using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi.WebSockets
{
    public interface IWebSocketServer
    {
        EventHandler<WebSocketReceiveArgs> WebSocketReceive { get; set; }

        DataFrame CreateDataFrame(object body = null);

        void SendDataFrame(DataFrame data);

        void SendDataFrame(DataFrame data, params long[] sessionid);

        void SendDataFrame(DataFrame data, ISession session);
    }
}
