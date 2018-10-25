using BeetleX.Buffers;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace BeetleX.FastHttpApi
{
    public class HttpResponse
    {

        public HttpResponse()
        {
            Header = new Header();
            Header[HeaderTypeFactory.SERVER] = "BeetleX-Fast-HttpServer";
            Header[HeaderTypeFactory.CONTENT_TYPE] = "text/html";

            AsyncResult = false;

        }

        private string mCode = "200";

        private string mCodeMsg = "OK";

        private List<string> mSetCookies = new List<string>();

        private object mBody;

        public string Code { get { return mCode; } set { mCode = value; } }

        public string CodeMsg { get { return mCodeMsg; } set { mCodeMsg = value; } }

        public Header Header { get; set; }

        internal ISession Session { get; set; }

        public HttpRequest Request { get; internal set; }

        public string RequestID { get; set; }

        internal bool AsyncResult { get; set; }

        public void Async()
        {
            AsyncResult = true;
        }

        public void SetCookie(string name, string value, DateTime? expires = null)
        {
            SetCookie(name, value, "/", expires);
        }

        public void SetCookie(string name, string value, string path, DateTime? expires = null)
        {
            string cookie;
            if (string.IsNullOrEmpty(name))
                return;
            name = System.Web.HttpUtility.UrlEncode(name);
            value = System.Web.HttpUtility.UrlEncode(value);
            if (expires == null)
            {
                cookie = string.Format("{0}={1};path={2}", name, value, path);
            }
            else
            {
                cookie = string.Format("{0}={1};path={2};expires={3}", name, value, path, expires.Value.ToString("r"));
            }
            mSetCookies.Add(cookie);
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
                IResult result = Request.Server.GetResponseResult(this, data);
                Completed(result);
            }
        }

        public void Result()
        {
            Completed(null);
        }

        private int mCompletedStatus = 0;

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
            mCode = code;
            mCodeMsg = msg;
        }

        internal void Write(PipeStream stream)
        {
            IResult result = mBody as IResult;
            if (result != null)
            {
                this.Header[HeaderTypeFactory.CONTENT_TYPE] = result.ContentType;
                result.Setting(this);
            }
            stream.Write(HttpVersion);
            stream.Write(HeaderTypeFactory.SPACE_BYTES[0]);
            stream.Write(mCode);
            stream.Write(HeaderTypeFactory.SPACE_BYTES[0]);
            stream.Write(CodeMsg);
            stream.Write(HeaderTypeFactory.LINE_BYTES);
          
            Header.Write(stream);
            for (int i = 0; i < mSetCookies.Count; i++)
            {
                HeaderTypeFactory.Write(HeaderTypeFactory.SET_COOKIE, stream);
                stream.Write(mSetCookies[i]);
                stream.Write(HeaderTypeFactory.LINE_BYTES);
            }
            if (mBody != null)
            {
                StaticResurce.FileBlock fb = mBody as StaticResurce.FileBlock;
                if (fb != null)
                {
                    stream.Write(HeaderTypeFactory.LINE_BYTES);
                    fb.Write(stream);
                }
                else
                {
                    if (result.HasBody)
                    {
                      
                        MemoryBlockCollection contentLength = stream.Allocate(28);
                        stream.Write(HeaderTypeFactory.LINE_BYTES);
                        int len = stream.CacheLength;
                        result.Write(stream, this);
                        int count = stream.CacheLength - len;
                        contentLength.Full("Content-Length: " + count.ToString().PadRight(10) + "\r\n", stream.Encoding);
                    }
                    else
                    {
                        stream.Write(HeaderTypeFactory.NULL_CONTENT_LENGTH_BYTES);
                        stream.Write(HeaderTypeFactory.LINE_BYTES);
                    }
                }
            }
            else
            {
                stream.Write(HeaderTypeFactory.NULL_CONTENT_LENGTH_BYTES);
                stream.Write(HeaderTypeFactory.LINE_BYTES);
            }

            if (Session.Server.EnableLog(EventArgs.LogType.Debug))
                Session.Server.Log(EventArgs.LogType.Debug, Session, "{0} {1}", Request.ClientIPAddress, this.ToString());

            if (Session.Server.EnableLog(EventArgs.LogType.Info))
            {
                Session.Server.Log(EventArgs.LogType.Info, Session, "{4} {0} {1} response {2} {3}", Request.Method, Request.Url, Code, CodeMsg, Request.ClientIPAddress);
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
