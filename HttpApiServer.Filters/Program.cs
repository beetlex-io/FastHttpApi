using System;

namespace HttpApiServer.Filters
{
    class Program
    {
        private static BeetleX.FastHttpApi.HttpApiServer mApiServer;

        static void Main(string[] args)
        {
            mApiServer = new BeetleX.FastHttpApi.HttpApiServer();   
            mApiServer.ServerConfig.BodySerializer = new BeetleX.FastHttpApi.JsonBodySerializer();
            mApiServer.ServerConfig.AddFilter<GlobalFilter>();
            mApiServer.Register(typeof(Program).Assembly);
            mApiServer.Open();
            Console.Read();
        }
    }
}
