function UrlHelper() {
    var vars = [], hash;
    var hashes = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');
    for (var i = 0; i < hashes.length; i++) {
        hash = hashes[i].split('=');
        vars.push(hash[0]);
        vars[hash[0].toLowerCase()] = hash[1];
    }
    this.queryString = vars;
    this.tag = null;
    this.ssl = window.location.protocol == "https:"
    var url = document.location.pathname;
    var tagIndex = document.location.href.indexOf('#');
    if (tagIndex > 0) {
        this.tag = document.location.href.substring(tagIndex + 1);
    }
    this.folder = url.substring(url.indexOf('/'), url.lastIndexOf('/'));
    url = url.substring(0, (url.indexOf("#") == -1) ? url.length : url.indexOf("#"));
    url = url.substring(0, (url.indexOf("?") == -1) ? url.length : url.indexOf("?"));
    url = url.substring(url.lastIndexOf("/") + 1, url.length);
    if (url) {
        this.fileName = decodeURIComponent(url);
        this.ext = this.fileName.substring(this.fileName.lastIndexOf(".") + 1, this.fileName.length)
        this.fileNameWithOutExt = this.fileName.substring(0, this.fileName.lastIndexOf(".") == -1 ? this.fileName.length : this.fileName.lastIndexOf("."));
    }
    
}

var _url = new UrlHelper();

function beetlexWebSocket() {
    this.wsUri = null;
    if (window.location.protocol == "https:") {
        this.wsUri = "wss://" + window.location.host;
    }
    else {
        this.wsUri = "ws://" + window.location.host;
    }
    this.websocket;
    this.status = false;
    this.messagHandlers = new Object();
    this.timeout = 2000;
    this.receive = null;
}

beetlexWebSocket.prototype.send = function (url, params, callback) {
    if (this.status == false) {
        if (callback != null) {
            callback({ Url: url, Code: 505, Error: 'disconnect' })
        }
    }
    this.messagHandlers[params._requestid] = callback;
    var data = { url: url, params: params };
    this.websocket.send(JSON.stringify(data));
}

beetlexWebSocket.prototype.onOpen = function (evt) {
    this.status = true;
}

beetlexWebSocket.prototype.onClose = function (evt) {
    this.status = false;
    var _this = this;
    if (evt.code == 1006) {
        setTimeout(function () {
            _this.Connect();
        }, _this.timeout);
        if (_this.timeout < 10000)
            _this.timeout += 1000;
    }

}

beetlexWebSocket.prototype.onMessage = function (evt) {
    var msg = JSON.parse(evt.data);
    var callback = this.messagHandlers[msg.ID];
    if (callback)
        callback(msg);
    else
        if (this.callback) {
            if (msg.Data != null && msg.Data != undefined)
                this.receive(msg.Data);
            else
                this.receive(msg);
        }
}

beetlexWebSocket.prototype.onReceiveMessage = function (callback) {
    this.callback = callback;
};
beetlexWebSocket.prototype.onError = function (evt) {

}

beetlexWebSocket.prototype.connect = function () {
    this.websocket = new WebSocket(this.wsUri);
    _this = this;
    this.websocket.onopen = function (evt) { _this.onOpen(evt) };
    this.websocket.onclose = function (evt) { _this.onClose(evt) };
    this.websocket.onmessage = function (evt) { _this.onMessage(evt) };
    this.websocket.onerror = function (evt) { _this.onError(evt) };
}

function beetlex4axios() {
    this._requestid = 1;
    this.errorHandlers = new Object();
    this.websocket = new beetlexWebSocket();
}

beetlex4axios.prototype.useWebsocket = function (host) {
    if (host)
        this.websocket.wsUri = host;
    this.websocket.connect();
}

beetlex4axios.prototype.getErrorHandler = function (code) {
    return this.errorHandlers[code];
}

beetlex4axios.prototype.SetErrorHandler = function (code, callback) {
    this.errorHandlers[code] = callback;
}

