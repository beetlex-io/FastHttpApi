using BeetleX.FastHttpApi.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi
{
    public interface IHttpContext
    {
        HttpRequest Request { get; }

        HttpResponse Response { get; }

        HttpApiServer Server { get; }

        ISession Session { get; }

        object this[string name] { get; set; }

        object Tag { get; }

        void Result(object data);

        void SendToWebSocket(ActionResult data, HttpRequest request);

        void SendToWebSocket(ActionResult data, Func<ISession, HttpRequest, bool> filter = null);

        void SendToWebSocket(WebSockets.DataFrame data, HttpRequest request);

        void SendToWebSocket(WebSockets.DataFrame data, Func<ISession, HttpRequest, bool> filter = null);

        void Async();

        bool WebSocket { get; }

        IDataContext Data { get; }

        string ActionUrl { get; }
    }
}
