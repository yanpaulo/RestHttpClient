# RestHttpClient
Simple and flexible REST client built on top of Microsoft's System.Net.HttpClient.

## Usage

```cs
var client = new RestHttpClient
{
    BaseAddress = new Uri("https://jsonplaceholder.typicode.com")
};

var list = await client.RestGetAsync<List<Todo>>("todos");

var todo = await client.RestGetAsync<Todo>("todos/1");

var model = new Todo
{
    Title = "Lorem Ipsum",
    UserId = 1
};

var createdTodo = await client.RestPostAsync<Todo>("todos", model);
```

### Customize serialization
Implement one of `ISerializer`, `IDeserializer`, `IConverter (combination of both)` and set to according property on `RestHttpClient`:

```cs 
var client = new RestHttpClient
{
    BaseAddress = new Uri("https://jsonplaceholder.typicode.com"),
    Converter = new JsonRestConverter()
};
```

### Authorization
For per-object authentication, just set HttpClient's default headers:
```cs 
client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic","QWxhZGRpbjpvcGVuIHNlc2FtZQ==");
```

For per-request authentication, create an instance of `RestHttpMessageHandler` and set its `AuthenticationHandler` property as follows:
Lambda syntax:
```cs 
var handler = new RestHttpMessageHandler();
{
    AuthenticationHandler = async (request) =>
    {
        request.Headers.Add("Authorization", "Basic QWxhZGRpbjpvcGVuIHNlc2FtZQ==");
        return request;
    }
};

//Set handler through RestHttpClient's constructor
var client = new RestHttpClient(handler);
```

Regular method syntax:
```cs 
//Declare handler method
async Task<HttpRequestMessage> AuthenticateRequestAsync(HttpRequestMessage request)
{
    request.Headers.Add("Authorization", "Basic QWxhZGRpbjpvcGVuIHNlc2FtZQ==");
    return request;
}

//Somethere else:
var handler = new RestHttpMessageHandler
{
    AuthenticationHandler = AuthenticateRequestAsync
};

//Set handler through RestHttpClient's constructor
var client = new RestHttpClient(handler);
```

### Error handling
Set a method or expression to `RestHttpMessageHandler`'s `ErrorHandler` property. It will be called in case of error responses:
```cs
var handler = new RestHttpMessageHandler();
var client = new RestHttpClient(handler);
handler.ErrorHandler = async (request, response) =>
{
    //Use request and response object to determine which action to take
    if (request.RequestUri.AbsolutePath.StartsWith("/api") && response.StatusCode == HttpStatusCode.Unauthorized)
    {
        //In this case we set Authentication global header...
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", "QWxhZGRpbjpvcGVuIHNlc2FtZQ==");
        ///... and then return a copy of the request, which will be resent.
        return request.Clone();
    }

    //If you don't wish to resend the request, just return null;
    return null;
};
```
If there isn't an error handler set or if the error handler doesn't return a Request object, `RestException` will be thrown:
```cs
try
{
    var item = await client.RestGetAsync<Todo>("todos/800");
}
//Use RestException's Request, Response or Content properties to determine how to handle the Exception
catch (RestException ex) when (ex.Response?.StatusCode == HttpStatusCode.NotFound)
{
    Console.WriteLine("Content not found.");
}
catch (RestException ex)
{
    Console.WriteLine("Request failed, check out its content: ");
    Console.Write(ex.Content);
    throw;
}
```
Please note that ErrorHandler method won't be called again if the returned request also fails.

### Less common cases
For less common cases, write your request using the `HttpRequestMessage` class and send it throuth one of the available `RestSendAsync` overloads:
```cs
var json = "{ title: 'Lorem Ipsum', userId: 1 }";

var request = new HttpRequestMessage
{
    Method = HttpMethod.Post,
    Content = new StringContent(json, Encoding.UTF8, "application/json"),
    RequestUri = new Uri("todos", UriKind.Relative)
};

var response = await client.RestSendAsync(request);

Assert.True(response.IsSuccessStatusCode);
```