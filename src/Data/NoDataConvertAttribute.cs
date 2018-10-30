using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi.Data
{
    [AttributeUsage(AttributeTargets.Method)]
    public class NoDataConvertAttribute : Attribute
    {
    }
}
