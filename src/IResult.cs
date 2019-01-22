using BeetleX.Buffers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace BeetleX.FastHttpApi
{
    public interface IResult
    {
        string ContentType { get; }

        int Length { get; set; }

        void Setting(HttpResponse response);

        bool HasBody { get; }

        void Write(PipeStream stream, HttpResponse response);
    }

    public abstract class ResultBase : IResult
    {
        public virtual string ContentType => "text/plain";

        public virtual int Length { get; set; }

        public virtual bool HasBody => true;

        public virtual void Setting(HttpResponse response)
        {

        }

        public virtual void Write(PipeStream stream, HttpResponse response)
        {

        }

    }



    public class NotFoundResult : ResultBase
    {
        public NotFoundResult(string message)
        {
            Message = message;
        }

        public NotFoundResult(string formater, params object[] data) : this(string.Format(formater, data)) { }

        public string Message { get; set; }


        public override bool HasBody => true;

        public override void Setting(HttpResponse response)
        {
            response.Request.Server.RequestError();
            response.Code = "404";
            response.CodeMsg = "not found";
            response.Request.ClearStream();
        }

        public override void Write(PipeStream stream, HttpResponse response)
        {
            stream.Write(Message);
        }
    }

    public class NoModifyResult : ResultBase
    {

        public override bool HasBody => false;

        public override void Setting(HttpResponse response)
        {
            response.Code = "304";
            response.CodeMsg = "Not Modified";
            response.Request.ClearStream();
        }
    }

    public class InnerErrorResult : ResultBase
    {
        public InnerErrorResult(string code, string messge)
        {
            Code = code;
            Error = messge;
        }

        public InnerErrorResult(string message, Exception e, bool outputStackTrace)
        {
            Error = message + ":" + e.Message;
            if (e.InnerException != null)
                Error += "->" + e.InnerException.Message;
            if (outputStackTrace)
                Code = e.StackTrace;
            else
                Code = "";
        }

        public string Error { get; set; }

        public string Code { get; set; }

        public override bool HasBody => true;

        public override void Setting(HttpResponse response)
        {
            response.Request.Server.RequestError();
            response.Code = "500";
            response.CodeMsg = "server inner error!";
            response.Request.ClearStream();

        }

        public override void Write(PipeStream stream, HttpResponse response)
        {
            stream.WriteLine(Error);
            stream.WriteLine(Code);
        }
    }

    public class UnauthorizedResult:ResultBase
    {
        public UnauthorizedResult(string message)
        {
            Message = message;
        }

        public override void Setting(HttpResponse response)
        {
            response.Request.Server.RequestError();
            response.Code = "401";
            response.CodeMsg = "Unauthorized";
            response.Request.ClearStream();
        }

        public override bool HasBody => true;

        public string Message { get; set; }

        public override void Write(PipeStream stream, HttpResponse response)
        {
            stream.Write(Message);
        }
    }

    public class NotSupportResult : ResultBase
    {
        public NotSupportResult(string messge)
        {
            Message = messge;
        }
        public NotSupportResult(string formater, params object[] data) : this(string.Format(formater, data)) { }

        public string Message { get; set; }


        public override bool HasBody => true;

        public override void Setting(HttpResponse response)
        {
            response.Request.Server.RequestError();
            response.Code = "403";
            response.CodeMsg = "not support";
            response.Request.ClearStream();
        }

        public override void Write(PipeStream stream, HttpResponse response)
        {
            stream.Write(Message);
        }
    }

    public class UpgradeWebsocketResult : ResultBase
    {
        public UpgradeWebsocketResult(string websocketKey)
        {
            WebsocketKey = websocketKey;
        }

        public string WebsocketKey { get; set; }


        public override bool HasBody => false;

        public override void Setting(HttpResponse response)
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
    }

    public class TextResult : ResultBase
    {
        public TextResult(string text)
        {
            Text = text == null ? "" : text;
        }

        public string Text { get; set; }

        public override bool HasBody => true;

        public override void Write(PipeStream stream, HttpResponse response)
        {
            stream.Write(Text);
        }
    }

    public class JsonResult : ResultBase
    {
        public JsonResult(object data)
        {
            Data = data;
        }

        public object Data { get; set; }

        public override string ContentType => "application/json";

        public override bool HasBody => true;

        public override void Write(PipeStream stream, HttpResponse response)
        {
            using (stream.LockFree())
            {
                response.JsonSerializer.Serialize(response.JsonWriter, Data);
                response.JsonWriter.Flush();
            }
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

    public class Move301Result : ResultBase
    {
        public Move301Result(string location)
        {
            Location = location;
        }

        public string Location { get; set; }


        public override bool HasBody => false;

        public override void Setting(HttpResponse response)
        {
            response.Code = "301";
            response.CodeMsg = "Moved Permanently";
            response.Header[HeaderTypeFactory.LOCATION] = Location;
        }

    }

    public class Move302Result : ResultBase
    {
        public Move302Result(string location)
        {
            Location = location;
        }

        public string Location { get; set; }


        public override bool HasBody => false;

        public override void Setting(HttpResponse response)
        {
            response.Code = "302";
            response.CodeMsg = "found";
            response.Header[HeaderTypeFactory.LOCATION] = Location;
        }

    }

    public class StringBytes : ResultBase
    {
        public StringBytes(byte[] data)
        {
            Data = data;
            Length = data.Length;
        }
        public byte[] Data { get; set; }

        public override bool HasBody => true;

        public override void Write(PipeStream stream, HttpResponse response)
        {
            stream.Write(Data, 0, Data.Length);
        }
    }

}
