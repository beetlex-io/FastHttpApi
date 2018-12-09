using BeetleX.Buffers;
using BeetleX.Clients;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BeetleX.FastHttpApi.Clients
{
    public class Request
    {
        public const string POST = "POST";

        public const string GET = "GET";

        public const string DELETE = "DELETE";

        public const string PUT = "PUT";

        public Request()
        {
            Method = GET;
            this.HttpProtocol = "HTTP/1.1";
        }

        public IClientBodyFormater Formater { get; set; }

        public Dictionary<string, string> QuestryString { get; set; }

        public Header Header { get; set; }

        public string Url { get; set; }

        public string Method { get; set; }

        public string HttpProtocol { get; set; }

        public Response Response { get; set; }

        public Object Body { get; set; }

        public Type BodyType { get; set; }

        public RequestStatus Status { get; set; }

        public IClient Client { get; set; }

        internal void Execute(PipeStream stream)
        {
            var buffer = HttpParse.GetByteBuffer();
            int offset = 0;
            offset += Encoding.ASCII.GetBytes(Method, 0, Method.Length, buffer, offset);
            buffer[offset] = HeaderTypeFactory._SPACE_BYTE;
            offset++;
            offset += Encoding.ASCII.GetBytes(Url, 0, Url.Length, buffer, offset);
            if (QuestryString != null && QuestryString.Count > 0)
            {
                int i = 0;
                foreach (var item in this.QuestryString)
                {
                    string key = item.Key;
                    string value = item.Value;
                    if (string.IsNullOrEmpty(value))
                        continue;
                    value = System.Net.WebUtility.UrlEncode(value);
                    if (i == 0)
                    {
                        buffer[offset] = HeaderTypeFactory._QMARK;
                        offset++;
                    }
                    else
                    {
                        buffer[offset] = HeaderTypeFactory._AND;
                        offset++;
                    }
                    offset += Encoding.ASCII.GetBytes(key, 0, key.Length, buffer, offset);
                    buffer[offset] = HeaderTypeFactory._EQ;
                    offset++;
                    offset += Encoding.ASCII.GetBytes(value, 0, value.Length, buffer, offset);
                    i++;
                }
            }
            buffer[offset] = HeaderTypeFactory._SPACE_BYTE;
            offset++;
            offset += Encoding.ASCII.GetBytes(HttpProtocol, 0, HttpProtocol.Length, buffer, offset);
            buffer[offset] = HeaderTypeFactory._LINE_R;
            offset++;
            buffer[offset] = HeaderTypeFactory._LINE_N;
            offset++;
            stream.Write(buffer, 0, offset);
            if (Header != null)
                Header.Write(stream);
            if (Method == POST || Method == PUT)
            {
                if (Body != null)
                {
                    stream.Write(HeaderTypeFactory.CONTENT_LENGTH_BYTES, 0, 16);
                    MemoryBlockCollection contentLength = stream.Allocate(10);
                    stream.Write(HeaderTypeFactory.TOW_LINE_BYTES, 0, 4);
                    int len = stream.CacheLength;
                    Formater.Serialization(Body, stream);
                    int count = stream.CacheLength - len;
                    contentLength.Full(count.ToString().PadRight(10), stream.Encoding);
                }
                else
                {
                    stream.Write(HeaderTypeFactory.NULL_CONTENT_LENGTH_BYTES, 0, HeaderTypeFactory.NULL_CONTENT_LENGTH_BYTES.Length);
                    stream.Write(HeaderTypeFactory.LINE_BYTES, 0, 2);
                }
            }
            else
            {
                stream.Write(HeaderTypeFactory.LINE_BYTES, 0, 2);
            }
        }

        public HttpHost HttpHost { get; set; }

        public Task<Response> Execute()
        {
            AnyCompletionSource<Response> taskCompletionSource = new AnyCompletionSource<Response>();
            OnExecute(taskCompletionSource);
            return taskCompletionSource.Task;
        }

        private async void OnExecute(AnyCompletionSource<Response> taskCompletionSource)
        {
            HttpClient client = null;
            try
            {
                client = HttpHost.Pool.Pop();
                client.RequestCommpletionSource = taskCompletionSource;
                Client = client.Client;
                if (client.Client is AsyncTcpClient)
                {
                    AsyncTcpClient asyncClient = (AsyncTcpClient)client.Client;
                    var a = asyncClient.ReceiveMessage<Response>();
                    if (!a.IsCompleted)
                    {
                        asyncClient.Send(this);
                        Status = RequestStatus.SendCompleted;
                    }
                    Response = await a;
                    if (Response.Exception != null)
                    {
                        Status = RequestStatus.Error;
                        throw Response.Exception;
                    }
                    else
                        Status = RequestStatus.Received;
                }
                else
                {
                    TcpClient syncClient = (TcpClient)client.Client;
                    syncClient.SendMessage(this);
                    Status = RequestStatus.SendCompleted;
                    Response = syncClient.ReceiveMessage<Response>();
                    if (Response.Exception != null)
                    {
                        Status = RequestStatus.Error;
                        throw Response.Exception;
                    }
                    else
                        Status = RequestStatus.Received;
                }
                int code = int.Parse(Response.Code);
                if (Response.Length > 0)
                {
                    try
                    {
                        if (code < 400)
                            Response.Body = this.Formater.Deserialization(Response.Stream, this.BodyType, Response.Length);
                        else
                            Response.Body = Response.Stream.ReadString(Response.Length);
                    }
                    finally
                    {
                        Response.Stream.ReadFree(Response.Length);
                        if (Response.Chunked)
                            Response.Stream.Dispose();
                        Response.Stream = null;
                    }
                }
                if (!Response.KeepAlive)
                    client.Client.DisConnect();
                if (code >= 400)
                    throw new System.Net.WebException($"{this.Method} {this.Url} {Response.Code} {Response.CodeMsg} {Response.Body}");
                Status = RequestStatus.Completed;
            }
            catch (Exception e_)
            {
                HttpClientException clientException = new HttpClientException(this, HttpHost.Uri, e_.Message, e_);
                if (e_ is System.Net.Sockets.SocketException || e_ is ObjectDisposedException)
                {
                    clientException.SocketError = true;
                }
                Response = new Response { Exception = clientException };
                Status = RequestStatus.Error;
            }
            if (Response.Exception != null)
                HttpHost.AddError(Response.Exception.SocketError);
            else
                HttpHost.AddSuccess();
            Response.Current = Response;
            if (client != null)
                HttpHost.Pool.Push(client);
            await Task.Run(() => taskCompletionSource.TrySetResult(Response));
        }
    }

    public enum RequestStatus
    {
        None,
        SendCompleted,
        Received,
        Completed,
        Error
    }
}
