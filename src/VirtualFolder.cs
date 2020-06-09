using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi
{
    public class VirtualFolder
    {
        public string Folder { get; set; }

        public string Path { get; set; }

        public void Verify()
        {
            if (Folder[Folder.Length - 1] != '/')
                Folder += "/";
            if (Folder[0] != '/')
                Folder += "/";
            if (Path[Path.Length - 1] != System.IO.Path.DirectorySeparatorChar)
            {
                Path += System.IO.Path.DirectorySeparatorChar;
            }
            if (!System.IO.Directory.Exists(Path))
            {
                throw new Exception($"Set virtual folder error {Path} path not found!");
            }
        }

        public string Match(string file)
        {
            if (file.IndexOf(Folder, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return Path + file.Substring(Folder.Length);
            }
            return null;
        }

        public string Match(HttpRequest request)
        {
            return Match(request.BaseUrl);
        }
    }
}
