using BeetleX.FastHttpApi;
using BeetleX.FastHttpApi.Clients;
using System.Collections.Generic;
using System;

namespace FastApiClient
{
    public class Program
    {
        private const string Host = "http://localhost:8080";

        static void Main(string[] args)
        {
            HttpApiClient client = new HttpApiClient(Host);

            IDataService service = client.Create<IDataService>();

            DateTime dt = service.GetTime();

            Console.WriteLine($"get time:{dt}");

            string hello = service.Hello("henry");

            Console.WriteLine($"hello :{hello}");

            var login = service.Login("admin", "123456");
            Console.WriteLine($"login status:{login}");

            var orders = service.Orders(3, null, 1, 20);
            if (orders != null)
                Console.WriteLine($"list orders: {orders.Count}");

            var emp = service.EmployeeGet(7);
            Console.WriteLine($"get employee id 7:{emp?.FirstName} {emp?.LastName}");

            var empnames = service.EmployeesGetName();
            Console.WriteLine($"employee names:{empnames.Count}");

            emp = service.EmployeeEdit(5, new Employee { FirstName = "fan", LastName = "henry" });
            Console.WriteLine($"edit employee :{emp?.EmployeeID} {emp?.FirstName} {emp?.LastName}");

            var count = service.EmployeeAdd(null);
            Console.WriteLine($"add employee :{count}");

            count = service.EmployeeAdd(new Employee { EmployeeID = 3 }, new Employee { EmployeeID = 5 });
            Console.WriteLine($"add employee :{count}");

            var cust = service.Customers(10);
            Console.WriteLine($"list customers :{cust.Count}");

            var custNames = service.CustomersGetName();
            Console.WriteLine($"list customers name :{custNames.Count}");
            Console.Read();
        }
    }

    [JsonFormater]
    [Controller(BaseUrl = "Home")]
    public interface IDataService
    {
        [Get]
        DateTime GetTime();
        [Get]
        string Hello(string name);
        [Get]
        bool Login(string name, string pwd);
        [Get]
        List<Employee> Employees();
        [Get]
        Employee EmployeeGet(int id);
        [Post]
        int EmployeeAdd(params Employee[] items);
        [Post]
        Employee EmployeeEdit([CQuery]int id, Employee emp);
        [Get]
        List<EmployeeName> EmployeesGetName();
        [Get]
        List<Customer> Customers(int count);
        [Get]
        List<CustomerName> CustomersGetName();
        [Get]
        List<Order> Orders(int? employeeid, string customerid, int index, int size);

    }

    public class Customer
    {

        public string CustomerID { get; set; }

        public string CompanyName { get; set; }

        public string ContactName { get; set; }

        public string ContactTitle { get; set; }

        public string Address { get; set; }

        public string City { get; set; }

        public string PostalCode { get; set; }

        public string Country { get; set; }

        public string Phone { get; set; }

        public string Fax { get; set; }
    }

    public class CustomerName
    {
        public string ID
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }
    }

    public class EmployeeName
    {
        public int ID
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

    }

    public class Employee
    {

        public int EmployeeID
        {
            get;
            set;
        }

        public string LastName
        {
            get;
            set;
        }

        public string FirstName
        {
            get;
            set;
        }

        public string Title
        {
            get;
            set;
        }

        public string TitleOfCourtesy { get; set; }

        public DateTime BirthDate { get; set; }

        public DateTime HireDate { get; set; }

        public string Address { get; set; }

        public string City { get; set; }

        public string Region { get; set; }

        public string PostalCode { get; set; }

        public string Country { get; set; }

        public string HomePhone { get; set; }

        public string Extension { get; set; }

        public string Photo { get; set; }

        public string Notes { get; set; }
    }

    public class Order
    {

        public int OrderID { get; set; }

        public string CustomerID { get; set; }

        public int EmployeeID { get; set; }

        public DateTime OrderDate { get; set; }

        public DateTime RequiredDate { get; set; }

        public DateTime ShippedDate { get; set; }

        public int ShipVia { get; set; }

        public double Freight { get; set; }

        public string ShipName { get; set; }

        public string ShipAddress { get; set; }

        public string ShipCity { get; set; }

        public string ShipPostalCode { get; set; }

        public string ShipCountry { get; set; }
    }
}
