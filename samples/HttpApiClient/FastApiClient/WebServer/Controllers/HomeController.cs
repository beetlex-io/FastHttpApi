using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebServer.Controllers
{
    public class HomeController : Controller
    {
        public DateTime GetTime()
        {
            return DateTime.Now;
        }

        public IActionResult Hello(string name)
        {
            return new JsonResult($"hello {name}");
        }

        public bool Login(string name, string pwd)
        {
            if (name == "admin" && pwd == "123456")
                return true;
            return false;
        }

        public object Employees()
        {
            return DataHelper.Employees;
        }

        public Employee EmployeeGet(int id)
        {
            Employee result = DataHelper.Employees.Find(e => e.EmployeeID == id);
            if (result == null)
                result = new Employee();
            return result;
        }

        [HttpPost]
        public int EmployeeAdd([FromBody]List<Employee> items)
        {
            return items == null ? 0 : items.Count;
        }

        [HttpPost]
        public Employee EmployeeEdit(int id, [FromBody]Employee emp)
        {
            Employee record = DataHelper.Employees.Find(e => e.EmployeeID == id);
            if (record != null)
            {
                record.City = emp.City;
                record.Address = emp.Address;
                record.Title = emp.Title;
                record.HomePhone = emp.HomePhone;
                return record;
            }
            return null;
        }

        public object EmployeesGetName()
        {
            return from e in DataHelper.Employees select new { ID = e.EmployeeID, Name = e.FirstName + " " + e.LastName };
        }

        public object Customers(int count)
        {
            List<Customer> result = new List<Customer>();
            if (count > DataHelper.Customers.Count)
                count = DataHelper.Customers.Count;
            for (int i = 0; i < count; i++)
            {
                result.Add(DataHelper.Customers[i]);
            }
            return result;
        }

        public object CustomersGetName()
        {
            return from c in DataHelper.Customers select new { ID = c.CustomerID, Name = c.CompanyName };
        }

        public object Orders(int employeeid, string customerid, int index, int size)
        {
            Func<Order, bool> exp = o => (employeeid == 0 || o.EmployeeID == employeeid)
             && (string.IsNullOrEmpty(customerid) || o.CustomerID == customerid);
            int count = DataHelper.Orders.Count(exp);
            if (size == 0)
                size = 10;
            int pages = count / size;
            if (count % size > 0)
                pages++;
            var items = DataHelper.Orders.Where(exp).Skip(index * size).Take(size);
            return items;

        }
    }
}
