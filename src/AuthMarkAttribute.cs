using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthMarkAttribute : Attribute
    {
        public AuthMarkAttribute(AuthMarkType type)
        {
            Type = type;
        }
        public AuthMarkType Type { get; set; }

        public string Token { get; set; }

        public virtual string GetTokenValue(IHttpContext context)
        {
            return Token;
        }
    }
    public enum AuthMarkType
    {
        None = 1,
        User = 2,
        Group = 4,
        Manager = 8,
        Admin = 16,
        System = 32,
        NoValidation = 1024
    }
}
