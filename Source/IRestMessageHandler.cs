using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Yansoft.Rest
{
    public interface IRestMessageHandler
    {
        Func<HttpRequestMessage, Task<HttpRequestMessage>> AuthenticationHandler { get; set; }
        Func<HttpRequestMessage, HttpResponseMessage, Task<HttpResponseMessage>> ErrorHandler { get; set; }

        event EventHandler<RequestErrorEventArgs> RequestFailed;
    }
}