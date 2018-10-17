using BeetleX.Buffers;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace BeetleX.FastHttpApi
{
    public interface IResult
    {
        string ContentType { get; }

        void Setting(HttpResponse response);

        bool HasBody { get; }

        void Write(PipeStream stream, HttpResponse response);
    }

    public class NotFoundResult : IResult
    {
        public NotFoundResult(string message)
        {
            Message = message;
        }

        public NotFoundResult(string formater, params object[] data) : this(string.Format(formater, data)) { }

        public string Message { get; set; }

        public string ContentType => "text/html";

        public bool HasBody => true;

        public void Setting(HttpResponse response)
        {
            response.Code = "404";
            response.CodeMsg = "not found";
            response.Request.ClearStream();
        }

        public void Write(PipeStream stream, HttpResponse response)
        {
            stream.Write(Message);
        }
    }

    public class NoModifyResult : IResult
    {

        public string ContentType => "text/html";

        public bool HasBody => false;

        public void Setting(HttpResponse response)
        {
            response.Code = "304";
            response.CodeMsg = "Not Modified";
            response.Request.ClearStream();
        }
        public void Write(PipeStream stream, HttpResponse response)
        {
        }
    }

    public class InnerErrorResult : IResult
    {
        public InnerErrorResult(string code, string messge)
        {
            Code = code;
            Error = messge;
        }

        public InnerErrorResult(Exception e, bool outputStackTrace)
        {
            Error = e.Message;
            if (e.InnerException != null)
                Error += "->" + e.InnerException.Message;
            if (outputStackTrace)
                Code = e.StackTrace;
            else
                Code = "";
        }

        public string Error { get; set; }

        public string Code { get; set; }

        public string ContentType => "text/html";

        public bool HasBody => true;

        public void Setting(HttpResponse response)
        {
            response.Code = "500";
            response.CodeMsg = "server inner error!";
            response.Request.ClearStream();

        }

        public void Write(PipeStream stream, HttpResponse response)
        {
            stream.WriteLine(Error);
            stream.WriteLine(Code);
        }
    }

    public class NotSupportResult : IResult
    {
        public NotSupportResult(string messge)
        {
            Message = messge;
        }
        public NotSupportResult(string formater, params object[] data) : this(string.Format(formater, data)) { }

        public string Message { get; set; }

        public string ContentType => "text/html";

        public bool HasBody => true;

        public void Setting(HttpResponse response)
        {
            response.Code = "403";
            response.CodeMsg = "not support";
            response.Request.ClearStream();
        }

        public void Write(PipeStream stream, HttpResponse response)
        {
            stream.Write(Message);
        }
    }

    public class UpgradeWebsocketResult : IResult
    {
        public UpgradeWebsocketResult(string websocketKey)
        {
            WebsocketKey = websocketKey;
        }

        public string WebsocketKey { get; set; }

        public string ContentType => "text/html";

        public bool HasBody => false;

        public void Setting(HttpResponse response)
        {
            response.Code = "101";
            response.CodeMsg = "Switching Protocols";
            response.Header.Add(HeaderTypeFactory.CONNECTION, "Upgrade");
            response.Header.Add(HeaderTypeFactory.UPGRADE, "websocket");
            response.Header.Add(HeaderTypeFactory.SEC_WEBSOCKET_VERSION, "13");
            SHA1 sha1 = new SHA1CryptoServiceProvider();
            byte[] bytes_sha1_in = Encoding.UTF8.GetBytes(WebsocketKey + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11");
            byte[] bytes_sha1_out = sha1.ComputeHash(bytes_sha1_in);
            string str_sha1_out = Convert.ToBase64String(bytes_sha1_out);
            response.Header.Add(HeaderTypeFactory.SEC_WEBSOCKT_ACCEPT, str_sha1_out);
        }

        public void Write(PipeStream stream, HttpResponse response)
        {
            throw new NotImplementedException();
        }
    }

    public class TextResult : IResult
    {
        public TextResult(string text)
        {
            Text = text == null ? "" : text;
        }

        public string Text { get; set; }

        public string ContentType => "text/html";

        public bool HasBody => true;

        public void Setting(HttpResponse response)
        {

        }

        public void Write(PipeStream stream, HttpResponse response)
        {
            stream.Write(Text);
        }
    }

    public class JsonResult : IResult
    {
        public JsonResult(object data)
        {
            Data = data;
        }

        public object Data { get; set; }

        public string ContentType => "application/json";

        public bool HasBody => true;

        public void Setting(HttpResponse response)
        {

        }

        public void Write(PipeStream stream, HttpResponse response)
        {
            string value = Newtonsoft.Json.JsonConvert.SerializeObject(Data);
            stream.Write(value);

        }
    }

    public class ActionJsonResult : JsonResult
    {
        public ActionJsonResult(object data) : base(
            new ActionResult(data)
            )
        { }

        public ActionJsonResult(int code, string error) : base(
            new ActionResult(code, error)
            )
        {

        }

    }

    public class Move301Result : IResult
    {
        public Move301Result(string location)
        {
            Location = location;
        }

        public string Location { get; set; }

        public string ContentType => "text/html";

        public bool HasBody => false;

        public void Setting(HttpResponse response)
        {
            response.Code = "301";
            response.CodeMsg = "Moved Permanently";
            response.Header[HeaderTypeFactory.LOCATION] = Location;
        }

        public void Write(PipeStream stream, HttpResponse response)
        {

        }
    }

    public class Move302Result : IResult
    {
        public Move302Result(string location)
        {
            Location = location;
        }

        public string Location { get; set; }

        public string ContentType => "text/html";

        public bool HasBody => false;

        public void Setting(HttpResponse response)
        {
            response.Code = "302";
            response.CodeMsg = "found";
            response.Header[HeaderTypeFactory.LOCATION] = Location;
        }

        public void Write(PipeStream stream, HttpResponse response)
        {

        }
    }

}
