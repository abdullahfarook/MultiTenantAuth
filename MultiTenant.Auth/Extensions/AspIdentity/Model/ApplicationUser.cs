using Microsoft.AspNetCore.Identity;

namespace MultiTenantAuth.Extensions.AspIdentity.Model
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
    }
    // Add profile data for application users by adding properties to the ApplicationUser class
    public sealed class ApplicationUserRole : IdentityUserRole<string>
    {
        public ApplicationUserRole(){}

        public ApplicationUserRole(Tenant tenant,IdentityRole role, ApplicationUser user)
        {
            TenantId = tenant.Id;
            RoleId = role.Id;
            UserId = user.Id;
        }
        public string TenantId { get; set; }
        //public Tenant Tenant { get; set; }
        //public IdentityRole Role { get; set; }
        //public ApplicationUser User { get; set; }
    }
}
