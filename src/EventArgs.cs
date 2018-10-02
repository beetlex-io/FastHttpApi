using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi
{
    public delegate void EventHttpServerLog(IServer server, BeetleX.EventArgs.ServerLogEventArgs e);

    public class WebSocketConnectArgs : System.EventArgs
    {
        public WebSocketConnectArgs(HttpRequest request)
        {
            Request = request;
            Cancel = false;
        }

        public HttpRequest Request { get; internal set; }

        public bool Cancel { get; set; }
    }

}
