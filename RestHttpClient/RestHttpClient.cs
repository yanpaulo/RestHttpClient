using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace Yansoft.Rest
{
    public class RestHttpClient : HttpClient
    {

        #region Attributes
        private ISerializer serializer;
        private IDeserializer deserializer;
        private IConverter converter = new JsonRestConverter(); 
        #endregion

        #region Events
        public event EventHandler<AuthorizationErrorEventArgs> AuthorizationFailed; 
        #endregion

        #region Properties
        public IAuthenticator Authenticator { get; set; }

        public ISerializer Serializer
        {
            get { return serializer; }
            set { CheckSerializer(value); serializer = value; }
        }

        public IDeserializer Deserializer
        {
            get { return deserializer; }
            set { CheckSerializer(value); deserializer = value; }
        }

        public IConverter Converter
        {
            get { return converter; }
            set { CheckConverter(value); converter = value; }
        } 
        #endregion

        #region RestSendAsync Overloads
        public async Task<T> RestGetAsync<T>(string url) =>
            await RestSendAsync<T>(new HttpRequestMessage { Method = HttpMethod.Get, RequestUri = new Uri(url, UriKind.RelativeOrAbsolute) });

        public async Task<T> RestDeleteAsync<T>(string url) =>
            await RestSendAsync<T>(new HttpRequestMessage { Method = HttpMethod.Delete, RequestUri = new Uri(url, UriKind.RelativeOrAbsolute) });

        public async Task<T> RestPostAsync<T>(string url, object content) =>
            await RestSendAsync<T>(new HttpRequestMessage { Method = HttpMethod.Post, RequestUri = new Uri(url, UriKind.RelativeOrAbsolute) }, content);

        public async Task<T> RestPutAsync<T>(string url, object content) =>
            await RestSendAsync<T>(new HttpRequestMessage { Method = HttpMethod.Put, RequestUri = new Uri(url, UriKind.RelativeOrAbsolute) }, content);

        public async Task<T> RestSendAsync<T>(HttpRequestMessage request) =>
            await RestSendAsync<T>(request, Converter ?? Deserializer);

        public async Task<T> RestSendAsync<T>(HttpRequestMessage request, object content) =>
            await RestSendAsync<T>(request, content, Converter ?? Serializer, Converter ?? Deserializer);

        public async Task<T> RestSendAsync<T>(HttpRequestMessage request, object content, IConverter converter) =>
            await RestSendAsync<T>(request, content, converter, converter); 
        #endregion

        #region RestSendAsync Implementations
        public async Task<T> RestSendAsync<T>(HttpRequestMessage request, IDeserializer deserializer)
        {
            var response = await RestSendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();
            return deserializer.Deserialize<T>(responseContent);
        }

        public async Task<T> RestSendAsync<T>(HttpRequestMessage request, object content, ISerializer serializer, IDeserializer deserializer)
        {
            request.Content = new StringContent(serializer.Serialize(content), serializer.Encoding, serializer.ContentType);

            var response = await RestSendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();
            return deserializer.Deserialize<T>(responseContent);
        }

        public async Task<HttpResponseMessage> RestSendAsync(HttpRequestMessage request, bool authRetry = true)
        {
            try
            {
                Authenticator?.Authenticate(request);
                var response = await SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    return response;
                }
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    OnAuthorizationError(request, response);
                    if (authRetry && Authenticator != null)
                    {
                        return await RestSendAsync(request, false);
                    }
                }
                var content = await response.Content.ReadAsStringAsync();

                throw new RestException(response, content);
            }
            catch (HttpRequestException ex)
            {
                throw new RestException($"Erro ao se conectar ao servidor.", ex);
            }
        }
        #endregion

        #region Event Handlers
        protected virtual void OnAuthorizationError(HttpRequestMessage request, HttpResponseMessage response)
        {
            Authenticator?.OnAuthorizationError(request, response);
            AuthorizationFailed?.Invoke(this, new AuthorizationErrorEventArgs(request, response));
        } 
        #endregion

        #region Utility Methods
        private void CheckSerializer(object value, [CallerMemberName]string memberName = null)
        {
            if (value == null && Converter == null)
            {
                throw new InvalidOperationException($"Cannot set both {memberName} and {nameof(Converter)} to null.");
            }
        }

        private void CheckConverter(object value)
        {
            if (value == null && (Serializer == null || Deserializer == null))
            {
                throw new InvalidOperationException($"Canot set {nameof(Converter)} to null when any of {nameof(Serializer)} or {nameof(Deserializer)} are null.");
            }
        } 
        #endregion
    }
}
