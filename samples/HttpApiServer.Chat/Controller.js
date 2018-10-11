/************************************************************************************
FastHttpApi javascript api Generator Copyright © henryfan 2018 email:henryfan@msn.com
https://github.com/IKende/FastHttpApi
**************************************************************************************/

var $Onlines$url='/onlines';
///<summary>
///  获取在线人数
/// </summary>
/// <returns>{ID, Name, IPAddress}</returns>
function $Onlines(useHttp)
{
    return api($Onlines$url,{},useHttp).sync();
}
function $Onlines$async(useHttp)
{
    return api($Onlines$url,{},useHttp);
}
var $GetRoomOnlines$url='/getroomonlines';
///<summary>
/// 获取房间在线人数
/// </summary>
/// <param name="roomName">房间名称</param>
/// <returns>{ID, Name, IPAddress}</returns>
function $GetRoomOnlines(roomName,useHttp)
{
    return api($GetRoomOnlines$url,{roomName:roomName},useHttp).sync();
}
function $GetRoomOnlines$async(roomName,useHttp)
{
    return api($GetRoomOnlines$url,{roomName:roomName},useHttp);
}
var $Login$url='/login';
///<summary>
/// 用户登陆
/// </summary>
/// <param name="userName">用户名</param>
/// <returns>true|false</returns>
function $Login(userName,useHttp)
{
    return api($Login$url,{userName:userName},useHttp).sync();
}
function $Login$async(userName,useHttp)
{
    return api($Login$url,{userName:userName},useHttp);
}
var $ListRooms$url='/listrooms';
///<summary>
/// 获取所有房间信息
/// </summary>
/// <returns>{Name,Count}</returns>
function $ListRooms(useHttp)
{
    return api($ListRooms$url,{},useHttp).sync();
}
function $ListRooms$async(useHttp)
{
    return api($ListRooms$url,{},useHttp);
}
var $CloseSession$url='/closesession';
///<summary>
/// 关闭连接
/// </summary>
/// <param name="sessions">[id1,id2,id3]</param>
function $CloseSession(body,useHttp)
{
    return api($CloseSession$url,{body:body},useHttp).sync();
}
function $CloseSession$async(body,useHttp)
{
    return api($CloseSession$url,{body:body},useHttp);
}
var $CloseRoom$url='/closeroom';
///<summary>
/// 关闭房间
/// </summary>
/// <param name="roomName">房间名称</param>
function $CloseRoom(roomName,useHttp)
{
    return api($CloseRoom$url,{roomName:roomName},useHttp).sync();
}
function $CloseRoom$async(roomName,useHttp)
{
    return api($CloseRoom$url,{roomName:roomName},useHttp);
}
var $CheckOutRoom$url='/checkoutroom';
///<summary>
/// 退出房间
/// </summary>
/// <param name="roomName">房间名称</param>
/// <returns>{Code:200,Error}</returns>
function $CheckOutRoom(roomName,useHttp)
{
    return api($CheckOutRoom$url,{roomName:roomName},useHttp).sync();
}
function $CheckOutRoom$async(roomName,useHttp)
{
    return api($CheckOutRoom$url,{roomName:roomName},useHttp);
}
var $CheckInRoom$url='/checkinroom';
///<summary>
/// 进入房间
/// </summary>
/// <param name="roomName">进入房间</param>
/// <returns>{Code:200,Error}</returns>
function $CheckInRoom(roomName,useHttp)
{
    return api($CheckInRoom$url,{roomName:roomName},useHttp).sync();
}
function $CheckInRoom$async(roomName,useHttp)
{
    return api($CheckInRoom$url,{roomName:roomName},useHttp);
}
var $CreateRoom$url='/createroom';
///<summary>
/// 创建记房间
/// </summary>
/// <param name="roomName">房间名称</param>
/// <returns>{Code:200,Error}</returns>
function $CreateRoom(roomName,useHttp)
{
    return api($CreateRoom$url,{roomName:roomName},useHttp).sync();
}
function $CreateRoom$async(roomName,useHttp)
{
    return api($CreateRoom$url,{roomName:roomName},useHttp);
}
var $SendMessage$url='/sendmessage';
///<summary>
/// 发送消息
/// </summary>
/// <param name="message">消息内容</param>
function $SendMessage(message,useHttp)
{
    return api($SendMessage$url,{message:message},useHttp).sync();
}
function $SendMessage$async(message,useHttp)
{
    return api($SendMessage$url,{message:message},useHttp);
}
