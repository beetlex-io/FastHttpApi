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
        public object Onlines(BeetleX.FastHttpApi.HttpApiServer server)
        {
            BeetleX.ISession[] sessions = server.BaseServer.GetOnlines();
            return from a in sessions
                   where ((HttpToken)a.Tag).WebSocket
                   select new { a.ID, a.Name };
        }

        private BeetleX.FastHttpApi.HttpApiServer mServer;

        public void SendMessage(int sessionid, string message, BeetleX.FastHttpApi.HttpApiServer server)
        {
            var msg = new Command { Name = "Admin", Type = "Talk", Message = message };
            var frame = server.CreateDataFrame(msg);
            if (sessionid > 0)
            {
                server.SendDataFrame(frame, sessionid);
            }
            else
            {
                server.SendDataFrame(frame);
            }
        }

        private void OnWebSocketReceive(object sender, WebSocketReceiveArgs e)
        {
            var item = Newtonsoft.Json.JsonConvert.DeserializeObject<Command>((string)e.Frame.Body);
            if (item.Type == "Login")
            {
                e.Sesson.Name = item.Name;
            }
            var login = mServer.CreateDataFrame(item);
            mServer.SendDataFrame(login);
        }

        private void OnHttpDisconnect(object sender, BeetleX.EventArgs.SessionEventArgs e)
        {
            Command cmd = new Command { Name = e.Session.Name, Type = "Quit", Message = "" };
            var quit = mServer.CreateDataFrame(cmd);
            mServer.SendDataFrame(quit);

        }

        private void OnHttpConnected(object sender, BeetleX.EventArgs.ConnectedEventArgs e)
        {

        }

        public void Init(BeetleX.FastHttpApi.HttpApiServer server)
        {
            server.HttpConnected = OnHttpConnected;
            server.HttpDisconnect = OnHttpDisconnect;
            server.WebSocketReceive = OnWebSocketReceive;
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
