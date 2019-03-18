using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Yansoft.Rest
{
    public class RestException : Exception
    {
        public string Content { get; set; }

        public HttpResponseMessage Response { get; set; }

        public RestException(HttpResponseMessage response, string content) : base($"Error requesting URL ({response.RequestMessage.RequestUri})")
        {
            Response = response;
            Content = content;
        }

        public RestException(string message) : base(message) { }

        public RestException(string message, Exception innerException) : base(message, innerException) { }
    }
}
