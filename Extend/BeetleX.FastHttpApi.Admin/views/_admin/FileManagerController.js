/************************************************************************************
FastHttpApi javascript api Generator Copyright © henryfan 2018 email:henryfan@msn.com
https://github.com/IKende/FastHttpApi
**************************************************************************************/

var $_admin$files$CreateFolder$url = '/_admin/files/createfolder';
function $_admin$files$CreateFolder(folder, name, useHttp) {
    return api($_admin$files$CreateFolder$url, { folder: folder, name: name }, useHttp).sync();
}
function $_admin$files$CreateFolder$async(folder, name, useHttp) {
    return api($_admin$files$CreateFolder$url, { folder: folder, name: name }, useHttp);
}
var $_admin$files$UploadFile$url = '/_admin/files/uploadfile';
function $_admin$files$UploadFile(folder, body, useHttp) {
    return api($_admin$files$UploadFile$url, { folder: folder, body: body }, useHttp).sync();
}
function $_admin$files$UploadFile$async(folder, body, useHttp) {
    return api($_admin$files$UploadFile$url, { folder: folder, body: body }, useHttp);
}
var $_admin$files$DeleteResource$url = '/_admin/files/deleteresource';
function $_admin$files$DeleteResource(folder, name, file, useHttp) {
    return api($_admin$files$DeleteResource$url, { folder: folder, name: name, file: file }, useHttp).sync();
}
function $_admin$files$DeleteResource$async(folder, name, file, useHttp) {
    return api($_admin$files$DeleteResource$url, { folder: folder, name: name, file: file }, useHttp);
}
var $_admin$files$List$url = '/_admin/files/list';
function $_admin$files$List(folder, useHttp) {
    return api($_admin$files$List$url, { folder: folder }, useHttp).sync();
}
function $_admin$files$List$async(folder, useHttp) {
    return api($_admin$files$List$url, { folder: folder }, useHttp);
}
