using System;

namespace HttpApiServer.Admin
{
    class Program
    {

        private static BeetleX.FastHttpApi.HttpApiServer mApiServer;

        static void Main(string[] args)
        {
            mApiServer = new BeetleX.FastHttpApi.HttpApiServer();

            mApiServer.Register(typeof(BeetleX.FastHttpApi.Admin._Admin).Assembly);
            mApiServer.Register(typeof(Program).Assembly);

            mApiServer.Options.UrlIgnoreCase = true;
            mApiServer.Open();
            Console.Write(mApiServer.BaseServer);
            Console.Read();
        }
    }
}
