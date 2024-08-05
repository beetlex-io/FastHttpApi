using BeetleX.Buffers;
using System;
using System.Collections.Generic;

using System.Text;

namespace BeetleX.FastHttpApi.Clients
{
    public class Response
    {

        public Response()
        {
            Header = new Header();
            Cookies = new Cookies();
            mState = LoadedState.None;
            this.KeepAlive = true;
        }

        public HttpClientException Exception { get; set; }

        public Cookies Cookies { get; private set; }

        public string Code { get; set; }

        public string CodeMsg { get; set; }

        public bool KeepAlive { get; set; }

        public string HttpVersion { get; set; }

        public object Body { get; set; }

        public int Length { get; set; }

        public Header Header { get; private set; }

        public bool Chunked { get; set; }

        public bool Gzip { get; set; }

        internal PipeStream Stream { get; set; }

        private LoadedState mState;

        public LoadedState Load(PipeStream stream)
        {
            string line;
            if (mState == LoadedState.None)
            {
                if (stream.TryReadWith(HeaderTypeFactory.LINE_BYTES, out line))
                {
                    HttpParse.AnalyzeResponseLine(line, this);
                    mState = LoadedState.Method;
                }
            }
            if (mState == LoadedState.Method)
            {
                if (Header.Read(stream, Cookies))
                {
                    mState = LoadedState.Header;
                }
            }
            if (mState == LoadedState.Header)
            {
                if (string.Compare(Header[HeaderTypeFactory.CONNECTION], "close", true) == 0)
                {
                    this.KeepAlive = false;
                }
                if (string.Compare(Header[HeaderTypeFactory.TRANSFER_ENCODING], "chunked", true) == 0)
                {
                    Chunked = true;
                }
                else
                {
                    string lenstr = Header[HeaderTypeFactory.CONTENT_LENGTH];
                    int length = 0;
                    if (lenstr != null)
                        int.TryParse(lenstr, out length);
                    Length = length;
                }
                mState = LoadedState.Completed;
            }
            return mState;
        }


        [ThreadStatic]
        private static Response mCurrent;
        public static Response Current
        {
            get
            {
                return mCurrent;
            }
            set
            {
                mCurrent = value;
            }
        }
    }
}
