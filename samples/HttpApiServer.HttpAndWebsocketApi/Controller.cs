using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using BeetleX.FastHttpApi;
using BeetleX.FastHttpApi.WebSockets;
using BeetleX.EventArgs;

namespace HttpApiServer.HttpAndWebsocketApi
{
    [BeetleX.FastHttpApi.Controller]
    public class Home : IController
    {
        public Home()
        {
            mEmployees = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Employee>>(Datas.Employees);
            mCustomers = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Customer>>(Datas.Customers);
            mOrders = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Order>>(Datas.Orders);
        }
        private List<Employee> mEmployees;
        private List<Customer> mCustomers;
        private List<Order> mOrders;

        private BeetleX.FastHttpApi.HttpApiServer mServer;
        /// <summary>
        /// 获取雇员信息
        /// </summary>
        /// <param name="id">雇员ID</param>
        /// <returns>{"employeeID":0,"lastName":null,"firstName":null,"title":null,"titleOfCourtesy":null,"birthDate":"0001-01-01T00:00:00","hireDate":"0001-01-01T00:00:00","address":null,"city":null,"region":null,"postalCode":null,"country":null,"homePhone":null,"extension":null,"photo":null,"notes":null}</returns>
        public Employee GetEmployee(int id)
        {
            Employee result = mEmployees.Find(e => e.EmployeeID == id);
            if (result == null)
                result = new Employee();
            return result;
        }
        /// <summary>
        /// 修改雇员信息
        /// </summary>
        /// <param name="id">雇员ID</param>
        /// <param name="emp">雇员信息:{"employeeID":0,"lastName":null,"firstName":null,"title":null,"titleOfCourtesy":null,"birthDate":"0001-01-01T00:00:00","hireDate":"0001-01-01T00:00:00","address":null,"city":null,"region":null,"postalCode":null,"country":null,"homePhone":null,"extension":null,"photo":null,"notes":null}</param>
        /// <returns>bool</returns>
        [Post]
        public bool EditEmployee(int id,Employee emp, IHttpContext context)
        {
            Employee record = mEmployees.Find(e => e.EmployeeID == id);
            if (record != null)
            {
                record.City = emp.City; 
                record.Address = emp.Address;
                record.Title = emp.Title;
                record.HomePhone = emp.HomePhone;
                return true;
            }
            return false;
        }
        /// <summary>
        /// 获取所有雇员信息
        /// </summary>
        /// <returns>[{"employeeID":0,"lastName":null,"firstName":null,"title":null,"titleOfCourtesy":null,"birthDate":"0001-01-01T00:00:00","hireDate":"0001-01-01T00:00:00","address":null,"city":null,"region":null,"postalCode":null,"country":null,"homePhone":null,"extension":null,"photo":null,"notes":null}]</returns>
        public object ListEmployees()
        {
            return mEmployees;
        }
        /// <summary>
        /// 获取雇员名称列表
        /// </summary>
        /// <returns>[{ID="",Name=""}]</returns>
        public object GetEmployeesName()
        {
            return from e in mEmployees select new { ID = e.EmployeeID, Name = e.FirstName + " " + e.LastName };
        }
        /// <summary>
        /// 获取客户名称列表
        /// </summary>
        /// <returns>[{ID="",Name=""}]</returns>
        public object GetCustomersName()
        {
            return from c in mCustomers select new { ID = c.CustomerID, Name = c.CompanyName };
        }
        /// <summary>
        /// 订单查询
        /// </summary>
        /// <param name="employeeid">雇员ID</param>
        /// <param name="customerid">客户ID</param>
        /// <param name="index">分页索引</param>
        /// <returns>{Index:0,Pages:0,Items:[order],Count:0}</returns>
        public object ListOrders(int employeeid, string customerid, int index, IHttpContext context)
        {
            Func<Order, bool> exp = o => (employeeid == 0 || o.EmployeeID == employeeid)
             && (string.IsNullOrEmpty(customerid) || o.CustomerID == customerid);
            int count = mOrders.Count(exp);
            int size = 20;
            int pages = count / size;
            if (count % size > 0)
                pages++;
            var items = mOrders.Where(exp).Skip(index * size).Take(size);
            return new { Index = index, Pages = pages, Items = items, Count = count };

        }
        [NotAction]
        public void Init(BeetleX.FastHttpApi.HttpApiServer server,string path)
        {
            mServer = server;
        }
    }
}
