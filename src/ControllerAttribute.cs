using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    public class ControllerAttribute : Attribute
    {
        public ControllerAttribute()
        {
            SingleInstance = true;
        }
        public string BaseUrl { get; set; }

        public bool SingleInstance { get; set; }
    }


    public interface IBodyFlag { }

    public interface IController
    {
        void Init(HttpApiServer server,string path);
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class NotActionAttribute : Attribute
    {

    }
}
