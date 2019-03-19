using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Yansoft.Rest
{
    public class JsonRestConverter : IConverter
    {
        public JsonSerializer JsonSerializer { get; set; } 
            = new JsonSerializer();

        public string ContentType => "application/json";

        public Encoding Encoding => Encoding.UTF8;
        
        public async Task<T> DeserializeAsync<T>(HttpContent content)
        {
            using (var stream = await content.ReadAsStreamAsync())
            using (var sr = new StreamReader(stream))
            using (JsonReader reader = new JsonTextReader(sr))
            {
                JsonSerializer serializer = new JsonSerializer();
                
                return JsonSerializer.Deserialize<T>(reader);
            }
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
            return JToken.FromObject(o, JsonSerializer).ToString();
        }
    }
}
