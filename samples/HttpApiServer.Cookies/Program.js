/************************************************************************************
FastHttpApi javascript api Generator Copyright © henryfan 2018 email:henryfan@msn.com
https://github.com/IKende/FastHttpApi
**************************************************************************************/




var HomesetCookieUrl='/setcookie';
/**
* 'var result= await HomesetCookie(params);'
**/
function HomesetCookie(name,value,useHttp)
{
    return api(HomesetCookieUrl,{name:name,value:value},useHttp).sync();
}
/**
* 'HomesetCookieAsync(params).execute(function(result){},useHttp);'
**/
function HomesetCookieAsync(name,value,useHttp)
{
    return api(HomesetCookieUrl,{name:name,value:value},useHttp);
}
var HomegetCookieUrl='/getcookie';
/**
* 'var result= await HomegetCookie(params);'
**/
function HomegetCookie(name,useHttp)
{
    return api(HomegetCookieUrl,{name:name},useHttp).sync();
}
/**
* 'HomegetCookieAsync(params).execute(function(result){},useHttp);'
**/
function HomegetCookieAsync(name,useHttp)
{
    return api(HomegetCookieUrl,{name:name},useHttp);
}
