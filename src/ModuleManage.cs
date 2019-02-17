using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO.Compression;
using System.Reflection;
using System.IO;

namespace BeetleX.FastHttpApi
{
    public class ModuleManager
    {

        public ModuleManager(HttpApiServer server)
        {
            Server = server;
            mPath = System.IO.Directory.GetCurrentDirectory();
            mPath += System.IO.Path.DirectorySeparatorChar + "_models" + System.IO.Path.DirectorySeparatorChar;
            if (!System.IO.Directory.Exists(mPath))
            {
                System.IO.Directory.CreateDirectory(mPath);
            }
            mRunningPath = System.IO.Directory.GetCurrentDirectory();
            mRunningPath += System.IO.Path.DirectorySeparatorChar + "_runing_models" + System.IO.Path.DirectorySeparatorChar;
            if (!System.IO.Directory.Exists(mRunningPath))
            {
                System.IO.Directory.CreateDirectory(mRunningPath);
            }

            fileSystemWatcher = new FileSystemWatcher(mPath, "*.zip");
            fileSystemWatcher.IncludeSubdirectories = false;
            fileSystemWatcher.Changed += OnFileWatchHandler;
            fileSystemWatcher.Created += OnFileWatchHandler;
            fileSystemWatcher.Renamed += OnFileWatchHandler;
            fileSystemWatcher.EnableRaisingEvents = true;
            mUpdateTimer = new System.Threading.Timer(OnUpdateHandler, null, 5000, 5000);
        }

        private System.Threading.Timer mUpdateTimer;

        private int mUpdateCount = 0;

        private System.Collections.Concurrent.ConcurrentDictionary<string, UpdateItem> mUpdateItems = new System.Collections.Concurrent.ConcurrentDictionary<string, UpdateItem>();

        class UpdateItem
        {
            public string Name { get; set; }

            public string FullName { get; set; }

            public string Module { get; set; }

            public long Time { get; set; }

        }

        private void OnUpdateHandler(object state)
        {
            mUpdateTimer.Change(-1, -1);
            try
            {
                var modules = mUpdateItems.Keys;
                foreach (string module in modules)
                {
                    if (mUpdateItems.TryGetValue(module, out UpdateItem item))
                    {
                        if (Server.BaseServer.GetRunTime() - item.Time > 10 * 1000)
                        {
                            try
                            {
                                Load(item.Module);
                                Server.Log(EventArgs.LogType.Info, $"{item.Module} upgrades success");
                            }
                            catch (Exception e_)
                            {
                                Server.Log(EventArgs.LogType.Error, $"{item.Module} upgrades error {e_.Message} {e_.StackTrace}");
                            }
                            finally
                            {
                                mUpdateItems.TryRemove(module, out item);
                            }
                        }
                    }
                }
            }
            catch (Exception e_)
            {
                Server.Log(EventArgs.LogType.Error, $"upgrades module error {e_.Message} {e_.StackTrace}");
            }
            finally
            {
                mUpdateTimer.Change(5000, 5000);
            }
        }

        private void OnFileWatchHandler(object sender, FileSystemEventArgs e)
        {
            UpdateItem item = null;
            if (mUpdateItems.TryGetValue(e.Name, out item))
            {
                item.Time = Server.BaseServer.GetRunTime();
            }
            else
            {
                item = new UpdateItem();
                item.Name = e.Name;
                item.Module = System.IO.Path.GetFileNameWithoutExtension(e.Name);
                item.FullName = e.FullPath;
                item.Time = Server.BaseServer.GetRunTime();
                mUpdateItems[e.Name] = item;
                Server.Log(EventArgs.LogType.Info, $"upload {e.Name} module");
            }
        }

        private FileSystemWatcher fileSystemWatcher;

        private string mRunningPath;

        private string mPath;

        public HttpApiServer Server { get; set; }

        public IEnumerable<string> List()
        {
            var result = from a in System.IO.Directory.GetFiles(mPath) select System.IO.Path.GetFileNameWithoutExtension(a);
            return result;
        }

        private void ClearFiles()
        {
            try
            {
                foreach (var folder in System.IO.Directory.GetDirectories(mRunningPath))
                {
                    System.IO.Directory.Delete(folder, true);
                }
            }
            catch (Exception e_)
            {
                Server.Log(EventArgs.LogType.Error, $"clear files error {e_.Message}");
            }
        }

