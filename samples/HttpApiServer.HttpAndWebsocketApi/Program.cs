using System;

namespace HttpApiServer.HttpAndWebsocketApi
{
    class Program
    {

        private static BeetleX.FastHttpApi.HttpApiServer mApiServer;

        static void Main(string[] args)
        {
            mApiServer = new BeetleX.FastHttpApi.HttpApiServer();
            mApiServer.Register(typeof(Program).Assembly);
            //config.SSL = true;
            //mApiServer.ServerConfig.CertificateFile = @"c:\ssltest.pfx";
            //mApiServer.ServerConfig.CertificatePassword = "123456";
            mApiServer.Debug();
            mApiServer.Open();
            Console.Write(mApiServer.BaseServer);
            Console.Read();
        }
    }
}
