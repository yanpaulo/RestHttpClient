using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Yansoft.Rest.Test
{
    public class TodoGetTests
    {
        private RestHttpClient client = new RestHttpClient
        {
            BaseAddress = new Uri("https://jsonplaceholder.typicode.com")
        };

        [Fact]
        public async void GetAll()
        {
            var todos = await client.GetAsync<List<Todo>>("todos");

            Assert.Equal(200, todos.Count);
        }
        
        [Fact]
        public async void GetAllAnonymous()
        {
            var todoListType = new List<object>()
                .Select(t => new { Id = 0, UserId = 0, Title = "" }).ToList(); 
            
            var todos = await client.GetAsync("todos", todoListType);

            Assert.Equal(200, todos.Count);
        }

        [Fact]
        public async void GetSingle()
        {
            var todo = await client.GetAsync<Todo>("todos/1");

            Assert.Equal(1, todo.Id);
        }
        
        [Fact]
        public async void GetSingleAnonymous()
        {
            var todoType = new { Id = 0, UserId = 0, Title = "" };

            var todo = await client.GetAsync("todos/1", todoType);

            Assert.Equal(1, todo.Id);
        }

        [Fact]
        public async void GetByFullUrlEqualToRelative()
        {
            const string path = "todos/1";
            var relative = await client.GetAsync<Todo>(path);
            var full = await client.GetAsync<Todo>($"https://jsonplaceholder.typicode.com/{path}");

            Assert.Equal(relative, full);
        }

        [Fact]
        public async void GetByFullUrlEqualToRelativeAnonymous()
        {
            var todoType = new { Id = 0, UserId = 0, Title = "" };
            
            const string path = "todos/1";
            //Leave one as standard type
            var relative = await client.GetAsync<Todo>(path);
            //One as anonymous type
            var full = await client.GetAsync($"https://jsonplaceholder.typicode.com/{path}", todoType);

            Assert.Equal(relative.Id, full.Id);
        }
    }
}