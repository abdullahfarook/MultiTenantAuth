using MultiTenancyServer;

namespace MultiTenantAuth.Extensions.AspIdentity.Model
{
    public class Tenant : TenancyTenant
    {
        // Custom property for display name of tenant.
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Logo { get; set; }
    }
}
