using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Yansoft.Rest
{
    public class AuthorizationErrorEventArgs : EventArgs
    {
        public HttpRequestMessage Request { get; private set; }

        public HttpResponseMessage Response { get; private set; }

        public AuthorizationErrorEventArgs(HttpRequestMessage request, HttpResponseMessage response)
        {
            Request = request;
            Response = response;
        }
    }
}
