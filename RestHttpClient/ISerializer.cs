using System;
using System.Collections.Generic;
using System.Text;

namespace Yansoft.Rest
{
    public interface ISerializer
    {
        string ContentType { get; }

        Encoding Encoding { get; }

        string Serialize(object o);
    }
}
