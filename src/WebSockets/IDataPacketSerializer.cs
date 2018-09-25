using BeetleX.Buffers;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi.WebSockets
{
    public interface IDataFrameSerializer
    {
        object FrameDeserialize(DataFrame data, PipeStream stream);

        ArraySegment<byte> FrameSerialize(DataFrame packet, object body);

        void FrameRecovery(byte[] buffer);

    }
}
