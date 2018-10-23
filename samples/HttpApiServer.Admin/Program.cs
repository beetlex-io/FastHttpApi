using System;

namespace HttpApiServer.Admin
{
    class Program
    {

        private static BeetleX.FastHttpApi.HttpApiServer mApiServer;

        static void Main(string[] args)
        {         
            mApiServer = new BeetleX.FastHttpApi.HttpApiServer();
            mApiServer.Debug();
            mApiServer.Register(typeof(BeetleX.FastHttpApi.Admin._Admin).Assembly);
            mApiServer.Register(typeof(Program).Assembly);
            mApiServer.HttpRequestNotfound += (o, e) =>
            {
                BeetleX.FastHttpApi.Move302Result result = new BeetleX.FastHttpApi.Move302Result("/_admin/index.html");
                e.Response.Result(result);
                e.Cancel = true;
            };

            mApiServer.Open();
            Console.Write(mApiServer.BaseServer);
            Console.Read();
        }
    }
}
