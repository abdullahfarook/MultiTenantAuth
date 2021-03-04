using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using MultiTenantAuth.Extensions.AspIdentity.Model;

namespace MultiTenantAuth.Extensions
{
    public static class ClaimsExtensions
    {
        /// <summary>
        /// Determines whether the client is configured to use PKCE.
        /// </summary>
        /// <param name="store">The store.</param>
        /// <param name="client_id">The client identifier.</param>
        /// <returns></returns>
        public static async Task<bool> IsPkceClientAsync(this IClientStore store, string client_id)
        {
            if (!string.IsNullOrWhiteSpace(client_id))
            {
                var client = await store.FindEnabledClientByIdAsync(client_id);
                return client?.RequirePkce == true;
            }

            return false;
        }
        public static string ToDotNotation(this string val)
        {
            if (!string.IsNullOrWhiteSpace(val))
            {
                return string.Concat(val!.Select(x => char.IsUpper(x) ? "." + x : x.ToString()));
            }

            return string.Empty;
        }

        public static bool IsBehindReverseProxy(this IWebHostEnvironment host, IConfiguration configuration)
        {
            var config = configuration["ASPNETCORE_REVERSEPROXY"];
            return !string.IsNullOrEmpty(config) && config.Equals("true");
        }

        public static void AddIfDontExist(this List<Claim> claims, Claim newClaim)
        {
            if (claims.Any(c => c.Type == newClaim.Type))
                return;

            claims.Add(newClaim);
        }


        public static void Merge(this List<Claim> claims, IEnumerable<Claim> newClaim)
        {
            foreach (var claim in newClaim)
            {
                if (claims.Any(c => c.Type == claim.Type))
                    continue;

                claims.Add(claim);
            }
        }
        public static void MergeValues(this List<Claim> claims, IEnumerable<Claim> newClaim)
        {
            foreach (var claim in newClaim)
            {
                if (claims.Any(c => c.Type == claim.Type && (c.Value == claim.Value)))
                    continue;

                claims.Add(claim);
            }
        }
        public static void Merge(this List<string> items, IEnumerable<string> content)
        {
            foreach (var claim in content)
            {
                if (items.Contains(claim))
                    continue;

                items.Add(claim);
            }
        }

        public static bool ExistType(this List<Claim> claims, params string[] type)
        {
            var claim = claims.FirstOrDefault(f => type.Contains(f.Type));
            return claim != null;
        }
        public static string GetValue(this List<Claim> claims, params string[] type)
        {
            var claim = claims.FirstOrDefault(f => type.Contains(f.Type));
            return claim?.Value;
        }
        public static void Remove(this List<Claim> claims, params string[] type)
        {
            claims.RemoveAll(f => type.Contains(f.Type));
        }
        public static T Get<T>(this IServiceProvider serviceProvider)
        {
            return (T)serviceProvider.GetService(typeof(T));
        }

        public static string GetTenantId(this ClaimsPrincipal claimsPrincipal)
            => claimsPrincipal.Claims.Where(x => x.Type == CustomClaimTypes.TenantId).Select(x => x.Value).FirstOrDefault();
        public static string GetTenantName(this ClaimsPrincipal claimsPrincipal)
            => claimsPrincipal.Claims.Where(x => x.Type == "tname").Select(x => x.Value).FirstOrDefault();
        public static Tenant GetTenant(this ClaimsPrincipal claimsPrincipal)
        {
            Tenant tenant = null;
            if (claimsPrincipal.Claims.Any(x => x.Type == CustomClaimTypes.TenantId))
            {
                var claims = claimsPrincipal.Claims;
                tenant = new Tenant(new Guid(claims.First(x => x.Type == CustomClaimTypes.TenantId).Value), claims.First(x => x.Type == "tname").Value);
            }

            return tenant;
        }
        public static bool ContainsTenant(this ClaimsPrincipal claimsPrincipal)
            => claimsPrincipal.Claims.Any(x => x.Type == CustomClaimTypes.TenantId);
    }
}
