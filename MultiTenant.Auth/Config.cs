// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Generic;
using IdentityServer4;
using IdentityServer4.Models;

namespace MultiTenantAuth
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> Ids =>
            new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Email(),
                new IdentityResources.Profile(),
                new IdentityResources.Address(),
                new IdentityResource(
                    "roles",
                    "Your role(s)",
                    new List<string>() { "role" }),
                new IdentityResource(
                    "country",
                    "The country you're living in",
                    new List<string>() { "country" }),
                new IdentityResource(
                    "subscriptionlevel",
                    "Your subscription level",
                    new List<string>() { "subscriptionlevel" })
            };

        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            {
                new ApiScope(
                    "imagegalleryapi",
                    "Image Gallery API scope"),
                new ApiScope()
                {
                    Name = "jp_api",
                    DisplayName = "OAuth2 Server",
                    Description = "Manage mode to IS4",
                    Required = true
                }
            };


        public static IEnumerable<ApiResource> ApiResources =>
            new ApiResource[] {
                new ApiResource(
                    "imagegalleryapi",
                    "Image Gallery API",
                    new[] { "role" })
                    {
                        Scopes = { "imagegalleryapi"}
                    },
                new ApiResource
                {
                    Name = "jp_api",
                    DisplayName = "JP API",
                    Description = "OAuth2 Server Management Api",
                    ApiSecrets = { new Secret("Q&tGrEQMypEk.XxPU:%bWDZMdpZeJiyMwpLv4F7d**w9x:7KuJ#fy,E8KPHpKz++".Sha256()) },

                    UserClaims =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        "is4-rights",
                        "username",
                        "role"
                    },
                    Scopes = { "jp_api" }
                    }
                };

        public static IEnumerable<Client> Clients =>
            new Client[]
            {
                new Client
                {

                    ClientId = "IS4-Admin",
                    ClientName = "IS4-Admin",
                    ClientUri = "http://localhost:4200",
                    AllowedGrantTypes = GrantTypes.Code,
                    RequireConsent = true,
                    AllowAccessTokensViaBrowser = false,
                    RequireClientSecret = false,
                    RequirePkce = true,
                    AllowPlainTextPkce = false,
                    RedirectUris = new[] {
                        $"http://localhost:4200/login-callback",
                        $"http://localhost:4200/silent-refresh.html"
                    },
                    AllowedCorsOrigins = { "http://localhost:4200"},
                    PostLogoutRedirectUris = {$"http://localhost:4200",},
                    LogoUri = "https://jpproject.blob.core.windows.net/images/jplogo.png",
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Email,
                        "role",
                        "jp_api"
                    }
                },
                new Client
                {
                    AccessTokenType = AccessTokenType.Reference,
                    AccessTokenLifetime = 120,
                    AllowOfflineAccess = true,
                    UpdateAccessTokenClaimsOnRefresh = true,
                    ClientName = "Image Gallery",
                    ClientId = "imagegalleryclient",
                    AllowedGrantTypes = GrantTypes.Code,
                    RequirePkce = true,
                    RedirectUris = new List<string>()
                    {
                        "https://localhost:4200/signin-oidc"
                    },
                    PostLogoutRedirectUris = new List<string>()
                    {
                        "https://localhost:4200/signout-callback-oidc"
                    },
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Address,
                        "roles",
                        "imagegalleryapi",
                        "country",
                        "subscriptionlevel"
                    },
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    }
                } };
    }
}