using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MultiTenantAuth.Extensions.AspIdentity.Model;

namespace MultiTenantAuth.Extensions.AspIdentity
{
    public class ApplicationSigninManager : SignInManager<ApplicationUser>
    {
        public ApplicationSigninManager(ApplicationUserManager userManager, IHttpContextAccessor contextAccessor,
            ApplicationUserClaimsPrincipalFactory claimsFactory, IOptions<IdentityOptions> optionsAccessor,
            ILogger<SignInManager<ApplicationUser>> logger, IAuthenticationSchemeProvider schemes,
            IUserConfirmation<ApplicationUser> confirmation) : base(userManager, contextAccessor, claimsFactory,
            optionsAccessor, logger, schemes, confirmation)
        {
        }

    }
}
