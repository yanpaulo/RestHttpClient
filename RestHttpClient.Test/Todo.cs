using System;
using System.Collections.Generic;
using System.Text;

namespace Yansoft.Rest.Test
{
    public class Todo : IEquatable<Todo>
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string Title { get; set; }

        public bool Completed { get; set; }

        public bool Equals(Todo other) => 
            Id == other?.Id;
    }

}
