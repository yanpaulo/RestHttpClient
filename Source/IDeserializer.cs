using System.Net.Http;
using System.Threading.Tasks;

namespace Yansoft.Rest
{
    public interface IDeserializer
    {
        Task<T> DeserializeAsync<T>(HttpContent content);
    }
}
