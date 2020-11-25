using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace MultiTenant.Api.Extension
{
    public class TenantProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public TenantProvider(IHttpContextAccessor httpContextAccessor)
            => _httpContextAccessor = httpContextAccessor;
        public string GetCurrentTenant()
        {
            string result = "";
            var authorizationHeader = _httpContextAccessor.HttpContext.Request.Headers
                .Where(x => x.Key == "Authorization" && x.Value.Any(x=> x.Contains("Bearer")))
                .Select(x =>
                {
                    var value = x.Value.First();
                    var search = "Bearer ";
                    return value.Substring(value.IndexOf(search) + search.Length);
                })
                .FirstOrDefault();
            if (!string.IsNullOrEmpty(authorizationHeader))
            {
                var handler = new JwtSecurityTokenHandler();
                var token = handler.ReadJwtToken(authorizationHeader);
                var tenantName = token.Claims.First(x => x.Type == "tname").Value;
                string authorityDomain = "tenants.local:5000";
                string authorityScheme = "https";
                string authorityUrl = $"{authorityScheme}://{tenantName}.{authorityDomain}";
                return authorityUrl;
            }
            
            return result;

        }
    }
}
