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
    public class Controller : IController
    {
        public Controller()
        {
            mEmployees = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Employee>>(Datas.Employees);
            mCustomers = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Customer>>(Datas.Customers);
            mOrders = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Order>>(Datas.Orders);
        }
        private List<Employee> mEmployees;
        private List<Customer> mCustomers;
        private List<Order> mOrders;

        private BeetleX.FastHttpApi.HttpApiServer mServer;

        public Employee GetEmployee(int id)
        {
            Employee result = mEmployees.Find(e => e.EmployeeID == id);
            if (result == null)
                result = new Employee();
            return result;
        }
        public bool EditEmployee(int id, [BodyParameter]Employee emp, BeetleX.FastHttpApi.HttpResponse response)
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
        public object ListEmployees()
        {
            return mEmployees;
        }
        public object GetEmployeesName()
        {
            return from e in mEmployees select new { ID = e.EmployeeID, Name = e.FirstName + " " + e.LastName };
        }
        public object GetCustomersName()
        {
            return from c in mCustomers select new { ID = c.CustomerID, Name = c.CompanyName };
        }
        public object ListOrders(int employeeid, string customerid, IDataContext context)
        {
            return mOrders.Where(o =>
            (employeeid == 0 || o.EmployeeID == employeeid)
            &&
            (string.IsNullOrEmpty(customerid) || o.CustomerID == customerid));
        }

        private void OnWebSocketReceive(object sender, WebSocketReceiveArgs e)
        {
            ActionResult result = mServer.ExecuteWS(e.Request, e.Frame);
            if (result.Code == 403)
            {
                mServer.BaseServer.Log(LogType.Error, e.Sesson, string.Format("{0} error {1}", result.Url, result.Error));
            }
        }

        public void Init(BeetleX.FastHttpApi.HttpApiServer server)
        {
            mServer = server;
            if (server.WebSocketReceive == null)
            {
                server.WebSocketReceive = OnWebSocketReceive;
            }
        }
    }
}
