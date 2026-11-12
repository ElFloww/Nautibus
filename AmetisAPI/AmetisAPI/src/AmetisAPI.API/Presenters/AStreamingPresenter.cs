namespace AmetisAPI.API.Presenters
{
    /// <summary>
    /// Presenter for registration request
    /// </summary>
    public abstract class AStreamingPresenter<T> : IStreamingOutputPort<UseCaseResponseMessage<T>> where T : class
    {
        /// <summary>
        /// Default handle method for streaming a response
        /// </summary>
        /// <param name="p_Http"></param>
        /// <param name="p_Response"></param>
        /// <param name="p_CancellationToken"></param>
        /// <returns></returns>
        public virtual async Task HandleAsync(HttpContext p_Http, IAsyncEnumerable<UseCaseResponseMessage<T>> p_Response, CancellationToken p_CancellationToken = default)
        {
            p_Http.Response.Headers.ContentType = "text/event-stream";
            await foreach (UseCaseResponseMessage<T> v_Value in p_Response.WithCancellation(p_CancellationToken))
            {
                if (v_Value.Success)
                {
                    await p_Http.Response
                        .WriteAsync(SerializeAndFormat(v_Value.Value), p_CancellationToken);

                    await p_Http.Response.Body.FlushAsync(p_CancellationToken);
                }
                else
                {
                    await p_Http.Response.WriteAsync(SerializeAndFormat(v_Value.Errors), cancellationToken: p_CancellationToken);
                    await p_Http.Response.Body.FlushAsync(p_CancellationToken);
                }
            }
        }

        private static string SerializeAndFormat(object p_ToSerialize) => $"data: {SerializationExtensions.PrettySerialize(p_ToSerialize)} \r\r";
    }
}