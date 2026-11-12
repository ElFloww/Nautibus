using AmetisAPI.Core.Requests.Gtfs;
using FluentValidation;

namespace AmetisAPI.API.Validators
{
    /// <summary>Taille de page du journal d'import.</summary>
    public class ListFeedVersionsRequestValidator : AbstractValidator<ListFeedVersionsRequest>
    {
        /// <summary>ctor</summary>
        public ListFeedVersionsRequestValidator()
        {
            RuleFor(p_R => p_R.Count).InclusiveBetween(1, 100).WithErrorCode(DomainErrors.Data.BadRequest);
        }
    }
}
