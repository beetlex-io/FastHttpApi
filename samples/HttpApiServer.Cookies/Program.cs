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
            mApiServer.ServerConfig.BodySerializer = new JsonBodySerializer();
            mApiServer.Register(typeof(Program).Assembly);
            mApiServer.Open();
            Console.Write(mApiServer.BaseServer);
            Console.Read();
        }
    }

    [Controller]
    public class Test
    {
        public void setCookie(string name, string value, HttpResponse response)
        {
            response.SetCookie(name, value);
            response.Result();
        }

        public void getCookie(string name, HttpRequest request, HttpResponse response)
        {
            string value = request.Cookies[name];
            response.Result(value);
        }
    }
}
