using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Yansoft.Rest
{
    public static class SystemNetExtensions
    {
        public static HttpRequestMessage Clone(this HttpRequestMessage request)
        {
            var clone = new HttpRequestMessage
            {
                Content = request.Content,
                Method = request.Method,
                RequestUri = request.RequestUri,
                Version = request.Version
            };
            foreach (var p in request.Properties)
            {
                clone.Properties.Add(p);
            }
            foreach (var h in request.Headers)
            {
                clone.Headers.Add(h.Key, h.Value);
            }

            return clone;
        }
    }
}
