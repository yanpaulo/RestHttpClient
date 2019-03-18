using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Yansoft.Rest
{
    public interface IAuthenticator
    {
        void Authenticate(HttpRequestMessage request);

        void OnAuthorizationError(HttpRequestMessage request, HttpResponseMessage response);
    }
}
