using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Yansoft.Rest
{
    public class RestHttpMessageHandler : HttpClientHandler
    {
        /// <summary>
        /// Event fired when a request fails.
        /// </summary>
        public event EventHandler<RequestErrorEventArgs> RequestFailed;

        /// <summary>
        /// Gets or sets the Authenticator for this instance.
        /// </summary>
        public Func<HttpRequestMessage, Task> AuthenticationHandler { get; set; }

        /// <summary>
        /// Gets or sets the ErrorHandler for ths instance.
        /// </summary>
        public Func<HttpRequestMessage, HttpResponseMessage, Task<HttpResponseMessage>> ErrorHandler { get; set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                if (AuthenticationHandler != null)
                {
                    await AuthenticationHandler(request);
                }
                var response = await base.SendAsync(request, cancellationToken);

                if (!response.IsSuccessStatusCode && ErrorHandler != null)
                {
                    response = await ErrorHandler(request, response);
                }
                if (response.IsSuccessStatusCode)
                {
                    return response;
                }

                OnRequestError(request, response);
                var content = await response.Content.ReadAsStringAsync();
                throw new RestException(request, response, content);
            }
            catch (HttpRequestException ex)
            {
                OnRequestError(request);
                throw new RestException($"Error connecting to server.", request, ex);
            }
        }

        /// <summary>
        /// Method called every time a request fails.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="response"></param>
        protected virtual void OnRequestError(HttpRequestMessage request, HttpResponseMessage response = null)
        {
            RequestFailed?.Invoke(this, new RequestErrorEventArgs(request, response));
        }
    }
}
