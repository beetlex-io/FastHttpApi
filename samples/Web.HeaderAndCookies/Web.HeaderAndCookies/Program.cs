using BeetleX.FastHttpApi;
using BeetleX.FastHttpApi.Hosting;
using Microsoft.Extensions.Hosting;
using System;

namespace Web.HeaderAndCookies
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new HostBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.UseBeetlexHttp(o =>
                    {
                        o.LogToConsole = true;
                        o.ManageApiEnabled = false;
                        o.Port = 80;
                        o.SetDebug();
                        o.LogLevel = BeetleX.EventArgs.LogType.Warring;
                    },
                    typeof(Program).Assembly);
                });
            builder.Build().Run();
        }
    }
    [BeetleX.FastHttpApi.Controller]
    public class Home
    {
        public object GetHeader(string name,IHttpContext context)
        {
           return context.Request.Header[name];
        }

        public void SetCookie(string name,string value, IHttpContext context)
        {
            context.Response.SetCookie(name, value);
        }

        public object GetCookie(string name, IHttpContext context)
        {
            return context.Request.Cookies[name];
        }
    }

}
