using AmetisAPI.Core.Push;
using AmetisAPI.Core.Requests.Push;
using FluentValidation;

namespace AmetisAPI.API.Validators
{
    /// <summary>Abonnement push : service push connu, clés présentes et bornées.</summary>
    public class SavePushSubscriptionRequestValidator : AbstractValidator<SavePushSubscriptionRequest>
    {
        /// <summary>ctor</summary>
        public SavePushSubscriptionRequestValidator()
        {
            RuleFor(p_R => p_R.Endpoint).NotEmpty().MaximumLength(2000).Must(PushEndpointRules.IsAllowed)
                .WithErrorCode(DomainErrors.Data.BadRequest).WithMessage("Service de notification non pris en charge");
            RuleFor(p_R => p_R.P256dh).NotEmpty().MaximumLength(200).WithErrorCode(DomainErrors.Data.BadRequest);
            RuleFor(p_R => p_R.Auth).NotEmpty().MaximumLength(100).WithErrorCode(DomainErrors.Data.BadRequest);
        }
    }
}
