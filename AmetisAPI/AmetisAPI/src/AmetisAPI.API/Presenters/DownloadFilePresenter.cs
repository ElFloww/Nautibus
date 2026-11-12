using AmetisAPI.Domain.Interfaces;

namespace AmetisAPI.API.Presenters
{
    /// <summary>
    /// Output for DownloadUseCase
    /// </summary>
    public sealed class DownloadFilePresenter : IOutputPort<FileResponse>
    {
        /// <summary>
        /// Result of the Output DownloadUseCase
        /// </summary>
        public ActionResult ContentResult { get; private set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public DownloadFilePresenter()
        {
        }

        /// <summary>
        /// Handle response from UseCase to octet stream
        /// </summary>
        /// <param name="p_Response"></param>
        public void Handle(FileResponse p_Response)
        {
            if (p_Response.Success)
            {
                ContentResult = new FileContentResult(p_Response.Value.Content, "application/octet-stream")
                {
                    FileDownloadName = p_Response.Value.Name
                };
            }
            else
            {
                ContentResult = new JsonContentResult
                {
                    Content = SerializationExtensions.PrettySerialize(p_Response.Errors),
                    StatusCode = (int)(p_Response.Errors?.Any(p_Obj => p_Obj.Code == DomainErrors.Data.NotFound) ?? false ? HttpStatusCode.NotFound : HttpStatusCode.InternalServerError)
                };
            }
        }
    }
}