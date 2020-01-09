using BeetleX.FastHttpApi;
using BeetleX.FastHttpApi.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Linq;
using System;

namespace Web.Caching
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = new HostBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddMemoryCache();
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
    [Controller]
    public class Home
    {
        [Cached]
        public object Employees()
        {
            return Northwind.Data.DataHelper.Defalut.Employees;
        }
        [Cached]
        public object Orders([CacheKeyParameter]int employee)
        {
            return from a in Northwind.Data.DataHelper.Defalut.Orders
                   where a.EmployeeID == employee
                   select a;
        }
    }

}
