using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MultiTenantAuth.Data;
using MultiTenantAuth.Extensions.AspIdentity.Model;

namespace MultiTenantAuth.Extensions.AspIdentity
{
    public static class AspIdentityDependencyExtension
    {
        public static IServiceCollection AddCustomAspIdentity(this IServiceCollection services)
        {
            services.AddScoped<ApplicationUserStore<ApplicationUser, Tenant>>();
            services.AddScoped<ApplicationUserClaimsPrincipalFactory>();
            services.AddScoped<ApplicationIdentityDbContext, ApplicationDbContext>();

            services.AddIdentity<ApplicationUser, ApplicationRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddUserManager<ApplicationUserManager>()
                .AddSignInManager<ApplicationSigninManager>()
                .AddClaimsPrincipalFactory<ApplicationUserClaimsPrincipalFactory>()
                .AddUserStore<ApplicationUserStore<ApplicationUser, Tenant>>()
                .AddDefaultTokenProviders();
            return services;
        }
    }
}
