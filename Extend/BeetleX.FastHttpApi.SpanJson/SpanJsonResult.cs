using BeetleX.Buffers;
using System;
using System.Collections.Generic;
using System.Text;
using SpanJson;
namespace BeetleX.FastHttpApi.SpanJson
{
    public class SpanJsonResult : ResultBase
    {
        public SpanJsonResult(object data)
        {
            Data = data;
        }

        public object Data { get; set; }

        public override string ContentType => "application/json";

        public override bool HasBody => true;

        public override void Write(PipeStream stream, HttpResponse response)
        {
            using (stream.LockFree())
            {
                var task = JsonSerializer.NonGeneric.Utf8.SerializeAsync(Data, stream).AsTask();
                task.Wait();
            }
        }
    }

    public class SpanJsonResultFilter : FilterAttribute
    {
        public override void Executed(ActionContext context)
        {
            base.Executed(context);
            if (!(context.Result is SpanJsonResult || context.Result is JsonResult))
                context.Result = new SpanJsonResult(context.Result);
        }
    }
}
