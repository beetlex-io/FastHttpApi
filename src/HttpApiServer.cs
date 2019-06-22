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
using System.Reflection;
using System.Collections.Concurrent;

namespace BeetleX.FastHttpApi
{
    public class HttpApiServer : ServerHandlerBase, BeetleX.ISessionSocketProcessHandler, WebSockets.IDataFrameSerializer, IWebSocketServer, IDisposable
    {

        public HttpApiServer() : this(null)
        {

        }

        public HttpApiServer(HttpOptions options)
        {
            mFileLog = new FileLogWriter("BEETLEX_HTTP_SERVER");
            FrameSerializer = this;
            if (options != null)
            {
                Options = options;
            }
            else
            {
                Options = LoadOptions();
            }
            mActionFactory = new ActionHandlerFactory(this);
            mResourceCenter = new StaticResurce.ResourceCenter(this);
            mUrlRewrite = new RouteRewrite(this);
            mModuleManager = new ModuleManager(this);
        }

        private string mConfigFile = "HttpConfig.json";

        private RouteRewrite mUrlRewrite;

        private ModuleManager mModuleManager;

        private StaticResurce.ResourceCenter mResourceCenter;

        private IServer mServer;

        private ServerCounter mServerCounter;

        public RouteRewrite UrlRewrite => mUrlRewrite;

        public ServerCounter ServerCounter => mServerCounter;

        private FileLogWriter mFileLog;

        private long mCurrentHttpRequests;

        //private long mCurrentWebSocketRequests;

        private long mRequestErrors;

        private long mTotalRequests;

        private long mTotalConnections;

        private ActionHandlerFactory mActionFactory;

        private ConcurrentQueue<LogRecord> mCacheLogQueue = new ConcurrentQueue<LogRecord>();

        public LogRecord[] GetCacheLog()
        {
            return mCacheLogQueue.ToArray();
        }

        private int mCacheLogLength = 0;

        private ConcurrentDictionary<string, object> mProperties = new ConcurrentDictionary<string, object>();

        public long CurrentHttpRequests => mCurrentHttpRequests;

        public long RequestErrors => mRequestErrors;

        public ModuleManager ModuleManager => mModuleManager;

        //public long CurrentWebSocketRequests => mCurrentWebSocketRequests;

        public StaticResurce.ResourceCenter ResourceCenter => mResourceCenter;

        public EventHttpServerLog ServerLog { get; set; }

        public IServer BaseServer => mServer;

        public ActionHandlerFactory ActionFactory => mActionFactory;

        public HttpOptions Options { get; set; }

        public IDataFrameSerializer FrameSerializer { get; set; }

        private ObjectPoolGroup<HttpRequest> mRequestPool = new ObjectPoolGroup<HttpRequest>();

        internal HttpRequest CreateRequest(ISession session)
        {
            HttpToken token = (HttpToken)session.Tag;
            return token.Request;
        }

        internal void Recovery(HttpRequest request)
        {

            if (!mRequestPool.Push(request))
            {

                request.Response = null;
            }
        }

        public void SaveOptions()
        {
            string file = Directory.GetCurrentDirectory() + System.IO.Path.DirectorySeparatorChar + mConfigFile;
            using (System.IO.StreamWriter writer = new StreamWriter(file))
            {
                System.Collections.Generic.Dictionary<string, object> config = new Dictionary<string, object>();
                config["HttpConfig"] = this.Options;
                string strConfig = Newtonsoft.Json.JsonConvert.SerializeObject(config);
                writer.Write(strConfig);
                writer.Flush();
                OnOptionLoad(new EventOptionsReloadArgs { HttpApiServer = this, HttpOptions = this.Options });
            }
        }

