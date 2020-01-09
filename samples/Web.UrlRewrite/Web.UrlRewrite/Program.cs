using BeetleX.FastHttpApi;
using BeetleX.FastHttpApi.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
namespace Web.UrlRewrite
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
                    s =>
                    {
                        s.UrlRewrite
                        .Add("/orders/employee/{employee}", "/orders?employee={employee}")
                        .Add("/orders/customer/{customer}", "/orders?customer={customer}");
                    },
                    typeof(Program).Assembly);
                });
            builder.Build().Run();
        }
    }
    [Controller]
    [DefaultJsonResultFilter]
    public class Home
    {
        public object Employees()
        {
            return Northwind.Data.DataHelper.Defalut.Employees;
        }

        public object Customers()
        {
            return Northwind.Data.DataHelper.Defalut.Customers;
        }

        public object Orders(int employee, string customer,IHttpContext context)
        {
            return from a in Northwind.Data.DataHelper.Defalut.Orders
                   where a.EmployeeID == employee || a.CustomerID == customer
                   select a;
        }
    }
}
