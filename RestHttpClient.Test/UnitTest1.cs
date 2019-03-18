using System;
using Xunit;

namespace Yansoft.Rest.Test
{
    public class UnitTest1
    {
        [Fact]
        public async void Test1()
        {
            var ws = new RestHttpClient
            {
                BaseAddress = new Uri("https://jsonplaceholder.typicode.com/")
            };

            var full = await ws.GetAsync<Todo>("https://jsonplaceholder.typicode.com/todos/1");

            Assert.Equal(1, full.Id);

            var relative = await ws.GetAsync<Todo>("todos/1");

            Assert.Equal(full, relative);
        }
    }
}
