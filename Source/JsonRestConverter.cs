using System.Runtime.CompilerServices;
using System.Text;
using Newtonsoft.Json;

namespace Yansoft.Rest
{
    public class JsonRestConverter : IConverter
    {
        public JsonSerializerSettings SerializerSettings { get; set; } 
            = new JsonSerializerSettings();

        public string ContentType => "application/json";

        public Encoding Encoding => Encoding.UTF8;

        public virtual T Deserialize<T>(string content)
        {
            return JsonConvert.DeserializeObject<T>(content, SerializerSettings);
        }

        /// <summary>
        /// Deserialize an object with type given, used for anonymous type objects
        /// </summary>
        /// <param name="content">Content to be deserialized</param>
        /// <param name="type">Type to deserialize to</param>
        /// <typeparam name="T">Type to deserialize to, inferred from type field</typeparam>
        /// <returns>Deserialized anonymous string from JSON String</returns>
        public T Deserialize<T>(string content, T type)
        {
            return JsonConvert.DeserializeAnonymousType(content, type, SerializerSettings);
        }

        public virtual string Serialize(object o)
        {
            return JsonConvert.SerializeObject(o, SerializerSettings);
        }
    }
}
