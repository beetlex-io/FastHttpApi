/************************************************************************************
FastHttpApi javascript api Generator Copyright © henryfan 2018 email:henryfan@msn.com
https://github.com/IKende/FastHttpApi
**************************************************************************************/




/** 
获取雇员信息 url 
**/
var HomeGetEmployeeUrl='/GetEmployee';
/** 获取雇员信息 'HomeGetEmployee(params).execute(function(result){},useHttp)'
/* @param id 雇员ID
/* @param useHttp only http request
/* @return {"employeeID":0,"lastName":null,"firstName":null,"title":null,"titleOfCourtesy":null,"birthDate":"0001-01-01T00:00:00","hireDate":"0001-01-01T00:00:00","address":null,"city":null,"region":null,"postalCode":null,"country":null,"homePhone":null,"extension":null,"photo":null,"notes":null}
**/
function HomeGetEmployee(id,useHttp)
{
    return api(HomeGetEmployeeUrl,{id:id},useHttp);
}
/** 
修改雇员信息 url 
**/
var HomeEditEmployeeUrl='/EditEmployee';
/** 修改雇员信息 'HomeEditEmployee(params).execute(function(result){},useHttp)'
/* @param id 雇员ID
/* @param emp 雇员信息:{"employeeID":0,"lastName":null,"firstName":null,"title":null,"titleOfCourtesy":null,"birthDate":"0001-01-01T00:00:00","hireDate":"0001-01-01T00:00:00","address":null,"city":null,"region":null,"postalCode":null,"country":null,"homePhone":null,"extension":null,"photo":null,"notes":null}
/* @param useHttp only http request
/* @return bool
**/
function HomeEditEmployee(id,emp,useHttp)
{
    return api(HomeEditEmployeeUrl,{id:id,emp:emp},useHttp,true);
}
/** 
获取所有雇员信息 url 
**/
var HomeListEmployeesUrl='/ListEmployees';
/** 获取所有雇员信息 'HomeListEmployees(params).execute(function(result){},useHttp)'
/* @param useHttp only http request
/* @return [{"employeeID":0,"lastName":null,"firstName":null,"title":null,"titleOfCourtesy":null,"birthDate":"0001-01-01T00:00:00","hireDate":"0001-01-01T00:00:00","address":null,"city":null,"region":null,"postalCode":null,"country":null,"homePhone":null,"extension":null,"photo":null,"notes":null}]
**/
function HomeListEmployees(useHttp)
{
    return api(HomeListEmployeesUrl,{},useHttp);
}
/** 
获取雇员名称列表 url 
**/
var HomeGetEmployeesNameUrl='/GetEmployeesName';
/** 获取雇员名称列表 'HomeGetEmployeesName(params).execute(function(result){},useHttp)'
/* @param useHttp only http request
/* @return [{ID="",Name=""}]
**/
function HomeGetEmployeesName(useHttp)
{
    return api(HomeGetEmployeesNameUrl,{},useHttp);
}
/** 
获取客户名称列表 url 
**/
var HomeGetCustomersNameUrl='/GetCustomersName';
/** 获取客户名称列表 'HomeGetCustomersName(params).execute(function(result){},useHttp)'
/* @param useHttp only http request
/* @return [{ID="",Name=""}]
**/
function HomeGetCustomersName(useHttp)
{
    return api(HomeGetCustomersNameUrl,{},useHttp);
}
/** 
订单查询 url 
**/
var HomeListOrdersUrl='/ListOrders';
/** 订单查询 'HomeListOrders(params).execute(function(result){},useHttp)'
/* @param employeeid 雇员ID
/* @param customerid 客户ID
/* @param index 分页索引
/* @param useHttp only http request
/* @return {Index:0,Pages:0,Items:[order],Count:0}
**/
function HomeListOrders(employeeid,customerid,index,useHttp)
{
    return api(HomeListOrdersUrl,{employeeid:employeeid,customerid:customerid,index:index},useHttp);
}
