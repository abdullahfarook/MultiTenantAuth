using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace MultiTenantAuth.Extensions.AspIdentity.Model
{
    public class ApplicationRole : IdentityRole
    {
        public virtual ICollection<ApplicationUserRole> UserRoles { get; protected set; } = new HashSet<ApplicationUserRole>();
        public ApplicationRole()
        {
            Id = Guid.NewGuid().ToString();
        }

        public ApplicationRole(string roleName):base(roleName)
        {
            Id = Guid.NewGuid().ToString();
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
