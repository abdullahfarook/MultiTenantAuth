using Microsoft.AspNetCore.Identity;

namespace MultiTenantAuth.Extensions.AspIdentity.Model
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUserRole : IdentityUserRole<string>
    {
        public ApplicationUserRole() { }

        public ApplicationUserRole(Tenant tenant, ApplicationRole role, ApplicationUser user)
        {
            TenantId = tenant.Id;
            RoleId = role.Id;
            UserId = user.Id;
        }
        public string TenantId { get; set; }
        public States State { get; set; }
        public virtual Tenant Tenant { get; set; }
        public virtual ApplicationUser User { get; set; }
        public virtual ApplicationRole Role { get; set; }
    }
}
