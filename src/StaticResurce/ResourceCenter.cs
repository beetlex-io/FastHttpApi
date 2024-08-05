using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeetleX.FastHttpApi.StaticResurce
{
    public class ResourceCenter
    {
        public ResourceCenter(HttpApiServer server)
        {
            Server = server;
            Path = Server.Options.StaticResourcePath;
            if (Path[Path.Length - 1] != System.IO.Path.DirectorySeparatorChar)
            {
                Path += System.IO.Path.DirectorySeparatorChar;
            }

            foreach (string item in server.Options.StaticResurceType.ToLower().Split(';'))
            {
                FileContentType fct = new FileContentType(item);
                mExts[fct.Ext] = fct;
            }
            mDefaultPages.AddRange(Server.Options.DefaultPage.Split(";"));

        }

        public void AddDefaultPage(string file)
        {
            if (!string.IsNullOrEmpty(file))
                mDefaultPages.Add(file);
        }

        public void SetDefaultPages(string files)
        {
            if (files != null)
            {
                Server.Options.DefaultPage = files;
                mDefaultPages.Clear();
                mDefaultPages.AddRange(files.Split(";"));
            }
        }

        public string SetFileExts(string exts)
        {
            if (exts != null)
            {
                foreach (string item in exts.ToLower().Split(';'))
                {
                    if (!mExts.ContainsKey(item))
                    {
                        FileContentType fct = new FileContentType(item);
                        mExts[fct.Ext] = fct;
                    }
                }
                Server.Options.StaticResurceType = string.Join(";", mExts.Keys.ToArray());

            }
            return Server.Options.StaticResurceType;
        }

        private ConcurrentDictionary<string, FileResource> mResources = new ConcurrentDictionary<string, FileResource>(StringComparer.OrdinalIgnoreCase);

        private ConcurrentDictionary<string, FileContentType> mExts = new ConcurrentDictionary<string, FileContentType>(StringComparer.OrdinalIgnoreCase);

        private List<FileSystemWatcher> mFileWatch = new List<FileSystemWatcher>();

        private List<string> mDefaultPages = new List<string>();

        public List<string> DefaultPages => mDefaultPages;

        public ConcurrentDictionary<string, FileContentType> Exts => mExts;

        public HttpApiServer Server { get; private set; }

        public string Path { get; internal set; }

        public event EventHandler<EventFindFileArgs> Find;

        public event EventHandler<EventFileResponseArgs> FileResponse;

        public bool Debug { get; set; }

        private string GetResourceUrl(string name)
        {
            char[] charname = name.ToCharArray();
            List<int> indexs = new List<int>();
            for (int i = 0; i < charname.Length; i++)
            {
                if (charname[i] == '.')
                    indexs.Add(i);
            }
            for (int i = 0; i < indexs.Count - 1; i++)
            {
                charname[indexs[i]] = '/';
            }
            //if (Server.Options.UrlIgnoreCase)
            //    return HttpParse.CharToLower(charname);
            //else
            return new string(charname);
        }

        private void SaveTempFile(System.Reflection.Assembly assembly, string recname, string filename)
        {
            using (System.IO.Stream stream = assembly.GetManifestResourceStream(recname))
            {
                byte[] buffer = HttpParse.GetByteBuffer();
                int length = (int)stream.Length;
                using (System.IO.FileStream fs = System.IO.File.Create(filename))
                {
                    while (length > 0)
                    {
                        int len = stream.Read(buffer, 0, buffer.Length);
                        fs.Write(buffer, 0, len);
                        fs.Flush();
                        length -= len;
                    }
                }
            }
        }

        public void LoadManifestResource(System.Reflection.Assembly assembly)
        {
            string[] files = assembly.GetManifestResourceNames();
            string tmpFolder = "_tempview";
            if (!System.IO.Directory.Exists(tmpFolder))
            {
                Directory.CreateDirectory(tmpFolder);
            }
            foreach (string item in files)
            {
                int offset = item.IndexOf(".views");
                if (offset > 0)
                {
                    string url = GetResourceUrl(item.Substring(offset + 6, item.Length - offset - 6));
                    string ext = System.IO.Path.GetExtension(item).ToLower();
                    ext = ext.Substring(1, ext.Length - 1);
                    if (mExts.ContainsKey(ext))
                    {
                        string urlname = url;
                        string filename = tmpFolder + System.IO.Path.DirectorySeparatorChar + item;
                        SaveTempFile(assembly, item, filename);
                        FileResource fr;
                        bool nogzip = !(Server?.Options.NoGzipFiles.IndexOf(ext) >= 0);
                        bool cachefile = Server?.Options.CacheFiles.IndexOf(ext) >= 0;
                        if (Debug)
                        {
                            fr = new NoCacheResource(filename, urlname);
                            if (nogzip)
                                fr.GZIP = true;
                        }
                        else
                        {
                            if (cachefile)
                            {
                                fr = new FileResource(filename, urlname);
                            }
                            else
                            {
                                fr = new NoCacheResource(filename, urlname);
                                if (nogzip)
                                    fr.GZIP = true;
                            }
                        }
                        mResources[urlname] = fr;
                        fr.Load();
                        if (Server != null && Server.BaseServer != null)
                        {
                            if (Server.BaseServer.EnableLog(EventArgs.LogType.Info))
                                Server.BaseServer.Log(EventArgs.LogType.Info, null, "load static resource " + urlname);
                        }

                    }
                }
            }
        }

        public void Load()
        {

            if (System.IO.Directory.Exists(Path))
            {
                LoadFolder(Path);
                string exts = "js;html;htm;css";
                foreach (string key in mExts.Keys)
                {
                    if (exts.IndexOf(key) >= 0)
                    {
                        FileSystemWatcher fsw = new FileSystemWatcher(Path, "*." + key);
                        fsw.IncludeSubdirectories = true;
                        fsw.Changed += (o, e) =>
                        {
                            Task.Run(async () =>
                            {
                                try
                                {
                                    await Task.Delay(500);
                                    CreateResource(e.FullPath, true);
                                }
                                catch { }
                            });

                        };
                        fsw.EnableRaisingEvents = true;
                        mFileWatch.Add(fsw);
                    }
                }
            }


        }

        private void OnCrossDomain(HttpResponse response)
        {

        }

        private void OutputFileResource(FileContentType fct, FileResource fr, HttpResponse response)
        {
            OnCrossDomain(response);
            if (!Debug)
            {
                string IfNoneMatch = response.Request.IfNoneMatch;
                if (!string.IsNullOrEmpty(IfNoneMatch) && IfNoneMatch == fr.FileMD5)
                {
                    if (Server.EnableLog(EventArgs.LogType.Info))
                        Server.BaseServer.Log(EventArgs.LogType.Info, response.Request.Session, $"HTTP {response.Request.ID} {response.Request.RemoteIPAddress} get {response.Request.Url} source no modify ");
                    if (Server.Options.StaticResurceCacheTime > 0)
                    {
                        response.Header.Add(HeaderTypeFactory.CACHE_CONTROL, "public, max-age=" + Server.Options.StaticResurceCacheTime);
                    }
                    NoModifyResult result = new NoModifyResult();
                    response.Result(result);
                    return;
                }
            }
            if (!Debug)
            {
                if (!string.IsNullOrEmpty(fr.FileMD5))
                {
                    response.Header.Add(HeaderTypeFactory.ETAG, fr.FileMD5);
                    if (Server.Options.StaticResurceCacheTime > 0)
                    {
                        response.Header.Add(HeaderTypeFactory.CACHE_CONTROL, "public, max-age=" + Server.Options.StaticResurceCacheTime);
                    }
                }
            }
            EventFileResponseArgs efra = new EventFileResponseArgs();
            efra.Request = response.Request;
            efra.Response = response;
            efra.Resource = fr;
            FileResponse?.Invoke(this, efra);
            if (!efra.Cancel)
            {
                if (fr.GZIP)
                {
                    SetGZIP(response);
                }
                SetChunked(response);
                if (Server.EnableLog(EventArgs.LogType.Info))
                {
                    Server.BaseServer.Log(EventArgs.LogType.Info, response.Request.Session, $"HTTP {response.Request.ID} {response.Request.RemoteIPAddress} get {response.Request.BaseUrl} response gzip {fr.GZIP}");
                }
                HttpToken token = (HttpToken)response.Session.Tag;
                token.File = fr.CreateFileBlock();
                response.SetContentType(fct.ContentType);
                response.Result(token.File);
            }

        }

        public void OutputFile(FileResult result, HttpRequest request, HttpResponse response)
        {
            OnCrossDomain(response);
            var file = result.File;
            if (file.IndexOf(System.IO.Path.DirectorySeparatorChar) == -1)
            {
                var vfile = MatchVirtuslFolder(file);
                if (vfile == null)
                {
                    file = file.Replace('/', System.IO.Path.DirectorySeparatorChar);
                    if (file[0] != System.IO.Path.DirectorySeparatorChar)
                        file = System.IO.Path.DirectorySeparatorChar + file;
                    var basePath = Server.Options.StaticResourcePath;
                    if (basePath[basePath.Length - 1] == System.IO.Path.DirectorySeparatorChar)
                    {
                        file = basePath + file.Substring(1);
                    }
                    else
                    {
                        file = basePath + file;
                    }
                }
                else
                {
                    file = vfile;
                }
            }
            var resource = new StaticResurce.NoCacheResource(file, "");
            string ext = System.IO.Path.GetExtension(file).Replace(".", "");
            var fileContentType = new StaticResurce.FileContentType(ext);
            if (!string.IsNullOrEmpty(result.ContentType))
            {
                fileContentType.ContentType = result.ContentType;
            }
            resource.GZIP = result.GZip;
            EventFileResponseArgs efra = new EventFileResponseArgs();
            efra.Request = response.Request;
            efra.Response = response;
            efra.Resource = resource;
            efra.ContentType = fileContentType;
            FileResponse?.Invoke(this, efra);
            if (!efra.Cancel)
            {
                if (!System.IO.File.Exists(file))
                {
                    NotFoundResult notFound = new NotFoundResult("{0} file not found", request.Url);
                    response.Result(notFound);
                }
                else
                {
                    efra.Resource.Load();
                    if (efra.Resource.GZIP)
                    {
                        SetGZIP(response);
                    }
                    SetChunked(response);
                    if (Server.EnableLog(EventArgs.LogType.Info))
                    {
                        Server.BaseServer.Log(EventArgs.LogType.Info, request.Session, $"HTTP {response.Request.ID} {response.Request.RemoteIPAddress} get {response.Request.BaseUrl} response gzip {efra.Resource.GZIP}");
                    }
                    HttpToken token = (HttpToken)response.Session.Tag;
                    token.File = efra.Resource.CreateFileBlock();
                    response.SetContentType(efra.ContentType.ContentType);
                    response.Result(token.File);
                }
            }
        }

        public string MatchVirtuslFolder(string file)
        {
            if (Server.Options.Virtuals != null)
                foreach (var item in Server.Options.Virtuals)
                {
                    var result = item.Match(file);
                    if (result != null)
                        return result;
                }
            return null;
        }
        public void ProcessFile(HttpRequest request, HttpResponse response)
        {
            FileContentType fct = null;
            FileResource fr = null;
            string url = request.BaseUrl;
            if (url[url.Length - 1] == '/')
            {
                for (int i = 0; i < mDefaultPages.Count; i++)
                {
                    string defaultpage = url + mDefaultPages[i];
                    string ext = HttpParse.GetBaseUrlExt(defaultpage);
                    if (!mExts.TryGetValue(ext, out fct))
                    {
                        continue;
                    }
                    fr = GetFileResource(defaultpage);
                    if (fr != null)
                    {
                        OutputFileResource(fct, fr, response);
                        return;
                    }
                }
                string result = null;

                for (int i = 0; i < mDefaultPages.Count; i++)
                {
                    string defaultpage = mDefaultPages[i];
                    result = MatchVirtuslFolder(url + defaultpage);
                    if (result != null)
                        break;
                }
                if (result != null)
                {
                    if (result != null)
                    {
                        if (Server.EnableLog(EventArgs.LogType.Info))
                            Server.BaseServer.Log(EventArgs.LogType.Warring, request.Session, $"HTTP {request.ID} {request.RemoteIPAddress} get {request.Url} file from {result}");
                        FileResult fileResult = new FileResult(result);
                        OutputFile(fileResult, request, response);
                        return;
                    }
                }
                if (Server.EnableLog(EventArgs.LogType.Warring))
                    Server.BaseServer.Log(EventArgs.LogType.Warring, request.Session, $"HTTP {request.ID} {request.RemoteIPAddress} get {request.Url} file not found");
                if (!Server.OnHttpRequesNotfound(request, response).Cancel)
                {
                    NotFoundResult notFound = new NotFoundResult("{0} file not found", request.Url);
                    response.Result(notFound);
                }
                return;
            }
            if (ExtSupport(request.Ext))
            {
                url = System.Net.WebUtility.UrlDecode(url);
                fct = mExts[request.Ext];
                fr = GetFileResource(url);
                var resourceData = fr;
                if (!Server.Options.Debug && fr != null)
                {
                    OutputFileResource(fct, fr, response);
                }
                else
                {
                    string file;
                    string fileurl = HttpParse.GetBaseUrl(url);
                    fileurl = System.Net.WebUtility.UrlDecode(fileurl);
                    if (ExistsFile(request, fileurl, out file))
                    {
                        fr = CreateResource(file, false);
                        if (fr != null)
                        {

                            OutputFileResource(fct, fr, response);
                        }
                    }
                    else
                    {
                        fileurl = HttpParse.GetBaseUrl(url);
                        string result = null;
                        result = MatchVirtuslFolder(fileurl);
                        if (result != null)
                        {
                            if (Server.EnableLog(EventArgs.LogType.Info))
                                Server.BaseServer.Log(EventArgs.LogType.Warring, request.Session, $"HTTP {request.ID} {request.RemoteIPAddress} get {request.Url} file from {result}");
                            FileResult fileResult = new FileResult(result);
                            OutputFile(fileResult, request, response);
                            return;
                        }
                        if (resourceData != null)
                        {
                            OutputFileResource(fct, resourceData, response);
                        }
                        else
                        {
                            if (Server.EnableLog(EventArgs.LogType.Warring))
                                Server.BaseServer.Log(EventArgs.LogType.Warring, request.Session, $"HTTP {request.ID} {request.RemoteIPAddress} get {request.Url} file not found");
                            if (!Server.OnHttpRequesNotfound(request, response).Cancel)
                            {
                                NotFoundResult notFound = new NotFoundResult("{0} file not found", request.Url);
                                response.Result(notFound);
                            }
                        }

                    }
                }
            }
            else
            {

                if (Server.EnableLog(EventArgs.LogType.Warring))
                    Server.BaseServer.Log(EventArgs.LogType.Warring, request.Session, $"HTTP {request.ID} {request.RemoteIPAddress} get { request.BaseUrl} file ext not support");
                //NotSupportResult notSupport = new NotSupportResult("get {0} file {1} ext not support", request.Url, request.Ext);
                //response.Result(notSupport);
                response.InnerError("403", $"file *.{request.Ext} not support");
            }
        }

        private void SetGZIP(HttpResponse response)
        {
            response.Header.Add("Content-Encoding", "gzip");
        }

        private void SetChunked(HttpResponse response)
        {

            response.Header.Add("Transfer-Encoding", "chunked");

        }

        public bool ExtSupport(string ext)
        {

            return mExts.ContainsKey(ext);
        }

        public FileResource GetFileResource(string url)
        {
            FileResource result = null;
            mResources.TryGetValue(url, out result);
            return result;
        }

        public bool ExistsFile(HttpRequest request, string url, out string file)
        {

            file = GetFile(request, url);
            bool has = System.IO.File.Exists(file);
            return has;
        }

        public string GetFile(HttpRequest request, string url)
        {
            EventFindFileArgs e = new EventFindFileArgs();
            e.Request = request;
            e.Url = url;
            Find?.Invoke(this, e);
            if (string.IsNullOrEmpty(e.File))
            {
                if (Path[Path.Length - 1] == System.IO.Path.DirectorySeparatorChar)
                {
                    return Path + url.Substring(1, url.Length - 1);
                }
                else
                {
                    return Path + url.Replace('/', System.IO.Path.DirectorySeparatorChar);
                }
            }
            return e.File;
        }

        public string GetUrl(string file)
        {
            ReadOnlySpan<char> filebuffer = file.AsSpan().Slice(Path.Length, file.Length - Path.Length);
            char[] charbuffer = HttpParse.GetCharBuffer();
            int offset = 0;
            if (filebuffer[0] != System.IO.Path.DirectorySeparatorChar)
            {
                offset += 1;
                charbuffer[0] = '/';
            }
            for (int i = 0; i < filebuffer.Length; i++)
            {
                if (filebuffer[i] == '\\')
                {
                    charbuffer[i + offset] = '/';
                }
                else
                {
                    //if (Server.Options.UrlIgnoreCase)
                    //    charbuffer[i + offset] = Char.ToLower(filebuffer[i]);
                    //else
                    charbuffer[i + offset] = filebuffer[i];
                }
            }
            return new string(charbuffer, 0, filebuffer.Length + offset);
        }

        private FileResource CreateResource(string file, bool cache)
        {
            try
            {
                string ext = System.IO.Path.GetExtension(file).ToLower();
                ext = ext.Substring(1, ext.Length - 1);
                if (mExts.ContainsKey(ext))
                {
                    FileResource fr;
                    string urlname = "";
                    urlname = GetUrl(file);
                    if (cache)
                    {

                        if (!Debug)
                        {
                            if (mResources.TryGetValue(urlname, out fr))
                            {
                                if (Server.BaseServer.GetRunTime() - fr.CreateTime < 2000)
                                    return fr;
                            }
                        }
                    }
                    bool nogzip = !(Server.Options.NoGzipFiles.IndexOf(ext) >= 0);
                    bool cachefile = Server.Options.CacheFiles.IndexOf(ext) >= 0;
                    if (Debug)
                    {
                        fr = new NoCacheResource(file, urlname);
                    }
                    else
                    {
                        FileInfo info = new FileInfo(file);
                        if (cachefile && info.Length < 1024 * Server.Options.CacheFileSize)
                        {
                            fr = new FileResource(file, urlname);
                        }
                        else
                        {
                            fr = new NoCacheResource(file, urlname);
                            if (nogzip)
                                fr.GZIP = true;
                        }
                        if (info.Length < 1024 * 2)
                            fr.GZIP = false;
                    }

                    fr.Load();
                    fr.CreateTime = Server.BaseServer.GetRunTime();
                    if (cache && mResources.Count < 5000)
                    {
                        mResources[urlname] = fr;
                    }
                    if (Server.EnableLog(EventArgs.LogType.Info))
                        Server.BaseServer.Log(EventArgs.LogType.Info, null, "update {0} static resource success", urlname);
                    return fr;
                }
            }
            catch (Exception e_)
            {
                Server.BaseServer.Error(e_, null, "update {0} resource error {1}", file, e_.Message);
            }
            return null;
        }

        private void LoadFolder(string path)
        {
            if (path[path.Length - 1] != System.IO.Path.DirectorySeparatorChar)
            {
                path += System.IO.Path.DirectorySeparatorChar;
            }
            foreach (string file in System.IO.Directory.GetFiles(path))
            {
                CreateResource(file, true);
            }
            foreach (string folder in System.IO.Directory.GetDirectories(path))
            {
                string vfolder = folder.Replace(Server.Options.StaticResourcePath, "")
                    .Replace(System.IO.Path.DirectorySeparatorChar.ToString(), @"\");
                if (Server.Options.NotLoadFolder.IndexOf(vfolder, 0, StringComparison.CurrentCultureIgnoreCase) >= 0)
                    continue;
                LoadFolder(folder);
            }
        }

    }
}
