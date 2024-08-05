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
using System.Runtime;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Buffers;
using System.Security.Principal;
using Newtonsoft.Json;

namespace BeetleX.FastHttpApi
{
    public partial class HttpApiServer : ServerHandlerBase, BeetleX.ISessionSocketProcessHandler, WebSockets.IDataFrameSerializer, IWebSocketServer, IDisposable
    {

        public const int WEBSOCKET_SUCCESS = 250;

        public const int WEBSOCKET_ERROR = 550;

        public const string LICENSE_URL = "/__GET_SN";

        public const string CODE_TREAK_PARENTID = "parent-id";

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
            mActionSettings.Load();
            mIPv4Tables.Load();
            mActionFactory = new ActionHandlerFactory(this);
            mResourceCenter = new StaticResurce.ResourceCenter(this);
            mResourceCenter.Debug = Options.Debug;
            mUrlRewrite = new RouteRewrite(this);
            mModuleManager = new ModuleManager(this);
            AllRpsLimit = new RpsLimit(Options.MaxRps);
            this.LoadLicenses();
            CommandLineParser = CommandLineParser.GetCommandLineParser();
        }

        const string mConfigFile = "HttpConfig.json";

        private RouteRewrite mUrlRewrite;

        private IPv4Tables mIPv4Tables = new IPv4Tables();

        private ModuleManager mModuleManager;

        private StaticResurce.ResourceCenter mResourceCenter;

        private IServer mServer;

        private ServerCounter mServerCounter;

        public CommandLineParser CommandLineParser { get; private set; }

        public RouteRewrite UrlRewrite => mUrlRewrite;

        public ServerCounter ServerCounter => mServerCounter;

        private FileLogWriter mFileLog;

        private IPLimit mIPLimit;

        private long mRequestErrors;

        private long mTotalRequests;

        private long mTotalConnections;

        private ActionSettings mActionSettings = new ActionSettings();

        private ActionHandlerFactory mActionFactory;

        private ConcurrentQueue<LogRecord> mCacheLogQueue = new ConcurrentQueue<LogRecord>();

        public LogRecord[] GetCacheLog()
        {
            return mCacheLogQueue.ToArray();
        }

        private int mCacheLogLength = 0;

