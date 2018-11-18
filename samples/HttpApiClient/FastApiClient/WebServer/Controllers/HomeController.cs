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

        public IEnumerable<Order> ListOrders(int employee, string customer)
        {
            Func<Order, bool> exp = o => (employee == 0 || o.EmployeeID == employee)
           && (string.IsNullOrEmpty(customer) || o.CustomerID == customer);
            var result = DataHelper.Orders.Where(exp);
            return result;
        }

        public Employee GetEmployee(int id)
        {
            Employee result = DataHelper.Employees.Find(e => e.EmployeeID == id);
            return result;
        }

        [HttpPost]
        public int AddEmployee([FromBody] List<Employee> items)
        {
            if (items == null)
                return 0;
            return items.Count;
        }

        [HttpPost]
        public Employee EditEmployee(int id, [FromBody]Employee employee)
        {
            employee.EmployeeID = id;
            return employee;
        }

        public bool Login(string name, string pwd)
        {
            if (name == "admin" && pwd == "123456")
                return true;
            return false;
        }

    }
}
