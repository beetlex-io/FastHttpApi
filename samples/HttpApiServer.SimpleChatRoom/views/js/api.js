/************************************************************************************
FastHttpApi javascript api Generator Copyright © henryfan 2018 email:henryfan@msn.com
https://github.com/IKende/FastHttpApi
**************************************************************************************/

var $Login$url = '/login';
function $Login(nickName, useHttp) {
    return api($Login$url, { nickName: nickName }, useHttp).sync();
}
function $Login$async(nickName, useHttp) {
    return api($Login$url, { nickName: nickName }, useHttp);
}
var $ListOnlines$url = '/listonlines';
function $ListOnlines(useHttp) {
    return api($ListOnlines$url, {}, useHttp).sync();
}
function $ListOnlines$async(useHttp) {
    return api($ListOnlines$url, {}, useHttp);
}
var $Talk$url = '/talk';
function $Talk(nickName, message, useHttp) {
    return api($Talk$url, { nickName: nickName, message: message }, useHttp).sync();
}
function $Talk$async(nickName, message, useHttp) {
    return api($Talk$url, { nickName: nickName, message: message }, useHttp);
}
