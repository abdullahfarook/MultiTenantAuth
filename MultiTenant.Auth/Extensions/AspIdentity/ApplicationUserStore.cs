using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MultiTenantAuth.Data;
using MultiTenantAuth.Extensions.AspIdentity.Model;
using ApplicationUserRole = MultiTenantAuth.Extensions.AspIdentity.Model.ApplicationUserRole;

namespace MultiTenantAuth.Extensions.AspIdentity
{
    public class ApplicationUserStore<TUser, TTenant> : UserStore<
        ApplicationUser, IdentityRole, ApplicationDbContext, string, IdentityUserClaim<string>, Model.ApplicationUserRole,
        IdentityUserLogin<string>, IdentityUserToken<string>, IdentityRoleClaim<string>>
        where TUser : ApplicationUser, new()
        where TTenant: Tenant
    {
        public ApplicationUserStore(ApplicationDbContext context, IdentityErrorDescriber describer = null) : base(context, describer)
        {
        }
        public async Task AddToRoleAsync(TUser user, TTenant tenant, string normalizedRoleName,
            CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (string.IsNullOrWhiteSpace(normalizedRoleName))
            {
                throw new ArgumentException($"Argument of type {normalizedRoleName} cannot be null");
            }
            var roleEntity = await FindRoleAsync(normalizedRoleName, cancellationToken);
            if (roleEntity == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "Roll not found"));
            }
            var userRole = Context.Set<ApplicationUserRole>();
            userRole.Add(new ApplicationUserRole(tenant,roleEntity,user));
        }


        public Task<IList<string>> GetRolesAsync(ApplicationUser user, Tenant tenant,
            CancellationToken cancellationToken = default)
            {
                var query = from userRole in Context.Set<ApplicationUserRole>()
                    join role in Context.Set<IdentityRole>() on userRole.RoleId equals role.Id
                    where userRole.UserId.Equals(user.Id) && userRole.TenantId.Equals(tenant.Id)
                    select role.Name;

                return query
                    .ToListAsync(cancellationToken: cancellationToken)
                    .ContinueWith(task => task.Result as IList<string>,
                        TaskContinuationOptions.OnlyOnRanToCompletion);
            }

        public async Task<bool> IsInRoleAsync(ApplicationUser user, Tenant tenant, string normalizedRoleName,
            CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (string.IsNullOrWhiteSpace(normalizedRoleName))
            {
                throw new ArgumentException($"Argument of type {normalizedRoleName} cannot be null");
            }
            var role = await FindRoleAsync(normalizedRoleName, cancellationToken);
            if (role != null)
            {
                var userRoles = Context.Set<ApplicationUserRole>();
                var userRole =
                    await userRoles.FirstOrDefaultAsync(x=> x.UserId.Equals(user.Id) && x.RoleId.Equals(role.Id) && x.TenantId.Equals(tenant.Id), cancellationToken);
                return userRole != null;
            }
            return false;
        }
    }
}
