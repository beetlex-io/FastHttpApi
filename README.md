# FastHttpApi
## 简介
是基于dotcore实现的一个高度精简化和高吞吐的HTTP API服务组件，它并没有完全实现HTTP SERVER的所有功能，而是只实现了在APP和WEB中提供数据服务最常用两个指令GET/SET，满足在应用实现JSON,PROTOBUF和MSGPACK等基于HTTP的数据交互功能，虽然是一个精简版本但针对SSL这方面的安全性还是支持。有牺牲就必然有收获，FastHttpApi作出这么大的精简必然在性能上有所收获取，经测试FastHttpApi在GET/POST这些数据交互的场景在性能上最少是Asp.net core集成的Kestrel吞吐能力的2.5倍以上。
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
转器是组件最常用的自定义功能，通过它可以实现不同种类的数据格式，如json,protobuf和msgpack等。以下是一个json转换器的实现
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
