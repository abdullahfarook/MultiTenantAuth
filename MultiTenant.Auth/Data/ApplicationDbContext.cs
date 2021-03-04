using System.Threading.Tasks;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Extensions;
using IdentityServer4.EntityFramework.Interfaces;
using IdentityServer4.EntityFramework.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MultiTenancyServer;
using MultiTenancyServer.EntityFramework;
using MultiTenancyServer.Options;
using MultiTenantAuth.Extensions.AspIdentity;
using MultiTenantAuth.Extensions.AspIdentity.Model;

namespace MultiTenantAuth.Data
{
    public class ApplicationDbContext :
        // Asp Core Identity
        ApplicationIdentityDbContext,

        // IdentityServer4 EF Core
        IConfigurationDbContext, 
        IPersistedGrantDbContext,

        // MultiTenancyServer EF Core
        ITenantDbContext<Tenant, string>
    {
        private readonly ConfigurationStoreOptions _storeOptions;
        private readonly OperationalStoreOptions _operationalOptions;

        private static TenancyModelState<string> _tenancyModelState;
        private readonly ITenancyContext<Tenant> _tenancyContext;
        private readonly ILogger _logger;

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            ConfigurationStoreOptions storeOptions,
            OperationalStoreOptions operationalOptions,
            ILogger<ApplicationDbContext> logger,
            ITenancyContext<Tenant> tenancyContext = null)
            : base(options)
        {
            _storeOptions = storeOptions;
            _operationalOptions = operationalOptions;

            _tenancyContext = tenancyContext;
            _logger = logger;
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            ConfigureIdentityContext(builder);
        }

        private void ConfigureIdentityContext(ModelBuilder builder)
        {
            base.OnModelCreating(builder);


            builder.ConfigureClientContext(_storeOptions);
            builder.ConfigureResourcesContext(_storeOptions);
            builder.ConfigurePersistedGrantContext(_operationalOptions);
            builder.ConfigureIdentity();
            builder.ConfigureTenant(out _tenancyModelState);
        }

        public Task<int> SaveChangesAsync()
        {
            this.EnsureTenancy(_tenancyContext?.Tenant?.Id, _tenancyModelState, _logger);
            return base.SaveChangesAsync();
        }

        public DbSet<Client> Clients { get; set; }
        public DbSet<ClientCorsOrigin> ClientCorsOrigins { get; set; }
        public DbSet<IdentityResource> IdentityResources { get; set; }
        public DbSet<ApiResource> ApiResources { get; set; }
        public DbSet<ApiScope> ApiScopes { get; set; }
        public DbSet<PersistedGrant> PersistedGrants { get; set; }
        public DbSet<DeviceFlowCodes> DeviceFlowCodes { get; set; }
        public DbSet<Tenant> Tenants { get; set; }
    }


    public static class ContextExtensions
    {
        public static void ConfigureIdentity(this ModelBuilder builder)
        {
            builder.Entity<ApplicationRole>().ToTable(TableConsts.IdentityRoles);
            builder.Entity<IdentityRoleClaim<string>>().ToTable(TableConsts.IdentityRoleClaims);
            builder.Entity<ApplicationUser>(userEntity =>
            {
                userEntity.ToTable(TableConsts.IdentityUsers);
                userEntity.Property(x => x.MultitenantEnabled).HasDefaultValue(true);
            });
            builder.Entity<IdentityUserLogin<string>>().ToTable(TableConsts.IdentityUserLogins);
            builder.Entity<IdentityUserClaim<string>>().ToTable(TableConsts.IdentityUserClaims);
            builder.Entity<IdentityUserToken<string>>().ToTable(TableConsts.IdentityUserTokens);
            builder.Entity<ApplicationUserRole>(b =>
            {
                b.ToTable(TableConsts.IdentityUserRoles);
                b.Property(x => x.State).HasDefaultValue(States.Active);

                b
                    .HasOne(c => c.User)
                    .WithMany(e => e.UserRoles)
                    .HasForeignKey(e => e.UserId)
                    .IsRequired();
                b
                    .HasOne(c => c.Role)
                    .WithMany(e => e.UserRoles)
                    .HasForeignKey(e => e.RoleId)
                    .IsRequired();
                b
                    .HasOne(c => c.Tenant)
                    .WithMany(e => e.UserRoles)
                    .HasForeignKey(e => e.TenantId)
                    .IsRequired();
                b.Ignore("Id");

                // Add multi-tenancy support to entity.
                //b.HasTenancy(() => _tenancyContext.Tenant.Id, _tenancyModelState, hasIndex: false);
                // Primary key
                b.HasKey(r => new { r.UserId, r.RoleId, r.TenantId });

                //Remove unique index on NormalizedName.
                b.HasIndex(r => r.UserId).IsUnique(false);
                b.HasIndex(r => r.RoleId).IsUnique(false);
                b.HasIndex(r => r.TenantId).IsUnique(false);
                // Add unique index on TenantId and NormalizedName.
                b.HasIndex(r => new { r.UserId, r.RoleId, r.TenantId })
                    .HasName("TenantUserRoleIndex").IsUnique();
            });
        }
        public static void ConfigureTenant(this ModelBuilder builder, out TenancyModelState<string> tenancyModelState)
        {
            // MultiTenancyServer configuration.
            var tenantStoreOptions = new TenantStoreOptions();
            builder.ConfigureTenantContext<Tenant, string>(tenantStoreOptions);

            // Add multi-tenancy support to model.
            var tenantReferenceOptions = new TenantReferenceOptions();
            builder.HasTenancy(tenantReferenceOptions, out tenancyModelState);
        }
    }
}
