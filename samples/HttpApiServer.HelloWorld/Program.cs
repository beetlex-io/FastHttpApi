using BeetleX.FastHttpApi;
using System;

namespace HttpApiServer.HelloWorld
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
    [Controller(SingleInstance =false)]
    public class Home
    {
        /// <summary>
        /// Hello Word
        /// </summary>
        /// <param name="name">string:  you name</param>
        /// <returns>string</returns>
        public object Hello(string name)
        {
            return new { Hello = "hello " + name, Time = DateTime.Now };
        }
    }
}
