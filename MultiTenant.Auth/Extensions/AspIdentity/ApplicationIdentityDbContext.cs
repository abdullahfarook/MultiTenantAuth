using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MultiTenantAuth.Data;
using MultiTenantAuth.Extensions.AspIdentity.Model;

namespace MultiTenantAuth.Extensions.AspIdentity
{
    public class ApplicationIdentityDbContext : IdentityDbContext<
        ApplicationUser, ApplicationRole, string, IdentityUserClaim<string>, ApplicationUserRole,
        IdentityUserLogin<string>, IdentityRoleClaim<string>, IdentityUserToken<string>>
    {
        public ApplicationIdentityDbContext(DbContextOptions<ApplicationDbContext> options):base(options){}
    }
}