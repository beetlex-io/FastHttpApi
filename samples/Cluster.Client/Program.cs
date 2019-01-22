using System;
using System.Threading.Tasks;
using BeetleX.FastHttpApi;
namespace Cluster.Client
{
    class Program
    {
        static BeetleX.FastHttpApi.Clients.HttpClusterApi HttpClusterApi;
        static IDataService DataService;
        static void Main(string[] args)
        {
            HttpClusterApi = new BeetleX.FastHttpApi.Clients.HttpClusterApi();
            DataService = HttpClusterApi.Create<IDataService>();
            Test();
            while (true)
            {
                Console.Write(HttpClusterApi.Status());
                System.Threading.Thread.Sleep(1000);
            }
        }

        static async void Test()
        {
            await HttpClusterApi.LoadNodeSource("default", "http://localhost:8080");
            int threads = 40;
            for (int i = 0; i < threads; i++)
            {
                System.Threading.ThreadPool.QueueUserWorkItem(o => { Hello(); });
                System.Threading.Thread.Sleep(1000);
            }

        }
        static void Hello()
        {
            while (true)
            {
                try
                {
                    var result = DataService.Hello("henry");
                }
                catch (Exception e_)
                {
                    Console.WriteLine(e_.Message);
                }


            }
        }
    }

    public interface IDataService
    {
        [Get(Route = "hello/{name}")]
        string Hello(string name);
    }
}
