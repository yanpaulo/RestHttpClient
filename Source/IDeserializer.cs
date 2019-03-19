namespace Yansoft.Rest
{
    public interface IDeserializer
    {
        T Deserialize<T>(string content);
        
        /// <summary>
        /// Deserialize an object with type given, used for anonymous type objects
        /// </summary>
        /// <param name="content">Content to be deserialized</param>
        /// <param name="type">Type to deserialize to</param>
        /// <typeparam name="T">Type to deserialize to, inferred from type field</typeparam>
        /// <returns>Deserialized anonymous string from JSON String</returns>
        T Deserialize<T>(string content, T type);
    }
}
