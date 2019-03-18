using System;
using System.Collections.Generic;
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
        public async void GetAll()
        {
            var todos = await client.RestGetAsync<List<Todo>>("todos");

            Assert.Equal(200, todos.Count);
        }

        [Fact]
        public async void GetSingle()
        {
            var todo = await client.RestGetAsync<Todo>("todos/1");

            Assert.Equal(1, todo.Id);
        }

        [Fact]
        public async void GetByFullUrlEqualToRelative()
        {
            const string path = "todos/1";
            var relative = await client.RestGetAsync<Todo>(path);
            var full = await client.RestGetAsync<Todo>($"https://jsonplaceholder.typicode.com/{path}");

            Assert.Equal(relative, full);
        }

        [Fact]
        public async void Post()
        {
            var todo = new Todo
            {
                UserId = 1,
                Title = "Lorem Ipsum"
            };

            var item = await client.RestPostAsync<Todo>("todos", todo);

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

            var response = await client.RestSendAsync(request, authRetry: false);
            Assert.True(response.IsSuccessStatusCode);
        }
    }
}