        public Dictionary<string, string> ActionExts { get; private set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        private ConcurrentDictionary<string, object> mProperties = new ConcurrentDictionary<string, object>();

        public long RequestErrors => mRequestErrors;

        public IPLimit IPLimit => mIPLimit;

        public IPv4Tables IPv4Tables => mIPv4Tables;

        public ModuleManager ModuleManager => mModuleManager;

        public StaticResurce.ResourceCenter ResourceCenter => mResourceCenter;

        public event EventHttpServerLog ServerLog;

        public IServer BaseServer => mServer;

        public ActionHandlerFactory ActionFactory => mActionFactory;

        public HttpOptions Options { get; set; }

        public IDataFrameSerializer FrameSerializer { get; set; }

        private Dictionary<string, Dictionary<string, string>> mLicenses = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

        private ObjectPoolGroup<HttpRequest> mRequestPool = new ObjectPoolGroup<HttpRequest>();

        private string[] mBindDomains = new string[0];

        private static Dictionary<string, string> LoadLicenseInfo(string info)
        {

            try
            {
                Dictionary<string, string> data = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(info);
                using (RSA rsa = RSA.Create(2048))
                {
                    RSAParameters rSAParameters = Newtonsoft.Json.JsonConvert.DeserializeObject<RSAParameters>(
                        "{\"D\": null,  \"DP\": null,  \"DQ\": null,  \"Exponent\": \"AQAB\",  \"InverseQ\": null,  \"Modulus\": \"00yOCUKe6cixa0Hr1upIRZVYXuncVYCVXzNtl9PAyjyEfxny4QtWVUuw3MK/DF1vx51QBtS3izb1dhQYdwmoD2FGZy7TehnZ9AvVEuesWvJtz6Npm6zmHM1wVYqYrCkyWzIgKenyv59yHgYgL55UN8c3oqqwg2voeq+12IscYjYAXpqWywA7x+33TsTz4J1wQzMaq5VM+6yPZuqDa7sCyubK6qlUpHy790Iy7bD0y48pGrlZOHibcA6NOCAu/LsqKJWCX/38tx8HEzEE+i8TyuhLhMaQpFGTUBgP5iJ9ONviu/irvIXlP68atFdntmBs2cvWktxNJzmcGB24upjatQ==\",  \"P\": null,  \"Q\": null}");
                    rsa.ImportParameters(rSAParameters);

                    string values = "";
                    string token = "";
                    foreach (var item
                        in from a in data orderby a.Key descending select a)
                    {
                        if (item.Key == "token")
                        {
                            token = item.Value;
                        }
                        else
                        {
                            values += item.Value;
                        }
                    }

                    if (rsa.VerifyData(Encoding.UTF8.GetBytes(values), Convert.FromBase64String(token), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1))
                    {
                        data.Remove("token");
                        return data;
                    }
                    return new Dictionary<string, string>();
                }

            }
            catch (Exception e_)
            {
                return new Dictionary<string, string>();
            }
        }

        private void LoadLicenses()
        {
            foreach (var file in System.IO.Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.sn"))
            {
                try
                {
                    using (StreamReader reader = new StreamReader(file, Encoding.UTF8))
                    {
                        string txt = reader.ReadToEnd();
                        var lincense = LoadLicenseInfo(txt);
                        if (lincense != null)
                        {
                            if (lincense.TryGetValue("InvalidDate", out string date))
                            {
                                if (DateTime.Now > DateTime.Parse(date))
                                    continue;
                            }
                            string name = System.IO.Path.GetFileNameWithoutExtension(file);
                            mLicenses[name] = lincense;
                        }
                    }
                }
                catch (Exception e)
                {

                }
            }
        }

        public void SetBindDomains(string domains)
        {
            Options.BindDomains = domains;
            if (!string.IsNullOrEmpty(Options.BindDomains))
            {
                mBindDomains = Options.BindDomains.Split(';');

            }
            else
            {
                mBindDomains = new string[0];
            }

        }

        private bool CheckDomains(HttpRequest request)
        {
            if (mBindDomains == null || mBindDomains.Length == 0)
                return true;
            var host = request.Host;
            if (string.IsNullOrEmpty(host))
                return false;
            foreach (var item in mBindDomains)
            {
                if (host.IndexOf(item) >= 0)
                    return true;
            }
            return false;
        }

        public Dictionary<string, string> GetLicense(string name)
        {
            mLicenses.TryGetValue(name, out Dictionary<string, string> result);
            return result;
        }

        internal HttpRequest CreateRequest(ISession session)
        {
            HttpToken token = (HttpToken)session.Tag;
            token.Request.RequestTime = TimeWatch.GetTotalMilliseconds();
            return token.Request;
        }

        internal void Recovery(HttpRequest request)
        {

            if (!mRequestPool.Push(request))
            {

                request.Response = null;
            }
        }

        public RpsLimit AllRpsLimit { get; private set; }


        public UrlLimit<UrlLimitConfig> UrlsLimit { get; private set; }

        public UrlLimit<DomainLimitConfig> DomainsLimit { get; private set; }

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

        public static HttpOptions LoadOptions()
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

        public Func<HttpRequest, ActionHandler, object, object> WSActionResultHandler { get; set; }

        public System.Collections.Concurrent.ConcurrentDictionary<string, IRpsLimitHandler> RpsLimitHandlers { get; private set; } = new ConcurrentDictionary<string, IRpsLimitHandler>();

        public EventHandler<WebSocketReceiveArgs> WebSocketReceive { get; set; }

        public event EventHandler<ConnectingEventArgs> HttpConnecting;

        public event EventHandler<EventActionRegistingArgs> ActionRegisting;

        public event EventHandler<ConnectedEventArgs> HttpConnected;

        public event EventHandler<EventHttpRequestArgs> HttpRequesting;

        public event EventHandler<EventHttpRequestArgs> HttpRequestNotfound;

        public event EventHandler<EventHttpInnerErrorArgs> HttpInnerError;

        public event EventHandler<SessionEventArgs> HttpDisconnect;

        public event EventHandler<EventHttpServerStartedArgs> Started;

        public event EventHandler<EventActionExecutingArgs> ActionExecuting;

        public event EventHandler<ActionExecutedArgs> ActionExecuted;

        public event EventHandler<EventOptionsReloadArgs> OptionLoad;

        public event EventHandler<EventHttpResponsedArgs> HttpResponsed;

        public event EventHandler<WebSocketConnectArgs> WebSocketConnect;

        private List<System.Reflection.Assembly> mAssemblies = new List<System.Reflection.Assembly>();

        public List<System.Reflection.Assembly> Assemblies => mAssemblies;

        private DispatchCenter<IOQueueProcessArgs> mRequestIOQueues;

        public ISession LogOutput { get; set; }

        public void Register(params System.Reflection.Assembly[] assemblies)
        {
            mAssemblies.AddRange(assemblies);
            try
            {
                mActionFactory.Register(assemblies);
            }
            catch (Exception e_)
            {
                Log(LogType.Error, null, "http api server load controller error " + e_.Message + "@" + e_.StackTrace);
            }
        }

        private void AutoLoad()
        {
            foreach (var item in System.IO.Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.dll"))
            {
                try
                {
                    var assembly = Assembly.LoadFile(item);
                    if (assembly.GetCustomAttribute<AssemblyAutoLoaderAttribute>() != null)
                    {
                        Register(assembly);
                    }
                }
                catch (Exception e)
                {

                }
            }
        }

        public virtual void IncrementResponsed(HttpRequest request, HttpResponse response, double time, int code, string msg)
        {
            try
            {
                System.Threading.Interlocked.Increment(ref mTotalRequests);
                if (code >= 500)
                    System.Threading.Interlocked.Increment(ref mRequestErrors);
                if (HttpResponsed != null)
                {
                    var e = new EventHttpResponsedArgs(request, response, time, code, msg);
                    HttpResponsed(this, e);
                }
            }
            catch (Exception e_)
            {
                if (EnableLog(LogType.Error))
                    Log(LogType.Error, request.Session, $"{request.Session.RemoteEndPoint} {request.Method} {request.Url} responsed event error {e_.Message}@{e_.StackTrace}");
            }
        }



        public async Task Open()
        {

            var result = Task.Run(() =>
             {
                 try
                 {
                     OnOpen();
                 }
                 catch (Exception e_)
                 {
                     Log(LogType.Error, null, $"HTTP open error {e_.Message}@{e_.StackTrace}!");
                 }
             });
            await result;
        }

        private void OnOpen()
        {
            AutoLoad();
            var date = GMTDate.Default.DATE;
            var ct = ContentTypes.TEXT_UTF8;
            var a = HeaderTypeFactory.Find("Content-Length");
            AppDomain.CurrentDomain.AssemblyResolve += ResolveHandler;
            if (Options.WebSocketFrameSerializer != null)
                this.FrameSerializer = Options.WebSocketFrameSerializer;
            HttpPacket hp = new HttpPacket(this, this.FrameSerializer);
            var gtmdate = GMTDate.Default;
            string serverInfo = $"Server: {Options.ServerTag}\r\n";
            HeaderTypeFactory.SERVAR_HEADER_BYTES = Encoding.ASCII.GetBytes(serverInfo);

            CommandLineArgs commandLineArgs = this.CommandLineParser.GetOption<CommandLineArgs>();
            if (!string.IsNullOrEmpty(commandLineArgs.SSLFile))
            {

                Options.SSL = true;
                Options.CertificateFile = commandLineArgs.SSLFile;
                Options.CertificatePassword = commandLineArgs.SSLPassWord;
            }
            if (!string.IsNullOrEmpty(commandLineArgs.Host))
            {
                Options.Host = commandLineArgs.Host;
            }
            if (commandLineArgs.Port > 0)
            {
                Options.Port = commandLineArgs.Port;
            }
            if (commandLineArgs.SSLPort > 0)
            {
                Options.SSLPort = commandLineArgs.SSLPort;
            }
            if (!string.IsNullOrEmpty(commandLineArgs.Sock))
            {
                Options.SockFile = commandLineArgs.Sock;
            }
            mServer = SocketFactory.CreateTcpServer(this, hp)
                .Setting(o =>
                {
                    o.SyncAccept = Options.SyncAccept;
                    // o.IOQueues = Options.IOQueues;
                    if (Options.SSLOnly)
                        o.DefaultListen.Enabled = false;
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
                    o.PrivateBufferPoolSize = Options.MaxBodyLength;
                    o.MaxWaitMessages = Options.MaxWaitQueue;
                });
            SetBindDomains(Options.BindDomains);
            if (Options.IOQueueEnabled)
            {
                mRequestIOQueues = new DispatchCenter<IOQueueProcessArgs>(OnIOQueueProcess, Options.IOQueues);
            }
            if (Options.SSL)
            {
                mServer.Setting(o =>
                {
                    o.AddListenSSL(Options.CertificateFile, Options.CertificatePassword, Options.SslProtocols, o.DefaultListen.Host, Options.SSLPort);
                });
            }
            UnixSocketUri unixsocket = SocketFactory.GetUnixSocketUrl(Options.SockFile);
            if (unixsocket.IsUnixSocket)
            {
                mServer.Setting(o =>
                {
                    o.AddListen(unixsocket.SockFile, 0);
                });
            }
            Name = "BeetleX Http Server";
            if (mAssemblies != null)
            {
                foreach (System.Reflection.Assembly assembly in mAssemblies)
                {
                    try
                    {
                        if (assembly.GetCustomAttribute<NotLoadResourceAttribute>() == null)
                        {
                            mResourceCenter.LoadManifestResource(assembly);
                        }
                    }
                    catch (Exception e_)
                    {
                        Log(LogType.Warring, null, $"HTTP resource center load {assembly.FullName} assembly error {e_.Message}");
                    }
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
            mServer.WriteLogo = WriteLogo ?? OutputLogo;

            mIPLimit = new IPLimit(this);
            mIPLimit.Load();
            UrlsLimit = new UrlLimit<UrlLimitConfig>();
            UrlsLimit.Load();
            DomainsLimit = new UrlLimit<DomainLimitConfig>();
            DomainsLimit.Load();
            mServer.Open();
            mServerCounter = new ServerCounter(this);
            mUrlRewrite.Load();
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

            OnOptionLoad(new EventOptionsReloadArgs { HttpApiServer = this, HttpOptions = this.Options });
            OnStrated(new EventHttpServerStartedArgs { HttpApiServer = this });
            if (Options.Virtuals != null)
            {
                foreach (var item in Options.Virtuals)
                {
                    item.Verify();
                    if (EnableLog(LogType.Info))
                        Log(LogType.Info, null, $"Set virtual folder {item.Folder} to {item.Path}");
                }
            }
            this.LoadActionExts();

        }

        private void LoadActionExts()
        {
            if (!string.IsNullOrEmpty(Options.ActionExt))
            {
                foreach (var item in Options.ActionExt.Split(';'))
                {
                    ActionExts[item] = item;
                }
                AddExts(Options.ActionExt);
            }
        }

        public void AddExts(string exts)
        {
            exts = exts.ToLower();
            ResourceCenter.SetFileExts(exts);
        }

        public void AddVirtualFolder(string folder, string path)
        {
            if (Options.Virtuals == null)
                Options.Virtuals = new List<VirtualFolder>();
            VirtualFolder vf = new VirtualFolder { Folder = folder, Path = path };
            vf.Verify();
            var has = Options.Virtuals.FirstOrDefault(p => string.Compare(p.Folder, vf.Folder, true) == 0);
            if (has == null)
            {
                Options.Virtuals.Add(vf);
            }
            else
            {
                has.Path = vf.Path;
            }
            SaveOptions();
        }

        public void ChangeExtContentType(string ext, string contentType)
        {
            ext = ext.ToLower();
            AddExts(ext);
            ResourceCenter.Exts[ext].ContentType = contentType;
        }

        public Action WriteLogo { get; set; }

        private void OutputLogo()
        {
            AssemblyCopyrightAttribute productAttr = typeof(BeetleX.FastHttpApi.HttpApiServer).Assembly.GetCustomAttribute<AssemblyCopyrightAttribute>();
            var logo = "\r\n";
            logo += " -----------------------------------------------------------------------------\r\n";
            logo +=
@"          ____                  _     _         __   __
         |  _ \                | |   | |        \ \ / /
         | |_) |   ___    ___  | |_  | |   ___   \ V / 
         |  _ <   / _ \  / _ \ | __| | |  / _ \   > <  
         | |_) | |  __/ |  __/ | |_  | | |  __/  / . \ 
         |____/   \___|  \___|  \__| |_|  \___| /_/ \_\ 

                            http and websocket framework   

";
            logo += " -----------------------------------------------------------------------------\r\n";
            logo += $" {productAttr.Copyright}\r\n";
            logo += $" ServerGC    [{GCSettings.IsServerGC}]\r\n";
            logo += $" BeetleX     Version [{typeof(BeetleX.BXException).Assembly.GetName().Version}]\r\n";
            logo += $" FastHttpApi Version [{ typeof(HttpApiServer).Assembly.GetName().Version}] \r\n";
            logo += " -----------------------------------------------------------------------------\r\n";
            foreach (var item in mServer.Options.Listens)
            {
                if (item.Enabled)
                    logo += $" {item}\r\n";
            }
            logo += " -----------------------------------------------------------------------------\r\n";

            Log(LogType.Info, null, logo);


        }

        public void ActionSettings(ActionHandler handler)
        {
            mActionSettings.SetAction(handler);
        }

        public void SaveActionSettings()
        {
            try
            {
                mActionSettings.Save(ActionFactory.Handlers);
                if (EnableLog(LogType.Info))
                    Log(LogType.Info, null, $"HTTP save actions settings success");
            }
            catch (Exception e_)
            {
                if (EnableLog(LogType.Error))
                    Log(LogType.Error, null, $"HTTP save actions settings error {e_.Message}@{e_.StackTrace}");
            }
        }

        public Assembly ResolveHandler(object sender, ResolveEventArgs args)
        {
            try
            {
                //  Log(LogType.Info, $"{args.RequestingAssembly.FullName} load assembly {args.Name}");
                string path = System.IO.Path.GetDirectoryName(args.RequestingAssembly.Location) + System.IO.Path.DirectorySeparatorChar;
                string name = args.Name.Substring(0, args.Name.IndexOf(','));
                string file = path + name + ".dll";
                Assembly result = Assembly.LoadFile(file);
                return result;
            }
            catch (Exception e_)
            {
                if (EnableLog(LogType.Warring))
                    Log(LogType.Warring, null, $"{args.RequestingAssembly.FullName} load assembly {args.Name} error {e_.Message}");
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
            token.HttpRpsLimit = new RpsLimit(Options.SessionMaxRps);
            token.WSRpsLimit = new RpsLimit(Options.WebSocketSessionMaxRps);
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
                    token.WebSocketData?.Dispose();
                }

                if (LogOutput == e.Session)
                    LogOutput = null;
                HttpDisconnect?.Invoke(server, e);
                SessionControllerFactory.DisposedFactory(e.Session);
                base.Disconnect(server, e);
            }
            finally
            {
                e.Session.Tag = null;
            }
        }

        //public void Log(LogType type, string message, params object[] parameters)
        //{
        //    Log(type, null, string.Format(message, parameters));
        //}

        //public void Log(LogType type, string tag, string message, params object[] parameters)
        //{
        //    Log(type, tag, string.Format(message, parameters));
        //}

        public void Log(LogType type, ISession session, object tag, string message)
        {
            try
            {
                Log(null, new HttpServerLogEventArgs(tag, message, type, session));
            }
            catch { }
        }

        public void Log(LogType type, ISession session, string message)
        {
            Log(type, session, null, message);
        }

        public override void Connecting(IServer server, ConnectingEventArgs e)
        {
            if (server.Count > Options.MaxConnections)
            {
                e.Cancel = true;
                if (EnableLog(LogType.Info))
                {
                    Log(LogType.Info, null, $"HTTP ${e.Socket.RemoteEndPoint} out of max connections!");
                }
            }
            if (e.Socket.RemoteEndPoint is IPEndPoint ipPoint)
            {
                e.Socket.NoDelay = true;
            }
            HttpConnecting?.Invoke(this, e);

        }

        public override void Error(IServer server, ServerErrorEventArgs e)
        {
            base.Error(server, e);
        }


        #region websocket
        public virtual object FrameDeserialize(DataFrame data, PipeStream stream, HttpRequest request)
        {
            //
            DataBuffer<byte> buffer = new DataBuffer<byte>((int)data.Length);
            stream.Read(buffer.Data, 0, buffer.Length);
            return buffer;
            //return stream.ReadString((int)data.Length);
        }

        private System.Collections.Concurrent.ConcurrentQueue<byte[]> mBuffers = new System.Collections.Concurrent.ConcurrentQueue<byte[]>();

        public virtual ArraySegment<byte> FrameSerialize(DataFrame data, object body, HttpRequest request)
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
            token.WebSocket = true;
            token.WebSocketData = new PipeStream(this.Server.ReceiveBufferPool.Next());
            token.WebSocketJsonSerializer = new JsonSerializer();
            request.WebSocket = true;
            if (EnableLog(LogType.Info))
            {
                mServer.Log(LogType.Info, request.Session, $"HTTP {request.ID} {request.Session.RemoteEndPoint} upgrade to websocket");
            }
            ConnectionUpgradeWebsocket(request, response);

        }

        protected virtual void ConnectionUpgradeWebsocket(HttpRequest request, HttpResponse response)
        {
            WebSocketConnectArgs wsca = new WebSocketConnectArgs(request, response);
            wsca.Request = request;
            WebSocketConnect?.Invoke(this, wsca);
            if (wsca.Cancel)
            {
                if (EnableLog(LogType.Warring))
                {
                    mServer.Log(LogType.Warring, request.Session, $"HTTP {request.ID} {request.Session.RemoteEndPoint} cancel upgrade to websocket");
                }
                if (wsca.Error == null)
                    wsca.Error = new UpgradeWebsocketError(500, "websocket upgrade cancel");
                response.Result(wsca.Error);
                Close(request.Session);

            }
            else
            {
                if (wsca.UpgradeSuccess != null)
                {
                    response.Result(wsca.UpgradeSuccess);
                }
                else
                {
                    UpgradeWebsocketSuccess upgradeWebsocket = new UpgradeWebsocketSuccess(request.Header[HeaderTypeFactory.SEC_WEBSOCKET_KEY]);
                    response.Result(upgradeWebsocket);
                }
            }
        }

        public virtual void ExecuteWS(HttpRequest request, HttpToken httpToken)
        {
            using (httpToken.WebSocketData.LockFree())
            {
                using (JsonTextReader reader = new JsonTextReader(new StreamReader(httpToken.WebSocketData)))
                {
                    JToken dataToken = (JToken)httpToken.WebSocketJsonSerializer.Deserialize(reader);
                    this.mActionFactory.ExecuteWS(request, this, dataToken);
                }
            }
        }

        public DataFrame CreateTextFrame(object body)
        {
            var result = CreateDataFrame(body);
            result.Type = DataPacketType.text;
            return result;
        }

        public DataFrame CreateBinaryFrame(object body)
        {
            var result = CreateDataFrame(body);
            result.Type = DataPacketType.binary;
            return result;
        }
        public DataFrame CreateDataFrame(object body = null)
        {
            DataFrame dp = new DataFrame(this);
            dp.DataPacketSerializer = this.FrameSerializer;
            dp.Body = body;
            return dp;
        }

        protected virtual void OnWebSocketRequest(HttpRequest request, ISession session, DataFrame data)
        {
            HttpToken token = (HttpToken)session.Tag;
            if (token.WSRpsLimit.Check(this.Options.SessionMaxRps))
            {
                var frame = CreateTextFrame("session max rps limit!");
                session.Send(frame);
                return;
            }

            if (EnableLog(LogType.Info))
            {
                mServer.Log(LogType.Info, session, $"Websocket {request.ID} {request.RemoteIPAddress} receive {data.Type.ToString()}");
            }

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
                if (AllRpsLimit.Check(this.Options.MaxRps))
                {
                    var frame = CreateTextFrame("server max rps limit!");
                    session.Send(frame);
                    return;
                }

                DataBuffer<byte> databuffer = data.Body as DataBuffer<byte>;
                try
                {
                    if (data.Type == DataPacketType.text || data.Type == DataPacketType.binary)
                    {
                        token.WSLastPacketType = data.Type;
                    }
                    if (WebSocketReceive == null)
                    {
                        if (data.Type == DataPacketType.text || (data.Type == DataPacketType.continuation && token.WSLastPacketType == DataPacketType.text))
                        {
                            token.WebSocketData?.Write(databuffer.Data, 0, databuffer.Length);
                            if (data.FIN)
                            {
                                token.WebSocketData.Position = 0;
                                try
                                {
                                    ExecuteWS(token.Request, token);
                                }
                                finally
                                {
                                    if (token.WebSocketData.Length > 0)
                                        token.WebSocketData.ReadFree((int)token.WebSocketData.Length);
                                }
                            }
                        }
                        else
                        {
                            var args = new WebSocketReceiveArgs();
                            args.Frame = data;
                            args.Sesson = session;
                            args.Server = this;
                            args.Request = token.Request;
                            WebSocketReceive?.Invoke(this, args);
                        }
                    }
                    else
                    {
                        var args = new WebSocketReceiveArgs();
                        args.Frame = data;
                        args.Sesson = session;
                        args.Server = this;
                        args.Request = token.Request;
                        WebSocketReceive?.Invoke(this, args);
                    }

                }
                finally
                {
                    databuffer?.Dispose();
                }

            }

        }

