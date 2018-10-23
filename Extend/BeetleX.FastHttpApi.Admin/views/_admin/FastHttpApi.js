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


function FastHttpApi(url, params, http, post) {
    if (http == true)
        this.http = true;
    else
        this.http = false;
    this.url = url;
    this.post = false;
    if (post == true)
        this.post = true;
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
        __id = 0;
    var httpurl;
    var keys;
    var index;
    this.params['_requestid'] = id;
    if (this.http || __websocket.status == false) {
        if (this.post) {
            httpurl = this.url;
            $.post(httpurl, JSON.stringify(this.params), function (result) {
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
                if (this.params[keys[i]]) {
                    if (index == 0) {
                        httpurl += "?";
                    }
                    else {
                        httpurl += "&";
                    }
                    httpurl += keys[i] + '=' + encodeURIComponent(this.params[keys[i]]);
                    index++;
                }
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

function api(url, params, http, post) {
    return new FastHttpApi(url, params, http, post);
}

function api_receive(callback) {
    __receive = callback;
}

var __websocket = new FastHttpApiWebSocket();
__websocket.Connect();

