using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4;
using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiTenantAuth.Data;
using MultiTenantAuth.Extensions.AspIdentity.Model;

namespace MultiTenantAuth.Quickstart.Tenants
{
    [SecurityHeaders]
    public class TenantsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TenantsController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(string returnUrl = "")
        {
            var userRoles = _context.UserRoles.ToList();
            var sub = User.GetSubjectId();
            var tenants = await (from tenant in _context.Tenants
                join userRole in _context.UserRoles on tenant.Id equals userRole.TenantId
                where userRole.UserId == sub
                select tenant).Distinct().ToListAsync();
            //var tenants = _context.UserRoles.Where(x=> x.UserId)
            ViewBag.returnUrl = returnUrl;
            ViewBag.referer = Request.Headers["Referer"].ToString();
            return View(new TenantViewModel
            {
                Tenants = tenants,
                ReturnUrl = returnUrl
            });
        }
        [HttpPost]
        public async Task<IActionResult> Select(Guid id, string name, string returnUrl)
        {
            string ReturnUrl = returnUrl;
            var identityUser = new IdentityServerUser(User.Claims.Single(r => r.Type == "sub").Value);
            identityUser.AdditionalClaims.Add(new Claim("tid", id.ToString()));
            identityUser.AdditionalClaims.Add(new Claim("tname", name));
            await HttpContext.SignInAsync(identityUser);
            return Redirect(ReturnUrl);

        }
        
    }
}
