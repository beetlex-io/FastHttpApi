using BeetleX.FastHttpApi.WebSockets;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi
{
    public interface IDataContext
    {

        bool TryGetString(string name, out string value);

        bool TryGetDateTime(string name, out DateTime value);

        bool TryGetDecimal(string name, out decimal value);

        bool TryGetFloat(string name, out float value);

        bool TryGetDouble(string name, out double value);

        bool TryGetUShort(string name, out ushort value);

        bool TryGetUInt(string name, out uint value);

        bool TryGetULong(string name, out ulong value);

        bool TryGetInt(string name, out int value);

        bool TryGetLong(string name, out long value);

        bool TryGetShort(string name, out short value);

        object GetBody(Type type);

        object GetObject(string name, Type type);

    }

    class WebsocketContext : IHttpContext, IDataContext
    {
        public WebsocketContext(HttpApiServer server, HttpRequest request, Newtonsoft.Json.Linq.JToken parameterData)
        {
            Server = server;
            Request = request;
            mParameterData = parameterData;
            AsyncResult = false;
            Tag = mParameterData;
        }

        private Newtonsoft.Json.Linq.JToken mParameterData;

        public HttpRequest Request { get; set; }

        public HttpResponse Response { get; set; }

        public HttpApiServer Server { get; set; }

        public object Tag { get; set; }

        public object GetBody(Type type)
        {
            JToken body = mParameterData["body"];
            if (body != null)
                return body.ToObject(type);
            return null;
        }

        public object GetObject(string name, Type type)
        {
            JToken body = mParameterData[name];
            if (body != null)
                return body.ToObject(type);
            return null;
        }


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

        public bool TryGetDateTime(string name, out DateTime value)
        {
            value = default(DateTime);
            JToken body = mParameterData[name];
            if (body != null)
            {
                var str = body.CreateReader().ReadAsString();
                return DateTime.TryParse(str, out value);
            }
            return false;
        }

        public bool TryGetDecimal(string name, out decimal value)
        {
            value = default(decimal);
            JToken body = mParameterData[name];
            if (body != null)
            {
                decimal? result = body.CreateReader().ReadAsDecimal();
                if (result != null)
                {
                    var str = body.CreateReader().ReadAsString();
                    return Decimal.TryParse(str, out value);
                }

            }
            return false;
        }

        public bool TryGetDouble(string name, out double value)
        {
            value = default(double);
            JToken body = mParameterData[name];
            if (body != null)
            {
                var str = body.CreateReader().ReadAsString();
                return double.TryParse(str, out value);
            }
            return false;
        }

        public bool TryGetFloat(string name, out float value)
        {
            value = default(float);
            JToken body = mParameterData[name];
            if (body != null)
            {
                var str = body.CreateReader().ReadAsString();
                return float.TryParse(str, out value);
            }
            return false;
        }

        public bool TryGetInt(string name, out int value)
        {
            value = default(int);
            JToken body = mParameterData[name];
            if (body != null)
            {
                var str = body.CreateReader().ReadAsString();
                return int.TryParse(str, out value);
            }
            return false;
        }

        public bool TryGetLong(string name, out long value)
        {
            value = default(long);
            JToken body = mParameterData[name];
            if (body != null)
            {
                var str = body.CreateReader().ReadAsString();
                return long.TryParse(str, out value);
            }
            return false;
        }

        public bool TryGetShort(string name, out short value)
        {
            value = default(short);
            JToken body = mParameterData[name];
            if (body != null)
            {
                var str = body.CreateReader().ReadAsString();
                return short.TryParse(str, out value);
            }
            return false;
        }

        public bool TryGetString(string name, out string value)
        {
            value = default(string);
            JToken body = mParameterData[name];
            if (body != null)
            {
                value = body.CreateReader().ReadAsString();
                return true;
            }
            return false;
        }

        public bool TryGetUInt(string name, out uint value)
        {
            value = default(uint);
            JToken body = mParameterData[name];
            if (body != null)
            {
                var str = body.CreateReader().ReadAsString();
                return uint.TryParse(str, out value);
            }
            return false;
        }

        public bool TryGetULong(string name, out ulong value)
        {
            value = default(ulong);
            JToken body = mParameterData[name];
            if (body != null)
            {
                var str = body.CreateReader().ReadAsString();
                return ulong.TryParse(str, out value);
            }
            return false;
        }

        public bool TryGetUShort(string name, out ushort value)
        {
            value = default(ushort);
            JToken body = mParameterData[name];
            if (body != null)
            {
                var str = body.CreateReader().ReadAsString();
                return ushort.TryParse(str, out value);
            }
            return false;
        }

        internal bool AsyncResult { get; set; }

        public bool WebSocket => true;

        public IDataContext Data => this;

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

    class HttpContext : IHttpContext, IDataContext
    {

        public HttpContext(HttpApiServer server, HttpRequest request, HttpResponse response)
        {
            Request = request;
            Response = response;
            Server = server;
        }

        public HttpRequest Request { get; set; }

        public HttpResponse Response { get; set; }

        public HttpApiServer Server { get; set; }

        public object Tag { get; set; }

        public bool WebSocket => false;

        public IDataContext Data => this;

        public ISession Session => Request.Session;

        public string ActionUrl { get; internal set; }

        public object this[string name] { get => Session[name]; set => Session[name] = value; }

        public object GetBody(Type type)
        {
            return Request.GetBody(type);
        }

        public bool TryGetDateTime(string name, out DateTime value)
        {
            return Request.QueryString.TryGetDateTime(name, out value);
        }

        public bool TryGetDecimal(string name, out decimal value)
        {
            return Request.QueryString.TryGetDecimal(name, out value);
        }

        public bool TryGetDouble(string name, out double value)
        {
            return Request.QueryString.TryGetDouble(name, out value);
        }

        public bool TryGetFloat(string name, out float value)
        {
            return Request.QueryString.TryGetFloat(name, out value);
        }

        public bool TryGetInt(string name, out int value)
        {
            return Request.QueryString.TryGetInt(name, out value);
        }

        public bool TryGetLong(string name, out long value)
        {
            return Request.QueryString.TryGetLong(name, out value);
        }

        public object GetObject(string name, Type type)
        {
            return null;
        }

        public bool TryGetShort(string name, out short value)
        {
            return Request.QueryString.TryGetShort(name, out value);
        }

        public bool TryGetString(string name, out string value)
        {
            return Request.QueryString.TryGetString(name, out value);
        }

        public bool TryGetUInt(string name, out uint value)
        {
            return Request.QueryString.TryGetUInt(name, out value);
        }

        public bool TryGetULong(string name, out ulong value)
        {
            return Request.QueryString.TryGetULong(name, out value);
        }

        public bool TryGetUShort(string name, out ushort value)
        {
            return Request.QueryString.TryGetUShort(name, out value);
        }

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
