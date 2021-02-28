using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using MultiTenancyServer;
using MultiTenantAuth.Extensions.AspIdentity.Model;

namespace MultiTenantAuth.Extensions.IdentityServer
{
    public class ProfileService:IProfileService
    {
        private readonly IUserClaimsPrincipalFactory<ApplicationUser> _claimsFactory;
        private readonly ITenancyContext<Tenant> _tenancyContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _context;
        public ProfileService(IHttpContextAccessor context, UserManager<ApplicationUser> userManager, 
            IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory, ITenancyContext<Tenant> tenancyContext)
        {
            _context = context;
            _userManager = userManager;
            _claimsFactory = claimsFactory;
            _tenancyContext = tenancyContext;
        }
        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var sub = context.Subject.GetSubjectId();
            var user = await _userManager.FindByIdAsync(sub);
            var principal = await _claimsFactory.CreateAsync(user);
            var claims = principal.Claims.ToList();
            claims = claims.Where(claim => context.RequestedClaimTypes.Contains(claim.Type)).Union(claims.Where(x=> x.Type=="tid"|| x.Type=="tname")).ToList();
            // Add custom claims in token here based on user properties or any other source
            claims.Add(new Claim("tid", _tenancyContext.Tenant?.Id ?? string.Empty));
            claims.Add(new Claim("tname", _tenancyContext.Tenant?.CanonicalName ?? string.Empty));
            context.IssuedClaims = claims;
        }

        private string GetTenantName(IsActiveContext context)
        {
            string tenantName;
            var tenantIdClaim = context.Subject.Claims.FirstOrDefault(x => x.Type == "tname");
            tenantName = tenantIdClaim?.Value;
            if (!tenantName.IsNullOrEmpty()) return tenantName;
            tenantName = _context.HttpContext.Request.Query["acr_values"].ToString().Replace("tenant:", "");
            return tenantName;
        }
        public async Task IsActiveAsync(IsActiveContext context)
        {

            //var tokenId = _interaction.
            var sub = context.Subject.GetSubjectId();
            var user = await _userManager.FindByIdAsync(sub);
            if (context.Caller == "AuthorizeEndpoint")
            {
                var tenantName = GetTenantName(context);
                if (user != null && tenantName == _tenancyContext.Tenant?.CanonicalName)
                {
                    context.IsActive = true;
                }
                //else
                //{
                //    context.IsActive = false;
                //}
            }
            //else
            //{
                context.IsActive = user != null;
            //}
        }
    }
}
