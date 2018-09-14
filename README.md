# FastHttpApi
## 简介
是基于dotcore实现的一个高度精简化和高吞吐的HTTP API服务组件，它并没有完全实现HTTP SERVER的所有功能，而是只实现了在APP和WEB中提供数据服务最常用两个指令GET/SET，满足在应用实现JSON,PROTOBUF和MSGPACK等基于HTTP的数据交互功能，虽然是一个精简版本但针对SSL这方面的安全性还是支持。有牺牲就必然有收获，FastHttpApi作出这么大的精简必然在性能上有所收获取，经测试FastHttpApi在GET/POST这些数据交互的场景在性能是Asp.net core集成的Kestrel吞吐能力提高可达到1.5倍以上。
## 使用便利性
FastHttpApi虽然在HTTP方面作了大量的精简，但并没有为此增加了它使用的复杂度。FastHttpApi具备asp.net core webapi的便利性；应用人员只需要制定和webapi一样的方法即可，在使用过程中和写普通逻辑方法没有多大的区别。
##### 定义一个控制器
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
![](http://m.qpic.cn/psb?/V10qzUzd2wklm2/VuvT0BKCBF*N1Z8pSSxW08iiQ9H6PD*QYdT2wbupO2A!/b/dDQBAAAAAAAA&bo=TAIXAQAAAAADF2o!&rf=viewer_4) 
##### FastHttpApi GET测试结果
![](http://m.qpic.cn/psb?/V10qzUzd2wklm2/quX*F8j.xsapnno96AGGQj8W54lonRnAWbOfrAgOutk!/b/dDQBAAAAAAAA&bo=UQIZAQAAAAADF3k!&rf=viewer_4) 
##### FastHttpApi GET测试结果开启Keep-Alive
![](http://m.qpic.cn/psb?/V10qzUzd2wklm2/aIk2H6Ubjs.yusEAjlvoKUOoERu72RVkihquHvYZ6w4!/b/dDQBAAAAAAAA&bo=TgIzAQAAAAADF0w!&rf=viewer_4) 
##### Kestrel asp.net core POST测试结果
![](http://m.qpic.cn/psb?/V10qzUzd2wklm2/Oa1tQI2xvk*BVAh0FhOJdx43HLNJJwaSKkF5o0DJB44!/b/dDABAAAAAAAA&bo=TgJHAQAAAAADFzg!&rf=viewer_4) 
##### FastHttpApi POST测试结果
![](http://m.qpic.cn/psb?/V10qzUzd2wklm2/79wH0rjFkqdZE.3zok.a2hVwF8ZuDRlTU1oiky631bI!/b/dC0BAAAAAAAA&bo=SQJHAQAAAAADFz8!&rf=viewer_4) 
##### FastHttpApi POST测试结果开启Keep-Alive
![](http://m.qpic.cn/psb?/V10qzUzd2wklm2/mSpgZ6xd2dJ15ivF6xAZBcFq6xyTjM54g5ldrFOGUBA!/b/dDEBAAAAAAAA&bo=TAJXAQAAAAADFyo!&rf=viewer_4) 

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
![](http://m.qpic.cn/psb?/V10qzUzd2wklm2/vGOg5tAvlK2gp39JyPOddKk3KFhwm9zf*CwmwauKJYQ!/b/dDcBAAAAAAAA&bo=4AMhAQAAAAADF*E!&rf=viewer_4) 
##### FastHttpApi测试结果
![](http://m.qpic.cn/psb?/V10qzUzd2wklm2/f7X9I9QuKiW5WGF1LoYZehEcTCir3iC5Cgcu4QnOPY8!/b/dDYBAAAAAAAA&bo=ygMhAQAAAAADF9s!&rf=viewer_4) 
