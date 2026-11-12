using AmetisAPI.Core.Requests.Network;
using FluentValidation;

namespace AmetisAPI.API.Validators
{
    /// <summary>Bornes de la consultation des horaires.</summary>
    public class GetDeparturesRequestValidator : AbstractValidator<GetDeparturesRequest>
    {
        /// <summary>ctor</summary>
        public GetDeparturesRequestValidator()
        {
            RuleFor(p_R => p_R.Limit).InclusiveBetween(1, 200).WithErrorCode(DomainErrors.Data.BadRequest);
            RuleFor(p_R => p_R.WindowMinutes).InclusiveBetween(1, 1440).WithErrorCode(DomainErrors.Data.BadRequest);
        }
    }
}
