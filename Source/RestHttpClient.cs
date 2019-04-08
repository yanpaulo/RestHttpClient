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

        #region Constructor
        public RestHttpClient() : base(new RestHttpMessageHandler())
        {
        }

        public RestHttpClient(HttpMessageHandler handler) : this(handler, true)
        {
        }

        public RestHttpClient(HttpMessageHandler handler, bool disposeHandler) : base(handler, disposeHandler)
        {
        }
        #endregion

        #region Properties
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
        /// Request URL prefix is obtained from RestResource attribute on T, if present.
        /// </summary>
        /// <typeparam name="T">Type of object to be returned.</typeparam>
        /// <returns>Content returned by the server, serialized as T.</returns>
        public async Task<T> GetAsync<T>()
        {
            return await GetAsync<T>(null);
        }

        /// <summary>
        /// Sends a GET request to the specified url and returns its content converted by a deserializer.
        /// Request URL prefix is obtained from RestResource attribute on T, if present.
        /// </summary>
        /// <typeparam name="T">Type of object to be returned.</typeparam>
        /// <param name="url">string to be appended after the request URL.</param>
        /// <returns>Content returned by the server, serialized as T.</returns>
        public async Task<T> GetAsync<T>(string url)
        {
            return await GetAsync<T>(url, true);
        }

        /// <summary>
        /// Sends a GET request to the specified url and returns its content converted by a deserializer.
        /// If urlAppend is true, then Request URL prefix is obtained from RestResource attribute on T, if present.
        /// </summary>
        /// <typeparam name="T">Type of the object to be returned.</typeparam>
        /// <param name="url">string to be appended after the request URL.</param>
        /// <param name="urlAppend">If true, 'url' parameter is appended after the path specified in RestResource on T.</param>
        /// <returns>Content returned by the server, serialized as T.</returns>
        public async Task<T> GetAsync<T>(string url, bool urlAppend)
        {
            url = BuildUrl<T>(url, urlAppend);
            return await SendAsync<T>(new HttpRequestMessage { Method = HttpMethod.Get, RequestUri = new Uri(url, UriKind.RelativeOrAbsolute) });
        }

        /// <summary>
        /// Sends a GET request to the specified url and returns its content converted by a deserializer.
        /// </summary>
        /// <typeparam name="T">Type of the object to be returned.</typeparam>
        /// <param name="url">string to be appended after the request URL.</param>
        /// <param name="typeObject">Object to infer the type from (usually an anonymous object).</param>
        /// <returns>Content returned by the server, serialized as T.</returns>
        public async Task<T> GetAsync<T>(string url, T typeObject) =>
            await GetAsync<T>(url);

        /// <summary>
        /// Sends a POST request to the specified url with its body serialized by a serializer and returns its content converted by a deserializer.
        /// Request URL prefix is obtained from RestResource attribute on T, if present.
        /// </summary>
        /// <typeparam name="T">Type of the object to be returned.</typeparam>
        /// <param name="url">string to be appended after the request URL.</param>
        /// <param name="content">Content to be serialized and send in the request body.</param>
        /// <returns>Content returned by the server, serialized as T</returns>
        public async Task<T> PostAsync<T>(object content) =>
            await PostAsync<T>(null, content);

        /// <summary>
        /// Sends a POST request to the specified url with its body serialized by a serializer and returns its content converted by a deserializer.
        /// Request URL prefix is obtained from RestResource attribute on T, if present.
        /// </summary>
        /// <typeparam name="T">Type of the object to be returned.</typeparam>
        /// <param name="url">string to be appended after the request URL.</param>
        /// <param name="content">Content to be serialized and send in the request body.</param>
        /// <returns>Content returned by the server, serialized as T</returns>
        public async Task<T> PostAsync<T>(string url, object content) =>
            await PostAsync<T>(url, content, true);

        /// <summary>
        /// Sends a POST request to the specified url with its body serialized by a serializer and returns its content converted by a deserializer.
        /// </summary>
        /// <param name="url">string to be appended after the request URL.</param>
        /// <param name="content">Content to be serialized and send in the request body.</param>
        /// <param name="urlAppend">If true, 'url' parameter is appended after the path specified in RestResource on T.</param>
        public async Task PostAsync(string url, object content, bool urlAppend) =>
            await SendAsync(url, HttpMethod.Post, content, urlAppend);

        /// <summary>
        /// Sends a POST request to the specified url with its body serialized by a serializer and returns its content converted by a deserializer.
        /// </summary>
        /// <typeparam name="T">Type of the object to be returned.</typeparam>
        /// <param name="url">string to be appended after the request URL.</param>
        /// <param name="content">Content to be serialized and send in the request body.</param>
        /// <param name="urlAppend">If true, 'url' parameter is appended after the path specified in RestResource on T.</param>
        /// <returns>Content returned by the server, serialized as T</returns>
        public async Task<T> PostAsync<T>(string url, object content, bool urlAppend) =>
            await SendAsync<T>(url, HttpMethod.Post, content, urlAppend);


        /// <summary>
        /// Sends a PUT request to the specified url with its body serialized by a serializer and returns its content converted by a deserializer.
        /// Request URL prefix is obtained from RestResource attribute on T, if present.
        /// </summary>
        /// <typeparam name="T">Type of the object to be returned.</typeparam>
        /// <param name="url">string to be appended after the request URL.</param>
        /// <param name="content">Content to be serialized and send in the request body.</param>
        /// <returns>Content returned by the server, serialized as T</returns>
        public async Task<T> PutAsync<T>(string url, object content) =>
            await PutAsync<T>(url, content, true);

        /// <summary>
        /// Sends a PUT request to the specified url with its body serialized by a serializer and returns its content converted by a deserializer.
        /// Request URL prefix is obtained from RestResource attribute on T, if present.
        /// </summary>
        /// <param name="url">string to be appended after the request URL.</param>
        /// <param name="content">Content to be serialized and send in the request body.</param>
        public async Task PutAsync(string url, object content) =>
            await PutAsync(url, content, true);

        /// <summary>
        /// Sends a PUT request to the specified url with its body serialized by a serializer and returns its content converted by a deserializer.
        /// </summary>
        /// <typeparam name="T">Type of the object to be returned.</typeparam>
        /// <param name="url">string to be appended after the request URL.</param>
        /// <param name="content">Content to be serialized and send in the request body.</param>
        /// <param name="urlAppend">If true, 'url' parameter is appended after the path specified in RestResource on T.</param>
        /// <returns>Content returned by the server, serialized as T</returns>
        public async Task<T> PutAsync<T>(string url, object content, bool urlAppend) =>
            await SendAsync<T>(url, HttpMethod.Put, content, urlAppend);

        /// <summary>
        /// Sends a PUT request to the specified url with its body serialized by a serializer and returns its content converted by a deserializer.
        /// </summary>
        /// <param name="url">string to be appended after the request URL.</param>
        /// <param name="content">Content to be serialized and send in the request body.</param>
        /// <param name="urlAppend">If true, 'url' parameter is appended after the path specified in RestResource on T.</param>
        public async Task PutAsync(string url, object content, bool urlAppend) =>
            await SendAsync(url, HttpMethod.Put, content, urlAppend);

        #endregion

        #region SendAsync Overloads
        public async Task<T> SendAsync<T>(string url, HttpMethod method, object content, bool urlAppend)
        {
            url = BuildUrl<T>(url, urlAppend);
            return await SendAsync<T>(new HttpRequestMessage { Method = method, RequestUri = new Uri(url, UriKind.RelativeOrAbsolute) }, content);
        }

        public async Task SendAsync(string url, HttpMethod method, object content, bool urlAppend)
        {
            url = BuildUrl(url, content, urlAppend);
            await SendAsync(new HttpRequestMessage { Method = method, RequestUri = new Uri(url, UriKind.RelativeOrAbsolute) }, content);
        }

        public async Task<T> SendAsync<T>(HttpRequestMessage request) =>
            await SendAsync<T>(request, Deserializer ?? Converter);

        public async Task<T> SendAsync<T>(HttpRequestMessage request, object content) =>
            await SendAsync<T>(request, content, Serializer ?? Converter, Deserializer ?? Converter);

        public async Task SendAsync(HttpRequestMessage request, object content) =>
            await SendAsync(request, content, Serializer ?? Converter);

        public async Task<T> SendAsync<T>(HttpRequestMessage request, object content, IConverter converter) =>
            await SendAsync<T>(request, content, converter, converter);

        public async Task<T> SendAsync<T>(HttpRequestMessage request, IDeserializer deserializer)
        {
            var response = await SendAsync(request);
            return await deserializer.DeserializeAsync<T>(response.Content);
        }

        public async Task SendAsync(HttpRequestMessage request, object content, ISerializer serializer)
        {
            request.Content = await serializer.SerializeAsync(content);
            await SendAsync(request);
        }

        public async Task<T> SendAsync<T>(HttpRequestMessage request, object content, ISerializer serializer, IDeserializer deserializer)
        {
            request.Content = await serializer.SerializeAsync(content);

            var response = await SendAsync(request);
            return await deserializer.DeserializeAsync<T>(response.Content);
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


        private static string BuildUrl<T>(string url, bool append) =>
            BuildUrl(url, typeof(T), append);

        private static string BuildUrl(string url, object o, bool append) =>
            BuildUrl(url, o.GetType(), append);

        private static string BuildUrl(string url, Type t, bool append)
        {
            if (append == false)
            {
                return url;
            }

            var path = RestUtility.ResourcePath(t);
            if (path != null)
            {
                url = $"{path}{url}";
            }

            return url;
        }
        #endregion
    }
}
