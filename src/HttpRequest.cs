using BeetleX.Buffers;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace BeetleX.FastHttpApi
{
    public enum LoadedState : int
    {
        None = 1,
        Method = 2,
        Header = 4,
        Completed = 8
    }

    public class HttpRequest
    {
        public HttpRequest()
        {
            Header = new Header();
            mDataContxt = new Data.DataContxt();
            mQueryString = new QueryString(mDataContxt);
            mCookies = new Cookies();
            mState = LoadedState.None;
            WebSocket = false;
            KeepAlive = true;
        }

        internal void Init(ISession session, HttpApiServer httpApiServer)
        {
            this.Session = session;
            this.Server = httpApiServer;
        }

        internal void Reset()
        {
            mState = LoadedState.None;
            mWebSocket = false;
            mKeepAlive = true;
            mExt = null;
            mLength = 0;
            Header.Clear();
            mDataContxt.Clear();
            mCookies.Clear();
        }

        internal void Recovery()
        {
            this.Server.Recovery(this);
        }

        private string mExt;

        private bool mWebSocket = false;

        private bool mKeepAlive = true;

        private Data.DataContxt mDataContxt;

        public Data.DataContxt Data => mDataContxt;

        public string Path { get; set; }

        public string VersionNumber { get; set; }

        public bool WebSocket { get { return mWebSocket; } set { mWebSocket = value; } }

        private LoadedState mState;

        private int mLength;

        private QueryString mQueryString;

        private Cookies mCookies;

        private PipeStream mStream;

        internal LoadedState State => mState;

        public PipeStream Stream => mStream;

        public bool KeepAlive { get { return mKeepAlive; } set { mKeepAlive = value; } }

        public Header Header { get; private set; }

        public Cookies Cookies => mCookies;

        public int Length => mLength;

        public string ClientIPAddress => Header[HeaderTypeFactory.CLIENT_IPADDRESS];

        public string Method { get; set; }

        public bool IsRewrite { get; set; }

        public string SourceUrl { get; set; }

        public string BaseUrl { get; set; }

        public string Url { get; set; }

        public string HttpVersion { get; set; }

        public string Ext { get { return mExt; } set { mExt = value; } }

        public HttpApiServer Server { get; set; }

        public string IfNoneMatch => Header[HeaderTypeFactory.IF_NONE_MATCH];

        public ISession Session { get; private set; }

        public LoadedState Read(PipeStream stream)
        {
            mStream = stream;
            LoadMethod(stream);
            LoadHeaer(stream);
            LoadBody(stream);
            return mState;
        }

        public void ClearStream()
        {
            if (Length > 0)
            {
                this.Stream.ReadFree(Length);
            }
        }



        private void LoadMethod(PipeStream stream)
        {
            if (mState == LoadedState.None)
            {
                string data;
                if (!stream.TryReadWith(HeaderTypeFactory.LINE_BYTES, out data))
                {
                    return;
                }
                HttpParse.AnalyzeRequestLine(data, this);
                HttpParse.ReadHttpVersionNumber(HttpVersion, mQueryString, this);
                int len = HttpParse.ReadUrlQueryString(Url, mQueryString, this);
                if (len > 0)
                    HttpParse.ReadUrlPathAndExt(Url.AsSpan().Slice(0, len), mQueryString, this, this.Server.ServerConfig);
                else
                    HttpParse.ReadUrlPathAndExt(Url.AsSpan(), mQueryString, this, this.Server.ServerConfig);

                RouteMatchResult routeMatchResult = new RouteMatchResult();
                if (Server.UrlRewrite.Match(this, ref routeMatchResult, mQueryString))
                {
                    this.IsRewrite = true;
                    this.SourceUrl = this.Url;
                    if (Server.EnableLog(EventArgs.LogType.Info))
                    {
                        Server.BaseServer.Log(EventArgs.LogType.Info, Session, "request rewrite {0}  to {1}", Url, routeMatchResult.RewriteUrl);
                    }
                    Url = routeMatchResult.RewriteUrl;
                    if (Server.ServerConfig.UrlIgnoreCase)
                        BaseUrl = routeMatchResult.RewriteUrlLower;
                    else
                        BaseUrl = routeMatchResult.RewriteUrl;
                    Ext = routeMatchResult.Ext;
                }
                mState = LoadedState.Method;
            }
        }

        private void LoadHeaer(PipeStream stream)
        {
            if (mState == LoadedState.Method)
            {
                if (this.Header.Read(stream, mCookies))
                {
                    mState = LoadedState.Header;
                    string length = Header[HeaderTypeFactory.CONTENT_LENGTH];
                    if (length != null)
                        int.TryParse(length, out mLength);
                    if (VersionNumber == "1.0")
                    {
                        string connection = Header[HeaderTypeFactory.CONNECTION];
                        KeepAlive = string.Compare(connection, "keep-alive", true) == 0;
                    }
                }
            }
        }

        private void LoadBody(PipeStream stream)
        {
            if (mState == LoadedState.Header)
            {
                if (mLength == 0)
                {
                    mState = LoadedState.Completed;
                }
                else
                {
                    if (stream.Length == mLength)
                    {
                        mState = LoadedState.Completed;
                    }
                }
            }
        }

        public HttpResponse CreateResponse()
        {
            HttpResponse response = new HttpResponse();
            response.HttpVersion = this.HttpVersion;
            response.Session = this.Session;
            response.HttpVersion = this.HttpVersion;
            response.Request = this;
            if (VersionNumber == "1.0" && this.KeepAlive)
                response.Header[HeaderTypeFactory.CONNECTION] = "Keep-Alive";
            response.RequestID = mQueryString["_requestid"];
            return response;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(this.Method + " " + this.Url);
            sb.Append(Header.ToString());
            sb.Append(this.Cookies.ToString());
            return sb.ToString();
        }
    }
}
