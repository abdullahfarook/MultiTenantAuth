// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Net;
using System.Reflection;
using IdentityServer4;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MultiTenancyServer;
using MultiTenantAuth.Data;
using MultiTenantAuth.Extensions.AspIdentity;
using MultiTenantAuth.Extensions.AspIdentity.Model;
using MultiTenantAuth.Extensions.IdentityServer;
using Serilog;

namespace MultiTenantAuth
{
    public class Startup
    {
        public IWebHostEnvironment Environment { get; }
        public IConfiguration Configuration { get; }

        public Startup(IWebHostEnvironment environment, IConfiguration configuration)
        {
            Environment = environment;
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = Configuration.GetConnectionString("DefaultConnection");
            var migrationsAssembly = typeof(ApplicationDbContext).GetTypeInfo().Assembly.GetName().Name;

            services.AddControllersWithViews().AddRazorRuntimeCompilation();
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(connectionString).EnableSensitiveDataLogging());

            // MultiTenent
            services.AddTransient<TenantManager<Tenant>>();
            services.AddMultiTenancy<Tenant, string>()
                // Add one or more IRequestParser (MultiTenancyServer.AspNetCore).
                .AddSubdomainParser(".tenants.localhost")
                .AddPathParser("/tenants/")
                .AddClaimParser("tname")
                .AddEntityFrameworkStore<ApplicationDbContext, Tenant, string>();

            services.AddScoped<IProfileService, ProfileService>();
            // Add ASP.NET Core Identity
            services.AddCustomAspIdentity();


            // Add IdentityServer 4 Configurations
            var builder = services.AddIdentityServer(options =>
                {
                    options.Events.RaiseErrorEvents = true;
                    options.Events.RaiseInformationEvents = true;
                    options.Events.RaiseFailureEvents = true;
                    options.Events.RaiseSuccessEvents = true;

                    // see https://identityserver4.readthedocs.io/en/latest/topics/resources.html
                    options.EmitStaticAudienceClaim = true;
                    //options.IssuerUri = "https://localhost:5000";
                })
                .AddAspNetIdentity<ApplicationUser>()
                //.AddInMemoryIdentityResources(Config.IdentityResources)
                .AddInMemoryApiScopes(Config.ApiScopes)
                .AddAuthorizeInteractionResponseGenerator<AccountChooserResponseGenerator>()
                //.AddInMemoryClients(Config.Clients)
                // Add the config data from DB (clients, resources)
                .AddConfigurationStore<ApplicationDbContext>(options =>
                {
                    options.ConfigureDbContext = b =>
                        b.UseSqlite(connectionString,
                            sql => sql.MigrationsAssembly(migrationsAssembly));
                })
                // Add the operational data from DB (codes, tokens, consents)
                .AddOperationalStore<ApplicationDbContext>(options =>
                {
                    options.ConfigureDbContext = b =>
                        b.UseSqlite(connectionString,
                            sql => sql.MigrationsAssembly(migrationsAssembly));
                }).AddProfileService<ProfileService>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // not recommended for production - you need to store your key material somewhere secure
            builder.AddDeveloperSigningCredential();

            services.AddAuthentication()
                .AddGoogle(options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    
                    // register your IdentityServer with Google at https://console.developers.google.com
                    // enable the Google+ API
                    // set the redirect URI to https://localhost:5001/signin-google
                    options.ClientId = "copy client ID from Google here";
                    options.ClientSecret = "copy client secret from Google here";
                })
                .AddCookie("Cookies")
                .AddOpenIdConnect("oidc", options =>
                {
                    options.Authority = "https://localhost:5000";
                    options.RequireHttpsMetadata = false;
                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.ClientId = "js";
                    options.SaveTokens = true;
                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    options.Scope.Add("TenantId");
                });
        }
        private static void NEVER_EAT_POISON_Disable_CertificateValidation()
        {
            // Disabling certificate validation can expose you to a man-in-the-middle attack
            // which may allow your encrypted message to be read by an attacker
            // https://stackoverflow.com/a/14907718/740639
            ServicePointManager.ServerCertificateValidationCallback =
                delegate
                {
                    return true;
                };
        }
        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            app.UseMultiTenancy<Tenant>();
            app.UseSerilogRequestLogging();
            app.UseStaticFiles();
            app.UseMultiTenancy<Tenant>();
            app.UseRouting();
            app.UseIdentityServer();
            app.UseAuthorization();
            app.UseAuthentication();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}