using BeetleX.Buffers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace BeetleX.FastHttpApi
{
    public class HttpResponse : IDataResponse
    {

        public HttpResponse()
        {
            Header = new Header();
            AsyncResult = false;
        }

        internal Newtonsoft.Json.JsonSerializer JsonSerializer { get; set; }

        internal StreamWriter StreamWriter { get; set; }

        internal Newtonsoft.Json.JsonTextWriter JsonWriter { get; set; }

        private int mCompletedStatus = 0;

        private List<string> mSetCookies = new List<string>();

        private object mBody;

        public string Code { get; set; } = "200";

        public string CodeMsg { get; set; } = "OK";

        public Header Header { get; internal set; }

        internal ISession Session { get; set; }

        public HttpRequest Request { get; internal set; }

        public string RequestID { get; set; }

        internal bool AsyncResult { get; set; }

        private byte[] mLengthBuffer = new byte[10];

        internal void Reset()
        {
            AsyncResult = false;
            Header.Clear();
            mSetCookies.Clear();
            //Header = new Header();
            //mSetCookies = new List<string>();
            mCompletedStatus = 0;
            mBody = null;
            Code = "200";
            CodeMsg = "OK";
        }

        public void Async()
        {
            AsyncResult = true;
        }

        public void SetCookie(string name, string value, string path, DateTime? expires = null)
        {
            SetCookie(name, value, path, null, expires);
        }

        public void SetCookie(string name, string value, DateTime? expires = null)
        {
            SetCookie(name, value, "/", null, expires);
        }

        public void SetCookie(string name, string value, string path, string domain, DateTime? expires = null)
        {
            if (string.IsNullOrEmpty(name))
                return;
            name = System.Web.HttpUtility.UrlEncode(name);
            value = System.Web.HttpUtility.UrlEncode(value);
            StringBuilder sb = new StringBuilder();
            sb.Append(name).Append("=").Append(value);

            if (!string.IsNullOrEmpty(path))
            {
                sb.Append(";Path=").Append(path);
            }
            if (!string.IsNullOrEmpty(domain))
            {
                sb.Append(";Domain=").Append(domain);
            }
            if (expires != null)
            {
                sb.Append(";Expires=").Append(expires.Value.ToString("r"));
            }

            sb.Append(";HttpOnly");
            mSetCookies.Add(sb.ToString());
        }

        public void Result(object data)
        {
            if (data is StaticResurce.FileBlock)
            {
                Completed(data);
            }
            else if (data is IResult)
            {
                Completed(data);
            }
            else
            {
                ActionResult actionResult = data as ActionResult;
                IResult result;
                if (actionResult == null)
                {
                    actionResult = new ActionResult(data);
                    actionResult.Url = Request.BaseUrl;
                    actionResult.ID = RequestID;
                }
                result = new JsonResult(actionResult);
                Completed(result);
            }
        }

        internal void Result()
        {
            Completed(null);
        }


        private void Completed(object data)
        {
            if (System.Threading.Interlocked.CompareExchange(ref mCompletedStatus, 1, 0) == 0)
            {
                mBody = data;
                Session.Server.Send(this, this.Session);
            }
        }

        public void SetContentType(string type)
        {
            Header[HeaderTypeFactory.CONTENT_TYPE] = type;
        }

        public string HttpVersion { get; set; }

        public void SetStatus(string code, string msg)
        {
            Code = code;
            CodeMsg = msg;
        }


        private byte[] GetLengthBuffer(string length)
        {
            Encoding.ASCII.GetBytes(length, 0, length.Length, mLengthBuffer, 0);
            for(int i=length.Length;i<10;i++)
            {
                mLengthBuffer[i] = 32;
            }
            return mLengthBuffer;
        }

        private void OnWrite(PipeStream stream)
        {
            IResult result = mBody as IResult;
            if (result != null)
            {
                result.Setting(this);
            }
            byte[] buffer = HttpParse.GetByteBuffer();
            int hlen = 0;
            hlen = hlen + Encoding.ASCII.GetBytes(HttpVersion, 0, HttpVersion.Length, buffer, hlen);
            buffer[hlen] = HeaderTypeFactory._SPACE_BYTE;
            hlen++;
            hlen = hlen + Encoding.ASCII.GetBytes(Code, 0, Code.Length, buffer, hlen);
            buffer[hlen] = HeaderTypeFactory._SPACE_BYTE;
            hlen++;
            hlen = hlen + Encoding.ASCII.GetBytes(CodeMsg, 0, CodeMsg.Length, buffer, hlen);

            buffer[hlen] = HeaderTypeFactory._LINE_R;
            hlen++;
            buffer[hlen] = HeaderTypeFactory._LINE_N;
            hlen++;

            stream.Write(buffer, 0, hlen);
            stream.Write(HeaderTypeFactory.SERVAR_HEADER_BYTES, 0, HeaderTypeFactory.SERVAR_HEADER_BYTES.Length);
            Header.Write(stream);
            if (result != null)
            {
                result.ContentType.Write(stream);
            }
            var datebuffer = GMTDate.Default.DATE;

            stream.Write(datebuffer.Array, 0, datebuffer.Count);

            for (int i = 0; i < mSetCookies.Count; i++)
            {
                HeaderTypeFactory.Write(HeaderTypeFactory.SET_COOKIE, stream);
                stream.Write(mSetCookies[i]);
                stream.Write(HeaderTypeFactory.LINE_BYTES, 0, 2);
            }
            if (mBody != null)
            {

                if (mBody is IDataResponse dataResponse)
                {
                    stream.Write(HeaderTypeFactory.LINE_BYTES, 0, 2);
                    dataResponse.Write(stream);
                }
                else
                {
                    if (result.HasBody)
                    {
                        if (result.Length > 0)
                        {
                            stream.Write(HeaderTypeFactory.CONTENT_LENGTH_BYTES, 0, HeaderTypeFactory.CONTENT_LENGTH_BYTES.Length);
                            stream.Write(result.Length.ToString());
                            stream.Write(HeaderTypeFactory.TOW_LINE_BYTES, 0, 4);
                            result.Write(stream, this);
                        }
                        else
                        {
                            stream.Write(HeaderTypeFactory.CONTENT_LENGTH_BYTES, 0, HeaderTypeFactory.CONTENT_LENGTH_BYTES.Length);
                            MemoryBlockCollection contentLength = stream.Allocate(10);
                            stream.Write(HeaderTypeFactory.TOW_LINE_BYTES, 0, 4);
                            int len = stream.CacheLength;
                            result.Write(stream, this);
                            int count = stream.CacheLength - len;
                            // contentLength.Full(count.ToString().PadRight(10), stream.Encoding);
                            contentLength.Full(GetLengthBuffer(count.ToString()));
                        }

                    }
                    else
                    {
                        stream.Write(HeaderTypeFactory.NULL_CONTENT_LENGTH_BYTES, 0, HeaderTypeFactory.NULL_CONTENT_LENGTH_BYTES.Length);
                        stream.Write(HeaderTypeFactory.LINE_BYTES, 0, 2);
                    }
                }
            }
            else
            {
                stream.Write(HeaderTypeFactory.NULL_CONTENT_LENGTH_BYTES, 0, HeaderTypeFactory.NULL_CONTENT_LENGTH_BYTES.Length);
                stream.Write(HeaderTypeFactory.LINE_BYTES, 0, 2);
            }

            if (Session.Server.EnableLog(EventArgs.LogType.Debug))
                Session.Server.Log(EventArgs.LogType.Debug, Session, "{0} {1}", Request.RemoteIPAddress, this.ToString());

            if (Session.Server.EnableLog(EventArgs.LogType.Info))
            {
                Session.Server.Log(EventArgs.LogType.Info, Session, "{4} {0} {1} response {2} {3}", Request.Method, Request.Url, Code, CodeMsg, Request.RemoteIPAddress);
            }
        }

        void IDataResponse.Write(PipeStream stream)
        {
            try
            {
                OnWrite(stream);
            }
            catch (Exception e_)
            {
                HttpApiServer server = Request.Server;
                if (server.EnableLog(EventArgs.LogType.Error))
                {
                    server.Log(EventArgs.LogType.Error, $"{Request.RemoteIPAddress} {Request.Method} {Request.Url} response write data error {e_.Message}@{e_.StackTrace}");
                    Request.Session.Dispose();
                }
            }
            finally
            {
                Request.Recovery();
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(Request.Method + " " + Request.Url + " response " + Code + " " + CodeMsg);
            sb.Append(this.Header.ToString());
            for (int i = 0; i < mSetCookies.Count; i++)
            {
                sb.AppendLine(mSetCookies[i]);
            }
            return sb.ToString();
        }

    }
}
