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
            mResourceCenter = new StaticResurce.ResourceCenter(this);
        }

        private StaticResurce.ResourceCenter mResourceCenter;

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
            mResourceCenter.Load();

        }


        public override void Connected(IServer server, ConnectedEventArgs e)
        {
            e.Session.Tag = new HttpToken();
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
            HttpRequest request = (HttpRequest)e.Message;
            HttpResponse response = request.CreateResponse();
            token.KeepAlive = request.KeepAlive;
            if (string.IsNullOrEmpty(request.Ext) && request.BaseUrl != "/")
            {
                mActionFactory.Execute(request, response, this);
            }
            else
            {
                OnProcessResource(request, response);
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
