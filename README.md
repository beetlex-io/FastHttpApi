`BeetleX.FastHttpApi` is a lightweight and high-performance HTTP service component in the dotnet core platform that supports WebSocket and SSL.

<a href="https://www.nuget.org/packages/BeetleX.FastHttpApi/" target="_blank"> 
    <img src="https://img.shields.io/nuget/vpre/beetlex.fasthttpapi?label=FastHttpApi"> 
							  <img src="https://img.shields.io/nuget/dt/BeetleX.FastHttpApi">
							  </a>
                              
## samples
[**[https://github.com/beetlex-io/BeetleX-Samples](https://github.com/beetlex-io/BeetleX-Samples)**]


## Web Framework Benchmarks
[Round 20](https://www.techempower.com/benchmarks/#section=data-r20&hw=ph&test=composite)
![benchmarks-round20](https://user-images.githubusercontent.com/2564178/107942248-eec41380-6fc5-11eb-94e4-410cadc8ae13.png)

## Using

### Install BeetleX.FastHttpApi
`Install-Package BeetleX.FastHttpApi`

### Base sample code
``` csharp
    [Controller]
    class Program
    {
        private static BeetleX.FastHttpApi.HttpApiServer mApiServer;
        static void Main(string[] args)
        {
            mApiServer = new BeetleX.FastHttpApi.HttpApiServer();
            mApiServer.Options.LogLevel = BeetleX.EventArgs.LogType.Trace;
            mApiServer.Options.LogToConsole = true;
            mApiServer.Debug();//set view path with vs project folder
            mApiServer.Register(typeof(Program).Assembly);
            //mApiServer.Options.Port=80; set listen port to 80
            mApiServer.Open();//default listen port 9090  
            Console.Write(mApiServer.BaseServer);
            Console.Read();
        }
        // Get /hello?name=henry 
        // or
        // Get /hello/henry
        [Get(Route="{name}")]
        public object Hello(string name)
        {
            return $"hello {name} {DateTime.Now}";
        }
        // Get /GetTime  
        public object GetTime()
        {
            return DateTime.Now;
        }
    }
```
### Url Map
``` csharp
mApiServer.Map("/", (ctx) =>
{
    ctx.Result(new TextResult("map /"));
});

mApiServer.Map("/user/{id}", async (ctx) =>
{
    ctx.Result(new TextResult((string)ctx.Data["id"]));
});
```

### Url rewrite
``` csharp
mApiServer.UrlRewrite.Add("/api/PostStream/{code}/{datacode}", "/api/PostStream");
mApiServer.UrlRewrite.Add("/api/PostStream/{code}", "/api/PostStream");
mApiServer.UrlRewrite.Add(null, "/gettime", "/time", null);
```

### Action route
``` csharp
[RouteMap("/map/{code}")]
[RouteMap("/map/{code}/{customer}")]
public object Map(string code, string customer)
{
    return new { code, customer };
}
```

### Hosting and DI services
`Install-Package BeetleX.FastHttpApi.Hosting`
``` csharp
    public class Program
    {
        static void Main(string[] args)
        {
            HttpServer host = new HttpServer(80);
            host.UseTLS("test.pfx", "123456");
            host.Setting((service, option) =>
            {
                service.AddTransient<UserInfo>();
                option.LogToConsole = true;
                option.LogLevel = BeetleX.EventArgs.LogType.Info;
            });
            host.Completed(server =>
            {

            });
            host.RegisterComponent<Program>();
            host.Run();
        }
    }

    [Controller]
    public class Home
    {
        public Home(UserInfo user)
        {
            mUser = user;
        }

        public object Hello()
        {
            return mUser.Name;
        }

        private UserInfo mUser;
    }

    public class UserInfo
    {
        public string Name { get; set; } = "admin";
    }
```
### EntityFrameworkCore extensions
`BeetleX.FastHttpApi.EFCore.Extension `
``` csharp
class Program
{
    static void Main(string[] args)
    {
        HttpApiServer server = new HttpApiServer();
        server.AddEFCoreDB<NorthwindEFCoreSqlite.NorthwindContext>();
        server.Options.Port = 80;
        server.Options.LogToConsole = true;
        server.Options.LogLevel = EventArgs.LogType.Info;
        server.Options.SetDebug();
        server.Register(typeof(Program).Assembly);
        server.AddExts("woff");
        server.Open();
        Console.Read();
    }
}
[Controller]
public class Webapi
{
    public DBObjectList<Customer> Customers(string name, string country, EFCoreDB<NorthwindContext> db)
    {
        Select<Customer> select = new Select<Customer>();
        if (!string.IsNullOrEmpty(name))
            select &= c => c.CompanyName.StartsWith(name);
        if (!string.IsNullOrEmpty(country))
            select &= c => c.Country == country;
        select.OrderBy(c => c.CompanyName.ASC());
        return (db.DBContext, select);
    }

    [Transaction]
    public void DeleteCustomer(string customer, EFCoreDB<NorthwindContext> db)
    {
        db.DBContext.Orders.Where(o => o.CustomerID == customer).Delete();
        db.DBContext.Customers.Where(c => c.CustomerID == customer).Delete();
    }

    public DBValueList<string> CustomerCountry(EFCoreDB<NorthwindContext> db)
    {
        SQL sql = "select distinct country from customers";
        return (db.DBContext, sql);
    }
}
```


## Setting https
- HttpConfig.json
```
 "SSL": true,
 "CertificateFile": "you.com.pfx",
 "CertificatePassword": "******",
```
- Code
``` csharp
mApiServer.ServerConfig.SSL=true;
mApiServer.ServerConfig.CertificateFile="you.com.pfx";
mApiServer.ServerConfig.CertificatePassword="******";
```
## Defined result
- Text result
``` csharp
    public class TextResult : ResultBase
    {
        public TextResult(string text)
        {
            Text = text == null ? "" : text;
        }
        public string Text { get; set; }
        public override bool HasBody => true;
        public override void Write(PipeStream stream, HttpResponse response)
        {
            stream.Write(Text);
        }
    }
```
## Download result
``` csharp
        public object DownloadImport(string id)
        {
           
            Dictionary<string, object> result = new Dictionary<string, object>();
            return new DownLoadResult(Newtonsoft.Json.JsonConvert.SerializeObject(result), $"test.json");
        }
```
- Use
``` csharp
        public object plaintext()
        {
            return new TextResult("Hello, World!");
        }
```
## Cookies 
``` csharp
        public object SetCookie(string name, string value, IHttpContext context)
        {
            Console.WriteLine(context.Data);
            context.Response.SetCookie(name, value);
            return $"{DateTime.Now}{name}={value}";
        }

        public string GetCookie(string name, IHttpContext context)
        {
            Console.WriteLine(context.Data);
            return $"{DateTime.Now} {name}= {context.Request.Cookies[name]}";
        }
```
## Header
``` csharp
        public void SetHeader(string token,IHttpContext context)
        {
            context.Response.Header["Token"]=token;
        }

        public string GetHeader(string name, IHttpContext context)
        {
            return  context.Request.Header[name];
        }
```
## Data bind
- Url

`/hello?name=xxx`or`/hello/henry`
``` csharp
        [Get(Route = "{name}")]
        public object Hello(string name, IHttpContext context)
        {
            return $"hello {name} {DateTime.Now}";
        }
```
`/SetValue?id=xxx&value=xxxx`or`/SetValue/xxx-xxx`
``` csharp
        [Get(Route = "{id}-{value}")]
        public object SetValue(string id, string value, IHttpContext context)
        {
            return $"{id}={value} {DateTime.Now}";
        }
```
- Json

`{"name":"xxxx","value":"xxx"}`
``` csharp
        [Post]
        [JsonDataConvert]
        public object Post(string name, string value, IHttpContext context)
        {
            Console.WriteLine(context.Data);
            return $"{name}={value}";
        }
```
or
``` csharp
        [Post]
        [JsonDataConvert]
        public object Post(Property body, IHttpContext context)
        {
            Console.WriteLine(context.Data);
            return $"{body.name}={body.value}";
        }
```
- x-www-form-urlencoded

`name=aaa&value=aaa`
``` csharp
        [Post]
        [FormUrlDataConvert]
        public object PostForm(string name, string value, IHttpContext context)
        {
            Console.WriteLine(context.Data);
            return $"{name}={value}";
        }
```
- multipart/form-data
``` csharp
        [Post]
        [MultiDataConvert]
        public object UploadFile(string remark, IHttpContext context)
        {
            foreach (var file in context.Request.Files)
                using (System.IO.Stream stream = System.IO.File.Create(file.FileName))
                {
                    file.Data.CopyTo(stream);
                }
            return $"{DateTime.Now} {remark} {string.Join(",", (from fs in context.Request.Files select fs.FileName).ToArray())}";
        }
```
- Read stream
``` csharp
        [Post]
        [NoDataConvert]
        public object PostStream(IHttpContext context)
        {
            Console.WriteLine(context.Data);
            string value = context.Request.Stream.ReadString(context.Request.Length);
            return value;
        }
```
## Filter
- Defined filter
``` csharp
    public class GlobalFilter : FilterAttribute
    {
        public override bool Executing(ActionContext context)
        {
            Console.WriteLine(DateTime.Now + " globalFilter execting...");
            return base.Executing(context);
        }
        public override void Executed(ActionContext context)
        {
            base.Executed(context);
            Console.WriteLine(DateTime.Now + " globalFilter executed");
        }
    }
```
- Use
``` csharp
        [CustomFilter]
        public string Hello(string name)
        {
            return DateTime.Now + " hello " + name;
        }
```
or
``` csharp
    [Controller]
    [CustomFilter]
    public class ControllerTest
    {
    
    }
```
- Skip filter
``` csharp
        [SkipFilter(typeof(GlobalFilter))]
        public string Hello(string name)
        {
            return DateTime.Now + " hello " + name;
        }
```
## Parameters validation
``` csharp
public bool Register(
      [StringRegion(Min = 5)]string name,
      [StringRegion(Min = 5)]string pwd,
      [DateRegion(Min = "2019-1-1", Max = "2019-2-1")]DateTime dateTime,
      [EmailFormater]string email,
      [IPFormater]string ipaddress,
      [NumberRegion(Min = 18, Max = 56)]int age,
      [DoubleRegion(Min = 10)]double memory
                  )
        {
           return true;
        }
```
## Async action
``` csharp
        [Get(Route = "{name}")]
        public Task<String> Hello(string name)
        {
            string result = $"hello {name} {DateTime.Now}";
            return Task.FromResult(result);
        }

        public async Task<String> Wait()
        {
            await Task.Delay(2000);
            return $"{DateTime.Now}";
        }
```
## Cross domain
``` csharp
        [Options(AllowOrigin = "www.ikende.com")]
        public string GetTime(IHttpContext context)
        {
            return DateTime.Now.ToShortDateString();
        }
```

## Websocket
- Server
``` csharp
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
        [Get(R"{name}")]
        public object Hello(string name)
        {
            return $"hello {name} {DateTime.Now}";
        }
        // Get /GetTime  
        public object GetTime()
        {
            return DateTime.Now;
        }
    }
```
- Hello

Request json
``` javascript
{
      url: '/Hello', 
      params: { name: 'test' }
}
```
- GetTime

Request json
``` javascript
{
      url: '/GetTime', 
      params: { }
}
```

