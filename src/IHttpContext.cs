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

        void ResultToWebSocket(object data, HttpRequest request);

        void ResultToWebSocket(object data, Func<ISession, HttpRequest, bool> filter = null);

        void Async();

        bool WebSocket { get; }

        IDataContext Data { get; }
    }
}
