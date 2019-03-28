# RestHttpClient
Simple and flexible REST client built on top of Microsoft's System.Net.HttpClient.

## Usage

```cs
var client = new RestHttpClient
{
    BaseAddress = new Uri("https://jsonplaceholder.typicode.com")
};

var list = await client.GetAsync<List<Todo>>("todos");

var todo = await client.GetAsync<Todo>("todos/1");

var model = new Todo
{
    Title = "Lorem Ipsum",
    UserId = 1
};

var createdTodo = await client.PostAsync<Todo>("todos", model);
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

//...and then somethere else:
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
Please note that ErrorHandler method won't be called again if the returned request also fails.

If there isn't an error handler set or if the error handler doesn't return a Request object, `RestException` will be thrown:
```cs
try
{
    var item = await client.GetAsync<Todo>("todos/800");
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


### Less common cases
For less common cases, write your request using the `HttpRequestMessage` class and send it throuth one of the available `SendAsync` overloads:
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
Of course you can always use any of HttpClient's methods for any case.

### RestHttpMessageHandler
As you may have thought, [Authorization](#authorization) and [Error handling](#error-handling) are actually done by RestHttpMessageHandler and not by RestHttpClient, and hence you can also use its capabilities with the regular HttpClient.
Naturally, Authorization and Error handling capabilities won't be present on RestHttpClient if you use an HttpMessageHandler other than RestHttpMessageHandler or devired ones.

### Version 0.5.0
v0.5.0 had those Rest-prefixes before RestHttpClient's specific calls. Some changes have been made and these prefixes don't exist anymore, so just get rid of them in your calls! ;)