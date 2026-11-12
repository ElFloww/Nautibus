namespace AmetisAPI.API.Presenters
{
    /// <summary>
    /// Streaming output contract
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    public interface IStreamingOutputPort<in TValue>
    {
        /// <summary>
        /// Stream a response
        /// </summary>
        /// <param name="p_Http"></param>
        /// <param name="p_Response"></param>
        /// <param name="p_CancellationToken"></param>
        /// <returns></returns>
        Task HandleAsync(HttpContext p_Http, IAsyncEnumerable<TValue> p_Response,
            CancellationToken p_CancellationToken = default);
    }
}