# FastHttpApi
## 更新日志
#### 2018-9-18 增加了对静态资源的支持，优化了Header写入的性能。
对Image类静态资源加入本地缓存标记；对HTML,CSS,JS资源加入了缓存修改标签，方便文件变更后刷新缓存；对HTML,CSS,JS等文件使用GZIP输出。
## 简介
是dotNet core下基于[Beetlex](https://github.com/IKende/BeetleX)实现的一个高度精简化和高吞吐的HTTP API服务组件，它并没有完全实现HTTP SERVER的所有功能，而是只实现了在APP和WEB中提供数据服务最常用两个指令GET/SET，满足在应用实现JSON,PROTOBUF和MSGPACK等基于HTTP的数据交互功能，虽然是一个精简版本但针对SSL这方面的安全性还是支持。有牺牲就必然有收获，FastHttpApi作出这么大的精简必然在性能上有所收获取，经测试FastHttpApi在GET/POST这些数据交互的场景下性能和吞吐能力是Asp.net core集成的Kestrel的2倍以上。
## 使用便利性
FastHttpApi虽然在HTTP方面作了大量的精简，但并没有为此增加了它使用的复杂度。FastHttpApi具备asp.net core webapi的便利性；应用人员只需要制定和webapi一样的方法即可，在使用过程中和写普通逻辑方法没有多大的区别。
##### 定义一个控制器
控制器用来定义具体相应URL处理的方法，只需要在类上定义Controller属性即可把类中的Public方法提供给Http访问;方法参数来源于QueryString,当参数标记为BodyParameter的时候参数来源于Http Body.
```
    [Controller]
    public class ControllerTest
    {
        //  /hello?name=
        public string Hello(string name)
        {
            return DateTime.Now + " hello " + name;
        }
        // /add?a=&b=
        public string Add(int a, int b)
        {
            return string.Format("{0}+{1}={2}", a, b, a + b);
        }
        // /post?name=
        public object Post(string name, [BodyParameter] UserInfo data)
        {
            return data;
        }
        // /listcustomer?count
        public IList<Customer> ListCustomer(int count)
        {
            return Customer.List(count);
        }
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
    }
```
##### Filter定义
Filter是Controller处理方法的拦载器，通Filter可以对所有方法进行统一拦载处理，如权限日志等。
```
    [Controller]
    [NotFoundFilter]
    public class ControllerTest
    {
        //  /hello?name=
        [SkipFilter(typeof(NotFoundFilter))]
        [CustomFilter]
        public string Hello(string name)
        {
            return DateTime.Now + " hello " + name;
        }
        // /add?a=&b=
        public string Add(int a, int b)
        {
            return string.Format("{0}+{1}={2}", a, b, a + b);
        }
    }
    public class GlobalFilter : FilterAttribute
    {
        public override void Execute(ActionContext context)
        {
            Console.WriteLine(DateTime.Now + " globalFilter execting...");
            context.Execute();
            Console.WriteLine(DateTime.Now + " globalFilter executed");
        }
    }

    public class NotFoundFilter : FilterAttribute
    {
        public override void Execute(ActionContext context)
        {
            Console.WriteLine(DateTime.Now + " NotFoundFilter execting...");
            context.Response.NotFound();
            Console.WriteLine(DateTime.Now + " NotFoundFilter executed");
        }
    }

    public class CustomFilter : FilterAttribute
    {
        public override void Execute(ActionContext context)
        {
            Console.WriteLine(DateTime.Now + " CustomFilter execting...");
            context.Execute();
            Console.WriteLine(DateTime.Now + " CustomFilter executed");
        }
    }
```
##### 启动服务
```
        static void Main(string[] args)
        {
            mApiServer = new BeetleX.FastHttpApi.HttpApiServer();
            mApiServer.Register(typeof(Program).Assembly);
            mApiServer.ServerConfig.BodySerializer = new BeetleX.FastHttpApi.JsonBodySerializer();
            mApiServer.Open();
            Console.Read();
        }
```
### 制定HTTP Body转换器
转换器是组件最常用的自定义功能，通过它可以实现不同种类的数据格式，如json,protobuf和msgpack等。以下是一个json转换器的实现
```
    public class JsonBodySerializer : IBodySerializer
    {
        public JsonBodySerializer()
        {
            ContentType = "application/json";
        }
        public string ContentType { get; set; }
        public object GetInnerError(Exception e, HttpResponse response, bool outputStackTrace)
        {
            return new ErrorResult { url = response.Request.Url, code = 500, error = e.Message, stackTrace = outputStackTrace? e.StackTrace:null };
        }
        public object GetNotSupport(HttpResponse response)
        {
            return new ErrorResult { url = response.Request.Url, code = 403, error = response.Request.Method + " method type not support" };
        }
        public object GetNotFoundData(HttpResponse response)
        {
            return new ErrorResult { url = response.Request.Url, code = 404 };
        }
        public class ErrorResult
        {
            public string url { get; set; }
            public int code { get; set; }
            public string error { get; set; }
            public string stackTrace { get; set; }
        }
        public virtual int Serialize(PipeStream stream, object data)
        {
            int length = stream.CacheLength;
            string value = Newtonsoft.Json.JsonConvert.SerializeObject(data);
            stream.Write(value);
            return stream.CacheLength - length;
        }
        public virtual bool TryDeserialize(PipeStream stream, int length, Type type, out object data)
        {
            data = null;
            if (stream.Length >= length)
            {
                string value = stream.ReadString(length);
                if (type != null)
                {
                    data = Newtonsoft.Json.JsonConvert.DeserializeObject(value,type);
                }
                else
                {
                    data = Newtonsoft.Json.JsonConvert.DeserializeObject(value);
                }
                return true;
            }
            return false;
        }
    }
```
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
