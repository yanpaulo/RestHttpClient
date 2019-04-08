using System;
using System.Collections.Generic;
using System.Text;

namespace Yansoft.Rest
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class RestResourceAttribute : Attribute
    {
        // See the attribute guidelines at 
        //  http://go.microsoft.com/fwlink/?LinkId=85236

        public string Path { get; private set; }

        public RestResourceAttribute(string path)
        {
            Path = path;
        }
    }
}
