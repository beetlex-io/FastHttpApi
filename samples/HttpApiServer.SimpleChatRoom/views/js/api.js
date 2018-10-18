/************************************************************************************
FastHttpApi javascript api Generator Copyright © henryfan 2018 email:henryfan@msn.com
https://github.com/IKende/FastHttpApi
**************************************************************************************/

var $Talk$url = '/talk';
function $Talk(name, message, useHttp) {
    return api($Talk$url, { name: name, message: message }, useHttp).sync();
}
function $Talk$async(name, message, useHttp) {
    return api($Talk$url, { name: name, message: message }, useHttp);
}
