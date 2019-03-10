using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace BeetleX.FastHttpApi.StaticResurce
{
    public class FileResource
    {
        public FileResource(string filename, string urlname, bool innerResource = false)
        {
            FullName = filename;
            UrlName = urlname;
            mInnerResource = innerResource;
            GZIP = true;
            Cached = false;
            Ext = System.IO.Path.GetExtension(filename);
        }

        public System.Reflection.Assembly Assembly { get; set; }

        private bool mInnerResource;

        public string Ext { get; set; }

        public string UrlName { get; set; }

        public long CreateTime { get; set; }

        public string FileMD5 { get; set; }

        public string Name { get; set; }

        public string FullName { get; set; }

        public int Length { get; set; }

        public byte[] Data { get; set; }

        public virtual bool GZIP { get; set; }

        public virtual bool Cached { get; set; }

        public bool InnerResource => mInnerResource;

        public virtual ArraySegment<byte> GetBlodk(int offset, int size, out int newOffset)
        {
            int length = Data.Length - offset;
            if (length >= size)
                length = size;
            newOffset = offset + length;
            return new ArraySegment<byte>(Data, offset, length);
        }

        public virtual void Recovery(byte[] buffer)
        {

        }

        protected virtual void LoadFile()
        {
            int length;
            System.IO.Stream fsstream;
            if (InnerResource)
            {
                fsstream = Assembly.GetManifestResourceStream(FullName);
            }
            else
            {
                fsstream = System.IO.File.Open(FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            using (fsstream)
            {
                length = (int)fsstream.Length;
                byte[] buffer = HttpParse.GetByteBuffer();
                if (length > 0)
                {
                    using (System.IO.MemoryStream memory = new MemoryStream())
                    {
                        using (GZipStream gstream = new GZipStream(memory, CompressionMode.Compress))
                        {
                            while (length > 0)
                            {
                                int len = fsstream.Read(buffer, 0, buffer.Length);
                                length -= len;
                                gstream.Write(buffer, 0, len);
                                gstream.Flush();
                                if (length == 0)
                                    Data = memory.ToArray();
                            }
                        }
                    }
                }
                else
                {
                    Data = new Byte[0];
                }
            }
            //  fsstream.Close();
            Length = Data.Length;
        }

        public void Load()
        {
            Name = System.IO.Path.GetFileName(FullName);
            if (!InnerResource)
            {
                FileInfo fi = new FileInfo(FullName);
                Length = (int)fi.Length;
            }
            else
            {
                using (System.IO.Stream stream = Assembly.GetManifestResourceStream(FullName))
                {
                    Length = (int)stream.Length;
                }
            }
            if (Length < 1024 * 1024 && !string.IsNullOrEmpty(UrlName))
            {
                FileMD5 = FMD5(FullName, this.Assembly);
            }
            LoadFile();
        }

        public string MD5Encrypt(string filename)
        {
            using (MD5 md5Hash = MD5.Create())
            {
                byte[] hash = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(filename));

                return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
            }
        }

        public virtual FileBlock CreateFileBlock()
        {
            return new FileBlock(this);
        }

        public static string FMD5(string filename, System.Reflection.Assembly assembly)
        {
            using (var md5 = MD5.Create())
            {
                System.IO.Stream stream;
                if (assembly != null)
                {
                    stream = assembly.GetManifestResourceStream(filename);
                }
                else
                {
                    stream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
                }
                using (stream)
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }
    }

    class NoCacheResource : FileResource
    {
        public NoCacheResource(string filename, string urlname, bool innerResource = false) : base(filename, urlname, innerResource)
        {

            GZIP = false;
        }

        protected override void LoadFile()
        {

        }

        private System.Collections.Concurrent.ConcurrentQueue<byte[]> mPool = new System.Collections.Concurrent.ConcurrentQueue<byte[]>();

        public override ArraySegment<byte> GetBlodk(int offset, int size, out int newOffset)
        {
            int length = Length - offset;
            if (length >= size)
                length = size;
            newOffset = offset + length;
            byte[] buffer = GetBuffer(size);
            System.IO.Stream fsstream;
            if (InnerResource)
            {
                fsstream = Assembly.GetManifestResourceStream(FullName);
            }
            else
            {
                fsstream = System.IO.File.Open(FullName, FileMode.Open, FileAccess.Read, FileShare.Read);
            }
            using (fsstream)
            {
                fsstream.Seek(offset, SeekOrigin.Begin);
                int len = fsstream.Read(buffer, 0, buffer.Length);
                return new ArraySegment<byte>(buffer, 0, len);
            }
        }

        public override FileBlock CreateFileBlock()
        {
            FileBlock result = base.CreateFileBlock();
            result.GZip = this.GZIP;
            return result;
        }

        protected virtual byte[] GetBuffer(int size)
        {
            byte[] buffer = null;
            if (!mPool.TryDequeue(out buffer))
            {
                buffer = new byte[size];
            }
            return buffer;
        }

        public override void Recovery(byte[] buffer)
        {
            mPool.Enqueue(buffer);

        }
    }
}
