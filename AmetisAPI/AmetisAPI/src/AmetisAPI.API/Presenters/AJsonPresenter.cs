using AmetisAPI.Domain.Interfaces;

namespace AmetisAPI.API.Presenters
{
    /// <summary>
    /// Base class for JSON data presentation
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class AJsonPresenter<T> : IOutputPort<T>
    {
        /// <summary>
        /// ctor
        /// </summary>
        protected AJsonPresenter()
        {
            ContentResult = new JsonContentResult();
        }

        /// <summary>
        /// Returned content
        /// </summary>
        public JsonContentResult ContentResult { get; }

        /// <summary>
        /// Handle the transformation from the response into JSON presentable data
        /// </summary>
        /// <param name="p_Response"></param>
        public virtual void Handle(T p_Response)
        {
            ContentResult.StatusCode = (int)GetStatusCode(p_Response);
            ContentResult.Content = SerializationExtensions.PrettySerialize(GetSuccessMember(p_Response));
        }

        /// <summary>
        /// Get the member to use in case of success to serialize it
        /// </summary>
        /// <param name="p_Response">Use case reponse</param>
        /// <returns>Value to serialize as object</returns>
        protected abstract object GetSuccessMember(T p_Response);

        /// <summary>
        /// Abstract method used by derived class to get status code based on their own logic
        /// </summary>
        /// <param name="p_Response"></param>
        /// <returns></returns>
        protected abstract HttpStatusCode GetStatusCode(T p_Response);
    }
}