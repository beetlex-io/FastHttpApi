function Home()
{
this.url_Hello='/Hello';
}
/** 
* 'Hello Word url 
* 'FastHttpApi javascript api Generator Copyright © henryfan 2018 email:henryfan@msn.com
* 'https://github.com/IKende/FastHttpApi
**/
/** Hello Word 'Hello(params).execute(function(result){},useHttp)'
/* @param name string:  you name
/* @param useHttp only http request
/* @return string
**/
Home.prototype.Hello= function(name,useHttp)
{
    return api(this.url_Hello,{name:name},useHttp);
}
