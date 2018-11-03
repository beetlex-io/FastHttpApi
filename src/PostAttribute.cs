using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi
{
    [AttributeUsage(AttributeTargets.Method)]
    public class PostAttribute : Attribute
    {
        public string Route { get; set; }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class GetAttribute : Attribute
    {
        public string Route { get; set; }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class DelAttribute : Attribute
    {
        public string Route { get; set; }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class PutAttribute : Attribute
    {
        public string Route { get; set; }
    }
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Interface | AttributeTargets.Parameter, AllowMultiple = true)]
    public class CHeaderAttribute : Attribute
    {
        public string Name { get; set; }

        public string Value { get; set; }
    }
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Method | AttributeTargets.Interface, AllowMultiple = true)]
    public class CQueryAttribute : Attribute
    {
        public string Name { get; set; }

        public string Value { get; set; }
    }
}
