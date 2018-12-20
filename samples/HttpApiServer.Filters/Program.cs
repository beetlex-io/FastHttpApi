using System;

namespace HttpApiServer.Filters
{
    class Program
    {
        private static BeetleX.FastHttpApi.HttpApiServer mApiServer;

        static void Main(string[] args)
        {
            mApiServer = new BeetleX.FastHttpApi.HttpApiServer();

            mApiServer.Options.AddFilter<GlobalFilter>();
            mApiServer.Register(typeof(Program).Assembly);
            mApiServer.Open();
            Console.Read();
        }
    }
}
