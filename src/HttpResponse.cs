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
                Request.Recovery();
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

            byte[] buffer = HttpParse.GetByteBuffer();
            //string line = string.Concat(HttpVersion, " ", mCode, " ", CodeMsg, "\r\n");
            //var hlen = Encoding.ASCII.GetBytes(line, 0, line.Length, buffer, 0);
            int hlen = 0;
            hlen = hlen + Encoding.ASCII.GetBytes(HttpVersion, 0, HttpVersion.Length, buffer, hlen);
            buffer[hlen] = HeaderTypeFactory._SPACE_BYTE;
            hlen++;
            hlen = hlen + Encoding.ASCII.GetBytes(mCode, 0, mCode.Length, buffer, hlen);
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
            for (int i = 0; i < mSetCookies.Count; i++)
            {
                HeaderTypeFactory.Write(HeaderTypeFactory.SET_COOKIE, stream);
                stream.Write(mSetCookies[i]);
                stream.Write(HeaderTypeFactory.LINE_BYTES, 0, 2);
            }
            if (mBody != null)
            {
                StaticResurce.FileBlock fb = mBody as StaticResurce.FileBlock;
                if (fb != null)
                {
                    stream.Write(HeaderTypeFactory.LINE_BYTES, 0, 2);
                    fb.Write(stream);
                }
                else
                {
                    if (result.HasBody)
                    {
                        stream.Write(HeaderTypeFactory.CONTENT_LENGTH_BYTES, 0, 16);
                        MemoryBlockCollection contentLength = stream.Allocate(10);
                        stream.Write(HeaderTypeFactory.TOW_LINE_BYTES, 0, 4);
                        int len = stream.CacheLength;
                        result.Write(stream, this);
                        int count = stream.CacheLength - len;
                        //contentLength.Full("Content-Length: " + count.ToString().PadRight(10) + "\r\n\r\n", stream.Encoding);
                        contentLength.Full(count.ToString().PadRight(10), stream.Encoding);
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
