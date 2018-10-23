/************************************************************************************
FastHttpApi javascript api Generator Copyright © henryfan 2018 email:henryfan@msn.com
https://github.com/IKende/FastHttpApi
**************************************************************************************/




/** 
获取在线人数 url 
**/
var ChatOnlinesUrl = '/onlines';
/** 获取在线人数 'var result=await ChatOnlines(params)'
/* @param useHttp only http request
/* @return {ID, Name, IPAddress}
**/
function ChatOnlines(useHttp) {
    return api(ChatOnlinesUrl, {}, useHttp).sync();
}
/** 获取在线人数 'ChatOnlinesAsync(params).execute(function(result){},useHttp)'
/* @param useHttp only http request
/* @return {ID, Name, IPAddress}
**/
function ChatOnlinesAsync(useHttp) {
    return api(ChatOnlinesUrl, {}, useHttp);
}
/** 
获取房间在线人数 url 
**/
var ChatGetRoomOnlinesUrl = '/getroomonlines';
/** 获取房间在线人数 'var result=await ChatGetRoomOnlines(params)'
/* @param roomName 房间名称
/* @param useHttp only http request
/* @return {ID, Name, IPAddress}
**/
function ChatGetRoomOnlines(roomName, useHttp) {
    return api(ChatGetRoomOnlinesUrl, { roomName: roomName }, useHttp).sync();
}
/** 获取房间在线人数 'ChatGetRoomOnlinesAsync(params).execute(function(result){},useHttp)'
/* @param roomName 房间名称
/* @param useHttp only http request
/* @return {ID, Name, IPAddress}
**/
function ChatGetRoomOnlinesAsync(roomName, useHttp) {
    return api(ChatGetRoomOnlinesUrl, { roomName: roomName }, useHttp);
}
/** 
用户登陆 url 
**/
var ChatLoginUrl = '/login';
/** 用户登陆 'var result=await ChatLogin(params)'
/* @param userName 用户名
/* @param useHttp only http request
/* @return true|false
**/
function ChatLogin(userName, useHttp) {
    return api(ChatLoginUrl, { userName: userName }, useHttp).sync();
}
/** 用户登陆 'ChatLoginAsync(params).execute(function(result){},useHttp)'
/* @param userName 用户名
/* @param useHttp only http request
/* @return true|false
**/
function ChatLoginAsync(userName, useHttp) {
    return api(ChatLoginUrl, { userName: userName }, useHttp);
}
/** 
获取所有房间信息 url 
**/
var ChatListRoomsUrl = '/listrooms';
/** 获取所有房间信息 'var result=await ChatListRooms(params)'
/* @param useHttp only http request
/* @return {Name,Count}
**/
function ChatListRooms(useHttp) {
    return api(ChatListRoomsUrl, {}, useHttp).sync();
}
/** 获取所有房间信息 'ChatListRoomsAsync(params).execute(function(result){},useHttp)'
/* @param useHttp only http request
/* @return {Name,Count}
**/
function ChatListRoomsAsync(useHttp) {
    return api(ChatListRoomsUrl, {}, useHttp);
}
/** 
关闭连接 url 
**/
var ChatCloseSessionUrl = '/closesession';
/** 关闭连接 'var result=await ChatCloseSession(params)'
/* @param sessions [id1,id2,id3]
/* @param useHttp only http request
/* @return 
**/
function ChatCloseSession(sessions, useHttp) {
    return api(ChatCloseSessionUrl, { sessions: sessions }, useHttp, true).sync();
}
/** 关闭连接 'ChatCloseSessionAsync(params).execute(function(result){},useHttp)'
/* @param sessions [id1,id2,id3]
/* @param useHttp only http request
/* @return 
**/
function ChatCloseSessionAsync(sessions, useHttp) {
    return api(ChatCloseSessionUrl, { sessions: sessions }, useHttp, true);
}
/** 
关闭房间 url 
**/
var ChatCloseRoomUrl = '/closeroom';
/** 关闭房间 'var result=await ChatCloseRoom(params)'
/* @param roomName 房间名称
/* @param useHttp only http request
/* @return 
**/
function ChatCloseRoom(roomName, useHttp) {
    return api(ChatCloseRoomUrl, { roomName: roomName }, useHttp).sync();
}
/** 关闭房间 'ChatCloseRoomAsync(params).execute(function(result){},useHttp)'
/* @param roomName 房间名称
/* @param useHttp only http request
/* @return 
**/
function ChatCloseRoomAsync(roomName, useHttp) {
    return api(ChatCloseRoomUrl, { roomName: roomName }, useHttp);
}
/** 
退出房间 url 
**/
var ChatCheckOutRoomUrl = '/checkoutroom';
/** 退出房间 'var result=await ChatCheckOutRoom(params)'
/* @param roomName 房间名称
/* @param useHttp only http request
/* @return {Code:200,Error}
**/
function ChatCheckOutRoom(roomName, useHttp) {
    return api(ChatCheckOutRoomUrl, { roomName: roomName }, useHttp).sync();
}
/** 退出房间 'ChatCheckOutRoomAsync(params).execute(function(result){},useHttp)'
/* @param roomName 房间名称
/* @param useHttp only http request
/* @return {Code:200,Error}
**/
function ChatCheckOutRoomAsync(roomName, useHttp) {
    return api(ChatCheckOutRoomUrl, { roomName: roomName }, useHttp);
}
/** 
进入房间 url 
**/
var ChatCheckInRoomUrl = '/checkinroom';
/** 进入房间 'var result=await ChatCheckInRoom(params)'
/* @param roomName 进入房间
/* @param useHttp only http request
/* @return {Code:200,Error}
**/
function ChatCheckInRoom(roomName, useHttp) {
    return api(ChatCheckInRoomUrl, { roomName: roomName }, useHttp).sync();
}
/** 进入房间 'ChatCheckInRoomAsync(params).execute(function(result){},useHttp)'
/* @param roomName 进入房间
/* @param useHttp only http request
/* @return {Code:200,Error}
**/
function ChatCheckInRoomAsync(roomName, useHttp) {
    return api(ChatCheckInRoomUrl, { roomName: roomName }, useHttp);
}
/** 
创建记房间 url 
**/
var ChatCreateRoomUrl = '/createroom';
/** 创建记房间 'var result=await ChatCreateRoom(params)'
/* @param roomName 房间名称
/* @param useHttp only http request
/* @return {Code:200,Error}
**/
function ChatCreateRoom(roomName, useHttp) {
    return api(ChatCreateRoomUrl, { roomName: roomName }, useHttp).sync();
}
/** 创建记房间 'ChatCreateRoomAsync(params).execute(function(result){},useHttp)'
/* @param roomName 房间名称
/* @param useHttp only http request
/* @return {Code:200,Error}
**/
function ChatCreateRoomAsync(roomName, useHttp) {
    return api(ChatCreateRoomUrl, { roomName: roomName }, useHttp);
}
/** 
发送消息 url 
**/
var ChatSendMessageUrl = '/sendmessage';
/** 发送消息 'var result=await ChatSendMessage(params)'
/* @param message 消息内容
/* @param useHttp only http request
/* @return 
**/
function ChatSendMessage(message, useHttp) {
    return api(ChatSendMessageUrl, { message: message }, useHttp).sync();
}
/** 发送消息 'ChatSendMessageAsync(params).execute(function(result){},useHttp)'
/* @param message 消息内容
/* @param useHttp only http request
/* @return 
**/
function ChatSendMessageAsync(message, useHttp) {
    return api(ChatSendMessageUrl, { message: message }, useHttp);
}
