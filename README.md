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

For per-request authentication, set a method or expression returning Task to `AuthenticationHandler` method.
You can use Lambda syntax:
```cs 
client.AuthenticationHandler = async (request) =>
{
    request.Headers.Add("Authorization", "Basic QWxhZGRpbjpvcGVuIHNlc2FtZQ==");
}
```

You can use regular method syntax:
```cs 
async Task AuthenticateRequestAsync(HttpRequestMessage request)
{
    request.Headers.Add("Authorization", "Basic QWxhZGRpbjpvcGVuIHNlc2FtZQ==");
}
//And somethere else:
client.AuthenticationHandler = AuthenticateRequestAsync;
```

### Error handling
Set a method or expression to `ErrorHandler` property:
```cs
client.ErrorHandler = async (request, response) =>
{
    //Use request and response object to determine which action to take
    if (request.RequestUri.AbsolutePath.StartsWith("/api") && response.StatusCode == HttpStatusCode.Unauthorized)
    {
        //In this case we set Authentication global header...
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic","QWxhZGRpbjpvcGVuIHNlc2FtZQ==");
        ///...and then resend a copy of the request, then return its response.
        return await client.SendAsync(request.Clone());
    }

    //If you don't wish to handle an error, just return back the response object
    return response;
};
```

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