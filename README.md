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

## 更新日志
#### 2018-10-30
修改路由处理，优化处理性能，添加和Go iris，core Kestrel和asp.net core mvc性能对比测试用例
#### 2018-10-23(FastHttpApi)
重写控制器参数绑定功能，支持json,x-www-form-urlencoded和自定义流读取，默认是JSON。修改javascript生成插件，把控制器注释转议到javascript下并提供vs提示支持。
#### 2018-10-17
新增IResult接口，所有Response输出内容都基于接口实现，方便扩展自定义输出内容；新增一些http requesting,http notfound事件，方便重定向一些内容; 把集成的管理模块移出，通过扩展组件的方式集成并集成文件管理；
#### 2018-10-13
服务监控后台增加连接数限制，日志等级设置，并可提供在线查看当前服务运行的日志
#### 2018-10-11
修复websocket在大数据包情况的问题，修改静态资源缓存配置，对不缓存资源也启用GZIP输出
#### 2018-10-8
增加javascript脚本，无缝兼容ajax和websocket,添加VS插件自动为控制器生成调用的javascript脚本.把samples改用vuejs整合
#### 2018-10-2
优化一下控制器数据协议，以更于更方便地给websocket和ajax调用，增加一个后台管理端可查看服务基础信息和API调用信息
#### 2018-9-25
增加对websocket的支持，同一控制器支持ajax和websocket访问
#### 2018-9-12
新增Debug模式设置，在Debug模式下静态资源目录指向项目文件目录，便于修改和调试
#### 2018-9-20
支持静态资源嵌入程序集中，并支持url访问；实现Cookie的读写
#### 2018-9-18
增加对静态资源支持，默认对html,css,js文件进行变更缓存管理和gzip压缩处理
## 稳定性测试
#### 10000k次连接创建和断开的QPS测试,运行和运行后的资源情况
![](https://i.imgur.com/u1cynsb.png)
#### 100000k次基于长连接的QPS测试,运行和运行后的资源情况
![](https://i.imgur.com/NkY6plh.png)

