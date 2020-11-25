using Microsoft.AspNetCore.Authorization;

namespace MultiTenant.Api.Extension.AuthorizationPolicy
{
    public class TenantRequirement :IAuthorizationRequirement
    {

        public TenantRequirement()
        {


        }
    }
}
