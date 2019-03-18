using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Yansoft.Rest
{
    public class RestHttpClient : HttpClient
    {

        private ISerializer serializer;
        private IDeserializer deserializer;
        private IConverter converter = new JsonRestConverter();

        public event EventHandler<AuthorizationErrorEventArgs> AuthorizationFailed;

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

        public async Task<T> GetAsync<T>(string url) =>
            await SendAsync<T>(new HttpRequestMessage { Method = HttpMethod.Get, RequestUri = new Uri(url, UriKind.RelativeOrAbsolute) });

        public async Task<T> DeleteAsync<T>(string url) =>
            await SendAsync<T>(new HttpRequestMessage { Method = HttpMethod.Delete, RequestUri = new Uri(url, UriKind.RelativeOrAbsolute) });

        public async Task<T> PostAsync<T>(string url, object content) =>
            await SendAsync<T>(new HttpRequestMessage { Method = HttpMethod.Post, RequestUri = new Uri(url, UriKind.RelativeOrAbsolute) }, content);

        public async Task<T> PutAsync<T>(string url, object content) =>
            await SendAsync<T>(new HttpRequestMessage { Method = HttpMethod.Put, RequestUri = new Uri(url, UriKind.RelativeOrAbsolute) }, content);

        public async Task<T> SendAsync<T>(HttpRequestMessage request, object content)
        {
            var serializer = Converter ?? Serializer;
            request.Content = new StringContent(serializer.Serialize(content), serializer.Encoding, serializer.ContentType);

            var response = await ExecuteAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();
            return Deserialize<T>(responseContent);
        }

        public async Task<T> SendAsync<T>(HttpRequestMessage request)
        {
            var response = await ExecuteAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();
            return Deserialize<T>(responseContent);
        }


        public async Task<HttpResponseMessage> ExecuteAsync(HttpRequestMessage request, bool authRetry = true)
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
                        return await ExecuteAsync(request, false);
                    }
                }
                var content = await response.Content.ReadAsStringAsync();

                throw new RestException(response, content);
            }
            catch (HttpRequestException ex)
            {
                throw new RestException($"Erro ao se conectar ao servidor.", ex);
            }


            throw new NotImplementedException();
        }

        protected virtual void OnAuthorizationError(HttpRequestMessage request, HttpResponseMessage response)
        {
            Authenticator?.OnAuthorizationError(request, response);
            AuthorizationFailed?.Invoke(this, new AuthorizationErrorEventArgs(request, response));
        }

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

        private T Deserialize<T>(string value) =>
            (Converter ?? Deserializer).Deserialize<T>(value);

        private string Serialize(object value) =>
           (Converter ?? Serializer).Serialize(value);
    }
}
