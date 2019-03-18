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


        private void CheckSerializer(object value, [CallerMemberName]string memberName = null)
        {
            if (value == null && Converter == null)
            {
                throw new InvalidOperationException($"Cannot set both {memberName} and {nameof(Converter)} to null.");
            }
        }

        public async Task<T> GetAsync<T>(string url)
        {
            return await SendAsync<T>(new HttpRequestMessage { RequestUri = new Uri(url, UriKind.RelativeOrAbsolute) });
        }

        public async Task<T> SendAsync<T>(HttpRequestMessage request, object data)
        {
            var serializer = Converter ?? Serializer;
            request.Content = new StringContent(serializer.Serialize(data), serializer.Encoding, serializer.ContentType);
            return Deserialize<T>(await ExecuteAsync(request));
        }

        public async Task<T> SendAsync<T>(HttpRequestMessage request)
        {
            return Deserialize<T>(await ExecuteAsync(request));
        }


        public async Task<string> ExecuteAsync(HttpRequestMessage request, bool authRetry = true)
        {
            try
            {
                Authenticator?.Authenticate(request);
                var response = await SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return content;
                }
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    OnAuthorizationError(request, response);
                    if (authRetry && Authenticator != null)
                    {
                        return await ExecuteAsync(request, false); 
                    }
                }
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
