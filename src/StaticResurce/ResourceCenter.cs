using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BeetleX.FastHttpApi.StaticResurce
{
    class ResourceCenter
    {
        public ResourceCenter(HttpApiServer server)
        {
            Server = server;
            Path = Server.ServerConfig.StaticResourcePath;
            if (Path[Path.Length - 1] != System.IO.Path.DirectorySeparatorChar)
            {
                Path += System.IO.Path.DirectorySeparatorChar;
            }

            foreach (string item in server.ServerConfig.StaticResurceType.ToLower().Split(';'))
            {
                FileContentType fct = new FileContentType(item);
                mExts[fct.Ext] = fct;
            }
            mDefaultPages.AddRange(Server.ServerConfig.DefaultPage.Split(";"));

        }

        private ConcurrentDictionary<string, FileResource> mResources = new ConcurrentDictionary<string, FileResource>();

        private ConcurrentDictionary<string, FileContentType> mExts = new ConcurrentDictionary<string, FileContentType>();

        private List<FileSystemWatcher> mFileWatch = new List<FileSystemWatcher>();

        private List<string> mDefaultPages = new List<string>();

        public HttpApiServer Server { get; private set; }

        public string Path { get; internal set; }

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
            return HttpParse.CharToLower(charname);
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
                        if (Debug)
                        {
                            fr = new NoGzipResource(filename, urlname);
                        }
                        else
                        {
                            if ("jpg;jpeg;png;gif;png".IndexOf(ext) >= 0)
                            {
                                fr = new NoGzipResource(filename, urlname);
                            }
                            else
                            {
                                fr = new FileResource(filename, urlname);
                            }
                        }
                        mResources[urlname] = fr;
                        fr.Load();
                        Server.BaseServer.Log(EventArgs.LogType.Info, null, "load static resource " + urlname);
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
                            CreateResource(e.FullPath);
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
                if (response.Request.IfNoneMatch == fr.FileMD5)
                {
                    response.NoModify();
                    return;
                }
            }

            if (fr.GZIP)
            {
                SetGZIP(response);
            }
            if (!Debug)
            {
                if (fr.Cached)
                {
                    response.Header.Add(HeaderType.CACHE_CONTROL, "private, max-age=31536000");
                }
                else
                {
                    response.Header.Add(HeaderType.ETAG, fr.FileMD5);
                }
            }
            SetChunked(response);
            HttpToken token = (HttpToken)response.Session.Tag;
            token.File = new FileBlock(fr);
            response.SetContentType(fct.ContentType);
            response.Result(token.File);
        }

        public void ProcessFile(HttpRequest reqeust, HttpResponse response)
        {
            string url = HttpParse.CharToLower(reqeust.BaseUrl);
            if (url == "/")
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
                response.NotFound();
                return;
            }

            if (ExtSupport(reqeust.Ext))
            {
                FileContentType fct = mExts[reqeust.Ext];
                FileResource fr = GetFileResource(url);
                if (fr != null)
                {
                    OutputFileResource(fct, fr, response);
                }
                else
                {
                    if (ExistsFile(reqeust.BaseUrl))
                    {
                        string file = GetFile(url);
                        fr = CreateResource(file);
                        if (fr != null)
                        {
                            OutputFileResource(fct, fr, response);
                        }
                    }
                    else
                    {
                        response.NotFound();
                    }
                }
            }
            else
            {
                response.NotSupport();
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

        public bool ExistsFile(string url)
        {
            string file = GetFile(url);
            return System.IO.File.Exists(file);
        }

        public string GetFile(string url)
        {
            if (Path[Path.Length - 1] == System.IO.Path.DirectorySeparatorChar)
            {
                return Path + url.Substring(1, url.Length - 1);
            }
            else
            {
                return Path + url;
            }
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
                    charbuffer[i + offset] = Char.ToLower(filebuffer[i]);
                }
            }
            return new string(charbuffer, 0, filebuffer.Length + offset);
        }

        private FileResource CreateResource(string file)
        {
            try
            {
                string ext = System.IO.Path.GetExtension(file).ToLower();
                ext = ext.Substring(1, ext.Length - 1);
                if (mExts.ContainsKey(ext))
                {
                    string urlname = GetUrl(file);
                    FileResource fr;
                    if (!Debug)
                    {
                        if (mResources.TryGetValue(urlname, out fr))
                        {
                            if (Server.BaseServer.GetRunTime() - fr.CreateTime < 2000)
                                return fr;
                        }
                    }
                    if (Debug)
                    {
                        fr = new NoGzipResource(file, urlname);
                    }
                    else
                    {
                        if ("jpg;jpeg;png;gif;png".IndexOf(ext) >= 0)
                        {
                            fr = new NoGzipResource(file, urlname);
                        }
                        else
                        {
                            fr = new FileResource(file, urlname);
                        }
                    }
                    mResources[urlname] = fr;
                    fr.CreateTime = Server.BaseServer.GetRunTime();
                    fr.Load();
                    Server.BaseServer.Log(EventArgs.LogType.Info, null, "load static resource " + urlname);
                    return fr;
                }
            }
            catch (Exception e_)
            {
                Server.BaseServer.Error(e_, null, "load " + file + " resource error");
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
                CreateResource(file);
            }
            foreach (string folder in System.IO.Directory.GetDirectories(path))
            {
                LoadFolder(folder);
            }
        }

    }
}
