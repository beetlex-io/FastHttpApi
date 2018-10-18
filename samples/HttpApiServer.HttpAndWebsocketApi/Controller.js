/************************************************************************************
FastHttpApi javascript api Generator Copyright © henryfan 2018 email:henryfan@msn.com
https://github.com/IKende/FastHttpApi
**************************************************************************************/

var $GetEmployee$url='/getemployee';
///<summary>
/// 获取雇员信息
/// </summary>
/// <param name="id">雇员ID</param>
/// <returns>{"employeeID":0,"lastName":null,"firstName":null,"title":null,"titleOfCourtesy":null,"birthDate":"0001-01-01T00:00:00","hireDate":"0001-01-01T00:00:00","address":null,"city":null,"region":null,"postalCode":null,"country":null,"homePhone":null,"extension":null,"photo":null,"notes":null}</returns>
function $GetEmployee(id,useHttp)
{
    return api($GetEmployee$url,{id:id},useHttp).sync();
}
function $GetEmployee$async(id,useHttp)
{
    return api($GetEmployee$url,{id:id},useHttp);
}
var $EditEmployee$url='/editemployee';
///<summary>
/// 修改雇员信息
/// </summary>
/// <param name="id">雇员ID</param>
/// <param name="emp">雇员信息:{"employeeID":0,"lastName":null,"firstName":null,"title":null,"titleOfCourtesy":null,"birthDate":"0001-01-01T00:00:00","hireDate":"0001-01-01T00:00:00","address":null,"city":null,"region":null,"postalCode":null,"country":null,"homePhone":null,"extension":null,"photo":null,"notes":null}</param>
/// <returns>bool</returns>
function $EditEmployee(id,body,useHttp)
{
    return api($EditEmployee$url,{id:id,body:body},useHttp).sync();
}
function $EditEmployee$async(id,body,useHttp)
{
    return api($EditEmployee$url,{id:id,body:body},useHttp);
}
var $ListEmployees$url='/listemployees';
///<summary>
/// 获取所有雇员信息
/// </summary>
/// <returns>[{"employeeID":0,"lastName":null,"firstName":null,"title":null,"titleOfCourtesy":null,"birthDate":"0001-01-01T00:00:00","hireDate":"0001-01-01T00:00:00","address":null,"city":null,"region":null,"postalCode":null,"country":null,"homePhone":null,"extension":null,"photo":null,"notes":null}]</returns>
function $ListEmployees(useHttp)
{
    return api($ListEmployees$url,{},useHttp).sync();
}
function $ListEmployees$async(useHttp)
{
    return api($ListEmployees$url,{},useHttp);
}
var $GetEmployeesName$url='/getemployeesname';
///<summary>
/// 获取雇员名称列表
/// </summary>
/// <returns>[{ID="",Name=""}]</returns>
function $GetEmployeesName(useHttp)
{
    return api($GetEmployeesName$url,{},useHttp).sync();
}
function $GetEmployeesName$async(useHttp)
{
    return api($GetEmployeesName$url,{},useHttp);
}
var $GetCustomersName$url='/getcustomersname';
///<summary>
/// 获取客户名称列表
/// </summary>
/// <returns>[{ID="",Name=""}]</returns>
function $GetCustomersName(useHttp)
{
    return api($GetCustomersName$url,{},useHttp).sync();
}
function $GetCustomersName$async(useHttp)
{
    return api($GetCustomersName$url,{},useHttp);
}
var $ListOrders$url='/listorders';
///<summary>
/// 订单查询
/// </summary>
/// <param name="employeeid">雇员ID</param>
/// <param name="customerid">客户ID</param>
/// <param name="index">分页索引</param>
/// <returns>{Index:0,Pages:0,Items:[order],Count:0}</returns>
function $ListOrders(employeeid,customerid,index,useHttp)
{
    return api($ListOrders$url,{employeeid:employeeid,customerid:customerid,index:index},useHttp).sync();
}
function $ListOrders$async(employeeid,customerid,index,useHttp)
{
    return api($ListOrders$url,{employeeid:employeeid,customerid:customerid,index:index},useHttp);
}
