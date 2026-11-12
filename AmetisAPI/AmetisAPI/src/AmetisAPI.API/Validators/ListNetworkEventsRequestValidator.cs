using AmetisAPI.Core.Requests.Events;
using FluentValidation;

namespace AmetisAPI.API.Validators
{
    /// <summary>Période consultable : jusqu'à la durée de conservation (30 jours).</summary>
    public class ListNetworkEventsRequestValidator : AbstractValidator<ListNetworkEventsRequest>
    {
        /// <summary>ctor</summary>
        public ListNetworkEventsRequestValidator()
        {
            RuleFor(p_R => p_R.Days).InclusiveBetween(1, 30).WithErrorCode(DomainErrors.Data.BadRequest);
        }
    }
}
