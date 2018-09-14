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
            Header[Header.SERVER] = "BeetleX-Fast-HttpServer";
            Header[Header.CONTENT_TYPE] = formater.ContentType;
            Serializer = formater;
        }

        private string mCode = "200";

        private string mCodeMsg = "OK";

        private object mBody;

        public string Code { get { return mCode; } }

        public string CodeMsg { get { return mCodeMsg; } }

        public Header Header { get; set; }

        public IBodySerializer Serializer { get; set; }

        internal ISession Session { get; set; }

        public HttpRequest Request { get; internal set; }

        public void InnerError(Exception e, bool outputStackTrace)
        {
            mCode = "500";
            mCodeMsg = "Internal Server Error";
            Completed(Serializer.GetInnerError(e, this, outputStackTrace));
        }

        public void NotFound()
        {
            mCode = "404";
            mCodeMsg = "Not found";
            Completed(Serializer.GetNotFoundData(this));
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

        private int mCompletedStatus = 0;

        private void Completed(object data)
        {
            if (System.Threading.Interlocked.CompareExchange(ref mCompletedStatus, 1, 0) == 0)
            {
                mBody = data;
                Session.Server.Send(this, this.Session);
            }
        }

        public string HttpVersion { get; set; }

        public void SetStatus(string code, string msg)
        {
            mCode = code;
            mCodeMsg = msg;
        }

        internal void Write(PipeStream stream)
        {
            string stateLine = string.Concat(HttpVersion, " ", mCode, " ", mCodeMsg);
            stream.WriteLine(stateLine);
            Header.Write(stream);
            MemoryBlockCollection contentLength = stream.Allocate(28);
            stream.WriteLine("");
            int count = Serializer.Serialize(stream, mBody);
            contentLength.Full("Content-Length: " + count.ToString().PadRight(10) + "\r\n", stream.Encoding);

        }

    }
}
