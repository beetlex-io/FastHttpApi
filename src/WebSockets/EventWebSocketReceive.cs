using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi.WebSockets
{


    public class WebSocketReceiveArgs : System.EventArgs
    {
        public ISession Sesson { get; internal set; }

        public HttpRequest Request { get; internal set; }

        public DataFrame Frame { get; internal set; }

        public IWebSocketServer Server { get; internal set; }

        public void Response(DataFrame data)
        {
            Sesson.Send(data);
        }

        public DataFrame CreateFrame(object body = null)
        {
            var result = Server.CreateDataFrame(body);
            return result;
        }

    }


}
