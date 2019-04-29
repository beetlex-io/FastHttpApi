using BeetleX.Buffers;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi.WebSockets
{


    public enum DataPacketLoadStep
    {
        None,
        Header,
        Length,
        Mask,
        Completed
    }


    public class DataFrame : IDataResponse
    {
        internal DataFrame()
        {
            this.FIN = true;
            Type = DataPacketType.text;
            IsMask = false;
        }

        const int CHECK_B1 = 0x1;

        const int CHECK_B2 = 0x2;

        const int CHECK_B3 = 0x4;

        const int CHECK_B4 = 0x8;

        const int CHECK_B5 = 0x10;

        const int CHECK_B6 = 0x20;

        const int CHECK_B7 = 0x40;

        const int CHECK_B8 = 0x80;

        public bool FIN { get; set; }

        public bool RSV1 { get; set; }

        public bool RSV2 { get; set; }

        public bool RSV3 { get; set; }

        internal IDataFrameSerializer DataPacketSerializer { get; set; }

        public DataPacketType Type { get; set; }

        public Object Body { get; set; }

        public bool IsMask { get; set; }

        internal byte PayloadLen { get; set; }

        public ulong Length { get; set; }

        public byte[] MaskKey { get; set; }

        private DataPacketLoadStep mLoadStep = DataPacketLoadStep.None;

        internal DataPacketLoadStep Read(PipeStream stream)
        {
            if (mLoadStep == DataPacketLoadStep.None)
            {
                if (stream.Length >= 2)
                {
                    byte value = (byte)stream.ReadByte();
                    this.FIN = (value & CHECK_B8) > 0;
                    this.RSV1 = (value & CHECK_B7) > 0;
                    this.RSV2 = (value & CHECK_B6) > 0;
                    this.RSV3 = (value & CHECK_B5) > 0;
                    this.Type = (DataPacketType)(byte)(value & 0xF);
                    value = (byte)stream.ReadByte();
                    this.IsMask = (value & CHECK_B8) > 0;
                    this.PayloadLen = (byte)(value & 0x7F);
                    mLoadStep = DataPacketLoadStep.Header;
                }
            }
            if (mLoadStep == DataPacketLoadStep.Header)
            {
                if (this.PayloadLen == 127)
                {
                    if (stream.Length >= 8)
                    {
                        Length = stream.ReadUInt64();
                        mLoadStep = DataPacketLoadStep.Length;
                    }
                }
                else if (this.PayloadLen == 126)
                {
                    if (stream.Length >= 2)
                    {
                        Length = stream.ReadUInt16();
                        mLoadStep = DataPacketLoadStep.Length;
                    }
                }
                else
                {
                    this.Length = this.PayloadLen;
                    mLoadStep = DataPacketLoadStep.Length;
                }
            }
            if (mLoadStep == DataPacketLoadStep.Length)
            {
                if (IsMask)
                {
                    if (stream.Length >= 4)
                    {
                        this.MaskKey = new byte[4];
                        stream.Read(this.MaskKey, 0, 4);
                        mLoadStep = DataPacketLoadStep.Mask;
                    }
                }
                else
                {
                    mLoadStep = DataPacketLoadStep.Mask;
                }
            }
            if (mLoadStep == DataPacketLoadStep.Mask)
            {
                if (this.Length > 0 && (ulong)stream.Length >= this.Length)
                {
                    if (this.IsMask)
                        ReadMask(stream);
                    Body = this.DataPacketSerializer.FrameDeserialize(this, stream);
                    mLoadStep = DataPacketLoadStep.Completed;
                }
            }
            return mLoadStep;
        }

        private void ReadMask(PipeStream stream)
        {
            IndexOfResult result = stream.IndexOf((int)this.Length);
            ulong index = 0;
            if (result.Start.ID == result.End.ID)
            {
                index = MarkBytes(result.Start.Data, result.StartPostion, result.EndPostion, index);
            }
            else
            {
                index = MarkBytes(result.Start.Data, result.StartPostion, result.Start.Length - 1, index);
                IMemoryBlock next = result.Start.NextMemory;
                while (next != null && index < this.Length)
                {
                    if (next.ID == result.End.ID)
                    {
                        index = MarkBytes(next.Data, 0, result.EndPostion, index);
                        break;
                    }
                    else
                    {
                        index = MarkBytes(next.Data, 0, next.Length - 1, index);
                    }
                    next = next.NextMemory;
                }
            }
        }

        private ulong MarkBytes(Span<byte> bytes, int start, int end, ulong index)
        {
            for (int i = start; i <= end; i++)
            {
                bytes[i] = (byte)(bytes[i] ^ MaskKey[index % 4]);
                index++;
                if (index >= this.Length)
                    break;
            }
            return index;
        }
        void IDataResponse.Write(PipeStream stream)
        {
            try
            {
                byte[] header = new byte[2];
                if (FIN)
                    header[0] |= CHECK_B8;
                if (RSV1)
                    header[0] |= CHECK_B7;
                if (RSV2)
                    header[0] |= CHECK_B6;
                if (RSV3)
                    header[0] |= CHECK_B5;
                header[0] |= (byte)Type;
                if (Body != null)
                {
                    ArraySegment<byte> data = this.DataPacketSerializer.FrameSerialize(this, Body);
                    try
                    {
                        if (MaskKey == null || MaskKey.Length != 4)
                            this.IsMask = false;
                        if (this.IsMask)
                        {
                            header[1] |= CHECK_B8;
                            int offset = data.Offset;
                            for (int i = offset; i < data.Count; i++)
                            {
                                data.Array[i] = (byte)(data.Array[i] ^ MaskKey[(i - offset) % 4]);
                            }
                        }
                        int len = data.Count;
                        if (len > 125 && len <= UInt16.MaxValue)
                        {
                            header[1] |= (byte)126;
                            stream.Write(header, 0, 2);
                            stream.Write((UInt16)len);
                        }
                        else if (len > UInt16.MaxValue)
                        {
                            header[1] |= (byte)127;
                            stream.Write(header, 0, 2);
                            stream.Write((ulong)len);
                        }
                        else
                        {
                            header[1] |= (byte)data.Count;
                            stream.Write(header, 0, 2);
                        }
                        if (IsMask)
                            stream.Write(MaskKey, 0, 4);
                        stream.Write(data.Array, data.Offset, data.Count);
                    }
                    finally
                    {
                        this.DataPacketSerializer.FrameRecovery(data.Array);
                    }
                }
                else
                {
                    stream.Write(header, 0, 2);
                }
            }
            finally
            {
                //this.DataPacketSerializer = null;
                //this.Body = null;
            }
        }

        public void Send(ISession session)
        {
            HttpToken token = (HttpToken)session.Tag;
            if (token != null && token.WebSocket)
            {
                session.Send(this);
                if (session.Server.EnableLog(EventArgs.LogType.Info))
                {
                    session.Server.Log(EventArgs.LogType.Info, session, "{0} websocket send data {1}", session.RemoteEndPoint, this.Type.ToString());
                }
            }
        }
    }
}
