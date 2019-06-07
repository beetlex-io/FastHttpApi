using BeetleX.FastHttpApi;
using System;

namespace HttpApiServer.JWT
{
    [Controller]
    class Program
    {
        private static BeetleX.FastHttpApi.HttpApiServer mApiServer;

        public static JWTHelper JWTHelper = new JWTHelper();

        static void Main(string[] args)
        {
            mApiServer = new BeetleX.FastHttpApi.HttpApiServer();
            
            mApiServer.Options.LogLevel = BeetleX.EventArgs.LogType.Trace;
            mApiServer.Options.LogToConsole = true;
            mApiServer.Debug();
            mApiServer.Register(typeof(Program).Assembly);
            mApiServer.Open();
            Console.Write(mApiServer.BaseServer);
            Console.Read();
        }
        public object GetToken(string name, string role)
        {
            return new TextResult(JWTHelper.CreateToken(name, role));
        }
        [JWTFilter]
        public object GetTime()
        {
            return DateTime.Now;
        }
    }

    public class JWTFilter : FilterAttribute
    {
        public override bool Executing(ActionContext context)
        {
            string token = context.HttpContext.Request.Header[HeaderTypeFactory.AUTHORIZATION];
            var user = Program.JWTHelper.GetUserInfo(token);
            if (!string.IsNullOrEmpty(user.Name))
            {
                return true;
            }
            else
            {
                context.Result = new TextResult("token not found");
                return false;
            }
        }
    }
}
