using BeetleX.FastHttpApi;
using System;

namespace Crossdomain
{
    [Options(AllowOrigin = "*")]
    [BeetleX.FastHttpApi.Controller]
    class Program
    {
        private static HttpApiServer mApiServer;

        static void Main(string[] args)
        {
            mApiServer = new HttpApiServer();
            mApiServer.Options.LogToConsole = true;
            mApiServer.Options.LogLevel = BeetleX.EventArgs.LogType.Debug;
            mApiServer.Debug();
            mApiServer.Register(typeof(Program).Assembly);
            mApiServer.Open();
            Console.Write(mApiServer.BaseServer);
            Console.WriteLine(Environment.ProcessorCount);
            Console.Read();
        }

        public string hello(string name)
        {
            return $"hello {name} {DateTime.Now}";
        }

        [Post]
        public string post(string name)
        {
            return $"post {name} {DateTime.Now}";
        }
    }
}
