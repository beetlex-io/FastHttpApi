using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi
{
    [AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
    public class AssemblyAutoLoaderAttribute : Attribute
    {
        public AssemblyAutoLoaderAttribute(string id)
        {

        }
    }
}
