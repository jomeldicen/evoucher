using System;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using System.Net.Http;
using System.Web;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System.Net;
using WebApp.Models;
using System.Linq;

namespace WebApp.Api.Admin
{
    [Authorize]
    [RoutePrefix("api/ChangePassword")]
    public class ChangePasswordController : ApiController
    {
        public ISecureDataFormat<AuthenticationTicket> AccessTokenFormat { get; private set; }
        private const string LocalLoginProvider = "Local";
        private ApplicationUserManager _userManager;

        public ChangePasswordController()
        {
        }

        public ChangePasswordController(ApplicationUserManager userManager,
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
            private set
            {
                _userManager = value;
            }
        }

        [Route("ChangePassword")]
        public async Task<IHttpActionResult> ChangePassword(ChangePasswordBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            using (WebAppEntities db = new WebAppEntities())
            {
                var Id = User.Identity.GetUserId();
                bool isAllowed = Convert.ToBoolean(db.Settings.Where(x => x.vSettingID == "A55D224B-8C28-4A27-A767-C15C089F26A8").FirstOrDefault().vSettingOption);
                if (isAllowed)
                {
                    IdentityResult result = await UserManager.ChangePasswordAsync(Id, model.OldPassword, model.NewPassword);
                    if (!result.Succeeded)
                        return BadRequest("Incorrect Password");

                    return Ok();
                }
                else
                    return BadRequest("Change Password Not Allowed");
            }
        }


        [Route("ResetPassword")]
        public async Task<IHttpActionResult> ResetPassword(ResetPasswordBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            using (WebAppEntities db = new WebAppEntities())
            {
                var cId = User.Identity.GetUserId();
                bool isUserSuperAdmin = db.AspNetUserRoles.Where(x => x.RoleId == "4594BBC7-831E-4BFE-B6C4-91DFA42DBB03" && x.UserId == cId).Any();
                if (isUserSuperAdmin)
                {
                    string token = UserManager.GeneratePasswordResetToken(model.Id);
                    var result = await UserManager.ResetPasswordAsync(model.Id, token, model.NewPassword);

                    if (!result.Succeeded)
                        return BadRequest("Password reset is not succeeded.");

                    return Ok();
                }
                else
                    return BadRequest("Only Super Administrator can reset password!");
            }
        }

        private IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return InternalServerError();
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (string error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                if (ModelState.IsValid)
                {
                    // No ModelState errors are available to send, so just return an empty BadRequest.
                    return BadRequest();
                }

                return BadRequest(ModelState);
            }

            return null;
        }
    }
}