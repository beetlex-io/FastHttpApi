using BeetleX.FastHttpApi;
using System;

namespace Cluster.Server
{
    class Program
    {
        private static BeetleX.FastHttpApi.HttpApiServer mApiServer;

        static void Main(string[] args)
        {
            mApiServer = new BeetleX.FastHttpApi.HttpApiServer();
            mApiServer.Debug();
            mApiServer.Register(typeof(Program).Assembly);
            mApiServer.Open();
            Console.Write(mApiServer.BaseServer);
            Console.Read();
        }
    }
    [Controller]
    public class Home
    {
        /// <summary>
        /// Hello Word
        /// </summary>
        /// <param name="name">string:  you name</param>
        /// <returns>string</returns>
        [Get(Route = "{name}")]
        [DefaultJsonResultFilter]
        public object Hello(string name)
        {
            return new { Hello = "hello " + name, Time = DateTime.Now };
        }
    }
}
