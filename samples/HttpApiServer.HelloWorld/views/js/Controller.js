/************************************************************************************
FastHttpApi javascript api Generator Copyright © henryfan 2018 email:henryfan@msn.com
https://github.com/IKende/FastHttpApi
**************************************************************************************/




/** 
Hello Word url 
**/
var HomeHelloUrl = '/hello';
/** Hello Word 'var result=await HomeHello(params)'
/* @param name string:  you name
/* @param useHttp only http request
/* @return string
**/
function HomeHello(name, useHttp) {
    return api(HomeHelloUrl, { name: name }, useHttp).sync();
}
/** Hello Word 'HomeHelloAsync(params).execute(function(result){},useHttp)'
/* @param name string:  you name
/* @param useHttp only http request
/* @return string
**/
function HomeHelloAsync(name, useHttp) {
    return api(HomeHelloUrl, { name: name }, useHttp);
}
