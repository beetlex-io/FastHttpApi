using BeetleX.FastHttpApi;
using System;

namespace HttpApiServer.Chat
{
    class Program
    {

        private static BeetleX.FastHttpApi.HttpApiServer mApiServer;

        static void Main(string[] args)
        {

            mApiServer = new BeetleX.FastHttpApi.HttpApiServer();
            mApiServer.ServerConfig.BodySerializer = new JsonBodySerializer();
            mApiServer.Register(typeof(Program).Assembly);
            mApiServer.Debug();
            mApiServer.Open();
            Console.Write(mApiServer.BaseServer);
            Console.Read();
        }
    }
}
