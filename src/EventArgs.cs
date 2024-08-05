using BeetleX.Buffers;
using BeetleX.EventArgs;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi
{
    public delegate void EventHttpServerLog(IServer server, BeetleX.EventArgs.ServerLogEventArgs e);

    public class ActionExecutedArgs
    {
        public string ServerType { get; set; }

        public string Url { get; set; }

        public string HTTPMethod { get; set; }

        public ActionHandler ActionHandler { get; set; }

        public IDictionary<string, string> Headers { get; set; }

        public long UseTime { get; set; }

        public DateTime Time { get; set; }

        public int Code { get; set; } = 200;

        public Exception Exception { get; set; }

    }

    public class HttpServerLogEventArgs : BeetleX.EventArgs.ServerLogEventArgs
    {
        public HttpServerLogEventArgs(object tag, string message, LogType type, ISession session = null)
            : base(message, type, session)
        {
            Tag = tag;
        }
        public object Tag { get; private set; }

        public bool OutputConsole { get; set; } = true;

        public bool OutputFile { get; set; } = true;
    }


    public struct EventHttpResponsedArgs
    {
        public EventHttpResponsedArgs(HttpRequest request, HttpResponse response, double time, int status, string statusMsg)
        {
            Request = request;
            Response = response;
            Time = time;
            Status = status;
            StatusMessage = statusMsg;
        }

        public HttpRequest Request { get; set; }

        public HttpResponse Response { get; set; }

        public double Time
        {
            get; set;
        }

        public int Status { get; set; }

        public string StatusMessage { get; set; }
    }

    public class WebSocketConnectArgs : System.EventArgs
    {
        public WebSocketConnectArgs(HttpRequest request, HttpResponse response)
        {
            Request = request;
            Response = response;
            Cancel = false;

        }

        public UpgradeWebsocketError Error { get; set; }

        public IResult UpgradeSuccess { get; set; }

        public HttpResponse Response { get; internal set; }

        public HttpRequest Request { get; internal set; }

        public bool Cancel { get; set; }
    }

    public class EventControllerInstanceArgs : System.EventArgs
    {
        public Type Type { get; internal set; }

        public object Controller { get; set; }

        public IHttpContext Context { get; internal set; }


    }

    public class EventParameterBinding : System.EventArgs
    {
        public Type Type { get; internal set; }

        public object Parameter { get; set; }

        public IHttpContext Context { get; internal set; }

        public ActionHandler ActionHandler { get; internal set; }
    }

    public class EventActionExecutingArgs : System.EventArgs
    {


        public ActionHandler Handler { get; internal set; }

        public IHttpContext HttpContext { get; internal set; }

        public bool Cancel { get; set; }
    }


    public class EventHttpRequestArgs : System.EventArgs
    {
        public HttpRequest Request { get; internal set; }

        public HttpResponse Response { get; internal set; }

        public bool Cancel { get; set; }
    }

    public class EventHttpInnerErrorArgs : EventHttpRequestArgs
    {
        public string Code { get; internal set; }

        public string Message { get; internal set; }

        public Exception Error { get; internal set; }

    }

    public class EventHttpServerStartedArgs : System.EventArgs
    {
        public HttpApiServer HttpApiServer { get; internal set; }
    }

    public class EventOptionsReloadArgs : System.EventArgs
    {
        public HttpApiServer HttpApiServer { get; internal set; }

        public HttpOptions HttpOptions { get; internal set; }
    }

    public class EventActionRegistingArgs
    {
        public bool Cancel { get; set; } = false;

        public HttpApiServer Server { get; internal set; }

        public ActionHandler Handler { get; internal set; }

        public string Url { get; set; }
    }

}
