using BeetleX.Buffers;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Text;

namespace BeetleX.FastHttpApi.StaticResurce
{
    public class FileBlock:IDataResponse
    {
        public FileBlock(FileResource rec)
        {
            Offset = 0;
            Size = 1024 * 8;
            Pages = rec.Length / Size;
            if (rec.Length % Size > 0)
            {
                Pages++;
            }
            mFileResource = rec;
            LoadData();


            GZip = false;
        }

        public bool GZip { get; set; }

        private FileResource mFileResource;

        private GZipStream gZipStream;

        private void LoadData()
        {
            int roffset;
            Data = mFileResource.GetBlodk(Offset, Size, out roffset);
            Offset = roffset;

        }

        public int Size;

        public int Offset;

        public int Pages;

        public ArraySegment<byte> Data;

        public FileBlock Next()
        {
            mFileResource.Recovery(Data.Array);
            if (Offset < mFileResource.Length)
            {
                LoadData();
                return this;
            }
            return null;
        }
        void IDataResponse.Write(PipeStream stream)
        {
            if (GZip)
            {
                var mb = stream.Allocate(16);
                stream.Write(HeaderTypeFactory.LINE_BYTES);
                int len = stream.CacheLength;
                if (gZipStream == null)
                    gZipStream = new GZipStream(stream, CompressionMode.Compress, true);
                gZipStream.Write(Data.Array, Data.Offset, Data.Count);
                gZipStream.Flush();
                if (Offset == mFileResource.Length)
                {
                    if (gZipStream != null)
                    {
                        using (stream.LockFree())
                        {
                            gZipStream.Dispose();
                        }
                    }
                }
                string lenstr = (stream.CacheLength - len).ToString("X");
                mb.Full(Encoding.UTF8.GetBytes(lenstr.PadRight(16)));
            }
            else
            {
                int len = Data.Count;
                stream.Write(len.ToString("X"));
                stream.Write(HeaderTypeFactory.LINE_BYTES);
                stream.Write(Data.Array, Data.Offset, Data.Count);
            }
            stream.WriteLine("");
            if (Offset == mFileResource.Length)
            {
                stream.Write(HeaderTypeFactory.CHUNKED_BYTES);
            }
        }
    }
}
