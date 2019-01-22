/************************************************************************************
FastHttpApi javascript api Generator Copyright © henryfan 2018 email:henryfan@msn.com
https://github.com/IKende/FastHttpApi
**************************************************************************************/




var ChatLoginUrl='/Login';
/**
* 'ChatLogin(params).execute(function(result){});'
**/
function ChatLogin(nickName,useHttp)
{
    return api(ChatLoginUrl,{nickName:nickName},useHttp);
}
var ChatListOnlinesUrl='/ListOnlines';
/**
* 'ChatListOnlines(params).execute(function(result){});'
**/
function ChatListOnlines(useHttp)
{
    return api(ChatListOnlinesUrl,{},useHttp);
}
var ChatTalkUrl='/Talk';
/**
* 'ChatTalk(params).execute(function(result){});'
**/
function ChatTalk(nickName,message,useHttp)
{
    return api(ChatTalkUrl,{nickName:nickName,message:message},useHttp);
}
