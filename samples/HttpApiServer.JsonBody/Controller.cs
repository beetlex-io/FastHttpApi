using BeetleX.FastHttpApi;
using System;
using System.Collections.Generic;
using System.Text;

namespace HttpApiServer.JsonBody
{
    [Controller]
    public class ControllerTest
    {
        //  /hello?name=
        public string Hello(string name)
        {
            return DateTime.Now + " hello " + name;
        }
        // /add?a=&b=
        public string Add(int a, int b)
        {
            return string.Format("{0}+{1}={2}", a, b, a + b);
        }
        // /post?name=
        public object Post(string name, [BodyParameter] UserInfo data)
        {
            return data;
        }
        // /listcustomer?count
        public IList<Customer> ListCustomer(int count)
        {
            return Customer.List(count);
        }
        // /listemployee?count
        public IList<Employee> ListEmployee(int count)
        {
            return Employee.List(count);
        }
        // post /AddEmployee 
        public Employee AddEmployee([BodyParameter]Employee item)
        {
            return item;
        }
    }

    public class UserInfo
    {
        public String Email { get; set; }

        public string City { get; set; }

        public string Address { get; set; }
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

        static IList<Employee> mEmployees;

        public static IList<Employee> List(int count)
        {
            lock (typeof(Employee))
            {
                if (mEmployees == null)
                {
                    mEmployees = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Employee>>(Data.Employees);
                }
            }
            if (count > mEmployees.Count)
                count = mEmployees.Count;
            List<Employee> result = new List<Employee>();
            for (int i = 0; i < count; i++)
            {
                result.Add(mEmployees[i]);
            }
            return result;
        }
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

        static IList<Customer> mCustomers;

        public static IList<Customer> List(int count)
        {
            lock (typeof(Customer))
            {
                if (mCustomers == null)
                    mCustomers = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Customer>>(Data.Customers);
            }
            List<Customer> result = new List<Customer>();
            if (count > mCustomers.Count)
                count = mCustomers.Count;
            for (int i = 0; i < count; i++)
                result.Add(mCustomers[i]);
            return result;
        }
    }
}