        #endregion

        private void CacheLog(ServerLogEventArgs e)
        {
            if (Options.CacheLogMaxSize > 0)
            {
                HttpToken token = e.Session != null ? (HttpToken)e.Session.Tag : null;
                string removip = token?.Request?.RemoteEndPoint;
                if (removip == null)
                    removip = "SYSTEM";
                if (Options.CacheLogFilter != null)
                {
                    if (removip.IndexOf(Options.CacheLogFilter) == -1)
                    {
                        return;
                    }
                }

                LogRecord record = new LogRecord();
                record.Type = e.Type.ToString();
                record.RemoveIP = token?.Request?.RemoteEndPoint;
                record.RemoveIP = removip;
                record.Message = e.Message;
                record.Time = DateTime.Now.ToString("H:mm:ss");
                System.Threading.Interlocked.Increment(ref mCacheLogLength);
                if (mCacheLogLength > Options.CacheLogMaxSize)
                {
                    mCacheLogQueue.TryDequeue(out LogRecord log);
                    System.Threading.Interlocked.Decrement(ref mCacheLogLength);
                }
                mCacheLogQueue.Enqueue(record);
            }
        }

        public override void Log(IServer server, ServerLogEventArgs e)
        {
            var httpLog = e as HttpServerLogEventArgs;
            HttpToken token = e.Session != null ? (HttpToken)e.Session.Tag : null;
            CacheLog(e);
            ServerLog?.Invoke(server, e);
            if (Options.LogToConsole && (httpLog == null || httpLog.OutputConsole))
                base.Log(server, e);
            if (Options.WriteLog && (httpLog == null || httpLog.OutputFile))
            {
                var endPoint = token?.Request?.RemoteEndPoint;
                if (endPoint == null)
                    endPoint = e.Session?.RemoteEndPoint.ToString();
                var localPoint = token?.Request?.Session?.Socket?.LocalEndPoint.ToString();
                if (localPoint == null)
                    localPoint = e.Session?.Socket.LocalEndPoint.ToString();
                mFileLog.Add(endPoint + "/" + localPoint, e.Type, e.Message);
            }
            ISession output = LogOutput;
            if (output != null && e.Session != output)
            {
                ActionResult log = new ActionResult();
                log.Data = new { LogType = e.Type.ToString(), Time = DateTime.Now.ToString("H:mm:ss"), Message = e.Message };
                CreateDataFrame(log).Send(output);
            }
        }

