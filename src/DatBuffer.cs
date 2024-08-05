using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi
{
    public class DataBuffer<T> : IDisposable
    {
        public DataBuffer(int length)
        {
            Length = length;
            Data = ArrayPool<T>.Shared.Rent(length);
        }

        public T[] Data { get; set; }

        public int Length { get; set; }

        public int Offset { get; set; }

        public void Dispose()
        {
            ArrayPool<T>.Shared.Return(Data);
        }
    }
}
