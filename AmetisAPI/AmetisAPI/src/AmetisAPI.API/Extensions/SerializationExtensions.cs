namespace AmetisAPI.API.Extensions
{
    /// <summary>
    /// Extension class for serialization
    /// </summary>
    public static class SerializationExtensions
    {
        private static readonly JsonSerializerOptions m_JsonSerializerOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        };

        /// <summary>
        /// Serialize an object with some pre-configured options
        /// </summary>
        /// <param name="p_Object"></param>
        /// <returns></returns>
        public static string PrettySerialize(object p_Object)
        {
            return JsonSerializer.Serialize(p_Object, m_JsonSerializerOptions);
        }
    }
}