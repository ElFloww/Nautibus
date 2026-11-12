namespace AmetisAPI.API.Presenters
{
    /// <summary>
    /// ContentResult class meant to be used to return content as json
    /// </summary>
    public class JsonContentResult : ContentResult
    {
        /// <summary>
        /// ctor
        /// </summary>
        public JsonContentResult()
        {
            ContentType = "application/json";
        }
    }
}