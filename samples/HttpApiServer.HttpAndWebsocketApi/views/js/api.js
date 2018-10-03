/*
FastHttpApi javascript api Generator Copyright © henryfan 2018 email:henryfan@msn.com
*/
/******** api base **********/
var __id = 0;
var __receive;
var __connect;
var __disconnect;
function FastHttpApiWebSocket() {
    this.wsUri = "ws://" + window.location.host;
    this.websocket;
    this.status = false;
    this.messagHandlers = new Object();
}

FastHttpApiWebSocket.prototype.send = function (url, params, callback) {
    if (this.status == false) {
        if (callback != null) {
            callback({ Url: url, Code: 505, Error: 'disconnect' })
        }
    }
    this.messagHandlers[params._requestid] = callback;
    var data = { url: url, params: params };
    this.websocket.send(JSON.stringify(data));
}

FastHttpApiWebSocket.prototype.onOpen = function (evt) {
    this.status = true;
    if (__connect)
        __connect(this);
}

FastHttpApiWebSocket.prototype.onClose = function (evt) {
    this.status = false;
    var _this = this;
    if (__disconnect)
        __disconnect(this);
    if (evt.code == 1006) {
        setTimeout(function () {
            _this.Connect();
        }, 2000);
    }

}

FastHttpApiWebSocket.prototype.onMessage = function (evt) {
    var msg = JSON.parse(evt.data);
    var callback = this.messagHandlers[msg.ID];
    if (callback)
        callback(msg);
    else
        if (__receive)
            __receive(msg);
}
FastHttpApiWebSocket.prototype.onError = function (evt) {

}

FastHttpApiWebSocket.prototype.Connect = function () {
    this.websocket = new WebSocket(this.wsUri);
    _this = this;
    this.websocket.onopen = function (evt) { _this.onOpen(evt) };
    this.websocket.onclose = function (evt) { _this.onClose(evt) };
    this.websocket.onmessage = function (evt) { _this.onMessage(evt) };
    this.websocket.onerror = function (evt) { _this.onError(evt) };
}


function FastHttpApi(url, params, http) {
    if (http == true)
        this.http = true;
    else
        this.http = false;
    this.url = url;
    this.params = params;
    if (!this.params)
        this.params = new Object();

}

FastHttpApi.prototype.sync = function () {
    var _this = this;
    return new Promise(resolve => {
        _this.execute(function (result) {
            resolve(result);
        });
    });
}
FastHttpApi.prototype.httpRequest = function () {
    this.http = true;
    return this.sync();
}

FastHttpApi.prototype.execute = function (callback, http) {
    if (http == true)
        this.http = true;
    var id = ++__id;
    if (__id > 1024)
        __id = 1024;
    var httpurl;
    var keys;
    var index;
    this.params['_requestid'] = id;
    if (this.http || __websocket.status == false) {
        if (this.params['body']) {
            //post
            var body;
            httpurl = this.url;
            keys = Object.keys(this.params);
            index = 0;
            for (i = 0; i < keys.length; i++) {
                if (keys[i] == 'body') {
                    body = this.params[keys[i]];
                }
                else {
                    if (index == 0) {
                        httpurl += "?";
                    }
                    else {
                        httpurl += "&";
                    }
                    httpurl += keys[i] + '=' + this.params[keys[i]];
                    index++;
                }
            }
            $.post(httpurl, JSON.stringify(body), function (result) {
                if (callback)
                    callback(result);
            });
        }
        else {
            //get
            httpurl = this.url;
            keys = Object.keys(this.params);
            index = 0;
            for (i = 0; i < keys.length; i++) {
                if (index == 0) {
                    httpurl += "?";
                }
                else {
                    httpurl += "&";
                }
                httpurl += keys[i] + '=' + this.params[keys[i]];
                index++;
            }
            $.get(httpurl, function (result) {
                if (callback)
                    callback(result);
            });
        }
    }
    else {
        __websocket.send(this.url, this.params, callback);
    }

}


function api_connect(callback) {
    __connect = callback;
}

