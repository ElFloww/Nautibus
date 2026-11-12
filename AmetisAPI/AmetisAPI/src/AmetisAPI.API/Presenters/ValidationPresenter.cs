using FluentValidation.Results;

namespace AmetisAPI.API.Presenters
{
    /// <summary>
    /// Presenter for registration request
    /// </summary>
    public class ValidationPresenter : AJsonPresenter<ValidationResult>
    {
        /// <inheritdoc />
        protected override object GetSuccessMember(ValidationResult p_Response)
        {
            return p_Response.Errors.Select(e => new ErrorDto(e.ErrorCode, e.ErrorMessage));
        }

        /// <inheritdoc />
        protected override HttpStatusCode GetStatusCode(ValidationResult p_Response)
        {
            return HttpStatusCode.BadRequest;
        }
    }
}