using System;

namespace HttpApiServer.WebSocket
{
    [BeetleX.FastHttpApi.Controller]
    class Program
    {
        private static BeetleX.FastHttpApi.HttpApiServer mApiServer;

        static void Main(string[] args)
        {
            mApiServer = new BeetleX.FastHttpApi.HttpApiServer();
            mApiServer.Debug();
            mApiServer.Register(typeof(Program).Assembly);
            mApiServer.Open();
            mApiServer.WebSocketReceive = (o, e) =>
            {
                Console.WriteLine(e.Frame.Body);
                var freame = e.CreateFrame($"{DateTime.Now}" + e.Frame.Body.ToString());
                e.Response(freame);
            };
            mApiServer.WebSocketConnect = (o, e) => {
                //e.Request.Header
                //e.Request.Cookies
                e.Cancel = true;
            };
         

            Console.Write(mApiServer.BaseServer);
            Console.Read();
        }

        public string Hello(string name)
        {
            return $"{name} {DateTime.Now}";
        }
    }
}
