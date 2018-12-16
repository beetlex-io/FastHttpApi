using BeetleX.FastHttpApi;
using BeetleX.FastHttpApi.Clients;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FastApiClusterClient
{
    class Program
    {

        private const string Host25 = "http://192.168.2.25:8080";

        private const string Host26 = "http://192.168.2.26:8080";

        private const string Host27 = "http://192.168.2.27:8080";

        private const string Host28 = "http://192.168.2.28:8080";

        private const string Host29 = "http://192.168.2.29:8080";

        private const string Host30 = "http://192.168.2.30:8080";

        private const string Host31 = "http://192.168.2.31:8080";

        private const string Host32 = "http://192.168.2.32:8080";

        private static HttpClusterApi HttpClusterApi = new HttpClusterApi();

        private static IDataService DataService;

        static void Main(string[] args)
        {
            HttpClusterApi.AddHost("*", Host25, Host29);
            HttpClusterApi.GetUrlNode("employee.*").Add(Host26).Add(Host27).Add(Host30, 0);
            HttpClusterApi.AddHost("customer.*", Host28, Host29);
            HttpClusterApi.AddHost("orders.*", Host25, Host29);
            DataService = HttpClusterApi.Create<IDataService>();
            for (int i = 0; i < 10; i++)
            {
                OnTest();
            }
            while (true)
            {
                System.Threading.Thread.Sleep(1000);
                Console.Write(HttpClusterApi.Stats());
            }
        }



        private static void OnTest()
        {

            System.Threading.ThreadPool.QueueUserWorkItem(async (o) =>
            {
                while (true)
                {
                    await DataService.Hello("henry");
                }
            });

            System.Threading.ThreadPool.QueueUserWorkItem(async (o) =>
            {
                while (true)
                {
                    await DataService.EmployeeGet(2);
                }
            });

            System.Threading.ThreadPool.QueueUserWorkItem(async (o) =>
            {
                while (true)
                {
                    await DataService.EmployeesGetName();
                }
            });

            System.Threading.ThreadPool.QueueUserWorkItem(async (o) =>
            {
                while (true)
                {
                    await DataService.Customers(4);
                }
            });

            System.Threading.ThreadPool.QueueUserWorkItem(async (o) =>
            {
                while (true)
                {
                    await DataService.CustomersGetName();
                }
            });

            System.Threading.ThreadPool.QueueUserWorkItem(async (o) =>
            {
                while (true)
                {
                    await DataService.Orders(3, null, 1, 5);
                }
            });

            System.Threading.ThreadPool.QueueUserWorkItem(async (o) =>
            {
                while (true)
                {
                    await DataService.Orders(2, null, 1, 5);
                }
            });
        }
    }

    [JsonFormater]
    [Controller(BaseUrl = "Home")]
    public interface IDataService
    {
        [Get]
        Task<DateTime> GetTime();
        [Get]
        Task<string> Hello(string name);
        [Get]
        Task<bool> Login(string name, string pwd);
        [Get]
        Task<List<Employee>> Employees();
        [Get]
        Task<Employee> EmployeeGet(int id);
        [Post]
        Task<int> EmployeeAdd(params Employee[] items);
        [Post]
        Task<Employee> EmployeeEdit([CQuery]int id, Employee emp);
        [Get]
        Task<List<EmployeeName>> EmployeesGetName();
        [Get]
        Task<List<Customer>> Customers(int count);
        [Get]
        Task<List<CustomerName>> CustomersGetName();
        [Get]
        Task<List<Order>> Orders(int? employeeid, string customerid, int index, int size);
    }

    //[JsonFormater]
    //[Controller(BaseUrl = "Home")]
    //public interface IDataService
    //{
    //    [Get]
    //    DateTime GetTime();
    //    [Get]
    //    string Hello(string name);
    //    [Get]
    //    bool Login(string name, string pwd);
    //    [Get]
    //    List<Employee> Employees();
    //    [Get]
    //    Employee EmployeeGet(int id);
    //    [Post]
    //    int EmployeeAdd(params Employee[] items);
    //    [Post]
    //    Employee EmployeeEdit([CQuery]int id, Employee emp);
    //    [Get]
    //    List<EmployeeName> EmployeesGetName();
    //    [Get]
    //    List<Customer> Customers(int count);
    //    [Get]
    //    List<CustomerName> CustomersGetName();
    //    [Get]
    //    List<Order> Orders(int? employeeid, string customerid, int index, int size);

    //}

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
