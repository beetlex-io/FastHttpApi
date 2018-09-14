using System;

namespace HttpApiServer.JsonBody
{
    class Program
    {
        private static BeetleX.FastHttpApi.HttpApiServer mApiServer;

        static void Main(string[] args)
        {
            mApiServer = new BeetleX.FastHttpApi.HttpApiServer();
            mApiServer.Register(typeof(Program).Assembly);
            mApiServer.ServerConfig.BodySerializer = new BeetleX.FastHttpApi.JsonBodySerializer();
            mApiServer.Open();
            Console.Read();
        }
    }
}
