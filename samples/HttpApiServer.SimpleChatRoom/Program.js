function Chat()
{
this.url_Login='/Login';
this.url_ListOnlines='/ListOnlines';
this.url_Talk='/Talk';
}
/**
* 'Login(params).execute(function(result){});'
* 'FastHttpApi javascript api Generator Copyright © henryfan 2018 email:henryfan@msn.com
* 'https://github.com/IKende/FastHttpApi
**/
Chat.prototype.Login= function(nickName,useHttp)
{
    return api(this.url_Login,{nickName:nickName},useHttp);
}
/**
* 'ListOnlines(params).execute(function(result){});'
* 'FastHttpApi javascript api Generator Copyright © henryfan 2018 email:henryfan@msn.com
* 'https://github.com/IKende/FastHttpApi
**/
Chat.prototype.ListOnlines= function(useHttp)
{
    return api(this.url_ListOnlines,{},useHttp);
}
/**
* 'Talk(params).execute(function(result){});'
* 'FastHttpApi javascript api Generator Copyright © henryfan 2018 email:henryfan@msn.com
* 'https://github.com/IKende/FastHttpApi
**/
Chat.prototype.Talk= function(nickName,message,useHttp)
{
    return api(this.url_Talk,{nickName:nickName,message:message},useHttp);
}
