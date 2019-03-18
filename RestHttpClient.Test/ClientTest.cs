using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Yansoft.Rest.Test
{
    public class ClientTest
    {
        private RestHttpClient ws = new RestHttpClient
        {
            BaseAddress = new Uri("https://jsonplaceholder.typicode.com")
        };


        [Fact]
        public async void GetAll()
        {
            var todos = await ws.GetAsync<List<Todo>>("todos");

            Assert.Equal(200, todos.Count);
        }

        [Fact]
        public async void GetSingle()
        {
            var todo = await ws.GetAsync<Todo>("todos/1");

            Assert.Equal(1, todo.Id);
        }

        [Fact]
        public async void GetByFullUrlEqualToRelative()
        {
            const string path = "todos/1";
            var relative = await ws.GetAsync<Todo>(path);
            var full = await ws.GetAsync<Todo>($"https://jsonplaceholder.typicode.com/{path}");

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

            var item = await ws.PostAsync<Todo>("todos", todo);

            Assert.Equal(todo.UserId, item.UserId);
            Assert.Equal(todo.Title, item.Title);
        }
    }
}
