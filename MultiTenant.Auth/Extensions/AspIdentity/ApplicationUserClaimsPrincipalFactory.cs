using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MultiTenantAuth.Extensions.AspIdentity.Model;

namespace MultiTenantAuth.Extensions.AspIdentity
{
    public class ApplicationUserClaimsPrincipalFactory:UserClaimsPrincipalFactory<ApplicationUser>
    {
        public ApplicationUserClaimsPrincipalFactory(ApplicationUserManager userManager, IOptions<IdentityOptions> optionsAccessor) 
            : base(userManager, optionsAccessor) { }
    }
}
