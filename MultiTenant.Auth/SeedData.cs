// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MultiTenancyServer;
using MultiTenantAuth.Data;
using MultiTenantAuth.Extensions.AspIdentity;
using MultiTenantAuth.Extensions.AspIdentity.Model;
using Serilog;

namespace MultiTenantAuth
{
    public class SeedData
    {
        public static void EnsureSeedData(string connectionString,IServiceProvider services)
        {
            //var services = new ServiceCollection();
            //services.AddLogging();
            //services.AddMultiTenancy<Tenant, string>()
            //    .AddEntityFrameworkStore<ApplicationDbContext, Tenant, string>();
            //services.AddDbContext<ApplicationDbContext>(options =>
            //   options.UseSqlite(connectionString));

            //services.AddIdentity<ApplicationUser, IdentityRole>()
            //    .AddEntityFrameworkStores<ApplicationDbContext>()
            //    .AddDefaultTokenProviders();

            //using var serviceProvider = services.BuildServiceProvider();
            using (var scope = services.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var context = scope.ServiceProvider.GetService<ApplicationDbContext>();
                context.Database.Migrate();

                var tenantMgr = scope.ServiceProvider.GetRequiredService<TenantManager<Tenant>>();
                var tenant1 = tenantMgr.FindByCanonicalNameAsync("tenant1").Result;
                if (tenant1 == null)
                {
                    tenant1 = new Tenant("tenant1", "Tenant One");
                    var result = tenantMgr.CreateAsync(tenant1).Result;
                    if (!result.Succeeded)
                    {
                        throw new Exception(result.Errors.First().Description);
                    }

                    Console.WriteLine("tenant1 created");
                }
                else
                {
                    Console.WriteLine("tenant1 already exists");
                }

                var tenant2 = tenantMgr.FindByCanonicalNameAsync("tenant2").Result;
                if (tenant2 == null)
                {
                    tenant2 = new Tenant("tenant2", "Tenant Two");
                    var result = tenantMgr.CreateAsync(tenant2).Result;
                    if (!result.Succeeded)
                    {
                        throw new Exception(result.Errors.First().Description);
                    }

                    Console.WriteLine("tenant2 created");
                }
                else
                {
                    Console.WriteLine("tenant2 already exists");
                }
            }
            using (var scope = services.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var tenantMgr = scope.ServiceProvider.GetRequiredService<TenantManager<Tenant>>();
                var tenant = tenantMgr.FindByCanonicalNameAsync("Tenant1").Result;
                var tenancyContext = scope.ServiceProvider.GetService<ITenancyContext<Tenant>>();
                tenancyContext.Tenant = tenant;

                var context = scope.ServiceProvider.GetService<ApplicationDbContext>();
                context.Database.Migrate();

                var userMgr = scope.ServiceProvider.GetRequiredService<ApplicationUserManager>();
                var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
                
                // Roles configuration
                var adminRole = "admin";
                
                var role = roleMgr.FindByNameAsync(adminRole).Result;
                if (role == null)
                {
                    roleMgr.CreateAsync(new ApplicationRole(adminRole)).Wait();
                    Log.Debug("admin role created");
                }
                var managerRole = "manager";
                role = roleMgr.FindByNameAsync(managerRole).Result;
                if (role == null)
                {
                    roleMgr.CreateAsync(new ApplicationRole(managerRole)).Wait();
                    Log.Debug("manager role created");
                }

                var alice = userMgr.FindByNameAsync("alice").Result;
                if (alice == null)
                {
                    alice = new ApplicationUser
                    {
                        UserName = "alice",
                        Email = "AliceSmith@email.com",
                        EmailConfirmed = true,
                    };
                    var result = userMgr.CreateAsync(alice, "Pass123$").Result;
                    if (!result.Succeeded)
                    {
                        throw new Exception(result.Errors.First().Description);
                    }

                    result = userMgr.AddClaimsAsync(alice, new Claim[]{
                        new Claim(JwtClaimTypes.Name, "Alice Smith"),
                        new Claim(JwtClaimTypes.GivenName, "Alice"),
                        new Claim(JwtClaimTypes.FamilyName, "Smith"),
                        new Claim(JwtClaimTypes.WebSite, "http://alice.com"),
                    }).Result;
                    if (!result.Succeeded)
                    {
                        throw new Exception(result.Errors.First().Description);
                    }
                    Log.Debug("alice created");
                    userMgr.AddToRoleAsync(alice, adminRole).Wait();
                    userMgr.AddToRoleAsync(alice, managerRole).Wait();

                    userMgr.AddToRoleAsync(tenant, alice, adminRole).Wait();
                    userMgr.AddToRoleAsync(tenant, alice, managerRole).Wait();
                    Log.Debug("Roles added to alice");
                }
                else
                {
                    Log.Debug("alice already exists");
                }


                var bob = userMgr.FindByNameAsync("bob").Result;
                if (bob == null)
                {
                    bob = new ApplicationUser
                    {
                        UserName = "bob",
                        Email = "BobSmith@email.com",
                        EmailConfirmed = true
                    };
                    var result = userMgr.CreateAsync(bob, "Pass123$").Result;
                    if (!result.Succeeded)
                    {
                        throw new Exception(result.Errors.First().Description);
                    }

                    result = userMgr.AddClaimsAsync(bob, new Claim[]{
                        new Claim(JwtClaimTypes.Name, "Bob Smith"),
                        new Claim(JwtClaimTypes.GivenName, "Bob"),
                        new Claim(JwtClaimTypes.FamilyName, "Smith"),
                        new Claim(JwtClaimTypes.WebSite, "http://bob.com"),
                        new Claim("location", "somewhere")
                    }).Result;
                    if (!result.Succeeded)
                    {
                        throw new Exception(result.Errors.First().Description);
                    }
                    Log.Debug("bob created");
                }
                else
                {
                    Log.Debug("bob already exists");
                }

                EnsureSeedIdentityServerData(context).Wait();

            }
        }
        /// <summary>
        /// Generate default clients, identity and api resources
        /// </summary>
        private static async Task EnsureSeedIdentityServerData(ApplicationDbContext context)
        {
            if (!context.Clients.Any())
            {
                foreach (var client in Config.Clients)
                {
                    await context.Clients.AddAsync(client.ToEntity());
                }

                await context.SaveChangesAsync();
            }

            if (!context.IdentityResources.Any())
            {
                var identityResources = Config.Ids;

                foreach (var resource in identityResources)
                {
                    await context.IdentityResources.AddAsync(resource.ToEntity());
                }

                await context.SaveChangesAsync();
            }

            if (!context.ApiResources.Any())
            {
                foreach (var resource in Config.ApiResources)
                {
                    await context.ApiResources.AddAsync(resource.ToEntity());
                }

                await context.SaveChangesAsync();
            }
            if (!context.ApiScopes.Any())
            {
                foreach (var resource in Config.ApiScopes)
                {
                    await context.ApiScopes.AddAsync(resource.ToEntity());
                }

                await context.SaveChangesAsync();
            }
        }

    }

}
