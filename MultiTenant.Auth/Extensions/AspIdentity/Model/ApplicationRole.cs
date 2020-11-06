using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace MultiTenantAuth.Extensions.AspIdentity.Model
{
    public sealed class ApplicationRole : IdentityRole
    {
        public ApplicationRole(){}
        public ApplicationRole(string roleName):base(roleName)
        {
        }

        public static List<ApplicationRole> DefaultRoles => new List<ApplicationRole>
        {
            new ApplicationRole("owner"),
            new ApplicationRole("admin"),
            new ApplicationRole("manager"),
            new ApplicationRole("member")
        };
    }
}
