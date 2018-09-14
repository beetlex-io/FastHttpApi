using System;
using System.Collections.Generic;
using System.Text;
using BeetleX.FastHttpApi;
namespace HttpApiServer.StringBody
{
    [Controller]
    public class ControllerTest
    {
        public string Hello(string name)
        {
            return DateTime.Now + " hello " + name;
        }

        public string Add(int a, int b)
        {
            return string.Format("{0}+{1}={2}", a, b, a + b);
        }

        public string Post(string name, [BodyParameter] string data)
        {
            return string.Format("{0} post {1}", name, data);
        }
    }
}
