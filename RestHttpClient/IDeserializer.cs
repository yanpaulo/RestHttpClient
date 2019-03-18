using System;
using System.Collections.Generic;
using System.Text;

namespace Yansoft.Rest
{
    public interface IDeserializer
    {
        T Deserialize<T>(string content);
    }
}
