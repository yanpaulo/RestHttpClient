using System;
using System.Net.Http;

namespace Yansoft.Rest
{
    public class RestException : Exception
    {
        public string Content { get; set; }

        public HttpRequestMessage Request { get; }

        public HttpResponseMessage Response { get; set; }

        public RestException(HttpRequestMessage request, HttpResponseMessage response, string content) : base($"Error requesting URL ({response.RequestMessage.RequestUri})")
        {
            Request = request;
            Response = response;
            Content = content;
        }


        public RestException(string message, HttpRequestMessage request, Exception innerException) : base(message, innerException) { }
    }
}
