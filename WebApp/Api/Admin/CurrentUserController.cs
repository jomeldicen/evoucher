using System;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using WebApp.Models;
using System.Data.Entity;

namespace WebApp.Api.Admin
{
    [Authorize]
    [RoutePrefix("api/CurrentUser")]
    public class CurrentUserController : ApiController
    {
        //start of find current user for event registration//
        private ApplicationUserManager _userManager;

        public CurrentUserController()
        {

        }
        public ISecureDataFormat<AuthenticationTicket> AccessTokenFormat { get; private set; }
        public CurrentUserController(ApplicationUserManager userManager,
         ISecureDataFormat<AuthenticationTicket> accessTokenFormat)
        {
            UserManager = userManager;
            AccessTokenFormat = accessTokenFormat;
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            set
            {
                _userManager = value;
            }
        }
        //end of current user find option//


        public async Task<IHttpActionResult> Get()
        {
            IdentityUser user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            using (WebAppEntities db = new WebAppEntities())
            {
                try
                {
                    var getCurrentUserData = await (from bs in db.AspNetUsers
                                                    where bs.Id == user.Id
                                                    select new
                                                    {
                                                        bs.Id,
                                                        bs.Email,
                                                        RoleName = bs.AspNetUserRoles.FirstOrDefault().AspNetRole.Name,
                                                        Name = bs.AspNetUsersProfile.vFirstName + " " + bs.AspNetUsersProfile.vLastName,
                                                        Photo = bs.AspNetUsersProfile.vPhoto
                                                    }).ToListAsync();

                    var data = new { GETDATA = getCurrentUserData };
                    return Ok(data);
                }
                catch (Exception ex)
                {
                    return BadRequest("" + ex.Message);
                }
            }
        }
    }
}