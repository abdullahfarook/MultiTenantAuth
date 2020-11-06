using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MultiTenantAuth.Extensions.AspIdentity.Model;

namespace MultiTenantAuth.Extensions.AspIdentity
{
    public class ApplicationUserManager:UserManager<ApplicationUser>
    {
        private readonly ApplicationUserStore<ApplicationUser,Tenant> _store;
        public ApplicationUserManager(ApplicationUserStore<ApplicationUser, Tenant> store, IOptions<IdentityOptions> optionsAccessor, IPasswordHasher<ApplicationUser> passwordHasher, IEnumerable<IUserValidator<ApplicationUser>> userValidators, IEnumerable<IPasswordValidator<ApplicationUser>> passwordValidators, ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<ApplicationUser>> logger) : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
            _store = store;
        }
        public Task<IList<string>> GetRolesAsync(string userId, int tenantId)
        {
            var user = _store.Context.Set<ApplicationUser>().Find(userId);
            var tenant = _store.Context.Set<Tenant>().Find(tenantId);

            if (null == user)
            {
                throw new Exception("User not found");
            }

            if (null == tenant)
            {
                throw new Exception("Tenant not found");
            }

            return _store.GetRolesAsync(user, tenant);
        }

        public async Task<IdentityResult> AddToRoleAsync(Tenant tenant,ApplicationUser user, string role)
        {
            ThrowIfDisposed();
            var userRoleStore = GetUserRoleStore();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }

            var normalizedRole = NormalizeName(role);
            if (await userRoleStore.IsInRoleAsync(user,tenant, normalizedRole, CancellationToken))
            {
                return await UserAlreadyInRoleError(user, role);
            }
            await userRoleStore.AddToRoleAsync(user, tenant, normalizedRole, CancellationToken);
            return await UpdateUserAsync(user);
        }
        private async Task<IdentityResult> UserAlreadyInRoleError(ApplicationUser user, string role)
        {
            Logger.LogWarning(5, "User {userId} is already in role {role}.", await GetUserIdAsync(user), role);
            return IdentityResult.Failed(ErrorDescriber.UserAlreadyInRole(role));
        }
        private ApplicationUserStore<ApplicationUser, Tenant> GetUserRoleStore()
        {
            var cast = Store as ApplicationUserStore<ApplicationUser,Tenant>;
            if (cast == null)
            {
                throw new NotSupportedException("StoreNotIUserRoleStore not supported to casting");
            }
            return cast;
        }
        public virtual async Task<IdentityResult> AddToRolesAsync(ApplicationUser user, Tenant tenant, IEnumerable<string> roles)
        {
            ThrowIfDisposed();
            var userRoleStore = GetUserRoleStore();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (roles == null)
            {
                throw new ArgumentNullException(nameof(roles));
            }

            foreach (var role in roles.Distinct())
            {
                var normalizedRole = NormalizeName(role);
                if (await userRoleStore.IsInRoleAsync(user, tenant, normalizedRole, CancellationToken))
                {
                    return await UserAlreadyInRoleError(user, role);
                }
                await userRoleStore.AddToRoleAsync(user, tenant, normalizedRole, CancellationToken);
            }
            return await UpdateUserAsync(user);
        }
    }
}
