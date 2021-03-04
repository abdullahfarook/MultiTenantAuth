using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.ResponseHandling;
using IdentityServer4.Services;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;

namespace MultiTenantAuth.Extensions.IdentityServer
{
    public class AccountChooserResponseGenerator : AuthorizeInteractionResponseGenerator
    {
        public AccountChooserResponseGenerator(ISystemClock clock,
            ILogger<AuthorizeInteractionResponseGenerator> logger,
            IConsentService consent, IProfileService profile)
            : base(clock, logger, consent, profile)
        {
        }
        public override async Task<InteractionResponse> ProcessInteractionAsync(ValidatedAuthorizeRequest request, ConsentResponse consent = null)
        {
            {
                var response = await base.ProcessInteractionAsync(request, consent);
                if (response.IsConsent || response.IsLogin || response.IsError)
                    return response;
                if (!request.Subject.HasClaim(c => c.Type == "tid" && c.Value != "0"))
                    return new InteractionResponse
                    {
                        RedirectUrl = "/Tenants"
                    };
                if (!request.Subject.HasClaim(c => c.Type == "companyProfileInComplete"))
                    return new InteractionResponse
                    {
                        RedirectUrl = "/OnBoarding"
                    };
                return new InteractionResponse();
            }
        }
    }
}
