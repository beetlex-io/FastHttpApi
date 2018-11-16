using BeetleX.FastHttpApi;
using System;

namespace FastHttpApiWeb
{
    [Controller(BaseUrl = "api")]
    public class Program
    {
        private static HttpApiServer mApiServer;

        static void Main(string[] args)
        {
            mApiServer = new HttpApiServer();

            mApiServer.Register(typeof(Program).Assembly);
            mApiServer.Open();

            Console.Write(mApiServer.BaseServer);
            Console.Read();
        }
        [Get(Route = "{name}")]
        public object values(string name)
        {
            return new TextResult(name);
        }
    }
}
