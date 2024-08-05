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

        private byte[] mLengthBuffer = new byte[10];

        public SameSiteType? SameSite { get; set; }

        public bool CookieSecure { get; set; } = false;

        public IList<string> SetCookies => mSetCookies;

        internal void Reset()
        {
            Header.Clear();
            mSetCookies.Clear();
            mCompletedStatus = 0;
            mBody = null;
            Code = "200";
            CodeMsg = "OK";
            if (Request.Server.Options.SameSite != null)
                this.SameSite = Request.Server.Options.SameSite;
            else
                this.SameSite = null;
            this.CookieSecure = Request.Server.Options.CookieSecure;
        }

        public void SetCookie(string name, string value, string path,  DateTime? expires = null)
        {
            SetCookie(name, value, path, null,  expires);
        }

        public void SetCookie(string name, string value,  DateTime? expires = null)
        {
            SetCookie(name, value, "/", null,  expires);
        }

        [ThreadStatic]
        static StringBuilder mCookerSB;

        public void SetCookie(string name, string value, string path, string domain,  DateTime? expires = null)
        {
            if (string.IsNullOrEmpty(name))
                return;
            name = System.Web.HttpUtility.UrlEncode(name);
            value = System.Web.HttpUtility.UrlEncode(value);
            if (mCookerSB == null)
                mCookerSB = new StringBuilder();
            mCookerSB.Clear();
            StringBuilder sb = mCookerSB;
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
            if (SameSite != null)
                sb.Append(";SameSite=" + Enum.GetName(typeof(SameSiteType), this.SameSite.Value));
            if (CookieSecure)
                sb.Append(";Secure");
            mSetCookies.Add(sb.ToString());
        }

        public void Result(object data)
        {
            if (data is StaticResurce.FileBlock)
            {
                Completed(data);
            }
            else if (data is ValueTuple<byte[], string> dataBuff)
            {
                Completed(new BinaryResult(new ArraySegment<byte>(dataBuff.Item1, 0, dataBuff.Item1.Length), dataBuff.Item2));
            }
            else if (data is ValueTuple<ArraySegment<byte>, string> dataBuff1)
            {
                Completed(new BinaryResult(dataBuff1.Item1, dataBuff1.Item2));

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
                result = new JsonResult(actionResult, Request.Server.Options.AutoGzip);
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
                Session?.Server?.Send(this, this.Session);
            }
        }

        public void SetContentType(string type)
        {
            Header[HeaderTypeFactory.CONTENT_TYPE] = type;
        }

        public string HttpVersion { get; set; } = "HTTP/1.1";

        public void SetStatus(string code, string msg)
        {
            Code = code;
            CodeMsg = msg;
        }


        private byte[] GetLengthBuffer(string length)
        {
            Encoding.ASCII.GetBytes(length, 0, length.Length, mLengthBuffer, 0);
            for (int i = length.Length; i < 10; i++)
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
            if (Request.Server.Options.OutputServerTag)
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
                Session.Server.Log(EventArgs.LogType.Debug, Session, $"HTTP {Request.ID} {Request.RemoteIPAddress} response detail {this.ToString()}");

            if (Session.Server.EnableLog(EventArgs.LogType.Info))
            {
                Session.Server.Log(EventArgs.LogType.Info, Session, $"HTTP {Request.ID} {Request.RemoteIPAddress} {Request.Method} {Request.Url} response {Code} {CodeMsg}");
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
                    server.Log(EventArgs.LogType.Error, Request.Session, $"{Request.RemoteIPAddress} {Request.Method} {Request.Url} response write data error {e_.Message}@{e_.StackTrace}");
                    Request.Session.Dispose();
                }
            }
            finally
            {
                Request.Server.IncrementResponsed(Request, this, TimeWatch.GetTotalMilliseconds() - Request.RequestTime, int.Parse(this.Code), CodeMsg);
                Request.Recovery();
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("");
            sb.AppendLine(Request.Method + " " + Request.Url + " response " + Code + " " + CodeMsg);
            sb.Append(this.Header.ToString());
            for (int i = 0; i < mSetCookies.Count; i++)
            {
                sb.AppendLine(mSetCookies[i]);
            }
            return sb.ToString();
        }

        public void InnerError(string code, string message, bool outputStackTrace = false)
        {
            InnerError(code, message, null, outputStackTrace);
        }
        public void InnerError(string message, Exception e, bool outputStackTrace)
        {
            InnerError("500", message, e, outputStackTrace);
        }
        public void InnerError(string code, string message, Exception e, bool outputStackTrace)
        {
            Request?.Server?.OnInnerError(this, code, message, e, outputStackTrace);
        }
    }
}
