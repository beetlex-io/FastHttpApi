using BeetleX.Buffers;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi
{
    public class HttpResponse
    {

        public HttpResponse(IBodySerializer formater)
        {
            Header = new Header();
            Header[HeaderType.SERVER] = "BeetleX-Fast-HttpServer";
            Header[HeaderType.CONTENT_TYPE] = formater.ContentType;
            Serializer = formater;
            AsyncResult = false;

        }

        private string mCode = "200";

        private string mCodeMsg = "OK";

        private List<string> mSetCookies = new List<string>();

        private object mBody;

        public string Code { get { return mCode; } }

        public string CodeMsg { get { return mCodeMsg; } }

        public Header Header { get; set; }

        public IBodySerializer Serializer { get; set; }

        internal ISession Session { get; set; }

        public HttpRequest Request { get; internal set; }

        internal bool AsyncResult { get; set; }

        public void Async()
        {
            AsyncResult = true;
        }

        public void InnerError(Exception e, bool outputStackTrace)
        {
            mCode = "500";
            mCodeMsg = "Internal Server Error";
            Completed(Serializer.GetInnerError(e, this, outputStackTrace));
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

        public void NotFound()
        {
            mCode = "404";
            mCodeMsg = "Not found";
            Completed(Serializer.GetNotFoundData(this));
        }

        public void NoModify()
        {
            mCode = "304";
            mCodeMsg = "Not Modified";
            Completed(null);
        }

        public void NotSupport()
        {
            mCode = "403";
            mCodeMsg = Request.Method + " method type not support!";
            Completed(Serializer.GetNotSupport(this));
        }

        public void Result(object data)
        {
            Completed(data);
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
            Header[HeaderType.CONTENT_TYPE] = type;
        }

        public void SetETag(string id)
        {

        }

        public string HttpVersion { get; set; }

        public void SetStatus(string code, string msg)
        {
            mCode = code;
            mCodeMsg = msg;
        }

        internal void Write(PipeStream stream)
        {
            stream.Write(HttpVersion);
            stream.Write(HeaderType.SPACE_BYTES[0]);
            stream.Write(mCode);
            stream.Write(HeaderType.SPACE_BYTES[0]);
            stream.Write(CodeMsg);
            stream.Write(HeaderType.LINE_BYTES);
            Header.Write(stream);
            for (int i = 0; i < mSetCookies.Count; i++)
            {
                HeaderType.Write(HeaderType.SET_COOKIE, stream);
                stream.Write(mSetCookies[i]);
                stream.Write(HeaderType.LINE_BYTES);
            }
            if (mBody != null)
            {
                StaticResurce.FileBlock fb = mBody as StaticResurce.FileBlock;
                if (fb != null)
                {
                    stream.Write(HeaderType.LINE_BYTES);
                    fb.Write(stream);
                }
                else
                {
                    MemoryBlockCollection contentLength = stream.Allocate(28);
                    stream.Write(HeaderType.LINE_BYTES);
                    int count = Serializer.Serialize(stream, mBody);
                    contentLength.Full("Content-Length: " + count.ToString().PadRight(10) + "\r\n", stream.Encoding);
                }
            }
            else
            {
                stream.Write(HeaderType.NULL_CONTENT_LENGTH_BYTES);
                stream.Write(HeaderType.LINE_BYTES);
            }

        }

    }
}
