using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace MultiTenant.Api.Extension.AuthorizationPolicy
{
    public class TenantHandler : AuthorizationHandler<TenantRequirement>
    {
        private readonly IHttpContextAccessor _context;

        public TenantHandler(IHttpContextAccessor context)
        {
            _context = context;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, TenantRequirement requirement)
        {

            if (!_context.HttpContext.User.HasClaim(c => c.Type == "tid")||!_context.HttpContext.User.HasClaim(c=>c.Type=="iss"))
            {

                return Task.CompletedTask;
            }

            var tenantId = context.User.Claims.FirstOrDefault(x => x.Type == "itd")?.Value;
            var requestUrl = $"{_context.HttpContext.Request.Host }";

            var requestTenantId = requestUrl.Split('.')[0];
            if(string.IsNullOrEmpty(tenantId)||string.IsNullOrEmpty(requestTenantId)||tenantId!=requestTenantId)
            {
                return Task.CompletedTask;
            }

            var issuer = context.User.FindFirst(x => x.Type == "iss").Value;
            var issuerTenant = new Uri(issuer).Host.Split('.')[0];

            if(string.IsNullOrEmpty(issuerTenant)||issuerTenant!=tenantId)
            {
                return Task.CompletedTask;

            }

            context.Succeed(requirement);

            return Task.CompletedTask;
        }

    }
}
