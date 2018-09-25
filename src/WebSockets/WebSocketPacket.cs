using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BeetleX.Buffers;
using BeetleX.EventArgs;

namespace BeetleX.FastHttpApi.WebSockets
{
    class WebSocketPacket : IPacket
    {
        public WebSocketPacket(HttpConfig serverConfig, IDataFrameSerializer dataPacketSerializer)
        {
            mServerConfig = serverConfig;
            mBodySerializer = new StringSerializer();
            mDataPacketSerializer = dataPacketSerializer;
        }

        private IBodySerializer mBodySerializer;

        private IDataFrameSerializer mDataPacketSerializer;

        public EventHandler<PacketDecodeCompletedEventArgs> Completed { get; set; }

        private DataFrame mDataPacket;

        private bool mLodingDataPacket = false;

        public IPacket Clone()
        {
            return new WebSocketPacket(mServerConfig, mDataPacketSerializer);
        }

        private HttpConfig mServerConfig;

        private PacketDecodeCompletedEventArgs mCompletedArgs = new PacketDecodeCompletedEventArgs();

        private HttpRequest mConnectionRequest;

        public void Decode(ISession session, Stream stream)
        {
            PipeStream pstream = stream.ToPipeStream();
            if (pstream.Length > mServerConfig.MaxBodyLength)
            {
                session.Server.Log(LogType.Error, session, "http body too long!");
                session.Dispose();
                return;
            }
            START:
            if (!mLodingDataPacket)
            {
                if (mConnectionRequest == null)
                {
                    mConnectionRequest = new HttpRequest(session, mBodySerializer);
                }
                if (mConnectionRequest.Read(pstream) == LoadedState.Completed)
                {
                    mLodingDataPacket = true;
                    Completed?.Invoke(this, mCompletedArgs.SetInfo(session, mConnectionRequest));
                    if (pstream.Length > 0)
                        goto START;
                }
            }
            else
            {
                if (mDataPacket == null)
                {
                    mDataPacket = new DataFrame();
                    mDataPacket.DataPacketSerializer = this.mDataPacketSerializer;
                }
                if (mDataPacket.Read(pstream) == DataPacketLoadStep.Completed)
                {
                    DataFrame data = mDataPacket;
                    mDataPacket = null;
                    Completed?.Invoke(this, mCompletedArgs.SetInfo(session, data));
                    if (pstream.Length > 0)
                        goto START;
                }
            }

        }

        public void Dispose()
        {

        }

        public void Encode(object data, ISession session, Stream stream)
        {
            OnEncode(session, data, stream);
        }

        private void OnEncode(ISession session, object data, System.IO.Stream stream)
        {
            PipeStream pstream = stream.ToPipeStream();
            DataFrame dp = data as DataFrame;
            if (dp != null)
            {
                dp.Write(pstream);
            }
            else
            {
                HttpResponse response = (HttpResponse)data;
                response.Write(pstream);
            }

        }

        public byte[] Encode(object data, IServer server)
        {
            byte[] result = null;
            using (Buffers.PipeStream stream = new PipeStream(server.BufferPool, server.Config.LittleEndian, server.Config.Encoding))
            {
                OnEncode(null, data, stream);
                stream.Position = 0;
                result = new byte[stream.Length];
                stream.Read(result, 0, result.Length);
            }
            return result;
        }

        public ArraySegment<byte> Encode(object data, IServer server, byte[] buffer)
        {
            using (Buffers.PipeStream stream = new PipeStream(server.BufferPool, server.Config.LittleEndian, server.Config.Encoding))
            {
                OnEncode(null, data, stream);
                stream.Position = 0;
                int count = (int)stream.Length;
                stream.Read(buffer, 0, count);
                return new ArraySegment<byte>(buffer, 0, count);
            }
        }
    }
}
