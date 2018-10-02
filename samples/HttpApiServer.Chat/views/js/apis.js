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
        }, 1000);
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


function FastHttpApi(url, params) {
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
    this.params['_requestid'] = id;
    if (this.http || __websocket.status == false) {
        if (this.params['body']) {
            //post
            var body;
            var httpurl = this.url;
            var keys = Object.keys(this.params);
            var index = 0;
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
            var httpurl = this.url;
            var keys = Object.keys(this.params);
            var index = 0;
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
/**
*关闭指定的连接,需要后台管理权限
*
* @param body [{"ID":0,"IPAddress":null},{"ID":0,"IPAddress":null}]
*/
var $_admin$CloseSession$url = '/_admin/closesession';
function $_admin$CloseSession$async(body) {
    return api($_admin$CloseSession$url, { body: body });
}
function $_admin$CloseSession(body) {
    return api($_admin$CloseSession$url, { body: body }).sync();
}
/**
*获取基于API相应调用Javascript代码,兼容http和websocket
*
*/
var $_admin$GetApiScript$url = '/_admin/getapiscript';
function $_admin$GetApiScript$async() {
    return api($_admin$GetApiScript$url);
}
function $_admin$GetApiScript() {
    return api($_admin$GetApiScript$url).sync();
}
/**
*获取后台登陆凭证
*
*/
var $_admin$GetKey$url = '/_admin/getkey';
function $_admin$GetKey$async() {
    return api($_admin$GetKey$url);
}
function $_admin$GetKey() {
    return api($_admin$GetKey$url).sync();
}
/**
*获取基础服务信息,需要后台管理权限
*
*/
var $_admin$GetServerInfo$url = '/_admin/getserverinfo';
function $_admin$GetServerInfo$async() {
    return api($_admin$GetServerInfo$url);
}
function $_admin$GetServerInfo() {
    return api($_admin$GetServerInfo$url).sync();
}
/**
*获取所有接口信息,需要后台管理权
*
*/
var $_admin$ListApi$url = '/_admin/listapi';
function $_admin$ListApi$async() {
    return api($_admin$ListApi$url);
}
function $_admin$ListApi() {
    return api($_admin$ListApi$url).sync();
}
/**
*获取在线连接信息,需要后台管理权限
*
* @param index 0
*/
var $_admin$ListConnection$url = '/_admin/listconnection';
function $_admin$ListConnection$async(index) {
    return api($_admin$ListConnection$url, { index: index });
}
function $_admin$ListConnection(index) {
    return api($_admin$ListConnection$url, { index: index }).sync();
}
/**
*管理后台登陆
*
* @param name ""
* @param pwd ""
*/
var $_admin$Login$url = '/_admin/login';
function $_admin$Login$async(name, pwd) {
    return api($_admin$Login$url, { name: name, pwd: pwd });
}
function $_admin$Login(name, pwd) {
    return api($_admin$Login$url, { name: name, pwd: pwd }).sync();
}
/**
*进入房间
*
* @param roomName ""
*/
var $CheckInRoom$url = '/checkinroom';
function $CheckInRoom$async(roomName) {
    return api($CheckInRoom$url, { roomName: roomName });
}
function $CheckInRoom(roomName) {
    return api($CheckInRoom$url, { roomName: roomName }).sync();
}
/**
*退出房间
*
* @param roomName ""
*/
var $CheckOutRoom$url = '/checkoutroom';
function $CheckOutRoom$async(roomName) {
    return api($CheckOutRoom$url, { roomName: roomName });
}
function $CheckOutRoom(roomName) {
    return api($CheckOutRoom$url, { roomName: roomName }).sync();
}
/**
*关闭房间
*
* @param roomName ""
*/
var $CloseRoom$url = '/closeroom';
function $CloseRoom$async(roomName) {
    return api($CloseRoom$url, { roomName: roomName });
}
function $CloseRoom(roomName) {
    return api($CloseRoom$url, { roomName: roomName }).sync();
}
/**
*关闭连接
*
* @param body [0,0]
*/
var $CloseSession$url = '/closesession';
function $CloseSession$async(body) {
    return api($CloseSession$url, { body: body });
}
function $CloseSession(body) {
    return api($CloseSession$url, { body: body }).sync();
}
/**
*创建房间
*
* @param roomName ""
*/
var $CreateRoom$url = '/createroom';
function $CreateRoom$async(roomName) {
    return api($CreateRoom$url, { roomName: roomName });
}
function $CreateRoom(roomName) {
    return api($CreateRoom$url, { roomName: roomName }).sync();
}
/**
*获取指定房间的在线人数
*
* @param roomName ""
*/
var $GetRoomOnlines$url = '/getroomonlines';
function $GetRoomOnlines$async(roomName) {
    return api($GetRoomOnlines$url, { roomName: roomName });
}
function $GetRoomOnlines(roomName) {
    return api($GetRoomOnlines$url, { roomName: roomName }).sync();
}
/**
*获取所有房间信息
*
*/
var $ListRooms$url = '/listrooms';
function $ListRooms$async() {
    return api($ListRooms$url);
}
function $ListRooms() {
    return api($ListRooms$url).sync();
}
/**
*用户登陆
*
* @param userName ""
*/
var $Login$url = '/login';
function $Login$async(userName) {
    return api($Login$url, { userName: userName });
}
function $Login(userName) {
    return api($Login$url, { userName: userName }).sync();
}
/**
*获取所有在线人数
*
*/
var $Onlines$url = '/onlines';
function $Onlines$async() {
    return api($Onlines$url);
}
function $Onlines() {
    return api($Onlines$url).sync();
}
/**
*发送消息
*
* @param message ""
*/
var $SendMessage$url = '/sendmessage';
function $SendMessage$async(message) {
    return api($SendMessage$url, { message: message });
}
function $SendMessage(message) {
    return api($SendMessage$url, { message: message }).sync();
}