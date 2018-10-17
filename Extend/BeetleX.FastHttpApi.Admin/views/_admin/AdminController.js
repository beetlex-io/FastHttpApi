/************************************************************************************
FastHttpApi javascript api Generator Copyright © henryfan 2018 email:henryfan@msn.com
https://github.com/IKende/FastHttpApi
**************************************************************************************/

var $_admin$ListApi$url = '/_admin/listapi';
function $_admin$ListApi(useHttp) {
    return api($_admin$ListApi$url, {}, useHttp).sync();
}
function $_admin$ListApi$async(useHttp) {
    return api($_admin$ListApi$url, {}, useHttp);
}
var $_admin$GetKey$url = '/_admin/getkey';
function $_admin$GetKey(useHttp) {
    return api($_admin$GetKey$url, {}, useHttp).sync();
}
function $_admin$GetKey$async(useHttp) {
    return api($_admin$GetKey$url, {}, useHttp);
}
var $_admin$GetSettingInfo$url = '/_admin/getsettinginfo';
function $_admin$GetSettingInfo(useHttp) {
    return api($_admin$GetSettingInfo$url, {}, useHttp).sync();
}
function $_admin$GetSettingInfo$async(useHttp) {
    return api($_admin$GetSettingInfo$url, {}, useHttp);
}
var $_admin$Setting$url = '/_admin/setting';
function $_admin$Setting(body, useHttp) {
    return api($_admin$Setting$url, { body: body }, useHttp).sync();
}
function $_admin$Setting$async(body, useHttp) {
    return api($_admin$Setting$url, { body: body }, useHttp);
}
var $_admin$LogConnect$url = '/_admin/logconnect';
function $_admin$LogConnect(useHttp) {
    return api($_admin$LogConnect$url, {}, useHttp).sync();
}
function $_admin$LogConnect$async(useHttp) {
    return api($_admin$LogConnect$url, {}, useHttp);
}
var $_admin$LogDisConnect$url = '/_admin/logdisconnect';
function $_admin$LogDisConnect(useHttp) {
    return api($_admin$LogDisConnect$url, {}, useHttp).sync();
}
function $_admin$LogDisConnect$async(useHttp) {
    return api($_admin$LogDisConnect$url, {}, useHttp);
}
var $_admin$GetApiScript$url = '/_admin/getapiscript';
function $_admin$GetApiScript(useHttp) {
    return api($_admin$GetApiScript$url, {}, useHttp).sync();
}
function $_admin$GetApiScript$async(useHttp) {
    return api($_admin$GetApiScript$url, {}, useHttp);
}
var $_admin$CloseSession$url = '/_admin/closesession';
function $_admin$CloseSession(body, useHttp) {
    return api($_admin$CloseSession$url, { body: body }, useHttp).sync();
}
function $_admin$CloseSession$async(body, useHttp) {
    return api($_admin$CloseSession$url, { body: body }, useHttp);
}
var $_admin$GetServerInfo$url = '/_admin/getserverinfo';
function $_admin$GetServerInfo(useHttp) {
    return api($_admin$GetServerInfo$url, {}, useHttp).sync();
}
function $_admin$GetServerInfo$async(useHttp) {
    return api($_admin$GetServerInfo$url, {}, useHttp);
}
var $_admin$ListConnection$url = '/_admin/listconnection';
function $_admin$ListConnection(index, useHttp) {
    return api($_admin$ListConnection$url, { index: index }, useHttp).sync();
}
function $_admin$ListConnection$async(index, useHttp) {
    return api($_admin$ListConnection$url, { index: index }, useHttp);
}
var $_admin$Login$url = '/_admin/login';
function $_admin$Login(name, pwd, useHttp) {
    return api($_admin$Login$url, { name: name, pwd: pwd }, useHttp).sync();
}
function $_admin$Login$async(name, pwd, useHttp) {
    return api($_admin$Login$url, { name: name, pwd: pwd }, useHttp);
}
