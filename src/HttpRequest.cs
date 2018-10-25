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
        public HttpRequest(ISession session, HttpApiServer httpApiServer)
        {
            Header = new Header();
            this.Session = session;
            mState = LoadedState.None;
            WebSocket = false;
            KeepAlive = true;
            this.Server = httpApiServer;
        }


        public string Path { get; set; }

        public string VersionNumber { get; set; }

        public bool WebSocket { get; set; }

        private LoadedState mState;

        private int mLength;

        private QueryString mQueryString = new QueryString();

        private Cookies mCookies = new Cookies();

        private PipeStream mStream;

        internal LoadedState State => mState;

        public PipeStream Stream => mStream;

        public bool KeepAlive { get; set; }

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

        public string Ext { get; set; }

        public HttpApiServer Server { get; set; }

        public string IfNoneMatch => Header[HeaderTypeFactory.IF_NONE_MATCH];

        public QueryString QueryString => mQueryString;

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
                IndexOfResult index = stream.IndexOf(HeaderTypeFactory.LINE_BYTES);
                if (index.End != null)
                {
                    ReadOnlySpan<Char> line = HttpParse.ReadCharLine(index);
                    stream.ReadFree(index.Length);
                    Tuple<string, string, string> result = HttpParse.AnalyzeRequestLine(line, mQueryString, this);
                    Method = result.Item1;
                    Url = result.Item2;
                    BaseUrl = HttpParse.GetBaseUrlToLower(Url);
                    Ext = HttpParse.GetBaseUrlExt(BaseUrl);
                    string rewriteUrl = null;
                    string rewriteurlLower;
                    if (Server.UrlRewrite.Match(this, out rewriteUrl, out rewriteurlLower, mQueryString))
                    {
                        this.IsRewrite = true;
                        this.SourceUrl = this.Url;
                        if (Server.EnableLog(EventArgs.LogType.Info))
                        {
                            Server.BaseServer.Log(EventArgs.LogType.Info, Session, "request rewrite {0}  to {1}", Url, rewriteUrl);
                        }
                        Url = rewriteUrl;
                        BaseUrl = rewriteurlLower;
                        Ext = HttpParse.GetBaseUrlExt(BaseUrl);
                    }
                    HttpVersion = result.Item3;
                    int numberIndex = HttpVersion.IndexOf('/');
                    VersionNumber = HttpVersion.Substring(numberIndex + 1, HttpVersion.Length - numberIndex - 1);
                    mState = LoadedState.Method;
                }
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
            response.Header[HeaderTypeFactory.HOST] = Header[HeaderTypeFactory.HOST];
            response.RequestID = QueryString["_requestid"];
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
