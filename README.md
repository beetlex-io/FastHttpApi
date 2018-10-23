# FastHttpApi

## 简介
是dotNet core下基于[Beetlex](https://github.com/IKende/BeetleX)实现的一个高度精简化和高吞吐支持双协议的组件，组件在同一端口服务下同时支持http和WebSocket协议；通过组件定义的web api同样支持http和websocket调用。组件除了支持HTTP/Websocket还提供静态网页资源输出，从而可以在组件的基础上直接构建网站应用.在安全性方面组件支持SSL，只需要简单配置证书文件即可让服务支持https的安全性访问。 在性能上经测试FastHttpApi在GET/POST这些数据交互的场景下性能和吞吐能力是Asp.net core集成的Kestrel的2倍以上。

1. 支持以函数的方式来制定HTTP请求逻辑
2. 支持使用者处理异步响应
3. 同一端口下同时支持http/websocket协议
4. 支持Filter功能，以便更好地控制请求方法的处理
5. 支持自定义Http Body解释器，方便制定基于json,xml,protobuf,msgpack等数据格式的传输
6. 支持QueryString参数和Cookies
7. 支持外置或内嵌到DLL的静态资源输出（默认对html,js,css资源进行GZIP处理）
8. 支持SSL和Https

**[详情查看官网](http://www.ikende.com/)**

## 更新日志
#### 2018-10-23(FastHttpApi)
重写控制器参数绑定功能，支持json,x-www-form-urlencoded和自定义流读取，默认是JSON。修改javascript生成插件，把控制器注释转议到javascript下并提供vs提示支持。
#### 2018-10-17(FastHttpApi)
新增IResult接口，所有Response输出内容都基于接口实现，方便扩展自定义输出内容；新增一些http requesting,http notfound事件，方便重定向一些内容; 把集成的管理模块移出，通过扩展组件的方式集成并集成文件管理；
#### 2018-10-13(FastHttpApi)
服务监控后台增加连接数限制，日志等级设置，并可提供在线查看当前服务运行的日志
#### 2018-10-11(FastHttpApi)
修复websocket在大数据包情况的问题，修改静态资源缓存配置，对不缓存资源也启用GZIP输出
#### 2018-10-8(FastHttpApi)
增加javascript脚本，无缝兼容ajax和websocket,添加VS插件自动为控制器生成调用的javascript脚本.把samples改用vuejs整合
#### 2018-10-2(FastHttpApi)
优化一下控制器数据协议，以更于更方便地给websocket和ajax调用，增加一个后台管理端可查看服务基础信息和API调用信息
#### 2018-9-25(FastHttpApi)
增加对websocket的支持，同一控制器支持ajax和websocket访问
#### 2018-9-12(FastHttpApi)
新增Debug模式设置，在Debug模式下静态资源目录指向项目文件目录，便于修改和调试
#### 2018-9-20(FastHttpApi)
支持静态资源嵌入程序集中，并支持url访问；实现Cookie的读写
#### 2018-9-18(FastHttpApi)
增加对静态资源支持，默认对html,css,js文件进行变更缓存管理和gzip压缩处理
## 稳定性测试
#### 10000k次连接创建和断开的QPS测试,运行和运行后的资源情况
![](https://i.imgur.com/u1cynsb.png)
#### 100000k次基于长连接的QPS测试,运行和运行后的资源情况
![](https://i.imgur.com/NkY6plh.png)
## 性能对比测试
由于dotnet core下面没有其他简化的http api组件，只能拿Kestrel asp.net core来作对比，虽然对asp.net core不公平，但这样的对比测也只是为了体现简化后的性能回报；测试服务器是阿里云的4核虚拟机，8G内存,测试工具是AB，测试功能主要是针对GET/POST的json数据处理。由于Kestrel asp.net core默认不支持AB的Keep-Alive选项，所以测试结果就并没有针对asp.net core的Keep-Alive测试
##### Kestrel asp.net core代码
```
        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult Get(int id)
        {
            return new JsonResult(Employee.List(id));
        }
        // POST api/values
        [HttpPost]
        public ActionResult Post([FromBody] Employee value)
        {
            return new JsonResult(value);
        }
```
##### FastHttpApi 代码
```
        // /listemployee?count
        public IList<Employee> ListEmployee(int count)
        {
            return Employee.List(count);
        }
        // post /AddEmployee 
        public Employee AddEmployee([BodyParameter]Employee item)
        {
            return item;
        }
```
##### Kestrel asp.net core GET测试结果
![](https://i.imgur.com/xQ6XeF2.png) 
##### FastHttpApi GET测试结果
![](https://i.imgur.com/ssFiLPp.png) 
##### FastHttpApi GET测试结果开启Keep-Alive
![](https://i.imgur.com/Moh3UvX.png) 
##### Kestrel asp.net core POST测试结果
![](https://i.imgur.com/lmYg41g.png) 
##### FastHttpApi POST测试结果
![](https://i.imgur.com/DTSoOLy.png) 
##### FastHttpApi POST测试结果开启Keep-Alive
![](https://i.imgur.com/utZFsdu.png) 

### 针对Kestrel的对比测试
对比一下两者在accept connection上的性能差异，开启了两个AB实例进行压测
##### Kestrel代码
```
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.Run(context =>
            {
                byte[] data = System.Text.Encoding.UTF8.GetBytes(DateTime.Now.ToString());
                return context.Response.Body.WriteAsync(data, 0, data.Length);
            });
        }
```
##### FastHttpApi代码
```
        //  /hello?name=
        public string Hello(string name)
        {
            return DateTime.Now + " hello " + name;
        }
```
##### Kestrel测试结果
![](https://i.imgur.com/f44TLci.png) 
##### FastHttpApi测试结果
![](https://i.imgur.com/CqClp6e.png) 
