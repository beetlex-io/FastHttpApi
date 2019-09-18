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

        public HttpPacket(HttpApiServer server, IDataFrameSerializer dataPacketSerializer)
        {
            mServerConfig = server.Options;
            mDataPacketSerializer = dataPacketSerializer;
            mServer = server;
        }

        public EventHandler<PacketDecodeCompletedEventArgs> Completed { get; set; }

        public IPacket Clone()
        {
            return new HttpPacket(mServer, this.mDataPacketSerializer);
        }

        private HttpOptions mServerConfig;

        private HttpApiServer mServer;

        private PacketDecodeCompletedEventArgs mCompletedArgs = new PacketDecodeCompletedEventArgs();

        private HttpRequest mRequest;

        private long mWebSocketRequest;

        private long mLastTime;

        private int mReceives = 0;

        private DataFrame mDataPacket;

        private WebSockets.IDataFrameSerializer mDataPacketSerializer;

        private void OnHttpDecode(ISession session, PipeStream pstream)
        {
            mReceives++;
            if (mRequest == null)
            {
                mRequest = mServer.CreateRequest(session);
            }
            if (mRequest.Read(pstream) == LoadedState.Completed)
            {
                try
                {
                    Completed?.Invoke(this, mCompletedArgs.SetInfo(session, mRequest));
                }
                finally
                {
                    mRequest = null;
                    mReceives = 0;
                }
                return;
            }
            else
            {
                if (session.Server.EnableLog(LogType.Info))
                    session.Server.Log(LogType.Info, session, $"{session.RemoteEndPoint} Multi receive to http request");
                if (mRequest.State == LoadedState.None)
                {
                    if (mReceives > 2 || pstream.FirstBuffer.Length < 32)
                    {
                        if (session.Server.EnableLog(LogType.Warring))
                        {
                            session.Server.Log(LogType.Warring, session, "{0} http receive data error!", session.RemoteEndPoint);
                        }
                        session.Dispose();
                        return;
                    }
                    var span = pstream.FirstBuffer.Memory.Slice(0, 10).Span;
                    var method = Encoding.ASCII.GetString(span.ToArray());
                    bool ismethod = method.IndexOf("GET") == 0 || method.IndexOf("POST") == 0 || method.IndexOf("HEAD") == 0 ||
                        method.IndexOf("PUT") == 0 || method.IndexOf("DELETE") == 0 || method.IndexOf("CONNECT") == 0 || method.IndexOf("OPTIONS") == 0 ||
                        method.IndexOf("TRACE") == 0;
                    if (!ismethod)
                    {
                        if (session.Server.EnableLog(LogType.Warring))
                        {
                            session.Server.Log(LogType.Warring, session, "{0} http protocol data error!", session.RemoteEndPoint);
                        }
                        session.Dispose();
                        return;
                    }
                }
                if ((int)mRequest.State < (int)LoadedState.Header && pstream.Length > 1024 * 4)
                {
                    if (session.Server.EnableLog(LogType.Warring))
                    {
                        session.Server.Log(LogType.Warring, session, "{0} http header too long!", session.RemoteEndPoint);
                    }
                    session.Dispose();
                }
                else if (mRequest.Length > mServerConfig.MaxBodyLength)
                {
                    if (session.Server.EnableLog(LogType.Warring))
                    {
                        session.Server.Log(LogType.Warring, session, "{0} http body too long!", session.RemoteEndPoint);
                    }
                    HttpToken token = (HttpToken)session.Tag;
                    token.KeepAlive = false;
                    var response = mRequest.CreateResponse();
                    InnerErrorResult innerErrorResult = new InnerErrorResult("413", "Request Entity Too Large");
                    response.Result(innerErrorResult);
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
                mWebSocketRequest++;
                long now = session.Server.GetRunTime();
                if (now - mLastTime > 1000)
                {
                    if (mServerConfig.WebSocketMaxRPS > 0 && mWebSocketRequest > mServerConfig.WebSocketMaxRPS)
                    {
                        if (session.Server.EnableLog(LogType.Warring))
                        {
                            session.Server.Log(LogType.Warring, session, "{0} websocket session rps to max!", session.RemoteEndPoint);
                        }
                        session.Dispose();
                    }
                    else
                    {
                        mWebSocketRequest = 0;
                        mLastTime = now;
                    }
                }
                DataFrame data = mDataPacket;
                mDataPacket = null;
                Completed?.Invoke(this, mCompletedArgs.SetInfo(session, data));
                if (pstream.Length > 0)
                    goto START;
            }
            else
            {
                if (pstream.Length > mServerConfig.MaxBodyLength)
                {
                    session.Dispose();
                }
            }
        }

        public void Decode(ISession session, Stream stream)
        {
            HttpToken token = (HttpToken)session.Tag;
            PipeStream pstream = stream.ToPipeStream();
            if (pstream.Length > mServerConfig.MaxBodyLength)
            {
                if (session.Server.EnableLog(LogType.Warring))
                {
                    session.Server.Log(LogType.Warring, session, "{0} http protocol data to long!", session.RemoteEndPoint);
                }
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
            IDataResponse dataResponse = data as IDataResponse;
            if (dataResponse != null)
            {
                dataResponse.Write(pstream);
            }
            else
            {
                if (session.Server.EnableLog(LogType.Error))
                    session.Server.Log(LogType.Error, session, $"{session.RemoteEndPoint} response {data} no impl  IDataResponse");
            }
        }

        public byte[] Encode(object data, IServer server)
        {
            byte[] result = null;
            using (Buffers.PipeStream stream = new PipeStream(server.SendBufferPool.Next(), server.Options.LittleEndian, server.Options.Encoding))
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
            using (Buffers.PipeStream stream = new PipeStream(server.SendBufferPool.Next(), server.Options.LittleEndian, server.Options.Encoding))
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
