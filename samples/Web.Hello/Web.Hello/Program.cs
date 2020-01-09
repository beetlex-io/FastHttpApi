using BeetleX.FastHttpApi;
using BeetleX.FastHttpApi.Hosting;
using Microsoft.Extensions.Hosting;
using System;

namespace Web.Hello
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
        public string Hello(string name)
        {
            return $"hello {name} {DateTime.Now}";
        }
    }

    public class Filter : BeetleX.FastHttpApi.FilterAttribute {

        public override bool Executing(ActionContext context)
        {
            context.HttpContext.Data.SetValue("data",);
            return base.Executing(context);
        }
    }


}
