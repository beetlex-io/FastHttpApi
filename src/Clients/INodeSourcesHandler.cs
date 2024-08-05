using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BeetleX.FastHttpApi.Clients
{
    public class NodeSource
    {
        public string Url { get; set; }

        public IApiNode Node { get; set; }
    }

    public interface INodeSourceHandler
    {
        int UpdateTime { get; set; }
        Task<ApiClusterInfo> Load();
    }

    [JsonFormater]
    public interface IHttpSourceApi
    {
        [Get]
        Task<ApiClusterInfo> _GetCluster(string cluster = "default");
    }

    public class HTTPRemoteSourceHandler : INodeSourceHandler
    {
        private HttpClusterApi mHttpClusterApi = new HttpClusterApi();

        private IHttpSourceApi mRemoteSourceApi;

        public HTTPRemoteSourceHandler(string name, params string[] hosts)
        {
            mHttpClusterApi.AddHost("*", hosts);
            mRemoteSourceApi = mHttpClusterApi.Create<IHttpSourceApi>();
            Name = name;
        }

        public string Name { get; set; }

        public int UpdateTime { get; set; } = 5;

        public Task<ApiClusterInfo> Load()
        {
            return mRemoteSourceApi._GetCluster(Name);
        }
    }


    public class ApiClusterInfo
    {
        public ApiClusterInfo()
        {
            Urls = new List<UrlNodeInfo>();
        }

        public string Name { get; set; }

        public string Version { get; set; }

        public List<UrlNodeInfo> Urls { get; set; }
    }

    public class UrlNodeInfo
    {

        public UrlNodeInfo()
        {
            Hosts = new List<UrlHostInfo>();
        }

        public string Name { get; set; }

        public List<UrlHostInfo> Hosts { get; set; }

        public ApiNode GetNode()
        {
            ApiNode result = new ApiNode(Name);
            foreach (var item in Hosts)
            {
                result.Add(item.Name, item.Weight);
            }
            return result;

        }
    }

    public class UrlHostInfo
    {
        public string Name { get; set; }

        public int Weight { get; set; }

        public int MaxRPS { get; set; }
    }
}
