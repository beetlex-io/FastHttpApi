using BeetleX.Buffers;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi.WebSockets
{
    public interface IDataFrameSerializer
    {
        object FrameDeserialize(DataFrame data, PipeStream stream,HttpRequest request);

        ArraySegment<byte> FrameSerialize(DataFrame packet, object body,HttpRequest request);

        void FrameRecovery(byte[] buffer);

    }
}