        private object mLockConsole = new object();

        protected override void OnLogToConsole(IServer server, ServerLogEventArgs e)
        {
            lock (mLockConsole)
            {
                HttpToken token = e.Session != null ? (HttpToken)e.Session.Tag : null;
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($">>{ DateTime.Now.ToString("HH:mmm:ss")}");
                switch (e.Type)
                {
                    case LogType.Error:
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        break;
                    case LogType.Warring:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;
                    case LogType.Fatal:
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    case LogType.Info:
                        Console.ForegroundColor = ConsoleColor.Green;
                        break;
                    default:
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                }
                Console.Write($" [{e.Type.ToString().PadRight(7)}]");
                var endPoint = token?.Request?.RemoteEndPoint;
                if (endPoint == null)
                    endPoint = e.Session?.RemoteEndPoint.ToString();
                var localPoint = token?.Request?.Session?.Socket?.LocalEndPoint.ToString();
                if (localPoint == null)
                    localPoint = e.Session?.Socket.LocalEndPoint.ToString();
                if (endPoint == null)
                {
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                    Console.Write($" [SYSTEM] ");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write($" {endPoint}/{localPoint} ");
                }
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine(e.Message);
            }
        }

        protected virtual void OnOptionLoad(EventOptionsReloadArgs e)
        {
            if (EnableLog(LogType.Debug))
                Log(LogType.Debug, null, "HTTP server options reload event!");
            OptionLoad?.Invoke(this, e);
        }

