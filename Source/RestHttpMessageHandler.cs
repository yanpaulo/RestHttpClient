using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Yansoft.Rest
{
    public class RestHttpMessageHandler : HttpClientHandler, IRestMessageHandler
    {
        /// <summary>
        /// Event fired when a request fails.
        /// </summary>
        public event EventHandler<RequestErrorEventArgs> RequestFailed;

        /// <summary>
        /// Gets or sets the Authenticator for this instance.
        /// </summary>
        public Func<HttpRequestMessage, Task<HttpRequestMessage>> AuthenticationHandler { get; set; }

        /// <summary>
        /// Gets or sets the ErrorHandler for ths instance.
        /// </summary>
        public Func<HttpRequestMessage, HttpResponseMessage, Task<HttpResponseMessage>> ErrorHandler { get; set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) => 
            await DoSendAsync(request, cancellationToken, this, base.SendAsync, AuthenticationHandler, ErrorHandler, RequestFailed);

        /// <summary>
        /// Method called every time a request fails.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        protected virtual void OnRequestError(HttpRequestMessage request, HttpResponseMessage response = null)
        {
            RequestFailed?.Invoke(this, new RequestErrorEventArgs(request, response));
        }

        public static async Task<HttpResponseMessage> DoSendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken,
            object sender,
            Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> send,
            Func<HttpRequestMessage, Task<HttpRequestMessage>> auth,
            Func<HttpRequestMessage, HttpResponseMessage, Task<HttpResponseMessage>> error,
            EventHandler<RequestErrorEventArgs> errorEventHandler)
        {
            try
            {
                if (auth != null)
                {
                    await auth(request);
                }
                var response = await send(request, cancellationToken);

                if (!response.IsSuccessStatusCode && error != null)
                {
                    response = await error(request, response);
                }
                if (response.IsSuccessStatusCode)
                {
                    return response;
                }
                
                errorEventHandler?.Invoke(sender, new RequestErrorEventArgs(request, response));
                var content = await response.Content.ReadAsStringAsync();
                throw new RestException(request, response, content);
            }
            catch (HttpRequestException ex)
            {
                errorEventHandler?.Invoke(sender, new RequestErrorEventArgs(request));
                throw new RestException($"Error connecting to server.", request, ex);
            }
        }
    }
}
