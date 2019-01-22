using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi.Clients
{
    public class HttpClientException : Exception
    {
        public HttpClientException(Request request, Uri host, string message, Exception innerError = null) : base($"request {host} error {message}", innerError)
        {
            Request = request;
            Host = host;
            SocketError = false;
            if (innerError != null && (innerError is System.Net.Sockets.SocketException || innerError is ObjectDisposedException))
            {
                SocketError = true;
            }
        }
        public Uri Host { get; set; }

        public Request Request { get; set; }

        public bool SocketError { get; internal set; }

    }
}