        protected virtual void OnStrated(EventHttpServerStartedArgs e)
        {
            if (EnableLog(LogType.Debug))
                Log(LogType.Debug, null, "HTTP server started event!");
            Started?.Invoke(this, e);
        }

        protected virtual void OnProcessResource(HttpRequest request, HttpResponse response)
        {

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
                        //InnerErrorResult result = new InnerErrorResult($"response file error ", e_, Options.OutputStackTrace);
                        //response.Result(result);
                        response.InnerError($"response file error!", e_, Options.OutputStackTrace);
                    }
                }
                else
                {
                    if (EnableLog(LogType.Info))
                        Log(LogType.Info, request.Session, $"{request.RemoteIPAddress}{request.Method} {request.Url} not support");
                    //NotSupportResult notSupport = new NotSupportResult($"{request.Method} {request.Url} not support");
                    //response.Result(notSupport);
                    response.InnerError("403", $"{request.Method} method not support!");
                }
            }
            finally
            {
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
                if (!e.Request.Session.IsDisposed)
                    OnHttpRequest(e.Request, e.Response);
            }
            catch (Exception e_)
            {
                if (EnableLog(LogType.Error))
                {
                    mServer.Error(e_, e.Request.Session, $"HTTP {e.Request.ID} {e.Request.RemoteIPAddress} on queue process error {e_.Message}@{e_.StackTrace}");
                }
            }
        }


        private void OnRequestHandler(PacketDecodeCompletedEventArgs e)
        {
            try
            {
                if (e.Session.IsDisposed)
                    return;
                HttpToken token = (HttpToken)e.Session.Tag;

                if (token.WebSocket)
                {
                    OnWebSocketRequest(token.Request, e.Session, (WebSockets.DataFrame)e.Message);
                }
                else
                {
                    HttpRequest request = (HttpRequest)e.Message;

                    if (EnableLog(LogType.Info))
                    {
                        mServer.Log(LogType.Info, e.Session, $"HTTP {request.ID} {request.RemoteIPAddress} {request.Method} {request.Url}");
                    }
                    if (EnableLog(LogType.Debug))
                    {
                        mServer.Log(LogType.Debug, e.Session, $"HTTP {request.ID} {request.RemoteIPAddress} {request.Method} {request.Url} detail {request.ToString()}");
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
                    mServer.Error(e_, e.Session, $"HTTP {e.Session.RemoteEndPoint} {0} OnRequestHandler error {e_.Message}@{e_.StackTrace}");
                }
            }
        }

        public override void SessionPacketDecodeCompleted(IServer server, PacketDecodeCompletedEventArgs e)
        {
            if (Options.SessionTimeOut > 0)
            {
                BaseServer.UpdateSession(e.Session);
            }
            OnRequestHandler(e);
        }

        internal void OnActionRegisting(EventActionRegistingArgs e)
        {
            ActionRegisting?.Invoke(this, e);
        }

        internal bool OnActionExecuting(IHttpContext context, ActionHandler handler)
        {
            if (ActionExecuting != null)
            {
                EventActionExecutingArgs e = new EventActionExecutingArgs();
                e.HttpContext = context;
                e.Handler = handler;
                ActionExecuting(this, e);
                return !e.Cancel;
            }
            return true;
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

        private void OnOutputLicense(HttpRequest request, HttpResponse response)
        {
            var name = request.Data["name"];
            var info = GetLicense(name);
            if (info == null)
                info = new Dictionary<string, string>();
            JsonResult result = new JsonResult(info);
            response.Result(result);
        }
        private bool CheckUrlLimit(HttpRequest request, HttpResponse response)
        {
            var url = request.BaseUrl;
            if (string.IsNullOrEmpty(url))
                return true;
            if (UrlsLimit.Count == 0)
                return true;
            var limit = UrlsLimit.Match(url, request);
            if (!limit.ValidateRPS())
            {
                response.InnerError("509", $"{url} max rps limit!");
                return false;
            }
            return true;
        }
        private bool CheckDomainsLimit(HttpRequest request, HttpResponse response)
        {
            var url = request.Host;
            if (string.IsNullOrEmpty(url))
                return true;
            if (DomainsLimit.Count == 0)
                return true;
            var limit = DomainsLimit.Match(url, request);
            if (!limit.ValidateRPS())
            {
                response.InnerError("509", $"{url} max rps limit!");
                return false;
            }
            return true;
        }
        private bool CheckIPTable(HttpRequest request, HttpResponse response)
        {
            if (IPv4Tables.Type == IPv4Tables.VerifyType.None)
                return true;
            if (!IPv4Tables.Verify(request.RemoteIPAddress))
            {
                var msg = $"HTTP ${request.RemoteIPAddress} IP tables verify no permission!";
                response.InnerError("509", msg);
                return false;
            }
            return true;

        }
        protected virtual void OnHttpRequest(HttpRequest request, HttpResponse response)
        {
            string baseUrl = request.BaseUrl;
            if (baseUrl.Length == LICENSE_URL.Length)
            {
                if (baseUrl[1] == '_' && baseUrl[2] == '_')
                {
                    if (string.Compare(baseUrl, LICENSE_URL, true) == 0)
                    {
                        OnOutputLicense(request, response);
                        return;
                    }
                }
            }
            if (!CheckIPTable(request, response))
                return;
            if (!CheckDomains(request))
            {
                if (string.IsNullOrEmpty(Options.InvalidDomainUrl))
                {
                    response.InnerError("509", "Invalid domain name!");
                }
                else
                {
                    Move302Result result = new Move302Result(Options.InvalidDomainUrl);
                    response.Result(result);
                }
                return;
            }
            HttpToken token = (HttpToken)request.Session.Tag;
            if (token.HttpRpsLimit.Check(this.Options.SessionMaxRps))
            {
                response.InnerError("509", "session max rps limit!");
                return;
            }
            if (!mIPLimit.ValidateRPS(request))
            {
                response.InnerError("509", $"{request.RemoteIPAddress} max rps limit!");
                return;
            }
            if (!CheckUrlLimit(request, response))
                return;
            if (!CheckDomainsLimit(request, response))
                return;
            if (RpsLimitHandlers.Count > 0)
                foreach (var handler in RpsLimitHandlers.Values)
                {
                    if (handler.Check(request, response))
                    {
                        response.InnerError("509", $"{handler.Name} max rps limit!");
                        return;
                    }
                }

            if (AllRpsLimit.Check(this.Options.MaxRps))
            {
                response.InnerError("509", "server max rps limit!");
                return;
            }


            if (!OnHttpRequesting(request, response).Cancel)
            {
                if (OnExecuteMap(request))
                {
                    return;
                }
                if ((string.IsNullOrEmpty(request.Ext) || ActionExts.ContainsKey(request.Ext)) && baseUrl[baseUrl.Length - 1] != '/')
                {
                    mActionFactory.Execute(request, response, this);
                }
                else
                {
                    OnProcessResource(request, response);
                }
            }
        }

        internal void OnInnerError(HttpResponse response, string code, string message, Exception e, bool outputStackTrace)
        {
            if (HttpInnerError != null)
            {
                EventHttpInnerErrorArgs error = new EventHttpInnerErrorArgs
                {
                    Code = code,
                    Message = message,
                    Error = e,
                    Request = response.Request,
                    Response = response
                };
                HttpInnerError.Invoke(this, error);
                if (error.Cancel)
                    return;
            }
            var result = new InnerErrorResult(code, message, e, outputStackTrace);
            response.Result(result);

        }

        public virtual void ReceiveCompleted(ISession session, SocketAsyncEventArgs e)
        {

        }

        public override void SessionDetection(IServer server, SessionDetectionEventArgs e)
        {
            base.SessionDetection(server, e);
        }


        public virtual void SendCompleted(ISession session, SocketAsyncEventArgs e, bool end)
        {
            if (end)
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

        public async void Close(ISession session, int delay = 1000)
        {
            try
            {
                await Task.Delay(delay);
                session?.Dispose();
            }
            catch (Exception e_)
            {

            }
        }
        public HttpApiServer GetLog(LogType logType)
        {
            if (EnableLog(logType))
                return this;
            return null;
        }

        public Validations.IValidationOutputHandler ValidationOutputHandler { get; set; } = new Validations.ValidationOutputHandler();

        public void Dispose()
        {
            if (BaseServer != null)
                BaseServer.Dispose();
        }

        internal void OnActionExecutedError(IHttpContext context, ActionHandler handler, Exception error, int code, long startTime)
        {
            if (ActionExecuted != null)
            {
                ActionExecutedArgs e = new ActionExecutedArgs();
                if (context.WebSocket)
                {
                    e.ServerType = "Websocket";
                }
                else
                {
                    e.ServerType = "HTTP";
                    e.HTTPMethod = context.Request.Method;
                }
                e.Code = code;
                e.Exception = error;
                e.Headers = context.Request.Header.Copy();
                e.ActionHandler = handler;
                e.Url = context.Request.Url;
                e.UseTime = context.Server.BaseServer.GetRunTime() - startTime;
                ActionExecuted(this, e);
            }
        }
        internal void OnActionExecutedSuccess(IHttpContext context, ActionHandler handler, long startTime)
        {
            if (ActionExecuted != null)
            {
                ActionExecutedArgs e = new ActionExecutedArgs();
                if (context.WebSocket)
                {
                    e.ServerType = "Websocket";
                }
                else
                {
                    e.ServerType = "HTTP";
                    e.HTTPMethod = context.Request.Method;
                }
                e.Headers = context.Request.Header.Copy();
                e.ActionHandler = handler;
                e.Url = context.Request.Url;
                e.UseTime = context.Server.BaseServer.GetRunTime() - startTime;
                ActionExecuted(this, e);
            }
        }
    }
}