        public HttpOptions LoadOptions()
        {
            string file = Directory.GetCurrentDirectory() + System.IO.Path.DirectorySeparatorChar + mConfigFile;
            if (System.IO.File.Exists(file))
            {
                using (System.IO.StreamReader reader = new StreamReader(mConfigFile, Encoding.UTF8))
                {
                    string json = reader.ReadToEnd();
                    if (string.IsNullOrEmpty(json))
                        return new HttpOptions();
                    Newtonsoft.Json.Linq.JToken toke = (Newtonsoft.Json.Linq.JToken)Newtonsoft.Json.JsonConvert.DeserializeObject(json);
                    if (toke["HttpConfig"] != null && toke["HttpConfig"].Type == JTokenType.Object)
                    {
                        return toke["HttpConfig"].ToObject<HttpOptions>();
                    }
                    return new HttpOptions();
                }
            }
            else
            {
                return new HttpOptions();
            }
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

        public event EventHandler<EventHttpServerStartedArgs> Started;

        public event EventHandler<EventOptionsReloadArgs> OptionLoad;

        public EventHandler<WebSocketConnectArgs> WebSocketConnect { get; set; }

        private List<System.Reflection.Assembly> mAssemblies = new List<System.Reflection.Assembly>();

        private DispatchCenter<IOQueueProcessArgs> mRequestIOQueues;

        public ISession LogOutput { get; set; }

        public void Register(params System.Reflection.Assembly[] assemblies)
        {
            mUrlRewrite.UrlIgnoreCase = Options.UrlIgnoreCase;
            mAssemblies.AddRange(assemblies);
            try
            {
                mActionFactory.Register(assemblies);
            }
            catch (Exception e_)
            {
                Log(LogType.Error, " http api server load controller error " + e_.Message + "[" + e_.StackTrace + "]");
            }
        }

        [Conditional("DEBUG")]
        public void Debug(string viewpath = null)
        {
            Options.Debug = true;
            if (string.IsNullOrEmpty(viewpath))
            {
                string path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..\\..\\..\\"));
                path += @"views";
                Options.StaticResourcePath = path;
            }
            else
            {
                Options.StaticResourcePath = viewpath;
            }
        }

        public void Open()
        {
            var date = GMTDate.Default.DATE;
            var ct = ContentTypes.TEXT_UTF8;
            var a = HeaderTypeFactory.Find("Content-Length");
            AppDomain.CurrentDomain.AssemblyResolve += ResolveHandler;
            HttpPacket hp = new HttpPacket(this, this);
            var gtmdate = GMTDate.Default;
            mServer = SocketFactory.CreateTcpServer(this, hp)
                .Setting(o =>
                {
                    o.SyncAccept = Options.SyncAccept;
                   // o.IOQueues = Options.IOQueues;
                    o.DefaultListen.Host = Options.Host;
                   // o.IOQueueEnabled = Options.IOQueueEnabled;
                    o.DefaultListen.Port = Options.Port;
                    o.BufferSize = Options.BufferSize;
                    o.LogLevel = Options.LogLevel;
                    o.Combined = Options.PacketCombined;
                    o.SessionTimeOut = Options.SessionTimeOut;
                    o.UseIPv6 = Options.UseIPv6;
                    o.BufferPoolMaxMemory = Options.BufferPoolMaxMemory;
                    o.LittleEndian = false;
                    o.Statistical = Options.Statistical;
                    o.BufferPoolGroups = Options.BufferPoolGroups;
                    o.BufferPoolSize = Options.BufferPoolSize;
                    o.PrivateBufferPool = Options.PrivateBufferPool;
                    o.PrivateBufferPoolSize = Options.MaxBodyLength;
                });
            if(Options.IOQueueEnabled)
            {
                mRequestIOQueues = new DispatchCenter<IOQueueProcessArgs>(OnIOQueueProcess, Options.IOQueues);
            }
            if (Options.SSL)
            {
                mServer.Setting(o =>
                {
                    o.AddListenSSL(Options.CertificateFile, Options.CertificatePassword, o.DefaultListen.Host, Options.SSLPort);
                });
            }
            Name = "BeetleX Http Server";
            if (mAssemblies != null)
            {
                foreach (System.Reflection.Assembly assembly in mAssemblies)
                {
                    mResourceCenter.LoadManifestResource(assembly);
                }
            }
            mResourceCenter.LoadManifestResource(typeof(HttpApiServer).Assembly);
            mResourceCenter.Path = Options.StaticResourcePath;
            mResourceCenter.Debug = Options.Debug;
            mResourceCenter.Load();
            ModuleManager.Load();
            if (Options.ManageApiEnabled)
            {
                ServerController serverStatusController = new ServerController();
                mActionFactory.Register(serverStatusController);
            }
            StartTime = DateTime.Now;
            mServer.Open();
            mServerCounter = new ServerCounter(this);
            mUrlRewrite.UrlIgnoreCase = Options.UrlIgnoreCase;
            mUrlRewrite.AddRegion(this.Options.Routes);
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
            mServer.Log(LogType.Info, null, $"BeetleX FastHttpApi [IOQueue:{Options.IOQueueEnabled}|Threads:{Options.IOQueues}] [V:{typeof(HttpApiServer).Assembly.GetName().Version}]");
            OnOptionLoad(new EventOptionsReloadArgs { HttpApiServer = this, HttpOptions = this.Options });
            OnStrated(new EventHttpServerStartedArgs { HttpApiServer = this });


        }

        public Assembly ResolveHandler(object sender, ResolveEventArgs args)
        {
            try
            {
                Log(LogType.Info, $"{args.RequestingAssembly.FullName} load assembly {args.Name}");
                string path = System.IO.Path.GetDirectoryName(args.RequestingAssembly.Location) + System.IO.Path.DirectorySeparatorChar;
                string name = args.Name.Substring(0, args.Name.IndexOf(','));
                string file = path + name + ".dll";
                Assembly result = Assembly.LoadFile(file);
                return result;
            }
            catch (Exception e_)
            {
                if (EnableLog(LogType.Error))
                    Log(LogType.Error, $"{args.RequestingAssembly.FullName} load assembly {args.Name} error {e_.Message}");
            }
            return null;
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
            HttpToken token = new HttpToken();
            token.Request = new HttpRequest();
            token.Request.Init(e.Session, this);
            e.Session.Tag = token;
            e.Session.SocketProcessHandler = this;
            if (Options.IOQueueEnabled)
                token.IOQueue = mRequestIOQueues.Next();
            HttpConnected?.Invoke(server, e);
            base.Connected(server, e);
        }

        public override void Disconnect(IServer server, SessionEventArgs e)
        {
            try
            {
                HttpToken token = (HttpToken)e.Session.Tag;
                if (token != null)
                {
                    if (token.Request != null)
                        token.Request.Response = null;
                    token.Request = null;
                }

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
            if (server.Count > Options.MaxConnections)
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
                result = new byte[this.Options.MaxBodyLength];
            }
            string value;
            if (body is string)
            {
                value = (string)body;
                int length = Options.Encoding.GetBytes(value, 0, value.Length, result, 0);
                return new ArraySegment<byte>(result, 0, length);
            }
            else
            {
                value = Newtonsoft.Json.JsonConvert.SerializeObject(body);
                int length = Options.Encoding.GetBytes(value, 0, value.Length, result, 0);
                return new ArraySegment<byte>(result, 0, length);
            }
        }

        public virtual void FrameRecovery(byte[] buffer)
        {
            mBuffers.Enqueue(buffer);
        }

        private void OnWebSocketConnect(HttpRequest request, HttpResponse response)
        {
            HttpToken token = (HttpToken)request.Session.Tag;
            token.KeepAlive = true;
            if (EnableLog(LogType.Info))
            {
                mServer.Log(LogType.Info, request.Session, "{0} upgrade to websocket", request.Session.RemoteEndPoint);
            }
            ConnectionUpgradeWebsocket(request, response);
            token.WebSocket = true;
            request.WebSocket = true;

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

            if (session.Count > Options.WebSocketMaxRPS)
            {
                if (EnableLog(LogType.Error))
                {
                    mServer.Log(LogType.Error, session, $"{session.RemoteEndPoint} Session message queuing exceeds maximum rps!");
                }
                session.Dispose();
                return;
            }

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
                        ActionResult result = ExecuteWS(token.Request, data);
                    }
                }
                else
                {
                    RequestExecting();
                    try
                    {
                        var args = new WebSocketReceiveArgs();
                        args.Frame = data;
                        args.Sesson = session;
                        args.Server = this;
                        args.Request = token.Request;
                        WebSocketReceive?.Invoke(this, args);
                    }
                    finally
                    {
                        RequestExecuted();
                    }
                }

            }

        }

        #endregion

        private void CacheLog(ServerLogEventArgs e)
        {
            if (Options.CacheLogLength > 0)
            {
                LogRecord record = new LogRecord();
                record.Type = e.Type.ToString();
                record.Message = e.Message;
                record.Time = DateTime.Now.ToString("H:mm:ss");
                System.Threading.Interlocked.Increment(ref mCacheLogLength);
                if (mCacheLogLength > Options.CacheLogLength)
                {
                    mCacheLogQueue.TryDequeue(out LogRecord log);
                    System.Threading.Interlocked.Decrement(ref mCacheLogLength);
                }
                mCacheLogQueue.Enqueue(record);
            }
        }

        public override void Log(IServer server, ServerLogEventArgs e)
        {
            CacheLog(e);
            if (ServerLog == null)
            {
                if (Options.LogToConsole)
                    base.Log(server, e);
                if (Options.WriteLog)
                    mFileLog.Add(e.Type, e.Message);
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

        protected virtual void OnOptionLoad(EventOptionsReloadArgs e)
        {
            if (EnableLog(LogType.Debug))
                Log(LogType.Debug, "invoke options reload event!");
            OptionLoad?.Invoke(this, e);
        }

        protected virtual void OnStrated(EventHttpServerStartedArgs e)
        {
            if (EnableLog(LogType.Debug))
                Log(LogType.Debug, "invoke server started event!");
            Started?.Invoke(this, e);
        }

        protected virtual void OnProcessResource(HttpRequest request, HttpResponse response)
        {
            RequestExecting();
            try
            {
                if (request.Method == HttpParse.GET_TAG)
                {
                    try
                    {
                        mResourceCenter.ProcessFile(request, response);
                    }
                    catch (Exception e_)
                    {
                        if (EnableLog(LogType.Error))
                        {
                            BaseServer.Error(e_, request.Session, $"{request.RemoteIPAddress} {request.Method} {request.BaseUrl} file error {e_.Message}");
                        }
                        InnerErrorResult result = new InnerErrorResult($"response file error ", e_, Options.OutputStackTrace);
                        response.Result(result);
                    }
                }
                else
                {
                    if (EnableLog(LogType.Info))
                        Log(LogType.Info, $"{request.RemoteIPAddress}{request.Method} {request.Url} not support");
                    NotSupportResult notSupport = new NotSupportResult($"{request.Method} {request.Url} not support");
                    response.Result(notSupport);

                }
            }
            finally
            {
                RequestExecuted();
            }
        }

        internal class IOQueueProcessArgs
        {

            public HttpRequest Request;

            public HttpResponse Response;

        }

        private void OnIOQueueProcess(IOQueueProcessArgs e)
        {
            try
            {
                OnHttpRequest(e.Request, e.Response);
            }
            catch(Exception e_)
            {
                if (EnableLog(LogType.Error))
                {
                    mServer.Error(e_, e.Request.Session, "{0} On queue process error {1}", e.Request.Session.RemoteEndPoint, e_.Message);
                }
            }
        }

        private void OnRequestHandler(PacketDecodeCompletedEventArgs e)
        {
            try
            {
                HttpToken token = (HttpToken)e.Session.Tag;
                if (token.WebSocket)
                {
                     OnWebSocketRequest(e.Session, (WebSockets.DataFrame)e.Message);
                }
                else
                {
                    HttpRequest request = (HttpRequest)e.Message;
                    if (EnableLog(LogType.Info))
                    {
                        mServer.Log(LogType.Info, e.Session, $"{request.RemoteIPAddress} {request.Method} {request.Url} request");
                    }
                    if (EnableLog(LogType.Debug))
                    {
                        mServer.Log(LogType.Debug, e.Session, $"{request.RemoteIPAddress} {request.Method} {request.Url} request detail {request.ToString()}");
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
                        if (!Options.IOQueueEnabled)
                        {
                            OnHttpRequest(request, response);
                        }
                        else
                        {
                            IOQueueProcessArgs args = new IOQueueProcessArgs
                            {
                                Request = request,
                                Response = response
                            };
                            token.IOQueue.Enqueue(args);
                        }                       
                    }
                }
            }
            catch (Exception e_)
            {
                if (EnableLog(LogType.Error))
                {
                    mServer.Error(e_, e.Session, "{0} OnRequestHandler error {1}", e.Session.RemoteEndPoint, e.Message);
                }
            }
        }

        public override void SessionPacketDecodeCompleted(IServer server, PacketDecodeCompletedEventArgs e)
        {
            if (Options.SessionTimeOut > 0 && Options.Statistical)
            {
                BaseServer.UpdateSession(e.Session);
            }
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

        public ServerCounter.ServerStatus GetServerInfo()
        {
            if (ServerCounter != null)
            {
                return ServerCounter.Next();
            }
            return new ServerCounter.ServerStatus();
        }

        public void RequestError()
        {
            if (Options.Statistical)
                System.Threading.Interlocked.Increment(ref mRequestErrors);
        }

        public void RequestExecting()
        {
            if(Options.Statistical)
                System.Threading.Interlocked.Increment(ref mCurrentHttpRequests);
        }

        public void RequestExecuted()
        {
            if (Options.Statistical)
            {
                System.Threading.Interlocked.Decrement(ref mCurrentHttpRequests);
                System.Threading.Interlocked.Increment(ref mTotalRequests);
            }
        }

        protected virtual void OnHttpRequest(HttpRequest request, HttpResponse response)
        {
            if (!OnHttpRequesting(request, response).Cancel)
            {
                string baseUrl = request.BaseUrl;
                if (string.IsNullOrEmpty(request.Ext) && baseUrl[baseUrl.Length - 1] != '/')
                {
                    mActionFactory.Execute(request, response, this);
                }
                else
                {
                    OnProcessResource(request, response);
                }
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
                                    lst.Add(token.Request);
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
            return (int)(this.Options.LogLevel) <= (int)logType;
        }

        public Validations.IValidationOutputHandler ValidationOutputHandler { get; set; } = new Validations.ValidationOutputHandler();

        public void Dispose()
        {
            if (BaseServer != null)
                BaseServer.Dispose();
        }
    }
}
