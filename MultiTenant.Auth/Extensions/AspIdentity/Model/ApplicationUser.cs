using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace MultiTenantAuth.Extensions.AspIdentity.Model
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser<string>
    {
        public bool UserProfileCompleted { get; set; }
        public bool TenantProfileCompleted { get; set; }
        public string DisplayName { get; set; }
        public string Pic { get; protected set; }
        public string FirstName { get; protected set; }
        public string LastName { get; protected set; }
        public string Country { get; protected set; }
        public string? Province { get; protected set; }
        public string? City { get; protected set; }
        public string? ZipCode { get; protected set; }
        public DateTime? DateOfBirth { get; protected set; }
        public bool? MultitenantEnabled { get; protected set; }
        public DateTime CreatedOn { get; protected set; }
        public DateTime UpdatedOn { get; protected set; }
        public virtual List<ApplicationUserRole> UserRoles { get; protected set; } = new List<ApplicationUserRole>();
        public ApplicationUser()
        {
            Id = Guid.NewGuid().ToString();
            SecurityStamp = Guid.NewGuid().ToString();
        }
        public void ConfirmEmail()
        {
            EmailConfirmed = true;
            UserProfileCompleted = true;
        }
        public void CompleteTenantProfile()
        {
            TenantProfileCompleted = true;
        }
    }

    public enum States
    {
        Inactive = 0,
        Active = 1
    }
}
