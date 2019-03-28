using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Yansoft.Rest.Test
{
    public class TodoTest
    {
        private RestHttpClient client = new RestHttpClient
        {
            BaseAddress = new Uri("https://jsonplaceholder.typicode.com")
        };

        [Fact]
        public async void Post()
        {
            var todo = new Todo
            {
                UserId = 1,
                Title = "Lorem Ipsum"
            };

            var item = await client.PostAsync<Todo>("todos", todo);

            Assert.Equal(todo.UserId, item.UserId);
            Assert.Equal(todo.Title, item.Title);
        }

        [Fact]
        public async void PostByHttpRequestMessage()
        {
            var json = "{ \"title\": \"Lorem Ipsum\", \"userId\": 1 }";

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                Content = new StringContent(json, Encoding.UTF8, "application/json"),
                RequestUri = new Uri("todos", UriKind.Relative)
            };

            var response = await client.SendAsync(request);
            Assert.True(response.IsSuccessStatusCode);
        }

        [Fact]
        public async void ThrowsExceptionOnError()
        {
            try
            {
                var item = await client.GetAsync<Todo>("todos/800");
                throw new InvalidOperationException("Shouldn't get here!");
            }
            //Use RestException's Request, Response or Content properties to determine how to handle the Exception
            catch (RestException ex) when (ex.Response?.StatusCode == HttpStatusCode.NotFound)
            {
                Console.WriteLine("Content not found.");
            }
            catch (RestException ex)
            {
                Console.WriteLine("Request failed, check out its content: ");
                Console.Write(ex.Content);
                throw;
            }
        }
    }
}
