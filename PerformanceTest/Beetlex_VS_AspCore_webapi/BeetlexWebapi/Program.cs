using BeetleX.FastHttpApi;
using BeetleX.FastHttpApi.SpanJson;
using Northwind.Data;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BeetlexWebapi
{
    class Program
    {
        private static HttpApiServer mApiServer;
        static void Main(string[] args)
        {
            mApiServer = new HttpApiServer();
            mApiServer.Register(typeof(Program).Assembly);//加载程序集中所有控制器信息和静态资源信息
            mApiServer.Options.Port = 9090;
            mApiServer.Options.LogLevel = BeetleX.EventArgs.LogType.Warring;
            mApiServer.Options.LogToConsole = true;
            mApiServer.Debug();//只有在Debug模式下生产，把静态资源加载目录指向项目的views目录
            mApiServer.Open();
            Console.Write(mApiServer.BaseServer);
            Console.Read();
        }
    }
    [Controller(BaseUrl = "Home")]
    public class Controller
    {
        public class JsonMessage
        {
            public string message { get; set; }
        }

        public object plaintext()
        {
            return new TextResult("Hello, World!");
        }

        public object json()
        {
            return new JsonResult(new JsonMessage { message = "Hello, World!" });
        }

        public List<Employee> Employees()
        {
            return DataHelper.Defalut.Employees;
        }
        [SpanJsonResultFilter]
        public List<Employee> EmployeesSpan()
        {
            return DataHelper.Defalut.Employees;
        }

        [Get(Route = "{id}")]
        public object Employee(int id)
        {
            Employee result = DataHelper.Defalut.Employees.Find(e => e.EmployeeID == id);
            if (result == null)
                result = new Employee();
            return new JsonResult(result);
        }
        [Get(Route = "{id}")]
        public object Orders(int id, string customerid, int index, int size)
        {
            Func<Order, bool> exp = o => (id == 0 || o.EmployeeID == id)
             && (string.IsNullOrEmpty(customerid) || o.CustomerID == customerid);
            int count = DataHelper.Defalut.Orders.Count(exp);
            if (size == 0)
                size = 10;
            int pages = count / size;
            if (count % size > 0)
                pages++;
            var items = DataHelper.Defalut.Orders.Where(exp).Skip(index * size).Take(size);
            return new JsonResult(items);

        }
    }
}
