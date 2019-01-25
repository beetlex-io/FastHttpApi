using BeetleX.Buffers;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi
{
    public interface IDataResponse
    {
        void Write(PipeStream stream);
    }
}
