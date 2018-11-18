using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using BeetleX.Buffers;
using BeetleX.EventArgs;
using BeetleX.FastHttpApi.WebSockets;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Net;
using BeetleX.Dispatchs;

namespace BeetleX.FastHttpApi
{
    public class HttpApiServer : ServerHandlerBase, BeetleX.ISessionSocketProcessHandler, WebSockets.IDataFrameSerializer, IWebSocketServer
    {

        public HttpApiServer() : this(null)
        {

        }

        public HttpApiServer(HttpConfig serverConfig)
        {
            mFileLog = new FileLog();
            FrameSerializer = this;
            if (serverConfig != null)
            {
                ServerConfig = serverConfig;
            }
            else
            {
                string configFile = "HttpConfig.json";
                if (System.IO.File.Exists(configFile))
                {
                    using (System.IO.StreamReader reader = new StreamReader(configFile, Encoding.UTF8))
                    {
                        string json = reader.ReadToEnd();
                        Newtonsoft.Json.Linq.JToken toke = (Newtonsoft.Json.Linq.JToken)Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                        ServerConfig = toke["HttpConfig"].ToObject<HttpConfig>();
                    }
                }
                else
                {
                    ServerConfig = new HttpConfig();
                }
            }
            mActionFactory = new ActionHandlerFactory();
            mUrlRewrite = new RouteRewrite(this);
        }

        private RouteRewrite mUrlRewrite;

        private StaticResurce.ResourceCenter mResourceCenter;

        private IServer mServer;

        private ServerCounter mServerCounter;

        public RouteRewrite UrlRewrite => mUrlRewrite;

        public ServerCounter ServerCounter => mServerCounter;

        private FileLog mFileLog;

        private long mCurrentHttpRequests;

        private long mCurrentWebSocketRequests;

        private long mTotalRequests;

        private long mTotalConnections;

        private ActionHandlerFactory mActionFactory;

        private System.Collections.Concurrent.ConcurrentDictionary<string, object> mProperties = new System.Collections.Concurrent.ConcurrentDictionary<string, object>();

        public long CurrentHttpRequests => mCurrentHttpRequests;

        public long CurrentWebSocketRequests => mCurrentWebSocketRequests;

        public StaticResurce.ResourceCenter ResourceCenter => mResourceCenter;

        public EventHttpServerLog ServerLog { get; set; }

        public IServer BaseServer => mServer;

        public ActionHandlerFactory ActionFactory => mActionFactory;

        public HttpConfig ServerConfig { get; set; }

        public IDataFrameSerializer FrameSerializer { get; set; }

        private ObjectPoolGroup<HttpRequest> mRequestPool = new ObjectPoolGroup<HttpRequest>();

        internal HttpRequest CreateRequest(ISession session)
        {
            HttpRequest request;
            if (!mRequestPool.TryPop(out request))
                request = new HttpRequest();
            request.Init(session, this);
            return request;
        }

        internal void Recovery(HttpRequest request)
        {

            mRequestPool.Push(request);
        }


        public object this[string name]
        {
            get
            {
                object result;
                mProperties.TryGetValue(name, out result);
                return result;
            }
            set
            {
                mProperties[name] = value;
            }
        }

        public DateTime StartTime { get; set; }

        public long TotalRequest => mTotalRequests;

        public long TotalConnections => mTotalConnections;

        public EventHandler<WebSocketReceiveArgs> WebSocketReceive { get; set; }

        public event EventHandler<ConnectedEventArgs> HttpConnected;

        public event EventHandler<EventHttpRequestArgs> HttpRequesting;

        public event EventHandler<EventHttpRequestArgs> HttpRequestNotfound;

        public event EventHandler<SessionEventArgs> HttpDisconnect;

        public EventHandler<WebSocketConnectArgs> WebSocketConnect { get; set; }

        private List<System.Reflection.Assembly> mAssemblies = new List<System.Reflection.Assembly>();

        public ISession LogOutput { get; set; }

        public void Register(params System.Reflection.Assembly[] assemblies)
        {
            mAssemblies.AddRange(assemblies);
            try
            {
                mActionFactory.Register(this.ServerConfig, this, assemblies);
            }
            catch (Exception e_)
            {
                Log(LogType.Error, " http api server load controller error " + e_.Message + "[" + e_.StackTrace + "]");
            }
        }

