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

namespace BeetleX.FastHttpApi
{
    public class HttpApiServer : ServerHandlerBase, BeetleX.ISessionSocketProcessHandler, WebSockets.IDataFrameSerializer, IWebSocketServer
    {

        public HttpApiServer() : this(null)
        { }

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

        private ActionHandlerFactory mActionFactory;

        private System.Collections.Concurrent.ConcurrentDictionary<string, object> mProperties = new System.Collections.Concurrent.ConcurrentDictionary<string, object>();

        public EventHttpServerLog ServerLog { get; set; }

        public IServer BaseServer => mServer;

        public HttpConfig ServerConfig { get; set; }

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

        public EventHandler<WebSocketReceiveArgs> WebSocketReceive { get; set; }

        public EventHandler<ConnectedEventArgs> HttpConnected { get; set; }

        public EventHandler<SessionEventArgs> HttpDisconnect { get; set; }

        private System.Reflection.Assembly[] mAssemblies;

        public void Register(params System.Reflection.Assembly[] assemblies)
        {
            mAssemblies = assemblies;
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
            if (!string.IsNullOrEmpty(config.CertificateFile))
                config.SSL = true;
            config.LittleEndian = false;
            HttpPacket hp = new HttpPacket(this.ServerConfig.BodySerializer, this.ServerConfig, this);
            mServer = SocketFactory.CreateTcpServer(config, this, hp);
            mServer.Open();
            if (mAssemblies != null)
            {
                foreach (System.Reflection.Assembly assembly in mAssemblies)
                {
                    mResourceCenter.LoadManifestResource(assembly);
                }
            }
            mResourceCenter.Path = ServerConfig.StaticResourcePath;
            mResourceCenter.Debug = ServerConfig.Debug;
            mResourceCenter.Load();
            mServer.Name = "FastHttpApi Http Server";

        }

        public void SendDataFrame(DataFrame data)
        {
            foreach (ISession item in BaseServer.GetOnlines())
            {
                SendDataFrame(data, item);
            }
        }

        public void SendDataFrame(DataFrame data, params long[] sessionid)
        {
            if (sessionid != null)
            {
                foreach (var item in sessionid)
                {
                    SendDataFrame(data, BaseServer.GetSession(item));
                }
            }
        }

        public void SendDataFrame(DataFrame data, ISession session)
        {
            if (session == null)
                return;
            HttpToken toke = (HttpToken)session.Tag;
            if (toke.WebSocket)
            {
                session.Send(data);
            }
        }

        public override void Connected(IServer server, ConnectedEventArgs e)
        {
            e.Session.Tag = new HttpToken();
            e.Session.SocketProcessHandler = this;
            HttpConnected?.Invoke(server, e);
        }

        public override void Disconnect(IServer server, SessionEventArgs e)
        {
            HttpDisconnect?.Invoke(server, e);
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

        }

        #region websocket
        public object FrameDeserialize(DataFrame data, PipeStream stream)
        {
            return stream.ReadString((int)data.Length);
        }

        private System.Collections.Concurrent.ConcurrentQueue<byte[]> mBuffers = new System.Collections.Concurrent.ConcurrentQueue<byte[]>();

        public ArraySegment<byte> FrameSerialize(DataFrame data, object body)
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

        public void FrameRecovery(byte[] buffer)
        {
            mBuffers.Enqueue(buffer);
        }

        private void OnWebSocketConnect(HttpRequest request, HttpResponse response)
        {
            HttpToken token = (HttpToken)request.Session.Tag;
            token.KeepAlive = true;
            token.WebSocketRequest = request;
            token.WebSocket = true;
            ConnectionUpgradeWebsocket(request, response);

        }

        protected virtual void ConnectionUpgradeWebsocket(HttpRequest request, HttpResponse response)
        {
            response.ConnectionUpgradeWebsocket(request.Header[HeaderType.SEC_WEBSOCKET_KEY]);
            request.Session.Send(response);
        }


        public ActionResult ExecuteWS(HttpRequest request, DataFrame dataFrame)
        {
            JToken dataToken = (JToken)Newtonsoft.Json.JsonConvert.DeserializeObject((string)dataFrame.Body);
            return this.mActionFactory.ExecuteWithWS(request, this, dataToken);
        }


        public DataFrame CreateDataFrame(object body = null)
        {
            DataFrame dp = new DataFrame();
            dp.DataPacketSerializer = this;
            dp.Body = body;
            return dp;
        }

        protected virtual void OnReceiveWebSocketData(ISession session, DataFrame data)
        {
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
                var args = new WebSocketReceiveArgs();
                args.Frame = data;
                args.Sesson = session;
                args.Server = this;
                args.Request = token.WebSocketRequest;
                WebSocketReceive?.Invoke(this, args);
            }
        }

        #endregion



        public override void Log(IServer server, ServerLogEventArgs e)
        {
            if (ServerLog == null)
                base.Log(server, e);
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
            HttpToken token = (HttpToken)e.Session.Tag;
            if (token.WebSocket)
            {
                OnReceiveWebSocketData(e.Session, (WebSockets.DataFrame)e.Message);
            }
            else
            {
                HttpRequest request = (HttpRequest)e.Message;
                request.Server = this;
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
    }
}
