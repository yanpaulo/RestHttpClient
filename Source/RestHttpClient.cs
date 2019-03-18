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
        /// <summary>
        /// Event fired every time a request fails with Unauthorized(401) status code.
        /// </summary>
        public event EventHandler<AuthorizationErrorEventArgs> AuthorizationFailed;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the Authenticator for this instance.
        /// </summary>
        public IAuthenticator Authenticator { get; set; }

        /// <summary>
        /// Gets or sets the Serializer for this instance.
        /// </summary>
        public ISerializer Serializer
        {
            get { return serializer; }
            set { CheckSerializer(value); serializer = value; }
        }

        /// <summary>
        /// Gets or sets the Deserializer for this instance.
        /// </summary>
        public IDeserializer Deserializer
        {
            get { return deserializer; }
            set { CheckSerializer(value); deserializer = value; }
        }

        /// <summary>
        /// Gets or sets the Converter for this instance.
        /// </summary>
        public IConverter Converter
        {
            get { return converter; }
            set { CheckConverter(value); converter = value; }
        }
        #endregion

        #region Quick Methods
        /// <summary>
        /// Sends a GET request to the specified url and returns its content converted by a deserializer.
        /// </summary>
        /// <typeparam name="T">Type of the object to be returned.</typeparam>
        /// <param name="url">Absolute or relative url to send the request to.</param>
        /// <returns>Content returned by the server, serialized as T.</returns>
        public async Task<T> RestGetAsync<T>(string url) =>
            await RestSendAsync<T>(new HttpRequestMessage { Method = HttpMethod.Get, RequestUri = new Uri(url, UriKind.RelativeOrAbsolute) });

        /// <summary>
        /// Sends a POST request to the specified url with its body serialized by a serializer and returns its content converted by a deserializer.
        /// </summary>
        /// <typeparam name="T">Type of the object to be returned.</typeparam>
        /// <param name="url">Absolute or relative url to send the request to.</param>
        /// <param name="content">Content to be serialized and send in the request body.</param>
        /// <returns>Content returned by the server, serialized as T</returns>
        public async Task<T> RestPostAsync<T>(string url, object content) =>
            await RestSendAsync<T>(new HttpRequestMessage { Method = HttpMethod.Post, RequestUri = new Uri(url, UriKind.RelativeOrAbsolute) }, content);

        /// <summary>
        /// Sends a PUT request to the specified url with its body serialized by a serializer and returns its content converted by a deserializer.
        /// </summary>
        /// <typeparam name="T">Type of the object to be returned.</typeparam>
        /// <param name="url">Absolute or relative url to send the request to.</param>
        /// <param name="content">Content to be serialized and send in the request body.</param>
        /// <returns>Content returned by the server, serialized as T</returns>
        public async Task<T> RestPutAsync<T>(string url, object content) =>
            await RestSendAsync<T>(new HttpRequestMessage { Method = HttpMethod.Put, RequestUri = new Uri(url, UriKind.RelativeOrAbsolute) }, content);

        /// <summary>
        /// Sends a DELETE request to the specified url and returns its content converted by a deserializer.
        /// </summary>
        /// <typeparam name="T">Type of the object to be returned.</typeparam>
        /// <param name="url">Absolute or relative url to send the request to.</param>
        public async Task RestDeleteAsync<T>(string url) =>
            await RestSendAsync(new HttpRequestMessage { Method = HttpMethod.Delete, RequestUri = new Uri(url, UriKind.RelativeOrAbsolute) });
        #endregion

        #region RestSendAsync Overloads
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

        /// <summary>
        /// Sends a request specified by the request parameter.
        /// </summary>
        /// <param name="request">HttpRequestMessage instance describing the request.</param>
        /// <param name="authRetry">Specifies wether the request should be retried in case of Unauthorized(401) status code on response.</param>
        /// <returns></returns>
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
        /// <summary>
        /// Method called every time a request fails with Unauthorized(401) status code.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        protected virtual void OnAuthorizationError(HttpRequestMessage request, HttpResponseMessage response)
        {
            AuthorizationFailed?.Invoke(this, new AuthorizationErrorEventArgs(request, response));
            Authenticator?.OnAuthorizationError(request, response);
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
