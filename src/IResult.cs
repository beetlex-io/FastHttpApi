using BeetleX.Buffers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace BeetleX.FastHttpApi
{
    public interface IResult
    {
        IHeaderItem ContentType { get; }

        int Length { get; set; }

        void Setting(HttpResponse response);

        bool HasBody { get; }

        void Write(PipeStream stream, HttpResponse response);
    }

    public abstract class ResultBase : IResult
    {
        public virtual IHeaderItem ContentType => ContentTypes.TEXT_UTF8;

        public virtual int Length { get; set; }

        public virtual bool HasBody => true;

        public virtual void Setting(HttpResponse response)
        {

        }

        public virtual void Write(PipeStream stream, HttpResponse response)
        {

        }

    }



    public class BadRequestResult : ResultBase
    {
        public BadRequestResult(string message)
        {
            Message = message;
        }

        public BadRequestResult(string formater, params object[] data) : this(string.Format(formater, data)) { }

        public string Message { get; set; }


        public override bool HasBody => true;

        public override void Setting(HttpResponse response)
        {
            response.Code = "400";
            response.CodeMsg = "Bad Request";
            response.Request.ClearStream();
        }

        public override void Write(PipeStream stream, HttpResponse response)
        {
            stream.Write(Message);
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
            Message = messge;
        }


        public InnerErrorResult(string message, Exception e, bool outputStackTrace) : this("500", message, e, outputStackTrace)
        {

        }

        public InnerErrorResult(string code, string message, Exception e, bool outputStackTrace)
        {
            Code = code;
            Message = message;
            if (e != null)
            {
                if (e.InnerException == null)
                    Error = e.Message;
                else
                {
                    Error = $"{e.Message} ({e.InnerException.Message})";
                }
            }

            if (outputStackTrace)
                SourceCode = e.StackTrace;
            else
                SourceCode = "";
        }

        public string Message { get; set; }

        public string Error { get; set; }

        public string Code { get; set; }

        public string SourceCode { get; set; }

        public override bool HasBody => true;

        public override void Setting(HttpResponse response)
        {
            response.Code = Code;
            response.CodeMsg = Message;
            response.Request.ClearStream();

        }

        public override void Write(PipeStream stream, HttpResponse response)
        {
            // stream.WriteLine(Message);
            if (!string.IsNullOrEmpty(Error))
            {
                stream.WriteLine(Error);
            }
            if (!string.IsNullOrEmpty(SourceCode))
            {
                stream.WriteLine(SourceCode);
            }
        }
    }

    public class UnauthorizedResult : ResultBase
    {
        public UnauthorizedResult(string message)
        {
            Message = message;
        }

        public override void Setting(HttpResponse response)
        {
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
            response.Code = "403";
            response.CodeMsg = "No permission";
            response.Request.ClearStream();
        }

        public override void Write(PipeStream stream, HttpResponse response)
        {
            stream.Write(Message);
        }
    }


    public class UpgradeWebsocketError : ResultBase
    {

        public UpgradeWebsocketError(int code, string msg)
        {
            Code = code;
            CodeMsg = msg;
        }

        public override void Setting(HttpResponse response)
        {
            response.Code = Code.ToString();
            response.CodeMsg = CodeMsg;
        }

        public int Code { get; set; }

        public string CodeMsg { get; set; }

        public override bool HasBody => false;
    }


    public class UpgradeWebsocketSuccess : ResultBase
    {
        public UpgradeWebsocketSuccess(string websocketKey)
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
        public TextResult(string text, bool autoGzip = false)
        {
            Text = text == null ? "" : text;
            mAutoGzip = autoGzip;
        }

        private bool mAutoGzip = false;

        public string Text { get; set; }

        public override bool HasBody => true;

        private ArraySegment<byte>? mGzipData;

        public override void Setting(HttpResponse response)
        {
            base.Setting(response);
            if (mAutoGzip && Text.Length > 1024)
            {
                var buffer = System.Buffers.ArrayPool<byte>.Shared.Rent(Text.Length * 6);
                var len = Encoding.UTF8.GetBytes(Text, buffer);
                mGzipData = new ArraySegment<byte>(buffer, 0, len);
                response.Header.Add("Content-Encoding", "gzip");
            }
        }

        public override void Write(PipeStream stream, HttpResponse response)
        {
            if (mGzipData != null)
            {
                try
                {
                    using (stream.LockFree())
                    {
                        using (var gzipStream = new GZipStream(stream, CompressionMode.Compress, true))
                        {
                            gzipStream.Write(mGzipData.Value.Array, mGzipData.Value.Offset, mGzipData.Value.Count);
                            gzipStream.Flush();
                        }
                    }
                }
                finally
                {
                    System.Buffers.ArrayPool<byte>.Shared.Return(mGzipData.Value.Array);
                }
            }
            else
            {
                stream.Write(Text);
            }
        }
    }

    public class JsonResult : ResultBase
    {
        public JsonResult(object data, bool autoGzip = false)
        {
            Data = data;
            mAutoGzip = autoGzip;

        }

        public object Data { get; set; }

        private bool mAutoGzip = false;

        private ArraySegment<byte> mJsonData;

        [ThreadStatic]
        private static System.Text.StringBuilder mJsonText;

        private void OnSerialize(HttpResponse response)
        {
            if (mJsonText == null)
                mJsonText = new System.Text.StringBuilder();
            mJsonText.Clear();
            JsonSerializer serializer = response.JsonSerializer;
            System.IO.StringWriter writer = new System.IO.StringWriter(mJsonText);
            JsonTextWriter jsonTextWriter = new JsonTextWriter(writer);
            serializer.Serialize(jsonTextWriter, Data);
            var charbuffer = System.Buffers.ArrayPool<Char>.Shared.Rent(mJsonText.Length);
            mJsonText.CopyTo(0, charbuffer, 0, mJsonText.Length);
            try
            {
                var bytes = System.Buffers.ArrayPool<byte>.Shared.Rent(mJsonText.Length * 6);
                var len = System.Text.Encoding.UTF8.GetBytes(charbuffer, 0, mJsonText.Length, bytes, 0);
                mJsonData = new ArraySegment<byte>(bytes, 0, len);
            }
            finally
            {
                System.Buffers.ArrayPool<char>.Shared.Return(charbuffer);
            }
        }

        public override void Setting(HttpResponse response)
        {
            base.Setting(response);
            if (this.mAutoGzip)
                OnSerialize(response);
            if (mAutoGzip && mJsonData.Count > 1024 * 2)
            {
                response.Header.Add("Content-Encoding", "gzip");
            }
        }

        public override IHeaderItem ContentType => ContentTypes.JSON;

        public override bool HasBody => true;

        public override void Write(PipeStream stream, HttpResponse response)
        {
            if (mAutoGzip)
            {
                try
                {
                    if (mJsonData.Count > 1024 * 2)
                    {
                        using (stream.LockFree())
                        {
                            using (var gzipStream = new GZipStream(stream, CompressionMode.Compress, true))
                            {
                                gzipStream.Write(mJsonData.Array, mJsonData.Offset, mJsonData.Count);
                                gzipStream.Flush();
                            }
                        }
                    }
                    else
                    {
                        stream.Write(mJsonData.Array, mJsonData.Offset, mJsonData.Count);
                    }
                }
                finally
                {
                    System.Buffers.ArrayPool<byte>.Shared.Return(mJsonData.Array);
                }
            }
            else
            {
                using (stream.LockFree())
                {
                    response.JsonSerializer.Serialize(response.JsonWriter, Data);
                    response.JsonWriter.Flush();
                }
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

    public class FileResult
    {
        public FileResult(string file) : this(file, null, false)
        {

        }
        public FileResult(string file, string contentType, bool gzip = false)
        {
            this.File = file;
            this.ContentType = contentType;
            GZip = gzip;
        }

        public string File { get; set; }

        public string ContentType { get; set; }

        public bool GZip { get; set; } = false;
    }


    public class BinaryResult : BeetleX.FastHttpApi.IResult
    {
        public BinaryResult(ArraySegment<byte> data, string contentType = null)
        {
            if (!string.IsNullOrEmpty(contentType))
            {
                mContentType = new ContentType(contentType);
            }
            Data = data;
        }

        private IHeaderItem mContentType = ContentTypes.OCTET_STREAM;

        public IHeaderItem ContentType => mContentType;

        public int Length { get; set; }

        public bool AutoGZIP { get; set; } = false;

        public bool HasBody => true;

        public ArraySegment<byte> Data { get; private set; }

        public Action<HttpResponse, BinaryResult> Completed { get; set; }

        public virtual void Setting(HttpResponse response)
        {

        }

        public virtual void Write(PipeStream stream, HttpResponse response)
        {
            if (AutoGZIP)
            {
                using (stream.LockFree())
                {
                    using (var gzipStream = new GZipStream(stream, CompressionMode.Compress, true))
                    {
                        gzipStream.Write(Data.Array, Data.Offset, Data.Count);
                        gzipStream.Flush();
                    }
                }
            }
            else
            {
                stream.Write(Data.Array, Data.Offset, Data.Count);
            }
            Completed?.Invoke(response, this);
        }
    }



    public class DownLoadResult : BeetleX.FastHttpApi.IResult
    {
        public DownLoadResult(string text, string fileName, IHeaderItem contentType = null)
        {
            mData = Encoding.UTF8.GetBytes(text);
            mFileName = System.Web.HttpUtility.UrlEncode(fileName);
            if (contentType != null)
                mContentType = contentType;
        }

        public DownLoadResult(byte[] data, string fileName, IHeaderItem contentType = null)
        {
            mData = data;
            mFileName = System.Web.HttpUtility.UrlEncode(fileName);
            if (contentType != null)
                mContentType = contentType;
        }

        private string mFileName;

        private byte[] mData;

        private IHeaderItem mContentType = ContentTypes.OCTET_STREAM;

        public IHeaderItem ContentType => mContentType;

        public int Length { get; set; }

        public bool HasBody => true;

        public void Setting(HttpResponse response)
        {
            response.Header.Add("Content-Disposition", $"attachment;filename={mFileName}");
        }
        public virtual void Write(PipeStream stream, HttpResponse response)
        {
            stream.Write(mData);
        }
    }
}
