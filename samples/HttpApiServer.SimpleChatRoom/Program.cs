using BeetleX.FastHttpApi;
using System;
using System.Linq;
namespace HttpApiServer.SimpleChatRoom
{
    class Program
    {
        private static BeetleX.FastHttpApi.HttpApiServer mApiServer;

        static void Main(string[] args)
        {
            mApiServer = new BeetleX.FastHttpApi.HttpApiServer();
            mApiServer.Options.LogLevel = BeetleX.EventArgs.LogType.Debug;
            mApiServer.Options.LogToConsole = true;
            mApiServer.Debug();
            mApiServer.Register(typeof(Program).Assembly);
            mApiServer.Open();
            Console.Write(mApiServer.BaseServer);
            Console.Read();
        }
    }
    [Controller]
    public class Chat : IController
    {
        public bool Login(string nickName, IHttpContext context)
        {
            context.Session.Name = nickName;
            ActionResult result = new ActionResult();
            result.Data = new { name = nickName, message = "login", type = "login", time = DateTime.Now.ToString("T") };
            context.SendToWebSocket(result);
            return true;
        }

        public object ListOnlines(IHttpContext context)
        {
            return from r in context.Server.GetWebSockets()
                   where r.Session.Name != null
                   select new { r.Session.Name, IP = r.Session.RemoteEndPoint.ToString() };
        }


        public bool Talk(string nickName, string message, IHttpContext context)
        {
            ActionResult result = new ActionResult();
            result.Data = new { name = nickName, message, type = "talk", time = DateTime.Now.ToString("T") };
            context.SendToWebSocket(result);
            return true;
        }
        [NotAction]
        public void Init(BeetleX.FastHttpApi.HttpApiServer server, string path)
        {
            server.HttpDisconnect += (o, e) =>
            {
                if (e.Session.Name != null)
                {
                    ActionResult result = new ActionResult();
                    result.Data = new { name = e.Session.Name, message = "quit", type = "quit", time = DateTime.Now.ToString("T") };
                    var data = server.CreateDataFrame(result);
                    server.SendToWebSocket(data);
                }
            };
        }
    }
}
