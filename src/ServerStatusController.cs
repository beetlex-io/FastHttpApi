using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi
{
    [Controller(BaseUrl = "/")]
    public class ServerStatusController
    {
        [DefaultJsonResultFilter]
        public object __ServerStatus(IHttpContext context)
        {
            if (context.Server.ServerCounter != null)
            {
                return context.Server.ServerCounter.Next();
            }
            return new ServerCounter.ServerStatus();
        }
    }
}