function api_disconnect(callback) {
    __disconnect = callback;
}

function api(url, params, http) {
    return new FastHttpApi(url, params, http);
}

function api_receive(callback) {
    __receive = callback;
}

var __websocket = new FastHttpApiWebSocket();
__websocket.Connect();
/******** api base **********/

var $GetEmployee$url = '/getemployee';
///<summary>
/// 获取雇员信息
/// </summary>
/// <param name="id">雇员ID</param>
/// <returns>{"employeeID":0,"lastName":null,"firstName":null,"title":null,"titleOfCourtesy":null,"birthDate":"0001-01-01T00:00:00","hireDate":"0001-01-01T00:00:00","address":null,"city":null,"region":null,"postalCode":null,"country":null,"homePhone":null,"extension":null,"photo":null,"notes":null}</returns>
function $GetEmployee(id, useHttp) {
    return api($GetEmployee$url, { id: id }, useHttp).sync();
}
function $GetEmployee$async(id, useHttp) {
    return api($GetEmployee$url, { id: id }, useHttp);
}
var $EditEmployee$url = '/editemployee';
///<summary>
/// 修改雇员信息
/// </summary>
/// <param name="id">雇员ID</param>
/// <param name="emp">雇员信息:{"employeeID":0,"lastName":null,"firstName":null,"title":null,"titleOfCourtesy":null,"birthDate":"0001-01-01T00:00:00","hireDate":"0001-01-01T00:00:00","address":null,"city":null,"region":null,"postalCode":null,"country":null,"homePhone":null,"extension":null,"photo":null,"notes":null}</param>
/// <returns>bool</returns>
function $EditEmployee(id, body, useHttp) {
    return api($EditEmployee$url, { id: id, body: body }, useHttp).sync();
}
function $EditEmployee$async(id, body, useHttp) {
    return api($EditEmployee$url, { id: id, body: body }, useHttp);
}
var $ListEmployees$url = '/listemployees';
///<summary>
/// 获取所有雇员信息
/// </summary>
/// <returns>[{"employeeID":0,"lastName":null,"firstName":null,"title":null,"titleOfCourtesy":null,"birthDate":"0001-01-01T00:00:00","hireDate":"0001-01-01T00:00:00","address":null,"city":null,"region":null,"postalCode":null,"country":null,"homePhone":null,"extension":null,"photo":null,"notes":null}]</returns>
function $ListEmployees(useHttp) {
    return api($ListEmployees$url, {}, useHttp).sync();
}
function $ListEmployees$async(useHttp) {
    return api($ListEmployees$url, {}, useHttp);
}
var $GetEmployeesName$url = '/getemployeesname';
///<summary>
/// 获取雇员名称列表
/// </summary>
/// <returns>[{ID="",Name=""}]</returns>
function $GetEmployeesName(useHttp) {
    return api($GetEmployeesName$url, {}, useHttp).sync();
}
function $GetEmployeesName$async(useHttp) {
    return api($GetEmployeesName$url, {}, useHttp);
}
var $GetCustomersName$url = '/getcustomersname';
///<summary>
/// 获取客户名称列表
/// </summary>
/// <returns>[{ID="",Name=""}]</returns>
function $GetCustomersName(useHttp) {
    return api($GetCustomersName$url, {}, useHttp).sync();
}
function $GetCustomersName$async(useHttp) {
    return api($GetCustomersName$url, {}, useHttp);
}
var $ListOrders$url = '/listorders';
///<summary>
/// 订单查询
/// </summary>
/// <param name="employeeid">雇员ID</param>
/// <param name="customerid">客户ID</param>
/// <param name="index">分页索引</param>
/// <returns>{Index:0,Pages:0,Items:[order],Count:0}</returns>
function $ListOrders(employeeid, customerid, index, useHttp) {
    return api($ListOrders$url, { employeeid: employeeid, customerid: customerid, index: index }, useHttp).sync();
}
function $ListOrders$async(employeeid, customerid, index, useHttp) {
    return api($ListOrders$url, { employeeid: employeeid, customerid: customerid, index: index }, useHttp);
}
