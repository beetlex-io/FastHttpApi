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

        public ActionResult(object data)
        {
            Code = 200;
            Data = data;
        }

        public ActionResult(int code, string error)
        {
            Code = code;
            Error = error;
        }

        public string Url { get; set; }

        public string Error { get; set; }

        public int Code { get; set; }

        public string StackTrace { get; set; }

        public object Data { get; set; }

        public string ID { get; set; }

    }
}
