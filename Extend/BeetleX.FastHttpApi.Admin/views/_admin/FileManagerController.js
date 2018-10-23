/************************************************************************************
FastHttpApi javascript api Generator Copyright © henryfan 2018 email:henryfan@msn.com
https://github.com/IKende/FastHttpApi
**************************************************************************************/




var _FileManagerCreateFolderUrl = '/_admin/files/createfolder';
/**
* 'var result= await _FileManagerCreateFolder(params);'
**/
function _FileManagerCreateFolder(folder, name, useHttp) {
    return api(_FileManagerCreateFolderUrl, { folder: folder, name: name }, useHttp).sync();
}
/**
* '_FileManagerCreateFolderAsync(params).execute(function(result){},useHttp);'
**/
function _FileManagerCreateFolderAsync(folder, name, useHttp) {
    return api(_FileManagerCreateFolderUrl, { folder: folder, name: name }, useHttp);
}
var _FileManagerUploadFileUrl = '/_admin/files/uploadfile';
/**
* 'var result= await _FileManagerUploadFile(params);'
**/
function _FileManagerUploadFile(folder, info, useHttp) {
    return api(_FileManagerUploadFileUrl, { folder: folder, info: info }, useHttp, true).sync();
}
/**
* '_FileManagerUploadFileAsync(params).execute(function(result){},useHttp);'
**/
function _FileManagerUploadFileAsync(folder, info, useHttp) {
    return api(_FileManagerUploadFileUrl, { folder: folder, info: info }, useHttp, true);
}
var _FileManagerDeleteResourceUrl = '/_admin/files/deleteresource';
/**
* 'var result= await _FileManagerDeleteResource(params);'
**/
function _FileManagerDeleteResource(folder, name, file, useHttp) {
    return api(_FileManagerDeleteResourceUrl, { folder: folder, name: name, file: file }, useHttp).sync();
}
/**
* '_FileManagerDeleteResourceAsync(params).execute(function(result){},useHttp);'
**/
function _FileManagerDeleteResourceAsync(folder, name, file, useHttp) {
    return api(_FileManagerDeleteResourceUrl, { folder: folder, name: name, file: file }, useHttp);
}
var _FileManagerListUrl = '/_admin/files/list';
/**
* 'var result= await _FileManagerList(params);'
**/
function _FileManagerList(folder, useHttp) {
    return api(_FileManagerListUrl, { folder: folder }, useHttp).sync();
}
/**
* '_FileManagerListAsync(params).execute(function(result){},useHttp);'
**/
function _FileManagerListAsync(folder, useHttp) {
    return api(_FileManagerListUrl, { folder: folder }, useHttp);
}
