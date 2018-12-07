using BeetleX.FastHttpApi;
using System;

namespace HttpApiServer.Cookies
{
    class Program
    {
        private static BeetleX.FastHttpApi.HttpApiServer mApiServer;

        static void Main(string[] args)
        {
            mApiServer = new BeetleX.FastHttpApi.HttpApiServer();
            mApiServer.Register(typeof(Program).Assembly);
            mApiServer.Debug();
            mApiServer.Open();
            Console.Write(mApiServer.BaseServer);
            Console.Read();
        }
    }

    [Controller]
    public class Home
    {
        public bool setCookie(string name, string value, IHttpContext context)
        {

            context.Response.SetCookie(name, value);
            return true;
        }

        public string getCookie(string name, HttpRequest request, IHttpContext context)
        {
            string value = context.Request.Cookies[name];
            return value;
        }


    }
}