beetlex4axios.prototype.getRequestID = function () {

    this._requestid++;
    if (this._requestid > 2000) {
        this._requestid = 1;
    }

    return this._requestid;
}

beetlex4axios.prototype.get = function (url, params, callback) {
    var httpurl = url;
    if (!params)
        params = new Object();
    var _this = this;
    params['_requestid'] = this.getRequestID();
    if (this.websocket.status == true) {
        var wscallback = function (r) {
            var data = r.Data;
            if (data.Code && data.Code != 200) {
                _this.onError(data.Code, data.Error);
            }
            else {
                if (callback) {
                    if (data.Data != null && data.Data != undefined)
                        callback(data.Data);
                    else
                        callback(data);
                }
            }
        };
        this.websocket.send(url, params, wscallback);
    }
    else {
        axios.get(httpurl, { params: params, headers: { 'Content-Type': 'application/json;charset=UTF-8' } })
            .then(function (response) {
                var data = response.data;
                if (data.Code && data.Code != 200) {
                    _this.onError(data.Code, data.Error);
                }
                else {
                    if (callback) {
                        if (data.Data != null && data.Data != undefined)
                            callback(data.Data);
                        else
                            callback(data);
                    }
                }
            })
            .catch(function (error) {
                var code = error.response ? error.response.status : 500;
                var message = error.message;
                if (error.response)
                    message += "\r\n" + error.response.data;
                _this.onError(code, message);
            });
    }
};

beetlex4axios.prototype.onError = function (code, message) {
    var handler = this.getErrorHandler(code);
    if (handler)
        handler(message);
    else
        alert(message);
}

beetlex4axios.prototype.post = function (url, params, callback) {
    var httpurl = url;
    if (!params)
        params = new Object();
    var id = this.getRequestID();
    var _this = this;
    params['_requestid'] = id;

    if (this.websocket.status == true) {
        var wscallback = function (r) {
            var data = r;
            if (data.Code && data.Code != 200) {
                _this.onError(data.Code, data.Error);
            }
            else {
                if (callback) {
                    if (data.Data != null && data.Data != undefined)
                        callback(data.Data);
                    else
                        callback(data);
                }
            }
        };
        this.websocket.send(url, params, wscallback);
    }
    else {
        axios.post(httpurl, JSON.stringify(params), { headers: { 'Content-Type': 'application/json;charset=UTF-8' } })
            .then(function (response) {
                var data = response.data;
                if (data.Code && data.Code != 200) {
                    _this.onError(data.Code, data.Error);
                }
                else {
                    if (callback) {
                        if (data.Data != null && data.Data != undefined)
                            callback(data.Data);
                        else
                            callback(data);
                    }
                }
            })
            .catch(function (error) {
                var code = error.response ? error.response.status : 500;
                var message = error.message;
                if (error.response)
                    message += "\r\n" + error.response.data;
                _this.onError(code, message);
            });
    }
};

var beetlex = new beetlex4axios();

function beetlexAction(actionUrl, actionData, defaultResult) {
    this.url = actionUrl;
    this.data = actionData;
    this.result = defaultResult;
    this.requesting = null;
    this.requested = null;

}

beetlexAction.prototype.onCallback = function (data) {
    if (this.requested)
        this.requested(data);
}

beetlexAction.prototype.onValidate = function (data) {
    if (this.requesting)
        return this.requesting(data);
    return true;
}

beetlexAction.prototype.get = function (data) {
    var _this = this;
    var _postData = this.data;
    if (data)
        _postData = data;
    if (!this.onValidate(_postData))
        return;
    beetlex.get(this.url, _postData, function (r) {

        _this.result = r;
        _this.onCallback(r);
    });
};

beetlexAction.prototype.post = function (data) {
    var _this = this;
    var _postData = this.data;
    if (data)
        _postData = data;
    if (!this.onValidate(_postData))
        return;
    beetlex.post(this.url, _postData, function (r) {
        _this.result = r;
        _this.onCallback(r);
    });

};


