using BeetleX.FastHttpApi;
using System;

namespace HttpApiServer.Options
{
    [BeetleX.FastHttpApi.Controller]
    [Options(AllowOrigin = "*")]
    public class Program
    {

        private static BeetleX.FastHttpApi.HttpApiServer mApiServer;

        static void Main(string[] args)
        {
            mApiServer = new BeetleX.FastHttpApi.HttpApiServer();
            mApiServer.Options.LogToConsole = true;
            mApiServer.Debug();
            mApiServer.Register(typeof(Program).Assembly);
            mApiServer.Open();
            Console.Write(mApiServer.BaseServer);
            Console.Read();
        }



        public object Hello(string name)
        {
            return new { Hello = "hello " + name, Time = DateTime.Now };
        }

        [Options(AllowOrigin = "www.ikende.com")]
        public string GetTime(IHttpContext context)
        {
            return DateTime.Now.ToShortDateString();
        }
    }
}
