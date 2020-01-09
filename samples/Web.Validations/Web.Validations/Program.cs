using BeetleX.FastHttpApi.Hosting;
using BeetleX.FastHttpApi.Validations;
using Microsoft.Extensions.Hosting;
using System;

namespace Web.Validations
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
        public object Register(
            [StringRegion(Min =3)]
            string name,
            [StringRegion(Min =6)]
            string pwd,
            [EmailFormater]
            string email
            )
        {
            return DateTime.Now;
        }
    }

}
