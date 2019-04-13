using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Northwind.Data;

namespace AspCoreWebapi.Controllers
{
    public class HomeController : Controller
    {

        public class JsonMessage
        {
            public string message { get; set; }
        }

        public string plaintext()
        {
            return "Hello, World!";
        }

        public object json()
        {
            return new JsonMessage { message = "Hello, World!" };
        }

        public List<Employee> Employees()
        {
            return DataHelper.Defalut.Employees;
        }

        public Employee Employee(int id)
        {
            Employee result = DataHelper.Defalut.Employees.Find(e => e.EmployeeID == id);
            if (result == null)
                result = new Employee();
            return result;
        }

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
            return items;

        }
    }
}
