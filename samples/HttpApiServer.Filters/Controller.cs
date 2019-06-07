using BeetleX.FastHttpApi;
using System;
using System.Collections.Generic;
using System.Text;

namespace HttpApiServer.Filters
{
    [Controller]
    public class ControllerTest
    {
        //  /hello?name=
        [SkipFilter(typeof(GlobalFilter))]
        [CustomFilter]
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
        [Post]
        public object Post(string name, UserInfo data)
        {
            return data;
        }
        // /listcustomer?count
        [SkipFilter(typeof(NotFoundFilter))]
        [CustomFilter]
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
        [Post]
        public Employee AddEmployee(Employee item)
        {
            return item;
        }
        [CatchException]
        public void Throw()
        {
            throw new Exception("hello");
        }
    }

    public class GlobalFilter : FilterAttribute
    {
        public override bool Executing(ActionContext context)
        {
            Console.WriteLine($"{DateTime.Now} {context.HttpContext.Request.Url} globalFilter execting...");
            return base.Executing(context);
        }
        public override void Executed(ActionContext context)
        {
            base.Executed(context);
            Console.WriteLine($"{DateTime.Now} {context.HttpContext.Request.Url} globalFilter executed");
        }
    }

    public class NotFoundFilter : FilterAttribute
    {
        public override bool Executing(ActionContext context)
        {
            Console.WriteLine(DateTime.Now + " NotFoundFilter execting...");
            NotFoundResult notFound = new NotFoundResult("not found");
            context.Result = notFound;
            return false;
        }
        public override void Executed(ActionContext context)
        {
            base.Executed(context);
        }

    }

    public class CustomFilter : FilterAttribute
    {
        public override bool Executing(ActionContext context)
        {
            Console.WriteLine(DateTime.Now + " CustomFilter execting...");
            return base.Executing(context);
        }
        public override void Executed(ActionContext context)
        {
            Console.WriteLine(DateTime.Now + " CustomFilter executed");
            base.Executed(context);
        }
    }

    public class CatchException : FilterAttribute
    {
        public override void Executed(ActionContext context)
        {
            base.Executed(context);
            if (context.Exception != null)
            {
                context.Result = new TextResult(context.Exception.Message);
                context.Exception = null;
            }
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
