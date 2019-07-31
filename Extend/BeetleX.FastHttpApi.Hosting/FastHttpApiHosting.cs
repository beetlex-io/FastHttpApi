using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BeetleX.FastHttpApi.Hosting
{

    public static class FastHttpApiExtension
    {
        public static IServiceCollection UseBeetlexHttp(this IServiceCollection service, Action<HttpOptions> action, params System.Reflection.Assembly[] assemblies)
        {
            HttpSettingHandler settingHandler = new HttpSettingHandler();
            settingHandler.Assemblies = assemblies;
            settingHandler.Options = action;
            settingHandler.Services = service;
            service.AddSingleton<HttpSettingHandler>(settingHandler);
            ServiceCollection services = new ServiceCollection();
            return service.AddHostedService<HttpServer>();
        }
    }

    public class HttpSettingHandler
    {
        public Action<HttpOptions> Options { get; set; }

        public System.Reflection.Assembly[] Assemblies { get; set; }

        public IServiceCollection Services { get; set; }
    }

    public class HttpServer : IHostedService
    {
        public HttpServer(HttpSettingHandler httpSettingHandler)
        {
            mSettingHandler = httpSettingHandler;
        }

        private ServiceCollection mHttpControllerServices = new ServiceCollection();

        private IServiceProvider mHttpControllerServiceProvider;

        private HttpSettingHandler mSettingHandler;

        private HttpApiServer mHttpServer;

        private void InitService()
        {
            ServiceDescriptor[] items = new ServiceDescriptor[mSettingHandler.Services.Count];
            mSettingHandler.Services.CopyTo(items, 0);
            foreach (var item in items)
            {
                mHttpControllerServices.Insert(0, item);
            }
            foreach (Assembly item in mSettingHandler.Assemblies)
            {
                Type[] types = item.GetTypes();
                foreach (Type type in types)
                {
                    ControllerAttribute ca = type.GetCustomAttribute<ControllerAttribute>(false);
                    if (ca != null)
                    {
                        if (ca.SingleInstance)
                        {
                            mHttpControllerServices.AddSingleton(type);
                        }
                        else
                        {
                            mHttpControllerServices.AddScoped(type);
                        }
                    }
                }
            }
            mHttpControllerServices.AddSingleton(mHttpServer);
            mHttpControllerServiceProvider = mHttpControllerServices.BuildServiceProvider();
            if (mSettingHandler.Assemblies != null)
                mHttpServer.Register(mSettingHandler.Assemblies);
        }

        public virtual Task StartAsync(CancellationToken cancellationToken)
        {
            var option = HttpApiServer.LoadOptions();
            mSettingHandler.Options(option);
            mHttpServer = new HttpApiServer(option);
            mHttpServer.ActionFactory.ControllerInstance += (o, e) =>
            {
                e.Controller = mHttpControllerServiceProvider.GetService(e.Type);
            };
            InitService();
            mHttpServer.Open();
            return Task.CompletedTask;
        }

        public virtual Task StopAsync(CancellationToken cancellationToken)
        {
            mHttpControllerServices.Clear();
            mHttpServer.Dispose();
            return Task.CompletedTask;
        }
    }
}
