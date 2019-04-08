using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yansoft.Rest
{
    public class RestUtility
    {
        public static string ResourcePath<T>() =>
            ResourcePath(typeof(T));

        public static string ResourcePath(Type t)
        {
            var args = t.GetGenericArguments();
            if (typeof(System.Collections.IEnumerable).IsAssignableFrom(t) && args.Length == 1)
            {
                t = args[0];
            }

            return t
                .GetCustomAttributes(false)
                .OfType<RestResourceAttribute>()
                .SingleOrDefault()?.Path ?? null;
        }
    }
}
