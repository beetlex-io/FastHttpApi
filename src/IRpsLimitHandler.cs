using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi
{
    public interface IRpsLimitHandler
    {
        string ID { get; set; }

        string Name { get; set; }

        bool Check(HttpRequest request, HttpResponse response);
    }
}
