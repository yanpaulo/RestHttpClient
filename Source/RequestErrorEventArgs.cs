using System;
using System.Net.Http;

namespace Yansoft.Rest
{
    public class RequestErrorEventArgs : EventArgs
    {
        public HttpRequestMessage Request { get; private set; }

        public HttpResponseMessage Response { get; private set; }

        public RequestErrorEventArgs(HttpRequestMessage request, HttpResponseMessage response)
        {
            Request = request;
            Response = response;
        }
    }
}
