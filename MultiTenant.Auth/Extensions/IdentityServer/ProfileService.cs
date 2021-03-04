using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using MultiTenancyServer;
using MultiTenantAuth.Extensions.AspIdentity;
using MultiTenantAuth.Extensions.AspIdentity.Model;

namespace MultiTenantAuth.Extensions.IdentityServer
{
    public class ProfileService:IProfileService
    {
        private readonly ApplicationUserClaimsPrincipalFactory _claimsFactory;
        private readonly ITenancyContext<Tenant> _tenancyContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _context;
        private readonly ILogger<DefaultProfileService> _logger;
        private readonly IResourceStore _resourceStore;
        public ProfileService(IHttpContextAccessor context, UserManager<ApplicationUser> userManager,
            ApplicationUserClaimsPrincipalFactory claimsFactory, ITenancyContext<Tenant> tenancyContext, 
            ILogger<DefaultProfileService> logger, IResourceStore resourceStore)
        {
            _context = context;
            _userManager = userManager;
            _claimsFactory = claimsFactory;
            _tenancyContext = tenancyContext;
            _logger = logger;
            _resourceStore = resourceStore;
        }
        public async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            // Get user data
            var user = await _userManager.GetUserAsync(context.Subject);
            var principal = await _claimsFactory.CreateAsync(user, GetTenantFromContext(context.Subject));
            var claimsUser = principal.Claims.ToList();
            var subjectClaims = context.Subject.Claims.ToList();

            // Merge ASP.NET Identity claims
            subjectClaims.Merge(claimsUser);
            subjectClaims.MergeValues(claimsUser.Where(x => x.Type == JwtClaimTypes.Role));

            subjectClaims.AddIfDontExist(new Claim("username", user.UserName));
            subjectClaims.AddIfDontExist(new Claim(JwtClaimTypes.Name, user.UserName));

            if (subjectClaims.All(a => a.Type != JwtClaimTypes.Role))
            {
                var roles = await _userManager.GetRolesAsync(user);
                subjectClaims.AddRange(roles.Select(s => new Claim(JwtClaimTypes.Role, s)));
            }

            // Sometimes IdentityResources are specified at UserClaims in ProtectedResource. Then we include all related claims to RequestedClaims
            var resources = await _resourceStore.GetAllResourcesAsync();
            var usersClaimsToGoWithin = GetIdentityResourcesToIncludeInRequestedClaims(context, resources);
            if (context.Subject.ContainsTenant())
            {
                usersClaimsToGoWithin.Add("tid");
                usersClaimsToGoWithin.Add("tname");
            }

            usersClaimsToGoWithin.Merge(context.RequestedClaimTypes);
            context.RequestedClaimTypes = usersClaimsToGoWithin;

            context.LogProfileRequest(_logger);
            context.AddRequestedClaims(subjectClaims);
            context.LogIssuedClaims(_logger);
        }
        private Tenant GetTenantFromContext(ClaimsPrincipal subject)
        {
            var tenant = _tenancyContext.Tenant;

            if (tenant is {})
                return tenant;

            tenant = subject.GetTenant();
            return tenant;
        }
        private static List<string> GetIdentityResourcesToIncludeInRequestedClaims(ProfileDataRequestContext context,
            Resources resources)
        {
            var usersClaimsToGoWithin = new List<string>();
            foreach (var contextRequestedClaimType in context.RequestedClaimTypes)
            {
                if (resources.IdentityResources.Any(a => a.Name == contextRequestedClaimType))
                {
                    usersClaimsToGoWithin.AddRange(resources.IdentityResources
                        .FirstOrDefault(f => f.Name == contextRequestedClaimType)
                        ?.UserClaims ?? new List<string>());
                }
            }

            return usersClaimsToGoWithin;
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
