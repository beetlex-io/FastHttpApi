using System;
using Microsoft.Extensions.DependencyInjection;
using BeetleX.FastHttpApi.Hosting;
using Microsoft.Extensions.Hosting;
using BeetleX.FastHttpApi;

namespace DependencyInjection
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new HostBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services
                    .AddSingleton<UserService>()
                    .UseBeetlexHttp(o => {
                        o.Port = 8080;
                        o.LogToConsole = true;
                        o.LogLevel = BeetleX.EventArgs.LogType.Debug;
                        o.SetDebug();
                    }, typeof(Program).Assembly);
                });
            builder.Build().Run();
        }
    }

    public class UserService
    {
        public bool Login(string name,string pwd)
        {
            return name == "admin" && pwd == "123456";
        }
    }


    [Controller]
    public class Home
    {
        public Home(HttpApiServer server,UserService userService)
        {
            mHttpApiServer = server;
            mUserService = userService;
        }

        private UserService mUserService;

        private HttpApiServer mHttpApiServer;

        public string Hello(string name)
        {
            return $"hello {name}";
        }
    }
}
