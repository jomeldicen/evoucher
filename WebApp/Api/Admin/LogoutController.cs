using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.Identity;
using WebApp.Models;
using System.Net.Http;
using System.Web;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using System.Net.Mail;
using System.Web.Configuration;
using System.IO;
using System.Linq;
using System.Data.Entity;

namespace WebApp.Api
{
    [Authorize]
    [RoutePrefix("api/Logout")]
    public class LogoutController : ApiController
    {
        private const string LocalLoginProvider = "Local";
        private ApplicationUserManager _userManager;

        private string smtpHost = "";
        //private string smtpEmail = "";
        //private string smtpPass = "";
        //private string smtpPort = "";
        //private string smtpAuth = "";
        //private string siteAddress = "";
        //private string fromEmail = "";
        //private string fromName = "";

        public LogoutController()
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                this.smtpHost = db.Settings.Where(x => x.vSettingID == "E0009323-4I18-9E37-W868-DICEN89JOMEL").FirstOrDefault().vSettingOption;
                //this.smtpEmail = db.Settings.Where(x => x.vSettingID == "E0009323-4I18-9E37-W868-DICEN89JOMEL").FirstOrDefault().vSettingOption;
                //this.smtpPass = db.Settings.Where(x => x.vSettingID == "E0009323-4I18-9E37-W868-DICEN89JOMEL").FirstOrDefault().vSettingOption;
                //this.smtpPort = db.Settings.Where(x => x.vSettingID == "E0009323-4I18-9E37-W868-DICEN89JOMEL").FirstOrDefault().vSettingOption;
            }

        }

        public LogoutController(ApplicationUserManager userManager,
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

        public ISecureDataFormat<AuthenticationTicket> AccessTokenFormat { get; private set; }


        private IAuthenticationManager Authentication
        {
            get { return Request.GetOwinContext().Authentication; }
        }

        [Route("Logout")]
        public IHttpActionResult Logout(string ID)
        {
            using (WebAppEntities db = new WebAppEntities())
            {
                using (var dbContextTransaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        if (ID == null)
                            return BadRequest("Something Wrong!");
                        else
                        {
                            string ip = "";
                            System.Web.HttpContext cont = System.Web.HttpContext.Current;
                            string ipAddress = cont.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                            if (!string.IsNullOrEmpty(ipAddress))
                            {
                                string[] addresses = ipAddress.Split(',');
                                if (addresses.Length != 0)
                                {
                                    ip = addresses[0];
                                }
                            }
                            ip = cont.Request.ServerVariables["REMOTE_ADDR"];
                            AspNetUsersLoginHistory anulh = db.AspNetUsersLoginHistories.Where(x => x.vULHID == ID && x.nvIPAddress == ip).FirstOrDefault();
                            anulh.dLogOut = DateTime.UtcNow;
                            db.Entry(anulh).State = EntityState.Modified;
                            db.SaveChanges();

                            Authentication.SignOut(CookieAuthenticationDefaults.AuthenticationType);
                            var request = HttpContext.Current.Request;
                            string webaddress = request.Url.Scheme + "://" + request.ServerVariables["HTTP_HOST"] + request.ApplicationPath;
                            if (!webaddress.EndsWith("/"))
                                webaddress += "/";

                            dbContextTransaction.Commit();
                            return Ok(webaddress);
                        }
                        
                    }
                    catch
                    {
                        dbContextTransaction.Rollback();
                        return BadRequest("Something Wrong!");
                    }
                }
            }
            
        }


        //forgot password
        //forgot password Send Mail
        [HttpPost]
        [AllowAnonymous]
        [Route("ForgotPasswordSendMail")]
        public async Task<IHttpActionResult> ForgotPasswordSendMail(ForgotPasswordSendMailModel data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            using (WebAppEntities db = new WebAppEntities())
            {
                AspNetUser anu = db.AspNetUsers.Where(x => x.Email == data.Email).FirstOrDefault();
                if (anu != null)
                {
                    bool isUserSuperAdmin = db.AspNetUserRoles.Where(x => x.RoleId == "4594BBC7-831E-4BFE-B6C4-91DFA42DBB03" && x.UserId == anu.Id).Any();
                    bool isAllowed = Convert.ToBoolean(db.Settings.Where(x => x.vSettingID == "95A1ED0B-9645-4E18-9BD1-CAAB4F9F21F5").FirstOrDefault().vSettingOption);
                    if (isUserSuperAdmin || isAllowed)
                    {
                        var code = await UserManager.GeneratePasswordResetTokenAsync(anu.Id);
                        var callbackUrl = new Uri(Url.Link("ConfirmEmailSendMailRoute", new { userId = anu.Id, code = code }));


                        var fromAddress = new MailAddress(WebConfigurationManager.AppSettings["FromEmail"], WebConfigurationManager.AppSettings["FromName"]);
                        var toAddress = new MailAddress(anu.Email, String.Concat(anu.AspNetUsersProfile.vFirstName, " ", anu.AspNetUsersProfile.vLastName));
                        const string subject = "Recover Your Password";

                        string link = callbackUrl.ToString();

                        var smtp = new SmtpClient
                        {
                            Host = WebConfigurationManager.AppSettings["Host"],
                            Port = Convert.ToInt32(WebConfigurationManager.AppSettings["Port"]),
                            EnableSsl = Convert.ToBoolean(WebConfigurationManager.AppSettings["EnableSsl"]),
                            DeliveryMethod = SmtpDeliveryMethod.Network,
                            UseDefaultCredentials = false,
                            Credentials = new NetworkCredential(fromAddress.Address, WebConfigurationManager.AppSettings["FromPassword"])
                        };
                        using (var message = new MailMessage(fromAddress, toAddress)
                        {
                            IsBodyHtml = true,
                            Subject = subject,
                            Body = PopulateBody(link)
                        })
                        {
                            smtp.Send(message);
                        }

                        return Ok();
                    }
                    else
                    {
                        return Content(HttpStatusCode.BadRequest, "Recover Password Not Allowed");
                    }
                }
                else
                {
                    return Content(HttpStatusCode.BadRequest, "invalid email");
                }
            }
        }

        private string PopulateBody(string callbackUrl)
        {
            string body = string.Empty;
            using (StreamReader reader = new StreamReader(System.Web.Hosting.HostingEnvironment.MapPath("~/Views/Auth/RecoverPassword.html")))
            {
                body = reader.ReadToEnd();
            }
            body = body.Replace("{link}", callbackUrl);

            return body;
        }


        //Confirm Mail For Forgot Password
        [AllowAnonymous]
        [HttpGet]
        [Route("ConfirmEmailSendMail", Name = "ConfirmEmailSendMailRoute")]
        public IHttpActionResult ConfirmEmailSendMail(string userId = "", string code = "")
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(code))
            {
                ModelState.AddModelError("", "User Id and Code are required");
                return BadRequest(ModelState);
            }

            string webaddress = WebConfigurationManager.AppSettings["SiteAddress"];
            string url = webaddress + "/Auth/CreateNewPassword?userId=" + userId + "&code=" + code;
            Uri uri = new Uri(url);
            return Redirect(uri);
        }

        [HttpPost]
        [AllowAnonymous]
        [Route("SetPassword")]
        public async Task<IHttpActionResult> SetPassword(SetPasswordBindingModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await UserManager.ResetPasswordAsync(model.userId, model.Code, model.NewPassword);

            if (!result.Succeeded)
            {
                return GetErrorResult(result);
            }

            return Ok();
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