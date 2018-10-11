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

namespace BeetleX.FastHttpApi
{
    public class HttpApiServer : ServerHandlerBase, BeetleX.ISessionSocketProcessHandler, WebSockets.IDataFrameSerializer, IWebSocketServer
    {

        public HttpApiServer() : this(null)
        {
            mFileLog = new FileLog();
            FrameSerializer = this;
        }

        public HttpApiServer(HttpConfig serverConfig)
        {
            mActionFactory = new ActionHandlerFactory();
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
            mResourceCenter = new StaticResurce.ResourceCenter(this);
        }

        private StaticResurce.ResourceCenter mResourceCenter;

        private IServer mServer;

        private FileLog mFileLog;

        private long mRequests;

        private ActionHandlerFactory mActionFactory;

        private System.Collections.Concurrent.ConcurrentDictionary<string, object> mProperties = new System.Collections.Concurrent.ConcurrentDictionary<string, object>();

        public EventHttpServerLog ServerLog { get; set; }

        public IServer BaseServer => mServer;

        public HttpConfig ServerConfig { get; set; }

        public IDataFrameSerializer FrameSerializer { get; set; }

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

        public long Request => mRequests;

        public EventHandler<WebSocketReceiveArgs> WebSocketReceive { get; set; }

        public event EventHandler<ConnectedEventArgs> HttpConnected;

        public event EventHandler<SessionEventArgs> HttpDisconnect;

        public EventHandler<WebSocketConnectArgs> WebSocketConnect { get; set; }

        private List<System.Reflection.Assembly> mAssemblies = new List<System.Reflection.Assembly>();

        public void Register(params System.Reflection.Assembly[] assemblies)
        {
            mAssemblies.AddRange(assemblies);
            try
            {
                mActionFactory.Register(this.ServerConfig, this, assemblies);
            }
            catch (Exception e_)
            {
                Log(LogType.Error, " http api server load controller error " + e_.Message);
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
            if (!string.IsNullOrEmpty(config.CertificateFile))
                config.SSL = true;
            config.LittleEndian = false;
            HttpPacket hp = new HttpPacket(this.ServerConfig.BodySerializer, this.ServerConfig, this);
            mServer = SocketFactory.CreateTcpServer(config, this, hp);
            Name = "FastHttpApi Http Server";
            Admin.AdminController aic = new Admin.AdminController();
            aic.HandleFactory = mActionFactory;
            aic.Server = this;
            mActionFactory.Register(ServerConfig, this, aic);
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
            if (EnableLog(LogType.Info))
            {
                mServer.Log(LogType.Info, null, "FastHttpApi Server started!");
            }
        }

        public string Name { get { return mServer.Name; } set { mServer.Name = value; } }



        public void SendToWebSocket(DataFrame data, Func<ISession, HttpRequest, bool> filter = null)
        {
            foreach (HttpRequest item in GetWebSockets())
            {
                if ((filter == null || filter(item.Session, item)))
                    SendToWebSocket(data, item);
            }
        }

        public void SendToWebSocket(DataFrame data, params HttpRequest[] request)
        {
            if (request != null)
                foreach (HttpRequest item in request)
                {
                    OnSendToWebSocket(data, item);
                }
        }

        private void OnSendToWebSocket(DataFrame data, HttpRequest request)
        {
            data.Send(request.Session);

        }

        public override void Connected(IServer server, ConnectedEventArgs e)
        {
            e.Session.Tag = new HttpToken();
            e.Session.SocketProcessHandler = this;
            HttpConnected?.Invoke(server, e);
            base.Connected(server, e);
        }

        public override void Disconnect(IServer server, SessionEventArgs e)
        {
            try
            {
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
                response.ConnectionUpgradeWebsocket(request.Header[HeaderType.SEC_WEBSOCKET_KEY]);
                request.Session.Send(response);
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

        protected virtual void OnReceiveWebSocketData(ISession session, DataFrame data)
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
        }

        protected virtual void OnProcessResource(HttpRequest request, HttpResponse response)
        {
            if (string.Compare(request.Method, "GET", true) == 0)
            {
                mResourceCenter.ProcessFile(request, response);
            }
            else
            {
                response.NotSupport();
            }
        }

        public override void SessionPacketDecodeCompleted(IServer server, PacketDecodeCompletedEventArgs e)
        {
            System.Threading.Interlocked.Increment(ref mRequests);
            HttpToken token = (HttpToken)e.Session.Tag;
            if (token.WebSocket)
            {
                OnReceiveWebSocketData(e.Session, (WebSockets.DataFrame)e.Message);
            }
            else
            {

                HttpRequest request = (HttpRequest)e.Message;
                if (EnableLog(LogType.Info))
                {
                    mServer.Log(LogType.Info, e.Session, "{0} Http {1} {2}",request.ClientIPAddress, request.Method, request.Url);
                }
                if(EnableLog(LogType.Debug))
                {
                    mServer.Log(LogType.Info, e.Session, "{0} {1}", request.ClientIPAddress, request.ToString());
                }
                request.Server = this;
                if (request.ClientIPAddress == null)
                {
                    request.Header.Add(HeaderType.CLIENT_IPADDRESS, ((IPEndPoint)e.Session.RemoteEndPoint).Address.ToString());
                }
                HttpResponse response = request.CreateResponse();
                token.KeepAlive = request.KeepAlive;
                if (token.FirstRequest && string.Compare(request.Header[HeaderType.UPGRADE], "websocket", true) == 0)
                {
                    token.FirstRequest = false;
                    OnWebSocketConnect(request, response);
                }
                else
                {
                    token.FirstRequest = false;
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
            if (session.SendMessages == 0 && !token.KeepAlive)
            {
                session.Dispose();
            }
        }

        private long mVersion;

        private IEnumerable<HttpRequest> mOnlines = new HttpRequest[0];

        public IEnumerable<HttpRequest> GetWebSockets()
        {
            if (mVersion != BaseServer.Version)
            {
                mVersion = BaseServer.Version;
                ISession[] items = BaseServer.GetOnlines();
                mOnlines = from s in items
                           where ((HttpToken)s.Tag).WebSocket
                           select ((HttpToken)s.Tag).WebSocketRequest;
            }
            return mOnlines;
        }

        public bool EnableLog(LogType logType)
        {
            return (int)(this.ServerConfig.LogLevel) <= (int)logType;
        }
    }
}
