using Microsoft.AspNet.Identity;
using Bravo.Common.DataContracts.Users;
using Bravo.Managers;
using Bravo.Managers.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Bravo.Common.AspIdentity
{
    public class ApplicationUser : User, IUser<int>
    {
        private readonly IAuthenticationManager _userManager = new ManagerFactory().CreateManager<IAuthenticationManager>();

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser, int> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);

            var userId = userIdentity.GetUserId<int>();

            var userRoles = _userManager.GetUserRoles(userId);

            foreach (var role in userRoles)
            {
                userIdentity.AddClaim(new Claim(ClaimTypes.Role, role.Name));
            }

            // Add custom user claims here
            return userIdentity;
        }
    }
}
