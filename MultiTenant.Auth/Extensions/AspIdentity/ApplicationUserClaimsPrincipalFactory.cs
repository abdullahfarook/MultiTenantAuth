using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MultiTenancyServer;
using MultiTenantAuth.Extensions.AspIdentity.Model;

namespace MultiTenantAuth.Extensions.AspIdentity
{
    public class ApplicationUserClaimsPrincipalFactory:UserClaimsPrincipalFactory<ApplicationUser>
    {
        private readonly ITenancyContext<Tenant> _tenancyContext;

        public ApplicationUserClaimsPrincipalFactory(ApplicationUserManager userManager,
            IOptions<IdentityOptions> optionsAccessor, ITenancyContext<Tenant> tenancyContext)
            : base(userManager, optionsAccessor)
        {
            _tenancyContext = tenancyContext;
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
        {
            var claims = await base.GenerateClaimsAsync(user);
            var tenant = _tenancyContext?.Tenant;
            if (tenant!=null)
            {
                claims.AddClaim(new Claim("tid", tenant.Id));
                claims.AddClaim(new Claim("tname", tenant.CanonicalName));
            }
           
            return claims;
        }
    }
}
