using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MultiTenancyServer;
using MultiTenantAuth.Extensions.AspIdentity.Model;

namespace MultiTenantAuth.Extensions.AspIdentity
{
    public class ApplicationUserClaimsPrincipalFactory:UserClaimsPrincipalFactory<ApplicationUser>
    {
        private readonly ITenancyContext<Tenant> _tenancyContext;
        private readonly ApplicationUserManager _userManager;
        private readonly IHttpContextAccessor _httpContext;
        public ApplicationUserClaimsPrincipalFactory(ApplicationUserManager userManager,
            IOptions<IdentityOptions> optionsAccessor, ITenancyContext<Tenant> tenancyContext, IHttpContextAccessor httpContext)
            : base(userManager, optionsAccessor)
        {
            _tenancyContext = tenancyContext;
            _httpContext = httpContext;
            _userManager = userManager;
        }

        public override async Task<ClaimsPrincipal> CreateAsync(ApplicationUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            var id = await GenerateClaimsAsync(user);
            return new ClaimsPrincipal(id);
        }

        public async Task<ClaimsPrincipal> CreateAsync(ApplicationUser user, Tenant tenant = null)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            var id = await GenerateClaimsAsync(user, tenant);
            return new ClaimsPrincipal(id);
        }
        protected async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user, Tenant tenant = null)
        {
            var identity = await base.GenerateClaimsAsync(user);
            var claims = new List<Claim>();

            // Check if Tenant is not created
            //if (!user.UserProfileCompleted) claims.Add(new Claim(CustomClaimTypes.ProfileIncomplete, "1"));
            if (!user.TenantProfileCompleted) claims.Add(new Claim(CustomClaimTypes.TenantProfileInComplete, "1"));

            AddIfDontExist(claims, new Claim(JwtClaimTypes.Name, user.UserName));
            AddIfDontExist(claims, new Claim(JwtClaimTypes.GivenName, user.UserName));
            if (tenant != null)
            {
                var roles = await _userManager.GetRolesAsync(user.Id, tenant.Id);
                if (identity.Claims.All(c => c.Type != JwtClaimTypes.Role))
                    claims.AddRange(roles.Select(s => new Claim(JwtClaimTypes.Role, s)));
                claims.Add(new Claim("tid", tenant.Id));
                claims.Add(new Claim("tname", tenant.CanonicalName));
            }

            identity.AddClaims(claims);
            return identity;
        }
        public void AddIfDontExist(List<Claim> claims, Claim newClaim)
        {
            if (claims.Any(c => c.Type == newClaim.Type))
                return;

            claims.Add(newClaim);
        }
    }
}
