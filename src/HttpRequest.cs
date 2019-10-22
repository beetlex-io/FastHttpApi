using BeetleX.Buffers;
using System;
using System.Collections.Generic;
using System.IO;
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
        static HttpRequest()
        {
            mIDPrefix = (long)(DateTime.Now - DateTime.Parse("1970-1-1")).TotalSeconds;
            mIDPrefix = mIDPrefix << 24;
        }

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

        private static long mIDPrefix;

        private static long mID = 0;

        internal static long GetID()
        {
            var id = System.Threading.Interlocked.Increment(ref mID);
            if (id > 10000000)
            {
                long prefix = (long)(DateTime.Now - DateTime.Parse("1970-1-1")).TotalSeconds;
                prefix = prefix << 24;
                System.Threading.Interlocked.Exchange(ref mID, 0);
                System.Threading.Interlocked.Exchange(ref mIDPrefix, prefix);
            }
            return mIDPrefix | id;
        }

        public long ID { get; internal set; }

        internal void Init(ISession session, HttpApiServer httpApiServer)
        {
            this.Session = session;
            this.Server = httpApiServer;
        }

        public double RequestTime { get; set; }

        internal void Reset()
        {
            IsRewrite = false;
            mState = LoadedState.None;
            mWebSocket = false;
            mKeepAlive = true;
            mExt = null;
            mLength = 0;

        }

        internal HttpResponse Response { get; set; }

        internal HttpResponse CreateResponse()
        {
            if (Response == null)
            {
                Response = new HttpResponse();
                Response.JsonSerializer = new Newtonsoft.Json.JsonSerializer();
                Response.StreamWriter = new StreamWriter(Session.Stream.ToPipeStream());
                Response.JsonWriter = new Newtonsoft.Json.JsonTextWriter(Response.StreamWriter);
            }
            else
                Response.Reset();
            HttpResponse response = Response;
            response.HttpVersion = this.HttpVersion;
            response.Session = this.Session;
            response.HttpVersion = this.HttpVersion;
            response.Request = this;
            if (VersionNumber == "1.0" && this.KeepAlive)
                response.Header[HeaderTypeFactory.CONNECTION] = "Keep-Alive";
            response.RequestID = mQueryString["_requestid"];
            return response;
        }

        public void Recovery()
        {
            if (!WebSocket)
            {
                ClearStream();

                Header.Clear();
                mFiles.Clear();
                mDataContxt.Clear();
                mCookies.Clear();
                Reset();
            }
        }

        private string mExt;

        private bool mWebSocket = false;

        private bool mKeepAlive = true;

        private Data.DataContxt mDataContxt;

        private List<PostFile> mFiles = new List<PostFile>();

        public IList<PostFile> Files => mFiles;

        public Data.DataContxt Data => mDataContxt;

        public string Path { get; internal set; }

        public string VersionNumber { get; internal set; }

        public bool WebSocket { get { return mWebSocket; } set { mWebSocket = value; } }

        private LoadedState mState;

        private int mLength;

        private QueryString mQueryString;

        private Cookies mCookies;

        private PipeStream mStream;

        internal LoadedState State => mState;

        internal int PathLevel { get; set; }

        public PipeStream Stream => mStream;

        public ActionHandler ActionHandler { get; set; }

        public bool KeepAlive { get { return mKeepAlive; } set { mKeepAlive = value; } }

        public Header Header { get; private set; }

        public Cookies Cookies => mCookies;

        public int Length => mLength;

        public string RemoteIPAddress
        {
            get
            {
                string value = Header[HeaderTypeFactory.CLIENT_IPADDRESS];
                if (value == null)
                {
                    if (Session.RemoteEndPoint is IPEndPoint IP)
                    {
                        value = IP.Address.ToString();
                        Header[HeaderTypeFactory.CLIENT_IPADDRESS] = value;
                    }
                }
                return value;
            }
        }

        // public long UrlCode { get; internal set; }

        public string Method { get; internal set; }

        public bool IsRewrite { get; internal set; }

        internal string SourcePath { get; set; }

        internal string SourceUrl { get; set; }

        public string GetSourcePath()
        {
            if (IsRewrite)
                return SourcePath;
            return Path;
        }


        public string GetSourceUrl()
        {
            if (IsRewrite)
                return SourceUrl;
            return Url;
        }

        internal string SourceBaseUrl { get; set; }

        public string GetSourceBaseUrl()
        {
            if (IsRewrite)
                return SourceBaseUrl;
            return BaseUrl;
        }

        public string BaseUrl { get; internal set; }

        public string Url { get; internal set; }

        public string HttpVersion { get; internal set; }

        public string Ext { get { return mExt; } internal set { mExt = value; } }

        public HttpApiServer Server { get; internal set; }

        public string IfNoneMatch => Header[HeaderTypeFactory.IF_NONE_MATCH];

        public string Host => Header[HeaderTypeFactory.HOST];

        public string Authorization => Header[HeaderTypeFactory.AUTHORIZATION];

        public string Accept => Header[HeaderTypeFactory.ACCEPT];

        public string AcceptEncoding => Header[HeaderTypeFactory.ACCEPT_ENCODING];

        public string AcceptLanguage => Header[HeaderTypeFactory.ACCEPT_LANGUAGE];

        public string AcceptCharset => Header[HeaderTypeFactory.ACCEPT_CHARSET];

        public string ContentType => Header[HeaderTypeFactory.CONTENT_TYPE];

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
                Span<char> data;
                if (!stream.ReadLine(out data))
                {
                    return;
                }
                PathLevel = 0;
                HttpParse.AnalyzeRequestLine(data, this);
                HttpParse.ReadHttpVersionNumber(HttpVersion, mQueryString, this);
                int len = HttpParse.ReadUrlQueryString(Url, mQueryString, this);
                if (len > 0)
                    HttpParse.ReadUrlPathAndExt(Url.AsSpan().Slice(0, len), mQueryString, this, this.Server.Options);
                else
                    HttpParse.ReadUrlPathAndExt(Url.AsSpan(), mQueryString, this, this.Server.Options);
                RouteMatchResult routeMatchResult;
                this.IsRewrite = false;
                if (Server.UrlRewrite.Count > 0 && Server.UrlRewrite.Match(this, out routeMatchResult, mQueryString))
                {
                    this.IsRewrite = true;
                    this.SourceUrl = Url;
                    this.SourceBaseUrl = BaseUrl;
                    this.SourcePath = Path;
                    Url = routeMatchResult.RewriteUrl;
                    if (Server.Options.AgentRewrite)
                    {
                        if (len > 0 && this.SourceUrl.Length > len + 1)
                        {
                            if (routeMatchResult.HasQueryString)
                            {
                                Url += "&";
                            }
                            else
                            {
                                Url += "?";
                            }
                            Url += new string(SourceUrl.AsSpan().Slice(len + 1));
                        }
                    }
                    len = HttpParse.ReadUrlQueryString(Url, null, this);
                    if (len > 0)
                        HttpParse.ReadUrlPathAndExt(Url.AsSpan().Slice(0, len), mQueryString, this, this.Server.Options);
                    else
                        HttpParse.ReadUrlPathAndExt(Url.AsSpan(), mQueryString, this, this.Server.Options);
                    if (Server.EnableLog(EventArgs.LogType.Info))
                    {
                        Server.BaseServer.Log(EventArgs.LogType.Info, Session, $"HTTP {ID} {((IPEndPoint)Session.RemoteEndPoint).Address} request {SourceUrl} rewrite to {Url}");
                    }
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

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("");
            sb.Append(Header.ToString());
            sb.Append(this.Cookies.ToString());
            return sb.ToString();
        }
    }
}