        [Conditional("DEBUG")]
        public void Debug(string viewpath = null)
        {
            ServerConfig.Debug = true;
            if (string.IsNullOrEmpty(viewpath))
            {
                string path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\"));
                path += @"views";
                ServerConfig.StaticResourcePath = path;
            }
            else
            {
                ServerConfig.StaticResourcePath = viewpath;
            }

        }

        public void Open()
        {
            NetConfig config = new NetConfig();
            config.Host = ServerConfig.Host;
            config.Port = ServerConfig.Port;
            config.CertificateFile = ServerConfig.CertificateFile;
            config.CertificatePassword = ServerConfig.CertificatePassword;
            config.BufferSize = ServerConfig.BufferSize;
            config.LogLevel = ServerConfig.LogLevel;
            config.Combined = ServerConfig.PacketCombined;
            config.UseIPv6 = ServerConfig.UseIPv6;
            if (!string.IsNullOrEmpty(config.CertificateFile))
                config.SSL = true;
            config.LittleEndian = false;
            //if (Environment.ProcessorCount >= 10)
            //    config.IOQueueEnabled = true;
            mResourceCenter = new StaticResurce.ResourceCenter(this);
            HttpPacket hp = new HttpPacket(this, this);
            mServer = SocketFactory.CreateTcpServer(config, this, hp);
            Name = "BeetleX Http Server";
            if (mAssemblies != null)
            {
                foreach (System.Reflection.Assembly assembly in mAssemblies)
                {
                    mResourceCenter.LoadManifestResource(assembly);
                }
            }
            mResourceCenter.LoadManifestResource(typeof(HttpApiServer).Assembly);
            mResourceCenter.Path = ServerConfig.StaticResourcePath;
            mResourceCenter.Debug = ServerConfig.Debug;
            mResourceCenter.Load();
            StartTime = DateTime.Now;
            mServer.Open();
            mServerCounter = new ServerCounter(this);
            mUrlRewrite.UrlIgnoreCase = ServerConfig.UrlIgnoreCase;
            mUrlRewrite.AddRegion(this.ServerConfig.Routes);
            HeaderTypeFactory.Find(HeaderTypeFactory.HOST);
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                using (System.IO.StreamWriter writer = new StreamWriter("__UnhandledException.txt"))
                {
                    Exception error = e.ExceptionObject as Exception;
                    writer.WriteLine(DateTime.Now);
                    if (error != null)
                    {
                        writer.WriteLine(error.Message);
                        writer.WriteLine(error.StackTrace);
                        if (error.InnerException != null)
                        {
                            writer.WriteLine(error.InnerException.Message);
                            writer.WriteLine(error.InnerException.StackTrace);
                        }
                    }
                    else
                    {
                        writer.WriteLine("Unhandled Exception:" + e.ExceptionObject.ToString());

                    }
                    writer.Flush();
                }
            };
            mServer.Log(LogType.Info, null, "FastHttpApi Server started [v:{0}]", typeof(HttpApiServer).Assembly.GetName().Version);

        }

        public string Name { get { return mServer.Name; } set { mServer.Name = value; } }

        public void SendToWebSocket(DataFrame data, Func<ISession, HttpRequest, bool> filter = null)
        {
            IList<HttpRequest> items = GetWebSockets();

            if (items.Count > 0)
            {
                List<ISession> receiveRequest = new List<ISession>();
                IServer server = items[0].Server.BaseServer;
                for (int i = 0; i < items.Count; i++)
                {
                    var item = items[i];
                    if ((filter == null || filter(item.Session, item)))
                    {
                        receiveRequest.Add(item.Session);
                    }

                }
                server.Send(data, receiveRequest.ToArray());
            }
        }

        public void SendToWebSocket(DataFrame data, params HttpRequest[] request)
        {
            if (request != null)
            {
                IServer server = request[0].Server.BaseServer;
                ISession[] sessions = new ISession[request.Length];
                for (int i = 0; i < request.Length; i++)
                {
                    sessions[i] = request[i].Session;
                }
                server.Send(data, sessions);

            }
        }

