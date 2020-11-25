using Microsoft.AspNetCore.Authorization;

namespace MultiTenant.Api.Authorization
{
    public class MustOwnImageRequirement : IAuthorizationRequirement
    {
        public MustOwnImageRequirement()
        {
        }
    }
}
