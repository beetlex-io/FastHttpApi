using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi.WebSockets
{
    public interface IWebSocketServer
    {
        EventHandler<WebSocketReceiveArgs> WebSocketReceive { get; set; }

        DataFrame CreateDataFrame(object body = null);

        void SendToWebSocket(DataFrame data, Func<ISession, HttpRequest, bool> filter = null);

        void SendToWebSocket(DataFrame data, params HttpRequest[] request);

        IList<HttpRequest> GetWebSockets();
    }

   
}
