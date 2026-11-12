namespace AmetisAPI.API.Presenters
{
    /// <summary>
    /// Empty presenter
    /// </summary>
    public class EmptyPresenter : AResponseMessageJsonPresenter<UseCaseResponseMessage>
    {
        /// <inheritdoc />
        protected override object GetSuccessMember(UseCaseResponseMessage p_Response)
        {
            return p_Response.Success;
        }
    }
}