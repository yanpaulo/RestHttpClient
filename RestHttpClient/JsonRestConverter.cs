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

        public T Deserialize<T>(string content)
        {
            return JsonConvert.DeserializeObject<T>(content, SerializerSettings);
        }

        public string Serialize(object o)
        {
            return JsonConvert.SerializeObject(o, SerializerSettings);
        }
    }
}
