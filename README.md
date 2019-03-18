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
Implement one of ISerializer, IDeserializer, IConverter (combination of both) and set to according property on RestHttpClient:

```cs 
    var client = new RestHttpClient
    {
        BaseAddress = new Uri("https://jsonplaceholder.typicode.com"),
        Converter = new JsonRestConverter()
    };
```

### Authorization
Implement IAuthenticator and set it to RestHttpClient.
```cs 
var client = new RestHttpClient
{
    BaseAddress = new Uri("https://jsonplaceholder.typicode.com"),
    Authenticator = new BasicAuthenticator()
};
```

You can also override OnAuthorizationError or add an event handler to AuthorizationFailed event.
Requests failed with Unauthorized(401) status are retried once. Before the retry happens,
OnAuthorizationError, AuthorizationFailed and Authenticator.OnAuthorizationError methods are invoked, in that order, and hence you can use these methods to update RestHttpClient or HttpRequestMessage to ensure that the next request will succeed.
Example:
```cs
var client = new RestHttpClient
{
    BaseAddress = new Uri("https://jsonplaceholder.typicode.com"),
};

client.AuthorizationFailed += (o, e) =>
{
    //Per request
    e.Request.Headers.Add("Authorization", "Basic QWxhZGRpbjpvcGVuIHNlc2FtZQ==");
    //Per instance
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", "QWxhZGRpbjpvcGVuIHNlc2FtZQ==");
};
```
