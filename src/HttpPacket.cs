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

        public string Name => "HTTP-SERVER";

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
                mRequest.ID = HttpRequest.GetID();
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
                HttpToken token = (HttpToken)session.Tag;
                if (session.Server.EnableLog(LogType.Info))
                    session.Server.Log(LogType.Info, session, $"HTTP {mRequest.ID} {session.RemoteEndPoint} request from multi receive");
                if (mRequest.State == LoadedState.None)
                {
                    if (mReceives > 3)
                    {
                        if (session.Server.EnableLog(LogType.Warring))
                        {
                            session.Server.Log(LogType.Warring, session, $"HTTP {mRequest.ID} {session.RemoteEndPoint} receive data error!");
                        }
                        token.KeepAlive = false;
                        var response = mRequest.CreateResponse();
                        //InnerErrorResult innerErrorResult = new InnerErrorResult("400", "Request http receive data error!");
                        //response.Result(innerErrorResult);
                        response.InnerError("400", "request http receive data error!");
                        return;
                    }
                    if (pstream.FirstBuffer.Length > 10)
                    {
                        var span = pstream.FirstBuffer.Memory.Slice(0, 10).Span;
                        var method = Encoding.ASCII.GetString(span.ToArray());
                        bool ismethod = method.IndexOf("GET") == 0 || method.IndexOf("POST") == 0 || method.IndexOf("HEAD") == 0 ||
                            method.IndexOf("PUT") == 0 || method.IndexOf("DELETE") == 0 || method.IndexOf("CONNECT") == 0 || method.IndexOf("OPTIONS") == 0 ||
                            method.IndexOf("TRACE") == 0;
                        if (!ismethod)
                        {
                            if (session.Server.EnableLog(LogType.Warring))
                            {
                                session.Server.Log(LogType.Warring, session, $"HTTP {mRequest.ID} {session.RemoteEndPoint} protocol data error!");
                            }
                            token.KeepAlive = false;
                            var response = mRequest.CreateResponse();
                            //InnerErrorResult innerErrorResult = new InnerErrorResult("400", "Request http protocol data error!");
                            //response.Result(innerErrorResult);
                            response.InnerError("400", "request http protocol data error!");
                            return;
                        }
                    }
                }
                if ((int)mRequest.State < (int)LoadedState.Header && (pstream.Length > 1024 * 4 || mReceives > 20))
                {
                    if (session.Server.EnableLog(LogType.Warring))
                    {
                        session.Server.Log(LogType.Warring, session, $"HTTP {mRequest.ID} {session.RemoteEndPoint} header too long!");
                    }
                    token.KeepAlive = false;
                    var response = mRequest.CreateResponse();
                    //InnerErrorResult innerErrorResult = new InnerErrorResult("400", "Request header too large");
                    //response.Result(innerErrorResult);
                    response.InnerError("400", "request header too large");
                }
                else if (mRequest.Length > mServerConfig.MaxBodyLength)
                {
                    if (session.Server.EnableLog(LogType.Warring))
                    {
                        session.Server.Log(LogType.Warring, session, $"HTTP {mRequest.ID} {session.RemoteEndPoint} body too long!");
                    }
                    token.KeepAlive = false;
                    var response = mRequest.CreateResponse();
                    //InnerErrorResult innerErrorResult = new InnerErrorResult("400", "Request entity too large");
                    //response.Result(innerErrorResult);
                    response.InnerError("400", "request entity too large");
                    return;
                }
                if (mReceives > 10 && pstream.Length < mReceives * 256)
                {
                    if (session.Server.EnableLog(LogType.Warring))
                    {
                        session.Server.Log(LogType.Warring, session, $"HTTP {mRequest.ID} {session.RemoteEndPoint} protocol data commit exception!");
                    }
                    token.KeepAlive = false;
                    var response = mRequest.CreateResponse();
                    //InnerErrorResult innerErrorResult = new InnerErrorResult("400", "protocol data commit exception");
                    //response.Result(innerErrorResult);
                    response.InnerError("400", "protocol data commit exception");
                }
                return;
            }
        }
        private void OnWebSocketDecode(ISession session, PipeStream pstream)
        {
            START:
            if (mDataPacket == null)
            {
                mDataPacket = new DataFrame(mServer);
                mDataPacket.DataPacketSerializer = this.mDataPacketSerializer;
            }
            mReceives++;
            HttpToken token = (HttpToken)session.Tag;
            mDataPacket.Request = token.Request;
            if (mDataPacket.Read(pstream) == DataPacketLoadStep.Completed)
            {
                mWebSocketRequest++;
                long now = session.Server.GetRunTime();
                if (now - mLastTime < 1000)
                {
                    if (mServerConfig.WebSocketSessionMaxRps > 0 && mWebSocketRequest > mServerConfig.WebSocketSessionMaxRps)
                    {
                        if (session.Server.EnableLog(LogType.Warring))
                        {
                            session.Server.Log(LogType.Warring, session, $"Websocket {mRequest?.ID} {session.RemoteEndPoint} session rps limit!");
                        }
                        token.KeepAlive = false;
                        var error = new ActionResult(500, "session rps limit!");
                        var frame = mServer.CreateDataFrame(error);
                        frame.Send(session);
                        //  session.Dispose();
                    }
                }
                else
                {
                    mWebSocketRequest = 0;
                    mLastTime = now;
                }
                DataFrame data = mDataPacket;
                mDataPacket = null;
                Completed?.Invoke(this, mCompletedArgs.SetInfo(session, data));
                mReceives = 0;
                if (pstream.Length > 0)
                    goto START;
            }
            else
            {
                if (pstream.Length > mServerConfig.MaxBodyLength)
                {
                    if (session.Server.EnableLog(LogType.Warring))
                    {
                        session.Server.Log(LogType.Warring, session, $"Websocket {mRequest?.ID} {session.RemoteEndPoint} protocol data is too long!");
                    }
                    session.Dispose();
                }

                if (mReceives > 10 & pstream.Length < mReceives * 512)
                {
                    if (session.Server.EnableLog(LogType.Warring))
                    {
                        session.Server.Log(LogType.Warring, session, $"Websocket {mRequest?.ID} {session.RemoteEndPoint} protocol data commit exception!");
                    }
                    session.Dispose();
                }

            }
        }

        public void Decode(ISession session, Stream stream)
        {
            HttpToken token = (HttpToken)session.Tag;
            try
            {
                PipeStream pstream = stream.ToPipeStream();
                if (pstream.Length > mServerConfig.MaxBodyLength)
                {
                    if (session.Server.EnableLog(LogType.Warring))
                    {
                        session.Server.Log(LogType.Warring, session, $"HTTP {session.RemoteEndPoint} protocol data to long!");
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
            catch (Exception e_)
            {
                if (session.Server.EnableLog(LogType.Warring))
                {
                    if (token.WebSocket)
                    {
                        session.Server.Log(LogType.Warring, session, $"Websocket {session.RemoteEndPoint} protocol decoding error {e_.Message}@{e_.StackTrace}!");
                    }
                    else
                    {
                        session.Server.Log(LogType.Warring, session, $"HTTP {session.RemoteEndPoint} protocol decoding error {e_.Message}@{e_.StackTrace}");
                    }
                }
                session.Dispose();
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
            HttpToken token = (HttpToken)session.Tag;
            if (data is DataFrame df)
                df.Request = token.Request;
            if (dataResponse != null)
            {
                dataResponse.Write(pstream);
            }
            else
            {
                if (session.Server.EnableLog(LogType.Error))
                    session.Server.Log(LogType.Error, session, $"HTTP {session.RemoteEndPoint} response {data} no impl  IDataResponse");
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
