using AmetisAPI.Core.Requests.Network;
using FluentValidation;

namespace AmetisAPI.API.Validators
{
    /// <summary>Bornes de la recherche d'arrêts.</summary>
    public class SearchStopsRequestValidator : AbstractValidator<SearchStopsRequest>
    {
        /// <summary>ctor</summary>
        public SearchStopsRequestValidator()
        {
            RuleFor(p_R => p_R.Limit).InclusiveBetween(1, 2000).WithErrorCode(DomainErrors.Data.BadRequest);
            RuleFor(p_R => p_R.RadiusMeters).InclusiveBetween(1, 5000).WithErrorCode(DomainErrors.Data.BadRequest);
            RuleFor(p_R => p_R.Latitude).InclusiveBetween(-90, 90).WithErrorCode(DomainErrors.Data.BadRequest);
            RuleFor(p_R => p_R.Longitude).InclusiveBetween(-180, 180).WithErrorCode(DomainErrors.Data.BadRequest);
            RuleFor(p_R => p_R).Must(p_R => p_R.Latitude.HasValue == p_R.Longitude.HasValue)
                .WithErrorCode(DomainErrors.Data.BadRequest).WithMessage("Latitude et longitude vont ensemble");
            RuleFor(p_R => p_R.Query).MaximumLength(100).WithErrorCode(DomainErrors.Data.BadRequest);
        }
    }
}
