using System;
using System.Collections.Generic;
using System.Text;

namespace BeetleX.FastHttpApi
{
    public class ActionResult
    {
        public ActionResult()
        {
            Code = 200;
        }

        public string Url { get; set; }

        public string Error { get; set; }

        public int Code { get; set; }

        public string StackTrace { get; set; }

        public object Data { get; set; }

    }
}
