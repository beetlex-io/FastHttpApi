using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;

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

        public void SetFileExts(string exts)
        {
            if (exts != null)
            {
                Server.Options.StaticResurceType = exts;
                foreach (string item in exts.ToLower().Split(';'))
                {
                    FileContentType fct = new FileContentType(item);
                    mExts[fct.Ext] = fct;
                }
            }
        }

        private ConcurrentDictionary<string, FileResource> mResources = new ConcurrentDictionary<string, FileResource>();

        private ConcurrentDictionary<string, FileContentType> mExts = new ConcurrentDictionary<string, FileContentType>();

        private List<FileSystemWatcher> mFileWatch = new List<FileSystemWatcher>();

        private List<string> mDefaultPages = new List<string>();

        public List<string> DefaultPages => mDefaultPages;

        public ConcurrentDictionary<string, FileContentType> Exts => mExts;

        public HttpApiServer Server { get; private set; }

        public string Path { get; internal set; }

        public event EventHandler<EventFindFileArgs> Find;

        public event EventHandler<EventFileResponseArgs> FileResponse;

        public class EventFindFileArgs : System.EventArgs
        {
            public string File { get; set; }

            public HttpRequest Request { get; internal set; }

            public string Url { get; internal set; }
        }

        public class EventFileResponseArgs : System.EventArgs
        {
            public HttpRequest Request { get; internal set; }

            public HttpResponse Response { get; internal set; }

            public FileResource Resource { get; set; }
        }

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
            if (Server.Options.UrlIgnoreCase)
                return HttpParse.CharToLower(charname);
            else
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
                        Server?.BaseServer?.Log(EventArgs.LogType.Info, null, "load static resource " + urlname);
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
                            CreateResource(e.FullPath, true);
                        };
                        fsw.EnableRaisingEvents = true;
                        mFileWatch.Add(fsw);
                    }
                }
            }


        }

        private void OutputFileResource(FileContentType fct, FileResource fr, HttpResponse response)
        {
            if (!Debug)
            {
                string IfNoneMatch = response.Request.IfNoneMatch;
                if (!string.IsNullOrEmpty(IfNoneMatch) && IfNoneMatch == fr.FileMD5)
                {
                    if (Server.EnableLog(EventArgs.LogType.Info))
                        Server.BaseServer.Log(EventArgs.LogType.Info, null, "{0} get {1} source no modify ", response.Request.RemoteIPAddress, response.Request.Url);
                    if (Server.Options.StaticResurceCacheTime > 0)
                    {
                        response.Header.Add(HeaderTypeFactory.CACHE_CONTROL, "public, max-age=" + Server.Options.StaticResurceCacheTime);
                    }
                    NoModifyResult result = new NoModifyResult();
                    response.Result(result);
                    return;
                }
            }
            if (fr.GZIP)
            {
                SetGZIP(response);
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
            SetChunked(response);
            EventFileResponseArgs efra = new EventFileResponseArgs();
            efra.Request = response.Request;
            efra.Response = response;
            efra.Resource = fr;
            FileResponse?.Invoke(this, efra);
            if (Server.EnableLog(EventArgs.LogType.Info))
            {
                Server.BaseServer.Log(EventArgs.LogType.Info, response.Request.Session, "{0} get {1} response gzip {2}",
                    response.Request.RemoteIPAddress, response.Request.BaseUrl, fr.GZIP);
            }
            HttpToken token = (HttpToken)response.Session.Tag;
            token.File = fr.CreateFileBlock();
            response.SetContentType(fct.ContentType);
            response.Result(token.File);

        }

        public void ProcessFile(HttpRequest request, HttpResponse response)
        {
            string url = request.BaseUrl;
            if (Server.Options.UrlIgnoreCase)
                url = HttpParse.CharToLower(request.BaseUrl);
            if (url[url.Length - 1] == '/')
            {
                for (int i = 0; i < mDefaultPages.Count; i++)
                {
                    string defaultpage = url + mDefaultPages[i];
                    string ext = HttpParse.GetBaseUrlExt(defaultpage);
                    FileContentType fct = null;
                    if (!mExts.TryGetValue(ext, out fct))
                    {
                        continue;
                    }
                    FileResource fr = GetFileResource(defaultpage);
                    if (fr != null)
                    {
                        OutputFileResource(fct, fr, response);
                        return;
                    }
                }
                if (Server.EnableLog(EventArgs.LogType.Warring))
                    Server.BaseServer.Log(EventArgs.LogType.Warring, request.Session, "{0} get {1} file not found", request.RemoteIPAddress, request.BaseUrl);
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
                FileContentType fct = mExts[request.Ext];
                FileResource fr = GetFileResource(url);
                if (fr != null)
                {
                    OutputFileResource(fct, fr, response);
                }
                else
                {
                    string file;
                    string fileurl = HttpParse.GetBaseUrl(request.Url);
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
                        if (Server.EnableLog(EventArgs.LogType.Warring))
                            Server.BaseServer.Log(EventArgs.LogType.Warring, request.Session, "{0} get {1} file not found", request.RemoteIPAddress, request.BaseUrl);
                        if (!Server.OnHttpRequesNotfound(request, response).Cancel)
                        {
                            NotFoundResult notFound = new NotFoundResult("{0} file not found", request.Url);
                            response.Result(notFound);
                        }

                    }
                }
            }
            else
            {

                if (Server.EnableLog(EventArgs.LogType.Warring))
                    Server.BaseServer.Log(EventArgs.LogType.Warring, request.Session, "{0} get {1} file ext not support", request.RemoteIPAddress, request.BaseUrl);
                NotSupportResult notSupport = new NotSupportResult("get {0} file {1} ext not support", request.Url, request.Ext);
                response.Result(notSupport);
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
                    if (Server.Options.UrlIgnoreCase)
                        charbuffer[i + offset] = Char.ToLower(filebuffer[i]);
                    else
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
                        if (nogzip)
                            fr.GZIP = true;
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
