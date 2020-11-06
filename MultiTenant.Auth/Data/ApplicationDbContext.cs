
using System.Threading;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.Entities;
using IdentityServer4.EntityFramework.Extensions;
using IdentityServer4.EntityFramework.Interfaces;
using IdentityServer4.EntityFramework.Options;
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
        ApplicationIdentityDbContext,
        // IdentityServer4 EF Core
        IConfigurationDbContext, IPersistedGrantDbContext,
        // MultiTenancyServer EF Core
        ITenantDbContext<Tenant, string>
    {
        private static TenancyModelState<string> _tenancyModelState;
        private readonly ITenancyContext<Tenant> _tenancyContext;
        private readonly ILogger _logger;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ILogger<ApplicationDbContext> logger, ITenancyContext<Tenant> tenancyContext = null)
            : base(options)
        {
            // The request scoped tenancy context.
            // Should not access the tenancyContext.Tenant property in the constructor yet,
            // as the request pipeline has not finished running yet and it will likely be null.
            _tenancyContext = tenancyContext;
            _logger = logger;
        }
        // IdentityServer4 implementation.
        public DbSet<Client> Clients { get; set; }
        public DbSet<ClientCorsOrigin> ClientCorsOrigins { get; set; }
        public DbSet<IdentityResource> IdentityResources { get; set; }
        public DbSet<ApiResource> ApiResources { get; set; }
        public DbSet<ApiScope> ApiScopes { get; set; }
       

        public DbSet<PersistedGrant> PersistedGrants { get; set; }
        public DbSet<DeviceFlowCodes> DeviceFlowCodes { get; set; }
        // MultiTenancyServer implementation.
        public DbSet<Tenant> Tenants { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);

            // IdentityServer4 configuration.
            var configurationStoreOptions = new ConfigurationStoreOptions();
            builder.ConfigureClientContext(configurationStoreOptions);
            builder.ConfigureResourcesContext(configurationStoreOptions);
            var operationalStoreOptions = new OperationalStoreOptions();
            builder.ConfigurePersistedGrantContext(operationalStoreOptions);

            // MultiTenancyServer configuration.
            var tenantStoreOptions = new TenantStoreOptions();
            builder.ConfigureTenantContext<Tenant, string>(tenantStoreOptions);

            // Add multi-tenancy support to model.
            var tenantReferenceOptions = new TenantReferenceOptions();
            builder.HasTenancy<string>(tenantReferenceOptions, out _tenancyModelState);

            // Configure custom properties on Tenant (MultiTenancyServer).
            builder.Entity<Tenant>(b =>
            {
                b.Property(t => t.Name).HasMaxLength(256);
            });

            // Configure properties on Role (ASP.NET Core Identity).
            builder.Entity <ApplicationUserRole>(b =>
                {
                    b.Ignore("Id");
                    // Add multi-tenancy support to entity.
                    b.HasTenancy(() => _tenancyContext!.Tenant.Id, _tenancyModelState, hasIndex: false);
                // Primary key
                b.HasKey(r => new { r.UserId, r.RoleId,r.TenantId });

                    //Remove unique index on NormalizedName.
                   b.HasIndex(r => r.UserId).IsUnique(false);
                   b.HasIndex(r => r.RoleId).IsUnique(false);
                   b.HasIndex(r => r.TenantId).IsUnique(false);

                    // Add unique index on TenantId and NormalizedName.
                    b.HasIndex("TenantId", "RoleId", "UserId")
                            .HasName("TenantUserRoleIndex").IsUnique();

                });

        }
        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            // Ensure multi-tenancy for all tenantTable entities.
            this.EnsureTenancy(_tenancyContext?.Tenant?.Id, _tenancyModelState, _logger);
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new CancellationToken())
        {
            // Ensure multi-tenancy for all tenantTable entities.
            this.EnsureTenancy(_tenancyContext?.Tenant?.Id, _tenancyModelState, _logger);
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
        public Task<int> SaveChangesAsync()
        {
            // Ensure multi-tenancy for all tenantTable entities.
            this.EnsureTenancy(_tenancyContext?.Tenant?.Id, _tenancyModelState, _logger);
            return base.SaveChangesAsync();
        }
    }
}
