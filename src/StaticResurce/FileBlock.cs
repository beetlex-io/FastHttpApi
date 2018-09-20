using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi.StaticResurce
{
    class FileBlock
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

        }

        private FileResource mFileResource;

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

        public void Write(BeetleX.Buffers.PipeStream stream)
        {
            int len = Data.Count;
            stream.Write(len.ToString("X"));
            stream.Write(HeaderType.LINE_BYTES);
            stream.Write(Data.Array, Data.Offset, Data.Count);
            stream.WriteLine("");
            if (Offset == mFileResource.Length)
                stream.Write(HeaderType.CHUNKED_BYTES);
        }
    }
}
