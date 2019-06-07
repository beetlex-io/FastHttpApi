using BeetleX.FastHttpApi;
using System;
using BeetleX.FastHttpApi.Validations;
namespace HttpApiServer.ParametersValidation
{

    [BeetleX.FastHttpApi.Controller]
    public class Program
    {

        private static BeetleX.FastHttpApi.HttpApiServer mApiServer;

        static void Main(string[] args)
        {
            mApiServer = new BeetleX.FastHttpApi.HttpApiServer();
            mApiServer.Options.LogToConsole = true;
            mApiServer.Debug();
            mApiServer.Register(typeof(Program).Assembly);
            
            mApiServer.Open();
            Console.Write(mApiServer.BaseServer);
            Console.Read();
        }

        public bool Login(
            [StringRegion(Min = 6)]string name, 
            [StringRegion(Min = 6)] string pwd)
        {
            return true;
        }
        [Post]
        public bool Register(
            [StringRegion(Min = 6)]string name,
            [StringRegion(Min = 6)]string pwd,
            [EmailFormater]string email,
            [MPhoneFormater]string phone,
            [UrlFormater]string homePage)
        {
            return true;
        }
    }
}
