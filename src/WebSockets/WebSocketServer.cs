using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using BeetleX.Buffers;
using BeetleX.EventArgs;
using System.Linq;

namespace BeetleX.FastHttpApi.WebSockets
{
    public class WebSocketServer : BeetleX.ServerHandlerBase, ISessionSocketProcessHandler, IDataFrameSerializer, IWebSocketServer
    {

        public WebSocketServer() : this(null) { }

        public WebSocketServer(HttpConfig serverConfig)
        {
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
        }

        private IServer mServer;

        public EventHandler<WebSocketReceiveArgs> WebSocketReceive { get; set; }

        public EventHttpServerLog ServerLog { get; set; }

        public IServer BaseServer => mServer;

        public HttpConfig ServerConfig { get; set; }

        public override void Connected(IServer server, ConnectedEventArgs e)
        {
            e.Session.Tag = new WebSocketToken();
            e.Session.SocketProcessHandler = this;

        }

        public override void Disconnect(IServer server, SessionEventArgs e)
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
            string value = (string)body;
            int length = ServerConfig.Encoding.GetBytes(value, 0, value.Length, result, 0);
            return new ArraySegment<byte>(result, 0, length);
        }

        public void FrameRecovery(byte[] buffer)
        {
            mBuffers.Enqueue(buffer);
        }

        protected virtual void OnWebSocketConnect(HttpRequest request, HttpResponse response)
        {
            response.ConnectionUpgradeWebsocket(request.Header[HeaderType.SEC_WEBSOCKET_KEY]);
            request.Session.Send(response);
            WebSocketToken token = (WebSocketToken)request.Session.Tag;
            token.Connected = true;

        }

        public DataFrame CreateDataFrame(object body = null)
        {
            DataFrame dp = new DataFrame();
            dp.DataPacketSerializer = this;
            dp.Body = body;
            return dp;
        }

        protected virtual void OnReceiveWebSocketData(HttpRequest request, DataFrame data)
        {
            WebSocketToken token = (WebSocketToken)request.Session.Tag;
            if (data.Type == DataPacketType.ping)
            {
                DataFrame pong = CreateDataFrame();
                pong.Type = DataPacketType.pong;
                pong.FIN = true;
                request.Session.Send(pong);
            }
            else if (data.Type == DataPacketType.connectionClose)
            {
                request.Session.Dispose();
            }
            else
            {
                var args = new WebSocketReceiveArgs();
                args.Frame = data;
                args.Sesson = request.Session;
                args.Server = this;
                args.Request = request;
                WebSocketReceive?.Invoke(this, args);
            }
        }


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
            request.Session.Send(data);

        }

        #endregion




        public override void Log(IServer server, ServerLogEventArgs e)
        {
            if (ServerLog == null)
                base.Log(server, e);
            else
                ServerLog(server, e);
        }

        public override void SessionPacketDecodeCompleted(IServer server, PacketDecodeCompletedEventArgs e)
        {
            WebSocketToken token = (WebSocketToken)e.Session.Tag;
            HttpRequest request = e.Message as HttpRequest;
            if (request != null)
            {
                token.ConnectionRequest = request;
                OnWebSocketConnect(request, request.CreateResponse());
            }
            else
            {
                OnReceiveWebSocketData(token.ConnectionRequest, (DataFrame)e.Message);
            }

        }

        public void Open()
        {
            NetConfig config = new NetConfig();
            config.Host = ServerConfig.Host;
            config.Port = ServerConfig.Port;
            config.CertificateFile = ServerConfig.CertificateFile;
            config.CertificatePassword = ServerConfig.CertificatePassword;
            config.LittleEndian = false;
            if (!string.IsNullOrEmpty(config.CertificateFile))
                config.SSL = true;
            WebSocketPacket hp = new WebSocketPacket(this.ServerConfig, this);
            mServer = SocketFactory.CreateTcpServer(config, this, hp);
            mServer.Open();
            mServer.Name = "FastHttpApi WebSocket Server";
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

        public void ReceiveCompleted(ISession session, SocketAsyncEventArgs e)
        {

        }

        public void SendCompleted(ISession session, SocketAsyncEventArgs e)
        {

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
    }
}
