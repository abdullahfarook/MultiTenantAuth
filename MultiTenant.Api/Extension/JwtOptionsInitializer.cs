using System.Diagnostics;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;

namespace MultiTenant.Api.Extension
{
    public class JwtOptionsInitializer : IConfigureNamedOptions<JwtBearerOptions>
    {
        private readonly TenantProvider _tenantProvider;
        public JwtOptionsInitializer(
            TenantProvider tenantProvider)
        {
            _tenantProvider = tenantProvider;
        }
        public void Configure(string name, JwtBearerOptions options)
        {
            var authority = _tenantProvider.GetCurrentTenant();
            options.RequireHttpsMetadata = true;
            options.Authority = authority;
        }
        public void Configure(JwtBearerOptions options)
            => Debug.Fail("This infrastructure method shouldn't be called.");
    }
}
