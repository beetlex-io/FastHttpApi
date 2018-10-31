### 简介
是dotNet core下基于[Beetlex](https://github.com/IKende/BeetleX)实现的一个高度精简化和高吞吐支持双协议的组件，组件在同一端口服务下同时支持http和WebSocket协议；通过组件定义的web api同样支持http和websocket调用。组件除了支持HTTP/Websocket还提供静态网页资源输出，从而可以在组件的基础上直接构建网站应用.在安全性方面组件支持SSL，只需要简单配置证书文件即可让服务支持https的安全性访问。 在性能上经测试FastHttpApi在GET/POST这些数据交互的场景下性能和吞吐能力都优胜于是Asp.net core集成的Kestrel！**[详情查看官网](http://www.ikende.com/)**.
#### 性能对比概要
**[详细测试代码master/PerformanceTest](https://github.com/IKende/FastHttpApi/tree/master/PerformanceTest)**
![](https://i.imgur.com/A4hYksO.png)

### 安装组件

```
Install-Package BeetleX.FastHttpApi -Version 0.9.9.7
```
### 项目设置ServerGC
`<ServerGarbageCollection>true</ServerGarbageCollection>`

```
    [Controller]
    class Program
    {
        private static BeetleX.FastHttpApi.HttpApiServer mApiServer;
        static void Main(string[] args)
        {
            mApiServer = new BeetleX.FastHttpApi.HttpApiServer();
            mApiServer.Debug();
            mApiServer.Register(typeof(Program).Assembly);
            mApiServer.Open();
            Console.Write(mApiServer.BaseServer);
            Console.Read();
        }
        // Get /hello?name=henry 
        // or
        // Get /hello/henry
        [RouteTemplate("{name}")]
        public object Hello(string name)
        {
            return $"hello {name} {DateTime.Now}";
        }
        // Get /GetTime  
        public object GetTime()
        {
            return DateTime.Now;
        }
        // Post /PostStream
        // name=aaa&value=bbb
        [Post]
        [NoDataConvert]
        public object PostStream(IHttpContext context)
        {
            Console.WriteLine(context.Data);
            string value = context.Request.Stream.ReadString(context.Request.Length);
            return value;
        }
        // Post /Post
        // {"name":"henry","value":"bbbb"}
        [Post]
        public object Post(string name, string value, IHttpContext context)
        {
            Console.WriteLine(context.Data);
            return $"{name}={value}";
        }
        
        // Post /PostForm
        // name=aaa&value=bbb
        [Post]
        [FormUrlDataConvert]
        public object PostForm(string name, string value, IHttpContext context)
        {
            Console.WriteLine(context.Data);
            return $"{name}={value}";
        }
    }
```

