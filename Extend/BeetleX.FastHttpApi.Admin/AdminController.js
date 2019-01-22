/************************************************************************************
FastHttpApi javascript api Generator Copyright © henryfan 2018 email:henryfan@msn.com
https://github.com/IKende/FastHttpApi
**************************************************************************************/




var _AdminListApiUrl='/_admin/ListApi';
/**
* '_AdminListApi(params).execute(function(result){});'
**/
function _AdminListApi(useHttp)
{
    return api(_AdminListApiUrl,{},useHttp);
}
var _AdminGetKeyUrl='/_admin/GetKey';
/**
* '_AdminGetKey(params).execute(function(result){});'
**/
function _AdminGetKey(useHttp)
{
    return api(_AdminGetKeyUrl,{},useHttp);
}
var _AdminGetSettingInfoUrl='/_admin/GetSettingInfo';
/**
* '_AdminGetSettingInfo(params).execute(function(result){});'
**/
function _AdminGetSettingInfo(useHttp)
{
    return api(_AdminGetSettingInfoUrl,{},useHttp);
}
var _AdminSettingUrl='/_admin/Setting';
/**
* '_AdminSetting(params).execute(function(result){});'
**/
function _AdminSetting(setting,useHttp)
{
    return api(_AdminSettingUrl,{setting:setting},useHttp,true);
}
var _AdminLogConnectUrl='/_admin/LogConnect';
/**
* '_AdminLogConnect(params).execute(function(result){});'
**/
function _AdminLogConnect(useHttp)
{
    return api(_AdminLogConnectUrl,{},useHttp);
}
var _AdminLogDisConnectUrl='/_admin/LogDisConnect';
/**
* '_AdminLogDisConnect(params).execute(function(result){});'
**/
function _AdminLogDisConnect(useHttp)
{
    return api(_AdminLogDisConnectUrl,{},useHttp);
}
var _AdminCloseSessionUrl='/_admin/CloseSession';
/**
* '_AdminCloseSession(params).execute(function(result){});'
**/
function _AdminCloseSession(items,useHttp)
{
    return api(_AdminCloseSessionUrl,{items:items},useHttp,true);
}
var _AdminGetServerInfoUrl='/_admin/GetServerInfo';
/**
* '_AdminGetServerInfo(params).execute(function(result){});'
**/
function _AdminGetServerInfo(useHttp)
{
    return api(_AdminGetServerInfoUrl,{},useHttp);
}
var _AdminListConnectionUrl='/_admin/ListConnection';
/**
* '_AdminListConnection(params).execute(function(result){});'
**/
function _AdminListConnection(index,useHttp)
{
    return api(_AdminListConnectionUrl,{index:index},useHttp);
}
var _AdminLoginUrl='/_admin/Login';
/**
* '_AdminLogin(params).execute(function(result){});'
**/
function _AdminLogin(name,pwd,useHttp)
{
    return api(_AdminLoginUrl,{name:name,pwd:pwd},useHttp);
}
