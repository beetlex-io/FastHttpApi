# FastHttpApi

## 简介
是dotNet core下基于[Beetlex](https://github.com/IKende/BeetleX)实现的一个高度精简化和高吞吐的HTTP API服务组件，它并没有完全实现HTTP SERVER的所有功能，而是只实现了在APP和WEB中提供数据服务最常用两个指令GET/SET，满足在应用实现JSON,PROTOBUF和MSGPACK等基于HTTP的数据交互功能，虽然是一个精简版本但针对SSL这方面的安全性还是支持。有牺牲就必然有收获，FastHttpApi作出这么大的精简必然在性能上有所收获取，经测试FastHttpApi在GET/POST这些数据交互的场景下性能和吞吐能力是Asp.net core集成的Kestrel的2倍以上。
 
 **[功能介绍和使用](https://github.com/IKende/FastHttpApi/wiki/%E5%8A%9F%E8%83%BD%E4%BB%8B%E7%BB%8D%E5%92%8C%E4%BD%BF%E7%94%A8)**

## 更新日志
#### 2018-9-25 添加对websocket协议的支持
在新版本中一个端口服务下默认同时支持HTTP和WebSocket协议，所定义的控制器方法同时支持两种协议方式调用
#### 2018-9-21 增加Debug方式
由于在开发过程修改静态资源需要编译导致调试麻烦，在Debug模式下资源目录会指向项目的资源目录，修改静态资源不需要重新编译项目。
#### 2018-9-20 支持静态资源嵌入到程序中，支持Cookies。
可以把静态资源打包到程序中，组件支持URL直接访问嵌入的静态资源，实现对Cookies的读写。
#### 2018-9-18 增加了对静态资源的支持，优化了Header写入的性能。
对Image类静态资源加入本地缓存标记；对HTML,CSS,JS资源加入了缓存修改标签，方便文件变更后刷新缓存；对HTML,CSS,JS等文件使用GZIP输出。
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
