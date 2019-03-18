namespace Yansoft.Rest
{
    public interface IDeserializer
    {
        T Deserialize<T>(string content);
    }
}