        public void Load()
        {
            ClearFiles();
            var items = List();
            if (items != null)
                foreach (var item in items)
                {
                    Load(item);
                }
        }

        private void OnLoadAssembly(IList<string> files, int count)
        {
            List<string> success = new List<string>();
            foreach (string file in files)
            {
                string aname = System.IO.Path.GetFileName(file);
                try
                {
                    System.Reflection.Assembly assembly = System.Reflection.Assembly.LoadFile(file);
                    Server.ResourceCenter.LoadManifestResource(assembly);
                    Server.ActionFactory.Register(assembly);
                    Server.Log(EventArgs.LogType.Info, $"loaded {aname} assembly success");
                    OnAssemblyLoding(new EventAssemblyLoadingArgs(assembly));
                    success.Add(file);

                }
                catch (Exception e_)
                {
                    Server.Log(EventArgs.LogType.Error, $"load {aname} assembly error {e_.Message} {e_.StackTrace}");
                }
            }
        }

        public bool SaveFile(string name, string md5, bool eof, byte[] data)
        {
            string file = mPath + name + ".tmp";
            using (System.IO.Stream stream = System.IO.File.Open(file, FileMode.Append))
            {
                stream.Write(data, 0, data.Length);
                stream.Flush();
            }
            if (eof)
            {
                if (Utils.GetFileMD5(file) != md5)
                {
                    System.IO.File.Delete(file);
                    throw new Exception("Verify file md5 value error!");

                }
                else
                {
                    string targetFile = mPath + name + ".zip";
                    if (System.IO.File.Exists(targetFile))
                        System.IO.File.Delete(targetFile);
                    System.IO.File.Move(file, targetFile);
                    return true;
                }
            }
            return false;
        }

        public void Load(string module)
        {
            try
            {
                mUpdateCount++;
                if (mUpdateCount >= 1000)
                    mUpdateCount = 0;
                Server.Log(EventArgs.LogType.Info, $"loding {module} module ...");
                string zipfile = mPath + module + ".zip";
                string target = mRunningPath + module + mUpdateCount.ToString("000") + System.IO.Path.DirectorySeparatorChar;
                if (System.IO.Directory.Exists(target))
                {
                    System.IO.Directory.Delete(target, true);
                }
                if (System.IO.File.Exists(zipfile))
                {
                    if (!System.IO.Directory.Exists(target))
                        System.IO.Directory.CreateDirectory(target);
                    ZipFile.ExtractToDirectory(zipfile, target, true);
                    string beetledll = System.IO.Directory.GetCurrentDirectory() + System.IO.Path.DirectorySeparatorChar + "BeetleX.dll";
                    string fastApidll = System.IO.Directory.GetCurrentDirectory() + System.IO.Path.DirectorySeparatorChar + "BeetleX.FastHttpApi.dll";
                    System.IO.File.Copy(beetledll, target + "BeetleX.dll", true);
                    System.IO.File.Copy(fastApidll, target + "BeetleX.FastHttpApi.dll", true);
                    List<string> files = new List<string>();
                    foreach (string assemblyFile in System.IO.Directory.GetFiles(target, "*.dll"))
                    {
                        files.Add(assemblyFile);
                    }
                    OnLoadAssembly(files, 0);
                    Server.Log(EventArgs.LogType.Info, $"loaded {module} module success");
                }
                else
                {
                    Server.Log(EventArgs.LogType.Warring, $"{module} not found!");
                }
            }
            catch (Exception e_)
            {
                Server.Log(EventArgs.LogType.Error, $"load {module} error {e_.Message} {e_.StackTrace}");
            }
        }


        public event EventHandler<EventAssemblyLoadingArgs> AssemblyLoding;

        protected virtual void OnAssemblyLoding(EventAssemblyLoadingArgs e)
        {
            AssemblyLoding?.Invoke(this, e);
        }

        public class EventAssemblyLoadingArgs : System.EventArgs
        {
            public EventAssemblyLoadingArgs(Assembly assembly)
            {
                Assembly = assembly;
            }
            public Assembly Assembly { get; private set; }
        }
    }
}
