using BeetleX.FastHttpApi;
using System;

namespace HttpApiServer.SimpleChatRoom
{
    class Program
    {
        private static BeetleX.FastHttpApi.HttpApiServer mApiServer;

        static void Main(string[] args)
        {
            mApiServer = new BeetleX.FastHttpApi.HttpApiServer();
            mApiServer.Debug();
            mApiServer.Register(typeof(Program).Assembly);
            mApiServer.Open();
            Console.Write(mApiServer.BaseServer);
            Console.Read();
        }
    }
    [Controller]
    public class Controller
    {
        public bool Talk(string name,string message, IHttpContext context)
        {
            ActionResult result = new ActionResult();
            result.Data = new { name, message };
            context.SendToWebSocket(result);
            return true;

        }
    }
}
