using AmetisAPI.Core.Requests.Accounts;
using AmetisAPI.Domain.Bo.Accounts;
using FluentValidation;

namespace AmetisAPI.API.Validators
{
    /// <summary>Favori : type connu, identifiant de cible renseigné et borné.</summary>
    public class AddFavoriteRequestValidator : AbstractValidator<AddFavoriteRequest>
    {
        /// <summary>ctor</summary>
        public AddFavoriteRequestValidator()
        {
            RuleFor(p_R => p_R.Kind).IsInEnum().WithErrorCode(DomainErrors.Data.BadRequest);
            RuleFor(p_R => p_R.TargetId).NotEmpty().MaximumLength(255).WithErrorCode(DomainErrors.Data.BadRequest);
        }
    }
}
