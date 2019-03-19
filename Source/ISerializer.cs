using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Yansoft.Rest
{
    public interface ISerializer
    {
        Task<HttpContent> SerializeAsync(object o);
    }
}
