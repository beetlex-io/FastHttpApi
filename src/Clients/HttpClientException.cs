using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi.Clients
{
    public class HttpClientException : Exception
    {
        public HttpClientException(Request request, Uri host, string message, Exception innerError = null) : base($"{host} error {message}", innerError)
        {
            Request = request;
            Host = host;
        }
        public Uri Host { get; set; }

        public Request Request { get; set; }

        public bool SocketError { get; internal set; }

    }
}
