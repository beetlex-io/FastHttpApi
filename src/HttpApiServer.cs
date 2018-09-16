using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using BeetleX.EventArgs;
using Microsoft.Extensions.Configuration;

namespace BeetleX.FastHttpApi
{
    public class HttpApiServer : ServerHandlerBase, BeetleX.ISessionSocketProcessHandler
    {

        public HttpApiServer(HttpConfig serverConfig = null)
        {
            mActionFactory = new ActionHandlerFactory();
            if (serverConfig != null)
            {
                ServerConfig = serverConfig;
            }
            else
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("HttpConfig.json", false);
                var configuration = builder.Build();
                ServerConfig = configuration.GetSection("HttpConfig").Get<HttpConfig>();
            }
        }

        private IServer mServer;

        private ActionHandlerFactory mActionFactory;

        public IServer BaseServer => mServer;

        public HttpConfig ServerConfig { get; set; }

        public void Register(params System.Reflection.Assembly[] assemblies)
        {
            try
            {
                mActionFactory.Register(this.ServerConfig, assemblies);
            }
            catch (Exception e_)
            {
                Log(null, new ServerLogEventArgs(" http api server load controller error " + e_.Message, LogType.Error));
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
            HttpPacket hp = new HttpPacket(this.ServerConfig.BodySerializer, this.ServerConfig);
            mServer = SocketFactory.CreateTcpServer(config, this, hp);
            mServer.Open();

        }


        public override void Connected(IServer server, ConnectedEventArgs e)
        {
            e.Session.SocketProcessHandler = this;

        }

        public override void Disconnect(IServer server, SessionEventArgs e)
        {

        }

        public override void Connecting(IServer server, ConnectingEventArgs e)
        {

        }
        public override void Log(IServer server, ServerLogEventArgs e)
        {
            base.Log(server, e);
        }
        public override void SessionPacketDecodeCompleted(IServer server, PacketDecodeCompletedEventArgs e)
        {
            HttpRequest request = (HttpRequest)e.Message;
            if (!request.KeepAlive)
            {
                e.Session.Tag = "close";
            }
            HttpResponse response = request.CreateResponse();
            mActionFactory.Execute(request, response, this);
        }

        public virtual void ReceiveCompleted(ISession session, SocketAsyncEventArgs e)
        {

        }

        public virtual void SendCompleted(ISession session, SocketAsyncEventArgs e)
        {
            if (session.SendMessages == 0 && session.Tag != null)
            {
                session.Dispose();
            }
        }
    }
}
