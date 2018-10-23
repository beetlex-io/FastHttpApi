using BeetleX.FastHttpApi.Data;
using BeetleX.FastHttpApi.WebSockets;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi
{


    class WebsocketJsonContext : IHttpContext
    {
        public WebsocketJsonContext(HttpApiServer server, HttpRequest request, IDataContext dataContext)
        {
            Server = server;
            Request = request;
            AsyncResult = false;
            mDataContext = dataContext;
        }

        private Data.IDataContext mDataContext;

        public HttpRequest Request { get; set; }

        public HttpResponse Response { get; set; }

        public HttpApiServer Server { get; set; }

        public object Tag { get; set; }


        public string RequestID { get; set; }

        public void Result(object data)
        {
            WebSockets.DataFrame frame = data as WebSockets.DataFrame;
            if (frame == null)
            {
                ActionResult result = data as ActionResult;
                if (result == null)
                {
                    result = new ActionResult();
                    result.Data = data;
                }
                result.ID = RequestID;
                if (result.Url == null)
                    result.Url = this.ActionUrl;
                frame = Server.CreateDataFrame(result);
            }
            Request.Session.Send(frame);
        }

        internal bool AsyncResult { get; set; }

        public bool WebSocket => true;

        public IDataContext Data => mDataContext;

        public ISession Session => Request.Session;

        public string ActionUrl { get; internal set; }

        public object this[string name] { get => Session[name]; set => Session[name] = value; }

        public void Async()
        {
            AsyncResult = true;
        }


        public void SendToWebSocket(WebSockets.DataFrame data, HttpRequest request)
        {
            Server.SendToWebSocket(data, request);
        }

        public void SendToWebSocket(WebSockets.DataFrame data, Func<ISession, HttpRequest, bool> filter = null)
        {
            Server.SendToWebSocket(data, filter);
        }


        public void SendToWebSocket(ActionResult data, HttpRequest request)
        {

            if (data.Url == null)
                data.Url = this.ActionUrl;
            DataFrame frame = Server.CreateDataFrame(data);
            Server.SendToWebSocket(frame, request);

        }

        public void SendToWebSocket(ActionResult data, Func<ISession, HttpRequest, bool> filter = null)
        {
            if (data.Url == null)
                data.Url = this.ActionUrl;
            DataFrame frame = Server.CreateDataFrame(data);
            Server.SendToWebSocket(frame, filter);
        }


    }

    class HttpContext : IHttpContext
    {

        public HttpContext(HttpApiServer server, HttpRequest request, HttpResponse response, IDataContext dataContext)
        {
            Request = request;
            Response = response;
            Server = server;
            mDataContext = dataContext;
        }

        private Data.IDataContext mDataContext;

        public HttpRequest Request { get; set; }

        public HttpResponse Response { get; set; }

        public HttpApiServer Server { get; set; }

        public object Tag { get; set; }

        public bool WebSocket => false;

        public IDataContext Data => mDataContext;

        public ISession Session => Request.Session;

        public string ActionUrl { get; internal set; }

        public object this[string name] { get => Session[name]; set => Session[name] = value; }

        public void Result(object data)
        {
            Response.Result(data);
        }

        public void Async()
        {
            Response.Async();
        }

        public void SendToWebSocket(WebSockets.DataFrame data, HttpRequest request)
        {
            Server.SendToWebSocket(data, request);
        }

        public void SendToWebSocket(WebSockets.DataFrame data, Func<ISession, HttpRequest, bool> filter = null)
        {
            Server.SendToWebSocket(data, filter);
        }


        public void SendToWebSocket(ActionResult data, HttpRequest request)
        {

            if (data.Url == null)
                data.Url = this.ActionUrl;
            DataFrame frame = Server.CreateDataFrame(data);
            Server.SendToWebSocket(frame, request);

        }

        public void SendToWebSocket(ActionResult data, Func<ISession, HttpRequest, bool> filter = null)
        {
            if (data.Url == null)
                data.Url = this.ActionUrl;
            DataFrame frame = Server.CreateDataFrame(data);
            Server.SendToWebSocket(frame, filter);
        }


    }
}
