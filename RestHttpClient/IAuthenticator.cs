using System.Net.Http;

namespace Yansoft.Rest
{
    public interface IAuthenticator
    {
        void Authenticate(HttpRequestMessage request);

        void OnAuthorizationError(HttpRequestMessage request, HttpResponseMessage response);
    }
}
