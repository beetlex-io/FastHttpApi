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
        public CHeaderAttribute()
        {

        }
        public CHeaderAttribute(string name)
        {
            Name = name;
        }
        public CHeaderAttribute(string name, string value)
        {
            Name = name;
            Value = value;
        }
        public string Name { get; set; }

        public string Value { get; set; }
    }
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Method | AttributeTargets.Interface, AllowMultiple = true)]
    public class CQueryAttribute : Attribute
    {
        public CQueryAttribute()
        {

        }

        public CQueryAttribute(string name)
        {
            Name = name;
        }
        public CQueryAttribute(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; set; }

        public string Value { get; set; }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RequestMaxRPS : Attribute
    {
        public RequestMaxRPS(int value)
        {
            Value = value;
        }

        public int Value { get; set; }
    }
}
