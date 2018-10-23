/************************************************************************************
FastHttpApi javascript api Generator Copyright © henryfan 2018 email:henryfan@msn.com
https://github.com/IKende/FastHttpApi
**************************************************************************************/




/** 
获取雇员信息 url 
**/
var HomeGetEmployeeUrl = '/getemployee';
/** 获取雇员信息 'var result=await HomeGetEmployee(params)'
/* @param id 雇员ID
/* @param useHttp only http request
/* @return {"employeeID":0,"lastName":null,"firstName":null,"title":null,"titleOfCourtesy":null,"birthDate":"0001-01-01T00:00:00","hireDate":"0001-01-01T00:00:00","address":null,"city":null,"region":null,"postalCode":null,"country":null,"homePhone":null,"extension":null,"photo":null,"notes":null}
**/
function HomeGetEmployee(id, useHttp) {
    return api(HomeGetEmployeeUrl, { id: id }, useHttp).sync();
}
/** 获取雇员信息 'HomeGetEmployeeAsync(params).execute(function(result){},useHttp)'
/* @param id 雇员ID
/* @param useHttp only http request
/* @return {"employeeID":0,"lastName":null,"firstName":null,"title":null,"titleOfCourtesy":null,"birthDate":"0001-01-01T00:00:00","hireDate":"0001-01-01T00:00:00","address":null,"city":null,"region":null,"postalCode":null,"country":null,"homePhone":null,"extension":null,"photo":null,"notes":null}
**/
function HomeGetEmployeeAsync(id, useHttp) {
    return api(HomeGetEmployeeUrl, { id: id }, useHttp);
}
/** 
修改雇员信息 url 
**/
var HomeEditEmployeeUrl = '/editemployee';
/** 修改雇员信息 'var result=await HomeEditEmployee(params)'
/* @param id 雇员ID
/* @param emp 雇员信息:{"employeeID":0,"lastName":null,"firstName":null,"title":null,"titleOfCourtesy":null,"birthDate":"0001-01-01T00:00:00","hireDate":"0001-01-01T00:00:00","address":null,"city":null,"region":null,"postalCode":null,"country":null,"homePhone":null,"extension":null,"photo":null,"notes":null}
/* @param useHttp only http request
/* @return bool
**/
function HomeEditEmployee(id, emp, useHttp) {
    return api(HomeEditEmployeeUrl, { id: id, emp: emp }, useHttp, true).sync();
}
/** 修改雇员信息 'HomeEditEmployeeAsync(params).execute(function(result){},useHttp)'
/* @param id 雇员ID
/* @param emp 雇员信息:{"employeeID":0,"lastName":null,"firstName":null,"title":null,"titleOfCourtesy":null,"birthDate":"0001-01-01T00:00:00","hireDate":"0001-01-01T00:00:00","address":null,"city":null,"region":null,"postalCode":null,"country":null,"homePhone":null,"extension":null,"photo":null,"notes":null}
/* @param useHttp only http request
/* @return bool
**/
function HomeEditEmployeeAsync(id, emp, useHttp) {
    return api(HomeEditEmployeeUrl, { id: id, emp: emp }, useHttp, true);
}
/** 
获取所有雇员信息 url 
**/
var HomeListEmployeesUrl = '/listemployees';
/** 获取所有雇员信息 'var result=await HomeListEmployees(params)'
/* @param useHttp only http request
/* @return [{"employeeID":0,"lastName":null,"firstName":null,"title":null,"titleOfCourtesy":null,"birthDate":"0001-01-01T00:00:00","hireDate":"0001-01-01T00:00:00","address":null,"city":null,"region":null,"postalCode":null,"country":null,"homePhone":null,"extension":null,"photo":null,"notes":null}]
**/
function HomeListEmployees(useHttp) {
    return api(HomeListEmployeesUrl, {}, useHttp).sync();
}
/** 获取所有雇员信息 'HomeListEmployeesAsync(params).execute(function(result){},useHttp)'
/* @param useHttp only http request
/* @return [{"employeeID":0,"lastName":null,"firstName":null,"title":null,"titleOfCourtesy":null,"birthDate":"0001-01-01T00:00:00","hireDate":"0001-01-01T00:00:00","address":null,"city":null,"region":null,"postalCode":null,"country":null,"homePhone":null,"extension":null,"photo":null,"notes":null}]
**/
function HomeListEmployeesAsync(useHttp) {
    return api(HomeListEmployeesUrl, {}, useHttp);
}
/** 
获取雇员名称列表 url 
**/
var HomeGetEmployeesNameUrl = '/getemployeesname';
/** 获取雇员名称列表 'var result=await HomeGetEmployeesName(params)'
/* @param useHttp only http request
/* @return [{ID="",Name=""}]
**/
function HomeGetEmployeesName(useHttp) {
    return api(HomeGetEmployeesNameUrl, {}, useHttp).sync();
}
/** 获取雇员名称列表 'HomeGetEmployeesNameAsync(params).execute(function(result){},useHttp)'
/* @param useHttp only http request
/* @return [{ID="",Name=""}]
**/
function HomeGetEmployeesNameAsync(useHttp) {
    return api(HomeGetEmployeesNameUrl, {}, useHttp);
}
/** 
获取客户名称列表 url 
**/
var HomeGetCustomersNameUrl = '/getcustomersname';
/** 获取客户名称列表 'var result=await HomeGetCustomersName(params)'
/* @param useHttp only http request
/* @return [{ID="",Name=""}]
**/
function HomeGetCustomersName(useHttp) {
    return api(HomeGetCustomersNameUrl, {}, useHttp).sync();
}
/** 获取客户名称列表 'HomeGetCustomersNameAsync(params).execute(function(result){},useHttp)'
/* @param useHttp only http request
/* @return [{ID="",Name=""}]
**/
function HomeGetCustomersNameAsync(useHttp) {
    return api(HomeGetCustomersNameUrl, {}, useHttp);
}
/** 
订单查询 url 
**/
var HomeListOrdersUrl = '/listorders';
/** 订单查询 'var result=await HomeListOrders(params)'
/* @param employeeid 雇员ID
/* @param customerid 客户ID
/* @param index 分页索引
/* @param useHttp only http request
/* @return {Index:0,Pages:0,Items:[order],Count:0}
**/
function HomeListOrders(employeeid, customerid, index, useHttp) {
    return api(HomeListOrdersUrl, { employeeid: employeeid, customerid: customerid, index: index }, useHttp).sync();
}
/** 订单查询 'HomeListOrdersAsync(params).execute(function(result){},useHttp)'
/* @param employeeid 雇员ID
/* @param customerid 客户ID
/* @param index 分页索引
/* @param useHttp only http request
/* @return {Index:0,Pages:0,Items:[order],Count:0}
**/
function HomeListOrdersAsync(employeeid, customerid, index, useHttp) {
    return api(HomeListOrdersUrl, { employeeid: employeeid, customerid: customerid, index: index }, useHttp);
}
