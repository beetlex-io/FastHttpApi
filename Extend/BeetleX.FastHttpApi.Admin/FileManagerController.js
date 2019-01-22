/************************************************************************************
FastHttpApi javascript api Generator Copyright © henryfan 2018 email:henryfan@msn.com
https://github.com/IKende/FastHttpApi
**************************************************************************************/




var _FileManagerCreateFolderUrl='/_admin/files/CreateFolder';
/**
* '_FileManagerCreateFolder(params).execute(function(result){});'
**/
function _FileManagerCreateFolder(folder,name,useHttp)
{
    return api(_FileManagerCreateFolderUrl,{folder:folder,name:name},useHttp);
}
var _FileManagerUploadFileUrl='/_admin/files/UploadFile';
/**
* '_FileManagerUploadFile(params).execute(function(result){});'
**/
function _FileManagerUploadFile(folder,info,useHttp)
{
    return api(_FileManagerUploadFileUrl,{folder:folder,info:info},useHttp,true);
}
var _FileManagerDeleteResourceUrl='/_admin/files/DeleteResource';
/**
* '_FileManagerDeleteResource(params).execute(function(result){});'
**/
function _FileManagerDeleteResource(folder,name,file,useHttp)
{
    return api(_FileManagerDeleteResourceUrl,{folder:folder,name:name,file:file},useHttp);
}
var _FileManagerListUrl='/_admin/files/List';
/**
* '_FileManagerList(params).execute(function(result){});'
**/
function _FileManagerList(folder,useHttp)
{
    return api(_FileManagerListUrl,{folder:folder},useHttp);
}
