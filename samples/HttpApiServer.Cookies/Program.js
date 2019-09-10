function Home()
{
this.url_setCookie='/setCookie';
this.url_getCookie='/getCookie';
}
/**
* 'setCookie(params).execute(function(result){});'
* 'FastHttpApi javascript api Generator Copyright © henryfan 2018 email:henryfan@msn.com
* 'https://github.com/IKende/FastHttpApi
**/
Home.prototype.setCookie= function(name,value,useHttp)
{
    return api(this.url_setCookie,{name:name,value:value},useHttp);
}
/**
* 'getCookie(params).execute(function(result){});'
* 'FastHttpApi javascript api Generator Copyright © henryfan 2018 email:henryfan@msn.com
* 'https://github.com/IKende/FastHttpApi
**/
Home.prototype.getCookie= function(name,useHttp)
{
    return api(this.url_getCookie,{name:name},useHttp);
}
