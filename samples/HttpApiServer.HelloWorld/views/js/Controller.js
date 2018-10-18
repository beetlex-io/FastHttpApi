/************************************************************************************
FastHttpApi javascript api Generator Copyright © henryfan 2018 email:henryfan@msn.com
https://github.com/IKende/FastHttpApi
**************************************************************************************/

var $Hello$url = '/hello';
function $Hello(name, useHttp) {
    return api($Hello$url, { name: name }, useHttp).sync();
}
function $Hello$async(name, useHttp) {
    return api($Hello$url, { name: name }, useHttp);
}
