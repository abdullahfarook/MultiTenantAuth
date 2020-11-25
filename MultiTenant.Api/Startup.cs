using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using AutoMapper;
using IdentityModel.AspNetCore.AccessTokenValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using MultiTenant.Api.Authorization;
using MultiTenant.Api.Entities;
using MultiTenant.Api.Extension;
using MultiTenant.Api.Extension.AuthorizationPolicy;
using MultiTenant.Api.Services;

namespace MultiTenant.Api
{
    public class Constants
    {
        public static string Authority { get; set; } = "https://tenants.localhost:5000";
        //public static string Authority { get; set; } = "https://localhost:5000";
        public static string Audience { get; set; } = "jp_api";
        public static string Secret { get; set; } = "Q&tGrEQMypEk.XxPU:%bWDZMdpZeJiyMwpLv4F7d**w9x:7KuJ#fy,E8KPHpKz++";
    }
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
                     .AddJsonOptions(opts => opts.JsonSerializerOptions.PropertyNamingPolicy = null);

            services.AddHttpContextAccessor();
            //services.AddSingleton<TenantProvider>();
            //services.AddSingleton<IOptionsMonitor<JwtBearerOptions>, JweBearerOptionsProvider>();
            //services.AddSingleton<IConfigureOptions<JwtBearerOptions>, JwtOptionsInitializer>();
            //services.AddSingleton<IAuthorizationHandler, TenantHandler>();

            services.AddScoped<IAuthorizationHandler, MustOwnImageHandler>();
            services.AddCors(x =>
                x.AddPolicy("default", builder => builder.AllowAnyHeader().AllowAnyMethod().WithOrigins("http://localhost:4200")));

            services.AddAuthorization(authorizationOptions =>
            {
                authorizationOptions.AddPolicy(
                    "MustOwnImage",
                    policyBuilder =>
                    {
                        policyBuilder.RequireAuthenticatedUser();
                        policyBuilder.AddRequirements(
                              new MustOwnImageRequirement());
                    });
                //authorizationOptions.AddPolicy("Tenant", policy =>
                //    policy.Requirements.Add(new TenantRequirement()));
            });
            //NEVER_EAT_POISON_Disable_CertificateValidation();
            services.AddAuthentication("Bearer")
                // JWT tokens
                .AddJwtBearer("Bearer", options =>
                {
                    options.Authority = Constants.Authority;
                    options.Audience = Constants.Audience;
                    options.RequireHttpsMetadata = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        IssuerValidator = ValidateIssuerWithPlaceholder
                        //IssuerValidator = IssuerValidator,
                        //ClockSkew = TimeSpan.Zero

                    };
                    // if token does not contain a dot, it is a reference token
                    options.ForwardDefaultSelector = Selector.ForwardReferenceToken("introspection");
                })

                // reference tokens
                .AddOAuth2Introspection("introspection", options =>
                {
                    options.Authority = Constants.Authority;
                    options.ClientId = Constants.Audience;
                    options.ClientSecret = "secret";
                });
            //.AddIdentityServerAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme, options =>
            //{
            //    options.Authority = "https://localhost:5000";
            //    options.ApiName = "jp_api";
            //    options.ApiSecret = "apisecret";
            //});

            // register the DbContext on the container, getting the connection string from
            // appSettings (note: use this during development; in a production environment,
            // it's better to store the connection string in an environment variable)
            services.AddDbContext<GalleryContext>(options =>
            {
                options.UseSqlServer(
                    Configuration["ConnectionStrings:ImageGalleryDBConnectionString"]);
            });

            // register the repository
            services.AddScoped<IGalleryRepository, GalleryRepository>();

            // register AutoMapper-related services
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

        }

        private static string ValidateIssuerWithPlaceholder(string issuer, SecurityToken token, TokenValidationParameters parameters)
        {
            // https://thomaslevesque.com/2018/12/24/multitenant-azure-ad-issuer-validation-in-asp-net-core/
            // Accepts any issuer of the form "https://login.microsoftonline.com/{tenantid}/v2.0",
            // where tenantid is the tid from the token.

            if (token is JwtSecurityToken jwt)
            {
                if (jwt.Payload.TryGetValue("tname", out var value) &&
                    value is string tokentName)
                {
                    //if(issuer.Contains(str.Substring(str.LastIndexOf('-') + 1);))
                    var validissuers = (parameters.ValidIssuers ?? Enumerable.Empty<string>())
                        .Append(parameters.ValidIssuer)
                        .Where(i => !string.IsNullOrEmpty(i));

                    if (validissuers.Any(i => i.Insert(i.LastIndexOf("://", StringComparison.Ordinal) + 3, tokentName + '.') == issuer))
                        return issuer;
                    //return "https://tenant3.tenants.localhost:5000";
                }
            }

            // Recreate the exception that is thrown by default
            // when issuer validation fails
            var validIssuer = parameters.ValidIssuer ?? "null";
            var validIssuers = parameters.ValidIssuers == null
                ? "null"
                : !parameters.ValidIssuers.Any()
                    ? "empty"
                    : string.Join(", ", parameters.ValidIssuers);
            string errorMessage = FormattableString.Invariant(
                $"IDX10205: Issuer validation failed. Issuer: '{issuer}'. Did not match: validationParameters.ValidIssuer: '{validIssuer}' or validationParameters.ValidIssuers: '{validIssuers}'.");

            throw new SecurityTokenInvalidIssuerException(errorMessage)
            {
                InvalidIssuer = issuer
            };
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                IdentityModelEventSource.ShowPII = true;
            }
            else
            {
                app.UseExceptionHandler(appBuilder =>
                {
                    appBuilder.Run(async context =>
                    {
                        // ensure generic 500 status code on fault.
                        context.Response.StatusCode = StatusCodes.Status500InternalServerError; ;
                        await context.Response.WriteAsync("An unexpected fault happened. Try again later.");
                    });
                });
                // The default HSTS value is 30 days. You may want to change this for 
                // production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();
            app.UseCors("default");
            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
