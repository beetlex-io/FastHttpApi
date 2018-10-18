/************************************************************************************
FastHttpApi javascript api Generator Copyright © henryfan 2018 email:henryfan@msn.com
https://github.com/IKende/FastHttpApi
**************************************************************************************/

var $setCookie$url='/setcookie';
function $setCookie(name,value,useHttp)
{
    return api($setCookie$url,{name:name,value:value},useHttp).sync();
}
function $setCookie$async(name,value,useHttp)
{
    return api($setCookie$url,{name:name,value:value},useHttp);
}
var $getCookie$url='/getcookie';
function $getCookie(name,useHttp)
{
    return api($getCookie$url,{name:name},useHttp).sync();
}
function $getCookie$async(name,useHttp)
{
    return api($getCookie$url,{name:name},useHttp);
}