        private void OnSendToWebSocket(DataFrame data, HttpRequest request)
        {
            data.Send(request.Session);

        }

        public override void Connected(IServer server, ConnectedEventArgs e)
        {
            System.Threading.Interlocked.Increment(ref mTotalConnections);
            e.Session.Tag = new HttpToken();
            e.Session.SocketProcessHandler = this;
            HttpConnected?.Invoke(server, e);
            base.Connected(server, e);
        }

        public override void Disconnect(IServer server, SessionEventArgs e)
        {
            try
            {
                if (LogOutput == e.Session)
                    LogOutput = null;
                HttpDisconnect?.Invoke(server, e);
                base.Disconnect(server, e);
            }
            finally
            {
                e.Session.Tag = null;
            }
        }

        public void Log(LogType type, string message, params object[] parameters)
        {
            Log(type, string.Format(message, parameters));
        }

        public void Log(LogType type, string message)
        {
            try
            {
                Log(null, new ServerLogEventArgs(message, type));
            }
            catch { }
        }

        public override void Connecting(IServer server, ConnectingEventArgs e)
        {
            if (server.Count > ServerConfig.MaxConnections)
            {
                e.Cancel = true;
            }
            e.Socket.NoDelay = true;
        }

        public override void Error(IServer server, ServerErrorEventArgs e)
        {
            base.Error(server, e);
        }



        #region websocket
        public virtual object FrameDeserialize(DataFrame data, PipeStream stream)
        {
            return stream.ReadString((int)data.Length);
        }

        private System.Collections.Concurrent.ConcurrentQueue<byte[]> mBuffers = new System.Collections.Concurrent.ConcurrentQueue<byte[]>();

        public virtual ArraySegment<byte> FrameSerialize(DataFrame data, object body)
        {
            byte[] result;
            if (!mBuffers.TryDequeue(out result))
            {
                result = new byte[this.ServerConfig.MaxBodyLength];
            }
            string value;
            if (body is string)
                value = (string)body;
            else
                value = Newtonsoft.Json.JsonConvert.SerializeObject(body);
            int length = ServerConfig.Encoding.GetBytes(value, 0, value.Length, result, 0);
            return new ArraySegment<byte>(result, 0, length);
        }

        public virtual void FrameRecovery(byte[] buffer)
        {
            mBuffers.Enqueue(buffer);
        }

        private void OnWebSocketConnect(HttpRequest request, HttpResponse response)
        {
            HttpToken token = (HttpToken)request.Session.Tag;
            token.KeepAlive = true;
            token.WebSocketRequest = request;
            token.WebSocket = true;
            request.WebSocket = true;
            if (EnableLog(LogType.Info))
            {
                mServer.Log(LogType.Info, request.Session, "{0} upgrade to websocket", request.Session.RemoteEndPoint);
            }
            ConnectionUpgradeWebsocket(request, response);

        }

        protected virtual void ConnectionUpgradeWebsocket(HttpRequest request, HttpResponse response)
        {
            WebSocketConnectArgs wsca = new WebSocketConnectArgs(request);
            wsca.Request = request;
            WebSocketConnect?.Invoke(this, wsca);
            if (wsca.Cancel)
            {
                if (EnableLog(LogType.Warring))
                {
                    mServer.Log(LogType.Warring, request.Session, "{0} cancel upgrade to websocket", request.Session.RemoteEndPoint);
                }
                response.Session.Dispose();
            }
            else
            {
                UpgradeWebsocketResult upgradeWebsocket = new UpgradeWebsocketResult(request.Header[HeaderTypeFactory.SEC_WEBSOCKET_KEY]);
                response.Result(upgradeWebsocket);
            }
        }

        public ActionResult ExecuteWS(HttpRequest request, DataFrame dataFrame)
        {
            JToken dataToken = (JToken)Newtonsoft.Json.JsonConvert.DeserializeObject((string)dataFrame.Body);
            return this.mActionFactory.ExecuteWithWS(request, this, dataToken);
        }


