/************************************************************************************
FastHttpApi javascript api Generator Copyright © henryfan 2018 email:henryfan@msn.com
https://github.com/IKende/FastHttpApi
**************************************************************************************/




var ChatLoginUrl = '/login';
/**
* 'var result= await ChatLogin(params);'
**/
function ChatLogin(nickName, useHttp) {
    return api(ChatLoginUrl, { nickName: nickName }, useHttp).sync();
}
/**
* 'ChatLoginAsync(params).execute(function(result){},useHttp);'
**/
function ChatLoginAsync(nickName, useHttp) {
    return api(ChatLoginUrl, { nickName: nickName }, useHttp);
}
var ChatListOnlinesUrl = '/listonlines';
/**
* 'var result= await ChatListOnlines(params);'
**/
function ChatListOnlines(useHttp) {
    return api(ChatListOnlinesUrl, {}, useHttp).sync();
}
/**
* 'ChatListOnlinesAsync(params).execute(function(result){},useHttp);'
**/
function ChatListOnlinesAsync(useHttp) {
    return api(ChatListOnlinesUrl, {}, useHttp);
}
var ChatTalkUrl = '/talk';
/**
* 'var result= await ChatTalk(params);'
**/
function ChatTalk(nickName, message, useHttp) {
    return api(ChatTalkUrl, { nickName: nickName, message: message }, useHttp).sync();
}
/**
* 'ChatTalkAsync(params).execute(function(result){},useHttp);'
**/
function ChatTalkAsync(nickName, message, useHttp) {
    return api(ChatTalkUrl, { nickName: nickName, message: message }, useHttp);
}
