using System;
using System.Collections.Generic;
using MultiTenancyServer;

namespace MultiTenantAuth.Extensions.AspIdentity.Model
{
    public class Tenant : TenancyTenant
    {
        // Custom property for display name of tenant.
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Logo { get; set; }
        public virtual List<ApplicationUserRole> UserRoles { get; protected set; } = new List<ApplicationUserRole>();
        protected Tenant() { }
        public Tenant(string uniqueName, string name)
        {
            base.CanonicalName = uniqueName;
            Name = name;
        }
        public Tenant(Guid id, string name)
        {
            base.CanonicalName = name;
            Id = id.ToString();
        }
    }
    public class TenantViewModel
    {
        public Guid TenantId { get; set; }
        public List<Tenant> Tenants { get; set; }
        public string ReturnUrl { get; set; }
    }
}
