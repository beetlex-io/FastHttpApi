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
            InstanceType = InstanceType.Single;
        }
        public string BaseUrl { get; set; }

        public InstanceType InstanceType { get; set; }

        public string ActorTag { get; set; }

        public bool SkipPublicFilter { get; set; } = false;
    }


    public enum InstanceType
    {
        Single,
        Session,
        None
    }


    public interface IBodyFlag { }

    public interface IController
    {
        void Init(HttpApiServer server, string path);
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class NotActionAttribute : Attribute
    {

    }
}
