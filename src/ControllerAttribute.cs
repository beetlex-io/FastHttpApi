using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ControllerAttribute : Attribute
    {
        public ControllerAttribute()
        {
        }
        public string BaseUrl { get; set; }
    }
    public interface IBodyFlag { }
}
