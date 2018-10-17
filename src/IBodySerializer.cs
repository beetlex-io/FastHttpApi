using BeetleX.Buffers;
using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi
{
    public interface IBodyProcessHandler
    {
        object GetRequestBody(HttpRequest request, PipeStream stream, int length, string name, Type type);

        IResult CreateResponseResult(HttpResponse response, object data);
    }
}
