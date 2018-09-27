using BeetleX.FastHttpApi;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using BeetleX.FastHttpApi.WebSockets;

namespace HttpApiServer.Chat
{
    [ControllerAttribute]
    public class Controller : IController
    {
        public object Onlines(IHttpContext context)
        {
            return from i in context.Server.GetWebSockets()
                   select new { i.Session.ID, i.Session.Name, IPAddress = i.Session.RemoteEndPoint.ToString() };

        }

        private BeetleX.FastHttpApi.HttpApiServer mServer;

        public void Login(string name, IHttpContext context)
        {
            context.Session.Name = name;
            Command cmd = new Command { Name = name, Type = "Login", Message = "" };
            context.ResultToWebSocket(cmd);
        }

        public void SendMessage(string message, IHttpContext context)
        {
            string name;
            if (context.WebSocket)
            {
                name = context.Session.Name;
            }
            else
            {
                name = "http user";
            }
            var msg = new Command { Name = name, Type = "Talk", Message = message };
            context.ResultToWebSocket(msg, (c, r) => c.Name != null);
        }

        private void OnHttpDisconnect(object sender, BeetleX.EventArgs.SessionEventArgs e)
        {
            if (e.Session.Name != null)
            {
                Command cmd = new Command { Name = e.Session.Name, Type = "Quit", Message = "" };
                DataFrame frame = mServer.CreateDataFrame(new ActionResult { Data = cmd });
                mServer.SendToWebSocket(frame, (s, r) => s.Name != null);
            }
        }

        private void OnHttpConnected(object sender, BeetleX.EventArgs.ConnectedEventArgs e)
        {

        }
        [NotAction]
        public void Init(BeetleX.FastHttpApi.HttpApiServer server)
        {
            server.HttpConnected = OnHttpConnected;
            server.HttpDisconnect = OnHttpDisconnect;
            mServer = server;
        }
        public class Command
        {
            public string Name { get; set; }

            public string Type { get; set; }

            public string Message { get; set; }
        }

    }
}
