using BeetleX.FastHttpApi;
using BeetleX.FastHttpApi.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;

namespace Web.JWT
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
                        JWTHelper.Init();
                    },
                    typeof(Program).Assembly);
                });
            builder.Build().Run();
        }
    }
    [BeetleX.FastHttpApi.Controller]
    public class Home
    {
        public bool Login(string name,string pwd,IHttpContext context)
        {
            var result= (name == "admin" && pwd == "123456");
            if (result)
                JWTHelper.Default.CreateToken(context.Response, name,"admin");
            return result;
        }
        [AdminFilter]
        public object List()
        {
            return Northwind.Data.DataHelper.Defalut.Employees;
        }
    }
}