        public DataFrame CreateDataFrame(object body = null)
        {
            DataFrame dp = new DataFrame();
            dp.DataPacketSerializer = this.FrameSerializer;
            dp.Body = body;
            return dp;
        }

        protected virtual void OnWebSocketRequest(ISession session, DataFrame data)
        {
            System.Threading.Interlocked.Increment(ref mCurrentWebSocketRequests);
            try
            {
                if (EnableLog(LogType.Info))
                {
                    mServer.Log(LogType.Info, session, "{0} receive websocket data {1}", session.RemoteEndPoint, data.Type.ToString());
                }
                HttpToken token = (HttpToken)session.Tag;
                if (data.Type == DataPacketType.ping)
                {
                    DataFrame pong = CreateDataFrame();
                    pong.Type = DataPacketType.pong;
                    pong.FIN = true;
                    session.Send(pong);
                }
                else if (data.Type == DataPacketType.connectionClose)
                {
                    session.Dispose();
                }
                else
                {
                    if (WebSocketReceive == null)
                    {
                        if (data.Type == DataPacketType.text)
                        {
                            ActionResult result = ExecuteWS(token.WebSocketRequest, data);
                        }
                    }
                    else
                    {
                        var args = new WebSocketReceiveArgs();
                        args.Frame = data;
                        args.Sesson = session;
                        args.Server = this;
                        args.Request = token.WebSocketRequest;
                        WebSocketReceive?.Invoke(this, args);
                    }

                }
            }
            finally
            {
                System.Threading.Interlocked.Decrement(ref mCurrentWebSocketRequests);
            }
        }

        #endregion



        public override void Log(IServer server, ServerLogEventArgs e)
        {
            if (ServerLog == null)
            {
                if (ServerConfig.LogToConsole)
                    base.Log(server, e);
                if (ServerConfig.WriteLog)
                    mFileLog.Add(e);
            }
            else
                ServerLog(server, e);
            ISession output = LogOutput;
            if (output != null && e.Session != output)
            {
                ActionResult log = new ActionResult();
                log.Data = new { LogType = e.Type.ToString(), Time = DateTime.Now.ToString("H:mm:ss"), Message = e.Message };
                CreateDataFrame(log).Send(output);
            }
        }

        protected virtual void OnProcessResource(HttpRequest request, HttpResponse response)
        {
            if (string.Compare(request.Method, "GET", true) == 0)
            {
                try
                {
                    mResourceCenter.ProcessFile(request, response);
                }
                catch (Exception e_)
                {
                    if (EnableLog(LogType.Error))
                    {
                        BaseServer.Error(e_, request.Session, "{0} response file error {1}", request.ClientIPAddress, e_.Message);
                        InnerErrorResult result = new InnerErrorResult($"response file error ", e_, ServerConfig.OutputStackTrace);
                        response.Result(result);
                    }
                }
            }
            else
            {
                NotSupportResult notSupport = new NotSupportResult("{0} method {1} not support", request.Url, request.Method);
                response.Result(notSupport);
            }
        }

        private void OnRequestHandler(object state)
        {
            PacketDecodeCompletedEventArgs e = (PacketDecodeCompletedEventArgs)state;
            try
            {
                System.Threading.Interlocked.Increment(ref mTotalRequests);
                HttpToken token = (HttpToken)e.Session.Tag;
                if (token.WebSocket)
                {
                    OnWebSocketRequest(e.Session, (WebSockets.DataFrame)e.Message);
                }
                else
                {
                    HttpRequest request = (HttpRequest)e.Message;
                    if (request.ClientIPAddress == null)
                    {
                        IPEndPoint IP = e.Session.RemoteEndPoint as IPEndPoint;
                        if (IP != null)
                        {
                            string ipstr = IP.Address.ToString() + ":" + IP.Port.ToString();
                            request.Header.Add(HeaderTypeFactory.CLIENT_IPADDRESS, ipstr);
                        }
                    }
                    if (EnableLog(LogType.Info))
                    {
                        mServer.Log(LogType.Info, e.Session, "{0} {1} {2}", request.ClientIPAddress, request.Method, request.Url);
                    }
                    if (EnableLog(LogType.Debug))
                    {
                        mServer.Log(LogType.Debug, e.Session, "{0} {1}", request.ClientIPAddress, request.ToString());
                    }
                    request.Server = this;
                    HttpResponse response = request.CreateResponse();
                    token.KeepAlive = request.KeepAlive;
                    if (token.FirstRequest && string.Compare(request.Header[HeaderTypeFactory.UPGRADE], "websocket", true) == 0)
                    {
                        token.FirstRequest = false;
                        OnWebSocketConnect(request, response);
                    }
                    else
                    {
                        token.FirstRequest = false;
                        OnHttpRequest(request, response);
                    }
                }
            }
            catch (Exception e_)
            {
                mServer.Error(e_, e.Session, "{0} OnRequestHandler error {1}", e.Session.RemoteEndPoint, e.Message);
            }
        }

