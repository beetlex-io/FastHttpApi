/************************************************************************************
FastHttpApi javascript api Generator Copyright © henryfan 2018 email:henryfan@msn.com
https://github.com/IKende/FastHttpApi
**************************************************************************************/




var _AdminListApiUrl='/_admin/ListApi';
/**
* 'var result= await _AdminListApi(params);'
**/
function _AdminListApi(useHttp)
{
    return api(_AdminListApiUrl,{},useHttp).sync();
}
/**
* '_AdminListApiAsync(params).execute(function(result){},useHttp);'
**/
function _AdminListApiAsync(useHttp)
{
    return api(_AdminListApiUrl,{},useHttp);
}
var _AdminGetKeyUrl='/_admin/GetKey';
/**
* 'var result= await _AdminGetKey(params);'
**/
function _AdminGetKey(useHttp)
{
    return api(_AdminGetKeyUrl,{},useHttp).sync();
}
/**
* '_AdminGetKeyAsync(params).execute(function(result){},useHttp);'
**/
function _AdminGetKeyAsync(useHttp)
{
    return api(_AdminGetKeyUrl,{},useHttp);
}
var _AdminGetSettingInfoUrl='/_admin/GetSettingInfo';
/**
* 'var result= await _AdminGetSettingInfo(params);'
**/
function _AdminGetSettingInfo(useHttp)
{
    return api(_AdminGetSettingInfoUrl,{},useHttp).sync();
}
/**
* '_AdminGetSettingInfoAsync(params).execute(function(result){},useHttp);'
**/
function _AdminGetSettingInfoAsync(useHttp)
{
    return api(_AdminGetSettingInfoUrl,{},useHttp);
}
var _AdminSettingUrl='/_admin/Setting';
/**
* 'var result= await _AdminSetting(params);'
**/
function _AdminSetting(setting,useHttp)
{
    return api(_AdminSettingUrl,{setting:setting},useHttp,true).sync();
}
/**
* '_AdminSettingAsync(params).execute(function(result){},useHttp);'
**/
function _AdminSettingAsync(setting,useHttp)
{
    return api(_AdminSettingUrl,{setting:setting},useHttp,true);
}
var _AdminLogConnectUrl='/_admin/LogConnect';
/**
* 'var result= await _AdminLogConnect(params);'
**/
function _AdminLogConnect(useHttp)
{
    return api(_AdminLogConnectUrl,{},useHttp).sync();
}
/**
* '_AdminLogConnectAsync(params).execute(function(result){},useHttp);'
**/
function _AdminLogConnectAsync(useHttp)
{
    return api(_AdminLogConnectUrl,{},useHttp);
}
var _AdminLogDisConnectUrl='/_admin/LogDisConnect';
/**
* 'var result= await _AdminLogDisConnect(params);'
**/
function _AdminLogDisConnect(useHttp)
{
    return api(_AdminLogDisConnectUrl,{},useHttp).sync();
}
/**
* '_AdminLogDisConnectAsync(params).execute(function(result){},useHttp);'
**/
function _AdminLogDisConnectAsync(useHttp)
{
    return api(_AdminLogDisConnectUrl,{},useHttp);
}
var _AdminCloseSessionUrl='/_admin/CloseSession';
/**
* 'var result= await _AdminCloseSession(params);'
**/
function _AdminCloseSession(items,useHttp)
{
    return api(_AdminCloseSessionUrl,{items:items},useHttp,true).sync();
}
/**
* '_AdminCloseSessionAsync(params).execute(function(result){},useHttp);'
**/
function _AdminCloseSessionAsync(items,useHttp)
{
    return api(_AdminCloseSessionUrl,{items:items},useHttp,true);
}
var _AdminGetServerInfoUrl='/_admin/GetServerInfo';
/**
* 'var result= await _AdminGetServerInfo(params);'
**/
function _AdminGetServerInfo(useHttp)
{
    return api(_AdminGetServerInfoUrl,{},useHttp).sync();
}
/**
* '_AdminGetServerInfoAsync(params).execute(function(result){},useHttp);'
**/
function _AdminGetServerInfoAsync(useHttp)
{
    return api(_AdminGetServerInfoUrl,{},useHttp);
}
var _AdminListConnectionUrl='/_admin/ListConnection';
/**
* 'var result= await _AdminListConnection(params);'
**/
function _AdminListConnection(index,useHttp)
{
    return api(_AdminListConnectionUrl,{index:index},useHttp).sync();
}
/**
* '_AdminListConnectionAsync(params).execute(function(result){},useHttp);'
**/
function _AdminListConnectionAsync(index,useHttp)
{
    return api(_AdminListConnectionUrl,{index:index},useHttp);
}
var _AdminLoginUrl='/_admin/Login';
/**
* 'var result= await _AdminLogin(params);'
**/
function _AdminLogin(name,pwd,useHttp)
{
    return api(_AdminLoginUrl,{name:name,pwd:pwd},useHttp).sync();
}
/**
* '_AdminLoginAsync(params).execute(function(result){},useHttp);'
**/
function _AdminLoginAsync(name,pwd,useHttp)
{
    return api(_AdminLoginUrl,{name:name,pwd:pwd},useHttp);
}
