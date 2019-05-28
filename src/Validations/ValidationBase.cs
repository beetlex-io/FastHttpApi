using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi.Validations
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = true)]
    public abstract class ValidationBase : Attribute
    {

        public bool Non { get; set; } = true;

        public int Code { get; set; } = 412;

        public virtual string GetResultMessage(string parameter)
        {

            return "";
        }

        public virtual bool Execute(object data)
        {
            return Non && data != null;
        }
    }

    public class Non : ValidationBase
    {
        public override string GetResultMessage(string parameter)
        {
            return $"The '{parameter}' parameter value cannot be null!";
        }
    }



}
