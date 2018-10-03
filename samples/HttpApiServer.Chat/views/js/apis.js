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

var $Onlines$url = '/onlines';
///<summary>
///  获取在线人数
/// </summary>
/// <returns>{ID, Name, IPAddress}</returns>
function $Onlines(useHttp) {
    return api($Onlines$url, {}, useHttp).sync();
}
function $Onlines$async(useHttp) {
    return api($Onlines$url, {}, useHttp);
}
var $GetRoomOnlines$url = '/getroomonlines';
///<summary>
/// 获取房间在线人数
/// </summary>
/// <param name="roomName">房间名称</param>
/// <returns>{ID, Name, IPAddress}</returns>
function $GetRoomOnlines(roomName, useHttp) {
    return api($GetRoomOnlines$url, { roomName: roomName }, useHttp).sync();
}
function $GetRoomOnlines$async(roomName, useHttp) {
    return api($GetRoomOnlines$url, { roomName: roomName }, useHttp);
}
var $Login$url = '/login';
///<summary>
/// 用户登陆
/// </summary>
/// <param name="userName">用户名</param>
/// <returns>true|false</returns>
function $Login(userName, useHttp) {
    return api($Login$url, { userName: userName }, useHttp).sync();
}
function $Login$async(userName, useHttp) {
    return api($Login$url, { userName: userName }, useHttp);
}
var $ListRooms$url = '/listrooms';
///<summary>
/// 获取所有房间信息
/// </summary>
/// <returns>{Name,Count}</returns>
function $ListRooms(useHttp) {
    return api($ListRooms$url, {}, useHttp).sync();
}
function $ListRooms$async(useHttp) {
    return api($ListRooms$url, {}, useHttp);
}
var $CloseSession$url = '/closesession';
///<summary>
/// 关闭连接
/// </summary>
/// <param name="sessions">[id1,id2,id3]</param>
function $CloseSession(body, useHttp) {
    return api($CloseSession$url, { body: body }, useHttp).sync();
}
function $CloseSession$async(body, useHttp) {
    return api($CloseSession$url, { body: body }, useHttp);
}
var $CloseRoom$url = '/closeroom';
///<summary>
/// 关闭房间
/// </summary>
/// <param name="roomName">房间名称</param>
function $CloseRoom(roomName, useHttp) {
    return api($CloseRoom$url, { roomName: roomName }, useHttp).sync();
}
function $CloseRoom$async(roomName, useHttp) {
    return api($CloseRoom$url, { roomName: roomName }, useHttp);
}
var $CheckOutRoom$url = '/checkoutroom';
///<summary>
/// 退出房间
/// </summary>
/// <param name="roomName">房间名称</param>
/// <returns>{Code:200,Error}</returns>
function $CheckOutRoom(roomName, useHttp) {
    return api($CheckOutRoom$url, { roomName: roomName }, useHttp).sync();
}
function $CheckOutRoom$async(roomName, useHttp) {
    return api($CheckOutRoom$url, { roomName: roomName }, useHttp);
}
var $CheckInRoom$url = '/checkinroom';
///<summary>
/// 进入房间
/// </summary>
/// <param name="roomName">进入房间</param>
/// <returns>{Code:200,Error}</returns>
function $CheckInRoom(roomName, useHttp) {
    return api($CheckInRoom$url, { roomName: roomName }, useHttp).sync();
}
function $CheckInRoom$async(roomName, useHttp) {
    return api($CheckInRoom$url, { roomName: roomName }, useHttp);
}
var $CreateRoom$url = '/createroom';
///<summary>
/// 创建记房间
/// </summary>
/// <param name="roomName">房间名称</param>
/// <returns>{Code:200,Error}</returns>
function $CreateRoom(roomName, useHttp) {
    return api($CreateRoom$url, { roomName: roomName }, useHttp).sync();
}
function $CreateRoom$async(roomName, useHttp) {
    return api($CreateRoom$url, { roomName: roomName }, useHttp);
}
var $SendMessage$url = '/sendmessage';
///<summary>
/// 发送消息
/// </summary>
/// <param name="message">消息内容</param>
function $SendMessage(message, useHttp) {
    return api($SendMessage$url, { message: message }, useHttp).sync();
}
function $SendMessage$async(message, useHttp) {
    return api($SendMessage$url, { message: message }, useHttp);
}
