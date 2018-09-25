using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using BeetleX.Buffers;
using BeetleX.EventArgs;
using BeetleX.FastHttpApi.WebSockets;

namespace BeetleX.FastHttpApi
{
    public class HttpPacket : IPacket
    {

        public HttpPacket(IBodySerializer bodySerializer, HttpConfig serverConfig, IDataFrameSerializer dataPacketSerializer)
        {
            Serializer = bodySerializer;
            mServerConfig = serverConfig;
            mDataPacketSerializer = dataPacketSerializer;
        }

        public EventHandler<PacketDecodeCompletedEventArgs> Completed { get; set; }

        public IPacket Clone()
        {
            return new HttpPacket(this.Serializer, mServerConfig, this.mDataPacketSerializer);
        }

        private HttpConfig mServerConfig;

        private PacketDecodeCompletedEventArgs mCompletedArgs = new PacketDecodeCompletedEventArgs();

        private HttpRequest mRequest;

        private DataFrame mDataPacket;

        private WebSockets.IDataFrameSerializer mDataPacketSerializer;

        public IBodySerializer Serializer { get; set; }

        private void OnHttpDecode(ISession session, PipeStream pstream)
        {
            START:
            if (mRequest == null)
            {
                mRequest = new HttpRequest(session, this.Serializer);
            }
            if (mRequest.Read(pstream) == LoadedState.Completed)
            {
                int length = mRequest.Length;
                int slength = (int)pstream.Length;
                if (string.Compare(mRequest.Method, "GET", true) == 0 || string.Compare(mRequest.Method, "POST", true) == 0)
                {
                    Completed?.Invoke(this, mCompletedArgs.SetInfo(session, mRequest));
                    if (pstream.Length == slength)
                    {
                        pstream.ReadFree(length);
                    }
                }
                else
                {
                    mRequest.CreateResponse().NotSupport();
                    if (length > 0)
                        pstream.ReadFree(length);
                }
                mRequest = null;
                if (pstream.Length == 0)
                    return;
                goto START;
            }
            else
            {
                if (mRequest.Length > mServerConfig.MaxBodyLength)
                {
                    session.Dispose();
                    return;
                }
                return;
            }
        }
        private void OnWebSocketDecode(ISession session, PipeStream pstream)
        {
            START:
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

        public void Decode(ISession session, Stream stream)
        {
            HttpToken token = (HttpToken)session.Tag;
            PipeStream pstream = stream.ToPipeStream();
            if (pstream.Length > mServerConfig.MaxBodyLength)
            {
                session.Server.Log(LogType.Error, session, "http body too long!");
                session.Dispose();
                return;
            }
            if (!token.WebSocket)
            {
                OnHttpDecode(session, pstream);
            }
            else
            {
                OnWebSocketDecode(session, pstream);
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
            StaticResurce.FileBlock fb = data as StaticResurce.FileBlock;
            if (fb != null)
            {
                fb.Write(pstream);
            }
            else
            {
                WebSockets.DataFrame dataPacket = data as WebSockets.DataFrame;
                if (dataPacket != null)
                {
                    dataPacket.Write(pstream);
                }
                else
                {
                    HttpResponse response = (HttpResponse)data;
                    response.Write(pstream);
                }
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