        public override void SessionPacketDecodeCompleted(IServer server, PacketDecodeCompletedEventArgs e)
        {
            OnRequestHandler(e);
        }


        internal EventHttpRequestArgs OnHttpRequesNotfound(HttpRequest request, HttpResponse response)
        {
            HttpToken token = (HttpToken)request.Session.Tag;
            token.RequestArgs.Request = request;
            token.RequestArgs.Response = response;
            token.RequestArgs.Cancel = false;
            HttpRequestNotfound?.Invoke(this, token.RequestArgs);
            return token.RequestArgs;
        }


        internal EventHttpRequestArgs OnHttpRequesting(HttpRequest request, HttpResponse response)
        {
            HttpToken token = (HttpToken)request.Session.Tag;
            token.RequestArgs.Request = request;
            token.RequestArgs.Response = response;
            token.RequestArgs.Cancel = false;
            HttpRequesting?.Invoke(this, token.RequestArgs);
            return token.RequestArgs;
        }

        protected virtual void OnHttpRequest(HttpRequest request, HttpResponse response)
        {
            System.Threading.Interlocked.Increment(ref mCurrentHttpRequests);
            try
            {
                if (!OnHttpRequesting(request, response).Cancel)
                {
                    if (string.IsNullOrEmpty(request.Ext) && request.BaseUrl != "/")
                    {
                        mActionFactory.Execute(request, response, this);
                    }
                    else
                    {
                        OnProcessResource(request, response);
                    }
                }
            }
            finally
            {
                System.Threading.Interlocked.Decrement(ref mCurrentHttpRequests);
            }
        }


        public virtual void ReceiveCompleted(ISession session, SocketAsyncEventArgs e)
        {

        }

        public virtual void SendCompleted(ISession session, SocketAsyncEventArgs e)
        {
            HttpToken token = (HttpToken)session.Tag;
            if (token.File != null)
            {
                token.File = token.File.Next();
                if (token.File != null)
                {
                    session.Send(token.File);
                    return;
                }
            }
            if (session.Count == 0 && !token.KeepAlive)
            {
                session.Dispose();
            }
        }

        private long mVersion;

        private IList<HttpRequest> mOnlines = new List<HttpRequest>();

        private int mGetWebsocketStatus = 0;

        public IList<HttpRequest> GetWebSockets()
        {
            if (mVersion != BaseServer.Version)
            {
                if (System.Threading.Interlocked.CompareExchange(ref mGetWebsocketStatus, 1, 0) == 0)
                {
                    try
                    {
                        if (mVersion != BaseServer.Version)
                        {
                            ISession[] items = BaseServer.GetOnlines();
                            List<HttpRequest> lst = new List<HttpRequest>();
                            for (int i = 0; i < items.Length; i++)
                            {
                                HttpToken token = (HttpToken)items[i].Tag;
                                if (token != null && token.WebSocket)
                                    lst.Add(token.WebSocketRequest);
                            }
                            mOnlines = lst;
                            mVersion = BaseServer.Version;
                        }
                    }
                    finally
                    {
                        System.Threading.Interlocked.Exchange(ref mGetWebsocketStatus, 0);
                    }

                }
            }
            return mOnlines;
        }

        public bool EnableLog(LogType logType)
        {
            return (int)(this.ServerConfig.LogLevel) <= (int)logType;
        }


    }
}
